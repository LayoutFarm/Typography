//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

namespace Typography.Rendering
{
    //This is PixelFarm's AutoFit
    //NOT FREE TYPE AUTO FIT***
    public class GlyphAutoFit
    {
        //----------
        //implement glyph auto fit hint
        //----------

        public void Hint(GlyphContourReader contourReader, float pxScale)
        {
            //master outline analysis

            List<GlyphContour> contours = contourReader.GetContours();
            int j = contours.Count;
            var analyzer = new GlyphPartAnalyzer();
            analyzer.NSteps = 4;
            analyzer.PixelScale = pxScale;
            for (int i = 0; i < j; ++i)
            {
                contours[i].Analyze(analyzer);
            }
            //-----------------------------------------------------------            
            //create fit contour
            //this version use only Agg's vertical hint only ****
            //(ONLY vertical fitting , NOT apply horizontal fit)
            //-----------------------------------------------------------         
            GlyphFitOutline glyphOutline = glyphOutline = TessWithPolyTri(contours, pxScale);
            {
                PixelFarm.Agg.VertexStore vxs2 = new PixelFarm.Agg.VertexStore();
                for (int i = 0; i < j; ++i)
                {
                    CreateFitContourVxs(vxs2, contours[i], pxScale, false, true);
                }
            }
        }

        const int GRID_SIZE = 1;
        const float GRID_SIZE_25 = 1f / 4f;
        const float GRID_SIZE_50 = 2f / 4f;
        const float GRID_SIZE_75 = 3f / 4f;

        const float GRID_SIZE_33 = 1f / 3f;
        const float GRID_SIZE_66 = 2f / 3f;

        static float RoundToNearestVerticalSide(float org)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);
            float floatModulo = actual1 - integer1;

            if (floatModulo >= (GRID_SIZE_50))
            {
                return (integer1 + 1);
            }
            else
            {
                return integer1;
            }
        }
        static float RoundToNearestHorizontalSide(float org)
        {
            float actual1 = org;
            float integer1 = (int)(actual1);//lower
            float floatModulo = actual1 - integer1;

            if (floatModulo >= (GRID_SIZE_50))
            {
                return (integer1 + 1);
            }
            else
            {
                return integer1;
            }
        }
        static void CreateFitContourVxs(PixelFarm.Agg.VertexStore vxs, GlyphContour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            List<GlyphPoint2D> mergePoints = contour.mergedPoints;
            int j = mergePoints.Count;
            //merge 0 = start
            double prev_px = 0;
            double prev_py = 0;
            double p_x = 0;
            double p_y = 0;
            double first_px = 0;
            double first_py = 0;

            {
                GlyphPoint2D p = mergePoints[0];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3)
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    //adjust right-side vertical edge
                    EdgeLine rightside = p.GetMatchingVerticalEdge();
                    if (rightside != null)
                    {

                    }
                    p_x = new_x;
                }
                vxs.AddMoveTo(p_x, p_y);
                //-------------
                first_px = prev_px = p_x;
                first_py = prev_py = p_y;
            }

            for (int i = 1; i < j; ++i)
            {
                //all merge point is polygon point
                GlyphPoint2D p = mergePoints[i];
                p_x = p.x * pixelScale;
                p_y = p.y * pixelScale;

                if (y_axis && p.isPartOfHorizontalEdge && p.isUpperSide && p_y > 3)
                {
                    //vertical fitting
                    //fit p_y to grid
                    p_y = RoundToNearestVerticalSide((float)p_y);
                }

                if (x_axis && p.IsPartOfVerticalEdge && p.IsLeftSide)
                {
                    //horizontal fitting
                    //fix p_x to grid
                    float new_x = RoundToNearestHorizontalSide((float)p_x);
                    ////adjust right-side vertical edge
                    //PixelFarm.Agg.Typography.EdgeLine rightside = p.GetMatchingVerticalEdge();
                    //if (rightside != null && !rightside.IsLeftSide && rightside.IsOutside)
                    //{
                    //    var rightSideP = rightside.p.userData as GlyphPoint2D;
                    //    var rightSideQ = rightside.q.userData as GlyphPoint2D;
                    //    //find move diff
                    //    float movediff = (float)p_x - new_x;
                    //    //adjust right side edge
                    //    rightSideP.x = rightSideP.x + movediff;
                    //    rightSideQ.x = rightSideQ.x + movediff;
                    //}
                    p_x = new_x;
                }
                //
                vxs.AddLineTo(p_x, p_y);
                //
                prev_px = p_x;
                prev_py = p_y;

            }
            vxs.AddLineTo(first_px, first_py);

        }
        static GlyphContour CreateFitContourVxs2(GlyphContour contour, float pixelScale, bool x_axis, bool y_axis)
        {
            GlyphContour newc = new GlyphContour();
            List<GlyphPart> parts = contour.parts;
            int m = parts.Count;
            for (int n = 0; n < m; ++n)
            {
                GlyphPart p = parts[n];
                switch (p.Kind)
                {
                    default: throw new NotSupportedException();
                    case GlyphPartKind.Curve3:
                        {
                            GlyphCurve3 curve3 = (GlyphCurve3)p;
                            newc.AddPart(new GlyphCurve3(
                                curve3.x0 * pixelScale, curve3.y0 * pixelScale,
                                curve3.p2x * pixelScale, curve3.p2y * pixelScale,
                                curve3.x * pixelScale, curve3.y * pixelScale));

                        }
                        break;
                    case GlyphPartKind.Curve4:
                        {
                            GlyphCurve4 curve4 = (GlyphCurve4)p;
                            newc.AddPart(new GlyphCurve4(
                                  curve4.x0 * pixelScale, curve4.y0 * pixelScale,
                                  curve4.p2x * pixelScale, curve4.p2y * pixelScale,
                                  curve4.p3x * pixelScale, curve4.p3y * pixelScale,
                                  curve4.x * pixelScale, curve4.y * pixelScale
                                ));
                        }
                        break;
                    case GlyphPartKind.Line:
                        {
                            GlyphLine line = (GlyphLine)p;
                            newc.AddPart(new GlyphLine(
                                line.x0 * pixelScale, line.y0 * pixelScale,
                                line.x1 * pixelScale, line.y1 * pixelScale
                                ));
                        }
                        break;
                }
            }


            return newc;
        }

        GlyphFitOutline TessWithPolyTri(List<GlyphContour> contours, float pixelScale)
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
            GlyphFitOutline glyphFitOutline = new GlyphFitOutline(polygon);
            glyphFitOutline.Analyze();
            //------------------------------------------

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
                        throw new NotSupportedException();
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