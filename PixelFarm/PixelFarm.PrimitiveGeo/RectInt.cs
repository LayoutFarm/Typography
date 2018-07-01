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
namespace PixelFarm.CpuBlit
{
    //TODO: merge with Rectangle
    //beware: the ctor!!=> left,bottom, right, top
    public struct RectInt
    {
        public int Left, Bottom, Right, Top;
        public RectInt(int left, int bottom, int right, int top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }


        // This function assumes the rect is normalized
        public int Width
        {
            get
            {
                return Right - Left;
            }
        }

        // This function assumes the rect is normalized
        public int Height
        {
            get
            {
                return Top - Bottom;
            }
        }

        public void Normalize()
        {
            int t;
            if (Left > Right) { t = Left; Left = Right; Right = t; }
            if (Bottom > Top) { t = Bottom; Bottom = Top; Top = t; }
        }

        public void ExpandToInclude(RectInt rectToInclude)
        {
            if (Right < rectToInclude.Right) Right = rectToInclude.Right;
            if (Top < rectToInclude.Top) Top = rectToInclude.Top;
            if (Left > rectToInclude.Left) Left = rectToInclude.Left;
            if (Bottom > rectToInclude.Bottom) Bottom = rectToInclude.Bottom;
        }

        public bool Clip(RectInt r)
        {
            if (Right > r.Right) Right = r.Right;
            if (Top > r.Top) Top = r.Top;
            if (Left < r.Left) Left = r.Left;
            if (Bottom < r.Bottom) Bottom = r.Bottom;
            return Left <= Right && Bottom <= Top;
        }

        public bool is_valid()
        {
            return Left <= Right && Bottom <= Top;
        }

        public bool Contains(int x, int y)
        {
            return (x >= Left && x <= Right && y >= Bottom && y <= Top);
        }

        public bool IntersectRectangles(RectInt rectToCopy, RectInt rectToIntersectWith)
        {
            Left = rectToCopy.Left;
            Bottom = rectToCopy.Bottom;
            Right = rectToCopy.Right;
            Top = rectToCopy.Top;
            if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
            if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
            if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
            if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;
            if (Left < Right && Bottom < Top)
            {
                return true;
            }

            return false;
        }

        public bool IntersectWithRectangle(RectInt rectToIntersectWith)
        {
            if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
            if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
            if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
            if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;
            if (Left < Right && Bottom < Top)
            {
                return true;
            }

            return false;
        }

        public static bool DoIntersect(RectInt rect1, RectInt rect2)
        {
            int x1 = rect1.Left;
            int y1 = rect1.Bottom;
            int x2 = rect1.Right;
            int y2 = rect1.Top;
            if (x1 < rect2.Left) x1 = rect2.Left;
            if (y1 < rect2.Bottom) y1 = rect2.Bottom;
            if (x2 > rect2.Right) x2 = rect2.Right;
            if (y2 > rect2.Top) y2 = rect2.Top;
            if (x1 < x2 && y1 < y2)
            {
                return true;
            }

            return false;
        }


        //---------------------------------------------------------unite_rectangles
        public void unite_rectangles(RectInt r1, RectInt r2)
        {
            Left = r1.Left;
            Bottom = r1.Bottom;
            Right = r1.Right;
            Right = r1.Top;
            if (Right < r2.Right) Right = r2.Right;
            if (Top < r2.Top) Top = r2.Top;
            if (Left > r2.Left) Left = r2.Left;
            if (Bottom > r2.Bottom) Bottom = r2.Bottom;
        }

        public void Inflate(int inflateSize)
        {
            Left = Left - inflateSize;
            Bottom = Bottom - inflateSize;
            Right = Right + inflateSize;
            Top = Top + inflateSize;
        }

        public void Offset(int x, int y)
        {
            Left = Left + x;
            Bottom = Bottom + y;
            Right = Right + x;
            Top = Top + y;
        }

