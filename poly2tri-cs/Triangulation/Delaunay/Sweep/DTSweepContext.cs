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

namespace Poly2Tri
{
    /**
     * 
     * @author Thomas Åhlén, thahlen@gmail.com
     *
     */
    public class DTSweepContext : TriangulationContext
    {
        //*** share object !

        // Inital triangle factor, seed triangle will extend 30% of 
        // PointSet width to both left and right.
        private const float ALPHA = 0.3f;
        internal AdvancingFront Front;
        TriangulationPoint Head { get; set; }
        TriangulationPoint Tail { get; set; }

        //----------------------------------
        //basin
        internal AdvancingFrontNode BasinLeftNode;
        internal AdvancingFrontNode BasinBottomNode;
        internal AdvancingFrontNode BasinRightNode;
        internal double BasinWidth;
        internal bool BasinLeftHighest;
        //----------------------------------
        internal DTSweepConstraint EdgeEventConstrainedEdge;
        internal bool EdgeEventRight;
        //----------------------------------
#if DEBUG

        static int dbugTotalId;
        public int dbugId = dbugTotalId++;
#endif

        public DTSweepContext()
        {
        }
        //public   bool IsDebugEnabled
        //{
        //    get
        //    {
        //        return base.IsDebugEnabled;
        //    }
        //    //protected set
        //    //{
        //    //    if (value && DebugContext == null)
        //    //    {
        //    //        DebugContext = new DTSweepDebugContext(this);
        //    //    }
        //    //    base.IsDebugEnabled = value;
        //    //}
        //}

        public void RemoveFromList(DelaunayTriangle triangle)
        {
            Triangles.Remove(triangle);
            // TODO: remove all neighbor pointers to this triangle
            //        for( int i=0; i<3; i++ )
            //        {
            //            if( triangle.neighbors[i] != null )
            //            {
            //                triangle.neighbors[i].clearNeighbor( triangle );
            //            }
            //        }
            //        triangle.clearNeighbors();
        }

        public void MeshClean(DelaunayTriangle triangle)
        {
            MeshCleanReq(triangle);
        }

        private void MeshCleanReq(DelaunayTriangle triangle)
        {
            if (triangle != null && !triangle.IsInterior)
            {
                triangle.IsInterior = true;
                Triangulatable.AddTriangle(triangle);
                //0
                if (!triangle.C0)
                {
                    MeshCleanReq(triangle.N0);
                }
                //1
                if (!triangle.C1)
                {
                    MeshCleanReq(triangle.N1);
                }
                //2
                if (!triangle.C2)
                {
                    MeshCleanReq(triangle.N2);
                }
                //for (int i = 0; i < 3; i++)
                //{
                //    if (!triangle.EdgeIsConstrained[i])
                //    {
                //        MeshCleanReq(triangle.Neighbors[i]);
                //    }
                //}
            }
        }

        public override void Clear()
        {
            base.Clear();
        }

        //public void AddNode(AdvancingFrontNode node)
        //{
        //    //        Console.WriteLine( "add:" + node.key + ":" + System.identityHashCode(node.key));
        //    //        m_nodeTree.put( node.getKey(), node );
        //    //Front.AddNode(node);
        //}

        //public void RemoveNode(AdvancingFrontNode node)
        //{
        //    //        Console.WriteLine( "remove:" + node.key + ":" + System.identityHashCode(node.key));
        //    //        m_nodeTree.delete( node.getKey() );
        //    // Front.RemoveNode(node);
        //}

        internal AdvancingFrontNode LocateNode(TriangulationPoint point)
        {
            return Front.LocateNode(point);
        }

        public void CreateAdvancingFront()
        {
            AdvancingFrontNode head, tail, middle;
            // Initial triangle
            DelaunayTriangle dtri = new DelaunayTriangle(Points[0], Tail, Head);
            Triangles.Add(dtri);
            head = new AdvancingFrontNode(dtri.P1);
            head.Triangle = dtri;
            middle = new AdvancingFrontNode(dtri.P0);
            middle.Triangle = dtri;
            tail = new AdvancingFrontNode(dtri.P2);
            Front = new AdvancingFront(head, tail);
            //Front.AddNode(middle);

            // TODO: I think it would be more intuitive if head is middles next and not previous
            //so swap head and tail
            Front.Head.Next = middle;
            middle.Next = Front.Tail;
            middle.Prev = Front.Head;
            Front.Tail.Prev = middle;
        }





