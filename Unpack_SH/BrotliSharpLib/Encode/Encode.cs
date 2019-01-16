using System;
using size_t = BrotliSharpLib.Brotli.SizeT;
using BrotliEncoderState = BrotliSharpLib.Brotli.BrotliEncoderStateStruct;

namespace BrotliSharpLib {
    public static partial class Brotli {
        internal static unsafe BrotliEncoderState BrotliEncoderCreateInstance(brotli_alloc_func alloc_func,
            brotli_free_func free_func, void* opaque) {
            BrotliEncoderState state = CreateStruct<BrotliEncoderState>();
            BrotliInitMemoryManager(
                ref state.memory_manager_, alloc_func, free_func, opaque);
            BrotliEncoderInitState(ref state);
            return state;
        }

        private static void BrotliEncoderInitParams(ref BrotliEncoderParams params_) {
            params_.mode = BROTLI_DEFAULT_MODE;
            params_.quality = BROTLI_DEFAULT_QUALITY;
            params_.lgwin = BROTLI_DEFAULT_WINDOW;
            params_.lgblock = 0;
            params_.size_hint = 0;
            params_.disable_literal_context_modeling = false;
        }

        private static unsafe void BrotliEncoderCleanupState(ref BrotliEncoderState s) {
            BrotliFree(ref s.memory_manager_, s.storage_);
            BrotliFree(ref s.memory_manager_, s.commands_);
            fixed (RingBuffer* rb = &s.ringbuffer_)
                RingBufferFree(ref s.memory_manager_, rb);
            fixed (HasherHandle* h = &s.hasher_)
                DestroyHasher(ref s.memory_manager_, h);
            BrotliFree(ref s.memory_manager_, s.large_table_);
            BrotliFree(ref s.memory_manager_, s.command_buf_);
            BrotliFree(ref s.memory_manager_, s.literal_buf_);
        }

        /* Deinitializes and frees BrotliEncoderState instance. */
        internal static unsafe void BrotliEncoderDestroyInstance(ref BrotliEncoderState state) {
            BrotliEncoderCleanupState(ref state);
        }

        private static unsafe void BrotliEncoderInitState(ref BrotliEncoderState s) {
            BrotliEncoderInitParams(ref s.params_);
            s.input_pos_ = 0;
            s.num_commands_ = 0;
            s.num_literals_ = 0;
            s.last_insert_len_ = 0;
            s.last_flush_pos_ = 0;
            s.last_processed_pos_ = 0;
            s.prev_byte_ = 0;
            s.prev_byte2_ = 0;
            s.storage_size_ = 0;
            s.storage_ = null;
            s.hasher_ = null;
            s.large_table_ = null;
            s.large_table_size_ = 0;
            s.cmd_code_numbits_ = 0;
            s.command_buf_ = null;
            s.literal_buf_ = null;
            s.next_out_ = null;
            s.available_out_ = 0;
            s.total_out_ = 0;
            s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING;
            s.is_last_block_emitted_ = false;
            s.is_initialized_ = false;

            RingBufferInit(ref s.ringbuffer_);

            s.commands_ = null;
            s.cmd_alloc_size_ = 0;

            /* Initialize distance cache. */
            fixed (int* dc = s.dist_cache_)
            fixed (int* sdc = s.saved_dist_cache_) {
                dc[0] = 4;
                dc[1] = 11;
                dc[2] = 15;
                dc[3] = 16;
                /* Save the state of the distance cache in case we need to restore it for
                   emitting an uncompressed block. */
                memcpy(sdc, dc, sizeof(int) * 4);
            }
        }

        internal static bool BrotliEncoderSetParameter(
            ref BrotliEncoderState state, BrotliEncoderParameter p, uint value) {
            /* Changing parameters on the fly is not implemented yet. */
            if (state.is_initialized_) return false;
            /* TODO: Validate/clamp parameters here. */
            switch (p) {
                case BrotliEncoderParameter.BROTLI_PARAM_MODE:
                    state.params_.mode = (BrotliEncoderMode) value;
                    return false;

                case BrotliEncoderParameter.BROTLI_PARAM_QUALITY:
                    state.params_.quality = (int) value;
                    return true;

                case BrotliEncoderParameter.BROTLI_PARAM_LGWIN:
                    state.params_.lgwin = (int) value;
                    return true;

                case BrotliEncoderParameter.BROTLI_PARAM_LGBLOCK:
                    state.params_.lgblock = (int) value;
                    return true;

                case BrotliEncoderParameter.BROTLI_PARAM_DISABLE_LITERAL_CONTEXT_MODELING:
                    if ((value != 0) && (value != 1)) return false;
                    state.params_.disable_literal_context_modeling = value != 0;
                    return true;

                case BrotliEncoderParameter.BROTLI_PARAM_SIZE_HINT:
                    state.params_.size_hint = value;
                    return true;

                default: return false;
            }
        }

        private static void EncodeWindowBits(int lgwin, out byte last_byte,
            out byte last_byte_bits) {
            if (lgwin == 16) {
                last_byte = 0;
                last_byte_bits = 1;
            }
            else if (lgwin == 17) {
                last_byte = 1;
                last_byte_bits = 7;
            }
            else if (lgwin > 17) {
                last_byte = (byte) (((lgwin - 17) << 1) | 1);
                last_byte_bits = 4;
            }
            else {
                last_byte = (byte) (((lgwin - 8) << 4) | 1);
                last_byte_bits = 7;
            }
        }

        /* Initializes the command and distance prefix codes for the first block. */
        private static unsafe void InitCommandPrefixCodes(byte* cmd_depths,
            ushort* cmd_bits,
            byte* cmd_code,
            size_t* cmd_code_numbits) {
            fixed (byte* kdcd = kDefaultCommandDepths)
                memcpy(cmd_depths, kdcd, kDefaultCommandDepths.Length);

            fixed (ushort* kdcb = kDefaultCommandBits)
                memcpy(cmd_bits, kdcb, kDefaultCommandBits.Length * sizeof(ushort));

            /* Initialize the pre-compressed form of the command and distance prefix
               codes. */
            fixed (byte* kdcc = kDefaultCommandCode)
                memcpy(cmd_code, kdcc, kDefaultCommandCode.Length);
            *cmd_code_numbits = kDefaultCommandCodeNumBits;
        }

