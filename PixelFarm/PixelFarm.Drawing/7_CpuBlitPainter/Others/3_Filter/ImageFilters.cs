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
// Image transformation filters,
// Filtering classes (ImageFilterLookUpTable, image_filter),
// Basic filter shape classes
//----------------------------------------------------------------------------

using System;
namespace PixelFarm.CpuBlit.Imaging
{
    //-----------------------------------------------image_filter_bilinear
    public struct ImageFilterBilinear : IImageFilter
    {
        public double GetRadius() { return 1.0; }
        public double CalculateWeight(double x)
        {
            if (Math.Abs(x) < 1)
            {
                if (x < 0)
                {
                    return 1.0 + x;
                }
                else
                {
                    return 1.0 - x;
                }
            }

            return 0;
        }
    }

    //-----------------------------------------------image_filter_hanning
    public struct ImageFilterHanning : IImageFilter
    {
        public double GetRadius() { return 1.0; }
        public double CalculateWeight(double x)
        {
            return 0.5 + 0.5 * Math.Cos(Math.PI * x);
        }
    }

    //-----------------------------------------------image_filter_hamming
    public struct ImageFilterHamming : IImageFilter
    {
        public double GetRadius() { return 1.0; }
        public double CalculateWeight(double x)
        {
            return 0.54 + 0.46 * Math.Cos(Math.PI * x);
        }
    }
    //-----------------------------------------------image_filter_hermite
    public struct ImageFilterHermite : IImageFilter
    {
        public double GetRadius() { return 1.0; }
        public double CalculateWeight(double x)
        {
            return (2.0 * x - 3.0) * x * x + 1.0;
        }
    }
    //------------------------------------------------image_filter_quadric
    public struct ImageFilterQuadric : IImageFilter
    {
        public double GetRadius() { return 1.5; }
        public double CalculateWeight(double x)
        {
            double t;
            if (x < 0.5) return 0.75 - x * x;
            if (x < 1.5) { t = x - 1.5; return 0.5 * t * t; }
            return 0.0;
        }
    }
    //------------------------------------------------image_filter_bicubic
    public class ImageFilterBicubic : IImageFilter
    {
        private static double pow3(double x)
        {
            return (x <= 0.0) ? 0.0 : x * x * x;
        }

        public double GetRadius() { return 2.0; }
        public double CalculateWeight(double x)
        {
            return
                (1.0 / 6.0) *
                (pow3(x + 2) - 4 * pow3(x + 1) + 6 * pow3(x) - 4 * pow3(x - 1));
        }
    }
    //-------------------------------------------------image_filter_kaiser
    public class ImageFilterKaiser : IImageFilter
    {
        double a;
        double i0a;
        double epsilon;
        public ImageFilterKaiser()
            : this(6.33)
        {
        }
        public ImageFilterKaiser(double b)
        {
            a = (b);
            epsilon = (1e-12);
            i0a = 1.0 / Bessel_i0(b);
        }

        public double GetRadius() { return 1.0; }
        public double CalculateWeight(double x)
        {
            return Bessel_i0(a * Math.Sqrt(1.0 - x * x)) * i0a;
        }

        double Bessel_i0(double x)
        {
            int i;
            double sum, y, t;
            sum = 1.0;
            y = x * x / 4.0;
            t = y;
            for (i = 2; t > epsilon; i++)
            {
                sum += t;
                t *= (double)y / (i * i);
            }
            return sum;
        }
    }
    //----------------------------------------------image_filter_catrom
    public struct ImageFilterCatrom : IImageFilter
    {
        public double GetRadius() { return 2.0; }
        public double CalculateWeight(double x)
        {
            if (x < 1.0) return 0.5 * (2.0 + x * x * (-5.0 + x * 3.0));
            if (x < 2.0) return 0.5 * (4.0 + x * (-8.0 + x * (5.0 - x)));
            return 0.0;
        }
    }
    //---------------------------------------------image_filter_mitchell
    public class ImageFilterMichell : IImageFilter
    {
        private double p0, p2, p3;
        private double q0, q1, q2, q3;
        public ImageFilterMichell()
            : this(1.0 / 3.0, 1.0 / 3.0)
        {
        }

