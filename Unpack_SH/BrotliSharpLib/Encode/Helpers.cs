using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static unsafe partial class Brotli {
        private static void BrotliEnsureCapacity(ref MemoryManager m, int t, void** a, size_t* c, size_t r) {
            if (*c < r) {
                size_t new_size = *c == 0 ? r : *c;
                void* new_array;
                while (new_size < r) new_size *= 2;
                new_array = BrotliAllocate(ref m, new_size * t);
                if (*c != 0)
                    memcpy(new_array, *a, *c * t);
                BrotliFree(ref m, *a);
                *a = new_array;
                *c = new_size;
            }
        }
    }
}