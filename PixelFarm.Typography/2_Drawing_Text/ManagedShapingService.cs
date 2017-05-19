//MIT, 2014-2017, WinterDev
//-----------------------------------

//MIT, 2016-2017, WinterDev
using System.Collections.Generic;
using PixelFarm.Drawing.Fonts;
using Typography.TextLayout;
namespace PixelFarm.Drawing.Text
{
    public class ManagedShapingService : TextShapingService
    {
        protected override void GetGlyphPosImpl(ActualFont actualFont, char[] buffer, int startAt, int len, List<GlyphPlan> properGlyphs)
        {
            //do shaping and set text layout
        }
    }
}