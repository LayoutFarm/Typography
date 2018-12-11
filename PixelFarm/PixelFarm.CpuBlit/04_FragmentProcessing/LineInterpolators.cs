//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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
//
// classes dda_line_interpolator, dda2_line_interpolator
//
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.FragmentProcessing
{
    //===================================================dda_line_interpolator
    public struct LineInterpolatorDDA
    {
        int _y;
        int _dy;
        readonly int _inc;
        readonly int _fractionShift;

        public LineInterpolatorDDA(int y1, int y2, int count, int fractionShift)
        {
            _fractionShift = fractionShift;
            _y = (y1);
            _inc = (((y2 - y1) << _fractionShift) / (int)(count));
            _dy = (0);
        }

        //--------------------------------------------------------------------
        //public void operator ++ ()
        public void Next()
        {
            _dy += _inc;
        }

        //--------------------------------------------------------------------
        //public void operator -- ()
        public void Prev()
        {
            _dy -= _inc;
        }

        //--------------------------------------------------------------------
        //public void operator += (int n)
        public void Next(int n)
        {
            _dy += _inc * n;
        }

        //--------------------------------------------------------------------
        //public void operator -= (int n)
        public void Prev(int n)
        {
            _dy -= _inc * n;
        }
        //--------------------------------------------------------------------
        public int y() => _y + (_dy >> (_fractionShift));  // - m_YShift)); }
        //
        public int dy() => _dy;
        //
    }

    //=================================================dda2_line_interpolator

    class LineInterpolatorDDA2
    {

        //----------------------
        //this need to be class ***
        //----------------------

        readonly int _cnt;
        readonly int _lft;
        readonly int _rem;
        int _mod;
        int _y;
        //-------------------------------------------- Forward-adjusted line
        public LineInterpolatorDDA2(int y1, int y2, int count)
        {
            //dbugIdN = 0;
            _cnt = (count <= 0 ? 1 : count);
            _lft = ((y2 - y1) / _cnt);
            _rem = ((y2 - y1) % _cnt);

            _mod = (_rem);
            _y = (y1);
            if (_mod <= 0)
            {
                _mod += count;
                _rem += count;
                _lft--;
            }
            _mod -= count;
        }
        public LineInterpolatorDDA2(int y, int count)
        {
            //dbugIdN = 0;
            _cnt = (count <= 0 ? 1 : count);
            _lft = ((y) / _cnt);
            _rem = ((y) % _cnt);
            _mod = (_rem);
            _y = (0);
            if (_mod <= 0)
            {
                _mod += count;
                _rem += count;
                _lft--;
            }
        }
#if DEBUG
        //static int dbugIdN;
#endif
        //public void operator ++()
        public void Next()
        {
            //dbugIdN++;

            _mod += _rem;
            _y += _lft;
            if (_mod > 0)
            {
                _mod -= _cnt;
                _y++;
            }
        }

        //--------------------------------------------------------------------
        //public void operator--()
        public void Prev()
        {
            if (_mod <= _rem)
            {
                _mod += _cnt;
                _y--;
            }
            _mod -= _rem;
            _y -= _lft;
        }

        //--------------------------------------------------------------------
        public void adjust_forward()
        {
            _mod -= _cnt;
        }
        //--------------------------------------------------------------------
        public void adjust_backward()
        {
            _mod += _cnt;
        }
        //
        public int Y => _y;
        //
    }


    struct LineInterpolatorDDA2S
    {
        readonly int _cnt;
        readonly int _lft;
        readonly int _rem;
        int _mod;
        int _y;
        //-------------------------------------------- Forward-adjusted line
        public LineInterpolatorDDA2S(int y1, int y2, int count)
        {

            _cnt = (count <= 0 ? 1 : count);
            _lft = ((y2 - y1) / _cnt);
            _rem = ((y2 - y1) % _cnt);

            _mod = (_rem);
            _y = (y1);
            if (_mod <= 0)
            {
                _mod += count;
                _rem += count;
                _lft--;
            }
            _mod -= count;
        }

        //public void operator ++()
        public void Next()
        {
            _mod += _rem;
            _y += _lft;
            if (_mod > 0)
            {
                _mod -= _cnt;
                _y++;
            }
        }
        //
        public int Y => _y;
        //
    }

    //---------------------------------------------line_bresenham_interpolator
    sealed class LineInterpolatorBresenham
    {
        int _x1_lr;
        int _y1_lr;
        int _x2_lr;
        int _y2_lr;
        bool _ver;
        int _len;
        int _inc;
        LineInterpolatorDDA2 _interpolator;
        //
        const int SUBPIXEL_SHIFT = 8;
        const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;
        const int SUBPIXEL_MASK = SUBPIXEL_SCALE - 1;
        //
        //--------------------------------------------------------------------
        public static int line_lr(int v) => v >> (int)SUBPIXEL_SHIFT;

        //--------------------------------------------------------------------
        public LineInterpolatorBresenham(int x1, int y1, int x2, int y2)
        {
            _x1_lr = (line_lr(x1));
            _y1_lr = (line_lr(y1));
            _x2_lr = (line_lr(x2));
            _y2_lr = (line_lr(y2));
            _ver = (Math.Abs(_x2_lr - _x1_lr) < Math.Abs(_y2_lr - _y1_lr));
            if (_ver)
            {
                _len = (int)Math.Abs(_y2_lr - _y1_lr);
            }
            else
            {
                _len = (int)Math.Abs(_x2_lr - _x1_lr);
            }

            _inc = (_ver ? ((y2 > y1) ? 1 : -1) : ((x2 > x1) ? 1 : -1));
            _interpolator = new LineInterpolatorDDA2(_ver ? x1 : y1,
                           _ver ? x2 : y2,
                           _len);
        }

        //--------------------------------------------------------------------
        public bool is_ver() => _ver;
        public int len() => _len;
        public int inc() => _inc;

        //--------------------------------------------------------------------
        public void hstep()
        {
            _interpolator.Next();
            _x1_lr += _inc;
        }

        //--------------------------------------------------------------------
        public void vstep()
        {
            _interpolator.Next();
            _y1_lr += _inc;
        }

        //--------------------------------------------------------------------
        public int x1() => _x1_lr;
        public int y1() => _y1_lr;
        public int x2() => line_lr(_interpolator.Y);
        public int y2() => line_lr(_interpolator.Y);
        public int x2_hr() => _interpolator.Y;
        public int y2_hr() => _interpolator.Y;
    }
}
