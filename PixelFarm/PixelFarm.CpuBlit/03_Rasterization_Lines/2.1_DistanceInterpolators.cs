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
        readonly int _dx;
        readonly int _dy;
        int _dist;
        public DistanceInterpolator0(int x1, int y1, int x2, int y2, int x, int y)
        {
            unchecked
            {
                _dx = (LineAA.Mr(x2) - LineAA.Mr(x1));
                _dy = (LineAA.Mr(y2) - LineAA.Mr(y1));
                _dist = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x2)) * _dy -
                       (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y2)) * _dx);
                _dx <<= LineAA.MR_SUBPIXEL_SHIFT;
                _dy <<= LineAA.MR_SUBPIXEL_SHIFT;
            }
        }
        //
        public void IncX() { _dist += _dy; }
        //
        public int Distance => _dist;
        //
    }

    //==================================================distance_interpolator00
    struct DistanceInterpolator00
    {
        readonly int _dx1;
        readonly int _dy1;
        readonly int _dx2;
        readonly int _dy2;
        int _dist1;
        int _dist2;
        public DistanceInterpolator00(int xc, int yc,
                                int x1, int y1, int x2, int y2,
                                int x, int y)
        {
            _dx1 = (LineAA.Mr(x1) - LineAA.Mr(xc));
            _dy1 = (LineAA.Mr(y1) - LineAA.Mr(yc));
            _dx2 = (LineAA.Mr(x2) - LineAA.Mr(xc));
            _dy2 = (LineAA.Mr(y2) - LineAA.Mr(yc));
            _dist1 = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x1)) * _dy1 -
                    (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y1)) * _dx1);
            _dist2 = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(x2)) * _dy2 -
                    (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(y2)) * _dx2);
            _dx1 <<= LineAA.MR_SUBPIXEL_SHIFT;
            _dy1 <<= LineAA.MR_SUBPIXEL_SHIFT;
            _dx2 <<= LineAA.MR_SUBPIXEL_SHIFT;
            _dy2 <<= LineAA.MR_SUBPIXEL_SHIFT;
        }

        //---------------------------------------------------------------------
        public void IncX() { _dist1 += _dy1; _dist2 += _dy2; }
        //
        public int Distance1 => _dist1;
        public int Distance2 => _dist2;
        //
    }

    //===================================================distance_interpolator1
    struct DistanceInterpolator1
    {
        readonly int _dx;
        readonly int _dy;
        int _dist;
        public DistanceInterpolator1(int x1, int y1, int x2, int y2, int x, int y)
        {
            _dx = (x2 - x1);
            _dy = (y2 - y1);
            _dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(_dx)));
            _dx <<= LineAA.SUBPIXEL_SHIFT;
            _dy <<= LineAA.SUBPIXEL_SHIFT;
        }


        public void IncX(int dy)
        {
            _dist += _dy;
            if (dy > 0)
            {
                _dist -= _dx;
            }
            else if (dy < 0)
            {
                _dist += _dx;
            }
        }


        public void DecX(int dy)
        {
            _dist -= _dy;
            if (dy > 0)
            {
                _dist -= _dx;
            }
            else if (dy < 0)
            {
                _dist += _dx;
            }
        }


        public void IncY(int dx)
        {
            _dist -= _dx;
            if (dx > 0)
            {
                _dist += _dy;
            }
            else if (dx < 0)
            {
                _dist -= _dy;
            }
        }
        public void DecY(int dx)
        {
            _dist += _dx;
            if (dx > 0)
            {
                _dist += _dy;
            }
            else if (dx < 0)
            {
                _dist -= _dy;
            }
        }
        //---------------------------------------------------------------------
        public int Distance => _dist;
    }

    //===================================================distance_interpolator2
    struct DistanceInterpolator2
    {
        readonly int _dx;
        readonly int _dy;
        readonly int _dx_start;
        readonly int _dy_start;
        int _dist;
        int _dist_start;
        //---------------------------------------------------------------------

        public DistanceInterpolator2(int x1, int y1, int x2, int y2,
                               int sx, int sy, int x, int y)
        {
            _dx = (x2 - x1);
            _dy = (y2 - y1);
            _dx_start = (LineAA.Mr(sx) - LineAA.Mr(x1));
            _dy_start = (LineAA.Mr(sy) - LineAA.Mr(y1));
            _dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(_dx)));
            _dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sx)) * _dy_start -
                         (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sy)) * _dx_start);
            _dx <<= LineAA.SUBPIXEL_SHIFT;
            _dy <<= LineAA.SUBPIXEL_SHIFT;
            _dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
            _dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
        }

        public DistanceInterpolator2(int x1, int y1, int x2, int y2,
                               int ex, int ey, int x, int y, int none)
        {
            _dx = (x2 - x1);
            _dy = (y2 - y1);
            _dx_start = (LineAA.Mr(ex) - LineAA.Mr(x2));
            _dy_start = (LineAA.Mr(ey) - LineAA.Mr(y2));
            _dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(_dy) -
                          (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(_dx)));
            _dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ex)) * _dy_start -
                         (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ey)) * _dx_start);
            _dx <<= LineAA.SUBPIXEL_SHIFT;
            _dy <<= LineAA.SUBPIXEL_SHIFT;
            _dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
            _dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
        }



        //---------------------------------------------------------------------
        public void IncX(int dy)
        {
            _dist += _dy;
            _dist_start += _dy_start;
            if (dy > 0)
            {
                _dist -= _dx;
                _dist_start -= _dx_start;
            }
            else if (dy < 0)
            {
                _dist += _dx;
                _dist_start += _dx_start;
            }
        }

        //---------------------------------------------------------------------
        public void DecX(int dy)
        {
            _dist -= _dy;
            _dist_start -= _dy_start;
            if (dy > 0)
            {
                _dist -= _dx;
                _dist_start -= _dx_start;
            }
            else if (dy < 0)
            {
                _dist += _dx;
                _dist_start += _dx_start;
            }
        }

        //---------------------------------------------------------------------
        public void IncY(int dx)
        {
            _dist -= _dx;
            _dist_start -= _dx_start;
            if (dx > 0)
            {
                _dist += _dy;
                _dist_start += _dy_start;
            }
            else if (dx < 0)
            {
                _dist -= _dy;
                _dist_start -= _dy_start;
            }
        }

        //---------------------------------------------------------------------
        public void DecY(int dx)
        {
            _dist += _dx;
            _dist_start += _dx_start;
            if (dx > 0)
            {
                _dist += _dy;
                _dist_start += _dy_start;
            }
            else if (dx < 0)
            {
                _dist -= _dy;
                _dist_start -= _dy_start;
            }
        }

        //---------------------------------------------------------------------
        public int Distance => _dist;
        public int DistanceStart => _dist_start;
        public int DistanceEnd => _dist_start;

        //---------------------------------------------------------------------

        public int DxStart => _dx_start;
        public int DyStart => _dy_start;
        public int DxEnd => _dx_start;
        public int DyEnd => _dy_start;

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
        readonly int _dx;
        readonly int _dy;
        readonly int _dx_start;
        readonly int _dy_start;
        readonly int _dx_end;
        readonly int _dy_end;
        int _dist;
        int _dist_start;
        int _dist_end;
        //---------------------------------------------------------------------

        public DistanceInterpolator3(int x1, int y1, int x2, int y2,
                               int sx, int sy, int ex, int ey,
                               int x, int y)
        {
            unchecked
            {
                _dx = (x2 - x1);
                _dy = (y2 - y1);
                _dx_start = (LineAA.Mr(sx) - LineAA.Mr(x1));
                _dy_start = (LineAA.Mr(sy) - LineAA.Mr(y1));
                _dx_end = (LineAA.Mr(ex) - LineAA.Mr(x2));
                _dy_end = (LineAA.Mr(ey) - LineAA.Mr(y2));
                _dist = (AggMath.iround((double)(x + LineAA.SUBPIXEL_SCALE / 2 - x2) * (double)(_dy) -
                              (double)(y + LineAA.SUBPIXEL_SCALE / 2 - y2) * (double)(_dx)));
                _dist_start = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sx)) * _dy_start -
                             (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(sy)) * _dx_start);
                _dist_end = ((LineAA.Mr(x + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ex)) * _dy_end -
                           (LineAA.Mr(y + LineAA.SUBPIXEL_SCALE / 2) - LineAA.Mr(ey)) * _dx_end);
                _dx <<= LineAA.SUBPIXEL_SHIFT;
                _dy <<= LineAA.SUBPIXEL_SHIFT;
                _dx_start <<= LineAA.MR_SUBPIXEL_SHIFT;
                _dy_start <<= LineAA.MR_SUBPIXEL_SHIFT;
                _dx_end <<= LineAA.MR_SUBPIXEL_SHIFT;
                _dy_end <<= LineAA.MR_SUBPIXEL_SHIFT;
            }
        }


        public void IncX(int dy)
        {
            _dist += _dy;
            _dist_start += _dy_start;
            _dist_end += _dy_end;
            if (dy > 0)
            {
                _dist -= _dx;
                _dist_start -= _dx_start;
                _dist_end -= _dx_end;
            }
            if (dy < 0)
            {
                _dist += _dx;
                _dist_start += _dx_start;
                _dist_end += _dx_end;
            }
        }

        public void DecX(int dy)
        {
            _dist -= _dy;
            _dist_start -= _dy_start;
            _dist_end -= _dy_end;
            if (dy > 0)
            {
                _dist -= _dx;
                _dist_start -= _dx_start;
                _dist_end -= _dx_end;
            }
            if (dy < 0)
            {
                _dist += _dx;
                _dist_start += _dx_start;
                _dist_end += _dx_end;
            }
        }

        public void IncY(int dx)
        {
            _dist -= _dx;
            _dist_start -= _dx_start;
            _dist_end -= _dx_end;
            if (dx > 0)
            {
                _dist += _dy;
                _dist_start += _dy_start;
                _dist_end += _dy_end;
            }
            if (dx < 0)
            {
                _dist -= _dy;
                _dist_start -= _dy_start;
                _dist_end -= _dy_end;
            }
        }

        public void DecY(int dx)
        {
            _dist += _dx;
            _dist_start += _dx_start;
            _dist_end += _dx_end;
            if (dx > 0)
            {
                _dist += _dy;
                _dist_start += _dy_start;
                _dist_end += _dy_end;
            }
            if (dx < 0)
            {
                _dist -= _dy;
                _dist_start -= _dy_start;
                _dist_end -= _dy_end;
            }
        }

        public int Distance => _dist;
        public int dist_start => _dist_start;
        public int dist_end => _dist_end;


        public int DxStart => _dx_start;
        public int DyStart => _dy_start;
        public int DxEnd => _dx_end;
        public int DyEnd => _dy_end;

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