        public ImageFilterMichell(double b, double c)
        {
            p0 = ((6.0 - 2.0 * b) / 6.0);
            p2 = ((-18.0 + 12.0 * b + 6.0 * c) / 6.0);
            p3 = ((12.0 - 9.0 * b - 6.0 * c) / 6.0);
            q0 = ((8.0 * b + 24.0 * c) / 6.0);
            q1 = ((-12.0 * b - 48.0 * c) / 6.0);
            q2 = ((6.0 * b + 30.0 * c) / 6.0);
            q3 = ((-b - 6.0 * c) / 6.0);
        }

        public double GetRadius() { return 2.0; }
        public double CalculateWeight(double x)
        {
            if (x < 1.0) return p0 + x * x * (p2 + x * p3);
            if (x < 2.0) return q0 + x * (q1 + x * (q2 + x * q3));
            return 0.0;
        }
    }
    //----------------------------------------------image_filter_spline16
    public struct ImageFilterSpline16 : IImageFilter
    {
        public double GetRadius() { return 2.0; }
        public double CalculateWeight(double x)
        {
            if (x < 1.0)
            {
                return ((x - 9.0 / 5.0) * x - 1.0 / 5.0) * x + 1.0;
            }
            return ((-1.0 / 3.0 * (x - 1) + 4.0 / 5.0) * (x - 1) - 7.0 / 15.0) * (x - 1);
        }
    }
    //---------------------------------------------image_filter_spline36
    public struct ImageFilterSpline36 : IImageFilter
    {
        public double GetRadius() { return 3.0; }
        public double CalculateWeight(double x)
        {
            if (x < 1.0)
            {
                return ((13.0 / 11.0 * x - 453.0 / 209.0) * x - 3.0 / 209.0) * x + 1.0;
            }
            if (x < 2.0)
            {
                return ((-6.0 / 11.0 * (x - 1) + 270.0 / 209.0) * (x - 1) - 156.0 / 209.0) * (x - 1);
            }
            return ((1.0 / 11.0 * (x - 2) - 45.0 / 209.0) * (x - 2) + 26.0 / 209.0) * (x - 2);
        }
    }
    //----------------------------------------------image_filter_gaussian
    public struct ImageFilterGaussian : IImageFilter
    {
        public double GetRadius() { return 2.0; }
        public double CalculateWeight(double x)
        {
            return Math.Exp(-2.0 * x * x) * Math.Sqrt(2.0 / Math.PI);
        }
    }
    //------------------------------------------------image_filter_bessel
    public struct ImageFilterBessel : IImageFilter
    {
        public double GetRadius() { return 3.2383; }
        public double CalculateWeight(double x)
        {
            return (x == 0.0) ? Math.PI / 4.0 : AggMath.besj(Math.PI * x, 1) / (2.0 * x);
        }
    }
    //-------------------------------------------------image_filter_sinc
    public class ImageFilterSinc : IImageFilter
    {
        public ImageFilterSinc(double r)
        {
            m_radius = (r < 2.0 ? 2.0 : r);
        }
        public double GetRadius() { return m_radius; }
        public double CalculateWeight(double x)
        {
            if (x == 0.0) return 1.0;
            x *= Math.PI;
            return Math.Sin(x) / x;
        }
        private double m_radius;
    }
    //-----------------------------------------------image_filter_lanczos
    public class ImageFilterLanczos : IImageFilter
    {
        public ImageFilterLanczos(double r)
        {
            m_radius = (r < 2.0 ? 2.0 : r);
        }
        public double GetRadius() { return m_radius; }
        public double CalculateWeight(double x)
        {
            if (x == 0.0) return 1.0;
            if (x > m_radius) return 0.0;
            x *= Math.PI;
            double xr = x / m_radius;
            return (Math.Sin(x) / x) * (Math.Sin(xr) / xr);
        }
        private double m_radius;
    }
    //----------------------------------------------image_filter_blackman
    public class ImageFilterBlackMan : IImageFilter
    {
        public ImageFilterBlackMan(double r)
        {
            m_radius = (r < 2.0 ? 2.0 : r);
        }

        public double GetRadius() { return m_radius; }

        public double CalculateWeight(double x)
        {
            if (x == 0.0)
            {
                return 1.0;
            }

            if (x > m_radius)
            {
                return 0.0;
            }

            x *= Math.PI;
            double xr = x / m_radius;
            return (Math.Sin(x) / x) * (0.42 + 0.5 * Math.Cos(xr) + 0.08 * Math.Cos(2 * xr));
        }

        private double m_radius;
    }
}

