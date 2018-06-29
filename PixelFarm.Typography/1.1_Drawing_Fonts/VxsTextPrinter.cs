//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using System.IO;

using PixelFarm.CpuBlit;

using Typography.Contours;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

using Typography.Rendering;
using Typography.TextLayout;
using Typography.TextServices;

namespace PixelFarm.Drawing.Fonts
{

    public class VxsTextPrinter : TextPrinterBase, ITextPrinter
    {
        LayoutFarm.OpenFontTextService _textServices;

        /// <summary>
        /// target canvas
        /// </summary>
        Painter _painter;
        RequestFont _reqFont;
        //----------------------------------------------------------- 


        PxScaledGlyphPlanList _outputPxScaledGlyphPlans = new PxScaledGlyphPlanList();

        Typeface _currentTypeface;

        GlyphMeshStore _glyphMeshStore;

        float _currentFontSizePxScale;

        public VxsTextPrinter(Painter painter)
        {
            StartDrawOnLeftTop = true;
            //
            this._painter = painter;
            _glyphMeshStore = new GlyphMeshStore();
            _glyphMeshStore.FlipGlyphUpward = true;
            this.PositionTechnique = PositionTechnique.OpenFont;
            //
            _textServices = new LayoutFarm.OpenFontTextService();
            ChangeFont(new RequestFont("tahoma", 10));
        }
        /// <summary>
        /// start draw on 'left-top' of a given area box
        /// </summary>
        public bool StartDrawOnLeftTop { get; set; }


        public AntialiasTechnique AntialiasTechnique { get; set; }

