//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

namespace Typography.TextBreak
{
    public class EngAndNonWordBreakingEngine : BreakingEngine
    {
        enum LexState
        {
            Init,
            Whitespace,
            Text,
            Number,
        }

        public bool BreakNumberAfterText { get; set; }
        public bool BreakPeroidInTextSpan { get; set; }

        public bool EnableCustomAbbrv { get; set; }
        public CustomAbbrvDic EngCustomAbbrvDic { get; set; }

        public EngAndNonWordBreakingEngine()
        {

        }
        public override bool CanHandle(char c)
        {
            //this is basic eng + surrogate-pair( eg. emoji)
            return (c <= 255) ||
                   char.IsHighSurrogate(c) ||
                   char.IsPunctuation(c) ||
                   char.IsWhiteSpace(c) ||
                   char.IsControl(c) ||
                   char.IsSymbol(c);
        }
        public override bool CanBeStartChar(char c)
        {
            return true;
        }
        internal override void BreakWord(WordVisitor visitor, System.ReadOnlySpan<char> input)
        {
            visitor.State = VisitorState.Parsing;
            //----------------------------------------
            //simple break word/ num/ punc / space
            //similar to lexer function            
            //----------------------------------------

            BreakBounds breakBounds = new BreakBounds();
            LexState lexState = LexState.Init;
            breakBounds.startIndex = 0;
            int endBefore = input.Length;

            char first = (char)0;
            char last = (char)255;

            bool breakPeroidInTextSpan = BreakPeroidInTextSpan;

            for (int i = 0; i < endBefore; ++i)
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
                                breakBounds.startIndex = i;
                                breakBounds.length = 2;
                                breakBounds.kind = WordKind.NewLine;
                                //
                                visitor.Break(breakBounds);

                                //
                                breakBounds.startIndex += 2;//***
                                breakBounds.length = 0;

                                lexState = LexState.Init;

                                i++;
                                continue;
                            }
                            else if (c == '\r' || c == '\n' || c == 0x85) //U+0085 NEXT LINE
                            {
                                breakBounds.startIndex = i;
                                breakBounds.length = 1;
                                breakBounds.kind = WordKind.NewLine;
                                //
                                visitor.Break(breakBounds);
                                //
                                breakBounds.length = 0;
                                breakBounds.startIndex++;//***
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsLetter(c))
                            {
                                if (c < first || c > last)
                                {
                                    //letter is out-of-range or not 
                                    //clear accum state
                                    if (i > breakBounds.startIndex)
                                    {

                                        //some remaining data
                                        breakBounds.length = i - breakBounds.startIndex;
                                        //
                                        visitor.Break(breakBounds);
                                        breakBounds.startIndex += breakBounds.length;//***

                                    }
                                    visitor.State = VisitorState.OutOfRangeChar;
                                    return;
                                }
                                //------------------
                                //just collect
                                breakBounds.startIndex = i;
                                breakBounds.kind = WordKind.Text;
                                lexState = LexState.Text;
                            }
                            else if (char.IsNumber(c))
                            {
                                breakBounds.startIndex = i;
                                breakBounds.kind = WordKind.Number;
                                lexState = LexState.Number;

                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                //we collect whitespace
                                breakBounds.startIndex = i;
                                breakBounds.kind = WordKind.Whitespace;
                                lexState = LexState.Whitespace;
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
                                        breakBounds.startIndex = i;
                                        breakBounds.kind = WordKind.Number;
                                        lexState = LexState.Number;
                                        continue;
                                    }
                                }

                                breakBounds.startIndex = i;
                                breakBounds.length = 1;
                                breakBounds.kind = WordKind.Punc;

