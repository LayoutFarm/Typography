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
**
*/

/* The mesh structure is similar in spirit, notation, and operations
 * to the "quad-edge" structure (see L. Guibas and J. Stolfi, Primitives
 * for the manipulation of general subdivisions and the computation of
 * Voronoi diagrams, ACM Transactions on Graphics, 4(2):74-123, April 1985).
 * For a simplified description, see the course notes for CS348a,
 * "Mathematical Foundations of Computer Graphics", available at the
 * Stanford bookstore (and taught during the fall quarter).
 * The implementation also borrows a tiny subset of the graph-based approach
 * use in Mantyla's Geometric Work Bench (see M. Mantyla, An Introduction
 * to Sold Modeling, Computer Science Press, Rockville, Maryland, 1988).
 *
 * The fundamental data structure is the "half-edge".  Two half-edges
 * go together to make an edge, but they point in opposite directions.
 * Each half-edge has a pointer to its mate (the "symmetric" half-edge Sym),
 * its origin vertex (Org), the face on its left side (Lface), and the
 * adjacent half-edges in the CCW direction around the origin vertex
 * (Onext) and around the left face (Lnext).  There is also a "next"
 * pointer for the global edge list (see below).
 *
 * The notation used for mesh navigation:
 *	Sym   = the mate of a half-edge (same edge, but opposite direction)
 *	Onext = edge CCW around origin vertex (keep same origin)
 *	Dnext = edge CCW around destination vertex (keep same dest)
 *	Lnext = edge CCW around left face (dest becomes new origin)
 *	Rnext = edge CCW around right face (origin becomes new dest)
 *
 * "prev" means to substitute CW for CCW in the definitions above.
 *
 * The mesh keeps global lists of all vertices, faces, and edges,
 * stored as doubly-linked circular lists with a dummy header node.
 * The mesh stores pointers to these dummy headers (vHead, fHead, eHead).
 *
 * The circular edge list is special; since half-edges always occur
 * in pairs (e and e.Sym), each half-edge stores a pointer in only
 * one direction.  Starting at eHead and following the e.next pointers
 * will visit each *edge* once (ie. e or e.Sym, but not both).
 * e.Sym stores a pointer in the opposite direction, thus it is
 * always true that e.Sym.next.Sym.next == e.
 *
 * Each vertex has a pointer to next and previous vertices in the
 * circular list, and a pointer to a half-edge with this vertex as
 * the origin (null if this is the dummy header).  There is also a
 * field "data" for client data.
 *
 * Each face has a pointer to the next and previous faces in the
 * circular list, and a pointer to a half-edge with this face as
 * the left face (null if this is the dummy header).  There is also
 * a field "data" for client data.
 *
 * Note that what we call a "face" is really a loop; faces may consist
 * of more than one loop (ie. not simply connected), but there is no
 * record of this in the data structure.  The mesh may consist of
 * several disconnected regions, so it may not be possible to visit
 * the entire mesh by starting at a half-edge and traversing the edge
 * structure.
 *
 * The mesh does NOT support isolated vertices; a vertex is deleted along
 * with its last edge.  Similarly when two faces are merged, one of the
 * faces is deleted.  For mesh operations,
 * all face (loop) and vertex pointers must not be null.  However, once
 * mesh manipulation is finished, ZapFace can be used to delete
 * faces of the mesh, one at a time.  All external faces can be "zapped"
 * before the mesh is returned to the client; then a null face indicates
 * a region which is not part of the output polygon.
 */

