using size_t = BrotliSharpLib.Brotli.SizeT;
using System.Collections.Generic;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private abstract unsafe class Hasher {
            public abstract size_t StoreLookahead();
            public abstract size_t HashTypeLength();

            public abstract size_t HashMemAllocInBytes(BrotliEncoderParams* params_, bool one_shot, size_t input_size);
            public abstract void Initialize(HasherHandle handle, BrotliEncoderParams* params_);
            public abstract void Prepare(HasherHandle handle, bool one_shot, size_t input_size, byte* data);

            public abstract void StoreRange(HasherHandle handle,
                byte* data, size_t mask, size_t ix_start,
                size_t ix_end);

            public abstract void Store(HasherHandle handle, byte* data, size_t mask, size_t ix);

            public abstract void StitchToPreviousBlock(HasherHandle handle, size_t num_bytes, size_t position,
                byte* ringbuffer, size_t ringbuffer_mask);

            public abstract void PrepareDistanceCache(HasherHandle handle, int* distance_cache);

            public abstract bool FindLongestMatch(HasherHandle handle,
                ushort* dictionary_hash,
                byte* data, size_t ring_buffer_mask,
                int* distance_cache,
                size_t cur_ix, size_t max_length, size_t max_backward,
                HasherSearchResult* out_);

            public abstract void CreateBackwardReferences(
                ushort* dictionary_hash,
                size_t num_bytes, size_t position,
                byte* ringbuffer, size_t ringbuffer_mask,
                BrotliEncoderParams* params_, HasherHandle hasher, int* dist_cache,
                size_t* last_insert_len, Command* commands, size_t* num_commands,
                size_t* num_literals);
        }
    }
}