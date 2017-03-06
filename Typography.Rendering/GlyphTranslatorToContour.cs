//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Typography.Rendering
{

    //this is PixelFarm version ***
    //render with MiniAgg

    public class GlyphTranslatorToContour : OpenFont.IGlyphTranslator
    {
        List<GlyphContour> contours;
        GlyphContourBuilder cntBuilder;
        public GlyphTranslatorToContour()
        {

        }
        public void BeginRead(int countourCount)
        {
            //-----------------------------------
            contours = new List<GlyphContour>();
            //start with blank contour
            cntBuilder = new GlyphContourBuilder();
        }
        public void CloseContour()
        {
            cntBuilder.CloseFigure();
            GlyphContour cntContour = cntBuilder.CurrentContour;
            cntContour.allPoints = cntBuilder.GetAllPoints();
            cntBuilder.Reset();
            contours.Add(cntContour);
        }
        public void Curve3(float x1, float y1, float x2, float y2)
        {
            cntBuilder.Curve3(x1, y1, x2, y2);
        }
        public void LineTo(float x1, float y1)
        {
            cntBuilder.LineTo(x1, y1);
        }
        public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            cntBuilder.Curve4(x1, y1, x2, y2, x3, y3);
        }
        public void MoveTo(float x0, float y0)
        {
            cntBuilder.MoveTo(x0, y0);
        }
        public void EndRead()
        {
            //do nothing
        }
        public List<GlyphContour> GetContours()
        {
            return contours;
        }
    }



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
            if (curX == latestMoveToX &&
                curY == latestMoveToY)
            {
                return;
            }
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
        public List<GlyphPart> parts = new List<GlyphPart>();
        public List<float> allPoints;

        //result of analysis
        public List<GlyphPoint2D> mergedPoints;
        bool analyzed;
        bool isClockwise;

        public GlyphContour()
        {
        }
        public void AddPart(GlyphPart part)
        {
            parts.Add(part);
        }

        public bool IsClockwise
        {
            get { return this.isClockwise; }
        }
        public void Analyze(GlyphPartAnalyzer analyzer)
        {
            if (analyzed) return;
            // 
            //---------------
            //http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
            //check if hole or not
            //clockwise or counter-clockwise
            {
                //Some of the suggested methods will fail in the case of a non-convex polygon, such as a crescent. 
                //Here's a simple one that will work with non-convex polygons (it'll even work with a self-intersecting polygon like a figure-eight, telling you whether it's mostly clockwise).

                //Sum over the edges, (x2 − x1)(y2 + y1). 
                //If the result is positive the curve is clockwise,
                //if it's negative the curve is counter-clockwise. (The result is twice the enclosed area, with a +/- convention.)
                int j = allPoints.Count;
                float total = 0;
                for (int i = 3; i < j; ++i)
                {
                    float x0 = allPoints[i - 3];
                    float y0 = allPoints[i - 2];
                    float x1 = allPoints[i - 1];
                    float y1 = allPoints[i];

                    total += (x1 - x0) * (y1 + y0);
                    i += 2;
                }
                //the last one
                {
                    float x0 = allPoints[j - 2];
                    float y0 = allPoints[j - 1];
                    float x1 = allPoints[0];
                    float y1 = allPoints[1];
                    total += (x1 - x0) * (y1 + y0);
                }

                isClockwise = total >= 0;

            }

            //flatten each part ...
            //-------------------------------
            {
                int j = parts.Count;
                //---------------
                for (int i = 0; i < j; ++i)
                {
                    parts[i].Analyze(analyzer);
                }
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
        public GlyphPartAnalyzer()
        {
            this.NSteps = 20;
        }
        public int NSteps { get; set; }
        public float PixelScale { get; set; }

        public void CreateBezierVxs4(
            int nsteps,
            List<GlyphPoint2D> points,
            Vector2 start, Vector2 end,
            Vector2 control1, Vector2 control2)
        {
            var curve = new BezierCurveCubic(
                start, end,
                control1, control2);
            points.Add(new GlyphPoint2D(start.X, start.Y, PointKind.C4Start));
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                points.Add(new GlyphPoint2D(vector2.X, vector2.Y, PointKind.CurveInbetween));
                stepSum += eachstep;
            }
            points.Add(new GlyphPoint2D(end.X, end.Y, PointKind.C4End));
        }
        public void CreateBezierVxs3(
            int nsteps,
            List<GlyphPoint2D> points,
            Vector2 start, Vector2 end,
            Vector2 control1)
        {
            var curve = new BezierCurveQuadric(
                start, end,
                control1);
            points.Add(new GlyphPoint2D(start.X, start.Y, PointKind.C3Start));
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                var vector2 = curve.CalculatePoint(stepSum);
                points.Add(new GlyphPoint2D(vector2.X, vector2.Y, PointKind.CurveInbetween));
                stepSum += eachstep;
            }
            points.Add(new GlyphPoint2D(end.X, end.Y, PointKind.C3End));
        }
    }
    public abstract class GlyphPart
    {
        public abstract GlyphPartKind Kind { get; }
        public abstract void Analyze(GlyphPartAnalyzer analyzer);
        public abstract List<GlyphPoint2D> GetFlattenPoints();

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
        public GlyphPart()
        {
            //if (this.dbugId == 16)
            //{
            //}
        }
#endif
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
        public readonly double x;
        public readonly double y;
        public PointKind kind;

        //
        public Poly2Tri.TriangulationPoint triangulationPoint;
        public double adjustedX;
        public double adjustedY;
        //
        public bool isPartOfHorizontalEdge;
        public bool isUpperSide;
        public EdgeLine horizontalEdge;
        // 
        List<EdgeLine> _edges;
        public GlyphPoint2D(double x, double y, PointKind kind)
        {
            this.x = x;
            this.y = y;
            this.kind = kind;
        }
        public bool IsEqualValues(GlyphPoint2D another)
        {
            return x == another.x && y == another.y;
        }


        public void AddVerticalEdge(EdgeLine v_edge)
        {
            //associated 
            if (!this.IsPartOfVerticalEdge)
            {
                this.IsPartOfVerticalEdge = true;
            }
            if (!this.IsLeftSide)
            {
                this.IsLeftSide = v_edge.IsLeftSide;
            }

            if (_edges == null)
            {
                _edges = new List<EdgeLine>();
            }
            _edges.Add(v_edge);
        }
        public EdgeLine GetMatchingVerticalEdge()
        {
            if (_edges == null)
            {
                return null;
            }
            if (_edges.Count == 1)
            {
                return _edges[0].GetMatchingOutsideEdge();
            }
            else
            {
                return null;
            }
        }
        public bool IsLeftSide { get; private set; }
        public bool IsPartOfVerticalEdge { get; private set; }
#if DEBUG
        public override string ToString()
        {
            return x + "," + y + " " + kind.ToString();
        }
#endif

    }
    public class GlyphLine : GlyphPart
    {
        public float x0;
        public float y0;
        public float x1;
        public float y1;

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
        public float x0, y0, p2x, p2y, x, y;
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
        public float x0, y0, p2x, p2y, p3x, p3y, x, y;
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