        private static unsafe bool EnsureInitialized(ref BrotliEncoderState s) {
            if (s.is_initialized_) return true;

            fixed (BrotliEncoderParams* params_ = &s.params_) {
                SanitizeParams(params_);
                s.params_.lgblock = ComputeLgBlock(params_);

                s.remaining_metadata_bytes_ = uint.MaxValue;

                fixed (RingBuffer* rb = &s.ringbuffer_)
                    RingBufferSetup(params_, rb);

                /* Initialize last byte with stream header. */
                {
                    int lgwin = s.params_.lgwin;
                    if (params_->quality == FAST_ONE_PASS_COMPRESSION_QUALITY ||
                        params_->quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                        lgwin = Math.Max(lgwin, 18);
                    }
                    EncodeWindowBits(lgwin, out s.last_byte_, out s.last_byte_bits_);
                }

                if (params_->quality == FAST_ONE_PASS_COMPRESSION_QUALITY) {
                    fixed (byte* cmd_depths = s.cmd_depths_)
                    fixed (ushort* cmd_bits = s.cmd_bits_)
                    fixed (byte* cmd_code = s.cmd_code_)
                    fixed (size_t* cmd_code_numbits = &s.cmd_code_numbits_)
                        InitCommandPrefixCodes(cmd_depths, cmd_bits,
                            cmd_code, cmd_code_numbits);
                }
            }

            s.is_initialized_ = true;
            return true;
        }

        /*
           Copies the given input data to the internal ring buffer of the compressor.
           No processing of the data occurs at this time and this function can be
           called multiple times before calling WriteBrotliData() to process the
           accumulated input. At most input_block_size() bytes of input data can be
           copied to the ring buffer, otherwise the next WriteBrotliData() will fail.
         */
        private static unsafe void CopyInputToRingBuffer(ref BrotliEncoderState s,
            size_t input_size,
            byte* input_buffer) {
            if (!EnsureInitialized(ref s)) return;
            fixed (RingBuffer* ringbuffer_ = &s.ringbuffer_) {
                RingBufferWrite(ref s.memory_manager_, input_buffer, input_size, ringbuffer_);
                s.input_pos_ += input_size;

                /* TL;DR: If needed, initialize 7 more bytes in the ring buffer to make the
                   hashing not depend on uninitialized data. This makes compression
                   deterministic and it prevents uninitialized memory warnings in Valgrind.
                   Even without erasing, the output would be valid (but nondeterministic).
              
                   Background information: The compressor stores short (at most 8 bytes)
                   substrings of the input already read in a hash table, and detects
                   repetitions by looking up such substrings in the hash table. If it
                   can find a substring, it checks whether the substring is really there
                   in the ring buffer (or it's just a hash collision). Should the hash
                   table become corrupt, this check makes sure that the output is
                   still valid, albeit the compression ratio would be bad.
              
                   The compressor populates the hash table from the ring buffer as it's
                   reading new bytes from the input. However, at the last few indexes of
                   the ring buffer, there are not enough bytes to build full-length
                   substrings from. Since the hash table always contains full-length
                   substrings, we erase with dummy zeros here to make sure that those
                   substrings will contain zeros at the end instead of uninitialized
                   data.
              
                   Please note that erasing is not necessary (because the
                   memory region is already initialized since he ring buffer
                   has a `tail' that holds a copy of the beginning,) so we
                   skip erasing if we have already gone around at least once in
                   the ring buffer.
              
                   Only clear during the first round of ring-buffer writes. On
                   subsequent rounds data in the ring-buffer would be affected. */
                if (ringbuffer_->pos_ <= ringbuffer_->mask_) {
                    /* This is the first time when the ring buffer is being written.
                       We clear 7 bytes just after the bytes that have been copied from
                       the input buffer.
                
                       The ring-buffer has a "tail" that holds a copy of the beginning,
                       but only once the ring buffer has been fully written once, i.e.,
                       pos <= mask. For the first time, we need to write values
                       in this tail (where index may be larger than mask), so that
                       we have exactly defined behavior and don't read uninitialized
                       memory. Due to performance reasons, hashing reads data using a
                       LOAD64, which can go 7 bytes beyond the bytes written in the
                       ring-buffer. */
                    memset(ringbuffer_->buffer_ + ringbuffer_->pos_, 0, 7);
                }
            }
        }

        private static size_t InputBlockSize(ref BrotliEncoderState s) {
            if (!EnsureInitialized(ref s)) return 0;
            return (size_t) 1 << s.params_.lgblock;
        }

        private static ulong UnprocessedInputSize(ref BrotliEncoderState s) {
            return s.input_pos_ - s.last_processed_pos_;
        }

        private static unsafe void UpdateSizeHint(ref BrotliEncoderState s, size_t available_in) {
            if (s.params_.size_hint == 0) {
                ulong delta = UnprocessedInputSize(ref s);
                ulong tail = available_in;
                uint limit = 1u << 30;
                uint total;
                if ((delta >= limit) || (tail >= limit) || ((delta + tail) >= limit)) {
                    total = limit;
                }
                else {
                    total = (uint) (delta + tail);
                }
                s.params_.size_hint = total;
            }
        }

        private static unsafe void InjectBytePaddingBlock(ref BrotliEncoderState s) {
            uint seal = s.last_byte_;
            size_t seal_bits = s.last_byte_bits_;
            byte* destination;
            s.last_byte_ = 0;
            s.last_byte_bits_ = 0;
            /* is_last = 0, data_nibbles = 11, reserved = 0, meta_nibbles = 00 */
            seal |= 0x6u << (int) seal_bits;
            seal_bits += 6;
            /* If we have already created storage, then append to it.
               Storage is valid until next block is being compressed. */
            if (s.next_out_ != null) {
                destination = s.next_out_ + s.available_out_;
            }
            else {
                fixed (byte* tbu8 = s.tiny_buf_u8)
                    destination = tbu8;
                s.next_out_ = destination;
            }
            destination[0] = (byte) seal;
            if (seal_bits > 8) destination[1] = (byte) (seal >> 8);
            s.available_out_ += (seal_bits + 7) >> 3;
        }

        /* Injects padding bits or pushes compressed data to output.
           Returns false if nothing is done. */
        private static unsafe bool InjectFlushOrPushOutput(ref BrotliEncoderState s,
            size_t* available_out, byte** next_out, size_t* total_out) {
            if (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_FLUSH_REQUESTED &&
                s.last_byte_bits_ != 0) {
                InjectBytePaddingBlock(ref s);
                return true;
            }

            if (s.available_out_ != 0 && *available_out != 0) {
                size_t copy_output_size =
                    Math.Min(s.available_out_, *available_out);
                memcpy(*next_out, s.next_out_, copy_output_size);
                *next_out += copy_output_size;
                *available_out -= copy_output_size;
                s.next_out_ += copy_output_size;
                s.available_out_ -= copy_output_size;
                s.total_out_ += copy_output_size;
                if (total_out != null) *total_out = s.total_out_;
                return true;
            }

            return false;
        }

