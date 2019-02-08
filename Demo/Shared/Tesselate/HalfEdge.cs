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

namespace Tesselate
{
    public class HalfEdge
    {
#if DEBUG
        public int debugIndex;
#endif
        internal bool _isFirstHalfEdge;
        internal HalfEdge _nextHalfEdge;		/* doubly-linked list (prev==Sym.next) */
        internal HalfEdge _otherHalfOfThisEdge;		/* same edge, opposite direction */
        internal HalfEdge _nextEdgeCCWAroundOrigin;		/* next edge CCW around origin */
        internal HalfEdge _nextEdgeCCWAroundLeftFace;		/* next edge CCW around left face */
        internal ContourVertex _originVertex;		/* origin vertex */
        internal Face _leftFace;		/* left face */
        /* Internal data (keep hidden) */
        internal ActiveRegion _regionThisIsUpperEdgeOf;	/* a region with this upper edge (sweep.c) */
        internal int _winding;	// change in winding number when crossing from the right face to the left face
        internal Face rightFace { get => _otherHalfOfThisEdge._leftFace; set => _otherHalfOfThisEdge._leftFace = value; }

        internal HalfEdge Lprev { get => _nextEdgeCCWAroundOrigin._otherHalfOfThisEdge; set => _nextEdgeCCWAroundOrigin._otherHalfOfThisEdge = value; }
        internal HalfEdge Oprev => _otherHalfOfThisEdge._nextEdgeCCWAroundLeftFace;

        internal ContourVertex DirectionVertex { get => _otherHalfOfThisEdge._originVertex; set => _otherHalfOfThisEdge._originVertex = value; }
        internal HalfEdge Dnext => Rprev._otherHalfOfThisEdge;
        internal HalfEdge Dprev => _nextEdgeCCWAroundLeftFace._otherHalfOfThisEdge;
        internal HalfEdge Rprev => _otherHalfOfThisEdge._nextEdgeCCWAroundOrigin;

        internal bool EdgeGoesLeft()
        {
            return this.DirectionVertex.VertLeq(_originVertex);
        }

        internal bool EdgeGoesRight()
        {
            return _originVertex.VertLeq(this.DirectionVertex);
        }
    }
}