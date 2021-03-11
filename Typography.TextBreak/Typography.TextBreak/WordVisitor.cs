//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License




using System;
using System.Collections.Generic;


namespace Typography.TextBreak
{
    public enum VisitorState
    {
        Init,
        Parsing,
        OutOfRangeChar,
        End,
    }

    public delegate void NewWordBreakHandlerDelegate(WordVisitor vistor);

    public class DelegateBaseWordVisitor : WordVisitor
    {
        readonly NewWordBreakHandlerDelegate _newWordBreakHandler;
        internal DelegateBaseWordVisitor(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            _newWordBreakHandler = newWordBreakHandler;
        }
        protected override void OnBreak()
        {
            _newWordBreakHandler(this);
        }
    }

    ref struct BreakBounds
    {
        public int startIndex;
        public int length;
        public WordKind kind;
        public void Consume()
        {
            startIndex += length;
            length = 0;
            kind = WordKind.Unknown;
        }
    }

    public abstract class WordVisitor
    {
        int _startAt;
        int _endIndex;
        int _latestBreakAt;
        InputReader _inputReader;

        readonly Stack<int> _tempCandidateBreaks = new Stack<int>();

        public virtual SpanBreakInfo SpanBreakInfo { get; set; }

        internal void LoadText(int[] buffer, int index, int len)
        {
            //input is utf32  
            _startAt = index;
            _endIndex = index + len;
            _inputReader = new InputReader(buffer, index, len);
            _endIndex = index + len;

            _latestBreakAt = LatestSpanStartAt = index;
            _tempCandidateBreaks.Clear();

        }
        internal void LoadText(char[] buffer, int index, int len)
        {
            _startAt = index;
            _endIndex = index + len;
            _inputReader = new InputReader(buffer, index, len);

            _latestBreakAt = LatestSpanStartAt = index;
            _tempCandidateBreaks.Clear();

        }
        internal void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        protected virtual void OnBreak() { }

        public VisitorState State { get; internal set; }
        //
        public int Offset => _inputReader.Offset;
        //
        public char Char => _inputReader.C0;

        internal int EndIndex => _endIndex;

        public bool IsEnd => _inputReader.IsEnd;

        internal bool Read() => _inputReader.ReadNext();
        internal char C0 => _inputReader.C0;
        internal char C1 => _inputReader.C1;
        internal char PeekNext() => _inputReader.PeekNext();
#if DEBUG
        //int dbugAddSteps;
#endif

        internal void AddWordBreakAt(int offset, WordKind wordKind)
        {
            int actualOffset = offset + _startAt;
#if DEBUG 
            if (actualOffset == _latestBreakAt)
            {
                throw new NotSupportedException();
            }
#endif

            LatestSpanLen = (ushort)(actualOffset - LatestBreakAt);
            LatestSpanStartAt = _latestBreakAt;
            LatestWordKind = wordKind;

            _latestBreakAt = actualOffset;//** 

            OnBreak();

            //#if DEBUG
            //            if (dbugCollectBreakAtList)
            //            {
            //                dbugBreakAtList.Add(new BreakAtInfo(index, wordKind));
            //            }

            //#endif
        }
        internal void AddWordBreakAtCurrentIndex(WordKind wordKind = WordKind.Text)
        {
            AddWordBreakAt(this.Offset, wordKind);
        }
        internal void AddWordBreak_AndSetCurrentIndex(int index, WordKind wordKind)
        {
            AddWordBreakAt(index, wordKind);
            SetCurrentIndex(LatestBreakAt - _startAt);
        }
        //
        public int LatestSpanStartAt { get; private set; }
        public int LatestBreakAt => _latestBreakAt;
        public WordKind LatestWordKind { get; private set; }
        public ushort LatestSpanLen { get; private set; }
        //

        internal void SetCurrentIndex(int offset)
        {
            int actualOffset = offset + _startAt;
            if (actualOffset < _endIndex)
            {
                _inputReader.SetCurrentOffset(offset);
            }
            else
            {
                _inputReader.SetCurrentOffset(_endIndex);
                //can't read next
                //the set state= end
                this.State = VisitorState.End;
            }
        }
        internal Stack<int> GetTempCandidateBreaks() => _tempCandidateBreaks;
    }



