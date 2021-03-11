//MIT, 2014-present, WinterDev

using System;
namespace Typography.Text
{


    public readonly struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        readonly object _buffer;
        public readonly bool IsUtf32Buffer;

        public TextBufferSpan(char[] rawCharBuffer)
        {
            this.start = 0;
            this.len = rawCharBuffer.Length;
            _buffer = rawCharBuffer;
            IsUtf32Buffer = false;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _buffer = rawCharBuffer;
            IsUtf32Buffer = false;
        }
        public TextBufferSpan(int[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _buffer = rawCharBuffer;
            IsUtf32Buffer = true;
        }
        public TextBufferSpan(int[] rawCharBuffer)
        {
            this.start = 0;
            this.len = rawCharBuffer.Length;
            _buffer = rawCharBuffer;
            IsUtf32Buffer = true;
        }
        private TextBufferSpan(object o, int start, int len, bool isUtf32)
        {
            this.start = start;
            this.len = len;
            _buffer = o;
            IsUtf32Buffer = isUtf32;
        }
        public TextBufferSpan CreateSubspan(int start, int len) => new TextBufferSpan(_buffer, this.start + start, len, IsUtf32Buffer);
        public override string ToString() => start + ":" + len;

        /// <summary>
        /// unsafe access to underlying buffer
        /// </summary>
        /// <returns></returns>
        public int[] GetRawUtf32Buffer() => (int[])_buffer;
        /// <summary>
        /// unsafe access to underlying buffer, review here again
        /// </summary>
        /// <returns></returns>
        public char[] GetRawUtf16Buffer() => (char[])_buffer;

        public int Count => len;
        public bool IsEmpty => len == 0;

        public TextBufferSpan MakeSubSpan(int startOffset, int count)
        {
            if (startOffset + count < len)
            {
                return new TextBufferSpan(_buffer, this.start + startOffset, len, IsUtf32Buffer);
            }
            else
            {
                throw new System.NotSupportedException();
            }
        }

        public int GetChar(int index)
        {
            if (IsUtf32Buffer)
            {
                int[] b = (int[])_buffer;
                return b[index + start];
            }
            else
            {
                char[] b = (char[])_buffer;
                return b[index + start];
            }
        }
        public void WriteTo(TextCopyBuffer sb)
        {
            if (len > 0)
            {
                if (IsUtf32Buffer)
                {
                    sb.Append((int[])_buffer, start, len);
                }
                else
                {
                    sb.Append((char[])_buffer, start, len);
                }
            }
        }
        public void WriteTo(TextCopyBuffer sb, int start, int count)
        {
            if (len > 0)
            {
                if (IsUtf32Buffer)
                {
                    sb.Append((int[])_buffer, this.start + start, count);
                }
                else
                {
                    sb.Append((char[])_buffer, this.start + start, count);
                }
            }
        }

        //public TextBufferSpan MakeSubSpan(int startOffset)
        //{
        //    return new TextBufferSpan(_buffer, this.startOffset + startOffset, len - startOffset, ContentKind);
        //}
        //public TextBufferSpan MakeLeftSubSpan(int count)
        //{
        //    return new TextBufferSpan(_buffer, startOffset, count, ContentKind);
        //}

    }


}