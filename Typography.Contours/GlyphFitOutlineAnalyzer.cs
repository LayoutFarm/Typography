//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.Rendering
{
    //This is PixelFarm's AutoFit
    //NOT FREE TYPE AUTO FIT***
    public class GlyphFitOutlineAnalyzer
    {

        public GlyphFitOutline Analyze(GlyphPointF[] glyphPoints, ushort[] glyphContours)
        {
            var glyphToCountor = new GlyphTranslatorToContour();
            glyphToCountor.Read(glyphPoints, glyphContours);
            //master outline analysis 
            List<GlyphContour> contours = glyphToCountor.GetContours(); //analyzed contour             
            int j = contours.Count;
            var analyzer = new GlyphPartAnalyzer();
            analyzer.NSteps = 4;

            for (int i = 0; i < j; ++i)
            {
                contours[i].Analyze(analyzer);
            }

            if (j > 0)
            {
                return TessWithPolyTri(contours);
            }
            else
            {
                return null;
            }

        }



        static GlyphFitOutline TessWithPolyTri(List<GlyphContour> contours)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();
            int cntCount = contours.Count;
            GlyphContour cnt = contours[0];
            Poly2Tri.Polygon polygon = CreatePolygon2(contours[0]);//first contour            
            bool isHoleIf = !cnt.IsClockwise;
            //if (cntCount > 0)
            //{
            //    //debug only
            for (int n = 1; n < cntCount; ++n)
            {
                cnt = contours[n];
                //IsHole is correct after we Analyze() the glyph contour
                polygon.AddHole(CreatePolygon2(cnt));
                //if (cnt.IsClockwise == isHoleIf)
                //{
                //     polygon.AddHole(CreatePolygon2(cnt));
                //}
                //else
                //{
                //    //eg i
                //    //the is a complete separate dot  (i head) over i body 
                //}
            }
            //}

            //------------------------------------------
            Poly2Tri.P2T.Triangulate(polygon); //that poly is triangulated 
            GlyphFitOutline glyphFitOutline = new GlyphFitOutline(polygon, contours);
            glyphFitOutline.Analyze();
            //------------------------------------------
#if DEBUG
            List<GlyphTriangle> triAngles = glyphFitOutline.dbugGetTriangles();
            int triangleCount = triAngles.Count;

            for (int i = 0; i < triangleCount; ++i)
            {
                //---------------
                GlyphTriangle tri = triAngles[i];
                AssignPointEdgeInvolvement(tri.e0);
                AssignPointEdgeInvolvement(tri.e1);
                AssignPointEdgeInvolvement(tri.e2);
            }
#endif
            return glyphFitOutline;
        }

        struct TmpPoint
        {
            public readonly double x;
            public readonly double y;
            public TmpPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
#if DEBUG
            public override string ToString()
            {
                return x + "," + y;
            }
#endif
        }

        /// <summary>
        /// create polygon from flatten curve outline point
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon2(GlyphContour cnt)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();
            List<GlyphPart> allParts = cnt.parts;
            //---------------------------------------
            //merge all generated points
            //also remove duplicated point too! 
            List<GlyphPoint2D> mergedPoints = new List<GlyphPoint2D>();
            cnt.mergedPoints = mergedPoints;
            //---------------------------------------
            {
                int tt = 0;
                int j = allParts.Count;

                for (int i = 0; i < j; ++i)
                {
                    GlyphPart p = allParts[i];

                    List<GlyphPoint2D> fpoints = p.GetFlattenPoints();
                    if (tt == 0)
                    {
                        int n = fpoints.Count;
                        for (int m = 0; m < n; ++m)
                        {
                            //GlyphPoint2D fp = fpoints[m];
                            mergedPoints.Add(fpoints[m]);
                            //allPoints.Add((float)fp.x);
                            //allPoints.Add((float)fp.y);
                        }
                        tt++;
                    }
                    else
                    {
                        //except first point
                        int n = fpoints.Count;
                        for (int m = 1; m < n; ++m)
                        {
                            //GlyphPoint2D fp = fpoints[m];
                            mergedPoints.Add(fpoints[m]);
                            //allPoints.Add((float)fp.x);
                            //allPoints.Add((float)fp.y);
                        }
                    }

                }

            }
            //---------------------------------------
            {
                //check last (x,y) and first (x,y)
                int lim = mergedPoints.Count - 1;
                {
                    if (mergedPoints[lim].IsEqualValues(mergedPoints[0]))
                    {
                        //remove last (x,y)
                        mergedPoints.RemoveAt(lim);
                        lim -= 1;
                    }
                }

                //limitation: poly tri not accept duplicated points!
                double prevX = 0;
                double prevY = 0;
                Dictionary<TmpPoint, bool> tmpPoints = new Dictionary<TmpPoint, bool>();
                lim = mergedPoints.Count;

                for (int i = 0; i < lim; ++i)
                {
                    GlyphPoint2D p = mergedPoints[i];
                    double x = p.x;
                    double y = p.y;

                    if (x == prevX && y == prevY)
                    {
                        if (i > 0)
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        TmpPoint tmp_point = new TmpPoint(x, y);
                        if (!tmpPoints.ContainsKey(tmp_point))
                        {
                            //ensure no duplicated point
                            tmpPoints.Add(tmp_point, true);
                            var userTriangulationPoint = new Poly2Tri.TriangulationPoint(x, y) { userData = p };
                            p.triangulationPoint = userTriangulationPoint;
                            points.Add(userTriangulationPoint);
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                        prevX = x;
                        prevY = y;
                    }
                }

                Poly2Tri.Polygon polygon = new Poly2Tri.Polygon(points.ToArray());
                return polygon;
            }
        }
        static void AssignPointEdgeInvolvement(EdgeLine edge)
        {
            if (!edge.IsOutside)
            {
                return;
            }

            switch (edge.SlopKind)
            {

                case LineSlopeKind.Horizontal:
                    {
                        //horiontal edge
                        //must check if this is upper horizontal 
                        //or lower horizontal 
                        //we know after do bone analysis

                        //------------
                        //both p and q of this edge is part of horizontal edge 
                        var p = edge.p.userData as GlyphPoint2D;
                        if (p != null)
                        {
                            //TODO: review here
                            p.isPartOfHorizontalEdge = true;
                            p.isUpperSide = edge.IsUpper;
                            p.horizontalEdge = edge;
                        }

                        var q = edge.q.userData as GlyphPoint2D;
                        if (q != null)
                        {
                            //TODO: review here
                            q.isPartOfHorizontalEdge = true;
                            q.horizontalEdge = edge;
                            q.isUpperSide = edge.IsUpper;
                        }
                    }
                    break;
                case LineSlopeKind.Vertical:
                    {
                        //both p and q of this edge is part of vertical edge 
                        var p = edge.p.userData as GlyphPoint2D;
                        if (p != null)
                        {
                            //TODO: review here 
                            p.AddVerticalEdge(edge);
                        }

                        var q = edge.q.userData as GlyphPoint2D;
                        if (q != null)
                        {   //TODO: review here

                            q.AddVerticalEdge(edge);
                        }
                    }
                    break;
            }

        }
    }


}