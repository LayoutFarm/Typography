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
    sealed partial class DevGdiTextPrinter : AbstractTextSpanPrinter
    {
        Typeface _currentTypeface;
        GlyphOutlineBuilder _currentGlyphPathBuilder;

        readonly TextServiceClient _txtClient;
        readonly GlyphTranslatorToGdiPath _txToGdiPath = new GlyphTranslatorToGdiPath();
        readonly SolidBrush _fillBrush = new SolidBrush(Color.Black);
        readonly Pen _outlinePen = new Pen(Color.Green);

        readonly GlyphMeshCollection<GraphicsPath> _glyphMeshCollections = new GlyphMeshCollection<GraphicsPath>();
        readonly Dictionary<Typeface, GlyphOutlineBuilder> _cacheGlyphOutlineBuilders = new Dictionary<Typeface, GlyphOutlineBuilder>();
        readonly UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();

        public DevGdiTextPrinter(TextServiceClient txtClient)
        {
            _txtClient = txtClient;
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
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
                if (!_cacheGlyphOutlineBuilders.TryGetValue(value, out _currentGlyphPathBuilder))
                {
                    //not found,
                    //create a new one
                    _currentGlyphPathBuilder = new GlyphOutlineBuilder(_currentTypeface);
                    _cacheGlyphOutlineBuilders.Add(value, _currentGlyphPathBuilder);
                }

                //4.
                OnFontSizeChanged();
            }
        }
        protected override void OnFontSizeChanged()
        {
            _txtClient.SetCurrentFont(_currentTypeface, this.FontSizeInPoints, ScriptLang);
            base.OnFontSizeChanged();
        }
        public void UpdateGlyphLayoutSettings()
        {
            _txtClient.SetCurrentFont(this.Typeface, this.FontSizeInPoints, ScriptLang);
        }
        public bool EnableColorGlyph { get; set; } = true;
        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

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


        readonly FormattedGlyphPlanList _fmtGlyphPlanList = new FormattedGlyphPlanList();
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

                _fmtGlyphPlanList.Clear();
                _txtClient.PrepareFormattedStringList(textBuffer, startAt, len, _fmtGlyphPlanList);

                int count = _fmtGlyphPlanList.Count;//re-count

                bool needRightToLeftArr = _fmtGlyphPlanList.IsRightToLeftDirection;
                float xpos = x;

                if (needRightToLeftArr)
                {
                    //special arr left-to-right

                    for (int i = count - 1; i >= 0; --i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlanList[i];

                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                    }
                }
                else
                {

                    for (int i = 0; i < count; ++i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlanList[i];

                        //change typeface                     
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    }
                }

                ClearTempFormattedGlyphPlanSeq();

                Typeface = defaultTypeface; //restore
            }
        }

        /// <summary>
        /// generate glyph plan for current typeface only (EnableMutliTypeface always=> false)
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <param name="unscaledGlyphPlan"></param>
        public void GenerateGlyphPlans(
              char[] textBuffer,
              int startAt,
              int len,
              IUnscaledGlyphPlanList unscaledGlyphPlanList)
        {
            var sp1 = new Typography.Text.TextBufferSpan(textBuffer, startAt, len);
            _txtClient.CreateGlyphPlanSeq(sp1, unscaledGlyphPlanList);

        }

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
            FormattedGlyphPlanList ftmGlyphPlans)
        {
            //similar to draw
            _txtClient.PrepareFormattedStringList(textBuffer, startAt, len, ftmGlyphPlans);
        }
        public void MeasureGlyphPlanList(FormattedGlyphPlanList list, out int width)
        {
            //layout and measure length

            bool needRightToLeftArr = false;
            int xpos = 0;
            if (needRightToLeftArr)
            {
                //special arr left-to-right
                int count = list.Count;//re-count

                for (int i = count - 1; i >= 0; --i)
                {
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = list[i];

                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    xpos += (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount);

                    GlyphPlanSequence seq = formattedGlyphPlanSeq.Seq;
                    MeasureGlyphPlanSeq(seq, 0, seq.Count, out int w);

                    xpos += w + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                }
                width = xpos;
            }
            else
            {
                int count = list.Count;//re-count

                for (int i = 0; i < count; ++i)
                {
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = list[i];

                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    xpos += (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount);

                    GlyphPlanSequence seq = formattedGlyphPlanSeq.Seq;
                    MeasureGlyphPlanSeq(seq, 0, seq.Count, out int w);

                    xpos += w + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                }
                width = xpos;
            }
        }
        public void DrawFromFormattedGlyphPlans(FormattedGlyphPlanList list, float x, float y)
        {
            bool needRightToLeftArr = false;

            float xpos = x;
            if (needRightToLeftArr)
            {
                //special arr left-to-right
                int count = list.Count;//re-count
                for (int i = count - 1; i >= 0; --i)
                {
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = list[i];

                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                    xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                }
            }
            else
            {
                int count = list.Count;//re-count

                for (int i = 0; i < count; ++i)
                {
                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = list[i];

                    //change typeface                     
                    ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                    Typeface = resolvedFont.Typeface;

                    DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                    xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                }
            }
        }

        void ClearTempFormattedGlyphPlanSeq()
        {
            _fmtGlyphPlanList.Clear();
        }


    }


}
