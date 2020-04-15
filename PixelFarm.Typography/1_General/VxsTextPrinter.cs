//MIT, 2016-present, WinterDev, Sam Hocevar
using System;

using PixelFarm.CpuBlit.BitmapAtlas;

using Typography.Contours;
using Typography.OpenFont;
using Typography.TextLayout;

namespace PixelFarm.Drawing
{

    public class VxsTextPrinter : TextPrinterBase, ITextPrinter
    {
        OpenFontTextService _textServices;

        /// <summary>
        /// target canvas
        /// </summary>
        Painter _painter;
        RequestFont _reqFont;
        Typeface _currentTypeface;
        GlyphMeshStore _glyphMeshStore;
        float _currentFontSizePxScale;

        GlyphBitmapStore _glyphBitmapStore;
        BitmapCacheForSvgGlyph _glyphSvgStore;

        public VxsTextPrinter(Painter painter, OpenFontTextService textService)
        {

            //
            _painter = painter;
            _glyphMeshStore = new GlyphMeshStore();
            _glyphMeshStore.FlipGlyphUpward = true;
            this.PositionTechnique = PositionTechnique.OpenFont;
            //
            _textServices = textService;
            ChangeFont(new RequestFont("Source Sans Pro", 10));

            _glyphBitmapStore = new GlyphBitmapStore();
            _glyphSvgStore = new BitmapCacheForSvgGlyph();
        }
        public void SetSvgBmpBuilderFunc(SvgBmpBuilderFunc svgBmpBuilderFunc)
        {
            _glyphSvgStore.SetSvgBmpBuilderFunc(svgBmpBuilderFunc);
        }

        public AntialiasTechnique AntialiasTechnique { get; set; }

        public void ChangeFont(RequestFont font)
        {
            //1.  resolve actual font file
            _reqFont = font;
            this.Typeface = _textServices.ResolveTypeface(font); //resolve for 'actual' font 
            this.FontSizeInPoints = font.SizeInPoints;
        }
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

        protected override void OnFontSizeChanged()
        {
            //update some font metrics property   
            Typeface currentTypeface = _currentTypeface;
            if (currentTypeface != null)
            {
                float pointToPixelScale = currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;


            }

        }
        public override GlyphLayout GlyphLayoutMan
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override Typeface Typeface
        {
            get => _currentTypeface;

            set
            {

                if (_currentTypeface == value) return;
                // 
                _currentTypeface = value;
                OnFontSizeChanged();
            }
        }



        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {
            UpdateGlyphLayoutSettings();
        }
        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx)
        {
            UpdateGlyphLayoutSettings();
        }

        public void UpdateGlyphLayoutSettings()
        {


            if (_reqFont == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(_painter.CurrentFont);
            }
            //2.1              
            if (Typeface == null) return;
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);
            _textServices.CurrentScriptLang = this.ScriptLang;

