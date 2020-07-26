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

    public abstract class WordVisitor
    {
        int _endIndex;
        int _latestBreakAt;
        InputReader _inputReader;

        readonly Stack<int> _tempCandidateBreaks = new Stack<int>();

        public virtual SpanBreakInfo SpanBreakInfo { get; set; }

        internal void LoadText(int[] buffer, int index, int len)
        {
            //input is utf32  
            _inputReader = new InputReader(buffer, index, len);
            _endIndex = index + len;
            _latestBreakAt = LatestSpanStartAt = index;
            _tempCandidateBreaks.Clear();
        }
        internal void LoadText(char[] buffer, int index, int len)
        {
            _inputReader = new InputReader(buffer, index, len);
            _endIndex = index + len;
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
        public int CurrentIndex => _inputReader.Index;
        //
        public char Char => _inputReader.C0;
        public int StartIndex => _inputReader.StartAt;
        internal int EndIndex => _endIndex;

        public bool IsEnd => _inputReader.IsEnd;

        internal bool Read() => _inputReader.Read();
        internal char C0 => _inputReader.C0;
        internal char C1 => _inputReader.C1;
        internal char PeekNext() => _inputReader.PeekNext();
        internal void PauseNextRead() => _inputReader.PauseNextRead();

#if DEBUG
        //int dbugAddSteps;
#endif

        internal void AddWordBreakAt(int index, WordKind wordKind)
        {

#if DEBUG
            //dbugAddSteps++;
            //if (dbugAddSteps >= 57)
            //{

            //}
            if (index == _latestBreakAt)
            {
                throw new NotSupportedException();
            }
#endif

            LatestSpanLen = (ushort)(index - LatestBreakAt);
            LatestSpanStartAt = _latestBreakAt;
            LatestWordKind = wordKind;


            _latestBreakAt = index;//**

            //if (_latestBreakAt == 243)
            //{

            //}

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
            AddWordBreakAt(this.CurrentIndex, wordKind);
        }
        internal void AddWordBreak_AndSetCurrentIndex(int index, WordKind wordKind)
        {
            AddWordBreakAt(index, wordKind);
            SetCurrentIndex(LatestBreakAt);
            PauseNextRead();
        }
        //
        public int LatestSpanStartAt { get; private set; }
        public int LatestBreakAt => _latestBreakAt;
        public WordKind LatestWordKind { get; private set; }
        public ushort LatestSpanLen { get; private set; }
        //

        internal void SetCurrentIndex(int index)
        {
            if (index < _endIndex)
            {
                _inputReader.SetCurrentIndex(index);
            }
            else
            {
                _inputReader.SetCurrentIndex(_endIndex);
                //can't read next
                //the set state= end
                this.State = VisitorState.End;
            }
        }
        internal Stack<int> GetTempCandidateBreaks() => _tempCandidateBreaks;
    }

    struct InputReader
    {
        readonly char[] _utf16Buffer;
        readonly int[] _utf32Buffer;
        int _start;
        int _len;
        int _end;
        int _index;
        int _inc;

        //
        char _c0;
        char _c1;

        public InputReader(char[] input, int start, int len)
        {
            _utf16Buffer = input;
            _utf32Buffer = null;
            _start = start;
            _len = len;
            _index = start;
            _end = start + len;
            _c0 = _c1 = '\0';
            _inc = 0;
            ReadCurrentIndex();
        }

        public InputReader(int[] input, int start, int len)
        {
            _utf16Buffer = null;
            _utf32Buffer = input;
            _start = start;
            _len = len;
            _index = start;
            _end = start + len;
            _c0 = _c1 = '\0';
            _inc = 0;
            ReadCurrentIndex();
        }
        public void SetNewReadingRange(int start, int len)
        {
            _start = start;
            _len = len;
            _index = start;
            _end = start + len;
            _c0 = _c1 = '\0';
            _inc = 0;
            ReadCurrentIndex();
        }
        public int StartAt => _start;

        public int Length => _len;
        public int Index => _index;
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

        //------------------
        // constants
        const int LEAD_OFFSET = 0xD800 - (0x10000 >> 10);
        const int SURROGATE_OFFSET = 0x10000 - (0xD800 << 10) - 0xDC00;

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

        void ReadCurrentIndex()
        {
            if (_index < _end)
            {
                if (_utf32Buffer != null)
                {
                    int codepoint = _utf32Buffer[_index];
                    //GetSurrogatePair(_utf32Buffer[_index], out _c0, out _c1);
                    _c0 = (char)(codepoint >> 16);

                    if (_c0 == '\0')
                    {
                        _c0 = (char)codepoint;
                        _c1 = '\0';
                    }
                    else
                    {
                        _c0 = (char)(LEAD_OFFSET + (codepoint >> 10));
                        _c1 = (char)(0xDC00 + (codepoint & 0x3FF));
                    }
                    _inc = 1;
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
                                throw new NotSupportedException();
                                //error
                            }
                        }
                        else
                        {
                            _c1 = '\0';
                            _inc = 0;
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
            }
        }

        public bool Read()
        {
            if (_index + _inc <= _end)
            {
                _index += _inc;
                ReadCurrentIndex();
                return true;
            }
            return false;
        }
        public void PauseNextRead()
        {
            _inc = 0;
        }
        public bool IsEnd => _index >= _end;

        public char C0 => _c0;
        public char C1 => _c1;

        public void SetCurrentIndex(int index)
        {
            _index = _start + index;
            _inc = 0;
            ReadCurrentIndex();
        }
    }

}
