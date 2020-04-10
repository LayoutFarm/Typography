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
        CustomDicTextBuffer? _textBuffer;
        WordGroup[]? _wordGroups;
        char _firstChar, _lastChar;


        public void SetCharRange(char firstChar, char lastChar)
        {
            _firstChar = firstChar;
            _lastChar = lastChar;
        }
        public char FirstChar => _firstChar;
        public char LastChar => _lastChar;
        internal CustomDicTextBuffer TextBuffer =>
            _textBuffer ?? throw new InvalidOperationException(nameof(LoadSortedUniqueWordList) + " not called");


        public void LoadSortedUniqueWordList(IEnumerable<string> sortedWordList)
        {
            // load unique and sorted word list
            if (_textBuffer != null)
            {
                return;
            }
            if (_firstChar == '\0' || _lastChar == '\0')
            {
                throw new NotSupportedException();
            }

            //---------------
            Dictionary<char, DevelopingWordGroup> wordGroups = new Dictionary<char, DevelopingWordGroup>();
            _textBuffer = new CustomDicTextBuffer(1024);
            foreach (string line in sortedWordList)
            {
                char[] lineBuffer = line.Trim().ToCharArray();
                int lineLen = lineBuffer.Length;
                char c0;
                if (lineLen > 0 && (c0 = lineBuffer[0]) != '#')
                {
                    int startAt = _textBuffer.CurrentPosition;
                    _textBuffer.AddWord(lineBuffer);

#if DEBUG
                    if (lineLen > byte.MaxValue)
                    {
                        throw new NotSupportedException();
                    }
#endif

                    WordSpan wordspan = new WordSpan(startAt, (byte)lineLen);
                    //each wordgroup contains text span

                    DevelopingWordGroup? found;
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
            _textBuffer.Freeze();
            //------------------------------------------------------------------ 
            //do index
            DoIndex(wordGroups);

            //clear, not used
            wordGroups.Clear();

        }

        int TransformCharToIndex(char c) => c - _firstChar;
        //
        void DoIndex(Dictionary<char, DevelopingWordGroup> wordGroups)
        {
            if (_textBuffer == null) throw new InvalidOperationException(nameof(LoadSortedUniqueWordList) + " not called");
            //1. expand word group
            WordGroup[] newWordGroups = new WordGroup[_lastChar - _firstChar + 1];

            foreach (var kp in wordGroups)
            {
                //for each dev word group
                int index = TransformCharToIndex(kp.Key);
                DevelopingWordGroup devWordGroup = kp.Value;
                devWordGroup.DoIndex(_textBuffer, this);
                newWordGroups[index] = devWordGroup.ResultWordGroup;
            }
            _wordGroups = newWordGroups;
        }
        public void GetWordList(char startWithChar, List<string> output)
        {
            if (_textBuffer == null) throw new InvalidOperationException(nameof(LoadSortedUniqueWordList) + " not called");
            if (_wordGroups == null) throw new InvalidOperationException(nameof(DoIndex) + " not called");
            if (startWithChar >= _firstChar && startWithChar <= _lastChar)
            {
                //in range 
                WordGroup found = _wordGroups[TransformCharToIndex(startWithChar)];
                if (found != null)
                {
                    //iterate and collect into 
                    found.CollectAllWords(_textBuffer, output);
                }
            }
        }
        internal WordGroup? GetWordGroupForFirstChar(char c)
        {
            if (_wordGroups == null) throw new InvalidOperationException(nameof(DoIndex) + " not called");

            if (c >= _firstChar && c <= _lastChar)
            {
                //in range
                return _wordGroups[TransformCharToIndex(c)];
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
        List<WordSpan>? _wordSpanList = new List<WordSpan>();
        DevelopingWordGroup[]? _subGroups;
        WordSpan _prefixSpan;
        internal DevelopingWordGroup(WordSpan prefixSpan)
        {
            _prefixSpan = prefixSpan;
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
        public static int DebugTotalId => debugTotalId;
        debugDataState dbugDataState;
#endif

        internal string GetPrefix(CustomDicTextBuffer buffer)
        {
            return _prefixSpan.GetString(buffer);
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
            if (_subGroups != null)
            {
                foreach (DevelopingWordGroup wordGroup in _subGroups)
                {
                    if (wordGroup != null)
                    {
                        wordGroup.CollectAllWords(textBuffer, output);
                    }
                }
            }
            if (_wordSpanList != null)
            {
                foreach (var span in _wordSpanList)
                {
                    output.Add(span.GetString(textBuffer));
                }
            }
        }
        //
        public int PrefixLen => _prefixSpan.len;
        //
        internal void AddWordSpan(WordSpan span)
        {
            _wordSpanList?.Add(span);
#if DEBUG
            dbugDataState = debugDataState.UnIndex;
#endif
        }
        public int WordSpanListCount
        {
            get
            {
                if (_wordSpanList == null) return 0;
                return _wordSpanList.Count;
            }
        }
        WordGroup? _resultWordGroup;//after call DoIndex()
        internal void DoIndex(CustomDicTextBuffer textBuffer, CustomDic owner)
        {
            if (_wordSpanList == null) throw new InvalidOperationException(nameof(DoIndex) + " already called");

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
            if (_subGroups == null)
            {
                _subGroups = new DevelopingWordGroup[owner.LastChar - owner.FirstChar + 1];
            }
            //--------------------------------
            int j = _wordSpanList.Count;
            int thisPrefixLen = this.PrefixLen;
            int doSepAt = thisPrefixLen;
            for (int i = 0; i < j; ++i)
            {
                WordSpan sp = _wordSpanList[i];
                if (sp.len > doSepAt)
                {
                    char c = sp.GetChar(doSepAt, textBuffer);
                    int c_index = c - owner.FirstChar;
                    DevelopingWordGroup found = _subGroups[c_index];
                    if (found == null)
                    {
                        //not found
                        found = new DevelopingWordGroup(new WordSpan(sp.startAt, (byte)(doSepAt + 1)));
                        _subGroups[c_index] = found;
                    }
                    found.AddWordSpan(sp);
                }
                else
                {
                    if (!hasEvalPrefix)
                    {
                        if (sp.SameTextContent(_prefixSpan, textBuffer))
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
            _wordSpanList.Clear();
            _wordSpanList = null;
            //--------------------------------
            //do sup index
            //foreach (WordGroup subgroup in this.wordGroups.Values)
            bool hasSomeSubGroup = false;
            foreach (DevelopingWordGroup subgroup in _subGroups)
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
                _subGroups = null;
            }

            //--------------------------------
            WordGroup[]? newsubGroups = null;
            if (_subGroups != null)
            {
                newsubGroups = new WordGroup[_subGroups.Length];
                for (int i = _subGroups.Length - 1; i >= 0; --i)
                {
                    DevelopingWordGroup subg = _subGroups[i];
                    if (subg != null)
                    {
                        newsubGroups[i] = subg.ResultWordGroup;
                    }
                }
            }
            //--------------------------------
            _resultWordGroup = new WordGroup(
                _prefixSpan,
                newsubGroups,
                null,
                this.PrefixIsWord);

        }
        //
        public WordGroup ResultWordGroup => _resultWordGroup ?? throw new InvalidOperationException(nameof(DoIndex) + " not called");
        //
        void DoIndexOfSmallAmount(CustomDicTextBuffer textBuffer)
        {
            if (_wordSpanList == null) throw new InvalidOperationException(nameof(DoIndex) + " already called");
            //convention...
            //data must me sorted (ascending) before use with the wordSpanList 

            for (int i = _wordSpanList.Count - 1; i >= 0; --i)
            {
                WordSpan sp = _wordSpanList[i];
#if DEBUG
                //string dbugStr = sp.GetString(textBuffer);
#endif

                if (sp.SameTextContent(_prefixSpan, textBuffer))
                {
                    this.PrefixIsWord = true;
                    break;
                }
            }

            _resultWordGroup = new WordGroup(
                _prefixSpan,
                null,
                _wordSpanList.ToArray(),
                this.PrefixIsWord);

        }

#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(_prefixSpan.startAt + " " + _prefixSpan.len);
            stbuilder.Append(" " + this.dbugDataState);
            //---------  

            if (_wordSpanList != null)
            {
                stbuilder.Append(",u_index=" + _wordSpanList.Count + " ");
            }
            return stbuilder.ToString();
        }
#endif

    }


    class CustomDicTextBuffer
    {
        List<char>? _tmpCharList;
        int _position;
        char[]? _charBuffer;
        public CustomDicTextBuffer(int initCapacity)
        {
            _tmpCharList = new List<char>(initCapacity);
        }
        public void AddWord(char[] wordBuffer)
        {
            if (_tmpCharList == null) return;
            _tmpCharList.AddRange(wordBuffer);
            //append with  ' ' 
            _tmpCharList.Add(' ');
            _position += wordBuffer.Length + 1;
        }
        public void Freeze()
        {
            _charBuffer ??= _tmpCharList?.ToArray();
            _tmpCharList = null;
        }
        //
        public int CurrentPosition => _position;
        //
        public char GetChar(int index)
        {
            //refactor note:
            //remain in this style -> easy to debug
            if (_charBuffer == null) throw new InvalidOperationException("Buffer not frozen yet");
            return _charBuffer[index];
        }
        public string GetString(int index, int len)
        {
            if (_charBuffer == null) throw new InvalidOperationException("Buffer not frozen yet");
            return new string(_charBuffer, index, len);
        }
    }

    public class WordGroup
    {
        readonly WordGroup[]? _subGroups;
        readonly WordSpan[]? _wordSpans;
        readonly WordSpan _prefixSpan;
        readonly bool _prefixIsWord;
        internal WordGroup(WordSpan prefixSpan, WordGroup[]? subGroups, WordSpan[]? wordSpanList, bool isPrefixIsWord)
        {
            _prefixSpan = prefixSpan;
            _subGroups = subGroups;
            _wordSpans = wordSpanList;
            _prefixIsWord = isPrefixIsWord;
        }

        internal string GetPrefix(CustomDicTextBuffer buffer)
        {
            return _prefixSpan.GetString(buffer);
        }
        //
        internal bool PrefixIsWord => _prefixIsWord;
        public int PrefixLen => _prefixSpan.len;
        //
        internal void CollectAllWords(CustomDicTextBuffer textBuffer, List<string> output)
        {
            if (this.PrefixIsWord)
            {
                output.Add(GetPrefix(textBuffer));
            }
            if (_subGroups != null)
            {
                foreach (WordGroup wordGroup in _subGroups)
                {
                    if (wordGroup != null)
                    {
                        wordGroup.CollectAllWords(textBuffer, output);
                    }
                }
            }
            if (_wordSpans != null)
            {
                foreach (var span in _wordSpans)
                {
                    output.Add(span.GetString(textBuffer));
                }
            }
        }
        public int WordSpanListCount
        {
            get
            {

                if (_wordSpans == null) return 0;
                return _wordSpans.Length;
            }
        }
        internal WordSpan[]? GetWordSpans() => _wordSpans;
        internal WordGroup[]? GetSubGroups() => _subGroups;

#if DEBUG
        public override string ToString()
        {
            StringBuilder stbuilder = new StringBuilder();
            stbuilder.Append(_prefixSpan.startAt + " " + _prefixSpan.len);

            return stbuilder.ToString();
        }
#endif

    }


    //----------------
    /// <summary>
    /// Abbreviation dic, special treatment for dot (.) in word parsing
    /// </summary>
    public class CustomAbbrvDic
    {
        /// <summary>
        /// load wellknown Abbreviation
        /// </summary>
        /// <param name="sortedWordList"></param>
        public void LoadSortedUniqueWordList(IEnumerable<string> sortedWordList)
        {
            if (sortedWordList == null)
            {
                return;
            }
            //---------------------
            //build a dic
            //each word contains one or more .

            //TODO: implement this...
            ////
            ////If we use this feature:
            ////when the central engine for . it consult the custom abbrv
            //foreach (string str in sortedWordList)
            //{
            //    string[] subparts = str.Split('.');


            //}
        }
    }

}