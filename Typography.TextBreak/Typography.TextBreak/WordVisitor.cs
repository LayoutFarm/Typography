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



    ref struct WordVisitor
    {
        ICollection<BreakAtInfo> _breakAtList;
        ReadOnlySpan<char> _buffer;

        int _currentIndex;
        char _currentChar;
        int _latestBreakAt;


        Stack<int> _tempCandidateBreaks;
        public WordVisitor(ReadOnlySpan<char> buffer, ICollection<BreakAtInfo> breakAtList, Stack<int> tempCandidateBreaks)
        {
            _buffer = buffer;
            _breakAtList = breakAtList;
            _tempCandidateBreaks = tempCandidateBreaks;
            _currentIndex = 0;
            _currentChar = buffer[0];
            State = VisitorState.Init;
            _latestBreakAt = 0;
        }

        public VisitorState State
        {
            get;
            set;
        }
        public int CurrentIndex
        {
            get { return this._currentIndex; }
        }
        public char Char
        {
            get { return _currentChar; }
        }
        public bool IsEnd
        {
            get { return _currentIndex >= _buffer.Length; }
        }


#if DEBUG
        //int dbugAddSteps;
#endif
        public void AddWordBreakAt(int index, WordKind wordKind)
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


            this._latestBreakAt = index;

            _breakAtList.Add(new BreakAtInfo(index, wordKind));
        }
        public void AddWordBreakAtCurrentIndex(WordKind wordKind = WordKind.Text)
        {
            AddWordBreakAt(this.CurrentIndex, wordKind);
        }
        public int LatestBreakAt
        {
            get { return this._latestBreakAt; }
        }
        public void SetCurrentIndex(int index)
        {
            this._currentIndex = index;
            if (index < _buffer.Length)
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
        public ICollection<BreakAtInfo> GetBreakList()
        {
            return _breakAtList;
        }
        internal Stack<int> GetTempCandidateBreaks()
        {
            return this._tempCandidateBreaks;
        }


    }

}