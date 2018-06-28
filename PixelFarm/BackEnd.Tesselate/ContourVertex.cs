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
    public class ContourVertex : IComparable<ContourVertex>
    {
        public ContourVertex nextVertex;		/* next vertex (never null) */
        public ContourVertex prevVertex;		/* previous vertex (never null) */
        public HalfEdge edgeThisIsOriginOf;	/* a half-edge with this origin */
        public int clientIndex;		/* client's data */
        /* Internal data (keep hidden) */
        /* vertex location in 3D */

        internal double C_0;
        internal double C_1;
        internal double C_2;
        public double x, y;		/* projection onto the sweep plane */
        internal RefItem<ContourVertex> priorityQueueHandle;	/* to allow deletion from priority queue */
        public int CompareTo(ContourVertex otherVertex)
        {
            if (VertEq(otherVertex))
            {
                return 0;
            }
            if (this.VertLeq(otherVertex))
            {
                return -1;
            }
            return 1;
        }

        public bool Equal2D(ContourVertex OtherVertex)
        {
            return (this.x == OtherVertex.x && this.y == OtherVertex.y);
        }

        public static double EdgeEval(ContourVertex u, ContourVertex v, ContourVertex w)
        {
            /* Given three vertices u,v,w such that VertLeq(u,v) && VertLeq(v,w),
             * evaluates the t-coord of the edge uw at the s-coord of the vertex v.
             * Returns v.t - (uw)(v.s), ie. the signed distance from uw to v.
             * If uw is vertical (and thus passes thru v), the result is zero.
             *
             * The calculation is extremely accurate and stable, even when v
             * is very close to u or w.  In particular if we set v.t = 0 and
             * let r be the negated result (this evaluates (uw)(v.s)), then
             * r is guaranteed to satisfy MIN(u.t,w.t) <= r <= MAX(u.t,w.t).
             */
            double gapL, gapR;
            if (!((u.VertLeq(u) && v.VertLeq(v))))
            {
                throw new Exception();
            }

            gapL = v.x - u.x;
            gapR = w.x - v.x;
            if (gapL + gapR > 0)
            {
                if (gapL < gapR)
                {
                    return (v.y - u.y) + (u.y - w.y) * (gapL / (gapL + gapR));
                }
                else
                {
                    return (v.y - w.y) + (w.y - u.y) * (gapR / (gapL + gapR));
                }
            }

            // vertical line
            return 0;
        }

        static public double EdgeSign(ContourVertex u, ContourVertex v, ContourVertex w)
        {
            /* Returns a number whose sign matches EdgeEval(u,w) but which
             * is cheaper to evaluate.  Returns > 0, == 0 , or < 0
             * as v is above, on, or below the edge uw.
             */
            double gapL, gapR;
            if (!u.VertLeq(v) || !v.VertLeq(w))
            {
                throw new System.Exception();
            }

            gapL = v.x - u.x;
            gapR = w.x - v.x;
            if (gapL + gapR > 0)
            {
                return (v.y - w.y) * gapL + (v.y - u.y) * gapR;
            }
            /* vertical line */
            return 0;
        }

        public bool VertEq(ContourVertex v)
        {
            return ((this.x == v.x) && this.y == v.y);
        }

        public bool VertLeq(ContourVertex v)
        {
            return ((this.x < v.x) || (this.x == v.x && this.y <= v.y));
        }

        public bool TransLeq(ContourVertex v)
        {
            return ((this.y < v.y) || (this.y == v.y && this.x <= v.x));
        }

        public static bool VertCCW(ContourVertex u, ContourVertex v, ContourVertex w)
        {
            /* For almost-degenerate situations, the results are not reliable.
             * Unless the floating-point arithmetic can be performed without
             * rounding errors, *any* implementation will give incorrect results
             * on some degenerate inputs, so the client must have some way to
             * handle this situation.
             */
            return (u.x * (v.y - w.y) + v.x * (w.y - u.y) + w.x * (u.y - v.y)) >= 0;
        }

#if DEBUG
        public override string ToString()
        {
            return this.C_0 + "," + this.C_1 + "," + this.C_2;
        }
#endif
    }
}