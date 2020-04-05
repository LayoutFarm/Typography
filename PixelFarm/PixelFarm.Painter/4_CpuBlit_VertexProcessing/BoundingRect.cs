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
// bounding_rect function template
//
//----------------------------------------------------------------------------

using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    
    public static class BoundingRect
    {

        public static RectD GetBoundingRect(this VertexStore vxs)
        {
            RectD bounds = RectD.ZeroIntersection;
            return GetBoundingRect(vxs, ref bounds) ?
                        bounds :
                        new RectD();
        }
        public static bool GetBoundingRect(this VertexStore vxs, ref RectD rect)
        {
            bool rValue = GetBoundingRectSingle(vxs,
                out double x1, out double y1,
                out double x2, out double y2);

            if (x1 < rect.Left)
            {
                rect.Left = x1;
            }
            if (y1 < rect.Bottom)
            {
                rect.Bottom = y1;
            }
            if (x2 > rect.Right)
            {
                rect.Right = x2;
            }

            if (y2 > rect.Top)
            {
                rect.Top = y2;
            }

            return rValue;
        }
         
        //-----------------------------------------------------bounding_rect_single
        //template<class VertexSource, class CoordT> 
        static bool GetBoundingRectSingle(
          VertexStore vxs,
          out double x1, out double y1,
          out double x2, out double y2)
        {

            x1 = double.MaxValue;
            y1 = double.MaxValue;
            x2 = double.MinValue;
            y2 = double.MinValue;

            int index = 0;
            VertexCmd cmd;

            for (; ; )
            {
                cmd = vxs.GetVertex(index++, out double x, out double y);
                switch (cmd)
                {
                    case VertexCmd.Close:
                        //in this case we don't include that x,y
                        break;
                    case VertexCmd.NoMore:
                        goto EXIT_LOOP;
                    default:
                        if (x < x1) x1 = x;
                        if (y < y1) y1 = y;
                        if (x > x2) x2 = x;
                        if (y > y2) y2 = y;
                        break;
                }
            }
        EXIT_LOOP:

            return x1 <= x2 && y1 <= y2;
        }
    }


    //----------------------------------------------------
    public static class BoundingRectInt
    {
        public static void GetBoundingRect(this VertexStore vxs, ref RectInt rect)
        {
            RectD rect1 = new RectD();
            BoundingRect.GetBoundingRect(vxs, ref rect1);
            rect.Left = (int)System.Math.Round(rect1.Left);
            rect.Bottom = (int)System.Math.Round(rect1.Bottom);
            rect.Right = (int)System.Math.Round(rect1.Right);
            rect.Top = (int)System.Math.Round(rect1.Top);
        }

    }
}
