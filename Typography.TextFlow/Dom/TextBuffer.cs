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
        internal char[] UnsafeGetInternalBuffer() => _buffer;
        internal TextBuffer()
        {

        }
    }
    public class ReusableTextBuffer : TextBuffer
    {
        public ReusableTextBuffer()
        {
            //for reusable textbuffer
        }
        public void SetRawCharBuffer(char[] buffer)
        {
            _buffer = buffer;
        }
    }


}