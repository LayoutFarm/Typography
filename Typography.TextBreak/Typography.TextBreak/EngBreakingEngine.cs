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

        internal override void BreakWord(WordVisitor visitor)
        {
            visitor.State = VisitorState.Parsing;
            visitor.SpanBreakInfo = s_latin;


            LexState lexState = LexState.Init;
            BreakBounds bb = new BreakBounds
            {
                startIndex = visitor.Offset
            };

            bool enableUnicodeRangeBreaker = EnableUnicodeRangeBreaker;
            bool breakPeroidInTextSpan = BreakPeroidInTextSpan;

            visitor.SpanBreakInfo = s_c0BasicLatin;//default

#if DEBUG
            int same_pos_count = 0;
            char prev_char = '\0';
            int prev_pos = -1;
#endif

            while (!visitor.IsEnd)
            {

#if DEBUG
                if (prev_pos == visitor.Offset)
                {
                    same_pos_count++;
                    if (same_pos_count > 5)
                    {
                        System.Diagnostics.Debugger.Break();
                    }
                }
                else
                {
                    prev_pos = visitor.Offset;
                }
#endif

                char c = visitor.C0;

                switch (lexState)
                {
                    case LexState.Init:
                        {
                            //check char
                            if (c == '\r' && visitor.PeekNext() == '\n')
                            {
                                //this is '\r\n' linebreak
                                bb.startIndex = visitor.Offset;
                                bb.length = 2;
                                bb.kind = WordKind.NewLine;

                                visitor.AddWordBreakAt(bb);

                                //
                                bb.Consume();
                                lexState = LexState.Init;

                                //TODO: review here
                                //visitor.Read();//consume \n
                                continue;
                            }
                            else if (c == '\r' || c == '\n' || c == 0x85) //U+0085 NEXT LINE
                            {
                                bb.startIndex = visitor.Offset;
                                bb.length = 1;
                                bb.kind = WordKind.NewLine;
                                //

                                visitor.AddWordBreakAt(bb);
                                //
                                bb.Consume();
                                lexState = LexState.Init;

                                continue;
                            }
                            else if (c == ' ')
                            {
                                //explicit whitespace
                                //we collect whitespace
                                bb.startIndex = visitor.Offset;
                                bb.kind = WordKind.Whitespace;
                                lexState = LexState.Whitespace;
                                visitor.Read();
                            }
                            else if (c == '\t')
                            {
                                //explicit tab
                                bb.startIndex = visitor.Offset;
                                bb.kind = WordKind.Tab;
                                lexState = LexState.Tab;
                                visitor.Read();
                            }
                            else if (char.IsLetter(c))
                            {
                                if (!IsInOurLetterRange(c, out SpanBreakInfo brkInfo))
                                {
                                    //letter is OUT_OF_RANGE
                                    if (visitor.Offset > bb.startIndex)
                                    {

                                        //???TODO: review here
                                        //flush
                                        bb.length = visitor.Offset - bb.startIndex;

                                        visitor.AddWordBreakAt(bb);
                                        bb.Consume();
                                    }

                                    if (char.IsHighSurrogate(c))
                                    {
                                        bb.kind = WordKind.SurrogatePair;
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
                                bb.startIndex = visitor.Offset;
                                bb.kind = WordKind.Text;
                                lexState = LexState.Text;
                                visitor.Read();
                            }
                            else if (char.IsNumber(c))
                            {
                                bb.startIndex = visitor.Offset;
                                bb.kind = WordKind.Number;
                                lexState = LexState.Number;
                                visitor.Read();
                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                //other whitespace***=> similar to control
                                bb.startIndex = visitor.Offset;
                                bb.length = 1;
                                bb.kind = WordKind.OtherWhitespace;
                                //
                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsControl(c))
                            {
                                //\t is control and \t is also whitespace
                                //we assign that \t to be a control
                                bb.startIndex = visitor.Offset;
                                bb.length = 1;
                                bb.kind = WordKind.Control;
                                //
                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                lexState = LexState.Init;
                                continue;
                            }

                            else if (char.IsPunctuation(c) || char.IsSymbol(c))
                            {

                                //for eng -
                                if (c == '-')
                                {
                                    //check next char is number or not
                                    if (char.IsNumber(visitor.PeekNext()))
                                    {
                                        //review here again
                                        bb.startIndex = visitor.Offset;
                                        bb.kind = WordKind.Number;
                                        lexState = LexState.Number;
                                        visitor.Read();
                                        continue;
                                    }
                                }

                                bb.startIndex = visitor.Offset;
                                bb.length = 1;
                                bb.kind = WordKind.Punc;

                                visitor.AddWordBreakAt(bb);

                                bb.Consume();
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
                                bb.length = visitor.Offset - bb.startIndex;
                                bb.kind = WordKind.Number;
                                //
                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                            //still in number state
                            visitor.Read();
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
                                    bb.length = visitor.Offset - bb.startIndex;
                                    bb.kind = WordKind.Text;
                                    //                                     
                                    visitor.AddWordBreakAt(bb);

                                    //
                                    bb.length = 1;
                                    bb.startIndex = visitor.Offset;
                                    lexState = LexState.Number;
                                }
                            }
                            else if (char.IsLetter(c))
                            {
                                //c is letter
                                if (!IsInOurLetterRange(c, out SpanBreakInfo brkInfo))
                                {
                                    if (visitor.Offset > bb.startIndex)
                                    {
                                        //flush
                                        bb.length = visitor.Offset - bb.startIndex;
                                        bb.kind = WordKind.Text;
                                        //                                         
                                        visitor.AddWordBreakAt(bb);
                                        bb.Consume();

                                    }

                                    if (char.IsHighSurrogate(c))
                                    {
                                        bb.kind = WordKind.SurrogatePair;
                                        bb.startIndex = visitor.Offset;

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
                                bb.length = visitor.Offset - bb.startIndex;
                                bb.kind = WordKind.Text;
                                //
                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }

                            visitor.Read();
                        }
                        break;
                    case LexState.Whitespace:
                        {
                            //for explicit whitespace
                            if (c != ' ')
                            {
                                bb.length = visitor.Offset - bb.startIndex;
                                bb.kind = WordKind.Whitespace;
                                //

                                visitor.AddWordBreakAt(bb);
                                //
                                bb.Consume();
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                            visitor.Read();
                        }
                        break;
                    case LexState.Tab:
                        {
                            if (c != '\t')
                            {
                                bb.length = visitor.Offset - bb.startIndex;
                                bb.kind = WordKind.Tab;
                                //
                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                            visitor.Read();
                        }
                        break;
                    case LexState.CollectSurrogatePair:
                        {

                            //check next is surrogate 
                            if (!char.IsLowSurrogate(visitor.C1))
                            {
                                //the next one this not low surrogate
                                //so this is error too
                                bb.length = 1;
                                bb.kind = WordKind.Unknown; //error 

                                visitor.AddWordBreakAt(bb);
                                bb.Consume();
                                lexState = LexState.Init;

                                continue; //***
                            }
                            else
                            {

                                //-------------------------------
                                //surrogate pair

                                if (SurrogatePairBreakingOption == SurrogatePairBreakingOption.OnlySurrogatePair)
                                {
                                    bb.startIndex = visitor.Offset;
                                    bb.length = 2;
                                    bb.kind = WordKind.SurrogatePair;

                                    visitor.AddWordBreakAt(bb);
                                    bb.Consume();
                                }
                                else
                                {
                                    //see https://github.com/LayoutFarm/Typography/issues/18#issuecomment-345480185
                                    int begin = visitor.Offset;

                                    CollectConsecutiveSurrogatePairs(visitor, SurrogatePairBreakingOption == SurrogatePairBreakingOption.ConsecutiveSurrogatePairsAndJoiner);

                                    bb.length = visitor.Offset - begin;
                                    bb.kind = WordKind.SurrogatePair;


                                    visitor.AddWordBreakAt(bb);
                                    bb.Consume();
                                }

                                lexState = LexState.Init;

                                continue; //***
                            }
                            //}
                        }
                    case LexState.CollectConsecutiveUnicode:
                        {

                            bb.startIndex = visitor.Offset;
                            bb.kind = WordKind.Text;

                            CollectConsecutiveUnicodeRange(visitor, out SpanBreakInfo spBreakInfo);

                            if (visitor.Offset > bb.startIndex)
                            {
                                visitor.SpanBreakInfo = spBreakInfo;
                                bb.length = visitor.Offset - bb.startIndex;

                                visitor.AddWordBreakAt(bb);
                                bb.Consume();

                                visitor.SpanBreakInfo = s_latin;//switch back
                                lexState = LexState.Init;
                            }
                            else
                            {
                                throw new OpenFont.OpenFontNotSupportedException();///???
                            }

                        }
                        break;
                }
            }

            if (lexState != LexState.Init &&
                bb.startIndex < visitor.Offset)
            {
                //some remaining data
                bb.length = visitor.Offset - bb.startIndex;
                //
                visitor.AddWordBreakAt(bb);
                //
            }
            visitor.State = VisitorState.End;
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


        static void CollectConsecutiveUnicodeRange(WordVisitor visitor, out SpanBreakInfo spanBreakInfo)
        {

            char c1 = visitor.C0;
            if (UnicodeRangeFinder.GetUniCodeRangeFor(c1, out UnicodeRangeInfo unicodeRangeInfo, out spanBreakInfo))
            {
                int startCodePoint = unicodeRangeInfo.StartCodepoint;
                int endCodePoint = unicodeRangeInfo.EndCodepoint;

                do
                {
                    c1 = visitor.C0;
                    if (c1 < startCodePoint || c1 > endCodePoint)
                    {
                        break;
                    }

                } while (visitor.Read());
            }
            else
            {
                //for unknown,
                //just collect until turn back to latin
#if DEBUG
                System.Diagnostics.Debug.WriteLine("unknown unicode range:");
#endif

                while (visitor.Read())
                {
                    c1 = visitor.C0;
                    if ((c1 >= 0 && c1 < 256) || //eng range
                        char.IsHighSurrogate(c1) || //surrogate pair
                        UnicodeRangeFinder.GetUniCodeRangeFor(c1, out unicodeRangeInfo, out spanBreakInfo)) //or found some wellknown range
                    {
                        //break here

                        return;
                    }
                }
                spanBreakInfo = s_unknown;
            }
        }

        static void CollectConsecutiveSurrogatePairs(WordVisitor visitor, bool withZeroWidthJoiner)
        {
            do
            {
                char c = visitor.C0;
                if (char.IsHighSurrogate(c) &&
                    char.IsLowSurrogate(visitor.C1))
                {

                }
                else if (withZeroWidthJoiner && c == 8205)
                {
                    //https://en.wikipedia.org/wiki/Zero-width_joiner
                }
                else
                {
                    //stop here
                    return;
                }
            }
            while (visitor.Read());
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

    }

    static class WordVisitorExtensions
    {
        public static void AddWordBreakAt(this WordVisitor vis, in BreakBounds bb)
        {
            //vis.AddWordBreakAtCurrentIndex(bb.kind);
            if (bb.startIndex + bb.length != vis.Offset)
            {
                //  System.Diagnostics.Debugger.Break();
            }


            vis.AddWordBreak_AndSetCurrentIndex(bb.startIndex + bb.length, bb.kind);
        }
    }


}
