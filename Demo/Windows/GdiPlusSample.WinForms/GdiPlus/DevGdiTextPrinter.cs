//MIT, 2016-2017, WinterDev
using System.IO;
using System.Drawing;
using System.Collections.Generic;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;


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
                //for gdi path***
                //3. glyph reader,output as Gdi+ GraphicsPath
                _txToGdiPath = new GlyphTranslatorToGdiPath();
                //4.
                OnFontSizeChanged();
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
        }
        protected override void OnFontSizeChanged()
        {
            //update some font matrix property  
            if (_currentTypeface != null)
            {
                float pointToPixelScale = _currentTypeface.CalculateToPixelScaleFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = _currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = _currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = _currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }

        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

        public override void DrawCaret(float xpos, float ypos)
        {
            this.TargetGraphics.DrawLine(Pens.Red, xpos, ypos, xpos, ypos + this.FontAscendingPx);
        }

        List<GlyphPlan> _outputGlyphPlans = new List<GlyphPlan>();//for internal use
        public override void DrawString(char[] textBuffer, int startAt, int len, float xpos, float ypos)
        {
            UpdateGlyphLayoutSettings();
            _outputGlyphPlans.Clear();
            this._glyphLayout.GenerateGlyphPlans(textBuffer, startAt, len, _outputGlyphPlans, null);
            //2. draw
            DrawGlyphPlanList(_outputGlyphPlans, xpos, ypos);
        }


        void UpdateGlyphLayoutSettings()
        {
            _glyphLayout.Typeface = this.Typeface;
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


        public override void DrawGlyphPlanList(List<GlyphPlan> glyphPlanList, int startAt, int len, float x, float y)
        {
            UpdateVisualOutputSettings();

            //draw data in glyph plan 
            //3. render each glyph 
            System.Drawing.Drawing2D.Matrix scaleMat = null;
            float sizeInPoints = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateToPixelScaleFromPointSize(sizeInPoints);
            //this draw a single line text span***
            int endBefore = startAt + len;

            Graphics g = this.TargetGraphics;
            for (int i = startAt; i < endBefore; ++i)
            {
                GlyphPlan glyphPlan = glyphPlanList[i];
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
    }
}
