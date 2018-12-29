//MIT, 2016-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.PixelProcessing;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{

    public partial class AggPainter : Painter
    {

        AggRenderSurface _aggsx; //target rendering surface  
        BitmapBuffer _bxt;
        //--------------------
        AggRenderSurface _aggsx_0; //primary render surface

        //--------------------  
        SmoothingMode _smoothingMode;
        RenderQuality _renderQuality;
        RenderSurfaceOrientation _orientation;


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

        public DrawBoard DrawBoard { get; set; }
        public AggRenderSurface RenderSurface => _aggsx;
        public BitmapBlenderBase DestBitmapBlender => _aggsx.DestBitmapBlender;
        public override int Width => _aggsx.Width;
        public override int Height => _aggsx.Height;
        public override float OriginX => _aggsx.ScanlineRasOriginX;
        public override float OriginY => _aggsx.ScanlineRasOriginY;
        public override void Clear(Color color) => _aggsx.Clear(color);
        public override void SetOrigin(float x, float y) => _aggsx.SetScanlineRasOrigin(x, y);

        public override RenderQuality RenderQuality
        {
            get => _renderQuality;
            set => _renderQuality = value;
        }

        public override RenderSurfaceOrientation Orientation
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

            AggRenderSurface renderSx = new AggRenderSurface(bmp);
            if (blender == null)
            {
                blender = new PixelProcessing.PixelBlenderBGRA();
            }
            renderSx.PixelBlender = blender;

            return new AggPainter(renderSx);
        }
    }

}