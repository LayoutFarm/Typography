//MIT, 2017, WinterDev
using System;
using System.Collections.Generic;
using PixelFarm.VectorMath;

using Poly2Tri;
namespace PixelFarm.Agg.Typography
{



    //sample/test 
    public class GlyphFitOutline
    {
        //this class store result of poly2tri

        Polygon _polygon;
        List<GlyphTriangle> _triangles = new List<GlyphTriangle>();
        public GlyphFitOutline(Polygon polygon)
        {
            this._polygon = polygon;
            foreach (DelaunayTriangle tri in polygon.Triangles)
            {
                tri.MarkAsActualTriangle();
                _triangles.Add(new GlyphTriangle(tri));
            }
        }
        public void Analyze()
        {
            //we analyze each triangle here
            int j = _triangles.Count;
            for (int i = 0; i < j; ++i)
            {
                _triangles[i].Analyze();
            }
        }


        public List<GlyphTriangle> dbugGetTriangles()
        {
            return _triangles;
        }

    }
    public enum GlyphTrianglePart : byte
    {
        Unknown,
        VericalStem,
        HorizontalStem,
        Other,
    }
    public class GlyphTriangle
    {
        GlyphTrianglePart part;
        DelaunayTriangle _tri;
        public EdgeLine e0;
        public EdgeLine e1;
        public EdgeLine e2;

        double centroidX;
        double centroidY;

        public GlyphTriangle(DelaunayTriangle tri)
        {
            this._tri = tri;
            TriangulationPoint p0 = _tri.P0;
            TriangulationPoint p1 = _tri.P1;
            TriangulationPoint p2 = _tri.P2;
            e0 = new EdgeLine(p0, p1);
            e1 = new EdgeLine(p1, p2);
            e2 = new EdgeLine(p2, p0); 
            tri.Centroid2(out centroidX, out centroidY);
             
            int edge_p01 = tri.FindEdgeIndex(tri.P0, tri.P1);
            int edge_p12 = tri.FindEdgeIndex(tri.P1, tri.P2);
            int edge_p20 = tri.FindEdgeIndex(tri.P2, tri.P0);
            e0.IsFreeSide = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P0, tri.P1));
            e1.IsFreeSide = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P1, tri.P2));
            e2.IsFreeSide = tri.EdgeIsConstrained(tri.FindEdgeIndex(tri.P2, tri.P0));
        }
        public void Analyze()
        {
            //check if triangle is part of vertical/horizontal stem or not
           
              
        }
        public double CentroidX
        {
            get { return centroidX; }
        }
        public double CentroidY
        {
            get { return centroidY; }
        }
#if DEBUG
        public override string ToString()
        {
            return this._tri.ToString();
        }
#endif
    }

    public enum LineSlopeKind : byte
    {
        Vertical,
        Horizontal,
        Other
    }

    public class EdgeLine
    {
        public double x0;
        public double y0;
        public double x1;
        public double y1;

        static readonly double _85degreeToRad = MathHelper.DegreesToRadians(85);
        static readonly double _15degreeToRad = MathHelper.DegreesToRadians(15);
        static readonly double _90degreeToRad = MathHelper.DegreesToRadians(90);

        Poly2Tri.TriangulationPoint p;
        Poly2Tri.TriangulationPoint q;
        public EdgeLine(Poly2Tri.TriangulationPoint p, Poly2Tri.TriangulationPoint q)
        {
            this.p = p;
            this.q = q;

            x0 = p.X;
            y0 = p.Y;
            x1 = q.X;
            y1 = q.Y;
            //-------------------
            if (x1 == x0)
            {
                this.SlopKind = LineSlopeKind.Vertical;
                SlopAngle = 1;
            }
            else
            {
                SlopAngle = Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
                if (SlopAngle > _85degreeToRad)
                {
                    SlopKind = LineSlopeKind.Vertical;
                }
                else if (SlopAngle < _15degreeToRad)
                {
                    SlopKind = LineSlopeKind.Horizontal;
                }
                else
                {
                    SlopKind = LineSlopeKind.Other;
                }
            }
        }
        public LineSlopeKind SlopKind
        {
            get;
            private set;
        }
        public bool IsFreeSide
        {
            get;
            internal set;
        }
        public double SlopAngle
        {
            get;
            private set;
        }
        //void Arrange()
        //{
        //    if (y1 < y0)
        //    {
        //        //swap
        //        double tmp_y = y1;
        //        y1 = y0;
        //        y0 = tmp_y;
        //        //swap x 
        //        double tmp_x = x1;
        //        x1 = x0;
        //        x0 = tmp_x;
        //    }
        //    else if (y1 == y0)
        //    {
        //        if (x1 < x0)
        //        {
        //            //swap
        //            //swap
        //            double tmp_y = y1;
        //            y1 = y0;
        //            y0 = tmp_y;
        //            //swap x 
        //            double tmp_x = x1;
        //            x1 = x0;
        //            x0 = tmp_x;
        //        }
        //    }
        //}
        public override string ToString()
        {
            return SlopKind + ":" + x0 + "," + y0 + "," + x1 + "," + y1;
        }
        //public bool SameCoordinateWidth(EdgeLine another)
        //{
        //    return this.x0 == another.x0 &&
        //        this.x1 == another.x1 &&
        //        this.y0 == another.y0 &&
        //        this.y1 == another.y1;
        //}


    }



    public class GlyphSkeleton
    {
        //reconstruction glyph ***


    }
}