//MIT, 2016-2017, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using System.IO;

using PixelFarm.Agg;

using Typography.Contours;
using Typography.OpenFont;
using Typography.Rendering;
using Typography.TextLayout;
using Typography.TextServices;

namespace PixelFarm.Drawing.Fonts
{

    public class VxsTextPrinter : DevTextPrinterBase, ITextPrinter
    {
        /// <summary>
        /// target canvas
        /// </summary>
        CanvasPainter canvasPainter;
        IFontLoader _fontLoader;
        RequestFont _reqFont;
        //----------------------------------------------------------- 
        GlyphLayout _glyphLayout = new GlyphLayout();
        GlyphPlanList _outputGlyphPlans = new GlyphPlanList();
        Typeface _currentTypeface;
        PixelScaleLayoutEngine _pxScaleEngine;
        GlyphMeshStore _glyphMeshStore;
        //-----------------------------------------------------------
        Dictionary<InstalledFont, Typeface> _cachedTypefaces = new Dictionary<InstalledFont, Typeface>();
        //-----------------------------------------------------------
        float _currentFontSizePxScale;

        public VxsTextPrinter(CanvasPainter canvasPainter, IFontLoader fontLoader)
        {
            this.canvasPainter = canvasPainter;
            this._fontLoader = fontLoader;

            _glyphMeshStore = new GlyphMeshStore();
            //
            _pxScaleEngine = new PixelScaleLayoutEngine();
            _pxScaleEngine.HintedFontStore = _glyphMeshStore;//share _glyphMeshStore with pixel-scale-layout-engine

        }
        public CanvasPainter TargetCanvasPainter { get; set; }
        bool TryGetTypeface(InstalledFont instFont, out Typeface found)
        {
            return _cachedTypefaces.TryGetValue(instFont, out found);
        }
        void RegisterTypeface(InstalledFont instFont, Typeface typeface)
        {
            _cachedTypefaces[instFont] = typeface;
        }
        /// <summary>
        /// for layout that use with our  lcd subpixel rendering technique 
        /// </summary>
        public bool UseWithLcdSubPixelRenderingTechnique
        {
            get { return _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique; }
            set
            {
                _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique = value;
            }
        }
        public void ChangeFont(RequestFont font)
        {
            //1.  resolve actual font file
            this._reqFont = font;
            InstalledFont installedFont = _fontLoader.GetFont(font.Name, font.Style.ConvToInstalledFontStyle());
            Typeface foundTypeface;

            if (!TryGetTypeface(installedFont, out foundTypeface))
            {
                //if not found then create a new one
                //if not found
                //create the new one
                using (FileStream fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {
                    var reader = new OpenFontReader();
                    foundTypeface = reader.Read(fs);
                }
                RegisterTypeface(installedFont, foundTypeface);
            }

            this.Typeface = foundTypeface;
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
                return _glyphLayout;
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
                _currentTypeface = value;
                OnFontSizeChanged();
            }
        }


        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
        {

            //1. update some props.. 
            //2. update current type face
            UpdateTypefaceAndGlyphBuilder();
            Typeface typeface = _currentTypeface;// _glyphPathBuilder.Typeface;
            _glyphLayout.Typeface = typeface;
            _glyphLayout.Layout(text, startAt, len);
            //
            //3. scale from design unit to specific font size
            _outputGlyphPlans.Clear();
            _pxScaleEngine.Layout(_glyphLayout.ResultUnscaledGlyphPositions, _outputGlyphPlans);
            TextPrinterHelper.CopyGlyphPlans(renderVx, _outputGlyphPlans, this._currentFontSizePxScale);
        }

        public override void DrawCaret(float x, float y)
        {

            CanvasPainter p = this.TargetCanvasPainter;
            PixelFarm.Drawing.Color prevColor = p.StrokeColor;
            p.StrokeColor = PixelFarm.Drawing.Color.Red;
            p.Line(x, y, x, y + this.FontAscendingPx);
            p.StrokeColor = prevColor;

        }

