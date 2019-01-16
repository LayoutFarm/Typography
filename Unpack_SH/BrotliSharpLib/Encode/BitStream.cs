using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        [StructLayout(LayoutKind.Sequential)]
        private struct BlockTypeCodeCalculator {
            public size_t last_type;
            public size_t second_last_type;
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BlockSplitCode {
            public BlockTypeCodeCalculator type_code_calculator;
            public fixed byte type_depths[BROTLI_MAX_BLOCK_TYPE_SYMBOLS];
            public fixed ushort type_bits[BROTLI_MAX_BLOCK_TYPE_SYMBOLS];
            public fixed byte length_depths[BROTLI_NUM_BLOCK_LEN_SYMBOLS];
            public fixed ushort length_bits[BROTLI_NUM_BLOCK_LEN_SYMBOLS];
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BlockEncoder {
            public size_t alphabet_size_;
            public size_t num_block_types_;
            public byte* block_types_; /* Not owned. */
            public uint* block_lengths_; /* Not owned. */
            public size_t num_blocks_;
            public BlockSplitCode block_split_code_;
            public size_t block_ix_;
            public size_t block_len_;
            public size_t entropy_ix_;
            public byte* depths_;
            public ushort* bits_;
        }

        private static uint BlockLengthPrefixCode(uint len) {
            uint code = (len >= 177U) ? (len >= 753U ? 20U : 14U) : (len >= 41U ? 7U : 0U);
            while (code < (BROTLI_NUM_BLOCK_LEN_SYMBOLS - 1) &&
                   len >= kBlockLengthPrefixCode[code + 1].offset) ++code;
            return code;
        }

        private static unsafe void GetBlockLengthPrefixCode(uint len, size_t* code,
            uint* n_extra, uint* extra) {
            *code = BlockLengthPrefixCode(len);
            *n_extra = kBlockLengthPrefixCode[*code].nbits;
            *extra = len - kBlockLengthPrefixCode[*code].offset;
        }

        private static unsafe void InitBlockTypeCodeCalculator(BlockTypeCodeCalculator* self) {
            self->last_type = 1;
            self->second_last_type = 0;
        }

        private static unsafe size_t NextBlockTypeCode(
            BlockTypeCodeCalculator* calculator, byte type) {
            size_t type_code = (type == calculator->last_type + 1)
                ? 1u
                : (type == calculator->second_last_type)
                    ? 0u
                    : type + 2u;
            calculator->second_last_type = calculator->last_type;
            calculator->last_type = type;
            return type_code;
        }

        private static unsafe void InitBlockEncoder(BlockEncoder* self, size_t alphabet_size,
            size_t num_block_types, byte* block_types,
            uint* block_lengths, size_t num_blocks) {
            self->alphabet_size_ = alphabet_size;
            self->num_block_types_ = num_block_types;
            self->block_types_ = block_types;
            self->block_lengths_ = block_lengths;
            self->num_blocks_ = num_blocks;
            InitBlockTypeCodeCalculator(&self->block_split_code_.type_code_calculator);
            self->block_ix_ = 0;
            self->block_len_ = num_blocks == 0 ? 0 : block_lengths[0];
            self->entropy_ix_ = 0;
            self->depths_ = null;
            self->bits_ = null;
        }

        /* Stores a number between 0 and 255. */
        private static unsafe void StoreVarLenUint8(size_t n, size_t* storage_ix, byte* storage) {
            if (n == 0) {
                BrotliWriteBits(1, 0, storage_ix, storage);
            }
            else {
                size_t nbits = Log2FloorNonZero(n);
                BrotliWriteBits(1, 1, storage_ix, storage);
                BrotliWriteBits(3, nbits, storage_ix, storage);
                BrotliWriteBits(nbits, n - ((size_t) 1 << (int) nbits), storage_ix, storage);
            }
        }

        /* Stores the block switch command with index block_ix to the bit stream. */
        private static unsafe void StoreBlockSwitch(BlockSplitCode* code,
            uint block_len,
            byte block_type,
            bool is_first_block,
            size_t* storage_ix,
            byte* storage) {
            size_t typecode = NextBlockTypeCode(&code->type_code_calculator, block_type);
            size_t lencode;
            uint len_nextra;
            uint len_extra;
            if (!is_first_block) {
                BrotliWriteBits(code->type_depths[typecode], code->type_bits[typecode],
                    storage_ix, storage);
            }
            GetBlockLengthPrefixCode(block_len, &lencode, &len_nextra, &len_extra);

            BrotliWriteBits(code->length_depths[lencode], code->length_bits[lencode],
                storage_ix, storage);
            BrotliWriteBits(len_nextra, len_extra, storage_ix, storage);
        }

        /* Builds a BlockSplitCode data structure from the block split given by the
   vector of block types and block lengths and stores it to the bit stream. */
        private static unsafe void BuildAndStoreBlockSplitCode(byte* types,
            uint* lengths,
            size_t num_blocks,
            size_t num_types,
            HuffmanTree* tree,
            BlockSplitCode* code,
            size_t* storage_ix,
            byte* storage) {
            uint* type_histo = stackalloc uint[BROTLI_MAX_BLOCK_TYPE_SYMBOLS];
            uint* length_histo = stackalloc uint[BROTLI_NUM_BLOCK_LEN_SYMBOLS];
            size_t i;
            BlockTypeCodeCalculator type_code_calculator;
            memset(type_histo, 0, (num_types + 2) * sizeof(uint));
            memset(length_histo, 0, BROTLI_NUM_BLOCK_LEN_SYMBOLS * sizeof(uint));
            InitBlockTypeCodeCalculator(&type_code_calculator);
            for (i = 0; i < num_blocks; ++i) {
                size_t type_code = NextBlockTypeCode(&type_code_calculator, types[i]);
                if (i != 0) ++type_histo[type_code];
                ++length_histo[BlockLengthPrefixCode(lengths[i])];
            }
            StoreVarLenUint8(num_types - 1, storage_ix, storage);
            if (num_types > 1) {
                /* TODO: else? could StoreBlockSwitch occur? */
                BuildAndStoreHuffmanTree(&type_histo[0], num_types + 2, tree,
                    &code->type_depths[0], &code->type_bits[0],
                    storage_ix, storage);
                BuildAndStoreHuffmanTree(&length_histo[0], BROTLI_NUM_BLOCK_LEN_SYMBOLS,
                    tree, &code->length_depths[0],
                    &code->length_bits[0], storage_ix, storage);
                StoreBlockSwitch(code, lengths[0], types[0], true, storage_ix, storage);
            }
        }

        /* Creates entropy codes of block lengths and block types and stores them
           to the bit stream. */
        private static unsafe void BuildAndStoreBlockSwitchEntropyCodes(BlockEncoder* self,
            HuffmanTree* tree, size_t* storage_ix, byte* storage) {
            BuildAndStoreBlockSplitCode(self->block_types_, self->block_lengths_,
                self->num_blocks_, self->num_block_types_, tree, &self->block_split_code_,
                storage_ix, storage);
        }

        /* Stores a context map where the histogram type is always the block type. */
        private static unsafe void StoreTrivialContextMap(size_t num_types,
            size_t context_bits,
            HuffmanTree* tree,
            size_t* storage_ix,
            byte* storage) {
            StoreVarLenUint8(num_types - 1, storage_ix, storage);
            if (num_types > 1) {
                size_t repeat_code = context_bits - 1u;
                size_t repeat_bits = (1u << (int) repeat_code) - 1u;
                size_t alphabet_size = num_types + repeat_code;
                uint* histogram = stackalloc uint[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];
                byte* depths = stackalloc byte[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];
                ushort* bits = stackalloc ushort[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];
                size_t i;
                memset(histogram, 0, alphabet_size * sizeof(uint));
                /* Write RLEMAX. */
                BrotliWriteBits(1, 1, storage_ix, storage);
                BrotliWriteBits(4, repeat_code - 1, storage_ix, storage);
                histogram[repeat_code] = (uint) num_types;
                histogram[0] = 1;
                for (i = context_bits; i < alphabet_size; ++i) {
                    histogram[i] = 1;
                }
                BuildAndStoreHuffmanTree(histogram, alphabet_size, tree,
                    depths, bits, storage_ix, storage);
                for (i = 0; i < num_types; ++i) {
                    size_t code = (i == 0 ? 0 : i + context_bits - 1);
                    BrotliWriteBits(depths[code], bits[code], storage_ix, storage);
                    BrotliWriteBits(
                        depths[repeat_code], bits[repeat_code], storage_ix, storage);
                    BrotliWriteBits(repeat_code, repeat_bits, storage_ix, storage);
                }
                /* Write IMTF (inverse-move-to-front) bit. */
                BrotliWriteBits(1, 1, storage_ix, storage);
            }
        }

        private static unsafe size_t IndexOf(byte* v, size_t v_size, byte value) {
            size_t i = 0;
            for (; i < v_size; ++i) {
                if (v[i] == value) return i;
            }
            return i;
        }

        private static unsafe void MoveToFront(byte* v, size_t index) {
            byte value = v[index];
            size_t i;
            for (i = index; i != 0; --i) {
                v[i] = v[i - 1];
            }
            v[0] = value;
        }

        private static unsafe void MoveToFrontTransform(uint* v_in,
            size_t v_size,
            uint* v_out) {
            size_t i;
            byte* mtf = stackalloc byte[256];
            uint max_value;
            if (v_size == 0) {
                return;
            }
            max_value = v_in[0];
            for (i = 1; i < v_size; ++i) {
                if (v_in[i] > max_value) max_value = v_in[i];
            }
            for (i = 0; i <= max_value; ++i) {
                mtf[i] = (byte) i;
            }
            {
                size_t mtf_size = max_value + 1;
                for (i = 0; i < v_size; ++i) {
                    size_t index = IndexOf(mtf, mtf_size, (byte) v_in[i]);
                    v_out[i] = (uint) index;
                    MoveToFront(mtf, index);
                }
            }
        }

        /* Finds runs of zeros in v[0..in_size) and replaces them with a prefix code of
           the run length plus extra bits (lower 9 bits is the prefix code and the rest
           are the extra bits). Non-zero values in v[] are shifted by
           *max_length_prefix. Will not create prefix codes bigger than the initial
           value of *max_run_length_prefix. The prefix code of run length L is simply
           Log2Floor(L) and the number of extra bits is the same as the prefix code. */
        private static unsafe void RunLengthCodeZeros(size_t in_size,
            uint* v, size_t* out_size,
            uint* max_run_length_prefix) {
            uint max_reps = 0;
            size_t i;
            uint max_prefix;
            for (i = 0; i < in_size;) {
                uint reps = 0;
                for (; i < in_size && v[i] != 0; ++i) ;
                for (; i < in_size && v[i] == 0; ++i) {
                    ++reps;
                }
                max_reps = Math.Max(reps, max_reps);
            }
            max_prefix = max_reps > 0 ? Log2FloorNonZero(max_reps) : 0;
            max_prefix = Math.Min(max_prefix, *max_run_length_prefix);
            *max_run_length_prefix = max_prefix;
            *out_size = 0;
            for (i = 0; i < in_size;) {
                if (v[i] != 0) {
                    v[*out_size] = v[i] + *max_run_length_prefix;
                    ++i;
                    ++(*out_size);
                }
                else {
                    uint reps = 1;
                    size_t k;
                    for (k = i + 1; k < in_size && v[k] == 0; ++k) {
                        ++reps;
                    }
                    i += reps;
                    while (reps != 0) {
                        if (reps < (2u << (int) max_prefix)) {
                            uint run_length_prefix = Log2FloorNonZero(reps);
                            uint extra_bits = reps - (1u << (int) run_length_prefix);
                            v[*out_size] = run_length_prefix + (extra_bits << 9);
                            ++(*out_size);
                            break;
                        }
                        else {
                            uint extra_bits = (1u << (int) max_prefix) - 1u;
                            v[*out_size] = max_prefix + (extra_bits << 9);
                            reps -= (2u << (int) max_prefix) - 1u;
                            ++(*out_size);
                        }
                    }
                }
            }
        }

        private const int SYMBOL_BITS = 9;

        private static unsafe void EncodeContextMap(ref MemoryManager m,
            uint* context_map,
            size_t context_map_size,
            size_t num_clusters,
            HuffmanTree* tree,
            size_t* storage_ix, byte* storage) {
            size_t i;
            uint* rle_symbols;
            uint max_run_length_prefix = 6;
            size_t num_rle_symbols = 0;
            uint* histogram = stackalloc uint[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];
            const uint kSymbolMask = (1u << SYMBOL_BITS) - 1u;
            byte* depths = stackalloc byte[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];
            ushort* bits = stackalloc ushort[BROTLI_MAX_CONTEXT_MAP_SYMBOLS];

            StoreVarLenUint8(num_clusters - 1, storage_ix, storage);

            if (num_clusters == 1) {
                return;
            }

            rle_symbols = (uint*) BrotliAllocate(ref m, context_map_size * sizeof(uint));

            MoveToFrontTransform(context_map, context_map_size, rle_symbols);
            RunLengthCodeZeros(context_map_size, rle_symbols,
                &num_rle_symbols, &max_run_length_prefix);
            memset(histogram, 0, BROTLI_MAX_CONTEXT_MAP_SYMBOLS * sizeof(uint));
            for (i = 0; i < num_rle_symbols; ++i) {
                ++histogram[rle_symbols[i] & kSymbolMask];
            }
            {
                bool use_rle = (max_run_length_prefix > 0);
                BrotliWriteBits(1, use_rle ? 1U : 0U, storage_ix, storage);
                if (use_rle) {
                    BrotliWriteBits(4, max_run_length_prefix - 1, storage_ix, storage);
                }
            }
            BuildAndStoreHuffmanTree(histogram, num_clusters + max_run_length_prefix,
                tree, depths, bits, storage_ix, storage);
            for (i = 0; i < num_rle_symbols; ++i) {
                uint rle_symbol = rle_symbols[i] & kSymbolMask;
                uint extra_bits_val = rle_symbols[i] >> SYMBOL_BITS;
                BrotliWriteBits(depths[rle_symbol], bits[rle_symbol], storage_ix, storage);
                if (rle_symbol > 0 && rle_symbol <= max_run_length_prefix) {
                    BrotliWriteBits(rle_symbol, extra_bits_val, storage_ix, storage);
                }
            }
            BrotliWriteBits(1, 1, storage_ix, storage); /* use move-to-front */
            BrotliFree(ref m, rle_symbols);
        }

        /* Stores the next symbol with the entropy code of the current block type.
           Updates the block type and block length at block boundaries. */
        private static unsafe void StoreSymbol(BlockEncoder* self, size_t symbol, size_t* storage_ix,
            byte* storage) {
            if (self->block_len_ == 0) {
                size_t block_ix = ++self->block_ix_;
                uint block_len = self->block_lengths_[block_ix];
                byte block_type = self->block_types_[block_ix];
                self->block_len_ = block_len;
                self->entropy_ix_ = block_type * self->alphabet_size_;
                StoreBlockSwitch(&self->block_split_code_, block_len, block_type, false,
                    storage_ix, storage);
            }
            --self->block_len_;
            {
                size_t ix = self->entropy_ix_ + symbol;
                BrotliWriteBits(self->depths_[ix], self->bits_[ix], storage_ix, storage);
            }
        }

        /* Stores the next symbol with the entropy code of the current block type and
           context value.
           Updates the block type and block length at block boundaries. */
        private static unsafe void StoreSymbolWithContext(BlockEncoder* self, size_t symbol,
            size_t context, uint* context_map, size_t* storage_ix,
            byte* storage, size_t context_bits) {
            if (self->block_len_ == 0) {
                size_t block_ix = ++self->block_ix_;
                uint block_len = self->block_lengths_[block_ix];
                byte block_type = self->block_types_[block_ix];
                self->block_len_ = block_len;
                self->entropy_ix_ = (size_t) block_type << (int) context_bits;
                StoreBlockSwitch(&self->block_split_code_, block_len, block_type, false,
                    storage_ix, storage);
            }
            --self->block_len_;
            {
                size_t histo_ix = context_map[self->entropy_ix_ + context];
                size_t ix = histo_ix * self->alphabet_size_ + symbol;
                BrotliWriteBits(self->depths_[ix], self->bits_[ix], storage_ix, storage);
            }
        }

        private static unsafe void CleanupBlockEncoder(ref MemoryManager m, BlockEncoder* self) {
            BrotliFree(ref m, self->depths_);
            BrotliFree(ref m, self->bits_);
        }

        private static unsafe void BrotliStoreMetaBlock(ref MemoryManager m,
            byte* input,
            size_t start_pos,
            size_t length,
            size_t mask,
            byte prev_byte,
            byte prev_byte2,
            bool is_last,
            uint num_direct_distance_codes,
            uint distance_postfix_bits,
            ContextType literal_context_mode,
            Command* commands,
            size_t n_commands,
            MetaBlockSplit* mb,
            size_t* storage_ix,
            byte* storage) {
            size_t pos = start_pos;
            size_t i;
            size_t num_distance_codes =
                BROTLI_NUM_DISTANCE_SHORT_CODES + num_direct_distance_codes +
                (48u << (int) distance_postfix_bits);
            HuffmanTree* tree;
            BlockEncoder literal_enc;
            BlockEncoder command_enc;
            BlockEncoder distance_enc;

            StoreCompressedMetaBlockHeader(is_last, length, storage_ix, storage);

            tree = (HuffmanTree*) BrotliAllocate(ref m, MAX_HUFFMAN_TREE_SIZE * sizeof(HuffmanTree));

            InitBlockEncoder(&literal_enc, 256, mb->literal_split.num_types,
                mb->literal_split.types, mb->literal_split.lengths,
                mb->literal_split.num_blocks);
            InitBlockEncoder(&command_enc, BROTLI_NUM_COMMAND_SYMBOLS,
                mb->command_split.num_types, mb->command_split.types,
                mb->command_split.lengths, mb->command_split.num_blocks);
            InitBlockEncoder(&distance_enc, num_distance_codes,
                mb->distance_split.num_types, mb->distance_split.types,
                mb->distance_split.lengths, mb->distance_split.num_blocks);

            BuildAndStoreBlockSwitchEntropyCodes(&literal_enc, tree, storage_ix, storage);
            BuildAndStoreBlockSwitchEntropyCodes(&command_enc, tree, storage_ix, storage);
            BuildAndStoreBlockSwitchEntropyCodes(
                &distance_enc, tree, storage_ix, storage);

            BrotliWriteBits(2, distance_postfix_bits, storage_ix, storage);
            BrotliWriteBits(4, num_direct_distance_codes >> (int) distance_postfix_bits,
                storage_ix, storage);
            for (i = 0; i < mb->literal_split.num_types; ++i) {
                BrotliWriteBits(2, (ulong) literal_context_mode, storage_ix, storage);
            }

            if (mb->literal_context_map_size == 0) {
                StoreTrivialContextMap(mb->literal_histograms_size,
                    BROTLI_LITERAL_CONTEXT_BITS, tree, storage_ix, storage);
            }
            else {
                EncodeContextMap(ref m,
                    mb->literal_context_map, mb->literal_context_map_size,
                    mb->literal_histograms_size, tree, storage_ix, storage);
            }

            if (mb->distance_context_map_size == 0) {
                StoreTrivialContextMap(mb->distance_histograms_size,
                    BROTLI_DISTANCE_CONTEXT_BITS, tree, storage_ix, storage);
            }
            else {
                EncodeContextMap(ref m,
                    mb->distance_context_map, mb->distance_context_map_size,
                    mb->distance_histograms_size, tree, storage_ix, storage);
            }

            BlockEncoderLiteral.BuildAndStoreEntropyCodes(ref m, &literal_enc, mb->literal_histograms,
                mb->literal_histograms_size, tree, storage_ix, storage);

            BlockEncoderCommand.BuildAndStoreEntropyCodes(ref m, &command_enc, mb->command_histograms,
                mb->command_histograms_size, tree, storage_ix, storage);

            BlockEncoderDistance.BuildAndStoreEntropyCodes(ref m, &distance_enc, mb->distance_histograms,
                mb->distance_histograms_size, tree, storage_ix, storage);

            BrotliFree(ref m, tree);

            for (i = 0; i < n_commands; ++i) {
                Command cmd = commands[i];
                size_t cmd_code = cmd.cmd_prefix_;
                StoreSymbol(&command_enc, cmd_code, storage_ix, storage);
                StoreCommandExtra(&cmd, storage_ix, storage);
                if (mb->literal_context_map_size == 0) {
                    size_t j;
                    for (j = cmd.insert_len_; j != 0; --j) {
                        StoreSymbol(&literal_enc, input[pos & mask], storage_ix, storage);
                        ++pos;
                    }
                }
                else {
                    size_t j;
                    for (j = cmd.insert_len_; j != 0; --j) {
                        size_t context = Context(prev_byte, prev_byte2, literal_context_mode);
                        byte literal = input[pos & mask];
                        StoreSymbolWithContext(&literal_enc, literal, context,
                            mb->literal_context_map, storage_ix, storage,
                            BROTLI_LITERAL_CONTEXT_BITS);
                        prev_byte2 = prev_byte;
                        prev_byte = literal;
                        ++pos;
                    }
                }
                pos += CommandCopyLen(&cmd);
                if (CommandCopyLen(&cmd) != 0) {
                    prev_byte2 = input[(pos - 2) & mask];
                    prev_byte = input[(pos - 1) & mask];
                    if (cmd.cmd_prefix_ >= 128) {
                        size_t dist_code = cmd.dist_prefix_;
                        uint distnumextra = cmd.dist_extra_ >> 24;
                        ulong distextra = cmd.dist_extra_ & 0xffffff;
                        if (mb->distance_context_map_size == 0) {
                            StoreSymbol(&distance_enc, dist_code, storage_ix, storage);
                        }
                        else {
                            size_t context = CommandDistanceContext(&cmd);
                            StoreSymbolWithContext(&distance_enc, dist_code, context,
                                mb->distance_context_map, storage_ix, storage,
                                BROTLI_DISTANCE_CONTEXT_BITS);
                        }
                        BrotliWriteBits(distnumextra, distextra, storage_ix, storage);
                    }
                }
            }
            CleanupBlockEncoder(ref m, &distance_enc);
            CleanupBlockEncoder(ref m, &command_enc);
            CleanupBlockEncoder(ref m, &literal_enc);
            if (is_last) {
                JumpToByteBoundary(storage_ix, storage);
            }
        }

        /* |nibblesbits| represents the 2 bits to encode MNIBBLES (0-3)
           REQUIRES: length > 0
           REQUIRES: length <= (1 << 24) */
        private static unsafe void BrotliEncodeMlen(size_t length, ulong* bits,
            size_t* numbits, ulong* nibblesbits) {
            size_t lg = (length == 1) ? 1 : Log2FloorNonZero((uint) (length - 1)) + 1;
            size_t mnibbles = (lg < 16 ? 16 : (lg + 3)) / 4;
            *nibblesbits = mnibbles - 4;
            *numbits = mnibbles * 4;
            *bits = length - 1;
        }

        private static unsafe void JumpToByteBoundary(size_t* storage_ix, byte* storage) {
            *storage_ix = (*storage_ix + 7u) & ~7u;
            storage[*storage_ix >> 3] = 0;
        }

        /* Stores the compressed meta-block header.
           REQUIRES: length > 0
           REQUIRES: length <= (1 << 24) */
        private static unsafe void StoreCompressedMetaBlockHeader(bool is_final_block,
            size_t length,
            size_t* storage_ix,
            byte* storage) {
            ulong lenbits;
            size_t nlenbits;
            ulong nibblesbits;

            /* Write ISLAST bit. */
            BrotliWriteBits(1, is_final_block ? 1U : 0U, storage_ix, storage);
            /* Write ISEMPTY bit. */
            if (is_final_block) {
                BrotliWriteBits(1, 0, storage_ix, storage);
            }

            BrotliEncodeMlen(length, &lenbits, &nlenbits, &nibblesbits);
            BrotliWriteBits(2, nibblesbits, storage_ix, storage);
            BrotliWriteBits(nlenbits, lenbits, storage_ix, storage);

            if (!is_final_block) {
                /* Write ISUNCOMPRESSED bit. */
                BrotliWriteBits(1, 0, storage_ix, storage);
            }
        }

        private static unsafe void StoreCommandExtra(
            Command* cmd, size_t* storage_ix, byte* storage) {
            uint copylen_code = CommandCopyLenCode(cmd);
            ushort inscode = GetInsertLengthCode(cmd->insert_len_);
            ushort copycode = GetCopyLengthCode(copylen_code);
            uint insnumextra = GetInsertExtra(inscode);
            ulong insextraval = cmd->insert_len_ - GetInsertBase(inscode);
            ulong copyextraval = copylen_code - GetCopyBase(copycode);
            ulong bits = (copyextraval << (int) insnumextra) | insextraval;
            BrotliWriteBits(
                insnumextra + GetCopyExtra(copycode), bits, storage_ix, storage);
        }

        private static unsafe void StoreDataWithHuffmanCodes(byte* input,
            size_t start_pos,
            size_t mask,
            Command* commands,
            size_t n_commands,
            byte* lit_depth,
            ushort* lit_bits,
            byte* cmd_depth,
            ushort* cmd_bits,
            byte* dist_depth,
            ushort* dist_bits,
            size_t* storage_ix,
            byte* storage) {
            size_t pos = start_pos;
            size_t i;
            for (i = 0; i < n_commands; ++i) {
                Command cmd = commands[i];
                size_t cmd_code = cmd.cmd_prefix_;
                size_t j;
                BrotliWriteBits(
                    cmd_depth[cmd_code], cmd_bits[cmd_code], storage_ix, storage);
                StoreCommandExtra(&cmd, storage_ix, storage);
                for (j = cmd.insert_len_; j != 0; --j) {
                    byte literal = input[pos & mask];
                    BrotliWriteBits(
                        lit_depth[literal], lit_bits[literal], storage_ix, storage);
                    ++pos;
                }
                pos += CommandCopyLen(&cmd);
                if (CommandCopyLen(&cmd) != 0 && cmd.cmd_prefix_ >= 128) {
                    size_t dist_code = cmd.dist_prefix_;
                    uint distnumextra = cmd.dist_extra_ >> 24;
                    uint distextra = cmd.dist_extra_ & 0xffffff;
                    BrotliWriteBits(dist_depth[dist_code], dist_bits[dist_code],
                        storage_ix, storage);
                    BrotliWriteBits(distnumextra, distextra, storage_ix, storage);
                }
            }
        }

        private static unsafe void BuildHistograms(byte* input,
            size_t start_pos,
            size_t mask,
            Command* commands,
            size_t n_commands,
            HistogramLiteral* lit_histo,
            HistogramCommand* cmd_histo,
            HistogramDistance* dist_histo) {
            size_t pos = start_pos;
            size_t i;
            for (i = 0; i < n_commands; ++i) {
                Command cmd = commands[i];
                size_t j;
                HistogramCommand.HistogramAdd(cmd_histo, cmd.cmd_prefix_);
                for (j = cmd.insert_len_; j != 0; --j) {
                    HistogramLiteral.HistogramAdd(lit_histo, input[pos & mask]);
                    ++pos;
                }
                pos += CommandCopyLen(&cmd);
                if (CommandCopyLen(&cmd) != 0 && cmd.cmd_prefix_ >= 128) {
                    HistogramDistance.HistogramAdd(dist_histo, cmd.dist_prefix_);
                }
            }
        }

        private static unsafe void StoreSimpleHuffmanTree(byte* depths,
            size_t* symbols,
            size_t num_symbols,
            size_t max_bits,
            size_t* storage_ix, byte* storage) {
            /* value of 1 indicates a simple Huffman code */
            BrotliWriteBits(2, 1, storage_ix, storage);
            BrotliWriteBits(2, num_symbols - 1, storage_ix, storage); /* NSYM - 1 */

            {
                /* Sort */
                size_t i;
                for (i = 0; i < num_symbols; i++) {
                    size_t j;
                    for (j = i + 1; j < num_symbols; j++) {
                        if (depths[symbols[j]] < depths[symbols[i]]) {
                            size_t tmp = symbols[j];
                            symbols[j] = symbols[i];
                            symbols[i] = tmp;
                        }
                    }
                }
            }

            if (num_symbols == 2) {
                BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
            }
            else if (num_symbols == 3) {
                BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[2], storage_ix, storage);
            }
            else {
                BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[2], storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[3], storage_ix, storage);
                /* tree-select */
                BrotliWriteBits(1, depths[symbols[0]] == 1 ? 1U : 0U, storage_ix, storage);
            }
        }

        /* Builds a Huffman tree from histogram[0:length] into depth[0:length] and
            bits[0:length] and stores the encoded tree to the bit stream. */
        private static unsafe void BuildAndStoreHuffmanTree(uint* histogram,
            size_t length,
            HuffmanTree* tree,
            byte* depth,
            ushort* bits,
            size_t* storage_ix,
            byte* storage) {
            size_t count = 0;
            size_t* s4 = stackalloc size_t[4];
            memset(s4, 0, 4 * sizeof(size_t));
            size_t i;
            size_t max_bits = 0;
            for (i = 0; i < length; i++) {
                if (histogram[i] != 0) {
                    if (count < 4) {
                        s4[count] = i;
                    }
                    else if (count > 4) {
                        break;
                    }
                    count++;
                }
            }

            {
                size_t max_bits_counter = length - 1;
                while (max_bits_counter != 0) {
                    max_bits_counter >>= 1;
                    ++max_bits;
                }
            }

            if (count <= 1) {
                BrotliWriteBits(4, 1, storage_ix, storage);
                BrotliWriteBits(max_bits, s4[0], storage_ix, storage);
                depth[s4[0]] = 0;
                bits[s4[0]] = 0;
                return;
            }

            memset(depth, 0, length);
            BrotliCreateHuffmanTree(histogram, length, 15, tree, depth);
            BrotliConvertBitDepthsToSymbols(depth, length, bits);

            if (count <= 4) {
                StoreSimpleHuffmanTree(depth, s4, count, max_bits, storage_ix, storage);
            }
            else {
                BrotliStoreHuffmanTree(depth, length, tree, storage_ix, storage);
            }
        }

        private static unsafe void BrotliStoreMetaBlockTrivial(ref MemoryManager m,
            byte* input,
            size_t start_pos,
            size_t length,
            size_t mask,
            bool is_last,
            Command* commands,
            size_t n_commands,
            size_t* storage_ix,
            byte* storage) {
            HistogramLiteral lit_histo;
            HistogramCommand cmd_histo;
            HistogramDistance dist_histo;
            byte* lit_depth = stackalloc byte[BROTLI_NUM_LITERAL_SYMBOLS];
            ushort* lit_bits = stackalloc ushort[BROTLI_NUM_LITERAL_SYMBOLS];
            byte* cmd_depth = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
            ushort* cmd_bits = stackalloc ushort[BROTLI_NUM_COMMAND_SYMBOLS];
            byte* dist_depth = stackalloc byte[SIMPLE_DISTANCE_ALPHABET_SIZE];
            ushort* dist_bits = stackalloc ushort[SIMPLE_DISTANCE_ALPHABET_SIZE];
            HuffmanTree* tree;

            StoreCompressedMetaBlockHeader(is_last, length, storage_ix, storage);

            HistogramLiteral.HistogramClear(&lit_histo);
            HistogramCommand.HistogramClear(&cmd_histo);
            HistogramDistance.HistogramClear(&dist_histo);

            BuildHistograms(input, start_pos, mask, commands, n_commands,
                &lit_histo, &cmd_histo, &dist_histo);

            BrotliWriteBits(13, 0, storage_ix, storage);

            tree = (HuffmanTree*) BrotliAllocate(ref m, MAX_HUFFMAN_TREE_SIZE * sizeof(HuffmanTree));
            BuildAndStoreHuffmanTree(lit_histo.data_, BROTLI_NUM_LITERAL_SYMBOLS, tree,
                lit_depth, lit_bits,
                storage_ix, storage);
            BuildAndStoreHuffmanTree(cmd_histo.data_, BROTLI_NUM_COMMAND_SYMBOLS, tree,
                cmd_depth, cmd_bits,
                storage_ix, storage);
            BuildAndStoreHuffmanTree(dist_histo.data_, SIMPLE_DISTANCE_ALPHABET_SIZE,
                tree,
                dist_depth, dist_bits,
                storage_ix, storage);
            BrotliFree(ref m, tree);
            StoreDataWithHuffmanCodes(input, start_pos, mask, commands,
                n_commands, lit_depth, lit_bits,
                cmd_depth, cmd_bits,
                dist_depth, dist_bits,
                storage_ix, storage);
            if (is_last) {
                JumpToByteBoundary(storage_ix, storage);
            }
        }

        private static unsafe void BrotliStoreMetaBlockFast(ref MemoryManager m,
            byte* input,
            size_t start_pos,
            size_t length,
            size_t mask,
            bool is_last,
            Command* commands,
            size_t n_commands,
            size_t* storage_ix,
            byte* storage) {
            StoreCompressedMetaBlockHeader(is_last, length, storage_ix, storage);

            BrotliWriteBits(13, 0, storage_ix, storage);

            if (n_commands <= 128) {
                uint* histogram = stackalloc uint[BROTLI_NUM_LITERAL_SYMBOLS];
                memset(histogram, 0, BROTLI_NUM_LITERAL_SYMBOLS * sizeof(uint));
                size_t pos = start_pos;
                size_t num_literals = 0;
                size_t i;
                byte* lit_depth = stackalloc byte[BROTLI_NUM_LITERAL_SYMBOLS];
                ushort* lit_bits = stackalloc ushort[BROTLI_NUM_LITERAL_SYMBOLS];
                for (i = 0; i < n_commands; ++i) {
                    Command cmd = commands[i];
                    size_t j;
                    for (j = cmd.insert_len_; j != 0; --j) {
                        ++histogram[input[pos & mask]];
                        ++pos;
                    }
                    num_literals += cmd.insert_len_;
                    pos += CommandCopyLen(&cmd);
                }
                BrotliBuildAndStoreHuffmanTreeFast(ref m, histogram, num_literals,
                    /* max_bits = */ 8,
                    lit_depth, lit_bits,
                    storage_ix, storage);
                StoreStaticCommandHuffmanTree(storage_ix, storage);
                StoreStaticDistanceHuffmanTree(storage_ix, storage);
                fixed (byte* command_code_depth = kStaticCommandCodeDepth)
                fixed (ushort* command_code_bits = kStaticCommandCodeBits)
                fixed (byte* distance_code_depth = kStaticDistanceCodeDepth)
                fixed (ushort* distance_code_bits = kStaticDistanceCodeBits)
                    StoreDataWithHuffmanCodes(input, start_pos, mask, commands,
                        n_commands, lit_depth, lit_bits,
                        command_code_depth,
                        command_code_bits,
                        distance_code_depth,
                        distance_code_bits,
                        storage_ix, storage);
            }
            else {
                HistogramLiteral lit_histo;
                HistogramCommand cmd_histo;
                HistogramDistance dist_histo;
                byte* lit_depth = stackalloc byte[BROTLI_NUM_LITERAL_SYMBOLS];
                ushort* lit_bits = stackalloc ushort[BROTLI_NUM_LITERAL_SYMBOLS];
                byte* cmd_depth = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
                ushort* cmd_bits = stackalloc ushort[BROTLI_NUM_COMMAND_SYMBOLS];
                byte* dist_depth = stackalloc byte[SIMPLE_DISTANCE_ALPHABET_SIZE];
                ushort* dist_bits = stackalloc ushort[SIMPLE_DISTANCE_ALPHABET_SIZE];
                HistogramLiteral.HistogramClear(&lit_histo);
                HistogramCommand.HistogramClear(&cmd_histo);
                HistogramDistance.HistogramClear(&dist_histo);
                BuildHistograms(input, start_pos, mask, commands, n_commands,
                    &lit_histo, &cmd_histo, &dist_histo);
                BrotliBuildAndStoreHuffmanTreeFast(ref m, lit_histo.data_,
                    lit_histo.total_count_,
                    /* max_bits = */ 8,
                    lit_depth, lit_bits,
                    storage_ix, storage);
                BrotliBuildAndStoreHuffmanTreeFast(ref m, cmd_histo.data_,
                    cmd_histo.total_count_,
                    /* max_bits = */ 10,
                    cmd_depth, cmd_bits,
                    storage_ix, storage);
                BrotliBuildAndStoreHuffmanTreeFast(ref m, dist_histo.data_,
                    dist_histo.total_count_,
                    /* max_bits = */
                    SIMPLE_DISTANCE_ALPHABET_BITS,
                    dist_depth, dist_bits,
                    storage_ix, storage);
                StoreDataWithHuffmanCodes(input, start_pos, mask, commands,
                    n_commands, lit_depth, lit_bits,
                    cmd_depth, cmd_bits,
                    dist_depth, dist_bits,
                    storage_ix, storage);
            }

            if (is_last) {
                JumpToByteBoundary(storage_ix, storage);
            }
        }

        /* This is for storing uncompressed blocks (simple raw storage of
            bytes-as-bytes). */
        private static unsafe void BrotliStoreUncompressedMetaBlock(bool is_final_block,
            byte* input,
            size_t position, size_t mask,
            size_t len,
            size_t* storage_ix,
            byte* storage) {
            size_t masked_pos = position & mask;
            BrotliStoreUncompressedMetaBlockHeader(len, storage_ix, storage);
            JumpToByteBoundary(storage_ix, storage);

            if (masked_pos + len > mask + 1) {
                size_t len1 = mask + 1 - masked_pos;
                memcpy(&storage[*storage_ix >> 3], &input[masked_pos], len1);
                *storage_ix += len1 << 3;
                len -= len1;
                masked_pos = 0;
            }
            memcpy(&storage[*storage_ix >> 3], &input[masked_pos], len);
            *storage_ix += len << 3;

            /* We need to clear the next 4 bytes to continue to be
               compatible with BrotliWriteBits. */
            BrotliWriteBitsPrepareStorage(*storage_ix, storage);

            /* Since the uncompressed block itself may not be the final block, add an
               empty one after this. */
            if (is_final_block) {
                BrotliWriteBits(1, 1, storage_ix, storage); /* islast */
                BrotliWriteBits(1, 1, storage_ix, storage); /* isempty */
                JumpToByteBoundary(storage_ix, storage);
            }
        }

        /* Stores the uncompressed meta-block header.
           REQUIRES: length > 0
           REQUIRES: length <= (1 << 24) */
        private static unsafe void BrotliStoreUncompressedMetaBlockHeader(size_t length,
            size_t* storage_ix,
            byte* storage) {
            ulong lenbits;
            size_t nlenbits;
            ulong nibblesbits;

            /* Write ISLAST bit.
               Uncompressed block cannot be the last one, so set to 0. */
            BrotliWriteBits(1, 0, storage_ix, storage);
            BrotliEncodeMlen(length, &lenbits, &nlenbits, &nibblesbits);
            BrotliWriteBits(2, nibblesbits, storage_ix, storage);
            BrotliWriteBits(nlenbits, lenbits, storage_ix, storage);
            /* Write ISUNCOMPRESSED bit. */
            BrotliWriteBits(1, 1, storage_ix, storage);
        }

        private static unsafe void BrotliStoreHuffmanTreeToBitMask(
            size_t huffman_tree_size, byte* huffman_tree,
            byte* huffman_tree_extra_bits, byte* code_length_bitdepth,
            ushort* code_length_bitdepth_symbols,
            size_t* storage_ix, byte* storage) {
            size_t i;
            for (i = 0; i < huffman_tree_size; ++i) {
                size_t ix = huffman_tree[i];
                BrotliWriteBits(code_length_bitdepth[ix], code_length_bitdepth_symbols[ix],
                    storage_ix, storage);
                /* Extra bits */
                switch ((int) ix) {
                    case BROTLI_REPEAT_PREVIOUS_CODE_LENGTH:
                        BrotliWriteBits(2, huffman_tree_extra_bits[i], storage_ix, storage);
                        break;
                    case BROTLI_REPEAT_ZERO_CODE_LENGTH:
                        BrotliWriteBits(3, huffman_tree_extra_bits[i], storage_ix, storage);
                        break;
                }
            }
        }

        private static unsafe void BrotliStoreHuffmanTreeOfHuffmanTreeToBitMask(
            int num_codes, byte* code_length_bitdepth,
            size_t* storage_ix, byte* storage) {
            byte[] kStorageOrder = {
                1, 2, 3, 4, 0, 5, 17, 6, 16, 7, 8, 9, 10, 11, 12, 13, 14, 15
            };
            /* The bit lengths of the Huffman code over the code length alphabet
               are compressed with the following private static unsafe Huffman code:
                 Symbol   Code
                 ------   ----
                 0          00
                 1        1110
                 2         110
                 3          01
                 4          10
                 5        1111 */
            byte[] kHuffmanBitLengthHuffmanCodeSymbols = {
                0, 7, 3, 2, 1, 15
            };
            byte[] kHuffmanBitLengthHuffmanCodeBitLengths = {
                2, 4, 3, 2, 2, 4
            };

            size_t skip_some = 0; /* skips none. */

            /* Throw away trailing zeros: */
            size_t codes_to_store = BROTLI_CODE_LENGTH_CODES;
            if (num_codes > 1) {
                for (; codes_to_store > 0; --codes_to_store) {
                    if (code_length_bitdepth[kStorageOrder[codes_to_store - 1]] != 0) {
                        break;
                    }
                }
            }
            if (code_length_bitdepth[kStorageOrder[0]] == 0 &&
                code_length_bitdepth[kStorageOrder[1]] == 0) {
                skip_some = 2; /* skips two. */
                if (code_length_bitdepth[kStorageOrder[2]] == 0) {
                    skip_some = 3; /* skips three. */
                }
            }
            BrotliWriteBits(2, skip_some, storage_ix, storage);
            {
                size_t i;
                for (i = skip_some; i < codes_to_store; ++i) {
                    size_t l = code_length_bitdepth[kStorageOrder[i]];
                    BrotliWriteBits(kHuffmanBitLengthHuffmanCodeBitLengths[l],
                        kHuffmanBitLengthHuffmanCodeSymbols[l], storage_ix, storage);
                }
            }
        }

        /* num = alphabet size
           depths = symbol depths */
        private static unsafe void BrotliStoreHuffmanTree(byte* depths, size_t num,
            HuffmanTree* tree,
            size_t* storage_ix, byte* storage) {
            /* Write the Huffman tree into the brotli-representation.
               The command alphabet is the largest, so this allocation will fit all
               alphabets. */
            byte* huffman_tree = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
            byte* huffman_tree_extra_bits = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
            size_t huffman_tree_size = 0;
            byte* code_length_bitdepth = stackalloc byte[BROTLI_CODE_LENGTH_CODES];
            memset(code_length_bitdepth, 0, BROTLI_CODE_LENGTH_CODES);
            ushort* code_length_bitdepth_symbols = stackalloc ushort[BROTLI_CODE_LENGTH_CODES];
            uint* huffman_tree_histogram = stackalloc uint[BROTLI_CODE_LENGTH_CODES];
            memset(huffman_tree_histogram, 0, BROTLI_CODE_LENGTH_CODES * sizeof(uint));
            size_t i;
            int num_codes = 0;
            size_t code = 0;

            BrotliWriteHuffmanTree(depths, num, &huffman_tree_size, huffman_tree,
                huffman_tree_extra_bits);

            /* Calculate the statistics of the Huffman tree in brotli-representation. */
            for (i = 0; i < huffman_tree_size; ++i) {
                ++huffman_tree_histogram[huffman_tree[i]];
            }

            for (i = 0; i < BROTLI_CODE_LENGTH_CODES; ++i) {
                if (huffman_tree_histogram[i] != 0) {
                    if (num_codes == 0) {
                        code = i;
                        num_codes = 1;
                    }
                    else if (num_codes == 1) {
                        num_codes = 2;
                        break;
                    }
                }
            }

            /* Calculate another Huffman tree to use for compressing both the
               earlier Huffman tree with. */
            BrotliCreateHuffmanTree(huffman_tree_histogram, BROTLI_CODE_LENGTH_CODES,
                5, tree, code_length_bitdepth);
            BrotliConvertBitDepthsToSymbols(code_length_bitdepth,
                BROTLI_CODE_LENGTH_CODES,
                code_length_bitdepth_symbols);

            /* Now, we have all the data, let's start storing it */
            BrotliStoreHuffmanTreeOfHuffmanTreeToBitMask(num_codes, code_length_bitdepth,
                storage_ix, storage);

            if (num_codes == 1) {
                code_length_bitdepth[code] = 0;
            }

            /* Store the real Huffman tree now. */
            BrotliStoreHuffmanTreeToBitMask(huffman_tree_size,
                huffman_tree,
                huffman_tree_extra_bits,
                code_length_bitdepth,
                code_length_bitdepth_symbols,
                storage_ix, storage);
        }

        private static unsafe bool SortHuffmanTreeBitStream(
            HuffmanTree* v0, HuffmanTree* v1) {
            return (v0->total_count_ < v1->total_count_);
        }

        private static unsafe void BrotliBuildAndStoreHuffmanTreeFast(ref MemoryManager m,
            uint* histogram,
            size_t histogram_total,
            size_t max_bits,
            byte* depth, ushort* bits,
            size_t* storage_ix,
            byte* storage) {
            size_t count = 0;
            size_t* symbols = stackalloc size_t[4];
            memset(symbols, 0, 4 * sizeof(size_t));
            size_t length = 0;
            size_t total = histogram_total;
            while (total != 0) {
                if (histogram[length] != 0) {
                    if (count < 4) {
                        symbols[count] = length;
                    }
                    ++count;
                    total -= histogram[length];
                }
                ++length;
            }

            if (count <= 1) {
                BrotliWriteBits(4, 1, storage_ix, storage);
                BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                depth[symbols[0]] = 0;
                bits[symbols[0]] = 0;
                return;
            }

            memset(depth, 0, length);
            {
                size_t max_tree_size = 2 * length + 1;
                HuffmanTree* tree = (HuffmanTree*) BrotliAllocate(ref m, max_tree_size * sizeof(HuffmanTree));
                uint count_limit;

                for (count_limit = 1;; count_limit *= 2) {
                    HuffmanTree* node = tree;
                    size_t l;
                    for (l = length; l != 0;) {
                        --l;
                        if (histogram[l] != 0) {
                            if ((histogram[l] >= count_limit)) {
                                InitHuffmanTree(node, histogram[l], -1, (short) l);
                            }
                            else {
                                InitHuffmanTree(node, count_limit, -1, (short) l);
                            }
                            ++node;
                        }
                    }
                    {
                        int n = (int) (node - tree);
                        HuffmanTree sentinel;
                        int i = 0; /* Points to the next leaf node. */
                        int j = n + 1; /* Points to the next non-leaf node. */
                        int k;

                        SortHuffmanTreeItems(tree, (size_t) n, SortHuffmanTreeBitStream);
                        /* The nodes are:
                           [0, n): the sorted leaf nodes that we start with.
                           [n]: we add a sentinel here.
                           [n + 1, 2n): new parent nodes are added here, starting from
                                        (n+1). These are naturally in ascending order.
                           [2n]: we add a sentinel at the end as well.
                           There will be (2n+1) elements at the end. */
                        InitHuffmanTree(&sentinel, uint.MaxValue, -1, -1);
                        *node++ = sentinel;
                        *node++ = sentinel;

                        for (k = n - 1; k > 0; --k) {
                            int left, right;
                            if (tree[i].total_count_ <= tree[j].total_count_) {
                                left = i;
                                ++i;
                            }
                            else {
                                left = j;
                                ++j;
                            }
                            if (tree[i].total_count_ <= tree[j].total_count_) {
                                right = i;
                                ++i;
                            }
                            else {
                                right = j;
                                ++j;
                            }
                            /* The sentinel node becomes the parent node. */
                            node[-1].total_count_ =
                                tree[left].total_count_ + tree[right].total_count_;
                            node[-1].index_left_ = (short) left;
                            node[-1].index_right_or_value_ = (short) right;
                            /* Add back the last sentinel node. */
                            *node++ = sentinel;
                        }
                        if (BrotliSetDepth(2 * n - 1, tree, depth, 14)) {
                            /* We need to pack the Huffman tree in 14 bits. If this was not
                               successful, add fake entities to the lowest values and retry. */
                            break;
                        }
                    }
                }
                BrotliFree(ref m, tree);
            }

            BrotliConvertBitDepthsToSymbols(depth, length, bits);
            if (count <= 4) {
                size_t i;
                /* value of 1 indicates a simple Huffman code */
                BrotliWriteBits(2, 1, storage_ix, storage);
                BrotliWriteBits(2, count - 1, storage_ix, storage); /* NSYM - 1 */

                /* Sort */
                for (i = 0; i < count; i++) {
                    size_t j;
                    for (j = i + 1; j < count; j++) {
                        if (depth[symbols[j]] < depth[symbols[i]]) {
                            size_t tmp = symbols[j];
                            symbols[j] = symbols[i];
                            symbols[i] = tmp;
                        }
                    }
                }

                if (count == 2) {
                    BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
                }
                else if (count == 3) {
                    BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[2], storage_ix, storage);
                }
                else {
                    BrotliWriteBits(max_bits, symbols[0], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[1], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[2], storage_ix, storage);
                    BrotliWriteBits(max_bits, symbols[3], storage_ix, storage);
                    /* tree-select */
                    BrotliWriteBits(1, depth[symbols[0]] == 1 ? 1U : 0U, storage_ix, storage);
                }
            }
            else {
                byte previous_value = 8;
                size_t i;
                /* Complex Huffman Tree */
                StoreStaticCodeLengthCode(storage_ix, storage);

                /* Actual RLE coding. */
                for (i = 0; i < length;) {
                    byte value = depth[i];
                    size_t reps = 1;
                    size_t k;
                    for (k = i + 1; k < length && depth[k] == value; ++k) {
                        ++reps;
                    }
                    i += reps;
                    if (value == 0) {
                        BrotliWriteBits(kZeroRepsDepth[reps], kZeroRepsBits[reps],
                            storage_ix, storage);
                    }
                    else {
                        if (previous_value != value) {
                            BrotliWriteBits(kCodeLengthDepth[value], kCodeLengthBits[value],
                                storage_ix, storage);
                            --reps;
                        }
                        if (reps < 3) {
                            while (reps != 0) {
                                reps--;
                                BrotliWriteBits(kCodeLengthDepth[value], kCodeLengthBits[value],
                                    storage_ix, storage);
                            }
                        }
                        else {
                            reps -= 3;
                            BrotliWriteBits(kNonZeroRepsDepth[reps], kNonZeroRepsBits[reps],
                                storage_ix, storage);
                        }
                        previous_value = value;
                    }
                }
            }
        }
    }
}