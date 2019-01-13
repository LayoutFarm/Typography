using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private const int BROTLI_NUM_LITERAL_SYMBOLS = 256;
        private const int BROTLI_NUM_COMMAND_SYMBOLS = 704;
        private const int BROTLI_NUM_BLOCK_LEN_SYMBOLS = 26;

        private const int BROTLI_MAX_NPOSTFIX = 3;
        private const int BROTLI_MAX_NDIRECT = 120;
        private const int BROTLI_NUM_DISTANCE_SYMBOLS = (BROTLI_NUM_DISTANCE_SHORT_CODES +
                                                         BROTLI_MAX_NDIRECT +
                                                         (BROTLI_MAX_DISTANCE_BITS <<
                                                          (BROTLI_MAX_NPOSTFIX + 1)));

        private const int BROTLI_REPEAT_PREVIOUS_CODE_LENGTH = 16;
        private const int BROTLI_REPEAT_ZERO_CODE_LENGTH = 17;
        private const int BROTLI_CODE_LENGTH_CODES = BROTLI_REPEAT_ZERO_CODE_LENGTH + 1;
        private const int BROTLI_INITIAL_REPEATED_CODE_LENGTH = 8;

        private const int BROTLI_NUM_DISTANCE_SHORT_CODES = 16;
        private const int BROTLI_MAX_DISTANCE_BITS = 24;

        private const int BROTLI_LITERAL_CONTEXT_BITS = 6;
        private const int BROTLI_DISTANCE_CONTEXT_BITS = 2;

        private const int BROTLI_WINDOW_GAP = 16;

        private static size_t BROTLI_MAX_BACKWARD_LIMIT(size_t W) {
            return ((size_t) 1 << (int) W) - BROTLI_WINDOW_GAP;
        }

#if X86 || X64
        private const bool BROTLI_ALIGNED_READ = false;
#else
        private static readonly bool BROTLI_ALIGNED_READ = !IsWhitelistedCPU();
#endif

        private static readonly Endianess BYTE_ORDER = GetEndianess();
        private static readonly bool BROTLI_LITTLE_ENDIAN = BYTE_ORDER == Endianess.Little;
        private static readonly bool BROTLI_BIG_ENDIAN = BYTE_ORDER == Endianess.Big;
    }
}