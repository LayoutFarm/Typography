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
//
// Adaptation for high precision colors has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
#if DEBUG
using System;
namespace PixelFarm.CpuBlit
{
    // Supported byte orders for RGB and RGBA pixel formats
    //=======================================================================
    //struct order_rgb { enum rgb_e { R = 0, G = 1, B = 2, rgb_tag }; };       //----order_rgb
    //struct order_bgr { enum bgr_e { B = 0, G = 1, R = 2, rgb_tag }; };       //----order_bgr
    //struct order_rgba { enum rgba_e { R = 0, G = 1, B = 2, A = 3, rgba_tag }; }; //----order_rgba
    //struct order_argb { enum argb_e { A = 0, R = 1, G = 2, B = 3, rgba_tag }; }; //----order_argb
    //struct order_abgr { enum abgr_e { A = 0, B = 1, G = 2, R = 3, rgba_tag }; }; //----order_abgr
    //struct order_bgra { enum bgra_e { B = 0, G = 1, R = 2, A = 3, rgba_tag }; }; //----order_bgra

    public struct ColorRGBAf
    {
        const int BASE_SHIFT = 8;
        const int BASE_SCALE = (int)(1 << BASE_SHIFT);
        const int BASE_MASK = BASE_SCALE - 1;
        public float red;
        public float green;
        public float blue;
        public float alpha;
        public int Red0To255
        {
            get { return AggMath.uround_f(red * (float)BASE_MASK); }
        }
        public int Green0To255
        {
            get { return AggMath.uround_f(green * (float)BASE_MASK); }
        }
        public int Blue0To255
        {
            get { return AggMath.uround_f(blue * (float)BASE_MASK); }
        }
        public int Alpha0To255
        {
            get { return AggMath.uround_f(alpha * (float)BASE_MASK); }
        }

        public float Red0To1 { get { return red; } }
        public float Green0To1 { get { return green; } }
        public float Blue0To1 { get { return blue; } }
        public float Alpha0To1 { get { return alpha; } }

        public static readonly ColorRGBAf White = new ColorRGBAf(1, 1, 1, 1);
        public static readonly ColorRGBAf Black = new ColorRGBAf(0, 0, 0, 1);
        public static readonly ColorRGBAf Red = new ColorRGBAf(1, 0, 0, 1);
        public static readonly ColorRGBAf Green = new ColorRGBAf(0, 1, 0, 1);
        public static readonly ColorRGBAf Blue = new ColorRGBAf(0, 0, 1, 1);
        public static readonly ColorRGBAf Cyan = new ColorRGBAf(0, 1, 1, 1);
        public static readonly ColorRGBAf Magenta = new ColorRGBAf(1, 0, 1, 1);
        public static readonly ColorRGBAf Yellow = new ColorRGBAf(1, 1, 0, 1);

        public ColorRGBAf(double r_, double g_, double b_)
        {
            red = (float)r_;
            green = (float)g_;
            blue = (float)b_;
            alpha = 1;
        }

        private ColorRGBAf(double r_, double g_, double b_, double a_)
        {
            red = (float)r_;
            green = (float)g_;
            blue = (float)b_;
            alpha = (float)a_;
        }
        public ColorRGBAf(float r_, float g_, float b_)
        {
            red = r_;
            green = g_;
            blue = b_;
            alpha = 1;
        }
        public ColorRGBAf(float r_, float g_, float b_, float a_)
        {
            red = r_;
            green = g_;
            blue = b_;
            alpha = a_;
        }
        public ColorRGBAf(ColorRGBAf c)
            : this(c, c.alpha)
        {
        }
        public ColorRGBAf(ColorRGBAf c, float a_)
        {
            red = c.red;
            green = c.green;
            blue = c.blue;
            alpha = a_;
        }
        public ColorRGBAf(float wavelen)
            : this(wavelen, 1.0f)
        {
        }
        public ColorRGBAf(float wavelen, float gamma)
        {
            this = FromWaveLength(wavelen, gamma);
        }