        /* Wraps 64-bit input position to 32-bit ring-buffer position preserving
           "not-a-first-lap" feature. */
        static uint WrapPosition(ulong position) {
            uint result = (uint) position;
            ulong gb = position >> 30;
            if (gb > 2) {
                /* Wrap every 2GiB; The first 3GB are continuous. */
                result = (result & ((1u << 30) - 1)) | ((uint) ((gb - 1) & 1) + 1) << 30;
            }
            return result;
        }

        private static unsafe byte* GetBrotliStorage(ref BrotliEncoderState s, size_t size) {
            if (s.storage_size_ < size) {
                BrotliFree(ref s.memory_manager_, s.storage_);
                s.storage_ = (byte*) BrotliAllocate(ref s.memory_manager_, size);
                s.storage_size_ = size;
            }
            return s.storage_;
        }

        private static size_t HashTableSize(size_t max_table_size, size_t input_size) {
            size_t htsize = 256;
            while (htsize < max_table_size && htsize < input_size) {
                htsize <<= 1;
            }
            return htsize;
        }

        private static unsafe int* GetHashTable(ref BrotliEncoderState s, int quality,
            size_t input_size, size_t* table_size) {
            /* Use smaller hash table when input.size() is smaller, since we
               fill the table, incurring O(hash table size) overhead for
               compression, and if the input is short, we won't need that
               many hash table entries anyway. */
            size_t max_table_size = MaxHashTableSize(quality);
            size_t htsize = HashTableSize(max_table_size, input_size);
            int* table;
            if (quality == FAST_ONE_PASS_COMPRESSION_QUALITY) {
                /* Only odd shifts are supported by fast-one-pass. */
                if ((htsize & 0xAAAAA) == 0) {
                    htsize <<= 1;
                }
            }

            if (htsize <= (1 << 10) / sizeof(int)) {
                fixed (int* st = s.small_table_)
                    table = st;
            }
            else {
                if (htsize > s.large_table_size_) {
                    s.large_table_size_ = htsize;
                    BrotliFree(ref s.memory_manager_, s.large_table_);
                    s.large_table_ = (int*) BrotliAllocate(ref s.memory_manager_, htsize * sizeof(int));
                }
                table = s.large_table_;
            }

            *table_size = htsize;
            memset(table, 0, htsize * sizeof(int));
            return table;
        }

        /* Marks all input as processed.
           Returns true if position wrapping occurs. */
        private static bool UpdateLastProcessedPos(ref BrotliEncoderState s) {
            uint wrapped_last_processed_pos = WrapPosition(s.last_processed_pos_);
            uint wrapped_input_pos = WrapPosition(s.input_pos_);
            s.last_processed_pos_ = s.input_pos_;
            return (wrapped_input_pos < wrapped_last_processed_pos);
        }

        private static unsafe bool ShouldCompress(
            byte* data, size_t mask, ulong last_flush_pos,
            size_t bytes, size_t num_literals, size_t num_commands) {
            if (num_commands < (bytes >> 8) + 2) {
                if (num_literals > 0.99 * (double) bytes) {
                    uint* literal_histo = stackalloc uint[256];
                    memset(literal_histo, 0, 256 * sizeof(uint));
                    const uint kSampleRate = 13;
                    const double kMinEntropy = 7.92;
                    double bit_cost_threshold =
                        (double) bytes * kMinEntropy / kSampleRate;
                    size_t t = (bytes + kSampleRate - 1) / kSampleRate;
                    uint pos = (uint) last_flush_pos;
                    size_t i;
                    for (i = 0; i < t; i++) {
                        ++literal_histo[data[pos & mask]];
                        pos += kSampleRate;
                    }
                    if (BitsEntropy(literal_histo, 256) > bit_cost_threshold) {
                        return false;
                    }
                }
            }
            return true;
        }

        /* Decide if we want to use a more complex private static unsafe context map containing 13
   context values, based on the entropy reduction of histograms over the
   first 5 bits of literals. */
        private static unsafe bool ShouldUseComplexStaticContextMap(byte* input,
            size_t start_pos, size_t length, size_t mask, int quality,
            size_t size_hint, ContextType* literal_context_mode,
            size_t* num_literal_contexts, uint** literal_context_map) {
            /* Try the more complex private static unsafe context map only for long data. */
            if (size_hint < (1 << 20)) {
                return false;
            }
            else {
                size_t end_pos = start_pos + length;
                /* To make entropy calculations faster and to fit on the stack, we collect
                   histograms over the 5 most significant bits of literals. One histogram
                   without context and 13 additional histograms for each context value. */
                uint* combined_histo = stackalloc uint[32];
                memset(combined_histo, 0, 32 * sizeof(uint));

                uint* context_histo = stackalloc uint[13 * 32];
                memset(context_histo, 0, 13 * 32 * sizeof(uint));

                uint total = 0;
                double* entropy = stackalloc double[3];
                size_t dummy;
                size_t i;
                for (; start_pos + 64 <= end_pos; start_pos += 4096) {
                    size_t stride_end_pos = start_pos + 64;
                    byte prev2 = input[start_pos & mask];
                    byte prev1 = input[(start_pos + 1) & mask];
                    size_t pos;
                    /* To make the analysis of the data faster we only examine 64 byte long
                       strides at every 4kB intervals. */
                    for (pos = start_pos + 2; pos < stride_end_pos; ++pos) {
                        byte literal = input[pos & mask];
                        byte context = (byte) kStaticContextMapComplexUTF8[
                            Context(prev1, prev2, ContextType.CONTEXT_UTF8)];
                        ++total;
                        ++combined_histo[literal >> 3];
                        ++context_histo[(context * 32) + (literal >> 3)];
                        prev2 = prev1;
                        prev1 = literal;
                    }
                }
                entropy[1] = ShannonEntropy(combined_histo, 32, &dummy);
                entropy[2] = 0;
                for (i = 0; i < 13; ++i) {
                    entropy[2] += ShannonEntropy(&context_histo[i * 32], 32, &dummy);
                }
                entropy[0] = 1.0 / (double) total;
                entropy[1] *= entropy[0];
                entropy[2] *= entropy[0];
                /* The triggering heuristics below were tuned by compressing the individual
                   files of the silesia corpus. If we skip this kind of context modeling
                   for not very well compressible input (i.e. entropy using context modeling
                   is 60% of maximal entropy) or if expected savings by symbol are less
                   than 0.2 bits, then in every case when it triggers, the final compression
                   ratio is improved. Note however that this heuristics might be too strict
                   for some cases and could be tuned further. */
                if (entropy[2] > 3.0 || entropy[1] - entropy[2] < 0.2) {
                    return false;
                }
                else {
                    *literal_context_mode = ContextType.CONTEXT_UTF8;
                    *num_literal_contexts = 13;
                    fixed (uint* context_map = kStaticContextMapComplexUTF8)
                        *literal_context_map = context_map;
                    return true;
                }
            }
        }

