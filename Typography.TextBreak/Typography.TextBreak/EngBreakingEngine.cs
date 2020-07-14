//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License



using System;
using Typography.OpenFont;

namespace Typography.TextBreak
{
    public enum SurrogatePairBreakingOption
    {
        OnlySurrogatePair,
        ConsecutiveSurrogatePairs,
        ConsecutiveSurrogatePairsAndJoiner
    }
    public class EngBreakingEngine : BreakingEngine
    {
        enum LexState
        {
            Init,
            Whitespace,
            Tab,
            Text,
            Number,
            CollectSurrogatePair,
            CollectConsecutiveUnicode,
        }

        public bool BreakNumberAfterText { get; set; }
        public bool BreakPeroidInTextSpan { get; set; }
        public bool EnableCustomAbbrv { get; set; }
        public bool EnableUnicodeRangeBreaker { get; set; }
        public bool IncludeLatinExtended { get; set; } = true;


        public SurrogatePairBreakingOption SurrogatePairBreakingOption { get; set; } = SurrogatePairBreakingOption.ConsecutiveSurrogatePairsAndJoiner;

        public CustomAbbrvDic EngCustomAbbrvDic { get; set; }

        struct BreakBounds
        {
            public int startIndex;
            public int length;
            public WordKind kind;
            public BreakBounds(int startIndex, int length, WordKind kind)
            {
                this.startIndex = startIndex;
                this.length = length;
                this.kind = kind;
            }
        }

        static readonly SpanBreakInfo s_c0BasicLatin = new SpanBreakInfo(Unicode13RangeInfoList.C0_Controls_and_Basic_Latin, false, ScriptTagDefs.Latin.Tag);
        static readonly SpanBreakInfo s_c1LatinSupplement = new SpanBreakInfo(Unicode13RangeInfoList.C1_Controls_and_Latin_1_Supplement, false, ScriptTagDefs.Latin.Tag);
        static readonly SpanBreakInfo s_latinExtendA = new SpanBreakInfo(Unicode13RangeInfoList.Latin_Extended_A, false, ScriptTagDefs.Latin.Tag);
        static readonly SpanBreakInfo s_latinExtendB = new SpanBreakInfo(Unicode13RangeInfoList.Latin_Extended_B, false, ScriptTagDefs.Latin.Tag);

        static readonly SpanBreakInfo s_emoticon = new SpanBreakInfo(Unicode13RangeInfoList.Emoticons, false, ScriptTagDefs.Latin.Tag);

        static readonly SpanBreakInfo s_latin = new SpanBreakInfo(false, ScriptTagDefs.Latin.Tag);//other         
        static readonly SpanBreakInfo s_unknown = new SpanBreakInfo(false, ScriptTagDefs.Latin.Tag);

        public EngBreakingEngine()
        {

        }
        internal override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        {
            visitor.State = VisitorState.Parsing;
            visitor.SpanBreakInfo = s_latin;

            DoBreak(visitor, charBuff, startAt, len);

        }
        public override bool CanHandle(char c)
        {
            //this is basic eng + surrogate-pair( eg. emoji)
            return (c <= 255) ||
                   char.IsHighSurrogate(c) ||
                   char.IsPunctuation(c) ||
                   char.IsWhiteSpace(c) ||
                   char.IsControl(c) ||
                   char.IsSymbol(c) ||
                   (IncludeLatinExtended && (IsLatinExtendedA(c) || IsLatinExtendedB(c)));
        }
        static bool IsLatinExtendedA(char c) => c >= 0x100 & c <= 0x017F;
        static bool IsLatinExtendedB(char c) => c >= 0x0180 & c <= 0x024F;
        //
        public override bool CanBeStartChar(char c) => true;
        //

        static void OnBreak(WordVisitor vis, in BreakBounds bb) => vis.AddWordBreak_AndSetCurrentIndex(bb.startIndex + bb.length, bb.kind);


