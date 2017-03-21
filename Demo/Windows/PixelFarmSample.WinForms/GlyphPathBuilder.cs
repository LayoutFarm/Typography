//MIT, 2016-2017, WinterDev

using Typography.OpenFont;
using System.Collections.Generic;

namespace Typography.Rendering
{


    public class GlyphPathBuilder : GlyphPathBuilderBase
    {
        GlyphFitOutlineAnalyzer _fitShapeAnalyzer = new GlyphFitOutlineAnalyzer();
        Dictionary<ushort, GlyphFitOutline> _fitoutlineCollection = new Dictionary<ushort, GlyphFitOutline>();
        GlyphFitOutline _fitOutline; 
        public GlyphPathBuilder(Typeface typeface)
            : base(typeface)
        {
        }
        protected override void FitCurrentGlyph(ushort glyphIndex, Glyph glyph)
        {
            //not use interperter so we need to scale it with our machnism
            //this demonstrate our auto hint engine ***
            //you can change this to your own hint engine***   
            if (this.UseVerticalHinting)
            {
                if (!_fitoutlineCollection.TryGetValue(glyphIndex, out _fitOutline))
                {
                    _fitOutline = _fitShapeAnalyzer.Analyze(
                        this._outputGlyphPoints,
                        this._outputContours);
                    _fitoutlineCollection.Add(glyphIndex, _fitOutline);
                }
            }
        }
        public override void ReadShapes(IGlyphTranslator tx)
        {
            if (this.UseVerticalHinting)
            {
                //read from our auto hint fitoutline
                //need scale from original.
                float toPixelScale = Typeface.CalculateToPixelScale(this.RecentFontSizeInPixels);
                if (toPixelScale < 0)
                {
                    toPixelScale = 1;
                }
                _fitOutline.ReadOutput(tx, toPixelScale);
            }
            else
            {
                base.ReadShapes(tx);
            }
        }
    }
}