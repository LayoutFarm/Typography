////BSD, 2014-2016, WinterDev
////----------------------------------------------------------------------------
//// Anti-Grain Geometry - Version 2.4
//// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
////
//// C# Port port by: Lars Brubaker
////                  larsbrubaker@gmail.com
//// Copyright (C) 2007
////
//// Permission to copy, use, modify, sell and distribute this software 
//// is granted provided this copyright notice appears in all copies. 
//// This software is provided "as is" without express or implied
//// warranty, and with no claim as to its suitability for any purpose.
////
////----------------------------------------------------------------------------
//// Contact: mcseem@antigrain.com
////          mcseemagg@yahoo.com
////          http://www.antigrain.com
////----------------------------------------------------------------------------
////
//// class ClippingPixelFormtProxy
////
////----------------------------------------------------------------------------
//using System;
//using System.IO;
//using MatterHackers.Agg;

//namespace MatterHackers.Agg.Image
//{
//    //public class ImageClippingProxyFloat : ImageProxyFloat
//    //{
//    //    private RectangleInt m_ClippingRect;

//    //    public const byte cover_full = 255;

//    //    public ImageClippingProxyFloat(IImageFloat ren)
//    //        : base(ren)
//    //    {
//    //        m_ClippingRect = new RectangleInt(0, 0, (int)ren.Width - 1, (int)ren.Height - 1);
//    //    }

//    //    public override void LinkToImage(IImageFloat ren)
//    //    {
//    //        base.LinkToImage(ren);
//    //        m_ClippingRect = new RectangleInt(0, 0, (int)ren.Width - 1, (int)ren.Height - 1);
//    //    }

//    //    public bool SetClippingBox(int x1, int y1, int x2, int y2)
//    //    {
//    //        RectangleInt cb = new RectangleInt(x1, y1, x2, y2);
//    //        cb.normalize();
//    //        if (cb.clip(new RectangleInt(0, 0, (int)Width - 1, (int)Height - 1)))
//    //        {
//    //            m_ClippingRect = cb;
//    //            return true;
//    //        }
//    //        m_ClippingRect.Left = 1;
//    //        m_ClippingRect.Bottom = 1;
//    //        m_ClippingRect.Right = 0;
//    //        m_ClippingRect.Top = 0;
//    //        return false;
//    //    }

//    //    public void reset_clipping(bool visibility)
//    //    {
//    //        if (visibility)
//    //        {
//    //            m_ClippingRect.Left = 0;
//    //            m_ClippingRect.Bottom = 0;
//    //            m_ClippingRect.Right = (int)Width - 1;
//    //            m_ClippingRect.Top = (int)Height - 1;
//    //        }
//    //        else
//    //        {
//    //            m_ClippingRect.Left = 1;
//    //            m_ClippingRect.Bottom = 1;
//    //            m_ClippingRect.Right = 0;
//    //            m_ClippingRect.Top = 0;
//    //        }
//    //    }

//    //    public void clip_box_naked(int x1, int y1, int x2, int y2)
//    //    {
//    //        m_ClippingRect.Left = x1;
//    //        m_ClippingRect.Bottom = y1;
//    //        m_ClippingRect.Right = x2;
//    //        m_ClippingRect.Top = y2;
//    //    }

//    //    public bool inbox(int x, int y)
//    //    {
//    //        return x >= m_ClippingRect.Left && y >= m_ClippingRect.Bottom &&
//    //               x <= m_ClippingRect.Right && y <= m_ClippingRect.Top;
//    //    }

//    //    public RectangleInt clip_box() { return m_ClippingRect; }
//    //    int XMin { return m_ClippingRect.Left; }
//    //    int YMin { return m_ClippingRect.Bottom; }
//    //    int XMax { return m_ClippingRect.Right; }
//    //    int YMax { return m_ClippingRect.Top; }

//    //    public RectangleInt bounding_clip_box() { return m_ClippingRect; }
//    //    public int bounding_XMin { return m_ClippingRect.Left; }
//    //    public int bounding_YMin { return m_ClippingRect.Bottom; }
//    //    public int bounding_XMax { return m_ClippingRect.Right; }
//    //    public int bounding_YMax { return m_ClippingRect.Top; }

//    //    public void clear(IColorType in_c)
//    //    {
//    //        int y;
//    //        RGBA_Floats colorFloat = in_c.GetAsRGBA_Floats();
//    //        if (Width != 0)
//    //        {
//    //            for (y = 0; y < Height; y++)
//    //            {
//    //                base.copy_hline(0, (int)y, (int)Width, colorFloat);
//    //            }
//    //        }
//    //    }

//    //    public override void copy_pixel(int x, int y, float[] c, int ByteOffset)
//    //    {
//    //        if (inbox(x, y))
//    //        {
//    //            base.copy_pixel(x, y, c, ByteOffset);
//    //        }
//    //    }

