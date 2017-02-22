//MIT, 2016-2017, WinterDev 
using System.Collections.Generic;
using Typography.OpenFont;
using Typography.TextLayout;
using PixelFarm.Drawing.Fonts;
namespace SampleWinForms
{

    partial class TextPrinter
    {
        partial void Print(Typeface typeface, float size, char[] str, List<GlyphPlan> glyphPlanBuffer)
        {
            //1. layout
            _glyphLayout.Layout(typeface, size, str, glyphPlanBuffer);
            var glyphPathBuilder = new MyGlyphPathBuilder(typeface);
            int j = glyphPlanBuffer.Count;

            float pxScale = typeface.CalculateFromPointToPixelScale(size);
            for (int i = 0; i < j; ++i)
            {

                GlyphPlan glyphPlan = glyphPlanBuffer[i];
                //-----------------------------------
                //check if we static vxs/bmp for this glyph
                //if not, create and cache
                //-----------------------------------  
                glyphPathBuilder.BuildFromGlyphIndex(glyphPlan.glyphIndex, size);
                //-----------------------------------  
                var vxsBuilder = new GlyphPathBuilderVxs();
                glyphPathBuilder.ReadShapes(vxsBuilder);
                glyphPlan.vxs = vxsBuilder.GetVxs(pxScale);
            }
        }
    }
}
