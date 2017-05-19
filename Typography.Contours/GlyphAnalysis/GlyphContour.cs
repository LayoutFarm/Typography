//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

using System.Numerics;

namespace Typography.Contours
{

    public class GlyphContour
    {

        public List<GlyphPart> parts = new List<GlyphPart>();
        internal List<GlyphPoint> flattenPoints; //original flatten points 
        List<OutsideEdgeLine> edges;
        bool analyzed;
        bool analyzedClockDirection;
        bool isClockwise;
        public GlyphContour()
        {
        }

        public void AddPart(GlyphPart part)
        {
            parts.Add(part);
        }


        internal void Flatten(GlyphPartFlattener flattener)
        {
            //flatten once
            if (analyzed) return;
            //flatten each part ...
            //-------------------------------
            int j = parts.Count;
            //---------------
            List<GlyphPoint> prevResult = flattener.Result;
            List<GlyphPoint> tmpFlattenPoints = flattenPoints = flattener.Result = new List<GlyphPoint>();
            //start ...
            for (int i = 0; i < j; ++i)
            {
                //flatten each part
                parts[i].Flatten(flattener);
            }

            //check duplicated the first point and last point
            int pointCount = tmpFlattenPoints.Count;
            if (GlyphPoint.SameCoordAs(tmpFlattenPoints[pointCount - 1], tmpFlattenPoints[0]))
            {
                //check if the last point is the same value as the first 
                //if yes => remove the last one
                tmpFlattenPoints.RemoveAt(pointCount - 1);
                pointCount--;
            }

            //assign number for all glyph point in this contour
            for (int i = 0; i < pointCount; ++i)
            {
                tmpFlattenPoints[i].SeqNo = i;

            }

            flattener.Result = prevResult;
            analyzed = true;
        }
        public bool IsClosewise()
        {
            //after flatten
            if (analyzedClockDirection)
            {
                return isClockwise;
            }

            List<GlyphPoint> f_points = this.flattenPoints;
            if (f_points == null)
            {
                throw new NotSupportedException();
            }
            analyzedClockDirection = true;


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
                    GlyphPoint p0 = f_points[i - 1];
                    GlyphPoint p1 = f_points[i];
                    total += (p1.OX - p0.OX) * (p1.OY + p0.OY);

                }
                //the last one
                {
                    GlyphPoint p0 = f_points[j - 1];
                    GlyphPoint p1 = f_points[0];

                    total += (p1.OX - p0.OX) * (p1.OY + p0.OY);
                }
                isClockwise = total >= 0;
            }
            return isClockwise;
        }

        internal void CreateGlyphEdges()
        {
            int lim = flattenPoints.Count - 1;
            GlyphPoint p = null, q = null;
            OutsideEdgeLine edgeLine = null;
            edges = new List<OutsideEdgeLine>();
            //
            for (int i = 0; i < lim; ++i)
            {
                //in order ...
                p = flattenPoints[i];
                q = flattenPoints[i + 1];
                if ((edgeLine = EdgeLine.FindCommonOutsideEdge(p, q)) != null)
                {
                    //from p point to q
                    //so ...
                    //edgeLine is outwardEdge for p.
                    //edgeLine is inwardEdge for q.
                    //p.OutwardEdge = q.InwardEdge = edgeLine;
                    edges.Add(edgeLine);
                }
                else
                {
                    //?
                }
            }
            //close   
            p = flattenPoints[lim];
            q = flattenPoints[0];

            if ((edgeLine = EdgeLine.FindCommonOutsideEdge(p, q)) != null)
            {
                //from p point to q
                //so ...
                //edgeLine is outwardEdge for p.
                //edgeLine is inwardEdge for q.
                //p.OutwardEdge = q.InwardEdge = edgeLine;
                edges.Add(edgeLine);
            }
            else
            {
                //not found
            }
        }

        internal void ApplyNewEdgeOffsetFromMasterOutline(float newEdgeOffsetFromMasterOutline)
        {
            int j = edges.Count;

            for (int i = 0; i < j; ++i)
            {
                edges[i].SetDynamicEdgeOffsetFromMasterOutline(newEdgeOffsetFromMasterOutline);
            }
            //calculate edge cutpoint  
            for (int i = flattenPoints.Count - 1; i >= 0; --i)
            {
                UpdateNewEdgeCut(flattenPoints[i]);
            }
        }
        /// <summary>
        /// find bounds of new fit glyph
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minY"></param>
        /// <param name="maxX"></param>
        /// <param name="maxY"></param>
        internal void FindBounds(ref float minX, ref float minY, ref float maxX, ref float maxY)
        {
            for (int i = flattenPoints.Count - 1; i >= 0; --i)
            {
                GlyphPoint p = flattenPoints[i];
                MyMath.FindMinMax(ref minX, ref maxX, p.X);
                MyMath.FindMinMax(ref minY, ref maxY, p.Y);
            }
        }

        /// <summary>
        /// update dynamic cutpoint of 2 adjacent edges
        /// </summary>
        /// <param name="p"></param>
        static void UpdateNewEdgeCut(GlyphPoint p)
        {
            OutsideEdgeLine e0 = p.E0;
            OutsideEdgeLine e1 = p.E1;

            Vector2 tmp_e0_q = e0._newDynamicMidPoint + e0.GetOriginalEdgeVector();
            Vector2 tmp_e1_p = e1._newDynamicMidPoint - e1.GetOriginalEdgeVector();

            Vector2 cutpoint;
            if (MyMath.FindCutPoint(e0._newDynamicMidPoint, tmp_e0_q, e1._newDynamicMidPoint, tmp_e1_p, out cutpoint))
            {
                p.SetNewXY(cutpoint.X, cutpoint.Y);
            }
            else
            {
                //pararell edges
            }
        }

    }

}