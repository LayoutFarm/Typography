//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {

        Color _fillColor; //fill color of solid brush
        Brush _curBrush;
        bool _useDefaultBrush;
        GouraudVerticeBuilder _gouraudVertBuilder;
        TessTool _tessTool;
        List<int> _reusablePolygonList = new List<int>();

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
#if DEBUG
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
                    case BrushKind.PolygonGradient:
                        break;
                    case BrushKind.Texture:
                        break;
                }
#endif
            }
        }
        public override Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                //use solid brush
                if (_curBrush != null)
                {
                    CurrentBrush = null;
                }
            }
        }

        public override bool UseSubPixelLcdEffect
        {
            get => _aggsx.UseSubPixelLcdEffect;
            set => _aggsx.UseSubPixelLcdEffect = value;
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
                Fill(aggRenderVx._vxs);
                _fillColor = prevColor;
            }
            else
            {
                Fill(aggRenderVx._vxs);
            }
        }
        public override void FillRenderVx(RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            Fill(aggRenderVx._vxs);
        }
        /// <summary>
        /// we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        /// <param name="spanGen"></param>
        public void Fill(VertexStore vxs, ISpanGenerator spanGen)
        {
            _aggsx.Render(vxs, spanGen);
        }

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
                            LinearGradientSpanGen linearSpanGen = ResolveLinearGrBrush((LinearGradientBrush)br);
                            linearSpanGen.SetOffset(0, 0);//TODO: review this offset
                            Fill(vxs, linearSpanGen);
                        }
                        break;
                    case BrushKind.CircularGraident:
                        {
                            RadialGradientSpanGen radialSpanGen = ResolveRadialGrBrush((RadialGradientBrush)br);
                            radialSpanGen.SetOrigin(0, 0);//TODO: review this offset 
                            radialSpanGen.Opactiy = FillOpacity;

                            Fill(vxs, radialSpanGen);

                        }
                        break;
                    case BrushKind.PolygonGradient:
                        {
                            FillWithPolygonGraidentBrush(vxs, (Drawing.PolygonGradientBrush)br);
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
        PolygonGradientBrush ResolvePolygonGradientBrush(Drawing.PolygonGradientBrush polygonGrBrush)
        {
            PolygonGradientBrush brush = polygonGrBrush.InnerBrush as PolygonGradientBrush;
            if (brush != null) return brush;

            // 
            if (_tessTool == null) { _tessTool = new TessTool(); }
            if (_gouraudVertBuilder == null) { _gouraudVertBuilder = new GouraudVerticeBuilder(); }

            RGBAGouraudSpanGen spanGen = polygonGrBrush.InnerBrush as RGBAGouraudSpanGen;
            if (spanGen == null)
            {
                spanGen = new RGBAGouraudSpanGen();
                polygonGrBrush.InnerBrush = spanGen;
            }


            //
            brush = new PolygonGradientBrush();
            brush.BuildFrom(polygonGrBrush);

            //tess user data, store tess result in the brush
            brush._vertIndices = _tessTool.TessAsTriIndexArray(
                brush.GetXYCoords(),
                null,
                out brush._outputCoords,
                out brush._vertexCount);

            brush.BuildCacheVertices(_gouraudVertBuilder);

            polygonGrBrush.InnerBrush = brush; //cache this brush
            return brush;
        }

        void FillWithPolygonGraidentBrush(VertexStore vxs, Drawing.PolygonGradientBrush polygonGrBrush)
        {
            //we use mask technique (simlar to texture brush) 
            //1. switch to mask layer

            SetClipRgn(vxs);

            PolygonGradientBrush brush = ResolvePolygonGradientBrush(polygonGrBrush);
            RGBAGouraudSpanGen spanGen = polygonGrBrush.InnerBrush as RGBAGouraudSpanGen;

            //TODO: add gamma here...
            //aggsx.ScanlineRasterizer.ResetGamma(new GammaLinear(0.0f, this.LinearGamma)); //*** 

            int partCount = brush.CachePartCount;
            for (int i = 0; i < partCount; i++)
            {
                brush.SetSpanGenWithCurrentValues(i, spanGen); //*** this affects assoc gouraudSpanGen
                this.Fill(brush.CurrentVxs, spanGen);
            }

            SetClipRgn(null);
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
        static LinearGradientSpanGen ResolveLinearGrBrush(LinearGradientBrush linearGr)
        {

            //check inner object
            LinearGradientSpanGen linearGradientSpanGen = linearGr.InnerBrush as LinearGradientSpanGen;
            if (linearGr == null)
            {
                //create a new one
                linearGradientSpanGen = new LinearGradientSpanGen();
                linearGradientSpanGen.ResolveBrush(linearGr);
                linearGr.InnerBrush = linearGradientSpanGen;
            }
            return linearGradientSpanGen;
        }
        static RadialGradientSpanGen ResolveRadialGrBrush(RadialGradientBrush radialGr)
        {
            RadialGradientSpanGen radialSpanGen = radialGr.InnerBrush as RadialGradientSpanGen;
            if (radialSpanGen == null)
            {
                radialSpanGen = new RadialGradientSpanGen();
                radialSpanGen.ResolveBrush(radialGr);
                radialGr.InnerBrush = radialSpanGen;
            }
            return radialSpanGen;
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
                                //check inner object
                                LinearGradientSpanGen linearGradientSpanGen = ResolveLinearGrBrush((LinearGradientBrush)br);
                                linearGradientSpanGen.SetOffset((float)-left, (float)-top);
                                Fill(rectTool.MakeVxs(v1), linearGradientSpanGen);
                            }
                            break;
                        case BrushKind.CircularGraident:
                            {
                                //var radialGr = (Drawing.RadialGradientBrush)br;
                                RadialGradientSpanGen radialSpanGen = ResolveRadialGrBrush((Drawing.RadialGradientBrush)br);
                                radialSpanGen.SetOrigin((float)-left, (float)-top);
                                Fill(rectTool.MakeVxs(v1), radialSpanGen);
                            }
                            break;
                        case BrushKind.PolygonGradient:
                            {
                                FillWithPolygonGraidentBrush(rectTool.MakeVxs(v1), (Drawing.PolygonGradientBrush)br);
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


    }

}