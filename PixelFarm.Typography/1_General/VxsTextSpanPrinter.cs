//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using PixelFarm.CpuBlit.BitmapAtlas;

using Typography.OpenFont;
using Typography.OpenFont.Contours;
using Typography.OpenFont.Extensions;
using Typography.TextLayout;
using Typography.Text;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.VertexProcessing;


namespace PixelFarm.Drawing
{
    public class VxsTextSpanPrinter : AbstractTextSpanPrinter
    {

        Typeface _currentTypeface;
        /// <summary>
        /// target canvas
        /// </summary>
        readonly Painter _painter;
        readonly GlyphMeshStore _glyphMeshStore;
        readonly GlyphBitmapStore _glyphBitmapStore;
        readonly TextServiceClient _txtClient;

        public VxsTextSpanPrinter(Painter painter, TextServiceClient txtClient)
        {
            _txtClient = txtClient;
            _painter = painter;
            _glyphMeshStore = new GlyphMeshStore() { FlipGlyphUpward = true };
            _glyphBitmapStore = new GlyphBitmapStore();
        }
        public AlternativeTypefaceSelector AlternativeTypefaceSelector
        {
            get => _txtClient.AlternativeTypefaceSelector;
            set => _txtClient.AlternativeTypefaceSelector = value;
        }
        public HintTechnique HintTechnique { get; set; }
        bool EnableColorGlyphBitmapCache { get; set; } = true;

        SvgBmpBuilderFunc _svgBmpBuilderFunc;
        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _svgBmpBuilderFunc = svgBmpBuilderFunc;
        }

        public AntialiasTechnique AntialiasTechnique { get; set; }

        public void ChangeFillColor(Color fontColor)
        {
            //change font color

#if DEBUG
            Console.Write("please impl change font color");
#endif
        }
        public void ChangeStrokeColor(Color strokeColor)
        {

        }

