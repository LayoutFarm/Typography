using System.Collections.Generic;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using score_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static readonly Dictionary<int, Hasher> kHashers =
            new Dictionary<int, Hasher> {
                {10, new HashToBinaryTreeH10()},
                {2, new HashLongestMatchQuicklyH2()},
                {3, new HashLongestMatchQuicklyH3()},
                {4, new HashLongestMatchQuicklyH4()},
                {5, new HashLongestMatchH5()},
                {6, new HashLongestMatch64H6()},
                {40, new HashForgetfulChainH40()},
                {41, new HashForgetfulChainH41()},
                {42, new HashForgetfulChainH42()},
                {54, new HashLongestMatchQuicklyH54()}
            };

        [StructLayout(LayoutKind.Sequential)]
        private struct HasherCommon {
            public BrotliHasherParams params_;

            /* False if hasher needs to be "prepared" before use. */
            public bool is_prepared_;

            public size_t dict_num_lookups;
            public size_t dict_num_matches;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BackwardMatch {
            public uint distance;
            public uint length_and_code;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HasherSearchResult {
            public size_t len;
            public size_t len_x_code; /* == len ^ len_code */
            public size_t distance;
            public score_t score;
        }

        private static unsafe void DestroyHasher(
            ref MemoryManager m, HasherHandle* handle)
        {
            if ((void*)(*handle) == null) return;
            BrotliFree(ref m, *handle);
        }

        private static unsafe score_t BackwardReferenceScoreUsingLastDistance(
            size_t copy_length)
        {
            return BROTLI_LITERAL_BYTE_SCORE * (score_t)copy_length +
                   BROTLI_SCORE_BASE + 15;
        }

        private static unsafe void PrepareDistanceCache(
            int* distance_cache, int num_distances)
        {
            if (num_distances > 4)
            {
                int last_distance = distance_cache[0];
                distance_cache[4] = last_distance - 1;
                distance_cache[5] = last_distance + 1;
                distance_cache[6] = last_distance - 2;
                distance_cache[7] = last_distance + 2;
                distance_cache[8] = last_distance - 3;
                distance_cache[9] = last_distance + 3;
                if (num_distances > 10)
                {
                    int next_last_distance = distance_cache[1];
                    distance_cache[10] = next_last_distance - 1;
                    distance_cache[11] = next_last_distance + 1;
                    distance_cache[12] = next_last_distance - 2;
                    distance_cache[13] = next_last_distance + 2;
                    distance_cache[14] = next_last_distance - 3;
                    distance_cache[15] = next_last_distance + 3;
                }
            }
        }

        /* Usually, we always choose the longest backward reference. This function
           allows for the exception of that rule.

           If we choose a backward reference that is further away, it will
           usually be coded with more bits. We approximate this by assuming
           log2(distance). If the distance can be expressed in terms of the
           last four distances, we use some heuristic ants to estimate
           the bits cost. For the first up to four literals we use the bit
           cost of the literals from the literal cost model, after that we
           use the average bit cost of the cost model.

           This function is used to sometimes discard a longer backward reference
           when it is not much longer and the bit cost for encoding it is more
           than the saved literals.

           backward_reference_offset MUST be positive. */
        private static unsafe score_t BackwardReferenceScore(
            size_t copy_length, size_t backward_reference_offset)
        {
            return BROTLI_SCORE_BASE + BROTLI_LITERAL_BYTE_SCORE * (score_t)copy_length -
                   BROTLI_DISTANCE_BIT_PENALTY * Log2FloorNonZero(backward_reference_offset);
        }

        private static unsafe uint Hash14(byte* data)
        {
            uint h = *(uint*)(data) * kHashMul32;
            /* The higher bits contain more mixture from the multiplication,
               so we take our results from there. */
            return h >> (32 - 14);
        }


        private static unsafe bool TestStaticDictionaryItem(
            size_t item, byte* data,
            size_t max_length, size_t max_backward, HasherSearchResult* out_)
        {
            size_t len;
            size_t dist;
            size_t offset;
            size_t matchlen;
            size_t backward;
            score_t score;
            len = item & 0x1F;
            dist = item >> 5;
            offset = kBrotliDictionaryOffsetsByLength[len] + len * dist;
            if (len > max_length)
            {
                return false;
            }

            fixed (byte* dict = kBrotliDictionary)
            matchlen =
                FindMatchLengthWithLimit(data, &dict[offset], len);
            if (matchlen + kCutoffTransformsCount <= len || matchlen == 0)
            {
                return false;
            }
            {
                size_t cut = len - matchlen;
                size_t transform_id =
                    (cut << 2) + (size_t)((kCutoffTransforms >> (int) (cut * 6)) & 0x3F);
                backward = max_backward + dist + 1 +
                           (transform_id << kBrotliDictionarySizeBitsByLength[len]);
            }
            score = BackwardReferenceScore(matchlen, backward);
            if (score < out_->score) {
                return false;
            }
            out_->len = matchlen;
            out_->len_x_code = len ^ matchlen;
            out_->distance = backward;
            out_->score = score;
            return true;
        }

        private static unsafe bool SearchInStaticDictionary(
            ushort* dictionary_hash,
            HasherHandle handle, byte* data, size_t max_length,
            size_t max_backward, HasherSearchResult* out_, bool shallow)
        {
            size_t key;
            size_t i;
            bool is_match_found = false;
            HasherCommon* self = GetHasherCommon(handle);
            if (self->dict_num_matches < (self->dict_num_lookups >> 7))
            {
                return false;
            }
            key = Hash14(data) << 1;
            for (i = 0; i < (shallow ? 1u : 2u); ++i, ++key)
            {
                size_t item = dictionary_hash[key];
                self->dict_num_lookups++;
                if (item != 0)
                {
                    bool item_matches = TestStaticDictionaryItem(
                        item, data, max_length, max_backward, out_);
                    if (item_matches)
                    {
                        self->dict_num_matches++;
                        is_match_found = true;
                    }
                }
            }
            return is_match_found;
        }

        private static score_t BackwardReferencePenaltyUsingLastDistance(
            size_t distance_short_code)
        {
            return (score_t)39 + ((0x1CA10 >> (int) (distance_short_code & 0xE)) & 0xE);
        }

        private static unsafe size_t BackwardMatchLength(BackwardMatch* self)
        {
            return self->length_and_code >> 5;
        }

        private static unsafe size_t BackwardMatchLengthCode(BackwardMatch* self)
        {
            size_t code = self->length_and_code & 31;
            return code != 0 ? code : BackwardMatchLength(self);
        }

        private static unsafe void InitBackwardMatch(BackwardMatch* self,
            size_t dist, size_t len) {
            self->distance = (uint) dist;
            self->length_and_code = (uint) (len << 5);
        }

        private static unsafe void InitDictionaryBackwardMatch(BackwardMatch* self,
            size_t dist, size_t len, size_t len_code)
        {
            self->distance = (uint)dist;
            self->length_and_code =
                (uint)((len << 5) | (len == len_code ? 0 : len_code));
        }

        private static unsafe HasherCommon* GetHasherCommon(HasherHandle handle) {
            return (HasherCommon*) handle;
        }

        private static unsafe void HasherReset(HasherHandle handle) {
            if ((void*) handle == null) return;
            GetHasherCommon(handle)->is_prepared_ = false;
        }

        private static unsafe size_t HasherSize(BrotliEncoderParams* params_,
            bool one_shot, size_t input_size) {
            size_t result = sizeof(HasherCommon);
            Hasher h;
            if (kHashers.TryGetValue(params_->hasher.type, out h))
                result += h.HashMemAllocInBytes(params_, one_shot, input_size);
            return result;
        }

        private static unsafe void HasherSetup(ref MemoryManager m, HasherHandle* handle,
            BrotliEncoderParams* params_, byte* data, size_t position,
            size_t input_size, bool is_last) {
            HasherHandle self = null;
            HasherCommon* common = null;
            bool one_shot = (position == 0 && is_last);
            if ((byte*) (*handle) == null) {
                size_t alloc_size;
                ChooseHasher(params_, &params_->hasher);
                alloc_size = HasherSize(params_, one_shot, input_size);
                self = BrotliAllocate(ref m, alloc_size);
                *handle = self;
                common = GetHasherCommon(self);
                common->params_ = params_->hasher;
                Hasher h;
                if (kHashers.TryGetValue(params_->hasher.type, out h))
                    h.Initialize(*handle, params_);
                HasherReset(*handle);
            }

            self = *handle;
            common = GetHasherCommon(self);
            if (!common->is_prepared_) {
                Hasher h;
                if (kHashers.TryGetValue(params_->hasher.type, out h))
                    h.Prepare(self, one_shot, input_size, data);
                if (position == 0) {
                    common->dict_num_lookups = 0;
                    common->dict_num_matches = 0;
                }
                common->is_prepared_ = true;
            }
        }

        /* Custom LZ77 window. */
        private static unsafe void HasherPrependCustomDictionary(
            ref MemoryManager m, HasherHandle* handle, BrotliEncoderParams* params_,
            size_t size, byte* dict) {
            size_t overlap;
            size_t i;
            HasherHandle self;
            HasherSetup(ref m, handle, params_, dict, 0, size, false);
            self = *handle;
            Hasher h;
            if (kHashers.TryGetValue(GetHasherCommon(self)->params_.type, out h)) {
                overlap = h.StoreLookahead() - 1;
                for (i = 0; i + overlap < size; i++)
                    h.Store(self, dict, ~(size_t) 0, i);
            }
        }

        private static unsafe void InitOrStitchToPreviousBlock(
            ref MemoryManager m, HasherHandle* handle, byte* data, size_t mask,
            BrotliEncoderParams* params_, size_t position, size_t input_size,
            bool is_last)
        {
            HasherHandle self;
            HasherSetup(ref m, handle, params_, data, position, input_size, is_last);
            self = *handle;
            Hasher h;
            if (kHashers.TryGetValue(GetHasherCommon(self)->params_.type, out h))
                h.StitchToPreviousBlock(self, input_size, position, data, mask);
        }
    }
}