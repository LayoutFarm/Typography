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
    struct EllipseBresenhamInterpolator
    {
        int _rx2;
        int _ry2;
        int _two_rx2;
        int _two_ry2;
        int _dx;
        int _dy;
        int _inc_x;
        int _inc_y;
        int _cur_f;
        public EllipseBresenhamInterpolator(int rx, int ry)
        {
            _rx2 = (rx * rx);
            _ry2 = (ry * ry);
            _two_rx2 = (_rx2 << 1);
            _two_ry2 = (_ry2 << 1);
            _dx = (0);
            _dy = (0);
            _inc_x = (0);
            _inc_y = (-ry * _two_rx2);
            _cur_f = (0);
        }
        //
        public int Dx => _dx;
        public int Dy => _dy;
        //
        public void Next()
        {
            int mx, my, mxy, min_m;
            int fx, fy, fxy;
            mx = fx = _cur_f + _inc_x + _ry2;
            if (mx < 0) mx = -mx;
            my = fy = _cur_f + _inc_y + _rx2;
            if (my < 0) my = -my;
            mxy = fxy = _cur_f + _inc_x + _ry2 + _inc_y + _rx2;
            if (mxy < 0) mxy = -mxy;
            min_m = mx;
            bool flag = true;
            if (min_m > my)
            {
                min_m = my;
                flag = false;
            }

            _dx = _dy = 0;
            if (min_m > mxy)
            {
                _inc_x += _two_ry2;
                _inc_y += _two_rx2;
                _cur_f = fxy;
                _dx = 1;
                _dy = 1;
                return;
            }

            if (flag)
            {
                _inc_x += _two_ry2;
                _cur_f = fx;
                _dx = 1;
                return;
            }

            _inc_y += _two_rx2;
            _cur_f = fy;
            _dy = 1;
        }
    }
}