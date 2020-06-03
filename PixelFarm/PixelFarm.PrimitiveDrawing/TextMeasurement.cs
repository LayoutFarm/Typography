//MIT, 2020, WinterDev

namespace PixelFarm.Drawing
{
     
    public struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        char[] _rawString;


        public TextBufferSpan(char[] rawCharBuffer)
        {
            _rawString = rawCharBuffer;
            this.len = rawCharBuffer.Length;
            this.start = 0;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _rawString = rawCharBuffer;
        }

        public char GetChar(int localOffset) => _rawString[start + localOffset];
        public char[] GetRawCharBuffer() => _rawString;

#if DEBUG
        public override string ToString()
        {
            return start + ":" + len;
        }
#endif


    }

    public struct TextSpanMeasureResult
    {
        public int[] outputXAdvances;
        public int outputTotalW;
        public ushort lineHeight;

        public bool hasSomeExtraOffsetY;
        public short minOffsetY;
        public short maxOffsetY;
    }


}