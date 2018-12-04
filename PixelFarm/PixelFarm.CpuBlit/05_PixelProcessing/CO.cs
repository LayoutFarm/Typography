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

//#define ABGR
#define ARGB
namespace PixelFarm.CpuBlit.PixelProcessing
{
    /// <summary>
    /// color order
    /// </summary>
    public static class CO
    {

#if ARGB  //  eg. Win32
        //eg. windows
        /// <summary>
        /// order b
        /// </summary>
        public const int B = 0;
        /// <summary>
        /// order g
        /// </summary>
        public const int G = 1;
        /// <summary>
        /// order r
        /// </summary>
        public const int R = 2;
        /// <summary>
        /// order a
        /// </summary>
        public const int A = 3; 
#endif

#if ABGR //   eg. Skia on iOS
        /// <summary>
        /// order b
        /// </summary>
        public const int B = 2;
        /// <summary>
        /// order g
        /// </summary>
        public const int G = 1;
        /// <summary>
        /// order r
        /// </summary>
        public const int R = 0;
        /// <summary>
        /// order a
        /// </summary>
        public const int A = 3; 
#endif

        public const int B_SHIFT = B * 8;
        /// <summary>
        /// order g
        /// </summary>
        public const int G_SHIFT = G * 8;
        /// <summary>
        /// order r
        /// </summary>
        public const int R_SHIFT = R * 8;
        /// <summary>
        /// order a
        /// </summary>
        public const int A_SHIFT = A * 8;
    }
}