using System;
namespace Tesselate
{
    // the mesh class 
    /* The mesh operations below have three motivations: completeness,
     * convenience, and efficiency.  The basic mesh operations are MakeEdge,
     * Splice, and Delete.  All the other edge operations can be implemented
     * in terms of these.  The other operations are provided for convenience
     * and/or efficiency.
     *
     * When a face is split or a vertex is added, they are inserted into the
     * global list *before* the existing vertex or face (ie. e.Org or e.Lface).
     * This makes it easier to process all vertices or faces in the global lists
     * without worrying about processing the same data twice.  As a convenience,
     * when a face is split, the "inside" flag is copied from the old face.
     * Other internal data (v.data, v.activeRegion, f.data, f.marked,
     * f.trail, e.winding) is set to zero.
     *
     * ********************** Basic Edge Operations **************************
     *
     * MakeEdge() creates one edge, two vertices, and a loop.
     * The loop (face) consists of the two new half-edges.
     *
     * Splice( eOrg, eDst ) is the basic operation for changing the
     * mesh connectivity and topology.  It changes the mesh so that
     *	eOrg.Onext <- OLD( eDst.Onext )
     *	eDst.Onext <- OLD( eOrg.Onext )
     * where OLD(...) means the value before the meshSplice operation.
     *
     * This can have two effects on the vertex structure:
     *  - if eOrg.Org != eDst.Org, the two vertices are merged together
     *  - if eOrg.Org == eDst.Org, the origin is split into two vertices
     * In both cases, eDst.Org is changed and eOrg.Org is untouched.
     *
     * Similarly (and independently) for the face structure,
     *  - if eOrg.Lface == eDst.Lface, one loop is split into two
     *  - if eOrg.Lface != eDst.Lface, two distinct loops are joined into one
     * In both cases, eDst.Lface is changed and eOrg.Lface is unaffected.
     *
     * __gl_meshDelete( eDel ) removes the edge eDel.  There are several cases:
     * if (eDel.Lface != eDel.Rface), we join two loops into one; the loop
     * eDel.Lface is deleted.  Otherwise, we are splitting one loop into two;
     * the newly created loop will contain eDel.Dst.  If the deletion of eDel
     * would create isolated vertices, those are deleted as well.
     *
     * ********************** Other Edge Operations **************************
     *
     * __gl_meshAddEdgeVertex( eOrg ) creates a new edge eNew such that
     * eNew == eOrg.Lnext, and eNew.Dst is a newly created vertex.
     * eOrg and eNew will have the same left face.
     *
     * __gl_meshSplitEdge( eOrg ) splits eOrg into two edges eOrg and eNew,
     * such that eNew == eOrg.Lnext.  The new vertex is eOrg.Dst == eNew.Org.
     * eOrg and eNew will have the same left face.
     *
     * __gl_meshConnect( eOrg, eDst ) creates a new edge from eOrg.Dst
     * to eDst.Org, and returns the corresponding half-edge eNew.
     * If eOrg.Lface == eDst.Lface, this splits one loop into two,
     * and the newly created loop is eNew.Lface.  Otherwise, two disjoint
     * loops are merged into one, and the loop eDst.Lface is destroyed.
     *
     * ************************ Other Operations *****************************
     *
     * __gl_meshNewMesh() creates a new mesh with no edges, no vertices,
     * and no loops (what we usually call a "face").
     *
     * __gl_meshUnion( mesh1, mesh2 ) forms the union of all structures in
     * both meshes, and returns the new mesh (the old meshes are destroyed).
     *
     * __gl_meshDeleteMesh( mesh ) will free all storage for any valid mesh.
     *
     * __gl_meshZapFace( fZap ) destroys a face and removes it from the
     * global face list.  All edges of fZap will have a null pointer as their
     * left face.  Any edges which also have a null pointer as their right face
     * are deleted entirely (along with any isolated vertices this produces).
     * An entire mesh can be deleted by zapping its faces, one at a time,
     * in any order.  Zapped faces cannot be used in further mesh operations!
     *
     * __gl_meshCheckMesh( mesh ) checks a mesh for self-consistency.
     */

    public class Mesh
    {
        public ContourVertex vertexHead = new ContourVertex();		/* dummy header for vertex list */
        public Face faceHead = new Face();		/* dummy header for face list */
        public HalfEdge halfEdgeHead = new HalfEdge();		/* dummy header for edge list */
        HalfEdge otherHalfOfThisEdgeHead = new HalfEdge();	/* and its symmetric counterpart */
        /* Creates a new mesh with no edges, no vertices,
        * and no loops (what we usually call a "face").
        */
        public Mesh()
        {
            HalfEdge otherHalfOfThisEdge = this.otherHalfOfThisEdgeHead;
            vertexHead.nextVertex = vertexHead.prevVertex = vertexHead;
            vertexHead.edgeThisIsOriginOf = null;
            vertexHead.clientIndex = 0;
            faceHead.nextFace = faceHead.prevFace = faceHead;
            faceHead.halfEdgeThisIsLeftFaceOf = null;
            faceHead.trail = null;
            faceHead.marked = false;
            faceHead.isInterior = false;
            halfEdgeHead.nextHalfEdge = halfEdgeHead;
            halfEdgeHead.otherHalfOfThisEdge = otherHalfOfThisEdge;
            halfEdgeHead.nextEdgeCCWAroundOrigin = null;
            halfEdgeHead.nextEdgeCCWAroundLeftFace = null;
            halfEdgeHead.originVertex = null;
            halfEdgeHead.leftFace = null;
            halfEdgeHead.winding = 0;
            halfEdgeHead.regionThisIsUpperEdgeOf = null;
            otherHalfOfThisEdge.nextHalfEdge = otherHalfOfThisEdge;
            otherHalfOfThisEdge.otherHalfOfThisEdge = halfEdgeHead;
            otherHalfOfThisEdge.nextEdgeCCWAroundOrigin = null;
            otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace = null;
            otherHalfOfThisEdge.originVertex = null;
            otherHalfOfThisEdge.leftFace = null;
            otherHalfOfThisEdge.winding = 0;
            otherHalfOfThisEdge.regionThisIsUpperEdgeOf = null;
        }

