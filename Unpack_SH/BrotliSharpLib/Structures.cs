using System.Runtime.InteropServices;
using size_t = BrotliSharpLib.Brotli.SizeT;

namespace BrotliSharpLib
{
    public static partial class Brotli {
        internal unsafe delegate void* brotli_alloc_func(void* opaque, size_t size);

        internal unsafe delegate void brotli_free_func(void* opaque, void* address);
    }
}
