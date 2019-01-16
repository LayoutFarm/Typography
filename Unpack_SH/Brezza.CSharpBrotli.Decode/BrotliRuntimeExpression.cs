//MIT, 2009, 2010, 2013-2016 by the Brotli Authors.
//MIT, 2017, brezza92 (C# port from original code, by hand)

using System;

namespace CSharpBrotli.Decode
{
    /// <summary>
    /// Unchecked exception used internally.
    /// </summary>
    public class BrotliRuntimeException : Exception
    {
        public BrotliRuntimeException() : base() { }
        public BrotliRuntimeException(string message) : base(message) { }
        public BrotliRuntimeException(string message, Exception innerException) : base(message, innerException) { }
    }
}