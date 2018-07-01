//MIT, 2016-present, WinterDev
using System;
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
                _glyphLayout.Typeface = value;
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




        UnscaledGlyphPlanList _reusableUnscaledGlyphPlanList = new UnscaledGlyphPlanList();
        public override void DrawString(char[] textBuffer, int startAt, int len, float x, float y)
        {

            _reusableUnscaledGlyphPlanList.Clear();
            //1. unscale layout, in design unit
            this._glyphLayout.Layout(textBuffer, startAt, len);
            _glyphLayout.GenerateUnscaledGlyphPlans(_reusableUnscaledGlyphPlanList);

            //draw from the glyph plan seq

            DrawFromGlyphPlans(
                new GlyphPlanSequence(_reusableUnscaledGlyphPlanList),
                x, y);

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
        //public override void DrawFromGlyphPlans(IPixelScaledGlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        //{
        //    UpdateVisualOutputSettings();

        //    //draw data in glyph plan 
        //    //3. render each glyph 

        //    float sizeInPoints = this.FontSizeInPoints;
        //    float scale = _currentTypeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
        //    //
        //    _glyphMeshCollections.SetCacheInfo(this.Typeface, sizeInPoints, this.HintTechnique);


        //    //this draw a single line text span***
        //    int endBefore = startAt + len;

        //    Graphics g = this.TargetGraphics;

        //    float acc_x = 0;
        //    float acc_y = 0;

        //    float g_x = 0;
        //    float g_y = 0;

        //    for (int i = startAt; i < endBefore; ++i)
        //    {
        //        PxScaledGlyphPlan glyphPlan = glyphPlanList[i];

        //        //check if we have a cache of this glyph
        //        //if not -> create it

        //        GraphicsPath foundPath;
        //        if (!_glyphMeshCollections.TryGetCacheGlyph(glyphPlan.glyphIndex, out foundPath))
        //        {
        //            //if not found then create a new one
        //            _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
        //            _txToGdiPath.Reset();
        //            _currentGlyphPathBuilder.ReadShapes(_txToGdiPath);
        //            foundPath = _txToGdiPath.ResultGraphicsPath;

        //            //register
        //            _glyphMeshCollections.RegisterCachedGlyph(glyphPlan.glyphIndex, foundPath);
        //        }
        //        //------
        //        //then move pen point to the position we want to draw a glyph

        //        float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX);
        //        float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY);

        //        g_x = (x + (ngx));
        //        g_y = (y + (ngy));

        //        acc_x += (float)Math.Round(glyphPlan.AdvanceX);

        //        //g_x = (float)Math.Round(g_x);
        //        g_y = (float)Math.Floor(g_y);
        //        g.TranslateTransform(g_x, g_y);

        //        if (FillBackground)
        //        {
        //            g.FillPath(_fillBrush, foundPath);
        //        }
        //        if (DrawOutline)
        //        {
        //            g.DrawPath(_outlinePen, foundPath);
        //        }
        //        //and then we reset back ***
        //        g.TranslateTransform(-g_x, -g_y);
        //    }
        //}
        public override void DrawFromGlyphPlans(GlyphPlanSequence glyphPlanList, int startAt, int len, float x, float y)
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

            float acc_x = 0;
            float acc_y = 0;

            float g_x = 0;
            float g_y = 0;

            for (int i = startAt; i < endBefore; ++i)
            {
                UnscaledGlyphPlan glyphPlan = glyphPlanList[i];

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

                float ngx = acc_x + (float)Math.Round(glyphPlan.OffsetX * scale);
                float ngy = acc_y + (float)Math.Round(glyphPlan.OffsetY * scale);

                g_x = (x + (ngx));
                g_y = (y + (ngy));

                acc_x += (float)Math.Round(glyphPlan.AdvanceX * scale);

                //g_x = (float)Math.Round(g_x);
                g_y = (float)Math.Floor(g_y);
                g.TranslateTransform(g_x, g_y);

                if (FillBackground)
                {
                    g.FillPath(_fillBrush, foundPath);
                }
                if (DrawOutline)
                {
                    g.DrawPath(_outlinePen, foundPath);
                }
                //and then we reset back ***
                g.TranslateTransform(-g_x, -g_y);
            }
        }

        //public override void DrawFromGlyphPlans(GlyphPlanList glyphPlanList, int startAt, int len, float x, float y)
        //{
        //    UpdateVisualOutputSettings();

        //    //draw data in glyph plan 
        //    //3. render each glyph 

        //    float sizeInPoints = this.FontSizeInPoints;
        //    float scale = Typeface.CalculateScaleToPixelFromPointSize(sizeInPoints);
        //    //
        //    _glyphMeshCollections.SetCacheInfo(this.Typeface, sizeInPoints, this.HintTechnique);


        //    //this draw a single line text span***
        //    int endBefore = startAt + len;

        //    Graphics g = this.TargetGraphics;
        //    for (int i = startAt; i < endBefore; ++i)
        //    {
        //        GlyphPlan glyphPlan = glyphPlanList[i];

        //        //check if we have a cache of this glyph
        //        //if not -> create it

        //        GraphicsPath foundPath;
        //        if (!_glyphMeshCollections.TryGetCacheGlyph(glyphPlan.glyphIndex, out foundPath))
        //        {
        //            //if not found then create a new one
        //            _currentGlyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, sizeInPoints);
        //            _txToGdiPath.Reset();
        //            _currentGlyphPathBuilder.ReadShapes(_txToGdiPath);
        //            foundPath = _txToGdiPath.ResultGraphicsPath;

        //            //register
        //            _glyphMeshCollections.RegisterCachedGlyph(glyphPlan.glyphIndex, foundPath);
        //        }
        //        //------
        //        //then move pen point to the position we want to draw a glyph
        //        float tx = x + glyphPlan.ExactX;
        //        float ty = y + glyphPlan.ExactY;
        //        g.TranslateTransform(tx, ty);

        //        if (FillBackground)
        //        {
        //            g.FillPath(_fillBrush, foundPath);
        //        }
        //        if (DrawOutline)
        //        {
        //            g.DrawPath(_outlinePen, foundPath);
        //        }
        //        //and then we reset back ***
        //        g.TranslateTransform(-tx, -ty);
        //    }
        //}
    }
}
