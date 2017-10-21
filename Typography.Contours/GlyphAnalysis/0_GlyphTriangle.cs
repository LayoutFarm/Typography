//MIT, 2017, WinterDev
using System;
using Poly2Tri;
namespace Typography.Contours
{

    class GlyphTriangle
    {

        DelaunayTriangle _tri;
        public readonly EdgeLine e0;
        public readonly EdgeLine e1;
        public readonly EdgeLine e2;


        public GlyphTriangle(DelaunayTriangle tri)
        {
            this._tri = tri;
            //---------------------------------------------
            TriangulationPoint p0 = _tri.P0;
            TriangulationPoint p1 = _tri.P1;
            TriangulationPoint p2 = _tri.P2;

            //we do not store triangulation points (p0,p1,02)
            //an EdgeLine is created after we create GlyphTriangles.

            //triangulate point p0->p1->p2 is CCW ***             
            e0 = NewEdgeLine(p0, p1, tri.EdgeIsConstrained(tri.FindEdgeIndex(p0, p1)));
            e1 = NewEdgeLine(p1, p2, tri.EdgeIsConstrained(tri.FindEdgeIndex(p1, p2)));
            e2 = NewEdgeLine(p2, p0, tri.EdgeIsConstrained(tri.FindEdgeIndex(p2, p0)));

            //if the order of original glyph point is CW
            //we may want to reverse the order of edge creation :
            //p2->p1->p0 

            //link back 
            tri.userData = this;
            //----------------

            //early analyze
            AnalyzeInsideEdge(e0, e1, e2);
            AnalyzeInsideEdge(e1, e0, e2);
            AnalyzeInsideEdge(e2, e0, e1);
            //at this point, 
            //we should know the direction of this triangle
            //then we known that if this triangle is left/right/upper/lower of the 'stroke' line 

            float cent_x, cent_y;
            this.CalculateCentroid(out cent_x, out cent_y);
            AnalyzeOutsideEdge(e0, cent_x, cent_y);
            AnalyzeOutsideEdge(e1, cent_x, cent_y);
            AnalyzeOutsideEdge(e2, cent_x, cent_y);
        }
        void AnalyzeOutsideEdge(EdgeLine d, float centroidX, float centroidY)
        {
            //check if edge slope
            if (!d.IsOutside) return;
            //---------------------------
            switch (d.SlopeKind)
            {
                case LineSlopeKind.Horizontal:

                    //check if upper or lower
                    //compare mid point with the centroid  
                    d.IsUpper = d.GetMidPoint().Y > centroidY;
                    break;
                case LineSlopeKind.Vertical:
                    d.IsLeftSide = d.GetMidPoint().X < centroidX;
                    break;
            }

        }
        void AnalyzeInsideEdge(EdgeLine d0, EdgeLine d1, EdgeLine d2)
        {
            if (d0._earlyInsideAnalysis) return;
            if (!d0.IsInside) return;
            //-------------------------------------------------
            //maybeInsideEdge is Inside ***
            //check another
            if (d1.IsInside)
            {
                if (d2.IsInside)
                {
                    //3 inside edges 
                }
                else
                {
                    //1 outside edge (d2) 
                    //2 inside edges (d0,d1)
                    //find a perpendicular line
                    FindPerpendicular(d2, d0);
                    FindPerpendicular(d2, d1);
                }
            }
            else if (d2.IsInside)
            {
                if (d1.IsInside)
                {
                    //3 inside edges

                }
                else
                {
                    //1 outside edge (d1)
                    //2 inside edges (d0,d2)
                    FindPerpendicular(d1, d0);
                    FindPerpendicular(d1, d2);

                }
            }
        }
        static void FindPerpendicular(EdgeLine outsideEdge, EdgeLine inside)
        {
            System.Numerics.Vector2 m0 = inside.GetMidPoint();
            System.Numerics.Vector2 cut_fromM0;
            if (MyMath.FindPerpendicularCutPoint(outsideEdge, new System.Numerics.Vector2(m0.X, m0.Y), out cut_fromM0))
            {
                ((OutsideEdgeLine)outsideEdge).SetControlEdge(inside);
            }
            else
            {

            }
            outsideEdge._earlyInsideAnalysis = inside._earlyInsideAnalysis = true;

        }
        EdgeLine NewEdgeLine(TriangulationPoint p, TriangulationPoint q, bool isOutside)
        {
            return isOutside ?
                (EdgeLine)(new OutsideEdgeLine(this, p.userData as GlyphPoint, q.userData as GlyphPoint)) :
                new InsideEdgeLine(this, p.userData as GlyphPoint, q.userData as GlyphPoint);
        }

        public void CalculateCentroid(out float centroidX, out float centroidY)
        {
            _tri.GetCentroid(out centroidX, out centroidY);
        }
        public bool IsConnectedTo(GlyphTriangle anotherTri)
        {
            DelaunayTriangle t2 = anotherTri._tri;
            if (t2 == this._tri)
            {
                throw new NotSupportedException();
            }
            //compare each neighbor 
            return this._tri.N0 == t2 ||
                   this._tri.N1 == t2 ||
                   this._tri.N2 == t2;
        }
        /// <summary>
        /// neighbor triangle 0
        /// </summary>
        public GlyphTriangle N0
        {
            get
            {
                return GetGlyphTriFromUserData(_tri.N0);
            }
        }
        /// <summary>
        /// neighbor triangle 1
        /// </summary>
        public GlyphTriangle N1
        {
            get
            {
                return GetGlyphTriFromUserData(_tri.N1);
            }
        }
        /// <summary>
        /// neighbor triangle 2
        /// </summary>
        public GlyphTriangle N2
        {
            get
            {
                return GetGlyphTriFromUserData(_tri.N2);
            }
        }
        static GlyphTriangle GetGlyphTriFromUserData(DelaunayTriangle tri)
        {
            if (tri == null) return null;
            return tri.userData as GlyphTriangle;
        }

    }

}