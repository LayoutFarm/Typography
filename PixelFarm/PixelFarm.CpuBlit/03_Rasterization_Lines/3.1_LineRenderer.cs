//BSD, 2014-present, WinterDev
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
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    public abstract class LineRenderer
    {

        public delegate bool CompareFunction(int value);
        public Color Color { get; set; }
        public abstract void SemiDot(CompareFunction cmp, int xc1, int yc1, int xc2, int yc2);
        public abstract void SemiDotHLine(CompareFunction cmp, int xc1, int yc1, int xc2, int yc2, int x1, int y1, int x2);
        public abstract void Pie(int xc, int yc, int x1, int y1, int x2, int y2);
        public abstract void Line0(LineParameters lp);
        public abstract void Line1(LineParameters lp, int sx, int sy);
        public abstract void Line2(LineParameters lp, int ex, int ey);
        public abstract void Line3(LineParameters lp, int sx, int sy, int ex, int ey);
    }

    //-----------------------------------------------------------line_coord_sat
    static class LineCoordSat
    {
        public static int Convert(double x)
        {
            return AggMath.iround(
                x * LineAA.SUBPIXEL_SCALE,
                LineAA.SUBPIXEL_COORD);
        }
    }
}