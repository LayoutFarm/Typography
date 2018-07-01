//MIT, 2014-present, WinterDev

using System;
namespace PixelFarm.Drawing
{
    using Fonts;
    public abstract class RenderVx : IDisposable
    {
        public virtual void Dispose() { }
    }
    public abstract class RenderVxFormattedString : RenderVx
    {
        public abstract string OriginalString { get; }
        public RenderVxGlyphPlan[] glyphList { get; set; }
        public int RecommmendLineSpacing { get; set; }
        public int LineGap { get; set; }
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