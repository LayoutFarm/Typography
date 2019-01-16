using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib {
    public static partial class Brotli {
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct MemoryManager {
            public brotli_alloc_func alloc_func;
            public brotli_free_func free_func;
            public void* opaque;
        }

        private static unsafe void BrotliInitMemoryManager(
            ref MemoryManager m, brotli_alloc_func alloc_func, brotli_free_func free_func,
            void* opaque) {
            if (alloc_func == null) {
                m.alloc_func = DefaultAllocFunc;
                m.free_func = DefaultFreeFunc;
                m.opaque = null;
            }
            else {
                m.alloc_func = alloc_func;
                m.free_func = free_func;
                m.opaque = opaque;
            }
        }

        private static unsafe void* BrotliAllocate(ref MemoryManager m, size_t n) {
            return m.alloc_func(m.opaque, n);
        }

        private static unsafe void BrotliFree(ref MemoryManager m, void* p) {
            m.free_func(m.opaque, p);
        }
    }
}