        /// <summary>
        /// Try to map a node to all sides of this triangle that don't have 
        /// a neighbor.
        /// </summary>
        public void MapTriangleToNodes(DelaunayTriangle t)
        {
            //for (int i = 0; i < 3; i++)
            //    if (t.Neighbors[i] == null)
            //    {
            //        AdvancingFrontNode n = Front.LocatePoint(t.PointCWFrom(t.Points[i]));
            //        if (n != null) n.Triangle = t;
            //    }

            //------------------------------
            //PointCWFrom
            //(FindIndexOf(0) + 2) % 3=>2
            //(FindIndexOf(1) + 2) % 3=>0
            //(FindIndexOf(2) + 2) % 3=>1
            //------------------------------ 

            if (t.N0 == null)
            {
                //AdvancingFrontNode n = Front.LocatePoint(t.PointCWFrom(t.P0));
                //(FindIndexOf(0) + 2) % 3=>2

                AdvancingFrontNode n = Front.LocatePoint(t.P2);
                if (n != null)
                {
                    n.Triangle = t;
                }
            }
            if (t.N1 == null)
            {
                //(FindIndexOf(1) + 2) % 3=>0
                AdvancingFrontNode n = Front.LocatePoint(t.P0);
                if (n != null)
                {
                    n.Triangle = t;
                }
            }
            if (t.N2 == null)
            {
                AdvancingFrontNode n = Front.LocatePoint(t.P1);
                if (n != null)
                {
                    n.Triangle = t;
                }
            }
        }

        public override void PrepareTriangulation(Triangulatable t)
        {
            //--------------------
            //initialization phase:
            //all points are sorted regarding y coordinate, 
            //regardless of whether they define an edge or not.
            //those points havingthe same y coordinates are also sorted
            //in the x direction.
            //Each point is associated with the information wheter nor not
            //it is the upper ending point of one or more edge e^i
            //--------------------
            //the following creates 'initial triangle',
            //max bounds,
            //p1 and p2=> artificial points
            //-------------------
            //during the fininalization phase,
            //all triangles, having at least one vertex among the 
            //artificial points, are erased.


            base.PrepareTriangulation(t);
            double xmax, xmin;
            double ymax, ymin;
            xmax = xmin = Points[0].X;
            ymax = ymin = Points[0].Y;
            // Calculate bounds. Should be combined with the sorting
            var tmp_points = this.Points;
            for (int i = tmp_points.Count - 1; i >= 0; --i)
            {
                var p = tmp_points[i];
                if (p.X > xmax) xmax = p.X;
                if (p.X < xmin) xmin = p.X;
                if (p.Y > ymax) ymax = p.Y;
                if (p.Y < ymin) ymin = p.Y;
            }


            double deltaX = ALPHA * (xmax - xmin);
            double deltaY = ALPHA * (ymax - ymin);
            TriangulationPoint p1 = new TriangulationPoint(xmax + deltaX, ymin - deltaY);
            TriangulationPoint p2 = new TriangulationPoint(xmin - deltaX, ymin - deltaY);
            Head = p1;
            Tail = p2;
            //long time = System.nanoTime();
            //Sort the points along y-axis
            Points.Sort(Compare);
            //logger.info( "Triangulation setup [{}ms]", ( System.nanoTime() - time ) / 1e6 );
        }
        static int Compare(TriangulationPoint p1, TriangulationPoint p2)
        {
            if (p1.Y < p2.Y)
            {
                return -1;
            }
            else if (p1.Y > p2.Y)
            {
                return 1;
            }
            else
            {
                if (p1.X < p2.X)
                {
                    return -1;
                }
                else if (p1.X > p2.X)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public void FinalizeTriangulation()
        {
            Triangulatable.AddTriangles(Triangles);
            Triangles.Clear();
        }

        public override void MakeNewConstraint(TriangulationPoint a, TriangulationPoint b)
        {
            //new DTSweepConstraint(a, b);
            DTSweepConstraintMaker.BuildConstraint(a, b);
        }

        public override TriangulationAlgorithm Algorithm
        {
            get { return TriangulationAlgorithm.DTSweep; }
        }
    }
}
