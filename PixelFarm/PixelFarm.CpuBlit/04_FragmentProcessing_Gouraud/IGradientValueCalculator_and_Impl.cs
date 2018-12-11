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
        int _r;
        int _fx;
        int _fy;
        double _r2;
        double _fx2;
        double _fy2;
        double _mul;
        //---------------------------------------------------------------------
        public GvcRadialFocus()
        {
            _r = (100 * GradientSpanGen.GR_SUBPIX_SCALE);
            _fx = 0;
            _fy = 0;
            UpdateValues();
        }

        //---------------------------------------------------------------------
        public void Setup(double r, double fx, double fy)
        {
            _r = AggMath.iround(r * GradientSpanGen.GR_SUBPIX_SCALE);
            _fx = AggMath.iround(fx * GradientSpanGen.GR_SUBPIX_SCALE);
            _fy = AggMath.iround(fy * GradientSpanGen.GR_SUBPIX_SCALE);
            UpdateValues();
        }

        //---------------------------------------------------------------------
        public double Radius => (double)(_r) / GradientSpanGen.GR_SUBPIX_SCALE;
        public double FocusX => (double)(_fx) / GradientSpanGen.GR_SUBPIX_SCALE;
        public double FocusY => (double)(_fy) / GradientSpanGen.GR_SUBPIX_SCALE;

        //---------------------------------------------------------------------
        public int Calculate(int x, int y, int d)
        {
            double dx = x - _fx;
            double dy = y - _fy;
            double d2 = dx * _fy - dy * _fx;
            double d3 = _r2 * (dx * dx + dy * dy) - d2 * d2;
            return AggMath.iround((dx * _fx + dy * _fy + System.Math.Sqrt(System.Math.Abs(d3))) * _mul);
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
            _r2 = _r * _r;
            _fx2 = _fx * _fx;
            _fy2 = _fy * _fy;
            double d = (_r2 - (_fx2 + _fy2));
            if (d == 0)
            {
                if (_fx != 0)
                {
                    if (_fx < 0) ++_fx; else --_fx;
                }

                if (_fy != 0)
                {
                    if (_fy < 0) ++_fy; else --_fy;
                }

                _fx2 = _fx * _fx;
                _fy2 = _fy * _fy;
                d = (_r2 - (_fx2 + _fy2));
            }
            _mul = _r / d;
        }
    }
    //==============================================================gradient_x
    public class GvcX : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d) => x;
    }
    //==============================================================gradient_y
    public class GvcY : IGradientValueCalculator
    {
        public int Calculate(int x, int y, int d) => y;
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