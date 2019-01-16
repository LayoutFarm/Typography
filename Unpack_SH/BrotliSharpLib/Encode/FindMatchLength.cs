using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static unsafe size_t FindMatchLengthWithLimit(byte* s1,
            byte* s2,
            size_t limit) {
            size_t matched = 0;
            byte* s2_limit = s2 + limit;
            byte* s2_ptr = s2;
            /* Find out how long the match is. We loop over the data 32 bits at a
               time until we find a 32-bit block that doesn't match; then we find
               the first non-matching bit and use that to calculate the total
               length of the match. */
            while (s2_ptr <= s2_limit - 4 &&
                   *(uint*) (s2_ptr) ==
                   *(uint*) (s1 + matched)) {
                s2_ptr += 4;
                matched += 4;
            }
            while ((s2_ptr < s2_limit) && (s1[matched] == *s2_ptr)) {
                ++s2_ptr;
                ++matched;
            }
            return matched;
        }
    }
}