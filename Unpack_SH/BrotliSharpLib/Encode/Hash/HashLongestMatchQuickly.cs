using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using score_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private class HashLongestMatchQuicklyH2 : Hasher {
            private const int BUCKET_BITS = 16;
            private const int BUCKET_SWEEP = 1;
            private const int HASH_LEN = 5;
            private const int USE_DICTIONARY = 1;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const int HASH_MAP_SIZE = 4 << BUCKET_BITS;

            public override size_t HashTypeLength() {
                return 8;
            }

            public override size_t StoreLookahead() {
                return 8;
            }

            /* HashBytes is the function that chooses the bucket to place
               the address in. The HashLongestMatch and HashLongestMatchQuickly
               classes have separate, different implementations of hashing. */
            private static unsafe uint HashBytes(byte* data) {
                ulong h = ((*(ulong*) (data) << (64 - 8 * HASH_LEN)) *
                           kHashMul64);
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return (uint) (h >> (64 - BUCKET_BITS));
            }

            /* A (forgetful) hash table to the data seen by the compressor, to
               help create backward references to previous data.

               This is a hash map of fixed size (BUCKET_SIZE). Starting from the
               given index, BUCKET_SWEEP buckets are used to store values of a key. */
            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashLongestMatchQuickly {
                public fixed uint buckets_[BUCKET_SIZE + BUCKET_SWEEP];
            }

            private static unsafe HashLongestMatchQuickly* Self(HasherHandle handle) {
                return (HashLongestMatchQuickly*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashLongestMatchQuickly* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = HASH_MAP_SIZE >> 7;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        uint key = HashBytes(&data[i]);
                        memset(&self->buckets_[key], 0, BUCKET_SWEEP * sizeof(uint));
                    }
                }
                else {
                    /* It is not strictly necessary to fill this buffer here, but
                       not filling will make the results of the compression stochastic
                       (but correct). This is because random data would cause the
                       system to find accidentally good backward references here and there. */
                    memset(&self->buckets_[0], 0, sizeof(uint) * (BUCKET_SIZE + BUCKET_SWEEP));
                }
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashLongestMatchQuickly);
            }

            /* Look at 5 bytes at &data[ix & mask].
               Compute a hash from these, and store the value somewhere within
               [ix .. ix+3]. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                uint key = HashBytes(&data[ix & mask]);
                /* Wiggle the value with the bucket sweep range. */
                uint off = (ix >> 3) % BUCKET_SWEEP;
                Self(handle)->buckets_[key + off] = (uint) ix;
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
            }

            /* Find a longest backward match of &data[cur_ix & ring_buffer_mask]
               up to the length of max_length and stores the position cur_ix in the
               hash table.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns true if match is found, otherwise false. */
            public override unsafe bool FindLongestMatch(
                HasherHandle handle,
                ushort* dictionary_hash, byte* data,
                size_t ring_buffer_mask, int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashLongestMatchQuickly* self = Self(handle);
                size_t best_len_in = out_->len;
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                uint key = HashBytes(&data[cur_ix_masked]);
                int compare_char = data[cur_ix_masked + best_len_in];
                score_t best_score = out_->score;
                size_t best_len = best_len_in;
                size_t cached_backward = (size_t) distance_cache[0];
                size_t prev_ix = cur_ix - cached_backward;
                bool is_match_found = false;
                out_->len_x_code = 0;
                if (prev_ix < cur_ix) {
                    prev_ix &= (uint) ring_buffer_mask;
                    if (compare_char == data[prev_ix + best_len]) {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            best_score = BackwardReferenceScoreUsingLastDistance(len);
                            best_len = len;
                            out_->len = len;
                            out_->distance = cached_backward;
                            out_->score = best_score;
                            compare_char = data[cur_ix_masked + best_len];
                            self->buckets_[key] = (uint) cur_ix;
                            return true;
                        }
                    }
                }
                {
                    size_t backward;
                    size_t len;
                    /* Only one to look for, don't bother to prepare for a loop. */
                    prev_ix = self->buckets_[key];
                    self->buckets_[key] = (uint) cur_ix;
                    backward = cur_ix - prev_ix;
                    prev_ix &= (uint) ring_buffer_mask;
                    if (compare_char != data[prev_ix + best_len_in]) {
                        return false;
                    }
                    if ((backward == 0 || backward > max_backward)) {
                        return false;
                    }
                    len = FindMatchLengthWithLimit(&data[prev_ix],
                        &data[cur_ix_masked],
                        max_length);
                    if (len >= 4) {
                        out_->len = len;
                        out_->distance = backward;
                        out_->score = BackwardReferenceScore(len, backward);
                        return true;
                    }
                }
                is_match_found = SearchInStaticDictionary(dictionary_hash,
                    handle, &data[cur_ix_masked], max_length, max_backward, out_,
                    true);
                self->buckets_[key + ((cur_ix >> 3) % BUCKET_SWEEP)] = (uint) cur_ix;
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

        private class HashLongestMatchQuicklyH3 : Hasher {
            private const int BUCKET_BITS = 16;
            private const int BUCKET_SWEEP = 2;
            private const int HASH_LEN = 5;
            private const int USE_DICTIONARY = 0;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const int HASH_MAP_SIZE = 4 << BUCKET_BITS;

            public override size_t HashTypeLength() {
                return 8;
            }

            public override size_t StoreLookahead() {
                return 8;
            }

            /* HashBytes is the function that chooses the bucket to place
               the address in. The HashLongestMatch and HashLongestMatchQuickly
               classes have separate, different implementations of hashing. */
            private static unsafe uint HashBytes(byte* data) {
                ulong h = ((*(ulong*) (data) << (64 - 8 * HASH_LEN)) *
                           kHashMul64);
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return (uint) (h >> (64 - BUCKET_BITS));
            }

            /* A (forgetful) hash table to the data seen by the compressor, to
               help create backward references to previous data.

               This is a hash map of fixed size (BUCKET_SIZE). Starting from the
               given index, BUCKET_SWEEP buckets are used to store values of a key. */
            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashLongestMatchQuickly {
                public fixed uint buckets_[BUCKET_SIZE + BUCKET_SWEEP];
            }

            private static unsafe HashLongestMatchQuickly* Self(HasherHandle handle) {
                return (HashLongestMatchQuickly*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashLongestMatchQuickly* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = HASH_MAP_SIZE >> 7;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        uint key = HashBytes(&data[i]);
                        memset(&self->buckets_[key], 0, BUCKET_SWEEP * sizeof(uint));
                    }
                }
                else {
                    /* It is not strictly necessary to fill this buffer here, but
                       not filling will make the results of the compression stochastic
                       (but correct). This is because random data would cause the
                       system to find accidentally good backward references here and there. */
                    memset(&self->buckets_[0], 0, sizeof(uint) * (BUCKET_SIZE + BUCKET_SWEEP));
                }
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashLongestMatchQuickly);
            }

            /* Look at 5 bytes at &data[ix & mask].
               Compute a hash from these, and store the value somewhere within
               [ix .. ix+3]. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                uint key = HashBytes(&data[ix & mask]);
                /* Wiggle the value with the bucket sweep range. */
                uint off = (ix >> 3) % BUCKET_SWEEP;
                Self(handle)->buckets_[key + off] = (uint) ix;
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
            }

            /* Find a longest backward match of &data[cur_ix & ring_buffer_mask]
               up to the length of max_length and stores the position cur_ix in the
               hash table.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns true if match is found, otherwise false. */
            public override unsafe bool FindLongestMatch(
                HasherHandle handle,
                ushort* dictionary_hash, byte* data,
                size_t ring_buffer_mask, int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashLongestMatchQuickly* self = Self(handle);
                size_t best_len_in = out_->len;
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                uint key = HashBytes(&data[cur_ix_masked]);
                int compare_char = data[cur_ix_masked + best_len_in];
                score_t best_score = out_->score;
                size_t best_len = best_len_in;
                size_t cached_backward = (size_t) distance_cache[0];
                size_t prev_ix = cur_ix - cached_backward;
                bool is_match_found = false;
                out_->len_x_code = 0;
                if (prev_ix < cur_ix) {
                    prev_ix &= (uint) ring_buffer_mask;
                    if (compare_char == data[prev_ix + best_len]) {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            best_score = BackwardReferenceScoreUsingLastDistance(len);
                            best_len = len;
                            out_->len = len;
                            out_->distance = cached_backward;
                            out_->score = best_score;
                            compare_char = data[cur_ix_masked + best_len];
                            {
                                is_match_found = true;
                            }
                        }
                    }
                }
                {
                    uint* bucket = self->buckets_ + key;
                    int i;
                    prev_ix = *bucket++;
                    for (i = 0; i < BUCKET_SWEEP; ++i, prev_ix = *bucket++) {
                        size_t backward = cur_ix - prev_ix;
                        size_t len;
                        prev_ix &= (uint) ring_buffer_mask;
                        if (compare_char != data[prev_ix + best_len]) {
                            continue;
                        }
                        if ((backward == 0 || backward > max_backward)) {
                            continue;
                        }
                        len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            score_t score = BackwardReferenceScore(len, backward);
                            if (best_score < score) {
                                best_score = score;
                                best_len = len;
                                out_->len = best_len;
                                out_->distance = backward;
                                out_->score = score;
                                compare_char = data[cur_ix_masked + best_len];
                                is_match_found = true;
                            }
                        }
                    }
                }
                self->buckets_[key + ((cur_ix >> 3) % BUCKET_SWEEP)] = (uint) cur_ix;
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

        private class HashLongestMatchQuicklyH4 : Hasher {
            private const int BUCKET_BITS = 17;
            private const int BUCKET_SWEEP = 4;
            private const int HASH_LEN = 5;
            private const int USE_DICTIONARY = 1;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const int HASH_MAP_SIZE = 4 << BUCKET_BITS;

            public override size_t HashTypeLength() {
                return 8;
            }

            public override size_t StoreLookahead() {
                return 8;
            }

            /* HashBytes is the function that chooses the bucket to place
               the address in. The HashLongestMatch and HashLongestMatchQuickly
               classes have separate, different implementations of hashing. */
            private static unsafe uint HashBytes(byte* data) {
                ulong h = ((*(ulong*) (data) << (64 - 8 * HASH_LEN)) *
                           kHashMul64);
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return (uint) (h >> (64 - BUCKET_BITS));
            }

            /* A (forgetful) hash table to the data seen by the compressor, to
               help create backward references to previous data.

               This is a hash map of fixed size (BUCKET_SIZE). Starting from the
               given index, BUCKET_SWEEP buckets are used to store values of a key. */
            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashLongestMatchQuickly {
                public fixed uint buckets_[BUCKET_SIZE + BUCKET_SWEEP];
            }

            private static unsafe HashLongestMatchQuickly* Self(HasherHandle handle) {
                return (HashLongestMatchQuickly*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashLongestMatchQuickly* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = HASH_MAP_SIZE >> 7;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        uint key = HashBytes(&data[i]);
                        memset(&self->buckets_[key], 0, BUCKET_SWEEP * sizeof(uint));
                    }
                }
                else {
                    /* It is not strictly necessary to fill this buffer here, but
                       not filling will make the results of the compression stochastic
                       (but correct). This is because random data would cause the
                       system to find accidentally good backward references here and there. */
                    memset(&self->buckets_[0], 0, sizeof(uint) * (BUCKET_SIZE + BUCKET_SWEEP));
                }
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashLongestMatchQuickly);
            }

            /* Look at 5 bytes at &data[ix & mask].
               Compute a hash from these, and store the value somewhere within
               [ix .. ix+3]. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                uint key = HashBytes(&data[ix & mask]);
                /* Wiggle the value with the bucket sweep range. */
                uint off = (ix >> 3) % BUCKET_SWEEP;
                Self(handle)->buckets_[key + off] = (uint) ix;
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
            }

            /* Find a longest backward match of &data[cur_ix & ring_buffer_mask]
               up to the length of max_length and stores the position cur_ix in the
               hash table.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns true if match is found, otherwise false. */
            public override unsafe bool FindLongestMatch(
                HasherHandle handle,
                ushort* dictionary_hash, byte* data,
                size_t ring_buffer_mask, int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashLongestMatchQuickly* self = Self(handle);
                size_t best_len_in = out_->len;
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                uint key = HashBytes(&data[cur_ix_masked]);
                int compare_char = data[cur_ix_masked + best_len_in];
                score_t best_score = out_->score;
                size_t best_len = best_len_in;
                size_t cached_backward = (size_t) distance_cache[0];
                size_t prev_ix = cur_ix - cached_backward;
                bool is_match_found = false;
                out_->len_x_code = 0;
                if (prev_ix < cur_ix) {
                    prev_ix &= (uint) ring_buffer_mask;
                    if (compare_char == data[prev_ix + best_len]) {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            best_score = BackwardReferenceScoreUsingLastDistance(len);
                            best_len = len;
                            out_->len = len;
                            out_->distance = cached_backward;
                            out_->score = best_score;
                            compare_char = data[cur_ix_masked + best_len];
                            {
                                is_match_found = true;
                            }
                        }
                    }
                }
                {
                    uint* bucket = self->buckets_ + key;
                    int i;
                    prev_ix = *bucket++;
                    for (i = 0; i < BUCKET_SWEEP; ++i, prev_ix = *bucket++) {
                        size_t backward = cur_ix - prev_ix;
                        size_t len;
                        prev_ix &= (uint) ring_buffer_mask;
                        if (compare_char != data[prev_ix + best_len]) {
                            continue;
                        }
                        if ((backward == 0 || backward > max_backward)) {
                            continue;
                        }
                        len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            score_t score = BackwardReferenceScore(len, backward);
                            if (best_score < score) {
                                best_score = score;
                                best_len = len;
                                out_->len = best_len;
                                out_->distance = backward;
                                out_->score = score;
                                compare_char = data[cur_ix_masked + best_len];
                                is_match_found = true;
                            }
                        }
                    }
                }
                if (USE_DICTIONARY != 0 && !is_match_found) {
                    is_match_found = SearchInStaticDictionary(dictionary_hash,
                        handle, &data[cur_ix_masked], max_length, max_backward, out_,
                        true);
                }
                self->buckets_[key + ((cur_ix >> 3) % BUCKET_SWEEP)] = (uint) cur_ix;
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

        private class HashLongestMatchQuicklyH54 : Hasher {
            private const int BUCKET_BITS = 20;
            private const int BUCKET_SWEEP = 4;
            private const int HASH_LEN = 7;
            private const int USE_DICTIONARY = 0;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;
            private const int HASH_MAP_SIZE = 4 << BUCKET_BITS;

            public override size_t HashTypeLength() {
                return 8;
            }

            public override size_t StoreLookahead() {
                return 8;
            }

            /* HashBytes is the function that chooses the bucket to place
               the address in. The HashLongestMatch and HashLongestMatchQuickly
               classes have separate, different implementations of hashing. */
            private static unsafe uint HashBytes(byte* data) {
                ulong h = ((*(ulong*) (data) << (64 - 8 * HASH_LEN)) *
                           kHashMul64);
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return (uint) (h >> (64 - BUCKET_BITS));
            }

            /* A (forgetful) hash table to the data seen by the compressor, to
               help create backward references to previous data.

               This is a hash map of fixed size (BUCKET_SIZE). Starting from the
               given index, BUCKET_SWEEP buckets are used to store values of a key. */
            [StructLayout(LayoutKind.Sequential)]
            private unsafe struct HashLongestMatchQuickly {
                public fixed uint buckets_[BUCKET_SIZE + BUCKET_SWEEP];
            }

            private static unsafe HashLongestMatchQuickly* Self(HasherHandle handle) {
                return (HashLongestMatchQuickly*) &(GetHasherCommon(handle)[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, SizeT input_size, byte* data) {
                HashLongestMatchQuickly* self = Self(handle);
                /* Partial preparation is 100 times slower (per socket). */
                size_t partial_prepare_threshold = HASH_MAP_SIZE >> 7;
                if (one_shot && input_size <= partial_prepare_threshold) {
                    size_t i;
                    for (i = 0; i < input_size; ++i) {
                        uint key = HashBytes(&data[i]);
                        memset(&self->buckets_[key], 0, BUCKET_SWEEP * sizeof(uint));
                    }
                }
                else {
                    /* It is not strictly necessary to fill this buffer here, but
                       not filling will make the results of the compression stochastic
                       (but correct). This is because random data would cause the
                       system to find accidentally good backward references here and there. */
                    memset(&self->buckets_[0], 0, sizeof(uint) * (BUCKET_SIZE + BUCKET_SWEEP));
                }
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                return sizeof(HashLongestMatchQuickly);
            }

            /* Look at 5 bytes at &data[ix & mask].
               Compute a hash from these, and store the value somewhere within
               [ix .. ix+3]. */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                uint key = HashBytes(&data[ix & mask]);
                /* Wiggle the value with the bucket sweep range. */
                uint off = (ix >> 3) % BUCKET_SWEEP;
                Self(handle)->buckets_[key + off] = (uint) ix;
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
            }

            /* Find a longest backward match of &data[cur_ix & ring_buffer_mask]
               up to the length of max_length and stores the position cur_ix in the
               hash table.

               Does not look for matches longer than max_length.
               Does not look for matches further away than max_backward.
               Writes the best match into |out|.
               Returns true if match is found, otherwise false. */
            public override unsafe bool FindLongestMatch(
                HasherHandle handle,
                ushort* dictionary_hash, byte* data,
                size_t ring_buffer_mask, int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                HashLongestMatchQuickly* self = Self(handle);
                size_t best_len_in = out_->len;
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                uint key = HashBytes(&data[cur_ix_masked]);
                int compare_char = data[cur_ix_masked + best_len_in];
                score_t best_score = out_->score;
                size_t best_len = best_len_in;
                size_t cached_backward = (size_t) distance_cache[0];
                size_t prev_ix = cur_ix - cached_backward;
                bool is_match_found = false;
                out_->len_x_code = 0;
                if (prev_ix < cur_ix) {
                    prev_ix &= (uint) ring_buffer_mask;
                    if (compare_char == data[prev_ix + best_len]) {
                        size_t len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            best_score = BackwardReferenceScoreUsingLastDistance(len);
                            best_len = len;
                            out_->len = len;
                            out_->distance = cached_backward;
                            out_->score = best_score;
                            compare_char = data[cur_ix_masked + best_len];
                            {
                                is_match_found = true;
                            }
                        }
                    }
                }
                {
                    uint* bucket = self->buckets_ + key;
                    int i;
                    prev_ix = *bucket++;
                    for (i = 0; i < BUCKET_SWEEP; ++i, prev_ix = *bucket++) {
                        size_t backward = cur_ix - prev_ix;
                        size_t len;
                        prev_ix &= (uint) ring_buffer_mask;
                        if (compare_char != data[prev_ix + best_len]) {
                            continue;
                        }
                        if ((backward == 0 || backward > max_backward)) {
                            continue;
                        }
                        len = FindMatchLengthWithLimit(&data[prev_ix],
                            &data[cur_ix_masked],
                            max_length);
                        if (len >= 4) {
                            score_t score = BackwardReferenceScore(len, backward);
                            if (best_score < score) {
                                best_score = score;
                                best_len = len;
                                out_->len = best_len;
                                out_->distance = backward;
                                out_->score = score;
                                compare_char = data[cur_ix_masked + best_len];
                                is_match_found = true;
                            }
                        }
                    }
                }
                self->buckets_[key + ((cur_ix >> 3) % BUCKET_SWEEP)] = (uint) cur_ix;
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