//MIT, 2016-2017, WinterDev
using System.IO;
using System.Drawing;
using System.Collections.Generic;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;
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

        string _currentSelectedFontFile;

        public DevGdiTextPrinter()
        {
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
        }
        public override string FontFilename
        {
            get
            {
                return _currentSelectedFontFile;
            }
            set
            {
                if (value == this._currentSelectedFontFile)
                {
                    return;
                }

                //--------------------------------
                //reset 
                _currentTypeface = null;
                _currentGlyphPathBuilder = null;

                _currentSelectedFontFile = value;
                //load new typeface 

                //1. read typeface from font file
                using (var fs = new FileStream(_currentSelectedFontFile, FileMode.Open))
                {
                    var reader = new OpenFontReader();
                    _currentTypeface = reader.Read(fs);
                }
                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphPathBuilder(_currentTypeface);
                _currentGlyphPathBuilder.MinorAdjustFitYForAutoFit = true;

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
            if (_currentTypeface != null)
            {
                float pointToPixelScale = _currentTypeface.CalculateFromPointToPixelScale(this.FontSizeInPoints);
                this.FontAscendingPx = _currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = _currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = _currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }



        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics DefaultTargetGraphics { get; set; }

        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {
            this.DrawString(this.DefaultTargetGraphics, textBuffer, startAt, len, xpos, ypos);
        }

        void UpdateTypefaceAndGlyphBuilder()
        {

            //1  
            _currentGlyphPathBuilder.SetHintTechnique(this.HintTechnique);
            //2
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
                int startAt,
                int len,
                float x,
                float y)
        {   
            //1. update
            UpdateTypefaceAndGlyphBuilder();
            // 
            //2. layout glyphs with selected layout technique
            float sizeInPoints = this.FontSizeInPoints;
            _outputGlyphPlans.Clear();
            _glyphLayout.Layout(_currentTypeface, sizeInPoints, textBuffer, startAt, len, _outputGlyphPlans);
            //----------------
            //
            //3. render each glyph 
            System.Drawing.Drawing2D.Matrix scaleMat = null;
            //this draw a single line text span***
            int j = _outputGlyphPlans.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = _outputGlyphPlans[i];
                _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                // 
                // float pxScale = _currentGlyphPathBuilder.GetPixelScale();
                scaleMat = new System.Drawing.Drawing2D.Matrix(
                    1, 0,//scale x
                    0, 1, //scale y
                    x + glyphPlan.x,
                    y + glyphPlan.y //xpos,ypos
                );

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


        }

    }
}
