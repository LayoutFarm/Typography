//MIT, 2014-present, WinterDev

using System;
namespace PixelFarm.Drawing
{

    public abstract class RenderVx : IDisposable
    {
        public virtual void Dispose() { }
    }

    public abstract class RenderVxFormattedString : RenderVx
    {
        public short DescendingInPx { get; set; }
        public float Width { get; set; }
        public float SpanHeight { get; set; }
        public VxState State { get; set; } 
        public enum VxState : byte
        {
            NoStrip,
            Waiting,
            Ready,
        }
#if DEBUG
        public abstract string dbugName { get; }
#endif
    }


    namespace Internal
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