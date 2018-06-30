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

using System; 
namespace PixelFarm.CpuBlit.FragmentProcessing
{
    public struct GammaNone : IGammaFunction
    {
        public float GetGamma(float x) { return x; }
    }
    //==============================================================gamma_power
    public class GammaPower : IGammaFunction
    {
        float m_gamma;
        public GammaPower() { m_gamma = 1.0f; }
        public GammaPower(float g) { m_gamma = g; }

        public void gamma(float g) { m_gamma = g; }
        public float gamma() { return m_gamma; }

        public float GetGamma(float x)
        {
            return (float)Math.Pow(x, m_gamma);
        }
    }

    //==========================================================gamma_threshold
    public class GammaThreshold : IGammaFunction
    {
        float m_threshold;
        public GammaThreshold() { m_threshold = 0.5f; }
        public GammaThreshold(float t) { m_threshold = t; }

        public void threshold(float t) { m_threshold = t; }
        public float threshold() { return m_threshold; }

        public float GetGamma(float x)
        {
            return (x < m_threshold) ? 0.0f : 1.0f;
        }
    }

    //============================================================gamma_linear
    public class GammaLinear : IGammaFunction
    {
        float m_start;
        float m_end;
        public GammaLinear()
        {
            m_start = 0.0f;
            m_end = 1.0f;
        }
        public GammaLinear(float s, float e)
        {
            m_start = (s);
            m_end = (e);
        }
        public float Start { get { return this.m_start; } }
        public float End { get { return this.m_end; } }
        public void Set(float s, float e) { m_start = s; m_end = e; }

        public float GetGamma(float x)
        {
            if (x < m_start) return 0.0f;
            if (x > m_end) return 1.0f;
            double EndMinusStart = m_end - m_start;
            if (EndMinusStart != 0)
                return (float)((x - m_start) / EndMinusStart);
            else
                return 0.0f;
        }
    }

    //==========================================================gamma_multiply
    public class GammaMultiply : IGammaFunction
    {
        float m_mul;
        public GammaMultiply()
        {
            m_mul = (1.0f);
        }
        public GammaMultiply(float v)
        {
            m_mul = (v);
        }

        public float GetGamma(float x)
        {
            float y = x * m_mul;
            if (y > 1.0) y = 1.0f;
            return y;
        }
    }
}