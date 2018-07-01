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
// Class scanline_p - a general purpose scanline container with packed spans.
//
//----------------------------------------------------------------------------
//
// Adaptation for 32-bit screen coordinates (scanline32_p) has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------


namespace PixelFarm.CpuBlit.Rasterization
{
    public struct ScanlineSpan
    {
        public readonly short x;
        public short len; //+ or - 
        public readonly short cover_index;
        public ScanlineSpan(int x, int cover_index)
        {
            //TODO: x should be ushort?
            this.x = (short)x;
            this.len = 1;
            this.cover_index = (short)cover_index;
        }
        public ScanlineSpan(int x, int len, int cover_index)
        {
            //TODO: x should be ushort?
            this.x = (short)x;
            this.len = (short)len;
            this.cover_index = (short)cover_index;
        }
#if DEBUG
        public override string ToString()
        {
            return "x:" + x + ",len:" + len + ",cover:" + cover_index;
        }
#endif
    }
}