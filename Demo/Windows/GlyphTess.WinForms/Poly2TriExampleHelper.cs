//MIT, 2019-present, WinterDev
using System;
using System.Collections.Generic;
namespace Test_WinForm_TessGlyph
{

    static class Poly2TriExampleHelper
    {


        static List<Poly2Tri.Polygon> _waitingHoles = new List<Poly2Tri.Polygon>();
        class GlyphContour
        {
            public List<System.Drawing.PointF> flattenPoints;
            bool _analyzedClockDirection;
            bool _isClockwise;
            public bool IsClockwise()
            {
                //after flatten
                if (_analyzedClockDirection)
                {
                    return _isClockwise;
                }

                List<System.Drawing.PointF> f_points = this.flattenPoints;
                if (f_points == null)
                {
                    throw new NotSupportedException();
                }
                _analyzedClockDirection = true;


                //TODO: review here again***
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
                    int j = flattenPoints.Count;
                    double total = 0;
                    for (int i = 1; i < j; ++i)
                    {
                        System.Drawing.PointF p0 = f_points[i - 1];
                        System.Drawing.PointF p1 = f_points[i];
                        total += (p1.X - p0.X) * (p1.Y + p0.Y);

                    }
                    //the last one
                    {
                        System.Drawing.PointF p0 = f_points[j - 1];
                        System.Drawing.PointF p1 = f_points[0];

                        total += (p1.X - p0.X) * (p1.Y + p0.Y);
                    }
                    _isClockwise = total >= 0;
                }
                return _isClockwise;
            }
        }

        static List<GlyphContour> CreateGlyphContours(float[] polygon1, int[] contourEndIndices)
        {
            List<GlyphContour> contours = new List<GlyphContour>();
            int contourCount = contourEndIndices.Length;

            int index = 0;
            for (int c = 0; c < contourCount; ++c)
            {
                GlyphContour contour = new GlyphContour();
                List<System.Drawing.PointF> list = new List<System.Drawing.PointF>();
                contour.flattenPoints = list;

                int endAt = contourEndIndices[c];

                for (; index < endAt;)
                {
                    list.Add(new System.Drawing.PointF(polygon1[index], polygon1[index + 1]));
                    index += 2;
                }

                //--
                //temp hack here!
                //ensure=> duplicated points,
                //most common => first point and last point
                System.Drawing.PointF p0 = list[0];
                System.Drawing.PointF lastPoint = list[list.Count - 1];
                if (p0.X == lastPoint.X && p0.Y == lastPoint.Y)
                {
                    list.RemoveAt(list.Count - 1);
                } 
                //--
                contours.Add(contour);
            }
            return contours;
        }


        /// <summary>
        /// create polygon from GlyphContour
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon(List<System.Drawing.PointF> flattenPoints)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();

            //limitation: poly tri not accept duplicated points! *** 
            double prevX = 0;
            double prevY = 0;

            int j = flattenPoints.Count;
            //pass
            for (int i = 0; i < j; ++i)
            {
                System.Drawing.PointF p = flattenPoints[i];
                double x = p.X; //start from original X***
                double y = p.Y; //start from original Y***

                if (x == prevX && y == prevY)
                {
                    if (i > 0)
                    {
                        //skip duplicated point
                        continue;
                    }
                }
                else
                {
                    var triPoint = new Poly2Tri.TriangulationPoint(prevX = x, prevY = y) { userData = p };
                    points.Add(triPoint);
                }
            }

            return new Poly2Tri.Polygon(points.ToArray());

        }
        public static void Triangulate(float[] polygon1, int[] contourEndIndices, bool yAxisFlipped, List<Poly2Tri.Polygon> outputPolygons)
        {
            //create 
            List<GlyphContour> flattenContours = CreateGlyphContours(polygon1, contourEndIndices);
            //--------------------------
            //TODO: review here, add hole or not  
            // more than 1 contours, no hole => eg.  i, j, ;,  etc
            // more than 1 contours, with hole => eg.  a,e ,   etc  

            //clockwise => not hole  
            _waitingHoles.Clear();

            int cntCount = flattenContours.Count;
            Poly2Tri.Polygon mainPolygon = null;
            //
            //this version if it is a hole=> we add it to main polygon
            //TODO: add to more proper polygon ***
            //eg i
            //-------------------------- 
            List<Poly2Tri.Polygon> otherPolygons = null;
            for (int n = 0; n < cntCount; ++n)
            {
                GlyphContour cnt = flattenContours[n];
                bool cntIsMainPolygon = cnt.IsClockwise();
                if (yAxisFlipped)
                {
                    cntIsMainPolygon = !cntIsMainPolygon;
                }

                if (cntIsMainPolygon)
                {
                    //main polygon
                    //not a hole
                    if (mainPolygon == null)
                    {
                        //if we don't have mainPolygon before
                        //this is main polygon
                        mainPolygon = CreatePolygon(cnt.flattenPoints);

                        if (_waitingHoles.Count > 0)
                        {
                            //flush all waiting holes to the main polygon
                            int j = _waitingHoles.Count;
                            for (int i = 0; i < j; ++i)
                            {
                                mainPolygon.AddHole(_waitingHoles[i]);
                            }
                            _waitingHoles.Clear();
                        }
                    }
                    else
                    {
                        //if we already have a main polygon
                        //then this is another sub polygon
                        //IsHole is correct after we Analyze() the glyph contour
                        Poly2Tri.Polygon subPolygon = CreatePolygon(cnt.flattenPoints);
                        if (otherPolygons == null)
                        {
                            otherPolygons = new List<Poly2Tri.Polygon>();
                        }
                        otherPolygons.Add(subPolygon);
                    }
                }
                else
                {
                    //this is a hole
                    Poly2Tri.Polygon subPolygon = CreatePolygon(cnt.flattenPoints);
                    if (mainPolygon == null)
                    {
                        //add to waiting polygon
                        _waitingHoles.Add(subPolygon);
                    }
                    else
                    {
                        //add to mainPolygon
                        mainPolygon.AddHole(subPolygon);
                    }
                }
            }
            if (_waitingHoles.Count > 0)
            {
                throw new NotSupportedException();
            }
            //------------------------------------------
            //2. tri angulate 
            Poly2Tri.P2T.Triangulate(mainPolygon); //that poly is triangulated 
            outputPolygons.Add(mainPolygon);

            if (otherPolygons != null)
            {
                outputPolygons.AddRange(otherPolygons);
                for (int i = otherPolygons.Count - 1; i >= 0; --i)
                {
                    Poly2Tri.P2T.Triangulate(otherPolygons[i]);
                }
            }
        }
    }
}

