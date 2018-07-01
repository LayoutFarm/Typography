//BSD, 2014-present, WinterDev

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
namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    //==========================================================line_parameters
    public struct LineParameters
    {
        //---------------------------------------------------------------------

        public readonly int x1, y1, x2, y2, len;
        readonly byte octant;
        public readonly bool vertical;
        public readonly short inc;


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
        static readonly byte[] s_orthogonal_quadrant = { 0, 0, 1, 1, 3, 3, 2, 2 };
        static readonly byte[] s_diagonal_quadrant = { 0, 1, 2, 1, 0, 3, 2, 3 };
        //---------------------------------------------------------------------
        public LineParameters(int x1, int y1, int x2, int y2, int len)
        {
            this.x1 = (x1);
            this.y1 = (y1);
            this.x2 = (x2);
            this.y2 = (y2);

            int sx = ((x2 > x1) ? 1 : -1); //sx =1 or -1
            int sy = ((y2 > y1) ? 1 : -1); //sy = 1 or -1


            //assign vertical value and evaluate inc value ***
            this.inc = (short)((vertical = ((Math.Abs(x2 - x1)) >= (Math.Abs(y2 - y1)))) ?
                        sy :
                        sx); //inc is 1 or -1

            this.len = (len);

            //1 byte is enough
            this.octant = (byte)((sy & 4) | (sx & 2) | (vertical ? 1 : 0));
        }

        //---------------------------------------------------------------------
        public uint OrthogonalQuadrant { get { return s_orthogonal_quadrant[octant]; } }
        public uint DiagonalQuadrant { get { return s_diagonal_quadrant[octant]; } }

        public int dx
        {
            get
            {
                return Math.Abs(x2 - x1);
            }
        }
        public int dy
        {
            get
            {
                return Math.Abs(y2 - y1);
            }
        }
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

        public void HalfDivide(out LineParameters lp1, out LineParameters lp2)
        {
            int xmid = (x1 + x2) >> 1;
            int ymid = (y1 + y2) >> 1;
            int len2 = len >> 1;

            lp1 = new LineParameters(this.x1, this.y1, xmid, ymid, len2);
            lp2 = new LineParameters(xmid, ymid, this.x2, this.y2, this.len);
            //lp1.x2 = xmid;
            //lp1.y2 = ymid;
            //lp1.len = len2;
            //lp1.dx = Math.Abs(lp1.x2 - lp1.x1);
            //lp1.dy = Math.Abs(lp1.y2 - lp1.y1);
            ////------------------------------------
            //lp2.x1 = xmid;
            //lp2.y1 = ymid;
            //lp2.len = len2;
            //lp2.dx = Math.Abs(lp2.x2 - lp2.x1);
            //lp2.dy = Math.Abs(lp2.y2 - lp2.y1);


            //old version
            //lp1 = new LineParameters(this.x1, this.y1, this.x2, this.y2, this.len);
            //lp2 = new LineParameters(this.x1, this.y1, this.x2, this.y2, this.len);
            //lp1.x2 = xmid;
            //lp1.y2 = ymid;
            //lp1.len = len2;
            //lp1.dx = Math.Abs(lp1.x2 - lp1.x1);
            //lp1.dy = Math.Abs(lp1.y2 - lp1.y1);
            ////------------------------------------
            //lp2.x1 = xmid;
            //lp2.y1 = ymid;
            //lp2.len = len2;
            //lp2.dx = Math.Abs(lp2.x2 - lp2.x1);
            //lp2.dy = Math.Abs(lp2.y2 - lp2.y1);
        }
    };
}