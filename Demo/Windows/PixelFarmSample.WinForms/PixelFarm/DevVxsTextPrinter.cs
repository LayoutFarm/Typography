//MIT, 2016-2017, WinterDev
using System;
using System.IO;
using System.Collections.Generic;
//
using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;
using Typography.OpenFont;
using Typography.TextLayout;


namespace SampleWinForms
{

    class DevVxsTextPrinter : DevTextPrinterBase
    {

        GlyphPathBuilder _glyphPathBuilder;
        GlyphLayout _glyphLayout = new GlyphLayout();
        Dictionary<string, GlyphPathBuilder> _cacheGlyphPathBuilders = new Dictionary<string, GlyphPathBuilder>();
        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>(20);

        public DevVxsTextPrinter()
        {

        }
        protected override void OnFontFilenameChanged()
        {

            //switch to another font  
            //store current typeface to cache
            if (_glyphPathBuilder != null && !_cacheGlyphPathBuilders.ContainsKey(_currentSelectedFontFile))
            {
                _cacheGlyphPathBuilders[_currentSelectedFontFile] = _glyphPathBuilder;
            }
            //check if we have this in cache ?
            //if we don't have it, this _currentTypeface will set to null ***                  
            _cacheGlyphPathBuilders.TryGetValue(_currentSelectedFontFile, out _glyphPathBuilder);

        }

        public CanvasPainter DefaultCanvasPainter { get; set; }
        public override void DrawString(char[] textBuffer, float xpos, float ypos)
        {
            this.DrawString(this.DefaultCanvasPainter, textBuffer, xpos, ypos);
        }
        public void DrawString(CanvasPainter canvasPainter, string text, double x, double y)
        {
            DrawString(canvasPainter, text.ToCharArray(), x, y);
        }
        public void DrawString(CanvasPainter canvasPainter, char[] text, double x, double y)
        {

            //1. update some props..

            //2. update current type face
            UpdateTypefaceAndGlyphBuilder();
            Typeface typeface = _glyphPathBuilder.Typeface;

            //3. layout glyphs with selected layout technique
            //TODO: review this again, we should use pixel?

            float fontSizePoint = this.FontSizeInPoints;
            _outputGlyphPlans.Clear();
            _glyphLayout.Layout(typeface, fontSizePoint, text, _outputGlyphPlans);
            //4. render each glyph
            float pxScale = typeface.CalculateFromPointToPixelScale(fontSizePoint);


            float ox = canvasPainter.OriginX;
            float oy = canvasPainter.OriginY;
            int j = _outputGlyphPlans.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = _outputGlyphPlans[i];
                //-----------------------------------

                //TODO: review here ***
                //PERFORMANCE revisit here 
                //if we have create a vxs we can cache it for later use?
                //-----------------------------------  
                _glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, fontSizePoint);
                //-----------------------------------  
                _glyphReader.Reset();
                _glyphPathBuilder.ReadShapes(_glyphReader);

                //TODO: review here, 

                VertexStore outputVxs = _vxsPool.GetFreeVxs();
                _glyphReader.WriteOutput(outputVxs, _vxsPool, pxScale);
                canvasPainter.SetOrigin((float)(glyphPlan.x + x), (float)(glyphPlan.y + y));
                canvasPainter.Fill(outputVxs);
                _vxsPool.Release(ref outputVxs);

            }
            //restore prev origin
            canvasPainter.SetOrigin(ox, oy);
        }

        //-----------------------
        VertexStorePool _vxsPool = new VertexStorePool();
        GlyphTranslatorToVxs _glyphReader = new GlyphTranslatorToVxs();
      

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
            var hintTech = this.HintTechnique;
            _glyphPathBuilder.UseTrueTypeInstructions = false;//reset
            _glyphPathBuilder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    _glyphPathBuilder.UseTrueTypeInstructions = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    _glyphPathBuilder.UseTrueTypeInstructions = true;
                    _glyphPathBuilder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    break;
            }
            //2.2
            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
            //3.
            //color...
        }
    }

}