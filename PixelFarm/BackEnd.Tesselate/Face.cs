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

using System;
namespace Tesselate
{
    public class Face
    {
        public int indexDebug;
        public Face nextFace;		/* next face (never null) */
        public Face prevFace;		/* previous face (never null) */
        public HalfEdge halfEdgeThisIsLeftFaceOf;	/* a half edge with this left face */
        /* Internal data (keep hidden) */
        public Face trail;		/* "stack" for conversion to strips */
        public bool marked;		/* flag for conversion to strips */
        public bool isInterior;		/* this face is in the polygon interior */
        /* Macros which keep track of faces we have marked temporarily, and allow
        * us to backtrack when necessary.  With triangle fans, this is not
        * really necessary, since the only awkward case is a loop of triangles
        * around a single origin vertex.  However with strips the situation is
        * more complicated, and we need a general tracking method like the
        * one here.
        */
        public bool Marked()
        {
            return (!this.isInterior || this.marked);
        }

        public static void AddToTrail(ref Face f, ref Face t)
        {
            f.trail = t;
            t = f;
            f.marked = true;
        }

        static public void FreeTrail(ref Face t)
        {
            while (t != null)
            {
                t.marked = false;
                t = t.trail;
            }
        }

        /* face.TessellateMonoRegion() tessellates a monotone region
        * (what else would it do??)  The region must consist of a single
        * loop of half-edges (see mesh.h) oriented CCW.  "Monotone" in this
        * case means that any vertical line intersects the interior of the
        * region in a single interval.  
        *
        * Tessellation consists of adding interior edges (actually pairs of
        * half-edges), to split the region into non-overlapping triangles.
        *
        * The basic idea is explained in Preparata and Shamos (which I don''t
        * have handy right now), although their implementation is more
        * complicated than this one.  The are two edge chains, an upper chain
        * and a lower chain.  We process all vertices from both chains in order,
        * from right to left.
        *
        * The algorithm ensures that the following invariant holds after each
        * vertex is processed: the untessellated region consists of two
        * chains, where one chain (say the upper) is a single edge, and
        * the other chain is concave.  The left vertex of the single edge
        * is always to the left of all vertices in the concave chain.
        *
        * Each step consists of adding the rightmost unprocessed vertex to one
        * of the two chains, and forming a fan of triangles from the rightmost
        * of two chain endpoints.  Determining whether we can add each triangle
        * to the fan is a simple orientation test.  By making the fan as large
        * as possible, we restore the invariant (check it yourself).
        */
        internal bool TessellateMonoRegion()
        {
            /* All edges are oriented CCW around the boundary of the region.
            * First, find the half-edge whose origin vertex is rightmost.
            * Since the sweep goes from left to right, face.anEdge should
            * be close to the edge we want.
            */
            HalfEdge up = this.halfEdgeThisIsLeftFaceOf;
            if (up.nextEdgeCCWAroundLeftFace == up || up.nextEdgeCCWAroundLeftFace.nextEdgeCCWAroundLeftFace == up)
            {
                throw new Exception();
            }

            for (; up.directionVertex.VertLeq(up.originVertex); up = up.Lprev)
                ;
            for (; up.originVertex.VertLeq(up.directionVertex); up = up.nextEdgeCCWAroundLeftFace)
                ;
            HalfEdge lo = up.Lprev;
            while (up.nextEdgeCCWAroundLeftFace != lo)
            {
                if (up.directionVertex.VertLeq(lo.originVertex))
                {
                    /* up.Dst is on the left.  It is safe to form triangles from lo.Org.
                    * The EdgeGoesLeft test guarantees progress even when some triangles
                    * are CW, given that the upper and lower chains are truly monotone.
                    */
                    while (lo.nextEdgeCCWAroundLeftFace != up && (lo.nextEdgeCCWAroundLeftFace.EdgeGoesLeft()
                        || ContourVertex.EdgeSign(lo.originVertex, lo.directionVertex, lo.nextEdgeCCWAroundLeftFace.directionVertex) <= 0))
                    {
                        HalfEdge tempHalfEdge = Mesh.meshConnect(lo.nextEdgeCCWAroundLeftFace, lo);
                        lo = tempHalfEdge.otherHalfOfThisEdge;
                    }
                    lo = lo.Lprev;
                }
                else
                {
                    /* lo.Org is on the left.  We can make CCW triangles from up.Dst. */
                    while (lo.nextEdgeCCWAroundLeftFace != up && (up.Lprev.EdgeGoesRight()
                        || ContourVertex.EdgeSign(up.directionVertex, up.originVertex, up.Lprev.originVertex) >= 0))
                    {
                        HalfEdge tempHalfEdge = Mesh.meshConnect(up, up.Lprev);
                        up = tempHalfEdge.otherHalfOfThisEdge;
                    }
                    up = up.nextEdgeCCWAroundLeftFace;
                }
            }

            // Now lo.Org == up.Dst == the leftmost vertex.  The remaining region
            // can be tessellated in a fan from this leftmost vertex.
            if (lo.nextEdgeCCWAroundLeftFace == up)
            {
                throw new Exception();
            }
            while (lo.nextEdgeCCWAroundLeftFace.nextEdgeCCWAroundLeftFace != up)
            {
                HalfEdge tempHalfEdge = Mesh.meshConnect(lo.nextEdgeCCWAroundLeftFace, lo);
                lo = tempHalfEdge.otherHalfOfThisEdge;
            }

            return true;
        }
    }
}