    public struct InputReader
    {
        readonly char[] _utf16Buffer;
        readonly int[] _utf32Buffer;
        readonly int _start;
        readonly int _len;
        readonly int _end;

        int _index;//index from start of original buffer
        /// <summary>
        /// inc for next read
        /// </summary>
        int _inc;//inc for next read

        //
        char _c0;
        char _c1;
        public InputReader(char[] input) : this(input, 0, input.Length) { }
        public InputReader(char[] input, int start, int len)
        {
            _utf16Buffer = input;
            _utf32Buffer = null;
            _index = _start = start;
            _len = len;

            _end = start + len;
            _c0 = _c1 = '\0';
            _inc = 0;//default
            ReadCurrentOffset();
        }
        public InputReader(int[] input, int start, int len)
        {
            _utf16Buffer = null;
            _utf32Buffer = input;
            _index = _start = start;
            _len = len;

            _end = start + len;
            _c0 = _c1 = '\0';
            _inc = 0;
            ReadCurrentOffset();
        }

        public int Length => _len;
        /// <summary>
        /// relative offset from start
        /// </summary>
        public int Offset => _index - _start;
        /// <summary>
        /// actual offset from original buffer
        /// </summary>
        public int ActualIndex => _index;

        public bool HasNext => _index >= _end;
        public char PeekNext()
        {
            int next = _index + 1;
            if (next < _end)
            {
                if (_utf32Buffer != null)
                {
                    int value = _utf32Buffer[next];
                    char c_0 = (char)(value >> 16);
                    if (c_0 == '\0')
                    {
                        //use lower
                        return (char)value;
                    }
                    return c_0;
                }
                else
                {
                    //utf16
                    return _utf16Buffer[next];
                }
            }
            return '\0';
        }

        public int Inc => _inc;

        //------------------
        // constants
        const int LEAD_OFFSET = 0xD800 - (0x10000 >> 10);
        const int SURROGATE_OFFSET = 0x10000 - (0xD800 << 10) - 0xDC00;

        public static void SeparateCodePoint(int codepoint, out char c0, out char c1)
        {
            //https://www.unicode.org/faq/utf_bom.html#utf16-1

            //the first snippet calculates the high (or leading) surrogate from a character code C.

            //    const UTF16 HI_SURROGATE_START = 0xD800

            //    UTF16 X = (UTF16) C;
            //    UTF32 U = (C >> 16) & ((1 << 5) - 1);
            //    UTF16 W = (UTF16) U - 1;
            //    UTF16 HiSurrogate = HI_SURROGATE_START | (W << 6) | X >> 10;

            //where X, U and W correspond to the labels used in Table 3-5 UTF-16 Bit Distribution. The next snippet does the same for the low surrogate.

            //    const UTF16 LO_SURROGATE_START = 0xDC00

            //    UTF16 X = (UTF16) C;
            //    UTF16 LoSurrogate = (UTF16) (LO_SURROGATE_START | X & ((1 << 10) - 1));

            //Finally, the reverse, where hi and lo are the high and low surrogate, and C the resulting character

            //    UTF32 X = (hi & ((1 << 6) -1)) << 10 | lo & ((1 << 10) -1);
            //    UTF32 W = (hi >> 6) & ((1 << 5) - 1);
            //    UTF32 U = W + 1;

            //    UTF32 C = U << 16 | X;


            ////from codepoint to upper and lower 
            //ushort lead = (ushort)(LEAD_OFFSET + (utf32_x1 >> 10));
            //ushort trail = (ushort)(0xDC00 + (utf32_x1 & 0x3FF));

            ////compute back
            //int codepoint_x = (lead << 10) + trail + SURROGATE_OFFSET;
            ////UTF32 codepoint = (lead << 10) + trail + SURROGATE_OFFSET;

            //char x1_0 = (char)(utf32_x1 >> 16);
            //char x1_1 = (char)(utf32_x1);

            //static void GetSurrogatePair(int codepoint, out char upper, out char lower)
            //{
            //    upper = (char)(LEAD_OFFSET + (codepoint >> 10));
            //    lower = (char)(0xDC00 + (codepoint & 0x3FF));
            //}

            c0 = (char)(codepoint >> 16);

            if (c0 == '\0')
            {
                c0 = (char)codepoint;
                c1 = '\0';
            }
            else
            {
                c0 = (char)(LEAD_OFFSET + (codepoint >> 10));
                c1 = (char)(0xDC00 + (codepoint & 0x3FF));
            }
        }

