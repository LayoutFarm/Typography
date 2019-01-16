using System;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private class HashToBinaryTreeH10 : Hasher {
            private const int BUCKET_BITS = 17;
            private const int MAX_TREE_SEARCH_DEPTH = 64;
            private const int MAX_TREE_COMP_LENGTH = 128;
            private const int BUCKET_SIZE = 1 << BUCKET_BITS;

            public override size_t HashTypeLength() {
                return 4;
            }

            public override size_t StoreLookahead() {
                return MAX_TREE_COMP_LENGTH;
            }

            private static unsafe uint HashBytes(byte* data) {
                uint h = *(uint*) data * kHashMul32;
                /* The higher bits contain more mixture from the multiplication,
                   so we take our results from there. */
                return h >> (32 - BUCKET_BITS);
            }

            private unsafe struct HashToBinaryTree {
                /* The window size minus 1 */
                public size_t window_mask_;

                /* Hash table that maps the 4-byte hashes of the sequence to the last
                   position where this hash was found, which is the root of the binary
                   tree of sequences that share this hash bucket. */
                public fixed uint buckets_[BUCKET_SIZE];

                /* A position used to mark a non-existent sequence, i.e. a tree is empty if
                   its root is at invalid_pos_ and a node is a leaf if both its children
                   are at invalid_pos_. */
                public uint invalid_pos_;

                /* --- Dynamic size members --- */

                /* The union of the binary trees of each hash bucket. The root of the tree
                   corresponding to a hash is a sequence starting at buckets_[hash] and
                   the left and right children of a sequence starting at pos are
                   forest_[2 * pos] and forest_[2 * pos + 1]. */
                /* uint32_t forest[2 * num_nodes] */
            }

            private static unsafe HashToBinaryTree* Self(HasherHandle handle) {
                return (HashToBinaryTree*) &(GetHasherCommon(handle)[1]);
            }

            private static unsafe uint* Forest(HashToBinaryTree* self) {
                return (uint*) (&self[1]);
            }

            public override unsafe void Initialize(HasherHandle handle, BrotliEncoderParams* params_) {
                HashToBinaryTree* self = Self(handle);
                self->window_mask_ = (1u << params_->lgwin) - 1u;
                self->invalid_pos_ = (uint) (0 - self->window_mask_);
            }

            public override unsafe void Prepare(HasherHandle handle, bool one_shot, size_t input_size, byte* data) {
                HashToBinaryTree* self = Self(handle);
                uint invalid_pos = self->invalid_pos_;
                uint i;
                for (i = 0; i < BUCKET_SIZE; i++) {
                    self->buckets_[i] = invalid_pos;
                }
            }

            public override unsafe size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot,
                size_t input_size) {
                size_t num_nodes = (size_t) 1 << params_->lgwin;
                if (one_shot && input_size < num_nodes) {
                    num_nodes = input_size;
                }
                return sizeof(HashToBinaryTree) + 2 * sizeof(uint) * num_nodes;
            }

            private static unsafe size_t LeftChildIndex(HashToBinaryTree* self, size_t pos) {
                return 2 * (pos & self->window_mask_);
            }

            private static unsafe size_t RightChildIndex(HashToBinaryTree* self, size_t pos) {
                return 2 * (pos & self->window_mask_) + 1;
            }


            /* Stores the hash of the next 4 bytes and in a single tree-traversal, the
               hash bucket's binary tree is searched for matches and is re-rooted at the
               current position.

               If less than MAX_TREE_COMP_LENGTH data is available, the hash bucket of the
               current position is searched for matches, but the state of the hash table
               is not changed, since we can not know the final sorting order of the
               current (incomplete) sequence.

               This function must be called with increasing cur_ix positions. */
            private static unsafe BackwardMatch* StoreAndFindMatches(
                HashToBinaryTree* self, byte* data,
                size_t cur_ix, size_t ring_buffer_mask, size_t max_length,
                size_t max_backward, size_t* best_len,
                BackwardMatch* matches) {
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                size_t max_comp_len =
                    Math.Min(max_length, MAX_TREE_COMP_LENGTH);
                bool should_reroot_tree =
                    max_length >= MAX_TREE_COMP_LENGTH;
                uint key = HashBytes(&data[cur_ix_masked]);
                uint* forest = Forest(self);
                size_t prev_ix = self->buckets_[key];
                /* The forest index of the rightmost node of the left subtree of the new
                   root, updated as we traverse and re-root the tree of the hash bucket. */
                size_t node_left = LeftChildIndex(self, cur_ix);
                /* The forest index of the leftmost node of the right subtree of the new
                   root, updated as we traverse and re-root the tree of the hash bucket. */
                size_t node_right = RightChildIndex(self, cur_ix);
                /* The match length of the rightmost node of the left subtree of the new
                   root, updated as we traverse and re-root the tree of the hash bucket. */
                size_t best_len_left = 0;
                /* The match length of the leftmost node of the right subtree of the new
                   root, updated as we traverse and re-root the tree of the hash bucket. */
                size_t best_len_right = 0;
                size_t depth_remaining;
                if (should_reroot_tree) {
                    self->buckets_[key] = (uint) cur_ix;
                }
                for (depth_remaining = MAX_TREE_SEARCH_DEPTH;; --depth_remaining) {
                    size_t backward = cur_ix - prev_ix;
                    size_t prev_ix_masked = prev_ix & ring_buffer_mask;
                    if (backward == 0 || backward > max_backward || depth_remaining == 0) {
                        if (should_reroot_tree) {
                            forest[node_left] = self->invalid_pos_;
                            forest[node_right] = self->invalid_pos_;
                        }
                        break;
                    }
                    {
                        size_t cur_len = Math.Min(best_len_left, best_len_right);
                        size_t len;
                        len = cur_len +
                              FindMatchLengthWithLimit(&data[cur_ix_masked + cur_len],
                                  &data[prev_ix_masked + cur_len],
                                  max_length - cur_len);
                        if (matches != null && len > *best_len) {
                            *best_len = len;
                            InitBackwardMatch(matches++, backward, len);
                        }
                        if (len >= max_comp_len) {
                            if (should_reroot_tree) {
                                forest[node_left] = forest[LeftChildIndex(self, prev_ix)];
                                forest[node_right] = forest[RightChildIndex(self, prev_ix)];
                            }
                            break;
                        }
                        if (data[cur_ix_masked + len] > data[prev_ix_masked + len]) {
                            best_len_left = len;
                            if (should_reroot_tree) {
                                forest[node_left] = (uint) prev_ix;
                            }
                            node_left = RightChildIndex(self, prev_ix);
                            prev_ix = forest[node_left];
                        }
                        else {
                            best_len_right = len;
                            if (should_reroot_tree) {
                                forest[node_right] = (uint) prev_ix;
                            }
                            node_right = LeftChildIndex(self, prev_ix);
                            prev_ix = forest[node_right];
                        }
                    }
                }
                return matches;
            }

            /* Finds all backward matches of &data[cur_ix & ring_buffer_mask] up to the
               length of max_length and stores the position cur_ix in the hash table.

               Sets *num_matches to the number of matches found, and stores the found
               matches in matches[0] to matches[*num_matches - 1]. The matches will be
               sorted by strictly increasing length and (non-strictly) increasing
               distance. */
            public static unsafe size_t FindAllMatches(HasherHandle handle,
                byte* data,
                size_t ring_buffer_mask, size_t cur_ix,
                size_t max_length, size_t max_backward,
                BrotliEncoderParams* params_, BackwardMatch* matches) {
                BackwardMatch* orig_matches = matches;
                size_t cur_ix_masked = cur_ix & ring_buffer_mask;
                size_t best_len = 1;
                size_t short_match_max_backward =
                    params_->quality != HQ_ZOPFLIFICATION_QUALITY ? 16 : 64;
                size_t stop = cur_ix - short_match_max_backward;
                uint[] dict_matches_arr = new uint[BROTLI_MAX_STATIC_DICTIONARY_MATCH_LEN + 1];
                fixed (uint* dict_matches = dict_matches_arr) {
                    size_t i;
                    if (cur_ix < short_match_max_backward) {
                        stop = 0;
                    }
                    for (i = cur_ix - 1; i > stop && best_len <= 2; --i) {
                        size_t prev_ix = i;
                        size_t backward = cur_ix - prev_ix;
                        if ((backward > max_backward)) {
                            break;
                        }
                        prev_ix &= ring_buffer_mask;
                        if (data[cur_ix_masked] != data[prev_ix] ||
                            data[cur_ix_masked + 1] != data[prev_ix + 1]) {
                            continue;
                        }
                        {
                            size_t len =
                                FindMatchLengthWithLimit(&data[prev_ix], &data[cur_ix_masked],
                                    max_length);
                            if (len > best_len) {
                                best_len = len;
                                InitBackwardMatch(matches++, backward, len);
                            }
                        }
                    }
                    if (best_len < max_length) {
                        matches = StoreAndFindMatches(Self(handle), data, cur_ix,
                            ring_buffer_mask, max_length, max_backward, &best_len, matches);
                    }
                    for (i = 0; i <= BROTLI_MAX_STATIC_DICTIONARY_MATCH_LEN; ++i) {
                        dict_matches[i] = kInvalidMatch;
                    }
                    {
                        size_t minlen = Math.Max(4, best_len + 1);
                        if (BrotliFindAllStaticDictionaryMatches(
                            &data[cur_ix_masked], minlen, max_length, &dict_matches[0])) {
                            size_t maxlen = Math.Min(
                                BROTLI_MAX_STATIC_DICTIONARY_MATCH_LEN, max_length);
                            size_t l;
                            for (l = minlen; l <= maxlen; ++l) {
                                uint dict_id = dict_matches[l];
                                if (dict_id < kInvalidMatch) {
                                    InitDictionaryBackwardMatch(matches++,
                                        max_backward + (dict_id >> 5) + 1, l, dict_id & 31);
                                }
                            }
                        }
                    }
                    return (size_t)(matches - orig_matches);
                }
            }

            /* Stores the hash of the next 4 bytes and re-roots the binary tree at the
               current sequence, without returning any matches.
               REQUIRES: ix + MAX_TREE_COMP_LENGTH <= end-of-current-block */
            public override unsafe void Store(HasherHandle handle,
                byte* data, size_t mask, size_t ix) {
                HashToBinaryTree* self = Self(handle);
                /* Maximum distance is window size - 16, see section 9.1. of the spec. */
                size_t max_backward = self->window_mask_ - BROTLI_WINDOW_GAP + 1;
                StoreAndFindMatches(self, data, ix, mask, MAX_TREE_COMP_LENGTH,
                    max_backward, null, null);
            }

            public override unsafe void StoreRange(HasherHandle handle,
                byte* data, size_t mask, size_t ix_start,
                size_t ix_end) {
                size_t i = ix_start;
                size_t j = ix_start;
                if (ix_start + 63 <= ix_end) {
                    i = ix_end - 63;
                }
                if (ix_start + 512 <= i) {
                    for (; j < i; j += 8) {
                        Store(handle, data, mask, j);
                    }
                }
                for (; i < ix_end; ++i) {
                    Store(handle, data, mask, i);
                }
            }

            public override unsafe void StitchToPreviousBlock(HasherHandle handle, size_t num_bytes, size_t position,
                byte* ringbuffer,
                size_t ringbuffer_mask) {
                HashToBinaryTree* self = Self(handle);
                if (num_bytes >= HashTypeLength() - 1 &&
                    position >= MAX_TREE_COMP_LENGTH) {
                    /* Store the last `MAX_TREE_COMP_LENGTH - 1` positions in the hasher.
                       These could not be calculated before, since they require knowledge
                       of both the previous and the current block. */
                    size_t i_start = position - MAX_TREE_COMP_LENGTH + 1;
                    size_t i_end = Math.Min(position, i_start + num_bytes);
                    size_t i;
                    for (i = i_start; i < i_end; ++i) {
                        /* Maximum distance is window size - 16, see section 9.1. of the spec.
                           Furthermore, we have to make sure that we don't look further back
                           from the start of the next block than the window size, otherwise we
                           could access already overwritten areas of the ring-buffer. */
                        size_t max_backward =
                            self->window_mask_ - Math.Max(
                                BROTLI_WINDOW_GAP - 1,
                                position - i);
                        /* We know that i + MAX_TREE_COMP_LENGTH <= position + num_bytes, i.e. the
                           end of the current block and that we have at least
                           MAX_TREE_COMP_LENGTH tail in the ring-buffer. */
                        StoreAndFindMatches(self, ringbuffer, i, ringbuffer_mask,
                            MAX_TREE_COMP_LENGTH, max_backward, null, null);
                    }
                }
            }

            public override unsafe void PrepareDistanceCache(HasherHandle handle, int* distance_cache) {
                throw new InvalidOperationException();
            }

            public override unsafe bool FindLongestMatch(HasherHandle handle,
                ushort* dictionary_hash,
                byte* data, size_t ring_buffer_mask,
                int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_) {
                throw new InvalidOperationException();
            }

            public override unsafe void CreateBackwardReferences(ushort* dictionary_hash, size_t num_bytes,
                size_t position, byte* ringbuffer, size_t ringbuffer_mask, BrotliEncoderParams* params_,
                HasherHandle hasher, int* dist_cache, size_t* last_insert_len, Command* commands, size_t* num_commands,
                size_t* num_literals) {
                throw new InvalidOperationException();
            }
        }
    }
}