        static void CollectConsecutiveUnicodeRange(char[] input, ref int start, int endBefore, out SpanBreakInfo spanBreakInfo)
        {

            char c1 = input[start];
            if (UnicodeRangeFinder.GetUniCodeRangeFor(c1, out UnicodeRangeInfo unicodeRangeInfo, out spanBreakInfo))
            {
                int startCodePoint = unicodeRangeInfo.StarCodepoint;
                int endCodePoint = unicodeRangeInfo.EndCodepoint;
                for (int i = start; i < endBefore; ++i)
                {
                    c1 = input[i];
                    if (c1 < startCodePoint || c1 > endCodePoint)
                    {
                        //out of range again
                        //break here
                        start = i;
                        return;
                    }
                }
                start = endBefore;
            }
            else
            {
                //for unknown,
                //just collect until turn back to latin
#if DEBUG
                System.Diagnostics.Debug.WriteLine("unknown unicode range:");
#endif

                for (int i = start; i < endBefore; ++i)
                {
                    c1 = input[i];
                    if ((c1 >= 0 && c1 < 256) || //eng range
                        char.IsHighSurrogate(c1) || //surrogate pair
                        UnicodeRangeFinder.GetUniCodeRangeFor(c1, out unicodeRangeInfo, out spanBreakInfo)) //or found some wellknown range
                    {
                        //break here
                        start = i;
                        return;
                    }
                }

                start = endBefore;
                spanBreakInfo = s_unknown;
            }
        }
        static void CollectConsecutiveSurrogatePairs(char[] input, ref int start, int len, bool withZeroWidthJoiner)
        {

            int lim = start + len;
            for (int i = start; i < lim;) //start+1
            {
                char c = input[i];

                if ((i + 1 < lim) &&
                    char.IsHighSurrogate(c) &&
                    char.IsLowSurrogate(input[i + 1]))
                {
                    i += 2;//**
                    start = i;
                }
                else if (withZeroWidthJoiner && c == 8205)
                {
                    //https://en.wikipedia.org/wiki/Zero-width_joiner
                    i += 1;
                    start = i;
                }
                else
                {
                    //stop
                    start = i;
                    return;
                }
            }

        }


        bool IsInOurLetterRange(char c, out SpanBreakInfo brkInfo)
        {
            if (c >= 0 && c <= 127)
            {
                brkInfo = s_c0BasicLatin;
                return true;
            }
            else if (c <= 255)
            {
                brkInfo = s_c1LatinSupplement;
                return true;
            }
            else if (IncludeLatinExtended)
            {
                if (s_latinExtendA.UnicodeRange.IsInRange(c))
                {
                    brkInfo = s_latinExtendA;
                    return true;
                }
                else if (s_latinExtendB.UnicodeRange.IsInRange(c))
                {
                    brkInfo = s_latinExtendB;
                    return true;
                }
            }
            brkInfo = null;
            return false;
        }