        void ReadCurrentOffset()
        {
            if (_index < _end)
            {
                if (_utf32Buffer != null)
                {
                    SeparateCodePoint(_utf32Buffer[_index], out _c0, out _c1);
                    _inc = 1;//inc for next read
                }
                else
                {
                    //utf16
                    _c0 = _utf16Buffer[_index];
                    if (char.IsHighSurrogate(_c0))
                    {
                        if (_index + 1 < _utf16Buffer.Length)
                        {
                            _c1 = _utf16Buffer[_index + 1];
                            if (char.IsLowSurrogate(_c1))
                            {
                                _inc = 2;
                            }
                            else
                            {
                                //ERROR
                                _inc = 1;
                                _c1 = '\0';
                                //error
                            }
                        }
                        else
                        {
                            _c1 = '\0';
                            _inc = 1;
                        }
                    }
                    else
                    {
                        _inc = 1;
                    }
                }
            }
            else
            {
                _c0 = '\0';
                _c1 = '\0';
                _inc = 0;
            }
        }

        public bool ReadNext()
        {
            if (_index + _inc <= _end)
            {
                _index += _inc;
                ReadCurrentOffset();                
                return true;
            }

            return false;
        }

        public enum LineEnd : byte
        {
            None,
            /// <summary>
            /// \r
            /// </summary>
            R,
            /// <summary>
            /// n, new line
            /// </summary>
            N,
            /// <summary>
            /// \r\n
            /// </summary>
            RN
        }


        public bool Readline(out int begin, out int len, out LineEnd endlineWith)
        {
            begin = _index;

            endlineWith = LineEnd.None;
            if (_index + _inc > _end)
            {
                len = 0;
                return false;
            }
            //----------------------
            //_index += _inc;
            //read until found the end of line

            //find       
            do
            {
                int codepoint = (_utf32Buffer != null) ? _utf32Buffer[_index] : _utf16Buffer[_index];
                if (codepoint == '\r')
                {
                    if (_index + 2 <= _end)
                    {
                        int next_codepoint = (_utf32Buffer != null) ? _utf32Buffer[_index + 1] : _utf16Buffer[_index + 1];
                        if (next_codepoint == '\n')
                        {
                            _index++;
                            _inc = 0;

                            len = _index - begin - 1;

                            _index++;
                            endlineWith = LineEnd.RN;
                            return true;
                        }
                        else
                        {
                            _inc = 1;

                            len = _index - begin;
                            endlineWith = LineEnd.R;
                            return true;
                        }
                    }
                    else
                    {
                        _inc = 1;

                        len = _index - begin;
                        endlineWith = LineEnd.R;
                        return true;
                    }
                }
                else if (codepoint == '\n')
                {

                    len = _index - begin;
                    _inc = 1;
                    endlineWith = LineEnd.N;
                    return true;
                }
                else
                {
                    _index++;

                }
            } while (_index < _end);
            //----------------
            if (_index > begin)
            {
                _inc = 1;
                len = _index - begin;
                return true;
            }
            len = 0;
            return false;
        }
        public bool IsEnd => _index >= _end;

        public char C0 => _c0;
        public char C1 => _c1;
        public int Codepoint => (_c1 != '\0') ? char.ConvertToUtf32(_c0, _c1) : _c0;

        public void SetCurrentOffset(int offset)
        {
            _index = _start + offset;
            _inc = 0;
            ReadCurrentOffset();
        }

        //temp fix
        public bool IsUtf32Buffer => _utf32Buffer != null;

        public ArraySegment<int> GetUtf32Segment(int start, int len) => new ArraySegment<int>(_utf32Buffer, start, len);
        public ArraySegment<char> GetUtf16Segment(int start, int len) => new ArraySegment<char>(_utf16Buffer, start, len);
    }

}
