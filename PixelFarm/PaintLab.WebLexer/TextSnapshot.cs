//BSD, 2014-present, WinterDev 

using System;
namespace LayoutFarm.WebLexer
{
    public class TextSnapshot
    {
        readonly char[] _textBuffer;
        readonly int _length;
        public TextSnapshot(char[] textBuffer)
        {
            _textBuffer = textBuffer;
            _length = textBuffer.Length;
        }

        public TextSnapshot(string str)
        {
            _textBuffer = str.ToCharArray();
            _length = _textBuffer.Length;
        }
        //
        public int Length => _length;
        //
        public char this[int index] => _textBuffer[index];

        //--------------
        public static char[] UnsafeGetInternalBuffer(TextSnapshot snap)
        {
            return snap._textBuffer;
        }
        public char[] Copy(int index, int length)
        {
            char[] newbuff = new char[length];
            Array.Copy(_textBuffer, index, newbuff, 0, length);
            return newbuff;
        }
        public string Substring(int index, int length)
        {
            return new string(_textBuffer, index, length);
        }
        public int IndexOf(char c)
        {
            return IndexOf(c, 0);
        }


        public int IndexOf(char c, int start)
        {
            char[] tmpChars = _textBuffer;
            int lim = tmpChars.Length;
            unsafe
            {
                fixed (char* start0 = &_textBuffer[0])
                {
                    char* curChar = start0 + start;
                    for (int i = start; i < lim; ++i)
                    {
                        if (*curChar == c)
                        {
                            return i;
                        }
                        curChar++;
                    }
                }
            }
            return -1;
        }

        internal int IndexOf(char c1, char c2, char c3, int start)
        {
            char[] tmpChars = _textBuffer;
            int lim = _length - 3;
            if (start < lim)
            {
                int i = start;
                char ex1 = tmpChars[i];
                char ex2 = tmpChars[i + 1];
                char ex3 = tmpChars[i + 2];
                do
                {
                    if (ex1 == c1 && ex2 == c2 && ex3 == c3)
                    {
                        return i;
                    }
                    i++;
                    ex1 = ex2;
                    ex2 = ex3;
                    ex3 = tmpChars[i];
                } while (i < lim);
            }
            //not found
            return -1;
        }
    }
}