        /* MakeFace( newFace, eOrig, fNext ) attaches a new face and makes it the left
        * face of all edges in the face loop to which eOrig belongs.  "fNext" gives
        * a place to insert the new face in the global face list.  We insert
        * the new face *before* fNext so that algorithms which walk the face
        * list will not see the newly created faces.
        */
        static int faceIndex = 0;
        static void MakeFace(Face newFace, HalfEdge eOrig, Face fNext)
        {
            HalfEdge e;
            Face fPrev;
            Face fNew = newFace;
            fNew.indexDebug = faceIndex++;
            // insert in circular doubly-linked list before fNext

            fPrev = fNext.prevFace;
            fNew.prevFace = fPrev;
            fPrev.nextFace = fNew;
            fNew.nextFace = fNext;
            fNext.prevFace = fNew;
            fNew.halfEdgeThisIsLeftFaceOf = eOrig;
            fNew.trail = null;
            fNew.marked = false;
            // The new face is marked "inside" if the old one was.  This is a
            // convenience for the common case where a face has been split in two.
            fNew.isInterior = fNext.isInterior;
            // fix other edges on this face loop
            e = eOrig;
            do
            {
                e.leftFace = fNew;
                e = e.nextEdgeCCWAroundLeftFace;
            } while (e != eOrig);
        }

        // __gl_meshMakeEdge creates one edge, two vertices, and a loop (face).
        // The loop consists of the two new half-edges.
        public HalfEdge MakeEdge()
        {
            ContourVertex newVertex1 = new ContourVertex();
            ContourVertex newVertex2 = new ContourVertex();
            Face newFace = new Face();
            HalfEdge e;
            e = MakeEdge(this.halfEdgeHead);
            MakeVertex(newVertex1, e, this.vertexHead);
            MakeVertex(newVertex2, e.otherHalfOfThisEdge, this.vertexHead);
            MakeFace(newFace, e, this.faceHead);
            return e;
        }

        /* MakeVertex( newVertex, eOrig, vNext ) attaches a new vertex and makes it the
        * origin of all edges in the vertex loop to which eOrig belongs. "vNext" gives
        * a place to insert the new vertex in the global vertex list.  We insert
        * the new vertex *before* vNext so that algorithms which walk the vertex
        * list will not see the newly created vertices.
        */
        static void MakeVertex(ContourVertex newVertex, HalfEdge eOrig, ContourVertex vNext)
        {
            HalfEdge e;
            ContourVertex vPrev;
            ContourVertex vNew = newVertex;
            /* insert in circular doubly-linked list before vNext */
            vPrev = vNext.prevVertex;
            vNew.prevVertex = vPrev;
            vPrev.nextVertex = vNew;
            vNew.nextVertex = vNext;
            vNext.prevVertex = vNew;
            vNew.edgeThisIsOriginOf = eOrig;
            vNew.clientIndex = 0;
            /* leave coords, s, t undefined */

            /* fix other edges on this vertex loop */
            e = eOrig;
            do
            {
                e.originVertex = vNew;
                e = e.nextEdgeCCWAroundOrigin;
            } while (e != eOrig);
        }

