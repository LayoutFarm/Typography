namespace BrotliSharpLib {
    public static partial class Brotli {
        private static unsafe int ToUpperCase(byte* p) {
            if (p[0] < 0xc0) {
                if (p[0] >= 'a' && p[0] <= 'z') {
                    p[0] ^= 32;
                }
                return 1;
            }
            /* An overly simplified uppercasing model for UTF-8. */
            if (p[0] < 0xe0) {
                p[1] ^= 32;
                return 2;
            }
            /* An arbitrary transform for three byte characters. */
            p[2] ^= 5;
            return 3;
        }

        private static unsafe int TransformDictionaryWord(
            byte* dst, byte* word, int len, int transform) {
            var idx = 0;
            {
                fixed (char* kps = kPrefixSuffix) {
                    var prefix = &kps[kTransforms[transform].prefix_id];
                    while (*prefix != 0) {
                        dst[idx++] = (byte) *prefix++;
                    }
                }
            }
            {
                int t = kTransforms[transform].transform;
                var i = 0;
                var skip = t - ((int) WordTransformType.kOmitFirst1 - 1);
                if (skip > 0) {
                    word += skip;
                    len -= skip;
                }
                else if (t <= (int) WordTransformType.kOmitLast9) {
                    len -= t;
                }
                while (i < len) {
                    dst[idx++] = word[i++];
                }
                if (t == (int) WordTransformType.kUppercaseFirst) {
                    ToUpperCase(&dst[idx - len]);
                }
                else if (t == (int) WordTransformType.kUppercaseAll) {
                    var uppercase = &dst[idx - len];
                    while (len > 0) {
                        var step = ToUpperCase(uppercase);
                        uppercase += step;
                        len -= step;
                    }
                }
            }
            {
                fixed (char* kps = kPrefixSuffix) {
                    var suffix = &kps[kTransforms[transform].suffix_id];
                    while (*suffix != 0) {
                        dst[idx++] = (byte) *suffix++;
                    }
                }
                return idx;
            }
        }
    }
}