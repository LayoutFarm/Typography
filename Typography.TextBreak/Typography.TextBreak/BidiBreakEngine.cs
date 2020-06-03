//MIT, 2016-present, WinterDev
//some code from ICU project with BSD license

using Typography.TextBreak.SheenBidi;

namespace Typography.TextBreak
{
    class BidiBreakEngine : BreakingEngine
    {
        public BidiBreakEngine()
        {
            //use with other breaking eng
        }
        public override bool CanBeStartChar(char c)
        {
            //temp fix
            return CanHandle(c);
        }
        static bool IsArabicChar(char c)
        {
            //https://en.wikipedia.org/wiki/Arabic_script_in_Unicode             

            //Rumi Numeral Symbols(10E60–10E7F, 31 characters)
            //Indic Siyaq Numbers(1EC70–1ECBF, 68 characters)
            //Ottoman Siyaq Numbers(1ED00–1ED4F, 61 characters)
            //Arabic Mathematical Alphabetic Symbols(1EE00–1EEFF, 143 characters)

            if (c >= 0x0600 && c <= 0x06FF)
            {
                //Arabic (0600–06FF, 255 characters)
                return true;
            }
            else if (c >= 0x0750 && c <= 0x077F)
            { //Arabic Supplement(0750–077F, 48 characters)
                return true;
            }
            else if (c >= 0x8A0 && c <= 0x08FF)
            {
                //Arabic Extended-A(08A0–08FF, 84 characters)
                return true;
            }
            else if (c >= 0xFB50 && c <= 0xFDFF)
            {  //Arabic Presentation Forms - A(FB50–FDFF, 611 characters)
                return true;
            }
            else if (c >= 0xFE70 && c <= 0xFEFF)
            {
                //Arabic Presentation Forms - B(FE70–FEFF, 141 characters)
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool CanHandle(char c) => IsArabicChar(c);

        RunAdapter _runAdapter = new RunAdapter();
        readonly SpanLayoutInfo _spanLayoutInfo = new SpanLayoutInfo(true, 0x0600, "arab");

        internal override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        {
            //use custom parsing
             
            visitor.State = VisitorState.Parsing;
            RunAgent agent = _runAdapter.Agent;

            //collect arabic char and break

            int arabic_len = 0;
            int lim = startAt + len;


            for (int i = startAt; i < lim; ++i)
            {
                char c = charBuff[i];
                if (IsArabicChar(c))
                {
                    arabic_len++;
                }
                else
                {
                    break;
                }
            }
            //
            if (arabic_len == 0)
            {
                visitor.State = VisitorState.OutOfRangeChar;
                return;
            }


            visitor.SpanLayoutInfo = _spanLayoutInfo;

            //only collect char
            Line line1 = new Line(new string(charBuff, startAt, arabic_len));
            _runAdapter.LoadLine(line1);

            while (_runAdapter.MoveNext())
            {
                int offset = agent.Offset;
                byte level = agent.Level;
                int sp_len = agent.Length;
                bool rtl = agent.IsRightToLeft;

                if (rtl)
                {
                    //temp fix
                    visitor.AddWordBreakAt(startAt + sp_len, WordKind.Text);
                    visitor.SetCurrentIndex(startAt + sp_len);
                }
                else
                {
                    //use other engine
                    break;
                }
                //iter each run-span
                //string tt = new string(buffer, offset, len);
                //System.Diagnostics.Debug.WriteLine(tt);
            }

            if (visitor.CurrentIndex == startAt + len)
            {
                visitor.State = VisitorState.End;
            }
            else
            {
                //continue to other parser
                visitor.State = VisitorState.OutOfRangeChar;
            }

        }
    }
}