using System;
using System.Runtime.InteropServices;
using reg_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private const int BROTLI_HUFFMAN_MAX_CODE_LENGTH = 15;


        private const int BROTLI_HUFFMAN_MAX_SIZE_26 = 396;
        private const int BROTLI_HUFFMAN_MAX_SIZE_258 = 632;
        private const int BROTLI_HUFFMAN_MAX_SIZE_272 = 646;
        private const int BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH = 5;
        private const int BROTLI_REVERSE_BITS_MAX = 8;
        private const int BROTLI_REVERSE_BITS_BASE = 0;

        private const int HUFFMAN_TABLE_MASK = 0xff;
        private const int BROTLI_MIN_DICTIONARY_WORD_LENGTH = 4;
        private const int BROTLI_MAX_DICTIONARY_WORD_LENGTH = 24;

        private const int HUFFMAN_TABLE_BITS = 8;

        private static readonly reg_t BROTLI_REVERSE_BITS_LOWEST = (reg_t) 1 <<
                                                                   (BROTLI_REVERSE_BITS_MAX - 1 +
                                                                    BROTLI_REVERSE_BITS_BASE);

        private static readonly int BROTLI_SHORT_FILL_BIT_WINDOW_READ =
#if SIZE_OF_T
            Marshal.SizeOf<reg_t>()
#else
            Marshal.SizeOf(typeof(reg_t))
#endif
            >> 1;
    }
}