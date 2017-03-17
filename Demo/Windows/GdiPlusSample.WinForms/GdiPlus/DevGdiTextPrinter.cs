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
    /// developer's version, Gdi+ text printer
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

        public override Typeface Typeface
        {
            get
            {
                return _currentTypeface;
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
        public Graphics TargetGraphics { get; set; }

        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {
            //1. generate glyph plan
            _outputGlyphPlans.Clear();
            GenerateGlyphPlans(_outputGlyphPlans, textBuffer, startAt, len);
            //2. draw
            DrawString(this.TargetGraphics, _outputGlyphPlans, xpos, ypos);
        }


        void UpdateGlyphLayoutSettings()
        {

            _glyphLayout.ScriptLang = this.ScriptLang;
            _glyphLayout.PositionTechnique = this.PositionTechnique;
            _glyphLayout.EnableLigature = this.EnableLigature;
        }
        void UpdateVisualOutputSettings()
        {
            _currentGlyphPathBuilder.SetHintTechnique(this.HintTechnique);
            _fillBrush.Color = this.FillColor;
            _outlinePen.Color = this.OutlineColor;
        }

        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();

        public override void GenerateGlyphPlans(
              List<GlyphPlan> userGlyphPlanList,
              char[] textBuffer,
              int startAt,
              int len)
        {

            //after we set the this TextPrinter
            //we can use this to print to formatted text buffer
            //similar to DrawString(), but we don't draw it to the canvas surface
            //--------------------------------- 
            //1. update
            UpdateGlyphLayoutSettings();
            // 
            //2. layout glyphs with selected layout technique 
            _glyphLayout.Layout(_currentTypeface, textBuffer, startAt, len, userGlyphPlanList);
            //note that we print to userGlyphPlanList
            //---------------- 
        }
        public void DrawString(Graphics g, List<GlyphPlan> userGlypgPlanList, float x, float y)
        {
            UpdateVisualOutputSettings();

            //draw data in glyph plan 
            //3. render each glyph 
            System.Drawing.Drawing2D.Matrix scaleMat = null;
            float sizeInPoints = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateFromPointToPixelScale(sizeInPoints);
            //this draw a single line text span***
            int j = userGlypgPlanList.Count;
            for (int i = 0; i < j; ++i)
            {
                GlyphPlan glyphPlan = userGlypgPlanList[i];
                _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                // 
                scaleMat = new System.Drawing.Drawing2D.Matrix(
                   1, 0, //scale x
                   0, 1, //scale y
                   x + glyphPlan.x * scale,
                   y + glyphPlan.y * scale //xpos,ypos
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


        //-------------------
        /// <summary>
        /// measure part of string based on current text printer's setting
        /// </summary>
        public override MeasureStringSize MeasureString(char[] textBuffer,
                int startAt,
                int len)
        {
            //TODO: consider extension method
            _outputGlyphPlans.Clear();
            GenerateGlyphPlans(_outputGlyphPlans, textBuffer, startAt, len);
            int j = _outputGlyphPlans.Count;
            if (j == 0)
            {
                return new MeasureStringSize(0, this.FontLineSpacingPx);
            }
            //get last one
            GlyphPlan lastOne = _outputGlyphPlans[j - 1];
            float scale = _currentTypeface.CalculateFromPointToPixelScale(this.FontSizeInPoints);
            return new MeasureStringSize((lastOne.x + lastOne.advX) * scale, this.FontLineSpacingPx);
        }
        public override void MeasureString(char[] textBuffer,
                int startAt,
                int len, out MeasuredStringBox strBox)
        {
            //TODO: consider extension method
            _outputGlyphPlans.Clear();
            GenerateGlyphPlans(_outputGlyphPlans, textBuffer, startAt, len);
            int j = _outputGlyphPlans.Count;
            if (j == 0)
            {
                strBox = new MeasuredStringBox(0,
                    this.FontAscendingPx,
                    this.FontDescedingPx,
                    this.FontLineGapPx);
            }
            //get last one
            GlyphPlan lastOne = _outputGlyphPlans[j - 1];
            float scale = _currentTypeface.CalculateFromPointToPixelScale(this.FontSizeInPoints);
            strBox = new MeasuredStringBox((lastOne.x + lastOne.advX) * scale,
                    this.FontAscendingPx,
                    this.FontDescedingPx,
                    this.FontLineGapPx);
        }
    }
}
