//BSD, 2014-2016, WinterDev

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
namespace PixelFarm.Agg
{
    public abstract class CanvasPainter
    {

        
        public abstract int Width { get; }
        public abstract int Height { get; }
        public abstract RectInt ClipBox { get; set; }
        public abstract void SetClipBox(int x1, int y1, int x2, int y2);
        //-------------------------------------------------------
        public abstract double StrokeWidth { get; set; }
        public abstract SmoothingMode SmoothingMode { get; set; }
        public abstract bool UseSubPixelRendering { get; set; }
        public abstract Color FillColor { get; set; }
        public abstract Color StrokeColor { get; set; }
        //-------------------------------------------------------
        public abstract void Clear(Color color);                
        public abstract void FillCircle(double x, double y, double radius, Color color);
        public abstract void FillCircle(double x, double y, double radius);
        public abstract void FillEllipse(double left, double bottom, double right, double top);
        public abstract void DrawEllipse(double left, double bottom, double right, double top);

        public abstract void Line(double x1, double y1, double x2, double y2, Color color);
        public abstract void Line(double x1, double y1, double x2, double y2);
        public abstract void Rectangle(double left, double bottom, double right, double top, Color color);
        public abstract void Rectangle(double left, double bottom, double right, double top);
        public abstract void FillRectangle(double left, double bottom, double right, double top, Color fillColor);
        public abstract void FillRectangle(double left, double bottom, double right, double top);
        public abstract void FillRectLBWH(double left, double bottom, double width, double height);
        public abstract void FillRoundRectangle(double left, double bottom, double right, double top, double radius);
        public abstract void DrawRoundRect(double left, double bottom, double right, double top, double radius);
        public abstract void DrawBezierCurve(float startX, float startY, float endX, float endY,
         float controlX1, float controlY1,
         float controlX2, float controlY2);
        //-------------------------------------------------------


       

        public abstract void DrawImage(ActualImage actualImage, double x, double y);
        public abstract void DrawImage(ActualImage actualImage, params Transform.AffinePlan[] affinePlans); 
        public abstract void DoFilterBlurStack(RectInt area, int r);
        public abstract void DoFilterBlurRecursive(RectInt area, int r);
        //-------------------------------------------------------
      

        ////////////////////////////////////////////////////////////////////////////
        //vertext store/snap/rendervx
        public abstract void Fill(VertexStoreSnap snap);
        public abstract void Fill(VertexStore vxs);
        public abstract void PaintSeries(VertexStore vxs, Color[] colors, int[] pathIndexs, int numPath);
        public abstract void Draw(VertexStore vxs);
        public abstract void Draw(VertexStoreSnap vxs);
        public abstract RenderVx CreateRenderVx(VertexStoreSnap snap);
        public abstract void FillRenderVx(Brush brush, RenderVx renderVx);
        public abstract void FillRenderVx(RenderVx renderVx);
        public abstract void DrawRenderVx(RenderVx renderVx);
        ////////////////////////////////////////////////////////////////////////////
        //text,string
        //TODO: review text drawing funcs 
        public abstract RequestFont CurrentFont { get; set; }
        public abstract void DrawString(
           string text,
           double x,
           double y);



    }

}