/*
** License Applicability. Except to the extent portions of this file are
** made subject to an alternative license as permitted in the SGI Free
** Software License B, Version 1.1 (the "License"), the contents of this
** file are subject only to the provisions of the License. You may not use
** this file except in compliance with the License. You may obtain a copy
** of the License at Silicon Graphics, Inc., attn: Legal Services, 1600
** Amphitheatre Parkway, Mountain View, CA 94043-1351, or at:
** 
** http://oss.sgi.com/projects/FreeB
** 
** Note that, as provided in the License, the Software is distributed on an
** "AS IS" basis, with ALL EXPRESS AND IMPLIED WARRANTIES AND CONDITIONS
** DISCLAIMED, INCLUDING, WITHOUT LIMITATION, ANY IMPLIED WARRANTIES AND
** CONDITIONS OF MERCHANTABILITY, SATISFACTORY QUALITY, FITNESS FOR A
** PARTICULAR PURPOSE, AND NON-INFRINGEMENT.
** 
** Original Code. The Original Code is: OpenGL Sample Implementation,
** Version 1.2.1, released January 26, 2000, developed by Silicon Graphics,
** Inc. The Original Code is Copyright (c) 1991-2000 Silicon Graphics, Inc.
** Copyright in any portions created by third parties is as indicated
** elsewhere herein. All Rights Reserved.
** 
** Additional Notice Provisions: The application programming interfaces
** established by SGI in conjunction with the Original Code are The
** OpenGL(R) Graphics System: A Specification (Version 1.2.1), released
** April 1, 1999; The OpenGL(R) Graphics System Utility Library (Version
** 1.3), released November 4, 1998; and OpenGL(R) Graphics with the X
** Window System(R) (Version 1.3), released October 19, 1998. This software
** was created using the OpenGL(R) version 1.2.1 Sample Implementation
** published by SGI, but has not been independently verified as being
** compliant with the OpenGL(R) version 1.2.1 Specification.
**
*/
/*
** Author: Eric Veach, July 1994.
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
// 2017, SGI (same license as above), WinterDev

**
*/

/* For each pair of adjacent edges crossing the sweep line, there is
 * an ActiveRegion to represent the region between them.  The active
 * regions are kept in sorted order in a dynamic dictionary.  As the
 * sweep line crosses each vertex, we update the affected regions.
 */



using System;
namespace Tesselate
{
    public class ActiveRegion
    {
        HalfEdge _upperHalfEdge;		/* upper edge, directed right to left */
        Dictionary.Node _upperHalfEdgeDictNode;	/* dictionary node corresponding to eUp */
        int _windingNumber;	/* used to determine which regions are
                                 * inside the polygon */
        bool _inside;		/* is this region inside the polygon? */
        bool _sentinel;	/* marks fake edges at t = +/-infinity */
        bool _dirty;		/* marks regions where the upper or lower
                                 * edge has changed, but we haven't checked
                                 * whether they intersect yet */
        bool _fixUpperEdge;	/* marks temporary edges introduced when
                                 * we process a "right vertex" (one without
                                 * any edges leaving to the right) */
        public ActiveRegion()
        {
        }


        /* __gl_computeInterior( tess ) computes the planar arrangement specified
         * by the given contours, and further subdivides this arrangement
         * into regions.  Each region is marked "inside" if it belongs
         * to the polygon, according to the rule given by tess.windingRule.
         * Each interior region is guaranteed be monotone.
         */
        public static int ComputeInterior(Tesselator tess)
        /*
         * __gl_computeInterior( tess ) computes the planar arrangement specified
         * by the given contours, and further subdivides this arrangement
         * into regions.  Each region is marked "inside" if it belongs
         * to the polygon, according to the rule given by tess.windingRule.
         * Each interior region is guaranteed be monotone.
         */
        {
            ContourVertex vertex, vertexNext;
            /* Each vertex defines an currentSweepVertex for our sweep line.  Start by inserting
             * all the vertices in a priority queue.  Events are processed in
             * lexicographic order, ie.
             *
             *	e1 < e2  iff  e1.x < e2.x || (e1.x == e2.x && e1.y < e2.y)
             */
            RemoveDegenerateEdges(tess);
            InitPriorityQue(tess);
            InitEdgeDict(tess);
            while (!tess._vertexPriorityQue.IsEmpty)
            {
                vertex = tess._vertexPriorityQue.DeleteMin();
                for (; ; )
                {
                    if (!tess._vertexPriorityQue.IsEmpty)
                    {
                        vertexNext = tess._vertexPriorityQue.FindMin(); /* __gl_pqSortMinimum */
                    }
                    else
                    {
                        vertexNext = null;
                    }
                    if (vertexNext == null || !vertexNext.Equal2D(vertex)) break;
                    /* Merge together all vertices at exactly the same location.
                     * This is more efficient than processing them one at a time,
                     * simplifies the code (see ConnectLeftDegenerate), and is also
                     * important for correct handling of certain degenerate cases.
                     * For example, suppose there are two identical edges A and B
                     * that belong to different contours (so without this code they would
                     * be processed by separate sweep events).  Suppose another edge C
                     * crosses A and B from above.  When A is processed, we split it
                     * at its intersection point with C.  However this also splits C,
                     * so when we insert B we may compute a slightly different
                     * intersection point.  This might leave two edges with a small
                     * gap between them.  This kind of error is especially obvious
                     * when using boundary extraction (GLU_TESS_BOUNDARY_ONLY).
                     */
                    vertexNext = tess._vertexPriorityQue.DeleteMin(); /* __gl_pqSortExtractMin*/
                    SpliceMergeVertices(tess, vertex._edgeThisIsOriginOf, vertexNext._edgeThisIsOriginOf);
                }
                SweepEvent(tess, vertex);
            }

            /* Set tess.currentSweepVertex for debugging purposes */
            /* __GL_DICTLISTKEY */
            /* __GL_DICTLISTMIN */
            tess.currentSweepVertex = tess._edgeDictionary.GetMinNode().Key._upperHalfEdge._originVertex;
            DoneEdgeDict(tess);
            DonePriorityQ(tess);
            if (!RemoveDegenerateFaces(tess._mesh))
            {
                return 0;
            }


            tess._mesh.CheckMesh();
            return 1;
        }

        /*
         * Invariants for the Edge Dictionary.
         * - each pair of adjacent edges e2=Succ(e1) satisfies EdgeLeq(e1,e2)
         *   at any valid location of the sweep event
         * - if EdgeLeq(e2,e1) as well (at any valid sweep event), then e1 and e2
         *   share a common endpoint
         * - for each e, e.Dst has been processed, but not e.Org
         * - each edge e satisfies VertLeq(e.Dst,currentSweepVertex) && VertLeq(currentSweepVertex,e.Org)
         *   where "currentSweepVertex" is the current sweep line event.
         * - no edge e has zero length
         *
         * Invariants for the Mesh (the processed portion).
         * - the portion of the mesh left of the sweep line is a planar graph,
         *   ie. there is *some* way to embed it in the plane
         * - no processed edge has zero length
         * - no two processed vertices have identical coordinates
         * - each "inside" region is monotone, ie. can be broken into two chains
         *   of monotonically increasing vertices according to VertLeq(v1,v2)
         *   - a non-invariant: these chains may intersect (very slightly)
         *
         * Invariants for the Sweep.
         * - if none of the edges incident to the currentSweepVertex vertex have an activeRegion
         *   (ie. none of these edges are in the edge dictionary), then the vertex
         *   has only right-going edges.
         * - if an edge is marked "fixUpperEdge" (it is a temporary edge introduced
         *   by ConnectRightVertex), then it is the only right-going edge from
         *   its associated vertex.  (This says that these edges exist only
         *   when it is necessary.)
         */

        //#undef	MAX
        //#undef	MIN
        //#define MAX(x,y)	((x) >= (y) ? (x) : (y))
        //#define MIN(x,y)	((x) <= (y) ? (x) : (y))

        /* When we merge two edges into one, we need to compute the combined
         * winding of the new edge.
         */

        static void AddWinding(HalfEdge eDst, HalfEdge eSrc)
        {
            eDst._winding += eSrc._winding;
            eDst._otherHalfOfThisEdge._winding += eSrc._otherHalfOfThisEdge._winding;
        }

