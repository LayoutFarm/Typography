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

        List<float> _points = new List<float>();
        List<PathCommand> _cmds = new List<PathCommand>();
        public GraphicsPath() { }
        public void AddArc(float x, float y,
            float width, float height,
            float startAngle,
            float sweepAngle)
        {
            _cmds.Add(PathCommand.Arc);
            _points.Add(x);
            _points.Add(y);
            _points.Add(width);
            _points.Add(height);
            _points.Add(startAngle);//***
            _points.Add(sweepAngle);//***
            

        }
        public void AddArc(RectangleF rectF, float startAngle, float sweepAngle)
        {
            _cmds.Add(PathCommand.Arc);
            _points.Add(rectF.X);
            _points.Add(rectF.Y);
            _points.Add(rectF.Width);
            _points.Add(rectF.Height);
            _points.Add(startAngle);//***
            _points.Add(sweepAngle);//***
        }
        public void AddLine(float x1, float y1, float x2, float y2)
        {
            _cmds.Add(PathCommand.Line);
            _points.Add(x1); _points.Add(y1);
            _points.Add(x2); _points.Add(y2);
        }
        public void AddLine(PointF p1, PointF p2)
        {
            _cmds.Add(PathCommand.Line);
            _points.Add(p1.X); _points.Add(p1.Y);
            _points.Add(p2.X); _points.Add(p2.Y);
        }
        public void CloseFigure()
        {
            _cmds.Add(PathCommand.CloseFigure);

            //no points
        }
        public void Dispose()
        {
        }
        public void StartFigure()
        {
            _cmds.Add(PathCommand.StartFigure);
            //no points
        }
        public void AddEllipse(float x, float y, float w, float h)
        {
            _cmds.Add(PathCommand.Ellipse);
            _points.Add(x);
            _points.Add(y);
            _points.Add(w);
            _points.Add(h);

        }
        public void AddRectangle(RectangleF rectF)
        {
            _cmds.Add(PathCommand.Rect);
            _points.Add(rectF.X);
            _points.Add(rectF.Y);
            _points.Add(rectF.Width);
            _points.Add(rectF.Height);
        }
        public object InnerPath { get; set; }
        public void AddBezierCurve(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            _cmds.Add(PathCommand.Bezier);
            _points.Add(p1.X); _points.Add(p1.Y);
            _points.Add(p2.X); _points.Add(p2.Y);
            _points.Add(p3.X); _points.Add(p3.Y);
            _points.Add(p4.X); _points.Add(p4.Y);

        } 
        public static void GetPathData(GraphicsPath p, out List<float> points, out List<PathCommand> cmds)
        {
            points = p._points;
            cmds = p._cmds;
        }
    }



}