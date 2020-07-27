//MIT, 2016-present, WinterDev
//some code from ICU project with BSD license

using System.Collections.Generic;
using System.Text;
using Typography.OpenFont;
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
        readonly static SpanBreakInfo s_brkArabic = new SpanBreakInfo(Unicode13RangeInfoList.Arabic, true, ScriptTagDefs.Arabic.Tag);
        readonly static SpanBreakInfo s_brkArabicSupplement = new SpanBreakInfo(Unicode13RangeInfoList.Arabic_Supplement, true, ScriptTagDefs.Arabic.Tag);
        readonly static SpanBreakInfo s_brkArabicExtendA = new SpanBreakInfo(Unicode13RangeInfoList.Arabic_Extended_A, true, ScriptTagDefs.Arabic.Tag);
        readonly static SpanBreakInfo s_brkArabicPresentFormA = new SpanBreakInfo(Unicode13RangeInfoList.Arabic_Presentation_Forms_A, true, ScriptTagDefs.Arabic.Tag);
        readonly static SpanBreakInfo s_brkArabicPresentFormB = new SpanBreakInfo(Unicode13RangeInfoList.Arabic_Presentation_Forms_B, true, ScriptTagDefs.Arabic.Tag);

        static bool IsArabicChar(char c, out SpanBreakInfo brInfo)
        {
            brInfo = s_brkArabic;
            if (brInfo.UnicodeRange.IsInRange(c)) return true;

            brInfo = s_brkArabicSupplement;
            if (brInfo.UnicodeRange.IsInRange(c)) return true;

            brInfo = s_brkArabicExtendA;
            if (brInfo.UnicodeRange.IsInRange(c)) return true;


            brInfo = s_brkArabicPresentFormA;
            if (brInfo.UnicodeRange.IsInRange(c)) return true;

            brInfo = s_brkArabicPresentFormB;
            if (brInfo.UnicodeRange.IsInRange(c)) return true;

            //
            brInfo = null;
            return false;
        }

        public override bool CanHandle(char c) => IsArabicChar(c, out _);

        readonly RunAdapter _runAdapter = new RunAdapter();

        //internal override void BreakWord(WordVisitor visitor, int[] charBuff, int startAt, int len)
        //{
        //    //input is in utf32 buffer

        //    visitor.State = VisitorState.Parsing;
        //    RunAgent agent = _runAdapter.Agent;

        //    //collect arabic char and break

        //    int arabic_len = 0;
        //    int lim = startAt + len;

        //    SpanBreakInfo latest_ar = null;
        //    for (int i = startAt; i < lim; ++i)
        //    {
        //        int c = charBuff[i];
        //        char lower = (char)c;
        //        if (IsArabicChar(lower, out SpanBreakInfo spBreak))
        //        {
        //            arabic_len++;
        //            latest_ar = spBreak;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    //
        //    if (arabic_len == 0)
        //    {
        //        visitor.State = VisitorState.OutOfRangeChar;
        //        return;
        //    }


        //    visitor.SpanBreakInfo = latest_ar;

        //    //only collect char
        //    Line line1;
        //    unsafe
        //    {

        //        fixed (int* buffer_head = &charBuff[0])
        //        {
        //            byte* buffer_h1 = (byte*)buffer_head;
        //            char[] buff = new char[charBuff.Length * 2];
        //            fixed (char* output1 = &buff[0])
        //            {
        //                int output_len = Encoding.UTF32.GetChars(buffer_h1, charBuff.Length * 4, output1, buff.Length);
        //                line1 = new Line(new string(output1, 0, output_len));
        //            }
        //        }
        //    }

        //    _runAdapter.LoadLine(line1);

        //    while (_runAdapter.MoveNext())
        //    {
        //        int offset = agent.Offset;
        //        byte level = agent.Level;
        //        int sp_len = agent.Length;
        //        bool rtl = agent.IsRightToLeft;

        //        if (rtl)
        //        {
        //            //temp fix
        //            visitor.AddWordBreak_AndSetCurrentIndex(startAt + sp_len, WordKind.Text);
        //        }
        //        else
        //        {
        //            //use other engine
        //            break;
        //        }
        //        //iter each run-span
        //        //string tt = new string(buffer, offset, len);
        //        //System.Diagnostics.Debug.WriteLine(tt);
        //    }

        //    if (visitor.CurrentIndex == startAt + len)
        //    {
        //        visitor.State = VisitorState.End;
        //    }
        //    else
        //    {
        //        //continue to other parser
        //        visitor.State = VisitorState.OutOfRangeChar;
        //    }
        //}


        readonly List<char> _arabicBuffer = new List<char>();
        internal override void BreakWord(WordVisitor visitor)
        {
            //collect arabic char and break


            int arabic_len = 0;
            int startAt = visitor.CurrentIndex;

            SpanBreakInfo latest_ar = null;
            _arabicBuffer.Clear();

            for (; !visitor.IsEnd;)
            {
                char c = visitor.C0;
                if (IsArabicChar(c, out SpanBreakInfo spBreak))
                {
                    arabic_len++;
                    _arabicBuffer.Add(c);
                    latest_ar = spBreak;
                    visitor.Read();//read next
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
            //----------------
            visitor.SpanBreakInfo = latest_ar;

            //only collect char

            RunAgent agent = _runAdapter.Agent;
            Line line1 = new Line(new string(_arabicBuffer.ToArray()));
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
                    visitor.AddWordBreak_AndSetCurrentIndex(startAt + sp_len, WordKind.Text);
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

            if (visitor.IsEnd)
            {
                visitor.State = VisitorState.End;
            }
            else
            {
                //continue to other parser
                visitor.State = VisitorState.OutOfRangeChar;
            }
        }
        //internal override void BreakWord(WordVisitor visitor, char[] charBuff, int startAt, int len)
        //{
        //    //use custom parsing

        //    visitor.State = VisitorState.Parsing;
        //    RunAgent agent = _runAdapter.Agent;

        //    //collect arabic char and break

        //    int arabic_len = 0;
        //    int lim = startAt + len;

        //    SpanBreakInfo latest_ar = null;
        //    for (int i = startAt; i < lim; ++i)
        //    {
        //        char c = charBuff[i];
        //        if (IsArabicChar(c, out SpanBreakInfo spBreak))
        //        {
        //            arabic_len++;
        //            latest_ar = spBreak;
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    //
        //    if (arabic_len == 0)
        //    {
        //        visitor.State = VisitorState.OutOfRangeChar;
        //        return;
        //    }


        //    visitor.SpanBreakInfo = latest_ar;

        //    //only collect char
        //    Line line1 = new Line(new string(charBuff, startAt, arabic_len));
        //    _runAdapter.LoadLine(line1);

        //    while (_runAdapter.MoveNext())
        //    {
        //        int offset = agent.Offset;
        //        byte level = agent.Level;
        //        int sp_len = agent.Length;
        //        bool rtl = agent.IsRightToLeft;

        //        if (rtl)
        //        {
        //            //temp fix
        //            visitor.AddWordBreak_AndSetCurrentIndex(startAt + sp_len, WordKind.Text);
        //        }
        //        else
        //        {
        //            //use other engine
        //            break;
        //        }
        //        //iter each run-span
        //        //string tt = new string(buffer, offset, len);
        //        //System.Diagnostics.Debug.WriteLine(tt);
        //    }

        //    if (visitor.CurrentIndex == startAt + len)
        //    {
        //        visitor.State = VisitorState.End;
        //    }
        //    else
        //    {
        //        //continue to other parser
        //        visitor.State = VisitorState.OutOfRangeChar;
        //    }

        //}
    }
}