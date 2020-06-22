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
    public abstract class Scanline
    {
        //store area coverage value ***
        protected byte[] _covers; //this field may be reassigned again when ResetSpans(int min_x, int max_x)
        protected ScanlineSpan[] _spans; //this field may be reassigned again when ResetSpans(int min_x, int max_x)

        protected int _last_span_index;
        protected int _cover_index;
        protected int _lineY;
        protected int _last_x;
        public Scanline()
        {
            _last_x = (0x7FFFFFF0);
            _covers = new byte[1024];
            _spans = new ScanlineSpan[1024];
        }


        public ScanlineSpan GetSpan(int index) => _spans[index];
        public int SpanCount => _last_span_index;
        public void CloseLine(int y) => _lineY = y;
        public int Y => _lineY;
        public byte[] GetCovers() => _covers;
        //---------------------------------------------------
        public abstract void AddCell(int x, int cover);
        public abstract void AddSpan(int x, int len, int cover);
        public abstract void ResetSpans(int min_x, int max_x);
        public abstract void ResetSpans();
    }
}
