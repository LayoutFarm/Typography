//MIT, 2020-present, WinterDev  

namespace PixelFarm.Drawing
{
    public readonly struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        readonly char[] _rawString;

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

        public override string ToString()
        {
            return start + ":" + len;
        }


        public char[] GetRawCharBuffer() => _rawString;
    }

}