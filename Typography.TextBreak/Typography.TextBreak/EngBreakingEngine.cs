//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License



namespace Typography.TextBreak
{
    public class EngBreakingEngine : BreakingEngine
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
        public CustomAbbrvDic? EngCustomAbbrvDic { get; set; }


        BreakBounds _breakBounds = new BreakBounds();
        public EngBreakingEngine()
        {

        }
        internal override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        {
            visitor.State = VisitorState.Parsing;
            DoBreak(visitor, charBuff, startAt, len, bb =>
            {
                visitor.AddWordBreakAt(bb.startIndex + bb.length, bb.kind);
                visitor.SetCurrentIndex(visitor.LatestBreakAt);

            });
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
        //
        public override bool CanBeStartChar(char c) => true;
        //
        void DoBreak(WordVisitor visitor, char[] input, int start, int len, OnBreak onBreak)
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
            _breakBounds.startIndex = start;

            char first = (char)0;
            char last = (char)255;

            bool breakPeroidInTextSpan = BreakPeroidInTextSpan;

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
                                _breakBounds.startIndex = i;
                                _breakBounds.length = 2;
                                _breakBounds.kind = WordKind.NewLine;
                                //
                                onBreak(_breakBounds);

                                //
                                _breakBounds.startIndex += 2;//***
                                _breakBounds.length = 0;

                                lexState = LexState.Init;

                                i++;
                                continue;
                            }
                            else if (c == '\r' || c == '\n' || c == 0x85) //U+0085 NEXT LINE
                            {
                                _breakBounds.startIndex = i;
                                _breakBounds.length = 1;
                                _breakBounds.kind = WordKind.NewLine;
                                //
                                onBreak(_breakBounds);
                                //
                                _breakBounds.length = 0;
                                _breakBounds.startIndex++;//***
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsLetter(c))
                            {
                                if (c < first || c > last)
                                {
                                    //letter is out-of-range or not 
                                    //clear accum state
                                    if (i > _breakBounds.startIndex)
                                    {

                                        //some remaining data
                                        _breakBounds.length = i - _breakBounds.startIndex;
                                        //
                                        onBreak(_breakBounds);
                                        _breakBounds.startIndex += _breakBounds.length;//***

                                    }
                                    visitor.State = VisitorState.OutOfRangeChar;
                                    return;
                                }
                                //------------------
                                //just collect
                                _breakBounds.startIndex = i;
                                _breakBounds.kind = WordKind.Text;
                                lexState = LexState.Text;
                            }
                            else if (char.IsNumber(c))
                            {
                                _breakBounds.startIndex = i;
                                _breakBounds.kind = WordKind.Number;
                                lexState = LexState.Number;

                            }
                            else if (char.IsWhiteSpace(c))
                            {
                                //we collect whitespace
                                _breakBounds.startIndex = i;
                                _breakBounds.kind = WordKind.Whitespace;
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
                                        _breakBounds.startIndex = i;
                                        _breakBounds.kind = WordKind.Number;
                                        lexState = LexState.Number;
                                        continue;
                                    }
                                }

                                _breakBounds.startIndex = i;
                                _breakBounds.length = 1;
                                _breakBounds.kind = WordKind.Punc;

                                //we not collect punc
                                onBreak(_breakBounds);
                                //
                                _breakBounds.startIndex += 1;
                                _breakBounds.length = 0;
                                lexState = LexState.Init;
                                continue;
                            }
                            else if (char.IsControl(c))
                            {
                                _breakBounds.startIndex = i;
                                _breakBounds.length = 1;
                                _breakBounds.kind = WordKind.Control;
                                //
                                onBreak(_breakBounds);
                                //
                                _breakBounds.length = 0;
                                _breakBounds.startIndex++;
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
                                    if (i > _breakBounds.startIndex)
                                    {
                                        //some remaining data
                                        _breakBounds.length = i - _breakBounds.startIndex;
                                        //
                                        onBreak(_breakBounds);
                                    }
                                    //-------------------------------
                                    //surrogate pair
                                    _breakBounds.startIndex = i;
                                    _breakBounds.length = 2;
                                    onBreak(_breakBounds);
                                    //-------------------------------

                                    i++;//consume next***

                                    _breakBounds.startIndex += 2;//reset
                                    _breakBounds.length = 0; //reset
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
                                if (i > _breakBounds.startIndex)
                                {
                                    //some remaining data
                                    _breakBounds.length = i - _breakBounds.startIndex;
                                    //
                                    onBreak(_breakBounds);
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
                                _breakBounds.length = i - _breakBounds.startIndex;
                                _breakBounds.kind = WordKind.Number;
                                //
                                onBreak(_breakBounds);
                                //
                                _breakBounds.length = 0;
                                _breakBounds.startIndex = i;
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
                                    if (i > _breakBounds.startIndex)
                                    {
                                        //flush
                                        _breakBounds.length = i - _breakBounds.startIndex;
                                        _breakBounds.kind = WordKind.Text;
                                        //
                                        onBreak(_breakBounds);
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
                                    _breakBounds.length = i - _breakBounds.startIndex;
                                    _breakBounds.kind = WordKind.Text;
                                    //
                                    onBreak(_breakBounds);
                                    //
                                    _breakBounds.length = 1;
                                    _breakBounds.startIndex = i;
                                    lexState = LexState.Number;
                                }
                            }
                            else
                            {
                                //flush existing text ***
                                _breakBounds.length = i - _breakBounds.startIndex;
                                _breakBounds.kind = WordKind.Text;
                                //
                                onBreak(_breakBounds);
                                //
                                _breakBounds.length = 0;
                                _breakBounds.startIndex = i;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }

                        }
                        break;
                    case LexState.Whitespace:
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                _breakBounds.length = i - _breakBounds.startIndex;
                                _breakBounds.kind = WordKind.Whitespace;
                                //
                                onBreak(_breakBounds);
                                //
                                _breakBounds.length = 0;
                                _breakBounds.startIndex = i;
                                lexState = LexState.Init;
                                goto case LexState.Init;
                            }
                        }
                        break;
                }

            }

            if (lexState != LexState.Init &&
                _breakBounds.startIndex < start + len)
            {
                //some remaining data

                _breakBounds.length = (start + len) - _breakBounds.startIndex;
                //
                onBreak(_breakBounds);
                //
            }
            visitor.State = VisitorState.End;
        }
    }
}
