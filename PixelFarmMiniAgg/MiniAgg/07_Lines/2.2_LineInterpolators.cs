//BSD, 2014-2017, WinterDev
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
using PixelFarm.Agg.Transform;
namespace PixelFarm.Agg.Lines
{
    //================================================line_interpolator_aa_base
    class LineInterpolatorAABase
    {
        protected LineParameters m_lp;
        protected Transform.LineInterpolatorDDA2 m_li;
        protected OutlineRenderer m_ren;
        int m_len;
        protected int m_x;
        protected int m_y;
        protected int m_old_x;
        protected int m_old_y;
        protected int m_count;
        protected int m_width;
        protected int m_max_extent;
        protected int m_step;
        protected int[] m_dist = new int[MAX_HALF_WIDTH + 1];
        protected byte[] m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];
        protected const int MAX_HALF_WIDTH = 64;
        public LineInterpolatorAABase(OutlineRenderer ren, LineParameters lp)
        {
            m_lp = lp;
            m_li = new LineInterpolatorDDA2(lp.vertical ? LineAA.DblHr(lp.x2 - lp.x1) : LineAA.DblHr(lp.y2 - lp.y1),
                lp.vertical ? Math.Abs(lp.y2 - lp.y1) : Math.Abs(lp.x2 - lp.x1) + 1);
            m_ren = ren;
            m_len = ((lp.vertical == (lp.inc > 0)) ? -lp.len : lp.len);
            m_x = (lp.x1 >> LineAA.SUBPIXEL_SHIFT);
            m_y = (lp.y1 >> LineAA.SUBPIXEL_SHIFT);
            m_old_x = (m_x);
            m_old_y = (m_y);
            m_count = ((lp.vertical ? Math.Abs((lp.y2 >> LineAA.SUBPIXEL_SHIFT) - m_y) :
                                   Math.Abs((lp.x2 >> LineAA.SUBPIXEL_SHIFT) - m_x)));
            m_width = (ren.SubPixelWidth);
            //m_max_extent(m_width >> (line_subpixel_shift - 2));
            m_max_extent = ((m_width + LineAA.SUBPIXEL_MARK) >> LineAA.SUBPIXEL_SHIFT);
            m_step = 0;
            LineInterpolatorDDA2 li = new LineInterpolatorDDA2(0,
                lp.vertical ? (lp.dy << LineAA.SUBPIXEL_SHIFT) : (lp.dx << LineAA.SUBPIXEL_SHIFT),
                lp.len);
            int i;
            int stop = m_width + LineAA.SUBPIXEL_SCALE * 2;
            for (i = 0; i < MAX_HALF_WIDTH; ++i)
            {
                m_dist[i] = li.Y;
                if (m_dist[i] >= stop) break;
                li.Next();
            }
            m_dist[i++] = 0x7FFF0000;
        }

        public int BaseStepH(DistanceInterpolator1 di)
        {
            m_li.Next();
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncX(m_y - m_old_y);
            else di.DecX(m_y - m_old_y);
            m_old_y = m_y;
            return di.Distance / m_len;
        }

        public int BaseStepH(DistanceInterpolator2 di)
        {
            m_li.Next();
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncX(m_y - m_old_y);
            else di.DecX(m_y - m_old_y);
            m_old_y = m_y;
            return di.Distance / m_len;
        }

        public int BaseStepH(DistanceInterpolator3 di)
        {
            m_li.Next();
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncX(m_y - m_old_y);
            else di.DecX(m_y - m_old_y);
            m_old_y = m_y;
            return di.Distance / m_len;
        }

        //-------------------------------------------------------
        public int BaseStepV(DistanceInterpolator1 di)
        {
            m_li.Next();
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncY(m_x - m_old_x);
            else di.DecY(m_x - m_old_x);
            m_old_x = m_x;
            return di.Distance / m_len;
        }

        public int BaseStepV(DistanceInterpolator2 di)
        {
            m_li.Next();
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncY(m_x - m_old_x);
            else di.DecY(m_x - m_old_x);
            m_old_x = m_x;
            return di.Distance / m_len;
        }

        public int BaseStepV(DistanceInterpolator3 di)
        {
            m_li.Next();
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncY(m_x - m_old_x);
            else di.DecY(m_x - m_old_x);
            m_old_x = m_x;
            return di.Distance / m_len;
        }

