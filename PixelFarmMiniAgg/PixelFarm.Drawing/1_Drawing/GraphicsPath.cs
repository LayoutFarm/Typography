//MIT, 2014-2016, WinterDev

namespace PixelFarm.Drawing
{
    public abstract class GraphicsPath : System.IDisposable
    {
        public abstract void AddArc(float x, float y, float width, float height, float startAngle, float sweepAngle);
        public abstract void AddArc(RectangleF rectF, float startAngle, float sweepAngle);
        public abstract void AddLine(float x1, float y1, float x2, float y2);
        public abstract void AddLine(PointF p1, PointF p2);
        public abstract void CloseFigure();
        public abstract void Dispose();
        public abstract void StartFigure();
        public abstract void AddEllipse(float x, float y, float w, float h);
        public abstract void AddRectangle(RectangleF r);
        public abstract object InnerPath { get; }
       

        public abstract void AddBezierCurve(PointF p1, PointF p2, PointF p3, PointF p4);
        
    }
}