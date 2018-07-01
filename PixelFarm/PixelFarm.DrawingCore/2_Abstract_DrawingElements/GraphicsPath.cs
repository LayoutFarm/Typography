//MIT, 2014-present, WinterDev
using System.Collections.Generic;

namespace PixelFarm.Drawing
{
    public enum PathCommand : byte
    {
        StartFigure,
        CloseFigure,
        Arc,
        Line,
        Ellipse,
        Rect,
        Bezier,
    }

     

    public sealed class GraphicsPath : System.IDisposable
    {

        List<float> points = new List<float>();
        List<PathCommand> cmds = new List<PathCommand>();
        public GraphicsPath() { }
        public void AddArc(float x, float y,
            float width, float height,
            float startAngle,
            float sweepAngle)
        {
            cmds.Add(PathCommand.Arc);
            points.Add(x);
            points.Add(y);
            points.Add(width);
            points.Add(height);
            points.Add(startAngle);//***
            points.Add(sweepAngle);//***

        }
        public void AddArc(RectangleF rectF, float startAngle, float sweepAngle)
        {
            cmds.Add(PathCommand.Arc);
            points.Add(rectF.X);
            points.Add(rectF.Y);
            points.Add(rectF.Width);
            points.Add(rectF.Height);
            points.Add(startAngle);//***
            points.Add(sweepAngle);//***
        }
        public void AddLine(float x1, float y1, float x2, float y2)
        {
            cmds.Add(PathCommand.Line);
            points.Add(x1); points.Add(y1);
            points.Add(x2); points.Add(y2);
        }
        public void AddLine(PointF p1, PointF p2)
        {
            cmds.Add(PathCommand.Line);
            points.Add(p1.X); points.Add(p1.Y);
            points.Add(p2.X); points.Add(p2.Y);
        }
        public void CloseFigure()
        {
            cmds.Add(PathCommand.CloseFigure);

            //no points
        }
        public void Dispose()
        {
        }
        public void StartFigure()
        {
            cmds.Add(PathCommand.StartFigure);
            //no points
        }
        public void AddEllipse(float x, float y, float w, float h)
        {
            cmds.Add(PathCommand.Ellipse);
            points.Add(x);
            points.Add(y);
            points.Add(w);
            points.Add(h);

        }
        public void AddRectangle(RectangleF rectF)
        {
            cmds.Add(PathCommand.Rect);
            points.Add(rectF.X);
            points.Add(rectF.Y);
            points.Add(rectF.Width);
            points.Add(rectF.Height);
        }
        public object InnerPath { get; set; }
        public void AddBezierCurve(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            cmds.Add(PathCommand.Bezier);
            points.Add(p1.X); points.Add(p1.Y);
            points.Add(p2.X); points.Add(p2.Y);
            points.Add(p3.X); points.Add(p3.Y);
            points.Add(p4.X); points.Add(p4.Y);

        } 
        public static void GetPathData(GraphicsPath p, out List<float> points, out List<PathCommand> cmds)
        {
            points = p.points;
            cmds = p.cmds;
        }
    }



}