        public override int GetHashCode()
        {
            return new { x1 = Left, x2 = Right, y1 = Bottom, y2 = Top }.GetHashCode();
        }

        public static bool ClipRects(RectInt pBoundingRect, ref RectInt pSourceRect, ref RectInt pDestRect)
        {
            // clip off the top so we don't write into random memory
            if (pDestRect.Top < pBoundingRect.Top)
            {
                // This type of clipping only works when we aren't scaling an image...
                // If we are scaling an image, the source and dest sizes won't match
                if (pSourceRect.Height != pDestRect.Height)
                {
                    throw new Exception("source and dest rects must have the same height");
                }

                pSourceRect.Top += pBoundingRect.Top - pDestRect.Top;
                pDestRect.Top = pBoundingRect.Top;
                if (pDestRect.Top >= pDestRect.Bottom)
                {
                    return false;
                }
            }
            // clip off the bottom
            if (pDestRect.Bottom > pBoundingRect.Bottom)
            {
                // This type of clipping only works when we arenst scaling an image...
                // If we are scaling an image, the source and desst sizes won't match
                if (pSourceRect.Height != pDestRect.Height)
                {
                    throw new Exception("source and dest rects must have the same height");
                }

                pSourceRect.Bottom -= pDestRect.Bottom - pBoundingRect.Bottom;
                pDestRect.Bottom = pBoundingRect.Bottom;
                if (pDestRect.Bottom <= pDestRect.Top)
                {
                    return false;
                }
            }

            // clip off the left
            if (pDestRect.Left < pBoundingRect.Left)
            {
                // This type of clipping only works when we aren't scaling an image...
                // If we are scaling an image, the source and dest sizes won't match
                if (pSourceRect.Width != pDestRect.Width)
                {
                    throw new Exception("source and dest rects must have the same width");
                }

                pSourceRect.Left += pBoundingRect.Left - pDestRect.Left;
                pDestRect.Left = pBoundingRect.Left;
                if (pDestRect.Left >= pDestRect.Right)
                {
                    return false;
                }
            }
            // clip off the right
            if (pDestRect.Right > pBoundingRect.Right)
            {
                // This type of clipping only works when we aren't scaling an image...
                // If we are scaling an image, the source and dest sizes won't match
                if (pSourceRect.Width != pDestRect.Width)
                {
                    throw new Exception("source and dest rects must have the same width");
                }

                pSourceRect.Right -= pDestRect.Right - pBoundingRect.Right;
                pDestRect.Right = pBoundingRect.Right;
                if (pDestRect.Right <= pDestRect.Left)
                {
                    return false;
                }
            }

            return true;
        }


        //***************************************************************************************************************************************************
        public static bool ClipRect(RectInt pBoundingRect, ref RectInt pDestRect)
        {
            // clip off the top so we don't write into random memory
            if (pDestRect.Top < pBoundingRect.Top)
            {
                pDestRect.Top = pBoundingRect.Top;
                if (pDestRect.Top >= pDestRect.Bottom)
                {
                    return false;
                }
            }
            // clip off the bottom
            if (pDestRect.Bottom > pBoundingRect.Bottom)
            {
                pDestRect.Bottom = pBoundingRect.Bottom;
                if (pDestRect.Bottom <= pDestRect.Top)
                {
                    return false;
                }
            }

            // clip off the left
            if (pDestRect.Left < pBoundingRect.Left)
            {
                pDestRect.Left = pBoundingRect.Left;
                if (pDestRect.Left >= pDestRect.Right)
                {
                    return false;
                }
            }

            // clip off the right
            if (pDestRect.Right > pBoundingRect.Right)
            {
                pDestRect.Right = pBoundingRect.Right;
                if (pDestRect.Right <= pDestRect.Left)
                {
                    return false;
                }
            }

            return true;
        }


#if DEBUG
        public override string ToString()
        {
            return "L:" + this.Left + ",T:" + this.Top + ",R:" + this.Right + ",B:" + this.Bottom;
        }
#endif
    }
}
