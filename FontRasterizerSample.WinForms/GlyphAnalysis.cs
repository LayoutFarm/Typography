//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace PixelFarm.Agg
{
    //this is PixelFarm version ***
    //render with MiniAgg

    public class GlyphContourBuilder
    {
        float curX;
        float curY;
        float latestMoveToX;
        float latestMoveToY;
        GlyphContour currentCnt;
        List<float> allPoints = new List<float>();

        public GlyphContourBuilder()
        {

            Reset();
        }
        public void MoveTo(float x, float y)
        {
            this.latestMoveToX = this.curX = x;
            this.latestMoveToY = this.curY = y;
        }
        public void LineTo(float x, float y)
        {
            currentCnt.AddPart(new GlyphLine(curX, curY, x, y));
            this.curX = x;
            this.curY = y;

            allPoints.Add(x);
            allPoints.Add(y);

        }
        public void CloseFigure()
        {
            currentCnt.AddPart(new GlyphLine(curX, curY, latestMoveToX, latestMoveToY));

            allPoints.Add(latestMoveToX);
            allPoints.Add(latestMoveToY);

            this.curX = latestMoveToX;
            this.curY = latestMoveToY;
        }

        public void Reset()
        {
            currentCnt = new GlyphContour();
            this.latestMoveToX = this.curX = this.latestMoveToY = this.curY = 0;
            allPoints = new List<float>();
        }
        public void Curve3(float p2x, float p2y, float x, float y)
        {
            currentCnt.AddPart(new GlyphCurve3(
                curX, curY,
                p2x, p2y,
                x, y));

            allPoints.Add(curX);
            allPoints.Add(curY);
            allPoints.Add(p2x);
            allPoints.Add(p2y);
            allPoints.Add(x);
            allPoints.Add(y);

            this.curX = x;
            this.curY = y;
        }
        public void Curve4(float p2x, float p2y, float p3x, float p3y, float x, float y)
        {
            currentCnt.AddPart(new GlyphCurve4(
                curX, curY,
                p2x, p2y,
                p3x, p3y,
                x, y));


            allPoints.Add(curX);
            allPoints.Add(curY);
            allPoints.Add(p2x);
            allPoints.Add(p2y);
            allPoints.Add(p3x);
            allPoints.Add(p3y);
            allPoints.Add(x);
            allPoints.Add(y);


            this.curX = x;
            this.curY = y;
        }
        public GlyphContour CurrentContour
        {
            get
            {
                return currentCnt;
            }
        }
        public List<float> GetAllPoints()
        {
            return this.allPoints;
        }
    }

    public class GlyphContour
    {
        internal List<GlyphPart> parts = new List<GlyphPart>();
        internal List<float> allPoints;
        bool analyzed;
        public void AddPart(GlyphPart part)
        {
            parts.Add(part);
        }

        public void Analyze(GlyphPartAnalyzer analyzer)
        {
            if (analyzed) return;
            //
            int j = parts.Count;


            for (int i = 0; i < j; ++i)
            {
                parts[i].Analyze(analyzer);
            }

            analyzed = true;
        }
    }

    public enum GlyphPartKind
    {
        Unknown,
        Line,
        Curve3,
        Curve4
    }

    public class GlyphPartAnalyzer
    {
        int n_steps = 20;
        public int NSteps
        {
            get { return n_steps; }
            set { n_steps = value; }
        }
        public void CreateBezierVxs4(
            int nsteps,
            List<GlyphPoint2D> points,
            Vector2 start, Vector2 end,
            Vector2 control1, Vector2 control2)
        {
            var curve = new VectorMath.BezierCurveCubic(
                start, end,
                control1, control2);
            points.Add(new GlyphPoint2D(start.x, start.y, PointKind.C4Start));
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                points.Add(new GlyphPoint2D(vector2.x, vector2.y, PointKind.CurveInbetween));
                stepSum += eachstep;
            }
            points.Add(new GlyphPoint2D(end.x, end.y, PointKind.C4End));
        }
        public void CreateBezierVxs3(
            int nsteps,
            List<GlyphPoint2D> points,
            Vector2 start, Vector2 end,
            Vector2 control1)
        {
            var curve = new VectorMath.BezierCurveQuadric(
                start, end,
                control1);
            points.Add(new GlyphPoint2D(start.x, start.y, PointKind.C3Start));
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                points.Add(new GlyphPoint2D(vector2.x, vector2.y, PointKind.CurveInbetween));
                stepSum += eachstep;
            }
            points.Add(new GlyphPoint2D(end.x, end.y, PointKind.C3End));
        }
    }
    public abstract class GlyphPart
    {
        public abstract GlyphPartKind Kind { get; }
        public abstract void Analyze(GlyphPartAnalyzer analyzer);
        public abstract List<GlyphPoint2D> GetFlattenPoints();
    }

    public enum PointKind : byte
    {
        LineStart,
        LineStop,
        //
        C3Start,
        C3Control1,
        C3End,
        //
        C4Start,
        C4Control1,
        C4Control2,
        C4End,

        CurveInbetween,
    }
    public class GlyphPoint2D
    {
        //glyph point 
        //for analysis
        public double x;
        public double y;
        public PointKind kind;
        public GlyphPoint2D(double x, double y, PointKind kind)
        {
            this.x = x;
            this.y = y;
            this.kind = kind;
        }
#if DEBUG
        public override string ToString()
        {
            return x + "," + y + " " + kind.ToString();
        }
#endif

    }
    public class GlyphLine : GlyphPart
    {
        internal float x0;
        internal float y0;
        internal float x1;
        internal float y1;

        List<GlyphPoint2D> points;
        public GlyphLine(float x0, float y0, float x1, float y1)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
        }


        public override void Analyze(GlyphPartAnalyzer analyzer)
        {
            points = new List<GlyphPoint2D>();
            points.Add(new GlyphPoint2D(x0, y0, PointKind.LineStart));
            points.Add(new GlyphPoint2D(x1, y1, PointKind.LineStop));
        }
        public override List<GlyphPoint2D> GetFlattenPoints()
        {
            return points;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Line; } }

