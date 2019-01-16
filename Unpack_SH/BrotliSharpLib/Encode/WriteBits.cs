using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        /* This function writes bits into bytes in increasing addresses, and within
           a byte least-significant-bit first.

           The function can write up to 56 bits in one go with WriteBits
           Example: let's assume that 3 bits (Rs below) have been written already:

           BYTE-0     BYTE+1       BYTE+2

           0000 0RRR    0000 0000    0000 0000

           Now, we could write 5 or less bits in MSB by just sifting by 3
           and OR'ing to BYTE-0.

           For n bits, we take the last 5 bits, OR that with high bits in BYTE-0,
           and locate the rest in BYTE+1, BYTE+2, etc. */
        private static unsafe void BrotliWriteBits(size_t n_bits,
            ulong bits,
            size_t* pos,
            byte* array) {
            if (BROTLI_LITTLE_ENDIAN) {
                byte* p = &array[*pos >> 3];
                ulong v = *p;
                v |= bits << (int) (*pos & 7);
                *(ulong*) p = v; /* Set some bits. */
                *pos += n_bits;
            }
            else {
                /* implicit & 0xff is assumed for uint8_t arithmetics */
                byte* array_pos = &array[*pos >> 3];
                size_t bits_reserved_in_first_byte = (*pos & 7);
                size_t bits_left_to_write;
                bits <<= (int) bits_reserved_in_first_byte;
                *array_pos++ |= (byte) bits;
                for (bits_left_to_write = n_bits + bits_reserved_in_first_byte;
                    bits_left_to_write >= 9;
                    bits_left_to_write -= 8) {
                    bits >>= 8;
                    *array_pos++ = (byte) bits;
                }
                *array_pos = 0;
                *pos += n_bits;
            }
        }

        private static unsafe void BrotliWriteBitsPrepareStorage(
            size_t pos, byte* array)
        {
            array[pos >> 3] = 0;
        }
    }
}