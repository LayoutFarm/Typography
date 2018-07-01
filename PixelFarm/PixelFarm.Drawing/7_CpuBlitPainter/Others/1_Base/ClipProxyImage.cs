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
// class ClippingPixelFormtProxy
//
//----------------------------------------------------------------------------

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.Imaging
{
    public sealed class ClipProxyImage : ProxyImage
    {
        RectInt _clippingRect;
        public ClipProxyImage(PixelProcessing.IBitmapBlender refImage)
            : base(refImage)
        {
            _clippingRect = new RectInt(0, 0, (int)refImage.Width - 1, (int)refImage.Height - 1);
        }

        public bool SetClippingBox(int x1, int y1, int x2, int y2)
        {
            RectInt cb = new RectInt(x1, y1, x2, y2);
            cb.Normalize();
            if (cb.Clip(new RectInt(0, 0, (int)Width - 1, (int)Height - 1)))
            {
                _clippingRect = cb;
                return true;
            }
            _clippingRect.Left = 1;
            _clippingRect.Bottom = 1;
            _clippingRect.Right = 0;
            _clippingRect.Top = 0;
            return false;
        }

        public bool InClipArea(int x, int y)
        {
            return _clippingRect.Contains(x, y);
        }
        public RectInt GetClipArea(ref RectInt destRect, ref RectInt sourceRect, int sourceWidth, int sourceHeight)
        {
            RectInt rc = new RectInt(0, 0, 0, 0);
            RectInt cb = ClipBox;
            ++cb.Right;
            ++cb.Top;
            if (sourceRect.Left < 0)
            {
                destRect.Left -= sourceRect.Left;
                sourceRect.Left = 0;
            }
            if (sourceRect.Bottom < 0)
            {
                destRect.Bottom -= sourceRect.Bottom;
                sourceRect.Bottom = 0;
            }

            if (sourceRect.Right > sourceWidth) sourceRect.Right = sourceWidth;
            if (sourceRect.Top > sourceHeight) sourceRect.Top = sourceHeight;
            if (destRect.Left < cb.Left)
            {
                sourceRect.Left += cb.Left - destRect.Left;
                destRect.Left = cb.Left;
            }
            if (destRect.Bottom < cb.Bottom)
            {
                sourceRect.Bottom += cb.Bottom - destRect.Bottom;
                destRect.Bottom = cb.Bottom;
            }

            if (destRect.Right > cb.Right) destRect.Right = cb.Right;
            if (destRect.Top > cb.Top) destRect.Top = cb.Top;
            rc.Right = destRect.Right - destRect.Left;
            rc.Top = destRect.Top - destRect.Bottom;
            if (rc.Right > sourceRect.Right - sourceRect.Left) rc.Right = sourceRect.Right - sourceRect.Left;
            if (rc.Top > sourceRect.Top - sourceRect.Bottom) rc.Top = sourceRect.Top - sourceRect.Bottom;
            return rc;
        }
        public void Clear(Color color)
        {
            Color c = color;
            int w = this.Width;
            if (w != 0)
            {
                for (int y = this.Height - 1; y >= 0; --y)
                {
                    base.CopyHL(0, y, w, c);
                }
            }
        }

        public RectInt ClipBox { get { return _clippingRect; } }
        int XMin { get { return _clippingRect.Left; } }
        int YMin { get { return _clippingRect.Bottom; } }
        int XMax { get { return _clippingRect.Right; } }
        int YMax { get { return _clippingRect.Top; } }



        public override Color GetPixel(int x, int y)
        {
            return InClipArea(x, y) ? base.GetPixel(x, y) : new Color();
        }

        public override void CopyHL(int x1, int y, int x2, Color c)
        {
            if (x1 > x2) { int t = (int)x2; x2 = (int)x1; x1 = t; }
            if (y > YMax) return;
            if (y < YMin) return;
            if (x1 > XMax) return;
            if (x2 < XMin) return;
            if (x1 < XMin) x1 = XMin;
            if (x2 > XMax) x2 = (int)XMax;
            base.CopyHL(x1, y, (int)(x2 - x1 + 1), c);
        }

        public override void CopyVL(int x, int y1, int y2, Color c)
        {
            if (y1 > y2) { int t = (int)y2; y2 = (int)y1; y1 = t; }
            if (x > XMax) return;
            if (x < XMin) return;
            if (y1 > YMax) return;
            if (y2 < YMin) return;
            if (y1 < YMin) y1 = YMin;
            if (y2 > YMax) y2 = YMax;
            base.CopyVL(x, y1, (y2 - y1 + 1), c);
        }

        public override void BlendHL(int x1, int y, int x2, Color c, byte cover)
        {
            if (x1 > x2)
            {
                int t = (int)x2;
                x2 = x1;
                x1 = t;
            }
            if (y > YMax)
                return;
            if (y < YMin)
                return;
            if (x1 > XMax)
                return;
            if (x2 < XMin)
                return;
            if (x1 < XMin)
                x1 = XMin;
            if (x2 > XMax)
                x2 = XMax;
            base.BlendHL(x1, y, x2, c, cover);
        }

        public override void BlendVL(int x, int y1, int y2, Color c, byte cover)
        {
            if (y1 > y2) { int t = y2; y2 = y1; y1 = t; }
            if (x > XMax) return;
            if (x < XMin) return;
            if (y1 > YMax) return;
            if (y2 < YMin) return;
            if (y1 < YMin) y1 = YMin;
            if (y2 > YMax) y2 = YMax;
            base.BlendVL(x, y1, y2, c, cover);
        }

        public override void BlendSolidHSpan(int x, int y, int len, Color c, byte[] covers, int coversIndex)
        {
#if false
            FileStream file = new FileStream("pixels.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.Write("h-x=" + x.ToString() + ",y=" + y.ToString() + ",len=" + len.ToString() + "\n");
            sw.Close();
            file.Close();
#endif

            if (y > YMax) return;
            if (y < YMin) return;
            if (x < XMin)
            {
                len -= XMin - x;
                if (len <= 0) return;
                coversIndex += XMin - x;
                x = XMin;
            }
            if (x + len > XMax)
            {
                len = XMax - x + 1;
                if (len <= 0) return;
            }
            base.BlendSolidHSpan(x, y, len, c, covers, coversIndex);
        }

        public override void BlendSolidVSpan(int x, int y, int len, Color c, byte[] covers, int coversIndex)
        {
#if false
            FileStream file = new FileStream("pixels.txt", FileMode.Append, FileAccess.Write);
            StreamWriter sw = new StreamWriter(file);
            sw.Write("v-x=" + x.ToString() + ",y=" + y.ToString() + ",len=" + len.ToString() + "\n");
            sw.Close();
            file.Close();
#endif

            if (x > XMax) return;
            if (x < XMin) return;
            if (y < YMin)
            {
                len -= (YMin - y);
                if (len <= 0) return;
                coversIndex += YMin - y;
                y = YMin;
            }
            if (y + len > YMax)
            {
                len = (YMax - y + 1);
                if (len <= 0) return;
            }
            base.BlendSolidVSpan(x, y, len, c, covers, coversIndex);
        }

        public override void CopyColorHSpan(int x, int y, int len, Color[] colors, int colorsIndex)
        {
            if (y > YMax) return;
            if (y < YMin) return;
            if (x < XMin)
            {
                int d = XMin - x;
                len -= d;
                if (len <= 0) return;
                colorsIndex += d;
                x = XMin;
            }
            if (x + len > XMax)
            {
                len = (XMax - x + 1);
                if (len <= 0) return;
            }
            base.CopyColorHSpan(x, y, len, colors, colorsIndex);
        }

        public override void CopyColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex)
        {
            if (x > XMax) return;
            if (x < XMin) return;
            if (y < YMin)
            {
                int d = YMin - y;
                len -= d;
                if (len <= 0) return;
                colorsIndex += d;
                y = YMin;
            }
            if (y + len > YMax)
            {
                len = (YMax - y + 1);
                if (len <= 0) return;
            }
            base.CopyColorVSpan(x, y, len, colors, colorsIndex);
        }

        public override void BlendColorHSpan(int x, int y, int in_len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            int len = (int)in_len;
            if (y > YMax)
                return;
            if (y < YMin)
                return;
            if (x < XMin)
            {
                int d = XMin - x;
                len -= d;
                if (len <= 0) return;
                if (covers != null) coversIndex += d;
                colorsIndex += d;
                x = XMin;
            }
            if (x + len - 1 > XMax)
            {
                len = XMax - x + 1;
                if (len <= 0) return;
            }

            base.BlendColorHSpan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
        }



        public override void SetPixel(int x, int y, Color color)
        {
            if ((uint)x < Width && (uint)y < Height)
            {
                base.SetPixel(x, y, color);
            }
        }

        public override void CopyFrom(IBitmapSrc sourceImage,
                       RectInt sourceImageRect,
                       int destXOffset,
                       int destYOffset)
        {
            RectInt destRect = sourceImageRect;
            destRect.Offset(destXOffset, destYOffset);
            RectInt clippedSourceRect = new RectInt();
            if (clippedSourceRect.IntersectRectangles(destRect, _clippingRect))
            {
                // move it back relative to the source
                clippedSourceRect.Offset(-destXOffset, -destYOffset);
                base.CopyFrom(sourceImage, clippedSourceRect, destXOffset, destYOffset);
            }
        }


        public override void BlendColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            if (x > XMax) return;
            if (x < XMin) return;
            if (y < YMin)
            {
                int d = YMin - y;
                len -= d;
                if (len <= 0) return;
                if (covers != null) coversIndex += d;
                colorsIndex += d;
                y = YMin;
            }
            if (y + len > YMax)
            {
                len = (YMax - y + 1);
                if (len <= 0) return;
            }
            base.BlendColorVSpan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
        }

        //public void reset_clipping(bool visibility)
        //{
        //    if (visibility)
        //    {
        //        m_ClippingRect.Left = 0;
        //        m_ClippingRect.Bottom = 0;
        //        m_ClippingRect.Right = (int)Width - 1;
        //        m_ClippingRect.Top = (int)Height - 1;
        //    }
        //    else
        //    {
        //        m_ClippingRect.Left = 1;
        //        m_ClippingRect.Bottom = 1;
        //        m_ClippingRect.Right = 0;
        //        m_ClippingRect.Top = 0;
        //    }
        //} 
    }
}
