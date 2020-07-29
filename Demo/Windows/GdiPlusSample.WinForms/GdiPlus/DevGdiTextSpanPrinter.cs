//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using Typography.OpenFont;
using Typography.OpenFont.Contours;
using Typography.OpenFont.Tables;
using Typography.TextLayout;
using Typography.Text;

namespace SampleWinForms
{

    /// <summary>
    /// developer's version, Gdi+ text-span printer
    /// </summary>
    partial class DevGdiTextSpanPrinter : AbstractTextSpanPrinter
    {
        Typeface _currentTypeface;
        GlyphOutlineBuilder _currentGlyphPathBuilder;

        readonly GlyphTranslatorToGdiPath _txToGdiPath;
        readonly TextServiceClient _txtClient;
        readonly Dictionary<Typeface, GlyphOutlineBuilder> _outlineBuilderCaches = new Dictionary<Typeface, GlyphOutlineBuilder>();
        readonly SolidBrush _fillBrush = new SolidBrush(Color.Black);
        readonly Pen _outlinePen = new Pen(Color.Green);
        //
        //for optimization
        readonly GlyphMeshCollection<GraphicsPath> _glyphMeshCollections = new GlyphMeshCollection<GraphicsPath>();

        public DevGdiTextSpanPrinter(TextServiceClient txtClient)
        {
            _txtClient = txtClient;
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
            _txToGdiPath = new GlyphTranslatorToGdiPath();
        }

        public HintTechnique HintTechnique { get; set; }
        public AlternativeTypefaceSelector AlternativeTypefaceSelector
        {
            get => _txtClient.AlternativeTypefaceSelector;
            set => _txtClient.AlternativeTypefaceSelector = value;
        }

        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {
                //check if we change it or not
                if (value == _currentTypeface) return;
                //change ...
                //check if we have used this typeface before?
                //if not, create a proper glyph builder for it
                //--------------------------------
                //reset 
                _currentTypeface = value;
                _currentGlyphPathBuilder = null;

                //--------------------------------
                if (value == null) return;
                //--------------------------------

                //2. glyph builder
                if (!_outlineBuilderCaches.TryGetValue(_currentTypeface, out _currentGlyphPathBuilder))
                {
                    //1 glyph outline builder per typeface
                    _outlineBuilderCaches.Add(_currentTypeface,
                        _currentGlyphPathBuilder = new GlyphOutlineBuilder(_currentTypeface));
                }

                OnFontSizeChanged();
            }
        }
        protected override void OnFontSizeChanged()
        {
            _txtClient.SetCurrentFont(_currentTypeface, this.FontSizeInPoints, ScriptLang);
            base.OnFontSizeChanged();
        }

        public bool EnableColorGlyph { get; set; } = true;
        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

        readonly UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();

        public void UpdateGlyphLayoutSettings()
        {
            _txtClient.SetCurrentFont(this.Typeface, this.FontSizeInPoints, ScriptLang, this.PositionTechnique);
        }
        void UpdateVisualOutputSettings()
        {
            _currentGlyphPathBuilder.SetHintTechnique(this.HintTechnique);
            _fillBrush.Color = this.FillColor;
            _outlinePen.Color = this.OutlineColor;
        }

        GraphicsPath GetExistingOrCreateGraphicsPath(ushort glyphIndex)
        {
            if (!_glyphMeshCollections.TryGetCacheGlyph(glyphIndex, out GraphicsPath path))
            {
                _txToGdiPath.Reset(); //clear

                //if not found then create a new one
                _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphIndex, this.FontSizeInPoints, _txToGdiPath);
                path = _txToGdiPath.ResultGraphicsPath;

                //register
                _glyphMeshCollections.RegisterCachedGlyph(glyphIndex, path);
            }

