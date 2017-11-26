////MIT, 2014-2017, WinterDev
//using System.Collections.Generic;
//using Typography.OpenFont;

//namespace Typography.TextLayout
//{
//    public class SingleLineGlyphFlowLayoutEngine
//    {
//        class InstalledFont
//        {

//        }

//        //PixelScaleLayoutEngine _pxScaleEngine;
//        //GlyphMeshStore _glyphMeshStore;

//        GlyphLayout _glyphLayout = new GlyphLayout();
//        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();
//        Typeface _currentTypeface;

//        public SingleLineGlyphFlowLayoutEngine()
//        {

//        }
//        public void Layout()
//        {
//            if (_glyphLayout.Typeface != this.Typeface)
//            {
//                _glyphLayout.Typeface = this.Typeface;
//            }
//            _glyphLayout.EnableLigature = true;
//        }
//        public void SetScriptLang(ScriptLang lang)
//        {
//            _glyphLayout.ScriptLang = lang;
//        }
//        public Typeface Typeface
//        {
//            get
//            {
//                return _currentTypeface;
//            }
//            set
//            {
//                if (_currentTypeface == value) return;
//                _currentTypeface = value;
//                OnFontSizeChanged();
//            }
//        }



//        Dictionary<InstalledFont, Typeface> _cachedTypefaces = new Dictionary<InstalledFont, Typeface>();

//        bool TryGetTypeface(InstalledFont instFont, out Typeface found)
//        {
//            return _cachedTypefaces.TryGetValue(instFont, out found);
//        }
//        void RegisterTypeface(InstalledFont instFont, Typeface typeface)
//        {
//            _cachedTypefaces[instFont] = typeface;
//        }
//        /// <summary>
//        /// for layout that use with our  lcd subpixel rendering technique 
//        /// </summary>
//        public bool UseWithLcdSubPixelRenderingTechnique
//        {
//            get { return _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique; }
//            set
//            {
//                _pxScaleEngine.UseWithLcdSubPixelRenderingTechnique = value;
//            }
//        }
//        public void ChangeFont(RequestFont font)
//        {
//            //1.  resolve actual font file
//            this._reqFont = font;
//            InstalledFont installedFont = _fontLoader.GetFont(font.Name, font.Style.ConvToInstalledFontStyle());
//            Typeface foundTypeface;

//            if (!TryGetTypeface(installedFont, out foundTypeface))
//            {
//                //if not found then create a new one
//                //if not found
//                //create the new one
//                using (FileStream fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
//                {
//                    var reader = new OpenFontReader();
//                    foundTypeface = reader.Read(fs);
//                }
//                RegisterTypeface(installedFont, foundTypeface);
//            }

//            this.Typeface = foundTypeface;
//        }

//        public
//        protected void OnFontSizeChanged()
//        {
//            //update some font metrics property   
//            Typeface currentTypeface = _currentTypeface;
//            if (currentTypeface != null)
//            {
//                float pointToPixelScale = currentTypeface.CalculateToPixelScaleFromPointSize(this.FontSizeInPoints);
//                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
//                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
//                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
//                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
//            }

//        }



//        public void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len)
//        {

//            //1. update some props.. 
//            //2. update current type face
//            UpdateTypefaceAndGlyphBuilder();
//            Typeface typeface = _currentTypeface;// _glyphPathBuilder.Typeface;
//            //3. layout glyphs with selected layout technique
//            //TODO: review this again, we should use pixel?

//            float pxscale = typeface.CalculateToPixelScaleFromPointSize(FontSizeInPoints);
//            _outputGlyphPlans.Clear();
//            _glyphLayout.Layout(typeface, text, startAt, len, _outputGlyphPlans);
//            TextPrinterHelper.CopyGlyphPlans(renderVx, _outputGlyphPlans, pxscale);
//        }


//        void UpdateTypefaceAndGlyphBuilder()
//        {
//            //1. update _glyphPathBuilder for current typeface
//            UpdateGlyphLayoutSettings();
//        }
//        public void UpdateGlyphLayoutSettings()
//        {
//            if (this._reqFont == null)
//            {
//                //this.ScriptLang = canvasPainter.CurrentFont.GetOpenFontScriptLang();
//                ChangeFont(canvasPainter.CurrentFont);
//            }

//            //2.1              
//            _glyphMeshStore.SetHintTechnique(this.HintTechnique);
//            //2.2
//            _glyphLayout.Typeface = this.Typeface;
//            _glyphLayout.ScriptLang = this.ScriptLang;
//            _glyphLayout.PositionTechnique = this.PositionTechnique;
//            _glyphLayout.EnableLigature = this.EnableLigature;
//            //3.
//            //color...
//        }

//        public void DrawString(RenderVxFormattedString renderVx, double x, double y)
//        {
//            float ox = canvasPainter.OriginX;
//            float oy = canvasPainter.OriginY;

//            //1. update some props.. 
//            //2. update current type face
//            UpdateTypefaceAndGlyphBuilder();
//            _glyphMeshStore.SetFont(_currentTypeface, this.FontSizeInPoints);
//            //3. layout glyphs with selected layout technique
//            //TODO: review this again, we should use pixel? 
//            float fontSizePoint = this.FontSizeInPoints;
//            float scale = _currentTypeface.CalculateToPixelScaleFromPointSize(fontSizePoint);
//            RenderVxGlyphPlan[] glyphPlans = renderVx.glyphList;
//            int j = glyphPlans.Length;
//            //---------------------------------------------------
//            //consider use cached glyph, to increase performance 

//            //GlyphPosPixelSnapKind x_snap = this.GlyphPosPixelSnapX;
//            //GlyphPosPixelSnapKind y_snap = this.GlyphPosPixelSnapY;
//            float g_x = 0;
//            float g_y = 0;
//            float baseY = (int)y;

//            for (int i = 0; i < j; ++i)
//            {
//                RenderVxGlyphPlan glyphPlan = glyphPlans[i];
//                //-----------------------------------
//                //TODO: review here ***
//                //PERFORMANCE revisit here 
//                //if we have create a vxs we can cache it for later use?
//                //-----------------------------------  
//                VertexStore vxs = _glyphMeshStore.GetGlyphMesh(glyphPlan.glyphIndex);
//                g_x = (float)(glyphPlan.x * scale + x);
//                g_y = (float)glyphPlan.y * scale;

//                canvasPainter.SetOrigin(g_x, g_y);
//                canvasPainter.Fill(vxs);
//            }
//            //restore prev origin
//            canvasPainter.SetOrigin(ox, oy);
//        }
//    }
//}