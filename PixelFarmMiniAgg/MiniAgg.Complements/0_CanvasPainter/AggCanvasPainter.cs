// 2016 ,BSD, WinterDev

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
using PixelFarm.Drawing.Fonts;
using PixelFarm.Agg.Image;
using PixelFarm.Agg.VertexSource;
namespace PixelFarm.Agg
{
    public class AggCanvasPainter : CanvasPainter
    {
        ImageGraphics2D gx;
        Stroke stroke;
        Color fillColor;
        Color strokeColor;
        ScanlinePacked8 scline;
        ScanlineRasterizer sclineRas;
        ScanlineRasToDestBitmapRenderer sclineRasToBmp;
        FilterMan filterMan = new FilterMan();
        PixelFarm.Drawing.Font currentFont;
        //-------------
        //tools
        //-------------
        SimpleRect simpleRect = new SimpleRect();
        Ellipse ellipse = new Ellipse();
        PathWriter lines = new PathWriter();
        RoundedRect roundRect = null;
        MyImageReaderWriter sharedImageWriterReader = new MyImageReaderWriter();
        CurveFlattener curveFlattener;
        TextPrinter textPrinter;
        int ellipseGenNSteps = 10;
        SmoothingMode _smoothingMode;

        public AggCanvasPainter(ImageGraphics2D graphic2d)
        {
            this.gx = graphic2d;
            this.sclineRas = gx.ScanlineRasterizer;
            this.stroke = new Stroke(1);//default
            this.scline = graphic2d.ScanlinePacked8;
            this.sclineRasToBmp = graphic2d.ScanlineRasToDestBitmap;

            //tmp1:
            if (graphic2d.GfxPlatform != null)
            {
                this.textPrinter = new TextPrinter(graphic2d.GfxPlatform);
            }
        }
        public override void Clear(Color color)
        {
            gx.Clear(color);
        }

        public override SmoothingMode SmoothingMode
        {
            get
            {
                return _smoothingMode;
            }
            set
            {
                switch (_smoothingMode = value)
                {
                    case Drawing.SmoothingMode.HighQuality:
                    case Drawing.SmoothingMode.AntiAlias:
                        gx.UseSubPixelRendering = true;
                        break;
                    case Drawing.SmoothingMode.HighSpeed:
                    default:
                        gx.UseSubPixelRendering = false;
                        break;
                }
            }
        }
        public override RectInt ClipBox
        {
            get { return this.gx.GetClippingRect(); }
            set { this.gx.SetClippingRect(value); }
        }
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            this.gx.SetClippingRect(new RectInt(x1, y1, x2, y2));
        }


        /// <summary>
        /// draw circle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="radius"></param>
        /// <param name="color"></param>
        public override void FillCircle(double x, double y, double radius, Color color)
        {
            ellipse.Reset(x, y, radius, radius);
            gx.Render(ellipse.MakeVxs(), color);
        }
        public override void FillCircle(double x, double y, double radius)
        {
            ellipse.Reset(x, y, radius, radius);
            gx.Render(ellipse.MakeVxs(), this.fillColor);
        }

        public override void FillEllipse(double left, double bottom, double right, double top)
        {
            ellipse.Reset((left + right) * 0.5,
                          (bottom + top) * 0.5,
                          (right - left) * 0.5,
                          (top - bottom) * 0.5,
                           ellipseGenNSteps);
            gx.Render(ellipse.MakeVxs(), this.fillColor);
        }
        public override void Draw(VertexStoreSnap vxs)
        {
            this.Fill(vxs);
        }
        public override void DrawEllipse(double left, double bottom, double right, double top)
        {
            ellipse.Reset((left + right) * 0.5,
                         (bottom + top) * 0.5,
                         (right - left) * 0.5,
                         (top - bottom) * 0.5,
                          ellipseGenNSteps);
            gx.Render(stroke.MakeVxs(ellipse.MakeVxs()), this.fillColor);
        }

