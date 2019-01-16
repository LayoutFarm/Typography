using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private static unsafe size_t ComputeDistanceCode(size_t distance,
            size_t max_distance,
            int* dist_cache)
        {
            if (distance <= max_distance)
            {
                size_t distance_plus_3 = distance + 3;
                size_t offset0 = distance_plus_3 - (size_t)dist_cache[0];
                size_t offset1 = distance_plus_3 - (size_t)dist_cache[1];
                if (distance == (size_t)dist_cache[0])
                {
                    return 0;
                }
                else if (distance == (size_t)dist_cache[1])
                {
                    return 1;
                }
                else if (offset0 < 7)
                {
                    return (0x9750468 >> (int) (4 * offset0)) & 0xF;
                }
                else if (offset1 < 7)
                {
                    return (0xFDB1ACE >> (int) (4 * offset1)) & 0xF;
                }
                else if (distance == (size_t)dist_cache[2])
                {
                    return 2;
                }
                else if (distance == (size_t)dist_cache[3])
                {
                    return 3;
                }
            }
            return distance + BROTLI_NUM_DISTANCE_SHORT_CODES - 1;
        }

        private static unsafe void BrotliCreateBackwardReferences(
            size_t num_bytes,
            size_t position,
            byte* ringbuffer,
            size_t ringbuffer_mask,
            BrotliEncoderParams* params_,
            HasherHandle hasher,
            int* dist_cache,
            size_t* last_insert_len,
            Command* commands,
            size_t* num_commands,
            size_t* num_literals)
        {
            switch (params_->hasher.type) {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 40:
                case 41:
                case 42:
                case 54:
                    fixed (ushort* ksdh = kStaticDictionaryHash)
                        kHashers[params_->hasher.type].CreateBackwardReferences(ksdh, num_bytes,
                            position, ringbuffer, ringbuffer_mask, params_, hasher, dist_cache,
                            last_insert_len, commands, num_commands, num_literals);
                    break;
            }
        }
    }
}