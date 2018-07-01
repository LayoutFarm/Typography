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
using PixelFarm.CpuBlit.FragmentProcessing;

namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    //================================================line_interpolator_aa_base
    struct LineInterpolatorAAData
    {
        public LineParameters m_lp;
        public LineInterpolatorDDA2 m_li;

        int m_len;
        public int m_x;
        public int m_y;
        public int m_old_x;
        public int m_old_y;
        public int m_count;
        public int m_width;
        public int m_max_extent;
        public int m_step;
        public int[] m_dist;
        public byte[] m_covers;
        public const int MAX_HALF_WIDTH = 64;
        public LineInterpolatorAAData(OutlineRenderer ren, LineParameters lp)
        {
            m_dist = new int[MAX_HALF_WIDTH + 1];
            m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];

            m_lp = lp;
            m_li = new LineInterpolatorDDA2(lp.vertical ? LineAA.DblHr(lp.x2 - lp.x1) : LineAA.DblHr(lp.y2 - lp.y1),
                lp.vertical ? Math.Abs(lp.y2 - lp.y1) : Math.Abs(lp.x2 - lp.x1) + 1);

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

        public int BaseStepH(ref DistanceInterpolator1 di)
        {
            m_li.Next();
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncX(m_y - m_old_y);
            else di.DecX(m_y - m_old_y);
            m_old_y = m_y;
            return di.Distance / m_len;
        }

        public int BaseStepH(ref DistanceInterpolator2 di)
        {
            m_li.Next();
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncX(m_y - m_old_y);
            else di.DecX(m_y - m_old_y);
            m_old_y = m_y;
            return di.Distance / m_len;
        }

        public int BaseStepH(ref DistanceInterpolator3 di)
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
        public int BaseStepV(ref DistanceInterpolator1 di)
        {
            m_li.Next();
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncY(m_x - m_old_x);
            else di.DecY(m_x - m_old_x);
            m_old_x = m_x;
            return di.Distance / m_len;
        }

        public int BaseStepV(ref DistanceInterpolator2 di)
        {
            m_li.Next();
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
            if (m_lp.inc > 0) di.IncY(m_x - m_old_x);
            else di.DecY(m_x - m_old_x);
            m_old_x = m_x;
            return di.Distance / m_len;
        }

        public int BaseStepV(ref DistanceInterpolator3 di)
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
    struct LineInterpolatorAA0
    {
        DistanceInterpolator1 _m_di;
        LineInterpolatorAAData _aa_data;
        readonly OutlineRenderer _ren;
        //---------------------------------------------------------------------
        public LineInterpolatorAA0(OutlineRenderer ren, LineParameters lp)
        {
            this._ren = ren;
            _aa_data = new LineInterpolatorAAData(ren, lp);
            _m_di = new DistanceInterpolator1(lp.x1, lp.y1, lp.x2, lp.y2,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);
            _aa_data.m_li.adjust_forward();
        }
        int Count { get { return _aa_data.Count; } }
        bool IsVertical
        {
            get { return _aa_data.IsVertical; }
        }
        public void Loop()
        {
            if (Count > 0)
            {
                if (IsVertical)
                {
                    while (StepV()) ;
                }
                else
                {
                    while (StepH()) ;
                }
            }
        }
        bool StepH()
        {
            int dist;
            int dy;
            int s1 = _aa_data.BaseStepH(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            _aa_data.m_covers[Offset1++] = (byte)_ren.GetCover(s1);
            dy = 1;
            while ((dist = _aa_data.m_dist[dy] - s1) <= _aa_data.m_width)
            {
                _aa_data.m_covers[Offset1++] = (byte)_ren.GetCover(dist);
                ++dy;
            }

            dy = 1;
            while ((dist = _aa_data.m_dist[dy] + s1) <= _aa_data.m_width)
            {
                _aa_data.m_covers[--Offset0] = (byte)_ren.GetCover(dist);
                ++dy;
            }
            _ren.BlendSolidVSpan(_aa_data.m_x,
                                               _aa_data.m_y - dy + 1,
                                               Offset1 - Offset0,
                                               _aa_data.m_covers, Offset0);
            return ++_aa_data.m_step < _aa_data.m_count;
        }
        bool StepV()
        {
            int dist;
            int dx;
            int s1 = _aa_data.BaseStepV(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            _aa_data.m_covers[Offset1++] = (byte)_ren.GetCover(s1);
            dx = 1;
            while ((dist = _aa_data.m_dist[dx] - s1) <= _aa_data.m_width)
            {
                _aa_data.m_covers[Offset1++] = (byte)_ren.GetCover(dist);
                ++dx;
            }

            dx = 1;
            while ((dist = _aa_data.m_dist[dx] + s1) <= _aa_data.m_width)
            {
                _aa_data.m_covers[--Offset0] = (byte)_ren.GetCover(dist);
                ++dx;
            }
            _ren.BlendSolidHSpan(_aa_data.m_x - dx + 1,
                                               _aa_data.m_y,
                                               Offset1 - Offset0,
                                               _aa_data.m_covers, Offset0);
            return ++_aa_data.m_step < _aa_data.m_count;
        }
    }

    //====================================================line_interpolator_aa1
    struct LineInterpolatorAA1
    {
        DistanceInterpolator2 _m_di;
        LineInterpolatorAAData _aa_data;
        readonly OutlineRenderer _ren;
        public LineInterpolatorAA1(OutlineRenderer ren, LineParameters lp, int sx, int sy)
        {
            this._ren = ren;
            _aa_data = new Lines.LineInterpolatorAAData(ren, lp);
            _m_di = new DistanceInterpolator2(lp.x1, lp.y1, lp.x2, lp.y2, sx, sy,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);


            int dist1_start;
            int dist2_start;
            int npix = 1;
            if (lp.vertical)
            {
                do
                {
                    _aa_data.m_li.Prev();
                    _aa_data.m_y -= lp.inc;
                    _aa_data.m_x = (_aa_data.m_lp.x1 + _aa_data.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    //
                    if (lp.inc > 0) _m_di.DecY(_aa_data.m_x - _aa_data.m_old_x);
                    else _m_di.IncY(_aa_data.m_x - _aa_data.m_old_x);
                    //
                    _aa_data.m_old_x = _aa_data.m_x;
                    dist1_start = dist2_start = _m_di.DistanceStart;
                    int dx = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start += _m_di.DyStart;
                        dist2_start -= _m_di.DyStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dx;
                    }
                    while (_aa_data.m_dist[dx] <= _aa_data.m_width);
                    --_aa_data.m_step;
                    if (npix == 0) break;
                    npix = 0;
                }
                while (_aa_data.m_step >= -_aa_data.m_max_extent);
            }
            else
            {
                do
                {
                    _aa_data.m_li.Prev();
                    _aa_data.m_x -= lp.inc;
                    _aa_data.m_y = (_aa_data.m_lp.y1 + _aa_data.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    //
                    if (lp.inc > 0) _m_di.DecX(_aa_data.m_y - _aa_data.m_old_y);
                    else _m_di.IncX(_aa_data.m_y - _aa_data.m_old_y);
                    //
                    _aa_data.m_old_y = _aa_data.m_y;
                    dist1_start = dist2_start = _m_di.DistanceStart;
                    int dy = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start -= _m_di.DxStart;
                        dist2_start += _m_di.DxStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dy;
                    }
                    while (_aa_data.m_dist[dy] <= _aa_data.m_width);
                    --_aa_data.m_step;
                    if (npix == 0) break;
                    npix = 0;
                }
                while (_aa_data.m_step >= -_aa_data.m_max_extent);
            }
            _aa_data.m_li.adjust_forward();
        }
        bool IsVertical
        {
            get { return _aa_data.IsVertical; }
        }
        public void Loop()
        {
            if (IsVertical)
            {
                while (StepV()) ;
            }
            else
            {
                while (StepH()) ;
            }
        }
        //---------------------------------------------------------------------
        bool StepH()
        {
            int dist_start;
            int dist;
            int dy;
            int s1 = _aa_data.BaseStepH(ref _m_di);
            dist_start = _m_di.DistanceStart;
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_start <= 0)
            {
                _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
            }
            ++Offset1;
            dy = 1;
            while ((dist = _aa_data.m_dist[dy] - s1) <= _aa_data.m_width)
            {
                dist_start -= _m_di.DxStart;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_start = _m_di.DistanceStart;
            while ((dist = _aa_data.m_dist[dy] + s1) <= _aa_data.m_width)
            {
                dist_start += _m_di.DxStart;
                _aa_data.m_covers[--Offset0] = 0;
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset0] = (byte)_ren.GetCover(dist);
                }
                ++dy;
            }

            int len = Offset1 - Offset0;
            _ren.BlendSolidVSpan(_aa_data.m_x,
                                               _aa_data.m_y - dy + 1,
                                               len, _aa_data.m_covers,
                                               Offset0);
            return ++_aa_data.m_step < _aa_data.m_count;
        }


        //---------------------------------------------------------------------
        bool StepV()
        {
            int dist_start;
            int dist;
            int dx;
            int s1 = _aa_data.BaseStepV(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = _m_di.DistanceStart;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_start <= 0)
            {
                _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
            }
            ++Offset1;
            dx = 1;
            while ((dist = _aa_data.m_dist[dx] - s1) <= _aa_data.m_width)
            {
                dist_start += _m_di.DyStart;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_start = _m_di.DistanceStart;
            while ((dist = _aa_data.m_dist[dx] + s1) <= _aa_data.m_width)
            {
                dist_start -= _m_di.DyStart;
                _aa_data.m_covers[--Offset0] = 0;
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset0] = (byte)_ren.GetCover(dist);
                }
                ++dx;
            }
            _ren.BlendSolidHSpan(_aa_data.m_x - dx + 1,
                                               _aa_data.m_y,
                                               Offset1 - Offset0, _aa_data.m_covers,
                                               Offset0);
            return ++_aa_data.m_step < _aa_data.m_count;
        }
    }

    //====================================================line_interpolator_aa2
    struct LineInterpolatorAA2
    {
        DistanceInterpolator2 _m_di;
        LineInterpolatorAAData _aa_data;
        readonly OutlineRenderer _ren;
        public LineInterpolatorAA2(
            OutlineRenderer ren, LineParameters lp,
            int ex, int ey)
        {
            this._ren = ren;
            _aa_data = new Lines.LineInterpolatorAAData(ren, lp);
            _m_di = new DistanceInterpolator2(lp.x1, lp.y1, lp.x2, lp.y2, ex, ey,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK,
                 0);
            _aa_data.m_li.adjust_forward();
            _aa_data.m_step -= _aa_data.m_max_extent;
        }
        public void Loop()
        {
            if (IsVertical)
            {
                while (StepV()) ;
            }
            else
            {
                while (StepH()) ;
            }
        }
        bool IsVertical
        {
            get { return _aa_data.IsVertical; }
        }


        bool StepH()
        {
            int dist_end;
            int dist;
            int dy;
            int s1 = _aa_data.BaseStepH(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_end = _m_di.DistanceEnd;
            int npix = 0;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
                ++npix;
            }
            ++Offset1;
            dy = 1;
            while ((dist = _aa_data.m_dist[dy] - s1) <= _aa_data.m_width)
            {
                dist_end -= _m_di.DxEnd;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_end > 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_end = _m_di.DistanceEnd;
            while ((dist = _aa_data.m_dist[dy] + s1) <= _aa_data.m_width)
            {
                dist_end += _m_di.DxEnd;
                _aa_data.m_covers[--Offset0] = 0;
                if (dist_end > 0)
                {
                    _aa_data.m_covers[Offset0] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++dy;
            }
            _ren.BlendSolidVSpan(_aa_data.m_x,
                                               _aa_data.m_y - dy + 1,
                                               Offset1 - Offset0, _aa_data.m_covers,
                                               Offset0);
            return npix != 0 && ++_aa_data.m_step < _aa_data.m_count;
        }

        //---------------------------------------------------------------------
        bool StepV()
        {
            int dist_end;
            int dist;
            int dx;
            int s1 = _aa_data.BaseStepV(ref _m_di);
            int offset_0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = offset_0;
            dist_end = _m_di.DistanceEnd;
            int npix = 0;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
                ++npix;
            }
            ++Offset1;
            dx = 1;
            while ((dist = _aa_data.m_dist[dx] - s1) <= _aa_data.m_width)
            {
                dist_end += _m_di.DyEnd;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_end > 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_end = _m_di.DistanceEnd;
            while ((dist = _aa_data.m_dist[dx] + s1) <= _aa_data.m_width)
            {
                dist_end -= _m_di.DyEnd;
                _aa_data.m_covers[--offset_0] = 0;
                if (dist_end > 0)
                {
                    _aa_data.m_covers[offset_0] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++dx;
            }
            _ren.BlendSolidHSpan(_aa_data.m_x - dx + 1,
                                               _aa_data.m_y,
                                               Offset1 - offset_0, _aa_data.m_covers,
                                               offset_0);
            return npix != 0 && ++_aa_data.m_step < _aa_data.m_count;
        }
    }

    //====================================================line_interpolator_aa3
    struct LineInterpolatorAA3
    {
        DistanceInterpolator3 _m_di;
        LineInterpolatorAAData _aa_data;
        readonly OutlineRenderer _ren;
        public LineInterpolatorAA3(OutlineRenderer ren, LineParameters lp,
                              int sx, int sy, int ex, int ey)
        {
            this._ren = ren;
            _aa_data = new Lines.LineInterpolatorAAData(ren, lp);
            _m_di = new DistanceInterpolator3(lp.x1, lp.y1, lp.x2, lp.y2, sx, sy, ex, ey,
                 lp.x1 & ~LineAA.SUBPIXEL_MARK, lp.y1 & ~LineAA.SUBPIXEL_MARK);
            int dist1_start;
            int dist2_start;
            int npix = 1;
            if (lp.vertical)
            {
                do
                {
                    _aa_data.m_li.Prev();
                    _aa_data.m_y -= lp.inc;
                    _aa_data.m_x = (_aa_data.m_lp.x1 + _aa_data.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) _m_di.DecY(_aa_data.m_x - _aa_data.m_old_x);
                    else _m_di.IncY(_aa_data.m_x - _aa_data.m_old_x);
                    _aa_data.m_old_x = _aa_data.m_x;
                    dist1_start = dist2_start = _m_di.dist_start;
                    int dx = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start += _m_di.DyStart;
                        dist2_start -= _m_di.DyStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dx;
                    }
                    while (_aa_data.m_dist[dx] <= _aa_data.m_width);
                    if (npix == 0) break;
                    npix = 0;
                }
                while (--_aa_data.m_step >= -_aa_data.m_max_extent);
            }
            else
            {
                do
                {
                    _aa_data.m_li.Prev();
                    _aa_data.m_x -= lp.inc;
                    _aa_data.m_y = (_aa_data.m_lp.y1 + _aa_data.m_li.Y) >> LineAA.SUBPIXEL_SHIFT;
                    if (lp.inc > 0) _m_di.DecX(_aa_data.m_y - _aa_data.m_old_y);
                    else _m_di.IncX(_aa_data.m_y - _aa_data.m_old_y);
                    _aa_data.m_old_y = _aa_data.m_y;
                    dist1_start = dist2_start = _m_di.dist_start;
                    int dy = 0;
                    if (dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start -= _m_di.DxStart;
                        dist2_start += _m_di.DxStart;
                        if (dist1_start < 0) ++npix;
                        if (dist2_start < 0) ++npix;
                        ++dy;
                    }
                    while (_aa_data.m_dist[dy] <= _aa_data.m_width);
                    if (npix == 0) break;
                    npix = 0;
                }
                while (--_aa_data.m_step >= -_aa_data.m_max_extent);
            }
            _aa_data.m_li.adjust_forward();
            _aa_data.m_step -= _aa_data.m_max_extent;
        }

        public void Loop()
        {
            if (IsVertical)
            {
                while (StepV()) ;
            }
            else
            {
                while (StepH()) ;
            }
        }
        bool IsVertical
        {
            get { return _aa_data.IsVertical; }
        }

        bool StepH()
        {
            int dist_start;
            int dist_end;
            int dist;
            int dy;
            int s1 = _aa_data.BaseStepH(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = _m_di.dist_start;
            dist_end = _m_di.dist_end;
            int npix = 0;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
                }
                ++npix;
            }
            ++Offset1;
            dy = 1;
            while ((dist = _aa_data.m_dist[dy] - s1) <= _aa_data.m_width)
            {
                dist_start -= _m_di.DxStart;
                dist_end -= _m_di.DxEnd;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dy;
            }

            dy = 1;
            dist_start = _m_di.dist_start;
            dist_end = _m_di.dist_end;
            while ((dist = _aa_data.m_dist[dy] + s1) <= _aa_data.m_width)
            {
                dist_start += _m_di.DxStart;
                dist_end += _m_di.DxEnd;
                _aa_data.m_covers[--Offset0] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    _aa_data.m_covers[Offset0] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++dy;
            }
            _ren.BlendSolidVSpan(_aa_data.m_x,
                                               _aa_data.m_y - dy + 1,
                                               Offset1 - Offset0, _aa_data.m_covers,
                                               Offset0);
            return npix != 0 && ++_aa_data.m_step < _aa_data.m_count;
        }

        //---------------------------------------------------------------------
        bool StepV()
        {
            int dist_start;
            int dist_end;
            int dist;
            int dx;
            int s1 = _aa_data.BaseStepV(ref _m_di);
            int Offset0 = LineInterpolatorAAData.MAX_HALF_WIDTH + 2;
            int Offset1 = Offset0;
            dist_start = _m_di.dist_start;
            dist_end = _m_di.dist_end;
            int npix = 0;
            _aa_data.m_covers[Offset1] = 0;
            if (dist_end > 0)
            {
                if (dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(s1);
                }
                ++npix;
            }
            ++Offset1;
            dx = 1;
            while ((dist = _aa_data.m_dist[dx] - s1) <= _aa_data.m_width)
            {
                dist_start += _m_di.DyStart;
                dist_end += _m_di.DyEnd;
                _aa_data.m_covers[Offset1] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    _aa_data.m_covers[Offset1] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++Offset1;
                ++dx;
            }

            dx = 1;
            dist_start = _m_di.dist_start;
            dist_end = _m_di.dist_end;
            while ((dist = _aa_data.m_dist[dx] + s1) <= _aa_data.m_width)
            {
                dist_start -= _m_di.DyStart;
                dist_end -= _m_di.DyEnd;
                _aa_data.m_covers[--Offset0] = 0;
                if (dist_end > 0 && dist_start <= 0)
                {
                    _aa_data.m_covers[Offset0] = (byte)_ren.GetCover(dist);
                    ++npix;
                }
                ++dx;
            }
            _ren.BlendSolidHSpan(_aa_data.m_x - dx + 1,
                                               _aa_data.m_y,
                                               Offset1 - Offset0, _aa_data.m_covers,
                                               Offset0);
            return npix != 0 && ++_aa_data.m_step < _aa_data.m_count;
        }
    }
}