        /// <summary>
        /// draw line
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        public override void Line(double x1, double y1, double x2, double y2, Color color)
        {
            lines.Clear();
            lines.MoveTo(x1, y1);
            lines.LineTo(x2, y2);
            gx.Render(stroke.MakeVxs(lines.Vxs), color);
        }
        /// <summary>
        /// draw line
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        public override void Line(double x1, double y1, double x2, double y2)
        {
            lines.Clear();
            lines.MoveTo(x1, y1);
            lines.LineTo(x2, y2);
            gx.Render(stroke.MakeVxs(lines.Vxs), this.strokeColor);
        }
        public override double StrokeWidth
        {
            get { return this.stroke.Width; }
            set { this.stroke.Width = value; }
        }
        public override void Draw(VertexStore vxs)
        {
            gx.Render(stroke.MakeVxs(vxs), this.strokeColor);
        }

        /// <summary>
        /// draw rectangle
        /// </summary>
        /// <param name="left"></param>
        /// <param name="bottom"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="color"></param>
        /// <param name="strokeWidth"></param>
        public override void Rectangle(double left, double bottom, double right, double top, Color color)
        {
            simpleRect.SetRect(left + .5, bottom + .5, right - .5, top - .5);
            gx.Render(stroke.MakeVxs(simpleRect.MakeVxs()), color);
        }
        public override void Rectangle(double left, double bottom, double right, double top)
        {
            simpleRect.SetRect(left + .5, bottom + .5, right - .5, top - .5);
            gx.Render(stroke.MakeVxs(simpleRect.MakeVxs()), this.fillColor);
        }
        public override void FillRectangle(double left, double bottom, double right, double top, Color fillColor)
        {
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }
            simpleRect.SetRect(left, bottom, right, top);
            gx.Render(simpleRect.MakeVertexSnap(), fillColor);
        }
        public override void FillRectangle(double left, double bottom, double right, double top)
        {
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }
            simpleRect.SetRect(left, bottom, right, top);
            gx.Render(simpleRect.MakeVertexSnap(), this.fillColor);
        }
        public override void FillRectLBWH(double left, double bottom, double width, double height)
        {
            double right = left + width;
            double top = bottom + height;
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }
            simpleRect.SetRect(left, bottom, right, top);
            gx.Render(simpleRect.MakeVertexSnap(), this.fillColor);
        }
        public override void FillRoundRectangle(double left, double bottom, double right, double top, double radius)
        {
            if (roundRect == null)
            {
                roundRect = new RoundedRect(left, bottom, right, top, radius);
                roundRect.NormalizeRadius();
            }
            else
            {
                roundRect.SetRect(left, bottom, right, top);
                roundRect.SetRadius(radius);
                roundRect.NormalizeRadius();
            }
            this.Fill(roundRect.MakeVxs());
        }
        public override void DrawRoundRect(double left, double bottom, double right, double top, double radius)
        {
            if (roundRect == null)
            {
                roundRect = new RoundedRect(left, bottom, right, top, radius);
                roundRect.NormalizeRadius();
            }
            else
            {
                roundRect.SetRect(left, bottom, right, top);
                roundRect.SetRadius(radius);
                roundRect.NormalizeRadius();
            }
            this.Draw(roundRect.MakeVxs());
        }

        public override Drawing.Font CurrentFont
        {
            get
            {
                return this.currentFont;
            }
            set
            {
                this.currentFont = value;
                textPrinter.CurrentFont = value;
            }
        }
        public override void DrawString(
           string text,
           double x,
           double y)
        {
            textPrinter.Print(this, text.ToString(), x, y);
        }
        //-------------------------------------------------------
        public Graphics2D Graphics
        {
            get { return this.gx; }
        }
        /// <summary>
        /// fill vertex store
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="c"></param>
        public override void Fill(VertexStoreSnap snap)
        {
            sclineRas.AddPath(snap);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, fillColor);
        }
        public override void Fill(VertexStore vxs)
        {
            sclineRas.AddPath(vxs);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, fillColor);
        }
        public override bool UseSubPixelRendering
        {
            get { return sclineRasToBmp.ScanlineRenderMode == ScanlineRenderMode.SubPixelRendering; }
            set { this.sclineRasToBmp.ScanlineRenderMode = value ? ScanlineRenderMode.SubPixelRendering : ScanlineRenderMode.Default; }
        }
        public override Color FillColor
        {
            get { return fillColor; }
            set { this.fillColor = value; }
        }
        public override Color StrokeColor
        {
            get { return strokeColor; }
            set { this.strokeColor = value; }
        }
        public override void PaintSeries(VertexStore vxs, Color[] colors, int[] pathIndexs, int numPath)
        {
            sclineRasToBmp.RenderSolidAllPaths(this.gx.DestImage,
                this.sclineRas,
                this.scline,
                vxs,
                colors,
                pathIndexs,
                numPath);
        }
        public void Fill(VertexStore vxs, ISpanGenerator spanGen)
        {
            this.sclineRas.AddPath(vxs);
            sclineRasToBmp.RenderWithSpan(this.gx.DestImage, sclineRas, scline, spanGen);
        }
        public override void DrawImage(ActualImage actualImage, double x, double y)
        {
            this.sharedImageWriterReader.ReloadImage(actualImage);
            this.gx.Render(this.sharedImageWriterReader, x, y);
        }
        public override void DrawImage(ActualImage actualImage, params Transform.AffinePlan[] affinePlans)
        {
            this.sharedImageWriterReader.ReloadImage(actualImage);
            this.gx.Render(sharedImageWriterReader, affinePlans);
        }

        //----------------------
        /// <summary>
        /// do filter at specific area
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="area"></param>
        public override void DoFilterBlurStack(RectInt area, int r)
        {
            ChildImage img = new ChildImage(this.gx.DestImage, gx.PixelBlender,
                area.Left, area.Bottom, area.Right, area.Top);
            filterMan.DoStackBlur(img, r);
        }
        public override void DoFilterBlurRecursive(RectInt area, int r)
        {
            ChildImage img = new ChildImage(this.gx.DestImage, gx.PixelBlender,
                area.Left, area.Bottom, area.Right, area.Top);
            filterMan.DoRecursiveBlur(img, r);
        }
        //---------------- 
        public override void DrawBezierCurve(float startX, float startY, float endX, float endY,
           float controlX1, float controlY1,
           float controlX2, float controlY2)
        {
            VertexStore vxs = new VertexStore();
            PixelFarm.Agg.VertexSource.BezierCurve.CreateBezierVxs4(vxs,
                new PixelFarm.VectorMath.Vector2(startX, startY),
                new PixelFarm.VectorMath.Vector2(endX, endY),
                new PixelFarm.VectorMath.Vector2(controlX1, controlY1),
                new PixelFarm.VectorMath.Vector2(controlX2, controlY2));
            vxs = this.stroke.MakeVxs(vxs);
            sclineRas.Reset();
            sclineRas.AddPath(vxs);
            //sclineRasToBmp.DrawWithColor(sclineRas, sclinePack8, this.fillColor);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, this.strokeColor);
        }
        //---------------- 
        public override VertexStore FlattenCurves(VertexStore srcVxs)
        {
            if (curveFlattener == null) { curveFlattener = new CurveFlattener(); }
            return curveFlattener.MakeVxs(srcVxs);
        }
        //---------------- 
        public override int Width
        {
            get
            {
                return 800;
            }
        }
        public override int Height
        {
            get { return 600; }
        }
        public override RenderVx CreateRenderVx(VertexStoreSnap snap)
        {
            return new AggRenderVx(snap);
        }
        public override void DrawRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Draw(aggRenderVx.snap);
        }
        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            //fill with brush 
            if (brush is SolidBrush)
            {
                SolidBrush solidBrush = (SolidBrush)brush;
                var prevColor = this.fillColor;
                this.fillColor = solidBrush.Color;
                Fill(aggRenderVx.snap);
                this.fillColor = prevColor;
            }
            else
            {
                Fill(aggRenderVx.snap);
            }
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Fill(aggRenderVx.snap);
        }
    }
}