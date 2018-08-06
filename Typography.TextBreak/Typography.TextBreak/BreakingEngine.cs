//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;

namespace Typography.TextBreak
{
    public abstract class BreakingEngine
    {
        internal abstract void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len);
        public abstract bool CanBeStartChar(char c);
        public abstract bool CanHandle(char c);
    }
    public abstract class DictionaryBreakingEngine : BreakingEngine
    {
        public abstract char FirstUnicodeChar { get; }
        public abstract char LastUnicodeChar { get; }
        public override bool CanHandle(char c)
        {
            //in this range or not
            return c >= this.FirstUnicodeChar && c <= this.LastUnicodeChar;
        }
        protected abstract CustomDic CurrentCustomDic { get; }
        protected abstract WordGroup GetWordGroupForFirstChar(char c);



        int _startAt;
        int _len;
        int _endAt;

        public bool DontMergeLastIncompleteWord { get; set; }
        internal override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        {
            visitor.State = VisitorState.Parsing;
            this._startAt = startAt;
            this._len = len;
            this._endAt = startAt + len;

            char c_first = this.FirstUnicodeChar;
            char c_last = this.LastUnicodeChar;
            int endAt = startAt + len;

            Stack<int> candidateBreakList = visitor.GetTempCandidateBreaks();

            for (int i = startAt; i < endAt;)
            {
                //find proper start words;
                char c = charBuff[i];
                //----------------------
                //check if c is in our responsiblity
                if (c < c_first || c > c_last)
                {
                    //out of our range
                    //should return ?
                    visitor.State = VisitorState.OutOfRangeChar;
                    return;
                }
                //----------------------
                WordGroup wordgroup = GetWordGroupForFirstChar(c);
                if (wordgroup == null)
                {
                    //continue next char
                    ++i;
                    visitor.AddWordBreakAt(i, WordKind.Text);
                    visitor.SetCurrentIndex(visitor.LatestBreakAt);
                }
                else
                {
                    //check if we can move next
                    if (visitor.IsEnd)
                    {
                        visitor.State = VisitorState.End;
                        return;
                    }
                    //---------------------
                    WordGroup c_wordgroup = wordgroup;
                    candidateBreakList.Clear();

                    int candidateLen = 1;

                    if (c_wordgroup.PrefixIsWord)
                    {
                        candidateBreakList.Push(candidateLen);
                    }

                    bool continueRead = true;

                    while (continueRead)
                    {
                        //not end
                        //then move next
                        candidateLen++;
                        visitor.SetCurrentIndex(i + 1);
                        if (visitor.IsEnd)
                        {
                            //end  ***
                            visitor.State = VisitorState.End;
                            //----------------------------------------
                            WordGroup next1 = GetSubGroup(visitor, c_wordgroup);

                            bool latest_candidate_isNotWord = false;
                            if (next1 != null)
                            {
                                //accept 

                                //---------------------
                                //since this is end word ...
                                //and next1 != null=> this has a link to next word group
                                //but it may be incomplete so => we need decision ***

                                if (next1.PrefixIsWord)
                                {

                                    candidateBreakList.Push(candidateLen);
                                }
                                else
                                {
                                    if (!DontMergeLastIncompleteWord)
                                    {
                                        latest_candidate_isNotWord = true;//word may has error
                                        candidateBreakList.Push(candidateLen);
                                    }
                                }
                                //---------------------
                            }
                            else
                            {
                                if (c_wordgroup.WordSpanListCount > 0)
                                {
                                    int p1 = visitor.CurrentIndex;
                                    //p2: suggest position
                                    int p2 = FindInWordSpans(visitor, c_wordgroup);
                                    if (p2 - p1 > 0)
                                    {
                                        visitor.AddWordBreakAt(p2, WordKind.Text);
                                        visitor.SetCurrentIndex(p2);
                                        candidateBreakList.Clear();
                                    }
                                }
                            }
                            //----------------------------------------
                            i = endAt; //temp fix, TODO: review here

                            //choose best match 
                            if (candidateBreakList.Count > 0)
                            {

                                int candi1 = candidateBreakList.Pop();
                                //try

                                visitor.SetCurrentIndex(visitor.LatestBreakAt + candi1);
                                if (latest_candidate_isNotWord)
                                {
                                    //use this
                                    //use this candidate if possible
                                    visitor.AddWordBreakAtCurrentIndex(WordKind.TextIncomplete);
                                }
                                else
                                {
                                    //use this
                                    //use this candidate if possible
                                    visitor.AddWordBreakAtCurrentIndex();
                                }

                                break;
                            }
                            continueRead = false;
                            //----------------------------------------
                            return;
                        }
                        WordGroup next = GetSubGroup(visitor, c_wordgroup);
                        //for debug
                        //string prefix = (next == null) ? "" : next.GetPrefix(CurrentCustomDic.TextBuffer);  
                        if (next != null)
                        {

                            if (next.PrefixIsWord)
                            {
                                candidateBreakList.Push(candidateLen);
                            }
                            c_wordgroup = next;
                            i = visitor.CurrentIndex;

                            if (visitor.IsEnd)
                            {

                                i = endAt; //temp fix, TODO: review here

#if DEBUG
                                bool dbugFoundCandidate = false;
#endif
                                //choose best match 
                                while (candidateBreakList.Count > 0)
                                {

                                    int candi1 = candidateBreakList.Pop();
                                    //try
                                    visitor.SetCurrentIndex(visitor.LatestBreakAt + candi1);
                                    if (visitor.State != VisitorState.End)
                                    {
                                        char next_char = visitor.Char;
                                        if (CanBeStartChar(next_char))
                                        {
                                            //use this
                                            //use this candidate if possible
                                            visitor.AddWordBreakAtCurrentIndex();
#if DEBUG
                                            dbugFoundCandidate = true;
#endif
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        visitor.AddWordBreakAtCurrentIndex();
#if DEBUG
                                        dbugFoundCandidate = true;
#endif
                                        break;
                                    }
                                }
                                continueRead = false;
                            }
                        }
                        else
                        {
                            continueRead = false;
                            //no deeper group
                            //then check if 
                            if (c_wordgroup.WordSpanListCount > 0)
                            {
                                int p1 = visitor.CurrentIndex;
                                //p2: suggest position
                                int p2 = FindInWordSpans(visitor, c_wordgroup);
                                if (p2 - p1 > 0)
                                {
                                    visitor.AddWordBreakAt(p2, WordKind.Text);
                                    visitor.SetCurrentIndex(p2);
                                }
                                else
                                {
                                    //on the same pos
                                    if (visitor.State == VisitorState.OutOfRangeChar)
                                    {
                                        visitor.AddWordBreakAtCurrentIndex();
                                        return;
                                    }
                                    else
                                    {
                                        bool foundCandidate = false;
                                        int candi_count = candidateBreakList.Count;
                                        if (candi_count == 0)
                                        {
                                            //no candidate 
                                            //need to step back
                                            int latestBreakAt = visitor.LatestBreakAt;
                                            if (visitor.CurrentIndex - 1 > latestBreakAt)
                                            {
                                                //steop back

                                                visitor.SetCurrentIndex(visitor.CurrentIndex - 1);
                                                char current_char = visitor.Char;
                                                if (CanBeStartChar(current_char))
                                                {

                                                    if (visitor.CurrentIndex - 1 > latestBreakAt)
                                                    {

                                                    }
                                                    else
                                                    {

                                                    }
                                                }
                                                else
                                                {

                                                }

                                            }
                                            else
                                            {
                                                throw new NotSupportedException("i-3311");
                                            }
                                        }
                                        else
                                        {
                                            while (candidateBreakList.Count > 0)
                                            {
                                                int candi1 = candidateBreakList.Pop();
                                                //try
                                                visitor.SetCurrentIndex(visitor.LatestBreakAt + candi1);
                                                //check if we can use this candidate
                                                if (visitor.State != VisitorState.End)
                                                {
                                                    char next_char = visitor.Char;
                                                    if (CanBeStartChar(next_char))
                                                    {
                                                        //use this
                                                        //use this candidate if possible
                                                        visitor.AddWordBreakAtCurrentIndex();
                                                        foundCandidate = true;
                                                        break;
                                                    }
                                                }
                                                else
                                                {
                                                    visitor.AddWordBreakAtCurrentIndex();
                                                    foundCandidate = true;
                                                }
                                            }
                                        }
                                        if (!foundCandidate)
                                        {
                                            //no next word, no candidate
                                            //skip this 
                                            char next_char = visitor.Char;
                                            if (CanBeStartChar(next_char))
                                            {
                                                //use this
                                                //use this candidate if possible
                                                visitor.AddWordBreakAtCurrentIndex();
                                                foundCandidate = true;
                                                break;
                                            }
                                            else
                                            {
                                                //TODO: review here
                                                visitor.SetCurrentIndex(visitor.LatestBreakAt + 1);
                                                visitor.AddWordBreakAtCurrentIndex();
                                                visitor.SetCurrentIndex(visitor.LatestBreakAt);
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {

                                bool foundCandidate = false;
                                while (candidateBreakList.Count > 0)
                                {

                                    int candi1 = candidateBreakList.Pop();
                                    //try
                                    visitor.SetCurrentIndex(visitor.LatestBreakAt + candi1);
                                    if (visitor.State == VisitorState.End)
                                    {
                                        visitor.AddWordBreakAtCurrentIndex();
                                        return;
                                    }
                                    //check if we can use this candidate
                                    char next_char = visitor.Char;
                                    if (!CanHandle(next_char))
                                    {
                                        //use this
                                        //use this candidate if possible
                                        visitor.AddWordBreakAtCurrentIndex();
                                        foundCandidate = true;
                                        break;
                                    }
                                    if (CanBeStartChar(next_char))
                                    {
                                        //use this
                                        //use this candidate if possible
                                        visitor.AddWordBreakAtCurrentIndex();
                                        foundCandidate = true;
                                        break;
                                    }
                                }
                                if (!foundCandidate)
                                {
                                    if (candidateLen > 0)
                                    {
                                        //use that candidate len
                                        visitor.AddWordBreakAtCurrentIndex();
                                        visitor.SetCurrentIndex(visitor.LatestBreakAt);
                                    }
                                }

                            }
                            i = visitor.CurrentIndex;
                        }
                    }
                }
            }
            //------
            if (visitor.CurrentIndex >= len - 1)
            {
                //the last one 
                visitor.State = VisitorState.End;
            }
        }
        internal WordGroup GetSubGroup(WordVisitor visitor, WordGroup wordGroup)
        {

            char c = visitor.Char;
            if (!CanHandle(c))
            {
                //can't handle
                //then no furtur sub group
                visitor.State = VisitorState.OutOfRangeChar;
                return null;
            }
            //-----------------
            //can handle 
            WordGroup[] subGroups = wordGroup.GetSubGroups();
            if (subGroups != null)
            {
                return subGroups[c - this.FirstUnicodeChar];
            }
            return null;
        }

        int FindInWordSpans(WordVisitor visitor, WordGroup wordGroup)
        {
            WordSpan[] wordSpans = wordGroup.GetWordSpans();
            if (wordSpans == null)
            {
                throw new NotSupportedException();
            }

            //at this wordgroup
            //no subground anymore
            //so we should find the word one by one
            //start at prefix
            //and select the one that 

            int readLen = visitor.CurrentIndex - visitor.LatestBreakAt;
            int nwords = wordSpans.Length;
            //only 1 that match 

            CustomDicTextBuffer currentTextBuffer = CurrentCustomDic.TextBuffer;

            //we sort unindex string ***
            //so we find from longest one( last) to begin 
            for (int i = nwords - 1; i >= 0; --i)
            {
                //loop test on each word
                WordSpan w = wordSpans[i];
#if DEBUG
                //string dbugstr = w.GetString(currentTextBuffer);
#endif

                int savedIndex = visitor.CurrentIndex;
                char c = visitor.Char;
                int wordLen = w.len;
                int matchCharCount = 0;
                if (wordLen > readLen)
                {
                    for (int p = readLen; p < wordLen; ++p)
                    {
                        char c2 = w.GetChar(p, currentTextBuffer);
                        if (c2 == c)
                        {
                            matchCharCount++;
                            //match 
                            //read next
                            if (!visitor.IsEnd)
                            {
                                visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                                c = visitor.Char;
                            }
                            else
                            {
                                //no more data in visitor

                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                //reset
                if (readLen + matchCharCount == wordLen)
                {
                    int newBreakAt = visitor.LatestBreakAt + wordLen;
                    visitor.SetCurrentIndex(newBreakAt);
                    //-------------------------------------------- 
                    if (visitor.State == VisitorState.End)
                    {
                        return newBreakAt;
                    }
                    //check next char can be the char of new word or not
                    //this depends on each lang 
                    char canBeStartChar = visitor.Char;
                    if (CanHandle(canBeStartChar))
                    {
                        if (CanBeStartChar(canBeStartChar))
                        {
                            return newBreakAt;
                        }
                        else
                        {
                            //back to savedIndex
                            visitor.SetCurrentIndex(savedIndex);
                            return savedIndex;
                        }
                    }
                    else
                    {
                        visitor.State = VisitorState.OutOfRangeChar;
                        return newBreakAt;
                    }
                }
                visitor.SetCurrentIndex(savedIndex);
            }
            return 0;
        }
    }



}