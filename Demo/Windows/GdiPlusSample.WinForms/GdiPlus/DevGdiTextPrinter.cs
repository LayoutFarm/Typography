//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.OpenFont.Tables;
using Typography.TextLayout;
using Typography.Contours;
using Typography.TextBreak;
using Typography.FontManagement;
using Typography.Text;

using PixelFarm.Drawing;
namespace SampleWinForms
{

    /// <summary>
    /// developer's version, Gdi+ text printer
    /// </summary>
    partial class DevGdiTextPrinter : TextPrinterBase
    {
        Typeface _currentTypeface;
        GlyphOutlineBuilder _currentGlyphPathBuilder;
        GlyphTranslatorToGdiPath _txToGdiPath;
        GlyphLayout _glyphLayout = new GlyphLayout();
        SolidBrush _fillBrush = new SolidBrush(Color.Black);
        Pen _outlinePen = new Pen(Color.Green);
        //
        //for optimization
        GlyphMeshCollection<GraphicsPath> _glyphMeshCollections = new GlyphMeshCollection<GraphicsPath>();

        public DevGdiTextPrinter()
        {
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
        }

        public override GlyphLayout GlyphLayoutMan => _glyphLayout;
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
                _glyphLayout.Typeface = value;
                //--------------------------------
                if (value == null) return;
                //--------------------------------

                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphOutlineBuilder(_currentTypeface);
                //for gdi path***
                //3. glyph reader,output as Gdi+ GraphicsPath
                _txToGdiPath = new GlyphTranslatorToGdiPath();
                //4.
                OnFontSizeChanged();
            }

        }
        protected override void OnFontSizeChanged()
        {
            //update some font matrix property  
            if (Typeface != null)
            {
                float pointToPixelScale = Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = Typeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = Typeface.Descender * pointToPixelScale;
                this.FontLineGapPx = Typeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }

        public bool EnableColorGlyph { get; set; } = true;
        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

        UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();




        public void UpdateGlyphLayoutSettings()
        {
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;

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


        //----------------------------
        TextServiceClient _txtClient;
        public void SetTextServiceClient(TextServiceClient txtClient)
        {
            _txtClient = txtClient;
        }

        public AlternativeTypefaceSelector AlternativeTypefaceSelector { get; set; }
        readonly TextPrinterWordVisitor _textPrinterWordVisitor = new TextPrinterWordVisitor();
        readonly TextPrinterLineSegmentList<TextPrinterLineSegment> _lineSegs = new TextPrinterLineSegmentList<TextPrinterLineSegment>();

        List<FormattedGlyphPlanSeq> _tmpGlyphPlanSeqs = new List<FormattedGlyphPlanSeq>();

        readonly Dictionary<int, ResolvedFont> _localResolvedFonts = new Dictionary<int, ResolvedFont>();

        ResolvedFont LocalResolveFont(Typeface typeface, float sizeInPoint)
        {
            //find local resolved font cache

            //check if we have a cache key or not
            int typefaceKey = TypefaceExtensions.GetCustomTypefaceKey(typeface);
            if (typefaceKey == 0)
            {
                char[] upperCaseName = typeface.Name.ToUpper().ToCharArray();
                Typography.OpenFont.Extensions.TypefaceExtensions.SetCustomTypefaceKey(
                    typeface,
                    typefaceKey = Typography.Text.CRC32.CalculateCRC32(upperCaseName, 0, upperCaseName.Length));

            }

            int key = CalculateGetHasCode(typefaceKey, sizeInPoint, (int)0);
            if (!_localResolvedFonts.TryGetValue(key, out ResolvedFont found))
            {
                return _localResolvedFonts[key] = new ResolvedFont(typeface, sizeInPoint, key);
            }
            return found;
        }


        static int CalculateGetHasCode(int typefaceKey, float fontSize, int fontstyle)
        {
            //modified from https://stackoverflow.com/questions/1646807/quick-and-simple-hash-code-combinations
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + typefaceKey.GetHashCode();
                hash = hash * 31 + fontSize.GetHashCode();
                hash = hash * 31 + fontstyle.GetHashCode();
                return hash;
            }
        }
        int _latestAccumulateWidth;
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

#if DEBUG
            if (textBuffer.Length == 2)
            {

            }
#endif

            _latestAccumulateWidth = 0;
            if (!EnableMultiTypefaces)
            {
                _reusableUnscaledGlyphPlanList.Clear();
                //1. unscale layout, in design unit
                _glyphLayout.Layout(textBuffer, startAt, len);
                _glyphLayout.GenerateUnscaledGlyphPlans(_reusableUnscaledGlyphPlanList);

                //draw from the glyph plan seq 
                DrawFromGlyphPlans(
                    new GlyphPlanSequence(_reusableUnscaledGlyphPlanList),
                    x, y);
            }
            else
            {
                var buffSpan = new TextBufferSpan(textBuffer, startAt, len);

                _lineSegs.Clear();//clear before reuse
                _textPrinterWordVisitor.SetLineSegmentList(_lineSegs);
                _txtClient.BreakToLineSegments(buffSpan, _textPrinterWordVisitor);//***
                _textPrinterWordVisitor.SetLineSegmentList(null);

                bool needRightToLeftArr = false;


                float xpos = x;

                Typeface defaultTypeface = this.Typeface;
                Typeface curTypeface = defaultTypeface;

                FormattedGlyphPlanSeq latestFmtGlyphPlanSeq = null;
                int prefix_whitespaceCount = 0;

                int count = _lineSegs.Count;
                for (int i = 0; i < count; ++i)
                {
                    //
                    TextPrinterLineSegment line_seg = _lineSegs.GetLineSegment(i);
                    SpanBreakInfo spBreakInfo = line_seg.BreakInfo;

                    if (spBreakInfo.RightToLeft)
                    {
                        needRightToLeftArr = true;
                    }


                    if (line_seg.WordKind == WordKind.Whitespace)
                    {
                        if (latestFmtGlyphPlanSeq == null)
                        {
                            prefix_whitespaceCount += line_seg.Length;
                        }
                        else
                        {
                            latestFmtGlyphPlanSeq.PostfixWhitespaceCount += line_seg.Length;
                        }
                        continue; //***
                    }

                    //each line segment may have different unicode range 
                    //and the current typeface may not support that range
                    //so we need to ensure that we get a proper typeface,
                    //if not => alternative typeface

                    ushort sample_glyphIndex = 0;
                    char sample_char = textBuffer[line_seg.StartAt];
                    int codepoint = sample_char;

                    if (line_seg.Length > 1 && line_seg.WordKind == WordKind.SurrogatePair)
                    {
                        sample_glyphIndex = curTypeface.GetGlyphIndex(codepoint = char.ConvertToUtf32(sample_char, textBuffer[line_seg.StartAt + 1]));
                    }
                    else
                    {
                        sample_glyphIndex = curTypeface.GetGlyphIndex(codepoint);
                    }


                    if (sample_glyphIndex == 0)
                    {
                        //not found then => find other typeface                    
                        //we need more information about line seg layout

                        if (AlternativeTypefaceSelector != null)
                        {
                            AlternativeTypefaceSelector.LatestTypeface = curTypeface;
                        }

                        if (_txtClient.TryGetAlternativeTypefaceFromCodepoint(codepoint, AlternativeTypefaceSelector, out Typeface alternative))
                        {
                            curTypeface = alternative;
                        }
                        else
                        {
#if DEBUG
                            if (sample_char >= 0 && sample_char < 255)
                            {


                            }
#endif
                        }
                    }



                    //layout glyphs in each context

                    TextBufferSpan buff = new TextBufferSpan(textBuffer, line_seg.StartAt, line_seg.Length);

                    _txtClient.CurrentScriptLang = new ScriptLang(spBreakInfo.ScriptTag, spBreakInfo.LangTag);

                    //in some text context (+typeface)=>user can disable gsub, gpos
                    //this is an example                  

                    if (line_seg.WordKind == WordKind.Tab || line_seg.WordKind == WordKind.Number ||
                        (spBreakInfo.UnicodeRange == Unicode13RangeInfoList.C0_Controls_and_Basic_Latin))
                    {
                        _txtClient.EnableGpos = false;
                        _txtClient.EnableGsub = false;
                    }
                    else
                    {
                        _txtClient.EnableGpos = true;
                        _txtClient.EnableGsub = true;
                    }


                    GlyphPlanSequence seq = _txtClient.CreateGlyphPlanSeq(buff, curTypeface, FontSizeInPoints);

                    seq.IsRightToLeft = spBreakInfo.RightToLeft;

                    //create an object that hold more information about GlyphPlanSequence

                    FormattedGlyphPlanSeq formattedGlyphPlanSeq = _pool.GetFreeFmtGlyphPlanSeqs();
                    formattedGlyphPlanSeq.PrefixWhitespaceCount = (ushort)prefix_whitespaceCount;//***
                    prefix_whitespaceCount = 0;//reset 

                    //TODO: other style?... (bold, italic)  

                    ResolvedFont foundResolvedFont = LocalResolveFont(curTypeface, FontSizeInPoints);//temp fix for regular
                    formattedGlyphPlanSeq.SetData(seq, foundResolvedFont);

                    _tmpGlyphPlanSeqs.Add(latestFmtGlyphPlanSeq = formattedGlyphPlanSeq);

                    curTypeface = defaultTypeface;//switch back to default

                    //restore latest script lang?
                }

                if (needRightToLeftArr)
                {
                    //special arr left-to-right
                    count = _tmpGlyphPlanSeqs.Count;//re-count
                    for (int i = count - 1; i >= 0; --i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);
                    }
                }
                else
                {
                    count = _tmpGlyphPlanSeqs.Count;//re-count

                    for (int i = 0; i < count; ++i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _tmpGlyphPlanSeqs[i];

                        //change typeface                     
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    }
                }
                ClearTempFormattedGlyphPlanSeq();
                Typeface = defaultTypeface;
            }
        }
        void ClearTempFormattedGlyphPlanSeq()
        {
            for (int i = _tmpGlyphPlanSeqs.Count - 1; i >= 0; --i)
            {
                //release back to pool
                _pool.ReleaseFmtGlyphPlanSeqs(_tmpGlyphPlanSeqs[i]);
            }
            _tmpGlyphPlanSeqs.Clear();
        }

        readonly FormattedGlyphPlanSeqPool _pool = new FormattedGlyphPlanSeqPool();

    }
}
