//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.PixelProcessing;

namespace PixelFarm.CpuBlit
{

    public partial class AggPainter : Painter
    {

        AggRenderSurface _aggsx; //target rendering surface   
        AggRenderSurface _aggsx_0; //primary render surface

        //--------------------  
        SmoothingMode _smoothingMode;

        RenderSurfaceOriginKind _orientation;
        TargetBuffer _targetBuffer;
        float _fillOpacity = 1;
        bool _hasFillOpacity = false;

        public AggPainter(AggRenderSurface aggsx)
        {
            //painter paint to target surface
            _orientation = RenderSurfaceOriginKind.LeftBottom;
            //----------------------------------------------------
            _aggsx = _aggsx_0 = aggsx; //set this as default *** 

            _aggsx_0.DstBitmapAttached += (s, e) =>
            {
                UpdateTargetBuffer(_targetBufferName);
            };
            _aggsx_0.DstBitmapDetached += (s, e) =>
            {
                DetachMaskPixelBlender();
            };

            TargetBufferName = TargetBufferName.Default;
            _stroke = new Stroke(1);//default
            _useDefaultBrush = true;
            _defaultPixelBlender = this.DestBitmapBlender.OutputPixelBlender;
        }

        public void Reset()
        {
            //TODO: ...
            //reset to init state
            //
            FillingRule = FillingRule.NonZero;
        }
        public override FillingRule FillingRule
        {
            //TODO: set filling for both aggsx (default and mask)
            get => _aggsx.FillingRule;
            set => _aggsx.FillingRule = value;
        }
        public override float FillOpacity
        {
            get => _fillOpacity;
            set
            {
                _fillOpacity = value;
                if (value < 0)
                {
                    _fillOpacity = 0;
                    _hasFillOpacity = true;
                }
                else if (value >= 1)
                {
                    _fillOpacity = 1;
                    _hasFillOpacity = false;
                }
                else
                {
                    _fillOpacity = value;
                    _hasFillOpacity = true;
                }
            }
        }


        public override TargetBuffer TargetBuffer
        {
            get => _targetBuffer;
            set
            {
                if (_targetBuffer == value) return;

                _targetBuffer = value;
                switch (value)
                {
                    case TargetBuffer.ColorBuffer:
                        this.TargetBufferName = TargetBufferName.Default;
                        break;
                    case TargetBuffer.MaskBuffer:
                        this.TargetBufferName = TargetBufferName.AlphaMask;
                        break;
                    default: throw new NotSupportedException();
                }
            }
        }
        public override bool EnableMask
        {
            get => EnableBuiltInMaskComposite;
            set => EnableBuiltInMaskComposite = value;
        }
        public override ICoordTransformer CoordTransformer
        {
            get => _aggsx.CurrentTransformMatrix;
            set => _aggsx.CurrentTransformMatrix = value;
        }

        public DrawBoard DrawBoard { get; set; }
        public AggRenderSurface RenderSurface => _aggsx;
        public BitmapBlenderBase DestBitmapBlender => _aggsx.DestBitmapBlender;
        public override int Width => _aggsx.Width;
        public override int Height => _aggsx.Height;
        public override float OriginX => _aggsx.ScanlineRasOriginX;
        public override float OriginY => _aggsx.ScanlineRasOriginY;
        public override void Clear(Color color) => _aggsx.Clear(color);
        public override void SetOrigin(float x, float y) => _aggsx.SetScanlineRasOrigin(x, y);

        public override RenderQuality RenderQuality { get; set; }

        public override RenderSurfaceOriginKind Orientation
        {
            get => _orientation;
            set => _orientation = value;
        }
        public override SmoothingMode SmoothingMode
        {
            get => _smoothingMode;
            set
            {
                switch (_smoothingMode = value)
                {
                    case Drawing.SmoothingMode.HighQuality:
                    case Drawing.SmoothingMode.AntiAlias:
                        //TODO: review here
                        //anti alias != lcd technique 
                        this.RenderQuality = RenderQuality.HighQuality;
                        //_aggsx.UseSubPixelLcdEffect = true;
                        break;
                    case Drawing.SmoothingMode.HighSpeed:
                    default:
                        this.RenderQuality = RenderQuality.Fast;
                        _aggsx.UseSubPixelLcdEffect = false;
                        break;
                }
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
        public override RenderVx CreateRenderVx(VertexStore vxs)
        {
            return new AggRenderVx(vxs);
        }


        public static AggPainter Create(MemBitmap bmp, PixelProcessing.PixelBlender32 blender = null)
        {
            //helper func

            AggRenderSurface renderSx = new AggRenderSurface();
            renderSx.AttachDstBitmap(bmp);

            if (blender == null)
            {
                blender = new PixelProcessing.PixelBlenderBGRA();
            }
            renderSx.PixelBlender = blender;

            return new AggPainter(renderSx);
        }
    }

}