        public static bool EdgeLeq(Tesselator tess, ActiveRegion reg1, ActiveRegion reg2)
        /*
         * Both edges must be directed from right to left (this is the canonical
         * direction for the upper edge of each region).
         *
         * The strategy is to evaluate a "t" value for each edge at the
         * current sweep line position, given by tess.currentSweepVertex.  The calculations
         * are designed to be very stable, but of course they are not perfect.
         *
         * Special case: if both edge destinations are at the sweep event,
         * we sort the edges by slope (they would otherwise compare equally).
         */
        {
            ContourVertex currentSweepVertex = tess.currentSweepVertex;
            HalfEdge e1, e2;
            double t1, t2;
            e1 = reg1._upperHalfEdge;
            e2 = reg2._upperHalfEdge;
            if (e1.DirectionVertex == currentSweepVertex)
            {
                if (e2.DirectionVertex == currentSweepVertex)
                {
                    /* Two edges right of the sweep line which meet at the sweep currentSweepVertex.
                     * Sort them by slope.
                     */
                    if (e1._originVertex.VertLeq(e2._originVertex))
                    {
                        return ContourVertex.EdgeSign(e2.DirectionVertex, e1._originVertex, e2._originVertex) <= 0;
                    }
                    return ContourVertex.EdgeSign(e1.DirectionVertex, e2._originVertex, e1._originVertex) >= 0;
                }
                return ContourVertex.EdgeSign(e2.DirectionVertex, currentSweepVertex, e2._originVertex) <= 0;
            }
            if (e2.DirectionVertex == currentSweepVertex)
            {
                return ContourVertex.EdgeSign(e1.DirectionVertex, currentSweepVertex, e1._originVertex) >= 0;
            }

            /* General case - compute signed distance *from* e1, e2 to currentSweepVertex */
            t1 = ContourVertex.EdgeEval(e1.DirectionVertex, currentSweepVertex, e1._originVertex);
            t2 = ContourVertex.EdgeEval(e2.DirectionVertex, currentSweepVertex, e2._originVertex);
            return (t1 >= t2);
        }

        static void DeleteRegion(ActiveRegion reg)
        {
            if (reg._fixUpperEdge)
            {
                /* It was created with zero winding number, so it better be
                 * deleted with zero winding number (ie. it better not get merged
                 * with a real edge).
                 */
                if (reg._upperHalfEdge._winding != 0)
                {
                    throw new System.Exception();
                }
            }
            reg._upperHalfEdge._regionThisIsUpperEdgeOf = null;
            reg._upperHalfEdgeDictNode.Delete();
            reg = null;
        }


        static bool FixUpperEdge(ActiveRegion reg, HalfEdge newEdge)
        /*
         * Replace an upper edge which needs fixing (see ConnectRightVertex).
         */
        {
            if (!reg._fixUpperEdge)
            {
                throw new Exception();
            }
            Mesh.DeleteHalfEdge(reg._upperHalfEdge);
            reg._fixUpperEdge = false;
            reg._upperHalfEdge = newEdge;
            newEdge._regionThisIsUpperEdgeOf = reg;
            return true;
        }

        ActiveRegion RegionAbove()
        {
            return _upperHalfEdgeDictNode.next.Key;
        }

        static ActiveRegion RegionBelow(ActiveRegion r)
        {
            return r._upperHalfEdgeDictNode.prev.Key;
        }

        static ActiveRegion TopLeftRegion(ActiveRegion reg)
        {
            ContourVertex org = reg._upperHalfEdge._originVertex;
            HalfEdge e;
            /* Find the region above the uppermost edge with the same origin */
            do
            {
                reg = reg.RegionAbove();
            } while (reg._upperHalfEdge._originVertex == org);
            /* If the edge above was a temporary edge introduced by ConnectRightVertex,
             * now is the time to fix it.
             */
            if (reg._fixUpperEdge)
            {
                e = Mesh.meshConnect(RegionBelow(reg)._upperHalfEdge._otherHalfOfThisEdge, reg._upperHalfEdge._nextEdgeCCWAroundLeftFace);
                if (e == null)
                {
                    return null;
                }
                if (!FixUpperEdge(reg, e))
                {
                    return null;
                }
                reg = reg.RegionAbove();
            }
            return reg;
        }

        static ActiveRegion TopRightRegion(ActiveRegion reg)
        {
            ContourVertex dst = reg._upperHalfEdge.DirectionVertex;
            /* Find the region above the uppermost edge with the same destination */
            do
            {
                reg = reg.RegionAbove();
            } while (reg._upperHalfEdge.DirectionVertex == dst);
            return reg;
        }

        static ActiveRegion AddRegionBelow(Tesselator tess,
                             ActiveRegion regAbove,
                             HalfEdge eNewUp)
        /*
         * Add a new active region to the sweep line, *somewhere* below "regAbove"
         * (according to where the new edge belongs in the sweep-line dictionary).
         * The upper edge of the new region will be "eNewUp".
         * Winding number and "inside" flag are not updated.
         */
        {
            ActiveRegion regNew = new ActiveRegion();
            regNew._upperHalfEdge = eNewUp;
            /* __gl_dictListInsertBefore */
            regNew._upperHalfEdgeDictNode = tess._edgeDictionary.InsertBefore(regAbove._upperHalfEdgeDictNode, regNew);
            regNew._fixUpperEdge = false;
            regNew._sentinel = false;
            regNew._dirty = false;
            eNewUp._regionThisIsUpperEdgeOf = regNew;
            return regNew;
        }

        static void ComputeWinding(Tesselator tess, ActiveRegion reg)
        {
            reg._windingNumber = reg.RegionAbove()._windingNumber + reg._upperHalfEdge._winding;
            reg._inside = tess.IsWindingInside(reg._windingNumber);
        }


        static void FinishRegion(Tesselator tess, ActiveRegion reg)
        /*
         * Delete a region from the sweep line.  This happens when the upper
         * and lower chains of a region meet (at a vertex on the sweep line).
         * The "inside" flag is copied to the appropriate mesh face (we could
         * not do this before -- since the structure of the mesh is always
         * changing, this face may not have even existed until now).
         */
        {
            HalfEdge e = reg._upperHalfEdge;
            Face f = e._leftFace;
            f._isInterior = reg._inside;
            f._halfEdgeThisIsLeftFaceOf = e;   // optimization for mesh.TessellateMonoRegion()
            DeleteRegion(reg);
        }


        static HalfEdge FinishLeftRegions(Tesselator tess,
                   ActiveRegion regFirst, ActiveRegion regLast)
        /*
         * We are given a vertex with one or more left-going edges.  All affected
         * edges should be in the edge dictionary.  Starting at regFirst.eUp,
         * we walk down deleting all regions where both edges have the same
         * origin vOrg.  At the same time we copy the "inside" flag from the
         * active region to the face, since at this point each face will belong
         * to at most one region (this was not necessarily true until this point
         * in the sweep).  The walk stops at the region above regLast; if regLast
         * is null we walk as far as possible.	At the same time we relink the
         * mesh if necessary, so that the ordering of edges around vOrg is the
         * same as in the dictionary.
         */
        {
            ActiveRegion reg, regPrev;
            HalfEdge e, ePrev;
            regPrev = regFirst;
            ePrev = regFirst._upperHalfEdge;
            while (regPrev != regLast)
            {
                regPrev._fixUpperEdge = false;	/* placement was OK */
                reg = RegionBelow(regPrev);
                e = reg._upperHalfEdge;
                if (e._originVertex != ePrev._originVertex)
                {
                    if (!reg._fixUpperEdge)
                    {
                        /* Remove the last left-going edge.  Even though there are no further
                         * edges in the dictionary with this origin, there may be further
                         * such edges in the mesh (if we are adding left edges to a vertex
                         * that has already been processed).  Thus it is important to call
                         * FinishRegion rather than just DeleteRegion.
                         */
                        FinishRegion(tess, regPrev);
                        break;
                    }
                    /* If the edge below was a temporary edge introduced by
                     * ConnectRightVertex, now is the time to fix it.
                     */
                    e = Mesh.meshConnect(ePrev.Lprev, e._otherHalfOfThisEdge);
                    FixUpperEdge(reg, e);
                }

                /* Relink edges so that ePrev.Onext == e */
                if (ePrev._nextEdgeCCWAroundOrigin != e)
                {
                    Mesh.meshSplice(e.Oprev, e);
                    Mesh.meshSplice(ePrev, e);
                }
                FinishRegion(tess, regPrev);	/* may change reg.eUp */
                ePrev = reg._upperHalfEdge;
                regPrev = reg;
            }
            return ePrev;
        }


