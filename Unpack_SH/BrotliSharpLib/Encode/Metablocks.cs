using System;
using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe partial struct BlockSplitterLiteral
        {
            /* Alphabet size of particular block category. */
            public size_t alphabet_size_;
            /* We collect at least this many symbols for each block. */
            public size_t min_block_size_;
            /* We merge histograms A and B if
                 entropy(A+B) < entropy(A) + entropy(B) + split_threshold_,
               where A is the current histogram and B is the histogram of the last or the
               second last block type. */
            public double split_threshold_;

            public size_t num_blocks_;
            public BlockSplit* split_;  /* not owned */
            public HistogramLiteral* histograms_;  /* not owned */
            public size_t* histograms_size_;  /* not owned */

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
            public fixed double last_entropy_[2];
            /* The number of times we merged the current block with the last one. */
            public size_t merge_last_count_;

            public static unsafe void InitBlockSplitter(
                ref MemoryManager m, BlockSplitterLiteral* self, size_t alphabet_size,
                size_t min_block_size, double split_threshold, size_t num_symbols,
                BlockSplit * split, HistogramLiteral* *histograms, size_t * histograms_size) {
                size_t max_num_blocks = num_symbols / min_block_size + 1;
                /* We have to allocate one more histogram than the maximum number of block
                   types for the current histogram when the meta-block is too big. */
                size_t max_num_types =
                    Math.Min(max_num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES + 1);
                self->alphabet_size_ = alphabet_size;
                self->min_block_size_ = min_block_size;
                self->split_threshold_ = split_threshold;
                self->num_blocks_ = 0;
                self->split_ = split;
                self->histograms_size_ = histograms_size;
                self->target_block_size_ = min_block_size;
                self->block_size_ = 0;
                self->curr_histogram_ix_ = 0;
                self->merge_last_count_ = 0;
                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, max_num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, max_num_blocks);
                self->split_->num_blocks = max_num_blocks;
                *histograms_size = max_num_types;
                *histograms = (HistogramLiteral*) BrotliAllocate(ref m, * histograms_size * sizeof(HistogramLiteral));
                self->histograms_ = *histograms;
                /* Clear only current histogram. */
                HistogramLiteral.HistogramClear(&self->histograms_[0]);
                self->last_histogram_ix_0 = self->last_histogram_ix_1 = 0;
            }

            /* Does either of three things:
             (1) emits the current block with a new block type;
             (2) emits the current block with the type of the second last block;
             (3) merges the current block with the last block. */
            public static unsafe void BlockSplitterFinishBlock(
            BlockSplitterLiteral* self, bool is_final) {
                BlockSplit* split = self->split_;
                double* last_entropy = self->last_entropy_;
                HistogramLiteral* histograms = self->histograms_;
                self->block_size_ =
                    Math.Max(self->block_size_, self->min_block_size_);
                if (self->num_blocks_ == 0) {
                    /* Create first block. */
                    split->lengths[0] = (uint)self->block_size_;
                    split->types[0] = 0;
                    last_entropy[0] =
                        BitsEntropy(histograms[0].data_, self->alphabet_size_);
                    last_entropy[1] = last_entropy[0];
                    ++self->num_blocks_;
                    ++split->num_types;
                    ++self->curr_histogram_ix_;
                    if (self->curr_histogram_ix_< *self->histograms_size_)
                        HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_]);
                    self->block_size_ = 0;
                } else if (self->block_size_ > 0) {
                    double entropy = BitsEntropy(histograms[self->curr_histogram_ix_].data_,
                        self->alphabet_size_);
                    HistogramLiteral* combined_histo = stackalloc HistogramLiteral[2];
                    double* combined_entropy = stackalloc double[2];
                    double* diff = stackalloc double[2];
                    size_t j;
                    for (j = 0; j< 2; ++j) {
                        size_t last_histogram_ix = j == 0 ? self->last_histogram_ix_0 : self->last_histogram_ix_1;
                        combined_histo[j] = histograms[self->curr_histogram_ix_];
                        HistogramLiteral.HistogramAddHistogram(&combined_histo[j],
                            &histograms[last_histogram_ix]);
                        combined_entropy[j] = BitsEntropy(
                            &combined_histo[j].data_[0], self->alphabet_size_);
                        diff[j] = combined_entropy[j] - entropy - last_entropy[j];
                    }

                    if (split->num_types<BROTLI_MAX_NUMBER_OF_BLOCK_TYPES &&
                        diff[0]> self->split_threshold_ &&
                        diff[1] > self->split_threshold_) {
                        /* Create new block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = (byte)split->num_types;
                        self->last_histogram_ix_1 = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = (byte)split->num_types;
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = entropy;
                        ++self->num_blocks_;
                        ++split->num_types;
                        ++self->curr_histogram_ix_;
                        if (self->curr_histogram_ix_< *self->histograms_size_)
                           HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->block_size_ = 0;
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    } else if (diff[1] < diff[0] - 20.0) {
                        /* Combine this block with second last block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = split->types[self->num_blocks_ - 2];
                        size_t tmp = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = self->last_histogram_ix_1;
                        self->last_histogram_ix_1 = tmp;
                        histograms[self->last_histogram_ix_0] = combined_histo[1];
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = combined_entropy[1];
                        ++self->num_blocks_;
                        self->block_size_ = 0;
                        HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    } else {
                        /* Combine this block with last block. */
                        split->lengths[self->num_blocks_ - 1] += (uint)self->block_size_;
                        histograms[self->last_histogram_ix_0] = combined_histo[0];
                        last_entropy[0] = combined_entropy[0];
                        if (split->num_types == 1) {
                            last_entropy[1] = last_entropy[0];
                        }
                        self->block_size_ = 0;
                        HistogramLiteral.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        if (++self->merge_last_count_ > 1) {
                            self->target_block_size_ += self->min_block_size_;
                        }
                    }
                }
                if (is_final) {
                    * self->histograms_size_ = split->num_types;
                    split->num_blocks = self->num_blocks_;
                }
            }

            /* Adds the next symbol to the current histogram. When the current histogram
               reaches the target size, decides on merging the block. */
            public static unsafe void BlockSplitterAddSymbol(BlockSplitterLiteral* self, size_t symbol) {
                HistogramLiteral.HistogramAdd(&self->histograms_[self->curr_histogram_ix_], symbol);
                ++self->block_size_;
                if (self->block_size_ == self->target_block_size_) {
                    BlockSplitterFinishBlock(self, /* is_final = */ false);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe partial struct BlockSplitterCommand
        {
            /* Alphabet size of particular block category. */
            public size_t alphabet_size_;
            /* We collect at least this many symbols for each block. */
            public size_t min_block_size_;
            /* We merge histograms A and B if
                 entropy(A+B) < entropy(A) + entropy(B) + split_threshold_,
               where A is the current histogram and B is the histogram of the last or the
               second last block type. */
            public double split_threshold_;

            public size_t num_blocks_;
            public BlockSplit* split_;  /* not owned */
            public HistogramCommand* histograms_;  /* not owned */
            public size_t* histograms_size_;  /* not owned */

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
            public fixed double last_entropy_[2];
            /* The number of times we merged the current block with the last one. */
            public size_t merge_last_count_;

            public static unsafe void InitBlockSplitter(
                ref MemoryManager m, BlockSplitterCommand* self, size_t alphabet_size,
                size_t min_block_size, double split_threshold, size_t num_symbols,
                BlockSplit* split, HistogramCommand** histograms, size_t* histograms_size)
            {
                size_t max_num_blocks = num_symbols / min_block_size + 1;
                /* We have to allocate one more histogram than the maximum number of block
                   types for the current histogram when the meta-block is too big. */
                size_t max_num_types =
                    Math.Min(max_num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES + 1);
                self->alphabet_size_ = alphabet_size;
                self->min_block_size_ = min_block_size;
                self->split_threshold_ = split_threshold;
                self->num_blocks_ = 0;
                self->split_ = split;
                self->histograms_size_ = histograms_size;
                self->target_block_size_ = min_block_size;
                self->block_size_ = 0;
                self->curr_histogram_ix_ = 0;
                self->merge_last_count_ = 0;
                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, max_num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, max_num_blocks);
                self->split_->num_blocks = max_num_blocks;
                *histograms_size = max_num_types;
                *histograms = (HistogramCommand*)BrotliAllocate(ref m, *histograms_size * sizeof(HistogramCommand));
                self->histograms_ = *histograms;
                /* Clear only current histogram. */
                HistogramCommand.HistogramClear(&self->histograms_[0]);
                self->last_histogram_ix_0 = self->last_histogram_ix_1 = 0;
            }

            /* Does either of three things:
             (1) emits the current block with a new block type;
             (2) emits the current block with the type of the second last block;
             (3) merges the current block with the last block. */
            public static unsafe void BlockSplitterFinishBlock(
                BlockSplitterCommand* self, bool is_final)
            {
                BlockSplit* split = self->split_;
                double* last_entropy = self->last_entropy_;
                HistogramCommand* histograms = self->histograms_;
                self->block_size_ =
                    Math.Max(self->block_size_, self->min_block_size_);
                if (self->num_blocks_ == 0)
                {
                    /* Create first block. */
                    split->lengths[0] = (uint)self->block_size_;
                    split->types[0] = 0;
                    last_entropy[0] =
                        BitsEntropy(histograms[0].data_, self->alphabet_size_);
                    last_entropy[1] = last_entropy[0];
                    ++self->num_blocks_;
                    ++split->num_types;
                    ++self->curr_histogram_ix_;
                    if (self->curr_histogram_ix_ < *self->histograms_size_)
                        HistogramCommand.HistogramClear(&histograms[self->curr_histogram_ix_]);
                    self->block_size_ = 0;
                }
                else if (self->block_size_ > 0)
                {
                    double entropy = BitsEntropy(histograms[self->curr_histogram_ix_].data_,
                        self->alphabet_size_);
                    HistogramCommand* combined_histo = stackalloc HistogramCommand[2];
                    double* combined_entropy = stackalloc double[2];
                    double* diff = stackalloc double[2];
                    size_t j;
                    for (j = 0; j < 2; ++j)
                    {
                        size_t last_histogram_ix = j == 0 ? self->last_histogram_ix_0 : self->last_histogram_ix_1;
                        combined_histo[j] = histograms[self->curr_histogram_ix_];
                        HistogramCommand.HistogramAddHistogram(&combined_histo[j],
                            &histograms[last_histogram_ix]);
                        combined_entropy[j] = BitsEntropy(
                            &combined_histo[j].data_[0], self->alphabet_size_);
                        diff[j] = combined_entropy[j] - entropy - last_entropy[j];
                    }

                    if (split->num_types < BROTLI_MAX_NUMBER_OF_BLOCK_TYPES &&
                        diff[0] > self->split_threshold_ &&
                        diff[1] > self->split_threshold_)
                    {
                        /* Create new block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = (byte)split->num_types;
                        self->last_histogram_ix_1 = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = (byte)split->num_types;
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = entropy;
                        ++self->num_blocks_;
                        ++split->num_types;
                        ++self->curr_histogram_ix_;
                        if (self->curr_histogram_ix_ < *self->histograms_size_)
                            HistogramCommand.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->block_size_ = 0;
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    }
                    else if (diff[1] < diff[0] - 20.0)
                    {
                        /* Combine this block with second last block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = split->types[self->num_blocks_ - 2];
                        size_t tmp = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = self->last_histogram_ix_1;
                        self->last_histogram_ix_1 = tmp;
                        histograms[self->last_histogram_ix_0] = combined_histo[1];
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = combined_entropy[1];
                        ++self->num_blocks_;
                        self->block_size_ = 0;
                        HistogramCommand.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    }
                    else
                    {
                        /* Combine this block with last block. */
                        split->lengths[self->num_blocks_ - 1] += (uint)self->block_size_;
                        histograms[self->last_histogram_ix_0] = combined_histo[0];
                        last_entropy[0] = combined_entropy[0];
                        if (split->num_types == 1)
                        {
                            last_entropy[1] = last_entropy[0];
                        }
                        self->block_size_ = 0;
                        HistogramCommand.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        if (++self->merge_last_count_ > 1)
                        {
                            self->target_block_size_ += self->min_block_size_;
                        }
                    }
                }
                if (is_final)
                {
                    *self->histograms_size_ = split->num_types;
                    split->num_blocks = self->num_blocks_;
                }
            }

            /* Adds the next symbol to the current histogram. When the current histogram
               reaches the target size, decides on merging the block. */
            public static unsafe void BlockSplitterAddSymbol(BlockSplitterCommand* self, size_t symbol)
            {
                HistogramCommand.HistogramAdd(&self->histograms_[self->curr_histogram_ix_], symbol);
                ++self->block_size_;
                if (self->block_size_ == self->target_block_size_)
                {
                    BlockSplitterFinishBlock(self, /* is_final = */ false);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe partial struct BlockSplitterDistance
        {
            /* Alphabet size of particular block category. */
            public size_t alphabet_size_;
            /* We collect at least this many symbols for each block. */
            public size_t min_block_size_;
            /* We merge histograms A and B if
                 entropy(A+B) < entropy(A) + entropy(B) + split_threshold_,
               where A is the current histogram and B is the histogram of the last or the
               second last block type. */
            public double split_threshold_;

            public size_t num_blocks_;
            public BlockSplit* split_;  /* not owned */
            public HistogramDistance* histograms_;  /* not owned */
            public size_t* histograms_size_;  /* not owned */

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
            public fixed double last_entropy_[2];
            /* The number of times we merged the current block with the last one. */
            public size_t merge_last_count_;

            public static unsafe void InitBlockSplitter(
                ref MemoryManager m, BlockSplitterDistance* self, size_t alphabet_size,
                size_t min_block_size, double split_threshold, size_t num_symbols,
                BlockSplit* split, HistogramDistance** histograms, size_t* histograms_size)
            {
                size_t max_num_blocks = num_symbols / min_block_size + 1;
                /* We have to allocate one more histogram than the maximum number of block
                   types for the current histogram when the meta-block is too big. */
                size_t max_num_types =
                    Math.Min(max_num_blocks, BROTLI_MAX_NUMBER_OF_BLOCK_TYPES + 1);
                self->alphabet_size_ = alphabet_size;
                self->min_block_size_ = min_block_size;
                self->split_threshold_ = split_threshold;
                self->num_blocks_ = 0;
                self->split_ = split;
                self->histograms_size_ = histograms_size;
                self->target_block_size_ = min_block_size;
                self->block_size_ = 0;
                self->curr_histogram_ix_ = 0;
                self->merge_last_count_ = 0;
                BrotliEnsureCapacity(ref m, sizeof(byte), (void**)&split->types, &split->types_alloc_size, max_num_blocks);
                BrotliEnsureCapacity(ref m, sizeof(uint), (void**)&split->lengths, &split->lengths_alloc_size, max_num_blocks);
                self->split_->num_blocks = max_num_blocks;
                *histograms_size = max_num_types;
                *histograms = (HistogramDistance*)BrotliAllocate(ref m, *histograms_size * sizeof(HistogramDistance));
                self->histograms_ = *histograms;
                /* Clear only current histogram. */
                HistogramDistance.HistogramClear(&self->histograms_[0]);
                self->last_histogram_ix_0 = self->last_histogram_ix_1 = 0;
            }

            /* Does either of three things:
             (1) emits the current block with a new block type;
             (2) emits the current block with the type of the second last block;
             (3) merges the current block with the last block. */
            public static unsafe void BlockSplitterFinishBlock(
                BlockSplitterDistance* self, bool is_final)
            {
                BlockSplit* split = self->split_;
                double* last_entropy = self->last_entropy_;
                HistogramDistance* histograms = self->histograms_;
                self->block_size_ =
                    Math.Max(self->block_size_, self->min_block_size_);
                if (self->num_blocks_ == 0)
                {
                    /* Create first block. */
                    split->lengths[0] = (uint)self->block_size_;
                    split->types[0] = 0;
                    last_entropy[0] =
                        BitsEntropy(histograms[0].data_, self->alphabet_size_);
                    last_entropy[1] = last_entropy[0];
                    ++self->num_blocks_;
                    ++split->num_types;
                    ++self->curr_histogram_ix_;
                    if (self->curr_histogram_ix_ < *self->histograms_size_)
                        HistogramDistance.HistogramClear(&histograms[self->curr_histogram_ix_]);
                    self->block_size_ = 0;
                }
                else if (self->block_size_ > 0)
                {
                    double entropy = BitsEntropy(histograms[self->curr_histogram_ix_].data_,
                        self->alphabet_size_);
                    HistogramDistance* combined_histo = stackalloc HistogramDistance[2];
                    double* combined_entropy = stackalloc double[2];
                    double* diff = stackalloc double[2];
                    size_t j;
                    for (j = 0; j < 2; ++j)
                    {
                        size_t last_histogram_ix = j == 0 ? self->last_histogram_ix_0 : self->last_histogram_ix_1;
                        combined_histo[j] = histograms[self->curr_histogram_ix_];
                        HistogramDistance.HistogramAddHistogram(&combined_histo[j],
                            &histograms[last_histogram_ix]);
                        combined_entropy[j] = BitsEntropy(
                            &combined_histo[j].data_[0], self->alphabet_size_);
                        diff[j] = combined_entropy[j] - entropy - last_entropy[j];
                    }

                    if (split->num_types < BROTLI_MAX_NUMBER_OF_BLOCK_TYPES &&
                        diff[0] > self->split_threshold_ &&
                        diff[1] > self->split_threshold_)
                    {
                        /* Create new block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = (byte)split->num_types;
                        self->last_histogram_ix_1 = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = (byte)split->num_types;
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = entropy;
                        ++self->num_blocks_;
                        ++split->num_types;
                        ++self->curr_histogram_ix_;
                        if (self->curr_histogram_ix_ < *self->histograms_size_)
                            HistogramDistance.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->block_size_ = 0;
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    }
                    else if (diff[1] < diff[0] - 20.0)
                    {
                        /* Combine this block with second last block. */
                        split->lengths[self->num_blocks_] = (uint)self->block_size_;
                        split->types[self->num_blocks_] = split->types[self->num_blocks_ - 2];
                        size_t tmp = self->last_histogram_ix_0;
                        self->last_histogram_ix_0 = self->last_histogram_ix_1;
                        self->last_histogram_ix_1 = tmp;
                        histograms[self->last_histogram_ix_0] = combined_histo[1];
                        last_entropy[1] = last_entropy[0];
                        last_entropy[0] = combined_entropy[1];
                        ++self->num_blocks_;
                        self->block_size_ = 0;
                        HistogramDistance.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        self->merge_last_count_ = 0;
                        self->target_block_size_ = self->min_block_size_;
                    }
                    else
                    {
                        /* Combine this block with last block. */
                        split->lengths[self->num_blocks_ - 1] += (uint)self->block_size_;
                        histograms[self->last_histogram_ix_0] = combined_histo[0];
                        last_entropy[0] = combined_entropy[0];
                        if (split->num_types == 1)
                        {
                            last_entropy[1] = last_entropy[0];
                        }
                        self->block_size_ = 0;
                        HistogramDistance.HistogramClear(&histograms[self->curr_histogram_ix_]);
                        if (++self->merge_last_count_ > 1)
                        {
                            self->target_block_size_ += self->min_block_size_;
                        }
                    }
                }
                if (is_final)
                {
                    *self->histograms_size_ = split->num_types;
                    split->num_blocks = self->num_blocks_;
                }
            }

            /* Adds the next symbol to the current histogram. When the current histogram
               reaches the target size, decides on merging the block. */
            public static unsafe void BlockSplitterAddSymbol(BlockSplitterDistance* self, size_t symbol)
            {
                HistogramDistance.HistogramAdd(&self->histograms_[self->curr_histogram_ix_], symbol);
                ++self->block_size_;
                if (self->block_size_ == self->target_block_size_)
                {
                    BlockSplitterFinishBlock(self, /* is_final = */ false);
                }
            }
        }
    }
}