//    //    public override RGBA_Floats GetPixel(int x, int y)
//    //    {
//    //        return inbox(x, y) ? base.GetPixel(x, y) : new RGBA_Floats();
//    //    }

//    //    public override void copy_hline(int x1, int y, int x2, RGBA_Floats c)
//    //    {
//    //        if (x1 > x2) { int t = (int)x2; x2 = (int)x1; x1 = t; }
//    //        if (y > YMax) return;
//    //        if (y < YMin) return;
//    //        if (x1 > XMax) return;
//    //        if (x2 < XMin) return;

//    //        if (x1 < XMin) x1 = XMin;
//    //        if (x2 > XMax) x2 = (int)XMax;

//    //        base.copy_hline(x1, y, (int)(x2 - x1 + 1), c);
//    //    }

//    //    public override void copy_vline(int x, int y1, int y2, RGBA_Floats c)
//    //    {
//    //        if (y1 > y2) { int t = (int)y2; y2 = (int)y1; y1 = t; }
//    //        if (x > XMax) return;
//    //        if (x < XMin) return;
//    //        if (y1 > YMax) return;
//    //        if (y2 < YMin) return;

//    //        if (y1 < YMin) y1 = YMin;
//    //        if (y2 > YMax) y2 = (int)YMax;

//    //        base.copy_vline(x, y1, (int)(y2 - y1 + 1), c);
//    //    }

//    //    public override void blend_hline(int x1, int y, int x2, RGBA_Floats c, byte cover)
//    //    {
//    //        if (x1 > x2)
//    //        {
//    //            int t = (int)x2;
//    //            x2 = x1;
//    //            x1 = t;
//    //        }
//    //        if (y > YMax)
//    //            return;
//    //        if (y < YMin)
//    //            return;
//    //        if (x1 > XMax)
//    //            return;
//    //        if (x2 < XMin)
//    //            return;

//    //        if (x1 < XMin)
//    //            x1 = XMin;
//    //        if (x2 > XMax)
//    //            x2 = XMax;

//    //        base.blend_hline(x1, y, x2, c, cover);
//    //    }

//    //    public override void blend_vline(int x, int y1, int y2, RGBA_Floats c, byte cover)
//    //    {
//    //        if (y1 > y2) { int t = y2; y2 = y1; y1 = t; }
//    //        if (x > XMax) return;
//    //        if (x < XMin) return;
//    //        if (y1 > YMax) return;
//    //        if (y2 < YMin) return;

//    //        if (y1 < YMin) y1 = YMin;
//    //        if (y2 > YMax) y2 = YMax;

//    //        base.blend_vline(x, y1, y2, c, cover);
//    //    }

//    //    public override void blend_solid_hspan(int x, int y, int in_len, RGBA_Floats c, byte[] covers, int coversIndex)
//    //    {
//    //        int len = (int)in_len;
//    //        if (y > YMax) return;
//    //        if (y < YMin) return;

//    //        if (x < XMin)
//    //        {
//    //            len -= XMin - x;
//    //            if (len <= 0) return;
//    //            coversIndex += XMin - x;
//    //            x = XMin;
//    //        }
//    //        if (x + len > XMax)
//    //        {
//    //            len = XMax - x + 1;
//    //            if (len <= 0) return;
//    //        }
//    //        base.blend_solid_hspan(x, y, len, c, covers, coversIndex);
//    //    }

//    //    public override void blend_solid_vspan(int x, int y, int len, RGBA_Floats c, byte[] covers, int coversIndex)
//    //    {
//    //        if (x > XMax) return;
//    //        if (x < XMin) return;

//    //        if (y < YMin)
//    //        {
//    //            len -= (YMin - y);
//    //            if (len <= 0) return;
//    //            coversIndex += YMin - y;
//    //            y = YMin;
//    //        }
//    //        if (y + len > YMax)
//    //        {
//    //            len = (YMax - y + 1);
//    //            if (len <= 0) return;
//    //        }
//    //        base.blend_solid_vspan(x, y, len, c, covers, coversIndex);
//    //    }

//    //    public override void copy_color_hspan(int x, int y, int len, RGBA_Floats[] colors, int colorsIndex)
//    //    {
//    //        if (y > YMax) return;
//    //        if (y < YMin) return;

//    //        if (x < XMin)
//    //        {
//    //            int d = XMin - x;
//    //            len -= d;
//    //            if (len <= 0) return;
//    //            colorsIndex += d;
//    //            x = XMin;
//    //        }
//    //        if (x + len > XMax)
//    //        {
//    //            len = (XMax - x + 1);
//    //            if (len <= 0) return;
//    //        }
//    //        base.copy_color_hspan(x, y, len, colors, colorsIndex);
//    //    }

//    //    public override void copy_color_vspan(int x, int y, int len, RGBA_Floats[] colors, int colorsIndex)
//    //    {
//    //        if (x > XMax) return;
//    //        if (x < XMin) return;