            return path;
        }

        public void MeasureGlyphPlanSeq(GlyphPlanSequence seq, out int width)
        {

            var snapToPxScale = new GlyphPlanSequenceSnapPixelScaleLayout(
                seq,
                seq.startAt,
                seq.len,
                 _currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints));

            snapToPxScale.ReadToEnd();
            width = snapToPxScale.AccumWidth;

        }
        public void MeasureGlyphPlanSeq(FormattedGlyphPlanSeq seq1, out int width)
        {
            width = 0;
            while (seq1 != null)
            {
                GlyphPlanSequence seq = seq1.Seq;

                //TODO: add 
                ResolvedFont resolvedFont = seq1.ResolvedFont;

                width += seq1.PrefixWhitespaceCount * resolvedFont.WhitespaceWidth;

                var snapToPxScale = new GlyphPlanSequenceSnapPixelScaleLayout(
                    seq,
                    seq.startAt,
                    seq.len,
                    resolvedFont.GetScaleToPixelFromPointUnit());


                snapToPxScale.ReadToEnd();

                width += snapToPxScale.AccumWidth;
                width += seq1.PostfixWhitespaceCount * resolvedFont.WhitespaceWidth;

                seq1 = seq1.Next;
            }

        }
        public void MeasureGlyphPlanSeq(GlyphPlanSequence seq, int startAt, int len, out int width)
        {
            var snapToPxScale = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len,
                 _currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints));

            snapToPxScale.ReadToEnd();
            width = snapToPxScale.AccumWidth;
        }

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float x, float y)
        {
            UpdateVisualOutputSettings();

            //draw data in glyph plan 
            //3. render each glyph

            float sizeInPoints = this.FontSizeInPoints;
            float pxscale = _currentTypeface.CalculateScaleToPixelFromPointSize(sizeInPoints);


            Typeface typeface = this.Typeface;
            _glyphMeshCollections.SetCacheInfo(typeface, sizeInPoints, this.HintTechnique);


            //this draw a single line text span*** 
            Graphics g = this.TargetGraphics;
            float baseline = y;
            var snapToPxScale = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, pxscale);


            COLR colrTable = typeface.COLRTable;
            CPAL cpalTable = typeface.CPALTable;

            bool canUseColorGlyph = EnableColorGlyph && colrTable != null && cpalTable != null;

            while (snapToPxScale.Read())
            {
                ushort glyphIndex = snapToPxScale.CurrentGlyphIndex;

                if (canUseColorGlyph && colrTable.LayerIndices.TryGetValue(glyphIndex, out ushort colorLayerStart))
                {

                    ushort colorLayerCount = colrTable.LayerCounts[glyphIndex];

                    for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                    {

                        GraphicsPath path = GetExistingOrCreateGraphicsPath(colrTable.GlyphLayers[c]);
                        if (path == null)
                        {
                            //???
#if DEBUG
                            System.Diagnostics.Debug.WriteLine("gdi_printer: no path?");
#endif
                            continue;
                        }

                        //------
                        //then move pen point to the position we want to draw a glyph
                        float cx = (float)Math.Round(snapToPxScale.ExactX + x);
                        float cy = (float)Math.Floor(snapToPxScale.ExactY + baseline);

                        int palette = 0; // FIXME: assume palette 0 for now 
                        cpalTable.GetColor(
                            cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                            out byte red, out byte green, out byte blue, out byte alpha);

                        g.TranslateTransform(cx, cy);

                        _fillBrush.Color = Color.FromArgb(red, green, blue);//***
                        if (FillBackground)
                        {
                            g.FillPath(_fillBrush, path);
                        }
                        if (DrawOutline)
                        {
                            g.DrawPath(_outlinePen, path);
                        }
                        //and then we reset back ***
                        g.TranslateTransform(-cx, -cy);
                    }
                    _latestAccumulateWidth = snapToPxScale.AccumWidth;
                }
                else
                {
                    GraphicsPath path = GetExistingOrCreateGraphicsPath(glyphIndex);

                    if (path == null)
                    {
                        //???
#if DEBUG
                        System.Diagnostics.Debug.WriteLine("gdi_printer: no path?");
#endif
                        continue;
                    }

                    //------
                    //then move pen point to the position we want to draw a glyph
                    float cx = (float)Math.Round(snapToPxScale.ExactX + x);
                    float cy = (float)Math.Floor(snapToPxScale.ExactY + baseline);

                    g.TranslateTransform(cx, cy);

                    if (FillBackground)
                    {
                        g.FillPath(_fillBrush, path);
                    }
                    if (DrawOutline)
                    {
                        g.DrawPath(_outlinePen, path);
                    }
                    //and then we reset back ***
                    g.TranslateTransform(-cx, -cy);

                    _latestAccumulateWidth = snapToPxScale.AccumWidth;
                }
            }
        }


        FormattedGlyphPlanSeqPool _glyphPlanSeqPool = new FormattedGlyphPlanSeqPool();

        int _latestAccumulateWidth;
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

#if DEBUG
            if (textBuffer.Length == 2)
            {

            }
