//MIT, 2016-2017, WinterDev

using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.Contours
{

    public class GlyphPathBuilder : GlyphPathBuilderBase
    {
        GlyphOutlineAnalyzer _fitShapeAnalyzer = new GlyphOutlineAnalyzer();
        Dictionary<ushort, GlyphDynamicOutline> _fitoutlineCollection = new Dictionary<ushort, GlyphDynamicOutline>();
        GlyphDynamicOutline _latestDynamicOutline;

        public GlyphPathBuilder(Typeface typeface)
            : base(typeface)
        {

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
                if (this.UseVerticalHinting)
                {
                    if (!_fitoutlineCollection.TryGetValue(glyph.GlyphIndex, out _latestDynamicOutline))
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
                            this._outputGlyphPoints,
                            this._outputContours);
                        //add more information for later scaling process
                        _latestDynamicOutline.OriginalAdvanceWidth = glyph.OriginalAdvanceWidth;
                        _latestDynamicOutline.OriginalGlyphControlBounds = glyph.Bounds;
                        //store to our dynamic outline collection
                        //so we can reuse it
                        _fitoutlineCollection.Add(glyph.GlyphIndex, _latestDynamicOutline);
                        //-------------------
                        //
                        _latestDynamicOutline.GenerateOutput(null, Typeface.CalculateScaleToPixel(RecentFontSizeInPixels));
                        //-------------------

                    }
                    else
                    {
                        if (IsSizeChanged)
                        {
                            _latestDynamicOutline.GenerateOutput(null, Typeface.CalculateScaleToPixel(RecentFontSizeInPixels));
                            IsSizeChanged = false;
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
            if (!TemporaryDisableCustomFit && this.UseVerticalHinting)
            {
                //read from our auto hint fitoutline
                //need scale from original. 
                float toPixelScale = Typeface.CalculateScaleToPixel(RecentFontSizeInPixels);
                if (toPixelScale < 0)
                {
                    toPixelScale = 1;
                }
                _latestDynamicOutline.GenerateOutput(tx, toPixelScale);
            }
            else
            {
                base.ReadShapes(tx);
            }
        }

        public GlyphDynamicOutline LatestGlyphFitOutline
        {
            get
            {
                return _latestDynamicOutline;
            }
        }


    }
}