        // Given H,S,L,A in range of 0-1
        // Returns a Color (RGB struct) in range of 0-255
        public static ColorRGBAf FromHSL(double hue0To1, double saturation0To1, double lightness0To1, double alpha = 1)
        {
            double v;
            double r, g, b;
            if (alpha > 1.0)
            {
                alpha = 1.0;
            }

            r = lightness0To1;   // default to gray
            g = lightness0To1;
            b = lightness0To1;
            v = lightness0To1 + saturation0To1 - lightness0To1 * saturation0To1;
            if (lightness0To1 <= 0.5)
            {
                v = lightness0To1 * (1.0 + saturation0To1);
            }

            if (v > 0)
            {
                double m;
                double sv;
                int sextant;
                double fract, vsf, mid1, mid2;
                m = lightness0To1 + lightness0To1 - v;
                sv = (v - m) / v;
                hue0To1 *= 6.0;
                sextant = (int)hue0To1;
                fract = hue0To1 - sextant;
                vsf = v * sv * fract;
                mid1 = m + vsf;
                mid2 = v - vsf;
                switch (sextant)
                {
                    case 0:
                        r = v;
                        g = mid1;
                        b = m;
                        break;
                    case 1:
                        r = mid2;
                        g = v;
                        b = m;
                        break;
                    case 2:
                        r = m;
                        g = v;
                        b = mid1;
                        break;
                    case 3:
                        r = m;
                        g = mid2;
                        b = v;
                        break;
                    case 4:
                        r = mid1;
                        g = m;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = m;
                        b = mid2;
                        break;
                    case 6:
                        goto case 0;
                }
            }

            return new ColorRGBAf(r, g, b, alpha);
        }

        public void GetHSL(out double hue0To1, out double saturation0To1, out double lightness0To1)
        {
            double maxRGB = Math.Max(red, Math.Max(green, blue));
            double minRGB = Math.Min(red, Math.Min(green, blue));
            double deltaMaxToMin = maxRGB - minRGB;
            double r2, g2, b2;
            hue0To1 = 0; // default to black
            saturation0To1 = 0;
            lightness0To1 = 0;
            lightness0To1 = (minRGB + maxRGB) / 2.0;
            if (lightness0To1 <= 0.0)
            {
                return;
            }
            saturation0To1 = deltaMaxToMin;
            if (saturation0To1 > 0.0)
            {
                saturation0To1 /= (lightness0To1 <= 0.5) ? (maxRGB + minRGB) : (2.0 - maxRGB - minRGB);
            }
            else
            {
                return;
            }
            r2 = (maxRGB - red) / deltaMaxToMin;
            g2 = (maxRGB - green) / deltaMaxToMin;
            b2 = (maxRGB - blue) / deltaMaxToMin;
            if (red == maxRGB)
            {
                if (green == minRGB)
                {
                    hue0To1 = 5.0 + b2;
                }
                else
                {
                    hue0To1 = 1.0 - g2;
                }
            }
            else if (green == maxRGB)
            {
                if (blue == minRGB)
                {
                    hue0To1 = 1.0 + r2;
                }
                else
                {
                    hue0To1 = 3.0 - b2;
                }
            }
            else
            {
                if (red == minRGB)
                {
                    hue0To1 = 3.0 + g2;
                }
                else
                {
                    hue0To1 = 5.0 - r2;
                }
            }
            hue0To1 /= 6.0;
        }

        public static ColorRGBAf AdjustSaturation(ColorRGBAf original, double saturationMultiplier)
        {
            double hue0To1;
            double saturation0To1;
            double lightness0To1;
            original.GetHSL(out hue0To1, out saturation0To1, out lightness0To1);
            saturation0To1 *= saturationMultiplier;
            return FromHSL(hue0To1, saturation0To1, lightness0To1);
        }

        public static ColorRGBAf AdjustLightness(ColorRGBAf original, double lightnessMultiplier)
        {
            double hue0To1;
            double saturation0To1;
            double lightness0To1;
            original.GetHSL(out hue0To1, out saturation0To1, out lightness0To1);
            lightness0To1 *= lightnessMultiplier;
            return FromHSL(hue0To1, saturation0To1, lightness0To1);
        }


