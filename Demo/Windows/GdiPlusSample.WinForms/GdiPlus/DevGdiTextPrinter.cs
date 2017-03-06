//MIT, 2016-2017, WinterDev
using System.IO;
using System.Drawing;
using System.Collections.Generic;
//
using Typography.OpenFont;
using Typography.TextLayout;
using System;

namespace SampleWinForms
{
    /// <summary>
    /// developer's version Gdi+ text printer
    /// </summary>
    class DevGdiTextPrinter : DevTextPrinterBase
    {


        Typeface _currentTypeface;
        GlyphPathBuilder _currentGlyphPathBuilder;
        GlyphTranslatorToGdiPath _txToGdiPath;
        GlyphLayout _glyphLayout = new GlyphLayout();
        SolidBrush _fillBrush = new SolidBrush(Color.Black);
        Pen _outlinePen = new Pen(Color.Green);

        public DevGdiTextPrinter()
        {
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
        }
        protected override void OnFontFilenameChanged()
        {
            //reset
            _currentTypeface = null;
            _currentGlyphPathBuilder = null;
        }
        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics DefaultTargetGraphics { get; set; }
        public override void DrawString(char[] textBuffer, float xpos, float ypos)
        {
            this.DrawString(this.DefaultTargetGraphics, textBuffer, xpos, ypos);
        }

        void UpdateTypefaceAndGlyphBuilder()
        {
            if (_currentTypeface == null)
            {

                //1. read typeface from font file
                using (var fs = new FileStream(_currentSelectedFontFile, FileMode.Open))
                {
                    var reader = new OpenFontReader();
                    _currentTypeface = reader.Read(fs);
                }
                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphPathBuilder(_currentTypeface);
                //3. glyph reader,output as Gdi+ GraphicsPath
                _txToGdiPath = new GlyphTranslatorToGdiPath();
            }


            //2.1 
            var hintTech = this.HintTechnique;
            _currentGlyphPathBuilder.UseTrueTypeInstructions = false;//reset
            _currentGlyphPathBuilder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    _currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    _currentGlyphPathBuilder.UseTrueTypeInstructions = true;
                    _currentGlyphPathBuilder.UseVerticalHinting = true;
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
            _fillBrush.Color = this.FillColor;
            _outlinePen.Color = this.OutlineColor;
        }
        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();

        public void DrawString(
                Graphics g,
                char[] textBuffer,
                float x,
                float y)
        {
            //---------------------------------
            //render glyph path with Gdi+ path 
            //this code is demonstration only
            //it is better to wrap it inside 'some class'  
            //---------------------------------
            //1. set some properties
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly   


            //--------------------------------- 
            //2. update
            UpdateTypefaceAndGlyphBuilder();
            // 
            //3. layout glyphs with selected layout technique
            float sizeInPoints = this.FontSizeInPoints;
            _outputGlyphPlans.Clear();
            _glyphLayout.Layout(_currentTypeface, sizeInPoints, textBuffer, _outputGlyphPlans);

            //
            //4. render each glyph
            float pxScale = _currentTypeface.CalculateFromPointToPixelScale(sizeInPoints);
            System.Drawing.Drawing2D.Matrix scaleMat = null;
            // 
            
            int j = _outputGlyphPlans.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = _outputGlyphPlans[i];
                _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                // 

                scaleMat = new System.Drawing.Drawing2D.Matrix(
                    pxScale, 0,//scale x
                    0, pxScale, //scale y
                    x + glyphPlan.x,
                    y + glyphPlan.y //xpos,ypos
                );
                //c_x += (glyphPlan.advX); //move xpos

                //
                _txToGdiPath.Reset();
                _currentGlyphPathBuilder.ReadShapes(_txToGdiPath);
                System.Drawing.Drawing2D.GraphicsPath path = _txToGdiPath.ResultGraphicsPath;
                path.Transform(scaleMat);

                if (FillBackground)
                {
                    g.FillPath(_fillBrush, path);
                }
                if (DrawOutline)
                {
                    g.DrawPath(_outlinePen, path);
                }
            }


            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            
        }

    }
}