        Typography.OpenFont.Tables.COLR _colrTable;
        Typography.OpenFont.Tables.CPAL _cpalTable;
        bool _hasColorInfo;

        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {
                if (_currentTypeface == value) return;
                // 
                _currentTypeface = value;
                if (value == null)
                {
                    return;
                }
                //
                _colrTable = _currentTypeface.COLRTable;
                _cpalTable = _currentTypeface.CPALTable;
                _hasColorInfo = _colrTable != null && _cpalTable != null;

                OnFontSizeChanged();
            }
        }
        public void UpdateGlyphLayoutSettings()
        {
            if (Typeface == null) return;
            //
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
        }

        protected override void OnFontSizeChanged()
        {
            if (Typeface != null)
            {
                _txtClient.SetCurrentFont(Typeface, FontSizeInPoints, ScriptLang);
            }

            if (!_renderingMultiTypefaceMode)
            {
                //during rendering phase, 
                //skip change the baseline
                base.OnFontSizeChanged();
            }
        }
        public void DrawString(RenderVxFormattedString renderVx, double left, double top)
        {
            //TODO: review here
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;

            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel? 
            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            Internal.RenderVxGlyphPlan[] glyphPlans = ((PixelFarm.CpuBlit.AggRenderVxFormattedString)renderVx).GlyphList;
            int j = glyphPlans.Length;
            //---------------------------------------------------
            //consider use cached glyph, to increase performance 

            float g_x = 0;
            float g_y = 0;
            float baseY = (int)top; //TODO, review here again

            for (int i = 0; i < j; ++i)
            {
                Internal.RenderVxGlyphPlan glyphPlan = glyphPlans[i];
                //-----------------------------------
                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                g_x = (float)(glyphPlan.x * scale + left);
                g_y = (float)glyphPlan.y * scale;

                _painter.SetOrigin(g_x, g_y);
                _painter.Fill(vxs);
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }


        int _latestAccumulateWidth;

#if DEBUG
        int dbugExportCount = 0;
#endif

        GlyphBitmap GetGlyphBitmapFromSvg(ushort glyphIndex)
        {
            if (_glyphBitmapStore.CurrrentBitmapCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }


            //TODO: use string builder from pool?
            var stbuilder = new System.Text.StringBuilder();
            _currentTypeface.ReadSvgContent(glyphIndex, stbuilder);

            float bmpScale = _currentTypeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
            float target_advW = _currentTypeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;

            var req = new SvgBmpBuilderReq
            {
                SvgContent = stbuilder,
                ExpectedWidth = target_advW
            };

            _svgBmpBuilderFunc.Invoke(req);

            MemBitmap memBmp = req.Output;

            if (memBmp == null)
            {
                //TODO: use blank img?
                return null;
            }

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            //find bitmap scale             

            //TODO...
            short offset_x = 0;
            short offset_y = 0;

            currentCache.RegisterBitmap(glyphIndex,
                glyphBmp = new GlyphBitmap
                {
                    Bitmap = memBmp,
                    Width = memBmp.Width,
                    Height = memBmp.Height,
                    ImageStartX = -offset_x,//offset back
                    ImageStartY = -offset_y //offset back
                });

            return glyphBmp;
        }
        GlyphBitmap GetGlyphBitmapFromColorOutlineGlyph(ushort glyphIndex, ushort colorLayerStart)
        {
            if (_glyphBitmapStore.CurrrentBitmapCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }


            //not found=> create a newone 
            Q1RectD totalBounds = Q1RectD.ZeroIntersection();
            {
                //calculate bounds of this glyph
                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {
                    BoundingRect.GetBoundingRect(_glyphMeshStore.GetGlyphMesh(_colrTable.GlyphLayers[c]), ref totalBounds);
                }
            }
            //dbugExportCount++;     
            var memBmp = new MemBitmap((int)Math.Round(totalBounds.Width), (int)Math.Round(totalBounds.Height));//???
            int offset_x = 0;
            int offset_y = 0;
            using (Tools.BorrowAggPainter(memBmp, out AggPainter painter))
            {
                painter.Clear(Color.Transparent);
                painter.SetOrigin(0, 0);

                offset_x = -(int)(totalBounds.Left);
                offset_y = -(int)(totalBounds.Bottom);

                ushort colorLayerCount = _colrTable.LayerCounts[glyphIndex];
                int palette = 0; // FIXME: assume palette 0 for now 
                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                {

                    _cpalTable.GetColor(
                        _cpalTable.Palettes[palette] + _colrTable.GlyphPalettes[c], //index
                         out byte r, out byte g, out byte b, out byte a);

                    ushort gIndex = _colrTable.GlyphLayers[c];
                    VertexStore vxs = _glyphMeshStore.GetGlyphMesh(gIndex);
                    using (Tools.BorrowVxs(out var v1))
                    {
                        vxs.TranslateToNewVxs(offset_x, offset_y, v1);
                        painter.FillColor = new Color(r, g, b);//? a component
                        painter.Fill(v1);
                    }
                }
                //find ex
#if DEBUG
                //memBmp.SaveImage("a0x" + (dbugExportCount) + ".png");
#endif
            }

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            //find bitmap scale             

            currentCache.RegisterBitmap(glyphIndex,
                glyphBmp = new GlyphBitmap
                {
                    Bitmap = memBmp,
                    Width = memBmp.Width,
                    Height = memBmp.Height,
                    ImageStartX = -offset_x,//offset back
                    ImageStartY = -offset_y //offset back
                });

            return glyphBmp;
        }
        GlyphBitmap GetGlyphBitmapFromBitmapFont(ushort glyphIndex)
        {

            TypefaceGlyphBitmapCache currentCache = _glyphBitmapStore.CurrrentBitmapCache;
            float actualCacheSize = currentCache.ActualCacheSize;

            //actual size of glyph that we store may not equal the current req size
            //since we do not store all font size in the cache

#if DEBUG
            if (actualCacheSize != FontSizeInPoints)
            {
                System.Diagnostics.Debugger.Break();
            }
#endif

            if (currentCache.TryGetBitmap(glyphIndex, out GlyphBitmap glyphBmp))
            {
                return glyphBmp;
            }

            //not found=> create a new one
            if (_currentTypeface.IsBitmapFont)
            {
                //try load
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    //load actual bitmap font
                    Glyph glyph = _currentTypeface.GetGlyph(glyphIndex);
                    _currentTypeface.ReadBitmapContent(glyph, ms);

                    using (MemBitmap memBitmap = MemBitmapExt.LoadBitmap(ms))
                    {
                        //bitmap that are load may be larger than we need
                        //so we need to scale it to specfic size

                        float bmpScale = _currentTypeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
                        float target_advW = _currentTypeface.GetAdvanceWidthFromGlyphIndex(glyphIndex) * bmpScale;
                        float scaleForBmp = target_advW / memBitmap.Width;

                        MemBitmap scaledMemBmp = memBitmap.ScaleImage(scaleForBmp, scaleForBmp);

                        var glyphBitmap = new GlyphBitmap
                        {
                            Width = scaledMemBmp.Width,
                            Height = scaledMemBmp.Height,
                            Bitmap = scaledMemBmp //**
                        };

                        _glyphBitmapStore.CurrrentBitmapCache.RegisterBitmap(glyphIndex, glyphBitmap);
                        return glyphBitmap;
                    }
                }
            }
            return null;
        }

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float left, float top)
        {
            _latestAccumulateWidth = 0;//reset

            if (_currentTypeface == null) return;

            float baseLine = top;
            switch (TextBaseline)
            {
                case Typography.Text.TextBaseline.Alphabetic:

                    break;
                case Typography.Text.TextBaseline.Top:
                    baseLine += this.FontAscendingPx;
                    break;
                case Typography.Text.TextBaseline.Bottom:
                    baseLine += this.FontDescedingPx;
                    break;
            }

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);

            //4. render each glyph 
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //--------------------------------------------------- 
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            //---------------------------------------------------

            if (_currentTypeface.HasSvgTable())
            {
                //Test svg font with Twitter Color Emoji Regular

                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);

                int seqLen = seq.Count;
                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    GlyphBitmap glyphBmp = GetGlyphBitmapFromSvg(snapToPx.CurrentGlyphIndex);
                    //since we use svg=>bitmap glyph, so need to offset Y back.
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY - (glyphBmp.Height + this.FontDescedingPx)));

                    //***                    
                    //how to draw the image
                    //1. 
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
                _latestAccumulateWidth = snapToPx.AccumWidth;
            }
            else if (_currentTypeface.IsBitmapFont)
            {
                //Test IsBitmapFont font with Noto Color Emoji

                //check if we have exported all the glyph bitmap 
                //to some 'ready' form?
                //if not then create it

                //TODO: review this again
                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);

                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    GlyphBitmap glyphBmp = GetGlyphBitmapFromBitmapFont(snapToPx.CurrentGlyphIndex);
                    //since we use svg=>bitmap glyph, so need to offset Y back.
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY - (glyphBmp.Height + this.FontDescedingPx)));

                    //how to draw the image
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
                _latestAccumulateWidth = snapToPx.AccumWidth;
            }
            else
            {
                if (!_hasColorInfo)
                {
                    //NO color information, 

                    bool savedUseLcdMode = _painter.UseLcdEffectSubPixelRendering; //save,restore later
                    RenderQuality savedRederQuality = _painter.RenderQuality;
                    _painter.RenderQuality = RenderQuality.HighQuality;
                    _painter.UseLcdEffectSubPixelRendering = true;

                    int seqLen = seq.Count;

                    if (len > seqLen)
                    {
                        len = seqLen;
                    }

                    var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                    while (snapToPx.Read())
                    {
                        _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));
                        _painter.Fill(_glyphMeshStore.GetGlyphMesh(snapToPx.CurrentGlyphIndex));
                    }

                    _latestAccumulateWidth = snapToPx.AccumWidth;
                    //restore
                    _painter.RenderQuality = savedRederQuality;
                    _painter.UseLcdEffectSubPixelRendering = savedUseLcdMode;

                }
                else
                {
                    //Test Color Outline Font with Firefox Emoji
                    //-------------    
                    //this glyph has color information
                    //-------------
                    Color originalFillColor = _painter.FillColor;
                    int seqLen = seq.Count;

                    if (len > seqLen)
                    {
                        len = seqLen;
                    }

                    var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                    float maxAccumWidth = 0;

                    if (EnableColorGlyphBitmapCache)
                    {
                        _glyphBitmapStore.SetCurrentTypeface(_currentTypeface, fontSizePoint);
                    }


                    while (snapToPx.Read())
                    {
                        float start_pos_x = (float)Math.Round(left + snapToPx.ExactX);

                        _painter.SetOrigin(start_pos_x, (float)Math.Floor(baseLine + snapToPx.ExactY));

                        if (_colrTable.LayerIndices.TryGetValue(snapToPx.CurrentGlyphIndex, out ushort colorLayerStart))
                        {
                            //check if we have a bitmap cache for this glyph or not
                            //
                            if (EnableColorGlyphBitmapCache)
                            {
                                GlyphBitmap glyphBmp = GetGlyphBitmapFromColorOutlineGlyph(snapToPx.CurrentGlyphIndex, colorLayerStart);

                                if (glyphBmp != null)
                                {
                                    _painter.DrawImage(glyphBmp.Bitmap, glyphBmp.ImageStartX, glyphBmp.ImageStartY);

                                    if (start_pos_x + glyphBmp.ImageStartX + glyphBmp.Width > maxAccumWidth)
                                    {
                                        maxAccumWidth = start_pos_x + glyphBmp.ImageStartX + glyphBmp.Width;
                                    }
                                }

                                _latestAccumulateWidth = (int)maxAccumWidth;
                            }
                            else
                            {

                                //TODO: optimize this                        
                                //we found color info for this glyph 
                                ushort colorLayerCount = _colrTable.LayerCounts[snapToPx.CurrentGlyphIndex];

                                int palette = 0; // FIXME: assume palette 0 for now 
                                for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                                {
                                    _cpalTable.GetColor(
                                        _cpalTable.Palettes[palette] + _colrTable.GlyphPalettes[c], //index
                                        out byte r, out byte g, out byte b, out byte a);

                                    _painter.FillColor = new Color(r, g, b);//? a component
                                    ushort gIndex = _colrTable.GlyphLayers[c];
                                    _painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
                                }

                                _latestAccumulateWidth = snapToPx.AccumWidth;
                            }
                        }
                        else
                        {
                            //-----------------------------------
                            //TODO: review here ***
                            //PERFORMANCE revisit here 
                            //if we have create a vxs we can cache it for later use?
                            //----------------------------------- 
                            _painter.Fill(_glyphMeshStore.GetGlyphMesh(snapToPx.CurrentGlyphIndex));
                            _latestAccumulateWidth = snapToPx.AccumWidth;
                        }
                    }

                    _painter.FillColor = originalFillColor; //restore color
                }
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }

        readonly FormattedGlyphPlanList _fmtGlyphPlans = new FormattedGlyphPlanList();
        bool _renderingMultiTypefaceMode;

        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

