//MIT, 2017, WinterDev
using System;
namespace Typography.Contours
{
    /// <summary>
    /// a link (line) that connects between centroid of 2 GlyphTriangle(p => q)
    /// </summary>
    struct GlyphCentroidPair
    {
        //this is a temporary object.
        //we crate glyph
        //1 centroid pair has 1 GlyphBoneJoint

        internal readonly GlyphTriangle p, q;

        internal GlyphCentroidPair(GlyphTriangle p, GlyphTriangle q)
        {

            //each triangle has 1 centroid point
            //a centrod line connects between 2 adjacent triangles via centroid 

            //p triangle=> (x0,y0)  (centroid of p)
            //q triangle=> (x1,y1)  (centroid of q)
            //a centroid line  move from p to q  
            this.p = p;
            this.q = q;
        }
        /// <summary>
        /// add information about edges to each triangle and create BoneJoint and Tip
        /// </summary>
        public GlyphBoneJoint AnalyzeEdgesAndCreateBoneJoint()
        {

#if DEBUG
            if (p == q)
            {
                throw new NotSupportedException();
            }
#endif

            //2 contact triangles share GlyphBoneJoint.
            //-------------------------------------- 
            //[C]
            //Find relation between edges of 2 triangle p and q
            //....
            //pick up a edge of p and compare to all edge of q
            //do until complete
            GlyphBoneJoint boneJoint = null;
            InsideEdgeLine p_edge, q_edge;
            if (FindCommonInsideEdges(p, q, out p_edge, out q_edge))
            {
                //create joint 
                boneJoint = new GlyphBoneJoint(p_edge, q_edge);
                double slopeAngle = CalculateCentroidPairSlopeNoDirection(this);
                //
                EdgeLine foundTipEdge = null;
                if ((foundTipEdge = CreateTipEdgeIfNeed(slopeAngle, p, p_edge)) != null)
                {
                    //P
                    boneJoint.SetTipEdge_P(foundTipEdge);
                }
                if ((foundTipEdge = CreateTipEdgeIfNeed(slopeAngle, q, q_edge)) != null)
                {
                    //Q
                    boneJoint.SetTipEdge_Q(foundTipEdge);
                }
            }
            return boneJoint;
        }

        static bool FindCommonInsideEdges(GlyphTriangle a, GlyphTriangle b, out InsideEdgeLine a_edge, out InsideEdgeLine b_edge)
        {
            //2 contact triangles share GlyphBoneJoint.          


            EdgeLine find_b_edge = b.e0;
            InsideEdgeLine matching_inside_edge_of_a = null;
            if ((matching_inside_edge_of_a = FindCommonInsideEdge(a, find_b_edge)) != null)
            {
                //found
                a_edge = matching_inside_edge_of_a;
                b_edge = (InsideEdgeLine)find_b_edge;
                return true;
            }
            //--------------
            find_b_edge = b.e1;
            if ((matching_inside_edge_of_a = FindCommonInsideEdge(a, find_b_edge)) != null)
            {
                //found
                a_edge = matching_inside_edge_of_a;
                b_edge = (InsideEdgeLine)find_b_edge;
                return true;
            }
            find_b_edge = b.e2;
            if ((matching_inside_edge_of_a = FindCommonInsideEdge(a, find_b_edge)) != null)
            {
                //found
                a_edge = matching_inside_edge_of_a;
                b_edge = (InsideEdgeLine)find_b_edge;
                return true;
            }
            a_edge = b_edge = null;
            return false;
        }
        static InsideEdgeLine FindCommonInsideEdge(GlyphTriangle a, EdgeLine b_edge)
        {
            //2 contact triangles share GlyphBoneJoint.            
            //compare 3 side of a's edge to b_edge
            if (b_edge.IsOutside) return null;
            //
            if (IsMatchingEdge(a.e0, b_edge)) return (InsideEdgeLine)a.e0;
            if (IsMatchingEdge(a.e1, b_edge)) return (InsideEdgeLine)a.e1;
            if (IsMatchingEdge(a.e2, b_edge)) return (InsideEdgeLine)a.e2;
            return null;
        }