        public bool IsVertical { get { return m_lp.vertical; } }
        public int Width { get { return m_width; } }
        public int Count { get { return m_count; } }
    }

    //====================================================line_interpolator_aa0
    class LineInterpolatorAA0 : LineInterpolatorAABase
    {
        DistanceInterpolator1 m_di;
        //---------------------------------------------------------------------
        public LineInterpolatorAA0(OutlineRenderer ren, LineParameters lp)
            : base(ren, lp)
        {
            m_di = new DistanceInterpolator1(lp.x1, lp.y1, lp.x2, lp.y2,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);
            m_li.adjust_forward();
        }


        public bool StepH()
        {
            int dist;
            int dy;
            int s1 = BaseStepH(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            m_covers[Offset1++] = (byte)m_ren.GetCover(s1);
            dy = 1;
            while ((dist = base.m_dist[dy] - s1) <= base.m_width)
            {
                m_covers[Offset1++] = (byte)base.m_ren.GetCover(dist);
                ++dy;
            }

            dy = 1;
            while ((dist = base.m_dist[dy] + s1) <= base.m_width)
            {
                m_covers[--Offset0] = (byte)base.m_ren.GetCover(dist);
                ++dy;
            }
            base.m_ren.BlendSolidVSpan(base.m_x,
                                               base.m_y - dy + 1,
                                               Offset1 - Offset0,
                                               m_covers, Offset0);
            return ++base.m_step < base.m_count;
        }


        public bool StepV()
        {
            int dist;
            int dx;
            int s1 = base.BaseStepV(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            m_covers[Offset1++] = (byte)m_ren.GetCover(s1);
            dx = 1;
            while ((dist = base.m_dist[dx] - s1) <= base.m_width)
            {
                m_covers[Offset1++] = (byte)base.m_ren.GetCover(dist);
                ++dx;
            }

            dx = 1;
            while ((dist = base.m_dist[dx] + s1) <= base.m_width)
            {
                m_covers[--Offset0] = (byte)base.m_ren.GetCover(dist);
                ++dx;
            }
            base.m_ren.BlendSolidHSpan(base.m_x - dx + 1,
                                               base.m_y,
                                               Offset1 - Offset0,
                                               m_covers, Offset0);
            return ++base.m_step < base.m_count;
        }
    }

    //====================================================line_interpolator_aa1
    class LineInterpolatorAA1 : LineInterpolatorAABase
    {
        DistanceInterpolator2 m_di;
        public LineInterpolatorAA1(OutlineRenderer ren, LineParameters lp, int sx, int sy)
            : base(ren, lp)
        {
            m_di = new DistanceInterpolator2(lp.x1, lp.y1, lp.x2, lp.y2, sx, sy,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);
            int dist1_start;
            int dist2_start;
            int npix = 1;
            if (lp.vertical)
            {
                do
                {
                    base.m_li.Prev();
                    base.m_y -= lp.inc;
                    base.m_x = (base.m_lp.x1 + base.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) m_di.DecY(base.m_x - base.m_old_x);
                    else m_di.IncY(base.m_x - base.m_old_x);
                    base.m_old_x = base.m_x;
                    dist1_start = dist2_start = m_di.DistanceStart;
                    int dx = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start += m_di.DyStart;
                        dist2_start -= m_di.DyStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dx;
                    }
                    while (base.m_dist[dx] <= base.m_width);
                    --base.m_step;
                    if (npix == 0) break;
                    npix = 0;
                }
                while (base.m_step >= -base.m_max_extent);
            }
            else
            {
                do
                {
                    base.m_li.Prev();
                    base.m_x -= lp.inc;
                    base.m_y = (base.m_lp.y1 + base.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) m_di.DecX(base.m_y - base.m_old_y);
                    else m_di.IncX(base.m_y - base.m_old_y);
                    base.m_old_y = base.m_y;
                    dist1_start = dist2_start = m_di.DistanceStart;
                    int dy = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start -= m_di.DxStart;
                        dist2_start += m_di.DxStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dy;
                    }
                    while (base.m_dist[dy] <= base.m_width);
                    --base.m_step;
                    if (npix == 0) break;
                    npix = 0;
                }
                while (base.m_step >= -base.m_max_extent);
            }
            base.m_li.adjust_forward();
        }

        //---------------------------------------------------------------------
        public bool StepH()
        {
            int dist_start;
            int dist;
            int dy;
            int s1 = base.BaseStepH(m_di);
            dist_start = m_di.DistanceStart;
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            m_covers[Offset1] = 0;
            if (dist_start <= 0)
            {
                m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
            }
            ++Offset1;
            dy = 1;
            while ((dist = base.m_dist[dy] - s1) <= base.m_width)
            {
                dist_start -= m_di.DxStart;
                m_covers[Offset1] = 0;
                if (dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_start = m_di.DistanceStart;
            while ((dist = base.m_dist[dy] + s1) <= base.m_width)
            {
                dist_start += m_di.DxStart;
                m_covers[--Offset0] = 0;
                if (dist_start <= 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                }
                ++dy;
            }

            int len = Offset1 - Offset0;
            base.m_ren.BlendSolidVSpan(base.m_x,
                                               base.m_y - dy + 1,
                                               len, m_covers,
                                               Offset0);
            return ++base.m_step < base.m_count;
        }

        //---------------------------------------------------------------------
        public bool StepV()
        {
            int dist_start;
            int dist;
            int dx;
            int s1 = base.BaseStepV(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = m_di.DistanceStart;
            m_covers[Offset1] = 0;
            if (dist_start <= 0)
            {
                m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
            }
            ++Offset1;
            dx = 1;
            while ((dist = base.m_dist[dx] - s1) <= base.m_width)
            {
                dist_start += m_di.DyStart;
                m_covers[Offset1] = 0;
                if (dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_start = m_di.DistanceStart;
            while ((dist = base.m_dist[dx] + s1) <= base.m_width)
            {
                dist_start -= m_di.DyStart;
                m_covers[--Offset0] = 0;
                if (dist_start <= 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                }
                ++dx;
            }
            base.m_ren.BlendSolidHSpan(base.m_x - dx + 1,
                                               base.m_y,
                                               Offset1 - Offset0, m_covers,
                                               Offset0);
            return ++base.m_step < base.m_count;
        }
    }

    //====================================================line_interpolator_aa2
    class LineInterpolatorAA2 : LineInterpolatorAABase
    {
        DistanceInterpolator2 m_di;
        //---------------------------------------------------------------------
        public LineInterpolatorAA2(OutlineRenderer ren, LineParameters lp,
                              int ex, int ey)
            : base(ren, lp)
        {
            m_di = new DistanceInterpolator2(lp.x1, lp.y1, lp.x2, lp.y2, ex, ey,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK,
                 0);
            base.m_li.adjust_forward();
            base.m_step -= base.m_max_extent;
        }

        //---------------------------------------------------------------------
        public bool StepH()
        {
            int dist_end;
            int dist;
            int dy;
            int s1 = base.BaseStepH(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_end = m_di.DistanceEnd;
            int npix = 0;
            m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
                ++npix;
            }
            ++Offset1;
            dy = 1;
            while ((dist = base.m_dist[dy] - s1) <= base.m_width)
            {
                dist_end -= m_di.DxEnd;
                m_covers[Offset1] = 0;
                if (dist_end > 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_end = m_di.DistanceEnd;
            while ((dist = base.m_dist[dy] + s1) <= base.m_width)
            {
                dist_end += m_di.DxEnd;
                m_covers[--Offset0] = 0;
                if (dist_end > 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++dy;
            }
            base.m_ren.BlendSolidVSpan(base.m_x,
                                               base.m_y - dy + 1,
                                               Offset1 - Offset0, m_covers,
                                               Offset0);
            return npix != 0 && ++base.m_step < base.m_count;
        }

        //---------------------------------------------------------------------
        public bool StepV()
        {
            int dist_end;
            int dist;
            int dx;
            int s1 = base.BaseStepV(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_end = m_di.DistanceEnd;
            int npix = 0;
            m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
                ++npix;
            }
            ++Offset1;
            dx = 1;
            while ((dist = base.m_dist[dx] - s1) <= base.m_width)
            {
                dist_end += m_di.DyEnd;
                m_covers[Offset1] = 0;
                if (dist_end > 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_end = m_di.DistanceEnd;
            while ((dist = base.m_dist[dx] + s1) <= base.m_width)
            {
                dist_end -= m_di.DyEnd;
                m_covers[--Offset0] = 0;
                if (dist_end > 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++dx;
            }
            base.m_ren.BlendSolidHSpan(base.m_x - dx + 1,
                                               base.m_y,
                                               Offset1 - Offset0, m_covers,
                                               Offset0);
            return npix != 0 && ++base.m_step < base.m_count;
        }
    }

    //====================================================line_interpolator_aa3
    class LineInterpolatorAA3 : LineInterpolatorAABase
    {
        DistanceInterpolator3 m_di;
        //---------------------------------------------------------------------
        public LineInterpolatorAA3(OutlineRenderer ren, LineParameters lp,
                              int sx, int sy, int ex, int ey)
            : base(ren, lp)
        {
            m_di = new DistanceInterpolator3(lp.x1, lp.y1, lp.x2, lp.y2, sx, sy, ex, ey,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);
            int dist1_start;
            int dist2_start;
            int npix = 1;
            if (lp.vertical)
            {
                do
                {
                    base.m_li.Prev();
                    base.m_y -= lp.inc;
                    base.m_x = (base.m_lp.x1 + base.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) m_di.DecY(base.m_x - base.m_old_x);
                    else m_di.IncY(base.m_x - base.m_old_x);
                    base.m_old_x = base.m_x;
                    dist1_start = dist2_start = m_di.dist_start;
                    int dx = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start += m_di.DyStart;
                        dist2_start -= m_di.DyStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dx;
                    }
                    while (base.m_dist[dx] <= base.m_width);
                    if (npix == 0) break;
                    npix = 0;
                }
                while (--base.m_step >= -base.m_max_extent);
            }
            else
            {
                do
                {
                    base.m_li.Prev();
                    base.m_x -= lp.inc;
                    base.m_y = (base.m_lp.y1 + base.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) m_di.DecX(base.m_y - base.m_old_y);
                    else m_di.IncX(base.m_y - base.m_old_y);
                    base.m_old_y = base.m_y;
                    dist1_start = dist2_start = m_di.dist_start;
                    int dy = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start -= m_di.DxStart;
                        dist2_start += m_di.DxStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dy;
                    }
                    while (base.m_dist[dy] <= base.m_width);
                    if (npix == 0) break;
                    npix = 0;
                }
                while (--base.m_step >= -base.m_max_extent);
            }
            base.m_li.adjust_forward();
            base.m_step -= base.m_max_extent;
        }


        //---------------------------------------------------------------------
        public bool StepH()
        {
            int dist_start;
            int dist_end;
            int dist;
            int dy;
            int s1 = base.BaseStepH(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = m_di.dist_start;
            dist_end = m_di.dist_end;
            int npix = 0;
            m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                if (dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
                }
                ++npix;
            }
            ++Offset1;
            dy = 1;
            while ((dist = base.m_dist[dy] - s1) <= base.m_width)
            {
                dist_start -= m_di.DxStart;
                dist_end -= m_di.DxEnd;
                m_covers[Offset1] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_start = m_di.dist_start;
            dist_end = m_di.dist_end;
            while ((dist = base.m_dist[dy] + s1) <= base.m_width)
            {
                dist_start += m_di.DxStart;
                dist_end += m_di.DxEnd;
                m_covers[--Offset0] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++dy;
            }
            base.m_ren.BlendSolidVSpan(base.m_x,
                                               base.m_y - dy + 1,
                                               Offset1 - Offset0, m_covers,
                                               Offset0);
            return npix != 0 && ++base.m_step < base.m_count;
        }

        //---------------------------------------------------------------------
        public bool StepV()
        {
            int dist_start;
            int dist_end;
            int dist;
            int dx;
            int s1 = base.BaseStepV(m_di);
            int Offset0 = MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = m_di.dist_start;
            dist_end = m_di.dist_end;
            int npix = 0;
            m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                if (dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(s1);
                }
                ++npix;
            }
            ++Offset1;
            dx = 1;
            while ((dist = base.m_dist[dx] - s1) <= base.m_width)
            {
                dist_start += m_di.DyStart;
                dist_end += m_di.DyEnd;
                m_covers[Offset1] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    m_covers[Offset1] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_start = m_di.dist_start;
            dist_end = m_di.dist_end;
            while ((dist = base.m_dist[dx] + s1) <= base.m_width)
            {
                dist_start -= m_di.DyStart;
                dist_end -= m_di.DyEnd;
                m_covers[--Offset0] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    m_covers[Offset0] = (byte)base.m_ren.GetCover(dist);
                    ++npix;
                }
                ++dx;
            }
            base.m_ren.BlendSolidHSpan(base.m_x - dx + 1,
                                               base.m_y,
                                               Offset1 - Offset0, m_covers,
                                               Offset0);
            return npix != 0 && ++base.m_step < base.m_count;
        }
    }
}