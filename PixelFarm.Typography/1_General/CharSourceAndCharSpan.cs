//MIT, 2014-present, WinterDev

using System;

using System.Text;
using PixelFarm.CpuBlit;
using Typography.TextBreak;

namespace Typography.Text
{
    public abstract class TextCopyBuffer
    {
        public enum BackupBufferKind
        {
            Utf16ArrayList,
            Utf32ArrayList
        }
        public abstract BackupBufferKind Kind { get; }
        public abstract void AppendLine();
        public abstract int Length { get; }
        public abstract void Clear();
        public abstract void AppendData(char[] buffer, int start, int len);
        public abstract void AppendData(int[] buffer, int start, int len);
        public abstract unsafe void AppendData(char* buffer, int len);
        //
        public abstract void CopyTo(ArrayList<int> output);
        public abstract void CopyTo(StringBuilder stbuilder);
        public abstract void CopyTo(StringBuilder stbuilder, int startIndex, int len);
        //
        public abstract void GetReader(out InputReader reader);
    }

    public sealed class TextCopyBufferUtf16 : TextCopyBuffer
    {
        readonly ArrayList<char> _utf16Buffer = new ArrayList<char>();
        public TextCopyBufferUtf16() { }
        public TextCopyBufferUtf16(char[] buffer)
        {
            AppendData(buffer, 0, buffer.Length);
        }
        public override BackupBufferKind Kind => BackupBufferKind.Utf16ArrayList;
        public override void AppendLine()
        {
            _utf16Buffer.Append('\r');
            _utf16Buffer.Append('\n');
        }
        public override int Length => _utf16Buffer.Count;
        public override void Clear()
        {
            _utf16Buffer.Clear();
        }
        public override unsafe void AppendData(char* buffer, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                _utf16Buffer.Append(*buffer);
                buffer++;//move next
            }
        }
        public override void AppendData(char[] buffer, int start, int len)
        {
            _utf16Buffer.Append(buffer, start, len);
        }
        public override void AppendData(int[] buffer, int start, int len)
        {
            int end = start + len;
            for (int i = start; i < end; ++i)
            {
                int d = buffer[i];
                char upper = (char)(d >> 16);
                if (upper > 0)
                {
                    InputReader.GetChars(buffer[i], out upper, out char lower);
                    _utf16Buffer.Append(upper);
                    _utf16Buffer.Append(lower);
                }
                else
                {
                    _utf16Buffer.Append((char)d);
                }
            }
        }
        public int GetChar(int index)
        {
            //TODO: review here
            return _utf16Buffer[index];
        }
        public override void CopyTo(ArrayList<int> output)
        {
            //from utf16 to utf32
            int end = _utf16Buffer.Length;
            char c1;
            for (int i = 0; i < end; ++i)
            {
                char c = _utf16Buffer[i];
                if (char.IsHighSurrogate(c) && (i + 1 < end) && char.IsLowSurrogate(c1 = _utf16Buffer[i + 1]))
                {
                    output.Append(char.ConvertToUtf32(c, c1));
                    ++i;
                }
                else
                {
                    output.Append(c);
                }
            }
        }
        public override void CopyTo(StringBuilder stbuilder)
        {
            stbuilder.Append(_utf16Buffer.UnsafeInternalArray, 0, _utf16Buffer.Length);
        }
        public override void CopyTo(StringBuilder stbuilder, int startIndex, int len)
        {
            //from utf16 to utf32
            stbuilder.Append(_utf16Buffer.UnsafeInternalArray, startIndex, len);
        }
        public override void GetReader(out InputReader reader)
        {
            reader = new InputReader(_utf16Buffer.UnsafeInternalArray, 0, _utf16Buffer.Length);
        }
    }
    public sealed class TextCopyBufferUtf32 : TextCopyBuffer
    {

        readonly ArrayList<int> _utf32Buffer = new ArrayList<int>();
        public TextCopyBufferUtf32() { }
        public TextCopyBufferUtf32(char[] buffer)
        {
            AppendData(buffer, 0, buffer.Length);
        }
        public override BackupBufferKind Kind => BackupBufferKind.Utf32ArrayList;
        public override void AppendLine()
        {
            _utf32Buffer.Append('\r');
            _utf32Buffer.Append('\n');
        }
        public override int Length => _utf32Buffer.Count;
        public override void Clear()
        {
            _utf32Buffer.Clear();
        }
        public override unsafe void AppendData(char* buffer, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                char c = *buffer;
                if (char.IsHighSurrogate(c) && i + 1 < len)
                {
                    buffer++;
                    char c2 = *buffer;
                    if (char.IsLowSurrogate(c2))
                    {
                        _utf32Buffer.Append(char.ConvertToUtf32(c, c2));
                    }
                    else
                    {
                        //skip c?
                        _utf32Buffer.Append(c2);
                    }
                }
                else
                {
                    _utf32Buffer.Append(c);
                }
                buffer++;
            }
        }

        public override void AppendData(char[] buffer, int start, int len)
        {
            int end = start + len;
            for (int i = start; i < end; ++i)
            {
                char c = buffer[i];
                if (char.IsHighSurrogate(c) && i + 1 < end)
                {
                    char c2 = buffer[i + 1];
                    if (char.IsLowSurrogate(c2))
                    {
                        _utf32Buffer.Append(char.ConvertToUtf32(c, c2));
                    }
                    else
                    {
                        //skip c?
                        _utf32Buffer.Append(c2);
                    }
                    i++;
                }
                else
                {
                    _utf32Buffer.Append(c);
                }
            }
        }

        public override void AppendData(int[] buffer, int start, int len)
        {
            _utf32Buffer.Append(buffer, start, len);
        }
        public int GetChar(int index)
        {
            //TODO: review here
            return _utf32Buffer[index];
        }
        public override void CopyTo(ArrayList<int> output)
        {
            output.Append(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Length);
        }
        public override void CopyTo(StringBuilder stbuilder)
        {
            int j = _utf32Buffer.Length;
            for (int i = 0; i < j; ++i)
            {
                int codepoint = _utf32Buffer[i];
                if (((codepoint) >> 16) != 0)
                {
                    InputReader.GetChars(codepoint, out char c0, out char c1);
                    stbuilder.Append(c0);
                    stbuilder.Append(c1);
                }
                else
                {
                    stbuilder.Append((char)codepoint);
                }
            }
        }
        /// <summary>
        /// copy content to string builder
        /// </summary>
        /// <param name="stbuilder"></param>
        /// <param name="startIndex"></param>
        /// <param name="len"></param>
        public override void CopyTo(StringBuilder stbuilder, int startIndex, int len)
        {

            for (int i = 0; i < len; ++i)
            {
                int codepoint = _utf32Buffer[i + startIndex];
                if (((codepoint) >> 16) != 0)
                {
                    InputReader.GetChars(codepoint, out char c0, out char c1);
                    stbuilder.Append(c0);
                    stbuilder.Append(c1);
                }
                else
                {
                    stbuilder.Append((char)codepoint);
                }
            }
        }
        public override void GetReader(out InputReader reader)
        {
            reader = new InputReader(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Length);
        }
    }



    public static class TextCopyBufferExtension
    {
        public static void AppendData(this TextCopyBuffer buff, char[] data)
        {
            unsafe
            {
                fixed (char* c = &data[0])
                {
                    buff.AppendData(c, data.Length);
                }
            }
        }
        public static void AppendData(this TextCopyBuffer buff, string data)
        {
            unsafe
            {
                fixed (char* c = data)
                {
                    buff.AppendData(c, data.Length);
                }
            }
        }
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
            _arrList.Append(c);
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
            int s = _arrList.Count;
            _arrList.Append(c);
            return new CharBufferSegment(this, s, 1);
        }
        public CharBufferSegment NewSpan(char[] charBuffer, int start, int len)
        {
            int s = _arrList.Count;
            int end = start + len;
            for (int i = start; i < end; ++i)
            {
                char c0 = charBuffer[i];
                if (char.IsHighSurrogate(c0) && i + 1 < len)
                {
                    //and not the last one
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
        public CharBufferSegment NewSpan(int[] charBuffer, int start, int len)
        {
            int s = _arrList.Count;
            _arrList.Append(charBuffer, start, len);
            return new CharBufferSegment(this, s, len);
        }
        public CharBufferSegment NewSpan(string str)
        {
            char[] chars = str.ToCharArray();
            return NewSpan(chars, 0, chars.Length);
        }
        public CharBufferSegment NewSegment(TextBufferSpan textspan)
        {
            if (textspan.IsUtf32Buffer)
            {
                return NewSpan(textspan.GetRawUtf32Buffer(), textspan.start, textspan.len);
            }
            else
            {
                return NewSpan(textspan.GetRawUtf16Buffer(), textspan.start, textspan.len);
            }
        }
        public CharBufferSegment NewSegment(ArraySegment<int> textspan)
        {
            return NewSpan(textspan.Array, textspan.Offset, textspan.Count);
        }
        public CharBufferSegment NewSegment(ArraySegment<char> textspan)
        {
            return NewSpan(textspan.Array, textspan.Offset, textspan.Count);
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
