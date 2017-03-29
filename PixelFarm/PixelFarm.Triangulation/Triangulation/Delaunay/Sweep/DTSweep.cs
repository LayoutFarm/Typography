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

/*
 * Sweep-line, Constrained Delauney Triangulation (CDT) See: Domiter, V. and
 * Zalik, B.(2008)'Sweep-line algorithm for constrained Delaunay triangulation',
 * International Journal of Geographical Information Science
 * 
 * "FlipScan" Constrained Edge Algorithm invented by author of this code.
 * 
 * Author: Thomas Åhlén, thahlen@gmail.com 
 */

/// Changes from the Java version
///   Turned DTSweep into a static class
///   Lots of deindentation via early bailout
/// Future possibilities
///   Comments!


using System; 
namespace Poly2Tri
{
    public static class DTSweep
    {
        private const double PI_div2 = Math.PI / 2;
        private const double PI_3div4 = 3 * Math.PI / 4;
        /// <summary>
        /// Triangulate simple polygon with holes
        /// </summary>
        public static void Triangulate(DTSweepContext tcx)
        {
            tcx.CreateAdvancingFront();
            Sweep(tcx);
            // TODO: remove temporary
            // Check if the sweep algorithm is legalize robust
            // By doing a legalize on all triangles and see if anything happens
            // we know if the sweep algorithm missed some legalizations
            //        Console.WriteLine("============================");
            //        foreach ( DelaunayTriangle t in tcx.Triangles )
            //        {
            //            if( Legalize( tcx, t ) )
            //            {
            //                tcx.getDebugContext().setPrimaryTriangle( t );
            //                Console.WriteLine("[FIX] Triangle needed legalization after sweep");
            //            }
            //        }

            // Finalize triangulation
            if (tcx.TriangulationMode == TriangulationMode.Polygon)
            {
                FinalizationPolygon(tcx);
            }
            else
            {
                FinalizationConvexHull(tcx);
            }
#if DEBUG
            tcx.dbugDone();
#endif
        }

        /// <summary>
        /// Start sweeping the Y-sorted point set from bottom to top
        /// </summary>
        static void Sweep(DTSweepContext tcx)
        {
            //--------------------------
            //step 2: (3.4) Sweeping
            //following the idea of the sweep-line paradigm, the
            //algorithm stops at every point and checks its status.
            //  2 cases are possible:
            //      1) a point is isolated OR 
            //        it is the lower edge point( we name this case as a 'point event')
            //      2) a point is the upper ending point of at least one line segment
            //      (this case is considered as an 'edge event')
            //Obviously, the point insertion is executed at each point regardless of its type, but the
            //edge insertion is done only when the whole edge is swept
            //--------------------------
            var points = tcx.Points;
            TriangulationPoint point;
            AdvancingFrontNode node;
            for (int i = 1; i < points.Count; i++)
            {
                point = points[i];
                node = PointEvent(tcx, point);
                var internalEdgeList = point.GetInternalEdgeList();
                if (internalEdgeList != null)
                {
                    int j = internalEdgeList.Count;
                    if (tcx.IsDebugEnabled)
                    {
                        for (int m = 0; m < j; ++m)
                        {
                            var e = internalEdgeList[m];
                            tcx.DTDebugContext.ActiveConstraint = e;
                            EdgeEvent(tcx, e, node);
                        }
                    }
                    else
                    {
                        for (int m = 0; m < j; ++m)
                        {
                            EdgeEvent(tcx, internalEdgeList[m], node);
                        }
                    }
                }

                //if (point.HasEdges)
                //{ 
                //    foreach (DTSweepConstraint e in point.GetEdgeIter())
                //    {
                //        if (tcx.IsDebugEnabled)
                //        {
                //            tcx.DTDebugContext.ActiveConstraint = e;
                //        }
                //        EdgeEvent(tcx, e, node);
                //    }
                //}
                tcx.Update(null);
            }
        }

        /// <summary>
        /// If this is a Delaunay Triangulation of a pointset we need to fill so the triangle mesh gets a ConvexHull 
        /// </summary>
        static void FinalizationConvexHull(DTSweepContext tcx)
        {
            AdvancingFrontNode n1, n2, n3;
            DelaunayTriangle t1;
            TriangulationPoint first, p1;
            n1 = tcx.Front.Head.Next;
            n2 = n1.Next;
            n3 = n2.Next;
            first = n1.Point;
            TurnAdvancingFrontConvex(tcx, n1, n2);
            n1 = tcx.Front.Tail.Prev;
            if (n1.Triangle.Contains(n1.Next.Point) && n1.Triangle.Contains(n1.Prev.Point))
            {
                t1 = n1.Triangle.NeighborAcrossFrom(n1.Point);
                RotateTrianglePair(n1.Triangle, n1.Point, t1, t1.OppositePoint(n1.Triangle, n1.Point));
                tcx.MapTriangleToNodes(n1.Triangle);
                tcx.MapTriangleToNodes(t1);
            }
            n1 = tcx.Front.Head.Next;
            if (n1.Triangle.Contains(n1.Prev.Point) && n1.Triangle.Contains(n1.Next.Point))
            {
                t1 = n1.Triangle.NeighborAcrossFrom(n1.Point);
                RotateTrianglePair(n1.Triangle, n1.Point, t1, t1.OppositePoint(n1.Triangle, n1.Point));
                tcx.MapTriangleToNodes(n1.Triangle);
                tcx.MapTriangleToNodes(t1);
            }

            // TODO: implement ConvexHull for lower right and left boundary
            // Lower right boundary 
            first = tcx.Front.Head.Point;
            n2 = tcx.Front.Tail.Prev;
            t1 = n2.Triangle;
            p1 = n2.Point;
            do
            {
                tcx.RemoveFromList(t1);
                p1 = t1.PointCCWFrom(p1);
                if (p1 == first) break;
                t1 = t1.NeighborCCWFrom(p1);
            } while (true);
            // Lower left boundary
            first = tcx.Front.Head.Next.Point;
            p1 = t1.PointCWFrom(tcx.Front.Head.Point);
            t1 = t1.NeighborCWFrom(tcx.Front.Head.Point);
            do
            {
                tcx.RemoveFromList(t1);
                p1 = t1.PointCCWFrom(p1);
                t1 = t1.NeighborCCWFrom(p1);
            } while (p1 != first);
            tcx.FinalizeTriangulation();
        }

