//BSD, 2014-2016, WinterDev


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



//
// System.Drawing.KnownColors
//
// Authors:
// Gonzalo Paniagua Javier (gonzalo@ximian.com)
// Peter Dennis Bartok (pbartok@novell.com)
// Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
namespace PixelFarm.Drawing
{
    public struct Color
    {
        byte r, g, b, a;
        public Color(byte a, byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public Color(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = 255;
        }
        public byte R
        {
            get { return this.r; }
        }
        public byte G
        {
            get { return this.g; }
        }
        public byte B
        {
            get { return this.b; }
        }
        public byte A
        {
            get { return this.a; }
        }
        public byte alpha
        {
            get { return this.a; }
            set { this.a = value; }
        }

        public byte red { get { return this.r; } set { this.r = value; } }
        public byte green { get { return this.g; } set { this.g = value; } }
        public byte blue { get { return this.b; } set { this.b = value; } }


        public static Color FromArgb(int a, Color c)
        {
            return new Color((byte)a, c.R, c.G, c.B);
        }
        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(int r, int g, int b)
        {
            return new Color(255, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(float a, float r, float g, float b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                Color c = (Color)obj;
                return c.a == this.a &&
                    c.b == this.b &&
                    c.r == this.r &&
                    c.g == this.g;
            }
            return false;
        }

        public static readonly Color Empty = new Color(0, 0, 0, 0);
        public static readonly Color Transparent = new Color(0, 255, 255, 255);
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(255, 0, 0, 0);
        public static readonly Color Blue = new Color(255, 0, 0, 255);
        public static readonly Color Red = new Color(255, 255, 0, 0);
        public static readonly Color Yellow = new Color(255, 255, 255, 0);
        public static readonly Color LightGray = new Color(0xFF, 0xD3, 0xD3, 0xD3);
        public static readonly Color Gray = new Color(0xFF, 0x80, 0x80, 0x80);
        public static readonly Color Green = new Color(0xFF, 0x00, 0x80, 0x00);
        public static readonly Color OrangeRed = new Color(0xFF, 0xFF, 0x45, 0x00);//0xFF FF 45 00
        public static readonly Color DeepPink = new Color(0xFF, 0xFF, 0x14, 0x93);
        public static readonly Color Magenta = new Color(0xFF, 0xFF, 0, 0xFF);
        public static Color FromName(string name)
        {
            var color = KnownColors.FromKnownColor(name);
            return new Color(color.A, color.R, color.G, color.B);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(Color c1, Color c2)
        {
            return (uint)((c1.a << 24) | (c1.r << 16) | (c1.g << 8) | (c1.b)) ==
                   (uint)((c2.a << 24) | (c2.r << 16) | (c2.g << 8) | (c2.b));
        }
        public static bool operator !=(Color c1, Color c2)
        {
            return (uint)((c1.a << 24) | (c1.r << 16) | (c1.g << 8) | (c1.b)) !=
                  (uint)((c2.a << 24) | (c2.r << 16) | (c2.g << 8) | (c2.b));
        }
        public uint ToARGB()
        {
            return (uint)((this.a << 24) | (this.r << 16) | (this.g << 8) | this.b);
        }
        public uint ToABGR()
        {
            return (uint)((this.a << 24) | (this.b << 16) | (this.g << 8) | this.r);
        }




        public const int COVER_SHIFT = 8;
        public const int COVER_SIZE = 1 << COVER_SHIFT;  //----cover_size 
        public const int COVER_MASK = COVER_SIZE - 1;    //----cover_mask   
        public const int BASE_SHIFT = 8;
        public const int BASE_SCALE = (1 << BASE_SHIFT);
        public const int BASE_MASK = (BASE_SCALE - 1);
        public byte Red0To255
        {
            get { return r; }
        }
        public byte Green0To255
        {
            get { return g; }
        }
        public byte Blue0To255
        {
            get { return b; }
        }
        public byte Alpha0To255
        {
            get { return a; }
        }

        //------------------------------------------
        public static Color Make(double r_, double g_, double b_, double a_)
        {
            return new Color(
               ((byte)Agg.AggBasics.uround(a_ * (double)BASE_MASK)),
               ((byte)Agg.AggBasics.uround(r_ * (double)BASE_MASK)),
               ((byte)Agg.AggBasics.uround(g_ * (double)BASE_MASK)),
               ((byte)Agg.AggBasics.uround(b_ * (double)BASE_MASK))
               );
        }
        public static Color Make(double r_, double g_, double b_)
        {
            return new Color(
               ((byte)Agg.AggBasics.uround(BASE_MASK)),
               ((byte)Agg.AggBasics.uround(r_ * (double)BASE_MASK)),
               ((byte)Agg.AggBasics.uround(g_ * (double)BASE_MASK)),
               ((byte)Agg.AggBasics.uround(b_ * (double)BASE_MASK)));
        }
        //------------------------------------------
        public static Color Make(float r_, float g_, float b_)
        {
            return new Color(
               ((byte)Agg.AggBasics.uround_f(BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(r_ * (float)BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(g_ * (float)BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(b_ * (float)BASE_MASK))
              );
        }
        public static Color Make(float r_, float g_, float b_, float a_)
        {
            return new Color(
               ((byte)Agg.AggBasics.uround_f(a_ * (float)BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(r_ * (float)BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(g_ * (float)BASE_MASK)),
               ((byte)Agg.AggBasics.uround_f(b_ * (float)BASE_MASK))
               );
        }
        //------------------------------------------
        public static Color Make(int r_, int g_, int b_, int a_)
        {
            return new Color(
               (byte)Math.Min(Math.Max(a_, 0), 255),
               (byte)Math.Min(Math.Max(r_, 0), 255),
               (byte)Math.Min(Math.Max(g_, 0), 255),
               (byte)Math.Min(Math.Max(b_, 0), 255)
               );
        }

        public Color CreateGradient(Color another, float colorDistanceRatio)
        {
            //int ik = AggBasics.uround(colorDistanceRatio * BASE_SCALE); 
            //byte r = (byte)((int)(Red0To255) + ((((int)(another.Red0To255) - Red0To255) * ik) >> BASE_SHIFT));
            //byte g = (byte)((int)(Green0To255) + ((((int)(another.Green0To255) - Green0To255) * ik) >> BASE_SHIFT));
            //byte b = (byte)((int)(Blue0To255) + ((((int)(another.Blue0To255) - Blue0To255) * ik) >> BASE_SHIFT));
            //byte a = (byte)((int)(Alpha0To255) + ((((int)(another.Alpha0To255) - Alpha0To255) * ik) >> BASE_SHIFT));

            //from this color to another c color
            //colorDistance ratio [0-1]
            //new_color = old_color + diff

            byte r = (byte)(Red0To255 + (another.Red0To255 - this.Red0To255) * colorDistanceRatio);
            byte g = (byte)(Green0To255 + (another.Green0To255 - this.Green0To255) * colorDistanceRatio);
            byte b = (byte)(Blue0To255 + (another.Blue0To255 - this.Blue0To255) * colorDistanceRatio);
            byte a = (byte)(Alpha0To255 + (another.Alpha0To255 - this.Alpha0To255) * colorDistanceRatio);
            return new Color(a, r, g, b);
        }

        static public Color operator +(Color A, Color B)
        {
            byte r = (byte)((A.r + B.r) > 255 ? 255 : (A.r + B.r));
            byte g = (byte)((A.g + B.g) > 255 ? 255 : (A.g + B.g));
            byte b = (byte)((A.b + B.b) > 255 ? 255 : (A.b + B.b));
            byte a = (byte)((A.a + B.a) > 255 ? 255 : (A.a + B.a));
            return new Color(a, r, g, b);
        }

        static public Color operator -(Color A, Color B)
        {
            byte red = (byte)((A.r - B.r) < 0 ? 0 : (A.r - B.r));
            byte green = (byte)((A.g - B.g) < 0 ? 0 : (A.g - B.g));
            byte blue = (byte)((A.b - B.b) < 0 ? 0 : (A.b - B.b));
            byte alpha = (byte)((A.a - B.a) < 0 ? 0 : (A.a - B.a));
            return new Color(alpha, red, green, blue);
        }

        static public Color operator *(Color A, float b)
        {
            float conv = b / 255f;
            return Make(A.r * conv, A.g * conv, A.b * conv, A.a * conv);
        }

        //public void AddColor(ColorRGBA c, int cover)
        //{
        //    int cr, cg, cb, ca;
        //    if (cover == COVER_MASK)
        //    {
        //        if (c.Alpha0To255 == BASE_MASK)
        //        {
        //            this = c;
        //        }
        //        else
        //        {
        //            cr = Red0To255 + c.Red0To255; Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //            cg = Green0To255 + c.Green0To255; Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //            cb = Blue0To255 + c.Blue0To255; Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //            ca = Alpha0To255 + c.Alpha0To255; Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //        }
        //    }
        //    else
        //    {
        //        cr = Red0To255 + ((c.Red0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cg = Green0To255 + ((c.Green0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cb = Blue0To255 + ((c.Blue0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        ca = Alpha0To255 + ((c.Alpha0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //        Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //        Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //        Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //    }
        //}

        //public void ApplyGammaDir(GammaLookUpTable gamma)
        //{
        //    Red0To255 = gamma.dir((byte)Red0To255);
        //    Green0To255 = gamma.dir((byte)Green0To255);
        //    Blue0To255 = gamma.dir((byte)Blue0To255);
        //}

        //-------------------------------------------------------------rgb8_packed
        static public Color CreatRGB8Packed(int v)
        {
            //argb
            return new Color(255, (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), ((byte)(v & 0xFF)));
        }

        public Color Blend(Color other, float weight)
        {
            return this * (1 - weight) + other * weight;
        }
#if DEBUG
        public override string ToString()
        {
            return "r:" + this.r + ",g:" + this.g + ",b:" + this.b + ",a:" + this.a;
        }
#endif
    }
}