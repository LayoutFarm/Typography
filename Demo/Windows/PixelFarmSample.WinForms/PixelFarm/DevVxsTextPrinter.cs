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


        public DevVxsTextPrinter()
        {

        }
        protected override void OnFontFilenameChanged()
        {

            //switch to another font              
            if (_glyphPathBuilder != null && !_cacheGlyphPathBuilders.ContainsKey(_currentSelectedFontFile))
            {
                //store current typeface to cache
                _cacheGlyphPathBuilders[_currentSelectedFontFile] = _glyphPathBuilder;
            }
            //check if we have this in cache ?
            //if we don't have it, this _currentTypeface will set to null ***                  
            _cacheGlyphPathBuilders.TryGetValue(_currentSelectedFontFile, out _glyphPathBuilder);
        }

        public CanvasPainter DefaultCanvasPainter { get; set; }

        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {

            DrawString(this.DefaultCanvasPainter, textBuffer, startAt, len, xpos, ypos);
        }
        public void DrawString(CanvasPainter canvasPainter, char[] text, int start, int len, double x, double y)
        {

            //1. update some props..

            //2. update current type face
            UpdateTypefaceAndGlyphBuilder();
            Typeface typeface = _glyphPathBuilder.Typeface;

            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            _outputGlyphPlans.Clear();
            _glyphLayout.Layout(typeface, fontSizePoint, text, start, len, _outputGlyphPlans);

            //4. render each glyph
            float ox = canvasPainter.OriginX;
            float oy = canvasPainter.OriginY;
            int j = _outputGlyphPlans.Count;

            //---------------------------------------------------
            //consider use cached glyph, to increase performance 
            hintGlyphCollection.SetCacheInfo(typeface, fontSizePoint, this.HintTechnique);
            //---------------------------------------------------
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = _outputGlyphPlans[i];
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
                canvasPainter.SetOrigin((float)(glyphPlan.x + x), (float)(glyphPlan.y + y));
                canvasPainter.Fill(glyphVxs);
            }
            //restore prev origin
            canvasPainter.SetOrigin(ox, oy);
        }

        //-----------------------
        VertexStorePool _vxsPool = new VertexStorePool();
        GlyphTranslatorToVxs _tovxs = new GlyphTranslatorToVxs();


        void UpdateTypefaceAndGlyphBuilder()
        {
            //1. update _glyphPathBuilder for current typeface 
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
            //2.1 

            _glyphPathBuilder.UseTrueTypeInstructions = this.UseTrueTypeInstructions;//reset
            _glyphPathBuilder.UseVerticalHinting = this.UseVerticalHint;//reset

            //2.2
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
            //3.
            //color...
        }
    }

}