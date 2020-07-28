//MIT, 2014-present, WinterDev

using System;
using System.Collections.Generic;

using System.Text;
using PixelFarm.CpuBlit;
using Typography.TextBreak;

namespace Typography.Text
{
    public class TextCopyBuffer
    {
        public enum BackupBufferKind
        {
            StringBuilder,
            Utf16ArrayList,
            Utf32ArrayList
        }

        readonly StringBuilder _sb = new StringBuilder();
        readonly ArrayList<int> _utf32Buffer = new ArrayList<int>();
        readonly ArrayList<char> _utf16Buffer = new ArrayList<char>();

        public TextCopyBuffer()
        {

        }
        public BackupBufferKind BackupKind { get; set; }

        public void AppendNewLine()
        {
            //push content of current line 
            //into plain doc
            switch (BackupKind)
            {
                default:
                    _sb.AppendLine();
                    break;
                case BackupBufferKind.Utf32ArrayList:
                    _utf32Buffer.Append('\r');
                    _utf32Buffer.Append('\n');
                    break;
                case BackupBufferKind.Utf16ArrayList:
                    _utf16Buffer.Append('\r');
                    _utf16Buffer.Append('\n');
                    break;
            }
        }
        public IEnumerable<string> GetLineIter()
        {
            //TODO: review this again
            switch (BackupKind)
            {
                default:
                    {
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
                    break;
                case BackupBufferKind.Utf16ArrayList:
                    {
                        InputReader reader = new InputReader(_utf16Buffer.UnsafeInternalArray, 0, _utf16Buffer.Length);
                        int latest_cut_begin = 0;
                        while (!reader.IsEnd)
                        {
                            char c0 = reader.C0;
                            if (c0 == '\r' && reader.PeekNext() == '\n')
                            {
                                //copy to a string line
                                yield return "";

                                reader.Read();
                            }
                            reader.Read();
                        }
                    }
                    break;
                case BackupBufferKind.Utf32ArrayList:
                    {
                        int latest_cut_begin = 0;
                        InputReader reader = new InputReader(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Length);
                        while (!reader.IsEnd)
                        {
                            char c0 = reader.C0;
                            if (c0 == '\r' && reader.PeekNext() == '\n')
                            {
                                //copy to a string line
                                yield return "";

                                reader.Read();
                            }
                            reader.Read();
                        }
                    }
                    break;
            }

        }


        public bool HasSomeRuns => _sb.Length > 0;

        public void AppendData(char[] buffer, int start, int len)
        {
            _sb.Append(buffer, start, len);
        }
        public void AppendData(int[] buffer, int start, int len)
        {
            int end = start + len;
            switch (BackupKind)
            {
                case BackupBufferKind.Utf32ArrayList:
                    {
                        _utf32Buffer.Append(buffer, start, len);
                    }
                    break;
                default:
                    {
                        for (int i = start; i < end; ++i)
                        {
                            int d = buffer[i];
                            char upper = (char)(d >> 16);
                            if (upper > 0)
                            {
                                InputReader.GetChars(buffer[i], out upper, out char lower);
                                _sb.Append(upper);
                                _sb.Append(lower);
                            }
                            else
                            {
                                _sb.Append((char)d);
                            }
                        }
                    }
                    break;
            }

        }
        public void Clear()
        {
            _sb.Length = 0;
            _utf32Buffer.Clear();
            _utf16Buffer.Clear();
        }

        public int Length
        {
            get
            {
                switch (BackupKind)
                {
                    default: return _sb.Length;
                    case BackupBufferKind.Utf16ArrayList:
                        return _utf16Buffer.Length;
                    case BackupBufferKind.Utf32ArrayList:
                        return _utf32Buffer.Length;
                }
            }
        }

        public void CopyTo(char[] charBuffer)
        {
            switch (BackupKind)
            {

            }
            _sb.CopyTo(0, charBuffer, 0, _sb.Length);
        }
        public void CopyTo(ArrayList<int> output)
        {
            switch (BackupKind)
            {
                case BackupBufferKind.Utf32ArrayList:
                    output.Append(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Length);
                    break;
                default:
                    throw new NotSupportedException();
            }

        }
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
    /// forward only character source 
    /// </summary>
    public class CharSource
    {
        //readonly ArrayList<char> _arrList = new ArrayList<char>();
        readonly ArrayList<int> _arrList = new ArrayList<int>();

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
            //convert utf32 to utf16 

            sb.AppendData(_arrList.UnsafeInternalArray, offset, len);
        }
        internal void Copy(int srcStart, int srcLen, char[] outputArr, int outputStart)
        {
            Array.Copy(_arrList.UnsafeInternalArray, srcStart, outputArr, outputStart, srcLen);
        }

        public void Append(CharSpan charSpan)
        {
            //append data from another charspan
            _arrList.Append(charSpan.UnsafeInternalCharArr, charSpan.beginAt, charSpan.len);

        }
        public void Append(int c)
        {
            _arrList.Append((char)c);
        }
        public void Append(char c)
        {
            //TODO: 
            _arrList.Append(c);
        }

        public CharBufferSegment NewSpan(char c)
        {
            int s = _arrList.Count;
            _arrList.Append(c);
            return new CharBufferSegment(this, s, 1);
        }
        public CharBufferSegment NewSpan(int c)
        {
            char c1 = (char)c;
            int s = _arrList.Count;
            _arrList.Append(c1);

            return new CharBufferSegment(this, s, 1);
        }
        public CharBufferSegment NewSpan(char[] charBuffer)
        {
            int s = _arrList.Count;

            for (int i = 0; i < charBuffer.Length; ++i)
            {
                char c0 = charBuffer[i];
                if (char.IsHighSurrogate(c0) && i + 1 < charBuffer.Length)
                {
                    _arrList.Append(char.ConvertToUtf32(c0, charBuffer[i + 1]));
                    i++;
                }
                else
                {
                    _arrList.Append(c0);
                }
            } 
           
            return new CharBufferSegment(this, s, _arrList.Count - s);
        }
        public CharBufferSegment NewSpan(string str)
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
        internal int[] UnsafeInternalArray => _arrList.UnsafeInternalArray;
    }


    public readonly ref struct CharSpan
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
        public int[] UnsafeInternalCharArr => _charSource.UnsafeInternalArray;
        public CharSource UnsafeInternalCharSource => _charSource;


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
        public string dbugGetString()
        {
            //convert internal utf32 to string
            return "";
            //return new string(UnsafeInternalCharArr, beginAt, len);
        }
        public override string ToString()
        {
            return beginAt + "," + len + "," + dbugGetString();
        }
#endif
    }

    public readonly struct CharBufferSegment
    {
        readonly CharSource _charSource;
        public readonly int beginAt;
        public readonly int len;
        public CharBufferSegment(CharSource charSource, int beginAt, int len)
        {
            _charSource = charSource;
            this.beginAt = beginAt;
            this.len = len;
        }
        public int Count => len;
        public int[] UnsafeInternalCharArr => _charSource.UnsafeInternalArray;
        public CharSource UnsafeInternalCharSource => _charSource;

        public Typography.Text.TextBufferSpan GetTextBufferSpan() => new Typography.Text.TextBufferSpan(UnsafeInternalCharArr, beginAt, len);

        public int GetUtf16Char(int index)
        {
            return _charSource.UnsafeInternalArray[beginAt + index];
        }
        public int GetUtf32Char(int index)
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
        public string dbugGetString()
        {
            //TODO: review here again
            return "";
        }
        public override string ToString()
        {
            return beginAt + "," + len + "," + dbugGetString();
        }
#endif
    }

}
