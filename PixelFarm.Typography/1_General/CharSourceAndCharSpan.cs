//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;
using System.Text;
using PixelFarm.CpuBlit;

namespace Typography.Text
{
    public class TextCopyBuffer
    {
        readonly StringBuilder _sb = new StringBuilder();
#if DEBUG
        public TextCopyBuffer()
        {

        }
#endif       
        public void AppendNewLine()
        {
            //push content of current line 
            //into plain doc
            _sb.AppendLine();
        }
        public IEnumerable<string> GetLineIter()
        {
            //TODO: review this again

            using (System.IO.StringReader reader = new System.IO.StringReader(_sb.ToString()))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    yield return line;
                    line = reader.ReadLine();
                }
            }
        }


        public bool HasSomeRuns => _sb.Length > 0;

        public void AppendData(char[] buffer, int start, int len) => _sb.Append(buffer, start, len);

        public void Clear() => _sb.Length = 0;

        public int Length => _sb.Length;

        public void CopyTo(char[] charBuffer) => _sb.CopyTo(0, charBuffer, 0, _sb.Length);

        [ThreadStatic]
        static ArrayList<char> s_tempBuffer;
        public void CopyTo(StringBuilder stbuilder)
        {

            if (s_tempBuffer == null)
            {
                s_tempBuffer = new ArrayList<char>();
            }
            s_tempBuffer.AdjustSize(_sb.Length);
            _sb.CopyTo(0, s_tempBuffer.UnsafeInternalArray, 0, _sb.Length);
            stbuilder.Append(s_tempBuffer.UnsafeInternalArray, 0, _sb.Length);
        }
        public override string ToString() => _sb.ToString();
    }


    /// <summary>
    /// character source 
    /// </summary>
    public class CharSource
    {
        readonly ArrayList<char> _arrList = new ArrayList<char>();
#if DEBUG
        public CharSource()
        {

        }
#endif
        /// <summary>
        /// write content to TextCopyBuffer
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="offset"></param>
        /// <param name="len"></param>
        internal void WriteTo(TextCopyBuffer sb, int offset, int len)
        {
            sb.AppendData(_arrList.UnsafeInternalArray, offset, len);
        }
        internal void Copy(int srcStart, int srcLen, char[] outputArr, int outputStart)
        {
            Array.Copy(_arrList.UnsafeInternalArray, srcStart, outputArr, outputStart, srcLen);
        }

        public void Append(CharSpan charSpan)
        {
            _arrList.Append(charSpan.UnsafeInternalCharArr, charSpan.beginAt, charSpan.len);
        }
        public void Append(char c)
        {
            _arrList.Append(c);
        }

        public CharSpan NewSpan(char c)
        {
            int s = _arrList.Count;
            _arrList.Append(c);
            return new CharSpan(this, s, 1);
        }
        public CharSpan NewSpan(char[] charBuffer)
        {
            int s = _arrList.Count;
            _arrList.Append(charBuffer);
            return new CharSpan(this, s, charBuffer.Length);
        }
        public CharSpan NewSpan(string str)
        {
            return NewSpan(str.ToCharArray());
        }

        public void CopyAndAppend(int start, int len)
        {
            //copy data from srcRange
            //and append to the last part of _arrList
            _arrList.CopyAndAppend(start, len);
        }

        public int LatestLen => _arrList.Count;
        public char[] UnsafeInternalArray => _arrList.UnsafeInternalArray;
    }

    public readonly struct CharSpan
    {
        readonly CharSource _charSource;
        public readonly int beginAt;
        public readonly int len;
        public CharSpan(CharSource charSource, int beginAt, int len)
        {
            _charSource = charSource;
            this.beginAt = beginAt;
            this.len = len;
        }
        public int Count => len;
        public char[] UnsafeInternalCharArr => _charSource.UnsafeInternalArray;
        public CharSource UnsafeInternalCharSource => _charSource;
        public string GetString()
        {
            return new string(UnsafeInternalCharArr, beginAt, len);
        }
        public Typography.Text.TextBufferSpan GetTextBufferSpan()
        {
            return new Typography.Text.TextBufferSpan(UnsafeInternalCharArr, beginAt, len);
        }
        public char GetUtf16Char(int index)
        {
            return _charSource.UnsafeInternalArray[beginAt + index];
        }
        public void WriteTo(TextCopyBuffer sb)
        {
            _charSource.WriteTo(sb, beginAt, len);
        }
        public void WriteTo(TextCopyBuffer sb, int start, int count)
        {
            _charSource.WriteTo(sb, beginAt + start, count);
        }
        public void Copy(char[] outputArr, int start, int count)
        {
            _charSource.Copy(beginAt + start, count, outputArr, 0);
        }
        public void Copy(char[] outputArr, int start)
        {
            _charSource.Copy(beginAt + start, beginAt + len - start, outputArr, 0);
        }
        public CharSpan MakeSubSpan(int startOffset, int count)
        {
            if (startOffset + count < len)
            {
                return new CharSpan(_charSource, beginAt + startOffset, len);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public CharSpan MakeSubSpan(int startOffset)
        {
            return new CharSpan(_charSource, beginAt + startOffset, len - startOffset);
        }
        public CharSpan MakeLeftSubSpan(int count)
        {
            return new CharSpan(_charSource, beginAt, count);
        }

        public static readonly ArrayListSegment<char> Empty = new ArrayListSegment<char>();
#if DEBUG
        public override string ToString()
        {
            return beginAt + "," + len + "," + GetString();
        }
#endif
    }


}
