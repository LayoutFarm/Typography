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


using System;
namespace PixelFarm.CpuBlit.Rasterization
{
    //=============================================================scanline_p8
    // 
    // This is a general purpose scanline container which supports the interface 
    // used in the rasterizer::render(). See description of scanline_u8
    // for details.
    // 
    //------------------------------------------------------------------------


    public sealed class ScanlinePacked8 : Scanline
    {
        public ScanlinePacked8()
        {
        }

        public override void ResetSpans(int min_x, int max_x)
        {
            int max_len = max_x - min_x + 3;
            if (max_len > _spans.Length)
            {
                _spans = new ScanlineSpan[max_len];
                _covers = new byte[max_len];
            }

            _last_x = 0x7FFFFFF0;
            _cover_index = 0; //make it ready for next add
            _last_span_index = 0;
            _spans[_last_span_index].len = 0;
        }
        public override void AddCell(int x, int cover)
        {
            _covers[_cover_index] = (byte)cover;
            if (x == _last_x + 1 && _spans[_last_span_index].len > 0)
            {
                //append to last cell
                _spans[_last_span_index].len++;
            }
            else
            {
                //start new  
                _last_span_index++;
                _spans[_last_span_index] = new ScanlineSpan((short)x, _cover_index);
            }
            _last_x = x;
            _cover_index++; //make it ready for next add
        }
        public override void AddSpan(int x, int len, int cover)
        {
            int backupCover = cover;
            if (x == _last_x + 1
                && _spans[_last_span_index].len < 0
                && cover == _spans[_last_span_index].cover_index)
            {
                //just append data to latest span ***
                _spans[_last_span_index].len -= (short)len;
            }
            else
            {
                _covers[_cover_index] = (byte)cover;
                _last_span_index++;
                //---------------------------------------------------
                //start new  
                _spans[_last_span_index] = new ScanlineSpan((short)x, (short)(-len), _cover_index);
                _cover_index++; //make it ready for next add
            }
            _last_x = x + len - 1;
        }
        public override void ResetSpans()
        {
            _last_x = 0x7FFFFFF0;
            _last_span_index = 0;
            _cover_index = 0; //make it ready for next add
            _spans[_last_span_index].len = 0;
        }
    }
}