        static void AddRightEdges(Tesselator tess, ActiveRegion regUp,
               HalfEdge eFirst, HalfEdge eLast, HalfEdge eTopLeft,
               bool cleanUp)
        /*
         * Purpose: insert right-going edges into the edge dictionary, and update
         * winding numbers and mesh connectivity appropriately.  All right-going
         * edges share a common origin vOrg.  Edges are inserted CCW starting at
         * eFirst; the last edge inserted is eLast.Oprev.  If vOrg has any
         * left-going edges already processed, then eTopLeft must be the edge
         * such that an imaginary upward vertical segment from vOrg would be
         * contained between eTopLeft.Oprev and eTopLeft; otherwise eTopLeft
         * should be null.
         */
        {
            ActiveRegion reg, regPrev;
            HalfEdge e, ePrev;
            bool firstTime = true;
            /* Insert the new right-going edges in the dictionary */
            e = eFirst;
            do
            {
                if (!e._originVertex.VertLeq(e.DirectionVertex))
                {
                    throw new Exception();
                }
                AddRegionBelow(tess, regUp, e._otherHalfOfThisEdge);
                e = e._nextEdgeCCWAroundOrigin;
            } while (e != eLast);
            /* Walk *all* right-going edges from e.Org, in the dictionary order,
             * updating the winding numbers of ea
             * 
             * ch region, and re-linking the mesh
             * edges to match the dictionary ordering (if necessary).
             */
            if (eTopLeft == null)
            {
                eTopLeft = RegionBelow(regUp)._upperHalfEdge.Rprev;
            }
            regPrev = regUp;
            ePrev = eTopLeft;
            for (; ; )
            {
                reg = RegionBelow(regPrev);
                e = reg._upperHalfEdge._otherHalfOfThisEdge;
                if (e._originVertex != ePrev._originVertex) break;
                if (e._nextEdgeCCWAroundOrigin != ePrev)
                {
                    /* Unlink e from its current position, and relink below ePrev */
                    Mesh.meshSplice(e.Oprev, e);
                    Mesh.meshSplice(ePrev.Oprev, e);
                }
                /* Compute the winding number and "inside" flag for the new regions */
                reg._windingNumber = regPrev._windingNumber - e._winding;
                reg._inside = tess.IsWindingInside(reg._windingNumber);
                /* Check for two outgoing edges with same slope -- process these
                 * before any intersection tests (see example in __gl_computeInterior).
                 */
                regPrev._dirty = true;
                if (!firstTime && CheckForRightSplice(tess, regPrev))
                {
                    AddWinding(e, ePrev);
                    DeleteRegion(regPrev);
                    Mesh.DeleteHalfEdge(ePrev);
                }
                firstTime = false;
                regPrev = reg;
                ePrev = e;
            }
            regPrev._dirty = true;
            if (regPrev._windingNumber - e._winding != reg._windingNumber)
            {
                throw new Exception();
            }

            if (cleanUp)
            {
                /* Check for intersections between newly adjacent edges. */
                WalkDirtyRegions(tess, regPrev);
            }
        }


        static void CallCombine(Tesselator tess, ContourVertex intersectionVertex, ref Tesselator.CombineParameters combinePars, bool needed)
        {
            /* Copy coord data in case the callback changes it. */
            double c0 = intersectionVertex._C_0;
            double c1 = intersectionVertex._C_1;
            double c2 = intersectionVertex._C_2;
            intersectionVertex._clientIndex = 0;

            tess.CallCombine(c0, c1, c2, ref combinePars, out intersectionVertex._clientIndex);
            if (intersectionVertex._clientIndex == 0)
            {
                if (!needed)
                {
                    intersectionVertex._clientIndex = combinePars.d0;
                }
                else
                {
                    /* The only fatal error is when two edges are found to intersect,
                     * but the user has not provided the callback necessary to handle
                     * generated intersection points.
                     */
                    throw new Exception("You need to provided a callback to handle generated intersection points.");
                }
            }
        }

        static void SpliceMergeVertices(Tesselator tess, HalfEdge e1, HalfEdge e2)
        /*
         * Two vertices with idential coordinates are combined into one.
         * e1.Org is kept, while e2.Org is discarded.
         */
        {
            //int[] data4 = new int[4];
            //double[] weights4 = new double[] { 0.5f, 0.5f, 0, 0 };
            //data4[0] = e1.originVertex.clientIndex;
            //data4[1] = e2.originVertex.clientIndex;

            var combinePars = new Tesselator.CombineParameters();
            combinePars.w0 = 0.5f; combinePars.w1 = 0.5f;
            combinePars.d0 = e1._originVertex._clientIndex;
            combinePars.d1 = e2._originVertex._clientIndex;

            CallCombine(tess, e1._originVertex, ref combinePars, false);
            Mesh.meshSplice(e1, e2);
        }

        static double VertL1dist(ContourVertex u, ContourVertex v)
        {
            return Math.Abs(u.x - v.x) + Math.Abs(u.y - v.y);
        }


        static void VertexWeights(ContourVertex isect, ContourVertex org, ContourVertex dst, out double weights0, out double weights1)
        /*
         * Find some weights which describe how the intersection vertex is
         * a linear combination of "org" and "dest".  Each of the two edges
         * which generated "isect" is allocated 50% of the weight; each edge
         * splits the weight between its org and dst according to the
         * relative distance to "isect".
         */
        {
            double t1 = VertL1dist(org, isect);
            double t2 = VertL1dist(dst, isect);
            weights0 = 0.5 * t2 / (t1 + t2);
            weights1 = 0.5 * t1 / (t1 + t2);
            isect._C_0 += weights0 * org._C_0 + weights1 * dst._C_0;
            isect._C_1 += weights0 * org._C_1 + weights1 * dst._C_1;
            isect._C_2 += weights0 * org._C_2 + weights1 * dst._C_2;
        }


        static void GetIntersectData(Tesselator tess, ContourVertex isect,
               ContourVertex orgUp, ContourVertex dstUp,
               ContourVertex orgLo, ContourVertex dstLo)
        /*
         * We've computed a new intersection point, now we need a "data" pointer
         * from the user so that we can refer to this new vertex in the
         * rendering callbacks.
         */
        {
            //int[] data4 = new int[4];
            //double[] weights4 = new double[4];
            //data4[0] = orgUp.clientIndex;
            //data4[1] = dstUp.clientIndex;
            //data4[2] = orgLo.clientIndex;
            //data4[3] = dstLo.clientIndex;

            var combinePars = new Tesselator.CombineParameters();
            combinePars.d0 = orgUp._clientIndex;
            combinePars.d1 = dstUp._clientIndex;
            combinePars.d2 = orgLo._clientIndex;
            combinePars.d3 = dstLo._clientIndex;

            isect._C_0 = isect._C_1 = isect._C_2 = 0;
            VertexWeights(isect, orgUp, dstUp, out combinePars.w0, out combinePars.w1);
            VertexWeights(isect, orgLo, dstLo, out combinePars.w2, out combinePars.w3);
            CallCombine(tess, isect, ref combinePars, true);
        }