        private static unsafe void RecomputeDistancePrefixes(Command* cmds,
            size_t num_commands,
            uint num_direct_distance_codes,
            uint distance_postfix_bits) {
            size_t i;
            if (num_direct_distance_codes == 0 && distance_postfix_bits == 0) {
                return;
            }
            for (i = 0; i < num_commands; ++i) {
                Command* cmd = &cmds[i];
                if (CommandCopyLen(cmd) != 0 && cmd->cmd_prefix_ >= 128) {
                    PrefixEncodeCopyDistance(CommandRestoreDistanceCode(cmd),
                        num_direct_distance_codes,
                        distance_postfix_bits,
                        &cmd->dist_prefix_,
                        &cmd->dist_extra_);
                }
            }
        }

        /* Decide about the context map based on the ability of the prediction
   ability of the previous byte UTF8-prefix on the next byte. The
   prediction ability is calculated as Shannon entropy. Here we need
   Shannon entropy instead of 'BitsEntropy' since the prefix will be
   encoded with the remaining 6 bits of the following byte, and
   BitsEntropy will assume that symbol to be stored alone using Huffman
   coding. */
        private static unsafe void ChooseContextMap(int quality,
            uint* bigram_histo,
            size_t* num_literal_contexts,
            uint** literal_context_map) {
            uint* monogram_histo = stackalloc uint[3];
            memset(monogram_histo, 0, 3 * sizeof(uint));
            uint* two_prefix_histo = stackalloc uint[6];
            memset(two_prefix_histo, 0, 6 * sizeof(uint));
            size_t total;
            size_t i;
            size_t dummy;
            double* entropy = stackalloc double[4];
            for (i = 0; i < 9; ++i) {
                monogram_histo[i % 3] += bigram_histo[i];
                two_prefix_histo[i % 6] += bigram_histo[i];
            }
            entropy[1] = ShannonEntropy(monogram_histo, 3, &dummy);
            entropy[2] = (ShannonEntropy(two_prefix_histo, 3, &dummy) +
                          ShannonEntropy(two_prefix_histo + 3, 3, &dummy));
            entropy[3] = 0;
            for (i = 0; i < 3; ++i) {
                entropy[3] += ShannonEntropy(bigram_histo + 3 * i, 3, &dummy);
            }

            total = monogram_histo[0] + monogram_histo[1] + monogram_histo[2];
            entropy[0] = 1.0 / (double) total;
            entropy[1] *= entropy[0];
            entropy[2] *= entropy[0];
            entropy[3] *= entropy[0];

            if (quality < MIN_QUALITY_FOR_HQ_CONTEXT_MODELING) {
                /* 3 context models is a bit slower, don't use it at lower qualities. */
                entropy[3] = entropy[1] * 10;
            }
            /* If expected savings by symbol are less than 0.2 bits, skip the
               context modeling -- in exchange for faster decoding speed. */
            if (entropy[1] - entropy[2] < 0.2 &&
                entropy[1] - entropy[3] < 0.2) {
                *num_literal_contexts = 1;
            }
            else if (entropy[2] - entropy[3] < 0.02) {
                *num_literal_contexts = 2;
                fixed (uint* context_map = kStaticContextMapSimpleUTF8)
                    *literal_context_map = context_map;
            }
            else {
                *num_literal_contexts = 3;
                fixed (uint* context_map = kStaticContextMapContinuation)
                    *literal_context_map = context_map;
            }
        }

        private static unsafe void DecideOverLiteralContextModeling(byte* input,
            size_t start_pos, size_t length, size_t mask, int quality,
            size_t size_hint, ContextType* literal_context_mode,
            size_t* num_literal_contexts, uint** literal_context_map) {
            if (quality < MIN_QUALITY_FOR_CONTEXT_MODELING || length < 64) {
                return;
            }
            else if (ShouldUseComplexStaticContextMap(
                input, start_pos, length, mask, quality, size_hint, literal_context_mode,
                num_literal_contexts, literal_context_map)) {
                /* Context map was already set, nothing else to do. */
            }
            else {
                /* Gather bi-gram data of the UTF8 byte prefixes. To make the analysis of
                   UTF8 data faster we only examine 64 byte long strides at every 4kB
                   intervals. */
                size_t end_pos = start_pos + length;
                uint* bigram_prefix_histo = stackalloc uint[9];
                memset(bigram_prefix_histo, 0, 9 * sizeof(uint));
                for (; start_pos + 64 <= end_pos; start_pos += 4096) {
                    int[] lut = {0, 0, 1, 2};
                    size_t stride_end_pos = start_pos + 64;
                    int prev = lut[input[start_pos & mask] >> 6] * 3;
                    size_t pos;
                    for (pos = start_pos + 1; pos < stride_end_pos; ++pos) {
                        byte literal = input[pos & mask];
                        ++bigram_prefix_histo[prev + lut[literal >> 6]];
                        prev = lut[literal >> 6] * 3;
                    }
                }
                *literal_context_mode = ContextType.CONTEXT_UTF8;
                ChooseContextMap(quality, &bigram_prefix_histo[0], num_literal_contexts,
                    literal_context_map);
            }
        }