        /// <summary>
        /// We will traverse the entire advancing front and fill it to form a convex hull.
        /// </summary>
        static void TurnAdvancingFrontConvex(DTSweepContext tcx, AdvancingFrontNode b, AdvancingFrontNode c)
        {
            AdvancingFrontNode first = b;
            while (c != tcx.Front.Tail)
            {
                if (tcx.IsDebugEnabled) tcx.DTDebugContext.ActiveNode = c;
                if (TriangulationUtil.Orient2d(b.Point, c.Point, c.Next.Point) == Orientation.CCW)
                {
                    // [b,c,d] Concave - fill around c
                    Fill(tcx, c);
                    c = c.Next;
                }
                else
                {
                    // [b,c,d] Convex
                    if (b != first && TriangulationUtil.Orient2d(b.Prev.Point, b.Point, c.Point) == Orientation.CCW)
                    {
                        // [a,b,c] Concave - fill around b
                        Fill(tcx, b);
                        b = b.Prev;
                    }
                    else
                    {
                        // [a,b,c] Convex - nothing to fill
                        b = c;
                        c = c.Next;
                    }
                }
            }
        }

        static void FinalizationPolygon(DTSweepContext tcx)
        {
            // Get an Internal triangle to start with
            DelaunayTriangle t = tcx.Front.Head.Next.Triangle;
            TriangulationPoint p = tcx.Front.Head.Next.Point;
            while (!t.GetConstrainedEdgeCW(p))
            {
                t = t.NeighborCCWFrom(p);
            }

            // Collect interior triangles constrained by edges
            tcx.MeshClean(t);
        }

        /// <summary>
        /// Find closes node to the left of the new point and
        /// create a new triangle. If needed new holes and basins
        /// will be filled to.
        /// </summary>
        static AdvancingFrontNode PointEvent(DTSweepContext tcx, TriangulationPoint point)
        {
            AdvancingFrontNode node, newNode;
            node = tcx.LocateNode(point);
            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.ActiveNode = node;
            }

            newNode = NewFrontTriangle(tcx, point, node);
            // Only need to check +epsilon since point never have smaller 
            // x value than node due to how we fetch nodes from the front
            if (point.X <= node.Point.X + TriangulationUtil.EPSILON)
            {
                Fill(tcx, node);
            }

            //tcx.AddNode(newNode);

            FillAdvancingFront(tcx, newNode);
            return newNode;
        }

        /// <summary>
        /// Creates a new front triangle and legalize it
        /// </summary>
        static AdvancingFrontNode NewFrontTriangle(DTSweepContext tcx, TriangulationPoint point, AdvancingFrontNode node)
        {
            DelaunayTriangle triangle = new DelaunayTriangle(point, node.Point, node.Next.Point);
            triangle.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(triangle);
            AdvancingFrontNode newNode = new AdvancingFrontNode(point);
            newNode.Next = node.Next;
            newNode.Prev = node;
            node.Next.Prev = newNode;
            node.Next = newNode;
            //tcx.AddNode(newNode); // XXX: BST

            if (tcx.IsDebugEnabled) tcx.DTDebugContext.ActiveNode = newNode;
            if (!Legalize(tcx, triangle)) tcx.MapTriangleToNodes(triangle);
            return newNode;
        }

