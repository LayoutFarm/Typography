//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace PixelFarm.Contours
{


    public class PartFlattener
    {
        /// <summary>
        /// result flatten points
        /// </summary>
        List<Vertex> _points;

        public PartFlattener()
        {
            this.NSteps = 2;//default
        }
        public List<Vertex> Result
        {
            get => _points;
            set => _points = value;
        }
        public int NSteps { get; set; }


        void AddPoint(float x, float y, VertexKind kind)
        {
            var p = new Vertex(x, y, kind);
#if DEBUG
            p.dbugOwnerPart = dbug_ownerPart;
#endif
            _points.Add(p);
        }

        public void GeneratePointsFromLine(Vector2f start, Vector2f end)
        {
            if (_points.Count == 0)
            {
                AddPoint(start.X, start.Y, VertexKind.LineStart);
            }
            AddPoint(end.X, end.Y, VertexKind.LineStop);
        }

        public void GeneratePointsFromCurve4(
            int nsteps,
            Vector2f start, Vector2f end,
            Vector2f control1, Vector2f control2)
        {
            var curve = new BezierCurveCubic( //Cubic curve -> curve4
                start, end,
                control1, control2);
            if (_points.Count == 0)
            {
                AddPoint(start.X, start.Y, VertexKind.C4Start);
            }
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                //start at i=1, this will not include the last step that stepSum=1
                Vector2f vector2 = curve.CalculatePoint(stepSum);
                AddPoint(vector2.X, vector2.Y, VertexKind.CurveInbetween);
                stepSum += eachstep;
            }

            AddPoint(end.X, end.Y, VertexKind.C4End);
        }
        public void GeneratePointsFromCurve3(
            int nsteps,
            Vector2f start, Vector2f end,
            Vector2f control1)
        {
            var curve = new BezierCurveQuadric( //Quadric curve-> curve3
                start, end,
                control1);
            if (_points.Count == 0)
            {
                AddPoint(start.X, start.Y, VertexKind.C3Start);
            }
            float eachstep = (float)1 / nsteps;
            float stepSum = eachstep;//start
            for (int i = 1; i < nsteps; ++i)
            {
                //start at i=1, this will not include the last step that stepSum=1
                Vector2f vector2 = curve.CalculatePoint(stepSum);
                AddPoint(vector2.X, vector2.Y, VertexKind.CurveInbetween);
                stepSum += eachstep;
            }
            AddPoint(end.X, end.Y, VertexKind.C3End);
        }

#if DEBUG
        ContourPart dbug_ownerPart;
        public void dbugSetCurrentOwnerPart(ContourPart dbug_ownerPart)
        {
            this.dbug_ownerPart = dbug_ownerPart;
        }
#endif
    }

}