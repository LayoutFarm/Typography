//BSD, 2014-2017, WinterDev
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

using PixelFarm.Drawing;
using PixelFarm.Agg.Lines;
namespace PixelFarm.Agg
{
    public struct PatternFilterBilnearColorRGBA : IPatternFilter
    {
        public int Dilation { get { return 1; } }

        public void SetPixelLowRes(Color[][] buf, Color[] p, int offset, int x, int y)
        {
            p[offset] = buf[y][x];
        }

        public void SetPixelHighRes(ImageReaderWriterBase sourceImage,
            Color[] destBuffer,
            int destBufferOffset,
            int x,
            int y)
        {
            int r, g, b, a;
            r = g = b = a = LineAA.SUBPIXEL_SCALE * LineAA.SUBPIXEL_SCALE / 2;
            int weight;
            int x_lr = x >> LineAA.SUBPIXEL_SHIFT;
            int y_lr = y >> LineAA.SUBPIXEL_SHIFT;
            x &= LineAA.SUBPIXEL_MARK;
            y &= LineAA.SUBPIXEL_MARK;
            int sourceOffset;
            byte[] ptr = sourceImage.GetBuffer();
            sourceOffset = sourceImage.GetBufferOffsetXY(x_lr, y_lr);
            weight = (LineAA.SUBPIXEL_SCALE - x) *
                     (LineAA.SUBPIXEL_SCALE - y);
            r += weight * ptr[sourceOffset + CO.R];
            g += weight * ptr[sourceOffset + CO.G];
            b += weight * ptr[sourceOffset + CO.B];
            a += weight * ptr[sourceOffset + CO.A];
            sourceOffset += sourceImage.BytesBetweenPixelsInclusive;
            weight = x * (LineAA.SUBPIXEL_SCALE - y);
            r += weight * ptr[sourceOffset + CO.R];
            g += weight * ptr[sourceOffset + CO.G];
            b += weight * ptr[sourceOffset + CO.B];
            a += weight * ptr[sourceOffset + CO.A];
            sourceOffset = sourceImage.GetBufferOffsetXY(x_lr, y_lr + 1);
            weight = (LineAA.SUBPIXEL_SCALE - x) * y;
            r += weight * ptr[sourceOffset + CO.R];
            g += weight * ptr[sourceOffset + CO.G];
            b += weight * ptr[sourceOffset + CO.B];
            a += weight * ptr[sourceOffset + CO.A];
            sourceOffset += sourceImage.BytesBetweenPixelsInclusive;
            weight = x * y;
            r += weight * ptr[sourceOffset + CO.R];
            g += weight * ptr[sourceOffset + CO.G];
            b += weight * ptr[sourceOffset + CO.B];
            a += weight * ptr[sourceOffset + CO.A];
            destBuffer[destBufferOffset].red = (byte)(r >> LineAA.SUBPIXEL_SHIFT * 2);
            destBuffer[destBufferOffset].green = (byte)(g >> LineAA.SUBPIXEL_SHIFT * 2);
            destBuffer[destBufferOffset].blue = (byte)(b >> LineAA.SUBPIXEL_SHIFT * 2);
            destBuffer[destBufferOffset].alpha = (byte)(a >> LineAA.SUBPIXEL_SHIFT * 2);
        }
    }
}
