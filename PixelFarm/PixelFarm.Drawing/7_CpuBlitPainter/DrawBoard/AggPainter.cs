//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.Imaging;
using PixelFarm.CpuBlit.FragmentProcessing;
using PixelFarm.CpuBlit.PixelProcessing;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{




    public class AggPainter : Painter
    {


        public enum LineRenderingTechnique
        {
            StrokeVxsGenerator,
            OutlineAARenderer,
        }

        AggRenderSurface _aggsx; //target rendering surface  
        BitmapBuffer _bxt;
        //--------------------
        AggRenderSurface _aggsx_0; //primary render surface
        AggRenderSurface _aggsx_mask;
        //--------------------
        //low-level outline renderer
        LineRenderingTechnique _lineRenderingTech;
        Rasterization.Lines.LineProfileAnitAlias _lineProfileAA;
        Rasterization.Lines.OutlineAARasterizer _outlineRas; //low-level outline aa

        //--------------------
        //pen 
        Stroke _stroke;
        Color _strokeColor;
        //--------------------
        //brush
        Color _fillColor;
        //--------------------
        //image processing,
        FilterMan _filterMan = new FilterMan();
        //font
        RequestFont _currentFont;
        //-------------
        //vector generators for various object



        LineDashGenerator _lineDashGen;
        int _ellipseGenNSteps = 20;
        SmoothingMode _smoothingMode;
        Brush _curBrush;
        Pen _curPen;

        //
        PixelBlender32 _defaultPixelBlender;
        PixelBlenderWithMask _maskPixelBlender;
        PixelBlenderPerColorComponentWithMask _maskPixelBlenderPerCompo;


        bool _useDefaultBrush;
        //
        public AggPainter(AggRenderSurface aggsx)
        {
            //painter paint to target surface
            _orientation = RenderSurfaceOrientation.LeftBottom;
            //----------------------------------------------------
            _aggsx_0 = aggsx; //set this as default ***            
            TargetBufferName = TargetBufferName.Default;
            _stroke = new Stroke(1);//default
            _useDefaultBrush = true;
            _defaultPixelBlender = this.DestBitmapBlender.OutputPixelBlender;
        }

        enum ClipingTechnique
        {
            None,
            ClipMask,
            ClipSimpleRect
        }

        ClipingTechnique _currentClipTech;

        public DrawBoard DrawBoard { get; set; }
        /// <summary>
        /// we DO NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void SetClipRgn(VertexStore vxs)
        {
            //clip rgn implementation
            //this version replace only
            //TODO: add append clip rgn 
            if (vxs != null)
            {
                if (SimpleRectClipEvaluator.EvaluateRectClip(vxs, out RectangleF clipRect))
                {

                    this.SetClipBox(
                        (int)Math.Floor(clipRect.X), (int)Math.Floor(clipRect.Y),
                        (int)Math.Ceiling(clipRect.Right), (int)Math.Ceiling(clipRect.Bottom));

                    _currentClipTech = ClipingTechnique.ClipSimpleRect;
                }
                else
                {
                    //not simple rect => 
                    //use mask technique

                    _currentClipTech = ClipingTechnique.ClipMask;
                    this.TargetBufferName = TargetBufferName.AlphaMask;
                    var prevColor = this.FillColor;
                    this.FillColor = Color.White;

                    this.Fill(vxs);
                    this.FillColor = prevColor;
                    this.TargetBufferName = TargetBufferName.Default;//swicth to default buffer
                    this.EnableBuiltInMaskComposite = true;
                }
            }
            else
            {
                //remove clip rgn if exists**
                switch (_currentClipTech)
                {
                    case ClipingTechnique.ClipMask:
                        this.EnableBuiltInMaskComposite = false;
                        this.TargetBufferName = TargetBufferName.AlphaMask;//swicth to mask buffer
                        this.Clear(Color.Black);
                        this.TargetBufferName = TargetBufferName.Default;

                        break;
                    case ClipingTechnique.ClipSimpleRect:

                        this.SetClipBox(0, 0, this.Width, this.Height);
                        break;
                }

                _currentClipTech = ClipingTechnique.None;
            }
        }
        public LineRenderingTechnique LineRenderingTech
        {
            get { return _lineRenderingTech; }
            set
            {

                if (value == LineRenderingTechnique.OutlineAARenderer &&
                    _outlineRas == null)
                {


                    _lineProfileAA = new Rasterization.Lines.LineProfileAnitAlias(this.StrokeWidth, null);

                    var blender = new PixelBlenderBGRA();

                    var outlineRenderer = new Rasterization.Lines.OutlineRenderer(
                        new ClipProxyImage(new SubBitmapBlender(_aggsx.DestBitmap, blender)), //Need ClipProxyImage
                        blender,
                        _lineProfileAA);
                    outlineRenderer.SetClipBox(0, 0, this.Width, this.Height);

                    //TODO: impl 'Pen'

                    _outlineRas = new Rasterization.Lines.OutlineAARasterizer(outlineRenderer);
                    _outlineRas.LineJoin = Rasterization.Lines.OutlineAARasterizer.OutlineJoin.Round;
                    _outlineRas.RoundCap = true;

                }
                _lineRenderingTech = value;
            }
        }
        public override Brush CurrentBrush
        {
            get
            {
                return _curBrush;
            }
            set
            {
                _curBrush = value;
                //check brush kind
                if (value == null)
                {
                    _useDefaultBrush = true;
                    return;
                }

                _useDefaultBrush = false;

                switch (value.BrushKind)
                {
                    default: throw new NotSupportedException();
                    //
                    case BrushKind.Solid:
                        break;
                    case BrushKind.LinearGradient:
                        break;
                    case BrushKind.CircularGraident:
                        break;
                    case BrushKind.GeometryGradient:
                        break;
                    case BrushKind.Texture:
                        break;
                }
            }
        }
        public override Pen CurrentPen
        {
            get => _curPen;
            set => _curPen = value;
        }
        public static AggPainter Create(MemBitmap bmp, PixelProcessing.PixelBlender32 blender = null)
        {
            //helper func

            AggRenderSurface renderSx = new AggRenderSurface(bmp);
            if (blender == null)
            {
                blender = new PixelProcessing.PixelBlenderBGRA();
            }
            renderSx.PixelBlender = blender;

            return new AggPainter(renderSx);
        }

        public AggRenderSurface RenderSurface => _aggsx;
        public BitmapBlenderBase DestBitmapBlender => _aggsx.DestBitmapBlender;
        public override int Width => _aggsx.Width;
        public override int Height => _aggsx.Height;
        public override float OriginX => _aggsx.ScanlineRasOriginX;
        public override float OriginY => _aggsx.ScanlineRasOriginY;
        public override void Clear(Color color) => _aggsx.Clear(color);
        public override void SetOrigin(float x, float y) => _aggsx.SetScanlineRasOrigin(x, y);

        RenderQuality _renderQuality;
        public override RenderQuality RenderQuality
        {
            get => _renderQuality;
            set => _renderQuality = value;
        }
        RenderSurfaceOrientation _orientation;
        public override RenderSurfaceOrientation Orientation
        {
            get => _orientation;
            set => _orientation = value;
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
                        //TODO: review here
                        //anti alias != lcd technique 
                        this.RenderQuality = RenderQuality.HighQuality;
                        _aggsx.UseSubPixelLcdEffect = true;
                        break;
                    case Drawing.SmoothingMode.HighSpeed:
                    default:
                        this.RenderQuality = RenderQuality.Fast;
                        _aggsx.UseSubPixelLcdEffect = false;
                        break;
                }
            }
        }
        public override RectInt ClipBox
        {
            get => _aggsx.GetClippingRect();
            set => _aggsx.SetClippingRect(value);
        }
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            _aggsx.SetClippingRect(new RectInt(x1, y1, x2, y2));
        }



        /// <summary>
        /// draw line
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="color"></param>
        public override void DrawLine(double x1, double y1, double x2, double y2)
        {
            //BitmapExt
            if (this.RenderQuality == RenderQuality.Fast)
            {
                _bxt.DrawLine(
                   (int)Math.Round(x1),
                   (int)Math.Round(y1),
                   (int)Math.Round(x2),
                   (int)Math.Round(y2),
                    _strokeColor.ToARGB()
                   );

                return;
            }

            //----------------------------------------------------------
            //Agg
            using (VxsTemp.Borrow(out var v1, out var v2))
            using (VectorToolBox.Borrow(v1, out PathWriter pw))
            {
                pw.Clear();
                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {
                    //as original 
                    pw.MoveTo(x1, y1);
                    pw.LineTo(x2, y2);
                }
                else
                {
                    //left-top
                    int h = this.Height;
                    pw.MoveTo(x1, h - y1);
                    pw.LineTo(x2, h - y2);
                }
                //----------------------------------------------------------
                if (_lineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {
                    _aggsx.Render(_stroke.MakeVxs(v1, v2), _strokeColor);

                }
                else
                {
                    _outlineRas.RenderVertexSnap(v1, _strokeColor);
                }
            }
        }

        double _strokeW;
        public override double StrokeWidth
        {
            get => _strokeW;
            set
            {
                if (value != _strokeW)
                {
                    //strokeW change
                    _strokeW = value;
                    if (_lineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                    {
                        _stroke.Width = value;
                    }
                    else
                    {
                        _lineProfileAA.SubPixelWidth = (float)value;
                    }
                }
            }
        }

        public override void Draw(VertexStore vxs)
        {
            if (RenderQuality == RenderQuality.Fast)
            {

                VertexCmd cmd;
                int index = 0;
                double lastMoveX = 0, lastMoveY = 0;
                double lastX = 0, lastY = 0;
                int stroke_color = _strokeColor.ToARGB();
                while ((cmd = vxs.GetVertex(index++, out double x, out double y)) != VertexCmd.NoMore)
                {
                    switch (cmd)
                    {
                        case VertexCmd.MoveTo:
                            lastX = lastMoveX = x;
                            lastY = lastMoveY = y;

                            break;
                        case VertexCmd.LineTo:

                            //need rounding
                            _bxt.DrawLine((int)Math.Round(lastX), (int)Math.Round(lastY), (int)Math.Round(x), (int)Math.Round(y), stroke_color);

                            lastX = x;
                            lastY = y;

                            break;
                        case VertexCmd.Close:

                            _bxt.DrawLine((int)Math.Round(lastX), (int)Math.Round(lastY), (int)Math.Round(lastMoveX), (int)Math.Round(lastMoveY), stroke_color);

                            lastX = x;
                            lastY = y;

                            break;
                    }
                }

                return;
            }
            if (_lineDashGen == null)
            {
                //no line dash

                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {
                    using (VxsTemp.Borrow(out var v1))
                    {
                        _aggsx.Render(_stroke.MakeVxs(vxs, v1), _strokeColor);
                    }
                }
                else
                {
                    _outlineRas.RenderVertexSnap(vxs, _strokeColor);
                }
            }
            else
            {
                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {

                    using (VxsTemp.Borrow(out var v1, out var v2))
                    {
                        //TODO: check lineDash

                        //_lineDashGen.CreateDash(vxs, v1);
                        _stroke.MakeVxs(v1, v2);
                        _aggsx.Render(v2, _strokeColor);
                    }
                }
                else
                {
                    using (VxsTemp.Borrow(out var v1))
                    {

                        //TODO: check lineDash

                        //_lineDashGen.CreateDash(vxs, v1);
                        _outlineRas.RenderVertexSnap(v1, _strokeColor);
                    }
                }

            }
        }

        public override void DrawRect(double left, double top, double width, double height)
        {

            //BitmapExt
            if (this.RenderQuality == RenderQuality.Fast)
            {

                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {

                    _bxt.DrawRectangle(
                       (int)Math.Round(left),
                       (int)Math.Round(top),
                       (int)Math.Round(left + width),
                       (int)Math.Round(top + height),

                   ColorInt.FromArgb(_strokeColor.ToARGB()));
                }
                else
                {
                    //TODO: review here
                    throw new NotSupportedException();
                    //int canvasH = this.Height; 
                    ////_simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
                    //_bmpBuffer.DrawRectangle(
                    //(int)Math.Round(left),
                    //(int)Math.Round(top),
                    //(int)Math.Round(left + width),
                    //(int)Math.Round(top + height),
                    //ColorInt.FromArgb(this.strokeColor.ToARGB()));
                }
                return; //exit
            }

            //----------------------------------------------------------
            //Agg


            using (VectorToolBox.Borrow(out SimpleRect rectTool))
            {
                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {
                    double right = left + width;
                    double bottom = top + height;
                    rectTool.SetRect(left + 0.5, bottom + 0.5, right - 0.5, top - 0.5);
                }
                else
                {
                    double right = left + width;
                    double bottom = top - height;
                    int canvasH = this.Height;
                    //_simpleRectVxsGen.SetRect(left + 0.5, canvasH - (bottom + 0.5), right - 0.5, canvasH - (top - 0.5));
                    rectTool.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
                }

                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {
                    using (VxsTemp.Borrow(out var v1, out var v2))
                    {
                        _aggsx.Render(_stroke.MakeVxs(rectTool.MakeVxs(v1), v2), _strokeColor);
                    }
                }
                else
                {
                    using (VxsTemp.Borrow(out var v1))
                    {
                        _outlineRas.RenderVertexSnap(rectTool.MakeVxs(v1), _strokeColor);
                    }
                }
            }
        }

        public override void DrawEllipse(double left, double top, double width, double height)
        {

            double ox = (left + width / 2);
            double oy = (top + height / 2);
            if (_orientation == RenderSurfaceOrientation.LeftTop)
            {
                //modified
                oy = this.Height - oy;
            }

            //---------------------------------------------------------- 
            //BitmapExt
            if (_renderQuality == RenderQuality.Fast)
            {

                _bxt.DrawEllipseCentered(
                   (int)Math.Round(ox), (int)Math.Round(oy),
                   (int)Math.Round(width / 2),
                   (int)Math.Round(height / 2),
                   _strokeColor.ToARGB());

                return;
            }



            using (VectorToolBox.Borrow(out Ellipse ellipseTool))
            {
                ellipseTool.Set(ox,
                       oy,
                       width / 2,
                       height / 2,
                       _ellipseGenNSteps);
                if (LineRenderingTech == LineRenderingTechnique.StrokeVxsGenerator)
                {
                    using (VxsTemp.Borrow(out var v1, out var v2))
                    {
                        _aggsx.Render(_stroke.MakeVxs(ellipseTool.MakeVxs(v1), v2), _strokeColor);
                    }
                }
                else
                {
                    using (VxsTemp.Borrow(out var v1))
                    {
                        _outlineRas.RenderVertexSnap(ellipseTool.MakeVxs(v1), _strokeColor);
                    }
                }
            }

        }
        public override void FillEllipse(double left, double top, double width, double height)
        {
            double ox = (left + width / 2);
            double oy = (top + height / 2);
            if (_orientation == RenderSurfaceOrientation.LeftTop)
            {
                //modified
                oy = this.Height - oy;
            }
            //---------------------------------------------------------- 
            //BitmapExt
            if (_renderQuality == RenderQuality.Fast)
            {
                _bxt.FillEllipseCentered(
                  (int)Math.Round(ox), (int)Math.Round(oy),
                  (int)Math.Round(width / 2),
                  (int)Math.Round(height / 2),
                  _fillColor.ToARGB());
                return;
            }


            //Agg
            //---------------------------------------------------------- 

            using (VectorToolBox.Borrow(out Ellipse ellipseTool))
            using (VxsTemp.Borrow(out var v1))
            {
                ellipseTool.Set(ox,
                         oy,
                         width / 2,
                         height / 2,
                         _ellipseGenNSteps);
                _aggsx.Render(ellipseTool.MakeVxs(v1), _fillColor);
            }
        }
        public override void FillRect(double left, double top, double width, double height)
        {


            //---------------------------------------------------------- 
            //BitmapExt
            if (_useDefaultBrush && _renderQuality == RenderQuality.Fast)
            {
                _bxt.FillRectangle(
                      (int)Math.Round(left),
                      (int)Math.Round(top),
                      (int)Math.Round(left + width),
                      (int)Math.Round(top + height),
                      ColorInt.FromArgb(_fillColor.ToARGB()));
                return;
            }

            //Agg 
            //---------------------------------------------------------- 

            using (VectorToolBox.Borrow(out SimpleRect rectTool))
            using (VxsTemp.Borrow(out var v1))
            {
                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {
                    double right = left + width;
                    double bottom = top - height;
                    if (right < left || top < bottom)
                    {
#if DEBUG
                        throw new ArgumentException();
#else
                return;
#endif
                    }

                    rectTool.SetRect(left + 0.5, (bottom + 0.5) + height, right - 0.5, (top - 0.5) + height);
                }
                else
                {

                    double right = left + width;
                    double bottom = top - height;
                    if (right < left || top < bottom)
                    {
#if DEBUG
                        throw new ArgumentException();
#else
                return;
#endif
                    }

                    int canvasH = this.Height;
                    rectTool.SetRect(left + 0.5, canvasH - (bottom + 0.5 + height), right - 0.5, canvasH - (top - 0.5 + height));
                }
                if (!_useDefaultBrush)
                {
                    Brush br = _curBrush;
                    switch (br.BrushKind)
                    {
                        case BrushKind.LinearGradient:
                            {
                                //fill linear gradient brush
                                //....

                                //check resolved object for br 
                                //if not then create a new one
                                //------------------------------------------- 
                                //original agg's gradient fill 

                                _aggGradientBrush.ResolveBrush((LinearGradientBrush)br);
                                _aggGradientBrush.SetOffset((float)-left, (float)-top);
                                Fill(rectTool.MakeVxs(v1), _aggGradientBrush);
                            }
                            break;
                        case BrushKind.CircularGraident:
                            {
                                _circularGradBrush.ResolveBrush((CircularGradientBrush)br);
                                _circularGradBrush.SetOffset((float)-left, (float)-top);
                                Fill(rectTool.MakeVxs(v1), _circularGradBrush);
                            }
                            break;
                        default:
                            {
                                _aggsx.Render(rectTool.MakeVxs(v1), _fillColor);
                            }
                            break;
                    }
                }
                else
                {
                    _aggsx.Render(rectTool.MakeVxs(v1), _fillColor);
                }

            }
        }



        AggLinearGradientBrush _aggGradientBrush = new AggLinearGradientBrush();
        AggCircularGradientBrush _circularGradBrush = new AggCircularGradientBrush();

        public override RequestFont CurrentFont
        {
            get
            {
                return _currentFont;
            }
            set
            {
                _currentFont = value;
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
                if (_orientation == RenderSurfaceOrientation.LeftBottom)
                {
                    _textPrinter.DrawString(text, x, y);
                }
                else
                {
                    //from current point size 
                    //we need line height of current font size
                    //then we will start on 'base line'

                    _textPrinter.DrawString(text, x, this.Height - y);
                }

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
                    _textPrinter.ChangeFont(_currentFont);
                }
            }
        }


        List<int> _reusablePolygonList = new List<int>();
        /// <summary>
        /// fill with BitmapBufferExtension lib
        /// </summary>
        void FillWithBxt(VertexStore vxs)
        {
            //transate the vxs/snap to command
            double x = 0;
            double y = 0;
            double offsetOrgX = this.OriginX;
            double offsetOrgY = this.OriginY;


            VertexCmd cmd;
            int index = 0;
            int latestMoveToX = 0, latestMoveToY = 0;
            int latestX = 0, latestY = 0;


            bool closed = false;

            _reusablePolygonList.Clear();

            while ((cmd = vxs.GetVertex(index++, out x, out y)) != VertexCmd.NoMore)
            {
                x += offsetOrgX;
                y += offsetOrgY;

                switch (cmd)
                {
                    case VertexCmd.MoveTo:
                        {
                            if (_reusablePolygonList.Count > 0)
                            {
                                //no drawline
                                _reusablePolygonList.Clear();
                            }

                            closed = false;
                            _reusablePolygonList.Add(latestMoveToX = latestX = (int)Math.Round(x));
                            _reusablePolygonList.Add(latestMoveToY = latestY = (int)Math.Round(y));

                        }
                        break;
                    case VertexCmd.LineTo:
                    case VertexCmd.P2c:
                    case VertexCmd.P3c:
                        {
                            //collect to the polygon
                            _reusablePolygonList.Add(latestX = (int)Math.Round(x));
                            _reusablePolygonList.Add(latestY = (int)Math.Round(y));
                        }
                        break;
                    case VertexCmd.Close:
                    case VertexCmd.CloseAndEndFigure:
                        {
                            if (_reusablePolygonList.Count > 0)
                            {
                                //flush by draw line
                                _reusablePolygonList.Add(latestX = latestMoveToX);
                                _reusablePolygonList.Add(latestY = latestMoveToY);

                                _bxt.FillPolygon(_reusablePolygonList.ToArray(),
                                    _fillColor.ToARGB());
                            }

                            _reusablePolygonList.Clear();
                            closed = true;
                        }
                        break;
                    default:
                        break;
                }
            }
            //---------------
            if (!closed && (_reusablePolygonList.Count > 0) &&
               (latestX == latestMoveToX) && (latestY == latestMoveToY))
            {

                //flush by draw line
                _reusablePolygonList.Add(latestMoveToX);
                _reusablePolygonList.Add(latestMoveToY);

                _bxt.FillPolygon(
                    _reusablePolygonList.ToArray(),
                    _fillColor.ToARGB());
            }
        }

        /// <summary>
        /// fill vxs, we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void Fill(VertexStore vxs)
        {
            //
            if (_useDefaultBrush && _renderQuality == RenderQuality.Fast)
            {
                FillWithBxt(vxs);
                return;
            }
            if (!_useDefaultBrush)
            {
                Brush br = _curBrush;
                switch (br.BrushKind)
                {
                    case BrushKind.LinearGradient:
                        {
                            //fill linear gradient brush
                            //....

                            //check resolved object for br 
                            //if not then create a new one
                            //------------------------------------------- 
                            //original agg's gradient fill 

                            _aggGradientBrush.ResolveBrush((LinearGradientBrush)br);
                            _aggGradientBrush.SetOffset(0, 0);
                            Fill(vxs, _aggGradientBrush);
                        }
                        break;
                    case BrushKind.CircularGraident:
                        {
                            _circularGradBrush.ResolveBrush((CircularGradientBrush)br);
                            _circularGradBrush.SetOffset(0, 0);
                            Fill(vxs, _circularGradBrush);
                        }
                        break;
                    default:
                        {

                            _aggsx.Render(vxs, _fillColor);
                        }
                        break;
                }
            }
            else
            {
                _aggsx.Render(vxs, _fillColor);
            }
        }
        public override void Render(RenderVx renderVx)
        {
            //VG Render?
            //if (renderVx is VgRenderVx)
            //{

            //}
            //else
            //{
            //    //?
            //    throw new NotSupportedException();
            //}
        }

        public override bool UseSubPixelLcdEffect
        {
            get => _aggsx.UseSubPixelLcdEffect;
            set => _aggsx.UseSubPixelLcdEffect = value;
        }
        public override Color FillColor
        {
            get => _fillColor;
            set => _fillColor = value;

        }
        public override Color StrokeColor
        {
            get => _strokeColor;
            set => _strokeColor = value;
        }

        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
        public void Fill(VertexStore vxs, ISpanGenerator spanGen)
        {
            _aggsx.Render(vxs, spanGen);

            //_sclineRas.AddPath(vxs);
            //_bmpRasterizer.RenderWithSpan(_aggsx.DestImage, _sclineRas, _scline, spanGen);
        }
        void DrawBitmap(MemBitmap memBmp, double left, double top)
        {
            //check image caching system 
            if (_renderQuality == RenderQuality.Fast)
            {
                TempMemPtr tmp = MemBitmap.GetBufferPtr(memBmp);
                unsafe
                {
                    BitmapBuffer srcBmp = new BitmapBuffer(memBmp.Width, memBmp.Height, tmp.Ptr, tmp.LengthInBytes);
                    try
                    {
                        _bxt.CopyBlit(this.OriginX + (int)left, this.OriginY + (int)top, srcBmp);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return;
            }

            //save, restore later... 
            bool useSubPix = UseSubPixelLcdEffect;
            //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;
            _aggsx.UseSubPixelLcdEffect = false;

            if (_orientation == RenderSurfaceOrientation.LeftTop)
            {
                //place left upper corner at specific x y                    
                _aggsx.Render(memBmp, left, this.Height - (top + memBmp.Height));
            }
            else
            {
                //left-bottom as original
                //place left-lower of the img at specific (x,y)
                _aggsx.Render(memBmp, left, top);
            }

            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
            _aggsx.UseSubPixelLcdEffect = useSubPix;
        }
        void DrawBitmap(MemBitmap memBmp, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            //check image caching system 
            if (_renderQuality == RenderQuality.Fast)
            {
                TempMemPtr tmp = MemBitmap.GetBufferPtr(memBmp);
                unsafe
                {
                    BitmapBuffer srcBmp = new BitmapBuffer(memBmp.Width, memBmp.Height, tmp.Ptr, tmp.LengthInBytes);
                    try
                    {
                        var src = new BitmapBufferEx.RectD(srcX, srcY, srcW, srcH);
                        var dest = new BitmapBufferEx.RectD(left, top, srcW, srcH);

                        BitmapBuffer bmpBuffer = new BitmapBuffer(memBmp.Width, memBmp.Height, tmp.Ptr, tmp.LengthInBytes);
                        _bxt.CopyBlit(dest, bmpBuffer, src);
                    }
                    catch (Exception ex)
                    {

                    }
                }
                return;
            }

            //save, restore later... 
            bool useSubPix = UseSubPixelLcdEffect;
            //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;

            if (_orientation == RenderSurfaceOrientation.LeftTop)
            {
                //place left upper corner at specific x y                    
                _aggsx.Render(memBmp, left, this.Height - (top + memBmp.Height), srcX, srcY, srcW, srcH);
            }
            else
            {
                //left-bottom as original
                //place left-lower of the img at specific (x,y)
                _aggsx.Render(memBmp, left, top, srcX, srcY, srcW, srcH);
            }

            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
        }
        public override void DrawImage(Image actualImage, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            MemBitmap memBmp = actualImage as MemBitmap;
            if (memBmp == null)
            {
                //test with other bitmap 
                return;
            }
            else
            {
                DrawBitmap(memBmp, left, top, srcX, srcY, srcW, srcH);
            }
        }
        public override void DrawImage(Image img, double left, double top)
        {
            MemBitmap memBmp = img as MemBitmap;
            if (memBmp == null)
            {
                //test with other bitmap 
                return;
            }
            else
            {
                DrawBitmap(memBmp, left, top);
            }
        }
        public override void DrawImage(Image img)
        {
            MemBitmap memBmp = img as MemBitmap;
            if (memBmp == null)
            {
                //? TODO
                return;
            }

            if (_renderQuality == RenderQuality.Fast)
            {
                //todo, review here again
                TempMemPtr tmp = MemBitmap.GetBufferPtr(memBmp);
                BitmapBuffer srcBmp = new BitmapBuffer(img.Width, img.Height, tmp.Ptr, tmp.LengthInBytes);

                //_bxt.BlitRender(srcBmp, false, 1, null);
                //_bxt.Blit(0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight, srcBmp, 0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight,
                //    ColorInt.FromArgb(255, 255, 255, 255),
                //    BitmapBufferExtensions.BlendMode.Alpha);
                _bxt.FastAlphaBlit(0, 0, srcBmp, 0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight);
                return;
            }
            //-------------------------------
            bool useSubPix = UseSubPixelLcdEffect; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;

            _aggsx.Render(memBmp, null as AffinePlan[]);
            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
        }
        public override void DrawImage(Image img, params AffinePlan[] affinePlans)
        {
            MemBitmap memBmp = img as MemBitmap;
            if (memBmp == null)
            {
                //? TODO
                return;
            }

            if (_renderQuality == RenderQuality.Fast)
            {
                //todo, review here again
                TempMemPtr tmp = MemBitmap.GetBufferPtr(memBmp);
                BitmapBuffer srcBmp = new BitmapBuffer(img.Width, img.Height, tmp.Ptr, tmp.LengthInBytes);
                if (affinePlans != null && affinePlans.Length > 0)
                {
                    _bxt.BlitRender(srcBmp, false, 1, new BitmapBufferEx.MatrixTransform(affinePlans));
                }
                else
                {
                    //_bxt.BlitRender(srcBmp, false, 1, null);
                    _bxt.Blit(0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight, srcBmp, 0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight,
                        ColorInt.FromArgb(255, 255, 255, 255),
                        BitmapBufferExtensions.BlendMode.Alpha);
                }
                return;
            }

            bool useSubPix = UseSubPixelLcdEffect; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;

            _aggsx.Render(memBmp, affinePlans);
            //restore...
            this.UseSubPixelLcdEffect = useSubPix;

        }
        public override void DrawImage(Image actualImage, double left, double top, ICoordTransformer coordTx)
        {
            //draw img with transform coord
            //
            MemBitmap memBmp = actualImage as MemBitmap;
            if (memBmp == null)
            {
                //? TODO
                return;
            }

            if (_renderQuality == RenderQuality.Fast)
            {
                //todo, review here again
                //TempMemPtr tmp = ActualBitmap.GetBufferPtr(actualImg);
                //BitmapBuffer srcBmp = new BitmapBuffer(actualImage.Width, actualImage.Height, tmp.Ptr, tmp.LengthInBytes); 
                //_bxt.BlitRender(srcBmp, false, 1, new BitmapBufferEx.MatrixTransform(affinePlans));

                //if (affinePlans != null && affinePlans.Length > 0)
                //{
                //    _bxt.BlitRender(srcBmp, false, 1, new BitmapBufferEx.MatrixTransform(affinePlans));
                //}
                //else
                //{
                //    //_bxt.BlitRender(srcBmp, false, 1, null);
                //    _bxt.Blit(0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight, srcBmp, 0, 0, srcBmp.PixelWidth, srcBmp.PixelHeight,
                //        ColorInt.FromArgb(255, 255, 255, 255),
                //        BitmapBufferExtensions.BlendMode.Alpha);
                //}
                return;
            }

            bool useSubPix = UseSubPixelLcdEffect; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseSubPixelLcdEffect = false;

            if (coordTx is Affine)
            {
                Affine aff = (Affine)coordTx;
                if (this.OriginX != 0 || this.OriginY != 0)
                {
                    coordTx = aff = aff * Affine.NewTranslation(this.OriginX, this.OriginY);
                }
            }

            //_aggsx.SetScanlineRasOrigin(OriginX, OriginY);

            _aggsx.Render(memBmp, coordTx);

            //_aggsx.SetScanlineRasOrigin(xx, yy);
            //restore...
            this.UseSubPixelLcdEffect = useSubPix;
        }
        public override void ApplyFilter(ImageFilter imgFilter)
        {
            ////----------------------
            ///// <summary>
            ///// do filter at specific area
            ///// </summary>
            ///// <param name="filter"></param>
            ///// <param name="area"></param>
            //public override void DoFilterBlurStack(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(_aggsx.DestImage, _aggsx.PixelBlender,
            //        area.Left, area.Bottom, area.Right, area.Top);
            //    filterMan.DoStackBlur(img, r);
            //}
            //public override void DoFilterBlurRecursive(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(_aggsx.DestImage, _aggsx.PixelBlender,
            //        area.Left, area.Bottom, area.Right, area.Top);
            //    filterMan.DoRecursiveBlur(img, r);
            //}
            //public override void DoFilter(RectInt area, int r)
            //{
            //    ChildImage img = new ChildImage(_aggsx.DestImage, _aggsx.PixelBlender,
            //      area.Left, area.Top, area.Right, area.Bottom);
            //    filterMan.DoSharpen(img, r);
            //}
            //TODO: implement this
            //resolve internal img filter
            //switch (imgFilter.Name)
            //{ 
            //} 
        }
        public override RenderVx CreateRenderVx(VertexStore vxs)
        {
            return new AggRenderVx(vxs);
        }
        public override void DrawRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Draw(aggRenderVx.vxs);
        }
        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            //fill with brush 
            if (brush is SolidBrush)
            {
                SolidBrush solidBrush = (SolidBrush)brush;
                Color prevColor = _fillColor;
                _fillColor = solidBrush.Color;
                Fill(aggRenderVx.vxs);
                _fillColor = prevColor;
            }
            else
            {
                Fill(aggRenderVx.vxs);
            }
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Fill(aggRenderVx.vxs);
        }
        public LineJoin LineJoin
        {
            get => _stroke.LineJoin;
            set => _stroke.LineJoin = value;
        }
        public LineCap LineCap
        {
            get => _stroke.LineCap;
            set => _stroke.LineCap = value;
        }
        public LineDashGenerator LineDashGen
        {
            get => _lineDashGen;
            set => _lineDashGen = value;
        }



        //---------------------------------------------------------------
        TargetBufferName _targetBufferName;
        bool _enableBuiltInMaskComposite;
        MemBitmap _alphaBitmap;

        void SetupMaskPixelBlender()
        {
            if (_aggsx_mask != null)
            {
                //also set the canvas origin for the aggsx_mask
                _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY);
                return;
            }
            //----------
            //same size                  

            _alphaBitmap = new MemBitmap(_aggsx_0.Width, _aggsx_0.Height);
#if DEBUG
            _alphaBitmap._dbugNote = "AggPrinter.SetupMaskPixelBlender";
#endif

            _aggsx_mask = new AggRenderSurface(_alphaBitmap) { PixelBlender = new PixelBlenderBGRA() };
            _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY); //also set the canvas origin for the aggsx_mask

            _maskPixelBlender = new PixelBlenderWithMask();
            _maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

            _maskPixelBlender.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
            _maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
        }
        public TargetBufferName TargetBufferName
        {
            get { return _targetBufferName; }
            set
            {
                //change or not
                if (_targetBufferName != value)
                {
                    switch (value)
                    {
                        default: throw new NotSupportedException();
                        case TargetBufferName.Default:
                            //default 
                            _aggsx = _aggsx_0; //*** 
                            break;
                        case TargetBufferName.AlphaMask:
                            SetupMaskPixelBlender();
                            _aggsx = _aggsx_mask;//*** 
                            break;
                    }


                    TempMemPtr tmp = MemBitmap.GetBufferPtr(_aggsx.DestBitmap);
                    unsafe
                    {
                        _bxt = new BitmapBuffer(
                       _aggsx.Width,
                       _aggsx.Height,
                        tmp.Ptr,
                        tmp.LengthInBytes);
                    }


                    _targetBufferName = value;
                }

            }
        }
        public bool EnableBuiltInMaskComposite
        {
            get { return _enableBuiltInMaskComposite; }
            set
            {
                if (_enableBuiltInMaskComposite != value)
                {
                    _enableBuiltInMaskComposite = value;
                    if (value)
                    {
                        //use mask composite
                        this.DestBitmapBlender.OutputPixelBlender = _maskPixelBlender;
                    }
                    else
                    {
                        //use default composite
                        this.DestBitmapBlender.OutputPixelBlender = _defaultPixelBlender;
                    }
                }
            }
        }



        static VertexStore GetStrokeVxsOrCreateNew(VertexStore vxs, float strokeW)
        {
            using (VectorToolBox.Borrow(out Stroke stroke))
            using (VxsTemp.Borrow(out var v1))
            {

                stroke.Width = strokeW;
                stroke.MakeVxs(vxs, v1);
                VertexStore vx = v1.CreateTrim();
                return vx;
            }
        }

    }


    public enum TargetBufferName
    {
        Unknown,
        Default,
        AlphaMask
    }






    class ReusableRotationTransformer : VertexProcessing.ICoordTransformer
    {

        double _angle;
        Affine affine;
        public ReusableRotationTransformer()
        {
            affine = Affine.IdentityMatrix;
        }
        public double Angle
        {
            get
            {
                return _angle;
            }
            set
            {
                if (value != _angle)
                {
                    affine = Affine.NewRotation(value);
                }
                _angle = value;
            }
        }

        public CoordTransformerKind Kind => CoordTransformerKind.Affine3x2;

        public void Transform(ref double x, ref double y)
        {
            affine.Transform(ref x, ref y);
        }

        ICoordTransformer ICoordTransformer.MultiplyWith(ICoordTransformer another)
        {
            return this.affine.MultiplyWith(another);
        }
        ICoordTransformer ICoordTransformer.CreateInvert()
        {
            return affine.CreateInvert();
        }

    }


    struct GradientSpanPart
    {
        public GradientSpanGen _spanGenGr;
        public LinearGradientColorsProvider _linearGradientColorProvider;
        public SpanInterpolatorLinear _linerInterpolator;
        public ReusableRotationTransformer _reusableRotationTransformer;

        public void SetData(IGradientValueCalculator gvc, LinearGradientPair pair)
        {

            _linerInterpolator = new SpanInterpolatorLinear();
            _linearGradientColorProvider = new LinearGradientColorsProvider();
            _spanGenGr = new GradientSpanGen();
            //TODO:
            //user can use other coord transformer 
            _linerInterpolator.Transformer =
                _reusableRotationTransformer = new ReusableRotationTransformer();
            _reusableRotationTransformer.Angle = pair.Angle;
            _linearGradientColorProvider.SetColors(pair.c1, pair.c2, pair.steps);
            _spanGenGr.Reset(_linerInterpolator,
                gvc,
                _linearGradientColorProvider,
               pair._distance);

            _spanGenGr.SetStartPoint(pair.x1, pair.y1);

        }

        public void SetOffset(float x, float y)
        {
            _spanGenGr.SetOffset(x, y);
        }
    }

    class AggLinearGradientBrush : ISpanGenerator
    {
        static IGradientValueCalculator _gvcX = new GvcX();
        static IGradientValueCalculator _gvcY = new GvcY();



        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;


        bool isInit;
        public void Prepare()
        {

        }


        public void ResolveBrush(LinearGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!isInit)
            {
                //temp fix  
                isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;

            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                IGradientValueCalculator gvc = null;
                switch (pair.Direction)
                {
                    case LinearGradientPair.GradientDirection.Vertical:
                        gvc = _gvcY;
                        break;
                    case LinearGradientPair.GradientDirection.Horizontal:
                        gvc = _gvcX;
                        break;
                    default:
                        //temp, 
                        //TODO: review here
                        gvc = _gvcX;
                        break;
                }

                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(gvc, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS
            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo < partCount)
                    {
                        return _moreSpanGenertors[fromPartNo]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }


        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

    class AggCircularGradientBrush : ISpanGenerator
    {

        static IGradientValueCalculator _gvcCircular = new GvcRadial();

        GradientSpanPart _grSpanGenPart;
        List<GradientSpanPart> _moreSpanGenertors;


        bool isInit;
        public void Prepare()
        {

        }


        public void ResolveBrush(CircularGradientBrush linearGrBrush)
        {
            //for gradient :
            int pairCount = linearGrBrush.PairCount;

            //resolve linear gradient to agg object  
            if (!isInit)
            {
                //temp fix   
                isInit = true;
            }
            if (_moreSpanGenertors == null)
            {
                _moreSpanGenertors = new List<GradientSpanPart>();
            }
            else
            {
                _moreSpanGenertors.Clear();
            }
            //
            //more than 1 pair   
            int partNo = 0;
            int partCount = linearGrBrush.PairCount;
            foreach (LinearGradientPair pair in linearGrBrush.GetColorPairIter())
            {
                _grSpanGenPart = new GradientSpanPart();
                _grSpanGenPart.SetData(_gvcCircular, pair);
                _grSpanGenPart._spanGenGr.PartNo = partNo;
                _grSpanGenPart._spanGenGr.IsLastPart = (partNo == partCount - 1);
                _moreSpanGenertors.Add(_grSpanGenPart);
                partNo++;
            }

            _grSpanGenPart = _moreSpanGenertors[0];

#if !COSMOS

            for (int i = 0; i < partCount - 1; ++i)
            {
                GradientSpanPart part = _moreSpanGenertors[i];
                part._spanGenGr.RequestGradientPart += (fromPartNo) =>
                {
                    if (fromPartNo != partCount - 1)
                    {
                        return _moreSpanGenertors[fromPartNo + 1]._spanGenGr;
                    }
                    else
                    {
                        return null;
                    }
                };
            }
#endif
        }



        public void SetOffset(float x, float y)
        {
            //apply offset to all span generator
            int j = _moreSpanGenertors.Count;
            for (int i = 0; i < j; ++i)
            {
                _moreSpanGenertors[i].SetOffset(x, y);
            }
        }
        public void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int spanLen)
        {

            //start at current span generator 
            _grSpanGenPart._spanGenGr.GenerateColors(outputColors, startIndex, x, y, spanLen);
        }

    }

}