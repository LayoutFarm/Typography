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

using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit
{



    public static class PainterExtensions
    {

        public static void Line(this Painter p, double x1, double y1, double x2, double y2, Color color)
        {
            Color prevColor = p.StrokeColor;
            p.StrokeColor = color;
            p.DrawLine(x1, y1, x2, y2);
            p.StrokeColor = prevColor;
        }
        public static void DrawRectangle(this Painter p, double left, double top, double width, double height, Color color)
        {
            Color prevColor = p.StrokeColor;
            p.StrokeColor = color;
            p.DrawRect(left, top, width, height);
            p.StrokeColor = prevColor;
        }
        public static void DrawCircle(this Painter p, double centerX, double centerY, double radius)
        {
            p.DrawEllipse(centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        public static void FillCircle(this Painter p, double centerX, double centerY, double radius)
        {
            p.FillEllipse(centerX - radius, centerY - radius, radius + radius, radius + radius);
        }
        public static void FillCircle(this Painter p, double x, double y, double radius, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;
            p.FillCircle(x, y, radius);
            p.FillColor = prevColor;
        }
        public static void FillRect(this Painter p, double left, double top, double width, double height, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;
            p.FillRect(left, top, width, height);
            p.FillColor = prevColor;
        }

        public static void Fill(this Painter p, VertexStoreSnap snap, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;
            p.Fill(snap);
            p.FillColor = prevColor;
        }
        public static void Fill(this Painter p, VertexStore vxs, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;
            p.Fill(vxs);
            p.FillColor = prevColor;
        }
        public static void Draw(this Painter p, VertexStore vxs, Color color)
        {
            Color prevColor = p.StrokeColor;
            p.StrokeColor = color;
            p.Draw(vxs);
            p.StrokeColor = prevColor;
        }
        public static void Draw(this Painter p, VertexStoreSnap vxs, Color color)
        {
            Color prevColor = p.StrokeColor;
            p.StrokeColor = color;
            p.Draw(vxs);
            p.StrokeColor = prevColor;
        }

    }


}