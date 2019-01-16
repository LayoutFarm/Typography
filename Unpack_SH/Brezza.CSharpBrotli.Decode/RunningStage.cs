//MIT, 2009, 2010, 2013-2016 by the Brotli Authors.
//MIT, 2017, brezza92 (C# port from original code, by hand)

namespace CSharpBrotli.Decode
{
    /// <summary>
    /// Enumeration of decoding state-machine.
    /// </summary>
    public enum RunningStage
    {
        UNINITIALIZED,
        BLOCK_START,
        COMPRESSED_BLOCK_START,
        MAIN_LOOP,
        READ_METADATA,
        COPY_UNCOMPRESSED,
        INSERT_LOOP,
        COPY_LOOP,
        COPY_WRAP_BUFFER,
        TRANSFORM,
        FINISHED,
        CLOSED,
        WRITE
    }
}