        private static unsafe void WriteMetaBlockInternal(ref MemoryManager m,
            byte* data,
            size_t mask,
            ulong last_flush_pos,
            size_t bytes,
            bool is_last,
            BrotliEncoderParams* params_,
            byte prev_byte,
            byte prev_byte2,
            size_t num_literals,
            size_t num_commands,
            Command* commands,
            int* saved_dist_cache,
            int* dist_cache,
            size_t* storage_ix,
            byte* storage) {
            uint wrapped_last_flush_pos = WrapPosition(last_flush_pos);
            byte last_byte;
            byte last_byte_bits;
            uint num_direct_distance_codes = 0;
            uint distance_postfix_bits = 0;

            if (bytes == 0) {
                /* Write the ISLAST and ISEMPTY bits. */
                BrotliWriteBits(2, 3, storage_ix, storage);
                *storage_ix = (*storage_ix + 7u) & ~7u;
                return;
            }

            if (!ShouldCompress(data, mask, last_flush_pos, bytes,
                num_literals, num_commands)) {
                /* Restore the distance cache, as its last update by
                   CreateBackwardReferences is now unused. */
                memcpy(dist_cache, saved_dist_cache, 4 * sizeof(int));
                BrotliStoreUncompressedMetaBlock(is_last, data,
                    wrapped_last_flush_pos, mask, bytes,
                    storage_ix, storage);
                return;
            }

            last_byte = storage[0];
            last_byte_bits = (byte) (*storage_ix & 0xff);
            if (params_->quality >= MIN_QUALITY_FOR_RECOMPUTE_DISTANCE_PREFIXES &&
                params_->mode == BrotliEncoderMode.BROTLI_MODE_FONT) {
                num_direct_distance_codes = 12;
                distance_postfix_bits = 1;
                RecomputeDistancePrefixes(commands,
                    num_commands,
                    num_direct_distance_codes,
                    distance_postfix_bits);
            }
            if (params_->quality <= MAX_QUALITY_FOR_STATIC_ENTROPY_CODES) {
                BrotliStoreMetaBlockFast(ref m, data, wrapped_last_flush_pos,
                    bytes, mask, is_last,
                    commands, num_commands,
                    storage_ix, storage);
            }
            else if (params_->quality < MIN_QUALITY_FOR_BLOCK_SPLIT) {
                BrotliStoreMetaBlockTrivial(ref m, data, wrapped_last_flush_pos,
                    bytes, mask, is_last,
                    commands, num_commands,
                    storage_ix, storage);
            }
            else {
                ContextType literal_context_mode = ContextType.CONTEXT_UTF8;
                MetaBlockSplit mb;
                InitMetaBlockSplit(&mb);
                if (params_->quality < MIN_QUALITY_FOR_HQ_BLOCK_SPLITTING) {
                    size_t num_literal_contexts = 1;
                    uint* literal_context_map = null;
                    if (!params_->disable_literal_context_modeling) {
                        DecideOverLiteralContextModeling(
                            data, wrapped_last_flush_pos, bytes, mask, params_->quality,
                            params_->size_hint, &literal_context_mode, &num_literal_contexts,
                            &literal_context_map);
                    }
                    BrotliBuildMetaBlockGreedy(ref m, data, wrapped_last_flush_pos, mask,
                        prev_byte, prev_byte2, literal_context_mode, num_literal_contexts,
                        literal_context_map, commands, num_commands, &mb);
                }
                else {
                    if (!BrotliIsMostlyUTF8(data, wrapped_last_flush_pos, mask, bytes,
                        kMinUTF8Ratio)) {
                        literal_context_mode = ContextType.CONTEXT_SIGNED;
                    }
                    BrotliBuildMetaBlock(ref m, data, wrapped_last_flush_pos, mask, params_,
                        prev_byte, prev_byte2,
                        commands, num_commands,
                        literal_context_mode,
                        &mb);
                }
                if (params_->quality >= MIN_QUALITY_FOR_OPTIMIZE_HISTOGRAMS) {
                    BrotliOptimizeHistograms(num_direct_distance_codes,
                        distance_postfix_bits,
                        &mb);
                }
                BrotliStoreMetaBlock(ref m, data, wrapped_last_flush_pos, bytes, mask,
                    prev_byte, prev_byte2,
                    is_last,
                    num_direct_distance_codes,
                    distance_postfix_bits,
                    literal_context_mode,
                    commands, num_commands,
                    &mb,
                    storage_ix, storage);
                DestroyMetaBlockSplit(ref m, &mb);
            }
            if (bytes + 4 < (*storage_ix >> 3)) {
                /* Restore the distance cache and last byte. */
                memcpy(dist_cache, saved_dist_cache, 4 * sizeof(int));
                storage[0] = last_byte;
                *storage_ix = last_byte_bits;
                BrotliStoreUncompressedMetaBlock(is_last, data,
                    wrapped_last_flush_pos, mask,
                    bytes, storage_ix, storage);
            }
        }