        void UpdateTypefaceAndGlyphBuilder()
        {
            //1. update _glyphPathBuilder for current typeface
            UpdateGlyphLayoutSettings();
        }
        public void UpdateGlyphLayoutSettings()
        {
            if (this._reqFont == null)
            {
                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
                ChangeFont(canvasPainter.CurrentFont);
            }

            //2.1              
            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
            //2.2
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;

            _currentFontSizePxScale = Typeface.CalculateScaleToPixelFromPointSize(FontSizeInPoints);

            //2.3
            if (_pxScaleEngine != null)
            {
                _pxScaleEngine.SetFont(this.Typeface, this.FontSizeInPoints);
            }
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?




            //3.
            //color...
        }

        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            float ox = canvasPainter.OriginX;
            float oy = canvasPainter.OriginY;

            //1. update some props.. 
            //2. update current type face
            UpdateTypefaceAndGlyphBuilder();
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

                canvasPainter.SetOrigin(g_x, g_y);
                canvasPainter.Fill(vxs);
            }
            //restore prev origin
            canvasPainter.SetOrigin(ox, oy);
        }
        public override void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            CanvasPainter canvasPainter = this.TargetCanvasPainter;
            //Typeface typeface = _glyphPathBuilder.Typeface;
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(fontSizePoint);


            //4. render each glyph 
            float ox = canvasPainter.OriginX;
            float oy = canvasPainter.OriginY;
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
                for (int i = startAt; i < endBefore; ++i)
                {   //-----------------------------------
                    //TODO: review here ***
                    //PERFORMANCE revisit here 
                    //if we have create a vxs we can cache it for later use?
                    //-----------------------------------   
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    g_x = glyphPlan.ExactX + x;
                    g_y = glyphPlan.ExactY + y;
                    canvasPainter.SetOrigin(g_x, g_y);
                    //-----------------------------------   
                    canvasPainter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                }
            }
            else
            {
                //-------------    
                //this glyph has color information
                //-------------
                Color originalFillColor = canvasPainter.FillColor;

                for (int i = startAt; i < endBefore; ++i)
                {
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    g_x = glyphPlan.ExactX + x;
                    g_y = glyphPlan.ExactY + y;
                    canvasPainter.SetOrigin(g_x, g_y);
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
                            canvasPainter.FillColor = new Color(r, g, b);//? a component
                            canvasPainter.Fill(_glyphMeshStore.GetGlyphMesh(gIndex));
                        }
                    }
                    else
                    {
                        //-----------------------------------
                        //TODO: review here ***
                        //PERFORMANCE revisit here 
                        //if we have create a vxs we can cache it for later use?
                        //----------------------------------- 
                        canvasPainter.Fill(_glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex));
                    }
                }
                canvasPainter.FillColor = originalFillColor; //restore color
            }
            //restore prev origin
            canvasPainter.SetOrigin(ox, oy);
        }

        public void DrawString(char[] text, int startAt, int len, double x, double y)
        {
            InternalDrawString(text, startAt, len, (float)x, (float)y);
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            InternalDrawString(textBuffer, startAt, len, x, y);
        }
         
        void InternalDrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            UpdateGlyphLayoutSettings();
            //unscale layout, with design unit scale
            _glyphLayout.Layout(textBuffer, startAt, len);
            //
            //
            _outputGlyphPlans.Clear();
            //
            if (this._pxScaleEngine != null)
            {
                //scale to specific font size 
                _pxScaleEngine.Layout(_glyphLayout.ResultUnscaledGlyphPositions, _outputGlyphPlans);
            }
            else
            {

                //no custom engine
                //then use default scale  
                GlyphLayoutExtensions.GenerateGlyphPlan(
                    _glyphLayout.ResultUnscaledGlyphPositions,
                    _currentFontSizePxScale,
                    false,
                    _outputGlyphPlans);

            }

            DrawFromGlyphPlans(_outputGlyphPlans, x, y);
        }


    }

    public static class TextPrinterHelper
    {
        public static void CopyGlyphPlans(RenderVxFormattedString renderVx, GlyphPlanList glyphPlans, float scale)
        {
            int n = glyphPlans.Count;
            //copy 
            var renderVxGlyphPlans = new RenderVxGlyphPlan[n];
            for (int i = 0; i < n; ++i)
            {
                GlyphPlan glyphPlan = glyphPlans[i];
                renderVxGlyphPlans[i] = new RenderVxGlyphPlan(
                    glyphPlan.glyphIndex,
                    glyphPlan.ExactX,
                    glyphPlan.ExactY,
                    glyphPlan.AdvanceX
                    );
            }
            renderVx.glyphList = renderVxGlyphPlans;
        }
    }


}
