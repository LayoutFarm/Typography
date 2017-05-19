//MIT, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
namespace Typography.Contours
{
    //This is PixelFarm's AutoFit
    //NOT FREE TYPE AUTO FIT***

    public class GlyphOutlineAnalyzer
    {
        GlyphPartFlattener _glyphFlattener = new GlyphPartFlattener();
        GlyphContourBuilder _glyphToContour = new GlyphContourBuilder();
        List<Poly2Tri.Polygon> waitingHoles = new List<Poly2Tri.Polygon>();

        public GlyphOutlineAnalyzer()
        {

        }

        /// <summary>
        /// calculate and create GlyphFitOutline
        /// </summary>
        /// <param name="glyphPoints"></param>
        /// <param name="glyphContours"></param>
        /// <returns></returns>
        public GlyphDynamicOutline CreateDynamicOutline(GlyphPointF[] glyphPoints, ushort[] glyphContours)
        {

            //1. convert original glyph point to contour
            _glyphToContour.Read(glyphPoints, glyphContours);
            //2. get result as list of contour
            List<GlyphContour> contours = _glyphToContour.GetContours();

            int cnt_count = contours.Count;
            //
            if (cnt_count > 0)
            {
                //3.before create dynamic contour we must flatten data inside the contour 
                _glyphFlattener.NSteps = 2;

                for (int i = 0; i < cnt_count; ++i)
                {
                    // (flatten each contour with the flattener)    
                    contours[i].Flatten(_glyphFlattener);
                }
                //4. after flatten, the we can create fit outline
                return CreateDynamicOutline(contours);
            }
            else
            {
                return GlyphDynamicOutline.CreateBlankDynamicOutline();
            }
        }

        /// <summary>
        /// create GlyphDynamicOutline from flatten contours
        /// </summary>
        /// <param name="flattenContours"></param>
        /// <returns></returns>
        GlyphDynamicOutline CreateDynamicOutline(List<GlyphContour> flattenContours)
        {
            //--------------------------
            //TODO: review here, add hole or not  
            // more than 1 contours, no hole => eg.  i, j, ;,  etc
            // more than 1 contours, with hole => eg.  a,e ,   etc  

            //closewise => not hole  
            waitingHoles.Clear();
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
                if (cnt.IsClosewise())
                {
                    //not a hole
                    if (mainPolygon == null)
                    {
                        //if we don't have mainPolygon before
                        //this is main polygon
                        mainPolygon = CreatePolygon(cnt.flattenPoints);

                        if (waitingHoles.Count > 0)
                        {
                            //flush all waiting holes to the main polygon
                            int j = waitingHoles.Count;
                            for (int i = 0; i < j; ++i)
                            {
                                mainPolygon.AddHole(waitingHoles[i]);
                            }
                            waitingHoles.Clear();
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
                        waitingHoles.Add(subPolygon);
                    }
                    else
                    {
                        //add to mainPolygon
                        mainPolygon.AddHole(subPolygon);
                    }
                }
            }
            if (waitingHoles.Count > 0)
            {
                throw new NotSupportedException();
            }
            //------------------------------------------
            //2. tri angulate 
            Poly2Tri.P2T.Triangulate(mainPolygon); //that poly is triangulated 

            Poly2Tri.Polygon[] subPolygons = (otherPolygons != null) ? otherPolygons.ToArray() : null;
            if (subPolygons != null)
            {
                for (int i = subPolygons.Length - 1; i >= 0; --i)
                {
                    Poly2Tri.P2T.Triangulate(subPolygons[i]);
                }
            }

            //3. intermediate outline is used inside this lib 
            //and then convert intermediate outline to dynamic outline
            return new GlyphDynamicOutline(
                new GlyphIntermediateOutline(flattenContours, mainPolygon, subPolygons));
        }


        /// <summary>
        /// create polygon from GlyphContour
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon(List<GlyphPoint> flattenPoints)
        {
            List<Poly2Tri.TriangulationPoint> points = new List<Poly2Tri.TriangulationPoint>();

            //limitation: poly tri not accept duplicated points! *** 
            double prevX = 0;
            double prevY = 0;

#if DEBUG
            //dbug check if all point is unique 
            dbugCheckAllGlyphsAreUnique(flattenPoints);
#endif


            int j = flattenPoints.Count;
            //pass
            for (int i = 0; i < j; ++i)
            {
                GlyphPoint p = flattenPoints[i];
                double x = p.OX; //start from original X***
                double y = p.OY; //start from original Y***

                if (x == prevX && y == prevY)
                {
                    if (i > 0)
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    var triPoint = new Poly2Tri.TriangulationPoint(prevX = x, prevY = y) { userData = p };
#if DEBUG
                    p.dbugTriangulationPoint = triPoint;
#endif
                    points.Add(triPoint);

                }
            }

            return new Poly2Tri.Polygon(points.ToArray());

        }
#if DEBUG
        struct dbugTmpPoint
        {
            public readonly double x;
            public readonly double y;
            public dbugTmpPoint(double x, double y)
            {
                this.x = x;
                this.y = y;
            }
            public override string ToString()
            {
                return x + "," + y;
            }
        }
        static Dictionary<dbugTmpPoint, bool> s_debugTmpPoints = new Dictionary<dbugTmpPoint, bool>();
        static void dbugCheckAllGlyphsAreUnique(List<GlyphPoint> flattenPoints)
        {
            double prevX = 0;
            double prevY = 0;
            s_debugTmpPoints = new Dictionary<dbugTmpPoint, bool>();
            int lim = flattenPoints.Count - 1;
            for (int i = 0; i < lim; ++i)
            {
                GlyphPoint p = flattenPoints[i];
                double x = p.OX; //start from original X***
                double y = p.OY; //start from original Y***

                if (x == prevX && y == prevY)
                {
                    if (i > 0)
                    {
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    dbugTmpPoint tmp_point = new dbugTmpPoint(x, y);
                    if (!s_debugTmpPoints.ContainsKey(tmp_point))
                    {
                        //ensure no duplicated point
                        s_debugTmpPoints.Add(tmp_point, true);

                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    prevX = x;
                    prevY = y;
                }
            }

        }
#endif 

    }
}
