//MIT, 2016-present, WinterDev, Sam Hocevar

using System;
using System.Collections.Generic;
using Typography.Text;
using Typography.TextBreak;

namespace Typography.TextLayout
{

    public abstract class FormattedGlyphPlanSeqProvider
    {
        public bool IsRightToLeftDirection { get; set; }
        public abstract FormattedGlyphPlanSeq AppendNew();
    }

    public class FormattedGlyphPlanSeqPool : FormattedGlyphPlanSeqProvider
    {

        int _newElemIndex = 0;
        const int DEFAULT_LEN = 255;

        readonly List<FormattedGlyphPlanSeq> _list;
        public FormattedGlyphPlanSeqPool()
        {
            _list = new List<FormattedGlyphPlanSeq>(DEFAULT_LEN);
            for (int i = 0; i < DEFAULT_LEN; ++i)
            {
                _list.Add(new FormattedGlyphPlanSeq());
            }
        }

        public FormattedGlyphPlanSeq GetFirst() => _list[0];
        public FormattedGlyphPlanSeq GetLast() => _list[_newElemIndex - 1];

        public int Count => _newElemIndex;
        public void Clear()
        {
            //return all back to pool
            for (int i = _newElemIndex - 1; i >= 0; --i)
            {
                _list[i].Reset();
            }
            _newElemIndex = 0;
            IsRightToLeftDirection = false;
        }
        public override FormattedGlyphPlanSeq AppendNew()
        {
            if (_newElemIndex + 1 > _list.Count)
            {
                //alloc more
                for (int i = 0; i < 64; ++i)
                {
                    _list.Add(new FormattedGlyphPlanSeq());
                }
                _list.TrimExcess();
            }

            FormattedGlyphPlanSeq fmtGlyphPlanSeq = _list[_newElemIndex];
            if (_newElemIndex > 0)
            {
                //link next-prev from pool
                _list[_newElemIndex - 1].Next = fmtGlyphPlanSeq;
                fmtGlyphPlanSeq.Prev = _list[_newElemIndex - 1];
            }
            _newElemIndex++;

            return fmtGlyphPlanSeq;
        }

    }



    public class FormattedGlyphPlanSeq
    {

        internal FormattedGlyphPlanSeq() { }
        static readonly GlyphPlanSequence s_EmptyGlypgPlanSeq = new GlyphPlanSequence();

        public GlyphPlanSequence Seq { get; private set; } = GlyphPlanSequence.Empty;

        public ResolvedFont ResolvedFont { get; private set; }

        public SpanBreakInfo BreakInfo { get; private set; }
        /// <summary>
        /// whitespace count at the end of this seq
        /// </summary>
        public ushort PostfixWhitespaceCount { get; set; }
        /// <summary>
        /// whitespace count at the begin of this seq
        /// </summary>
        public ushort PrefixWhitespaceCount { get; set; }

        public bool ColorGlyphOnTransparentBG { get; set; }

        public void SetData(GlyphPlanSequence seq, ResolvedFont resolvedFont, SpanBreakInfo spBreakInfo)
        {
            Seq = seq;
            ResolvedFont = resolvedFont;
            BreakInfo = spBreakInfo;
        }
        public bool IsEmpty() => Seq.IsEmpty();
        public void Reset()
        {
            Seq = s_EmptyGlypgPlanSeq;
            ResolvedFont = null;
            BreakInfo = null;
            ColorGlyphOnTransparentBG = false;
            PrefixWhitespaceCount = PostfixWhitespaceCount = 0;
            Next = null; //
            Prev = null;
        }

        //single-linked node

        FormattedGlyphPlanSeq _next;
        FormattedGlyphPlanSeq _prev;
        public FormattedGlyphPlanSeq Next
        {
            get => _next;
            set
            {
                if (_next == this) { throw new NotSupportedException(); }
                _next = value;
            }
        }
        public FormattedGlyphPlanSeq Prev
        {
            get => _prev;
            set
            {
                if (_prev == this) { throw new NotSupportedException(); }
                _prev = value;
            }
        }
        //-----------

    }

}