using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        internal enum BrotliEncoderStreamState {
            /* Default state. */
            BROTLI_STREAM_PROCESSING = 0,

            /* Intermediate state; after next block is emitted, byte-padding should be
               performed before getting back to default state. */
            BROTLI_STREAM_FLUSH_REQUESTED = 1,

            /* Last metablock was produced; no more input is acceptable. */
            BROTLI_STREAM_FINISHED = 2,

            /* Flushing compressed block and writing meta-data block header. */
            BROTLI_STREAM_METADATA_HEAD = 3,

            /* Writing metadata block body. */
            BROTLI_STREAM_METADATA_BODY = 4
        }

        internal enum BrotliEncoderOperation {
            /**
             * Process input.
             *
             * Encoder may postpone producing output, until it has processed enough input.
             */
            BROTLI_OPERATION_PROCESS = 0,

            /**
             * Produce output for all processed input.
             *
             * Actual flush is performed when input stream is depleted and there is enough
             * space in output stream. This means that client should repeat
             * ::BROTLI_OPERATION_FLUSH operation until @p available_in becomes @c 0, and
             * ::BrotliEncoderHasMoreOutput returns ::BROTLI_FALSE.
             *
             * @warning Until flush is complete, client @b SHOULD @b NOT swap,
             *          reduce or extend input stream.
             *
             * When flush is complete, output data will be sufficient for decoder to
             * reproduce all the given input.
             */
            BROTLI_OPERATION_FLUSH = 1,

            /**
             * Finalize the stream.
             *
             * Actual finalization is performed when input stream is depleted and there is
             * enough space in output stream. This means that client should repeat
             * ::BROTLI_OPERATION_FLUSH operation until @p available_in becomes @c 0, and
             * ::BrotliEncoderHasMoreOutput returns ::BROTLI_FALSE.
             *
             * @warning Until finalization is complete, client @b SHOULD @b NOT swap,
             *          reduce or extend input stream.
             *
             * Helper function ::BrotliEncoderIsFinished checks if stream is finalized and
             * output fully dumped.
             *
             * Adding more input data to finalized stream is impossible.
             */
            BROTLI_OPERATION_FINISH = 2,

            /**
             * Emit metadata block to stream.
             *
             * Metadata is opaque to Brotli: neither encoder, nor decoder processes this
             * data or relies on it. It may be used to pass some extra information from
             * encoder client to decoder client without interfering with main data stream.
             *
             * @note Encoder may emit empty metadata blocks internally, to pad encoded
             *       stream to byte boundary.
             *
             * @warning Until emitting metadata is complete client @b SHOULD @b NOT swap,
             *          reduce or extend input stream.
             *
             * @warning The whole content of input buffer is considered to be the content
             *          of metadata block. Do @b NOT @e append metadata to input stream,
             *          before it is depleted with other operations.
             *
             * Stream is soft-flushed before metadata block is emitted. Metadata block
             * @b MUST be no longer than than 16MiB.
             */
            BROTLI_OPERATION_EMIT_METADATA = 3
        }

        internal enum BrotliEncoderMode {
            /**
             * Default compression mode.
             *
             * In this mode compressor does not know anything in advance about the
             * properties of the input.
             */
            BROTLI_MODE_GENERIC = 0,

            /** Compression mode for UTF-8 formatted text input. */
            BROTLI_MODE_TEXT = 1,

            /** Compression mode used in WOFF 2.0. */
            BROTLI_MODE_FONT = 2
        }

        internal enum BrotliEncoderParameter {
            /**
             * Tune encoder for specific input.
             *
             * ::BrotliEncoderMode enumerates all available values.
             */
            BROTLI_PARAM_MODE = 0,

            /**
             * The main compression speed-density lever.
             *
             * The higher the quality, the slower the compression. Range is
             * from ::BROTLI_MIN_QUALITY to ::BROTLI_MAX_QUALITY.
             */
            BROTLI_PARAM_QUALITY = 1,

            /**
             * Recommended sliding LZ77 window size.
             *
             * Encoder may reduce this value, e.g. if input is much smaller than
             * window size.
             *
             * Window size is `(1 << value) - 16`.
             *
             * Range is from ::BROTLI_MIN_WINDOW_BITS to ::BROTLI_MAX_WINDOW_BITS.
             */
            BROTLI_PARAM_LGWIN = 2,

            /**
             * Recommended input block size.
             *
             * Encoder may reduce this value, e.g. if input is much smaller than input
             * block size.
             *
             * Range is from ::BROTLI_MIN_INPUT_BLOCK_BITS to
             * ::BROTLI_MAX_INPUT_BLOCK_BITS.
             *
             * @note Bigger input block size allows better compression, but consumes more
             *       memory. \n The rough formula of memory used for temporary input
             *       storage is `3 << lgBlock`.
             */
            BROTLI_PARAM_LGBLOCK = 3,

            /**
             * Flag that affects usage of "literal context modeling" format feature.
             *
             * This flag is a "decoding-speed vs compression ratio" trade-off.
             */
            BROTLI_PARAM_DISABLE_LITERAL_CONTEXT_MODELING = 4,

            /**
             * Estimated total input size for all ::BrotliEncoderCompressStream calls.
             *
             * The default value is 0, which means that the total input size is unknown.
             */
            BROTLI_PARAM_SIZE_HINT = 5
        }

        internal unsafe struct HasherHandle {
            private byte* handle;

            public HasherHandle(void* h) {
                handle = (byte*) h;
            }

            public static implicit operator void*(HasherHandle h) {
                return h.handle;
            }

            public static implicit operator HasherHandle(void* h) {
                return new HasherHandle(h);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct BrotliEncoderStateStruct {
            public BrotliEncoderParams params_;

            public MemoryManager memory_manager_;

            public HasherHandle hasher_;
            public ulong input_pos_;
            public RingBuffer ringbuffer_;
            public size_t cmd_alloc_size_;
            public Command* commands_;
            public size_t num_commands_;
            public size_t num_literals_;
            public size_t last_insert_len_;
            public ulong last_flush_pos_;
            public ulong last_processed_pos_;

            public fixed int dist_cache_[BROTLI_NUM_DISTANCE_SHORT_CODES];
            public fixed int saved_dist_cache_[4];
            public byte last_byte_;
            public byte last_byte_bits_;
            public byte prev_byte_;
            public byte prev_byte2_;
            public size_t storage_size_;

            public byte* storage_;

            /* Hash table for FAST_ONE_PASS_COMPRESSION_QUALITY mode. */
            public fixed int small_table_[1 << 10]; /* 4KiB */

            public int* large_table_; /* Allocated only when needed */

            public size_t large_table_size_;

            /* Command and distance prefix codes (each 64 symbols, stored back-to-back)
               used for the next block in FAST_ONE_PASS_COMPRESSION_QUALITY. The command
               prefix code is over a smaller alphabet with the following 64 symbols:
                  0 - 15: insert length code 0, copy length code 0 - 15, same distance
                 16 - 39: insert length code 0, copy length code 0 - 23
                 40 - 63: insert length code 0 - 23, copy length code 0
               Note that symbols 16 and 40 represent the same code in the full alphabet,
               but we do not use either of them in FAST_ONE_PASS_COMPRESSION_QUALITY. */
            public fixed byte cmd_depths_[128];

            public fixed ushort cmd_bits_[128];

            /* The compressed form of the command and distance prefix codes for the next
               block in FAST_ONE_PASS_COMPRESSION_QUALITY. */
            public fixed byte cmd_code_[512];

            public size_t cmd_code_numbits_;

            /* Command and literal buffers for FAST_TWO_PASS_COMPRESSION_QUALITY. */
            public uint* command_buf_;

            public byte* literal_buf_;

            public byte* next_out_;
            public size_t available_out_;

            public size_t total_out_;

            /* Temporary buffer for padding flush bits or metadata block header / body. */
            public fixed byte tiny_buf_u8[16];

            public uint remaining_metadata_bytes_;
            public BrotliEncoderStreamState stream_state_;

            public bool is_last_block_emitted_;
            public bool is_initialized_;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BrotliHasherParams {
            public int type;
            public int bucket_bits;
            public int block_bits;
            public int hash_len;
            public int num_last_distances_to_check;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BrotliEncoderParams {
            public BrotliEncoderMode mode;
            public int quality;
            public int lgwin;
            public int lgblock;
            public size_t size_hint;
            public bool disable_literal_context_modeling;
            public BrotliHasherParams hasher;
        }
    }
}