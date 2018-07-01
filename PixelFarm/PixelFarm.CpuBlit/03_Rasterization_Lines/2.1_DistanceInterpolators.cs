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

namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    //===================================================distance_interpolator0
    struct DistanceInterpolator0
    {
        readonly int m_dx;
        readonly int m_dy;
        int m_dist;
        public DistanceInterpolator0(int x1, int y1, int x2, int y2, int x, int y)
        {
            unchecked
            {
                m_dx = (LineAA.Mr(x2) - LineAA.Mr(x1));
                m_dy = (LineAA.Mr(y2) - LineAA.Mr(y1));
                m_dist = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x2)) * m_dy -
                       (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y2)) * m_dx);
                m_dx <<= LineAA.MR_SUBPIXEL_SHIFT;
                m_dy <<= LineAA.MR_SUBPIXEL_SHIFT;
            }
        }
        public void IncX() { m_dist += m_dy; }
        public int Distance { get { return m_dist; } }
    }

    //==================================================distance_interpolator00
    struct DistanceInterpolator00
    {
        readonly int m_dx1;
        readonly int m_dy1;
        readonly int m_dx2;
        readonly int m_dy2;
        int m_dist1;
        int m_dist2;
        public DistanceInterpolator00(int xc, int yc,
                                int x1, int y1, int x2, int y2,
                                int x, int y)
        {
            m_dx1 = (LineAA.Mr(x1) - LineAA.Mr(xc));
            m_dy1 = (LineAA.Mr(y1) - LineAA.Mr(yc));
            m_dx2 = (LineAA.Mr(x2) - LineAA.Mr(xc));
            m_dy2 = (LineAA.Mr(y2) - LineAA.Mr(yc));
            m_dist1 = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x1)) * m_dy1 -
                    (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y1)) * m_dx1);
            m_dist2 = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x2)) * m_dy2 -
                    (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y2)) * m_dx2);
            m_dx1 <<= LineAA.MR_SUBPIXEL_SHIFT;
            m_dy1 <<= LineAA.MR_SUBPIXEL_SHIFT;
            m_dx2 <<= LineAA.MR_SUBPIXEL_SHIFT;
            m_dy2 <<= LineAA.MR_SUBPIXEL_SHIFT;
        }

        //---------------------------------------------------------------------
        public void IncX() { m_dist1 += m_dy1; m_dist2 += m_dy2; }
        public int Distance1 { get { return m_dist1; } }
        public int Distance2 { get { return m_dist2; } }
    }

    //===================================================distance_interpolator1
    struct DistanceInterpolator1
    {
        readonly int m_dx;
        readonly int m_dy;
        int m_dist;
        public DistanceInterpolator1(int x1, int y1, int x2, int y2, int x, int y)
        {
            m_dx = (x2 - x1);
            m_dy = (y2 - y1);
            m_dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(m_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(m_dx)));
            m_dx <<= LineAA.SUBPIXEL_SHIFT;
            m_dy <<= LineAA.SUBPIXEL_SHIFT;
        }


        public void IncX(int dy)
        {
            m_dist += m_dy;
            if (dy > 0)
            {
                m_dist -= m_dx;
            }
            else if (dy < 0)
            {
                m_dist += m_dx;
            }
        }


        public void DecX(int dy)
        {
            m_dist -= m_dy;
            if (dy > 0)
            {
                m_dist -= m_dx;
            }
            else if (dy < 0)
            {
                m_dist += m_dx;
            }
        }


        public void IncY(int dx)
        {
            m_dist -= m_dx;
            if (dx > 0)
            {
                m_dist += m_dy;
            }
            else if (dx < 0)
            {
                m_dist -= m_dy;
            }
        }
        public void DecY(int dx)
        {
            m_dist += m_dx;
            if (dx > 0)
            {
                m_dist += m_dy;
            }
            else if (dx < 0)
            {
                m_dist -= m_dy;
            }
        }
        //---------------------------------------------------------------------
        public int Distance { get { return m_dist; } }

        //public int dx() { return m_dx; }
        //public int dy() { return m_dy; }
        ////---------------------------------------------------------------------
        //public void inc_x() { m_dist += m_dy; }
        //public void dec_x() { m_dist -= m_dy; }
        //public void inc_y() { m_dist -= m_dx; }
        //public void dec_y() { m_dist += m_dx; } 
        //---------------------------------------------------------------------

    
    }

    //===================================================distance_interpolator2
    struct DistanceInterpolator2
    {
        readonly int m_dx;
        readonly int m_dy;
        readonly int m_dx_start;
        readonly int m_dy_start;
        int m_dist;
        int m_dist_start;
        //---------------------------------------------------------------------

        public DistanceInterpolator2(int x1, int y1, int x2, int y2,
                               int sx, int sy, int x, int y)
        {
            m_dx = (x2 - x1);
            m_dy = (y2 - y1);
            m_dx_start = (LineAA.Mr(sx) - LineAA.Mr(x1));
            m_dy_start = (LineAA.Mr(sy) - LineAA.Mr(y1));
            m_dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(m_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(m_dx)));
            m_dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sx)) * m_dy_start -
                         (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sy)) * m_dx_start);
            m_dx <<= LineAA.SUBPIXEL_SHIFT;
            m_dy <<= LineAA.SUBPIXEL_SHIFT;
            m_dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
            m_dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
        }

        public DistanceInterpolator2(int x1, int y1, int x2, int y2,
                               int ex, int ey, int x, int y, int none)
        {
            m_dx = (x2 - x1);
            m_dy = (y2 - y1);
            m_dx_start = (LineAA.Mr(ex) - LineAA.Mr(x2));
            m_dy_start = (LineAA.Mr(ey) - LineAA.Mr(y2));
            m_dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(m_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(m_dx)));
            m_dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ex)) * m_dy_start -
                         (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ey)) * m_dx_start);
            m_dx <<= LineAA.SUBPIXEL_SHIFT;
            m_dy <<= LineAA.SUBPIXEL_SHIFT;
            m_dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
            m_dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
        }



        //---------------------------------------------------------------------
        public void IncX(int dy)
        {
            m_dist += m_dy;
            m_dist_start += m_dy_start;
            if (dy > 0)
            {
                m_dist -= m_dx;
                m_dist_start -= m_dx_start;
            }
            else if (dy < 0)
            {
                m_dist += m_dx;
                m_dist_start += m_dx_start;
            }
        }

        //---------------------------------------------------------------------
        public void DecX(int dy)
        {
            m_dist -= m_dy;
            m_dist_start -= m_dy_start;
            if (dy > 0)
            {
                m_dist -= m_dx;
                m_dist_start -= m_dx_start;
            }
            else if (dy < 0)
            {
                m_dist += m_dx;
                m_dist_start += m_dx_start;
            }
        }

        //---------------------------------------------------------------------
        public void IncY(int dx)
        {
            m_dist -= m_dx;
            m_dist_start -= m_dx_start;
            if (dx > 0)
            {
                m_dist += m_dy;
                m_dist_start += m_dy_start;
            }
            else if (dx < 0)
            {
                m_dist -= m_dy;
                m_dist_start -= m_dy_start;
            }
        }

        //---------------------------------------------------------------------
        public void DecY(int dx)
        {
            m_dist += m_dx;
            m_dist_start += m_dx_start;
            if (dx > 0)
            {
                m_dist += m_dy;
                m_dist_start += m_dy_start;
            }
            else if (dx < 0)
            {
                m_dist -= m_dy;
                m_dist_start -= m_dy_start;
            }
        }

        //---------------------------------------------------------------------
        public int Distance { get { return m_dist; } }
        public int DistanceStart { get { return m_dist_start; } }
        public int DistanceEnd { get { return m_dist_start; } }

        //---------------------------------------------------------------------

        public int DxStart { get { return m_dx_start; } }
        public int DyStart { get { return m_dy_start; } }
        public int DxEnd { get { return m_dx_start; } }
        public int DyEnd { get { return m_dy_start; } }

        //public int dx() { return m_dx; }
        //public int dy() { return m_dy; }
        //---------------------------------------------------------------------
        //public void inc_x() { m_dist += m_dy; m_dist_start += m_dy_start; }
        //public void dec_x() { m_dist -= m_dy; m_dist_start -= m_dy_start; }
        //public void inc_y() { m_dist -= m_dx; m_dist_start -= m_dx_start; }
        //public void dec_y() { m_dist += m_dx; m_dist_start += m_dx_start; }
    }


    //===================================================distance_interpolator3
    struct DistanceInterpolator3
    {
        readonly int m_dx;
        readonly int m_dy;
        readonly int m_dx_start;
        readonly int m_dy_start;
        readonly int m_dx_end;
        readonly int m_dy_end;
        int m_dist;
        int m_dist_start;
        int m_dist_end;
        //---------------------------------------------------------------------

        public DistanceInterpolator3(int x1, int y1, int x2, int y2,
                               int sx, int sy, int ex, int ey,
                               int x, int y)
        {
            unchecked
            {
                m_dx = (x2 - x1);
                m_dy = (y2 - y1);
                m_dx_start = (LineAA.Mr(sx) - LineAA.Mr(x1));
                m_dy_start = (LineAA.Mr(sy) - LineAA.Mr(y1));
                m_dx_end = (LineAA.Mr(ex) - LineAA.Mr(x2));
                m_dy_end = (LineAA.Mr(ey) - LineAA.Mr(y2));
                m_dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(m_dy) -
                              (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(m_dx)));
                m_dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sx)) * m_dy_start -
                             (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sy)) * m_dx_start);
                m_dist_end = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ex)) * m_dy_end -
                           (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ey)) * m_dx_end);
                m_dx <<= LineAA.SUBPIXEL_SHIFT;
                m_dy <<= LineAA.SUBPIXEL_SHIFT;
                m_dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
                m_dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
                m_dx_end <<= LineAA.MR_SUBPIXEL_SHIFT;
                m_dy_end <<= LineAA.MR_SUBPIXEL_SHIFT;
            }
        }


        public void IncX(int dy)
        {
            m_dist += m_dy;
            m_dist_start += m_dy_start;
            m_dist_end += m_dy_end;
            if (dy > 0)
            {
                m_dist -= m_dx;
                m_dist_start -= m_dx_start;
                m_dist_end -= m_dx_end;
            }
            if (dy < 0)
            {
                m_dist += m_dx;
                m_dist_start += m_dx_start;
                m_dist_end += m_dx_end;
            }
        }

        public void DecX(int dy)
        {
            m_dist -= m_dy;
            m_dist_start -= m_dy_start;
            m_dist_end -= m_dy_end;
            if (dy > 0)
            {
                m_dist -= m_dx;
                m_dist_start -= m_dx_start;
                m_dist_end -= m_dx_end;
            }
            if (dy < 0)
            {
                m_dist += m_dx;
                m_dist_start += m_dx_start;
                m_dist_end += m_dx_end;
            }
        }

        public void IncY(int dx)
        {
            m_dist -= m_dx;
            m_dist_start -= m_dx_start;
            m_dist_end -= m_dx_end;
            if (dx > 0)
            {
                m_dist += m_dy;
                m_dist_start += m_dy_start;
                m_dist_end += m_dy_end;
            }
            if (dx < 0)
            {
                m_dist -= m_dy;
                m_dist_start -= m_dy_start;
                m_dist_end -= m_dy_end;
            }
        }

        public void DecY(int dx)
        {
            m_dist += m_dx;
            m_dist_start += m_dx_start;
            m_dist_end += m_dx_end;
            if (dx > 0)
            {
                m_dist += m_dy;
                m_dist_start += m_dy_start;
                m_dist_end += m_dy_end;
            }
            if (dx < 0)
            {
                m_dist -= m_dy;
                m_dist_start -= m_dy_start;
                m_dist_end -= m_dy_end;
            }
        }

        public int Distance { get { return m_dist; } }
        public int dist_start { get { return m_dist_start; } }
        public int dist_end { get { return m_dist_end; } }


        public int DxStart { get { return m_dx_start; } }
        public int DyStart { get { return m_dy_start; } }
        public int DxEnd { get { return m_dx_end; } }
        public int DyEnd { get { return m_dy_end; } }

        //int dx() { return m_dx; }
        //int dy() { return m_dy; }
        //void inc_x() { m_dist += m_dy; m_dist_start += m_dy_start; m_dist_end += m_dy_end; }
        //void dec_x() { m_dist -= m_dy; m_dist_start -= m_dy_start; m_dist_end -= m_dy_end; }
        //void inc_y() { m_dist -= m_dx; m_dist_start -= m_dx_start; m_dist_end -= m_dx_end; }
        //void dec_y() { m_dist += m_dx; m_dist_start += m_dx_start; m_dist_end += m_dx_end; }
    }

    //public class DistanceInterpolator4
    //{
    //    int m_dx;
    //    int m_dy;
    //    int m_dx_start;
    //    int m_dy_start;
    //    int m_dx_pict;
    //    int m_dy_pict;
    //    int m_dx_end;
    //    int m_dy_end;

    //    int m_dist;
    //    int m_dist_start;
    //    int m_dist_pict;
    //    int m_dist_end;
    //    int m_len;

    //    //---------------------------------------------------------------------

    //    public DistanceInterpolator4(int x1, int y1, int x2, int y2,
    //                           int sx, int sy, int ex, int ey,
    //                           int len, double scale, int x, int y)
    //    {
    //        m_dx = (x2 - x1);
    //        m_dy = (y2 - y1);
    //        m_dx_start = (LineAABasics.line_mr(sx) - LineAABasics.line_mr(x1));
    //        m_dy_start = (LineAABasics.line_mr(sy) - LineAABasics.line_mr(y1));
    //        m_dx_end = (LineAABasics.line_mr(ex) - LineAABasics.line_mr(x2));
    //        m_dy_end = (LineAABasics.line_mr(ey) - LineAABasics.line_mr(y2));

    //        m_dist = (AggBasics.iround((double)(x + LineAABasics.SUBPIXEL_SCALE / 2 - x2) * (double)(m_dy) -
    //                      (double)(y + LineAABasics.SUBPIXEL_SCALE / 2 - y2) * (double)(m_dx)));

    //        m_dist_start = ((LineAABasics.line_mr(x + LineAABasics.SUBPIXEL_SCALE / 2) - LineAABasics.line_mr(sx)) * m_dy_start -
    //                     (LineAABasics.line_mr(y + LineAABasics.SUBPIXEL_SCALE / 2) - LineAABasics.line_mr(sy)) * m_dx_start);

    //        m_dist_end = ((LineAABasics.line_mr(x + LineAABasics.SUBPIXEL_SCALE / 2) - LineAABasics.line_mr(ex)) * m_dy_end -
    //                   (LineAABasics.line_mr(y + LineAABasics.SUBPIXEL_SCALE / 2) - LineAABasics.line_mr(ey)) * m_dx_end);
    //        m_len = (int)(AggBasics.uround(len / scale));

    //        double d = len * scale;
    //        int dx = AggBasics.iround(((x2 - x1) << LineAABasics.SUBPIXEL_SHIFT) / d);
    //        int dy = AggBasics.iround(((y2 - y1) << LineAABasics.SUBPIXEL_SHIFT) / d);
    //        m_dx_pict = -dy;
    //        m_dy_pict = dx;
    //        m_dist_pict = ((x + LineAABasics.SUBPIXEL_SCALE / 2 - (x1 - dy)) * m_dy_pict -
    //                        (y + LineAABasics.SUBPIXEL_SCALE / 2 - (y1 + dx)) * m_dx_pict) >>
    //                       LineAABasics.SUBPIXEL_SHIFT;

    //        m_dx <<= LineAABasics.SUBPIXEL_SHIFT;
    //        m_dy <<= LineAABasics.SUBPIXEL_SHIFT;
    //        m_dx_start <<= LineAABasics.MR_SUBPIXEL_SHIFT;
    //        m_dy_start <<= LineAABasics.MR_SUBPIXEL_SHIFT;
    //        m_dx_end <<= LineAABasics.MR_SUBPIXEL_SHIFT;
    //        m_dy_end <<= LineAABasics.MR_SUBPIXEL_SHIFT;
    //    }

    //    //---------------------------------------------------------------------
    //    public void inc_x()
    //    {
    //        m_dist += m_dy;
    //        m_dist_start += m_dy_start;
    //        m_dist_pict += m_dy_pict;
    //        m_dist_end += m_dy_end;
    //    }

    //    //---------------------------------------------------------------------
    //    public void dec_x()
    //    {
    //        m_dist -= m_dy;
    //        m_dist_start -= m_dy_start;
    //        m_dist_pict -= m_dy_pict;
    //        m_dist_end -= m_dy_end;
    //    }

    //    //---------------------------------------------------------------------
    //    public void inc_y()
    //    {
    //        m_dist -= m_dx;
    //        m_dist_start -= m_dx_start;
    //        m_dist_pict -= m_dx_pict;
    //        m_dist_end -= m_dx_end;
    //    }

    //    //---------------------------------------------------------------------
    //    public void dec_y()
    //    {
    //        m_dist += m_dx;
    //        m_dist_start += m_dx_start;
    //        m_dist_pict += m_dx_pict;
    //        m_dist_end += m_dx_end;
    //    }

    //    //---------------------------------------------------------------------
    //    public void inc_x(int dy)
    //    {
    //        m_dist += m_dy;
    //        m_dist_start += m_dy_start;
    //        m_dist_pict += m_dy_pict;
    //        m_dist_end += m_dy_end;
    //        if (dy > 0)
    //        {
    //            m_dist -= m_dx;
    //            m_dist_start -= m_dx_start;
    //            m_dist_pict -= m_dx_pict;
    //            m_dist_end -= m_dx_end;
    //        }
    //        if (dy < 0)
    //        {
    //            m_dist += m_dx;
    //            m_dist_start += m_dx_start;
    //            m_dist_pict += m_dx_pict;
    //            m_dist_end += m_dx_end;
    //        }
    //    }

    //    //---------------------------------------------------------------------
    //    public void dec_x(int dy)
    //    {
    //        m_dist -= m_dy;
    //        m_dist_start -= m_dy_start;
    //        m_dist_pict -= m_dy_pict;
    //        m_dist_end -= m_dy_end;
    //        if (dy > 0)
    //        {
    //            m_dist -= m_dx;
    //            m_dist_start -= m_dx_start;
    //            m_dist_pict -= m_dx_pict;
    //            m_dist_end -= m_dx_end;
    //        }
    //        if (dy < 0)
    //        {
    //            m_dist += m_dx;
    //            m_dist_start += m_dx_start;
    //            m_dist_pict += m_dx_pict;
    //            m_dist_end += m_dx_end;
    //        }
    //    }

    //    //---------------------------------------------------------------------
    //    public void inc_y(int dx)
    //    {
    //        m_dist -= m_dx;
    //        m_dist_start -= m_dx_start;
    //        m_dist_pict -= m_dx_pict;
    //        m_dist_end -= m_dx_end;
    //        if (dx > 0)
    //        {
    //            m_dist += m_dy;
    //            m_dist_start += m_dy_start;
    //            m_dist_pict += m_dy_pict;
    //            m_dist_end += m_dy_end;
    //        }
    //        if (dx < 0)
    //        {
    //            m_dist -= m_dy;
    //            m_dist_start -= m_dy_start;
    //            m_dist_pict -= m_dy_pict;
    //            m_dist_end -= m_dy_end;
    //        }
    //    }

    //    //---------------------------------------------------------------------
    //    public void dec_y(int dx)
    //    {
    //        m_dist += m_dx;
    //        m_dist_start += m_dx_start;
    //        m_dist_pict += m_dx_pict;
    //        m_dist_end += m_dx_end;
    //        if (dx > 0)
    //        {
    //            m_dist += m_dy;
    //            m_dist_start += m_dy_start;
    //            m_dist_pict += m_dy_pict;
    //            m_dist_end += m_dy_end;
    //        }
    //        if (dx < 0)
    //        {
    //            m_dist -= m_dy;
    //            m_dist_start -= m_dy_start;
    //            m_dist_pict -= m_dy_pict;
    //            m_dist_end -= m_dy_end;
    //        }
    //    }

    //    //---------------------------------------------------------------------
    //    public int dist() { return m_dist; }
    //    public int dist_start() { return m_dist_start; }
    //    public int dist_pict() { return m_dist_pict; }
    //    public int dist_end() { return m_dist_end; }

    //    //---------------------------------------------------------------------
    //    public int dx() { return m_dx; }
    //    public int dy() { return m_dy; }
    //    public int dx_start() { return m_dx_start; }
    //    public int dy_start() { return m_dy_start; }
    //    public int dx_pict() { return m_dx_pict; }
    //    public int dy_pict() { return m_dy_pict; }
    //    public int dx_end() { return m_dx_end; }
    //    public int dy_end() { return m_dy_end; }
    //    public int len() { return m_len; }
    //}
}