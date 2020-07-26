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
        internal void LoadText(char[] buffer, int index)
        {
            LoadText(buffer, index, buffer.Length);
        }
        internal void LoadText(char[] buffer, int index, int len)
        {
            _inputReader = new InputReader(buffer, index, len);
            _endIndex = index + len;
            _latestBreakAt = LatestSpanStartAt = index;

            _tempCandidateBreaks.Clear();
        }

        protected virtual void OnBreak() { }

        public VisitorState State { get; internal set; }
        //
        public int CurrentIndex => _inputReader.Index;
        //
        public char Char => _inputReader.C0;

        //
        public bool IsEnd => _inputReader.IsEnd;
        internal bool Read() => _inputReader.Read();
        internal char C0 => _inputReader.C0;
        internal char C1 => _inputReader.C1;
        internal char PeekNext() => _inputReader.PeekNext();
        internal void PauseNextRead() => _inputReader.PauseNextRead();

        //public string CopyCurrentSpanString()
        //{
        //    return new string(_utf16Buffer, LatestSpanStartAt, LatestSpanLen);
        //}

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

                //can't read next
                //the set state= end
                this.State = VisitorState.End;
            }
        }
        internal Stack<int> GetTempCandidateBreaks() => _tempCandidateBreaks;
    }



}
