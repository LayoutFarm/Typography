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
    public sealed class RGBAGouraudSpanGen : GouraudSpanGen, ISpanGenerator
    {
        bool m_swap;
        int m_y2;
        RGBA_Calculator m_rgba1;
        RGBA_Calculator m_rgba2;
        RGBA_Calculator m_rgba3;
        const int SUBPIXEL_SHIFT = 4;
        const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;
        //--------------------------------------------------------------------
        struct RGBA_Calculator
        {
            public void Init(GouraudSpanGen.CoordAndColor c1, GouraudSpanGen.CoordAndColor c2)
            {
                m_x1 = c1.x - 0.5;
                m_y1 = c1.y - 0.5;
                m_dx = c2.x - c1.x;
                double dy = c2.y - c1.y;
                m_1dy = (dy < 1e-5) ? 1e5 : 1.0 / dy;
                m_r1 = (int)c1.color.red;
                m_g1 = (int)c1.color.green;
                m_b1 = (int)c1.color.blue;
                m_a1 = (int)c1.color.alpha;
                m_dr = (int)c2.color.red - m_r1;
                m_dg = (int)c2.color.green - m_g1;
                m_db = (int)c2.color.blue - m_b1;
                m_da = (int)c2.color.alpha - m_a1;
            }

            public void Calculate(double y)
            {
                double k = (y - m_y1) * m_1dy;
                if (k < 0.0) k = 0.0;
                if (k > 1.0) k = 1.0;
                m_r = m_r1 + AggMath.iround(m_dr * k);
                m_g = m_g1 + AggMath.iround(m_dg * k);
                m_b = m_b1 + AggMath.iround(m_db * k);
                m_a = m_a1 + AggMath.iround(m_da * k);
                m_x = AggMath.iround((m_x1 + m_dx * k) * (double)SUBPIXEL_SCALE);
            }

            public double m_x1;
            public double m_y1;
            public double m_dx;
            public double m_1dy;
            public int m_r1;
            public int m_g1;
            public int m_b1;
            public int m_a1;
            public int m_dr;
            public int m_dg;
            public int m_db;
            public int m_da;
            public int m_r;
            public int m_g;
            public int m_b;
            public int m_a;
            public int m_x;
        }

        //--------------------------------------------------------------------
        public RGBAGouraudSpanGen() { }
        public RGBAGouraudSpanGen(Color c1,
                          Color c2,
                          Color c3,
                          double x1, double y1,
                          double x2, double y2,
                          double x3, double y3)
            : this(c1, c2, c3, x1, y1, x2, y2, x3, y3, 0)
        { }

        public RGBAGouraudSpanGen(Color c1,
                          Color c2,
                          Color c3,
                          double x1, double y1,
                          double x2, double y2,
                          double x3, double y3,
                          double d)
            : base(c1, c2, c3, x1, y1, x2, y2, x3, y3, d)
        { }

        //--------------------------------------------------------------------
        public void Prepare()
        {
            CoordAndColor c0, c1, c2;
            base.LoadArrangedVertices(out c0, out c1, out c2);
            m_y2 = (int)c1.y;
            m_swap = AggMath.Cross(c0.x, c0.y,
                                   c2.x, c2.y,
                                   c1.x, c1.y) < 0.0;
            m_rgba1.Init(c0, c2);
            m_rgba2.Init(c0, c1);
            m_rgba3.Init(c1, c2);
        }

        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int len)
        {
            m_rgba1.Calculate(y);//(m_rgba1.m_1dy > 2) ? m_rgba1.m_y1 : y);
            RGBA_Calculator pc1 = m_rgba1;
            RGBA_Calculator pc2 = m_rgba2;
            if (y <= m_y2)
            {
                // Bottom part of the triangle (first subtriangle)
                //-------------------------
                m_rgba2.Calculate(y + m_rgba2.m_1dy);
            }
            else
            {
                // Upper part (second subtriangle)
                m_rgba3.Calculate(y - m_rgba3.m_1dy);
                //-------------------------
                pc2 = m_rgba3;
            }

            if (m_swap)
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
            int nlen = Math.Abs(pc2.m_x - pc1.m_x);
            if (nlen <= 0) nlen = 1;
            var line_r = new LineInterpolatorDDA(pc1.m_r, pc2.m_r, nlen, 14);
            var line_g = new LineInterpolatorDDA(pc1.m_g, pc2.m_g, nlen, 14);
            var line_b = new LineInterpolatorDDA(pc1.m_b, pc2.m_b, nlen, 14);
            var line_a = new LineInterpolatorDDA(pc1.m_a, pc2.m_a, nlen, 14);
            // Calculate the starting point of the gradient with subpixel 
            // accuracy and correct (roll back) the interpolators.
            // This operation will also clip the beginning of the span
            // if necessary.
            //-------------------------
            int start = pc1.m_x - (x << (int)SUBPIXEL_SHIFT);
            line_r.Prev(start);
            line_g.Prev(start);
            line_b.Prev(start);
            line_a.Prev(start);
            nlen += start;
            int vr, vg, vb, va;
            uint lim = 255;
            // Beginning part of the span. Since we rolled back the 
            // interpolators, the color values may have overflowed.
            // So that, we render the beginning part with checking 
            // for overflow. It lasts until "start" is positive;
            // typically it's 1-2 pixels, but may be more in some cases.
            //-------------------------
            while (len != 0 && start > 0)
            {
                vr = line_r.y();
                vg = line_g.y();
                vb = line_b.y();
                va = line_a.y();
                if (vr < 0) { vr = 0; } else if (vr > lim) { vr = (int)lim; }
                if (vg < 0) { vg = 0; } else if (vg > lim) { vg = (int)lim; }
                if (vb < 0) { vb = 0; } else if (vb > lim) { vb = (int)lim; }
                if (va < 0) { va = 0; } else if (va > lim) { va = (int)lim; }

                //outputColors[startIndex].red = (byte)vr;
                //outputColors[startIndex].green = (byte)vg;
                //outputColors[startIndex].blue = (byte)vb;
                //outputColors[startIndex].alpha = (byte)va;

                outputColors[startIndex] = Color.FromArgb((byte)va, (byte)vr, (byte)vg, (byte)vb);

                line_r.Next(SUBPIXEL_SCALE);
                line_g.Next(SUBPIXEL_SCALE);
                line_b.Next(SUBPIXEL_SCALE);
                line_a.Next(SUBPIXEL_SCALE);
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
                line_r.Next(SUBPIXEL_SCALE);
                line_g.Next(SUBPIXEL_SCALE);
                line_b.Next(SUBPIXEL_SCALE);
                line_a.Next(SUBPIXEL_SCALE);
                nlen -= SUBPIXEL_SCALE;
                ++startIndex;
                --len;
            }

            // Ending part; checking for overflow.
            // Typically it's 1-2 pixels, but may be more in some cases.
            //-------------------------
            while (len != 0)
            {
                vr = line_r.y();
                vg = line_g.y();
                vb = line_b.y();
                va = line_a.y();
                if (vr < 0) { vr = 0; } else if (vr > lim) { vr = (int)lim; }
                if (vg < 0) { vg = 0; } else if (vg > lim) { vg = (int)lim; }
                if (vb < 0) { vb = 0; } else if (vb > lim) { vb = (int)lim; }
                if (va < 0) { va = 0; } else if (va > lim) { va = (int)lim; }

                //outputColors[startIndex].red = ((byte)vr);
                //outputColors[startIndex].green = ((byte)vg);
                //outputColors[startIndex].blue = ((byte)vb);
                //outputColors[startIndex].alpha = ((byte)va);
                outputColors[startIndex] = Color.FromArgb(((byte)va), ((byte)vr), ((byte)vg), ((byte)vb));
                line_r.Next(SUBPIXEL_SCALE);
                line_g.Next(SUBPIXEL_SCALE);
                line_b.Next(SUBPIXEL_SCALE);
                line_a.Next(SUBPIXEL_SCALE);
                ++startIndex;
                --len;
            }
        }
    }
}
