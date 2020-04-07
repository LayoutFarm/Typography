//MIT, 2016-present, WinterDev 

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

    public sealed class Tools
    {
        /// <summary>
        /// instance for extension methods
        /// </summary>
        public static readonly Tools More = new Tools();
        private Tools() { }

        public static TempContext<AggPainter> BorrowAggPainter(MemBitmap bmp, out AggPainter painter)
        {

            if (!Temp<AggPainter>.IsInit())
            {
                Temp<AggPainter>.SetNewHandler(
                    () => new AggPainter(new AggRenderSurface()),
                    p =>
                    {
                        p.RenderSurface.DetachDstBitmap();
                        p.Reset();
                    }
                    );
            }

            var tmpPainter = Temp<AggPainter>.Borrow(out painter);
            painter.RenderSurface.AttachDstBitmap(bmp);
            return tmpPainter;
        }
        public static TempContext<ShapeBuilder> BorrowShapeBuilder(out ShapeBuilder shapeBuilder)
        {
            if (!Temp<ShapeBuilder>.IsInit())
            {
                Temp<ShapeBuilder>.SetNewHandler(
                    () => new ShapeBuilder(),
                    f => f.Reset());
            }

            TempContext<ShapeBuilder> context = Temp<ShapeBuilder>.Borrow(out shapeBuilder);
            shapeBuilder.InitVxs();//make it ready-to-use
            return context;
        }
        public static TempContext<PathWriter> BorrowPathWriter(VertexStore vxs, out PathWriter pathWriter) => VectorToolBox.Borrow(vxs, out pathWriter);

        //TODO: add agressive inlining...
        public static TempContext<Arc> BorrowArc(out Arc arc) => VectorToolBox.Borrow(out arc);
        public static TempContext<Stroke> BorrowStroke(out Stroke stroke) => VectorToolBox.Borrow(out stroke);
        public static VxsContext1 BorrowVxs(out VertexStore vxs) => new VxsContext1(out vxs);
        public static VxsContext2 BorrowVxs(out VertexStore vxs1, out VertexStore vxs2) => new VxsContext2(out vxs1, out vxs2);
        public static VxsContext3 BorrowVxs(out VertexStore vxs1, out VertexStore vxs2, out VertexStore vxs3) => new VxsContext3(out vxs1, out vxs2, out vxs3);

        public static TempContext<Ellipse> BorrowEllipse(out Ellipse ellipse)
        {
            if (!Temp<Ellipse>.IsInit())
            {
                Temp<Ellipse>.SetNewHandler(() => new Ellipse());
            }
            return Temp<Ellipse>.Borrow(out ellipse);
        }
        public static TempContext<RoundedRect> BorrowRoundedRect(out RoundedRect roundRect)
        {
            if (!Temp<RoundedRect>.IsInit())
            {
                Temp<RoundedRect>.SetNewHandler(() => new RoundedRect());
            }
            return Temp<RoundedRect>.Borrow(out roundRect);
        }

        public static TempContext<SimpleRect> BorrowRect(out SimpleRect simpleRect)
        {
            if (!Temp<SimpleRect>.IsInit())
            {
                Temp<SimpleRect>.SetNewHandler(() => new SimpleRect());
            }
            return Temp<SimpleRect>.Borrow(out simpleRect);
        }
        
        public static TempContext<Spiral> BorrowSpiral(out Spiral spiral)
        {
            if (!Temp<Spiral>.IsInit())
            {
                Temp<Spiral>.SetNewHandler(() => new Spiral());
            }
            return Temp<Spiral>.Borrow(out spiral);
        }
        public static TempContext<CurveFlattener> BorrowCurveFlattener(out CurveFlattener flattener)
        {
            if (!Temp<CurveFlattener>.IsInit())
            {
                Temp<CurveFlattener>.SetNewHandler(
                    () => new CurveFlattener(),
                    f => f.Reset());
            }
            return Temp<CurveFlattener>.Borrow(out flattener);
        }
        public static TempContext<PolygonSimplifier> BorrowPolygonSimplifier(out PolygonSimplifier flattener)
        {
            if (!Temp<PolygonSimplifier>.IsInit())
            {
                Temp<PolygonSimplifier>.SetNewHandler(
                    () => new PolygonSimplifier(),
                    f => f.Reset());
            }
            return Temp<PolygonSimplifier>.Borrow(out flattener);
        }

    }

    public class ShapeBuilder
    {
        VertexStore _vxs;
        public void Reset()
        {
            if (_vxs != null)
            {
                VxsTemp.ReleaseVxs(_vxs);
                _vxs = null;
            }
        }
        public ShapeBuilder InitVxs()
        {
            Reset();
            VxsTemp.Borrow(out _vxs);
            return this;
        }
        public ShapeBuilder InitVxs(VertexStore src)
        {
            Reset();
            VxsTemp.Borrow(out _vxs);
            _vxs.AppendVertexStore(src);
            return this;
        }
        public ShapeBuilder MoveTo(double x0, double y0)
        {
            _vxs.AddMoveTo(x0, y0);
            return this;
        }
        public ShapeBuilder LineTo(double x1, double y1)
        {
            _vxs.AddLineTo(x1, y1);
            return this;
        }
        public ShapeBuilder CloseFigure()
        {
            _vxs.AddCloseFigure();
            return this;
        }
        public ShapeBuilder Scale(float s)
        {
            VxsTemp.Borrow(out VertexStore v2);
            Affine aff = Affine.NewScaling(s, s);
            aff.TransformToVxs(_vxs, v2);

            //release _vxs
            VxsTemp.ReleaseVxs(_vxs);
            _vxs = v2;
            return this;
        }
        public ShapeBuilder Stroke(Stroke stroke)
        {
            VxsTemp.Borrow(out VertexStore v2);
            stroke.MakeVxs(_vxs, v2);
            VxsTemp.ReleaseVxs(_vxs);
            _vxs = v2;
            return this;
        }
        public ShapeBuilder Stroke(float width)
        {
            VxsTemp.Borrow(out VertexStore v2);
            using (VectorToolBox.Borrow(out Stroke stroke))
            {
                stroke.Width = width;
                stroke.MakeVxs(_vxs, v2);
            }
            VxsTemp.ReleaseVxs(_vxs);
            _vxs = v2;
            return this;
        }
        public ShapeBuilder Curve4To(
            double x1, double y1,
            double x2, double y2,
            double x3, double y3)
        {
            _vxs.AddVertex(x1, y1, VertexCmd.C4);
            _vxs.AddVertex(x2, y2, VertexCmd.C4);
            _vxs.AddVertex(x3, y3, VertexCmd.LineTo);
            return this;
        }
        public ShapeBuilder Curve3To(
           double x1, double y1,
           double x2, double y2)
        {
            _vxs.AddVertex(x1, y1, VertexCmd.C3);
            _vxs.AddVertex(x2, y2, VertexCmd.LineTo);
            return this;
        }
        public ShapeBuilder NoMore()
        {
            _vxs.AddNoMore();
            return this;
        }
        public VertexStore CreateTrim()
        {
            return _vxs.CreateTrim();
        }

        public ShapeBuilder TranslateToNewVxs(double dx, double dy)
        {
            VxsTemp.Borrow(out VertexStore v2);
            int count = _vxs.Count;
            VertexCmd cmd;
            for (int i = 0; i < count; ++i)
            {
                cmd = _vxs.GetVertex(i, out double x, out double y);
                x += dx;
                y += dy;
                v2.AddVertex(x, y, cmd);
            }
            VxsTemp.ReleaseVxs(_vxs);
            _vxs = v2;
            return this;
        }
        public ShapeBuilder Flatten(CurveFlattener flattener)
        {
            VxsTemp.Borrow(out VertexStore v2);
            flattener.MakeVxs(_vxs, v2);
            VxsTemp.ReleaseVxs(_vxs);
            _vxs = v2;
            return this;
        }
        /// <summary>
        /// flatten with default setting
        /// </summary>
        /// <returns></returns>
        public ShapeBuilder Flatten()
        {
            using (Tools.BorrowCurveFlattener(out var flattener))
            {
                return Flatten(flattener);
            }
        }
        public VertexStore CurrentSharedVxs => _vxs;
    }

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
        public static void Fill(this Painter p, Region rgn, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;
            p.Fill(rgn);
            p.FillColor = prevColor;
        }

        /// <summary>
        /// create stroke-vxs from a given vxs, and fill stroke-vxs with input color
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vxs"></param>
        /// <param name="strokeW"></param>
        /// <param name="color"></param>
        public static void FillStroke(this Painter p, VertexStore vxs, float strokeW, Color color)
        {
            Color prevColor = p.FillColor;
            p.FillColor = color;

            using (Tools.BorrowStroke(out var s))
            using (Tools.BorrowVxs(out var v1))
            {
                s.Width = strokeW;
                s.MakeVxs(vxs, v1);
                p.Fill(v1);
            }

            p.FillColor = prevColor;
        }

