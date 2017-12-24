//MIT, 2016-2017, WinterDev

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Contours;


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
        //
        //for optimization
        GlyphMeshCollection<GraphicsPath> _glyphMeshCollections = new GlyphMeshCollection<GraphicsPath>();


        public DevGdiTextPrinter()
        {
            FillBackground = true;
            FillColor = Color.Black;
            OutlineColor = Color.Green;
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
                //check if we change it or not
                if (value == _currentTypeface) return;
                //change ...
                //check if we have used this typeface before?
                //if not, create a proper glyph builder for it
                //--------------------------------
                //reset 
                _currentTypeface = value;
                _currentGlyphPathBuilder = null;
                //--------------------------------
                if (value == null) return;
                //--------------------------------

                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphPathBuilder(_currentTypeface);
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
                float pointToPixelScale = _currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = _currentTypeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = _currentTypeface.Descender * pointToPixelScale;
                this.FontLineGapPx = _currentTypeface.LineGap * pointToPixelScale;
                this.FontLineSpacingPx = FontAscendingPx - FontDescedingPx + FontLineGapPx;
            }
        }

        public Color FillColor { get; set; }
        public Color OutlineColor { get; set; }
        public Graphics TargetGraphics { get; set; }

        public override void DrawCaret(float x, float y)
        {
            this.TargetGraphics.DrawLine(Pens.Red, x, y, x, y + this.FontAscendingPx);
        }

        GlyphPlanList _outputGlyphPlans = new GlyphPlanList();//for internal use
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {
            //1. update
            UpdateGlyphLayoutSettings();

            //2. unscale layout, in design unit
            this._glyphLayout.Layout(textBuffer, startAt, len);

            //3. scale  to specific font size
            _outputGlyphPlans.Clear();

            GlyphLayoutExtensions.GenerateGlyphPlan(
                _glyphLayout.ResultUnscaledGlyphPositions,
                _currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints),
                false,
                _outputGlyphPlans);

            DrawFromGlyphPlans(_outputGlyphPlans, x, y);
        }

        public MeasuredStringBox MeasureString(char[] textBuffer, int startAt, int len)
        {
            //1. update
            UpdateGlyphLayoutSettings();

            //2. unscale layout, in design unit
            this._glyphLayout.Layout(textBuffer, startAt, len);

            //3. scale  to specific font size
            _outputGlyphPlans.Clear();

            GlyphLayoutExtensions.GenerateGlyphPlan(
                _glyphLayout.ResultUnscaledGlyphPositions,
                _currentTypeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints),
                false,
                _outputGlyphPlans);
            //
            float pxscale = this.Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
            return new MeasuredStringBox(
                  _outputGlyphPlans.AccumAdvanceX * pxscale,
                  _currentTypeface.Ascender * pxscale,
                  _currentTypeface.Descender * pxscale,
                  _currentTypeface.LineGap * pxscale,
                   Typography.OpenFont.Extensions.TypefaceExtensions.CalculateRecommendLineSpacing(_currentTypeface) * pxscale);

        }
        public void UpdateGlyphLayoutSettings()
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
        public override void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        {
            UpdateVisualOutputSettings();

            //draw data in glyph plan 
            //3. render each glyph 

            float sizeInPoints = this.FontSizeInPoints;
            float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
            //
            _glyphMeshCollections.SetCacheInfo(this.Typeface, sizeInPoints, this.HintTechnique);


            //this draw a single line text span***
            int endBefore = startAt + len;

            Graphics g = this.TargetGraphics;
            for (int i = startAt; i < endBefore; ++i)
            {
                GlyphPlan glyphPlan = glyphPlanList[i];

                //check if we have a cache of this glyph
                //if not -> create it

                GraphicsPath foundPath;
                if (!_glyphMeshCollections.TryGetCacheGlyph(glyphPlan.glyphIndex, out foundPath))
                {
                    //if not found then create a new one
                    _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
                    _txToGdiPath.Reset();
                    _currentGlyphPathBuilder.ReadShapes(_txToGdiPath);
                    foundPath = _txToGdiPath.ResultGraphicsPath;

                    //register
                    _glyphMeshCollections.RegisterCachedGlyph(glyphPlan.glyphIndex, foundPath);
                }
                //------
                //then move pen point to the position we want to draw a glyph
                float tx = x + glyphPlan.ExactX;
                float ty = y + glyphPlan.ExactY;
                g.TranslateTransform(tx, ty);

                if (FillBackground)
                {
                    g.FillPath(_fillBrush, foundPath);
                }
                if (DrawOutline)
                {
                    g.DrawPath(_outlinePen, foundPath);
                }
                //and then we reset back ***
                g.TranslateTransform(-tx, -ty);
            }
        }
    }
}