            ////2.3
            //if (_pxScaleEngine != null)
            //{
            //    _pxScaleEngine.SetFont(this.Typeface, this.FontSizeInPoints);
            //}
        }


        public void MeasureString(char[] buffer, int startAt, int len, out int w, out int h)
        {
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            var textBuffSpan = new TextBufferSpan(buffer, startAt, len);
            Size s = _textServices.MeasureString(textBuffSpan, _painter.CurrentFont);
            w = s.Width;
            h = s.Height;
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
            Internal.RenderVxGlyphPlan[] glyphPlans = renderVx.GlyphList;
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

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float left, float top)
        {

            float baseLine = top;
            switch (TextBaseline)
            {
                case TextBaseline.Alphabetic:

                    break;
                case TextBaseline.Top:
                    baseLine += this.FontAscendingPx;
                    break;
                case TextBaseline.Bottom:
                    baseLine += this.FontDescedingPx;
                    break;
            }

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);

            //4. render each glyph 
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;


            Typography.OpenFont.Tables.COLR colrTable = _currentTypeface.COLRTable;
            Typography.OpenFont.Tables.CPAL cpalTable = _currentTypeface.CPALTable;
            bool hasColorGlyphs = (colrTable != null) && (cpalTable != null);

            //--------------------------------------------------- 
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            _glyphMeshStore.SimulateOblique = this.SimulateSlant;
            //---------------------------------------------------


            if (_currentTypeface.HasSvgTable())
            {
                _glyphSvgStore.SetCurrentTypeface(_currentTypeface);
                int seqLen = seq.Count;
                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));

                    GlyphBitmap glyphBmp = _glyphSvgStore.GetGlyphBitmap(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    //1. 
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }
                }
            }
            else if (_currentTypeface.IsBitmapFont)
            {
                //check if we have exported all the glyph bitmap 
                //to some 'ready' form?
                //if not then create it
                _glyphBitmapStore.SetCurrentTypeface(_currentTypeface);

                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                var snapToPx = new GlyphPlanSequenceSnapPixelScaleLayout(seq, startAt, len, scale);
                while (snapToPx.Read())
                {
                    _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX) + 0.33f, (float)Math.Floor(baseLine + snapToPx.ExactY));
                    GlyphBitmap glyphBmp = _glyphBitmapStore.GetGlyphBitmap(snapToPx.CurrentGlyphIndex);
                    //how to draw the image
                    if (glyphBmp != null)
                    {
                        _painter.DrawImage(glyphBmp.Bitmap);
                    }                    
                }
            }
            else
            {
                if (!hasColorGlyphs)
                {
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

                    //restore
                    _painter.RenderQuality = savedRederQuality;
                    _painter.UseLcdEffectSubPixelRendering = savedUseLcdMode;

                }
                else
                {
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
                    while (snapToPx.Read())
                    {

                        _painter.SetOrigin((float)Math.Round(left + snapToPx.ExactX), (float)Math.Floor(baseLine + snapToPx.ExactY));

                        if (colrTable.LayerIndices.TryGetValue(snapToPx.CurrentGlyphIndex, out ushort colorLayerStart))
                        {
                            //TODO: optimize this                        
                            //we found color info for this glyph 
                            ushort colorLayerCount = colrTable.LayerCounts[snapToPx.CurrentGlyphIndex];
                            byte r, g, b, a;
                            for (int c = colorLayerStart; c < colorLayerStart + colorLayerCount; ++c)
                            {
                                ushort gIndex = colrTable.GlyphLayers[c];

                                int palette = 0; // FIXME: assume palette 0 for now 
                                cpalTable.GetColor(
                                    cpalTable.Palettes[palette] + colrTable.GlyphPalettes[c], //index
                                    out r, out g, out b, out a);
                                //-----------  
                                _painter.FillColor = new Color(r, g, b);//? a component
                                _painter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
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
                        }
                    }

                    _painter.FillColor = originalFillColor; //restore color
                }
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }


        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            DrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            UpdateGlyphLayoutSettings();
            //unscale layout, with design unit scale
            var buffSpan = new TextBufferSpan(textBuffer, startAt, len);
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(buffSpan, _reqFont);
            DrawFromGlyphPlans(glyphPlanSeq, x, y);
        }


    }


    //public static class TextPrinterHelper
    //{
    //    //public static void CopyGlyphPlans(RenderVxFormattedString renderVx, PxScaledGlyphPlanList glyphPlans)
    //    //{
    //    //    int n = glyphPlans.Count;
    //    //    //copy 
    //    //    var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
    //    //    float acc_x = 0;
    //    //    float acc_y = 0;
    //    //    float x = 0;
    //    //    float y = 0;
    //    //    float g_x = 0;
    //    //    float g_y = 0;

    //    //    for (int i = 0; i < n; ++i)
    //    //    {
    //    //        PxScaledGlyphPlan glyphPlan = glyphPlans[i];


    //    //        float ngx = acc_x + glyphPlan.OffsetX;
    //    //        float ngy = acc_y + glyphPlan.OffsetY;
    //    //        //NOTE:
    //    //        // -glyphData.TextureXOffset => restore to original pos
    //    //        // -glyphData.TextureYOffset => restore to original pos 
    //    //        //--------------------------
    //    //        g_x = (float)(x + ngx); //ideal x
    //    //        g_y = (float)(y + ngy);


    //    //        float g_w = glyphPlan.AdvanceX;
    //    //        acc_x += g_w;

    //    //        //g_x = (float)Math.Round(g_x);
    //    //        g_y = (float)Math.Floor(g_y);


    //    //        renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
    //    //            glyphPlan.glyphIndex,
    //    //            g_x,
    //    //            g_y,
    //    //            g_w
    //    //            );
    //    //    }
    //    //    renderVx.glyphList = renderVxGlyphPlans;
    //    //}
    //    //public static void CopyGlyphPlans(RenderVxFormattedString renderVx, GlyphPlanSequence glyphPlans, float scale)
    //    //{
    //    //    int n = glyphPlans.Count;
    //    //    //copy 
    //    //    var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
    //    //    float acc_x = 0;
    //    //    float acc_y = 0;
    //    //    float x = 0;
    //    //    float y = 0;
    //    //    float g_x = 0;
    //    //    float g_y = 0;

    //    //    for (int i = 0; i < n; ++i)
    //    //    {
    //    //        UnscaledGlyphPlan glyphPlan = glyphPlans[i];


    //    //        float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
    //    //        float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);
    //    //        //NOTE:
    //    //        // -glyphData.TextureXOffset => restore to original pos
    //    //        // -glyphData.TextureYOffset => restore to original pos 
    //    //        //--------------------------
    //    //        g_x = (float)(x + ngx); //ideal x
    //    //        g_y = (float)(y + ngy);


    //    //        float g_w = (float)Math.Round(glyphPlan.AdvanceX * scale);
    //    //        acc_x += g_w;

    //    //        //g_x = (float)Math.Round(g_x);
    //    //        g_y = (float)Math.Floor(g_y);


    //    //        renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
    //    //            glyphPlan.glyphIndex,
    //    //            g_x,
    //    //            g_y,
    //    //            g_w
    //    //            );
    //    //    }
    //    //    renderVx.glyphList = renderVxGlyphPlans;
    //    //}
    //}

}
