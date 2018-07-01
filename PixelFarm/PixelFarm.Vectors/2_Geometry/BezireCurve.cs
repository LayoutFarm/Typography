//BSD, 2014-present, WinterDev

using PixelFarm.VectorMath;
namespace PixelFarm.CpuBlit.VertexProcessing
{
    /// <summary>
    /// bezire curve generator
    /// </summary>
    public static class BezierCurve
    {
        static int NSteps = 20; 

        public static void Curve3GetControlPoints(Vector2 start, Vector2 controlPoint, Vector2 endPoint, out Vector2 control1, out Vector2 control2)
        {
            double x1 = start.X + (controlPoint.X - start.X) * 2 / 3;
            double y1 = start.Y + (controlPoint.Y - start.Y) * 2 / 3;
            double x2 = controlPoint.X + (endPoint.X - controlPoint.X) / 3;
            double y2 = controlPoint.Y + (endPoint.Y - controlPoint.Y) / 3;
            control1 = new Vector2(x1, y1);
            control2 = new Vector2(x2, y2);
        }

    }
}