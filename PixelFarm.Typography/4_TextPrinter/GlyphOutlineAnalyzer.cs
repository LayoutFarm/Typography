//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.OpenFont;
using PixelFarm.CpuBlit.VertexProcessing;

namespace PixelFarm.Contours
{

    //This is PixelFarm's AutoFit
    //NOT FREE TYPE AUTO FIT***

    public class GlyphOutlineAnalyzer
    {
        readonly PartFlattener _partFlattener = new PartFlattener();
        readonly List<Poly2Tri.Polygon> _waitingHoles = new List<Poly2Tri.Polygon>();
        readonly ContourBuilder _contourBuilder = new ContourBuilder();

        readonly Typography.Contours.GlyphTranslatorToContourBuilder _glyphTxToContourBuilder;

        public GlyphOutlineAnalyzer()
        {
            _glyphTxToContourBuilder = new Typography.Contours.GlyphTranslatorToContourBuilder(_contourBuilder);
        }

        /// <summary>
        /// calculate and create Dynamic outline from original glyph-point
        /// </summary>
        /// <param name="glyphPoints"></param>
        /// <param name="glyphContours"></param>
        /// <returns></returns>
        public DynamicOutline CreateDynamicOutline(GlyphPointF[] glyphPoints, ushort[] glyphContours)
        {

            //1. convert original glyph point to contour
            _glyphTxToContourBuilder.Read(glyphPoints, glyphContours);

            //2. get result as list of contour
            List<Contour> contours = _contourBuilder.GetContours();

            int cnt_count = contours.Count;
            //
            if (cnt_count > 0)
            {
                //3.before create dynamic contour we must flatten data inside the contour 
                _partFlattener.NSteps = 2;

                for (int i = 0; i < cnt_count; ++i)
                {
                    // (flatten each contour with the flattener)    
                    contours[i].Flatten(_partFlattener);
                }
                //4. after flatten, the we can create fit outline
                return CreateDynamicOutline(contours);
            }
            else
            {
                return DynamicOutline.CreateBlankDynamicOutline();
            }
        }

        /// <summary>
        /// create GlyphDynamicOutline from flatten contours
        /// </summary>
        /// <param name="flattenContours"></param>
        /// <returns></returns>
        DynamicOutline CreateDynamicOutline(List<Contour> flattenContours)
        {
            using (Poly2TriTool.Borrow(out var p23tool))
            {
                List<Poly2Tri.Polygon> output = new List<Poly2Tri.Polygon>();
                p23tool.Triangulate(flattenContours, output);
                return new DynamicOutline(new IntermediateOutline(flattenContours, output));
            }             
        }


        /// <summary>
        /// create polygon from GlyphContour
        /// </summary>
        /// <param name="cnt"></param>
        /// <returns></returns>
        static Poly2Tri.Polygon CreatePolygon(List<Vertex> flattenPoints)
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
                Vertex p = flattenPoints[i];
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
        static void dbugCheckAllGlyphsAreUnique(List<Vertex> flattenPoints)
        {
            double prevX = 0;
            double prevY = 0;
            s_debugTmpPoints = new Dictionary<dbugTmpPoint, bool>();
            int lim = flattenPoints.Count - 1;
            for (int i = 0; i < lim; ++i)
            {
                Vertex p = flattenPoints[i];
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
