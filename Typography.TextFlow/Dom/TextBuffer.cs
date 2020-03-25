//MIT, 2014-present, WinterDev


namespace Typography.TextLayout
{

    public class TextBuffer
    {
        internal char[] _buffer;
        public TextBuffer(char[] buffer)
        {
            _buffer = buffer;
        }

        public int Len => _buffer.Length;

        public string CopyString(int start, int len)
        {
            return new string(_buffer, start, len);
        }
        //-------- 
        public char[] UnsafeGetInternalBuffer() => _buffer;
    }

    public class ReusableTextBuffer : TextBuffer
    {
        public ReusableTextBuffer(char[] buffer) : base(buffer)
        {
            _buffer = buffer;
        }
        public void SetRawCharBuffer(char[] buffer)
        {
            _buffer = buffer;
        }
    }


}