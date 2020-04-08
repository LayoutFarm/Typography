//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.FragmentProcessing;


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
            get => _curBrush;

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

        public override bool UseLcdEffectSubPixelRendering
        {
            get => _aggsx.UseSubPixelLcdEffect;
            set => _aggsx.UseSubPixelLcdEffect = value;
        }

        public override void FillRenderVx(Brush brush, RenderVx renderVx)
        {
            AggRenderVx aggRenderVx = (AggRenderVx)renderVx;
            //fill with brush 
            if (brush is SolidBrush solidBrush)
            {
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
        /// fill vxs, we do NOT store vxs
        /// </summary>
        /// <param name="vxs"></param>
        public override void Fill(VertexStore vxs)
        {

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
                            Q1RectD bounds = vxs.GetBoundingRect();

                            Point prevOrg = linearSpanGen.SpanOrigin;
                            //TODO: rounding

                            linearSpanGen.SpanOrigin = new Point((int)(OriginX + bounds.Left), (int)(OriginY + bounds.Bottom)); //*** 

                            Fill(vxs, linearSpanGen);

                            linearSpanGen.SpanOrigin = prevOrg;//restore

                        }
                        break;
                    case BrushKind.CircularGraident:
                        {
                            Q1RectD bounds = vxs.GetBoundingRect();
                            RadialGradientSpanGen radialSpanGen = ResolveRadialGrBrush((RadialGradientBrush)br);
                            //radialSpanGen.SetOrigin(0, 0);//TODO: review this offset 
                            Point prevOrg = radialSpanGen.SpanOrigin;
                            radialSpanGen.SpanOrigin = new Point((int)(OriginX + bounds.Left), (int)(OriginY + bounds.Bottom)); //*** 
                            radialSpanGen.Opactiy = FillOpacity;
                            Fill(vxs, radialSpanGen);
                            radialSpanGen.SpanOrigin = prevOrg;//restore
                        }
                        break;
                    case BrushKind.PolygonGradient:
                        {
                            FillWithPolygonGraidentBrush(vxs, (Drawing.PolygonGradientBrush)br);
                        }
                        break;
                    case BrushKind.Solid:
                        {
                            _aggsx.Render(vxs, ((SolidBrush)_curBrush).Color);
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
            if (polygonGrBrush.InnerBrush is PolygonGradientBrush brush) return brush;

            // 
            if (_tessTool == null) { _tessTool = new TessTool(); }
            if (_gouraudVertBuilder == null) { _gouraudVertBuilder = new GouraudVerticeBuilder(); }

            brush = new PolygonGradientBrush();
            brush.BuildFrom(polygonGrBrush);

            //tess user data, store tess result in the brush
            brush._vertIndices = _tessTool.TessAsTriIndexArray(
                brush.GetXYCoords(),
                null,
                out brush._outputCoords,
                out brush._vertexCount);

            brush.SpanOrigin = new Point((int)OriginX, (int)OriginY);
            brush.BuildCacheVertices(_gouraudVertBuilder);

            polygonGrBrush.InnerBrush = brush; //cache this brush
            return brush;
        }

        RGBAGouraudSpanGen _rgbaGourandSpanGen = new RGBAGouraudSpanGen();
        void FillWithPolygonGraidentBrush(VertexStore vxs, Drawing.PolygonGradientBrush polygonGrBrush)
        {
            //we use mask technique (simlar to texture brush) 

            Q1RectD bounds = vxs.GetBoundingRect();

            //1. switch to mask layer 
            SetClipRgn(vxs);

            Point prevOrg = _rgbaGourandSpanGen.SpanOrigin;
            float ox = OriginX;
            float oy = OriginY;

            Point newOrg = new Point((int)(bounds.Left + ox), (int)(bounds.Bottom + oy));
            _rgbaGourandSpanGen.SpanOrigin = newOrg;

            PolygonGradientBrush brush = ResolvePolygonGradientBrush(polygonGrBrush);

            //TODO: add gamma here...
            //aggsx.ScanlineRasterizer.ResetGamma(new GammaLinear(0.0f, this.LinearGamma)); //*** 

            int partCount = brush.CachePartCount;

            SetOrigin(newOrg.X, newOrg.Y);

            for (int i = 0; i < partCount; i++)
            {
                brush.SetSpanGenWithCurrentValues(i, _rgbaGourandSpanGen); //*** this affects assoc gouraudSpanGen 

                this.Fill(brush.CurrentVxs, _rgbaGourandSpanGen);
            }

            SetClipRgn(null);
            _rgbaGourandSpanGen.SpanOrigin = prevOrg;//restore

            SetOrigin(ox, oy);
        }
        public override void FillEllipse(double left, double top, double width, double height)
        {
            double ox = (left + width / 2);
            double oy = (top + height / 2);
            if (_orientation == RenderSurfaceOriginKind.LeftTop)
            {
                //modified
                oy = this.Height - oy;
            }

            //Agg
            //----------------------------------------------------------  
            using (Tools.BorrowEllipse(out var ellipseTool))
            using (Tools.BorrowVxs(out var v1))
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
            if (!(linearGr.InnerBrush is LinearGradientSpanGen linearGradientSpanGen))
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
            if (!(radialGr.InnerBrush is RadialGradientSpanGen radialSpanGen))
            {
                radialSpanGen = new RadialGradientSpanGen();
                radialSpanGen.ResolveBrush(radialGr);
                radialGr.InnerBrush = radialSpanGen;
            }
            return radialSpanGen;
        }
        public override void FillRect(double left, double top, double width, double height)
        {

            //Agg 
            //---------------------------------------------------------- 

            using (Tools.BorrowRect(out var rectTool))
            using (Tools.BorrowVxs(out var v1))
            {
                if (_orientation == RenderSurfaceOriginKind.LeftBottom)
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
                                Point prev_o = linearGradientSpanGen.SpanOrigin;
                                linearGradientSpanGen.SpanOrigin = new Point((int)(left + OriginX), (int)(top + OriginY));
                                Fill(rectTool.MakeVxs(v1), linearGradientSpanGen);
                                linearGradientSpanGen.SpanOrigin = prev_o;
                            }
                            break;
                        case BrushKind.CircularGraident:
                            {
                                //var radialGr = (Drawing.RadialGradientBrush)br;
                                RadialGradientSpanGen radialSpanGen = ResolveRadialGrBrush((Drawing.RadialGradientBrush)br);
                                Point prev_o = radialSpanGen.SpanOrigin;
                                radialSpanGen.SpanOrigin = new Point((int)(left + OriginX), (int)(top + OriginY));
                                Fill(rectTool.MakeVxs(v1), radialSpanGen);
                                radialSpanGen.SpanOrigin = prev_o;
                            }
                            break;
                        case BrushKind.PolygonGradient:
                            {
                                //we use mask technique (simlar to texture brush) 


                                Point prevOrg = _rgbaGourandSpanGen.SpanOrigin;
                                float ox = OriginX;
                                float oy = OriginY;

                                Point newOrg = new Point((int)(left + ox), (int)(top + oy));
                                _rgbaGourandSpanGen.SpanOrigin = newOrg;

                                PolygonGradientBrush brush = ResolvePolygonGradientBrush((Drawing.PolygonGradientBrush)br);

                                //TODO: add gamma here...
                                //aggsx.ScanlineRasterizer.ResetGamma(new GammaLinear(0.0f, this.LinearGamma)); //*** 

                                int partCount = brush.CachePartCount;

                                SetOrigin(newOrg.X, newOrg.Y);

                                for (int i = 0; i < partCount; i++)
                                {
                                    brush.SetSpanGenWithCurrentValues(i, _rgbaGourandSpanGen); //*** this affects assoc gouraudSpanGen 

                                    this.Fill(brush.CurrentVxs, _rgbaGourandSpanGen);
                                }

                                SetClipRgn(null);
                                _rgbaGourandSpanGen.SpanOrigin = prevOrg;//restore

                                SetOrigin(ox, oy);
                            }
                            break;
                        case BrushKind.Solid:
                            {
                                _aggsx.Render(rectTool.MakeVxs(v1), ((SolidBrush)br).Color);
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