        static bool CheckForRightSplice(Tesselator tess, ActiveRegion regUp)
        /*
         * Check the upper and lower edge of "regUp", to make sure that the
         * eUp.Org is above eLo, or eLo.Org is below eUp (depending on which
         * origin is leftmost).
         *
         * The main purpose is to splice right-going edges with the same
         * dest vertex and nearly identical slopes (ie. we can't distinguish
         * the slopes numerically).  However the splicing can also help us
         * to recover from numerical errors.  For example, suppose at one
         * point we checked eUp and eLo, and decided that eUp.Org is barely
         * above eLo.  Then later, we split eLo into two edges (eg. from
         * a splice operation like this one).  This can change the result of
         * our test so that now eUp.Org is incident to eLo, or barely below it.
         * We must correct this condition to maintain the dictionary invariants.
         *
         * One possibility is to check these edges for intersection again
         * (ie. CheckForIntersect).  This is what we do if possible.  However
         * CheckForIntersect requires that tess.currentSweepVertex lies between eUp and eLo,
         * so that it has something to fall back on when the intersection
         * calculation gives us an unusable answer.  So, for those cases where
         * we can't check for intersection, this routine fixes the problem
         * by just splicing the offending vertex into the other edge.
         * This is a guaranteed solution, no matter how degenerate things get.
         * Basically this is a combinatorial solution to a numerical problem.
         */
        {
            ActiveRegion regLo = RegionBelow(regUp);
            HalfEdge eUp = regUp._upperHalfEdge;
            HalfEdge eLo = regLo._upperHalfEdge;
            if (eUp._originVertex.VertLeq(eLo._originVertex))
            {
                if (ContourVertex.EdgeSign(eLo.DirectionVertex, eUp._originVertex, eLo._originVertex) > 0)
                {
                    return false;
                }

                /* eUp.Org appears to be below eLo */
                if (!eUp._originVertex.VertEq(eLo._originVertex))
                {
                    /* Splice eUp.Org into eLo */
                    Mesh.meshSplitEdge(eLo._otherHalfOfThisEdge);
                    Mesh.meshSplice(eUp, eLo.Oprev);
                    regUp._dirty = regLo._dirty = true;
                }
                else if (eUp._originVertex != eLo._originVertex)
                {
                    /* merge the two vertices, discarding eUp.Org */
                    tess._vertexPriorityQue.Delete(eUp._originVertex._priorityQueueHandle);
                    //pqDelete(tess.pq, eUp.Org.pqHandle); /* __gl_pqSortDelete */
                    SpliceMergeVertices(tess, eLo.Oprev, eUp);
                }
            }
            else
            {
                if (ContourVertex.EdgeSign(eUp.DirectionVertex, eLo._originVertex, eUp._originVertex) < 0)
                {
                    return false;
                }

                /* eLo.Org appears to be above eUp, so splice eLo.Org into eUp */
                regUp.RegionAbove()._dirty = regUp._dirty = true;
                Mesh.meshSplitEdge(eUp._otherHalfOfThisEdge);
                Mesh.meshSplice(eLo.Oprev, eUp);
            }
            return true;
        }

        static bool CheckForLeftSplice(Tesselator tess, ActiveRegion regUp)
        /*
         * Check the upper and lower edge of "regUp", to make sure that the
         * eUp.Dst is above eLo, or eLo.Dst is below eUp (depending on which
         * destination is rightmost).
         *
         * Theoretically, this should always be true.  However, splitting an edge
         * into two pieces can change the results of previous tests.  For example,
         * suppose at one point we checked eUp and eLo, and decided that eUp.Dst
         * is barely above eLo.  Then later, we split eLo into two edges (eg. from
         * a splice operation like this one).  This can change the result of
         * the test so that now eUp.Dst is incident to eLo, or barely below it.
         * We must correct this condition to maintain the dictionary invariants
         * (otherwise new edges might get inserted in the wrong place in the
         * dictionary, and bad stuff will happen).
         *
         * We fix the problem by just splicing the offending vertex into the
         * other edge.
         */
        {
            ActiveRegion regLo = RegionBelow(regUp);
            HalfEdge eUp = regUp._upperHalfEdge;
            HalfEdge eLo = regLo._upperHalfEdge;
            HalfEdge e;
            if (eUp.DirectionVertex.VertEq(eLo.DirectionVertex))
            {
                throw new Exception();
            }

            if (eUp.DirectionVertex.VertLeq(eLo.DirectionVertex))
            {
                if (ContourVertex.EdgeSign(eUp.DirectionVertex, eLo.DirectionVertex, eUp._originVertex) < 0)
                {
                    return false;
                }

                /* eLo.Dst is above eUp, so splice eLo.Dst into eUp */
                regUp.RegionAbove()._dirty = regUp._dirty = true;
                e = Mesh.meshSplitEdge(eUp);
                Mesh.meshSplice(eLo._otherHalfOfThisEdge, e);
                e._leftFace._isInterior = regUp._inside;
            }
            else
            {
                if (ContourVertex.EdgeSign(eLo.DirectionVertex, eUp.DirectionVertex, eLo._originVertex) > 0) return false;
                /* eUp.Dst is below eLo, so splice eUp.Dst into eLo */
                regUp._dirty = regLo._dirty = true;
                e = Mesh.meshSplitEdge(eLo);
                Mesh.meshSplice(eUp._nextEdgeCCWAroundLeftFace, eLo._otherHalfOfThisEdge);
                e.rightFace._isInterior = regUp._inside;
            }
            return true;
        }

        static void Swap(ref ContourVertex a, ref ContourVertex b)
        {
            ContourVertex t = a;
            a = b;
            b = t;
        }

        /* Given parameters a,x,b,y returns the value (b*x+a*y)/(a+b),
         * or (x+y)/2 if a==b==0.  It requires that a,b >= 0, and enforces
         * this in the rare case that one argument is slightly negative.
         * The implementation is extremely stable numerically.
         * In particular it guarantees that the result r satisfies
         * MIN(x,y) <= r <= MAX(x,y), and the results are very accurate
         * even when a and b differ greatly in magnitude.
         */
        static double Interpolate(double a, double x, double b, double y)
        {
            //return (a = (a < 0) ? 0 : a, b = (b < 0) ? 0 : b,	
            //                ((a <= b) ? ((b == 0) ? ((x+y) / 2)			
            //                : (x + (y-x) * (a/(a+b))))	
            //                : (y + (x-y) * (b/(a+b)))));

            if (a < 0) a = 0;
            if (b < 0) b = 0;
            if (a <= b)
                if (b == 0)
                    return (x + y) / 2;
                else
                    return (x + (y - x) * (a / (a + b)));
            else
                return (y + (x - y) * (b / (a + b)));
        }

        static double TransEval(ContourVertex u, ContourVertex v, ContourVertex w)
        {
            /* Given three vertices u,v,w such that TransLeq(u,v) && TransLeq(v,w),
             * evaluates the t-coord of the edge uw at the s-coord of the vertex v.
             * Returns v.s - (uw)(v.t), ie. the signed distance from uw to v.
             * If uw is vertical (and thus passes thru v), the result is zero.
             *
             * The calculation is extremely accurate and stable, even when v
             * is very close to u or w.  In particular if we set v.s = 0 and
             * let r be the negated result (this evaluates (uw)(v.t)), then
             * r is guaranteed to satisfy MIN(u.s,w.s) <= r <= MAX(u.s,w.s).
             */
            double gapL, gapR;
            if (!u.TransLeq(v) || !v.TransLeq(w))
            {
                throw new Exception();
            }

            gapL = v.y - u.y;
            gapR = w.y - v.y;
            if (gapL + gapR > 0)
            {
                if (gapL < gapR)
                {
                    return (v.x - u.x) + (u.x - w.x) * (gapL / (gapL + gapR));
                }
                else
                {
                    return (v.x - w.x) + (w.x - u.x) * (gapR / (gapL + gapR));
                }
            }
            /* vertical line */
            return 0;
        }

        static double TransSign(ContourVertex u, ContourVertex v, ContourVertex w)
        {
            /* Returns a number whose sign matches TransEval(u,v,w) but which
             * is cheaper to evaluate.  Returns > 0, == 0 , or < 0
             * as v is above, on, or below the edge uw.
             */
            double gapL, gapR;
            if (!u.TransLeq(v) || !v.TransLeq(w))
            {
                throw new Exception();
            }

            gapL = v.y - u.y;
            gapR = w.y - v.y;
            if (gapL + gapR > 0)
            {
                return (v.x - w.x) * gapL + (v.x - u.x) * gapR;
            }
            /* vertical line */
            return 0;
        }

