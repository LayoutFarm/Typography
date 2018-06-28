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
// Image transformations with filtering. Span generator base class
//
//----------------------------------------------------------------------------

using System;
using img_subpix_const = PixelFarm.CpuBlit.Imaging.ImageFilterLookUpTable.ImgSubPixConst;
namespace PixelFarm.CpuBlit.FragmentProcessing
{
    /// <summary>
    ///a image-span generator  generate 'color'-span from input image, send this spans to output     
    /// </summary>
    public abstract class ImgSpanGen : ISpanGenerator
    {
        ISpanInterpolator m_interpolator;
        double m_dx_dbl;
        double m_dy_dbl;
        int m_dx_int;
        int m_dy_int;
        public ImgSpanGen(ISpanInterpolator interpolator)
        {
            m_interpolator = interpolator;
            m_dx_dbl = 0.5;
            m_dy_dbl = 0.5;
            m_dx_int = (img_subpix_const.SCALE / 2);
            m_dy_int = (img_subpix_const.SCALE / 2);
        }

        public abstract void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len);
        protected ISpanInterpolator Interpolator
        {
            get { return m_interpolator; }
        }

        public double dx { get { return m_dx_dbl; } }
        public double dy { get { return m_dy_dbl; } }
        public int dxInt { get { return m_dx_int; } }
        public int dyInt { get { return m_dy_int; } }


        public void SetFilterOffset(double dx, double dy)
        {
            m_dx_dbl = dx;
            m_dy_dbl = dy;
            m_dx_int = AggMath.iround(dx * img_subpix_const.SCALE);
            m_dy_int = AggMath.iround(dy * img_subpix_const.SCALE);
        }
        public void SetFilterOffset(double d) { SetFilterOffset(d, d); }
        public virtual void Prepare() { }
    }
}