//MIT, 2014-2017, WinterDev


namespace Typography.TextLayout
{
    public class TextBuffer
    {
        char[] _buffer;
        public TextBuffer(char[] buffer)
        {
            this._buffer = buffer;
        }
        public int Len { get { return _buffer.Length; } }
        public static char[] UnsafeGetCharBuffer(TextBuffer textBuffer)
        {
            return textBuffer._buffer;
        }
        internal char[] UnsafeGetInternalBuffer()
        {
            return _buffer;
        }
        public string CopyString(int start, int len)
        {
            return new string(_buffer, start, len);
        }
    }
}