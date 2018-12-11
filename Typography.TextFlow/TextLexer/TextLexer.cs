//MIT, 2014-present, WinterDev
using System.Collections.Generic;

using Typography.TextLayout;
namespace Typography.TextBreak
{
    public class TextBlockLexer
    {
        List<LexWordSpan> _spans = new List<LexWordSpan>();
        public List<LexWordSpan> ResultSpans => _spans;
        //
        public virtual void Lex(TextBuffer textBuffer)
        {
            WordSpanKind lexMode = WordSpanKind.Unknown;
            //simple line break / whitespace lexer
            char[] buffer = textBuffer.UnsafeGetInternalBuffer();
            int j = buffer.Length;

            int startIndex = 0;
            int len = 0;

            _spans.Clear();

            for (int i = 0; i < j; ++i)
            {
                char c = buffer[i];
                if (c == '\r')
                {
                    if (len > 0)
                    {
                        //TODO: check if len > int? 
                        var newspan = new LexWordSpan(startIndex,
                           (ushort)len, lexMode);
                        _spans.Add(newspan);

                        startIndex = i;
                        len = 0;
                    }
                    len++;
                    lexMode = WordSpanKind.NewLine;
                    //if next is '\n'
                    if (i < j - 1)
                    {
                        char c1 = buffer[i + 1];
                        if (c1 == '\n')
                        {
                            //push accum wordspan
                            // 
                            //newline wordspan
                            i++;
                            len++;
                            var newspan = new LexWordSpan(startIndex,
                                (ushort)len, lexMode);
                            _spans.Add(newspan);
                            startIndex += len;
                            len = 0;

                        }
                        else
                        {
                            var newspan = new LexWordSpan(startIndex,
                               (ushort)len, lexMode);
                            _spans.Add(newspan);
                            startIndex += len;
                            len = 0;

                        }
                    }
                    else
                    {
                        //the \r is last char
                        var newspan = new LexWordSpan(startIndex,
                               (ushort)len, lexMode);
                        _spans.Add(newspan);

                        startIndex += len;
                        len = 0;
                    }
                }
                else if (c == '\n')
                {
                    //new line?
                    if (len > 0)
                    {
                        //TODO: check if len > int? 
                        var newspan = new LexWordSpan(startIndex,
                           (ushort)len, lexMode);
                        _spans.Add(newspan);
                        startIndex = i;
                        len = 0;
                    }
                    //
                    lexMode = WordSpanKind.NewLine;
                    var newspan2 = new LexWordSpan(startIndex,
                             (ushort)len, lexMode);
                    _spans.Add(newspan2);
                    len = 0;
                    startIndex = i;
                }
                else if (c == '\t')
                {
                    //tab character
                    //
                    if (lexMode != WordSpanKind.Tab)
                    {
                        //flush
                        if (len > 0)
                        {
                            var newspan = new LexWordSpan(startIndex,
                              (ushort)len, lexMode);
                            _spans.Add(newspan);
                            startIndex = i;
                            len = 0;
                        }
                    }
                    len++;
                    lexMode = WordSpanKind.Tab;
                }
                else if (char.IsWhiteSpace(c))
                {
                    //whitespace
                    if (lexMode != WordSpanKind.WhiteSpace)
                    {
                        //change mode
                        if (len > 0)
                        {
                            //TODO: check if len > int? 
                            var newspan = new LexWordSpan(startIndex,
                               (ushort)len, lexMode);
                            _spans.Add(newspan);
                            startIndex = i;
                            len = 0;
                        }
                    }
                    len++;
                    lexMode = WordSpanKind.WhiteSpace;
                }
                else
                {
                    if (lexMode != WordSpanKind.Text)
                    {
                        //change mode
                        if (len > 0)
                        {
                            //TODO: check if len > int? 
                            var newspan = new LexWordSpan(startIndex,
                               (ushort)len, lexMode);
                            _spans.Add(newspan);
                            startIndex = i;
                            len = 0;
                        }
                    }
                    len++;
                    lexMode = WordSpanKind.Text;
                }
            }
            //
            //finish
            if (len > 0)
            {
                var newspan = new LexWordSpan(startIndex,
                              (ushort)len, lexMode);
                _spans.Add(newspan);
                len = 0;
            }

        }
    }


    public struct LexWordSpan
    {
        public int start;
        public ushort len; //span should not too long more than ushort.Max
        public WordSpanKind kind;
        public LexWordSpan(int start, ushort len, WordSpanKind kind)
        {
            this.start = start;
            this.len = len;
            this.kind = kind;
        }
#if DEBUG
        public override string ToString()
        {
            return "(" + kind + ")," + start + "," + len;
        }
#endif
    }


}