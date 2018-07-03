//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;
using System.Text;

namespace Typography.TextBreak
{
    /// <summary>
    /// my custom dic
    /// </summary>
    public class CustomDic
    {
        CustomDicTextBuffer textBuffer;
        WordGroup[] wordGroups;
        char firstChar, lastChar;

        internal CustomDicTextBuffer TextBuffer { get { return textBuffer; } }
        public void SetCharRange(char firstChar, char lastChar)
        {
            this.firstChar = firstChar;
            this.lastChar = lastChar;
        }
        public char FirstChar { get { return firstChar; } }
        public char LastChar { get { return lastChar; } }


        public void LoadSortedUniqueWordList(IEnumerable<string> sortedWordList)
        {
            // load unique and sorted word list
            if (textBuffer != null)
            {
                return;
            }
            if (firstChar == '\0' || lastChar == '\0')
            {
                throw new NotSupportedException();
            }

            //---------------
            Dictionary<char, DevelopingWordGroup> wordGroups = new Dictionary<char, DevelopingWordGroup>();
            textBuffer = new CustomDicTextBuffer(1024);
            foreach (string line in sortedWordList)
            {
                char[] lineBuffer = line.Trim().ToCharArray();
                int lineLen = lineBuffer.Length;
                char c0;
                if (lineLen > 0 && (c0 = lineBuffer[0]) != '#')
                {
                    int startAt = textBuffer.CurrentPosition;
                    textBuffer.AddWord(lineBuffer);

#if DEBUG
                    if (lineLen > byte.MaxValue)
                    {
                        throw new NotSupportedException();
                    }
#endif

                    WordSpan wordspan = new WordSpan(startAt, (byte)lineLen);
                    //each wordgroup contains text span

                    DevelopingWordGroup found;
                    if (!wordGroups.TryGetValue(c0, out found))
                    {
                        found = new DevelopingWordGroup(new WordSpan(startAt, 1));
                        wordGroups.Add(c0, found);
                    }
                    found.AddWordSpan(wordspan);
                }
                //- next line
            }
            //------------------------------------------------------------------
            textBuffer.Freeze();
            //------------------------------------------------------------------ 
            //do index
            DoIndex(wordGroups);

            //clear, not used
            wordGroups.Clear();

        }

        int TransformCharToIndex(char c)
        {
            return c - this.firstChar;
        }
        void DoIndex(Dictionary<char, DevelopingWordGroup> wordGroups)
        {
            //1. expand word group
            WordGroup[] newWordGroups = new WordGroup[this.lastChar - this.firstChar + 1];

            foreach (var kp in wordGroups)
            {
                //for each dev word group
                int index = TransformCharToIndex(kp.Key);
                DevelopingWordGroup devWordGroup = kp.Value;
                devWordGroup.DoIndex(this.textBuffer, this);
                newWordGroups[index] = devWordGroup.ResultWordGroup;
            }
            this.wordGroups = newWordGroups;
        }
        public void GetWordList(char startWithChar, List<string> output)
        {
            if (startWithChar >= firstChar && startWithChar <= lastChar)
            {
                //in range 
                WordGroup found = this.wordGroups[TransformCharToIndex(startWithChar)];
                if (found != null)
                {
                    //iterate and collect into 
                    found.CollectAllWords(this.textBuffer, output);
                }
            }
        }
        internal WordGroup GetWordGroupForFirstChar(char c)
        {
            if (c >= firstChar && c <= lastChar)
            {
                //in range
                return this.wordGroups[TransformCharToIndex(c)];
            }
            return null;
        }
    }


    struct WordSpan
    {
        public readonly int startAt;
        public readonly byte len;