        /*
           Processes the accumulated input data and sets |*out_size| to the length of
           the new output meta-block, or to zero if no new output meta-block has been
           created (in this case the processed input data is buffered internally).
           If |*out_size| is positive, |*output| points to the start of the output
           data. If |is_last| or |force_flush| is true, an output meta-block is
           always created. However, until |is_last| is true encoder may retain up
           to 7 bits of the last byte of output. To force encoder to dump the remaining
           bits use WriteMetadata() to append an empty meta-data block.
           Returns false if the size of the input data is larger than
           input_block_size().
         */
        private static unsafe bool EncodeData(
            ref BrotliEncoderState s, bool is_last,
            bool force_flush, size_t* out_size, byte** output) {
            ulong delta = UnprocessedInputSize(ref s);
            uint bytes = (uint) delta;
            uint wrapped_last_processed_pos =
                WrapPosition(s.last_processed_pos_);
            byte* data;
            uint mask;

            if (!EnsureInitialized(ref s)) return false;
            data = s.ringbuffer_.buffer_;
            mask = s.ringbuffer_.mask_;

            /* Adding more blocks after "last" block is forbidden. */
            if (s.is_last_block_emitted_) return false;
            if (is_last) s.is_last_block_emitted_ = true;

            if (delta > InputBlockSize(ref s)) {
                return false;
            }
            if (s.params_.quality == FAST_TWO_PASS_COMPRESSION_QUALITY &&
                s.command_buf_ == null) {
                s.command_buf_ = (uint*)
                    BrotliAllocate(ref s.memory_manager_, kCompressFragmentTwoPassBlockSize * sizeof(uint));
                s.literal_buf_ = (byte*)
                    BrotliAllocate(ref s.memory_manager_, kCompressFragmentTwoPassBlockSize);
            }

            if (s.params_.quality == FAST_ONE_PASS_COMPRESSION_QUALITY ||
                s.params_.quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                byte* storage;
                size_t storage_ix = s.last_byte_bits_;
                size_t table_size;
                int* table;

                if (delta == 0 && !is_last) {
                    /* We have no new input data and we don't have to finish the stream, so
                       nothing to do. */
                    *out_size = 0;
                    return true;
                }
                storage = GetBrotliStorage(ref s, 2 * bytes + 502);
                storage[0] = s.last_byte_;
                table = GetHashTable(ref s, s.params_.quality, bytes, &table_size);
                if (s.params_.quality == FAST_ONE_PASS_COMPRESSION_QUALITY) {
                    fixed (byte* cd = s.cmd_depths_)
                    fixed (ushort* cb = s.cmd_bits_)
                    fixed (size_t* ccnb = &s.cmd_code_numbits_)
                    fixed (byte* cc = s.cmd_code_)
                        BrotliCompressFragmentFast(
                            ref s.memory_manager_, &data[wrapped_last_processed_pos & mask],
                            bytes, is_last,
                            table, table_size,
                            cd, cb,
                            ccnb, cc,
                            &storage_ix, storage);
                }
                else {
                    BrotliCompressFragmentTwoPass(
                        ref s.memory_manager_, &data[wrapped_last_processed_pos & mask],
                        bytes, is_last,
                        s.command_buf_, s.literal_buf_,
                        table, table_size,
                        &storage_ix, storage);
                }
                s.last_byte_ = storage[storage_ix >> 3];
                s.last_byte_bits_ = (byte) (storage_ix & 7u);
                UpdateLastProcessedPos(ref s);
                *output = &storage[0];
                *out_size = storage_ix >> 3;
                return true;
            }

            {
                /* Theoretical max number of commands is 1 per 2 bytes. */
                size_t newsize = s.num_commands_ + bytes / 2 + 1;
                if (newsize > s.cmd_alloc_size_) {
                    Command* new_commands;
                    /* Reserve a bit more memory to allow merging with a next block
                       without reallocation: that would impact speed. */
                    newsize += (bytes / 4) + 16;
                    s.cmd_alloc_size_ = newsize;
                    new_commands = (Command*) BrotliAllocate(ref s.memory_manager_, newsize * sizeof(Command));
                    if (s.commands_ != null) {
                        memcpy(new_commands, s.commands_, sizeof(Command) * s.num_commands_);
                        BrotliFree(ref s.memory_manager_, s.commands_);
                    }
                    s.commands_ = new_commands;
                }
            }

            fixed (BrotliEncoderParams* params_ = &s.params_) {
                fixed (HasherHandle* hasher_ = &s.hasher_)
                    InitOrStitchToPreviousBlock(ref s.memory_manager_, hasher_, data, mask, params_,
                        wrapped_last_processed_pos, bytes, is_last);

                fixed (int* dist_cache_ = s.dist_cache_)
                fixed (size_t* last_insert_len_ = &s.last_insert_len_)
                fixed (size_t* num_commands_ = &s.num_commands_)
                fixed (size_t* num_literals_ = &s.num_literals_) {
                    if (s.params_.quality == ZOPFLIFICATION_QUALITY) {
                        BrotliCreateZopfliBackwardReferences(
                            ref s.memory_manager_, bytes, wrapped_last_processed_pos, data, mask,
                            params_, s.hasher_, dist_cache_, last_insert_len_,
                            &s.commands_[s.num_commands_], num_commands_, num_literals_);
                    }
                    else if (s.params_.quality == HQ_ZOPFLIFICATION_QUALITY) {
                        BrotliCreateHqZopfliBackwardReferences(
                            ref s.memory_manager_, bytes, wrapped_last_processed_pos, data, mask,
                            params_, s.hasher_, dist_cache_, last_insert_len_,
                            &s.commands_[s.num_commands_], num_commands_, num_literals_);
                    }
                    else {
                        BrotliCreateBackwardReferences(
                            bytes, wrapped_last_processed_pos, data, mask,
                            params_, s.hasher_, dist_cache_, last_insert_len_,
                            &s.commands_[s.num_commands_], num_commands_, num_literals_);
                    }
                }

                {
                    size_t max_length = MaxMetablockSize(params_);
                    size_t max_literals = max_length / 8;
                    size_t max_commands = max_length / 8;
                    size_t processed_bytes = (size_t) (s.input_pos_ - s.last_flush_pos_);
                    /* If maximal possible additional block doesn't fit metablock, flush now. */
                    /* TODO: Postpone decision until next block arrives? */
                    bool next_input_fits_metablock = (
                        processed_bytes + InputBlockSize(ref s) <= max_length);
                    /* If block splitting is not used, then flush as soon as there is some
                       amount of commands / literals produced. */
                    bool should_flush = (
                        s.params_.quality < MIN_QUALITY_FOR_BLOCK_SPLIT &&
                        s.num_literals_ + s.num_commands_ >= MAX_NUM_DELAYED_SYMBOLS);
                    if (!is_last && !force_flush && !should_flush &&
                        next_input_fits_metablock &&
                        s.num_literals_ < max_literals &&
                        s.num_commands_ < max_commands) {
                        /* Merge with next input block. Everything will happen later. */
                        if (UpdateLastProcessedPos(ref s)) {
                            HasherReset(s.hasher_);
                        }
                        *out_size = 0;
                        return true;
                    }
                }

                /* Create the last insert-only command. */
                if (s.last_insert_len_ > 0) {
                    InitInsertCommand(&s.commands_[s.num_commands_++], s.last_insert_len_);
                    s.num_literals_ += s.last_insert_len_;
                    s.last_insert_len_ = 0;
                }

                if (!is_last && s.input_pos_ == s.last_flush_pos_) {
                    /* We have no new input data and we don't have to finish the stream, so
                       nothing to do. */
                    *out_size = 0;
                    return true;
                }
                {
                    uint metablock_size =
                        (uint) (s.input_pos_ - s.last_flush_pos_);
                    byte* storage = GetBrotliStorage(ref s, 2 * metablock_size + 502);
                    size_t storage_ix = s.last_byte_bits_;
                    storage[0] = s.last_byte_;
                    fixed (int* dc = s.dist_cache_)
                    fixed (int* sdc = s.saved_dist_cache_) {
                        WriteMetaBlockInternal(
                            ref s.memory_manager_, data, mask, s.last_flush_pos_, metablock_size, is_last,
                            params_, s.prev_byte_, s.prev_byte2_,
                            s.num_literals_, s.num_commands_, s.commands_, sdc,
                            dc, &storage_ix, storage);
                        s.last_byte_ = storage[storage_ix >> 3];
                        s.last_byte_bits_ = (byte) (storage_ix & 7u);
                        s.last_flush_pos_ = s.input_pos_;
                        if (UpdateLastProcessedPos(ref s)) {
                            HasherReset(s.hasher_);
                        }
                        if (s.last_flush_pos_ > 0) {
                            s.prev_byte_ = data[((uint) s.last_flush_pos_ - 1) & mask];
                        }
                        if (s.last_flush_pos_ > 1) {
                            s.prev_byte2_ = data[(uint) (s.last_flush_pos_ - 2) & mask];
                        }
                        s.num_commands_ = 0;
                        s.num_literals_ = 0;
                        /* Save the state of the distance cache in case we need to restore it for
                           emitting an uncompressed block. */
                        memcpy(sdc, dc, 4 * sizeof(int));
                    }
                    *output = &storage[0];
                    *out_size = storage_ix >> 3;
                    return true;
                }
            }
        }

        /* Dumps remaining output bits and metadata header to |header|.
           Returns number of produced bytes.
           REQUIRED: |header| should be 8-byte aligned and at least 16 bytes long.
           REQUIRED: |block_size| <= (1 << 24). */
        private static unsafe size_t WriteMetadataHeader(
            ref BrotliEncoderState s, size_t block_size, byte* header) {
            size_t storage_ix;
            storage_ix = s.last_byte_bits_;
            header[0] = s.last_byte_;
            s.last_byte_ = 0;
            s.last_byte_bits_ = 0;

            BrotliWriteBits(1, 0, &storage_ix, header);
            BrotliWriteBits(2, 3, &storage_ix, header);
            BrotliWriteBits(1, 0, &storage_ix, header);
            if (block_size == 0) {
                BrotliWriteBits(2, 0, &storage_ix, header);
            }
            else {
                uint nbits = (block_size == 1) ? 0 : (Log2FloorNonZero((uint) block_size - 1) + 1);
                uint nbytes = (nbits + 7) / 8;
                BrotliWriteBits(2, nbytes, &storage_ix, header);
                BrotliWriteBits(8 * nbytes, block_size - 1, &storage_ix, header);
            }
            return (storage_ix + 7u) >> 3;
        }

