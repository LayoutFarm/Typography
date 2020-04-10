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
    //

    public class WordVisitor
    {

#if DEBUG
        List<BreakAtInfo> dbugBreakAtList = new List<BreakAtInfo>();
#endif
        char[]? _buffer;

        int _startIndex;
        int _endIndex;

        int _currentIndex;
        char _currentChar;
        int _latestBreakAt;


        NewWordBreakHandlerDelegate _newWordBreakHandler;

        Stack<int> _tempCandidateBreaks = new Stack<int>();
        internal WordVisitor(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            _newWordBreakHandler = newWordBreakHandler;
        }

        internal void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        internal void LoadText(char[] buffer, int index, int len)
        {
            //check index < buffer

            //reset all
            _buffer = buffer;
            _endIndex = index + len;

            _startIndex = _currentIndex = index;
            LatestSpanStartAt = _startIndex;

            _currentChar = buffer[_currentIndex];


            _tempCandidateBreaks.Clear();
            _latestBreakAt = 0;

#if DEBUG
            dbugBreakAtList.Clear();
#endif
        }


        public VisitorState State { get; internal set; }
        //
        public int CurrentIndex => _currentIndex;
        //
        public char Char => _currentChar;
        //
        public bool IsEnd => _currentIndex >= _endIndex;
        //

        public string CopyCurrentSpanString()
        {
            if (_buffer == null) throw new InvalidOperationException(nameof(LoadText) + " not called");
            return new string(_buffer, LatestSpanStartAt, LatestSpanLen);
        }

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
            _latestBreakAt = index;

            this.LatestWordKind = wordKind;
            _newWordBreakHandler(this);

#if DEBUG
            dbugBreakAtList.Add(new BreakAtInfo(index, wordKind));
#endif
        }
        internal void AddWordBreakAtCurrentIndex(WordKind wordKind = WordKind.Text)
        {

            AddWordBreakAt(this.CurrentIndex, wordKind);
        }
        //
        public int LatestSpanStartAt { get; private set; }
        public int LatestBreakAt => _latestBreakAt;
        public WordKind LatestWordKind { get; private set; }
        public ushort LatestSpanLen { get; private set; }
        //
        internal void SetCurrentIndex(int index)
        {
            if (_buffer == null) throw new InvalidOperationException(nameof(LoadText) + " not called");
            _currentIndex = index;
            if (index < _endIndex)
            {
                _currentChar = _buffer[index];
            }
            else
            {
                //can't read next
                //the set state= end
                this.State = VisitorState.End;
            }
        }
        internal Stack<int> GetTempCandidateBreaks() => _tempCandidateBreaks;
    }


    public static class WordBreakExtensions
    {
        public static BreakSpan GetBreakSpan(this WordVisitor vis)
        {
            return new BreakSpan()
            {
                startAt = vis.LatestSpanStartAt,
                len = vis.LatestSpanLen,
                wordKind = vis.LatestWordKind
            };
        }
    }
}