using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli
    {
        private static unsafe class BlockEncoderLiteral {
            public static unsafe void BuildAndStoreEntropyCodes(ref MemoryManager m, BlockEncoder* self,
                HistogramLiteral* histograms, size_t histograms_size,
                HuffmanTree* tree, size_t* storage_ix, byte* storage)
            {
                size_t alphabet_size = self->alphabet_size_;
                size_t table_size = histograms_size * alphabet_size;
                self->depths_ = (byte*)BrotliAllocate(ref m, table_size * sizeof(byte));
                self->bits_ = (ushort*)BrotliAllocate(ref m, table_size * sizeof(ushort));
                {
                    size_t i;
                    for (i = 0; i < histograms_size; ++i)
                    {
                        size_t ix = i * alphabet_size;
                        BuildAndStoreHuffmanTree(&histograms[i].data_[0], alphabet_size, tree,
                            &self->depths_[ix], &self->bits_[ix], storage_ix, storage);
                    }
                }
            }
        }

        private static unsafe class BlockEncoderDistance
        {
            public static unsafe void BuildAndStoreEntropyCodes(ref MemoryManager m, BlockEncoder* self,
                HistogramDistance* histograms, size_t histograms_size,
                HuffmanTree* tree, size_t* storage_ix, byte* storage)
            {
                size_t alphabet_size = self->alphabet_size_;
                size_t table_size = histograms_size * alphabet_size;
                self->depths_ = (byte*)BrotliAllocate(ref m, table_size * sizeof(byte));
                self->bits_ = (ushort*)BrotliAllocate(ref m, table_size * sizeof(ushort));
                {
                    size_t i;
                    for (i = 0; i < histograms_size; ++i)
                    {
                        size_t ix = i * alphabet_size;
                        BuildAndStoreHuffmanTree(&histograms[i].data_[0], alphabet_size, tree,
                            &self->depths_[ix], &self->bits_[ix], storage_ix, storage);
                    }
                }
            }
        }

        private static unsafe class BlockEncoderCommand
        {
            public static unsafe void BuildAndStoreEntropyCodes(ref MemoryManager m, BlockEncoder* self,
                HistogramCommand* histograms, size_t histograms_size,
                HuffmanTree* tree, size_t* storage_ix, byte* storage)
            {
                size_t alphabet_size = self->alphabet_size_;
                size_t table_size = histograms_size * alphabet_size;
                self->depths_ = (byte*)BrotliAllocate(ref m, table_size * sizeof(byte));
                self->bits_ = (ushort*)BrotliAllocate(ref m, table_size * sizeof(ushort));
                {
                    size_t i;
                    for (i = 0; i < histograms_size; ++i)
                    {
                        size_t ix = i * alphabet_size;
                        BuildAndStoreHuffmanTree(&histograms[i].data_[0], alphabet_size, tree,
                            &self->depths_[ix], &self->bits_[ix], storage_ix, storage);
                    }
                }
            }
        }
    }
}