#endif

            _latestAccumulateWidth = 0;
            var buffSpan = new Typography.Text.TextBufferSpan(textBuffer, startAt, len);

            if (!EnableMultiTypefaces)
            {
                _reusableUnscaledGlyphPlanList.Clear();
                GlyphPlanSequence seq = _txtClient.CreateGlyphPlanSeq(buffSpan, _currentTypeface, FontSizeInPoints);
                DrawFromGlyphPlans(seq, x, y);
            }
            else
            {
                Typeface defaultTypeface = _currentTypeface; //save, restore later
                _glyphPlanSeqPool.Clear();
                _txtClient.PrepareFormattedStringList(textBuffer, startAt, len, _glyphPlanSeqPool);
                int count = _glyphPlanSeqPool.Count;//re-count

                bool needRightToLeftArr = _glyphPlanSeqPool.IsRightToLeftDirection;
                float xpos = x;

                if (needRightToLeftArr)
                {
                    //special arr left-to-right
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = _glyphPlanSeqPool.GetLast();
                    while (formattedGlyphPlanSeq != null)
                    {
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                        formattedGlyphPlanSeq = formattedGlyphPlanSeq.Prev;
                    }
                }
                else
                {
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = _glyphPlanSeqPool.GetFirst();
                    while (formattedGlyphPlanSeq != null)
                    {


                        //change typeface                     
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                        formattedGlyphPlanSeq = formattedGlyphPlanSeq.Next;
                    }
                }

                ClearTempFormattedGlyphPlanSeq();

                Typeface = defaultTypeface; //restore
            }
        }

        ///// <summary>
        ///// generate glyph plan for current typeface only (EnableMutliTypeface always=> false)
        ///// </summary>
        ///// <param name="textBuffer"></param>
        ///// <param name="startAt"></param>
        ///// <param name="len"></param>
        ///// <param name="unscaledGlyphPlan"></param>
        //public void GenerateGlyphPlans(
        //      char[] textBuffer,
        //      int startAt,
        //      int len,
        //      IUnscaledGlyphPlanList unscaledGlyphPlanList)
        //{
        //    var sp1 = new Typography.Text.TextBufferSpan(textBuffer, startAt, len);
        //    _txtClient.CreateGlyphPlanSeq(sp1, unscaledGlyphPlanList);
        //}

        /// <summary>
        /// generate glyph plan with MultipleTypeface= true
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="pool"></param>
        public void GenerateGlyphPlans(
            char[] textBuffer,
            int startAt,
            int len,
            FormattedGlyphPlanSeqProvider ftmGlyphPlans)
        {
            //similar to draw
            _txtClient.PrepareFormattedStringList(textBuffer, startAt, len, ftmGlyphPlans);
        }
        public void MeasureGlyphPlanList(FormattedGlyphPlanSeqPool list, out int width)
        {
            //layout and measure length

            bool needRightToLeftArr = false;
            int xpos = 0;
            if (needRightToLeftArr)
            {
                //special arr left-to-right
                int count = list.Count;//re-count

                FormattedGlyphPlanSeq formattedGlyphPlanSeq = list.GetLast();
                while (formattedGlyphPlanSeq != null)
                {
                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    xpos += (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount);

                    GlyphPlanSequence seq = formattedGlyphPlanSeq.Seq;
                    MeasureGlyphPlanSeq(seq, 0, seq.Count, out int w);

                    xpos += w + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    formattedGlyphPlanSeq = formattedGlyphPlanSeq.Prev;
                }
                width = xpos;
            }
            else
            {
                int count = list.Count;//re-count
                FormattedGlyphPlanSeq formattedGlyphPlanSeq = list.GetFirst();
                while (formattedGlyphPlanSeq != null)
                {

                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    xpos += (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount);

                    GlyphPlanSequence seq = formattedGlyphPlanSeq.Seq;
                    MeasureGlyphPlanSeq(seq, 0, seq.Count, out int w);

                    xpos += w + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    formattedGlyphPlanSeq = formattedGlyphPlanSeq.Next;
                }
                width = xpos;
            }
        }
        public void DrawFromFormattedGlyphPlans(FormattedGlyphPlanSeqPool list, float x, float y)
        {
            bool needRightToLeftArr = false;

            float xpos = x;
            if (needRightToLeftArr)
            {
                //special arr left-to-right
                int count = list.Count;//re-count
                FormattedGlyphPlanSeq formattedGlyphPlanSeq = list.GetLast();
                while (formattedGlyphPlanSeq != null)
                {
                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                    xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    formattedGlyphPlanSeq = formattedGlyphPlanSeq.Prev;
                }
            }
            else
            {
                int count = list.Count;//re-count
                FormattedGlyphPlanSeq formattedGlyphPlanSeq = list.GetFirst();
                for (int i = 0; i < count; ++i)
                {
                    //change typeface                     
                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                    xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    formattedGlyphPlanSeq = formattedGlyphPlanSeq.Next;
                }
            }
        }

        void ClearTempFormattedGlyphPlanSeq()
        {

        }


    }


}