        public void ChangeFont(RequestFont font)
        {
            //1.  resolve actual font file
            this._reqFont = font;
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
            get
            {
                return _currentTypeface;
            }
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

        public override void DrawCaret(float x, float y)
        {
            //TODO: remove draw caret here, this is for debug only 

        }
        public void UpdateGlyphLayoutSettings()
        {
            if (this._reqFont == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(_painter.CurrentFont);
            }
            //2.1              
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

            ////2.3
            //if (_pxScaleEngine != null)
            //{
            //    _pxScaleEngine.SetFont(this.Typeface, this.FontSizeInPoints);
            //}
        }

        /// <summary>
        /// draw specfic glyph with current settings, at specific position
        /// </summary>
        /// <param name="glyph"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawGlyph(Glyph glyph, double x, double y)
        {
            //TODO...
        }
        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //TODO: review here
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;

            //1. update some props.. 
            //2. update current type face
            UpdateGlyphLayoutSettings();
            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel? 
            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            RenderVxGlyphPlan[] glyphPlans = renderVx.glyphList;
            int j = glyphPlans.Length;
            //---------------------------------------------------
            //consider use cached glyph, to increase performance 

            //GlyphPosPixelSnapKind x_snap = this.GlyphPosPixelSnapX;
            //GlyphPosPixelSnapKind y_snap = this.GlyphPosPixelSnapY;
            float g_x = 0;
            float g_y = 0;
            float baseY = (int)y;

            for (int i = 0; i < j; ++i)
            {
                RenderVxGlyphPlan glyphPlan = glyphPlans[i];
                //-----------------------------------
                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                g_x = (float)(glyphPlan.x * scale + x);
                g_y = (float)glyphPlan.y * scale;

                _painter.SetOrigin(g_x, g_y);
                _painter.Fill(vxs);
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }


        public override void DrawFromGlyphPlans(PxScaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {

            if (StartDrawOnLeftTop)
            {
                //version 2
                //offset y down 
                y += this.FontLineSpacingPx;
            }
            //Typeface typeface = _glyphPathBuilder.Typeface;
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel? 
            float fontSizePoint = this.FontSizeInPoints;
            //float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);
            float scale = 1;
            //4. render each glyph 
            float ox = _painter.OriginX;
            float oy = _painter.OriginY;
            int endBefore = startAt + len;

            Typography.OpenFont.Tables.COLR colrTable = _currentTypeface.COLRTable;
            Typography.OpenFont.Tables.CPAL cpalTable = _currentTypeface.CPALTable;
            bool hasColorGlyphs = (colrTable != null) && (cpalTable != null);

            //--------------------------------------------------- 
            _glyphMeshStore.SetFont(_currentTypeface, fontSizePoint);
            //---------------------------------------------------

            float g_x = 0;
            float g_y = 0;
            float baseY = (int)y;
            if (!hasColorGlyphs)
            {

                bool savedUseLcdMode = _painter.UseSubPixelLcdEffect; //save,restore later
                RenderQualtity savedRederQuality = _painter.RenderQuality;
                _painter.RenderQuality = RenderQualtity.HighQuality;
                _painter.UseSubPixelLcdEffect = true;

                CpuBlit.VertexProcessing.Affine flipY = CpuBlit.VertexProcessing.Affine.NewMatix(
                    CpuBlit.VertexProcessing.AffinePlan.Scale(1, -1)); //flip Y

                VertexStore reusableVxs = new VertexStore();

                float acc_x = 0; //acummulate x
                float acc_y = 0; //acummulate y

                for (int i = startAt; i < endBefore; ++i)
                {   //-----------------------------------
                    //TODO: review here ***
                    //PERFORMANCE revisit here 
                    //if we have create a vxs we can cache it for later use?
                    //-----------------------------------   
                    PxScaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);
                    g_x = ngx;
                    g_y = ngy;
                    _painter.SetOrigin((int)Math.Round(g_x) + 0.33f, (int)Math.Round(g_y) + 0.33f);

                    //-----------------------------------  

                    //invert each glyph 
                    //version 3:

                    reusableVxs.Clear();
                    VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    PixelFarm.CpuBlit.VertexProcessing.VertexStoreTransformExtensions.TransformToVxs(flipY, vxs, reusableVxs);
                    _painter.Fill(reusableVxs);


                    //version2; 
                    //VertexStore vsx = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
                    //_vxs1 = _invertY.TransformToVxs(vsx, _vxs1);
                    //painter.Fill(_vxs1);
                    //_vxs1.Clear();

                    //version1
                    //painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                }
                //restore
                _painter.RenderQuality = savedRederQuality;
                _painter.UseSubPixelLcdEffect = savedUseLcdMode;

            }
            else
            {
                //-------------    
                //this glyph has color information
                //-------------
                Color originalFillColor = _painter.FillColor;

                float acc_x = 0;
                float acc_y = 0;
                for (int i = startAt; i < endBefore; ++i)
                {
                    PxScaledGlyphPlan glyphPlan = glyphPlanList[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    g_x = ngx;
                    g_y = ngy;

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);
                    _painter.SetOrigin(g_x, g_y);


                    //-----------------------------------  
                    ushort colorLayerStart;
                    if (colrTable.LayerIndices.TryGetValue(glyphPlan.glyphIndex, out colorLayerStart))
                    {
                        //TODO: optimize this                        
                        //we found color info for this glyph 
                        ushort colorLayerCount = colrTable.LayerCounts[glyphPlan.glyphIndex];
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
                        _painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                    }
                }
                _painter.FillColor = originalFillColor; //restore color
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }

        public override void DrawFromGlyphPlans(GlyphPlanSequence seq, int startAt, int len, float x, float y)
        {

            if (StartDrawOnLeftTop)
            {
                //version 2
                //offset y down 
                y += this.FontLineSpacingPx;
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
            //---------------------------------------------------

            float gx = 0;
            float gy = 0;
            float baseY = (int)y;

#if DEBUG
            if (_currentTypeface.HasSvgTable())
            {

            }
#endif


            if (!hasColorGlyphs)
            {



                bool savedUseLcdMode = _painter.UseSubPixelLcdEffect; //save,restore later
                RenderQualtity savedRederQuality = _painter.RenderQuality;
                _painter.RenderQuality = RenderQualtity.HighQuality;
                _painter.UseSubPixelLcdEffect = true;


                float acc_x = 0; //acummulate x
                float acc_y = 0; //acummulate y

                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                for (int i = startAt; i < len; ++i)
                {   //-----------------------------------
                    //TODO: review here ***
                    //PERFORMANCE revisit here 
                    //if we have create a vxs we can cache it for later use?
                    //-----------------------------------   
                    UnscaledGlyphPlan glyphPlan = seq[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    //glyph width 
                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);

                    gx = x + ngx;
                    gy = y + ngy;
                    //move start draw point to gx and gy
                    //I found that ... if we render this with Lcd Effect =>  +0.33px, (1/3 px) make this look sharp.
                    //BUT, ... if we render this with grey-scaled stencil effect => not need to +0.33px

                    _painter.SetOrigin((float)Math.Round(gx) + 0.33f, (float)Math.Round(gy));
                    _painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                }
                //restore
                _painter.RenderQuality = savedRederQuality;
                _painter.UseSubPixelLcdEffect = savedUseLcdMode;

            }
            else
            {
                //-------------    
                //this glyph has color information
                //-------------
                Color originalFillColor = _painter.FillColor;

                float acc_x = 0;
                float acc_y = 0;
                int seqLen = seq.Count;

                if (len > seqLen)
                {
                    len = seqLen;
                }

                for (int i = startAt; i < len; ++i)
                {
                    UnscaledGlyphPlan glyphPlan = seq[i];

                    float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                    float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                    gx = x + ngx;
                    gy = y + ngy;

                    acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);
                    _painter.SetOrigin(gx, gy);


                    //-----------------------------------  
                    ushort colorLayerStart;
                    if (colrTable.LayerIndices.TryGetValue(glyphPlan.glyphIndex, out colorLayerStart))
                    {
                        //TODO: optimize this                        
                        //we found color info for this glyph 
                        ushort colorLayerCount = colrTable.LayerCounts[glyphPlan.glyphIndex];
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
                        _painter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                    }
                }
                _painter.FillColor = originalFillColor; //restore color
            }
            //restore prev origin
            _painter.SetOrigin(ox, oy);
        }


        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            InternalDrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InternalDrawString(textBuffer, startAt, len, x, y);
        }


        void InternalDrawString(char[] buffer, int startAt, int len, float x, float y)
        {
            UpdateGlyphLayoutSettings();
            //unscale layout, with design unit scale
            TextBufferSpan buffSpan = new TextBufferSpan(buffer, startAt, len);
            GlyphPlanSequence glyphPlanSeq = _textServices.CreateGlyphPlanSeq(ref buffSpan, _reqFont);
            DrawFromGlyphPlans(glyphPlanSeq, x, y);
        }
    }
    public static class TextPrinterHelper
    {
        public static void CopyGlyphPlans(RenderVxFormattedString renderVx, PxScaledGlyphPlanList glyphPlans)
        {
            int n = glyphPlans.Count;
            //copy 
            var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
            float acc_x = 0;
            float acc_y = 0;
            float x = 0;
            float y = 0;
            float g_x = 0;
            float g_y = 0;

            for (int i = 0; i < n; ++i)
            {
                PxScaledGlyphPlan glyphPlan = glyphPlans[i];


                float ngx = acc_x + glyphPlan.OffsetX;
                float ngy = acc_y + glyphPlan.OffsetY;
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + ngx); //ideal x
                g_y = (float)(y + ngy);


                float g_w = glyphPlan.AdvanceX;
                acc_x += g_w;

                //g_x = (float)Math.Round(g_x);
                g_y = (float)Math.Floor(g_y);


                renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
                    glyphPlan.glyphIndex,
                    g_x,
                    g_y,
                    g_w
                    );
            }
            renderVx.glyphList = renderVxGlyphPlans;
        }
        public static void CopyGlyphPlans(RenderVxFormattedString renderVx, GlyphPlanSequence glyphPlans, float scale)
        {
            int n = glyphPlans.Count;
            //copy 
            var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
            float acc_x = 0;
            float acc_y = 0;
            float x = 0;
            float y = 0;
            float g_x = 0;
            float g_y = 0;

            for (int i = 0; i < n; ++i)
            {
                UnscaledGlyphPlan glyphPlan = glyphPlans[i];


                float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);
                //NOTE:
                // -glyphData.TextureXOffset => restore to original pos
                // -glyphData.TextureYOffset => restore to original pos 
                //--------------------------
                g_x = (float)(x + ngx); //ideal x
                g_y = (float)(y + ngy);


                float g_w = (float)Math.Round(glyphPlan.AdvanceX * scale);
                acc_x += g_w;

                //g_x = (float)Math.Round(g_x);
                g_y = (float)Math.Floor(g_y);


                renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
                    glyphPlan.glyphIndex,
                    g_x,
                    g_y,
                    g_w
                    );
            }
            renderVx.glyphList = renderVxGlyphPlans;
        }
    }

}
