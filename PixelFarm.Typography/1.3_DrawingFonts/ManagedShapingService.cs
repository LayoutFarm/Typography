//MIT, 2014-present, WinterDev
//-----------------------------------

//MIT, 2016-present, WinterDev
using System.Collections.Generic;
using PixelFarm.Drawing.Fonts;
using Typography.TextLayout;
namespace PixelFarm.Drawing.Text
{
    public class ManagedShapingService : TextShapingService
    {
        protected override void GetGlyphPosImpl(ActualFont actualFont, char[] buffer, int startAt, int len, List<UnscaledGlyphPlan> properGlyphs)
        {
            //do shaping and set text layout
        }
    }
}