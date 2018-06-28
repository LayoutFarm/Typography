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
    public class GammaLookUpTable
    {
        const int GAMMA_SHIFT = 8;
        const int GAMMA_SIZE = 1 << GAMMA_SHIFT;
        const int GAMMA_MASK = GAMMA_SIZE - 1;
        float m_gamma;
        byte[] m_dir_gamma;
        byte[] m_inv_gamma;
        public GammaLookUpTable(float gamma)
        {
            m_gamma = gamma;
            m_dir_gamma = new byte[GAMMA_SIZE];
            m_inv_gamma = new byte[GAMMA_SIZE];
            SetGamma(m_gamma);
        }

        void SetGamma(float g)
        {
            m_gamma = g;
            float inv_g = (float)(1.0 / g);
            for (int i = GAMMA_SIZE - 1; i >= 0; --i)
            {
                m_dir_gamma[i] = (byte)AggMath.uround(Math.Pow(i / (float)GAMMA_MASK, m_gamma) * (float)GAMMA_MASK);
                m_inv_gamma[i] = (byte)AggMath.uround(Math.Pow(i / (float)GAMMA_MASK, inv_g) * (float)GAMMA_MASK);
            }
        }

        public double Gamma
        {
            get { return m_gamma; }
        }

        public byte dir(int v)
        {
            return m_dir_gamma[v];
        }

        public byte inv(int v)
        {
            return m_inv_gamma[v];
        }
    }
}