#if DEBUG
        static int dbugId = 0;
#endif


    }

    public static class AggRenderSurfaceExtensions
    {

        public static void Rectangle(this AggRenderSurface gx, double left, double bottom, double right, double top, Color color, double strokeWidth = 1)
        {

            using (Tools.BorrowStroke(out var stroke))
            using (Tools.BorrowRect(out var rect))
            using (Tools.BorrowVxs(out var v1, out var v2))
            {
                stroke.Width = strokeWidth;
                rect.SetRect(left + .5, bottom + .5, right - .5, top - .5);
                gx.Render(stroke.MakeVxs(rect.MakeVxs(v1), v2), color);
            }

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

            using (Tools.BorrowRect(out var rect))
            using (Tools.BorrowVxs(out var v1))
            {
                rect.SetRect(left, bottom, right, top);
                gx.Render(rect.MakeVxs(v1), fillColor);
            }

        }
        public static void Circle(this AggRenderSurface g, double x, double y, double radius, Color color)
        {
            using (Tools.BorrowEllipse(out var ellipse))
            using (Tools.BorrowVxs(out var v1))
            {
                ellipse.Set(x, y, radius, radius);
                g.Render(ellipse.MakeVxs(v1), color);
            }

        }
        public static void Circle(this AggRenderSurface g, Vector2 origin, double radius, Color color)
        {
            Circle(g, origin.x, origin.y, radius, color);
        }



    }

}