        static void ClassifyTriangleEdges(
            GlyphTriangle triangle,
            EdgeLine knownInsideEdge,
            out EdgeLine anotherInsideEdge,
            out EdgeLine outside0,
            out EdgeLine outside1,
            out EdgeLine outside2,
            out int outsideCount)
        {
            outsideCount = 0;
            outside0 = outside1 = outside2 = anotherInsideEdge = null;

            if (triangle.e0.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e0; break;
                    case 1: outside1 = triangle.e0; break;
                    case 2: outside2 = triangle.e0; break;
                }
                outsideCount++;
            }
            else
            {
                //e0 is not known inside edge
                if (triangle.e0 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e0;
                }
            }
            //
            if (triangle.e1.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e1; break;
                    case 1: outside1 = triangle.e1; break;
                    case 2: outside2 = triangle.e1; break;
                }
                outsideCount++;
            }
            else
            {
                if (triangle.e1 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e1;
                }
            }
            //
            if (triangle.e2.IsOutside)
            {
                switch (outsideCount)
                {
                    case 0: outside0 = triangle.e2; break;
                    case 1: outside1 = triangle.e2; break;
                    case 2: outside2 = triangle.e2; break;
                }
                outsideCount++;
            }
            else
            {
                if (triangle.e2 != knownInsideEdge)
                {
                    anotherInsideEdge = triangle.e2;
                }
            }
        }

        static double CalculateCentroidPairSlopeNoDirection(GlyphCentroidPair centroidPair)
        {
            //calculate centroid pair slope 
            //p
            float x0, y0;
            centroidPair.p.CalculateCentroid(out x0, out y0);

            //q
            float x1, y1;
            centroidPair.q.CalculateCentroid(out x1, out y1);

            //return slop angle no direction,we don't care direction of vector  
            return Math.Abs(Math.Atan2(Math.Abs(y1 - y0), Math.Abs(x1 - x0)));
        }

        static void SelectMostProperTipEdge(
          double slopeAngle,
          EdgeLine outside0,
          EdgeLine outside1,
          out EdgeLine tipEdge,
          out EdgeLine notTipEdge)
        {

            //slop angle in rad

            double diff0 = Math.Abs(outside0.GetSlopeAngleNoDirection() - slopeAngle);
            double diff1 = Math.Abs(outside1.GetSlopeAngleNoDirection() - slopeAngle);
            if (diff0 > diff1)
            {
                tipEdge = outside0;
                notTipEdge = outside1;
            }
            else
            {
                tipEdge = outside1;
                notTipEdge = outside0;
            }

        }


        /// <summary>
        /// add information about each edge of a triangle, compare to the contactEdge of a ownerEdgeJoint
        /// </summary>
        /// <param name="triangle"></param>
        /// <param name="boneJoint"></param>
        /// <param name="knownInsideEdge"></param>
        static EdgeLine CreateTipEdgeIfNeed(
          double cent_slopAngle,
          GlyphTriangle triangle,
          EdgeLine knownInsideEdge)
        {

            int outsideCount;
            EdgeLine outside0, outside1, outside2, anotherInsideEdge;
            ClassifyTriangleEdges(
                triangle,
                knownInsideEdge,
                out anotherInsideEdge,
                out outside0,
                out outside1,
                out outside2,
                out outsideCount);

            switch (outsideCount)
            {
                default: throw new NotSupportedException();
                case 0:
                case 1: break;
                case 3: throw new NotImplementedException();//TODO: implement this  
                case 2:

                    //tip end 
                    //find which edge should be 'tip edge'                         
                    //in this version we compare each edge slope to centroid line slope.
                    //the most diff angle should be opposite edge (to the centroid) => tip edge
                    //------------------------------------------------------------------------- 
                    EdgeLine tipEdge, notTipEdge;
                    SelectMostProperTipEdge(cent_slopAngle,
                        outside0,
                        outside1,
                        out tipEdge,
                        out notTipEdge);
                    return tipEdge;

            }
            return null;
        }
         
 
        /// <summary>
        /// check if the 2 triangle is matching or not
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static bool IsMatchingEdge(EdgeLine a, EdgeLine b)
        {
            //x-axis
            if ((a.PX == b.PX && a.QX == b.QX) ||
                (a.PX == b.QX && a.QX == b.PX))
            {
                //pass x-axis
                //
                //y_axis
                if ((a.PY == b.PY && a.QY == b.QY) ||
                    (a.PY == b.QY && a.QY == b.PY))
                {
                    return true;
                }
            }
            //otherwise...
            return false;
        }

        public override string ToString()
        {
            return p + " -> " + q;
        }
    }
}