                                //we not collect punc
                                visitor.Break(breakBounds);
                                //
                                breakBounds.startIndex += 1;
                                breakBounds.length = 0;
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsControl(c))
                            {
                                breakBounds.startIndex = i;
                                breakBounds.length = 1;
                                breakBounds.kind = WordKind.Control;
                                //
                                visitor.Break(breakBounds);
                                //
                                breakBounds.length = 0;
                                breakBounds.startIndex++;
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsHighSurrogate(c))
                            {
                                if (i < endBefore - 1 && //not the last one
                                   char.IsLowSurrogate(input[i + 1]))
                                {
                                    //surrogate pair 
                                    //clear accum state
                                    if (i > breakBounds.startIndex)
                                    {
                                        //some remaining data
                                        breakBounds.length = i - breakBounds.startIndex;
                                        //
                                        visitor.Break(breakBounds);
                                    }
                                    //-------------------------------
                                    //surrogate pair
                                    breakBounds.startIndex = i;
                                    breakBounds.length = 2;
                                    visitor.Break(breakBounds);
                                    //-------------------------------

                                    i++;//consume next***

                                    breakBounds.startIndex += 2;//reset
                                    breakBounds.length = 0; //reset
                                    lexState = LexState.Init;
                                    continue; //***
                                }
                                else
                                {
                                    //error
                                    throw new System.FormatException($"A high surrogate (U+{((ushort)c).ToString("X4")}) was not followed by a low surrogate.");
                                }
                            }
                            else if (c < first || c > last)
                            {
                                //letter is out-of-range or not 
                                //clear accum state
                                if (i > breakBounds.startIndex)
                                {
                                    //some remaining data
                                    breakBounds.length = i - breakBounds.startIndex;
                                    //
                                    visitor.Break(breakBounds);
                                    //
                                    //
                                    //TODO: check if we should set startIndex and length
                                    //      like other 'after' onBreak()
                                }
                                visitor.State = VisitorState.OutOfRangeChar;
                                return;
                            }
                            else
                            {
                                throw new System.NotSupportedException($"The character {c} (U+{((ushort)c).ToString("X4")}) was unhandled.");
                            }
                        }
                        break;
                    case LexState.Number:
                        {
                            //in number state
                            if (!char.IsNumber(c) && c != '.')
                            {
                                //if number then continue collect
                                //if not

                                //flush current state 
                                breakBounds.length = i - breakBounds.startIndex;
                                breakBounds.kind = WordKind.Number;
                                //
                                visitor.Break(breakBounds);
                                //
                                breakBounds.length = 0;
                                breakBounds.startIndex = i;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                    case LexState.Text:
                        {
                            bool is_number = char.IsNumber(c);
                            if (char.IsLetter(c) || is_number || (c == '.' && !breakPeroidInTextSpan))
                            {
                                //
                                //c may be out-of-range letter
                                //letter is out-of-range or not 
                                //clear accum state   

                                if (c < first || c > last)
                                {
                                    if (i > breakBounds.startIndex)
                                    {
                                        //flush
                                        breakBounds.length = i - breakBounds.startIndex;
                                        breakBounds.kind = WordKind.Text;
                                        //
                                        visitor.Break(breakBounds);
                                        //
                                        //TODO: check if we should set startIndex and length
                                        //      like other 'after' onBreak()
                                    }
                                    visitor.State = VisitorState.OutOfRangeChar;
                                    return;
                                }

                                if (is_number && BreakNumberAfterText)
                                {
                                    //flush 
                                    breakBounds.length = i - breakBounds.startIndex;
                                    breakBounds.kind = WordKind.Text;
                                    //
                                    visitor.Break(breakBounds);
                                    //
                                    breakBounds.length = 1;
                                    breakBounds.startIndex = i;
                                    lexState = LexState.Number;
                                }
                            }
                            else
                            {
                                //flush existing text ***
                                breakBounds.length = i - breakBounds.startIndex;
                                breakBounds.kind = WordKind.Text;
                                //
                                visitor.Break(breakBounds);
                                //
                                breakBounds.length = 0;
                                breakBounds.startIndex = i;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }

                        }
                        break;
                    case LexState.Whitespace:
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                breakBounds.length = i - breakBounds.startIndex;
                                breakBounds.kind = WordKind.Whitespace;
                                //
                                visitor.Break(breakBounds);
                                //
                                breakBounds.length = 0;
                                breakBounds.startIndex = i;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                }

            }

            if (lexState != LexState.Init &&
                breakBounds.startIndex < endBefore)
            {
                //some remaining data

                breakBounds.length = endBefore - breakBounds.startIndex;
                //
                visitor.Break(breakBounds);
                //
            }
            visitor.State = VisitorState.End;
        }
    }
}
