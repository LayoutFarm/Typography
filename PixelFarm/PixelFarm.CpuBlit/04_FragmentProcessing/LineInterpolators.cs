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
        int m_y;
        int m_inc;
        int m_dy;
        int m_fractionShift;

        public LineInterpolatorDDA(int y1, int y2, int count, int fractionShift)
        {
            m_fractionShift = fractionShift;
            m_y = (y1);
            m_inc = (((y2 - y1) << m_fractionShift) / (int)(count));
            m_dy = (0);
        }

        //--------------------------------------------------------------------
        //public void operator ++ ()
        public void Next()
        {
            m_dy += m_inc;
        }

        //--------------------------------------------------------------------
        //public void operator -- ()
        public void Prev()
        {
            m_dy -= m_inc;
        }

        //--------------------------------------------------------------------
        //public void operator += (int n)
        public void Next(int n)
        {
            m_dy += m_inc * (int)n;
        }

        //--------------------------------------------------------------------
        //public void operator -= (int n)
        public void Prev(int n)
        {
            m_dy -= m_inc * (int)n;
        }


        //--------------------------------------------------------------------
        public int y() { return m_y + (m_dy >> (m_fractionShift)); } // - m_YShift)); }
        public int dy() { return m_dy; }
    }

    //=================================================dda2_line_interpolator
    struct LineInterpolatorDDA2
    {
        int m_cnt;
        int m_lft;
        int m_rem;
        int m_mod;
        int m_y;
        //--------------------------------------------------------------------
        //public LineInterpolatorDDA2() { }

        //-------------------------------------------- Forward-adjusted line
        public LineInterpolatorDDA2(int y1, int y2, int count)
        {
            m_cnt = (count <= 0 ? 1 : count);
            m_lft = ((y2 - y1) / m_cnt);
            m_rem = ((y2 - y1) % m_cnt);
            m_mod = (m_rem);
            m_y = (y1);
            if (m_mod <= 0)
            {
                m_mod += count;
                m_rem += count;
                m_lft--;
            }
            m_mod -= count;
        }

        //-------------------------------------------- Backward-adjusted line
        public LineInterpolatorDDA2(int y1, int y2, int count, int unused)
        {
            m_cnt = (count <= 0 ? 1 : count);
            m_lft = ((y2 - y1) / m_cnt);
            m_rem = ((y2 - y1) % m_cnt);
            m_mod = (m_rem);
            m_y = (y1);
            if (m_mod <= 0)
            {
                m_mod += count;
                m_rem += count;
                m_lft--;
            }
        }

        //-------------------------------------------- Backward-adjusted line
        public LineInterpolatorDDA2(int y, int count)
        {
            m_cnt = (count <= 0 ? 1 : count);
            m_lft = ((y) / m_cnt);
            m_rem = ((y) % m_cnt);
            m_mod = (m_rem);
            m_y = (0);
            if (m_mod <= 0)
            {
                m_mod += count;
                m_rem += count;
                m_lft--;
            }
        }

        /*
        //--------------------------------------------------------------------
        public void save(save_data_type* data)
        {
            data[0] = m_mod;
            data[1] = m_y;
        }

        //--------------------------------------------------------------------
        public void load(save_data_type* data)
        {
            m_mod = data[0];
            m_y   = data[1];
        }
         */

        //--------------------------------------------------------------------
        //public void operator++()
        public void Next()
        {
            m_mod += m_rem;
            m_y += m_lft;
            if (m_mod > 0)
            {
                m_mod -= m_cnt;
                m_y++;
            }
        }

        //--------------------------------------------------------------------
        //public void operator--()
        public void Prev()
        {
            if (m_mod <= m_rem)
            {
                m_mod += m_cnt;
                m_y--;
            }
            m_mod -= m_rem;
            m_y -= m_lft;
        }

        //--------------------------------------------------------------------
        public void adjust_forward()
        {
            m_mod -= m_cnt;
        }

        //--------------------------------------------------------------------
        public void adjust_backward()
        {
            m_mod += m_cnt;
        }

        //--------------------------------------------------------------------
        public int mod() { return m_mod; }
        public int rem() { return m_rem; }
        public int lft() { return m_lft; }

        //--------------------------------------------------------------------
        public int Y { get { return m_y; } }
    }


    //---------------------------------------------line_bresenham_interpolator
    sealed class LineInterpolatorBresenham
    {
        int m_x1_lr;
        int m_y1_lr;
        int m_x2_lr;
        int m_y2_lr;
        bool m_ver;
        int m_len;
        int m_inc;
        LineInterpolatorDDA2 m_interpolator;
        const int SUBPIXEL_SHIFT = 8;
        const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;
        const int SUBPIXEL_MASK = SUBPIXEL_SCALE - 1;
        //--------------------------------------------------------------------
        public static int line_lr(int v) { return v >> (int)SUBPIXEL_SHIFT; }

        //--------------------------------------------------------------------
        public LineInterpolatorBresenham(int x1, int y1, int x2, int y2)
        {
            m_x1_lr = (line_lr(x1));
            m_y1_lr = (line_lr(y1));
            m_x2_lr = (line_lr(x2));
            m_y2_lr = (line_lr(y2));
            m_ver = (Math.Abs(m_x2_lr - m_x1_lr) < Math.Abs(m_y2_lr - m_y1_lr));
            if (m_ver)
            {
                m_len = (int)Math.Abs(m_y2_lr - m_y1_lr);
            }
            else
            {
                m_len = (int)Math.Abs(m_x2_lr - m_x1_lr);
            }

            m_inc = (m_ver ? ((y2 > y1) ? 1 : -1) : ((x2 > x1) ? 1 : -1));
            m_interpolator = new LineInterpolatorDDA2(m_ver ? x1 : y1,
                           m_ver ? x2 : y2,
                           (int)m_len);
        }

        //--------------------------------------------------------------------
        public bool is_ver() { return m_ver; }
        public int len() { return m_len; }
        public int inc() { return m_inc; }

        //--------------------------------------------------------------------
        public void hstep()
        {
            m_interpolator.Next();
            m_x1_lr += m_inc;
        }

        //--------------------------------------------------------------------
        public void vstep()
        {
            m_interpolator.Next();
            m_y1_lr += m_inc;
        }

        //--------------------------------------------------------------------
        public int x1() { return m_x1_lr; }
        public int y1() { return m_y1_lr; }
        public int x2() { return line_lr(m_interpolator.Y); }
        public int y2() { return line_lr(m_interpolator.Y); }
        public int x2_hr() { return m_interpolator.Y; }
        public int y2_hr() { return m_interpolator.Y; }
    }
}
