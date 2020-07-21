//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using Typography.Text;
using Typography.TextBreak;

namespace Typography.TextLayout
{


    public class FormattedGlyphPlanList
    {

        int _newElemIndex = 0;
        const int DEFAULT_LEN = 255;

        readonly List<FormattedGlyphPlanSeq> _list;
        public FormattedGlyphPlanList()
        {
            _list = new List<FormattedGlyphPlanSeq>(DEFAULT_LEN);
            for (int i = 0; i < DEFAULT_LEN; ++i)
            {
                _list.Add(new FormattedGlyphPlanSeq());
            }
        }

        public FormattedGlyphPlanSeq this[int index]
        {
            get
            {
                if (index < 0) { throw new NotSupportedException(); }

                if (index > _list.Count) { throw new NotSupportedException(); }

                if (index >= _newElemIndex) { throw new NotSupportedException(); }


                return _list[index];
            }
        }
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
        public FormattedGlyphPlanSeq AppendNew()
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
            _newElemIndex++;

            return fmtGlyphPlanSeq;
        }
        public bool IsRightToLeftDirection { get; set; }
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
        }


    }

}