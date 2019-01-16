using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using reg_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        internal enum BrotliDecoderErrorCode {
            /* Same as BrotliDecoderResult values */
            BROTLI_DECODER_NO_ERROR,
            BROTLI_DECODER_SUCCESS,
            BROTLI_DECODER_NEEDS_MORE_INPUT,
            BROTLI_DECODER_NEEDS_MORE_OUTPUT,

            /* Errors caused by invalid input */
            BROTLI_DECODER_ERROR_FORMAT_EXUBERANT_NIBBLE = -1,
            BROTLI_DECODER_ERROR_FORMAT_RESERVED = -2,
            BROTLI_DECODER_ERROR_FORMAT_EXUBERANT_META_NIBBLE = -3,
            BROTLI_DECODER_ERROR_FORMAT_SIMPLE_HUFFMAN_ALPHABET = -4,
            BROTLI_DECODER_ERROR_FORMAT_SIMPLE_HUFFMAN_SAME = -5,
            BROTLI_DECODER_ERROR_FORMAT_CL_SPACE = -6,
            BROTLI_DECODER_ERROR_FORMAT_HUFFMAN_SPACE = -7,
            BROTLI_DECODER_ERROR_FORMAT_CONTEXT_MAP_REPEAT = -8,
            BROTLI_DECODER_ERROR_FORMAT_BLOCK_LENGTH_1 = -9,
            BROTLI_DECODER_ERROR_FORMAT_BLOCK_LENGTH_2 = -10,
            BROTLI_DECODER_ERROR_FORMAT_TRANSFORM = -11,
            BROTLI_DECODER_ERROR_FORMAT_DICTIONARY = -12,
            BROTLI_DECODER_ERROR_FORMAT_WINDOW_BITS = -13,
            BROTLI_DECODER_ERROR_FORMAT_PADDING_1 = -14,
            BROTLI_DECODER_ERROR_FORMAT_PADDING_2 = -15,

            /* -16..-19 codes are reserved */

            BROTLI_DECODER_ERROR_INVALID_ARGUMENTS = -20,

            /* Memory allocation problems */
            BROTLI_DECODER_ERROR_ALLOC_CONTEXT_MODES = -21,
            /* Literalinsert and distance trees together */
            BROTLI_DECODER_ERROR_ALLOC_TREE_GROUPS = -22,
            /* -23..-24 codes are reserved for distinct tree groups */
            BROTLI_DECODER_ERROR_ALLOC_CONTEXT_MAP = -25,
            BROTLI_DECODER_ERROR_ALLOC_RING_BUFFER_1 = -26,
            BROTLI_DECODER_ERROR_ALLOC_RING_BUFFER_2 = -27,
            /* -28..-29 codes are reserved for dynamic ring-buffer allocation */
            BROTLI_DECODER_ERROR_ALLOC_BLOCK_TYPE_TREES = -30,

            /* "Impossible" states */
            BROTLI_DECODER_ERROR_UNREACHABLE = -31
        }

        internal enum BrotliDecoderResult {
            /// <summary>Decoding error, e.g. corrupted input or memory allocation problem.</summary>
            BROTLI_DECODER_RESULT_ERROR = 0,

            /// <summary>Decoding successfully completed</summary>
            BROTLI_DECODER_RESULT_SUCCESS = 1,

            /// <summary>Partially done; should be called again with more input</summary>
            BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT = 2,

            /// <summary>Partially done; should be called again with more output</summary>
            BROTLI_DECODER_RESULT_NEEDS_MORE_OUTPUT = 3
        }

        internal enum BrotliRunningContextMapState {
            BROTLI_STATE_CONTEXT_MAP_NONE,
            BROTLI_STATE_CONTEXT_MAP_READ_PREFIX,
            BROTLI_STATE_CONTEXT_MAP_HUFFMAN,
            BROTLI_STATE_CONTEXT_MAP_DECODE,
            BROTLI_STATE_CONTEXT_MAP_TRANSFORM
        }

        internal enum BrotliRunningDecodeUint8State {
            BROTLI_STATE_DECODE_UINT8_NONE,
            BROTLI_STATE_DECODE_UINT8_SHORT,
            BROTLI_STATE_DECODE_UINT8_LONG
        }

        internal enum BrotliRunningHuffmanState {
            BROTLI_STATE_HUFFMAN_NONE,
            BROTLI_STATE_HUFFMAN_SIMPLE_SIZE,
            BROTLI_STATE_HUFFMAN_SIMPLE_READ,
            BROTLI_STATE_HUFFMAN_SIMPLE_BUILD,
            BROTLI_STATE_HUFFMAN_COMPLEX,
            BROTLI_STATE_HUFFMAN_LENGTH_SYMBOLS
        }

        internal enum BrotliRunningMetablockHeaderState {
            BROTLI_STATE_METABLOCK_HEADER_NONE,
            BROTLI_STATE_METABLOCK_HEADER_EMPTY,
            BROTLI_STATE_METABLOCK_HEADER_NIBBLES,
            BROTLI_STATE_METABLOCK_HEADER_SIZE,
            BROTLI_STATE_METABLOCK_HEADER_UNCOMPRESSED,
            BROTLI_STATE_METABLOCK_HEADER_RESERVED,
            BROTLI_STATE_METABLOCK_HEADER_BYTES,
            BROTLI_STATE_METABLOCK_HEADER_METADATA
        }

        internal enum BrotliRunningReadBlockLengthState {
            BROTLI_STATE_READ_BLOCK_LENGTH_NONE,
            BROTLI_STATE_READ_BLOCK_LENGTH_SUFFIX
        }

        internal enum BrotliRunningState {
            BROTLI_STATE_UNINITED,
            BROTLI_STATE_METABLOCK_BEGIN,
            BROTLI_STATE_METABLOCK_HEADER,
            BROTLI_STATE_METABLOCK_HEADER_2,
            BROTLI_STATE_CONTEXT_MODES,
            BROTLI_STATE_COMMAND_BEGIN,
            BROTLI_STATE_COMMAND_INNER,
            BROTLI_STATE_COMMAND_POST_DECODE_LITERALS,
            BROTLI_STATE_COMMAND_POST_WRAP_COPY,
            BROTLI_STATE_UNCOMPRESSED,
            BROTLI_STATE_METADATA,
            BROTLI_STATE_COMMAND_INNER_WRITE,
            BROTLI_STATE_METABLOCK_DONE,
            BROTLI_STATE_COMMAND_POST_WRITE_1,
            BROTLI_STATE_COMMAND_POST_WRITE_2,
            BROTLI_STATE_HUFFMAN_CODE_0,
            BROTLI_STATE_HUFFMAN_CODE_1,
            BROTLI_STATE_HUFFMAN_CODE_2,
            BROTLI_STATE_HUFFMAN_CODE_3,
            BROTLI_STATE_CONTEXT_MAP_1,
            BROTLI_STATE_CONTEXT_MAP_2,
            BROTLI_STATE_TREE_GROUP,
            BROTLI_STATE_DONE
        }

        internal enum BrotliRunningTreeGroupState {
            BROTLI_STATE_TREE_GROUP_NONE,
            BROTLI_STATE_TREE_GROUP_LOOP
        }

        internal enum BrotliRunningUncompressedState {
            BROTLI_STATE_UNCOMPRESSED_NONE,
            BROTLI_STATE_UNCOMPRESSED_WRITE
        }

        private static readonly string kPrefixSuffix =
            "\0 \0, \0 of the \0 of \0s \0.\0 and \0 in \0\"\0 to \0\">\0\n\0. \0]\0" +
            " for \0 a \0 that \0\'\0 with \0 from \0 by \0(\0. The \0 on \0 as \0" +
            " is \0ing \0\n\t\0:\0ed \0=\"\0 at \0ly \0,\0=\'\0.com/\0. This \0" +
            " not \0er \0al \0ful \0ive \0less \0est \0ize \0\xc2\xa0\0ous ";

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct BrotliBitReader {
            public reg_t val_;
            public uint bit_pos_;
            public byte* next_in;
            public size_t avail_in;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CmdLutElement
        {
            public byte insert_len_extra_bits;
            public byte copy_len_extra_bits;
            public sbyte distance_code;
            public byte context;
            public ushort insert_len_offset;
            public ushort copy_len_offset;

            public CmdLutElement(byte a, byte b, sbyte c, byte d, ushort e, ushort f)
            {
                insert_len_extra_bits = a;
                copy_len_extra_bits = b;
                distance_code = c;
                context = d;
                insert_len_offset = e;
                copy_len_offset = f;
            }

            public static implicit operator CmdLutElement(short[] a)
            {
                return new CmdLutElement((byte)a[0], (byte)a[1], (sbyte)a[2], (byte)a[3],
                    (ushort)a[4], (ushort)a[5]);
            }
        }

        /* Represents the range of values belonging to a prefix code: */
        /* [offset, offset + 2^nbits) */

        [StructLayout(LayoutKind.Sequential)]
        internal struct PrefixCodeRange {
            public ushort offset;
            public byte nbits;

            public PrefixCodeRange(ushort o, byte n) {
                offset = o;
                nbits = n;
            }

            public static implicit operator PrefixCodeRange(ushort[] a) {
                return new PrefixCodeRange((ushort) a[0], (byte) a[1]);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct BrotliDecoderStateBuffer {
            public ulong u64;

            public unsafe byte* u8 {
                get {
                    fixed (ulong* u = &u64) {
                        return (byte*) u;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct HuffmanCode {
            public byte bits;
            public ushort value;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct HuffmanTreeGroup {
            public HuffmanCode** htrees;
            public HuffmanCode* codes;
            public ushort alphabet_size;
            public ushort num_htrees;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct BrotliDecoderStateStruct {
            public BrotliRunningState state;

            /* This counter is reused for several disjoint loops. */
            public int loop_counter;

            public BrotliBitReader br;

            public brotli_alloc_func alloc_func;
            public brotli_free_func free_func;
            public void* memory_manager_opaque;

            public BrotliDecoderStateBuffer buffer;
            public uint buffer_length;

            public int pos;
            public int max_backward_distance;
            public int max_distance;
            public int ringbuffer_size;
            public int ringbuffer_mask;
            public int dist_rb_idx;
            public fixed int dist_rb [4];
            public int error_code;
            public uint sub_loop_counter;
            public byte* ringbuffer;
            public byte* ringbuffer_end;
            public HuffmanCode* htree_command;
            public byte* context_lookup1;
            public byte* context_lookup2;
            public byte* context_map_slice;
            public byte* dist_context_map_slice;

            /* This ring buffer holds a few past copy distances that will be used by */
            /* some special distance codes. */
            public HuffmanTreeGroup literal_hgroup;
            public HuffmanTreeGroup insert_copy_hgroup;
            public HuffmanTreeGroup distance_hgroup;
            public HuffmanCode* block_type_trees;
            public HuffmanCode* block_len_trees;
            /* This is true if the literal context map histogram type always matches the
            block type. It is then not needed to keep the context (faster decoding). */
            public int trivial_literal_context;
            /* Distance context is actual after command is decoded and before distance
            is computed. After distance computation it is used as a temporary variable. */
            public int distance_context;
            public int meta_block_remaining_len;
            public uint block_length_index;
            public fixed uint block_length [3];
            public fixed uint num_block_types [3];
            public fixed uint block_type_rb [6];
            public uint distance_postfix_bits;
            public uint num_direct_distance_codes;
            public int distance_postfix_mask;
            public uint num_dist_htrees;
            public byte* dist_context_map;
            public HuffmanCode* literal_htree;
            public byte dist_htree_index;
            public uint repeat_code_len;
            public uint prev_code_len;

            public int copy_length;
            public int distance_code;

            /* For partial write operations */
            public size_t rb_roundtrips; /* How many times we went around the ring-buffer */
            public size_t partial_pos_out; /* How much output to the user in total */

            /* For ReadHuffmanCode */
            public uint symbol;
            public uint repeat;
            public uint space;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public HuffmanCode[] table;
            /* List of of symbol chains. */
            public ushort* symbol_lists {
                get {
                    /* Make small negative indexes addressable. */
                    fixed (ushort* sla = symbols_lists_array)
                        return &sla[BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1];
                }
            }

            /* Storage from symbol_lists. */

            public fixed ushort symbols_lists_array [BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1 +
                                                     BROTLI_NUM_COMMAND_SYMBOLS];

            /* Tails of symbol chains. */
            public fixed int next_symbol [32];
            public fixed byte code_length_code_lengths [BROTLI_CODE_LENGTH_CODES];
            /* Population counts for the code lengths */
            public fixed ushort code_length_histo [16];

            /* For HuffmanTreeGroupDecode */
            public int htree_index;
            public HuffmanCode* next;

            /* For DecodeContextMap */
            public uint context_index;
            public uint max_run_length_prefix;
            public uint code;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = BROTLI_HUFFMAN_MAX_SIZE_272)] public HuffmanCode[]
                context_map_table;

            /* For InverseMoveToFrontTransform */
            public uint mtf_upper_bound;
            public fixed uint mtf [64 + 1];

            /* For custom dictionaries */
            public byte* custom_dict;
            public int custom_dict_size;

            /* less used attributes are in the end of this struct */
            /* States inside function calls */
            public BrotliRunningMetablockHeaderState substate_metablock_header;
            public BrotliRunningTreeGroupState substate_tree_group;
            public BrotliRunningContextMapState substate_context_map;
            public BrotliRunningUncompressedState substate_uncompressed;
            public BrotliRunningHuffmanState substate_huffman;
            public BrotliRunningDecodeUint8State substate_decode_uint8;
            public BrotliRunningReadBlockLengthState substate_read_block_length;

            public bool is_last_metablock;
            public bool is_uncompressed;
            public bool is_metadata;
            public bool should_wrap_ringbuffer;
            public byte size_nibbles;
            public uint window_bits;

            public int new_ringbuffer_size;

            public uint num_literal_htrees;
            public byte* context_map;
            public byte* context_modes;
            public byte* dictionary;

            public fixed uint trivial_literal_contexts [8]; /* 256 bits */
        }

        private enum TransformID {
            /* EMPTY = ""
               SP = " "
               DQUOT = "\""
               SQUOT = "'"
               CLOSEBR = "]"
               OPEN = "("
               SLASH = "/"
               NBSP = non-breaking space "\0xc2\xa0"
            */
            kPFix_EMPTY = 0,
            kPFix_SP = 1,
            kPFix_COMMASP = 3,
            kPFix_SPofSPtheSP = 6,
            kPFix_SPtheSP = 9,
            kPFix_eSP = 12,
            kPFix_SPofSP = 15,
            kPFix_sSP = 20,
            kPFix_DOT = 23,
            kPFix_SPandSP = 25,
            kPFix_SPinSP = 31,
            kPFix_DQUOT = 36,
            kPFix_SPtoSP = 38,
            kPFix_DQUOTGT = 43,
            kPFix_NEWLINE = 46,
            kPFix_DOTSP = 48,
            kPFix_CLOSEBR = 51,
            kPFix_SPforSP = 53,
            kPFix_SPaSP = 59,
            kPFix_SPthatSP = 63,
            kPFix_SQUOT = 70,
            kPFix_SPwithSP = 72,
            kPFix_SPfromSP = 79,
            kPFix_SPbySP = 86,
            kPFix_OPEN = 91,
            kPFix_DOTSPTheSP = 93,
            kPFix_SPonSP = 100,
            kPFix_SPasSP = 105,
            kPFix_SPisSP = 110,
            kPFix_ingSP = 115,
            kPFix_NEWLINETAB = 120,
            kPFix_COLON = 123,
            kPFix_edSP = 125,
            kPFix_EQDQUOT = 129,
            kPFix_SPatSP = 132,
            kPFix_lySP = 137,
            kPFix_COMMA = 141,
            kPFix_EQSQUOT = 143,
            kPFix_DOTcomSLASH = 146,
            kPFix_DOTSPThisSP = 152,
            kPFix_SPnotSP = 160,
            kPFix_erSP = 166,
            kPFix_alSP = 170,
            kPFix_fulSP = 174,
            kPFix_iveSP = 179,
            kPFix_lessSP = 184,
            kPFix_estSP = 190,
            kPFix_izeSP = 195,
            kPFix_NBSP = 200,
            kPFix_ousSP = 203
        }

        private enum WordTransformType {
            kIdentity = 0,
            kOmitLast1 = 1,
            kOmitLast2 = 2,
            kOmitLast3 = 3,
            kOmitLast4 = 4,
            kOmitLast5 = 5,
            kOmitLast6 = 6,
            kOmitLast7 = 7,
            kOmitLast8 = 8,
            kOmitLast9 = 9,
            kUppercaseFirst = 10,
            kUppercaseAll = 11,
            kOmitFirst1 = 12,
            kOmitFirst2 = 13,
            kOmitFirst3 = 14,
            kOmitFirst4 = 15,
            kOmitFirst5 = 16,
            kOmitFirst6 = 17,
            kOmitFirst7 = 18,
            kOmitFirst8 = 19,
            kOmitFirst9 = 20
        }

        internal struct Transform {
            public byte prefix_id;
            public byte transform;
            public byte suffix_id;

            public static implicit operator Transform(Enum[] e) {
                return new Transform {
                    prefix_id = Convert.ToByte(e[0]),
                    transform = Convert.ToByte(e[1]),
                    suffix_id = Convert.ToByte(e[2])
                };
            }
        }
    }
}