//MIT, 2014-2016, WinterDev


namespace PixelFarm.Drawing
{
    public interface IRenderElement
    {
        void DrawToThisCanvas(Canvas canvas, Rectangle updateArea);
#if DEBUG
        void dbugShowRenderPart(Canvas canvas, Rectangle r);
#endif
    }

    public interface IRootGraphics
    {
    }
}