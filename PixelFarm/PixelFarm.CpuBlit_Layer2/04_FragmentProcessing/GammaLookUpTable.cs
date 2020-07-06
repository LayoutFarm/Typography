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
        float _gamma;
        byte[] _dir_gamma;
        byte[] _inv_gamma;
        public GammaLookUpTable(float gamma)
        {
            _gamma = gamma;
            _dir_gamma = new byte[GAMMA_SIZE];
            _inv_gamma = new byte[GAMMA_SIZE];
            SetGamma(_gamma);
        }

        void SetGamma(float g)
        {
            _gamma = g;
            float inv_g = (float)(1.0 / g);
            for (int i = GAMMA_SIZE - 1; i >= 0; --i)
            {
                _dir_gamma[i] = (byte)AggMath.uround(Math.Pow(i / (float)GAMMA_MASK, _gamma) * (float)GAMMA_MASK);
                _inv_gamma[i] = (byte)AggMath.uround(Math.Pow(i / (float)GAMMA_MASK, inv_g) * (float)GAMMA_MASK);
            }
        }
        //
        public double Gamma => _gamma;
        //
        public byte dir(int v) => _dir_gamma[v];
        //
        public byte inv(int v) => _inv_gamma[v];
    }
}