//    //        if (y < YMin)
//    //        {
//    //            int d = YMin - y;
//    //            len -= d;
//    //            if (len <= 0) return;
//    //            colorsIndex += d;
//    //            y = YMin;
//    //        }
//    //        if (y + len > YMax)
//    //        {
//    //            len = (YMax - y + 1);
//    //            if (len <= 0) return;
//    //        }
//    //        base.copy_color_vspan(x, y, len, colors, colorsIndex);
//    //    }

//    //    public override void blend_color_hspan(int x, int y, int in_len, RGBA_Floats[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
//    //    {
//    //        int len = (int)in_len;
//    //        if (y > YMax)
//    //            return;
//    //        if (y < YMin)
//    //            return;

//    //        if (x < XMin)
//    //        {
//    //            int d = XMin - x;
//    //            len -= d;
//    //            if (len <= 0) return;
//    //            if (covers != null) coversIndex += d;
//    //            colorsIndex += d;
//    //            x = XMin;
//    //        }
//    //        if (x + len - 1 > XMax)
//    //        {
//    //            len = XMax - x + 1;
//    //            if (len <= 0) return;
//    //        }

//    //        base.blend_color_hspan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
//    //    }

//    //    public void copy_from(IImageFloat src)
//    //    {
//    //        CopyFrom(src, new RectangleInt(0, 0, (int)src.Width, (int)src.Height), 0, 0);
//    //    }

//    //    public override void SetPixel(int x, int y, RGBA_Floats color)
//    //    {
//    //        if ((uint)x < Width && (uint)y < Height)
//    //        {
//    //            base.SetPixel(x, y, color);
//    //        }
//    //    }

//    //    public override void CopyFrom(IImageFloat sourceImage,
//    //                   RectangleInt sourceImageRect,
//    //                   int destXOffset,
//    //                   int destYOffset)
//    //    {
//    //        RectangleInt destRect = sourceImageRect;
//    //        destRect.Offset(destXOffset, destYOffset);

//    //        RectangleInt clippedSourceRect = new RectangleInt();
//    //        if (clippedSourceRect.IntersectRectangles(destRect, m_ClippingRect))
//    //        {
//    //            // move it back relative to the source
//    //            clippedSourceRect.Offset(-destXOffset, -destYOffset);

//    //            base.CopyFrom(sourceImage, clippedSourceRect, destXOffset, destYOffset);
//    //        }
//    //    }

//    //    public RectangleInt clip_rect_area(ref RectangleInt destRect, ref RectangleInt sourceRect, int sourceWidth, int sourceHeight)
//    //    {
//    //        RectangleInt rc = new RectangleInt(0, 0, 0, 0);
//    //        RectangleInt cb = clip_box();
//    //        ++cb.Right;
//    //        ++cb.Top;

//    //        if (sourceRect.Left < 0)
//    //        {
//    //            destRect.Left -= sourceRect.Left;
//    //            sourceRect.Left = 0;
//    //        }
//    //        if (sourceRect.Bottom < 0)
//    //        {
//    //            destRect.Bottom -= sourceRect.Bottom;
//    //            sourceRect.Bottom = 0;
//    //        }

//    //        if (sourceRect.Right > sourceWidth) sourceRect.Right = sourceWidth;
//    //        if (sourceRect.Top > sourceHeight) sourceRect.Top = sourceHeight;

//    //        if (destRect.Left < cb.Left)
//    //        {
//    //            sourceRect.Left += cb.Left - destRect.Left;
//    //            destRect.Left = cb.Left;
//    //        }
//    //        if (destRect.Bottom < cb.Bottom)
//    //        {
//    //            sourceRect.Bottom += cb.Bottom - destRect.Bottom;
//    //            destRect.Bottom = cb.Bottom;
//    //        }

//    //        if (destRect.Right > cb.Right) destRect.Right = cb.Right;
//    //        if (destRect.Top > cb.Top) destRect.Top = cb.Top;

//    //        rc.Right = destRect.Right - destRect.Left;
//    //        rc.Top = destRect.Top - destRect.Bottom;

//    //        if (rc.Right > sourceRect.Right - sourceRect.Left) rc.Right = sourceRect.Right - sourceRect.Left;
//    //        if (rc.Top > sourceRect.Top - sourceRect.Bottom) rc.Top = sourceRect.Top - sourceRect.Bottom;
//    //        return rc;
//    //    }

//    //    public override void blend_color_vspan(int x, int y, int len, RGBA_Floats[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
//    //    {
//    //        if (x > XMax) return;
//    //        if (x < XMin) return;

//    //        if (y < YMin)
//    //        {
//    //            int d = YMin - y;
//    //            len -= d;
//    //            if (len <= 0) return;
//    //            if (covers != null) coversIndex += d;
//    //            colorsIndex += d;
//    //            y = YMin;
//    //        }
//    //        if (y + len > YMax)
//    //        {
//    //            len = (YMax - y + 1);
//    //            if (len <= 0) return;
//    //        }
//    //        base.blend_color_vspan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
//    //    }
//    //}
//}