//BSD, 2014-2017, WinterDev

//MatterHackers
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.Agg.Lines
{
    //==========================================================line_parameters
    public class LineParameters
    {
        //---------------------------------------------------------------------
        public int x1, y1, x2, y2, dx, dy, sx, sy;
        public bool vertical;
        public int inc;
        public int len;
        public int octant;
        // The number of the octant is determined as a 3-bit value as follows:
        // bit 0 = vertical flag
        // bit 1 = sx < 0
        // bit 2 = sy < 0
        //
        // [N] shows the number of the orthogonal quadrant
        // <M> shows the number of the diagonal quadrant
        //               <1>
        //   [1]          |          [0]
        //       . (3)011 | 001(1) .
        //         .      |      .
        //           .    |    . 
        //             .  |  . 
        //    (2)010     .|.     000(0)
        // <2> ----------.+.----------- <0>
        //    (6)110   .  |  .   100(4)
        //           .    |    .
        //         .      |      .
        //       .        |        .
        //         (7)111 | 101(5) 
        //   [2]          |          [3]
        //               <3> 
        //                                                        0,1,2,3,4,5,6,7 
        public static readonly byte[] s_orthogonal_quadrant = { 0, 0, 1, 1, 3, 3, 2, 2 };
        public static readonly byte[] s_diagonal_quadrant = { 0, 1, 2, 1, 0, 3, 2, 3 };
        //---------------------------------------------------------------------
        public LineParameters(int x1_, int y1_, int x2_, int y2_, int len_)
        {
            x1 = (x1_);
            y1 = (y1_);
            x2 = (x2_);
            y2 = (y2_);
            dx = (Math.Abs(x2_ - x1_));
            dy = (Math.Abs(y2_ - y1_));
            sx = ((x2_ > x1_) ? 1 : -1);
            sy = ((y2_ > y1_) ? 1 : -1);
            vertical = (dy >= dx);
            inc = (vertical ? sy : sx);
            len = (len_);
            octant = ((sy & 4) | (sx & 2) | (vertical ? 1 : 0));
        }

        //---------------------------------------------------------------------
        public uint OrthogonalQuadrant { get { return s_orthogonal_quadrant[octant]; } }
        public uint DiagonalQuadrant { get { return s_diagonal_quadrant[octant]; } }

        //---------------------------------------------------------------------
        public bool IsSameOrthogonalQuadrant(LineParameters lp)
        {
            return s_orthogonal_quadrant[octant] == s_orthogonal_quadrant[lp.octant];
        }

        //---------------------------------------------------------------------
        public bool IsSameDiagonalQuadrant(LineParameters lp)
        {
            return s_diagonal_quadrant[octant] == s_diagonal_quadrant[lp.octant];
        }

        //---------------------------------------------------------------------
        public void Divide(out LineParameters lp1, out LineParameters lp2)
        {
            int xmid = (x1 + x2) >> 1;
            int ymid = (y1 + y2) >> 1;
            int len2 = len >> 1;
            //lp1 = this; // it is a struct so this is a copy
            //lp2 = this; // it is a struct so this is a copy

            lp1 = new LineParameters(this.x1, this.y1, this.x2, this.y2, this.len);
            lp2 = new LineParameters(this.x1, this.y1, this.x2, this.y2, this.len);
            lp1.x2 = xmid;
            lp1.y2 = ymid;
            lp1.len = len2;
            lp1.dx = Math.Abs(lp1.x2 - lp1.x1);
            lp1.dy = Math.Abs(lp1.y2 - lp1.y1);
            lp2.x1 = xmid;
            lp2.y1 = ymid;
            lp2.len = len2;
            lp2.dx = Math.Abs(lp2.x2 - lp2.x1);
            lp2.dy = Math.Abs(lp2.y2 - lp2.y1);
        }
    };
}