        static void EdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            try
            {
                tcx.EdgeEventConstrainedEdge = edge;
                tcx.EdgeEventRight = edge.P.X > edge.Q.X;
                if (tcx.IsDebugEnabled) { tcx.DTDebugContext.PrimaryTriangle = node.Triangle; }

                if (MarkEdgeSideOfTriangle(node.Triangle, edge.P, edge.Q)) return;
                // For now we will do all needed filling
                // TODO: integrate with flip process might give some better performance 
                //       but for now this avoid the issue with cases that needs both flips and fills
                FillEdgeEvent(tcx, edge, node);
                EdgeEvent(tcx, edge.P, edge.Q, node.Triangle, edge.Q);
            }
            catch (PointOnEdgeException e)
            {
                //Debug.WriteLine(String.Format("Warning: Skipping Edge: {0}", e.Message));
                throw;
            }
        }

        static void FillEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.EdgeEventRight)
            {
                FillRightAboveEdgeEvent(tcx, edge, node);
            }
            else
            {
                FillLeftAboveEdgeEvent(tcx, edge, node);
            }
        }

        static void FillRightConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Next);
            if (node.Next.Point != edge.P)
            {
                // Next above or below edge?
                if (TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW)
                {
                    // Below
                    if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
                    {
                        // Next is concave
                        FillRightConcaveEdgeEvent(tcx, edge, node);
                    }
                    else
                    {
                        // Next is convex
                    }
                }
            }
        }

        private static void FillRightConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            // Next concave or convex?
            if (TriangulationUtil.Orient2d(node.Next.Point, node.Next.Next.Point, node.Next.Next.Next.Point) == Orientation.CCW)
            {
                // Concave
                FillRightConcaveEdgeEvent(tcx, edge, node.Next);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriangulationUtil.Orient2d(edge.Q, node.Next.Next.Point, edge.P) == Orientation.CCW)
                {
                    // Below
                    FillRightConvexEdgeEvent(tcx, edge, node.Next);
                }
                else
                {
                    // Above
                }
            }
        }

        private static void FillRightBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.IsDebugEnabled) tcx.DTDebugContext.ActiveNode = node;
            if (node.Point.X < edge.P.X)
            { // needed?
                if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
                {
                    // Concave 
                    FillRightConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    // Convex
                    FillRightConvexEdgeEvent(tcx, edge, node);
                    // Retry this one
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillRightAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Next.Point.X < edge.P.X)
            {
                if (tcx.IsDebugEnabled) { tcx.DTDebugContext.ActiveNode = node; }

                Orientation o1 = TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P);
                if (o1 == Orientation.CCW)
                {
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        private static void FillLeftConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            // Next concave or convex?
            if (TriangulationUtil.Orient2d(node.Prev.Point, node.Prev.Prev.Point, node.Prev.Prev.Prev.Point) == Orientation.CW)
            {
                // Concave
                FillLeftConcaveEdgeEvent(tcx, edge, node.Prev);
            }
            else
            {
                // Convex
                // Next above or below edge?
                if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Prev.Point, edge.P) == Orientation.CW)
                {
                    // Below
                    FillLeftConvexEdgeEvent(tcx, edge, node.Prev);
                }
                else
                {
                    // Above
                }
            }
        }

        private static void FillLeftConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Prev);
            if (node.Prev.Point != edge.P)
            {
                // Next above or below edge?
                if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW)
                {
                    // Below
                    if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW)
                    {
                        // Next is concave
                        FillLeftConcaveEdgeEvent(tcx, edge, node);
                    }
                    else
                    {
                        // Next is convex
                    }
                }
            }
        }

        private static void FillLeftBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.IsDebugEnabled) tcx.DTDebugContext.ActiveNode = node;
            if (node.Point.X > edge.P.X)
            {
                if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW)
                {
                    // Concave 
                    FillLeftConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    // Convex
                    FillLeftConvexEdgeEvent(tcx, edge, node);
                    // Retry this one
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillLeftAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Prev.Point.X > edge.P.X)
            {
                if (tcx.IsDebugEnabled) tcx.DTDebugContext.ActiveNode = node;
                // Check if next node is below the edge
                Orientation o1 = TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P);
                if (o1 == Orientation.CW)
                {
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Prev;
                }
            }
        }


        private static bool MarkEdgeSideOfTriangle(DelaunayTriangle triangle, TriangulationPoint ep, TriangulationPoint eq)
        {
            switch (triangle.FindEdgeIndex(ep, eq))
            {
                case 0:
                    {   //mark constraint
                        triangle.C0 = true;
                        //change triangle here
                        if ((triangle = triangle.N0) != null) { triangle.SelectAndMarkConstrainedEdge(ep, eq); }
                        return true;
                    }
                case 1:
                    {
                        triangle.C1 = true;
                        //change triangle here
                        if ((triangle = triangle.N1) != null) { triangle.SelectAndMarkConstrainedEdge(ep, eq); }
                        return true;
                    }
                case 2:
                    {
                        triangle.C2 = true;
                        //change triangle here
                        if ((triangle = triangle.N2) != null) { triangle.SelectAndMarkConstrainedEdge(ep, eq); }
                        return true;
                    }
                default:
                    //may be -1 
                    return false;
            }
        }

        private static void EdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle triangle, TriangulationPoint point)
        {
            TriangulationPoint p1, p2;
            if (tcx.IsDebugEnabled) tcx.DTDebugContext.PrimaryTriangle = triangle;
            if (MarkEdgeSideOfTriangle(triangle, ep, eq)) return;
            p1 = triangle.PointCCWFrom(point);
            Orientation o1 = TriangulationUtil.Orient2d(eq, p1, ep);
            if (o1 == Orientation.Collinear)
            {
                // TODO: Split edge in two
                ////            splitEdge( ep, eq, p1 );
                //            edgeEvent( tcx, p1, eq, triangle, point );
                //            edgeEvent( tcx, ep, p1, triangle, p1 );
                //            return;
                throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet", eq, p1, ep);
            }

            p2 = triangle.PointCWFrom(point);
            Orientation o2 = TriangulationUtil.Orient2d(eq, p2, ep);
            if (o2 == Orientation.Collinear)
            {
                // TODO: Split edge in two
                //            edgeEvent( tcx, p2, eq, triangle, point );
                //            edgeEvent( tcx, ep, p2, triangle, p2 );
                //            return;
                throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet", eq, p2, ep);
            }

            if (o1 == o2)
            {
                // Need to decide if we are rotating CW or CCW to get to a triangle
                // that will cross edge
                if (o1 == Orientation.CW)
                {
                    triangle = triangle.NeighborCCWFrom(point);
                }
                else
                {
                    triangle = triangle.NeighborCWFrom(point);
                }
                EdgeEvent(tcx, ep, eq, triangle, point);
            }
            else
            {
                // This triangle crosses constraint so lets flippin start!
                FlipEdgeEvent(tcx, ep, eq, triangle, point);
            }
        }

        /// <summary>
        /// In the case of a pointset with some constraint edges. If a triangle side is collinear
        /// with a part of the constraint we split the constraint into two constraints. This could
        /// happen when the given constraint migth intersect a point in the set.<br>
        /// This can never happen in the case when we are working with a polygon.
        /// 
        /// Think of two triangles that have non shared sides that are collinear and the constraint
        /// is set from a point in triangle A to a point in triangle B so that the constraint is
        /// the union of both those sides. We then have to split the constraint into two so we get
        /// one constraint for each triangle.  
        /// </summary>
        /// <param name="ep"></param>
        /// <param name="eq"></param>
        /// <param name="p">point on the edge between ep->eq</param>
        //private static void SplitEdge(TriangulationPoint ep, TriangulationPoint eq, TriangulationPoint p)
        //{

        //    //DTSweepConstraint edge = eq.Edges.First(e => e.Q == ep || e.P == ep);
        //    //DTSweepConstraint edge = FindFirst(eq.Edges, ep);//.First(e => e.Q == ep || e.P == ep);
        //    DTSweepConstraint edge = eq.FindFirst(ep);
        //    edge.P = p;
        //    constraintBuilder.BuildConstraint(ep, p);// Et tu, Brute? --MM


        //    //        // Redo this edge now that we have split the constraint
        //    //          newEdgeEvent( tcx, edge, triangle, point );
        //    //          // Continue with new edge
        //    //          newEdgeEvent( tcx, edge, triangle, p2 );
        //}

        static void FlipEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle ot = t.NeighborAcrossFrom(p);
            TriangulationPoint op = ot.OppositePoint(t, p);
            if (ot == null)
            {
                // If we want to integrate the fillEdgeEvent do it here
                // With current implementation we should never get here
                throw new InvalidOperationException("[BUG:FIXME] FLIP failed due to missing triangle");
            }

            if (tcx.IsDebugEnabled)
            {
                tcx.DTDebugContext.PrimaryTriangle = t;
                tcx.DTDebugContext.SecondaryTriangle = ot;
            } // TODO: remove



            int p_indexOnT = t.IndexOf(p);//p is point in t ***
            if (TriangulationUtil.InScanArea(p, t.PointCCWFrom(p_indexOnT), t.PointCWFrom(p_indexOnT), op))
            {
                // Lets rotate shared edge one vertex CW
                RotateTrianglePair(t, p, ot, op);
                tcx.MapTriangleToNodes(t);
                tcx.MapTriangleToNodes(ot);
                if (p == eq && op == ep)
                {
                    if (eq == tcx.EdgeEventConstrainedEdge.Q
                        && ep == tcx.EdgeEventConstrainedEdge.P)
                    {
                        if (tcx.IsDebugEnabled) Console.WriteLine("[FLIP] - constrained edge done"); // TODO: remove
                        //
                        t.SelectAndMarkConstrainedEdge(ep, eq);
                        ot.SelectAndMarkConstrainedEdge(ep, eq);
                        Legalize(tcx, t);
                        Legalize(tcx, ot);
                    }
                    else
                    {
                        if (tcx.IsDebugEnabled) Console.WriteLine("[FLIP] - subedge done"); // TODO: remove
                        // XXX: I think one of the triangles should be legalized here?
                    }
                }
                else
                {
                    if (tcx.IsDebugEnabled) Console.WriteLine("[FLIP] - flipping and continuing with triangle still crossing edge"); // TODO: remove
                    //
                    Orientation o = TriangulationUtil.Orient2d(eq, op, ep);
                    t = NextFlipTriangle(tcx, o, t, ot, p, op);
                    FlipEdgeEvent(tcx, ep, eq, t, p);
                }
            }
            else
            {
                TriangulationPoint newP = NextFlipPoint(ep, eq, ot, op);
                FlipScanEdgeEvent(tcx, ep, eq, t, ot, newP);
                EdgeEvent(tcx, ep, eq, t, p);
            }
        }

        /// <summary>
        /// When we need to traverse from one triangle to the next we need 
        /// the point in current triangle that is the opposite point to the next
        /// triangle. 
        /// </summary>
        private static TriangulationPoint NextFlipPoint(TriangulationPoint ep,
            TriangulationPoint eq, DelaunayTriangle ot, TriangulationPoint op)
        {
            Orientation o2d = TriangulationUtil.Orient2d(eq, op, ep);
            switch (o2d)
            {
                case Orientation.CW: return ot.PointCCWFrom(op);
                case Orientation.CCW: return ot.PointCWFrom(op);
                case Orientation.Collinear:
                    // TODO: implement support for point on constraint edge
                    throw new PointOnEdgeException("Point on constrained edge not supported yet", eq, op, ep);
                default:
                    throw new NotImplementedException("Orientation not handled");
            }
        }

        /// <summary>
        /// After a flip we have two triangles and know that only one will still be
        /// intersecting the edge. So decide which to contiune with and legalize the other
        /// </summary>
        /// <param name="tcx"></param>
        /// <param name="o">should be the result of an TriangulationUtil.orient2d( eq, op, ep )</param>
        /// <param name="t">triangle 1</param>
        /// <param name="ot">triangle 2</param>
        /// <param name="p">a point shared by both triangles</param>
        /// <param name="op">another point shared by both triangles</param>
        /// <returns>returns the triangle still intersecting the edge</returns>
        private static DelaunayTriangle NextFlipTriangle(DTSweepContext tcx, Orientation o, DelaunayTriangle t, DelaunayTriangle ot, TriangulationPoint p, TriangulationPoint op)
        {
            int edgeIndex;
            if (o == Orientation.CCW)
            {
                // ot is not crossing edge after flip
                edgeIndex = ot.FindEdgeIndex(p, op);
                //ot.EdgeIsDelaunay[edgeIndex] = true;
                ot.MarkEdgeDelunay(edgeIndex, true);
                Legalize(tcx, ot);
                ot.ClearAllEdgeDelaunayMarks();
                return t;
            }
            // t is not crossing edge after flip
            edgeIndex = t.FindEdgeIndex(p, op);
            //t.EdgeIsDelaunay[edgeIndex] = true;
            t.MarkEdgeDelunay(edgeIndex, true);
            Legalize(tcx, t);
            t.ClearAllEdgeDelaunayMarks();
            return ot;
        }

        /// <summary>
        /// Scan part of the FlipScan algorithm<br>
        /// When a triangle pair isn't flippable we will scan for the next 
        /// point that is inside the flip triangle scan area. When found 
        /// we generate a new flipEdgeEvent
        /// </summary>
        /// <param name="tcx"></param>
        /// <param name="ep">last point on the edge we are traversing</param>
        /// <param name="eq">first point on the edge we are traversing</param>
        /// <param name="flipTriangle">the current triangle sharing the point eq with edge</param>
        /// <param name="t"></param>
        /// <param name="p"></param>
        static void FlipScanEdgeEvent(DTSweepContext tcx,
          TriangulationPoint ep, TriangulationPoint eq,
          DelaunayTriangle flipTriangle,
          DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle ot;
            TriangulationPoint op;
            ot = t.NeighborAcrossFrom(p);
            op = ot.OppositePoint(t, p);
            if (ot == null)
            {
                // If we want to integrate the fillEdgeEvent do it here
                // With current implementation we should never get here
                throw new Exception("[BUG:FIXME] FLIP failed due to missing triangle");
            }

            if (tcx.IsDebugEnabled)
            {
                Console.WriteLine("[FLIP:SCAN] - scan next point"); // TODO: remove
                tcx.DTDebugContext.PrimaryTriangle = t;
                tcx.DTDebugContext.SecondaryTriangle = ot;
            }

            int index_of_eq = flipTriangle.IndexOf(eq);
            if (TriangulationUtil.InScanArea(eq,
                flipTriangle.PointCCWFrom(index_of_eq),
                flipTriangle.PointCWFrom(index_of_eq), op))
            {
                // flip with new edge op->eq
                FlipEdgeEvent(tcx, eq, op, ot, op);
                // TODO: Actually I just figured out that it should be possible to 
                //       improve this by getting the next ot and op before the the above 
                //       flip and continue the flipScanEdgeEvent here
                // set new ot and op here and loop back to inScanArea test
                // also need to set a new flipTriangle first
                // Turns out at first glance that this is somewhat complicated
                // so it will have to wait.
            }
            else
            {
                TriangulationPoint newP = NextFlipPoint(ep, eq, ot, op);
                FlipScanEdgeEvent(tcx, ep, eq, flipTriangle, ot, newP);
            }
        }

        /// <summary>
        /// Fills holes in the Advancing Front
        /// </summary>
        static void FillAdvancingFront(DTSweepContext tcx, AdvancingFrontNode n)
        {
            AdvancingFrontNode node;
            double angle;
            // Fill right holes
            node = n.Next;
            while (node.HasNext)
            {
                angle = HoleAngle(node);
                if (angle > PI_div2 || angle < -PI_div2) break;
                Fill(tcx, node);
                node = node.Next;
            }

            // Fill left holes
            node = n.Prev;
            while (node.HasPrev)
            {
                angle = HoleAngle(node);
                if (angle > PI_div2 || angle < -PI_div2) break;
                Fill(tcx, node);
                node = node.Prev;
            }

            // Fill right basins
            if (n.HasNext && n.Next.HasNext)
            {
                angle = BasinAngle(n);
                if (angle < PI_3div4) FillBasin(tcx, n);
            }
        }

        /// <summary>
        /// Fills a basin that has formed on the Advancing Front to the right
        /// of given node.<br>
        /// First we decide a left,bottom and right node that forms the 
        /// boundaries of the basin. Then we do a reqursive fill.
        /// </summary>
        /// <param name="tcx"></param>
        /// <param name="node">starting node, this or next node will be left node</param>
        static void FillBasin(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
            {
                // tcx.BasinLeftNode = node.next.next;
                tcx.BasinLeftNode = node;
            }
            else
            {
                tcx.BasinLeftNode = node.Next;
            }

            // Find the bottom and right node
            tcx.BasinBottomNode = tcx.BasinLeftNode;
            while (tcx.BasinBottomNode.HasNext && tcx.BasinBottomNode.Point.Y >= tcx.BasinBottomNode.Next.Point.Y) tcx.BasinBottomNode = tcx.BasinBottomNode.Next;
            if (tcx.BasinBottomNode == tcx.BasinLeftNode) return; // No valid basin
            tcx.BasinRightNode = tcx.BasinBottomNode;
            while (tcx.BasinRightNode.HasNext && tcx.BasinRightNode.Point.Y < tcx.BasinRightNode.Next.Point.Y) tcx.BasinRightNode = tcx.BasinRightNode.Next;
            if (tcx.BasinRightNode == tcx.BasinBottomNode) return; // No valid basins
            tcx.BasinWidth = tcx.BasinRightNode.Point.X - tcx.BasinLeftNode.Point.X;
            tcx.BasinLeftHighest = tcx.BasinLeftNode.Point.Y > tcx.BasinRightNode.Point.Y;
            FillBasinReq(tcx, tcx.BasinBottomNode);
        }

        /// <summary>
        /// Recursive algorithm to fill a Basin with triangles
        /// </summary>
        static void FillBasinReq(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (IsShallow(tcx, node)) return; // if shallow stop filling
            Fill(tcx, node);
            if (node.Prev == tcx.BasinLeftNode && node.Next == tcx.BasinRightNode)
            {
                return;
            }
            else if (node.Prev == tcx.BasinLeftNode)
            {
                Orientation o = TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point);
                if (o == Orientation.CW) return;
                node = node.Next;
            }
            else if (node.Next == tcx.BasinRightNode)
            {
                Orientation o = TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point);
                if (o == Orientation.CCW) return;
                node = node.Prev;
            }
            else
            {
                // Continue with the neighbor node with lowest Y value
                if (node.Prev.Point.Y < node.Next.Point.Y)
                {
                    node = node.Prev;
                }
                else
                {
                    node = node.Next;
                }
            }
            FillBasinReq(tcx, node);
        }

        static bool IsShallow(DTSweepContext tcx, AdvancingFrontNode node)
        {
            double height;
            if (tcx.BasinLeftHighest)
            {
                height = tcx.BasinLeftNode.Point.Y - node.Point.Y;
            }
            else
            {
                height = tcx.BasinRightNode.Point.Y - node.Point.Y;
            }
            if (tcx.BasinWidth > height)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// ???
        /// </summary>
        /// <param name="node">middle node</param>
        /// <returns>the angle between 3 front nodes</returns>
        static double HoleAngle(AdvancingFrontNode node)
        {
            // XXX: do we really need a signed angle for holeAngle?
            //      could possible save some cycles here
            /* Complex plane
             * ab = cosA +i*sinA
             * ab = (ax + ay*i)(bx + by*i) = (ax*bx + ay*by) + i(ax*by-ay*bx)
             * atan2(y,x) computes the principal value of the argument function
             * applied to the complex number x+iy
             * Where x = ax*bx + ay*by
             *       y = ax*by - ay*bx
             */
            double px = node.Point.X;
            double py = node.Point.Y;
            double ax = node.Next.Point.X - px;
            double ay = node.Next.Point.Y - py;
            double bx = node.Prev.Point.X - px;
            double by = node.Prev.Point.Y - py;
            return Math.Atan2(ax * by - ay * bx, ax * bx + ay * by);
        }

        /// <summary>
        /// The basin angle is decided against the horizontal line [1,0]
        /// </summary>
        static double BasinAngle(AdvancingFrontNode node)
        {
            double ax = node.Point.X - node.Next.Next.Point.X;
            double ay = node.Point.Y - node.Next.Next.Point.Y;
            return Math.Atan2(ay, ax);
        }

        /// <summary>
        /// Adds a triangle to the advancing front to fill a hole.
        /// </summary>
        /// <param name="tcx"></param>
        /// <param name="node">middle node, that is the bottom of the hole</param>
        static void Fill(DTSweepContext tcx, AdvancingFrontNode node)
        {
            DelaunayTriangle triangle = new DelaunayTriangle(node.Prev.Point, node.Point, node.Next.Point);
            // TODO: should copy the cEdge value from neighbor triangles
            //       for now cEdge values are copied during the legalize 
            triangle.MarkNeighbor(node.Prev.Triangle);
            triangle.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(triangle);
            // Update the advancing front
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            // tcx.RemoveNode(node);

            // If it was legalized the triangle has already been mapped
            if (!Legalize(tcx, triangle)) tcx.MapTriangleToNodes(triangle);
        }

        /// <summary>
        /// Returns true if triangle was legalized
        /// </summary>
        static bool Legalize(DTSweepContext tcx, DelaunayTriangle t)
        {
            // To legalize a triangle we start by finding if any of the three edges
            // violate the Delaunay condition
            //-----------------------------------------------------------------------


            for (int i = 0; i < 3; i++)
            {
                // TODO: fix so that cEdge is always valid when creating new triangles then we can check it here
                //       instead of below with ot  
                //if (t.EdgeIsDelaunay(i))
                //{
                //    continue;
                //}
                DelaunayTriangle ot = null;
                TriangulationPoint p = null;
                switch (i)
                {
                    case 0:
                    default:
                        {
                            if (t.D0) continue;
                            p = t.P0;
                            ot = t.N0;
                        }
                        break;
                    case 1:
                        {
                            if (t.D1) continue;
                            p = t.P1;
                            ot = t.N1;
                        }
                        break;
                    case 2:
                        {
                            if (t.D2) continue;
                            p = t.P2;
                            ot = t.N2;
                        }
                        break;
                }
                if (ot == null)
                {
                    continue;
                }

                //TriangulationPoint op = ot.OppositePoint(t, p);
                int oi;
                bool ot_EdgeIsConstrained;
                bool ot_EdgeDelunaury;
                TriangulationPoint op = ot.OppositePoint(t, p, i, out oi, out ot_EdgeIsConstrained, out ot_EdgeDelunaury);
                // If this is a Constrained Edge or a Delaunay Edge(only during recursive legalization)
                // then we should not try to legalize           
                if (ot_EdgeIsConstrained || ot_EdgeDelunaury)
                {
                    t.MarkEdgeConstraint(i, ot_EdgeIsConstrained);
                    // XXX: have no good way of setting this property when creating new triangles so lets set it here
                    continue;
                }
                //----------------------------------------------------- 
                //if (!TriangulationUtil.SmartIncircle(p, t.PointCCWFrom(p), t.PointCWFrom(p), op)) continue;
                if (!TriangulationUtil.SmartInCircle(p, t.PointCCWFrom(i), t.PointCWFrom(i), op)) continue;
                //----------------------------------------------------- 

                // Lets mark this shared edge as Delaunay 
                t.MarkEdgeDelunay(i, true);
                ot.MarkEdgeDelunay(oi, true);
                // Lets rotate shared edge one vertex CW to legalize it
                RotateTrianglePair(t, p, ot, op);
                // We now got one valid Delaunay Edge shared by two triangles
                // This gives us 4 new edges to check for Delaunay

                // Make sure that triangle to node mapping is done only one time for a specific triangle
                if (!Legalize(tcx, t)) tcx.MapTriangleToNodes(t);
                if (!Legalize(tcx, ot)) tcx.MapTriangleToNodes(ot);
                // Reset the Delaunay edges, since they only are valid Delaunay edges
                // until we add a new triangle or point.
                // XXX: need to think about this. Can these edges be tried after we 
                //      return to previous recursive level?
                //t.EdgeIsDelaunay[i] = false;
                //ot.EdgeIsDelaunay[oi] = false;
                t.MarkEdgeDelunay(i, false);
                ot.MarkEdgeDelunay(oi, false);
                // If triangle have been legalized no need to check the other edges since
                // the recursive legalization will handles those so we can end here.
                return true;
            }
            return false;
        }

        //----------------
        //backup original
        //private static bool Legalize(DTSweepContext tcx, DelaunayTriangle t)
        //{
        //    // To legalize a triangle we start by finding if any of the three edges
        //    // violate the Delaunay condition
        //    for (int i = 0; i < 3; i++)
        //    {
        //        // TODO: fix so that cEdge is always valid when creating new triangles then we can check it here
        //        //       instead of below with ot
        //        if (t.EdgeIsDelaunay[i]) continue;

        //        DelaunayTriangle ot = t.Neighbors[i];
        //        if (ot == null) continue;

        //        TriangulationPoint p = null;
        //        switch (i)
        //        {
        //            case 0:
        //            default:
        //                {
        //                    p = t.P0;
        //                } break;
        //            case 1:
        //                {
        //                    p = t.P1;
        //                } break;
        //            case 2:
        //                {
        //                    p = t.P2;
        //                } break;
        //        }
        //        //TriangulationPoint p = t.Points[i];
        //        TriangulationPoint op = ot.OppositePoint(t, p);
        //        int oi = ot.IndexOf(op);
        //        // If this is a Constrained Edge or a Delaunay Edge(only during recursive legalization)
        //        // then we should not try to legalize
        //        if (ot.EdgeIsConstrained[oi] || ot.EdgeIsDelaunay[oi])
        //        {
        //            t.EdgeIsConstrained[i] = ot.EdgeIsConstrained[oi]; // XXX: have no good way of setting this property when creating new triangles so lets set it here
        //            continue;
        //        }

        //        if (!TriangulationUtil.SmartIncircle(p, t.PointCCWFrom(p), t.PointCWFrom(p), op)) continue;

        //        // Lets mark this shared edge as Delaunay 
        //        t.EdgeIsDelaunay[i] = true;
        //        ot.EdgeIsDelaunay[oi] = true;

        //        // Lets rotate shared edge one vertex CW to legalize it
        //        RotateTrianglePair(t, p, ot, op);

        //        // We now got one valid Delaunay Edge shared by two triangles
        //        // This gives us 4 new edges to check for Delaunay

        //        // Make sure that triangle to node mapping is done only one time for a specific triangle
        //        if (!Legalize(tcx, t)) tcx.MapTriangleToNodes(t);
        //        if (!Legalize(tcx, ot)) tcx.MapTriangleToNodes(ot);

        //        // Reset the Delaunay edges, since they only are valid Delaunay edges
        //        // until we add a new triangle or point.
        //        // XXX: need to think about this. Can these edges be tried after we 
        //        //      return to previous recursive level?
        //        t.EdgeIsDelaunay[i] = false;
        //        ot.EdgeIsDelaunay[oi] = false;

        //        // If triangle have been legalized no need to check the other edges since
        //        // the recursive legalization will handles those so we can end here.
        //        return true;
        //    }
        //    return false;
        //}


        /// <summary>
        /// Rotates a triangle pair one vertex CW
        ///       n2                    n2
        ///  P +-----+             P +-----+
        ///    | t  /|               |\  t |  
        ///    |   / |               | \   |
        ///  n1|  /  |n3           n1|  \  |n3
        ///    | /   |    after CW   |   \ |
        ///    |/ oT |               | oT \|
        ///    +-----+ oP            +-----+
        ///       n4                    n4
        /// </summary>
        static void RotateTrianglePair(DelaunayTriangle t, TriangulationPoint p, DelaunayTriangle ot, TriangulationPoint op)
        {
            //n1 = t.NeighborCCWFrom(p);
            //n2 = t.NeighborCWFrom(p);          
            //n3 = ot.NeighborCCWFrom(op);
            //n4 = ot.NeighborCWFrom(op);
            //ce1 = t.GetConstrainedEdgeCCW(p);
            //ce2 = t.GetConstrainedEdgeCW(p);
            //ce3 = ot.GetConstrainedEdgeCCW(op);
            //ce4 = ot.GetConstrainedEdgeCW(op);
            //de1 = t.GetDelaunayEdgeCCW(p);
            //de2 = t.GetDelaunayEdgeCW(p);
            //de3 = ot.GetDelaunayEdgeCCW(op);
            //de4 = ot.GetDelaunayEdgeCW(op);

            DelaunayTriangle n1, n2, n3, n4;
            bool ce1, ce2, ce3, ce4;
            bool de1, de2, de3, de4;
            int p_foundAt, op_foundAt;
            t.GetNBs(p, out p_foundAt, out n1, out n2, out ce1, out ce2, out de1, out de2);
            ot.GetNBs(op, out op_foundAt, out n3, out n4, out ce3, out ce4, out de3, out de4);
            int new_p_foundAt;
            t.Legalize(p_foundAt, p, op, out new_p_foundAt);
            int new_op_foundAt;
            ot.Legalize(op_foundAt, op, p, out new_op_foundAt);
            int p_pos_on_ot = ot.IndexOf(p);
            int op_pos_on_ot = new_op_foundAt;//ot.FindIndexOf(op);
            int p_pos_on_t = new_p_foundAt; //t.FindIndexOf(p);
            int op_pos_on_t = t.IndexOf(op);
            //ot.SetDelaunayEdgeCCW(p, de1);
            //ot.SetConstrainedEdgeCCW(p, ce1);
            ot.SetNBCCW(p_pos_on_ot, ce1, de1);
            //ot.SetDelaunayEdgeCW(op, de4);
            //ot.SetConstrainedEdgeCW(op, ce4);
            ot.SetNBCW(op_pos_on_ot, ce4, de4);
            //t.SetDelaunayEdgeCW(p, de2);
            //t.SetConstrainedEdgeCW(p, ce2);
            t.SetNBCW(p_pos_on_t, ce2, de2);
            //t.SetDelaunayEdgeCCW(op, de3);
            //t.SetConstrainedEdgeCCW(op, ce3);
            t.SetNBCCW(op_pos_on_t, ce3, de3);
            // Remap neighbors
            // XXX: might optimize the markNeighbor by keeping track of
            //      what side should be assigned to what neighbor after the 
            //      rotation. Now mark neighbor does lots of testing to find 
            //      the right side.
            //t.Neighbors.Clear();
            //ot.Neighbors.Clear();

            t.ClearAllNBs();
            ot.ClearAllNBs();
            if (n1 != null) ot.MarkNeighbor(n1);
            if (n2 != null) t.MarkNeighbor(n2);
            if (n3 != null) t.MarkNeighbor(n3);
            if (n4 != null) ot.MarkNeighbor(n4);
            t.MarkNeighbor(ot);
        }
    }
}
