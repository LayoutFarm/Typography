using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private static unsafe size_t BrotliParseAsUTF8(
            int* symbol, byte* input, size_t size)
        {
            /* ASCII */
            if ((input[0] & 0x80) == 0)
            {
                *symbol = input[0];
                if (*symbol > 0)
                {
                    return 1;
                }
            }
            /* 2-byte UTF8 */
            if (size > 1u &&
                (input[0] & 0xe0) == 0xc0 &&
                (input[1] & 0xc0) == 0x80)
            {
                *symbol = (((input[0] & 0x1f) << 6) |
                           (input[1] & 0x3f));
                if (*symbol > 0x7f)
                {
                    return 2;
                }
            }
            /* 3-byte UFT8 */
            if (size > 2u &&
                (input[0] & 0xf0) == 0xe0 &&
                (input[1] & 0xc0) == 0x80 &&
                (input[2] & 0xc0) == 0x80)
            {
                *symbol = (((input[0] & 0x0f) << 12) |
                           ((input[1] & 0x3f) << 6) |
                           (input[2] & 0x3f));
                if (*symbol > 0x7ff)
                {
                    return 3;
                }
            }
            /* 4-byte UFT8 */
            if (size > 3u &&
                (input[0] & 0xf8) == 0xf0 &&
                (input[1] & 0xc0) == 0x80 &&
                (input[2] & 0xc0) == 0x80 &&
                (input[3] & 0xc0) == 0x80)
            {
                *symbol = (((input[0] & 0x07) << 18) |
                           ((input[1] & 0x3f) << 12) |
                           ((input[2] & 0x3f) << 6) |
                           (input[3] & 0x3f));
                if (*symbol > 0xffff && *symbol <= 0x10ffff)
                {
                    return 4;
                }
            }
            /* Not UTF8, emit a special symbol above the UTF8-code space */
            *symbol = 0x110000 | input[0];
            return 1;
        }

        /* Returns 1 if at least min_fraction of the data is UTF8-encoded.*/
        private static unsafe bool BrotliIsMostlyUTF8(
            byte* data, size_t pos, size_t mask,
            size_t length, double min_fraction)
        {
            size_t size_utf8 = 0;
            size_t i = 0;
            while (i < length)
            {
                int symbol;
                size_t bytes_read =
                    BrotliParseAsUTF8(&symbol, &data[(pos + i) & mask], length - i);
                i += bytes_read;
                if (symbol < 0x110000) size_utf8 += bytes_read;
            }
            return (size_utf8 > min_fraction * (double)length);
        }
    }
}