        private static unsafe bool ProcessMetadata(
            ref BrotliEncoderState s, size_t* available_in, byte** next_in,
            size_t* available_out, byte** next_out, size_t* total_out) {
            if (*available_in > (1u << 24)) return false;
            /* Switch to metadata block workflow, if required. */
            if (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING) {
                s.remaining_metadata_bytes_ = (uint) *available_in;
                s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_METADATA_HEAD;
            }
            if (s.stream_state_ != BrotliEncoderStreamState.BROTLI_STREAM_METADATA_HEAD &&
                s.stream_state_ != BrotliEncoderStreamState.BROTLI_STREAM_METADATA_BODY) {
                return false;
            }

            while (true) {
                if (InjectFlushOrPushOutput(ref s, available_out, next_out, total_out)) {
                    continue;
                }
                if (s.available_out_ != 0) break;

                if (s.input_pos_ != s.last_flush_pos_) {
                    fixed (size_t* available_out_ = &s.available_out_)
                    fixed (byte** next_out_ = &s.next_out_) {
                        bool result = EncodeData(ref s, false, true,
                            available_out_, next_out_);
                        if (!result) return false;
                        continue;
                    }
                }

                if (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_METADATA_HEAD) {
                    fixed (byte* tbu8 = s.tiny_buf_u8) {
                        s.next_out_ = tbu8;
                        s.available_out_ =
                            WriteMetadataHeader(ref s, s.remaining_metadata_bytes_, s.next_out_);
                    }
                    s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_METADATA_BODY;
                    continue;
                }
                else {
                    /* Exit workflow only when there is no more input and no more output.
                       Otherwise client may continue producing empty metadata blocks. */
                    if (s.remaining_metadata_bytes_ == 0) {
                        s.remaining_metadata_bytes_ = uint.MaxValue;
                        s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING;
                        break;
                    }
                    if (*available_out != 0) {
                        /* Directly copy input to output. */
                        uint copy = (uint) Math.Min(s.remaining_metadata_bytes_, *available_out);
                        memcpy(*next_out, *next_in, copy);
                        *next_in += copy;
                        *available_in -= copy;
                        s.remaining_metadata_bytes_ -= copy;
                        *next_out += copy;
                        *available_out -= copy;
                    }
                    else {
                        /* This guarantees progress in "TakeOutput" workflow. */
                        uint copy = Math.Min(s.remaining_metadata_bytes_, 16);
                        fixed (byte* tbu8 = s.tiny_buf_u8) {
                            s.next_out_ = tbu8;
                            memcpy(s.next_out_, *next_in, copy);
                        }
                        *next_in += copy;
                        *available_in -= copy;
                        s.remaining_metadata_bytes_ -= copy;
                        s.available_out_ = copy;
                    }
                    continue;
                }
            }

            return true;
        }

        private static unsafe size_t RemainingInputBlockSize(ref BrotliEncoderState s) {
            ulong delta = UnprocessedInputSize(ref s);
            size_t block_size = InputBlockSize(ref s);
            if (delta >= block_size) return 0;
            return block_size - (size_t) delta;
        }

        private static unsafe void CheckFlushComplete(ref BrotliEncoderState s) {
            if (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_FLUSH_REQUESTED &&
                s.available_out_ == 0) {
                s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING;
                s.next_out_ = null;
            }
        }

        private static unsafe bool BrotliEncoderCompressStreamFast(
            ref BrotliEncoderState s, BrotliEncoderOperation op, size_t* available_in,
            byte** next_in, size_t* available_out, byte** next_out,
            size_t* total_out) {
            size_t block_size_limit = (size_t) 1 << s.params_.lgwin;
            size_t buf_size = Math.Min(kCompressFragmentTwoPassBlockSize,
                Math.Min(*available_in, block_size_limit));
            uint* tmp_command_buf = null;
            uint* command_buf = null;
            byte* tmp_literal_buf = null;
            byte* literal_buf = null;
            if (s.params_.quality != FAST_ONE_PASS_COMPRESSION_QUALITY &&
                s.params_.quality != FAST_TWO_PASS_COMPRESSION_QUALITY) {
                return false;
            }
            if (s.params_.quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                if (s.command_buf_ == null && buf_size == kCompressFragmentTwoPassBlockSize) {
                    s.command_buf_ =
                        (uint*) BrotliAllocate(ref s.memory_manager_, kCompressFragmentTwoPassBlockSize * sizeof(uint));
                    s.literal_buf_ =
                        (byte*) BrotliAllocate(ref s.memory_manager_, kCompressFragmentTwoPassBlockSize * sizeof(byte));
                }
                if (s.command_buf_ != null) {
                    command_buf = s.command_buf_;
                    literal_buf = s.literal_buf_;
                }
                else {
                    tmp_command_buf = (uint*) BrotliAllocate(ref s.memory_manager_, buf_size * sizeof(uint));
                    tmp_literal_buf = (byte*) BrotliAllocate(ref s.memory_manager_, buf_size * sizeof(byte));
                    command_buf = tmp_command_buf;
                    literal_buf = tmp_literal_buf;
                }
            }

            while (true) {
                if (InjectFlushOrPushOutput(ref s, available_out, next_out, total_out)) {
                    continue;
                }

                /* Compress block only when internal output buffer is empty, stream is not
                   finished, there is no pending flush request, and there is either
                   additional input or pending operation. */
                if (s.available_out_ == 0 &&
                    s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING &&
                    (*available_in != 0 || op != BrotliEncoderOperation.BROTLI_OPERATION_PROCESS)) {
                    size_t block_size = Math.Min(block_size_limit, *available_in);
                    bool is_last =
                        (*available_in == block_size) && (op == BrotliEncoderOperation.BROTLI_OPERATION_FINISH);
                    bool force_flush =
                        (*available_in == block_size) && (op == BrotliEncoderOperation.BROTLI_OPERATION_FLUSH);
                    size_t max_out_size = 2 * block_size + 502;
                    bool inplace = true;
                    byte* storage = null;
                    size_t storage_ix = s.last_byte_bits_;
                    size_t table_size;
                    int* table;

                    if (force_flush && block_size == 0) {
                        s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_FLUSH_REQUESTED;
                        continue;
                    }
                    if (max_out_size <= *available_out) {
                        storage = *next_out;
                    }
                    else {
                        inplace = false;
                        storage = GetBrotliStorage(ref s, max_out_size);
                    }
                    storage[0] = s.last_byte_;
                    table = GetHashTable(ref s, s.params_.quality, block_size, &table_size);

                    if (s.params_.quality == FAST_ONE_PASS_COMPRESSION_QUALITY) {
                        fixed (byte* cmd_depths_ = s.cmd_depths_)
                        fixed (ushort* cmd_bits_ = s.cmd_bits_)
                        fixed (size_t* cmd_code_numbits_ = &s.cmd_code_numbits_)
                        fixed (byte* cmd_code_ = s.cmd_code_)
                            BrotliCompressFragmentFast(ref s.memory_manager_, *next_in, block_size, is_last, table,
                                table_size, cmd_depths_, cmd_bits_, cmd_code_numbits_,
                                cmd_code_, &storage_ix, storage);
                    }
                    else {
                        BrotliCompressFragmentTwoPass(ref s.memory_manager_, *next_in, block_size, is_last,
                            command_buf, literal_buf, table, table_size,
                            &storage_ix, storage);
                    }
                    *next_in += block_size;
                    *available_in -= block_size;
                    if (inplace) {
                        size_t out_bytes = storage_ix >> 3;
                        *next_out += out_bytes;
                        *available_out -= out_bytes;
                        s.total_out_ += out_bytes;
                        if (total_out != null) *total_out = s.total_out_;
                    }
                    else {
                        size_t out_bytes = storage_ix >> 3;
                        s.next_out_ = storage;
                        s.available_out_ = out_bytes;
                    }
                    s.last_byte_ = storage[storage_ix >> 3];
                    s.last_byte_bits_ = (byte) (storage_ix & 7u);

                    if (force_flush) s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_FLUSH_REQUESTED;
                    if (is_last) s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_FINISHED;
                    continue;
                }
                break;
            }
            BrotliFree(ref s.memory_manager_, tmp_command_buf);
            BrotliFree(ref s.memory_manager_, tmp_literal_buf);
            CheckFlushComplete(ref s);
            return true;
        }

