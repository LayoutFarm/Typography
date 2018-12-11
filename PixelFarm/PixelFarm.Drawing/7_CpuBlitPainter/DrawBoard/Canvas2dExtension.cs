//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007-2011
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
//
// Class StringPrinter.cs
// 
// Class to output the vertex source of a string as a run of glyphs.
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.VectorMath;

namespace PixelFarm.CpuBlit
{
    public static class Canvas2dExtension
    {
        //helper tools, run in render thread***
        //not thread safe ***


        [ThreadStatic]
        static object s_threadInit = null;
        [ThreadStatic]
        static Stroke s_stroke;
        [ThreadStatic]
        static RoundedRect s_roundRect;
        [ThreadStatic]
        static SimpleRect s_simpleRect;
        [ThreadStatic]
        static Ellipse s_ellipse;

        static void CheckInit()
        {
            if (s_threadInit == null)
            {
                s_threadInit = new object();
                s_stroke = new Stroke(1);
                s_roundRect = new RoundedRect();
                s_simpleRect = new SimpleRect();
                s_ellipse = new Ellipse();
            }
        }
        public static void Rectangle(this AggRenderSurface gx, double left, double bottom, double right, double top, Color color, double strokeWidth = 1)
        {

            if (s_threadInit == null) CheckInit();
            //------------------------------------
            s_stroke.Width = strokeWidth;
            s_simpleRect.SetRect(left + .5, bottom + .5, right - .5, top - .5);

            using (VxsTemp.Borrow(out var v1, out var v2))
            {
                gx.Render(s_stroke.MakeVxs(s_simpleRect.MakeVxs(v1), v2), color);
            }

        }
        public static void Rectangle(this AggRenderSurface gx, RectD rect, Color color, double strokeWidth = 1)
        {
            gx.Rectangle(rect.Left, rect.Bottom, rect.Right, rect.Top, color, strokeWidth);
        }

        public static void Rectangle(this AggRenderSurface gx, RectInt rect, Color color)
        {
            gx.Rectangle(rect.Left, rect.Bottom, rect.Right, rect.Top, color);
        }

        public static void FillRectangle(this AggRenderSurface gx, RectD rect, Color fillColor)
        {
            gx.FillRectangle(rect.Left, rect.Bottom, rect.Right, rect.Top, fillColor);
        }

        public static void FillRectangle(this AggRenderSurface gx, RectInt rect, Color fillColor)
        {
            gx.FillRectangle(rect.Left, rect.Bottom, rect.Right, rect.Top, fillColor);
        }

        public static void FillRectangle(this AggRenderSurface gx,
            Vector2 leftBottom,
            Vector2 rightTop, Color fillColor)
        {
            gx.FillRectangle(leftBottom.x, leftBottom.y, rightTop.x, rightTop.y, fillColor);
        }

        public static void FillRectangle(this AggRenderSurface gx, double left,
            double bottom, double right, double top, Color fillColor)
        {
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }


            if (s_threadInit == null) CheckInit();
            //------------------------------------

            s_simpleRect.SetRect(left, bottom, right, top);
            using (VxsTemp.Borrow(out var v1))
            {
                gx.Render(s_simpleRect.MakeVxs(v1), fillColor);
            }

        }
        public static void Circle(this AggRenderSurface g, double x, double y, double radius, Color color)
        {
            if (s_threadInit == null) CheckInit();
            //------------------------------------

            s_ellipse.Set(x, y, radius, radius);
            using (VxsTemp.Borrow(out var v1))
            {
                g.Render(s_ellipse.MakeVxs(v1), color);
            }

        }
        public static void Circle(this AggRenderSurface g, Vector2 origin, double radius, Color color)
        {
            Circle(g, origin.x, origin.y, radius, color);
        }



    }
}

