using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct MetaBlockSplit {
            public BlockSplit literal_split;
            public BlockSplit command_split;
            public BlockSplit distance_split;
            public uint* literal_context_map;
            public size_t literal_context_map_size;
            public uint* distance_context_map;
            public size_t distance_context_map_size;
            public HistogramLiteral* literal_histograms;
            public size_t literal_histograms_size;
            public HistogramCommand* command_histograms;
            public size_t command_histograms_size;
            public HistogramDistance* distance_histograms;
            public size_t distance_histograms_size;
        }

        /* Greedy block splitter for one block category (literal, command or distance).
           Gathers histograms for all context buckets. */
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct ContextBlockSplitter {
            /* Alphabet size of particular block category. */
            public size_t alphabet_size_;

            public size_t num_contexts_;

            public size_t max_block_types_;

            /* We collect at least this many symbols for each block. */
            public size_t min_block_size_;

            /* We merge histograms A and B if
                 entropy(A+B) < entropy(A) + entropy(B) + split_threshold_,
               where A is the current histogram and B is the histogram of the last or the
               second last block type. */
            public double split_threshold_;

            public size_t num_blocks_;
            public BlockSplit* split_; /* not owned */
            public HistogramLiteral* histograms_; /* not owned */
            public size_t* histograms_size_; /* not owned */

            /* The number of symbols that we want to collect before deciding on whether
               or not to merge the block with a previous one or emit a new block. */
            public size_t target_block_size_;

            /* The number of symbols in the current histogram. */
            public size_t block_size_;

            /* Offset of the current histogram. */
            public size_t curr_histogram_ix_;

            /* Offset of the histograms of the previous two block types. */
            public size_t last_histogram_ix_0;

            public size_t last_histogram_ix_1;

            /* Entropy of the previous two block types. */
            public fixed double last_entropy_[2 * BROTLI_MAX_STATIC_CONTEXTS];

            /* The number of times we merged the current block with the last one. */
            public size_t merge_last_count_;
        }

        private static unsafe void BrotliBuildMetaBlock(ref MemoryManager m,
            byte* ringbuffer,
            size_t pos,
            size_t mask,
            BrotliEncoderParams* params_,
            byte prev_byte,
            byte prev_byte2,
            Command* cmds,
            size_t num_commands,
            ContextType literal_context_mode,
            MetaBlockSplit* mb) {
            /* Histogram ids need to fit in one byte. */
            size_t kMaxNumberOfHistograms = 256;
            HistogramDistance* distance_histograms;
            HistogramLiteral* literal_histograms;
            ContextType* literal_context_modes = null;
            size_t literal_histograms_size;
            size_t distance_histograms_size;
            size_t i;
            size_t literal_context_multiplier = 1;

            BrotliSplitBlock(ref m, cmds, num_commands,
                ringbuffer, pos, mask, params_,
                &mb->literal_split,
                &mb->command_split,
                &mb->distance_split);

            if (!params_->disable_literal_context_modeling) {
                literal_context_multiplier = 1 << BROTLI_LITERAL_CONTEXT_BITS;
                literal_context_modes =
                    (ContextType*) BrotliAllocate(ref m, mb->literal_split.num_types * sizeof(ContextType));
                for (i = 0; i < mb->literal_split.num_types; ++i) {
                    literal_context_modes[i] = literal_context_mode;
                }
            }

            literal_histograms_size =
                mb->literal_split.num_types * literal_context_multiplier;
            literal_histograms =
                (HistogramLiteral*) BrotliAllocate(ref m, literal_histograms_size * sizeof(HistogramLiteral));
            HistogramLiteral.ClearHistograms(literal_histograms, literal_histograms_size);

            distance_histograms_size =
                mb->distance_split.num_types << BROTLI_DISTANCE_CONTEXT_BITS;
            distance_histograms =
                (HistogramDistance*) BrotliAllocate(ref m, distance_histograms_size * sizeof(HistogramDistance));
            HistogramDistance.ClearHistograms(distance_histograms, distance_histograms_size);

            mb->command_histograms_size = mb->command_split.num_types;
            mb->command_histograms =
                (HistogramCommand*) BrotliAllocate(ref m, mb->command_histograms_size * sizeof(HistogramCommand));
            HistogramCommand.ClearHistograms(mb->command_histograms, mb->command_histograms_size);

            BrotliBuildHistogramsWithContext(cmds, num_commands,
                &mb->literal_split, &mb->command_split, &mb->distance_split,
                ringbuffer, pos, mask, prev_byte, prev_byte2, literal_context_modes,
                literal_histograms, mb->command_histograms, distance_histograms);
            BrotliFree(ref m, literal_context_modes);

            mb->literal_context_map_size =
                mb->literal_split.num_types << BROTLI_LITERAL_CONTEXT_BITS;
            mb->literal_context_map =
                (uint*) BrotliAllocate(ref m, mb->literal_context_map_size * sizeof(uint));

            mb->literal_histograms_size = mb->literal_context_map_size;
            mb->literal_histograms =
                (HistogramLiteral*) BrotliAllocate(ref m, mb->literal_histograms_size * sizeof(HistogramLiteral));

            ClusterLiteral.BrotliClusterHistograms(ref m, literal_histograms, literal_histograms_size,
                kMaxNumberOfHistograms, mb->literal_histograms,
                &mb->literal_histograms_size, mb->literal_context_map);
            BrotliFree(ref m, literal_histograms);

            if (params_->disable_literal_context_modeling) {
                /* Distribute assignment to all contexts. */
                for (i = mb->literal_split.num_types; i != 0;) {
                    size_t j = 0;
                    i--;
                    for (; j < (1 << BROTLI_LITERAL_CONTEXT_BITS); j++) {
                        mb->literal_context_map[(i << BROTLI_LITERAL_CONTEXT_BITS) + j] =
                            mb->literal_context_map[i];
                    }
                }
            }

            mb->distance_context_map_size =
                mb->distance_split.num_types << BROTLI_DISTANCE_CONTEXT_BITS;
            mb->distance_context_map =
                (uint*) BrotliAllocate(ref m, mb->distance_context_map_size * sizeof(uint));

            mb->distance_histograms_size = mb->distance_context_map_size;
            mb->distance_histograms =
                (HistogramDistance*) BrotliAllocate(ref m, mb->distance_histograms_size * sizeof(HistogramDistance));

            ClusterDistance.BrotliClusterHistograms(ref m, distance_histograms,
                mb->distance_context_map_size,
                kMaxNumberOfHistograms,
                mb->distance_histograms,
                &mb->distance_histograms_size,
                mb->distance_context_map);
            BrotliFree(ref m, distance_histograms);
        }

        private static unsafe void InitMetaBlockSplit(MetaBlockSplit* mb) {
            BrotliInitBlockSplit(&mb->literal_split);
            BrotliInitBlockSplit(&mb->command_split);
            BrotliInitBlockSplit(&mb->distance_split);
            mb->literal_context_map = null;
            mb->literal_context_map_size = 0;
            mb->distance_context_map = null;
            mb->distance_context_map_size = 0;
            mb->literal_histograms = null;
            mb->literal_histograms_size = 0;
            mb->command_histograms = null;
            mb->command_histograms_size = 0;
            mb->distance_histograms = null;
            mb->distance_histograms_size = 0;
        }

        private static unsafe void InitContextBlockSplitter(
            ref MemoryManager m, ContextBlockSplitter* self, size_t alphabet_size,
            size_t num_contexts, size_t min_block_size, double split_threshold,
            size_t num_symbols, BlockSplit* split, HistogramLiteral** histograms,
            size_t* histograms_size) {
            size_t max_num_blocks = num_symbols / min_block_size + 1;
            size_t max_num_types;

            self->alphabet_size_ = alphabet_size;
            self->num_contexts_ = num_contexts;
            self->max_block_types_ = BROTLI_MAX_NUMBER_OF_BLOCK_TYPES / num_contexts;
            self->min_block_size_ = min_block_size;
            self->split_threshold_ = split_threshold;
            self->num_blocks_ = 0;
            self->split_ = split;
            self->histograms_size_ = histograms_size;
            self->target_block_size_ = min_block_size;
            self->block_size_ = 0;
            self->curr_histogram_ix_ = 0;
            self->merge_last_count_ = 0;

            /* We have to allocate one more histogram than the maximum number of block
               types for the current histogram when the meta-block is too big. */
            max_num_types =
                Math.Min(max_num_blocks, self->max_block_types_ + 1);
            BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, max_num_blocks);
            BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, max_num_blocks);
            split->num_blocks = max_num_blocks;
            *histograms_size = max_num_types * num_contexts;
            *histograms = (HistogramLiteral*) BrotliAllocate(ref m, *histograms_size * sizeof(HistogramLiteral));
            self->histograms_ = *histograms;
            /* Clear only current histogram. */
            HistogramLiteral.ClearHistograms(&self->histograms_[0], num_contexts);
            self->last_histogram_ix_0 = self->last_histogram_ix_1 = 0;
        }

        /* Does either of three things:
             (1) emits the current block with a new block type;
             (2) emits the current block with the type of the second last block;
             (3) merges the current block with the last block. */
        private static unsafe void ContextBlockSplitterFinishBlock(
            ContextBlockSplitter* self, ref MemoryManager m, bool is_final) {
            BlockSplit* split = self->split_;
            size_t num_contexts = self->num_contexts_;
            double* last_entropy = self->last_entropy_;
            HistogramLiteral* histograms = self->histograms_;

            if (self->block_size_ < self->min_block_size_) {
                self->block_size_ = self->min_block_size_;
            }
            if (self->num_blocks_ == 0) {
                size_t i;
                /* Create first block. */
                split->lengths[0] = (uint) self->block_size_;
                split->types[0] = 0;

                for (i = 0; i < num_contexts; ++i) {
                    last_entropy[i] =
                        BitsEntropy(histograms[i].data_, self->alphabet_size_);
                    last_entropy[num_contexts + i] = last_entropy[i];
                }
                ++self->num_blocks_;
                ++split->num_types;
                self->curr_histogram_ix_ += num_contexts;
                if (self->curr_histogram_ix_ < *self->histograms_size_) {
                    HistogramLiteral.ClearHistograms(
                        &self->histograms_[self->curr_histogram_ix_], self->num_contexts_);
                }
                self->block_size_ = 0;
            }
            else if (self->block_size_ > 0) {
                /* Try merging the set of histograms for the current block type with the
                   respective set of histograms for the last and second last block types.
                   Decide over the split based on the total reduction of entropy across
                   all contexts. */
                double* entropy = stackalloc double[BROTLI_MAX_STATIC_CONTEXTS];
                HistogramLiteral* combined_histo =
                    (HistogramLiteral*) BrotliAllocate(ref m, 2 * num_contexts * sizeof(HistogramLiteral));
                double[] combined_entropy = new double[2 * BROTLI_MAX_STATIC_CONTEXTS];
                double[] diff = {0.0, 0.0};
                size_t i;
                for (i = 0; i < num_contexts; ++i) {
                    size_t curr_histo_ix = self->curr_histogram_ix_ + i;
                    size_t j;
                    entropy[i] = BitsEntropy(histograms[curr_histo_ix].data_,
                        self->alphabet_size_);
                    for (j = 0; j < 2; ++j) {
                        size_t jx = j * num_contexts + i;
                        size_t last_histogram_ix = (j == 0 ? self->last_histogram_ix_0 : self->last_histogram_ix_1) + i;
                        combined_histo[jx] = histograms[curr_histo_ix];
                        HistogramLiteral.HistogramAddHistogram(&combined_histo[jx],
                            &histograms[last_histogram_ix]);
                        combined_entropy[jx] = BitsEntropy(
                            &combined_histo[jx].data_[0], self->alphabet_size_);
                        diff[j] += combined_entropy[jx] - entropy[i] - last_entropy[jx];
                    }
                }

                if (split->num_types < self->max_block_types_ &&
                    diff[0] > self->split_threshold_ &&
                    diff[1] > self->split_threshold_) {
                    /* Create new block. */
                    split->lengths[self->num_blocks_] = (uint) self->block_size_;
                    split->types[self->num_blocks_] = (byte) split->num_types;
                    self->last_histogram_ix_1 = self->last_histogram_ix_0;
                    self->last_histogram_ix_0 = split->num_types * num_contexts;
                    for (i = 0; i < num_contexts; ++i) {
                        last_entropy[num_contexts + i] = last_entropy[i];
                        last_entropy[i] = entropy[i];
                    }
                    ++self->num_blocks_;
                    ++split->num_types;
                    self->curr_histogram_ix_ += num_contexts;
                    if (self->curr_histogram_ix_ < *self->histograms_size_) {
                        HistogramLiteral.ClearHistograms(
                            &self->histograms_[self->curr_histogram_ix_], self->num_contexts_);
                    }
                    self->block_size_ = 0;
                    self->merge_last_count_ = 0;
                    self->target_block_size_ = self->min_block_size_;
                }
                else if (diff[1] < diff[0] - 20.0) {
                    /* Combine this block with second last block. */
                    split->lengths[self->num_blocks_] = (uint) self->block_size_;
                    split->types[self->num_blocks_] = split->types[self->num_blocks_ - 2];
                    size_t tmp = self->last_histogram_ix_0;
                    self->last_histogram_ix_0 = self->last_histogram_ix_1;
                    self->last_histogram_ix_1 = tmp;
                    for (i = 0; i < num_contexts; ++i) {
                        histograms[self->last_histogram_ix_0 + i] =
                            combined_histo[num_contexts + i];
                        last_entropy[num_contexts + i] = last_entropy[i];
                        last_entropy[i] = combined_entropy[num_contexts + i];
                        HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_ + i]);
                    }
                    ++self->num_blocks_;
                    self->block_size_ = 0;
                    self->merge_last_count_ = 0;
                    self->target_block_size_ = self->min_block_size_;
                }
                else {
                    /* Combine this block with last block. */
                    split->lengths[self->num_blocks_ - 1] += (uint) self->block_size_;
                    for (i = 0; i < num_contexts; ++i) {
                        histograms[self->last_histogram_ix_0 + i] = combined_histo[i];
                        last_entropy[i] = combined_entropy[i];
                        if (split->num_types == 1) {
                            last_entropy[num_contexts + i] = last_entropy[i];
                        }
                        HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_ + i]);
                    }
                    self->block_size_ = 0;
                    if (++self->merge_last_count_ > 1) {
                        self->target_block_size_ += self->min_block_size_;
                    }
                }
                BrotliFree(ref m, combined_histo);
            }
            if (is_final) {
                *self->histograms_size_ = split->num_types * num_contexts;
                split->num_blocks = self->num_blocks_;
            }
        }

        /* Adds the next symbol to the current block type and context. When the
           current block reaches the target size, decides on merging the block. */
        private static unsafe void ContextBlockSplitterAddSymbol(
            ContextBlockSplitter* self, ref MemoryManager m,
            size_t symbol, size_t context) {
            HistogramLiteral.HistogramAdd(&self->histograms_[self->curr_histogram_ix_ + context],
                symbol);
            ++self->block_size_;
            if (self->block_size_ == self->target_block_size_) {
                ContextBlockSplitterFinishBlock(self, ref m, /* is_final = */ false);
            }
        }

        private static unsafe void MapStaticContexts(ref MemoryManager m,
            size_t num_contexts,
            uint* static_context_map,
            MetaBlockSplit* mb) {
            size_t i;
            mb->literal_context_map_size =
                mb->literal_split.num_types << BROTLI_LITERAL_CONTEXT_BITS;
            mb->literal_context_map =
                (uint*) BrotliAllocate(ref m, mb->literal_context_map_size * sizeof(uint));

            for (i = 0; i < mb->literal_split.num_types; ++i) {
                uint offset = (uint) (i * num_contexts);
                size_t j;
                for (j = 0; j < (1u << BROTLI_LITERAL_CONTEXT_BITS); ++j) {
                    mb->literal_context_map[(i << BROTLI_LITERAL_CONTEXT_BITS) + j] =
                        offset + static_context_map[j];
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct lit_blocks_union {
            public ContextBlockSplitter ctx;
        }

        private static unsafe void BrotliBuildMetaBlockGreedyInternal(
            ref MemoryManager m, byte* ringbuffer, size_t pos, size_t mask,
            byte prev_byte, byte prev_byte2, ContextType literal_context_mode,
            size_t num_contexts, uint* static_context_map,
            Command* commands, size_t n_commands, MetaBlockSplit* mb) {
            lit_blocks_union lit_blocks = new lit_blocks_union();
            BlockSplitterCommand cmd_blocks;
            BlockSplitterDistance dist_blocks;
            size_t num_literals = 0;
            size_t i;
            for (i = 0; i < n_commands; ++i) {
                num_literals += commands[i].insert_len_;
            }

            if (num_contexts == 1) {
                BlockSplitterLiteral.InitBlockSplitter(ref m, (BlockSplitterLiteral*) &lit_blocks, 256, 512, 400.0,
                    num_literals, &mb->literal_split, &mb->literal_histograms,
                    &mb->literal_histograms_size);
            }
            else {
                InitContextBlockSplitter(ref m, &lit_blocks.ctx, 256, num_contexts, 512, 400.0,
                    num_literals, &mb->literal_split, &mb->literal_histograms,
                    &mb->literal_histograms_size);
            }
            BlockSplitterCommand.InitBlockSplitter(ref m, &cmd_blocks, BROTLI_NUM_COMMAND_SYMBOLS, 1024,
                500.0, n_commands, &mb->command_split, &mb->command_histograms,
                &mb->command_histograms_size);
            BlockSplitterDistance.InitBlockSplitter(ref m, &dist_blocks, 64, 512, 100.0, n_commands,
                &mb->distance_split, &mb->distance_histograms,
                &mb->distance_histograms_size);

            for (i = 0; i < n_commands; ++i) {
                Command cmd = commands[i];
                size_t j;
                BlockSplitterCommand.BlockSplitterAddSymbol(&cmd_blocks, cmd.cmd_prefix_);
                for (j = cmd.insert_len_; j != 0; --j) {
                    byte literal = ringbuffer[pos & mask];
                    if (num_contexts == 1) {
                        BlockSplitterLiteral.BlockSplitterAddSymbol((BlockSplitterLiteral*) &lit_blocks, literal);
                    }
                    else {
                        size_t context = Context(prev_byte, prev_byte2, literal_context_mode);
                        ContextBlockSplitterAddSymbol(&lit_blocks.ctx, ref m, literal,
                            static_context_map[context]);
                    }
                    prev_byte2 = prev_byte;
                    prev_byte = literal;
                    ++pos;
                }
                pos += CommandCopyLen(&cmd);
                if (CommandCopyLen(&cmd) != 0) {
                    prev_byte2 = ringbuffer[(pos - 2) & mask];
                    prev_byte = ringbuffer[(pos - 1) & mask];
                    if (cmd.cmd_prefix_ >= 128) {
                        BlockSplitterDistance.BlockSplitterAddSymbol(&dist_blocks, cmd.dist_prefix_);
                    }
                }
            }

            if (num_contexts == 1) {
                BlockSplitterLiteral.BlockSplitterFinishBlock(
                    (BlockSplitterLiteral*) &lit_blocks, /* is_final = */ true);
            }
            else {
                ContextBlockSplitterFinishBlock(
                    &lit_blocks.ctx, ref m, /* is_final = */ true);
            }
            BlockSplitterCommand.BlockSplitterFinishBlock(&cmd_blocks, /* is_final = */ true);
            BlockSplitterDistance.BlockSplitterFinishBlock(&dist_blocks, /* is_final = */ true);

            if (num_contexts > 1) {
                MapStaticContexts(ref m, num_contexts, static_context_map, mb);
            }
        }

        private static unsafe void BrotliBuildMetaBlockGreedy(ref MemoryManager m,
            byte* ringbuffer,
            size_t pos,
            size_t mask,
            byte prev_byte,
            byte prev_byte2,
            ContextType literal_context_mode,
            size_t num_contexts,
            uint* static_context_map,
            Command* commands,
            size_t n_commands,
            MetaBlockSplit* mb) {
            if (num_contexts == 1) {
                BrotliBuildMetaBlockGreedyInternal(ref m, ringbuffer, pos, mask, prev_byte,
                    prev_byte2, literal_context_mode, 1, null, commands, n_commands, mb);
            }
            else {
                BrotliBuildMetaBlockGreedyInternal(ref m, ringbuffer, pos, mask, prev_byte,
                    prev_byte2, literal_context_mode, num_contexts, static_context_map,
                    commands, n_commands, mb);
            }
        }

        private static unsafe void DestroyMetaBlockSplit(
            ref MemoryManager m, MetaBlockSplit* mb)
        {
            BrotliDestroyBlockSplit(ref m, &mb->literal_split);
            BrotliDestroyBlockSplit(ref m, &mb->command_split);
            BrotliDestroyBlockSplit(ref m, &mb->distance_split);
            BrotliFree(ref m, mb->literal_context_map);
            BrotliFree(ref m, mb->distance_context_map);
            BrotliFree(ref m, mb->literal_histograms);
            BrotliFree(ref m, mb->command_histograms);
            BrotliFree(ref m, mb->distance_histograms);
        }

        private static unsafe void BrotliOptimizeHistograms(size_t num_direct_distance_codes,
            size_t distance_postfix_bits,
            MetaBlockSplit* mb)
        {
            byte* good_for_rle = stackalloc byte[BROTLI_NUM_COMMAND_SYMBOLS];
            size_t num_distance_codes;
            size_t i;
            for (i = 0; i < mb->literal_histograms_size; ++i)
            {
                BrotliOptimizeHuffmanCountsForRle(256, mb->literal_histograms[i].data_,
                    good_for_rle);
            }
            for (i = 0; i < mb->command_histograms_size; ++i)
            {
                BrotliOptimizeHuffmanCountsForRle(BROTLI_NUM_COMMAND_SYMBOLS,
                    mb->command_histograms[i].data_,
                    good_for_rle);
            }
            num_distance_codes = BROTLI_NUM_DISTANCE_SHORT_CODES +
                                 num_direct_distance_codes +
                                 ((2 * BROTLI_MAX_DISTANCE_BITS) << (int) distance_postfix_bits);
            for (i = 0; i < mb->distance_histograms_size; ++i)
            {
                BrotliOptimizeHuffmanCountsForRle(num_distance_codes,
                    mb->distance_histograms[i].data_,
                    good_for_rle);
            }
        }
    }
}