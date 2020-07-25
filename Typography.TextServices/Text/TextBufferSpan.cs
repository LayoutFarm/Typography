//MIT, 2014-present, WinterDev


namespace Typography.Text
{
    public readonly struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        readonly char[] _utf16Buffer;
        readonly int[] _utf32Buffer;
        public TextBufferSpan(char[] rawCharBuffer)
        {
            _utf16Buffer = rawCharBuffer;
            _utf32Buffer = null;
            this.len = rawCharBuffer.Length;
            this.start = 0;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _utf16Buffer = rawCharBuffer;
            _utf32Buffer = null;
        }
        public TextBufferSpan(int[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _utf16Buffer = null;
            _utf32Buffer = rawCharBuffer;
        }

        public TextBufferSpan CreateSubspan(int start, int len)
        {
            return (_utf16Buffer != null) ?
                new TextBufferSpan((char[])_utf16Buffer, start, len) :
                new TextBufferSpan(_utf32Buffer, start, len);
        }

        public override string ToString()
        {
            return start + ":" + len;
        }
        public int[] GetRawUtf32Buffer() => _utf32Buffer;
        public char[] GetRawUtf16Buffer() => _utf16Buffer;
        public bool IsUtf32Buffer => _utf32Buffer != null;
    }

}