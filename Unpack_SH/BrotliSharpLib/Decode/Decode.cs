using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;
using reg_t = BrotliSharpLib.Brotli.SizeT;
using BrotliDecoderState = BrotliSharpLib.Brotli.BrotliDecoderStateStruct;
using BrotliBitReaderState = BrotliSharpLib.Brotli.BrotliBitReader;

// ReSharper disable All

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static unsafe T CreateStruct<T>() {
#if SIZE_OF_T
            var sz = Marshal.SizeOf<T>();
#else
            var sz = Marshal.SizeOf(typeof(T));
#endif
            var hMem = Marshal.AllocHGlobal(sz);
            memset(hMem.ToPointer(), 0, sz);
#if SIZE_OF_T
            var s = Marshal.PtrToStructure<T>(hMem);
#else
            var s = Marshal.PtrToStructure(hMem, typeof(T));
#endif
            Marshal.FreeHGlobal(hMem);
            return (T) s;
        }

        private static unsafe void* DefaultAllocFunc(void* opaque, size_t size) {
            return Marshal.AllocHGlobal((int) size).ToPointer();
        }

        private static unsafe void DefaultFreeFunc(void* opaque, void* address) {
            Marshal.FreeHGlobal((IntPtr) address);
        }

        internal static BrotliDecoderState BrotliCreateDecoderState() {
            return CreateStruct<BrotliDecoderState>();
        }


        internal static unsafe void BrotliDecoderSetCustomDictionary(
            ref BrotliDecoderState s, size_t size, byte* dict)
        {
            if (size > (1u << 24))
            {
                return;
            }
            s.custom_dict = dict;
            s.custom_dict_size = (int)size;
        }

        private static unsafe BrotliDecoderResult BrotliDecoderDecompress(
            size_t encoded_size, byte* encoded_buffer, size_t* decoded_size,
            byte* decoded_buffer) {
            var s = BrotliCreateDecoderState();
            size_t total_out = 0;
            var available_in = encoded_size;
            var next_in = encoded_buffer;
            var available_out = *decoded_size;
            var next_out = decoded_buffer;
            BrotliDecoderStateInit(ref s);
            var result = BrotliDecoderDecompressStream(
                ref s, &available_in, &next_in, &available_out, &next_out, &total_out);
            *decoded_size = total_out;
            BrotliDecoderStateCleanup(ref s);
            if (result != BrotliDecoderResult.BROTLI_DECODER_RESULT_SUCCESS)
                result = BrotliDecoderResult.BROTLI_DECODER_RESULT_ERROR;
            return result;
        }

        /* Saves error code and converts it to BrotliDecoderResult */
        private static BrotliDecoderResult SaveErrorCode(
            ref BrotliDecoderState s, BrotliDecoderErrorCode e) {
            s.error_code = (int) e;
            switch (e) {
                case BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS:
                    return BrotliDecoderResult.BROTLI_DECODER_RESULT_SUCCESS;
                case BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT:
                    return BrotliDecoderResult.BROTLI_DECODER_RESULT_NEEDS_MORE_INPUT;
                case BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_OUTPUT:
                    return BrotliDecoderResult.BROTLI_DECODER_RESULT_NEEDS_MORE_OUTPUT;
                default:
                    return BrotliDecoderResult.BROTLI_DECODER_RESULT_ERROR;
            }
        }

        private static size_t UnwrittenBytes(ref BrotliDecoderState s, bool wrap) {
            var pos = wrap && s.pos > s.ringbuffer_size
                ? (size_t) s.ringbuffer_size
                : (size_t) (s.pos);
            var partial_pos_rb = (s.rb_roundtrips * (size_t) s.ringbuffer_size) + pos;
            return partial_pos_rb - s.partial_pos_out;
        }

        private static unsafe BrotliDecoderErrorCode WriteRingBuffer(
            ref BrotliDecoderState s, size_t* available_out, byte** next_out,
            size_t* total_out, bool force) {
            var start =
                s.ringbuffer + (s.partial_pos_out & (size_t) s.ringbuffer_mask);
            var to_write = UnwrittenBytes(ref s, true);
            var num_written = *available_out;
            if (num_written > to_write) {
                num_written = to_write;
            }
            if (s.meta_block_remaining_len < 0) {
                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_BLOCK_LENGTH_1;
            }
            if (next_out != null && *next_out == null) {
                *next_out = start;
            }
            else {
                if (next_out != null) {
                    memcpy(*next_out, start, num_written);
                    *next_out += num_written;
                }
            }
            *available_out -= num_written;
            s.partial_pos_out += num_written;
            if (total_out != null) *total_out = s.partial_pos_out - s.custom_dict_size;
            if (num_written < to_write) {
                if (s.ringbuffer_size == (1 << (int) s.window_bits) || force) {
                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_OUTPUT;
                }
                else {
                    return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                }
            }
            /* Wrap ring buffer only if it has reached its maximal size. */
            if (s.ringbuffer_size == (1 << (int) s.window_bits) &&
                s.pos >= s.ringbuffer_size) {
                s.pos -= s.ringbuffer_size;
                s.rb_roundtrips++;
                s.should_wrap_ringbuffer = (size_t) s.pos != 0;
            }
            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
        }


        /* Decodes a number in the range [9..24], by reading 1 - 7 bits.
           Precondition: bit-reader accumulator has at least 7 bits. */
        private static unsafe uint DecodeWindowBits(BrotliBitReader* br) {
            uint n;
            BrotliTakeBits(br, 1, &n);
            if (n == 0) {
                return 16;
            }
            BrotliTakeBits(br, 3, &n);
            if (n != 0) {
                return 17 + n;
            }
            BrotliTakeBits(br, 3, &n);
            if (n != 0) {
                return 8 + n;
            }
            return 17;
        }

        /* Decodes a metablock length and flags by reading 2 - 31 bits. */
        private static unsafe BrotliDecoderErrorCode DecodeMetaBlockLength(
            ref BrotliDecoderState s, BrotliBitReader* br) {
            uint bits;
            int i;
            for (;;) {
                switch (s.substate_metablock_header) {
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE:
                        if (!BrotliSafeReadBits(br, 1, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        s.is_last_metablock = bits != 0;
                        s.meta_block_remaining_len = 0;
                        s.is_uncompressed = false;
                        s.is_metadata = false;
                        if (!s.is_last_metablock) {
                            s.substate_metablock_header =
                                BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NIBBLES;
                            break;
                        }
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_EMPTY;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_EMPTY;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_EMPTY:
                        if (!BrotliSafeReadBits(br, 1, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        if (bits != 0) {
                            s.substate_metablock_header =
                                BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                        }
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NIBBLES;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NIBBLES;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NIBBLES:
                        if (!BrotliSafeReadBits(br, 2, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        s.size_nibbles = (byte) (bits + 4);
                        s.loop_counter = 0;
                        if (bits == 3) {
                            s.is_metadata = true;
                            s.substate_metablock_header =
                                BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_RESERVED;
                            break;
                        }
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_SIZE;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_SIZE;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_SIZE:
                        i = s.loop_counter;
                        for (; i < (int) s.size_nibbles; ++i) {
                            if (!BrotliSafeReadBits(br, 4, &bits)) {
                                s.loop_counter = i;
                                return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                            }
                            if (i + 1 == s.size_nibbles && s.size_nibbles > 4 && bits == 0) {
                                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_EXUBERANT_NIBBLE;
                            }
                            s.meta_block_remaining_len |= (int) (bits << (i * 4));
                        }
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_UNCOMPRESSED;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_UNCOMPRESSED;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_UNCOMPRESSED:
                        if (!s.is_last_metablock) {
                            if (!BrotliSafeReadBits(br, 1, &bits)) {
                                return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                            }
                            s.is_uncompressed = bits != 0;
                        }
                        ++s.meta_block_remaining_len;
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;

                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_RESERVED:
                        if (!BrotliSafeReadBits(br, 1, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        if (bits != 0) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_RESERVED;
                        }
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_BYTES;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_BYTES;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_BYTES:
                        if (!BrotliSafeReadBits(br, 2, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        if (bits == 0) {
                            s.substate_metablock_header =
                                BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                        }
                        s.size_nibbles = (byte) bits;
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_METADATA;
                        /* No break, transit to the next state. */
                        goto case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_METADATA;
                    case BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_METADATA:
                        i = s.loop_counter;
                        for (; i < (int) s.size_nibbles; ++i) {
                            if (!BrotliSafeReadBits(br, 8, &bits)) {
                                s.loop_counter = i;
                                return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                            }
                            if (i + 1 == s.size_nibbles && s.size_nibbles > 1 && bits == 0) {
                                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_EXUBERANT_META_NIBBLE;
                            }
                            s.meta_block_remaining_len |= (int) (bits << (i * 8));
                        }
                        ++s.meta_block_remaining_len;
                        s.substate_metablock_header =
                            BrotliRunningMetablockHeaderState.BROTLI_STATE_METABLOCK_HEADER_NONE;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;

                    default:
                        return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE;
                }
            }
        }

        /* Calculates the smallest feasible ring buffer.

           If we know the data size is small, do not allocate more ring buffer
           size than needed to reduce memory usage.

           When this method is called, metablock size and flags MUST be decoded.
        */
        private static unsafe void BrotliCalculateRingBufferSize(
            ref BrotliDecoderState s) {
            var window_size = 1 << (int) s.window_bits;
            var new_ringbuffer_size = window_size;
            /* We need at least 2 bytes of ring buffer size to get the last two
               bytes for context from there */
            var min_size = s.ringbuffer_size != 0 ? s.ringbuffer_size : 1024;
            int output_size;

            /* If maximum is already reached, no further extension is retired. */
            if (s.ringbuffer_size == window_size) {
                return;
            }

            /* Metadata blocks does not touch ring buffer. */
            if (s.is_metadata) {
                return;
            }

            if (s.ringbuffer == null) {
                /* Custom dictionary counts as a "virtual" output. */
                output_size = s.custom_dict_size;
            }
            else {
                output_size = s.pos;
            }
            output_size += s.meta_block_remaining_len;
            min_size = min_size < output_size ? output_size : min_size;

            while ((new_ringbuffer_size >> 1) >= min_size) {
                new_ringbuffer_size >>= 1;
            }

            s.new_ringbuffer_size = new_ringbuffer_size;
        }

        /* Allocates ring-buffer.

           s->ringbuffer_size MUST be updated by BrotliCalculateRingBufferSize before
           this function is called.

           Last two bytes of ring-buffer are initialized to 0, so context calculation
           could be done uniformly for the first two and all other positions.

           Custom dictionary, if any, is copied to the end of ring-buffer.
        */
        private static unsafe bool BrotliEnsureRingBuffer(
            ref BrotliDecoderState s) {
            /* We need the slack region for the following reasons:
                - doing up to two 16-byte copies for fast backward copying
                - inserting transformed dictionary word (5 prefix + 24 base + 8 suffix) */
            const int kRingBufferWriteAheadSlack = 42;
            var old_ringbuffer = s.ringbuffer;
            if (s.ringbuffer_size == s.new_ringbuffer_size) {
                return true;
            }

            s.ringbuffer = (byte*) s.alloc_func(s.memory_manager_opaque, (size_t) (s.new_ringbuffer_size +
                                                                                   kRingBufferWriteAheadSlack));
            if (s.ringbuffer == null) {
                /* Restore previous value. */
                s.ringbuffer = old_ringbuffer;
                return false;
            }
            s.ringbuffer[s.new_ringbuffer_size - 2] = 0;
            s.ringbuffer[s.new_ringbuffer_size - 1] = 0;

            if (old_ringbuffer == null) {
                if (s.custom_dict != null) {
                    memcpy(s.ringbuffer, s.custom_dict, (size_t) s.custom_dict_size);
                    s.partial_pos_out = (size_t) s.custom_dict_size;
                    s.pos = s.custom_dict_size;
                }
            }
            else {
                memcpy(s.ringbuffer, old_ringbuffer, (size_t) s.pos);
                s.free_func(s.memory_manager_opaque, old_ringbuffer);
                old_ringbuffer = null;
            }

            s.ringbuffer_size = s.new_ringbuffer_size;
            s.ringbuffer_mask = s.new_ringbuffer_size - 1;
            s.ringbuffer_end = s.ringbuffer + s.ringbuffer_size;

            return true;
        }

        private static unsafe BrotliDecoderErrorCode CopyUncompressedBlockToOutput(
            size_t* available_out, byte** next_out, size_t* total_out,
            ref BrotliDecoderState s) {
            /* TODO: avoid allocation for single uncompressed block. */
            if (!BrotliEnsureRingBuffer(ref s)) {
                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_RING_BUFFER_1;
            }

            /* State machine */
            for (;;) {
                switch (s.substate_uncompressed) {
                    case BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_NONE: {
                        fixed (BrotliBitReader* br = &s.br) {
                            var nbytes = (int) BrotliGetRemainingBytes(br);
                            if (nbytes > s.meta_block_remaining_len) {
                                nbytes = s.meta_block_remaining_len;
                            }
                            if (s.pos + nbytes > s.ringbuffer_size) {
                                nbytes = s.ringbuffer_size - s.pos;
                            }
                            /* Copy remaining bytes from s.br.buf_ to ring-buffer. */
                            BrotliCopyBytes(&s.ringbuffer[s.pos], br, (size_t) nbytes);
                            s.pos += nbytes;
                            s.meta_block_remaining_len -= nbytes;
                            if (s.pos < 1 << (int) s.window_bits) {
                                if (s.meta_block_remaining_len == 0) {
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                                }
                                return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                            }
                            s.substate_uncompressed = BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_WRITE;
                            /* No break, continue to next state */
                            goto case BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_WRITE;
                        }
                    }
                    case BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_WRITE: {
                        BrotliDecoderErrorCode result;
                        result = WriteRingBuffer(
                            ref s, available_out, next_out, total_out, false);
                        if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                            return result;
                        }
                        if (s.ringbuffer_size == 1 << (int) s.window_bits) {
                            s.max_distance = s.max_backward_distance;
                        }
                        s.substate_uncompressed = BrotliRunningUncompressedState.BROTLI_STATE_UNCOMPRESSED_NONE;
                        break;
                    }
                }
            }
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static uint Log2Floor(uint x) {
            uint y = x; // JIT ETW (Inlinee writes to an argument)
            uint result = 0;
            while (y != 0) {
                y >>= 1;
                ++result;
            }
            return result;
        }

        /* Reads (s->symbol + 1) symbols.
           Totally 1..4 symbols are read, 1..10 bits each.
           The list of symbols MUST NOT contain duplicates.
         */
        private static unsafe BrotliDecoderErrorCode ReadSimpleHuffmanSymbols(
            uint alphabet_size, ref BrotliDecoderState s) {
            /* max_bits == 1..10; symbol == 0..3; 1..40 bits will be read. */
            var max_bits = Log2Floor(alphabet_size - 1);
            var i = s.sub_loop_counter;
            var num_symbols = s.symbol;
            while (i <= num_symbols) {
                uint v;
                fixed (BrotliBitReader* br = &s.br)
                    if (!BrotliSafeReadBits(br, max_bits, &v)) {
                        s.sub_loop_counter = i;
                        s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_READ;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                    }
                if (v >= alphabet_size) {
                    return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_SIMPLE_HUFFMAN_ALPHABET;
                }
                fixed (ushort* sla = s.symbols_lists_array)
                    sla[i] = (ushort) v;
                ++i;
            }

            fixed (ushort* sla = s.symbols_lists_array) {
                for (i = 0; i < num_symbols; ++i) {
                    var k = i + 1;
                    for (; k <= num_symbols; ++k) {
                        if (sla[i] == sla[k]) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_SIMPLE_HUFFMAN_SAME;
                        }
                    }
                }
            }

            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
        }

        /* Decodes a number in the range [0..255], by reading 1 - 11 bits. */
        private static unsafe BrotliDecoderErrorCode DecodeVarLenUint8(
            ref BrotliDecoderState s, BrotliBitReader* br, uint* value) {
            uint bits;
            switch (s.substate_decode_uint8) {
                case BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_NONE:
                    if (!BrotliSafeReadBits(br, 1, &bits)) {
                        return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                    }
                    if (bits == 0) {
                        *value = 0;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                    }
                    /* No break, transit to the next state. */
                    goto case BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_SHORT;
                case BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_SHORT:
                    if (!BrotliSafeReadBits(br, 3, &bits)) {
                        s.substate_decode_uint8 = BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_SHORT;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                    }
                    if (bits == 0) {
                        *value = 1;
                        s.substate_decode_uint8 = BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_NONE;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                    }
                    /* Use output value as a temporary storage. It MUST be persisted. */
                    *value = bits;
                    /* No break, transit to the next state. */
                    goto case BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_LONG;
                case BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_LONG:
                    if (!BrotliSafeReadBits(br, *value, &bits)) {
                        s.substate_decode_uint8 = BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_LONG;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                    }
                    *value = (1U << (int) *value) + bits;
                    s.substate_decode_uint8 = BrotliRunningDecodeUint8State.BROTLI_STATE_DECODE_UINT8_NONE;
                    return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;

                default:
                    return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE;
            }
        }


        /* Reads and decodes 15..18 codes using static prefix code.
           Each code is 2..4 bits long. In total 30..72 bits are used. */
        private static unsafe BrotliDecoderErrorCode ReadCodeLengthCodeLengths(ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                var num_codes = s.repeat;
                var space = s.space;
                var i = s.sub_loop_counter;
                for (; i < BROTLI_CODE_LENGTH_CODES; ++i) {
                    var code_len_idx = kCodeLengthCodeOrder[i];
                    uint ix;
                    uint v;
                    if (!BrotliSafeGetBits(br, 4, &ix)) {
                        var available_bits = BrotliGetAvailableBits(br);
                        if (available_bits != 0) {
                            ix = BrotliGetBitsUnmasked(br) & 0xF;
                        }
                        else {
                            ix = 0;
                        }
                        if (kCodeLengthPrefixLength[ix] > available_bits) {
                            s.sub_loop_counter = i;
                            s.repeat = num_codes;
                            s.space = space;
                            s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_COMPLEX;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                    }
                    v = kCodeLengthPrefixValue[ix];
                    BrotliDropBits(br, kCodeLengthPrefixLength[ix]);
                    fixed (byte* clcl = s.code_length_code_lengths)
                        clcl[code_len_idx] = (byte) v;
                    if (v != 0) {
                        space = space - (32U >> (int) v);
                        ++num_codes;
                        fixed (ushort* clh = s.code_length_histo)
                            ++clh[v];
                        if (space - 1U >= 32U) {
                            /* space is 0 or wrapped around */
                            break;
                        }
                    }
                }
                if (!(num_codes == 1 || space == 0)) {
                    return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_CL_SPACE;
                }
                return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
            }
        }

        /* Process single decoded symbol code length:
            A) reset the repeat variable
            B) remember code length (if it is not 0)
            C) extend corresponding index-chain
            D) reduce the Huffman space
            E) update the histogram
         */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void ProcessSingleCodeLength(uint code_len,
            uint* symbol, uint* repeat, uint* space,
            uint* prev_code_len, ushort* symbol_lists,
            ushort* code_length_histo, int* next_symbol) {
            *repeat = 0;
            if (code_len != 0) {
                /* code_len == 1..15 */
                symbol_lists[next_symbol[code_len]] = (ushort) (*symbol);
                next_symbol[code_len] = (int) (*symbol);
                *prev_code_len = code_len;
                *space -= 32768U >> (int) code_len;
                code_length_histo[code_len]++;
            }
            (*symbol)++;
        }

        /* Process repeated symbol code length.
            A) Check if it is the extension of previous repeat sequence; if the decoded
               value is not BROTLI_REPEAT_PREVIOUS_CODE_LENGTH, then it is a new
               symbol-skip
            B) Update repeat variable
            C) Check if operation is feasible (fits alphabet)
            D) For each symbol do the same operations as in ProcessSingleCodeLength

           PRECONDITION: code_len == BROTLI_REPEAT_PREVIOUS_CODE_LENGTH or
                         code_len == BROTLI_REPEAT_ZERO_CODE_LENGTH
         */
        private static unsafe void ProcessRepeatedCodeLength(uint code_len,
            uint repeat_delta, uint alphabet_size, uint* symbol,
            uint* repeat, uint* space, uint* prev_code_len,
            uint* repeat_code_len, ushort* symbol_lists,
            ushort* code_length_histo, int* next_symbol) {
            uint old_repeat;
            uint extra_bits = 3; /* for BROTLI_REPEAT_ZERO_CODE_LENGTH */
            uint new_len = 0; /* for BROTLI_REPEAT_ZERO_CODE_LENGTH */
            if (code_len == BROTLI_REPEAT_PREVIOUS_CODE_LENGTH) {
                new_len = *prev_code_len;
                extra_bits = 2;
            }
            if (*repeat_code_len != new_len) {
                *repeat = 0;
                *repeat_code_len = new_len;
            }
            old_repeat = *repeat;
            if (*repeat > 0) {
                *repeat -= 2;
                *repeat <<= (int) extra_bits;
            }
            *repeat += repeat_delta + 3U;
            repeat_delta = *repeat - old_repeat;
            if (*symbol + repeat_delta > alphabet_size) {
                *symbol = alphabet_size;
                *space = 0xFFFFF;
                return;
            }
            if (*repeat_code_len != 0) {
                var last = *symbol + repeat_delta;
                var next = next_symbol[*repeat_code_len];
                do {
                    symbol_lists[next] = (ushort) *symbol;
                    next = (int) *symbol;
                } while (++(*symbol) != last);
                next_symbol[*repeat_code_len] = next;
                *space -= repeat_delta << (int) (15 - *repeat_code_len);
                code_length_histo[*repeat_code_len] =
                    (ushort) (code_length_histo[*repeat_code_len] + repeat_delta);
            }
            else {
                *symbol += repeat_delta;
            }
        }

        private static unsafe BrotliDecoderErrorCode SafeReadSymbolCodeLengths(
            uint alphabet_size, ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                bool get_byte = false;
                while (s.symbol < alphabet_size && s.space > 0) {
                    fixed (HuffmanCode* t = s.table) {
                        var p = t;
                        uint code_len;
                        uint bits = 0;
                        if (get_byte && !BrotliPullByte(br))
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        get_byte = false;
                        var available_bits = BrotliGetAvailableBits(br);
                        if (available_bits != 0) {
                            bits = (uint) BrotliGetBitsUnmasked(br);
                        }
                        p += bits & BitMask(BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH);
                        if (p->bits > available_bits) {
                            get_byte = true;
                            continue;
                        }
                        code_len = p->value; /* code_len == 0..17 */
                        fixed (uint* symbol = &s.symbol)
                        fixed (uint* repeat = &s.repeat)
                        fixed (uint* space = &s.space)
                        fixed (uint* pcl = &s.prev_code_len)
                        fixed (ushort* clh = s.code_length_histo)
                        fixed (int* ns = s.next_symbol) {
                            if (code_len < BROTLI_REPEAT_PREVIOUS_CODE_LENGTH) {
                                BrotliDropBits(br, p->bits);
                                ProcessSingleCodeLength(code_len, symbol, repeat, space,
                                    pcl, s.symbol_lists, clh,
                                    ns);
                            }
                            else {
                                /* code_len == 16..17, extra_bits == 2..3 */
                                var extra_bits = code_len - 14U;
                                var repeat_delta = (bits >> p->bits) & BitMask(extra_bits);
                                if (available_bits < p->bits + extra_bits) {
                                    get_byte = true;
                                    continue;
                                }
                                BrotliDropBits(br, p->bits + extra_bits);
                                fixed (uint* rcl = &s.repeat_code_len)
                                    ProcessRepeatedCodeLength(code_len, repeat_delta, alphabet_size,
                                        symbol, repeat, space, pcl,
                                        rcl, s.symbol_lists, clh,
                                        ns);
                            }
                        }
                    }
                }
            }
            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
        }

        /* Reads and decodes symbol codelengths. */

        private static unsafe BrotliDecoderErrorCode ReadSymbolCodeLengths(
            uint alphabet_size, ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                var symbol = s.symbol;
                var repeat = s.repeat;
                var space = s.space;
                var prev_code_len = s.prev_code_len;
                var repeat_code_len = s.repeat_code_len;
                var symbol_lists = s.symbol_lists;
                fixed (ushort* clh = s.code_length_histo) {
                    fixed (int* ns = s.next_symbol) {
                        var code_length_histo = clh;
                        var next_symbol = ns;
                        if (!BrotliWarmupBitReader(br)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        while (symbol < alphabet_size && space > 0) {
                            fixed (HuffmanCode* t = s.table) {
                                var p = t;
                                uint code_len;
                                if (!BrotliCheckInputAmount(br, BROTLI_SHORT_FILL_BIT_WINDOW_READ)) {
                                    s.symbol = symbol;
                                    s.repeat = repeat;
                                    s.prev_code_len = prev_code_len;
                                    s.repeat_code_len = repeat_code_len;
                                    s.space = space;
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                }
                                BrotliFillBitWindow16(br);
                                p += BrotliGetBitsUnmasked(br) &
                                     BitMask(BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH);
                                BrotliDropBits(br, p->bits); /* Use 1..5 bits */
                                code_len = p->value; /* code_len == 0..17 */
                                if (code_len < BROTLI_REPEAT_PREVIOUS_CODE_LENGTH) {
                                    ProcessSingleCodeLength(code_len, &symbol, &repeat, &space,
                                        &prev_code_len, symbol_lists, code_length_histo, next_symbol);
                                }
                                else {
                                    /* code_len == 16..17, extra_bits == 2..3 */
                                    var extra_bits =
                                        (code_len == BROTLI_REPEAT_PREVIOUS_CODE_LENGTH) ? 2u : 3u;
                                    var repeat_delta =
                                        (uint) (BrotliGetBitsUnmasked(br) & BitMask(extra_bits));
                                    BrotliDropBits(br, extra_bits);
                                    ProcessRepeatedCodeLength(code_len, repeat_delta, alphabet_size,
                                        &symbol, &repeat, &space, &prev_code_len, &repeat_code_len,
                                        symbol_lists, code_length_histo, next_symbol);
                                }
                            }
                        }

                        s.space = space;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                    }
                }
            }
        }

        /* Decodes the Huffman tables.
           There are 2 scenarios:
            A) Huffman code contains only few symbols (1..4). Those symbols are read
               directly; their code lengths are defined by the number of symbols.
               For this scenario 4 - 45 bits will be read.

            B) 2-phase decoding:
            B.1) Small Huffman table is decoded; it is specified with code lengths
                 encoded with predefined entropy code. 32 - 74 bits are used.
            B.2) Decoded table is used to decode code lengths of symbols in resulting
                 Huffman table. In worst case 3520 bits are read.
        */
        private static unsafe BrotliDecoderErrorCode ReadHuffmanCode(uint alphabet_size,
            HuffmanCode* table,
            uint* opt_table_size,
            ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                /* Unnecessary masking, but might be good for safety. */
                alphabet_size &= 0x3ff;
                /* State machine */
                for (;;) {
                    switch (s.substate_huffman) {
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_NONE:
                            fixed (uint* slc = &s.sub_loop_counter)
                                if (!BrotliSafeReadBits(br, 2, slc)) {
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                }
                            /* The value is used as follows:
                               1 for simple code;
                               0 for no skipping, 2 skips 2 code lengths, 3 skips 3 code lengths */
                            if (s.sub_loop_counter != 1) {
                                s.space = 32;
                                s.repeat = 0; /* num_codes */
                                fixed (ushort* clh = s.code_length_histo)
                                    memset(&clh[0], 0, sizeof(ushort) *
                                                       (BROTLI_HUFFMAN_MAX_CODE_LENGTH_CODE_LENGTH + 1));
                                fixed (byte* clcl = s.code_length_code_lengths)
                                    memset(&clcl[0], 0,
                                        BROTLI_CODE_LENGTH_CODES);
                                s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_COMPLEX;
                                continue;
                            }
                            /* No break, transit to the next state. */
                            goto case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_SIZE;
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_SIZE:
                            /* Read symbols, codes & code lengths directly. */
                            fixed (uint* ss = &s.symbol)
                                if (!BrotliSafeReadBits(br, 2, ss)) {
                                    /* num_symbols */
                                    s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_SIZE;
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                }
                            s.sub_loop_counter = 0;
                            /* No break, transit to the next state. */
                            goto case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_READ;
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_READ: {
                            var result =
                                ReadSimpleHuffmanSymbols(alphabet_size, ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                return result;
                            }
                            /* No break, transit to the next state. */
                            goto case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_BUILD;
                        }
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_BUILD: {
                            uint table_size;
                            if (s.symbol == 3) {
                                uint bits;
                                if (!BrotliSafeReadBits(br, 1, &bits)) {
                                    s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_SIMPLE_BUILD;
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                }
                                s.symbol += bits;
                            }
                            fixed (ushort* sla = s.symbols_lists_array)
                                table_size = BrotliBuildSimpleHuffmanTable(
                                    table, HUFFMAN_TABLE_BITS, sla, s.symbol);
                            if (opt_table_size != null) {
                                *opt_table_size = table_size;
                            }
                            s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_NONE;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                        }

                        /* Decode Huffman-coded code lengths. */
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_COMPLEX: {
                            uint i;
                            var result = ReadCodeLengthCodeLengths(ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                return result;
                            }
                            fixed (HuffmanCode* t = s.table)
                            fixed (byte* clcl = s.code_length_code_lengths)
                            fixed (ushort* clh = s.code_length_histo) {
                                BrotliBuildCodeLengthsHuffmanTable(t,
                                    clcl,
                                    clh);
                                memset(&clh[0], 0, sizeof(ushort) * 16);
                                fixed (int* ns = s.next_symbol)
                                    for (i = 0; i <= BROTLI_HUFFMAN_MAX_CODE_LENGTH; ++i) {
                                        ns[i] = (int) i - (BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1);
                                        s.symbol_lists[(int) i - (BROTLI_HUFFMAN_MAX_CODE_LENGTH + 1)] = 0xFFFF;
                                    }
                            }

                            s.symbol = 0;
                            s.prev_code_len = BROTLI_INITIAL_REPEATED_CODE_LENGTH;
                            s.repeat = 0;
                            s.repeat_code_len = 0;
                            s.space = 32768;
                            s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_LENGTH_SYMBOLS;
                            /* No break, transit to the next state. */
                            goto case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_LENGTH_SYMBOLS;
                        }
                        case BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_LENGTH_SYMBOLS: {
                            uint table_size;
                            var result = ReadSymbolCodeLengths(alphabet_size, ref s);
                            if (result == BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT) {
                                result = SafeReadSymbolCodeLengths(alphabet_size, ref s);
                            }
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                return result;
                            }

                            if (s.space != 0) {
                                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_HUFFMAN_SPACE;
                            }
                            fixed (ushort* clh = s.code_length_histo)
                                table_size = BrotliBuildHuffmanTable(
                                    table, HUFFMAN_TABLE_BITS, s.symbol_lists, clh);
                            if (opt_table_size != null) {
                                *opt_table_size = table_size;
                            }
                            s.substate_huffman = BrotliRunningHuffmanState.BROTLI_STATE_HUFFMAN_NONE;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                        }

                        default:
                            return
                                BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE;
                    }
                }
            }
        }

        /* Decodes the Huffman code.
           This method doesn't read data from the bit reader, BUT drops the amount of
           bits that correspond to the decoded symbol.
           bits MUST contain at least 15 (BROTLI_HUFFMAN_MAX_CODE_LENGTH) valid bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint DecodeSymbol(uint bits,
            HuffmanCode* table,
            BrotliBitReader* br) {
            table += bits & HUFFMAN_TABLE_MASK;
            if (table->bits > HUFFMAN_TABLE_BITS) {
                var nbits = (uint) (table->bits - HUFFMAN_TABLE_BITS);
                BrotliDropBits(br, HUFFMAN_TABLE_BITS);
                table += table->value;
                table += (bits >> HUFFMAN_TABLE_BITS) & BitMask(nbits);
            }
            BrotliDropBits(br, table->bits);
            return table->value;
        }

        /* Same as DecodeSymbol, but it is known that there is less than 15 bits of
            input are currently available. */
        private static unsafe bool SafeDecodeSymbol(
            HuffmanCode* table, BrotliBitReader* br, uint* result) {
            uint val;
            var available_bits = BrotliGetAvailableBits(br);
            if (available_bits == 0) {
                if (table->bits == 0) {
                    *result = table->value;
                    return true;
                }
                return false; /* No valid bits at all. */
            }
            val = (uint) BrotliGetBitsUnmasked(br);
            table += val & HUFFMAN_TABLE_MASK;
            if (table->bits <= HUFFMAN_TABLE_BITS) {
                if (table->bits <= available_bits) {
                    BrotliDropBits(br, table->bits);
                    *result = table->value;
                    return true;
                }
                else {
                    return false; /* Not enough bits for the first level. */
                }
            }
            if (available_bits <= HUFFMAN_TABLE_BITS) {
                return false; /* Not enough bits to move to the second level. */
            }

            /* Speculatively drop HUFFMAN_TABLE_BITS. */
            val = (val & BitMask(table->bits)) >> HUFFMAN_TABLE_BITS;
            available_bits -= HUFFMAN_TABLE_BITS;
            table += table->value + val;
            if (available_bits < table->bits) {
                return false; /* Not enough bits for the second level. */
            }

            BrotliDropBits(br, (uint) (HUFFMAN_TABLE_BITS + table->bits));
            *result = table->value;
            return true;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool SafeReadSymbol(
            HuffmanCode* table, BrotliBitReader* br, uint* result) {
            uint val;
            if (BrotliSafeGetBits(br, 15, &val)) {
                *result = DecodeSymbol(val, table, br);
                return true;
            }
            return SafeDecodeSymbol(table, br, result);
        }

        /* WARNING: if state is not BROTLI_STATE_READ_BLOCK_LENGTH_NONE, then
            reading can't be continued with ReadBlockLength. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool SafeReadBlockLength(
            ref BrotliDecoderState s, uint* result, HuffmanCode* table,
            BrotliBitReader* br) {
            uint index;
            if (s.substate_read_block_length == BrotliRunningReadBlockLengthState.BROTLI_STATE_READ_BLOCK_LENGTH_NONE) {
                if (!SafeReadSymbol(table, br, &index)) {
                    return false;
                }
            }
            else {
                index = s.block_length_index;
            }
            {
                uint bits;
                uint nbits = kBlockLengthPrefixCode[index].nbits; /* nbits == 2..24 */
                if (!BrotliSafeReadBits(br, nbits, &bits)) {
                    s.block_length_index = index;
                    s.substate_read_block_length =
                        BrotliRunningReadBlockLengthState.BROTLI_STATE_READ_BLOCK_LENGTH_SUFFIX;
                    return false;
                }
                *result = kBlockLengthPrefixCode[index].offset + bits;
                s.substate_read_block_length = BrotliRunningReadBlockLengthState.BROTLI_STATE_READ_BLOCK_LENGTH_NONE;
                return true;
            }
        }

        /* Reads 1..256 2-bit context modes. */
        private static unsafe BrotliDecoderErrorCode ReadContextModes(ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                var i = s.loop_counter;
                fixed (uint* nbt = s.num_block_types)
                    while (i < (int) nbt[0]) {
                        uint bits;
                        if (!BrotliSafeReadBits(br, 2, &bits)) {
                            s.loop_counter = i;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        s.context_modes[i] = (byte) (bits << 1);
                        i++;
                    }
            }
            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
        }

        /* Transform:
            1) initialize list L with values 0, 1,... 255
            2) For each input element X:
            2.1) let Y = L[X]
            2.2) remove X-th element from L
            2.3) prepend Y to L
            2.4) append Y to output

           In most cases max(Y) <= 7, so most of L remains intact.
           To reduce the cost of initialization, we reuse L, remember the upper bound
           of Y values, and reinitialize only first elements in L.

           Most of input values are 0 and 1. To reduce number of branches, we replace
           inner for loop with do-while.
         */
        private static unsafe void InverseMoveToFrontTransform(
            byte* v, uint v_len, ref BrotliDecoderState state) {
            /* Reinitialize elements that could have been changed. */
            uint i = 1;
            var upper_bound = state.mtf_upper_bound;
            fixed (uint* m = state.mtf) {
                var mtf = &m[1]; /* Make mtf[-1] addressable. */
                var mtf_u8 = (byte*) mtf;
                /* Load endian-aware constant. */
                var b0123 = new byte[] {0, 1, 2, 3};
                uint pattern;
                fixed (byte* b = b0123)
                    memcpy(&pattern, b, 4);

                /* Initialize list using 4 consequent values pattern. */
                mtf[0] = pattern;
                do {
                    pattern += 0x04040404; /* Advance all 4 values by 4. */
                    mtf[i] = pattern;
                    i++;
                } while (i <= upper_bound);

                /* Transform the input. */
                upper_bound = 0;
                for (i = 0; i < v_len; ++i) {
                    int index = v[i];
                    var value = mtf_u8[index];
                    upper_bound |= v[i];
                    v[i] = value;
                    mtf_u8[-1] = value;
                    do {
                        index--;
                        mtf_u8[index + 1] = mtf_u8[index];
                    } while (index >= 0);
                }
                /* Remember amount of elements to be reinitialized. */
                state.mtf_upper_bound = upper_bound >> 2;
            }
        }

        /* Decodes a context map.
           Decoding is done in 4 phases:
            1) Read auxiliary information (6..16 bits) and allocate memory.
               In case of trivial context map, decoding is finished at this phase.
            2) Decode Huffman table using ReadHuffmanCode function.
               This table will be used for reading context map items.
            3) Read context map items; "0" values could be run-length encoded.
            4) Optionally, apply InverseMoveToFront transform to the resulting map.
         */
        private static unsafe BrotliDecoderErrorCode DecodeContextMap(uint context_map_size,
            uint* num_htrees,
            byte** context_map_arg,
            ref BrotliDecoderState s) {
            fixed (BrotliBitReader* br = &s.br) {
                var result = BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;

                switch (s.substate_context_map) {
                    case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_NONE:
                        result = DecodeVarLenUint8(ref s, br, num_htrees);
                        if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                            return result;
                        }
                        (*num_htrees)++;
                        s.context_index = 0;
                        *context_map_arg = (byte*) s.alloc_func(s.memory_manager_opaque, (size_t) context_map_size);
                        if (*context_map_arg == null) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_CONTEXT_MAP;
                        }
                        if (*num_htrees <= 1) {
                            memset(*context_map_arg, 0, (size_t) context_map_size);
                            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                        }
                        s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_READ_PREFIX;
                        /* No break, continue to next state. */
                        goto case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_READ_PREFIX;
                    case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_READ_PREFIX: {
                        uint bits;
                        /* In next stage ReadHuffmanCode uses at least 4 bits, so it is safe
                           to peek 4 bits ahead. */
                        if (!BrotliSafeGetBits(br, 5, &bits)) {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        if ((bits & 1) != 0) {
                            /* Use RLE for zeros. */
                            s.max_run_length_prefix = (bits >> 1) + 1;
                            BrotliDropBits(br, 5);
                        }
                        else {
                            s.max_run_length_prefix = 0;
                            BrotliDropBits(br, 1);
                        }
                        s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_HUFFMAN;
                        /* No break, continue to next state. */
                        goto case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_HUFFMAN;
                    }
                    case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_HUFFMAN:
                        fixed (HuffmanCode* cmt = s.context_map_table)
                            result = ReadHuffmanCode(*num_htrees + s.max_run_length_prefix,
                                cmt, null, ref s);
                        if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) return result;
                        s.code = 0xFFFF;
                        s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_DECODE;
                        /* No break, continue to next state. */
                        goto case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_DECODE;
                    case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_DECODE: {
                        var context_index = s.context_index;
                        var max_run_length_prefix = s.max_run_length_prefix;
                        var context_map = *context_map_arg;
                        var code = s.code;
                        var skip_preamble = (code != 0xffff);
                        while (context_index < context_map_size || skip_preamble) {
                            if (!skip_preamble) {
                                fixed (HuffmanCode* cmt = s.context_map_table)
                                    if (!SafeReadSymbol(cmt, br, &code)) {
                                        s.code = 0xFFFF;
                                        s.context_index = context_index;
                                        return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    }

                                if (code == 0) {
                                    context_map[context_index++] = 0;
                                    continue;
                                }
                                if (code > max_run_length_prefix) {
                                    context_map[context_index++] =
                                        (byte) (code - max_run_length_prefix);
                                    continue;
                                }
                            }
                            else {
                                skip_preamble = false;
                            }
                            /* RLE sub-stage. */
                            {
                                uint reps;
                                if (!BrotliSafeReadBits(br, code, &reps)) {
                                    s.code = code;
                                    s.context_index = context_index;
                                    return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                }
                                reps += 1U << (int) code;
                                //Debug.WriteLine((context_index + reps) + "," + context_map_size);
                                if (context_index + reps > context_map_size) {
                                    return
                                        BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_CONTEXT_MAP_REPEAT;
                                }
                                do {
                                    context_map[context_index++] = 0;
                                } while (--reps != 0);
                            }
                        }
                        /* No break, continue to next state. */
                        goto case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_TRANSFORM;
                    }
                    case BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_TRANSFORM: {
                        uint bits;
                        if (!BrotliSafeReadBits(br, 1, &bits)) {
                            s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_TRANSFORM;
                            return BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        }
                        if (bits != 0) {
                            InverseMoveToFrontTransform(*context_map_arg, context_map_size, ref s);
                        }
                        s.substate_context_map = BrotliRunningContextMapState.BROTLI_STATE_CONTEXT_MAP_NONE;
                        return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                    }
                    default:
                        return
                            BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE;
                }
            }
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void DetectTrivialLiteralBlockTypes(
            ref BrotliDecoderState s) {
            size_t i;
            fixed (uint* tlc = s.trivial_literal_contexts)
            fixed (uint* nbt = s.num_block_types) {
                for (i = 0; i < 8; ++i) tlc[i] = 0;
                for (i = 0; i < nbt[0]; i++) {
                    var offset = i << BROTLI_LITERAL_CONTEXT_BITS;
                    size_t error = 0;
                    size_t sample = (int) s.context_map[offset];
                    size_t j;
                    for (j = 0; j < (1u << BROTLI_LITERAL_CONTEXT_BITS);) {
                        for (var z = 1; z <= 4; z *= 2) {
                            if ((4 & z) != 0) {
                                for (var x = 0; x < z; x++) {
                                    error |= s.context_map[offset + j++] ^ sample;
                                }
                            }
                        }
                    }
                    if (error == 0) {
                        tlc[i >> 5] |= 1u << (int) (i & 31);
                    }
                }
            }
        }

        /* Decodes a series of Huffman table using ReadHuffmanCode function. */
        private static unsafe BrotliDecoderErrorCode HuffmanTreeGroupDecode(
            HuffmanTreeGroup* group, ref BrotliDecoderState s) {
            if (s.substate_tree_group != BrotliRunningTreeGroupState.BROTLI_STATE_TREE_GROUP_LOOP) {
                s.next = group->codes;
                s.htree_index = 0;
                s.substate_tree_group = BrotliRunningTreeGroupState.BROTLI_STATE_TREE_GROUP_LOOP;
            }
            while (s.htree_index < group->num_htrees) {
                uint table_size;
                var result =
                    ReadHuffmanCode(group->alphabet_size, s.next, &table_size, ref s);
                if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) return result;
                group->htrees[s.htree_index] = s.next;
                s.next += table_size;
                ++s.htree_index;
            }
            s.substate_tree_group = BrotliRunningTreeGroupState.BROTLI_STATE_TREE_GROUP_NONE;
            return BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void PrepareLiteralDecoding(ref BrotliDecoderState s) {
            byte context_mode;
            size_t trivial;
            fixed (uint* btr = s.block_type_rb)
            fixed (uint* tlc = s.trivial_literal_contexts) {
                var block_type = btr[1];
                var context_offset = block_type << BROTLI_LITERAL_CONTEXT_BITS;
                s.context_map_slice = s.context_map + context_offset;
                trivial = tlc[block_type >> 5];
                s.trivial_literal_context = (int) ((trivial >> (int) (block_type & 31)) & 1);
                s.literal_htree = s.literal_hgroup.htrees[s.context_map_slice[0]];
                context_mode = s.context_modes[block_type];
                fixed (byte* kcl = kContextLookup) {
                    s.context_lookup1 = &kcl[kContextLookupOffsets[context_mode]];
                    s.context_lookup2 = &kcl[kContextLookupOffsets[context_mode + 1]];
                }
            }
        }

        private static unsafe bool CheckInputAmount(
            int safe, BrotliBitReader* br, size_t num) {
            if (safe != 0) {
                return true;
            }
            return BrotliCheckInputAmount(br, num);
        }

        /* Reads and decodes the next Huffman code from bit-stream.
        This method peeks 16 bits of input and drops 0 - 15 of them. */
        private static unsafe uint ReadSymbol(HuffmanCode* table,
            BrotliBitReader* br) {
            return DecodeSymbol(BrotliGet16BitsUnmasked(br), table, br);
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        /* Decodes a block length by reading 3..39 bits. */
        private static unsafe uint ReadBlockLength(HuffmanCode* table,
            BrotliBitReader* br) {
            uint code;
            uint nbits;
            code = ReadSymbol(table, br);
            nbits = kBlockLengthPrefixCode[code].nbits; /* nbits == 2..24 */
            return kBlockLengthPrefixCode[code].offset + BrotliReadBits(br, nbits);
        }

        /* Decodes a command or literal and updates block type ring-buffer.
           Reads 3..54 bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool DecodeBlockTypeAndLength(
            int safe, ref BrotliDecoderState s, int tree_type) {
            fixed (uint* nbt = s.num_block_types) {
                var max_block_type = nbt[tree_type];
                var type_tree = &s.block_type_trees[
                    tree_type * BROTLI_HUFFMAN_MAX_SIZE_258];
                var len_tree = &s.block_len_trees[
                    tree_type * BROTLI_HUFFMAN_MAX_SIZE_26];
                fixed (BrotliBitReader* br = &s.br)
                fixed (uint* btr = s.block_type_rb)
                fixed (uint* bl = s.block_length) {
                    var ringbuffer = &btr[tree_type * 2];
                    uint block_type;

                    /* Read 0..15 + 3..39 bits */
                    if (safe == 0) {
                        block_type = ReadSymbol(type_tree, br);
                        bl[tree_type] = ReadBlockLength(len_tree, br);
                    }
                    else {
                        BrotliBitReaderState memento;
                        BrotliBitReaderSaveState(br, &memento);
                        if (!SafeReadSymbol(type_tree, br, &block_type)) return false;
                        if (!SafeReadBlockLength(ref s, &bl[tree_type], len_tree, br)) {
                            s.substate_read_block_length =
                                BrotliRunningReadBlockLengthState.BROTLI_STATE_READ_BLOCK_LENGTH_NONE;
                            BrotliBitReaderRestoreState(br, &memento);
                            return false;
                        }
                    }

                    if (block_type == 1) {
                        block_type = ringbuffer[1] + 1;
                    }
                    else if (block_type == 0) {
                        block_type = ringbuffer[0];
                    }
                    else {
                        block_type -= 2;
                    }
                    if (block_type >= max_block_type) {
                        block_type -= max_block_type;
                    }
                    ringbuffer[0] = ringbuffer[1];
                    ringbuffer[1] = block_type;
                    return true;
                }
            }
        }

        /* Block switch for insert/copy length.
           Reads 3..54 bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool DecodeCommandBlockSwitchInternal(
            int safe, ref BrotliDecoderState s) {
            if (!DecodeBlockTypeAndLength(safe, ref s, 1)) {
                return false;
            }
            fixed (uint* btr = s.block_type_rb)
                s.htree_command = s.insert_copy_hgroup.htrees[btr[3]];
            return true;
        }

        private static unsafe void DecodeCommandBlockSwitch(ref BrotliDecoderState s) {
            DecodeCommandBlockSwitchInternal(0, ref s);
        }

        private static unsafe bool SafeDecodeCommandBlockSwitch(
            ref BrotliDecoderState s) {
            return DecodeCommandBlockSwitchInternal(1, ref s);
        }

        private static unsafe bool SafeReadBits(
            BrotliBitReader* br, uint n_bits, uint* val) {
            if (n_bits != 0) {
                return BrotliSafeReadBits(br, n_bits, val);
            }
            else {
                *val = 0;
                return true;
            }
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool ReadCommandInternal(
            int safe, ref BrotliDecoderState s, BrotliBitReader* br, int* insert_length) {
            uint cmd_code;
            uint insert_len_extra = 0;
            uint copy_length = 0;
            CmdLutElement v;
            BrotliBitReaderState memento;
            if (safe == 0) {
                cmd_code = ReadSymbol(s.htree_command, br);
            }
            else {
                BrotliBitReaderSaveState(br, &memento);
                if (!SafeReadSymbol(s.htree_command, br, &cmd_code)) {
                    return false;
                }
            }
            v = kCmdLut[cmd_code];
            s.distance_code = v.distance_code;
            s.distance_context = v.context;
            s.dist_htree_index = s.dist_context_map_slice[s.distance_context];
            *insert_length = v.insert_len_offset;
            if (safe == 0) {
                if (v.insert_len_extra_bits != 0) {
                    insert_len_extra = BrotliReadBits(br, v.insert_len_extra_bits);
                }
                copy_length = BrotliReadBits(br, v.copy_len_extra_bits);
            }
            else {
                if (!SafeReadBits(br, v.insert_len_extra_bits, &insert_len_extra) ||
                    !SafeReadBits(br, v.copy_len_extra_bits, &copy_length)) {
                    BrotliBitReaderRestoreState(br, &memento);
                    return false;
                }
            }
            s.copy_length = (int) copy_length + v.copy_len_offset;
            fixed (uint* bl = s.block_length)
                --bl[1];
            *insert_length += (int) insert_len_extra;
            return true;
        }

        private static unsafe void ReadCommand(
            ref BrotliDecoderState s, BrotliBitReader* br, int* insert_length) {
            ReadCommandInternal(0, ref s, br, insert_length);
        }

        private static unsafe bool SafeReadCommand(
            ref BrotliDecoderState s, BrotliBitReader* br, int* insert_length) {
            return ReadCommandInternal(1, ref s, br, insert_length);
        }

        /* Makes a look-up in first level Huffman table. Peeks 8 bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void PreloadSymbol(int safe,
            HuffmanCode* table,
            BrotliBitReader* br,
            uint* bits,
            uint* value) {
            if (safe != 0) {
                return;
            }
            var t = table; // JIT ETW (Inlinee writes to an argument)
            t += BrotliGetBits(br, HUFFMAN_TABLE_BITS);
            *bits = t->bits;
            *value = t->value;
        }

        /* Decodes the block type and updates the state for literal context.
           Reads 3..54 bits. */
        private static unsafe bool DecodeLiteralBlockSwitchInternal(
            int safe, ref BrotliDecoderState s) {
            if (!DecodeBlockTypeAndLength(safe, ref s, 0)) {
                return false;
            }
            PrepareLiteralDecoding(ref s);
            return true;
        }

        private static unsafe void DecodeLiteralBlockSwitch(ref BrotliDecoderState s) {
            DecodeLiteralBlockSwitchInternal(0, ref s);
        }

        private static unsafe bool SafeDecodeLiteralBlockSwitch(
            ref BrotliDecoderState s) {
            return DecodeLiteralBlockSwitchInternal(1, ref s);
        }


        /* Decodes the next Huffman code using data prepared by PreloadSymbol.
           Reads 0 - 15 bits. Also peeks 8 following bits. */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe uint ReadPreloadedSymbol(HuffmanCode* table,
            BrotliBitReader* br,
            uint* bits,
            uint* value) {
            var result = *value;
            if (*bits > HUFFMAN_TABLE_BITS) {
                var val = BrotliGet16BitsUnmasked(br);
                var ext = table + (val & HUFFMAN_TABLE_MASK) + *value;
                var mask = BitMask((*bits - HUFFMAN_TABLE_BITS));
                BrotliDropBits(br, HUFFMAN_TABLE_BITS);
                ext += (val >> HUFFMAN_TABLE_BITS) & mask;
                BrotliDropBits(br, ext->bits);
                result = ext->value;
            }
            else {
                BrotliDropBits(br, *bits);
            }
            PreloadSymbol(0, table, br, bits, value);
            return result;
        }

        /* Block switch for distance codes.
           Reads 3..54 bits. */
        private static unsafe bool DecodeDistanceBlockSwitchInternal(
            int safe, ref BrotliDecoderState s) {
            if (!DecodeBlockTypeAndLength(safe, ref s, 2)) {
                return false;
            }
            fixed (uint* btr = s.block_type_rb)
                s.dist_context_map_slice = s.dist_context_map +
                                           (btr[5] << BROTLI_DISTANCE_CONTEXT_BITS);
            s.dist_htree_index = s.dist_context_map_slice[s.distance_context];
            return true;
        }

        private static unsafe void DecodeDistanceBlockSwitch(ref BrotliDecoderState s) {
            DecodeDistanceBlockSwitchInternal(0, ref s);
        }

        private static unsafe bool SafeDecodeDistanceBlockSwitch(
            ref BrotliDecoderState s) {
            return DecodeDistanceBlockSwitchInternal(1, ref s);
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe void TakeDistanceFromRingBuffer(ref BrotliDecoderState s) {
            fixed (int* drb = s.dist_rb) {
                if (s.distance_code == 0) {
                    --s.dist_rb_idx;
                    s.distance_code = drb[s.dist_rb_idx & 3];
                    /* Compensate double distance-ring-buffer roll for dictionary items. */
                    s.distance_context = 1;
                }
                else {
                    var distance_code = s.distance_code << 1;
                    /* kDistanceShortCodeIndexOffset has 2-bit values from LSB: */
                    /* 3, 2, 1, 0, 3, 3, 3, 3, 3, 3, 2, 2, 2, 2, 2, 2 */
                    const uint kDistanceShortCodeIndexOffset = 0xaaafff1b;
                    /* kDistanceShortCodeValueOffset has 2-bit values from LSB: */
                    /*-0, 0,-0, 0,-1, 1,-2, 2,-3, 3,-1, 1,-2, 2,-3, 3 */
                    const uint kDistanceShortCodeValueOffset = 0xfa5fa500;
                    var v = (s.dist_rb_idx +
                             (int) (kDistanceShortCodeIndexOffset >> distance_code)) & 0x3;
                    s.distance_code = drb[v];
                    v = (int) (kDistanceShortCodeValueOffset >> distance_code) & 0x3;
                    if ((distance_code & 0x3) != 0) {
                        s.distance_code += v;
                    }
                    else {
                        s.distance_code -= v;
                        if (s.distance_code <= 0) {
                            /* A huge distance will cause a BROTLI_FAILURE() soon. */
                            /* This is a little faster than failing here. */
                            s.distance_code = 0x0fffffff;
                        }
                    }
                }
            }
        }

        /* Precondition: s->distance_code < 0 */
#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe bool ReadDistanceInternal(
            int safe, ref BrotliDecoderState s, BrotliBitReader* br) {
            fixed (uint* bl = s.block_length) {
                int distval;
                BrotliBitReaderState memento;
                var distance_tree = s.distance_hgroup.htrees[s.dist_htree_index];
                if (safe == 0) {
                    s.distance_code = (int) ReadSymbol(distance_tree, br);
                }
                else {
                    uint code;
                    BrotliBitReaderSaveState(br, &memento);
                    if (!SafeReadSymbol(distance_tree, br, &code)) {
                        return false;
                    }
                    s.distance_code = (int) code;
                }
                /* Convert the distance code to the actual distance by possibly */
                /* looking up past distances from the s.ringbuffer. */
                s.distance_context = 0;
                if ((s.distance_code & ~0xf) == 0) {
                    TakeDistanceFromRingBuffer(ref s);
                    --bl[2];
                    return true;
                }
                distval = s.distance_code - (int) s.num_direct_distance_codes;
                if (distval >= 0) {
                    uint nbits;
                    int postfix;
                    int offset;
                    if (safe == 0 && (s.distance_postfix_bits == 0)) {
                        nbits = ((uint) distval >> 1) + 1;
                        offset = ((2 + (distval & 1)) << (int) nbits) - 4;
                        s.distance_code = (int) s.num_direct_distance_codes + offset +
                                          (int) BrotliReadBits(br, nbits);
                    }
                    else {
                        /* This branch also works well when s.distance_postfix_bits == 0 */
                        uint bits;
                        postfix = distval & s.distance_postfix_mask;
                        distval >>= (int) s.distance_postfix_bits;
                        nbits = ((uint) distval >> 1) + 1;
                        if (safe != 0) {
                            if (!SafeReadBits(br, nbits, &bits)) {
                                s.distance_code = -1; /* Restore precondition. */
                                BrotliBitReaderRestoreState(br, &memento);
                                return false;
                            }
                        }
                        else {
                            bits = BrotliReadBits(br, nbits);
                        }
                        offset = ((2 + (distval & 1)) << (int) nbits) - 4;
                        s.distance_code = (int) s.num_direct_distance_codes +
                                          ((offset + (int) bits) << (int) s.distance_postfix_bits) + postfix;
                    }
                }
                s.distance_code = s.distance_code - BROTLI_NUM_DISTANCE_SHORT_CODES + 1;
                --bl[2];
                return true;
            }
        }

        private static unsafe void ReadDistance(
            ref BrotliDecoderState s, BrotliBitReader* br) {
            ReadDistanceInternal(0, ref s, br);
        }

        private static unsafe bool SafeReadDistance(
            ref BrotliDecoderState s, BrotliBitReader* br) {
            return ReadDistanceInternal(1, ref s, br);
        }

#if AGGRESSIVE_INLINING
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        private static unsafe BrotliDecoderErrorCode ProcessCommandsInternal(
            int safe, ref BrotliDecoderState s) {
            var pos = s.pos;
            var i = s.loop_counter;
            var result = BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
            fixed (BrotliBitReader* br = &s.br) {
                fixed (uint* bl = s.block_length) {
                    if (!CheckInputAmount(safe, br, 28)) {
                        result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        goto saveStateAndReturn;
                    }
                    if (safe == 0) {
                        BrotliWarmupBitReader(br);
                    }

                    /* Jump into state machine. */
                    if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN) {
                        goto CommandBegin;
                    }
                    else if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_INNER) {
                        goto CommandInner;
                    }
                    else if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_POST_DECODE_LITERALS) {
                        goto CommandPostDecodeLiterals;
                    }
                    else if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRAP_COPY) {
                        goto CommandPostWrapCopy;
                    }
                    else {
                        return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE;
                    }

                    CommandBegin:
                    if (safe != 0) {
                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN;
                    }
                    if (!CheckInputAmount(safe, br, 28)) {
                        /* 156 bits + 7 bytes */
                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN;
                        result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                        goto saveStateAndReturn;
                    }
                    if (bl[1] == 0) {
                        if (safe != 0) {
                            if (!SafeDecodeCommandBlockSwitch(ref s)) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                goto saveStateAndReturn;
                            }
                        }
                        else {
                            DecodeCommandBlockSwitch(ref s);
                        }
                        goto CommandBegin;
                    }
                    /* Read the insert/copy length in the command */
                    if (safe != 0) {
                        if (!SafeReadCommand(ref s, br, &i)) {
                            result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                            goto saveStateAndReturn;
                        }
                    }
                    else {
                        ReadCommand(ref s, br, &i);
                    }
                    if (i == 0) {
                        goto CommandPostDecodeLiterals;
                    }
                    s.meta_block_remaining_len -= i;
                    CommandInner:
                    if (safe != 0) {
                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER;
                    }
                    /* Read the literals in the command */
                    if (s.trivial_literal_context != 0) {
                        uint bits;
                        uint value;
                        PreloadSymbol(safe, s.literal_htree, br, &bits, &value);
                        do {
                            if (!CheckInputAmount(safe, br, 28)) {
                                /* 162 bits + 7 bytes */
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER;
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                goto saveStateAndReturn;
                            }
                            if (bl[0] == 0) {
                                if (safe != 0) {
                                    if (!SafeDecodeLiteralBlockSwitch(ref s)) {
                                        result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                        goto saveStateAndReturn;
                                    }
                                }
                                else {
                                    DecodeLiteralBlockSwitch(ref s);
                                }
                                PreloadSymbol(safe, s.literal_htree, br, &bits, &value);
                                if (s.trivial_literal_context == 0) goto CommandInner;
                            }
                            if (safe == 0) {
                                s.ringbuffer[pos] =
                                    (byte) ReadPreloadedSymbol(s.literal_htree, br, &bits, &value);
                            }
                            else {
                                uint literal;
                                if (!SafeReadSymbol(s.literal_htree, br, &literal)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    goto saveStateAndReturn;
                                }
                                s.ringbuffer[pos] = (byte) literal;
                            }
                            --bl[0];
                            ++pos;
                            if (pos == s.ringbuffer_size) {
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER_WRITE;
                                --i;
                                goto saveStateAndReturn;
                            }
                        } while (--i != 0);
                    }
                    else {
                        var p1 = s.ringbuffer[(pos - 1) & s.ringbuffer_mask];
                        var p2 = s.ringbuffer[(pos - 2) & s.ringbuffer_mask];
                        do {
                            HuffmanCode* hc;
                            byte context;
                            if (!CheckInputAmount(safe, br, 28)) {
                                /* 162 bits + 7 bytes */
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER;
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                goto saveStateAndReturn;
                            }
                            if (bl[0] == 0) {
                                if (safe != 0) {
                                    if (!SafeDecodeLiteralBlockSwitch(ref s)) {
                                        result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                        goto saveStateAndReturn;
                                    }
                                }
                                else {
                                    DecodeLiteralBlockSwitch(ref s);
                                }
                                if (s.trivial_literal_context != 0) goto CommandInner;
                            }
                            context = (byte) (s.context_lookup1[p1] | s.context_lookup2[p2]);
                            hc = s.literal_hgroup.htrees[s.context_map_slice[context]];
                            p2 = p1;
                            if (safe == 0) {
                                p1 = (byte) ReadSymbol(hc, br);
                            }
                            else {
                                uint literal;
                                if (!SafeReadSymbol(hc, br, &literal)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    goto saveStateAndReturn;
                                }
                                p1 = (byte) literal;
                            }
                            s.ringbuffer[pos] = p1;
                            --bl[0];
                            ++pos;
                            if (pos == s.ringbuffer_size) {
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER_WRITE;
                                --i;
                                goto saveStateAndReturn;
                            }
                        } while (--i != 0);
                    }
                    if (s.meta_block_remaining_len <= 0) {
                        s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                        goto saveStateAndReturn;
                    }

                    CommandPostDecodeLiterals:
                    if (safe != 0) {
                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_POST_DECODE_LITERALS;
                    }
                    if (s.distance_code >= 0) {
                        /* Implicit distance case. */
                        s.distance_context = s.distance_code != 0 ? 0 : 1;
                        --s.dist_rb_idx;
                        fixed (int* dr = s.dist_rb)
                            s.distance_code = dr[s.dist_rb_idx & 3];
                    }
                    else {
                        /* Read distance code in the command, unless it was implicitly zero. */
                        if (bl[2] == 0) {
                            if (safe != 0) {
                                if (!SafeDecodeDistanceBlockSwitch(ref s)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    goto saveStateAndReturn;
                                }
                            }
                            else {
                                DecodeDistanceBlockSwitch(ref s);
                            }
                        }
                        if (safe != 0) {
                            if (!SafeReadDistance(ref s, br)) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                goto saveStateAndReturn;
                            }
                        }
                        else {
                            ReadDistance(ref s, br);
                        }
                    }
                    if (s.max_distance != s.max_backward_distance) {
                        s.max_distance =
                            (pos < s.max_backward_distance) ? pos : s.max_backward_distance;
                    }
                    i = s.copy_length;
                    /* Apply copy of LZ77 back-reference, or static dictionary reference if
                    the distance is larger than the max LZ77 distance */
                    if (s.distance_code > s.max_distance) {
                        if (i >= BROTLI_MIN_DICTIONARY_WORD_LENGTH &&
                            i <= BROTLI_MAX_DICTIONARY_WORD_LENGTH) {
                            var offset = (int) kBrotliDictionaryOffsetsByLength[i];
                            var word_id = s.distance_code - s.max_distance - 1;
                            uint shift = kBrotliDictionarySizeBitsByLength[i];
                            var mask = (int) BitMask(shift);
                            var word_idx = word_id & mask;
                            var transform_idx = word_id >> (int) shift;
                            offset += word_idx * i;
                            if (transform_idx < kNumTransforms) {
                                fixed (byte* dict = kBrotliDictionary) {
                                    var word = &dict[offset];
                                    var len = i;
                                    if (transform_idx == 0) {
                                        memcpy(&s.ringbuffer[pos], word, (size_t) len);
                                    }
                                    else {
                                        len = TransformDictionaryWord(
                                            &s.ringbuffer[pos], word, len, transform_idx);
                                    }
                                    pos += len;
                                    s.meta_block_remaining_len -= len;
                                    if (pos >= s.ringbuffer_size) {
                                        /*s.partial_pos_rb += (size_t)s.ringbuffer_size;*/
                                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_1;
                                        goto saveStateAndReturn;
                                    }
                                }
                            }
                            else {
                                return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_TRANSFORM;
                            }
                        }
                        else {
                            return BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_DICTIONARY;
                        }
                    }
                    else {
                        var src_start = (pos - s.distance_code) & s.ringbuffer_mask;
                        var copy_dst = &s.ringbuffer[pos];
                        var copy_src = &s.ringbuffer[src_start];
                        var dst_end = pos + i;
                        var src_end = src_start + i;
                        /* update the recent distances cache */
                        fixed (int* drb = s.dist_rb)
                            drb[s.dist_rb_idx & 3] = s.distance_code;
                        ++s.dist_rb_idx;
                        s.meta_block_remaining_len -= i;
                        /* There are 32+ bytes of slack in the ring-buffer allocation.
                           Also, we have 16 short codes, that make these 16 bytes irrelevant
                           in the ring-buffer. Let's copy over them as a first guess.
                         */
                        memmove16(copy_dst, copy_src);
                        if (src_end > pos && dst_end > src_start) {
                            /* Regions intersect. */
                            goto CommandPostWrapCopy;
                        }
                        if (dst_end >= s.ringbuffer_size || src_end >= s.ringbuffer_size) {
                            /* At least one region wraps. */
                            goto CommandPostWrapCopy;
                        }
                        pos += i;
                        if (i > 16) {
                            if (i > 32) {
                                memcpy(copy_dst + 16, copy_src + 16, (size_t) (i - 16));
                            }
                            else {
                                /* This branch covers about 45% cases.
                                   Fixed size short copy allows more compiler optimizations. */
                                memmove16(copy_dst + 16, copy_src + 16);
                            }
                        }
                    }
                    if (s.meta_block_remaining_len <= 0) {
                        /* Next metablock, if any */
                        s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                        goto saveStateAndReturn;
                    }
                    else {
                        goto CommandBegin;
                    }
                    CommandPostWrapCopy:
                    {
                        var wrap_guard = s.ringbuffer_size - pos;
                        while (--i >= 0) {
                            s.ringbuffer[pos] =
                                s.ringbuffer[(pos - s.distance_code) & s.ringbuffer_mask];
                            ++pos;
                            if (--wrap_guard == 0) {
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_2;
                                goto saveStateAndReturn;
                            }
                        }
                    }
                    if (s.meta_block_remaining_len <= 0) {
                        /* Next metablock, if any */
                        s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                        goto saveStateAndReturn;
                    }
                    else {
                        goto CommandBegin;
                    }

                    saveStateAndReturn:
                    s.pos = pos;
                    s.loop_counter = i;
                    return result;
                }
            }
        }

        private static BrotliDecoderErrorCode ProcessCommands(
            ref BrotliDecoderState s) {
            return ProcessCommandsInternal(0, ref s);
        }

        private static BrotliDecoderErrorCode SafeProcessCommands(
            ref BrotliDecoderState s) {
            return ProcessCommandsInternal(1, ref s);
        }

        private static unsafe void WrapRingBuffer(ref BrotliDecoderState s) {
            if (s.should_wrap_ringbuffer) {
                memcpy(s.ringbuffer, s.ringbuffer_end, (size_t) s.pos);
                s.should_wrap_ringbuffer = false;
            }
        }

        private static unsafe void BrotliDecoderHuffmanTreeGroupRelease(
            ref BrotliDecoderState s, HuffmanTreeGroup* group) {
            s.free_func(s.memory_manager_opaque, group->htrees);
            group->htrees = null;
        }

        internal static unsafe BrotliDecoderResult BrotliDecoderDecompressStream(
            ref BrotliDecoderState s, size_t* available_in, byte** next_in,
            size_t* available_out, byte** next_out, size_t* total_out) {
            var result = BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
            fixed (BrotliBitReader* br = &s.br) {
                if (*available_out != 0 && (next_out == null || *next_out == null))
                    return SaveErrorCode(ref s, BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_INVALID_ARGUMENTS);
                if (*available_out == 0) next_out = null;
                if (s.buffer_length == 0) {
                    /* Just connect bit reader to input stream. */
                    br->avail_in = *available_in;
                    br->next_in = *next_in;
                }
                else {
                    /* At least one byte of input is required. More than one byte of input may
                       be required to complete the transaction -> reading more data must be
                       done in a loop -> do it in a main loop. */
                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                    br->next_in = &s.buffer.u8[0];
                }

                /* State machine */
                for (;;) {
                    if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                        /* Error, needs more input/output */
                        if (result == BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT) {
                            if (s.ringbuffer != null) {
                                /* Pro-actively push output. */
                                WriteRingBuffer(ref s, available_out, next_out, total_out, true);
                            }
                            if (s.buffer_length != 0) {
                                /* Used with internal buffer. */
                                if (br->avail_in == 0) {
                                    /* Successfully finished read transaction. */
                                    /* Accumulator contains less than 8 bits, because internal buffer
                                       is expanded byte-by-byte until it is enough to complete read. */
                                    s.buffer_length = 0;
                                    /* Switch to input stream and restart. */
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                                    br->avail_in = *available_in;
                                    br->next_in = *next_in;
                                    continue;
                                }
                                else if (*available_in != 0) {
                                    /* Not enough data in buffer, but can take one more byte from
                                       input stream. */
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS;
                                    s.buffer.u8[s.buffer_length] = **next_in;
                                    s.buffer_length++;
                                    br->avail_in = s.buffer_length;
                                    (*next_in)++;
                                    (*available_in)--;
                                    /* Retry with more data in buffer. */
                                    continue;
                                }
                                /* Can't finish reading and no more input.*/
                                break;
                            }
                            else {
                                /* Input stream doesn't contain enough input. */
                                /* Copy tail to internal buffer and return. */
                                *next_in = br->next_in;
                                *available_in = br->avail_in;
                                while (*available_in != 0) {
                                    s.buffer.u8[s.buffer_length] = **next_in;
                                    s.buffer_length++;
                                    (*next_in)++;
                                    (*available_in)--;
                                }
                                break;
                            }
                            /* Unreachable. */
                        }

                        /* Fail or needs more output. */
                        if (s.buffer_length != 0) {
                            /* Just consumed the buffered input and produced some output. Otherwise
                               it would result in "needs more input". Reset internal buffer.*/
                            s.buffer_length = 0;
                        }
                        else {
                            /* Using input stream in last iteration. When decoder switches to input
                               stream it has less than 8 bits in accumulator, so it is safe to
                               return unused accumulator bits there. */
                            BrotliBitReaderUnload(br);
                            *available_in = br->avail_in;
                            *next_in = br->next_in;
                        }
                        break;
                    }
                    switch (s.state) {
                        case BrotliRunningState.BROTLI_STATE_UNINITED:
                            /* Prepare to the first read. */
                            if (!BrotliWarmupBitReader(br)) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                break;
                            }
                            /* Decode window size. */
                            s.window_bits = DecodeWindowBits(br); /* Reads 1..7 bits. */
                            if (s.window_bits == 9) {
                                /* Value 9 is reserved for future use. */
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_WINDOW_BITS;
                                break;
                            }
                            /* Maximum distance, see section 9.1. of the spec. */
                            s.max_backward_distance = (1 << (int) s.window_bits) - BROTLI_WINDOW_GAP;
                            /* Limit custom dictionary size. */
                            if (s.custom_dict_size >= s.max_backward_distance) {
                                s.custom_dict += s.custom_dict_size - s.max_backward_distance;
                                s.custom_dict_size = s.max_backward_distance;
                            }

                            /* Allocate memory for both block_type_trees and block_len_trees. */
                            s.block_type_trees = (HuffmanCode*) s.alloc_func(s.memory_manager_opaque,
                                sizeof(HuffmanCode) * 3 *
                                (BROTLI_HUFFMAN_MAX_SIZE_258 + BROTLI_HUFFMAN_MAX_SIZE_26));
                            if (s.block_type_trees == null) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_BLOCK_TYPE_TREES;
                                break;
                            }
                            s.block_len_trees =
                                s.block_type_trees + 3 * BROTLI_HUFFMAN_MAX_SIZE_258;

                            s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_BEGIN;
                            goto case BrotliRunningState.BROTLI_STATE_METABLOCK_BEGIN;
                        /* No break, continue to next state */
                        case BrotliRunningState.BROTLI_STATE_METABLOCK_BEGIN:
                            BrotliDecoderStateMetablockBegin(ref s);
                            s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_HEADER;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_METABLOCK_HEADER;
                        /* No break, continue to next state */
                        case BrotliRunningState.BROTLI_STATE_METABLOCK_HEADER:
                            result = DecodeMetaBlockLength(ref s, br); /* Reads 2 - 31 bits. */
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                break;
                            }
                            if (s.is_metadata || s.is_uncompressed) {
                                if (!BrotliJumpToByteBoundary(br)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_PADDING_1;
                                    break;
                                }
                            }
                            if (s.is_metadata) {
                                s.state = BrotliRunningState.BROTLI_STATE_METADATA;
                                break;
                            }
                            if (s.meta_block_remaining_len == 0) {
                                s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                                break;
                            }
                            BrotliCalculateRingBufferSize(ref s);
                            if (s.is_uncompressed) {
                                s.state = BrotliRunningState.BROTLI_STATE_UNCOMPRESSED;
                                break;
                            }
                            s.loop_counter = 0;
                            s.state = BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_0;
                            break;
                        case BrotliRunningState.BROTLI_STATE_UNCOMPRESSED: {
                            result = CopyUncompressedBlockToOutput(
                                available_out, next_out, total_out, ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                break;
                            }
                            s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                            break;
                        }
                        case BrotliRunningState.BROTLI_STATE_METADATA:
                            for (; s.meta_block_remaining_len > 0; --s.meta_block_remaining_len) {
                                uint bits;
                                /* Read one byte and ignore it. */
                                if (!BrotliSafeReadBits(br, 8, &bits)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    break;
                                }
                            }
                            if (result == BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                            }
                            break;
                        case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_0:
                            if (s.loop_counter >= 3) {
                                s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_HEADER_2;
                                break;
                            }
                            /* Reads 1..11 bits. */
                            fixed (uint* nbt = s.num_block_types) {
                                result = DecodeVarLenUint8(ref s, br, &nbt[s.loop_counter]);
                                if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                    break;
                                }
                                nbt[s.loop_counter]++;
                                if (nbt[s.loop_counter] < 2) {
                                    s.loop_counter++;
                                    break;
                                }
                            }
                            s.state = BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_1;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_1;
                        case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_1: {
                            var tree_offset = s.loop_counter * BROTLI_HUFFMAN_MAX_SIZE_258;
                            fixed (uint* nbt = s.num_block_types)
                                result = ReadHuffmanCode(nbt[s.loop_counter] + 2,
                                    &s.block_type_trees[tree_offset], null, ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) break;
                            s.state = BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_2;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_2;
                        }
                        case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_2: {
                            var tree_offset = s.loop_counter * BROTLI_HUFFMAN_MAX_SIZE_26;
                            result = ReadHuffmanCode(BROTLI_NUM_BLOCK_LEN_SYMBOLS,
                                &s.block_len_trees[tree_offset], null, ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) break;
                            s.state = BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_3;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_3;
                        }
                        case BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_3: {
                            var tree_offset = s.loop_counter * BROTLI_HUFFMAN_MAX_SIZE_26;
                            fixed (uint* bl = s.block_length)
                                if (!SafeReadBlockLength(ref s, &bl[s.loop_counter],
                                    &s.block_len_trees[tree_offset], br)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                    break;
                                }
                            s.loop_counter++;
                            s.state = BrotliRunningState.BROTLI_STATE_HUFFMAN_CODE_0;
                            break;
                        }
                        case BrotliRunningState.BROTLI_STATE_METABLOCK_HEADER_2: {
                            uint bits;
                            if (!BrotliSafeReadBits(br, 6, &bits)) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT;
                                break;
                            }
                            s.distance_postfix_bits = bits & BitMask(2);
                            bits >>= 2;
                            s.num_direct_distance_codes = BROTLI_NUM_DISTANCE_SHORT_CODES +
                                                          (bits << (int) s.distance_postfix_bits);
                            s.distance_postfix_mask = (int) BitMask(s.distance_postfix_bits);
                            fixed (uint* nbt = s.num_block_types)
                                s.context_modes =
                                    (byte*) s.alloc_func(s.memory_manager_opaque, (size_t) nbt[0]);
                            if (s.context_modes == null) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_CONTEXT_MODES;
                                break;
                            }
                            s.loop_counter = 0;
                            s.state = BrotliRunningState.BROTLI_STATE_CONTEXT_MODES;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_CONTEXT_MODES;
                        }
                        case BrotliRunningState.BROTLI_STATE_CONTEXT_MODES:
                            result = ReadContextModes(ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                break;
                            }
                            s.state = BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_1;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_1;
                        case BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_1:
                            fixed (uint* nbt = s.num_block_types)
                            fixed (uint* nlh = &s.num_literal_htrees)
                            fixed (byte** cm = &s.context_map)
                                result = DecodeContextMap(
                                    nbt[0] << BROTLI_LITERAL_CONTEXT_BITS,
                                    nlh, cm, ref s);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                break;
                            }
                            DetectTrivialLiteralBlockTypes(ref s);
                            s.state = BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_2;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_2;
                        case BrotliRunningState.BROTLI_STATE_CONTEXT_MAP_2: {
                            var num_distance_codes = (uint) (s.num_direct_distance_codes +
                                                             ((2 * BROTLI_MAX_DISTANCE_BITS) <<
                                                              (int) s.distance_postfix_bits));
                            var allocation_success = true;
                            fixed (uint* nbt = s.num_block_types)
                            fixed (uint* ndh = &s.num_dist_htrees)
                            fixed (byte** dcm = &s.dist_context_map) {
                                result = DecodeContextMap(
                                    nbt[2] << BROTLI_DISTANCE_CONTEXT_BITS,
                                    ndh, dcm, ref s);
                                if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                    break;
                                }
                                fixed (HuffmanTreeGroup* lh = &s.literal_hgroup)
                                    allocation_success &= BrotliDecoderHuffmanTreeGroupInit(
                                        ref s, lh, BROTLI_NUM_LITERAL_SYMBOLS,
                                        s.num_literal_htrees);
                                fixed (HuffmanTreeGroup* ich = &s.insert_copy_hgroup)
                                    allocation_success &= BrotliDecoderHuffmanTreeGroupInit(
                                        ref s, ich, BROTLI_NUM_COMMAND_SYMBOLS,
                                        nbt[1]);
                                fixed (HuffmanTreeGroup* dh = &s.distance_hgroup)
                                    allocation_success &= BrotliDecoderHuffmanTreeGroupInit(
                                        ref s, dh, num_distance_codes,
                                        s.num_dist_htrees);
                                if (!allocation_success) {
                                    return SaveErrorCode(ref s,
                                        BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_TREE_GROUPS);
                                }
                            }
                        }
                            s.loop_counter = 0;
                            s.state = BrotliRunningState.BROTLI_STATE_TREE_GROUP;
                            goto case BrotliRunningState.BROTLI_STATE_TREE_GROUP;
                        /* No break, continue to next state */
                        case BrotliRunningState.BROTLI_STATE_TREE_GROUP: {
                            HuffmanTreeGroup* hgroup = null;
                            fixed (HuffmanTreeGroup* lh = &s.literal_hgroup)
                            fixed (HuffmanTreeGroup* ich = &s.insert_copy_hgroup)
                            fixed (HuffmanTreeGroup* dh = &s.distance_hgroup) {
                                switch (s.loop_counter) {
                                    case 0:
                                        hgroup = lh;
                                        break;
                                    case 1:
                                        hgroup = ich;
                                        break;
                                    case 2:
                                        hgroup = dh;
                                        break;
                                    default:
                                        return SaveErrorCode(ref s,
                                            BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_UNREACHABLE);
                                }
                                result = HuffmanTreeGroupDecode(hgroup, ref s);
                            }
                        }
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) break;
                            s.loop_counter++;
                            if (s.loop_counter >= 3) {
                                PrepareLiteralDecoding(ref s);
                                s.dist_context_map_slice = s.dist_context_map;
                                s.htree_command = s.insert_copy_hgroup.htrees[0];
                                if (!BrotliEnsureRingBuffer(ref s)) {
                                    result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_ALLOC_RING_BUFFER_2;
                                    break;
                                }
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN;
                            }
                            break;
                        case BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN:
                        case BrotliRunningState.BROTLI_STATE_COMMAND_INNER:
                        case BrotliRunningState.BROTLI_STATE_COMMAND_POST_DECODE_LITERALS:
                        case BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRAP_COPY:
                            result = ProcessCommands(ref s);
                            if (result == BrotliDecoderErrorCode.BROTLI_DECODER_NEEDS_MORE_INPUT) {
                                result = SafeProcessCommands(ref s);
                            }
                            break;
                        case BrotliRunningState.BROTLI_STATE_COMMAND_INNER_WRITE:
                        case BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_1:
                        case BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_2:
                            result = WriteRingBuffer(
                                ref s, available_out, next_out, total_out, false);
                            if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                break;
                            }
                            WrapRingBuffer(ref s);
                            if (s.ringbuffer_size == 1 << (int) s.window_bits) {
                                s.max_distance = s.max_backward_distance;
                            }
                            if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_1) {
                                if (s.meta_block_remaining_len == 0) {
                                    /* Next metablock, if any */
                                    s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                                }
                                else {
                                    s.state = BrotliRunningState.BROTLI_STATE_COMMAND_BEGIN;
                                }
                                break;
                            }
                            else if (s.state == BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRITE_2) {
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_POST_WRAP_COPY;
                            }
                            else {
                                /* BROTLI_STATE_COMMAND_INNER_WRITE */
                                if (s.loop_counter == 0) {
                                    if (s.meta_block_remaining_len == 0) {
                                        s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_DONE;
                                    }
                                    else {
                                        s.state = BrotliRunningState.BROTLI_STATE_COMMAND_POST_DECODE_LITERALS;
                                    }
                                    break;
                                }
                                s.state = BrotliRunningState.BROTLI_STATE_COMMAND_INNER;
                            }
                            break;
                        case BrotliRunningState.BROTLI_STATE_METABLOCK_DONE:
                            if (s.meta_block_remaining_len < 0) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_BLOCK_LENGTH_2;
                                break;
                            }
                            BrotliDecoderStateCleanupAfterMetablock(ref s);
                            if (!s.is_last_metablock) {
                                s.state = BrotliRunningState.BROTLI_STATE_METABLOCK_BEGIN;
                                break;
                            }
                            if (!BrotliJumpToByteBoundary(br)) {
                                result = BrotliDecoderErrorCode.BROTLI_DECODER_ERROR_FORMAT_PADDING_2;
                                break;
                            }
                            if (s.buffer_length == 0) {
                                BrotliBitReaderUnload(br);
                                *available_in = br->avail_in;
                                *next_in = br->next_in;
                            }
                            s.state = BrotliRunningState.BROTLI_STATE_DONE;
                            /* No break, continue to next state */
                            goto case BrotliRunningState.BROTLI_STATE_DONE;
                        case BrotliRunningState.BROTLI_STATE_DONE:
                            if (s.ringbuffer != null) {
                                result = WriteRingBuffer(
                                    ref s, available_out, next_out, total_out, true);
                                if (result != BrotliDecoderErrorCode.BROTLI_DECODER_SUCCESS) {
                                    break;
                                }
                            }
                            return SaveErrorCode(ref s, result);
                    }
                }
            }
            return SaveErrorCode(ref s, result);
        }
    }
}