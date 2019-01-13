using System;
using System.Runtime.InteropServices;

namespace BrotliSharpLib {
    public static partial class Brotli {
        static Brotli() {
            // BrotliNative uses fixed on these arrays, ensure to pin them to prevent GC from cleaning them up,
            // especially with kContextLookup since address references are stored which could break
            GCHandle.Alloc(kContextLookup, GCHandleType.Pinned);
            GCHandle.Alloc(kBrotliDictionary, GCHandleType.Pinned);
            GCHandle.Alloc(kStaticDictionaryHash, GCHandleType.Pinned);
            GCHandle.Alloc(kStaticContextMapComplexUTF8, GCHandleType.Pinned);
            GCHandle.Alloc(kStaticContextMapContinuation, GCHandleType.Pinned);
            GCHandle.Alloc(kStaticContextMapSimpleUTF8, GCHandleType.Pinned);
        }
    }
}