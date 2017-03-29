//MIT, 2014-2017, WinterDev


namespace PixelFarm.Drawing
{
    using Fonts;
    public abstract class RenderVx
    {
    }
    public abstract class RenderVxFormattedString
    {
        public string OriginalString { get; set; }
        public RenderVxGlyphPlan[] glyphList { get; set; }
    }


    namespace Fonts
    {
        public struct RenderVxGlyphPlan
        {
            public readonly ushort glyphIndex;
            public readonly float x;
            public readonly float y;
            public readonly float advX;
            public RenderVxGlyphPlan(ushort glyphIndex, float x, float y, float advX)
            {
                this.glyphIndex = glyphIndex;
                this.x = x;
                this.y = y;
                this.advX = advX;
            }
#if DEBUG
            public override string ToString()
            {
                return "(" + x + "," + y + "), adv:" + advX;
            }
#endif
        }

    }

}