#if DEBUG
            if (textBuffer.Length > 2)
            {

            }
#endif 

            UpdateGlyphLayoutSettings();
            _latestAccumulateWidth = 0;
            //unscale layout, with design unit scale
            var buffSpan = new Typography.Text.TextBufferSpan(textBuffer, startAt, len);

            float xpos = x;
            float ypos = y;

            if (!EnableMultiTypefaces)
            {
                GlyphPlanSequence glyphPlanSeq = _txtClient.CreateGlyphPlanSeq(buffSpan, _currentTypeface, FontSizeInPoints);
                DrawFromGlyphPlans(glyphPlanSeq, xpos, y);
            }
            else
            {
                Typeface prevTypeface = Typeface;

                //a single string may be broken into many glyph-plan-seq
                //set segmentlist
                _fmtGlyphPlans.Clear();
                _txtClient.PrepareFormattedStringList(textBuffer, startAt, len, _fmtGlyphPlans);

                bool needRightToLeftArr = _fmtGlyphPlans.IsRightToLeftDirection;

                _renderingMultiTypefaceMode = true;

                int count = _fmtGlyphPlans.Count;

                if (needRightToLeftArr)
                {
                    //special arr left-to-right 
                    for (int i = count - 1; i >= 0; --i)
                    {
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlans[i];

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
                        FormattedGlyphPlanSeq formattedGlyphPlanSeq = _fmtGlyphPlans[i];

                        //change typeface                     
                        ResolvedFont resolvedFont = formattedGlyphPlanSeq.ResolvedFont;
                        Typeface = resolvedFont.Typeface;

                        DrawFromGlyphPlans(formattedGlyphPlanSeq.Seq, xpos + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PrefixWhitespaceCount), y);

                        xpos += _latestAccumulateWidth + (resolvedFont.WhitespaceWidth * formattedGlyphPlanSeq.PostfixWhitespaceCount);

                    }
                }
                _renderingMultiTypefaceMode = false;

                _fmtGlyphPlans.Clear();

                //restore prev typeface & settings
                Typeface = prevTypeface;
            }
        }

        public void ChangeFont(RequestFont reqFont)
        {
            ResolvedFont resolvedFont = _txtClient.ResolveFont(reqFont);
            if (resolvedFont != null)
            {
                this.Typeface = resolvedFont.Typeface;
                //
                this.FontSizeInPoints = reqFont.SizeInPoints;
            }
            else
            {
                throw new NotSupportedException();
            }
        }





    }


}
