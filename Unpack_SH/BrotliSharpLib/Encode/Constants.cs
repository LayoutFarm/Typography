using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        internal const int BROTLI_MIN_WINDOW_BITS = 10;
        internal const int BROTLI_MAX_WINDOW_BITS = 24;
        private const int BROTLI_MIN_INPUT_BLOCK_BITS = 16;
        private const int BROTLI_MAX_INPUT_BLOCK_BITS = 24;
        internal const int BROTLI_MIN_QUALITY = 0;
        internal const int BROTLI_MAX_QUALITY = 11;

        private const int FAST_ONE_PASS_COMPRESSION_QUALITY = 0;
        private const int FAST_TWO_PASS_COMPRESSION_QUALITY = 1;
        private const int ZOPFLIFICATION_QUALITY = 10;
        private const int HQ_ZOPFLIFICATION_QUALITY = 11;

        private const int MAX_QUALITY_FOR_STATIC_ENTROPY_CODES = 2;
        private const int MIN_QUALITY_FOR_BLOCK_SPLIT = 4;
        private const int MIN_QUALITY_FOR_OPTIMIZE_HISTOGRAMS = 4;
        private const int MIN_QUALITY_FOR_EXTENSIVE_REFERENCE_SEARCH = 5;
        private const int MIN_QUALITY_FOR_CONTEXT_MODELING = 5;
        private const int MIN_QUALITY_FOR_HQ_CONTEXT_MODELING = 7;

        private const int BROTLI_MAX_STATIC_CONTEXTS = 13;

        private const int HISTOGRAMS_PER_BATCH = 64;
        private const int CLUSTERS_PER_BATCH = 16;

        private const int BROTLI_MAX_NUMBER_OF_BLOCK_TYPES = 256;

        private const int BROTLI_CONTEXT_MAP_MAX_RLE = 16;
        private const int BROTLI_MAX_CONTEXT_MAP_SYMBOLS = BROTLI_MAX_NUMBER_OF_BLOCK_TYPES +
                                                           BROTLI_CONTEXT_MAP_MAX_RLE;
        private const int BROTLI_MAX_BLOCK_TYPE_SYMBOLS = (BROTLI_MAX_NUMBER_OF_BLOCK_TYPES + 2);

        private const int MAX_HUFFMAN_TREE_SIZE = (2 * BROTLI_NUM_COMMAND_SYMBOLS + 1);
        private const int SIMPLE_DISTANCE_ALPHABET_SIZE =
            (BROTLI_NUM_DISTANCE_SHORT_CODES + (2 * BROTLI_MAX_DISTANCE_BITS));

        private const int SIMPLE_DISTANCE_ALPHABET_BITS = 6;

        private const int MAX_NUM_DELAYED_SYMBOLS = 0x2fff;

        private const int MAX_ZOPFLI_LEN_QUALITY_10 = 150;
        private const int MAX_ZOPFLI_LEN_QUALITY_11 = 325;
        private const int BROTLI_LONG_COPY_QUICK_STEP = 16384;

        private const int BROTLI_LITERAL_BYTE_SCORE = 135;
        private const int BROTLI_DISTANCE_BIT_PENALTY = 30;
        /* Score must be positive after applying maximal penalty. */
        private static readonly unsafe int BROTLI_SCORE_BASE = BROTLI_DISTANCE_BIT_PENALTY * 8 * sizeof(size_t);

        private const uint kCutoffTransformsCount = 10;
        /*   0,  12,   27,    23,    42,    63,    56,    48,    59,    64 */
        /* 0+0, 4+8, 8+19, 12+11, 16+26, 20+43, 24+32, 28+20, 32+27, 36+28 */
        private const ulong kCutoffTransforms = 0x071B520ADA2D3200;

        private const int BROTLI_MAX_STATIC_DICTIONARY_MATCH_LEN = 37;
        private const uint kInvalidMatch = 0xffffffff;

        private const int MAX_NUM_MATCHES_H10 = 128;

        private const int MIN_QUALITY_FOR_HQ_BLOCK_SPLITTING = 10;

        /* Only for "font" mode. */
        private const int MIN_QUALITY_FOR_RECOMPUTE_DISTANCE_PREFIXES = 10;

        private const uint kHashMul32 = 0x1e35a7bd;
        private const ulong kHashMul64 = 0x1e35a7bd1e35a7bd;
        private const ulong kHashMul64Long = 0x1fe35a7bd3579bd3;
        private const int kDictNumBits = 15;
        private const uint kDictHashMul32 = 0x1e35a7bd;

        private const float kInfinity = 1.7e38f;  /* ~= 2 ^ 127 */

        private const byte kUppercaseFirst = 10;

        private const double kMinUTF8Ratio = 0.75f;

        private const int MAX_HUFFMAN_BITS = 16;

        private static readonly size_t kCompressFragmentTwoPassBlockSize = 1 << 17;

        private const int BROTLI_DEFAULT_QUALITY = 11;
        private const int BROTLI_DEFAULT_WINDOW = 22;
        private const BrotliEncoderMode BROTLI_DEFAULT_MODE = BrotliEncoderMode.BROTLI_MODE_GENERIC;
    }
}