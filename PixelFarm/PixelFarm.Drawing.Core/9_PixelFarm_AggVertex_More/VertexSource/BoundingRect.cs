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
//
// bounding_rect function template
//
//----------------------------------------------------------------------------


namespace PixelFarm.Agg
{
    public static class BoundingRect
    {
        public static bool GetBoundingRect(VertexStore vxs, int[] gi,
                           int num,
                           out RectD boundingRect)
        {
            return GetBoundingRect(vxs, gi, num, out boundingRect.Left, out boundingRect.Bottom, out boundingRect.Right, out boundingRect.Top);
        }
        public static bool GetBoundingRect(VertexStoreSnap vs, ref RectD rect)
        {
            double x1, y1, x2, y2;
            bool rValue = GetBoundingRectSingle(vs, out x1, out y1, out x2, out y2);
            rect.Left = x1;
            rect.Bottom = y1;
            rect.Right = x2;
            rect.Top = y2;
            return rValue;
        }

        //----------------------------------
        static bool GetBoundingRect(VertexStore vxs, int[] gi,
                         int num,
                         out double x1,
                         out double y1,
                         out double x2,
                         out double y2)
        {
            int i;
            double x = 0;
            double y = 0;
            bool first = true;
            x1 = 1;
            y1 = 1;
            x2 = 0;
            y2 = 0;
            int iterindex = 0;
            for (i = 0; i < num; i++)
            {
                VertexCmd flags;
                while ((flags = vxs.GetVertex(iterindex++, out x, out y)) != VertexCmd.NoMore)
                {
                    switch (flags)
                    {
                        //if is vertext cmd
                        case VertexCmd.LineTo:
                        case VertexCmd.MoveTo:
                        case VertexCmd.P2c:
                        case VertexCmd.P3c:
                            {
                                if (first)
                                {
                                    x1 = x;
                                    y1 = y;
                                    x2 = x;
                                    y2 = y;
                                    first = false;
                                }
                                else
                                {
                                    if (x < x1) x1 = x;
                                    if (y < y1) y1 = y;
                                    if (x > x2) x2 = x;
                                    if (y > y2) y2 = y;
                                }
                            }
                            break;
                    }
                }
            }
            return x1 <= x2 && y1 <= y2;
        }

        //-----------------------------------------------------bounding_rect_single
        //template<class VertexSource, class CoordT> 
        static bool GetBoundingRectSingle(
          VertexStoreSnap vs,
          out double x1, out double y1,
          out double x2, out double y2)
        {
            double x = 0;
            double y = 0;
            bool first = true;
            x1 = 1;
            y1 = 1;
            x2 = 0;
            y2 = 0;
            var vsnapIter = vs.GetVertexSnapIter();
            VertexCmd PathAndFlags;
            while (!VertexHelper.IsEmpty(PathAndFlags = vsnapIter.GetNextVertex(out x, out y)))
            {
                if (VertexHelper.IsVertextCommand(PathAndFlags))
                {
                    if (first)
                    {
                        x1 = x;
                        y1 = y;
                        x2 = x;
                        y2 = y;
                        first = false;
                    }
                    else
                    {
                        if (x < x1) x1 = x;
                        if (y < y1) y1 = y;
                        if (x > x2) x2 = x;
                        if (y > y2) y2 = y;
                    }
                }
            }
            return x1 <= x2 && y1 <= y2;
        }
    }


    //----------------------------------------------------
    public static class BoundingRectInt
    {
        public static bool GetBoundingRect(VertexStoreSnap vs, ref RectInt rect)
        {
            int x1, y1, x2, y2;
            bool rValue = GetBoundingRect(vs, out x1, out y1, out x2, out y2);
            rect.Left = x1;
            rect.Bottom = y1;
            rect.Right = x2;
            rect.Top = y2;
            return rValue;
        }
        public static RectInt GetBoundingRect(VertexStoreSnap vs)
        {
            int x1, y1, x2, y2;
            bool rValue = GetBoundingRect(vs, out x1, out y1, out x2, out y2);
            return new RectInt(x1, y1, x2, y2);
        }
        public static RectInt GetBoundingRect(VertexStore vxs)
        {
            int x1, y1, x2, y2;
            bool rValue = GetBoundingRect(new VertexStoreSnap(vxs), out x1, out y1, out x2, out y2);
            return new RectInt(x1, y1, x2, y2);
        }


        //-----------------------------------------------------bounding_rect_single
        //template<class VertexSource, class CoordT> 
        static bool GetBoundingRect(
          VertexStoreSnap vs,
          out int x1, out int y1,
          out int x2, out int y2)
        {
            double x_d = 0;
            double y_d = 0;
            int x = 0;
            int y = 0;
            bool first = true;
            x1 = 1;
            y1 = 1;
            x2 = 0;
            y2 = 0;
            var vsnapIter = vs.GetVertexSnapIter();
            VertexCmd PathAndFlags;
            while (!VertexHelper.IsEmpty(PathAndFlags = vsnapIter.GetNextVertex(out x_d, out y_d)))
            {
                x = (int)x_d;
                y = (int)y_d;
                if (VertexHelper.IsVertextCommand(PathAndFlags))
                {
                    if (first)
                    {
                        x1 = x;
                        y1 = y;
                        x2 = x;
                        y2 = y;
                        first = false;
                    }
                    else
                    {
                        if (x < x1) x1 = x;
                        if (y < y1) y1 = y;
                        if (x > x2) x2 = x;
                        if (y > y2) y2 = y;
                    }
                }
            }
            return x1 <= x2 && y1 <= y2;
        }
    }
}

//#endif
