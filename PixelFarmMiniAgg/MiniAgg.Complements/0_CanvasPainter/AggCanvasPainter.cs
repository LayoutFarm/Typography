//MIT, 2016-2017, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Fonts;
using PixelFarm.Agg.Imaging;
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
        /// <summary>
        /// scanline rasterizer to bitmap
        /// </summary>
        ScanlineRasToDestBitmapRenderer sclineRasToBmp;
        FilterMan filterMan = new FilterMan();
        RequestFont currentFont;
        //-------------
        //tools
        //-------------
        SimpleRect simpleRect = new SimpleRect();
        Ellipse ellipse = new Ellipse();
        PathWriter lines = new PathWriter();
        RoundedRect roundRect = null;
        MyImageReaderWriter sharedImageWriterReader = new MyImageReaderWriter();

        LineDashGenerator _lineDashGen;
        int ellipseGenNSteps = 10;
        SmoothingMode _smoothingMode;

        public AggCanvasPainter(ImageGraphics2D graphic2d)
        {
            this.gx = graphic2d;
            this.sclineRas = gx.ScanlineRasterizer;
            this.stroke = new Stroke(1);//default
            this.scline = graphic2d.ScanlinePacked8;
            this.sclineRasToBmp = graphic2d.ScanlineRasToDestBitmap;


        }
        public Graphics2D Graphics
        {
            get { return this.gx; }
        }
        public override int Width
        {
            get
            {
                //TODO: review here
                return 800;
            }
        }
        public override int Height
        {
            //TODO: review here
            get { return 600; }
        }
        public override void Clear(Color color)
        {
            gx.Clear(color);
        }
        public override float OriginX
        {
            get { return sclineRas.OffsetOriginX; }
        }
        public override float OriginY
        {
            get { return sclineRas.OffsetOriginY; }
        }
        public override void SetOrigin(float x, float y)
        {
            sclineRas.OffsetOriginX = x;
            sclineRas.OffsetOriginY = y;
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


        VertexStorePool _vxsPool = new VertexStorePool();

        VertexStore GetFreeVxs()
        {
            return _vxsPool.GetFreeVxs();
        }
        void ReleaseVxs(ref VertexStore vxs)
        {
            _vxsPool.Release(ref vxs);
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
            var v1 = GetFreeVxs();
            gx.Render(ellipse.MakeVxs(v1), color);
            ReleaseVxs(ref v1);
        }
        public override void FillCircle(double x, double y, double radius)
        {
            ellipse.Reset(x, y, radius, radius);
            var v1 = GetFreeVxs();
            gx.Render(ellipse.MakeVxs(v1), this.fillColor);
            ReleaseVxs(ref v1);
        }

        public override void FillEllipse(double left, double bottom, double right, double top)
        {
            ellipse.Reset((left + right) * 0.5,
                          (bottom + top) * 0.5,
                          (right - left) * 0.5,
                          (top - bottom) * 0.5,
                           ellipseGenNSteps);
            var v1 = GetFreeVxs();
            gx.Render(ellipse.MakeVxs(v1), this.fillColor);
            ReleaseVxs(ref v1);
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
            var v1 = GetFreeVxs();
            var v2 = GetFreeVxs();
            gx.Render(stroke.MakeVxs(ellipse.MakeVxs(v1), v2), this.fillColor);
            ReleaseVxs(ref v1);
            ReleaseVxs(ref v2);
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

            var v1 = GetFreeVxs();
            gx.Render(stroke.MakeVxs(lines.Vxs, v1), color);
            ReleaseVxs(ref v1);
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
            var v1 = GetFreeVxs();
            gx.Render(stroke.MakeVxs(lines.Vxs, v1), this.strokeColor);
            ReleaseVxs(ref v1);

        }
        public override double StrokeWidth
        {
            get { return this.stroke.Width; }
            set { this.stroke.Width = value; }
        }
        public override void Draw(VertexStore vxs)
        {
            if (_lineDashGen == null)
            {
                //no line dash
                var v1 = GetFreeVxs();
                gx.Render(stroke.MakeVxs(vxs, v1), this.strokeColor);
                ReleaseVxs(ref v1);
            }
            else
            {
                var v1 = GetFreeVxs();
                var v2 = GetFreeVxs();
                _lineDashGen.CreateDash(vxs, v1);
                stroke.MakeVxs(v1, v2);
                gx.Render(v2, this.strokeColor);

                ReleaseVxs(ref v1);
                ReleaseVxs(ref v2);
            }

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

            var v1 = GetFreeVxs();
            var v2 = GetFreeVxs();


            gx.Render(stroke.MakeVxs(simpleRect.MakeVxs(v1), v2), color);

            ReleaseVxs(ref v1);
            ReleaseVxs(ref v2);
        }
        public override void Rectangle(double left, double bottom, double right, double top)
        {
            simpleRect.SetRect(left + .5, bottom + .5, right - .5, top - .5);
            var v1 = GetFreeVxs();
            var v2 = GetFreeVxs();
            //
            gx.Render(stroke.MakeVxs(simpleRect.MakeVxs(v1), v2), this.fillColor);
            //
            ReleaseVxs(ref v1);
            ReleaseVxs(ref v2);
        }
        public override void FillRectangle(double left, double bottom, double right, double top, Color fillColor)
        {
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }
            simpleRect.SetRect(left, bottom, right, top);
            var v1 = GetFreeVxs();
            simpleRect.MakeVertexSnap(v1);
            gx.Render(v1, fillColor);
            ReleaseVxs(ref v1);
        }
        public override void FillRectangle(double left, double bottom, double right, double top)
        {
            if (right < left || top < bottom)
            {
                throw new ArgumentException();
            }
            simpleRect.SetRect(left, bottom, right, top);
            var v1 = GetFreeVxs();
            gx.Render(simpleRect.MakeVertexSnap(v1), this.fillColor);
            ReleaseVxs(ref v1);
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
            var v1 = GetFreeVxs();
            gx.Render(simpleRect.MakeVertexSnap(v1), this.fillColor);
            ReleaseVxs(ref v1);
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
            var v1 = GetFreeVxs();
            this.Fill(roundRect.MakeVxs(v1));
            ReleaseVxs(ref v1);
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
            var v1 = GetFreeVxs();
            this.Draw(roundRect.MakeVxs(v1));
            ReleaseVxs(ref v1);
        }

        public override RequestFont CurrentFont
        {
            get
            {
                return this.currentFont;
            }
            set
            {
                this.currentFont = value;
                //this request font must resolve to actual font
                //within canvas *** 
                //TODO: review drawing string  with agg here 
                if (_textPrinter != null && value != null)
                {
                    _textPrinter.ChangeFont(value);
                }
            }
        }

        public override void DrawString(
           string text,
           double x,
           double y)
        {
            //TODO: review drawing string  with agg here   
            if (_textPrinter != null)
            {
                _textPrinter.DrawString(text, x, y);
            }
        }
        public override void DrawString(RenderVxFormattedString renderVx, double x, double y)
        {
            //draw string from render vx
            if (_textPrinter != null)
            {
                _textPrinter.DrawString(renderVx, x, y);
            }
        }
        public override RenderVxFormattedString CreateRenderVx(string textspan)
        {

            var renderVxFmtStr = new AggRenderVxFormattedString(textspan);
            if (_textPrinter != null)
            {
                char[] buffer = textspan.ToCharArray();
                _textPrinter.PrepareStringForRenderVx(renderVxFmtStr, buffer, 0, buffer.Length);

            }
            return renderVxFmtStr;
        }

        ITextPrinter _textPrinter;
        public ITextPrinter TextPrinter
        {
            get
            {
                return _textPrinter;
            }
            set
            {
                _textPrinter = value;
                if (_textPrinter != null)
                {
                    _textPrinter.ChangeFont(this.currentFont);
                }
            }

        }
        /// <summary>
        /// fill vertex store, we do NOT store snap
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="c"></param>
        public override void Fill(VertexStoreSnap snap)
        {
            sclineRas.AddPath(snap);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, fillColor);
        }
        /// <summary>
        /// fill vxs, we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void Fill(VertexStore vxs)
        {
            sclineRas.AddPath(vxs);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, fillColor);
        }


        public override bool UseSubPixelRendering
        {
            get
            {
                return this.sclineRasToBmp.ScanlineRenderMode == ScanlineRenderMode.SubPixelRendering;
            }
            set
            {
                if (value)
                {
                    //TODO: review here again             
                    this.sclineRas.ExtendX3ForSubPixelRendering = true;
                    this.sclineRasToBmp.ScanlineRenderMode = ScanlineRenderMode.SubPixelRendering;
                }
                else
                {
                    this.sclineRas.ExtendX3ForSubPixelRendering = false;
                    this.sclineRasToBmp.ScanlineRenderMode = ScanlineRenderMode.Default;
                }
            }
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
        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
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
            var v1 = GetFreeVxs();
            PixelFarm.Agg.VertexSource.BezierCurve.CreateBezierVxs4(v1,
                new PixelFarm.VectorMath.Vector2(startX, startY),
                new PixelFarm.VectorMath.Vector2(endX, endY),
                new PixelFarm.VectorMath.Vector2(controlX1, controlY1),
                new PixelFarm.VectorMath.Vector2(controlX2, controlY2));
            //
            var v2 = this.stroke.MakeVxs(v1, GetFreeVxs());
            //
            sclineRas.Reset();
            sclineRas.AddPath(v2);
            sclineRasToBmp.RenderWithColor(this.gx.DestImage, sclineRas, scline, this.strokeColor);
            ReleaseVxs(ref v1);
            ReleaseVxs(ref v2);
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
        public LineJoin LineJoin
        {
            get { return stroke.LineJoin; }
            set
            {
                stroke.LineJoin = value;
            }
        }
        public LineCap LineCap
        {
            get { return stroke.LineCap; }
            set
            {
                stroke.LineCap = value;
            }
        }
         
        //--------------------------------------------------
        public LineDashGenerator LineDashGen
        {
            get { return this._lineDashGen; }
            set { this._lineDashGen = value; }
        }
        
    }
}