        void DoBreak(WordVisitor visitor, char[] input, int start, int len)
        {

            //----------------------------------------
            //simple break word/ num/ punc / space
            //similar to lexer function            
            //----------------------------------------
            int endBefore = start + len;
            if (endBefore > input.Length)
                throw new System.ArgumentOutOfRangeException(nameof(len), len, "The range provided was partially out of bounds.");
            else if (start < 0)
                throw new System.ArgumentOutOfRangeException(nameof(start), start, "The starting index was negative.");
            //throw instead of skipping the entire for loop
            else if (len < 0)
                throw new System.ArgumentOutOfRangeException(nameof(len), len, "The length provided was negative.");
            //----------------------------------------

            LexState lexState = LexState.Init;
            BreakBounds bb = new BreakBounds();
            bb.startIndex = start;

            bool enableUnicodeRangeBreaker = EnableUnicodeRangeBreaker;
            bool breakPeroidInTextSpan = BreakPeroidInTextSpan;

            visitor.SpanBreakInfo = s_c0BasicLatin;//default

            for (int i = start; i < endBefore; ++i)
            {
                char c = input[i];
                switch (lexState)
                {
                    case LexState.Init:
                        {
                            //check char
                            if (c == '\r' && i < endBefore - 1 && input[i + 1] == '\n')
                            {
                                //this is '\r\n' linebreak
                                bb.startIndex = i;
                                bb.length = 2;
                                bb.kind = WordKind.NewLine;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.startIndex += 2;//***
                                bb.length = 0;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;

                                i++;
                                continue;
                            }
                            else if (c == '\r' || c == '\n' || c == 0x85) //U+0085 NEXT LINE
                            {
                                bb.startIndex = i;
                                bb.length = 1;
                                bb.kind = WordKind.NewLine;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex++;//***
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (c == ' ')
                            {
                                //explicit whitespace
                                //we collect whitespace
                                bb.startIndex = i;
                                bb.kind = WordKind.Whitespace;
                                lexState = LexState.Whitespace;
                            }
                            else if (c == '\t')
                            {
                                //explicit tab
                                bb.startIndex = i;
                                bb.kind = WordKind.Tab;
                                lexState = LexState.Tab;
                            } 
                            else if (char.IsLetter(c))
                            {
                                if (!IsInOurLetterRange(c, out SpanBreakInfo brkInfo))
                                {
                                    //letter is OUT_OF_RANGE
                                    if (i > bb.startIndex)
                                    {

                                        //???TODO: review here
                                        //flush
                                        bb.length = i - bb.startIndex;
                                        //
                                        OnBreak(visitor, bb);
                                        bb.startIndex += bb.length;//***
                                    }

                                    if (char.IsHighSurrogate(c))
                                    {
                                        lexState = LexState.CollectSurrogatePair;
                                        goto case LexState.CollectSurrogatePair;
                                    }

                                    if (enableUnicodeRangeBreaker)
                                    {
                                        //collect text until end for specific unicode range eng
                                        //find a proper unicode engine and collect until end of its range 
                                        lexState = LexState.CollectConsecutiveUnicode;
                                        goto case LexState.CollectConsecutiveUnicode;
                                    }
                                    else
                                    {
                                        visitor.State = VisitorState.OutOfRangeChar;
                                        return;
                                    }
                                }

                                visitor.SpanBreakInfo = brkInfo;

                                //just collect
                                bb.startIndex = i;
                                bb.kind = WordKind.Text;
                                lexState = LexState.Text;
                            }
                            else if (char.IsNumber(c))
                            {
                                bb.startIndex = i;
                                bb.kind = WordKind.Number;
                                lexState = LexState.Number;
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                //other whitespace***=> similar to control
                                bb.startIndex = i;
                                bb.length = 1;
                                bb.kind = WordKind.OtherWhitespace;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex++;
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsControl(c))
                            {
                                //\t is control and \t is also whitespace
                                //we assign that \t to be a control
                                bb.startIndex = i;
                                bb.length = 1;
                                bb.kind = WordKind.Control;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex++;
                                lexState = LexState.Init;
                                continue;
                            }

                            else if (char.IsPunctuation(c) || char.IsSymbol(c))
                            {

                                //for eng -
                                if (c == '-')
                                {
                                    //check next char is number or not
                                    if (i < endBefore - 1 &&
                                       char.IsNumber(input[i + 1]))
                                    {
                                        bb.startIndex = i;
                                        bb.kind = WordKind.Number;
                                        lexState = LexState.Number;
                                        continue;
                                    }
                                }

                                bb.startIndex = i;
                                bb.length = 1;
                                bb.kind = WordKind.Punc;

                                //we not collect punc
                                OnBreak(visitor, bb);
                                //
                                bb.startIndex += 1;
                                bb.length = 0;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                continue;
                            }

                            else if (char.IsHighSurrogate(c))
                            {
                                lexState = LexState.CollectSurrogatePair;
                                goto case LexState.CollectSurrogatePair;
                            }
                            else
                            {
                                if (enableUnicodeRangeBreaker)
                                {
                                    lexState = LexState.CollectConsecutiveUnicode;
                                    goto case LexState.CollectConsecutiveUnicode;
                                }
                                else
                                {
                                    visitor.State = VisitorState.OutOfRangeChar;
                                    return;
                                }
                            }

                        }
                        break;
                    case LexState.Number:
                        {
                            //in number state
                            if (!char.IsNumber(c) && c != '.')
                            {
                                //if not
                                //flush current state 
                                bb.length = i - bb.startIndex;
                                bb.kind = WordKind.Number;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex = i;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                    case LexState.Text:
                        {
                            //we are in letter mode

                            if (char.IsNumber(c))
                            {
                                //flush 
                                if (BreakNumberAfterText)
                                {
                                    bb.length = i - bb.startIndex;
                                    bb.kind = WordKind.Text;
                                    //
                                    OnBreak(visitor, bb);
                                    //
                                    bb.length = 1;
                                    bb.startIndex = i;
                                    lexState = LexState.Number;
                                }
                            }
                            else if (char.IsLetter(c))
                            {
                                //c is letter
                                if (!IsInOurLetterRange(c, out SpanBreakInfo brkInfo))
                                {
                                    if (i > bb.startIndex)
                                    {
                                        //flush
                                        bb.length = i - bb.startIndex;
                                        bb.kind = WordKind.Text;
                                        //
                                        OnBreak(visitor, bb);
                                        //TODO: check if we should set startIndex and length
                                        //      like other 'after' onBreak()
                                        bb.startIndex += bb.length;//***

                                    }

                                    if (char.IsHighSurrogate(c))
                                    {
                                        lexState = LexState.CollectSurrogatePair;
                                        goto case LexState.CollectSurrogatePair;
                                    }

                                    if (enableUnicodeRangeBreaker)
                                    {
                                        lexState = LexState.CollectConsecutiveUnicode;
                                        goto case LexState.CollectConsecutiveUnicode;
                                    }
                                    else
                                    {
                                        visitor.State = VisitorState.OutOfRangeChar;
                                        return;
                                    }
                                }

                                //if this is a letter in our range 
                                //special for eng breaking ening, check 
                                //if a letter is in basic latin range or not                                
                                //------------------
                                if (visitor.SpanBreakInfo != brkInfo)
                                {
                                    //mixed span break info => change to general latin
                                    visitor.SpanBreakInfo = s_latin;
                                }

                            }
                            else if (c == '.' && !breakPeroidInTextSpan)
                            {
                                //continue collecting
                            }
                            else
                            {
                                //other characer
                                //flush existing text ***
                                bb.length = i - bb.startIndex;
                                bb.kind = WordKind.Text;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex = i;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                    case LexState.Whitespace:
                        {
                            //for explicit whitespace
                            if (c != ' ')
                            {
                                bb.length = i - bb.startIndex;
                                bb.kind = WordKind.Whitespace;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex = i;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                    case LexState.Tab:
                        {
                            if (c != '\t')
                            {
                                bb.length = i - bb.startIndex;
                                bb.kind = WordKind.Tab;
                                //
                                OnBreak(visitor, bb);
                                //
                                bb.length = 0;
                                bb.startIndex = i;
                                bb.kind = WordKind.Unknown;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                    case LexState.CollectSurrogatePair:
                        {
                            if (i == endBefore - 1)
                            {
                                //can't check next char
                                //error 

                                bb.length = 1;
                                bb.kind = WordKind.Unknown; //error 
                                OnBreak(visitor, bb);

                                bb.length = 0; //reset
                                bb.startIndex++;

                                lexState = LexState.Init;
                                continue; //***
                            }
                            else
                            {
                                if (!char.IsLowSurrogate(input[i + 1]))
                                {
                                    //the next one this not low surrogate
                                    //so this is error too
                                    bb.length = 1;
                                    bb.kind = WordKind.Unknown; //error 
                                    OnBreak(visitor, bb);

                                    bb.length = 0; //reset
                                    bb.startIndex++;
                                    lexState = LexState.Init;
                                    continue; //***
                                }
                                else
                                {
                                    //surrogate pair 
                                    //clear accum state
                                    if (i > bb.startIndex)
                                    {
                                        //some remaining data
                                        bb.length = i - bb.startIndex;
                                        //
                                        OnBreak(visitor, bb);
                                    }
                                    //-------------------------------
                                    //surrogate pair

                                    if (SurrogatePairBreakingOption == SurrogatePairBreakingOption.OnlySurrogatePair)
                                    {
                                        bb.startIndex = i;
                                        bb.length = 2;
                                        bb.kind = WordKind.SurrogatePair;

                                        OnBreak(visitor, bb);

                                        i++;//consume next***
                                        bb.startIndex += 2;//reset 
                                    }
                                    else
                                    {
                                        //see https://github.com/LayoutFarm/Typography/issues/18#issuecomment-345480185
                                        int begin = i + 2;
                                        CollectConsecutiveSurrogatePairs(input, ref begin, endBefore - begin, SurrogatePairBreakingOption == SurrogatePairBreakingOption.ConsecutiveSurrogatePairsAndJoiner);

                                        bb.startIndex = i;
                                        bb.length = begin - i;
                                        bb.kind = WordKind.SurrogatePair; 
                                        
                                        OnBreak(visitor, bb);

                                        i += bb.length - 1;//consume 
                                        bb.startIndex += bb.length;//reset 
                                    }
                                    bb.length = 0; //reset
                                    lexState = LexState.Init;
                                    continue; //***
                                }
                            }
                        }
                        break;
                    case LexState.CollectConsecutiveUnicode:
                        {
                            int begin = i;
                            bb.startIndex = i;
                            bb.kind = WordKind.Text;
                            CollectConsecutiveUnicodeRange(input, ref begin, endBefore, out SpanBreakInfo spBreakInfo);
                            bb.length = begin - i;
                            if (bb.length > 0)
                            {
                                visitor.SpanBreakInfo = spBreakInfo;

                                OnBreak(visitor, bb); //flush

                                visitor.SpanBreakInfo = s_latin;//switch back
                                bb.length = 0;
                                bb.startIndex = begin;
                            }
                            else
                            {
                                throw new NotSupportedException();///???
                            }

                            i = begin - 1;
                            lexState = LexState.Init;
                        }
                        break;
                }
            }

            if (lexState != LexState.Init &&
                bb.startIndex < start + len)
            {
                //some remaining data

                bb.length = (start + len) - bb.startIndex;
                //
                OnBreak(visitor, bb);
                //
            }
            visitor.State = VisitorState.End;
        }
    }
}
