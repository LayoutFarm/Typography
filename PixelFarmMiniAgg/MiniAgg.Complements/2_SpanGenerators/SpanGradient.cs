//MIT, 2014-2016, WinterDev
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

using PixelFarm.Drawing;
using PixelFarm.Agg.Gradients;
namespace PixelFarm.Agg
{
    //==========================================================span_gradient
    public class SpanGenGradient : ISpanGenerator
    {
        const int GR_SUBPIX_SHIFT = 4;                              //-----gradient_subpixel_shift
        public const int GR_SUBPIX_SCALE = 1 << GR_SUBPIX_SHIFT;   //-----gradient_subpixel_scale
        const int GR_SUBPIX_MASK = GR_SUBPIX_SCALE - 1;    //-----gradient_subpixel_mask
        const int SUBPIX_SHIFT = 8;
        const int DOWN_SCALE_SHIFT = SUBPIX_SHIFT - GR_SUBPIX_SHIFT;
        readonly ISpanInterpolator m_interpolator;
        readonly IGradientValueCalculator m_grValueCalculator;
        readonly IGradientColorsProvider m_colorsProvider;
        readonly int m_d1;
        readonly int m_d2;
        readonly int dd;
        readonly float stepRatio;
        //--------------------------------------------------------------------
        public SpanGenGradient(ISpanInterpolator inter,
                      IGradientValueCalculator gvc,
                      IGradientColorsProvider m_colorsProvider,
                      double d1, double d2)
        {
            this.m_interpolator = inter;
            this.m_grValueCalculator = gvc;
            this.m_colorsProvider = m_colorsProvider;
            m_d1 = AggBasics.iround(d1 * GR_SUBPIX_SCALE);
            m_d2 = AggBasics.iround(d2 * GR_SUBPIX_SCALE);
            dd = m_d2 - m_d1;
            if (dd < 1) dd = 1;
            stepRatio = (float)m_colorsProvider.GradientSteps / (float)dd;
        }


        //--------------------------------------------------------------------
        public void Prepare() { }
        //--------------------------------------------------------------------
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int len)
        {
            m_interpolator.Begin(x + 0.5, y + 0.5, len);
            do
            {
                m_interpolator.GetCoord(out x, out y);
                float d = m_grValueCalculator.Calculate(x >> DOWN_SCALE_SHIFT,
                                                      y >> DOWN_SCALE_SHIFT,
                                                      m_d2);
                d = ((d - m_d1) * stepRatio);
                if (d < 0) d = 0;
                if (d >= m_colorsProvider.GradientSteps)
                {
                    d = m_colorsProvider.GradientSteps - 1;
                }

                outputColors[startIndex++] = m_colorsProvider.GetColor((int)d);
                m_interpolator.Next();
            }
            while (--len != 0);
        }
    }

    //=====================================================gradient_linear_color
    public class LinearGradientColorsProvider : Gradients.IGradientColorsProvider
    {
        Color m_c1;
        Color m_c2;
        int gradientSteps;
        public LinearGradientColorsProvider(Color c1, Color c2)
            : this(c1, c2, 256)
        {
        }
        public LinearGradientColorsProvider(Color c1, Color c2, int gradientSteps)
        {
            m_c1 = c1;
            m_c2 = c2;
            this.gradientSteps = gradientSteps;
        }
        public int GradientSteps { get { return gradientSteps; } }
        public Color GetColor(int v)
        {
            return m_c1.CreateGradient(m_c2, (float)(v) / (float)(gradientSteps - 1));
        }
        public void SetColors(Color c1, Color c2)
        {
            SetColors(c1, c2, 256);
        }
        public void SetColors(Color c1, Color c2, int gradientSteps)
        {
            m_c1 = c1;
            m_c2 = c2;
            this.gradientSteps = gradientSteps;
        }
    }
}