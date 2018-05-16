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
    class DevGdiTextPrinter : TextPrinterBase
    {
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

        public override GlyphLayout GlyphLayout
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
                return base.Typeface;
            }
            set
            {
                //check if we change it or not
                if (value == base.Typeface) return;
                //change ...
                //check if we have used this typeface before?
                //if not, create a proper glyph builder for it
                //--------------------------------
                //reset 
                base.Typeface = value;
                _currentGlyphPathBuilder = null;
                //--------------------------------
                if (value == null) return;
                //--------------------------------

                //2. glyph builder
                _currentGlyphPathBuilder = new GlyphPathBuilder(base.Typeface);
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
            if (Typeface != null)
            {
                float pointToPixelScale = Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints);
                this.FontAscendingPx = Typeface.Ascender * pointToPixelScale;
                this.FontDescedingPx = Typeface.Descender * pointToPixelScale;
                this.FontLineGapPx = Typeface.LineGap * pointToPixelScale;
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
            //1. unscale layout, in design unit
            this._glyphLayout.Layout(textBuffer, startAt, len);

            //2. scale  to specific font size
            _outputGlyphPlans.Clear();

            GlyphLayoutExtensions.GenerateGlyphPlan(
                _glyphLayout.ResultUnscaledGlyphPositions,
                Typeface.CalculateScaleToPixelFromPointSize(this.FontSizeInPoints),
                false,
                _outputGlyphPlans);

            DrawFromGlyphPlans(_outputGlyphPlans, x, y);
        }

        public MeasuredStringBox MeasureString(char[] textBuffer, int startAt, int len)
        {
            //unscale layout, in design unit
            return this._glyphLayout.LayoutAndMeasureString(textBuffer, startAt, len, this.FontSizeInPoints, out _outputGlyphPlans);
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
            float scale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
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
