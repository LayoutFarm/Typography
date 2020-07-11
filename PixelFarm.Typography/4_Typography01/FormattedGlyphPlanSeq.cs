//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using Typography.Text;
using Typography.TextLayout;

namespace PixelFarm.Drawing
{
    //utils for help text printer

    public class FormattedGlyphPlanSeqPool
    {
        Queue<FormattedGlyphPlanSeq> _pool = new Queue<FormattedGlyphPlanSeq>();

        public FormattedGlyphPlanSeq GetFreeFmtGlyphPlanSeqs() => (_pool.Count > 0) ? _pool.Dequeue() : new FormattedGlyphPlanSeq();
        public void ReleaseFmtGlyphPlanSeqs(FormattedGlyphPlanSeq seq)
        {
            seq.Reset();
            _pool.Enqueue(seq);
        }
    }

    public class FormattedGlyphPlanSeq
    {
        static readonly GlyphPlanSequence s_EmptyGlypgPlanSeq = new GlyphPlanSequence();

        public GlyphPlanSequence Seq { get; private set; } = GlyphPlanSequence.Empty;

        public ResolvedFont ResolvedFont { get; private set; }

        /// <summary>
        /// whitespace count at the end of this seq
        /// </summary>
        public ushort PostfixWhitespaceCount { get; set; }
        /// <summary>
        /// whitespace count at the begin of this seq
        /// </summary>
        public ushort PrefixWhitespaceCount { get; set; }

        public bool ColorGlyphOnTransparentBG { get; set; }

        public void SetData(GlyphPlanSequence seq, ResolvedFont resolvedFont)
        {
            Seq = seq;
            ResolvedFont = resolvedFont;
        }
        public bool IsEmpty() => Seq.IsEmpty();
        public void Reset()
        {
            Seq = s_EmptyGlypgPlanSeq;
            ResolvedFont = null;
            ColorGlyphOnTransparentBG = false;
            PrefixWhitespaceCount = PostfixWhitespaceCount = 0;
        }


    }

}