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


        static Stroke stroke = new Stroke(1);
        static RoundedRect roundRect = new RoundedRect();
        static SimpleRect simpleRect = new SimpleRect();
        static Ellipse ellipse = new Ellipse();

        //static SvgFontStore svgFontStore = new SvgFontStore();
        //public static void DrawString(this ImageGraphics2D gx,
        //    string text,
        //    double x,
        //    double y,
        //    double pointSize = 12,
        //    Justification justification = Justification.Left,
        //    Baseline baseline = Baseline.Text,
        //    Color color = new Color(),
        //    bool drawFromHintedCache = false,
        //    Color backgroundColor = new Color())
        //{
        //    ////use svg font 
        //    var svgFont = svgFontStore.LoadFont(SvgFontStore.DEFAULT_SVG_FONTNAME, (int)pointSize);
        //    //TODO: review here
        //    //stringPrinter on each platform may not interchangeable ***
        //    if (stringPrinter == null)
        //    {
        //        stringPrinter = new MyTypeFacePrinter(gx.GfxPlatform);

        //    }

        //    stringPrinter.CurrentActualFont = svgFont;
        //    stringPrinter.DrawFromHintedCache = false;
        //    stringPrinter.LoadText(text);
        //    VertexStore vxs = stringPrinter.MakeVxs();
        //    vxs = Affine.NewTranslation(x, y).TransformToVxs(vxs);
        //    gx.Render(vxs, Color.Black);
        //}

        public static void Rectangle(this AggRenderSurface gx, double left, double bottom, double right, double top, Color color, double strokeWidth = 1)
        {
            stroke.Width = strokeWidth;
            simpleRect.SetRect(left + .5, bottom + .5, right - .5, top - .5);


            VectorToolBox.GetFreeVxs(out VertexStore v1, out VertexStore v2);

            gx.Render(stroke.MakeVxs(simpleRect.MakeVxs(v1), v2), color);

            VectorToolBox.ReleaseVxs(ref v1, ref v2);

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

            simpleRect.SetRect(left, bottom, right, top);

            VectorToolBox.GetFreeVxs(out var v1);
            gx.Render(simpleRect.MakeVertexSnap(v1), fillColor);
            VectorToolBox.ReleaseVxs(ref v1);
        }
        public static void Circle(this AggRenderSurface g, double x, double y, double radius, Color color)
        {
            ellipse.Set(x, y, radius, radius);
            VectorToolBox.GetFreeVxs(out var v1);
            g.Render(ellipse.MakeVxs(v1), color);
            VectorToolBox.ReleaseVxs(ref v1);
        }
        public static void Circle(this AggRenderSurface g, Vector2 origin, double radius, Color color)
        {
            Circle(g, origin.x, origin.y, radius, color);
        }



    }
}

