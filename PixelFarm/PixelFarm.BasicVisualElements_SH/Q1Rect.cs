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
namespace PixelFarm.CpuBlit.VertexProcessing
{

    //beware: the ctor!!=> left,bottom, right, top

    /// <summary>
    ///  Cartesian's Quadrant1 Rect, (x0,y0)=> (x1,y1) = (left,bottom)=> (right,top)
    /// </summary>
    public readonly struct Q1Rect
    {
        //(x0,y0)=> (x1,y1)
        public readonly int Left, Bottom, Right, Top;
        public Q1Rect(int left, int bottom, int right, int top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }
        public Q1Rect(int left, int bottom, int right, int top, bool withNormalizeCheck)
        {
            //Cartesian's Quadrant1 Rect 
            //bottom must <  top
            int t;
            if (left > right) { t = left; left = right; right = t; }
            if (bottom > top) { t = bottom; bottom = top; top = t; }
            //----------
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        public int Width => Right - Left; // This function assumes the rect is normalized

        public int Height => Top - Bottom; // This function assumes the rect is normalized

        /// <summary>
        /// create new rect from offset
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public Q1Rect CreateNewFromOffset(int dx, int dy) => new Q1Rect(Left + dx, Bottom + dy, Right + dx, Top + dy);
         

        public bool Contains(int x, int y) => (x >= Left && x <= Right && y >= Bottom && y <= Top);
        public static bool IntersectsWith(Q1Rect a, Q1Rect b)
        {
            int a_left = a.Left;
            int a_bottom = a.Bottom;
            int a_right = a.Right;
            int a_top = a.Top;

            if (a_left < b.Left) a_left = b.Left;
            if (a_bottom < b.Bottom) a_bottom = b.Bottom;
            if (a_right > b.Right) a_right = b.Right;
            if (a_top > b.Top) a_top = b.Top;

            return (a_left < a_right && a_bottom < a_top);
        }
        public static bool IntersectRectangles(Q1Rect a, Q1Rect b, out Q1Rect result)
        {
            int a_left = a.Left;
            int a_bottom = a.Bottom;
            int a_right = a.Right;
            int a_top = a.Top;

            if (a_left < b.Left) a_left = b.Left;
            if (a_bottom < b.Bottom) a_bottom = b.Bottom;
            if (a_right > b.Right) a_right = b.Right;
            if (a_top > b.Top) a_top = b.Top;

            if (a_left < a_right && a_bottom < a_top)
            {
                result = new Q1Rect(a_left, a_bottom, a_right, a_top);
                return true;
            }

            result = new Q1Rect();//empty
            return false;
        }

        public static bool Clip(Q1Rect a, Q1Rect b, out Q1Rect result)
        {
            int a_left = a.Left;
            int a_bottom = a.Bottom;
            int a_right = a.Right;
            int a_top = a.Top;

            if (a_left < b.Left) a_left = b.Left;
            if (a_bottom < b.Bottom) a_bottom = b.Bottom;
            if (a_right > b.Right) a_right = b.Right;
            if (a_top > b.Top) a_top = b.Top;

            if (a_left <= a_right && a_bottom <= a_top)
            {
                result = new Q1Rect(a_left, a_bottom, a_right, a_top);
                return true;
            }

            result = new Q1Rect();//empty
            return false;
        }


        //public bool Clip(in Q1Rect r)
        //{
        //    if (Right > r.Right) Right = r.Right;
        //    if (Top > r.Top) Top = r.Top;
        //    if (Left < r.Left) Left = r.Left;
        //    if (Bottom < r.Bottom) Bottom = r.Bottom;
        //    return Left <= Right && Bottom <= Top;
        //}

        //public bool IntersectRectangles(Q1Rect rectToCopy, Q1Rect rectToIntersectWith)
        //{
        //    Left = rectToCopy.Left;
        //    Bottom = rectToCopy.Bottom;
        //    Right = rectToCopy.Right;
        //    Top = rectToCopy.Top;
        //    if (Left < rectToIntersectWith.Left) Left = rectToIntersectWith.Left;
        //    if (Bottom < rectToIntersectWith.Bottom) Bottom = rectToIntersectWith.Bottom;
        //    if (Right > rectToIntersectWith.Right) Right = rectToIntersectWith.Right;
        //    if (Top > rectToIntersectWith.Top) Top = rectToIntersectWith.Top;
        //    if (Left < Right && Bottom < Top)
        //    {
        //        return true;
        //    }

        //    return false;
        //}

        //public void ExpandToInclude(Q1Rect rectToInclude)
        //{
        //    if (Right < rectToInclude.Right) Right = rectToInclude.Right;
        //    if (Top < rectToInclude.Top) Top = rectToInclude.Top;
        //    if (Left > rectToInclude.Left) Left = rectToInclude.Left;
        //    if (Bottom > rectToInclude.Bottom) Bottom = rectToInclude.Bottom;
        //}
        //public bool is_valid() => Left <= Right && Bottom <= Top;
        ////---------------------------------------------------------unite_rectangles
        //public void unite_rectangles(Q1Rect r1, Q1Rect r2)
        //{
        //    Left = r1.Left;
        //    Bottom = r1.Bottom;
        //    Right = r1.Right;
        //    Right = r1.Top;
        //    if (Right < r2.Right) Right = r2.Right;
        //    if (Top < r2.Top) Top = r2.Top;
        //    if (Left > r2.Left) Left = r2.Left;
        //    if (Bottom > r2.Bottom) Bottom = r2.Bottom;
        //}

        //public void Inflate(int inflateSize)
        //{
        //    Left = Left - inflateSize;
        //    Bottom = Bottom - inflateSize;
        //    Right = Right + inflateSize;
        //    Top = Top + inflateSize;
        //}



        //public override int GetHashCode()
        //{
        //    //TODO: review this again?
        //    return new { x1 = Left, x2 = Right, y1 = Bottom, y2 = Top }.GetHashCode();
        //}

        //public static bool ClipRects(Q1Rect pBoundingRect, ref Q1Rect src, ref Q1Rect dst)
        //{
        //    // clip off the top so we don't write into random memory
        //    if (dst.Top < pBoundingRect.Top)
        //    {
        //        // This type of clipping only works when we aren't scaling an image...
        //        // If we are scaling an image, the source and dest sizes won't match
        //        if (src.Height != dst.Height)
        //        {
        //            throw new Exception("source and dest rects must have the same height");
        //        }

        //        src.Top += pBoundingRect.Top - dst.Top;
        //        dst.Top = pBoundingRect.Top;
        //        if (dst.Top >= dst.Bottom)
        //        {
        //            return false;
        //        }
        //    }
        //    // clip off the bottom
        //    if (dst.Bottom > pBoundingRect.Bottom)
        //    {
        //        // This type of clipping only works when we arenst scaling an image...
        //        // If we are scaling an image, the source and desst sizes won't match
        //        if (src.Height != dst.Height)
        //        {
        //            throw new Exception("source and dest rects must have the same height");
        //        }

        //        src.Bottom -= dst.Bottom - pBoundingRect.Bottom;
        //        dst.Bottom = pBoundingRect.Bottom;
        //        if (dst.Bottom <= dst.Top)
        //        {
        //            return false;
        //        }
        //    }

        //    // clip off the left
        //    if (dst.Left < pBoundingRect.Left)
        //    {
        //        // This type of clipping only works when we aren't scaling an image...
        //        // If we are scaling an image, the source and dest sizes won't match
        //        if (src.Width != dst.Width)
        //        {
        //            throw new Exception("source and dest rects must have the same width");
        //        }

        //        src.Left += pBoundingRect.Left - dst.Left;
        //        dst.Left = pBoundingRect.Left;
        //        if (dst.Left >= dst.Right)
        //        {
        //            return false;
        //        }
        //    }
        //    // clip off the right
        //    if (dst.Right > pBoundingRect.Right)
        //    {
        //        // This type of clipping only works when we aren't scaling an image...
        //        // If we are scaling an image, the source and dest sizes won't match
        //        if (src.Width != dst.Width)
        //        {
        //            throw new Exception("source and dest rects must have the same width");
        //        }

        //        src.Right -= dst.Right - pBoundingRect.Right;
        //        dst.Right = pBoundingRect.Right;
        //        if (dst.Right <= dst.Left)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //public static bool ClipRect(Q1Rect pBoundingRect, ref Q1Rect pDestRect)
        //{
        //    // clip off the top so we don't write into random memory
        //    if (pDestRect.Top < pBoundingRect.Top)
        //    {
        //        pDestRect.Top = pBoundingRect.Top;
        //        if (pDestRect.Top >= pDestRect.Bottom)
        //        {
        //            return false;
        //        }
        //    }
        //    // clip off the bottom
        //    if (pDestRect.Bottom > pBoundingRect.Bottom)
        //    {
        //        pDestRect.Bottom = pBoundingRect.Bottom;
        //        if (pDestRect.Bottom <= pDestRect.Top)
        //        {
        //            return false;
        //        }
        //    }

        //    // clip off the left
        //    if (pDestRect.Left < pBoundingRect.Left)
        //    {
        //        pDestRect.Left = pBoundingRect.Left;
        //        if (pDestRect.Left >= pDestRect.Right)
        //        {
        //            return false;
        //        }
        //    }

        //    // clip off the right
        //    if (pDestRect.Right > pBoundingRect.Right)
        //    {
        //        pDestRect.Right = pBoundingRect.Right;
        //        if (pDestRect.Right <= pDestRect.Left)
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}


#if DEBUG
        public override string ToString()
        {
            return "L:" + this.Left + ",T:" + this.Top + ",R:" + this.Right + ",B:" + this.Bottom;
        }
#endif
    }
}