        internal static unsafe bool BrotliEncoderCompressStream(
            ref BrotliEncoderState s, BrotliEncoderOperation op, size_t* available_in,
            byte** next_in, size_t* available_out, byte** next_out,
            size_t* total_out) {
            if (!EnsureInitialized(ref s)) return false;

            /* Unfinished metadata block; check requirements. */
            if (s.remaining_metadata_bytes_ != uint.MaxValue) {
                if (*available_in != s.remaining_metadata_bytes_) return false;
                if (op != BrotliEncoderOperation.BROTLI_OPERATION_EMIT_METADATA) return false;
            }

            if (op == BrotliEncoderOperation.BROTLI_OPERATION_EMIT_METADATA) {
                UpdateSizeHint(ref s, 0); /* First data metablock might be emitted here. */
                return ProcessMetadata(
                    ref s, available_in, next_in, available_out, next_out, total_out);
            }

            if (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_METADATA_HEAD ||
                s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_METADATA_BODY) {
                return false;
            }

            if (s.stream_state_ != BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING && *available_in != 0) {
                return false;
            }

            if (s.params_.quality == FAST_ONE_PASS_COMPRESSION_QUALITY ||
                s.params_.quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                return BrotliEncoderCompressStreamFast(ref s, op, available_in, next_in,
                    available_out, next_out, total_out);
            }
            while (true) {
                size_t remaining_block_size = RemainingInputBlockSize(ref s);

                if (remaining_block_size != 0 && *available_in != 0) {
                    size_t copy_input_size =
                        Math.Min(remaining_block_size, *available_in);
                    CopyInputToRingBuffer(ref s, copy_input_size, *next_in);
                    *next_in += copy_input_size;
                    *available_in -= copy_input_size;
                    continue;
                }

                if (InjectFlushOrPushOutput(ref s, available_out, next_out, total_out)) {
                    continue;
                }

                /* Compress data only when internal output buffer is empty, stream is not
                   finished and there is no pending flush request. */
                if (s.available_out_ == 0 &&
                    s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_PROCESSING) {
                    if (remaining_block_size == 0 || op != BrotliEncoderOperation.BROTLI_OPERATION_PROCESS) {
                        bool is_last = (
                            (*available_in == 0) && op == BrotliEncoderOperation.BROTLI_OPERATION_FINISH);
                        bool force_flush = (
                            (*available_in == 0) && op == BrotliEncoderOperation.BROTLI_OPERATION_FLUSH);
                        bool result;
                        UpdateSizeHint(ref s, *available_in);
                        fixed (size_t* available_out_ = &s.available_out_)
                        fixed (byte** next_out_ = &s.next_out_)
                            result = EncodeData(ref s, is_last, force_flush,
                                available_out_, next_out_);
                        if (!result) return false;
                        if (force_flush) s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_FLUSH_REQUESTED;
                        if (is_last) s.stream_state_ = BrotliEncoderStreamState.BROTLI_STREAM_FINISHED;
                        continue;
                    }
                }
                break;
            }
            CheckFlushComplete(ref s);
            return true;
        }

        internal static bool BrotliEncoderIsFinished(ref BrotliEncoderState s) {
            return (s.stream_state_ == BrotliEncoderStreamState.BROTLI_STREAM_FINISHED &&
                    !BrotliEncoderHasMoreOutput(ref s));
        }

        private static bool BrotliEncoderHasMoreOutput(ref BrotliEncoderState s) {
            return (s.available_out_ != 0);
        }

        internal static unsafe void BrotliEncoderSetCustomDictionary(ref BrotliEncoderState s, size_t size, byte* dict) {
            size_t max_dict_size = BROTLI_MAX_BACKWARD_LIMIT(s.params_.lgwin);
            size_t dict_size = size;

            if (!EnsureInitialized(ref s)) return;

            if (dict_size == 0 ||
                s.params_.quality == FAST_ONE_PASS_COMPRESSION_QUALITY ||
                s.params_.quality == FAST_TWO_PASS_COMPRESSION_QUALITY) {
                return;
            }
            if (size > max_dict_size) {
                dict += size - max_dict_size;
                dict_size = max_dict_size;
            }
            CopyInputToRingBuffer(ref s, dict_size, dict);
            s.last_flush_pos_ = dict_size;
            s.last_processed_pos_ = dict_size;
            if (dict_size > 0) {
                s.prev_byte_ = dict[dict_size - 1];
            }
            if (dict_size > 1) {
                s.prev_byte2_ = dict[dict_size - 2];
            }
            fixed (HasherHandle* hasher_ = &s.hasher_)
            fixed (BrotliEncoderParams* params_ = &s.params_)
                HasherPrependCustomDictionary(ref s.memory_manager_, hasher_, params_, dict_size, dict);
        }
    }
}