        static void EdgeIntersect(ContourVertex o1, ContourVertex d1,
            ContourVertex o2, ContourVertex d2,
            ref ContourVertex v)
        /* Given edges (o1,d1) and (o2,d2), compute their point of intersection.
         * The computed point is guaranteed to lie in the intersection of the
         * bounding rectangles defined by each edge.
         */
        {
            double z1, z2;
            /* This is certainly not the most efficient way to find the intersection
             * of two line segments, but it is very numerically stable.
             *
             * Strategy: find the two middle vertices in the VertLeq ordering,
             * and interpolate the intersection s-value from these.  Then repeat
             * using the TransLeq ordering to find the intersection t-value.
             */

            if (!o1.VertLeq(d1)) { Swap(ref o1, ref d1); }
            if (!o2.VertLeq(d2)) { Swap(ref o2, ref d2); }
            if (!o1.VertLeq(o2)) { Swap(ref o1, ref o2); Swap(ref d1, ref d2); }

            if (!o2.VertLeq(d1))
            {
                /* Technically, no intersection -- do our best */
                v.x = (o2.x + d1.x) / 2;
            }
            else if (d1.VertLeq(d2))
            {
                /* Interpolate between o2 and d1 */
                z1 = ContourVertex.EdgeEval(o1, o2, d1);
                z2 = ContourVertex.EdgeEval(o2, d1, d2);
                if (z1 + z2 < 0) { z1 = -z1; z2 = -z2; }
                v.x = Interpolate(z1, o2.x, z2, d1.x);
            }
            else
            {
                /* Interpolate between o2 and d2 */
                z1 = ContourVertex.EdgeSign(o1, o2, d1);
                z2 = -ContourVertex.EdgeSign(o1, d2, d1);
                if (z1 + z2 < 0) { z1 = -z1; z2 = -z2; }
                v.x = Interpolate(z1, o2.x, z2, d2.x);
            }

            /* Now repeat the process for t */

            if (!o1.TransLeq(d1)) { Swap(ref o1, ref d1); }
            if (!o2.TransLeq(d2)) { Swap(ref o2, ref d2); }
            if (!o1.TransLeq(o2)) { Swap(ref o1, ref o2); Swap(ref d1, ref d2); }

            if (!o2.TransLeq(d1))
            {
                /* Technically, no intersection -- do our best */
                v.y = (o2.y + d1.y) / 2;
            }
            else if (d1.TransLeq(d2))
            {
                /* Interpolate between o2 and d1 */
                z1 = TransEval(o1, o2, d1);
                z2 = TransEval(o2, d1, d2);
                if (z1 + z2 < 0) { z1 = -z1; z2 = -z2; }
                v.y = Interpolate(z1, o2.y, z2, d1.y);
            }
            else
            {
                /* Interpolate between o2 and d2 */
                z1 = TransSign(o1, o2, d1);
                z2 = -TransSign(o1, d2, d1);
                if (z1 + z2 < 0) { z1 = -z1; z2 = -z2; }
                v.y = Interpolate(z1, o2.y, z2, d2.y);
            }
        }

        static bool CheckForIntersect(Tesselator tess, ActiveRegion regUp)
        /*
         * Check the upper and lower edges of the given region to see if
         * they intersect.  If so, create the intersection and add it
         * to the data structures.
         *
         * Returns true if adding the new intersection resulted in a recursive
         * call to AddRightEdges(); in this case all "dirty" regions have been
         * checked for intersections, and possibly regUp has been deleted.
         */
        {
            ActiveRegion regLo = RegionBelow(regUp);
            HalfEdge eUp = regUp._upperHalfEdge;
            HalfEdge eLo = regLo._upperHalfEdge;
            ContourVertex orgUp = eUp._originVertex;
            ContourVertex orgLo = eLo._originVertex;
            ContourVertex dstUp = eUp.DirectionVertex;
            ContourVertex dstLo = eLo.DirectionVertex;
            double tMinUp, tMaxLo;
            ContourVertex isect = new ContourVertex();
            ContourVertex orgMin;
            HalfEdge e;
            if (dstLo.VertEq(dstUp))
            {
                throw new Exception();
            }
            if (ContourVertex.EdgeSign(dstUp, tess.currentSweepVertex, orgUp) > 0)
            {
                throw new Exception();
            }
            if (ContourVertex.EdgeSign(dstLo, tess.currentSweepVertex, orgLo) < 0)
            {
                throw new Exception();
            }
            if (orgUp == tess.currentSweepVertex || orgLo == tess.currentSweepVertex)
            {
                throw new Exception();
            }
            if (regUp._fixUpperEdge || regLo._fixUpperEdge)
            {
                throw new Exception();
            }

            if (orgUp == orgLo)
            {
                return false;	/* right endpoints are the same */
            }

            tMinUp = Math.Min(orgUp.y, dstUp.y);
            tMaxLo = Math.Max(orgLo.y, dstLo.y);
            if (tMinUp > tMaxLo)
            {
                return false;	/* t ranges do not overlap */
            }

            if (orgUp.VertLeq(orgLo))
            {
                if (ContourVertex.EdgeSign(dstLo, orgUp, orgLo) > 0)
                {
                    return false;
                }
            }
            else
            {
                if (ContourVertex.EdgeSign(dstUp, orgLo, orgUp) < 0)
                {
                    return false;
                }
            }

            EdgeIntersect(dstUp, orgUp, dstLo, orgLo, ref isect);
            // The following properties are guaranteed:
            if (!(Math.Min(orgUp.y, dstUp.y) <= isect.y))
            {
                throw new System.Exception();
            }
            if (!(isect.y <= Math.Max(orgLo.y, dstLo.y)))
            {
                throw new System.Exception();
            }
            if (!(Math.Min(dstLo.x, dstUp.x) <= isect.x))
            {
                throw new System.Exception();
            }
            if (!(isect.x <= Math.Max(orgLo.x, orgUp.x)))
            {
                throw new System.Exception();
            }

            if (isect.VertLeq(tess.currentSweepVertex))
            {
                /* The intersection point lies slightly to the left of the sweep line,
                 * so move it until it''s slightly to the right of the sweep line.
                 * (If we had perfect numerical precision, this would never happen
                 * in the first place).  The easiest and safest thing to do is
                 * replace the intersection by tess.currentSweepVertex.
                 */
                isect.x = tess.currentSweepVertex.x;
                isect.y = tess.currentSweepVertex.y;
            }
            /* Similarly, if the computed intersection lies to the right of the
             * rightmost origin (which should rarely happen), it can cause
             * unbelievable inefficiency on sufficiently degenerate inputs.
             * (If you have the test program, try running test54.d with the
             * "X zoom" option turned on).
             */
            orgMin = orgUp.VertLeq(orgLo) ? orgUp : orgLo;
            if (orgMin.VertLeq(isect))
            {
                isect.x = orgMin.x;
                isect.y = orgMin.y;
            }

            if (isect.VertEq(orgUp) || isect.VertEq(orgLo))
            {
                /* Easy case -- intersection at one of the right endpoints */
                CheckForRightSplice(tess, regUp);
                return false;
            }

            if ((!dstUp.VertEq(tess.currentSweepVertex)
                && ContourVertex.EdgeSign(dstUp, tess.currentSweepVertex, isect) >= 0)
                || (!dstLo.VertEq(tess.currentSweepVertex)
                && ContourVertex.EdgeSign(dstLo, tess.currentSweepVertex, isect) <= 0))
            {
                /* Very unusual -- the new upper or lower edge would pass on the
                 * wrong side of the sweep currentSweepVertex, or through it.  This can happen
                 * due to very small numerical errors in the intersection calculation.
                 */
                if (dstLo == tess.currentSweepVertex)
                {
                    /* Splice dstLo into eUp, and process the new region(s) */
                    Mesh.meshSplitEdge(eUp._otherHalfOfThisEdge);
                    Mesh.meshSplice(eLo._otherHalfOfThisEdge, eUp);
                    regUp = TopLeftRegion(regUp);
                    eUp = RegionBelow(regUp)._upperHalfEdge;
                    FinishLeftRegions(tess, RegionBelow(regUp), regLo);
                    AddRightEdges(tess, regUp, eUp.Oprev, eUp, eUp, true);
                    return true;
                }
                if (dstUp == tess.currentSweepVertex)
                {
                    /* Splice dstUp into eLo, and process the new region(s) */
                    Mesh.meshSplitEdge(eLo._otherHalfOfThisEdge);
                    Mesh.meshSplice(eUp._nextEdgeCCWAroundLeftFace, eLo.Oprev);
                    regLo = regUp;
                    regUp = TopRightRegion(regUp);
                    e = RegionBelow(regUp)._upperHalfEdge.Rprev;
                    regLo._upperHalfEdge = eLo.Oprev;
                    eLo = FinishLeftRegions(tess, regLo, null);
                    AddRightEdges(tess, regUp, eLo._nextEdgeCCWAroundOrigin, eUp.Rprev, e, true);
                    return true;
                }

                /* Special case: called from ConnectRightVertex.  If either
                 * edge passes on the wrong side of tess.currentSweepVertex, split it
                 * (and wait for ConnectRightVertex to splice it appropriately).
                 */
                if (ContourVertex.EdgeSign(dstUp, tess.currentSweepVertex, isect) >= 0)
                {
                    regUp.RegionAbove()._dirty = regUp._dirty = true;
                    Mesh.meshSplitEdge(eUp._otherHalfOfThisEdge);
                    eUp._originVertex.x = tess.currentSweepVertex.x;
                    eUp._originVertex.y = tess.currentSweepVertex.y;
                }
                if (ContourVertex.EdgeSign(dstLo, tess.currentSweepVertex, isect) <= 0)
                {
                    regUp._dirty = regLo._dirty = true;
                    Mesh.meshSplitEdge(eLo._otherHalfOfThisEdge);
                    eLo._originVertex.x = tess.currentSweepVertex.x;
                    eLo._originVertex.y = tess.currentSweepVertex.y;
                }
                /* leave the rest for ConnectRightVertex */
                return false;
            }

            /* General case -- split both edges, splice into new vertex.
             * When we do the splice operation, the order of the arguments is
             * arbitrary as far as correctness goes.  However, when the operation
             * creates a new face, the work done is proportional to the size of
             * the new face.  We expect the faces in the processed part of
             * the mesh (ie. eUp.Lface) to be smaller than the faces in the
             * unprocessed original contours (which will be eLo.Oprev.Lface).
             */
            Mesh.meshSplitEdge(eUp._otherHalfOfThisEdge);
            Mesh.meshSplitEdge(eLo._otherHalfOfThisEdge);
            Mesh.meshSplice(eLo.Oprev, eUp);
            eUp._originVertex.x = isect.x;
            eUp._originVertex.y = isect.y;
            tess._vertexPriorityQue.Add(out eUp._originVertex._priorityQueueHandle, eUp._originVertex); /* __gl_pqSortInsert */
            GetIntersectData(tess, eUp._originVertex, orgUp, dstUp, orgLo, dstLo);
            regUp.RegionAbove()._dirty = regUp._dirty = regLo._dirty = true;
            return false;
        }

