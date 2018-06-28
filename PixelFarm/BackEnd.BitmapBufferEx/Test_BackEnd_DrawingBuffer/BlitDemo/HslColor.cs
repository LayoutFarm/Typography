//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
using System;
using PixelFarm.BitmapBufferEx;

namespace WinFormGdiPlus
{
    public struct HslColor
    {

        // value from 0 to 1 
        public double A;
        // value from 0 to 360 
        public double H;
        // value from 0 to 1 
        public double S;
        // value from 0 to 1 
        public double L;



        private static double ByteToPct(byte v)
        {
            double d = v;
            d /= 255;
            return d;
        }

        private static byte PctToByte(double pct)
        {
            pct *= 255;
            pct += .5;
            if (pct > 255) pct = 255;
            if (pct < 0) pct = 0;
            return (byte)pct;
        }

        public static HslColor FromColor(ColorInt c)
        {
            return HslColor.FromArgb(c.A, c.R, c.G, c.B);
        }

        public static HslColor FromArgb(byte A, byte R, byte G, byte B)
        {
            HslColor c = FromRgb(R, G, B);
            c.A = ByteToPct(A);
            return c;
        }

        public static HslColor FromRgb(byte R, byte G, byte B)
        {
            HslColor c = new HslColor();
            c.A = 1;
            double r = ByteToPct(R);
            double g = ByteToPct(G);
            double b = ByteToPct(B);
            double max = Math.Max(b, Math.Max(r, g));
            double min = Math.Min(b, Math.Min(r, g));
            if (max == min)
            {
                c.H = 0;
            }
            else if (max == r && g >= b)
            {
                c.H = 60 * ((g - b) / (max - min));
            }
            else if (max == r && g < b)
            {
                c.H = 60 * ((g - b) / (max - min)) + 360;
            }
            else if (max == g)
            {
                c.H = 60 * ((b - r) / (max - min)) + 120;
            }
            else if (max == b)
            {
                c.H = 60 * ((r - g) / (max - min)) + 240;
            }

            c.L = .5 * (max + min);
            if (max == min)
            {
                c.S = 0;
            }
            else if (c.L <= .5)
            {
                c.S = (max - min) / (2 * c.L);
            }
            else if (c.L > .5)
            {
                c.S = (max - min) / (2 - 2 * c.L);
            }
            return c;
        }

        public HslColor Lighten(double pct)
        {
            HslColor c = new HslColor();
            c.A = this.A;
            c.H = this.H;
            c.S = this.S;
            c.L = Math.Min(Math.Max(this.L + pct, 0), 1);
            return c;
        }

        public HslColor Darken(double pct)
        {
            return Lighten(-pct);
        }

        private double norm(double d)
        {
            if (d < 0) d += 1;
            if (d > 1) d -= 1;
            return d;
        }

        private double getComponent(double tc, double p, double q)
        {
            if (tc < (1.0 / 6.0))
            {
                return p + ((q - p) * 6 * tc);
            }
            if (tc < .5)
            {
                return q;
            }
            if (tc < (2.0 / 3.0))
            {
                return p + ((q - p) * 6 * ((2.0 / 3.0) - tc));
            }
            return p;
        }

        public ColorInt ToColor()
        {
            double q = 0;
            if (L < .5)
            {
                q = L * (1 + S);
            }
            else
            {
                q = L + S - (L * S);
            }
            double p = (2 * L) - q;
            double hk = H / 360;
            double r = getComponent(norm(hk + (1.0 / 3.0)), p, q);
            double g = getComponent(norm(hk), p, q);
            double b = getComponent(norm(hk - (1.0 / 3.0)), p, q);
            return ColorInt.FromArgb(PctToByte(A), PctToByte(r), PctToByte(g), PctToByte(b));
        }

    }


}