        public static bool operator ==(ColorRGBAf a, ColorRGBAf b)
        {
            if (a.red == b.red && a.green == b.green && a.blue == b.blue && a.alpha == b.alpha)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(ColorRGBAf a, ColorRGBAf b)
        {
            if (a.red != b.red || a.green != b.green || a.blue != b.blue || a.alpha != b.alpha)
            {
                return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(ColorRGBAf))
            {
                return this == (ColorRGBAf)obj;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return new { blue, green, red, alpha }.GetHashCode();
        }



        static public ColorRGBAf operator +(ColorRGBAf A, ColorRGBAf B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red + B.red;
            temp.green = A.green + B.green;
            temp.blue = A.blue + B.blue;
            temp.alpha = A.alpha + B.alpha;
            return temp;
        }

        static public ColorRGBAf operator -(ColorRGBAf A, ColorRGBAf B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red - B.red;
            temp.green = A.green - B.green;
            temp.blue = A.blue - B.blue;
            temp.alpha = A.alpha - B.alpha;
            return temp;
        }

        static public ColorRGBAf operator *(ColorRGBAf A, ColorRGBAf B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red * B.red;
            temp.green = A.green * B.green;
            temp.blue = A.blue * B.blue;
            temp.alpha = A.alpha * B.alpha;
            return temp;
        }

        static public ColorRGBAf operator /(ColorRGBAf A, ColorRGBAf B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red / B.red;
            temp.green = A.green / B.green;
            temp.blue = A.blue / B.blue;
            temp.alpha = A.alpha / B.alpha;
            return temp;
        }

        static public ColorRGBAf operator /(ColorRGBAf A, float B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red / B;
            temp.green = A.green / B;
            temp.blue = A.blue / B;
            temp.alpha = A.alpha / B;
            return temp;
        }

        static public ColorRGBAf operator /(ColorRGBAf A, double doubleB)
        {
            float B = (float)doubleB;
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red / B;
            temp.green = A.green / B;
            temp.blue = A.blue / B;
            temp.alpha = A.alpha / B;
            return temp;
        }

        static public ColorRGBAf operator *(ColorRGBAf A, float B)
        {
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red * B;
            temp.green = A.green * B;
            temp.blue = A.blue * B;
            temp.alpha = A.alpha * B;
            return temp;
        }

        static public ColorRGBAf operator *(ColorRGBAf A, double doubleB)
        {
            float B = (float)doubleB;
            ColorRGBAf temp = new ColorRGBAf();
            temp.red = A.red * B;
            temp.green = A.green * B;
            temp.blue = A.blue * B;
            temp.alpha = A.alpha * B;
            return temp;
        }

        public void clear()
        {
            red = green = blue = alpha = 0;
        }

        public ColorRGBAf transparent()
        {
            alpha = 0.0f;
            return this;
        }

        public ColorRGBAf opacity(float a_)
        {
            if (a_ < 0.0) a_ = 0.0f;
            if (a_ > 1.0) a_ = 1.0f;
            alpha = a_;
            return this;
        }

        public float opacity()
        {
            return alpha;
        }

        public ColorRGBAf premultiply()
        {
            red *= alpha;
            green *= alpha;
            blue *= alpha;
            return this;
        }

        public ColorRGBAf premultiply(float a_)
        {
            if (alpha <= 0.0 || a_ <= 0.0)
            {
                red = green = blue = alpha = 0.0f;
                return this;
            }
            a_ /= alpha;
            red *= a_;
            green *= a_;
            blue *= a_;
            alpha = a_;
            return this;
        }

        public ColorRGBAf demultiply()
        {
            if (alpha == 0)
            {
                red = green = blue = 0;
                return this;
            }
            float a_ = 1.0f / alpha;
            red *= a_;
            green *= a_;
            blue *= a_;
            return this;
        }

        public Drawing.Color gradient(Drawing.Color c_8, double k)
        {
            float c_red = c_8.R / 255f;
            float c_green = c_8.G / 255f;
            float c_blue = c_8.B / 255f;
            float c_alpha = c_8.A / 255f;
            float n_red = (float)(red + (c_red - red) * k);
            float n_green = (float)(green + (c_green - green) * k);
            float n_blue = (float)(blue + (c_blue - blue) * k);
            float n_alpha = (float)(alpha + (c_alpha - alpha) * k);
            return Drawing.Color.FromArgb(c_alpha, c_red, c_green, c_blue);
        }


        //public static Color GetTransparentColor() { return transparentColor; }

        public static ColorRGBAf FromWaveLength(float wl)
        {
            return FromWaveLength(wl, 1.0f);
        }

        public static ColorRGBAf FromWaveLength(float wl, float gamma)
        {
            ColorRGBAf t = new ColorRGBAf(0.0f, 0.0f, 0.0f);
            if (wl >= 380.0 && wl <= 440.0)
            {
                t.red = (float)(-1.0 * (wl - 440.0) / (440.0 - 380.0));
                t.blue = 1.0f;
            }
            else if (wl >= 440.0 && wl <= 490.0)
            {
                t.green = (float)((wl - 440.0) / (490.0 - 440.0));
                t.blue = 1.0f;
            }
            else if (wl >= 490.0 && wl <= 510.0)
            {
                t.green = 1.0f;
                t.blue = (float)(-1.0 * (wl - 510.0) / (510.0 - 490.0));
            }
            else if (wl >= 510.0 && wl <= 580.0)
            {
                t.red = (float)((wl - 510.0) / (580.0 - 510.0));
                t.green = 1.0f;
            }
            else if (wl >= 580.0 && wl <= 645.0)
            {
                t.red = 1.0f;
                t.green = (float)(-1.0 * (wl - 645.0) / (645.0 - 580.0));
            }
            else if (wl >= 645.0 && wl <= 780.0)
            {
                t.red = 1.0f;
            }

            float s = 1.0f;
            if (wl > 700.0) s = (float)(0.3 + 0.7 * (780.0 - wl) / (780.0 - 700.0));
            else if (wl < 420.0) s = (float)(0.3 + 0.7 * (wl - 380.0) / (420.0 - 380.0));
            t.red = (float)Math.Pow(t.red * s, gamma);
            t.green = (float)Math.Pow(t.green * s, gamma);
            t.blue = (float)Math.Pow(t.blue * s, gamma);
            return t;
        }

        public static ColorRGBAf rgba_pre(double r, double g, double b)
        {
            return rgba_pre((float)r, (float)g, (float)b, 1.0f);
        }

        public static ColorRGBAf rgba_pre(float r, float g, float b)
        {
            return rgba_pre(r, g, b, 1.0f);
        }

        public static ColorRGBAf rgba_pre(float r, float g, float b, float a)
        {
            return new ColorRGBAf(r, g, b, a).premultiply();
        }

        public static ColorRGBAf rgba_pre(double r, double g, double b, double a)
        {
            return new ColorRGBAf((float)r, (float)g, (float)b, (float)a).premultiply();
        }

        public static ColorRGBAf rgba_pre(ColorRGBAf c)
        {
            return new ColorRGBAf(c).premultiply();
        }

        public static ColorRGBAf rgba_pre(ColorRGBAf c, float a)
        {
            return new ColorRGBAf(c, a).premultiply();
        }

        public static ColorRGBAf GetTweenColor(ColorRGBAf c1, ColorRGBAf c2, float ratioOf2)
        {
            if (ratioOf2 <= 0)
            {
                return new ColorRGBAf(c1);
            }

            if (ratioOf2 >= 1.0)
            {
                return new ColorRGBAf(c2);
            }

            // figure out how much of each color we should be.
            double ratioOf1 = 1.0 - ratioOf2;
            return new ColorRGBAf(
                c1.red * ratioOf1 + c2.red * ratioOf2,
                c1.green * ratioOf1 + c2.green * ratioOf2,
                c1.blue * ratioOf1 + c2.blue * ratioOf2);
        }

        public ColorRGBAf Blend(ColorRGBAf other, double weight)
        {
            ColorRGBAf result = new ColorRGBAf(this);
            result = this * (1 - weight) + other * weight;
            return result;
        }

        public double SumOfDistances(ColorRGBAf other)
        {
            double dist = Math.Abs(red - other.red) + Math.Abs(green - other.green) + Math.Abs(blue - other.blue);
            return dist;
        }


        static void Clamp0To1(ref float value)
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > 1)
            {
                value = 1;
            }
        }

        public void Clamp0To1()
        {
            Clamp0To1(ref red);
            Clamp0To1(ref green);
            Clamp0To1(ref blue);
            Clamp0To1(ref alpha);
        }
    }
}
#endif