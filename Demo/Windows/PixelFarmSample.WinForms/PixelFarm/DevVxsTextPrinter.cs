//MIT, 2016-2017, WinterDev
using System;
using System.IO;
using System.Collections.Generic;
//
using PixelFarm.Agg;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;

namespace PixelFarm.Drawing.Fonts
{


    class DevVxsTextPrinter : DevTextPrinterBase
    {

        GlyphPathBuilder _glyphPathBuilder;
        GlyphLayout _glyphLayout = new GlyphLayout();
        Dictionary<string, GlyphPathBuilder> _cacheGlyphPathBuilders = new Dictionary<string, GlyphPathBuilder>();
        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();
        //
        HintedVxsGlyphCollection hintGlyphCollection = new HintedVxsGlyphCollection();
        VertexStorePool _vxsPool = new VertexStorePool();
        GlyphTranslatorToVxs _tovxs = new GlyphTranslatorToVxs();


        string _currentSelectedFontFile;
        public DevVxsTextPrinter()
        {

        }
        public override Typeface Typeface
        {
            get
            {
                return _glyphPathBuilder.Typeface;
            }
        }
        public override GlyphLayout GlyphLayoutMan
        {
            get
            {
                return _glyphLayout;
            }
        }
        public override string FontFilename
        {
            get
            {
                return _currentSelectedFontFile;
            }
            set
            {
                if (_currentSelectedFontFile == value)
                {
                    return;
                }

                //switch to another font              
                if (_glyphPathBuilder != null && !_cacheGlyphPathBuilders.ContainsKey(_currentSelectedFontFile))
                {
                    //store current typeface to cache
                    _cacheGlyphPathBuilders[_currentSelectedFontFile] = _glyphPathBuilder;
                }
                _currentSelectedFontFile = value;
                //check if we have this in cache ?
                //if we don't have it, this _currentTypeface will set to null ***                  
                _cacheGlyphPathBuilders.TryGetValue(_currentSelectedFontFile, out _glyphPathBuilder);

                //--------
                if (_glyphPathBuilder == null)
                {
                    //TODO: review here about how to load font file and glyph builder 
                    //1. read typeface ...   
                    using (FileStream fs = new FileStream(_currentSelectedFontFile, FileMode.Open, FileAccess.Read))
                    {
                        var reader = new OpenFontReader();
                        _glyphPathBuilder = new GlyphPathBuilder(reader.Read(fs));
                    }
                }
                OnFontSizeChanged();
            }
        }
        protected override void OnFontSizeChanged()
        {
            //update some font matrix property  
            if (_glyphPathBuilder != null)
            {
                Typeface currentTypeface = _glyphPathBuilder.Typeface;
                float pointToPixelScale = currentTypeface.CalculateFromPointToPixelScale(this.FontSizeInPoints);
                this.FontAscendingPx = currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }

        public CanvasPainter TargetCanvasPainter { get; set; }
        public override void DrawCaret(float xpos, float ypos)
        {
            CanvasPainter p = this.TargetCanvasPainter;
            PixelFarm.Drawing.Color prevColor = p.StrokeColor;
            p.StrokeColor = PixelFarm.Drawing.Color.Red;
            p.Line(xpos, ypos, xpos, ypos + this.FontAscendingPx);
            p.StrokeColor = prevColor;
        }
        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {
            UpdateGlyphLayoutSettings();
            _outputGlyphPlans.Clear();
            _glyphLayout.GenerateGlyphPlans(textBuffer, startAt, len, _outputGlyphPlans, null);
            DrawGlyphPlanList(_outputGlyphPlans, xpos, ypos);

        }

        public override void DrawGlyphPlanList(List<GlyphPlan> glyphPlanList, int startAt, int len, float xpos, float ypos)
        {

            CanvasPainter canvasPainter = this.TargetCanvasPainter;
            Typeface typeface = _glyphPathBuilder.Typeface;
            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            float scale = typeface.CalculateFromPointToPixelScale(fontSizePoint);


            //4. render each glyph
            float ox = canvasPainter.OriginX;
            float oy = canvasPainter.OriginY;
            int endBefore = startAt + len;

            //---------------------------------------------------
            //consider use cached glyph, to increase performance 
            hintGlyphCollection.SetCacheInfo(typeface, fontSizePoint, this.HintTechnique);
            //---------------------------------------------------
            for (int i = startAt; i < endBefore; ++i)
            {
                GlyphPlan glyphPlan = glyphPlanList[i];
                //-----------------------------------
                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                VertexStore glyphVxs;
                if (!hintGlyphCollection.TryGetCacheGlyph(glyphPlan.glyphIndex, out glyphVxs))
                {
                    //if not found then create new glyph vxs and cache it
                    _glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, fontSizePoint);
                    //-----------------------------------  
                    _tovxs.Reset();
                    _glyphPathBuilder.ReadShapes(_tovxs);

                    //TODO: review here, 
                    //float pxScale = _glyphPathBuilder.GetPixelScale();
                    glyphVxs = new VertexStore();
                    _tovxs.WriteOutput(glyphVxs, _vxsPool);
                    //
                    hintGlyphCollection.RegisterCachedGlyph(glyphPlan.glyphIndex, glyphVxs);
                }
                canvasPainter.SetOrigin((float)(glyphPlan.x * scale + xpos), (float)(glyphPlan.y * scale + ypos));
                canvasPainter.Fill(glyphVxs);
            }
            //restore prev origin
            canvasPainter.SetOrigin(ox, oy);
        }

        void UpdateGlyphLayoutSettings()
        {

            //2.1 
            _glyphPathBuilder.SetHintTechnique(this.HintTechnique);
            //2.2
            _glyphLayout.Typeface = this.Typeface;
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
            //3.
            //color...
        }

    }

}