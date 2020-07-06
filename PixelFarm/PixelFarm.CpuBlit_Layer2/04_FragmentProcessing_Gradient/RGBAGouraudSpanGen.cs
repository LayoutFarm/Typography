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
//
// Adaptation for high precision colors has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.FragmentProcessing
{
    //=======================================================span_gouraud_rgba
    public sealed class RGBAGouraudSpanGen : ISpanGenerator
    {
        bool _swap;
        int _y2;
        RGBA_Calculator _rgba1;
        RGBA_Calculator _rgba2;
        RGBA_Calculator _rgba3;

        GouraudVerticeBuilder.CoordAndColor _c0, _c1, _c2;

        const int SUBPIXEL_SHIFT = 4;
        const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;
        //--------------------------------------------------------------------
        struct RGBA_Calculator
        {
            public void Init(GouraudVerticeBuilder.CoordAndColor c1, GouraudVerticeBuilder.CoordAndColor c2)
            {
                _x1 = c1.x - 0.5;
                _y1 = c1.y - 0.5;
                _dx = c2.x - c1.x;
                double dy = c2.y - c1.y;
                _1dy = (dy < 1e-5) ? 1e5 : 1.0 / dy;

                _r1 = c1.color.R;
                _g1 = c1.color.G;
                _b1 = c1.color.B;
                _a1 = c1.color.A;

                _dr = c2.color.R - _r1;
                _dg = c2.color.G - _g1;
                _db = c2.color.B - _b1;
                _da = c2.color.A - _a1;
            }

            public void Calculate(double y)
            {
                double k = (y - _y1) * _1dy;
                if (k < 0.0) k = 0.0;
                if (k > 1.0) k = 1.0;
                _r = _r1 + AggMath.iround(_dr * k);
                _g = _g1 + AggMath.iround(_dg * k);
                _b = _b1 + AggMath.iround(_db * k);
                _a = _a1 + AggMath.iround(_da * k);
                _x = AggMath.iround((_x1 + _dx * k) * (double)SUBPIXEL_SCALE);
            }

            public double _x1;
            public double _y1;
            public double _dx;
            public double _1dy;
            public int _r1;
            public int _g1;
            public int _b1;
            public int _a1;
            public int _dr;
            public int _dg;
            public int _db;
            public int _da;
            public int _r;
            public int _g;
            public int _b;
            public int _a;
            public int _x;
        }

        //--------------------------------------------------------------------
        public RGBAGouraudSpanGen() { }

        struct LineInterpolatorDDA255
        {
            //my custom extension of LineInterpolatorDDA
            int _y;
            int _dy;
            readonly int _inc;
            readonly int _fractionShift;

            public LineInterpolatorDDA255(int y1, int y2, int count, int fractionShift)
            {
                _fractionShift = fractionShift;
                _y = (y1);
                _inc = (((y2 - y1) << _fractionShift) / (int)(count));
                _dy = (0);
            }

            public void Next() => _dy += _inc;//public void operator ++ ()

            public void Prev() => _dy -= _inc;//public void operator -- ()

            public void NextN() => _dy += _inc * SUBPIXEL_SCALE;//public void operator += (int n)

            public void Prev(int n) => _dy -= _inc * n;//public void operator -= (int n)

            //--------------------------------------------------------------------
            public int y() => _y + (_dy >> (_fractionShift));  // - m_YShift)); }
                                                               //
            public int dy() => _dy;
            //--------------------------------------------------------------------
            //special
            public int y_clamp0_255()
            {
                int v = _y + (_dy >> (_fractionShift));

                if (v < 0)
                {
                    return 0;
                }
                else if (v > 255)
                {
                    return 255;
                }
                else
                {
                    return v;
                }
            }
        }


        public void SetColorAndCoords(
            GouraudVerticeBuilder.CoordAndColor c0,
            GouraudVerticeBuilder.CoordAndColor c1,
            GouraudVerticeBuilder.CoordAndColor c2)
        {
            _c0 = c0;
            _c1 = c1;
            _c2 = c2;
        }
        void ISpanGenerator.Prepare()
        {

            _y2 = (int)_c1.y;
            _swap = AggMath.Cross(_c0.x, _c0.y,
                                   _c2.x, _c2.y,
                                   _c1.x, _c1.y) < 0.0;
            _rgba1.Init(_c0, _c2);
            _rgba2.Init(_c0, _c1);
            _rgba3.Init(_c1, _c2);
        }

        void ISpanGenerator.GenerateColors(Color[] outputColors, int startIndex, int x, int y, int len)
        {

            _rgba1.Calculate(y);//(m_rgba1.m_1dy > 2) ? m_rgba1.m_y1 : y);
            RGBA_Calculator pc1 = _rgba1;
            RGBA_Calculator pc2 = _rgba2;
            if (y <= _y2)
            {
                // Bottom part of the triangle (first subtriangle)
                //-------------------------
                _rgba2.Calculate(y + _rgba2._1dy);
            }
            else
            {
                // Upper part (second subtriangle)
                _rgba3.Calculate(y - _rgba3._1dy);
                //-------------------------
                pc2 = _rgba3;
            }

            if (_swap)
            {
                // It means that the triangle is oriented clockwise, 
                // so that we need to swap the controlling structures
                //-------------------------
                RGBA_Calculator t = pc2;
                pc2 = pc1;
                pc1 = t;
            }

            // Get the horizontal length with subpixel accuracy
            // and protect it from division by zero
            //-------------------------
            int nlen = Math.Abs(pc2._x - pc1._x);
            if (nlen <= 0) nlen = 1;

#if DEBUG
            if (SUBPIXEL_SHIFT != 4)
            {
                throw new NotSupportedException();
                //use LineInterpolatorDDA instead of LineInterpolatorDDA255
            }
#endif


            var line_r = new LineInterpolatorDDA255(pc1._r, pc2._r, nlen, 14);
            var line_g = new LineInterpolatorDDA255(pc1._g, pc2._g, nlen, 14);
            var line_b = new LineInterpolatorDDA255(pc1._b, pc2._b, nlen, 14);
            var line_a = new LineInterpolatorDDA255(pc1._a, pc2._a, nlen, 14);
            // Calculate the starting point of the gradient with subpixel 
            // accuracy and correct (roll back) the interpolators.
            // This operation will also clip the beginning of the span
            // if necessary.
            //-------------------------
            int start = pc1._x - (x << (int)SUBPIXEL_SHIFT);
            line_r.Prev(start);
            line_g.Prev(start);
            line_b.Prev(start);
            line_a.Prev(start);
            nlen += start;
            //int vr, vg, vb, va;

            // Beginning part of the span. Since we rolled back the 
            // interpolators, the color values may have overflowed.
            // So that, we render the beginning part with checking 
            // for overflow. It lasts until "start" is positive;
            // typically it's 1-2 pixels, but may be more in some cases.
            //-------------------------
            while (len != 0 && start > 0)
            {
                //vr = line_r.y();
                //vg = line_g.y();
                //vb = line_b.y();
                //va = line_a.y();

                ////clamp between 0 and 255--------
                //if (vr < 0) { vr = 0; } else if (vr > 255) { vr = 255; }
                //if (vg < 0) { vg = 0; } else if (vg > 255) { vg = 255; }
                //if (vb < 0) { vb = 0; } else if (vb > 255) { vb = 255; }
                //if (va < 0) { va = 0; } else if (va > 255) { va = 255; }
                ////-------------------------------

                //outputColors[startIndex].red = (byte)vr;
                //outputColors[startIndex].green = (byte)vg;
                //outputColors[startIndex].blue = (byte)vb;
                //outputColors[startIndex].alpha = (byte)va;

                outputColors[startIndex] = Color.FromArgb(
                    line_a.y_clamp0_255(),
                    line_r.y_clamp0_255(),
                    line_g.y_clamp0_255(),
                    line_b.y_clamp0_255());

                line_r.NextN();
                line_g.NextN();
                line_b.NextN();
                line_a.NextN();
                nlen -= SUBPIXEL_SCALE;
                start -= SUBPIXEL_SCALE;
                ++startIndex;
                --len;
            }

            // Middle part, no checking for overflow.
            // Actual spans can be longer than the calculated length
            // because of anti-aliasing, thus, the interpolators can 
            // overflow. But while "nlen" is positive we are safe.
            //-------------------------
            while (len != 0 && nlen > 0)
            {
                //outputColors[startIndex].red = ((byte)line_r.y());
                //outputColors[startIndex].green = ((byte)line_g.y());
                //outputColors[startIndex].blue = ((byte)line_b.y());
                //outputColors[startIndex].alpha = ((byte)line_a.y());

                outputColors[startIndex] = Color.FromArgb(
                    (byte)line_a.y(),
                    (byte)line_r.y(),
                    (byte)line_g.y(),
                    (byte)line_b.y()
                    );
                line_r.NextN();
                line_g.NextN();
                line_b.NextN();
                line_a.NextN();
                nlen -= SUBPIXEL_SCALE;
                ++startIndex;
                --len;
            }

            // Ending part; checking for overflow.
            // Typically it's 1-2 pixels, but may be more in some cases.
            //-------------------------
            while (len != 0)
            {
                //vr = line_r.y();
                //vg = line_g.y();
                //vb = line_b.y();
                //va = line_a.y();
                ////clamp between 0 and 255
                //if (vr < 0) { vr = 0; } else if (vr > 255) { vr = 255; }
                //if (vg < 0) { vg = 0; } else if (vg > 255) { vg = 255; }
                //if (vb < 0) { vb = 0; } else if (vb > 255) { vb = 255; }
                //if (va < 0) { va = 0; } else if (va > 255) { va = 255; }

                //outputColors[startIndex].red = ((byte)vr);
                //outputColors[startIndex].green = ((byte)vg);
                //outputColors[startIndex].blue = ((byte)vb);
                //outputColors[startIndex].alpha = ((byte)va);
                outputColors[startIndex] = Color.FromArgb(
                       line_a.y_clamp0_255(),
                       line_r.y_clamp0_255(),
                       line_g.y_clamp0_255(),
                       line_b.y_clamp0_255());

                line_r.NextN();
                line_g.NextN();
                line_b.NextN();
                line_a.NextN();
                ++startIndex;
                --len;
            }
        }
    }
}
