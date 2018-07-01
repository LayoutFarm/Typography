//MIT, 2014-present, WinterDev
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


namespace PixelFarm.CpuBlit.FragmentProcessing
{
    public interface IGradientValueCalculator
    {
        int Calculate(int x, int y, int d);
    }

    public interface IGradientColorsProvider
    {
        int GradientSteps { get; }
        Drawing.Color GetColor(int v);
    }


    //==========================================================gradient_radial
    /// <summary>
    /// gradient value calculator 
    /// </summary>
    public class GvcRadial : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            //TODO: check Taxicab => https://en.wikipedia.org/wiki/Taxicab_geometry
            //return (int)(AggMath.fast_sqrt((int)(x * x + y * y)));
            return (int)(System.Math.Sqrt(x * x + y * y));
        }
    }

    //====================================================gradient_radial_focus
    public class GvcRadialFocus : IGradientValueCalculator
    {
        int m_r;
        int m_fx;
        int m_fy;
        double m_r2;
        double m_fx2;
        double m_fy2;
        double m_mul;
        //---------------------------------------------------------------------
        public GvcRadialFocus()
        {
            m_r = (100 * GradientSpanGen.GR_SUBPIX_SCALE);
            m_fx = 0;
            m_fy = 0;
            UpdateValues();
        }

        //---------------------------------------------------------------------
        public void Setup(double r, double fx, double fy)
        {
            m_r = AggMath.iround(r * GradientSpanGen.GR_SUBPIX_SCALE);
            m_fx = AggMath.iround(fx * GradientSpanGen.GR_SUBPIX_SCALE);
            m_fy = AggMath.iround(fy * GradientSpanGen.GR_SUBPIX_SCALE);
            UpdateValues();
        }

        //---------------------------------------------------------------------
        public double Radius { get { return (double)(m_r) / GradientSpanGen.GR_SUBPIX_SCALE; } }
        public double FocusX { get { return (double)(m_fx) / GradientSpanGen.GR_SUBPIX_SCALE; } }
        public double FocusY { get { return (double)(m_fy) / GradientSpanGen.GR_SUBPIX_SCALE; } }

        //---------------------------------------------------------------------
        public int Calculate(int x, int y, int d)
        {
            double dx = x - m_fx;
            double dy = y - m_fy;
            double d2 = dx * m_fy - dy * m_fx;
            double d3 = m_r2 * (dx * dx + dy * dy) - d2 * d2;
            return AggMath.iround((dx * m_fx + dy * m_fy + System.Math.Sqrt(System.Math.Abs(d3))) * m_mul);
        }

        //---------------------------------------------------------------------
        private void UpdateValues()
        {
            // Calculate the invariant values. In case the focal center
            // lies exactly on the gradient circle the divisor degenerates
            // into zero. In this case we just move the focal center by
            // one subpixel unit possibly in the direction to the origin (0,0)
            // and calculate the values again.
            //-------------------------
            m_r2 = m_r * m_r;
            m_fx2 = m_fx * m_fx;
            m_fy2 = m_fy * m_fy;
            double d = (m_r2 - (m_fx2 + m_fy2));
            if (d == 0)
            {
                if (m_fx != 0)
                {
                    if (m_fx < 0) ++m_fx; else --m_fx;
                }

                if (m_fy != 0)
                {
                    if (m_fy < 0) ++m_fy; else --m_fy;
                }

                m_fx2 = m_fx * m_fx;
                m_fy2 = m_fy * m_fy;
                d = (m_r2 - (m_fx2 + m_fy2));
            }
            m_mul = m_r / d;
        }
    }
    //==============================================================gradient_x
    public class GvcX : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            return x;
        }
    }
    //==============================================================gradient_y
    public class GvcY : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d) { return y; }
    }

    //========================================================gradient_diamond
    public class GvcDiamond : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            int ax = System.Math.Abs(x);
            int ay = System.Math.Abs(y);
            return ax > ay ? ax : ay;
        }
    }

    //=============================================================gradient_xy
    public class GvcXY : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            return System.Math.Abs(x * y) / d;
        }
    }

    //========================================================gradient_sqrt_xy
    public class GvcSquareXY : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            //return (int)System.Math.Sqrt((int)(System.Math.Abs(x) * System.Math.Abs(y)));
            return (int)AggMath.fast_sqrt((int)(System.Math.Abs(x * y)));
        }
    }

    //==========================================================gradient_conic
    public class GvcConic : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d)
        {
            return (int)AggMath.uround(System.Math.Abs(System.Math.Atan2((double)(y), (double)(x))) * (double)(d) / System.Math.PI);
        }
    }

    //=================================================gradient_repeat_adaptor
    public class GvcRepeatAdaptor : IGradientValueCalculator
    {
        IGradientValueCalculator m_gradient;
        public GvcRepeatAdaptor(IGradientValueCalculator gradient)
        {
            m_gradient = gradient;
        }
        public int Calculate(int x, int y, int d)
        {
            int ret = m_gradient.Calculate(x, y, d) % d;
            if (ret < 0) ret += d;
            return ret;
        }
    }

    //================================================gradient_reflect_adaptor
    public class GvcReflectAdaptor : IGradientValueCalculator
    {
        IGradientValueCalculator m_gradient;
        public GvcReflectAdaptor(IGradientValueCalculator gradient)
        {
            m_gradient = gradient;
        }

        public int Calculate(int x, int y, int d)
        {
            int d2 = d << 1;
            int ret = m_gradient.Calculate(x, y, d) % d2;
            if (ret < 0) ret += d2;
            if (ret >= d) ret = d2 - ret;
            return ret;
        }
    }

    public class GvcClampAdapter : IGradientValueCalculator
    {
        IGradientValueCalculator m_gradient;
        public GvcClampAdapter(IGradientValueCalculator gradient)
        {
            m_gradient = gradient;
        }

        public int Calculate(int x, int y, int d)
        {
            int ret = m_gradient.Calculate(x, y, d);
            if (ret < 0) ret = 0;
            if (ret > d) ret = d;
            return ret;
        }
    }
}