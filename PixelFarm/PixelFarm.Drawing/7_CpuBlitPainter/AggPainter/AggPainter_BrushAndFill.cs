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
        //brush
        Color _fillColor;
        Brush _curBrush;
        bool _useDefaultBrush;
        AggLinearGradientBrush _aggGradientBrush = new AggLinearGradientBrush();
        AggCircularGradientBrush _circularGradBrush = new AggCircularGradientBrush();

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
        public override Color FillColor
        {
            get => _fillColor;
            set => _fillColor = value;
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


    }

}