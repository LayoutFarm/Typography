//BSD, 2014-2017, WinterDev

using PixelFarm.VectorMath;
namespace PixelFarm.Agg.VertexSource
{
    /// <summary>
    /// bezire curve generator
    /// </summary>
    public static class BezierCurve
    {
        static int NSteps = 20;
        public static void CreateBezierVxs4(VertexStore vxs, Vector2 start, Vector2 end,
            Vector2 control1, Vector2 control2)
        {
            var curve = new VectorMath.BezierCurveCubic(
                start, end,
                control1, control2);
            vxs.AddLineTo(start.x, start.y);
            float eachstep = (float)1 / NSteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < NSteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                vxs.AddLineTo(vector2.x, vector2.y);
                stepSum += eachstep;
            }
            vxs.AddLineTo(end.x, end.y);
        }
        public static void Curve3GetControlPoints(Vector2 start, Vector2 controlPoint, Vector2 endPoint, out Vector2 control1, out Vector2 control2)
        {
            double x1 = start.X + (controlPoint.X - start.X) * 2 / 3;
            double y1 = start.Y + (controlPoint.Y - start.Y) * 2 / 3;
            double x2 = controlPoint.X + (endPoint.X - controlPoint.X) / 3;
            double y2 = controlPoint.Y + (endPoint.Y - controlPoint.Y) / 3;
            control1 = new Vector2(x1, y1);
            control2 = new Vector2(x2, y2);
        }
        public static void CreateBezierVxs3(VertexStore vxs, Vector2 start, Vector2 end,
           Vector2 control1)
        {
            var curve = new VectorMath.BezierCurveQuadric(
                start, end,
                control1);
            vxs.AddLineTo(start.x, start.y);
            float eachstep = (float)1 / NSteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < NSteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                vxs.AddLineTo(vector2.x, vector2.y);
                stepSum += eachstep;
            }
            vxs.AddLineTo(end.x, end.y);
            //------------------------------------------------------
            //convert c3 to c4
            //Vector2 c4p2, c4p3;
            //Curve3GetControlPoints(start, control1, end, out c4p2, out c4p3);
            //CreateBezierVxs4(vxs, start, end, c4p2, c4p3); 
        }
    }
}