#if DEBUG
        public override string ToString()
        {
            return "L(" + x0 + "," + y0 + "), (" + x1 + "," + y1 + ")";
        }
#endif
    }
    public class GlyphCurve3 : GlyphPart
    {
        internal float x0, y0, p2x, p2y, x, y;
        List<GlyphPoint2D> points;
        public GlyphCurve3(float x0, float y0, float p2x, float p2y, float x, float y)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.p2x = p2x;
            this.p2y = p2y;
            this.x = x;
            this.y = y;
        }

        public override void Analyze(GlyphPartAnalyzer analyzer)
        {
            points = new List<GlyphPoint2D>();
            analyzer.CreateBezierVxs3(
                analyzer.NSteps,
                points,
                new Vector2(x0, y0),
                new Vector2(x, y),
                new Vector2(p2x, p2y));
        }
        public override List<GlyphPoint2D> GetFlattenPoints()
        {
            return points;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve3; } }
#if DEBUG
        public override string ToString()
        {
            return "C3(" + x0 + "," + y0 + "), (" + p2x + "," + p2y + "),(" + x + "," + y + ")";
        }
#endif
    }
    public class GlyphCurve4 : GlyphPart
    {
        internal float x0, y0, p2x, p2y, p3x, p3y, x, y;
        List<GlyphPoint2D> points;
        public GlyphCurve4(float x0, float y0, float p2x, float p2y,
            float p3x, float p3y,
            float x, float y)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.p2x = p2x;
            this.p2y = p2y;
            this.p3x = p3x;
            this.p3y = p3y;
            this.x = x;
            this.y = y;
        }
        public override void Analyze(GlyphPartAnalyzer analyzer)
        {
            points = new List<GlyphPoint2D>();
            analyzer.CreateBezierVxs4(
                analyzer.NSteps,
                points,
                new Vector2(x0, y0),
                new Vector2(x, y),
                new Vector2(p2x, p2y),
                new Vector2(p3x, p3y)
                );
        }
        public override List<GlyphPoint2D> GetFlattenPoints()
        {
            return points;
        }
        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve4; } }
#if DEBUG
        public override string ToString()
        {
            return "C4(" + x0 + "," + y0 + "), (" + p2x + "," + p2y + "),(" + p3x + "," + p3y + "), (" + x + "," + y + ")";
        }
#endif

    }


}