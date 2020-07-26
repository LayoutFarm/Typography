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

        //#if DEBUG
        //        List<BreakAtInfo> dbugBreakAtList = new List<BreakAtInfo>();
        //        bool dbugCollectBreakAtList;
        //#endif
        char[] _utf16Buffer;
        int[] _utf32Buffer;

        int _startIndex;
        int _endIndex;

        int _currentIndex;
        char _currentChar; //store as utf32
        int _latestBreakAt;

        readonly Stack<int> _tempCandidateBreaks = new Stack<int>();
        public virtual SpanBreakInfo SpanBreakInfo { get; set; }
        internal void LoadText(int[] buffer, int index, int len)
        {
            //input is utf32  
            _utf16Buffer = null;
            _utf32Buffer = buffer;

            _endIndex = index + len;

            _startIndex = _currentIndex = index;
            _latestBreakAt = LatestSpanStartAt = _startIndex;

            //in this case, the current char is upper 
            int c1 = buffer[_currentIndex];

            char upper = (char)(c1 >> 16);
            char lower = (char)c1;

            if (upper == '\0')
            {
                //use lower
                _currentChar = lower;
            }
            else
            {
                _currentChar = upper;
            }


            _tempCandidateBreaks.Clear();
        }
        internal void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        internal void LoadText(char[] buffer, int index, int len)
        {

            //input is utf16
            //reset all
            _utf16Buffer = buffer;
            _utf32Buffer = null;

            _endIndex = index + len;

            _startIndex = _currentIndex = index;
            _latestBreakAt = LatestSpanStartAt = _startIndex;


            _currentChar = buffer[_currentIndex];
            _tempCandidateBreaks.Clear();
        }
        protected virtual void OnBreak() { }

        public VisitorState State { get; internal set; }
        //
        public int CurrentIndex => _currentIndex;
        //
        public char Char => _currentChar;

        //
        public bool IsEnd => _currentIndex >= _endIndex;

        public string CopyCurrentSpanString()
        {
            return new string(_utf16Buffer, LatestSpanStartAt, LatestSpanLen);
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
            LatestWordKind = wordKind;

            _latestBreakAt = index;//**

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
        }
        //
        public int LatestSpanStartAt { get; private set; }
        public int LatestBreakAt => _latestBreakAt;
        public WordKind LatestWordKind { get; private set; }
        public ushort LatestSpanLen { get; private set; }
        //

        internal void SetCurrentIndex(int index)
        {
            _currentIndex = index;
            if (index < _endIndex)
            {
                if (_utf16Buffer != null)
                {
                    _currentChar = _utf16Buffer[index];
                }
                else
                {
                    int c1 = _utf32Buffer[_currentIndex];

                    char upper = (char)(c1 >> 16);
                    char lower = (char)c1;

                    if (upper == '\0')
                    {
                        //use lower
                        _currentChar = lower;
                    }
                    else
                    {
                        _currentChar = upper;
                    }
                }

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



}