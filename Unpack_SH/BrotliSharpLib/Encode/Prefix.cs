using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        /* Here distance_code is an intermediate code, i.e. one of the special codes or
           the actual distance increased by BROTLI_NUM_DISTANCE_SHORT_CODES - 1. */
        private static unsafe void PrefixEncodeCopyDistance(size_t distance_code,
            size_t num_direct_codes,
            size_t postfix_bits,
            ushort* code,
            uint* extra_bits)
        {
            if (distance_code < BROTLI_NUM_DISTANCE_SHORT_CODES + num_direct_codes)
            {
                *code = (ushort)distance_code;
                *extra_bits = 0;
                return;
            }
            else
            {
                size_t dist = ((size_t)1 << (int) (postfix_bits + 2u)) +
                              (distance_code - BROTLI_NUM_DISTANCE_SHORT_CODES - num_direct_codes);
                size_t bucket = Log2FloorNonZero(dist) - 1;
                size_t postfix_mask = (1u << (int)postfix_bits) - 1;
                size_t postfix = dist & postfix_mask;
                size_t prefix = (dist >> (int)bucket) & 1;
                size_t offset = (2 + prefix) << (int)bucket;
                size_t nbits = bucket - postfix_bits;
                *code = (ushort)(
                    (BROTLI_NUM_DISTANCE_SHORT_CODES + num_direct_codes +
                     ((2 * (nbits - 1) + prefix) << (int) postfix_bits) + postfix));
                *extra_bits = (uint)(
                    (nbits << 24) | ((dist - offset) >> (int) postfix_bits));
            }
        }
    }
}