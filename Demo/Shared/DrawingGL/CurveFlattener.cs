//MIT, 2016-present, WinterDev

using System.Collections.Generic;

namespace DrawingGL
{
    /// <summary>
    /// Represents a cubic bezier curve with two anchor and two control points.
    /// </summary>
    //[Serializable]
    struct BezierCurveCubic
    {

        /// <summary>
        /// Start anchor point.
        /// </summary>
        public Vector2 StartAnchor;
        /// <summary>
        /// End anchor point.
        /// </summary>
        public Vector2 EndAnchor;
        /// <summary>
        /// First control point, controls the direction of the curve start.
        /// </summary>
        public Vector2 FirstControlPoint;
        /// <summary>
        /// Second control point, controls the direction of the curve end.
        /// </summary>
        public Vector2 SecondControlPoint;



        /// <summary>
        /// Constructs a new <see cref="BezierCurveCubic"/>.
        /// </summary>
        /// <param name="startAnchor">The start anchor point.</param>
        /// <param name="endAnchor">The end anchor point.</param>
        /// <param name="firstControlPoint">The first control point.</param>
        /// <param name="secondControlPoint">The second control point.</param>
        public BezierCurveCubic(Vector2 startAnchor, Vector2 endAnchor, Vector2 firstControlPoint, Vector2 secondControlPoint)
        {
            this.StartAnchor = startAnchor;
            this.EndAnchor = endAnchor;
            this.FirstControlPoint = firstControlPoint;
            this.SecondControlPoint = secondControlPoint;

        }



        /// <summary>
        /// Calculates the point with the specified t.
        /// </summary>
        /// <param name="t">The t value, between 0.0f and 1.0f.</param>
        /// <returns>Resulting point.</returns>
        public Vector2 CalculatePoint(float t)
        {
            Vector2 r = new Vector2();
            float c = 1.0f - t;
            r.X = (StartAnchor.X * c * c * c) + (FirstControlPoint.X * 3 * t * c * c) + (SecondControlPoint.X * 3 * t * t * c)
                + EndAnchor.X * t * t * t;
            r.Y = (StartAnchor.Y * c * c * c) + (FirstControlPoint.Y * 3 * t * c * c) + (SecondControlPoint.Y * 3 * t * t * c)
                + EndAnchor.Y * t * t * t;
            return r;
        }


    }

    class SimpleCurveFlattener
    {

        int nsteps = 3;
        List<float> pointList = new List<float>();
        List<int> endPointList = new List<int>();

        void FlattenBezire(
          List<float> pointList,
          float x0, float y0,
          float x1, float y1,
          float x2, float y2,
          float x3, float y3)
        {
            if (nsteps > 0)
            {
                //--------------------------------
                //don't add 1st point (x0, y0)
                //because we've added it.
                //--------------------------------

                var curve = new BezierCurveCubic(
                    new Vector2(x0, y0),
                    new Vector2(x3, y3),
                    new Vector2(x1, y1),
                    new Vector2(x2, y2));

                float eachstep = (float)1 / nsteps;
                float stepSum = eachstep;//start

                int n = nsteps - 1;
                for (int i = 1; i < n; ++i)
                {
                    Vector2 vector2 = curve.CalculatePoint(stepSum);
                    pointList.Add(vector2.X); pointList.Add(vector2.Y);
                    stepSum += eachstep;
                }
            }
            pointList.Add(x3); pointList.Add(y3);
        }

        public float[] Flatten(List<PathPoint> points, out int[] endContours)
        {

            //reset
            endPointList.Clear();
            pointList.Clear();
            //----------
            int j = points.Count;
            if (j == 0) { endContours = null; return null; }
            //----------
            //first 

            float latest_x = points[0].x;
            float latest_y = points[0].y;

            for (int i = 1; i < j; ++i)
            {
                //we have point or curve4
                //no curve 3
                PathPoint p1 = points[i];
                switch (p1.kind)
                {
                    default: throw new System.NotSupportedException();
                    case PathPointKind.Point:
                        {
                            pointList.Add(latest_x = p1.x);
                            pointList.Add(latest_y = p1.y);
                        }
                        break;
                    case PathPointKind.CloseFigure:
                        {
                            //add stop mark point
                            endPointList.Add(pointList.Count - 1);
                        }
                        break;
                    case PathPointKind.CurveControl:
                        {
                            //read next curve
                            //curve4

                            PathPoint p2 = points[i + 1];
                            PathPoint p3 = points[i + 2];
                            //--------------
                            FlattenBezire(
                                pointList,
                                latest_x, latest_y,
                                p1.x, p1.y,
                                p2.x, p2.y,
                                latest_x = p3.x, latest_y = p3.y
                                );
                            //--------------
                            i += 2;
                        }
                        break;
                }
                //close 
            }
            endContours = endPointList.ToArray();
            return pointList.ToArray();
        }
    }
}