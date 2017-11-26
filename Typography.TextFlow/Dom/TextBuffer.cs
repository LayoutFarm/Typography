//MIT, 2014-2017, WinterDev
using System.Collections.Generic;

namespace Typography.TextLayout
{
    public class TextBuffer
    {
        char[] buffer;
        public TextBuffer(char[] buffer)
        {
            this.buffer = buffer;
        }
        internal char[] UnsafeGetInternalBuffer()
        {
            return buffer;
        }
        public string CopyString(int start, int len)
        {
            return new string(buffer, start, len);
        }
    }
}