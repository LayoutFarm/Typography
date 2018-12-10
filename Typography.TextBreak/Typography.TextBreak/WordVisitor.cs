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


    public delegate void NewWordBreakHandlerDelegate(int pos, WordKind wordKind);
    //
    class WordVisitor
    {

#if DEBUG
        List<BreakAtInfo> dbugBreakAtList = new List<BreakAtInfo>();
#endif
        char[] _buffer;

        int _startIndex;
        int _endIndex;

        int _currentIndex;
        char _currentChar;
        int _latestBreakAt;

        
        NewWordBreakHandlerDelegate _newWordBreakHandler;

        Stack<int> _tempCandidateBreaks = new Stack<int>();
        public WordVisitor(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            _newWordBreakHandler = newWordBreakHandler;
        }

        public void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        public void LoadText(char[] buffer, int index, int len)
        {
            //check index < buffer

            //reset all
            _buffer = buffer;
            _endIndex = index + len;

            _startIndex = _currentIndex = index;
            _currentChar = buffer[_currentIndex];


            _tempCandidateBreaks.Clear();
            _latestBreakAt = 0;

#if DEBUG
            dbugBreakAtList.Clear();
#endif
        }


        public VisitorState State { get; set; }
        //
        public int CurrentIndex => _currentIndex;
        //
        public char Char => _currentChar;
        //
        public bool IsEnd => _currentIndex >= _endIndex;
        //

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


            _latestBreakAt = index;
            _newWordBreakHandler(index, wordKind);

#if DEBUG
            dbugBreakAtList.Add(new BreakAtInfo(index, wordKind));
#endif
        }
        public void AddWordBreakAtCurrentIndex(WordKind wordKind = WordKind.Text)
        {
            AddWordBreakAt(this.CurrentIndex, wordKind);
        }
        //
        public int LatestBreakAt => _latestBreakAt;
        //
        public void SetCurrentIndex(int index)
        {
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
        //
        public List<BreakAtInfo> GetBreakList() => dbugBreakAtList;
        //
        internal Stack<int> GetTempCandidateBreaks() => _tempCandidateBreaks;
    }

}