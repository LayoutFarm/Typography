//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Contours;

using Typography.OpenFont;

namespace Typography.Contours
{
    public class GlyphTranslatorToContourBuilder : IGlyphTranslator
    {
        IContourBuilder _b;

        public GlyphTranslatorToContourBuilder(IContourBuilder b) => _b = b;

        public void BeginRead(int contourCount) => _b.BeginRead(contourCount);

        public void CloseContour() => _b.CloseContour();

        public void Curve3(float x1, float y1, float x2, float y2) => _b.Curve3(x1, y1, x2, y2);

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => _b.Curve4(x1, y1, x2, y2, x3, y3);

        public void EndRead() => _b.EndRead();

        public void LineTo(float x1, float y1) => _b.LineTo(x1, y1);

        public void MoveTo(float x0, float y0) => _b.MoveTo(x0, y0);
    }
    public class ContourToGlyphTranslator : IContourBuilder
    {
        readonly IGlyphTranslator _tx;

        public ContourToGlyphTranslator(IGlyphTranslator tx) => _tx = tx;

        public void BeginRead(int contourCount) => _tx.BeginRead(contourCount);

        public void CloseContour() => _tx.CloseContour();

        public void Curve3(float x1, float y1, float x2, float y2) => _tx.Curve3(x1, y1, x2, y2);

        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => _tx.Curve4(x1, y1, x2, y2, x3, y3);

        public void EndRead() => _tx.EndRead();

        public void LineTo(float x1, float y1) => _tx.LineTo(x1, y1);

        public void MoveTo(float x0, float y0) => _tx.MoveTo(x0, y0);
    }

    public class GlyphOutlineBuilder : GlyphPathBuilder
    {
        GlyphOutlineAnalyzer _fitShapeAnalyzer = new GlyphOutlineAnalyzer();
        Dictionary<ushort, DynamicOutline> _fitOutlineCollection = new Dictionary<ushort, DynamicOutline>();
        DynamicOutline _latestDynamicOutline;

        public GlyphOutlineBuilder(Typeface typeface)
            : base()
        {
            Typeface = typeface;
            //for specific typeface ***
            //float offsetLenFromMasterOutline = GlyphDynamicEdgeOffset;
            //_latestDynamicOutline.SetDynamicEdgeOffsetFromMasterOutline(offsetLenFromMasterOutline / toPixelScale);
        }

#if DEBUG
        public bool dbugAlwaysDoCurveAnalysis;
#endif

        internal bool TemporaryDisableCustomFit { get; set; }
        /// <summary>
        /// glyph dynamic edge offset
        /// </summary>
        public float GlyphDynamicEdgeOffset { get; set; }

        protected override void FitCurrentGlyph(Glyph glyph)
        {
            //not use interperter so we need to scale it with our mechanism
            //this demonstrate our auto hint engine ***
            //you can change this to your own hint engine***   
            _latestDynamicOutline = null;//reset
            if (this.UseTrueTypeInstructions)
            {
                base.FitCurrentGlyph(glyph);
            }
            else
            {
                //
                if (TemporaryDisableCustomFit)
                {
                    return;
                }
                //
                if (this.UseTrueTypeVerticalHinting)
                {
                    if (!_fitOutlineCollection.TryGetValue(glyph.GlyphIndex, out _latestDynamicOutline))
                    {

                        //---------------------------------------------
                        //test code 
                        //GlyphContourBuilder contBuilder = new GlyphContourBuilder();
                        //contBuilder.Reset();
                        //int x = 100, y = 120, w = 700, h = 200; 
                        //contBuilder.MoveTo(x, y);
                        //contBuilder.LineTo(x + w, y);
                        //contBuilder.LineTo(x + w, y + h);
                        //contBuilder.LineTo(x, y + h);
                        //contBuilder.CloseFigure(); 
                        //--------------------------------------------- 


                        _latestDynamicOutline = _fitShapeAnalyzer.CreateDynamicOutline(
                            _outputGlyphPoints,
                            _outputContours);
                        //add more information for later scaling process
                        _latestDynamicOutline.OriginalAdvanceWidth = glyph.OriginalAdvanceWidth;

                        _latestDynamicOutline.SetDynamicEdgeOffsetFromMasterOutline(GlyphDynamicEdgeOffset);

                        _latestDynamicOutline.SetOriginalGlyphControlBounds(
                            glyph.Bounds.XMin, glyph.Bounds.YMin,
                            glyph.Bounds.XMax, glyph.Bounds.YMax);
                        //store to our dynamic outline collection
                        //so we can reuse it
                        _fitOutlineCollection.Add(glyph.GlyphIndex, _latestDynamicOutline);
                        //-------------------
                        //
                        _latestDynamicOutline.GenerateOutput(null, Typeface.CalculateScaleToPixel(RecentFontSizeInPixels));
                        //-------------------

                    }
                    else
                    {
                        if (HasSizeChanged)
                        {
                            _latestDynamicOutline.GenerateOutput(null, Typeface.CalculateScaleToPixel(RecentFontSizeInPixels));
                            HasSizeChanged = false;
                        }
                    }
                }
            }
        }

        public override void ReadShapes(IGlyphTranslator tx)
        {
            //read output shape from dynamic outline

            if (this.UseTrueTypeInstructions)
            {
                base.ReadShapes(tx);
                return;
            }
            if (!TemporaryDisableCustomFit && this.UseTrueTypeVerticalHinting)
            {
                //read from our auto hint fitoutline
                //need scale from original. 
                float toPixelScale = Typeface.CalculateScaleToPixel(RecentFontSizeInPixels);
                if (toPixelScale < 0)
                {
                    toPixelScale = 1;
                }
                _latestDynamicOutline.GenerateOutput(new ContourToGlyphTranslator(tx), toPixelScale);
            }
            else
            {
                base.ReadShapes(tx);
            }
        }

        public DynamicOutline LatestGlyphFitOutline => _latestDynamicOutline;
    }
}