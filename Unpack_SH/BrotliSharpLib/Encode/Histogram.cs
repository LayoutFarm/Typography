using System;
using size_t = BrotliSharpLib.Brotli.SizeT;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct HistogramLiteral {
            private const int DATA_SIZE = BROTLI_NUM_LITERAL_SYMBOLS;

            public fixed uint data_[DATA_SIZE];
            public size_t total_count_;
            public double bit_cost_;

            public static void HistogramClear(HistogramLiteral* self) {
                memset(self->data_, 0, DATA_SIZE * sizeof(uint));
                self->total_count_ = 0;
                self->bit_cost_ = double.MaxValue;
            }

            public static void ClearHistograms(HistogramLiteral* array, size_t length) {
                size_t i;
                for (i = 0; i < length; ++i) HistogramClear(array + i);
            }

            public static void HistogramAdd(HistogramLiteral* self, size_t val) {
                ++self->data_[val];
                ++self->total_count_;
            }

            public static void HistogramAddVector(HistogramLiteral* self, byte* p, size_t n) {
                self->total_count_ += n;
                n += 1;
                while (--n != 0) ++self->data_[*p++];
            }

            public static void HistogramAddHistogram(HistogramLiteral* self, HistogramLiteral* v) {
                size_t i;
                self->total_count_ += v->total_count_;
                for (i = 0; i < DATA_SIZE; ++i)
                {
                    self->data_[i] += v->data_[i];
                }
            }

            public static size_t HistogramDataSize() {
                return DATA_SIZE;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct HistogramCommand
        {
            private const int DATA_SIZE = BROTLI_NUM_COMMAND_SYMBOLS;

            public fixed uint data_[DATA_SIZE];
            public size_t total_count_;
            public double bit_cost_;

            public static void HistogramClear(HistogramCommand* self)
            {
                memset(self->data_, 0, DATA_SIZE * sizeof(uint));
                self->total_count_ = 0;
                self->bit_cost_ = double.MaxValue;
            }

            public static void ClearHistograms(HistogramCommand* array, size_t length)
            {
                size_t i;
                for (i = 0; i < length; ++i) HistogramClear(array + i);
            }

            public static void HistogramAdd(HistogramCommand* self, size_t val)
            {
                ++self->data_[val];
                ++self->total_count_;
            }

            public static void HistogramAddVector(HistogramCommand* self, ushort* p, size_t n)
            {
                self->total_count_ += n;
                n += 1;
                while (--n != 0) ++self->data_[*p++];
            }

            public static void HistogramAddHistogram(HistogramCommand* self, HistogramCommand* v)
            {
                size_t i;
                self->total_count_ += v->total_count_;
                for (i = 0; i < DATA_SIZE; ++i)
                {
                    self->data_[i] += v->data_[i];
                }
            }

            public static size_t HistogramDataSize()
            {
                return DATA_SIZE;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct HistogramDistance
        {
            private const int DATA_SIZE = BROTLI_NUM_DISTANCE_SYMBOLS;

            public fixed uint data_[DATA_SIZE];
            public size_t total_count_;
            public double bit_cost_;

            public static void HistogramClear(HistogramDistance* self)
            {
                memset(self->data_, 0, DATA_SIZE * sizeof(uint));
                self->total_count_ = 0;
                self->bit_cost_ = double.MaxValue;
            }

            public static void ClearHistograms(HistogramDistance* array, size_t length)
            {
                size_t i;
                for (i = 0; i < length; ++i) HistogramClear(array + i);
            }

            public static void HistogramAdd(HistogramDistance* self, size_t val)
            {
                ++self->data_[val];
                ++self->total_count_;
            }

            public static void HistogramAddVector(HistogramDistance* self, ushort* p, size_t n)
            {
                self->total_count_ += n;
                n += 1;
                while (--n != 0) ++self->data_[*p++];
            }

            public static void HistogramAddHistogram(HistogramDistance* self, HistogramDistance* v)
            {
                size_t i;
                self->total_count_ += v->total_count_;
                for (i = 0; i < DATA_SIZE; ++i)
                {
                    self->data_[i] += v->data_[i];
                }
            }

            public static size_t HistogramDataSize()
            {
                return DATA_SIZE;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct BlockSplitIterator
        {
            public BlockSplit* split_;  /* Not owned. */
            public size_t idx_;
            public size_t type_;
            public size_t length_;
        }

        private static unsafe void InitBlockSplitIterator(BlockSplitIterator* self,
            BlockSplit* split)
        {
            self->split_ = split;
            self->idx_ = 0;
            self->type_ = 0;
            self->length_ = split->lengths != null ? split->lengths[0] : 0;
        }

        private static unsafe void BlockSplitIteratorNext(BlockSplitIterator* self)
        {
            if (self->length_ == 0)
            {
                ++self->idx_;
                self->type_ = self->split_->types[self->idx_];
                self->length_ = self->split_->lengths[self->idx_];
            }
            --self->length_;
        }

        private static unsafe void BrotliBuildHistogramsWithContext(
            Command* cmds, size_t num_commands,
            BlockSplit* literal_split, BlockSplit* insert_and_copy_split,
            BlockSplit* dist_split, byte* ringbuffer, size_t start_pos,
            size_t mask, byte prev_byte, byte prev_byte2,
            ContextType* context_modes, HistogramLiteral* literal_histograms,
            HistogramCommand* insert_and_copy_histograms,
            HistogramDistance* copy_dist_histograms)
        {
            size_t pos = start_pos;
            BlockSplitIterator literal_it;
            BlockSplitIterator insert_and_copy_it;
            BlockSplitIterator dist_it;
            size_t i;

            InitBlockSplitIterator(&literal_it, literal_split);
            InitBlockSplitIterator(&insert_and_copy_it, insert_and_copy_split);
            InitBlockSplitIterator(&dist_it, dist_split);
            for (i = 0; i < num_commands; ++i)
            {
                Command* cmd = &cmds[i];
                size_t j;
                BlockSplitIteratorNext(&insert_and_copy_it);
                HistogramCommand.HistogramAdd(&insert_and_copy_histograms[insert_and_copy_it.type_],
                    cmd->cmd_prefix_);
                for (j = cmd->insert_len_; j != 0; --j)
                {
                    size_t context;
                    BlockSplitIteratorNext(&literal_it);
                    context = context_modes != null ?
                        ((literal_it.type_ << BROTLI_LITERAL_CONTEXT_BITS) +
                         Context(prev_byte, prev_byte2, context_modes[literal_it.type_])) :
                        literal_it.type_;
                    HistogramLiteral.HistogramAdd(&literal_histograms[context],
                        ringbuffer[pos & mask]);
                    prev_byte2 = prev_byte;
                    prev_byte = ringbuffer[pos & mask];
                    ++pos;
                }
                pos += CommandCopyLen(cmd);
                if (CommandCopyLen(cmd) != 0)
                {
                    prev_byte2 = ringbuffer[(pos - 2) & mask];
                    prev_byte = ringbuffer[(pos - 1) & mask];
                    if (cmd->cmd_prefix_ >= 128)
                    {
                        size_t context;
                        BlockSplitIteratorNext(&dist_it);
                        context = (dist_it.type_ << BROTLI_DISTANCE_CONTEXT_BITS) +
                                  CommandDistanceContext(cmd);
                        HistogramDistance.HistogramAdd(&copy_dist_histograms[context],
                            cmd->dist_prefix_);
                    }
                }
            }
        }
    }
}