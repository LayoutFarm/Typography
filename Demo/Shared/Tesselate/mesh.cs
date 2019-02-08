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
        internal ContourVertex _vertexHead = new ContourVertex();		/* dummy header for vertex list */
        internal Face _faceHead = new Face();		/* dummy header for face list */
        internal HalfEdge _halfEdgeHead = new HalfEdge();		/* dummy header for edge list */
        HalfEdge _otherHalfOfThisEdgeHead = new HalfEdge();	/* and its symmetric counterpart */
        /* Creates a new mesh with no edges, no vertices,
        * and no loops (what we usually call a "face").
        */
        public Mesh()
        {
            HalfEdge otherHalfOfThisEdge = _otherHalfOfThisEdgeHead;
            _vertexHead._nextVertex = _vertexHead._prevVertex = _vertexHead;
            _vertexHead._edgeThisIsOriginOf = null;
            _vertexHead._clientIndex = 0;
            _faceHead._nextFace = _faceHead._prevFace = _faceHead;
            _faceHead._halfEdgeThisIsLeftFaceOf = null;
            _faceHead._trail = null;
            _faceHead._marked = false;
            _faceHead._isInterior = false;
            _halfEdgeHead._nextHalfEdge = _halfEdgeHead;
            _halfEdgeHead._otherHalfOfThisEdge = otherHalfOfThisEdge;
            _halfEdgeHead._nextEdgeCCWAroundOrigin = null;
            _halfEdgeHead._nextEdgeCCWAroundLeftFace = null;
            _halfEdgeHead._originVertex = null;
            _halfEdgeHead._leftFace = null;
            _halfEdgeHead._winding = 0;
            _halfEdgeHead._regionThisIsUpperEdgeOf = null;
            otherHalfOfThisEdge._nextHalfEdge = otherHalfOfThisEdge;
            otherHalfOfThisEdge._otherHalfOfThisEdge = _halfEdgeHead;
            otherHalfOfThisEdge._nextEdgeCCWAroundOrigin = null;
            otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace = null;
            otherHalfOfThisEdge._originVertex = null;
            otherHalfOfThisEdge._leftFace = null;
            otherHalfOfThisEdge._winding = 0;
            otherHalfOfThisEdge._regionThisIsUpperEdgeOf = null;
        }

        /* MakeFace( newFace, eOrig, fNext ) attaches a new face and makes it the left
        * face of all edges in the face loop to which eOrig belongs.  "fNext" gives
        * a place to insert the new face in the global face list.  We insert
        * the new face *before* fNext so that algorithms which walk the face
        * list will not see the newly created faces.
        */

#if DEBUG
        static int s_dbugFaceIndexTotal = 0;
