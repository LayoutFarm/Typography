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
using System.Collections.Generic;
using PixelFarm.CpuBlit.PrimitiveProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;

namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    //==========================================================line_profile_aa
    //
    // See Implementation agg_line_profile_aa.cpp 
    // 
    public class LineProfileAnitAlias
    {
        const int SUBPIX_SHIFT = 8;
        const int SUBPIX_SCALE = 1 << SUBPIX_SHIFT;

        const int SUBPIX_MASK = SUBPIX_SCALE - 1;
        const int AA_SHIFT = 8;
        const int AA_SCALE = 1 << AA_SHIFT;
        const int AA_MASK = AA_SCALE - 1;

        byte[] _profile = new byte[64];
        byte[] _gamma;
        float _subpixel_width;
        double _min_width;
        double _smoother_width;


        static byte[] s_gamma_none;
        static LineProfileAnitAlias()
        {
            //GammaNone => just return i***
            s_gamma_none = new byte[AA_SCALE];
            for (int i = AA_SCALE - 1; i >= 0; --i)
            {
                s_gamma_none[i] = (byte)(AggMath.uround(((float)(i) / AA_MASK) * AA_MASK));
            }
        }

        public LineProfileAnitAlias(double w, IGammaFunction gamma_function)
        {
            //1. init value
            _subpixel_width = 0;
            _min_width = 1.0;
            _smoother_width = 1.0;

            //2. set gamma before set width
            SetGamma(gamma_function);
            //3. set width table
            SetWidth(w);
        }
        public float SubPixelWidth
        {
            get => _subpixel_width;
            set => SetWidth(value);               //subpixel width
        }

        //#if DEBUG
        //        static int dbugCount1;
        //#endif
        public byte GetProfileValue(int dist)
        {
#if DEBUG
            //dbugCount1++;
            //if (dbugCount1 < 17)
            //{
            //    Console.WriteLine(dbugCount1 + " " + dist);
            //}
            //else
            //{

            //}
            //int tmp = dist + SUBPIX_SCALE * 2;
            //if (tmp < 0 || tmp > m_profile.Length)
            //{
            //    throw new NotSupportedException();
            //    //?
            //    return 255;
            //}
#endif

            return _profile[dist + SUBPIX_SCALE * 2];
        }
        byte[] GetProfileBuffer(double w)
        {
            _subpixel_width = AggMath.uround(w * SUBPIX_SCALE);
            int size = (int)_subpixel_width + SUBPIX_SCALE * 6;
            if (size > _profile.Length)
            {
                //clear ?
                _profile = new byte[size];
                //m_profile.Resize(size);
            }
            return _profile;
        }
        void SetGamma(IGammaFunction gamma_function)
        {
            //TODO:review gamma again***
            if (gamma_function == null)
            {
                _gamma = s_gamma_none;
            }
            else
            {
                _gamma = new byte[AA_SCALE];
                for (int i = AA_SCALE - 1; i >= 0; --i)
                {
                    //pass i to gamma func ***
                    _gamma[i] = (byte)(AggMath.uround(gamma_function.GetGamma((float)(i) / AA_MASK) * AA_MASK));
                }
            }
        }

        //---------------------------------------------
        // void line_profile_aa::width(double w)
        void SetWidth(double w)
        {
            if (w < 0.0) w = 0.0;
            //
            if (w < _smoother_width) w += w;
            else/**/                  w += _smoother_width;
            //
            w *= 0.5;
            w -= _smoother_width;
            double s = _smoother_width;
            if (w < 0.0)
            {
                s += w;
                w = 0.0;
            }
            SetCenterAndSmootherWidth(w, s);
        }
        //---------------------------------------------
        //  void line_profile_aa::set(double center_width, double smoother_width)
        void SetCenterAndSmootherWidth(double center_width, double smoother_width)
        {
            double base_val = 1.0;
            if (center_width == 0.0) center_width = 1.0 / SUBPIX_SCALE;
            if (smoother_width == 0.0) smoother_width = 1.0 / SUBPIX_SCALE;
            //
            double width = center_width + smoother_width;
            if (width < _min_width)
            {
                double k = width / _min_width;
                base_val *= k;
                center_width /= k;
                smoother_width /= k;
            }

            byte[] ch = GetProfileBuffer(center_width + smoother_width);
            int chIndex = 0;
            //
#if DEBUG
            if (center_width * SUBPIX_SCALE > int.MaxValue ||
               smoother_width * SUBPIX_SCALE > int.MaxValue)
            {

            }
#endif
            int subpixel_center_width = (int)(center_width * SUBPIX_SCALE);
            int subpixel_smoother_width = (int)(smoother_width * SUBPIX_SCALE);
            //
            int ch_center = SUBPIX_SCALE * 2;
            int ch_smoother = ch_center + subpixel_center_width;
            //
            int i;
            int val = _gamma[(int)(base_val * AA_MASK)];
            chIndex = ch_center;
            for (i = 0; i < subpixel_center_width; i++)
            {
                ch[chIndex++] = (byte)val;
            }

            for (i = 0; i < subpixel_smoother_width; i++)
            {
                ch[ch_smoother++] =
                    _gamma[(int)((base_val -
                                      base_val *
                                      ((double)(i) / subpixel_smoother_width)) * AA_MASK)];
            }

            int n_smoother = ch.Length -
                                  subpixel_smoother_width -
                                  subpixel_center_width -
                                  SUBPIX_SCALE * 2;
            val = _gamma[0];
            for (i = 0; i < n_smoother; i++)
            {
                ch[ch_smoother++] = (byte)val;
            }

            chIndex = ch_center;
            for (i = 0; i < SUBPIX_SCALE * 2; i++)
            {
                ch[--chIndex] = ch[ch_center++];
            }

            for (i = ch.Length - 1; i >= 0; --i)
            {
                _profile[i] = ch[i];
            }
        }
    }




    //======================================================renderer_outline_aa
    public class OutlineRenderer : LineRenderer
    {
        const int MAX_HALF_WIDTH = 64;
        PixelProcessing.IBitmapBlender _destImageSurface;
        LineProfileAnitAlias _lineProfile;
        RectInt _clippingRectangle;
        bool _doClipping;
        PixelProcessing.PixelBlender32 _destPixelBlender;

#if false
        public int min_x() { throw new System.NotImplementedException(); }
        public int min_y() { throw new System.NotImplementedException(); }
        public int max_x() { throw new System.NotImplementedException(); }
        public int max_y() { throw new System.NotImplementedException(); }
        public void gamma(IGammaFunction gamma_function) { throw new System.NotImplementedException(); }
        public bool sweep_scanline(IScanlineCache sl) { throw new System.NotImplementedException(); }
        public void reset() { throw new System.NotImplementedException(); }
#endif

        //---------------------------------------------------------------------
        public OutlineRenderer(PixelProcessing.IBitmapBlender destImage,
            PixelProcessing.PixelBlender32 destPixelBlender,
            LineProfileAnitAlias profile)
        {
            _destImageSurface = destImage;
            _lineProfile = profile;
            _clippingRectangle = new RectInt(0, 0, 0, 0);
            _doClipping = false;
            _destPixelBlender = destPixelBlender;
        }


        //---------------------------------------------------------------------
        public int SubPixelWidth => (int)_lineProfile.SubPixelWidth;

        //---------------------------------------------------------------------
        public void ResetClipping() { _doClipping = false; }
        public void SetClipBox(double x1, double y1, double x2, double y2)
        {
            _clippingRectangle.Left = LineCoordSat.Convert(x1);
            _clippingRectangle.Bottom = LineCoordSat.Convert(y1);
            _clippingRectangle.Right = LineCoordSat.Convert(x2);
            _clippingRectangle.Top = LineCoordSat.Convert(y2);

            //clippingRectangle.Left = LineCoordSat.Convert(x1);
            //clippingRectangle.Top = LineCoordSat.Convert(y1);
            //clippingRectangle.Right = LineCoordSat.Convert(x2);
            //clippingRectangle.Bottom = LineCoordSat.Convert(y2);
            _doClipping = true;
        }

        //---------------------------------------------------------------------
        public byte GetCover(int d)
        {
            return _lineProfile.GetProfileValue(d);
        }

        public void BlendSolidHSpan(int x, int y, int len, byte[] covers, int coversOffset)
        {
            _destImageSurface.BlendSolidHSpan(x, y, len, Color, covers, coversOffset);
        }

        public void BlendSolidVSpan(int x, int y, int len, byte[] covers, int coversOffset)
        {
            _destImageSurface.BlendSolidVSpan(x, y, len, Color, covers, coversOffset);
        }

        public static bool AccurateJoinOnly { get { return false; } }

        public override void SemiDotHLine(CompareFunction cmp,
                           int xc1, int yc1, int xc2, int yc2,
                           int x1, int y1, int x2)
        {
            byte[] covers = new byte[MAX_HALF_WIDTH * 2 + 4];
            int offset0 = 0;
            int offset1 = 0;
            int x = x1 << LineAA.SUBPIXEL_SHIFT;
            int y = y1 << LineAA.SUBPIXEL_SHIFT;
            int w = SubPixelWidth;
            DistanceInterpolator0 di = new DistanceInterpolator0(xc1, yc1, xc2, yc2, x, y);
            x += LineAA.SUBPIXEL_SCALE / 2;
            y += LineAA.SUBPIXEL_SCALE / 2;
            int x0 = x1;
            int dx = x - xc1;
            int dy = y - yc1;
            do
            {
                int d = (int)(AggMath.fast_sqrt(dx * dx + dy * dy));
                covers[offset1] = 0;
                if (cmp(di.Distance) && d <= w)
                {
                    covers[offset1] = GetCover(d);
                }
                ++offset1;
                dx += LineAA.SUBPIXEL_SCALE;
                di.IncX();
            }
            while (++x1 <= x2);
            _destImageSurface.BlendSolidHSpan(x0, y1,
                                     offset1 - offset0,
                                     Color, covers,
                                     offset0);
        }

        public override void SemiDot(CompareFunction cmp, int xc1, int yc1, int xc2, int yc2)
        {
            if (_doClipping && ClipLiangBarsky.Flags(xc1, yc1, _clippingRectangle) != 0) return;
            int r = ((SubPixelWidth + LineAA.SUBPIXEL_MARK) >> LineAA.SUBPIXEL_SHIFT);
            if (r < 1) r = 1;
            EllipseBresenhamInterpolator ei = new EllipseBresenhamInterpolator(r, r);
            int dx = 0;
            int dy = -r;
            int dy0 = dy;
            int dx0 = dx;
            int x = xc1 >> LineAA.SUBPIXEL_SHIFT;
            int y = yc1 >> LineAA.SUBPIXEL_SHIFT;
            do
            {
                dx += ei.Dx;
                dy += ei.Dy;
                if (dy != dy0)
                {
                    SemiDotHLine(cmp, xc1, yc1, xc2, yc2, x - dx0, y + dy0, x + dx0);
                    SemiDotHLine(cmp, xc1, yc1, xc2, yc2, x - dx0, y - dy0, x + dx0);
                }
                dx0 = dx;
                dy0 = dy;
                ei.Next();
            }
            while (dy < 0);
            SemiDotHLine(cmp, xc1, yc1, xc2, yc2, x - dx0, y + dy0, x + dx0);
        }

        public void PineHLine(int xc, int yc, int xp1, int yp1, int xp2, int yp2,
                       int xh1, int yh1, int xh2)
        {
            if (_doClipping && ClipLiangBarsky.Flags(xc, yc, _clippingRectangle) != 0) return;
            byte[] covers = new byte[MAX_HALF_WIDTH * 2 + 4];
            int index0 = 0;
            int index1 = 0;
            int x = xh1 << LineAA.SUBPIXEL_SHIFT;
            int y = yh1 << LineAA.SUBPIXEL_SHIFT;
            int w = SubPixelWidth;
            DistanceInterpolator00 di = new DistanceInterpolator00(xc, yc, xp1, yp1, xp2, yp2, x, y);
            x += LineAA.SUBPIXEL_SCALE / 2;
            y += LineAA.SUBPIXEL_SCALE / 2;
            int xh0 = xh1;
            int dx = x - xc;
            int dy = y - yc;
            do
            {
                int d = (int)(AggMath.fast_sqrt(dx * dx + dy * dy));
                covers[index1] = 0;
                if (di.Distance1 <= 0 && di.Distance2 > 0 && d <= w)
                {
                    covers[index1] = GetCover(d);
                }
                ++index1;
                dx += LineAA.SUBPIXEL_SCALE;
                di.IncX();
            }
            while (++xh1 <= xh2);
            _destImageSurface.BlendSolidHSpan(xh0, yh1, index1 - index0, Color, covers, index0);
        }

        public override void Pie(int xc, int yc, int x1, int y1, int x2, int y2)
        {
            int r = ((SubPixelWidth + LineAA.SUBPIXEL_MARK) >> LineAA.SUBPIXEL_SHIFT);
            if (r < 1) r = 1;
            EllipseBresenhamInterpolator ei = new EllipseBresenhamInterpolator(r, r);
            int dx = 0;
            int dy = -r;
            int dy0 = dy;
            int dx0 = dx;
            int x = xc >> LineAA.SUBPIXEL_SHIFT;
            int y = yc >> LineAA.SUBPIXEL_SHIFT;
            do
            {
                dx += ei.Dx;
                dy += ei.Dy;
                if (dy != dy0)
                {
                    PineHLine(xc, yc, x1, y1, x2, y2, x - dx0, y + dy0, x + dx0);
                    PineHLine(xc, yc, x1, y1, x2, y2, x - dx0, y - dy0, x + dx0);
                }
                dx0 = dx;
                dy0 = dy;
                ei.Next();
            }
            while (dy < 0);
            PineHLine(xc, yc, x1, y1, x2, y2, x - dx0, y + dy0, x + dx0);
        }

        const int MAX_LINE0_NO_CLIP_RECURSIVE = 32;

        void Line0NoClip(int level, LineParameters lp)
        {
            if (level > MAX_LINE0_NO_CLIP_RECURSIVE)
            {
                return;
            }

            //recursive
            if (lp.len > LineAA.MAX_LENGTH)
            {
                LineParameters lp1, lp2;
                if (lp.Divide(out lp1, out lp2))
                {
                    //recursive
                    Line0NoClip(level + 1, lp1);
                    Line0NoClip(level + 1, lp2);
                }
                return;
            }
            (new LineInterpolatorAA0(this, lp)).Loop();
        }

        public override void Line0(LineParameters lp)
        {
            if (_doClipping)
            {
                int x1 = lp.x1;
                int y1 = lp.y1;
                int x2 = lp.x2;
                int y2 = lp.y2;
                int flags = ClipLiangBarsky.ClipLineSegment(ref x1, ref y1, ref x2, ref y2, _clippingRectangle);
                if ((flags & 4) == 0)
                {
                    if (flags != 0)
                    {
                        LineParameters lp2 = new LineParameters(x1, y1, x2, y2,
                                                AggMath.uround(AggMath.calc_distance(x1, y1, x2, y2)));
                        Line0NoClip(0, lp2);
                    }
                    else
                    {
                        Line0NoClip(0, lp);
                    }
                }
            }
            else
            {
                Line0NoClip(0, lp);
            }
        }

        void Line1NoClip(LineParameters lp, int sx, int sy)
        {
            if (lp.len > LineAA.MAX_LENGTH)
            {
                LineParameters lp1, lp2;
                if (lp.Divide(out lp1, out lp2))
                {
                    Line1NoClip(lp1, (lp.x1 + sx) >> 1, (lp.y1 + sy) >> 1);
                    Line1NoClip(lp2, lp1.x2 + (lp1.y2 - lp1.y1), lp1.y2 - (lp1.x2 - lp1.x1));
                }
                return;
            }

            LineAA.FixDegenBisectrixStart(lp, ref sx, ref sy);
            using (var aa = new LineInterpolatorAA1(this, lp, sx, sy))
            {
                aa.Loop();
            }
        }

        public override void Line1(LineParameters lp, int sx, int sy)
        {
            if (_doClipping)
            {
                int x1 = lp.x1;
                int y1 = lp.y1;
                int x2 = lp.x2;
                int y2 = lp.y2;
                int flags = ClipLiangBarsky.ClipLineSegment(ref x1, ref y1, ref x2, ref y2, _clippingRectangle);
                if ((flags & 4) == 0)
                {
                    if (flags != 0)
                    {
                        LineParameters lp2 = new LineParameters(x1, y1, x2, y2,
                                                 AggMath.uround(AggMath.calc_distance(x1, y1, x2, y2)));
                        if (((int)flags & 1) != 0)
                        {
                            sx = x1 + (y2 - y1);
                            sy = y1 - (x2 - x1);
                        }
                        else
                        {
                            while (Math.Abs(sx - lp.x1) + Math.Abs(sy - lp.y1) > lp2.len)
                            {
                                sx = (lp.x1 + sx) >> 1;
                                sy = (lp.y1 + sy) >> 1;
                            }
                        }
                        Line1NoClip(lp2, sx, sy);
                    }
                    else
                    {
                        Line1NoClip(lp, sx, sy);
                    }
                }
            }
            else
            {
                Line1NoClip(lp, sx, sy);
            }
        }

        void Line2NoClip(LineParameters lp, int ex, int ey)
        {
            if (lp.len > LineAA.MAX_LENGTH)
            {
                LineParameters lp1, lp2;
                if (lp.Divide(out lp1, out lp2))
                {
                    Line2NoClip(lp1, lp1.x2 + (lp1.y2 - lp1.y1), lp1.y2 - (lp1.x2 - lp1.x1));
                    Line2NoClip(lp2, (lp.x2 + ex) >> 1, (lp.y2 + ey) >> 1);
                }
                return;
            }

            LineAA.FixDegenBisectrixEnd(lp, ref ex, ref ey);
            using (var aa = new LineInterpolatorAA2(this, lp, ex, ey))
            {
                aa.Loop();
            }

        }

        public override void Line2(LineParameters lp, int ex, int ey)
        {
            if (_doClipping)
            {
                int x1 = lp.x1;
                int y1 = lp.y1;
                int x2 = lp.x2;
                int y2 = lp.y2;
                int flags = ClipLiangBarsky.ClipLineSegment(ref x1, ref y1, ref x2, ref y2, _clippingRectangle);
                if ((flags & 4) == 0)
                {
                    if (flags != 0)
                    {
                        LineParameters lp2 = new LineParameters(x1, y1, x2, y2,
                                                 AggMath.uround(AggMath.calc_distance(x1, y1, x2, y2)));
                        if ((flags & 2) != 0)
                        {
                            ex = x2 + (y2 - y1);
                            ey = y2 - (x2 - x1);
                        }
                        else
                        {
                            while (Math.Abs(ex - lp.x2) + Math.Abs(ey - lp.y2) > lp2.len)
                            {
                                ex = (lp.x2 + ex) >> 1;
                                ey = (lp.y2 + ey) >> 1;
                            }
                        }
                        Line2NoClip(lp2, ex, ey);
                    }
                    else
                    {
                        Line2NoClip(lp, ex, ey);
                    }
                }
            }
            else
            {
                Line2NoClip(lp, ex, ey);
            }
        }

        void Line3NoClip(LineParameters lp,
                         int sx, int sy, int ex, int ey)
        {
            if (lp.len > LineAA.MAX_LENGTH)
            {
                LineParameters lp1, lp2;
                if (lp.Divide(out lp1, out lp2))
                {
                    int mx = lp1.x2 + (lp1.y2 - lp1.y1);
                    int my = lp1.y2 - (lp1.x2 - lp1.x1);
                    Line3NoClip(lp1, (lp.x1 + sx) >> 1, (lp.y1 + sy) >> 1, mx, my);
                    Line3NoClip(lp2, mx, my, (lp.x2 + ex) >> 1, (lp.y2 + ey) >> 1);
                }
                return;
            }

            LineAA.FixDegenBisectrixStart(lp, ref sx, ref sy);
            LineAA.FixDegenBisectrixEnd(lp, ref ex, ref ey);
            using (var aa = new LineInterpolatorAA3(this, lp, sx, sy, ex, ey))
            {
                aa.Loop();
            }

        }

        public override void Line3(LineParameters lp,
                   int sx, int sy, int ex, int ey)
        {
            if (_doClipping)
            {
                int x1 = lp.x1;
                int y1 = lp.y1;
                int x2 = lp.x2;
                int y2 = lp.y2;
                int flags = ClipLiangBarsky.ClipLineSegment(ref x1, ref y1, ref x2, ref y2, _clippingRectangle);
                if ((flags & 4) == 0)
                {
                    if (flags != 0)
                    {
                        LineParameters lp2 = new LineParameters(x1, y1, x2, y2,
                            AggMath.uround(AggMath.calc_distance(x1, y1, x2, y2)));
                        if ((flags & 1) != 0)
                        {
                            sx = x1 + (y2 - y1);
                            sy = y1 - (x2 - x1);
                        }
                        else
                        {
                            while (Math.Abs(sx - lp.x1) + Math.Abs(sy - lp.y1) > lp2.len)
                            {
                                sx = (lp.x1 + sx) >> 1;
                                sy = (lp.y1 + sy) >> 1;
                            }
                        }
                        if ((flags & 2) != 0)
                        {
                            ex = x2 + (y2 - y1);
                            ey = y2 - (x2 - x1);
                        }
                        else
                        {
                            while (Math.Abs(ex - lp.x2) + Math.Abs(ey - lp.y2) > lp2.len)
                            {
                                ex = (lp.x2 + ex) >> 1;
                                ey = (lp.y2 + ey) >> 1;
                            }
                        }
                        Line3NoClip(lp2, sx, sy, ex, ey);
                    }
                    else
                    {
                        Line3NoClip(lp, sx, sy, ex, ey);
                    }
                }
            }
            else
            {
                Line3NoClip(lp, sx, sy, ex, ey);
            }
        }

        //LineInterpolatorAAData.max
        //m_dist = new int[MAX_HALF_WIDTH + 1];
        //    m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];

        Stack<int[]> _freeDistPool = new Stack<int[]>();
        Stack<byte[]> _freeConvPool = new Stack<byte[]>();

        internal int[] GetFreeDistArray()
        {
            if (_freeDistPool.Count > 0)
            {
                return _freeDistPool.Pop();
            }
            else
            {
                //m_dist = new int[MAX_HALF_WIDTH + 1];
                //m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];


                return new int[LineInterpolatorAAData.MAX_HALF_WIDTH + 1];
            }
        }
        internal void ReleaseDistArray(int[] distArray)
        {
            //clear and add to list
            Array.Clear(distArray, 0, distArray.Length);
            _freeDistPool.Push(distArray);
        }
        internal byte[] GetFreeConvArray()
        {
            if (_freeConvPool.Count > 0)
            {
                return _freeConvPool.Pop();
            }
            else
            {
                //m_dist = new int[MAX_HALF_WIDTH + 1];
                //m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];
                return new byte[(MAX_HALF_WIDTH + 1) * 2];
            }
        }
        internal void ReleaseConvArray(byte[] convArray)
        {
            //clear and add to list
            Array.Clear(convArray, 0, convArray.Length);
            _freeConvPool.Push(convArray);
        }
    }
}