        static void WalkDirtyRegions(Tesselator tess, ActiveRegion regUp)
        /*
         * When the upper or lower edge of any region changes, the region is
         * marked "dirty".  This routine walks through all the dirty regions
         * and makes sure that the dictionary invariants are satisfied
         * (see the comments at the beginning of this file).  Of course
         * new dirty regions can be created as we make changes to restore
         * the invariants.
         */
        {
            ActiveRegion regLo = RegionBelow(regUp);
            HalfEdge eUp, eLo;
            for (; ; )
            {
                /* Find the lowest dirty region (we walk from the bottom up). */
                while (regLo._dirty)
                {
                    regUp = regLo;
                    regLo = RegionBelow(regLo);
                }
                if (!regUp._dirty)
                {
                    regLo = regUp;
                    regUp = regUp.RegionAbove();
                    if (regUp == null || !regUp._dirty)
                    {
                        /* We've walked all the dirty regions */
                        return;
                    }
                }
                regUp._dirty = false;
                eUp = regUp._upperHalfEdge;
                eLo = regLo._upperHalfEdge;
                if (eUp.DirectionVertex != eLo.DirectionVertex)
                {
                    /* Check that the edge ordering is obeyed at the Dst vertices. */
                    if (CheckForLeftSplice(tess, regUp))
                    {
                        /* If the upper or lower edge was marked fixUpperEdge, then
                         * we no longer need it (since these edges are needed only for
                         * vertices which otherwise have no right-going edges).
                         */
                        if (regLo._fixUpperEdge)
                        {
                            DeleteRegion(regLo);
                            Mesh.DeleteHalfEdge(eLo);
                            regLo = RegionBelow(regUp);
                            eLo = regLo._upperHalfEdge;
                        }
                        else if (regUp._fixUpperEdge)
                        {
                            DeleteRegion(regUp);
                            Mesh.DeleteHalfEdge(eUp);
                            regUp = regLo.RegionAbove();
                            eUp = regUp._upperHalfEdge;
                        }
                    }
                }
                if (eUp._originVertex != eLo._originVertex)
                {
                    if (eUp.DirectionVertex != eLo.DirectionVertex
                    && !regUp._fixUpperEdge && !regLo._fixUpperEdge
                    && (eUp.DirectionVertex == tess.currentSweepVertex || eLo.DirectionVertex == tess.currentSweepVertex))
                    {
                        /* When all else fails in CheckForIntersect(), it uses tess.currentSweepVertex
                         * as the intersection location.  To make this possible, it requires
                         * that tess.currentSweepVertex lie between the upper and lower edges, and also
                         * that neither of these is marked fixUpperEdge (since in the worst
                         * case it might splice one of these edges into tess.currentSweepVertex, and
                         * violate the invariant that fixable edges are the only right-going
                         * edge from their associated vertex).
                         */
                        if (CheckForIntersect(tess, regUp))
                        {
                            /* WalkDirtyRegions() was called recursively; we're done */
                            return;
                        }
                    }
                    else
                    {
                        /* Even though we can't use CheckForIntersect(), the Org vertices
                         * may violate the dictionary edge ordering.  Check and correct this.
                         */
                        CheckForRightSplice(tess, regUp);
                    }
                }
                if (eUp._originVertex == eLo._originVertex && eUp.DirectionVertex == eLo.DirectionVertex)
                {
                    /* A degenerate loop consisting of only two edges -- delete it. */
                    AddWinding(eLo, eUp);
                    DeleteRegion(regUp);
                    Mesh.DeleteHalfEdge(eUp);
                    regUp = regLo.RegionAbove();
                }
            }
        }


        static void ConnectRightVertex(Tesselator tess, ActiveRegion regUp, HalfEdge eBottomLeft)
        /*
         * Purpose: connect a "right" vertex vEvent (one where all edges go left)
         * to the unprocessed portion of the mesh.  Since there are no right-going
         * edges, two regions (one above vEvent and one below) are being merged
         * into one.  "regUp" is the upper of these two regions.
         *
         * There are two reasons for doing this (adding a right-going edge):
         *  - if the two regions being merged are "inside", we must add an edge
         *    to keep them separated (the combined region would not be monotone).
         *  - in any case, we must leave some record of vEvent in the dictionary,
         *    so that we can merge vEvent with features that we have not seen yet.
         *    For example, maybe there is a vertical edge which passes just to
         *    the right of vEvent; we would like to splice vEvent into this edge.
         *
         * However, we don't want to connect vEvent to just any vertex.  We don''t
         * want the new edge to cross any other edges; otherwise we will create
         * intersection vertices even when the input data had no self-intersections.
         * (This is a bad thing; if the user's input data has no intersections,
         * we don't want to generate any false intersections ourselves.)
         *
         * Our eventual goal is to connect vEvent to the leftmost unprocessed
         * vertex of the combined region (the union of regUp and regLo).
         * But because of unseen vertices with all right-going edges, and also
         * new vertices which may be created by edge intersections, we don''t
         * know where that leftmost unprocessed vertex is.  In the meantime, we
         * connect vEvent to the closest vertex of either chain, and mark the region
         * as "fixUpperEdge".  This flag says to delete and reconnect this edge
         * to the next processed vertex on the boundary of the combined region.
         * Quite possibly the vertex we connected to will turn out to be the
         * closest one, in which case we won''t need to make any changes.
         */
        {
            HalfEdge eNew;
            HalfEdge eTopLeft = eBottomLeft._nextEdgeCCWAroundOrigin;
            ActiveRegion regLo = RegionBelow(regUp);
            HalfEdge eUp = regUp._upperHalfEdge;
            HalfEdge eLo = regLo._upperHalfEdge;
            bool degenerate = false;
            if (eUp.DirectionVertex != eLo.DirectionVertex)
            {
                CheckForIntersect(tess, regUp);
            }

            /* Possible new degeneracies: upper or lower edge of regUp may pass
             * through vEvent, or may coincide with new intersection vertex
             */
            if (eUp._originVertex.VertEq(tess.currentSweepVertex))
            {
                Mesh.meshSplice(eTopLeft.Oprev, eUp);
                regUp = TopLeftRegion(regUp);
                eTopLeft = RegionBelow(regUp)._upperHalfEdge;
                FinishLeftRegions(tess, RegionBelow(regUp), regLo);
                degenerate = true;
            }
            if (eLo._originVertex.VertEq(tess.currentSweepVertex))
            {
                Mesh.meshSplice(eBottomLeft, eLo.Oprev);
                eBottomLeft = FinishLeftRegions(tess, regLo, null);
                degenerate = true;
            }
            if (degenerate)
            {
                AddRightEdges(tess, regUp, eBottomLeft._nextEdgeCCWAroundOrigin, eTopLeft, eTopLeft, true);
                return;
            }

            /* Non-degenerate situation -- need to add a temporary, fixable edge.
             * Connect to the closer of eLo.Org, eUp.Org.
             */
            if (eLo._originVertex.VertLeq(eUp._originVertex))
            {
                eNew = eLo.Oprev;
            }
            else
            {
                eNew = eUp;
            }
            eNew = Mesh.meshConnect(eBottomLeft.Lprev, eNew);
            /* Prevent cleanup, otherwise eNew might disappear before we've even
             * had a chance to mark it as a temporary edge.
             */
            AddRightEdges(tess, regUp, eNew, eNew._nextEdgeCCWAroundOrigin, eNew._nextEdgeCCWAroundOrigin, false);
            eNew._otherHalfOfThisEdge._regionThisIsUpperEdgeOf._fixUpperEdge = true;
            WalkDirtyRegions(tess, regUp);
        }