#endif
        static void MakeFace(Face newFace, HalfEdge eOrig, Face fNext)
        {
            HalfEdge e;
            Face fPrev;
            Face fNew = newFace;
#if DEBUG
            fNew.dbugIndex = s_dbugFaceIndexTotal++;
#endif
            // insert in circular doubly-linked list before fNext

            fPrev = fNext._prevFace;
            fNew._prevFace = fPrev;
            fPrev._nextFace = fNew;
            fNew._nextFace = fNext;
            fNext._prevFace = fNew;
            fNew._halfEdgeThisIsLeftFaceOf = eOrig;
            fNew._trail = null;
            fNew._marked = false;
            // The new face is marked "inside" if the old one was.  This is a
            // convenience for the common case where a face has been split in two.
            fNew._isInterior = fNext._isInterior;
            // fix other edges on this face loop
            e = eOrig;
            do
            {
                e._leftFace = fNew;
                e = e._nextEdgeCCWAroundLeftFace;
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
            e = MakeEdge(_halfEdgeHead);
            MakeVertex(newVertex1, e, _vertexHead);
            MakeVertex(newVertex2, e._otherHalfOfThisEdge, _vertexHead);
            MakeFace(newFace, e, _faceHead);
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
            vPrev = vNext._prevVertex;
            vNew._prevVertex = vPrev;
            vPrev._nextVertex = vNew;
            vNew._nextVertex = vNext;
            vNext._prevVertex = vNew;
            vNew._edgeThisIsOriginOf = eOrig;
            vNew._clientIndex = 0;
            /* leave coords, s, t undefined */

            /* fix other edges on this vertex loop */
            e = eOrig;
            do
            {
                e._originVertex = vNew;
                e = e._nextEdgeCCWAroundOrigin;
            } while (e != eOrig);
        }

        /* KillVertex( vDel ) destroys a vertex and removes it from the global
        * vertex list.  It updates the vertex loop to point to a given new vertex.
        */
        static void KillVertex(ContourVertex vDel, ContourVertex newOrg)
        {
            HalfEdge e, eStart = vDel._edgeThisIsOriginOf;
            ContourVertex vPrev, vNext;
            /* change the origin of all affected edges */
            e = eStart;
            do
            {
                e._originVertex = newOrg;
                e = e._nextEdgeCCWAroundOrigin;
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            vPrev = vDel._prevVertex;
            vNext = vDel._nextVertex;
            vNext._prevVertex = vPrev;
            vPrev._nextVertex = vNext;
        }

        /* KillFace( fDel ) destroys a face and removes it from the global face
        * list.  It updates the face loop to point to a given new face.
        */
        static void KillFace(Face fDel, Face newLface)
        {
            HalfEdge e, eStart = fDel._halfEdgeThisIsLeftFaceOf;
            Face fPrev, fNext;
            /* change the left face of all affected edges */
            e = eStart;
            do
            {
                e._leftFace = newLface;
                e = e._nextEdgeCCWAroundLeftFace;
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            fPrev = fDel._prevFace;
            fNext = fDel._nextFace;
            fNext._prevFace = fPrev;
            fPrev._nextFace = fNext;
        }

        /* Splice( a, b ) is best described by the Guibas/Stolfi paper or the
        * CS348a notes (see mesh.h).  Basically it modifies the mesh so that
        * a.Onext and b.Onext are exchanged.  This can have various effects
        * depending on whether a and b belong to different face or vertex rings.
        * For more explanation see __gl_meshSplice() below.
        */
        static void Splice(HalfEdge a, HalfEdge b)
        {
            HalfEdge aOnext = a._nextEdgeCCWAroundOrigin;
            HalfEdge bOnext = b._nextEdgeCCWAroundOrigin;
            aOnext._otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace = b;
            bOnext._otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace = a;
            a._nextEdgeCCWAroundOrigin = bOnext;
            b._nextEdgeCCWAroundOrigin = aOnext;
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
            if (eDst._originVertex != eOrg._originVertex)
            {
                /* We are merging two disjoint vertices -- destroy eDst.Org */
                joiningVertices = true;
                KillVertex(eDst._originVertex, eOrg._originVertex);
            }
            if (eDst._leftFace != eOrg._leftFace)
            {
                /* We are connecting two disjoint loops -- destroy eDst.Lface */
                joiningLoops = true;
                KillFace(eDst._leftFace, eOrg._leftFace);
            }

            /* Change the edge structure */
            Splice(eDst, eOrg);
            if (!joiningVertices)
            {
                ContourVertex newVertex = new ContourVertex();
                /* We split one vertex into two -- the new vertex is eDst.Org.
                * Make sure the old vertex points to a valid half-edge.
                */
                MakeVertex(newVertex, eDst, eOrg._originVertex);
                eOrg._originVertex._edgeThisIsOriginOf = eOrg;
            }
            if (!joiningLoops)
            {
                Face newFace = new Face();
                /* We split one loop into two -- the new loop is eDst.Lface.
                * Make sure the old face points to a valid half-edge.
                */
                MakeFace(newFace, eDst, eOrg._leftFace);
                eOrg._leftFace._halfEdgeThisIsLeftFaceOf = eOrg;
            }
        }

        /* KillEdge( eDel ) destroys an edge (the half-edges eDel and eDel.Sym),
        * and removes from the global edge list.
        */
        static void KillEdge(HalfEdge eDel)
        {
            HalfEdge ePrev, eNext;
            /* Half-edges are allocated in pairs, see EdgePair above */
            if (eDel._otherHalfOfThisEdge._isFirstHalfEdge)
            {
                eDel = eDel._otherHalfOfThisEdge;
            }

            /* delete from circular doubly-linked list */
            eNext = eDel._nextHalfEdge;
            ePrev = eDel._otherHalfOfThisEdge._nextHalfEdge;
            eNext._otherHalfOfThisEdge._nextHalfEdge = ePrev;
            ePrev._otherHalfOfThisEdge._nextHalfEdge = eNext;
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
            HalfEdge otherHalfOfEdgeToDelete = edgeToDelete._otherHalfOfThisEdge;
            bool joiningLoops = false;
            // First step: disconnect the origin vertex eDel.Org.  We make all
            // changes to get a consistent mesh in this "intermediate" state.
            if (edgeToDelete._leftFace != edgeToDelete.rightFace)
            {
                // We are joining two loops into one -- remove the left face
                joiningLoops = true;
                KillFace(edgeToDelete._leftFace, edgeToDelete.rightFace);
            }

            if (edgeToDelete._nextEdgeCCWAroundOrigin == edgeToDelete)
            {
                KillVertex(edgeToDelete._originVertex, null);
            }
            else
            {
                // Make sure that eDel.Org and eDel.Rface point to valid half-edges
                edgeToDelete.rightFace._halfEdgeThisIsLeftFaceOf = edgeToDelete.Oprev;
                edgeToDelete._originVertex._edgeThisIsOriginOf = edgeToDelete._nextEdgeCCWAroundOrigin;
                Splice(edgeToDelete, edgeToDelete.Oprev);
                if (!joiningLoops)
                {
                    Face newFace = new Face();
                    // We are splitting one loop into two -- create a new loop for eDel.
                    MakeFace(newFace, edgeToDelete, edgeToDelete._leftFace);
                }
            }

            // Claim: the mesh is now in a consistent state, except that eDel.Org
            // may have been deleted.  Now we disconnect eDel.Dst.
            if (otherHalfOfEdgeToDelete._nextEdgeCCWAroundOrigin == otherHalfOfEdgeToDelete)
            {
                KillVertex(otherHalfOfEdgeToDelete._originVertex, null);
                KillFace(otherHalfOfEdgeToDelete._leftFace, null);
            }
            else
            {
                // Make sure that eDel.Dst and eDel.Lface point to valid half-edges
                edgeToDelete._leftFace._halfEdgeThisIsLeftFaceOf = otherHalfOfEdgeToDelete.Oprev;
                otherHalfOfEdgeToDelete._originVertex._edgeThisIsOriginOf = otherHalfOfEdgeToDelete._nextEdgeCCWAroundOrigin;
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
            eNewSym = eNew._otherHalfOfThisEdge;
            /* Connect the new edge appropriately */
            Splice(eNew, eOrg._nextEdgeCCWAroundLeftFace);
            /* Set the vertex and face information */
            eNew._originVertex = eOrg.DirectionVertex;
            {
                ContourVertex newVertex = new ContourVertex();
                MakeVertex(newVertex, eNewSym, eNew._originVertex);
            }
            eNew._leftFace = eNewSym._leftFace = eOrg._leftFace;
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
            eNew = tempHalfEdge._otherHalfOfThisEdge;
            /* Disconnect eOrg from eOrg.Dst and connect it to eNew.Org */
            Splice(eOrg._otherHalfOfThisEdge, eOrg._otherHalfOfThisEdge.Oprev);
            Splice(eOrg._otherHalfOfThisEdge, eNew);
            /* Set the vertex and face information */
            eOrg.DirectionVertex = eNew._originVertex;
            eNew.DirectionVertex._edgeThisIsOriginOf = eNew._otherHalfOfThisEdge;	/* may have pointed to eOrg.Sym */
            eNew.rightFace = eOrg.rightFace;
            eNew._winding = eOrg._winding;	/* copy old winding information */
            eNew._otherHalfOfThisEdge._winding = eOrg._otherHalfOfThisEdge._winding;
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
            if (eNext._otherHalfOfThisEdge._isFirstHalfEdge)
            {
                eNext = eNext._otherHalfOfThisEdge;
            }

            /* Insert in circular doubly-linked list before eNext.
            * Note that the prev pointer is stored in Sym.next.
            */
            ePrev = eNext._otherHalfOfThisEdge._nextHalfEdge;
            pair.eSym._nextHalfEdge = ePrev;
            ePrev._otherHalfOfThisEdge._nextHalfEdge = pair.e;
            pair.e._nextHalfEdge = eNext;
            eNext._otherHalfOfThisEdge._nextHalfEdge = pair.eSym;
            pair.e._isFirstHalfEdge = true;
            pair.e._otherHalfOfThisEdge = pair.eSym;
            pair.e._nextEdgeCCWAroundOrigin = pair.e;
            pair.e._nextEdgeCCWAroundLeftFace = pair.eSym;
            pair.e._originVertex = null;
            pair.e._leftFace = null;
            pair.e._winding = 0;
            pair.e._regionThisIsUpperEdgeOf = null;
            pair.eSym._isFirstHalfEdge = false;
            pair.eSym._otherHalfOfThisEdge = pair.e;
            pair.eSym._nextEdgeCCWAroundOrigin = pair.eSym;
            pair.eSym._nextEdgeCCWAroundLeftFace = pair.e;
            pair.eSym._originVertex = null;
            pair.eSym._leftFace = null;
            pair.eSym._winding = 0;
            pair.eSym._regionThisIsUpperEdgeOf = null;
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
            eNewSym = eNew._otherHalfOfThisEdge;
            if (eDst._leftFace != eOrg._leftFace)
            {
                /* We are connecting two disjoint loops -- destroy eDst.Lface */
                joiningLoops = true;
                KillFace(eDst._leftFace, eOrg._leftFace);
            }

            /* Connect the new edge appropriately */
            Splice(eNew, eOrg._nextEdgeCCWAroundLeftFace);
            Splice(eNewSym, eDst);
            /* Set the vertex and face information */
            eNew._originVertex = eOrg.DirectionVertex;
            eNewSym._originVertex = eDst._originVertex;
            eNew._leftFace = eNewSym._leftFace = eOrg._leftFace;
            /* Make sure the old face points to a valid half-edge */
            eOrg._leftFace._halfEdgeThisIsLeftFaceOf = eNewSym;
            if (!joiningLoops)
            {
                Face newFace = new Face();
                /* We split one loop into two -- the new loop is eNew.Lface */
                MakeFace(newFace, eNew, eOrg._leftFace);
            }
            return eNew;
        }

        /* __gl_meshUnion( mesh1, mesh2 ) forms the union of all structures in
        * both meshes, and returns the new mesh (the old meshes are destroyed).
        */
        Mesh meshUnion(Mesh mesh1, Mesh mesh2)
        {
            Face f1 = mesh1._faceHead;
            ContourVertex v1 = mesh1._vertexHead;
            HalfEdge e1 = mesh1._halfEdgeHead;
            Face f2 = mesh2._faceHead;
            ContourVertex v2 = mesh2._vertexHead;
            HalfEdge e2 = mesh2._halfEdgeHead;
            /* Add the faces, vertices, and edges of mesh2 to those of mesh1 */
            if (f2._nextFace != f2)
            {
                f1._prevFace._nextFace = f2._nextFace;
                f2._nextFace._prevFace = f1._prevFace;
                f2._prevFace._nextFace = f1;
                f1._prevFace = f2._prevFace;
            }

            if (v2._nextVertex != v2)
            {
                v1._prevVertex._nextVertex = v2._nextVertex;
                v2._nextVertex._prevVertex = v1._prevVertex;
                v2._prevVertex._nextVertex = v1;
                v1._prevVertex = v2._prevVertex;
            }

            if (e2._nextHalfEdge != e2)
            {
                e1._otherHalfOfThisEdge._nextHalfEdge._otherHalfOfThisEdge._nextHalfEdge = e2._nextHalfEdge;
                e2._nextHalfEdge._otherHalfOfThisEdge._nextHalfEdge = e1._otherHalfOfThisEdge._nextHalfEdge;
                e2._otherHalfOfThisEdge._nextHalfEdge._otherHalfOfThisEdge._nextHalfEdge = e1;
                e1._otherHalfOfThisEdge._nextHalfEdge = e2._otherHalfOfThisEdge._nextHalfEdge;
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
            HalfEdge eStart = fZap._halfEdgeThisIsLeftFaceOf;
            HalfEdge e, eNext, eSym;
            Face fPrev, fNext;
            /* walk around face, deleting edges whose right face is also null */
            eNext = eStart._nextEdgeCCWAroundLeftFace;
            do
            {
                e = eNext;
                eNext = e._nextEdgeCCWAroundLeftFace;
                e._leftFace = null;
                if (e.rightFace == null)
                {
                    /* delete the edge -- see __gl_MeshDelete above */

                    if (e._nextEdgeCCWAroundOrigin == e)
                    {
                        KillVertex(e._originVertex, null);
                    }
                    else
                    {
                        /* Make sure that e.Org points to a valid half-edge */
                        e._originVertex._edgeThisIsOriginOf = e._nextEdgeCCWAroundOrigin;
                        Splice(e, e.Oprev);
                    }
                    eSym = e._otherHalfOfThisEdge;
                    if (eSym._nextEdgeCCWAroundOrigin == eSym)
                    {
                        KillVertex(eSym._originVertex, null);
                    }
                    else
                    {
                        /* Make sure that eSym.Org points to a valid half-edge */
                        eSym._originVertex._edgeThisIsOriginOf = eSym._nextEdgeCCWAroundOrigin;
                        Splice(eSym, eSym.Oprev);
                    }
                    KillEdge(e);
                }
            } while (e != eStart);
            /* delete from circular doubly-linked list */
            fPrev = fZap._prevFace;
            fNext = fZap._nextFace;
            fNext._prevFace = fPrev;
            fPrev._nextFace = fNext;
            fZap = null;
        }

        /* __gl_meshCheckMesh( mesh ) checks a mesh for self-consistency.
        */
        public void CheckMesh()
        {
            Face fHead = _faceHead;
            ContourVertex vHead = _vertexHead;
            HalfEdge eHead = _halfEdgeHead;
            Face f, fPrev;
            ContourVertex v, vPrev;
            HalfEdge e, ePrev;
            fPrev = fHead;
            for (fPrev = fHead; (f = fPrev._nextFace) != fHead; fPrev = f)
            {
                if (f._prevFace != fPrev)
                {
                    throw new Exception();
                }
                e = f._halfEdgeThisIsLeftFaceOf;
                do
                {
                    if (e._otherHalfOfThisEdge == e)
                    {
                        throw new Exception();
                    }
                    if (e._otherHalfOfThisEdge._otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e._nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace != e)
                    {
                        throw new Exception();
                    }
                    if (e._leftFace != f)
                    {
                        throw new Exception();
                    }
                    e = e._nextEdgeCCWAroundLeftFace;
                } while (e != f._halfEdgeThisIsLeftFaceOf);
            }
            if (f._prevFace != fPrev || f._halfEdgeThisIsLeftFaceOf != null)
            {
                throw new Exception();
            }

            vPrev = vHead;
            for (vPrev = vHead; (v = vPrev._nextVertex) != vHead; vPrev = v)
            {
                if (v._prevVertex != vPrev)
                {
                    throw new Exception();
                }
                e = v._edgeThisIsOriginOf;
                do
                {
                    if (e._otherHalfOfThisEdge == e)
                    {
                        throw new Exception();
                    }
                    if (e._otherHalfOfThisEdge._otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e._nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge != e)
                    {
                        throw new Exception();
                    }
                    if (e._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace != e)
                    {
                        throw new Exception();
                    }
                    if (e._originVertex != v)
                    {
                        throw new Exception();
                    }
                    e = e._nextEdgeCCWAroundOrigin;
                } while (e != v._edgeThisIsOriginOf);
            }
            if (v._prevVertex != vPrev || v._edgeThisIsOriginOf != null || v._clientIndex != 0)
            {
                throw new Exception();
            }

            ePrev = eHead;
            for (ePrev = eHead; (e = ePrev._nextHalfEdge) != eHead; ePrev = e)
            {
                if (e._otherHalfOfThisEdge._nextHalfEdge != ePrev._otherHalfOfThisEdge)
                {
                    throw new Exception();
                }
                if (e._otherHalfOfThisEdge == e)
                {
                    throw new Exception();
                }
                if (e._otherHalfOfThisEdge._otherHalfOfThisEdge != e)
                {
                    throw new Exception();
                }
                if (e._originVertex == null)
                {
                    throw new Exception();
                }
                if (e.DirectionVertex == null)
                {
                    throw new Exception();
                }
                if (e._nextEdgeCCWAroundLeftFace._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge != e)
                {
                    throw new Exception();
                }
                if (e._nextEdgeCCWAroundOrigin._otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace != e)
                {
                    throw new Exception();
                }
            }
            if (e._otherHalfOfThisEdge._nextHalfEdge != ePrev._otherHalfOfThisEdge
                || e._otherHalfOfThisEdge != _otherHalfOfThisEdgeHead
                || e._otherHalfOfThisEdge._otherHalfOfThisEdge != e
                || e._originVertex != null || e.DirectionVertex != null
                || e._leftFace != null || e.rightFace != null)
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
            for (e = _halfEdgeHead._nextHalfEdge; e != _halfEdgeHead; e = eNext)
            {
                eNext = e._nextHalfEdge;
                if (e.rightFace._isInterior != e._leftFace._isInterior)
                {
                    /* This is a boundary edge (one side is interior, one is exterior). */
                    e._winding = (e._leftFace._isInterior) ? value : -value;
                }
                else
                {
                    /* Both regions are interior, or both are exterior. */
                    if (!keepOnlyBoundary)
                    {
                        e._winding = 0;
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
            for (f = _faceHead._nextFace; f != _faceHead; f = next)
            {
                /* Since f will be destroyed, save its next pointer. */
                next = f._nextFace;
                if (!f._isInterior)
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
            for (f = _faceHead._nextFace; f != _faceHead; f = next)
            {
                /* Make sure we don''t try to tessellate the new triangles. */
                next = f._nextFace;
                if (f._isInterior)
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