        /* KillVertex( vDel ) destroys a vertex and removes it from the global
        * vertex list.  It updates the vertex loop to point to a given new vertex.
        */
        static void KillVertex(ContourVertex vDel, ContourVertex newOrg)
        {
            HalfEdge e, eStart = vDel.edgeThisIsOriginOf;
            ContourVertex vPrev, vNext;
            /* change the origin of all affected edges */
            e = eStart;
            do
            {
                e.originVertex = newOrg;
                e = e.nextEdgeCCWAroundOrigin;
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            vPrev = vDel.prevVertex;
            vNext = vDel.nextVertex;
            vNext.prevVertex = vPrev;
            vPrev.nextVertex = vNext;
        }

        /* KillFace( fDel ) destroys a face and removes it from the global face
        * list.  It updates the face loop to point to a given new face.
        */
        static void KillFace(Face fDel, Face newLface)
        {
            HalfEdge e, eStart = fDel.halfEdgeThisIsLeftFaceOf;
            Face fPrev, fNext;
            /* change the left face of all affected edges */
            e = eStart;
            do
            {
                e.leftFace = newLface;
                e = e.nextEdgeCCWAroundLeftFace;
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            fPrev = fDel.prevFace;
            fNext = fDel.nextFace;
            fNext.prevFace = fPrev;
            fPrev.nextFace = fNext;
        }

        /* Splice( a, b ) is best described by the Guibas/Stolfi paper or the
        * CS348a notes (see mesh.h).  Basically it modifies the mesh so that
        * a.Onext and b.Onext are exchanged.  This can have various effects
        * depending on whether a and b belong to different face or vertex rings.
        * For more explanation see __gl_meshSplice() below.
        */
        static void Splice(HalfEdge a, HalfEdge b)
        {
            HalfEdge aOnext = a.nextEdgeCCWAroundOrigin;
            HalfEdge bOnext = b.nextEdgeCCWAroundOrigin;
            aOnext.otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace = b;
            bOnext.otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace = a;
            a.nextEdgeCCWAroundOrigin = bOnext;
            b.nextEdgeCCWAroundOrigin = aOnext;
        }

        /* __gl_meshSplice( eOrg, eDst ) is the basic operation for changing the
        * mesh connectivity and topology.  It changes the mesh so that
        *	eOrg.Onext <- OLD( eDst.Onext )
        *	eDst.Onext <- OLD( eOrg.Onext )
        * where OLD(...) means the value before the meshSplice operation.
        *
        * This can have two effects on the vertex structure:
        *  - if eOrg.Org != eDst.Org, the two vertices are merged together
        *  - if eOrg.Org == eDst.Org, the origin is split into two vertices
        * In both cases, eDst.Org is changed and eOrg.Org is untouched.
        *
        * Similarly (and independently) for the face structure,
        *  - if eOrg.Lface == eDst.Lface, one loop is split into two
        *  - if eOrg.Lface != eDst.Lface, two distinct loops are joined into one
        * In both cases, eDst.Lface is changed and eOrg.Lface is unaffected.
        *
        * Some special cases:
        * If eDst == eOrg, the operation has no effect.
        * If eDst == eOrg.Lnext, the new face will have a single edge.
        * If eDst == eOrg.Lprev, the old face will have a single edge.
        * If eDst == eOrg.Onext, the new vertex will have a single edge.
        * If eDst == eOrg.Oprev, the old vertex will have a single edge.
        */
        public static void meshSplice(HalfEdge eOrg, HalfEdge eDst)
        {
            bool joiningLoops = false;
            bool joiningVertices = false;
            if (eOrg == eDst) return;
            if (eDst.originVertex != eOrg.originVertex)
            {
                /* We are merging two disjoint vertices -- destroy eDst.Org */
                joiningVertices = true;
                KillVertex(eDst.originVertex, eOrg.originVertex);
            }
            if (eDst.leftFace != eOrg.leftFace)
            {
                /* We are connecting two disjoint loops -- destroy eDst.Lface */
                joiningLoops = true;
                KillFace(eDst.leftFace, eOrg.leftFace);
            }

            /* Change the edge structure */
            Splice(eDst, eOrg);
            if (!joiningVertices)
            {
                ContourVertex newVertex = new ContourVertex();
                /* We split one vertex into two -- the new vertex is eDst.Org.
                * Make sure the old vertex points to a valid half-edge.
                */
                MakeVertex(newVertex, eDst, eOrg.originVertex);
                eOrg.originVertex.edgeThisIsOriginOf = eOrg;
            }
            if (!joiningLoops)
            {
                Face newFace = new Face();
                /* We split one loop into two -- the new loop is eDst.Lface.
                * Make sure the old face points to a valid half-edge.
                */
                MakeFace(newFace, eDst, eOrg.leftFace);
                eOrg.leftFace.halfEdgeThisIsLeftFaceOf = eOrg;
            }
        }

        /* KillEdge( eDel ) destroys an edge (the half-edges eDel and eDel.Sym),
        * and removes from the global edge list.
        */
        static void KillEdge(HalfEdge eDel)
        {
            HalfEdge ePrev, eNext;
            /* Half-edges are allocated in pairs, see EdgePair above */
            if (eDel.otherHalfOfThisEdge.isFirstHalfEdge)
            {
                eDel = eDel.otherHalfOfThisEdge;
            }

            /* delete from circular doubly-linked list */
            eNext = eDel.nextHalfEdge;
            ePrev = eDel.otherHalfOfThisEdge.nextHalfEdge;
            eNext.otherHalfOfThisEdge.nextHalfEdge = ePrev;
            ePrev.otherHalfOfThisEdge.nextHalfEdge = eNext;
        }

        /* __gl_meshDelete( eDel ) removes the edge eDel.  There are several cases:
        * if (eDel.Lface != eDel.Rface), we join two loops into one; the loop
        * eDel.Lface is deleted.  Otherwise, we are splitting one loop into two;
        * the newly created loop will contain eDel.Dst.  If the deletion of eDel
        * would create isolated vertices, those are deleted as well.
        *
        * This function could be implemented as two calls to __gl_meshSplice
        * plus a few calls to free, but this would allocate and delete
        * unnecessary vertices and faces.
        */
        public static void DeleteHalfEdge(HalfEdge edgeToDelete)
        {
            HalfEdge otherHalfOfEdgeToDelete = edgeToDelete.otherHalfOfThisEdge;
            bool joiningLoops = false;
            // First step: disconnect the origin vertex eDel.Org.  We make all
            // changes to get a consistent mesh in this "intermediate" state.
            if (edgeToDelete.leftFace != edgeToDelete.rightFace)
            {
                // We are joining two loops into one -- remove the left face
                joiningLoops = true;
                KillFace(edgeToDelete.leftFace, edgeToDelete.rightFace);
            }

            if (edgeToDelete.nextEdgeCCWAroundOrigin == edgeToDelete)
            {
                KillVertex(edgeToDelete.originVertex, null);
            }
            else
            {
                // Make sure that eDel.Org and eDel.Rface point to valid half-edges
                edgeToDelete.rightFace.halfEdgeThisIsLeftFaceOf = edgeToDelete.Oprev;
                edgeToDelete.originVertex.edgeThisIsOriginOf = edgeToDelete.nextEdgeCCWAroundOrigin;
                Splice(edgeToDelete, edgeToDelete.Oprev);
                if (!joiningLoops)
                {
                    Face newFace = new Face();
                    // We are splitting one loop into two -- create a new loop for eDel.
                    MakeFace(newFace, edgeToDelete, edgeToDelete.leftFace);
                }
            }

            // Claim: the mesh is now in a consistent state, except that eDel.Org
            // may have been deleted.  Now we disconnect eDel.Dst.
            if (otherHalfOfEdgeToDelete.nextEdgeCCWAroundOrigin == otherHalfOfEdgeToDelete)
            {
                KillVertex(otherHalfOfEdgeToDelete.originVertex, null);
                KillFace(otherHalfOfEdgeToDelete.leftFace, null);
            }
            else
            {
                // Make sure that eDel.Dst and eDel.Lface point to valid half-edges
                edgeToDelete.leftFace.halfEdgeThisIsLeftFaceOf = otherHalfOfEdgeToDelete.Oprev;
                otherHalfOfEdgeToDelete.originVertex.edgeThisIsOriginOf = otherHalfOfEdgeToDelete.nextEdgeCCWAroundOrigin;
                Splice(otherHalfOfEdgeToDelete, otherHalfOfEdgeToDelete.Oprev);
            }

            // Any isolated vertices or faces have already been freed.
            KillEdge(edgeToDelete);
        }

        /* __gl_meshAddEdgeVertex( eOrg ) creates a new edge eNew such that
        * eNew == eOrg.Lnext, and eNew.Dst is a newly created vertex.
        * eOrg and eNew will have the same left face.
        */
        static HalfEdge meshAddEdgeVertex(HalfEdge eOrg)
        {
            HalfEdge eNewSym;
            HalfEdge eNew = MakeEdge(eOrg);
            eNewSym = eNew.otherHalfOfThisEdge;
            /* Connect the new edge appropriately */
            Splice(eNew, eOrg.nextEdgeCCWAroundLeftFace);
            /* Set the vertex and face information */
            eNew.originVertex = eOrg.directionVertex;
            {
                ContourVertex newVertex = new ContourVertex();
                MakeVertex(newVertex, eNewSym, eNew.originVertex);
            }
            eNew.leftFace = eNewSym.leftFace = eOrg.leftFace;
            return eNew;
        }

        /* __gl_meshSplitEdge( eOrg ) splits eOrg into two edges eOrg and eNew,
        * such that eNew == eOrg.Lnext.  The new vertex is eOrg.Dst == eNew.Org.
        * eOrg and eNew will have the same left face.
        */
        public static HalfEdge meshSplitEdge(HalfEdge eOrg)
        {
            HalfEdge eNew;
            HalfEdge tempHalfEdge = meshAddEdgeVertex(eOrg);
            eNew = tempHalfEdge.otherHalfOfThisEdge;
            /* Disconnect eOrg from eOrg.Dst and connect it to eNew.Org */
            Splice(eOrg.otherHalfOfThisEdge, eOrg.otherHalfOfThisEdge.Oprev);
            Splice(eOrg.otherHalfOfThisEdge, eNew);
            /* Set the vertex and face information */
            eOrg.directionVertex = eNew.originVertex;
            eNew.directionVertex.edgeThisIsOriginOf = eNew.otherHalfOfThisEdge;	/* may have pointed to eOrg.Sym */
            eNew.rightFace = eOrg.rightFace;
            eNew.winding = eOrg.winding;	/* copy old winding information */
            eNew.otherHalfOfThisEdge.winding = eOrg.otherHalfOfThisEdge.winding;
            return eNew;
        }

        /* Allocate and free half-edges in pairs for efficiency.
        * The *only* place that should use this fact is allocation/free.
        */
        class EdgePair
        {
            public readonly HalfEdge e = new HalfEdge();
            public readonly HalfEdge eSym = new HalfEdge();
#if DEBUG
            static int debugIndex;
#endif
            public EdgePair()
            {
#if DEBUG
                e.debugIndex = debugIndex++;
                eSym.debugIndex = debugIndex++;
#endif
            }
        };
        /* MakeEdge creates a new pair of half-edges which form their own loop.
        * No vertex or face structures are allocated, but these must be assigned
        * before the current edge operation is completed.
        */
        static HalfEdge MakeEdge(HalfEdge eNext)
        {
            HalfEdge ePrev;
            EdgePair pair = new EdgePair();
            /* Make sure eNext points to the first edge of the edge pair */
            if (eNext.otherHalfOfThisEdge.isFirstHalfEdge)
            {
                eNext = eNext.otherHalfOfThisEdge;
            }

            /* Insert in circular doubly-linked list before eNext.
            * Note that the prev pointer is stored in Sym.next.
            */
            ePrev = eNext.otherHalfOfThisEdge.nextHalfEdge;
            pair.eSym.nextHalfEdge = ePrev;
            ePrev.otherHalfOfThisEdge.nextHalfEdge = pair.e;
            pair.e.nextHalfEdge = eNext;
            eNext.otherHalfOfThisEdge.nextHalfEdge = pair.eSym;
            pair.e.isFirstHalfEdge = true;
            pair.e.otherHalfOfThisEdge = pair.eSym;
            pair.e.nextEdgeCCWAroundOrigin = pair.e;
            pair.e.nextEdgeCCWAroundLeftFace = pair.eSym;
            pair.e.originVertex = null;
            pair.e.leftFace = null;
            pair.e.winding = 0;
            pair.e.regionThisIsUpperEdgeOf = null;
            pair.eSym.isFirstHalfEdge = false;
            pair.eSym.otherHalfOfThisEdge = pair.e;
            pair.eSym.nextEdgeCCWAroundOrigin = pair.eSym;
            pair.eSym.nextEdgeCCWAroundLeftFace = pair.e;
            pair.eSym.originVertex = null;
            pair.eSym.leftFace = null;
            pair.eSym.winding = 0;
            pair.eSym.regionThisIsUpperEdgeOf = null;
            return pair.e;
        }

        /* __gl_meshConnect( eOrg, eDst ) creates a new edge from eOrg.Dst
        * to eDst.Org, and returns the corresponding half-edge eNew.
        * If eOrg.Lface == eDst.Lface, this splits one loop into two,
        * and the newly created loop is eNew.Lface.  Otherwise, two disjoint
        * loops are merged into one, and the loop eDst.Lface is destroyed.
        *
        * If (eOrg == eDst), the new face will have only two edges.
        * If (eOrg.Lnext == eDst), the old face is reduced to a single edge.
        * If (eOrg.Lnext.Lnext == eDst), the old face is reduced to two edges.
        */
        public static HalfEdge meshConnect(HalfEdge eOrg, HalfEdge eDst)
        {
            HalfEdge eNewSym;
            bool joiningLoops = false;
            HalfEdge eNew = MakeEdge(eOrg);
            eNewSym = eNew.otherHalfOfThisEdge;
            if (eDst.leftFace != eOrg.leftFace)
            {
                /* We are connecting two disjoint loops -- destroy eDst.Lface */
                joiningLoops = true;
                KillFace(eDst.leftFace, eOrg.leftFace);
            }

            /* Connect the new edge appropriately */
            Splice(eNew, eOrg.nextEdgeCCWAroundLeftFace);
            Splice(eNewSym, eDst);
            /* Set the vertex and face information */
            eNew.originVertex = eOrg.directionVertex;
            eNewSym.originVertex = eDst.originVertex;
            eNew.leftFace = eNewSym.leftFace = eOrg.leftFace;
            /* Make sure the old face points to a valid half-edge */
            eOrg.leftFace.halfEdgeThisIsLeftFaceOf = eNewSym;
            if (!joiningLoops)
            {
                Face newFace = new Face();
                /* We split one loop into two -- the new loop is eNew.Lface */
                MakeFace(newFace, eNew, eOrg.leftFace);
            }
            return eNew;
        }

        /* __gl_meshUnion( mesh1, mesh2 ) forms the union of all structures in
        * both meshes, and returns the new mesh (the old meshes are destroyed).
        */
        Mesh meshUnion(Mesh mesh1, Mesh mesh2)
        {
            Face f1 = mesh1.faceHead;
            ContourVertex v1 = mesh1.vertexHead;
            HalfEdge e1 = mesh1.halfEdgeHead;
            Face f2 = mesh2.faceHead;
            ContourVertex v2 = mesh2.vertexHead;
            HalfEdge e2 = mesh2.halfEdgeHead;
            /* Add the faces, vertices, and edges of mesh2 to those of mesh1 */
            if (f2.nextFace != f2)
            {
                f1.prevFace.nextFace = f2.nextFace;
                f2.nextFace.prevFace = f1.prevFace;
                f2.prevFace.nextFace = f1;
                f1.prevFace = f2.prevFace;
            }

            if (v2.nextVertex != v2)
            {
                v1.prevVertex.nextVertex = v2.nextVertex;
                v2.nextVertex.prevVertex = v1.prevVertex;
                v2.prevVertex.nextVertex = v1;
                v1.prevVertex = v2.prevVertex;
            }

            if (e2.nextHalfEdge != e2)
            {
                e1.otherHalfOfThisEdge.nextHalfEdge.otherHalfOfThisEdge.nextHalfEdge = e2.nextHalfEdge;
                e2.nextHalfEdge.otherHalfOfThisEdge.nextHalfEdge = e1.otherHalfOfThisEdge.nextHalfEdge;
                e2.otherHalfOfThisEdge.nextHalfEdge.otherHalfOfThisEdge.nextHalfEdge = e1;
                e1.otherHalfOfThisEdge.nextHalfEdge = e2.otherHalfOfThisEdge.nextHalfEdge;
            }

            mesh2 = null;
            return mesh1;
        }

        /* __gl_meshZapFace( fZap ) destroys a face and removes it from the
        * global face list.  All edges of fZap will have a null pointer as their
        * left face.  Any edges which also have a null pointer as their right face
        * are deleted entirely (along with any isolated vertices this produces).
        * An entire mesh can be deleted by zapping its faces, one at a time,
        * in any order.  Zapped faces cannot be used in further mesh operations!
        */
        public static void meshZapFace(Face fZap)
        {
            HalfEdge eStart = fZap.halfEdgeThisIsLeftFaceOf;
            HalfEdge e, eNext, eSym;
            Face fPrev, fNext;
            /* walk around face, deleting edges whose right face is also null */
            eNext = eStart.nextEdgeCCWAroundLeftFace;
            do
            {
                e = eNext;
                eNext = e.nextEdgeCCWAroundLeftFace;
                e.leftFace = null;
                if (e.rightFace == null)
                {
                    /* delete the edge -- see __gl_MeshDelete above */

                    if (e.nextEdgeCCWAroundOrigin == e)
                    {
                        KillVertex(e.originVertex, null);
                    }
                    else
                    {
                        /* Make sure that e.Org points to a valid half-edge */
                        e.originVertex.edgeThisIsOriginOf = e.nextEdgeCCWAroundOrigin;
                        Splice(e, e.Oprev);
                    }
                    eSym = e.otherHalfOfThisEdge;
                    if (eSym.nextEdgeCCWAroundOrigin == eSym)
                    {
                        KillVertex(eSym.originVertex, null);
                    }
                    else
                    {
                        /* Make sure that eSym.Org points to a valid half-edge */
                        eSym.originVertex.edgeThisIsOriginOf = eSym.nextEdgeCCWAroundOrigin;
                        Splice(eSym, eSym.Oprev);
                    }
                    KillEdge(e);
                }
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            fPrev = fZap.prevFace;
            fNext = fZap.nextFace;
            fNext.prevFace = fPrev;
            fPrev.nextFace = fNext;
            fZap = null;
        }

        /* __gl_meshCheckMesh( mesh ) checks a mesh for self-consistency.
        */
        public void CheckMesh()
        {
            Face fHead = this.faceHead;
            ContourVertex vHead = this.vertexHead;
            HalfEdge eHead = this.halfEdgeHead;
            Face f, fPrev;
            ContourVertex v, vPrev;
            HalfEdge e, ePrev;
            fPrev = fHead;
            for (fPrev = fHead; (f = fPrev.nextFace) != fHead; fPrev = f)
            {
                if (f.prevFace != fPrev)
                {
                    throw new Exception();
                }
                e = f.halfEdgeThisIsLeftFaceOf;
                do
                {
                    if (e.otherHalfOfThisEdge == e)
                    {
                        throw new Exception();
                    }
                    if (e.otherHalfOfThisEdge.otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e.nextEdgeCCWAroundLeftFace.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace != e)
                    {
                        throw new Exception();
                    }
                    if (e.leftFace != f)
                    {
                        throw new Exception();
                    }
                    e = e.nextEdgeCCWAroundLeftFace;
                } while (e != f.halfEdgeThisIsLeftFaceOf);
            }
            if (f.prevFace != fPrev || f.halfEdgeThisIsLeftFaceOf != null)
            {
                throw new Exception();
            }

            vPrev = vHead;
            for (vPrev = vHead; (v = vPrev.nextVertex) != vHead; vPrev = v)
            {
                if (v.prevVertex != vPrev)
                {
                    throw new Exception();
                }
                e = v.edgeThisIsOriginOf;
                do
                {
                    if (e.otherHalfOfThisEdge == e)
                    {
                        throw new Exception();
                    }
                    if (e.otherHalfOfThisEdge.otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e.nextEdgeCCWAroundLeftFace.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace != e)
                    {
                        throw new Exception();
                    }
                    if (e.originVertex != v)
                    {
                        throw new Exception();
                    }
                    e = e.nextEdgeCCWAroundOrigin;
                } while (e != v.edgeThisIsOriginOf);
            }
            if (v.prevVertex != vPrev || v.edgeThisIsOriginOf != null || v.clientIndex != 0)
            {
                throw new Exception();
            }

            ePrev = eHead;
            for (ePrev = eHead; (e = ePrev.nextHalfEdge) != eHead; ePrev = e)
            {
                if (e.otherHalfOfThisEdge.nextHalfEdge != ePrev.otherHalfOfThisEdge)
                {
                    throw new Exception();
                }
                if (e.otherHalfOfThisEdge == e)
                {
                    throw new Exception();
                }
                if (e.otherHalfOfThisEdge.otherHalfOfThisEdge != e)
                {
                    throw new Exception();
                }
                if (e.originVertex == null)
                {
                    throw new Exception();
                }
                if (e.directionVertex == null)
                {
                    throw new Exception();
                }
                if (e.nextEdgeCCWAroundLeftFace.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge != e)
                {
                    throw new Exception();
                }
                if (e.nextEdgeCCWAroundOrigin.otherHalfOfThisEdge.nextEdgeCCWAroundLeftFace != e)
                {
                    throw new Exception();
                }
            }
            if (e.otherHalfOfThisEdge.nextHalfEdge != ePrev.otherHalfOfThisEdge
                || e.otherHalfOfThisEdge != this.otherHalfOfThisEdgeHead
                || e.otherHalfOfThisEdge.otherHalfOfThisEdge != e
                || e.originVertex != null || e.directionVertex != null
                || e.leftFace != null || e.rightFace != null)
            {
                throw new Exception();
            }
        }

        /* SetWindingNumber( value, keepOnlyBoundary ) resets the
        * winding numbers on all edges so that regions marked "inside" the
        * polygon have a winding number of "value", and regions outside
        * have a winding number of 0.
        *
        * If keepOnlyBoundary is TRUE, it also deletes all edges which do not
        * separate an interior region from an exterior one.
        */
        public bool SetWindingNumber(int value, bool keepOnlyBoundary)
        {
            HalfEdge e, eNext;
            for (e = this.halfEdgeHead.nextHalfEdge; e != this.halfEdgeHead; e = eNext)
            {
                eNext = e.nextHalfEdge;
                if (e.rightFace.isInterior != e.leftFace.isInterior)
                {
                    /* This is a boundary edge (one side is interior, one is exterior). */
                    e.winding = (e.leftFace.isInterior) ? value : -value;
                }
                else
                {
                    /* Both regions are interior, or both are exterior. */
                    if (!keepOnlyBoundary)
                    {
                        e.winding = 0;
                    }
                    else
                    {
                        Mesh.DeleteHalfEdge(e);
                    }
                }
            }

            return true;
        }

        /* DiscardExterior() zaps (ie. sets to NULL) all faces
        * which are not marked "inside" the polygon.  Since further mesh operations
        * on NULL faces are not allowed, the main purpose is to clean up the
        * mesh so that exterior loops are not represented in the data structure.
        */
        public void DiscardExterior()
        {
            Face f, next;
            for (f = this.faceHead.nextFace; f != this.faceHead; f = next)
            {
                /* Since f will be destroyed, save its next pointer. */
                next = f.nextFace;
                if (!f.isInterior)
                {
                    Mesh.meshZapFace(f);
                }
            }
        }

        /* TessellateInterior() tessellates each region of
        * the mesh which is marked "inside" the polygon.  Each such region
        * must be monotone.
        */
        public bool TessellateInterior()
        {
            Face f, next;
            for (f = this.faceHead.nextFace; f != this.faceHead; f = next)
            {
                /* Make sure we don''t try to tessellate the new triangles. */
                next = f.nextFace;
                if (f.isInterior)
                {
                    if (!f.TessellateMonoRegion())
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}