        public WordSpan(int startAt, byte len)
        {
            this.startAt = startAt;
            this.len = len;
        }
        public char GetChar(int index, CustomDicTextBuffer textBuffer)
        {
            return textBuffer.GetChar(startAt + index);
        }
        public string GetString(CustomDicTextBuffer textBuffer)
        {
            return textBuffer.GetString(startAt, len);
        }
        public bool SameTextContent(WordSpan another, CustomDicTextBuffer textBuffer)
        {
            if (another.len == this.len)
            {
                for (int i = another.len - 1; i >= 0; --i)
                {
                    if (this.GetChar(i, textBuffer) != another.GetChar(i, textBuffer))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }



    public struct BreakSpan
    {
        public int startAt;
        public ushort len;
        public WordKind wordKind;
    }

    class DevelopingWordGroup
    {
        List<WordSpan> wordSpanList = new List<WordSpan>();
        DevelopingWordGroup[] subGroups;
        WordSpan prefixSpan;
        internal DevelopingWordGroup(WordSpan prefixSpan)
        {
            this.prefixSpan = prefixSpan;
        }


#if DEBUG
        public enum debugDataState : byte
        {
            UnIndex,
            Indexed,
            TooLongPrefix,
            SmallAmountOfMembers
        }
        static int debugTotalId;
        int debugId = debugTotalId++;
        public static int DebugTotalId { get { return debugTotalId; } }
        debugDataState dbugDataState;
#endif

        internal string GetPrefix(CustomDicTextBuffer buffer)
        {
            return prefixSpan.GetString(buffer);
        }
        internal bool PrefixIsWord
        {
            get;
            private set;
        }
        internal void CollectAllWords(CustomDicTextBuffer textBuffer, List<string> output)
        {
            if (this.PrefixIsWord)
            {
                output.Add(GetPrefix(textBuffer));
            }
            if (subGroups != null)
            {
                foreach (DevelopingWordGroup wordGroup in subGroups)
                {
                    if (wordGroup != null)
                    {
                        wordGroup.CollectAllWords(textBuffer, output);
                    }
                }
            }
            if (wordSpanList != null)
            {
                foreach (var span in wordSpanList)
                {
                    output.Add(span.GetString(textBuffer));
                }
            }
        }
        public int PrefixLen { get { return this.prefixSpan.len; } }

        internal void AddWordSpan(WordSpan span)
        {
            wordSpanList.Add(span);
#if DEBUG
            dbugDataState = debugDataState.UnIndex;
#endif
        }
        public int WordSpanListCount
        {
            get
            {

                if (wordSpanList == null) return 0;
                return wordSpanList.Count;
            }
        }
        WordGroup _resultWordGroup;//after call DoIndex()
        internal void DoIndex(CustomDicTextBuffer textBuffer, CustomDic owner)
        {

            //recursive
            if (this.PrefixLen > 7)
            {
                DoIndexOfSmallAmount(textBuffer);
#if DEBUG
                dbugDataState = debugDataState.TooLongPrefix;
#endif
                return;
            }
            //-----------------------------------------------

            bool hasEvalPrefix = false;
            if (subGroups == null)
            {
                subGroups = new DevelopingWordGroup[owner.LastChar - owner.FirstChar + 1];
            }
            //--------------------------------
            int j = wordSpanList.Count;
            int thisPrefixLen = this.PrefixLen;
            int doSepAt = thisPrefixLen;
            for (int i = 0; i < j; ++i)
            {
                WordSpan sp = wordSpanList[i];
                if (sp.len > doSepAt)
                {
                    char c = sp.GetChar(doSepAt, textBuffer);
                    int c_index = c - owner.FirstChar;
                    DevelopingWordGroup found = subGroups[c_index];
                    if (found == null)
                    {
                        //not found
                        found = new DevelopingWordGroup(new WordSpan(sp.startAt, (byte)(doSepAt + 1)));
                        subGroups[c_index] = found;
                    }
                    found.AddWordSpan(sp);
                }
                else
                {
                    if (!hasEvalPrefix)
                    {
                        if (sp.SameTextContent(this.prefixSpan, textBuffer))
                        {
                            hasEvalPrefix = true;
                            this.PrefixIsWord = true;
                        }
                    }
                }

            }
#if DEBUG
            this.dbugDataState = debugDataState.Indexed;
#endif
            wordSpanList.Clear();
            wordSpanList = null;
            //--------------------------------
            //do sup index
            //foreach (WordGroup subgroup in this.wordGroups.Values)
            bool hasSomeSubGroup = false;
            foreach (DevelopingWordGroup subgroup in this.subGroups)
            {
                if (subgroup != null)
                {
                    hasSomeSubGroup = true;

                    //****
                    //performance factor here,****
                    //in this current version 
                    //if we not call DoIndex(),
                    //this subgroup need linear search-> so it slow                   
                    //so we call DoIndex until member count in the group <=3
                    //then it search faster, 
                    //but dictionary-building time may increase.

                    if (subgroup.WordSpanListCount > 2)
                    {
                        subgroup.DoIndex(textBuffer, owner);
                    }
                    else
                    {
#if DEBUG
                        subgroup.dbugDataState = debugDataState.SmallAmountOfMembers;
#endif
                        subgroup.DoIndexOfSmallAmount(textBuffer);
                    }
                }
            }
            //--------------------------------
#if DEBUG
            this.dbugDataState = debugDataState.Indexed;
#endif
            if (!hasSomeSubGroup)
            {
                //clear
                subGroups = null;
            }

            //--------------------------------
            WordGroup[] newsubGroups = null;
            if (subGroups != null)
            {
                newsubGroups = new WordGroup[subGroups.Length];
                for (int i = subGroups.Length - 1; i >= 0; --i)
                {
                    DevelopingWordGroup subg = subGroups[i];
                    if (subg != null)
                    {
                        newsubGroups[i] = subg.ResultWordGroup;
                    }
                }
            }
            //--------------------------------
            this._resultWordGroup = new WordGroup(
                this.prefixSpan,
                newsubGroups,
                null,
                this.PrefixIsWord);

        }

        public WordGroup ResultWordGroup
        {
            get
            {
                return _resultWordGroup;
            }
        }

        void DoIndexOfSmallAmount(CustomDicTextBuffer textBuffer)
        {

            //convention...
            //data must me sorted (ascending) before use with the wordSpanList 

            for (int i = wordSpanList.Count - 1; i >= 0; --i)
            {
                WordSpan sp = wordSpanList[i];
#if DEBUG
                //string dbugStr = sp.GetString(textBuffer);
#endif

                if (sp.SameTextContent(this.prefixSpan, textBuffer))
                {
                    this.PrefixIsWord = true;
                    break;
                }
            }

            this._resultWordGroup = new WordGroup(
                this.prefixSpan,
                null,
                this.wordSpanList.ToArray(),
                this.PrefixIsWord);

        }

#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(this.prefixSpan.startAt + " " + this.prefixSpan.len);
            stbuilder.Append(" " + this.dbugDataState);
            //---------  

            if (wordSpanList != null)
            {
                stbuilder.Append(",u_index=" + wordSpanList.Count + " ");
            }
            return stbuilder.ToString();
        }
#endif

    }


    class CustomDicTextBuffer
    {
        List<char> _tmpCharList;
        int position;
        char[] charBuffer;
        public CustomDicTextBuffer(int initCapacity)
        {
            _tmpCharList = new List<char>(initCapacity);
        }
        public void AddWord(char[] wordBuffer)
        {
            _tmpCharList.AddRange(wordBuffer);
            //append with  ' ' 
            _tmpCharList.Add(' ');
            position += wordBuffer.Length + 1;
        }
        public void Freeze()
        {
            charBuffer = _tmpCharList.ToArray();
            _tmpCharList = null;
        }
        public int CurrentPosition
        {
            get { return position; }
        }
        public char GetChar(int index)
        {
            return charBuffer[index];
        }
        public string GetString(int index, int len)
        {
            return new string(this.charBuffer, index, len);
        }
    }

    public class WordGroup
    {
        readonly WordGroup[] subGroups;
        readonly WordSpan[] wordSpans;
        readonly WordSpan prefixSpan;
        readonly bool prefixIsWord;
        internal WordGroup(WordSpan prefixSpan, WordGroup[] subGroups, WordSpan[] wordSpanList, bool isPrefixIsWord)
        {
            this.prefixSpan = prefixSpan;
            this.subGroups = subGroups;
            this.wordSpans = wordSpanList;
            this.prefixIsWord = isPrefixIsWord;
        }

        internal string GetPrefix(CustomDicTextBuffer buffer)
        {
            return prefixSpan.GetString(buffer);
        }
        internal bool PrefixIsWord
        {
            get { return this.prefixIsWord; }
        }
        internal void CollectAllWords(CustomDicTextBuffer textBuffer, List<string> output)
        {
            if (this.PrefixIsWord)
            {
                output.Add(GetPrefix(textBuffer));
            }
            if (subGroups != null)
            {
                foreach (WordGroup wordGroup in subGroups)
                {
                    if (wordGroup != null)
                    {
                        wordGroup.CollectAllWords(textBuffer, output);
                    }
                }
            }
            if (wordSpans != null)
            {
                foreach (var span in wordSpans)
                {
                    output.Add(span.GetString(textBuffer));
                }
            }
        }
        public int PrefixLen { get { return this.prefixSpan.len; } }


        public int WordSpanListCount
        {
            get
            {

                if (wordSpans == null) return 0;
                return wordSpans.Length;
            }
        }



        internal WordSpan[] GetWordSpans() { return wordSpans; }
        internal WordGroup[] GetSubGroups() { return subGroups; }



#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(this.prefixSpan.startAt + " " + this.prefixSpan.len);

            return stbuilder.ToString();
        }
#endif

    }


}