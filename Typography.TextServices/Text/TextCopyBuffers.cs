//MIT, 2014-present, WinterDev

using System;

using System.Text;
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

        public abstract int Length { get; }
        public abstract void Clear();
        public abstract void Append(char[] buffer, int start, int len);
        public abstract void Append(int[] buffer, int start, int len);
        public abstract unsafe void Append(char* buffer, int len);
        //
        public abstract void CopyTo(TextBuilder<int> output);
        public abstract void CopyTo(StringBuilder stbuilder);
        public abstract void CopyTo(StringBuilder stbuilder, int startIndex, int len);
        //
        public abstract void GetReader(out InputReader reader);

        public void Append(TextBufferSpan span)
        {

        }
        public void Append(TextBufferSpan span, int start, int len)
        {

        }
    }

    public sealed class TextCopyBufferUtf16 : TextCopyBuffer
    {
        readonly TextBuilder<char> _utf16Buffer = new TextBuilder<char>();
        public TextCopyBufferUtf16() { }
        public TextCopyBufferUtf16(char[] buffer)
        {
            Append(buffer, 0, buffer.Length);
        }
        public override BackupBufferKind Kind => BackupBufferKind.Utf16ArrayList;

        public override int Length => _utf16Buffer.Count;
        public override void Clear()
        {
            _utf16Buffer.Clear();
        }

        public override unsafe void Append(char* buffer, int len)
        {
            for (int i = 0; i < len; ++i)
            {
                _utf16Buffer.Append(*buffer);
                buffer++;//move next
            }
        }
        public override void Append(char[] buffer, int start, int len)
        {
            _utf16Buffer.Append(buffer, start, len);
        }
        public override void Append(int[] buffer, int start, int len)
        {
            int end = start + len;
            for (int i = start; i < end; ++i)
            {
                int d = buffer[i];
                char upper = (char)(d >> 16);
                if (upper > 0)
                {
                    InputReader.SeparateCodePoint(buffer[i], out upper, out char lower);
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
        public override void CopyTo(TextBuilder<int> output)
        {
            //from utf16 to utf32
            int end = _utf16Buffer.Count;
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
            stbuilder.Append(_utf16Buffer.UnsafeInternalArray, 0, _utf16Buffer.Count);
        }
        public override void CopyTo(StringBuilder stbuilder, int startIndex, int len)
        {
            //from utf16 to utf32
            stbuilder.Append(_utf16Buffer.UnsafeInternalArray, startIndex, len);
        }
        public override void GetReader(out InputReader reader)
        {
            reader = new InputReader(_utf16Buffer.UnsafeInternalArray, 0, _utf16Buffer.Count);
        }

        public static char[] UnsafeGetInternalArray(TextCopyBufferUtf16 copyBuffer)
        {
            return copyBuffer._utf16Buffer.UnsafeInternalArray;
        }
    }

    public sealed class TextCopyBufferUtf32 : TextCopyBuffer
    {
        //**reuseable** copy buffer

        readonly TextBuilder<int> _utf32Buffer = new TextBuilder<int>();
        public TextCopyBufferUtf32() { }
        public override BackupBufferKind Kind => BackupBufferKind.Utf32ArrayList;
        public TextCopyBufferUtf32(char[] buffer)
        {
            Append(buffer, 0, buffer.Length);
        }
        public override int Length => _utf32Buffer.Count;


        public void Append(int c) => _utf32Buffer.Append(c);

        public override unsafe void Append(char* buffer, int len)
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
                    ++i;
                }
                else
                {
                    _utf32Buffer.Append(c);
                }
                buffer++;
            }
        }
        public override void Append(char[] buffer, int start, int len)
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

        public override void Append(int[] buffer, int start, int len)
        {
            _utf32Buffer.Append(buffer, start, len);
        }
        public void Append(TextBuilder<int> buffer, int start, int len)
        {
            _utf32Buffer.Append(buffer, start, len);
        }
        public void Append(TextCopyBufferUtf32 src)
        {
            _utf32Buffer.Append(src._utf32Buffer, 0, src.Length);
        }
        public void Append(TextCopyBufferUtf32 src, int start, int len)
        {
            _utf32Buffer.Append(src._utf32Buffer, start, len);
        }

        public int GetChar(int index)
        {
            //TODO: review here
            return _utf32Buffer[index];
        }
        public void Insert(int index, int c) => _utf32Buffer.Insert(index, c);
        public void Insert(TextCopyBufferUtf32 src, int dstStart, int srcStart, int srcLen)
        {
            _utf32Buffer.Insert(dstStart, src._utf32Buffer, srcStart, srcLen);
        }
        public override void Clear()
        {
            _utf32Buffer.Clear();
        }
        public void Remove(int index) => _utf32Buffer.Remove(index, 1);
        public void Remove(int index, int len) => _utf32Buffer.Remove(index, len);
        public void CopyTo(TextCopyBufferUtf32 output)
        {
            //append to output
            output._utf32Buffer.Append(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Count);
        }
        public void CopyTo(TextBuilder<int> output, int dstStart, int srcStart, int srcLen)
        {
            output.Insert(dstStart, _utf32Buffer.UnsafeInternalArray, srcStart, srcLen);
        }
        public override void CopyTo(TextBuilder<int> output)
        {
            //append to source
            output.Append(_utf32Buffer, 0, _utf32Buffer.Count);
        }
        public override void CopyTo(StringBuilder stbuilder)
        {
            int j = _utf32Buffer.Count;
            for (int i = 0; i < j; ++i)
            {
                int codepoint = _utf32Buffer[i];
                if (((codepoint) >> 16) != 0)
                {
                    InputReader.SeparateCodePoint(codepoint, out char c0, out char c1);
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
                    InputReader.SeparateCodePoint(codepoint, out char c0, out char c1);
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
            reader = new InputReader(_utf32Buffer.UnsafeInternalArray, 0, _utf32Buffer.Count);
        }

        /// <summary>
        /// copy from internal buffer to output
        /// </summary>
        /// <param name="output"></param>
        /// <param name="dstBegin"></param>
        /// <param name="srcBegin"></param>
        /// <param name="srcLen"></param>
        public void CopyTo(System.Collections.Generic.List<int> output, int dstBegin, int srcBegin, int srcLen)
        {
            if (output.Count == dstBegin)
            {
                //append last
                for (int i = 0; i < srcLen; ++i)
                {
                    output.Add(_utf32Buffer[srcBegin + i]);
                }
            }
            else
            {
                for (int i = 0; i < srcLen; ++i)
                {
                    //insert range***
                    output.Insert(dstBegin, _utf32Buffer[srcBegin + i]);
                    dstBegin++;
                }
            }
        }

        public static int[] UnsafeGetInternalArray(TextCopyBufferUtf32 copyBuffer)
        {

            return copyBuffer._utf32Buffer.UnsafeInternalArray;
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
                    buff.Append(c, data.Length);
                }
            }
        }
        public static void AppendData(this TextCopyBuffer buff, string data)
        {
            unsafe
            {
                fixed (char* c = data)
                {
                    buff.Append(c, data.Length);
                }
            }
        }
    }

}