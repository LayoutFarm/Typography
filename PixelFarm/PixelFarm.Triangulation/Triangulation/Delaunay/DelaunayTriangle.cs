//BSD, 2014-2017, WinterDev

/* Poly2Tri
 * Copyright (c) 2009-2010, Poly2Tri Contributors
 * http://code.google.com/p/poly2tri/
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * * Redistributions of source code must retain the above copyright notice,
 *   this list of conditions and the following disclaimer.
 * * Redistributions in binary form must reproduce the above copyright notice,
 *   this list of conditions and the following disclaimer in the documentation
 *   and/or other materials provided with the distribution.
 * * Neither the name of Poly2Tri nor the names of its contributors may be
 *   used to endorse or promote products derived from this software without specific
 *   prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

/// Changes from the Java version
///   attributification
/// Future possibilities
///   Flattening out the number of indirections 
///     Bundling everything into an AoS mess?



using System;
using System.Diagnostics;
using System.Collections.Generic;
namespace Poly2Tri
{
    public sealed class DelaunayTriangle
    {
        public TriangulationPoint P0, P1, P2;
        /// <summary>
        /// neighbors 
        /// </summary>
        public DelaunayTriangle N0, N1, N2;
        //edge Delaunay  mark
        internal bool D0, D1, D2;
        //edge Constraint mark
        public bool C0, C1, C2;
        //lower 4 bits for EdgeIsDelaunay
        //next 4 bits for EdgeIsConstrained
        //int edgedNoteFlags;
        public bool isActualTriangle;

        /// <summary>
        /// store general purpose data 
        /// </summary>
        public object userData;
#if DEBUG

        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
#endif

        public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
        {
            this.P0 = p1;
            this.P1 = p2;
            this.P2 = p3;
        }
        public void MarkAsActualTriangle()
        {
            isActualTriangle = true;
        }

        public bool IsInterior { get; set; }
        public int IndexOf(TriangulationPoint p)
        {
            //if (TriangulationPoint.IsEqualPointCoord(P0, p)) return 0;
            //if (TriangulationPoint.IsEqualPointCoord(P1, p)) return 1;
            //if (TriangulationPoint.IsEqualPointCoord(P2, p)) return 2;

            if (P0 == p) return 0;
            if (P1 == p) return 1;
            if (P2 == p) return 2;
            throw new Exception("Calling index with a point that doesn't exist in triangle");
            return -1;
            //return -1;
            //int i = Points.IndexOf(p);
            //if (i == -1) throw new Exception("Calling index with a point that doesn't exist in triangle");
            //return i;
        }
        //internal int FindIndexOf(TriangulationPoint p)
        //{
        //    if (P0 == p) return 0;
        //    if (P1 == p) return 1;
        //    if (P2 == p) return 2;
        //    //if (TriangulationPoint.IsEqualPointCoord(P0, p)) return 0;
        //    //if (TriangulationPoint.IsEqualPointCoord(P1, p)) return 1;
        //    //if (TriangulationPoint.IsEqualPointCoord(P2, p)) return 2;

        //    return -1;
        //    //return -1;
        //    //int i = Points.IndexOf(p);
        //    //if (i == -1) throw new Exception("Calling index with a point that doesn't exist in triangle");
        //    //return i;
        //}

        public bool ContainsPoint(TriangulationPoint p)
        {
            return (P0 == p) ||
                    (P1 == p) ||
                    (P2 == p);

            //if (TriangulationPoint.IsEqualPointCoord(P0, p)) return true;
            //if (TriangulationPoint.IsEqualPointCoord(P1, p)) return true;
            //if (TriangulationPoint.IsEqualPointCoord(P2, p)) return true;
            //return false;
        }
        public bool EdgeIsDelaunay(int index)
        {
            //lower 4 bits for EdgeIsDelaunay
            //return ((edgedNoteFlags >> (int)index) & 0x7) != 0;
            switch (index)
            {
                case 0:
                    return this.D0;
                case 1:
                    return this.D1;
                default:
                    return this.D2;
            }
        }
        public void MarkEdgeDelunay(int index, bool value)
        {
            //clear old flags 
            //and then flags new value
            switch (index)
            {
                case 0:
                    this.D0 = value;
                    break;
                case 1:
                    this.D1 = value;
                    break;
                default:
                    this.D2 = value;
                    break;
            }
            //if (value)
            //{
            //    edgedNoteFlags |= (1 << index);
            //}
            //else
            //{
            //    edgedNoteFlags &= ~(1 << index);
            //}
        }

        public bool EdgeIsConstrained(int index)
        {
            switch (index)
            {
                case 0:
                    return this.C0;
                case 1:
                    return this.C1;
                default:
                    return this.C2;
            }
        }
        public void MarkEdgeConstraint(int index, bool value)
        {
            //clear old flags 
            //and then flags new value
            switch (index)
            {
                case 0:
                    this.C0 = value;
                    break;
                case 1:
                    this.C1 = value;
                    break;
                case 2:
                    this.C2 = value;
                    break;
                default:
                    //may be -1
                    break;
            }
        }
        public void ClearAllEdgeDelaunayMarks()
        {
            this.D0 = this.D1 = this.D2 = false;
            //this.edgedNoteFlags &= ~0x7;
        }
        public int IndexCWFrom(TriangulationPoint p)
        {
            return (IndexOf(p) + 2) % 3;
        }
        public int IndexCCWFrom(TriangulationPoint p)
        {
            return (IndexOf(p) + 1) % 3;
        }

        public bool Contains(TriangulationPoint p)
        {
            return ContainsPoint(p);
        }

        /// <summary>
        /// Update neighbor pointers
        /// </summary>
        /// <param name="p1">Point 1 of the shared edge</param>
        /// <param name="p2">Point 2 of the shared edge</param>
        /// <param name="t">This triangle's new neighbor</param>
        private void MarkNeighbor(TriangulationPoint p1, TriangulationPoint p2, DelaunayTriangle t)
        {
            //int i =;
            //if (i == -1) throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!"); 
            switch (FindEdgeIndex(p1, p2))
            {
                case 0:
                    {
                        this.N0 = t;
                    }
                    break;
                case 1:
                    {
                        this.N1 = t;
                    }
                    break;
                case 2:
                    {
                        this.N2 = t;
                    }
                    break;
                default:
                    {   //may be -1 
                        throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!");
                    }
            }
            //Neighbors[i] = t;
        }
        private void MarkNeighbor(int i_p1, int i_p2, DelaunayTriangle t)
        {
            //int i =;
            //if (i == -1) throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!"); 
            switch (FindEdgeIndex(i_p1, i_p2))
            {
                case 0:
                    {
                        this.N0 = t;
                    }
                    break;
                case 1:
                    {
                        this.N1 = t;
                    }
                    break;
                case 2:
                    {
                        this.N2 = t;
                    }
                    break;
                default:
                    {   //may be -1 
                        throw new Exception("Error marking neighbors -- t doesn't contain edge p1-p2!");
                    }
            }
            //Neighbors[i] = t;
        }

        public void ClearAllNBs()
        {
            N0 = N1 = N2 = null;
        }

        /// <summary>
        /// Exhaustive search to update neighbor pointers
        /// </summary>
        public void MarkNeighbor(DelaunayTriangle t)
        {
            // Points of this triangle also belonging to t 
            //-------------------------
            //use temp name technique 2 ***
            //1. clear points of this
            P0.tempName = P1.tempName = P2.tempName = 3;
            //2. assign tempName for t
            t.P0.tempName = 0; t.P1.tempName = 1; t.P2.tempName = 2;
            bool a = P0.tempName != 3;
            bool b = P1.tempName != 3;
            bool c = P2.tempName != 3;
            //P1.tempName 
            if (b && c) { N0 = t; t.MarkNeighbor(P1.tempName, P2.tempName, this); }
            else if (a && c) { N1 = t; t.MarkNeighbor(P0.tempName, P2.tempName, this); }
            else if (a && b) { N2 = t; t.MarkNeighbor(P0.tempName, P1.tempName, this); }
            else throw new Exception("Failed to mark neighbor, doesn't share an edge!");
        }

        /// <param name="t">Opposite triangle</param>
        /// <param name="p">The point in t that isn't shared between the triangles</param>
        public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
        {
            Debug.Assert(t != this, "self-pointer error");
            return PointCWFrom(t.PointCWFrom(p));
        }
        public TriangulationPoint OppositePoint(DelaunayTriangle t,
            TriangulationPoint p, int iPonT,
            out int foundAtIndex,
            out bool related_ec, out bool related_ed)
        {
            Debug.Assert(t != this, "self-pointer error");
            //----
            //note original function 
            //PointCWFrom(t.PointCWFrom(p));
            //so separate into 2 steps
            var cw_point_on_T = t.PointCWFrom(iPonT);
            //tempname techniqe ***
            P0.tempName = 0; P1.tempName = 1; P2.tempName = 2;
            switch (foundAtIndex = CalculateCWPoint(cw_point_on_T.tempName))
            {
                case 0:
                    {
                        related_ed = this.D0;
                        related_ec = this.C0;
                        return P0;
                    }
                case 1:
                    {
                        related_ed = this.D1;
                        related_ec = this.C1;
                        return P1;
                    }
                case 2:
                default:
                    {
                        related_ed = this.D2;
                        related_ec = this.C2;
                        return P2;
                    }
            }

            //int finalPoint = ((hintPointNumOfT + 2) % 3); //CW=> (FindIndexOf(point) + 2) % 3 
            //switch ((finalPoint + 2) % 3)   //CW=> (FindIndexOf(point) + 2) % 3
            //{
            //    case 0: return P0;
            //    case 1: return P1;
            //    default: return P2;
            //}
        }
        //public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
        //{
        //    Debug.Assert(t != this, "self-pointer error");
        //    return PointCWFrom(t.PointCWFrom(p));
        //}
        //public TriangulationPoint OppositePointOfP0
        //{
        //    get
        //    {
        //        //PointCWFrom
        //        //return Points[(IndexOf(point) + 2) % 3];
        //        //


        //    }
        //}
        //public TriangulationPoint OppositePointOfP1
        //{
        //    get
        //    {
        //    }
        //}
        //public TriangulationPoint OppositePointOfP2
        //{
        //    get
        //    {
        //    }
        //}
        public DelaunayTriangle NeighborCWFrom(TriangulationPoint point)
        {
            switch ((IndexOf(point) + 1) % 3)
            {
                case 0:
                    return N0;
                case 1:
                    return N1;
                default:
                    return N2;
            }
            //return Neighbors[(IndexOf(point) + 1) % 3];
        }
        public DelaunayTriangle NeighborCCWFrom(TriangulationPoint point)
        {
            // return Neighbors[(InternalIndexOf(point) + 2) % 3];
            switch ((IndexOf(point) + 2) % 3)
            {
                case 0:
                    return N0;
                case 1:
                    return N1;
                default:
                    return N2;
            }
        }
        /// <summary>
        /// get neighbor CW and CCW
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cw"></param>
        /// <param name="ccw"></param>
        public void GetNBs(TriangulationPoint point,
            out int foundAt,
            out DelaunayTriangle t_ccw,
            out DelaunayTriangle t_cw,
            out bool c_ccw,
            out bool c_cw,
            out bool d_ccw,
            out bool d_cw)
        {
            switch (foundAt = IndexOf(point)) //ccw
            {
                case 0:
                    t_cw = N1;
                    t_ccw = N2;
                    c_cw = C1;
                    c_ccw = C2;
                    d_cw = D1;
                    d_ccw = D2;
                    break;
                case 1:
                    t_cw = N2;
                    t_ccw = N0;
                    c_cw = C2;
                    c_ccw = C0;
                    d_cw = D2;
                    d_ccw = D0;
                    break;
                default://2
                    t_cw = N0;
                    t_ccw = N1;
                    c_cw = C0;
                    c_ccw = C1;
                    d_cw = D0;
                    d_ccw = D1;
                    break;
            }
        }

        public DelaunayTriangle NeighborAcrossFrom(TriangulationPoint point)
        {
            switch (IndexOf(point))
            {
                case 0:
                    return N0;
                case 1:
                    return N1;
                default:
                    return N2;
            }
        }

        public TriangulationPoint PointCCWFrom(TriangulationPoint point)
        {
            //return Points[(IndexOf(point) + 1) % 3];

            switch ((IndexOf(point) + 1) % 3)
            {
                case 0:
                    return this.P0;
                case 1:
                    return this.P1;
                case 2:
                default:
                    return this.P2;
            }
        }
        public TriangulationPoint PointCCWFrom(int index)
        {
            //return Points[(IndexOf(point) + 1) % 3]; 
            switch ((index + 1) % 3)
            {
                case 0:
                    return this.P0;
                case 1:
                    return this.P1;
                case 2:
                default:
                    return this.P2;
            }
        }
        public TriangulationPoint PointCWFrom(TriangulationPoint point)
        {
            //return Points[(IndexOf(point) + 2) % 3]; 
            switch ((IndexOf(point) + 2) % 3)
            {
                case 0:
                    return this.P0;
                case 1:
                    return this.P1;
                case 2:
                default:
                    return this.P2;
            }
        }
        public TriangulationPoint PointCWFrom(int index)
        {
            //return Points[(IndexOf(point) + 2) % 3]; 
            switch ((index + 2) % 3)
            {
                case 0:
                    return this.P0;
                case 1:
                    return this.P1;
                case 2:
                default:
                    return this.P2;
            }
        }
        public static int CalculateCWPoint(int index)
        {
            return (index + 2) % 3;
        }
        public static int CalculateCCWPoint(int index)
        {
            return (index + 1) % 3;
        }
        public TriangulationPoint GetPoint(int index)
        {
            switch (index)
            {
                case 0:
                    return this.P0;
                case 1:
                    return this.P1;
                case 2:
                default:
                    return this.P2;
            }
        }
        /// <summary>
        /// Legalize triangle by rotating clockwise around oPoint
        /// </summary>
        /// <param name="oPoint">The origin point to rotate around</param>
        /// <param name="nPoint">???</param>
        internal void Legalize(int previousOPointIndex, TriangulationPoint oPoint, TriangulationPoint nPoint, out int newOPointIndex)
        {
            //----------------
            //rotate cw (clockwise) 
            var temp = P2;
            P2 = P1;
            P1 = P0;
            P0 = temp;
            //---------------- 
            switch (previousOPointIndex)
            {
                case 0:
                    {
                        //after rotate cw , previousOPointIndex of oPoint 
                        // from 0 => 1;
                        newOPointIndex = 1;
                        // (IndexOf(1) + 1) % 3; =>2
                        // 2%3 = 2                        
                        P2 = nPoint;
                    }
                    break;
                case 1:
                    {
                        //after rotate cw , previousOPointIndex  of oPoint
                        //1 => 2;
                        newOPointIndex = 2;
                        //(IndexOf(2) + 1) % 3; ==>0
                        //3 %3 =0 
                        P0 = nPoint;
                    }
                    break;
                case 2:
                default:
                    {
                        //after rotate cw , previousOPointIndex  of 
                        //oPoint  2 => 0;
                        newOPointIndex = 0;
                        //return (IndexOf(0) + 1) % 3;==>1
                        //1%3 = 1; 
                        P1 = nPoint;
                    }
                    break;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return this.dbugId + (isActualTriangle ? "*" : "") + ": {" + P0 + "," + P1 + "," + P2 + "}";
        }
#endif
        /// <summary>
        /// Finalize edge marking
        /// </summary>
        public void MarkNeighborEdges()
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    if (EdgeIsConstrained[i] && Neighbors[i] != null)
            //    {
            //        Neighbors[i].MarkConstrainedEdge(Points[(i + 1) % 3], Points[(i + 2) % 3]);
            //    }

            //}

            //-----------------
            //0
            if (this.C0 && N0 != null)
            {
                //(0 + 1) % 3 => 2
                //(0 + 2) % 3 => 1
                N0.SelectAndMarkConstrainedEdge(2, 1);
            }
            //-----------------
            //1
            if (this.C1 && N1 != null)
            {
                //(1 + 1) % 3 => 1
                //(1 + 2) % 3 => 0
                N1.SelectAndMarkConstrainedEdge(1, 0);
            }
            //-----------------
            //2
            if (this.C2 && N2 != null)
            {
                //(2 + 1) % 3 => 0
                //(2 + 2) % 3 => 1

                N2.SelectAndMarkConstrainedEdge(0, 1);
            }
        }

        public void MarkEdge(DelaunayTriangle triangle)
        {
            //for (int i = 0; i < 3; i++)
            //{
            //    if (EdgeIsConstrained[i])
            //    {
            //        triangle.MarkConstrainedEdge(Points[(i + 1) % 3], Points[(i + 2) % 3]);
            //    }
            //}
            if (this.C0)
            {    //(0 + 1) % 3 => 2
                //(0 + 2) % 3 => 1
                triangle.SelectAndMarkConstrainedEdge(2, 11);
            }
            if (this.C1)
            {   //(1 + 1) % 3 => 1
                //(1 + 2) % 3 => 0
                triangle.SelectAndMarkConstrainedEdge(1, 0);
            }
            if (this.C2)
            {
                //(2 + 1) % 3 => 0
                //(2 + 2) % 3 => 1
                triangle.SelectAndMarkConstrainedEdge(0, 1);
            }
        }

        public void MarkEdge(List<DelaunayTriangle> tList)
        {
            foreach (DelaunayTriangle t in tList)
            {
                //for (int i = 0; i < 3; i++)
                //{
                //    if (t.EdgeIsConstrained[i])
                //    {
                //        MarkConstrainedEdge(t.Points[(i + 1) % 3], t.Points[(i + 2) % 3]);
                //    }
                //}
                //-----------------------------
                //0
                if (t.C0)
                {
                    //(0 + 1) % 3 => 2;
                    //(0 + 2) % 3 => 1;
                    SelectAndMarkConstrainedEdge(2, 1);
                }
                //-----------------------------
                //1
                if (t.C1)
                {
                    //(1 + 1) % 3 => 1;
                    //(1 + 2) % 3 => 0;
                    SelectAndMarkConstrainedEdge(1, 0);
                }
                //-----------------------------
                //2
                if (t.C2)
                {
                    //(2 + 1) % 3 => 0;
                    //(2 + 2) % 3 => 1;
                    SelectAndMarkConstrainedEdge(0, 1);
                }
            }
        }


        //public void SelectAndMarkConstrainedEdge(DTSweepConstraint edge)
        //{
        //    SelectAndMarkConstrainedEdge(edge.P, edge.Q);
        //}

        /// <summary>
        /// Mark edge as constrained
        /// </summary>
        public void SelectAndMarkConstrainedEdge(TriangulationPoint p, TriangulationPoint q)
        {
            MarkEdgeConstraint(FindEdgeIndex(p, q), true);
        }
        public void SelectAndMarkConstrainedEdge(int i_p, int i_q)
        {
            MarkEdgeConstraint(FindEdgeIndex(i_p, i_q), true);
        }
        public double Area()
        {
            double b = P0.X - P1.X;
            double h = P2.Y - P1.Y;
            return Math.Abs((b * h * 0.5f));
        }


        public void GetCentroid(out float cx, out float cy)
        {
            cx = (float)((P0.X + P1.X + P2.X) / 3f);
            cy = (float)((P0.Y + P1.Y + P2.Y) / 3f);
        }
        /// <summary>
        /// Get the index of the neighbor that shares this edge (or -1 if it isn't shared)
        /// </summary>
        /// <returns>index of the shared edge or -1 if edge isn't shared</returns>
        public int FindEdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
        {
            //temporary naming 3 points
            //-----------------------------
            //temp num technique , don't use with recursive
            //1.clear unknown point
            p1.tempName = p2.tempName = 3;
            //2. just name  my points
            P0.tempName = 0; //a as 1
            P1.tempName = 1; //b as 2
            P2.tempName = 2; //c as 3
            //-----------------------------   
            //int i1 = p1.tempName;
            //int i2 = p2.tempName;
            //bool a = (i1 == 0 || i2 == 0);
            //bool b = (i1 == 1 || i2 == 1);
            //bool c = (i1 == 2 || i2 == 2);
            //if (b && c) return 0;
            //if (a && c) return 1;
            //if (a && b) return 2;
            //return -1; 
            return FindEdgeIndex(p1.tempName, p2.tempName);
        }

        // public bool GetConstrainedEdgeCCW(TriangulationPoint p) { return EdgeIsConstrained((IndexOf(p) + 2) % 3); }
        public bool GetConstrainedEdgeCW(TriangulationPoint p) { return EdgeIsConstrained((IndexOf(p) + 1) % 3); }
        // public bool GetConstrainedEdgeAcross(TriangulationPoint p) { return EdgeIsConstrained(IndexOf(p)); }
        //public void SetConstrainedEdgeCCW(TriangulationPoint p, bool ce) { MarkEdgeConstraint((IndexOf(p) + 2) % 3, ce); }
        // public void SetConstrainedEdgeCW(TriangulationPoint p, bool ce) { MarkEdgeConstraint((IndexOf(p) + 1) % 3, ce); }
        // public void SetConstrainedEdgeAcross(TriangulationPoint p, bool ce) { MarkEdgeConstraint(IndexOf(p), ce); }

        //public bool GetDelaunayEdgeCCW(TriangulationPoint p) { return EdgeIsDelaunay((IndexOf(p) + 2) % 3); }
        //public bool GetDelaunayEdgeCW(TriangulationPoint p) { return EdgeIsDelaunay((IndexOf(p) + 1) % 3); }
        // public bool GetDelaunayEdgeAcross(TriangulationPoint p) { return EdgeIsDelaunay(IndexOf(p)); }

        //public void SetDelaunayEdgeCCW(TriangulationPoint p, bool ce) { MarkEdgeDelunay((IndexOf(p) + 2) % 3, ce); }
        // public void SetDelaunayEdgeCW(TriangulationPoint p, bool ce) { MarkEdgeDelunay((IndexOf(p) + 1) % 3, ce); }
        // public void SetDelaunayEdgeAcross(TriangulationPoint p, bool ce) { MarkEdgeDelunay(IndexOf(p), ce); }

        public void SetNBCW(int index, bool c, bool d)
        {
            //IndexOf(p) + 1) % 3
            switch ((index + 1) % 3)
            {
                case 0:
                    {
                        C0 = c;
                        D0 = d;
                    }
                    break;
                case 1:
                    {
                        //(1+1)%3= 1
                        C1 = c;
                        D1 = d;
                    }
                    break;
                case 2:
                    {
                        C2 = c;
                        D2 = d;
                    }
                    break;
            }
        }
        public void SetNBCCW(int index, bool c, bool d)
        {
            //  IndexOf(p) + 2) % 3
            switch ((index + 2) % 3)
            {
                case 0:
                    {
                        C0 = c;
                        D0 = d;
                    }
                    break;
                case 1:
                    {
                        //(1+1)%3= 1
                        C1 = c;
                        D1 = d;
                    }
                    break;
                case 2:
                    {
                        C2 = c;
                        D2 = d;
                    }
                    break;
            }
        }
        public static int FindEdgeIndexWithTempNameFlags(int totalFlags)
        {
            //a =0,b =1,c= 3
            //a && a= 0+0 =>0 =>err
            //b && b = 1 +1 => 2 =>err
            //c && c=  3+ 3=>6 => err 

            //(a && b) = (b && a) => 0 + 1=> 1 : return 2
            //(a && c) ==(c &&a) => 0+ 3=>3 : return 1
            //(b && c) == (c&& b) =>1 +3=>4 : return 0

            switch (totalFlags)
            {
                case 1:
                    return 2;
                case 2:
                    return 1;
                case 3:
                    return 0;
                default:
                    return -1;
            }
        }
        public static int FindEdgeIndex(int i1, int i2)
        {
            //-------------------------------
            //implement switch table ***
            //-------------------------------
            //i1=0,=>a
            //     i2=0=>err,i2=>1 =b,     i2=2=>c ,//i2=0=>err
            //i1=1=>b
            //     i2=0=>a,  i2=>1 =>err , i2=2=>c
            //i1=2=>c
            //     i2=0=>a,  i2=>1=>b,     i2=2=>c
            //-------------------------------
            //version 2
            i1++; // 0=>1 (01) ,1=>2  (10),2=>3 (11)
            i2 = (i2 + 1) << 2;//0=>1  (0100) ,1=>2   (1000), 2=>3 (1100)
            switch (i1 | i2)
            {
                //a && b
                //b && a
                case ((1 << 2) | 2): //
                case ((2 << 2) | 1): //  
                    return 2;
                //------------------
                //a &&c 
                //c&& a
                case ((1 << 2) | 3): //
                case ((3 << 2) | 1): //  
                    return 1;
                //b && c
                //c && b
                case ((2 << 2) | 3): //
                case ((3 << 2) | 2): //  
                    return 0;
            }
            return -1;
            //-------------------------------
            //version 1
            //-------------------------------
            //implement switch table ***
            //-------------------------------
            //i1=0,=>a
            //     i2=0=>err,i2=>1 =b,     i2=2=>c ,//i2=0=>err
            //i1=1=>b
            //     i2=0=>a,  i2=>1 =>err , i2=2=>c
            //i1=2=>c
            //     i2=0=>a,  i2=>1=>b,     i2=2=>c
            //-------------------------------
            //switch (i1)
            //{
            //    case 0://a
            //        {
            //            switch (i2)
            //            {
            //                case 0:
            //                    return -1;
            //                case 1: //b => a && b
            //                    return 2;
            //                case 2://c => a&&c
            //                    return 1;
            //            }
            //        } break;
            //    case 1: //b
            //        {
            //            switch (i2)
            //            {
            //                case 0: //a => b && a => a&& b
            //                    return 2;
            //                case 1: //b => b && b
            //                    return -1;
            //                case 2://c => b && c
            //                    return 0;
            //            }
            //        } break;
            //    case 2://c
            //        {
            //            switch (i2)
            //            {
            //                case 0: //a => c && a => a&& c
            //                    return 1;
            //                case 1: //b => c && b => b &&c 
            //                    return 0;
            //                case 2://c => c && c=>err
            //                    return -1;
            //            }
            //        } break;
            //}
            //-------------------------------
            //original 

            //bool a = (i1 == 0 || i2 == 0);
            //bool b = (i1 == 1 || i2 == 1);
            //bool c = (i1 == 2 || i2 == 2);
            //if (b && c) return 0;
            //if (a && c) return 1;
            //if (a && b) return 2;

            //return -1;
        }
    }
}
