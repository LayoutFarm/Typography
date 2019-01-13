using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using score_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private class HashForgetfulChainH40 : Hasher {
            private const int BUCKET_BITS = 15;
            private const int NUM_LAST_DISTANCES_TO_CHECK = 1;
            private const int NUM_BANKS = 1;
            private const int BANK_BITS = 16;
            private const int BANK_SIZE = 1 << BANK_BITS;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const bool CAPPED_CHAINS = false;

            public override size_t HashTypeLength() {
                return 4;
            }

            public override size_t StoreLookahead() {
                return 4;
            }

            /* HashBytes is the function that chooses the bucket to place the address in. */
            private static unsafe uint HashBytes(byte* data) {
                uint h = *(uint*) (data) * kHashMul32;
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return h >> (32 - BUCKET_BITS);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Slot {
                public ushort delta;
                public ushort next;
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct Bank {
                private fixed ushort slots_[BANK_SIZE * 2];

                public Slot* slots(size_t index) {
                    fixed (ushort* s = slots_)
                        return (Slot*) &s[index * 2];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashForgetfulChain {
                public fixed uint addr[BUCKET_SIZE];

                public fixed ushort head[BUCKET_SIZE];

                /* Truncated hash used for quick rejection of "distance cache" candidates. */
                public fixed byte tiny_hash[65536];

                private fixed ushort banks_[NUM_BANKS * (BANK_SIZE * 2)];
                public fixed ushort free_slot_idx[NUM_BANKS];
                public size_t max_hops;

                public Bank* banks(size_t index) {
                    fixed (ushort* s = banks_)
                        return (Bank*) &s[index * BANK_SIZE * 2];
                }
            }

            private static unsafe HashForgetfulChain* Self(HasherHandle handle) {
                return (HashForgetfulChain*) &GetHasherCommon(handle)[1];
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
                Self(handle)->max_hops =
                    (params_->quality > 6 ? 7u : 8u) << (params_->quality - 4);
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashForgetfulChain* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = BUCKET_SIZE >> 6;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        size_t bucket = HashBytes(&data[i]);
                        /* See InitEmpty comment. */
                        self->addr[bucket] = 0xCCCCCCCC;
                        self->head[bucket] = 0xCCCC;
                    }
                }
                else {
                    /* Fill |addr| array with 0xCCCCCCCC value. Because of wrapping, position
                       processed by hasher never reaches 3GB + 64M; this makes all new chains
                       to be terminated after the first node. */
                    memset(self->addr, 0xCC, BUCKET_SIZE * sizeof(uint));
                    memset(self->head, 0, BUCKET_SIZE * sizeof(ushort));
                }
                memset(self->tiny_hash, 0, 65536);
                memset(self->free_slot_idx, 0, NUM_BANKS * sizeof(ushort));
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashForgetfulChain);
            }

            /* Look at 4 bytes at &data[ix & mask]. Compute a hash from these, and prepend
               node to corresponding chain; also update tiny_hash for current position. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                HashForgetfulChain* self = Self(handle);
                size_t key = HashBytes(&data[ix & mask]);
                size_t bank = key & (NUM_BANKS - 1);
                size_t idx = self->free_slot_idx[bank]++ & (BANK_SIZE - 1);
                size_t delta = ix - self->addr[key];
                self->tiny_hash[(ushort) ix] = (byte) key;
                if (delta > 0xFFFF) delta = CAPPED_CHAINS ? 0 : 0xFFFF;
                self->banks(bank)->slots(idx)->delta = (ushort) delta;
                self->banks(bank)->slots(idx)->next = self->head[key];
                self->addr[key] = (uint) ix;
                self->head[key] = (ushort) idx;
            }

            public override unsafe void StoreRange(HasherHandle handle,
                byte* data, size_t mask, size_t ix_start,
                size_t ix_end) {
                size_t i;
                for (i = ix_start; i < ix_end; ++i) {
                    Store(handle, data, mask, i);
                }
            }

            public override unsafe void StitchToPreviousBlock(HasherHandle handle, size_t num_bytes, size_t position,
                byte* ringbuffer,
                size_t ringbuffer_mask) {
                if (num_bytes >= HashTypeLength() - 1 && position >= 3) {
                    /* Prepare the hashes for three last bytes of the last write.
                       These could not be calculated before, since they require knowledge
                       of both the previous and the current block. */
                    Store(handle, ringbuffer, ringbuffer_mask, position - 3);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 2);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 1);
                }
            }

            public override unsafe void PrepareDistanceCache(HasherHandle handle, int* distance_cache) {
                Brotli.PrepareDistanceCache(distance_cache, NUM_LAST_DISTANCES_TO_CHECK);
            }

            /* Find a longest backward match of &data[cur_ix] up to the length of
               max_length and stores the position cur_ix in the hash table.

               REQUIRES: FN(PrepareDistanceCache) must be invoked for current distance cache
                         values; if this method is invoked repeatedly with the same distance
                         cache values, it is enough to invoke FN(PrepareDistanceCache) once.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns 1 when match is found, otherwise 0. */
            public override unsafe bool FindLongestMatch(HasherHandle handle,
                ushort* dictionary_hash,
                byte* data, size_t ring_buffer_mask,
                int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashForgetfulChain* self = Self(handle);
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                bool is_match_found = false;
                /* Don't accept a short copy from far away. */
                score_t best_score = out_->score;
                size_t best_len = out_->len;
                size_t i;
                size_t key = HashBytes(&data[cur_ix_masked]);
                byte tiny_hash = (byte) (key);
                out_->len = 0;
                out_->len_x_code = 0;
                /* Try last distance first. */
                for (i = 0; i < NUM_LAST_DISTANCES_TO_CHECK; ++i) {
                    size_t backward = (size_t) distance_cache[i];
                    size_t prev_ix = (cur_ix - backward);
                    /* For distance code 0 we want to consider 2-byte matches. */
                    if (i > 0 && self->tiny_hash[(ushort) prev_ix] != tiny_hash) continue;
                    if (prev_ix >= cur_ix || backward > max_backward) {
                        continue;
                    }
                    prev_ix &= ring_buffer_mask;
                    {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 2) {
                            score_t score = BackwardReferenceScoreUsingLastDistance(len);
                            if (best_score < score) {
                                if (i != 0) score -= BackwardReferencePenaltyUsingLastDistance(i);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                }
                {
                    size_t bank = key & (NUM_BANKS - 1);
                    size_t backward = 0;
                    size_t hops = self->max_hops;
                    size_t delta = cur_ix - self->addr[key];
                    size_t slot = self->head[key];
                    while ((hops--) != 0) {
                        size_t prev_ix;
                        size_t last = slot;
                        backward += delta;
                        if (backward > max_backward || (CAPPED_CHAINS && delta == 0)) break;
                        prev_ix = (cur_ix - backward) & ring_buffer_mask;
                        slot = self->banks(bank)->slots(last)->next;
                        delta = self->banks(bank)->slots(last)->delta;
                        if (cur_ix_masked + best_len > ring_buffer_mask ||
                            prev_ix + best_len > ring_buffer_mask ||
                            data[cur_ix_masked + best_len] != data[prev_ix + best_len]) {
                            continue;
                        }
                        {
                            size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                                &data[cur_ix_masked],
                                max_length);
                            if (len >= 4) {
                                /* Comparing for >= 3 does not change the semantics, but just saves
                                   for a few unnecessary binary logarithms in backward reference
                                   score, since we are not interested in such short matches. */
                                score_t score = BackwardReferenceScore(len, backward);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                    Store(handle, data, ring_buffer_mask, cur_ix);
                }
                if (!is_match_found) {
                    is_match_found = SearchInStaticDictionary(dictionary_hash,
                        handle, &data[cur_ix_masked], max_length, max_backward, out_,
                        false);
                }
                return is_match_found;
            }

            public override unsafe void CreateBackwardReferences(
                ushort* dictionary_hash,
                size_t num_bytes, size_t position,
                byte* ringbuffer, size_t ringbuffer_mask,
                BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
                size_t* last_insert_len, Command* commands, size_t* num_commands,
                size_t* num_literals) {
                /* Set maximum distance, see section 9.1. of the spec. */
                size_t max_backward_limit = BROTLI_MAX_BACKWARD_LIMIT(params_->lgwin);

                Command* orig_commands = commands;
                size_t insert_length = *last_insert_len;
                size_t pos_end = position + num_bytes;
                size_t store_end = num_bytes >= StoreLookahead()
                    ? position + num_bytes - StoreLookahead() + 1
                    : position;

                /* For speed up heuristics for random data. */
                size_t random_heuristics_window_size =
                    LiteralSpreeLengthForSparseSearch(params_);
                size_t apply_random_heuristics = position + random_heuristics_window_size;

                /* Minimum score to accept a backward reference. */
                score_t kMinScore = BROTLI_SCORE_BASE + 100;

                PrepareDistanceCache(hasher, dist_cache);

                while (position + HashTypeLength() < pos_end) {
                    size_t max_length = pos_end - position;
                    size_t max_distance = Math.Min(position, max_backward_limit);
                    HasherSearchResult sr = new HasherSearchResult();
                    sr.len = 0;
                    sr.len_x_code = 0;
                    sr.distance = 0;
                    sr.score = kMinScore;
                    if (FindLongestMatch(hasher, dictionary_hash,
                        ringbuffer, ringbuffer_mask, dist_cache,
                        position, max_length, max_distance, &sr)) {
                        /* Found a match. Let's look for something even better ahead. */
                        int delayed_backward_references_in_row = 0;
                        --max_length;
                        for (;; --max_length) {
                            score_t cost_diff_lazy = 175;
                            bool is_match_found;
                            HasherSearchResult sr2;
                            sr2.len = params_->quality < MIN_QUALITY_FOR_EXTENSIVE_REFERENCE_SEARCH
                                ? Math.Min(sr.len - 1, max_length)
                                : 0;
                            sr2.len_x_code = 0;
                            sr2.distance = 0;
                            sr2.score = kMinScore;
                            max_distance = Math.Min(position + 1, max_backward_limit);
                            is_match_found = FindLongestMatch(hasher,
                                dictionary_hash, ringbuffer, ringbuffer_mask, dist_cache,
                                position + 1, max_length, max_distance, &sr2);
                            if (is_match_found && sr2.score >= sr.score + cost_diff_lazy) {
                                /* Ok, let's just write one byte for now and start a match from the
                                   next byte. */
                                ++position;
                                ++insert_length;
                                sr = sr2;
                                if (++delayed_backward_references_in_row < 4 &&
                                    position + HashTypeLength() < pos_end) {
                                    continue;
                                }
                            }
                            break;
                        }
                        apply_random_heuristics =
                            position + 2 * sr.len + random_heuristics_window_size;
                        max_distance = Math.Min(position, max_backward_limit);
                        {
                            /* The first 16 codes are special short-codes,
                               and the minimum offset is 1. */
                            size_t distance_code =
                                ComputeDistanceCode(sr.distance, max_distance, dist_cache);
                            if (sr.distance <= max_distance && distance_code > 0) {
                                dist_cache[3] = dist_cache[2];
                                dist_cache[2] = dist_cache[1];
                                dist_cache[1] = dist_cache[0];
                                dist_cache[0] = (int) sr.distance;
                                PrepareDistanceCache(hasher, dist_cache);
                            }
                            InitCommand(commands++, insert_length, sr.len, sr.len ^ sr.len_x_code,
                                distance_code);
                        }
                        *num_literals += insert_length;
                        insert_length = 0;
                        /* Put the hash keys into the table, if there are enough bytes left.
                           Depending on the hasher implementation, it can push all positions
                           in the given range or only a subset of them. */
                        StoreRange(hasher, ringbuffer, ringbuffer_mask, position + 2,
                            Math.Min(position + sr.len, store_end));
                        position += sr.len;
                    }
                    else {
                        ++insert_length;
                        ++position;
                        /* If we have not seen matches for a long time, we can skip some
                           match lookups. Unsuccessful match lookups are very very expensive
                           and this kind of a heuristic speeds up compression quite
                           a lot. */
                        if (position > apply_random_heuristics) {
                            /* Going through uncompressible data, jump. */
                            if (position >
                                apply_random_heuristics + 4 * random_heuristics_window_size) {
                                /* It is quite a long time since we saw a copy, so we assume
                                   that this data is not compressible, and store hashes less
                                   often. Hashes of non compressible data are less likely to
                                   turn out to be useful in the future, too, so we store less of
                                   them to not to flood out the hash table of good compressible
                                   data. */
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 4);
                                size_t pos_jump =
                                    Math.Min(position + 16, pos_end - kMargin);
                                for (; position < pos_jump; position += 4) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 4;
                                }
                            }
                            else {
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 2);
                                size_t pos_jump =
                                    Math.Min(position + 8, pos_end - kMargin);
                                for (; position < pos_jump; position += 2) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 2;
                                }
                            }
                        }
                    }
                }
                insert_length += pos_end - position;
                *last_insert_len = insert_length;
                *num_commands += (size_t) (commands - orig_commands);
            }
        }

        private class HashForgetfulChainH41 : Hasher {
            private const int BUCKET_BITS = 15;
            private const int NUM_LAST_DISTANCES_TO_CHECK = 10;
            private const int NUM_BANKS = 1;
            private const int BANK_BITS = 16;
            private const int BANK_SIZE = 1 << BANK_BITS;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const bool CAPPED_CHAINS = false;

            public override size_t HashTypeLength() {
                return 4;
            }

            public override size_t StoreLookahead() {
                return 4;
            }

            /* HashBytes is the function that chooses the bucket to place the address in. */
            private static unsafe uint HashBytes(byte* data) {
                uint h = *(uint*) (data) * kHashMul32;
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return h >> (32 - BUCKET_BITS);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Slot {
                public ushort delta;
                public ushort next;
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct Bank {
                private fixed ushort slots_[BANK_SIZE * 2];

                public Slot* slots(size_t index) {
                    fixed (ushort* s = slots_)
                        return (Slot*) &s[index * 2];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashForgetfulChain {
                public fixed uint addr[BUCKET_SIZE];

                public fixed ushort head[BUCKET_SIZE];

                /* Truncated hash used for quick rejection of "distance cache" candidates. */
                public fixed byte tiny_hash[65536];

                private fixed ushort banks_[NUM_BANKS * (BANK_SIZE * 2)];
                public fixed ushort free_slot_idx[NUM_BANKS];
                public size_t max_hops;

                public Bank* banks(size_t index) {
                    fixed (ushort* s = banks_)
                        return (Bank*) &s[index * BANK_SIZE * 2];
                }
            }

            private static unsafe HashForgetfulChain* Self(HasherHandle handle) {
                return (HashForgetfulChain*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
                Self(handle)->max_hops =
                    (params_->quality > 6 ? 7u : 8u) << (params_->quality - 4);
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashForgetfulChain* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = BUCKET_SIZE >> 6;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        size_t bucket = HashBytes(&data[i]);
                        /* See InitEmpty comment. */
                        self->addr[bucket] = 0xCCCCCCCC;
                        self->head[bucket] = 0xCCCC;
                    }
                }
                else {
                    /* Fill |addr| array with 0xCCCCCCCC value. Because of wrapping, position
                       processed by hasher never reaches 3GB + 64M; this makes all new chains
                       to be terminated after the first node. */
                    memset(self->addr, 0xCC, BUCKET_SIZE * sizeof(uint));
                    memset(self->head, 0, BUCKET_SIZE * sizeof(ushort));
                }
                memset(self->tiny_hash, 0, 65536);
                memset(self->free_slot_idx, 0, NUM_BANKS * sizeof(ushort));
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashForgetfulChain);
            }

            /* Look at 4 bytes at &data[ix & mask]. Compute a hash from these, and prepend
               node to corresponding chain; also update tiny_hash for current position. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                HashForgetfulChain* self = Self(handle);
                size_t key = HashBytes(&data[ix & mask]);
                size_t bank = key & (NUM_BANKS - 1);
                size_t idx = self->free_slot_idx[bank]++ & (BANK_SIZE - 1);
                size_t delta = ix - self->addr[key];
                self->tiny_hash[(ushort) ix] = (byte) key;
                if (delta > 0xFFFF) delta = CAPPED_CHAINS ? 0 : 0xFFFF;
                self->banks(bank)->slots(idx)->delta = (ushort) delta;
                self->banks(bank)->slots(idx)->next = self->head[key];
                self->addr[key] = (uint) ix;
                self->head[key] = (ushort) idx;
            }

            public override unsafe void StoreRange(HasherHandle handle,
                byte* data, size_t mask, size_t ix_start,
                size_t ix_end) {
                size_t i;
                for (i = ix_start; i < ix_end; ++i) {
                    Store(handle, data, mask, i);
                }
            }

            public override unsafe void StitchToPreviousBlock(HasherHandle handle, size_t num_bytes, size_t position,
                byte* ringbuffer,
                size_t ringbuffer_mask) {
                if (num_bytes >= HashTypeLength() - 1 && position >= 3) {
                    /* Prepare the hashes for three last bytes of the last write.
                       These could not be calculated before, since they require knowledge
                       of both the previous and the current block. */
                    Store(handle, ringbuffer, ringbuffer_mask, position - 3);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 2);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 1);
                }
            }


            public override unsafe void PrepareDistanceCache(HasherHandle handle, int* distance_cache) {
                Brotli.PrepareDistanceCache(distance_cache, NUM_LAST_DISTANCES_TO_CHECK);
            }

            /* Find a longest backward match of &data[cur_ix] up to the length of
               max_length and stores the position cur_ix in the hash table.

               REQUIRES: FN(PrepareDistanceCache) must be invoked for current distance cache
                         values; if this method is invoked repeatedly with the same distance
                         cache values, it is enough to invoke FN(PrepareDistanceCache) once.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns 1 when match is found, otherwise 0. */
            public override unsafe bool FindLongestMatch(HasherHandle handle,
                ushort* dictionary_hash,
                byte* data, size_t ring_buffer_mask,
                int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashForgetfulChain* self = Self(handle);
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                bool is_match_found = false;
                /* Don't accept a short copy from far away. */
                score_t best_score = out_->score;
                size_t best_len = out_->len;
                size_t i;
                size_t key = HashBytes(&data[cur_ix_masked]);
                byte tiny_hash = (byte) (key);
                out_->len = 0;
                out_->len_x_code = 0;
                /* Try last distance first. */
                for (i = 0; i < NUM_LAST_DISTANCES_TO_CHECK; ++i) {
                    size_t backward = (size_t) distance_cache[i];
                    size_t prev_ix = (cur_ix - backward);
                    /* For distance code 0 we want to consider 2-byte matches. */
                    if (i > 0 && self->tiny_hash[(ushort) prev_ix] != tiny_hash) continue;
                    if (prev_ix >= cur_ix || backward > max_backward) {
                        continue;
                    }
                    prev_ix &= ring_buffer_mask;
                    {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 2) {
                            score_t score = BackwardReferenceScoreUsingLastDistance(len);
                            if (best_score < score) {
                                if (i != 0) score -= BackwardReferencePenaltyUsingLastDistance(i);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                }
                {
                    size_t bank = key & (NUM_BANKS - 1);
                    size_t backward = 0;
                    size_t hops = self->max_hops;
                    size_t delta = cur_ix - self->addr[key];
                    size_t slot = self->head[key];
                    while ((hops--) != 0) {
                        size_t prev_ix;
                        size_t last = slot;
                        backward += delta;
                        if (backward > max_backward || (CAPPED_CHAINS && delta == 0)) break;
                        prev_ix = (cur_ix - backward) & ring_buffer_mask;
                        slot = self->banks(bank)->slots(last)->next;
                        delta = self->banks(bank)->slots(last)->delta;
                        if (cur_ix_masked + best_len > ring_buffer_mask ||
                            prev_ix + best_len > ring_buffer_mask ||
                            data[cur_ix_masked + best_len] != data[prev_ix + best_len]) {
                            continue;
                        }
                        {
                            size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                                &data[cur_ix_masked],
                                max_length);
                            if (len >= 4) {
                                /* Comparing for >= 3 does not change the semantics, but just saves
                                   for a few unnecessary binary logarithms in backward reference
                                   score, since we are not interested in such short matches. */
                                score_t score = BackwardReferenceScore(len, backward);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                    Store(handle, data, ring_buffer_mask, cur_ix);
                }
                if (!is_match_found) {
                    is_match_found = SearchInStaticDictionary(dictionary_hash,
                        handle, &data[cur_ix_masked], max_length, max_backward, out_,
                        false);
                }
                return is_match_found;
            }

            public override unsafe void CreateBackwardReferences(
                ushort* dictionary_hash,
                size_t num_bytes, size_t position,
                byte* ringbuffer, size_t ringbuffer_mask,
                BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
                size_t* last_insert_len, Command* commands, size_t* num_commands,
                size_t* num_literals) {
                /* Set maximum distance, see section 9.1. of the spec. */
                size_t max_backward_limit = BROTLI_MAX_BACKWARD_LIMIT(params_->lgwin);

                Command* orig_commands = commands;
                size_t insert_length = *last_insert_len;
                size_t pos_end = position + num_bytes;
                size_t store_end = num_bytes >= StoreLookahead()
                    ? position + num_bytes - StoreLookahead() + 1
                    : position;

                /* For speed up heuristics for random data. */
                size_t random_heuristics_window_size =
                    LiteralSpreeLengthForSparseSearch(params_);
                size_t apply_random_heuristics = position + random_heuristics_window_size;

                /* Minimum score to accept a backward reference. */
                score_t kMinScore = BROTLI_SCORE_BASE + 100;

                PrepareDistanceCache(hasher, dist_cache);

                while (position + HashTypeLength() < pos_end) {
                    size_t max_length = pos_end - position;
                    size_t max_distance = Math.Min(position, max_backward_limit);
                    HasherSearchResult sr = new HasherSearchResult();
                    sr.len = 0;
                    sr.len_x_code = 0;
                    sr.distance = 0;
                    sr.score = kMinScore;
                    if (FindLongestMatch(hasher, dictionary_hash,
                        ringbuffer, ringbuffer_mask, dist_cache,
                        position, max_length, max_distance, &sr)) {
                        /* Found a match. Let's look for something even better ahead. */
                        int delayed_backward_references_in_row = 0;
                        --max_length;
                        for (;; --max_length) {
                            score_t cost_diff_lazy = 175;
                            bool is_match_found;
                            HasherSearchResult sr2;
                            sr2.len = params_->quality < MIN_QUALITY_FOR_EXTENSIVE_REFERENCE_SEARCH
                                ? Math.Min(sr.len - 1, max_length)
                                : 0;
                            sr2.len_x_code = 0;
                            sr2.distance = 0;
                            sr2.score = kMinScore;
                            max_distance = Math.Min(position + 1, max_backward_limit);
                            is_match_found = FindLongestMatch(hasher,
                                dictionary_hash, ringbuffer, ringbuffer_mask, dist_cache,
                                position + 1, max_length, max_distance, &sr2);
                            if (is_match_found && sr2.score >= sr.score + cost_diff_lazy) {
                                /* Ok, let's just write one byte for now and start a match from the
                                   next byte. */
                                ++position;
                                ++insert_length;
                                sr = sr2;
                                if (++delayed_backward_references_in_row < 4 &&
                                    position + HashTypeLength() < pos_end) {
                                    continue;
                                }
                            }
                            break;
                        }
                        apply_random_heuristics =
                            position + 2 * sr.len + random_heuristics_window_size;
                        max_distance = Math.Min(position, max_backward_limit);
                        {
                            /* The first 16 codes are special short-codes,
                               and the minimum offset is 1. */
                            size_t distance_code =
                                ComputeDistanceCode(sr.distance, max_distance, dist_cache);
                            if (sr.distance <= max_distance && distance_code > 0) {
                                dist_cache[3] = dist_cache[2];
                                dist_cache[2] = dist_cache[1];
                                dist_cache[1] = dist_cache[0];
                                dist_cache[0] = (int) sr.distance;
                                PrepareDistanceCache(hasher, dist_cache);
                            }
                            InitCommand(commands++, insert_length, sr.len, sr.len ^ sr.len_x_code,
                                distance_code);
                        }
                        *num_literals += insert_length;
                        insert_length = 0;
                        /* Put the hash keys into the table, if there are enough bytes left.
                           Depending on the hasher implementation, it can push all positions
                           in the given range or only a subset of them. */
                        StoreRange(hasher, ringbuffer, ringbuffer_mask, position + 2,
                            Math.Min(position + sr.len, store_end));
                        position += sr.len;
                    }
                    else {
                        ++insert_length;
                        ++position;
                        /* If we have not seen matches for a long time, we can skip some
                           match lookups. Unsuccessful match lookups are very very expensive
                           and this kind of a heuristic speeds up compression quite
                           a lot. */
                        if (position > apply_random_heuristics) {
                            /* Going through uncompressible data, jump. */
                            if (position >
                                apply_random_heuristics + 4 * random_heuristics_window_size) {
                                /* It is quite a long time since we saw a copy, so we assume
                                   that this data is not compressible, and store hashes less
                                   often. Hashes of non compressible data are less likely to
                                   turn out to be useful in the future, too, so we store less of
                                   them to not to flood out the hash table of good compressible
                                   data. */
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 4);
                                size_t pos_jump =
                                    Math.Min(position + 16, pos_end - kMargin);
                                for (; position < pos_jump; position += 4) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 4;
                                }
                            }
                            else {
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 2);
                                size_t pos_jump =
                                    Math.Min(position + 8, pos_end - kMargin);
                                for (; position < pos_jump; position += 2) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 2;
                                }
                            }
                        }
                    }
                }
                insert_length += pos_end - position;
                *last_insert_len = insert_length;
                *num_commands += (size_t) (commands - orig_commands);
            }
        }

        private class HashForgetfulChainH42 : Hasher {
            private const int BUCKET_BITS = 15;
            private const int NUM_LAST_DISTANCES_TO_CHECK = 16;
            private const int NUM_BANKS = 512;
            private const int BANK_BITS = 9;
            private const int BANK_SIZE = 1 << BANK_BITS;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const bool CAPPED_CHAINS = false;

            public override size_t HashTypeLength() {
                return 4;
            }

            public override size_t StoreLookahead() {
                return 4;
            }

            /* HashBytes is the function that chooses the bucket to place the address in. */
            private static unsafe uint HashBytes(byte* data) {
                uint h = *(uint*) (data) * kHashMul32;
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return h >> (32 - BUCKET_BITS);
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct Slot {
                public ushort delta;
                public ushort next;
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct Bank {
                private fixed ushort slots_[BANK_SIZE * 2];

                public Slot* slots(size_t index) {
                    fixed (ushort* s = slots_)
                        return (Slot*) &s[index * 2];
                }
            }

            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashForgetfulChain {
                public fixed uint addr[BUCKET_SIZE];

                public fixed ushort head[BUCKET_SIZE];

                /* Truncated hash used for quick rejection of "distance cache" candidates. */
                public fixed byte tiny_hash[65536];

                private fixed ushort banks_[NUM_BANKS * (BANK_SIZE * 2)];
                public fixed ushort free_slot_idx[NUM_BANKS];
                public size_t max_hops;

                public Bank* banks(size_t index) {
                    fixed (ushort* s = banks_)
                        return (Bank*) &s[index * BANK_SIZE * 2];
                }
            }

            private static unsafe HashForgetfulChain* Self(HasherHandle handle) {
                return (HashForgetfulChain*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
                Self(handle)->max_hops =
                    (params_->quality > 6 ? 7u : 8u) << (params_->quality - 4);
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashForgetfulChain* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = BUCKET_SIZE >> 6;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        size_t bucket = HashBytes(&data[i]);
                        /* See InitEmpty comment. */
                        self->addr[bucket] = 0xCCCCCCCC;
                        self->head[bucket] = 0xCCCC;
                    }
                }
                else {
                    /* Fill |addr| array with 0xCCCCCCCC value. Because of wrapping, position
                       processed by hasher never reaches 3GB + 64M; this makes all new chains
                       to be terminated after the first node. */
                    memset(self->addr, 0xCC, BUCKET_SIZE * sizeof(uint));
                    memset(self->head, 0, BUCKET_SIZE * sizeof(ushort));
                }
                memset(self->tiny_hash, 0, 65536);
                memset(self->free_slot_idx, 0, NUM_BANKS * sizeof(ushort));
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashForgetfulChain);
            }

            /* Look at 4 bytes at &data[ix & mask]. Compute a hash from these, and prepend
               node to corresponding chain; also update tiny_hash for current position. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                HashForgetfulChain* self = Self(handle);
                size_t key = HashBytes(&data[ix & mask]);
                size_t bank = key & (NUM_BANKS - 1);
                size_t idx = self->free_slot_idx[bank]++ & (BANK_SIZE - 1);
                size_t delta = ix - self->addr[key];
                self->tiny_hash[(ushort) ix] = (byte) key;
                if (delta > 0xFFFF) delta = CAPPED_CHAINS ? 0 : 0xFFFF;
                self->banks(bank)->slots(idx)->delta = (ushort) delta;
                self->banks(bank)->slots(idx)->next = self->head[key];
                self->addr[key] = (uint) ix;
                self->head[key] = (ushort) idx;
            }

            public override unsafe void StoreRange(HasherHandle handle,
                byte* data, size_t mask, size_t ix_start,
                size_t ix_end) {
                size_t i;
                for (i = ix_start; i < ix_end; ++i) {
                    Store(handle, data, mask, i);
                }
            }

            public override unsafe void StitchToPreviousBlock(HasherHandle handle, size_t num_bytes, size_t position,
                byte* ringbuffer,
                size_t ringbuffer_mask) {
                if (num_bytes >= HashTypeLength() - 1 && position >= 3) {
                    /* Prepare the hashes for three last bytes of the last write.
                       These could not be calculated before, since they require knowledge
                       of both the previous and the current block. */
                    Store(handle, ringbuffer, ringbuffer_mask, position - 3);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 2);
                    Store(handle, ringbuffer, ringbuffer_mask, position - 1);
                }
            }


            public override unsafe void PrepareDistanceCache(HasherHandle handle, int* distance_cache) {
                Brotli.PrepareDistanceCache(distance_cache, NUM_LAST_DISTANCES_TO_CHECK);
            }

            /* Find a longest backward match of &data[cur_ix] up to the length of
               max_length and stores the position cur_ix in the hash table.

               REQUIRES: FN(PrepareDistanceCache) must be invoked for current distance cache
                         values; if this method is invoked repeatedly with the same distance
                         cache values, it is enough to invoke FN(PrepareDistanceCache) once.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns 1 when match is found, otherwise 0. */
            public override unsafe bool FindLongestMatch(HasherHandle handle,
                ushort* dictionary_hash,
                byte* data, size_t ring_buffer_mask,
                int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashForgetfulChain* self = Self(handle);
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                bool is_match_found = false;
                /* Don't accept a short copy from far away. */
                score_t best_score = out_->score;
                size_t best_len = out_->len;
                size_t i;
                size_t key = HashBytes(&data[cur_ix_masked]);
                byte tiny_hash = (byte) (key);
                out_->len = 0;
                out_->len_x_code = 0;
                /* Try last distance first. */
                for (i = 0; i < NUM_LAST_DISTANCES_TO_CHECK; ++i) {
                    size_t backward = (size_t) distance_cache[i];
                    size_t prev_ix = (cur_ix - backward);
                    /* For distance code 0 we want to consider 2-byte matches. */
                    if (i > 0 && self->tiny_hash[(ushort) prev_ix] != tiny_hash) continue;
                    if (prev_ix >= cur_ix || backward > max_backward) {
                        continue;
                    }
                    prev_ix &= ring_buffer_mask;
                    {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 2) {
                            score_t score = BackwardReferenceScoreUsingLastDistance(len);
                            if (best_score < score) {
                                if (i != 0) score -= BackwardReferencePenaltyUsingLastDistance(i);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                }
                {
                    size_t bank = key & (NUM_BANKS - 1);
                    size_t backward = 0;
                    size_t hops = self->max_hops;
                    size_t delta = cur_ix - self->addr[key];
                    size_t slot = self->head[key];
                    while ((hops--) != 0) {
                        size_t prev_ix;
                        size_t last = slot;
                        backward += delta;
                        if (backward > max_backward || (CAPPED_CHAINS && delta == 0)) break;
                        prev_ix = (cur_ix - backward) & ring_buffer_mask;
                        slot = self->banks(bank)->slots(last)->next;
                        delta = self->banks(bank)->slots(last)->delta;
                        if (cur_ix_masked + best_len > ring_buffer_mask ||
                            prev_ix + best_len > ring_buffer_mask ||
                            data[cur_ix_masked + best_len] != data[prev_ix + best_len]) {
                            continue;
                        }
                        {
                            size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                                &data[cur_ix_masked],
                                max_length);
                            if (len >= 4) {
                                /* Comparing for >= 3 does not change the semantics, but just saves
                                   for a few unnecessary binary logarithms in backward reference
                                   score, since we are not interested in such short matches. */
                                score_t score = BackwardReferenceScore(len, backward);
                                if (best_score < score) {
                                    best_score = score;
                                    best_len = len;
                                    out_->len = best_len;
                                    out_->distance = backward;
                                    out_->score = best_score;
                                    is_match_found = true;
                                }
                            }
                        }
                    }
                    Store(handle, data, ring_buffer_mask, cur_ix);
                }
                if (!is_match_found) {
                    is_match_found = SearchInStaticDictionary(dictionary_hash,
                        handle, &data[cur_ix_masked], max_length, max_backward, out_,
                        false);
                }
                return is_match_found;
            }

            public override unsafe void CreateBackwardReferences(
                ushort* dictionary_hash,
                size_t num_bytes, size_t position,
                byte* ringbuffer, size_t ringbuffer_mask,
                BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
                size_t* last_insert_len, Command* commands, size_t* num_commands,
                size_t* num_literals) {
                /* Set maximum distance, see section 9.1. of the spec. */
                size_t max_backward_limit = BROTLI_MAX_BACKWARD_LIMIT(params_->lgwin);

                Command* orig_commands = commands;
                size_t insert_length = *last_insert_len;
                size_t pos_end = position + num_bytes;
                size_t store_end = num_bytes >= StoreLookahead()
                    ? position + num_bytes - StoreLookahead() + 1
                    : position;

                /* For speed up heuristics for random data. */
                size_t random_heuristics_window_size =
                    LiteralSpreeLengthForSparseSearch(params_);
                size_t apply_random_heuristics = position + random_heuristics_window_size;

                /* Minimum score to accept a backward reference. */
                score_t kMinScore = BROTLI_SCORE_BASE + 100;

                PrepareDistanceCache(hasher, dist_cache);

                while (position + HashTypeLength() < pos_end) {
                    size_t max_length = pos_end - position;
                    size_t max_distance = Math.Min(position, max_backward_limit);
                    HasherSearchResult sr = new HasherSearchResult();
                    sr.len = 0;
                    sr.len_x_code = 0;
                    sr.distance = 0;
                    sr.score = kMinScore;
                    if (FindLongestMatch(hasher, dictionary_hash,
                        ringbuffer, ringbuffer_mask, dist_cache,
                        position, max_length, max_distance, &sr)) {
                        /* Found a match. Let's look for something even better ahead. */
                        int delayed_backward_references_in_row = 0;
                        --max_length;
                        for (;; --max_length) {
                            score_t cost_diff_lazy = 175;
                            bool is_match_found;
                            HasherSearchResult sr2;
                            sr2.len = params_->quality < MIN_QUALITY_FOR_EXTENSIVE_REFERENCE_SEARCH
                                ? Math.Min(sr.len - 1, max_length)
                                : 0;
                            sr2.len_x_code = 0;
                            sr2.distance = 0;
                            sr2.score = kMinScore;
                            max_distance = Math.Min(position + 1, max_backward_limit);
                            is_match_found = FindLongestMatch(hasher,
                                dictionary_hash, ringbuffer, ringbuffer_mask, dist_cache,
                                position + 1, max_length, max_distance, &sr2);
                            if (is_match_found && sr2.score >= sr.score + cost_diff_lazy) {
                                /* Ok, let's just write one byte for now and start a match from the
                                   next byte. */
                                ++position;
                                ++insert_length;
                                sr = sr2;
                                if (++delayed_backward_references_in_row < 4 &&
                                    position + HashTypeLength() < pos_end) {
                                    continue;
                                }
                            }
                            break;
                        }
                        apply_random_heuristics =
                            position + 2 * sr.len + random_heuristics_window_size;
                        max_distance = Math.Min(position, max_backward_limit);
                        {
                            /* The first 16 codes are special short-codes,
                               and the minimum offset is 1. */
                            size_t distance_code =
                                ComputeDistanceCode(sr.distance, max_distance, dist_cache);
                            if (sr.distance <= max_distance && distance_code > 0) {
                                dist_cache[3] = dist_cache[2];
                                dist_cache[2] = dist_cache[1];
                                dist_cache[1] = dist_cache[0];
                                dist_cache[0] = (int) sr.distance;
                                PrepareDistanceCache(hasher, dist_cache);
                            }
                            InitCommand(commands++, insert_length, sr.len, sr.len ^ sr.len_x_code,
                                distance_code);
                        }
                        *num_literals += insert_length;
                        insert_length = 0;
                        /* Put the hash keys into the table, if there are enough bytes left.
                           Depending on the hasher implementation, it can push all positions
                           in the given range or only a subset of them. */
                        StoreRange(hasher, ringbuffer, ringbuffer_mask, position + 2,
                            Math.Min(position + sr.len, store_end));
                        position += sr.len;
                    }
                    else {
                        ++insert_length;
                        ++position;
                        /* If we have not seen matches for a long time, we can skip some
                           match lookups. Unsuccessful match lookups are very very expensive
                           and this kind of a heuristic speeds up compression quite
                           a lot. */
                        if (position > apply_random_heuristics) {
                            /* Going through uncompressible data, jump. */
                            if (position >
                                apply_random_heuristics + 4 * random_heuristics_window_size) {
                                /* It is quite a long time since we saw a copy, so we assume
                                   that this data is not compressible, and store hashes less
                                   often. Hashes of non compressible data are less likely to
                                   turn out to be useful in the future, too, so we store less of
                                   them to not to flood out the hash table of good compressible
                                   data. */
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 4);
                                size_t pos_jump =
                                    Math.Min(position + 16, pos_end - kMargin);
                                for (; position < pos_jump; position += 4) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 4;
                                }
                            }
                            else {
                                size_t kMargin =
                                    Math.Max(StoreLookahead() - 1, 2);
                                size_t pos_jump =
                                    Math.Min(position + 8, pos_end - kMargin);
                                for (; position < pos_jump; position += 2) {
                                    Store(hasher, ringbuffer, ringbuffer_mask, position);
                                    insert_length += 2;
                                }
                            }
                        }
                    }
                }
                insert_length += pos_end - position;
                *last_insert_len = insert_length;
                *num_commands += (size_t) (commands - orig_commands);
            }
        }
    }
}