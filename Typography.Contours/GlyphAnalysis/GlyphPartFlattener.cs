//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Typography.Contours
{


    class GlyphPartFlattener
    {
        /// <summary>
        /// result flatten points
        /// </summary>
        List<GlyphPoint> points;
        
        public GlyphPartFlattener()
        {
            this.NSteps = 2;//default
        }
        public List<GlyphPoint> Result
        {
            get { return points; }
            set { points = value; }
        }
        public int NSteps { get; set; }
        
        
        void AddPoint(float x, float y, PointKind kind)
        {
            var p = new GlyphPoint(x, y, kind);
#if DEBUG
            p.dbugOwnerPart = dbug_ownerPart;
#endif
            points.Add(p);
        }

        public void GeneratePointsFromLine(Vector2 start, Vector2 end)
        {
            if (points.Count == 0)
            {
                AddPoint(start.X, start.Y, PointKind.LineStart);
            }
            AddPoint(end.X, end.Y, PointKind.LineStop);
        }

        public void GeneratePointsFromCurve4(
            int nsteps,
            Vector2 start, Vector2 end,
            Vector2 control1, Vector2 control2)
        {
            var curve = new BezierCurveCubic( //Cubic curve -> curve4
                start, end,
                control1, control2);
            if (points.Count == 0)
            {
                AddPoint(start.X, start.Y, PointKind.C4Start);
            }
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                //start at i=1, this will not include the last step that stepSum=1
                Vector2 vector2 = curve.CalculatePoint(stepSum);
                AddPoint(vector2.X, vector2.Y, PointKind.CurveInbetween);
                stepSum += eachstep;
            }

            AddPoint(end.X, end.Y, PointKind.C4End);
        }
        public void GeneratePointsFromCurve3(
            int nsteps,
            Vector2 start, Vector2 end,
            Vector2 control1)
        {
            var curve = new BezierCurveQuadric( //Quadric curve-> curve3
                start, end,
                control1);
            if (points.Count == 0)
            {
                AddPoint(start.X, start.Y, PointKind.C3Start);
            }
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                //start at i=1, this will not include the last step that stepSum=1
                Vector2 vector2 = curve.CalculatePoint(stepSum);
                AddPoint(vector2.X, vector2.Y, PointKind.CurveInbetween);
                stepSum += eachstep;
            }
            AddPoint(end.X, end.Y, PointKind.C3End);
        } 

#if DEBUG
        GlyphPart dbug_ownerPart;
        public void dbugSetCurrentOwnerPart(GlyphPart dbug_ownerPart)
        {
            this.dbug_ownerPart = dbug_ownerPart;
        }
#endif
    }

}