        static void ConnectLeftDegenerate(Tesselator tess, ActiveRegion regUp, ContourVertex vEvent)
        /*
         * The currentSweepVertex vertex lies exactly on an already-processed edge or vertex.
         * Adding the new vertex involves splicing it into the already-processed
         * part of the mesh.
         */
        {
            HalfEdge e, eTopLeft, eTopRight, eLast;
            ActiveRegion reg;
            e = regUp._upperHalfEdge;
            if (e._originVertex.VertEq(vEvent))
            {
                /* e.Org is an unprocessed vertex - just combine them, and wait
                 * for e.Org to be pulled from the queue
                 */
                SpliceMergeVertices(tess, e, vEvent._edgeThisIsOriginOf);
                return;
            }

            if (!e.DirectionVertex.VertEq(vEvent))
            {
                /* General case -- splice vEvent into edge e which passes through it */
                Mesh.meshSplitEdge(e._otherHalfOfThisEdge);
                if (regUp._fixUpperEdge)
                {
                    /* This edge was fixable -- delete unused portion of original edge */
                    Mesh.DeleteHalfEdge(e._nextEdgeCCWAroundOrigin);
                    regUp._fixUpperEdge = false;
                }
                Mesh.meshSplice(vEvent._edgeThisIsOriginOf, e);
                SweepEvent(tess, vEvent); /* recurse */
                return;
            }

            /* vEvent coincides with e.Dst, which has already been processed.
             * Splice in the additional right-going edges.
             */
            regUp = TopRightRegion(regUp);
            reg = RegionBelow(regUp);
            eTopRight = reg._upperHalfEdge._otherHalfOfThisEdge;
            eTopLeft = eLast = eTopRight._nextEdgeCCWAroundOrigin;
            if (reg._fixUpperEdge)
            {
                /* Here e.Dst has only a single fixable edge going right.
                 * We can delete it since now we have some real right-going edges.
                 */
                if (eTopLeft == eTopRight)
                {
                    throw new Exception();   /* there are some left edges too */
                }
                DeleteRegion(reg);
                Mesh.DeleteHalfEdge(eTopRight);
                eTopRight = eTopLeft.Oprev;
            }
            Mesh.meshSplice(vEvent._edgeThisIsOriginOf, eTopRight);
            if (!eTopLeft.EdgeGoesLeft())
            {
                /* e.Dst had no left-going edges -- indicate this to AddRightEdges() */
                eTopLeft = null;
            }
            AddRightEdges(tess, regUp, eTopRight._nextEdgeCCWAroundOrigin, eLast, eTopLeft, true);
        }

        static void ConnectLeftVertex(Tesselator tess, ContourVertex vEvent)
        /*
         * Purpose: connect a "left" vertex (one where both edges go right)
         * to the processed portion of the mesh.  Let R be the active region
         * containing vEvent, and let U and L be the upper and lower edge
         * chains of R.  There are two possibilities:
         *
         * - the normal case: split R into two regions, by connecting vEvent to
         *   the rightmost vertex of U or L lying to the left of the sweep line
         *
         * - the degenerate case: if vEvent is close enough to U or L, we
         *   merge vEvent into that edge chain.  The sub-cases are:
         *	- merging with the rightmost vertex of U or L
         *	- merging with the active edge of U or L
         *	- merging with an already-processed portion of U or L
         */
        {
            ActiveRegion regUp, regLo, reg;
            HalfEdge eUp, eLo, eNew;
            ActiveRegion tmp = new ActiveRegion();
            /* assert( vEvent.anEdge.Onext.Onext == vEvent.anEdge ); */

            /* Get a pointer to the active region containing vEvent */
            tmp._upperHalfEdge = vEvent._edgeThisIsOriginOf._otherHalfOfThisEdge;
            /* __GL_DICTLISTKEY */
            /* __gl_dictListSearch */
            regUp = Dictionary.dictSearch(tess._edgeDictionary, tmp).Key;
            regLo = RegionBelow(regUp);
            eUp = regUp._upperHalfEdge;
            eLo = regLo._upperHalfEdge;
            /* Try merging with U or L first */
            if (ContourVertex.EdgeSign(eUp.DirectionVertex, vEvent, eUp._originVertex) == 0)
            {
                ConnectLeftDegenerate(tess, regUp, vEvent);
                return;
            }

            /* Connect vEvent to rightmost processed vertex of either chain.
             * e.Dst is the vertex that we will connect to vEvent.
             */
            reg = eLo.DirectionVertex.VertLeq(eUp.DirectionVertex) ? regUp : regLo;
            if (regUp._inside || reg._fixUpperEdge)
            {
                if (reg == regUp)
                {
                    eNew = Mesh.meshConnect(vEvent._edgeThisIsOriginOf._otherHalfOfThisEdge, eUp._nextEdgeCCWAroundLeftFace);
                }
                else
                {
                    HalfEdge tempHalfEdge = Mesh.meshConnect(eLo.Dnext, vEvent._edgeThisIsOriginOf);
                    eNew = tempHalfEdge._otherHalfOfThisEdge;
                }
                if (reg._fixUpperEdge)
                {
                    FixUpperEdge(reg, eNew);
                }
                else
                {
                    ComputeWinding(tess, AddRegionBelow(tess, regUp, eNew));
                }
                SweepEvent(tess, vEvent);
            }
            else
            {
                /* The new vertex is in a region which does not belong to the polygon.
                 * We don''t need to connect this vertex to the rest of the mesh.
                 */
                AddRightEdges(tess, regUp, vEvent._edgeThisIsOriginOf, vEvent._edgeThisIsOriginOf, null, true);
            }
        }


