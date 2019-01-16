using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        private static unsafe void StoreStaticCodeLengthCode(
            size_t* storage_ix, byte* storage) {
            BrotliWriteBits(
                40, 0x0000ff55555554U, storage_ix, storage);
        }

        private static unsafe void StoreStaticCommandHuffmanTree(
            size_t* storage_ix, byte* storage)
        {
            BrotliWriteBits(
                56, 0x92624416307003U, storage_ix, storage);
            BrotliWriteBits(3, 0x00000000U, storage_ix, storage);
        }

        private static unsafe void StoreStaticDistanceHuffmanTree(
            size_t* storage_ix, byte* storage)
        {
            BrotliWriteBits(28, 0x0369dc03U, storage_ix, storage);
        }
    }
}