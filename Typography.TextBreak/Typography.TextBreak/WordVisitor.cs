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



    class WordVisitor
    {
        List<BreakAtInfo> _breakAtList = new List<BreakAtInfo>();
        char[] _buffer;

        int _startIndex;
        int _endIndex;

        int _currentIndex;
        char _currentChar;
        int _latestBreakAt;


        Stack<int> _tempCandidateBreaks = new Stack<int>();
        public WordVisitor()
        {
        }

        public void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        public void LoadText(char[] buffer, int index, int len)
        {
            //check index < buffer

            //reset all
            this._buffer = buffer;
            this._endIndex = index + len;

            this._startIndex = _currentIndex = index;
            this._currentChar = buffer[_currentIndex];
            _breakAtList.Clear();
            _tempCandidateBreaks.Clear();
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
            get { return _currentIndex >= _endIndex; }
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
        public List<BreakAtInfo> GetBreakList()
        {
            return _breakAtList;
        }
        internal Stack<int> GetTempCandidateBreaks()
        {
            return this._tempCandidateBreaks;
        }


    }

}