        static void SweepEvent(Tesselator tess, ContourVertex vEvent)
        /*
         * Does everything necessary when the sweep line crosses a vertex.
         * Updates the mesh and the edge dictionary.
         */
        {
            ActiveRegion regUp, reg;
            HalfEdge e, eTopLeft, eBottomLeft;
            tess.currentSweepVertex = vEvent; 	/* for access in EdgeLeq() */
            /* Check if this vertex is the right endpoint of an edge that is
             * already in the dictionary.  In this case we don't need to waste
             * time searching for the location to insert new edges.
             */
            e = vEvent._edgeThisIsOriginOf;
            while (e._regionThisIsUpperEdgeOf == null)
            {
                e = e._nextEdgeCCWAroundOrigin;
                if (e == vEvent._edgeThisIsOriginOf)
                {
                    /* All edges go right -- not incident to any processed edges */
                    ConnectLeftVertex(tess, vEvent);
                    return;
                }
            }

            /* Processing consists of two phases: first we "finish" all the
             * active regions where both the upper and lower edges terminate
             * at vEvent (ie. vEvent is closing off these regions).
             * We mark these faces "inside" or "outside" the polygon according
             * to their winding number, and delete the edges from the dictionary.
             * This takes care of all the left-going edges from vEvent.
             */
            regUp = TopLeftRegion(e._regionThisIsUpperEdgeOf);
            reg = RegionBelow(regUp);
            eTopLeft = reg._upperHalfEdge;
            eBottomLeft = FinishLeftRegions(tess, reg, null);
            /* Next we process all the right-going edges from vEvent.  This
             * involves adding the edges to the dictionary, and creating the
             * associated "active regions" which record information about the
             * regions between adjacent dictionary edges.
             */
            if (eBottomLeft._nextEdgeCCWAroundOrigin == eTopLeft)
            {
                /* No right-going edges -- add a temporary "fixable" edge */
                ConnectRightVertex(tess, regUp, eBottomLeft);
            }
            else
            {
                AddRightEdges(tess, regUp, eBottomLeft._nextEdgeCCWAroundOrigin, eTopLeft, eTopLeft, true);
            }
        }


        /* Make the sentinel coordinates big enough that they will never be
         * merged with real input features.  (Even with the largest possible
         * input contour and the maximum tolerance of 1.0, no merging will be
         * done with coordinates larger than 3 * GLU_TESS_MAX_COORD).
         */
        const double SENTINEL_COORD = (4 * Tesselator.MAX_COORD);
        static void AddSentinel(Tesselator tess, double t)
        /*
         * We add two sentinel edges above and below all other edges,
         * to avoid special cases at the top and bottom.
         */
        {
            HalfEdge halfEdge;
            ActiveRegion activeRedion = new ActiveRegion();
            halfEdge = tess._mesh.MakeEdge();
            halfEdge._originVertex.x = SENTINEL_COORD;
            halfEdge._originVertex.y = t;
            halfEdge.DirectionVertex.x = -SENTINEL_COORD;
            halfEdge.DirectionVertex.y = t;
            tess.currentSweepVertex = halfEdge.DirectionVertex; 	/* initialize it */
            activeRedion._upperHalfEdge = halfEdge;
            activeRedion._windingNumber = 0;
            activeRedion._inside = false;
            activeRedion._fixUpperEdge = false;
            activeRedion._sentinel = true;
            activeRedion._dirty = false;
            activeRedion._upperHalfEdgeDictNode = tess._edgeDictionary.Insert(activeRedion); /* __gl_dictListInsertBefore */
        }


        static void InitEdgeDict(Tesselator tess)
        /*
         * We maintain an ordering of edge intersections with the sweep line.
         * This order is maintained in a dynamic dictionary.
         */
        {
            /* __gl_dictListNewDict */
            tess._edgeDictionary = new Dictionary(tess);
            AddSentinel(tess, -SENTINEL_COORD);
            AddSentinel(tess, SENTINEL_COORD);
        }


        static void DoneEdgeDict(Tesselator tess)
        {
            ActiveRegion reg;
            int fixedEdges = 0;
            /* __GL_DICTLISTKEY */
            /* __GL_DICTLISTMIN */
            while ((reg = tess._edgeDictionary.GetMinNode().Key) != null)
            {
                /*
                 * At the end of all processing, the dictionary should contain
                 * only the two sentinel edges, plus at most one "fixable" edge
                 * created by ConnectRightVertex().
                 */
                if (!reg._sentinel)
                {
                    if (!reg._fixUpperEdge)
                    {
                        throw new System.Exception();
                    }
                    if (++fixedEdges != 1)
                    {
                        throw new System.Exception();
                    }
                }
                if (reg._windingNumber != 0)
                {
                    throw new Exception();
                }
                DeleteRegion(reg);
            }
            tess._edgeDictionary = null;
        }


        static void RemoveDegenerateEdges(Tesselator tess)
        {
            // Remove zero-length edges, and contours with fewer than 3 vertices.
            HalfEdge edgeHead = tess._mesh._halfEdgeHead;
            HalfEdge nextHalfEdge;
            for (HalfEdge currentEdge = edgeHead._nextHalfEdge; currentEdge != edgeHead; currentEdge = nextHalfEdge)
            {
                nextHalfEdge = currentEdge._nextHalfEdge;
                HalfEdge nextEdgeCCWAroundLeftFace = currentEdge._nextEdgeCCWAroundLeftFace;
                if (currentEdge._originVertex.VertEq(currentEdge.DirectionVertex)
                    && currentEdge._nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundLeftFace != currentEdge)
                {
                    // Zero-length edge, contour has at least 3 edges
                    SpliceMergeVertices(tess, nextEdgeCCWAroundLeftFace, currentEdge);	/* deletes e.Org */
                    Mesh.DeleteHalfEdge(currentEdge); /* e is a self-loop */
                    currentEdge = nextEdgeCCWAroundLeftFace;
                    nextEdgeCCWAroundLeftFace = currentEdge._nextEdgeCCWAroundLeftFace;
                }

                if (nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundLeftFace == currentEdge)
                {
                    // Degenerate contour (one or two edges)
                    if (nextEdgeCCWAroundLeftFace != currentEdge)
                    {
                        if (nextEdgeCCWAroundLeftFace == nextHalfEdge || nextEdgeCCWAroundLeftFace == nextHalfEdge._otherHalfOfThisEdge)
                        {
                            nextHalfEdge = nextHalfEdge._nextHalfEdge;
                        }

                        Mesh.DeleteHalfEdge(nextEdgeCCWAroundLeftFace);
                    }
                    if (currentEdge == nextHalfEdge || currentEdge == nextHalfEdge._otherHalfOfThisEdge) { nextHalfEdge = nextHalfEdge._nextHalfEdge; }
                    Mesh.DeleteHalfEdge(currentEdge);
                }
            }
        }

        static void InitPriorityQue(Tesselator tess)
        /*
         * Insert all vertices into the priority queue which determines the
         * order in which vertices cross the sweep line.
         */
        {
            MaxFirstList<ContourVertex> priorityQue = tess._vertexPriorityQue = new MaxFirstList<ContourVertex>();
            ContourVertex vertexHead = tess._mesh._vertexHead;
            for (ContourVertex curVertex = vertexHead._nextVertex; curVertex != vertexHead; curVertex = curVertex._nextVertex)
            {
                priorityQue.Add(out curVertex._priorityQueueHandle, curVertex);
            }
        }


        static void DonePriorityQ(Tesselator tess)
        {
            tess._vertexPriorityQue = null; /* __gl_pqSortDeletePriorityQ */
        }


        static bool RemoveDegenerateFaces(Mesh mesh)
        /*
         * Delete any degenerate faces with only two edges.  WalkDirtyRegions()
         * will catch almost all of these, but it won't catch degenerate faces
         * produced by splice operations on already-processed edges.
         * The two places this can happen are in FinishLeftRegions(), when
         * we splice in a "temporary" edge produced by ConnectRightVertex(),
         * and in CheckForLeftSplice(), where we splice already-processed
         * edges to ensure that our dictionary invariants are not violated
         * by numerical errors.
         *
         * In both these cases it is *very* dangerous to delete the offending
         * edge at the time, since one of the routines further up the stack
         * will sometimes be keeping a pointer to that edge.
         */
        {
            Face f, fNext;
            HalfEdge e;
            for (f = mesh._faceHead._nextFace; f != mesh._faceHead; f = fNext)
            {
                fNext = f._nextFace;
                e = f._halfEdgeThisIsLeftFaceOf;
                if (e._nextEdgeCCWAroundLeftFace == e)
                {
                    throw new Exception();
                }

                if (e._nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundLeftFace == e)
                {
                    /* A face with only two edges */
                    AddWinding(e._nextEdgeCCWAroundOrigin, e);
                    Mesh.DeleteHalfEdge(e);
                }
            }

            return true;
        }
    }
}

