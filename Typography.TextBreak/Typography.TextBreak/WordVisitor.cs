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
        CustomBreaker ownerBreak;
        //
        List<BreakAtInfo> breakAtList = new List<BreakAtInfo>();

        //
        char[] buffer;
        int bufferLen;
        int startIndex;
        int currentIndex;
        char currentChar;
        int latestBreakAt;

        Stack<int> tempCandidateBreaks = new Stack<int>();


        public WordVisitor(CustomBreaker ownerBreak)
        {
            this.ownerBreak = ownerBreak;
        }
        public void LoadText(char[] buffer, int index)
        {
            this.buffer = buffer;
            this.bufferLen = buffer.Length;
            this.startIndex = currentIndex = index;
            this.currentChar = buffer[currentIndex];
            breakAtList.Clear();
            latestBreakAt = 0;
        }
        public VisitorState State
        {
            get;
            set;
        }
        public int CurrentIndex
        {
            get { return this.currentIndex; }
        }
        public char Char
        {
            get { return currentChar; }
        }



        public bool IsEnd
        {
            get { return currentIndex >= bufferLen - 1; }
        }
        public void AddWordBreakAt(int index, WordKind wordKind)
        {

#if DEBUG
            if (index == latestBreakAt)
            {
                throw new NotSupportedException();
            }
#endif
            this.latestBreakAt = index;

            breakAtList.Add(new BreakAtInfo(index, wordKind));
        }
        public void AddWordBreakAtCurrentIndex(WordKind wordKind = WordKind.Text)
        {
            AddWordBreakAt(this.CurrentIndex, wordKind);
        }

        public int LatestBreakAt
        {
            get { return this.latestBreakAt; }
        }
        public void SetCurrentIndex(int index)
        {
            this.currentIndex = index;
            if (index < buffer.Length)
            {
                currentChar = buffer[index];
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
            return breakAtList;
        }

        internal Stack<int> GetTempCandidateBreaks()
        {
            return this.tempCandidateBreaks;
        }


    }

}