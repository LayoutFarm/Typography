//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;

using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.PixelProcessing;

namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {
        AggRenderSurface _aggsx_mask;

        TargetBufferName _targetBufferName;
        bool _enableBuiltInMaskComposite;
        MemBitmap _alphaBitmap;

        PixelBlender32 _defaultPixelBlender;
        PixelBlenderWithMask _maskPixelBlender;
        PixelBlenderPerColorComponentWithMask _maskPixelBlenderPerCompo;
        ClipingTechnique _currentClipTech;

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
                        (int)Math.Floor(clipRect.Left), (int)Math.Floor(clipRect.Top),
                        (int)Math.Ceiling(clipRect.Right), (int)Math.Ceiling(clipRect.Bottom));

                    _currentClipTech = ClipingTechnique.ClipSimpleRect;
                }
                else
                {
                    //not simple rect => 
                    //use mask technique

                    _currentClipTech = ClipingTechnique.ClipMask;
                    //1. switch to mask buffer
                    this.TargetBufferName = TargetBufferName.AlphaMask;
                    //2.
                    Color prevColor = this.FillColor; //save

                    this.FillColor = Color.White;
                    _aggsx.Render(vxs, FillColor);

                    //fill vxs with white color (on black bg)

                    this.FillColor = prevColor; //restore
                    //3. switch back to default layer
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
        public override Rectangle ClipBox
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
        //public override RectInt ClipBox
        //{
        //    get => _aggsx.GetClippingRect();
        //    set => _aggsx.SetClippingRect(value);
        //}
        public override void SetClipBox(int x1, int y1, int x2, int y2)
        {
            _aggsx.SetClippingRect(new Q1Rect(x1, y1, x2, y2));
        }
        //---------------------------------------------------------------

        void SetupMaskPixelBlender()
        {
            //create when need and
            //after _aggsx_0 is attach to the surface
            if (_aggsx_mask != null)
            {
                //also set the canvas origin for the aggsx_mask
                _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY);
                return;//***
            }
            //----------
            //same size as primary _aggsx_0 

            _alphaBitmap = new MemBitmap(_aggsx_0.Width, _aggsx_0.Height);

            _aggsx_mask = new AggRenderSurface() { PixelBlender = new PixelBlenderBGRA() };
            _aggsx_mask.AttachDstBitmap(_alphaBitmap);
            _aggsx_mask.SetScanlineRasOrigin(this.OriginX, this.OriginY); //also set the canvas origin for the aggsx_mask
#if DEBUG
            _aggsx_mask.dbugName = "mask";
            _alphaBitmap._dbugNote = "AggPrinter.SetupMaskPixelBlender";
#endif
            _maskPixelBlender = new PixelBlenderWithMask();
            _maskPixelBlenderPerCompo = new PixelBlenderPerColorComponentWithMask();

            _maskPixelBlender.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
            _maskPixelBlenderPerCompo.SetMaskBitmap(_alphaBitmap); //same alpha bitmap
        }
        void DetachMaskPixelBlender()
        {
            if (_aggsx_mask != null)
            {
                _aggsx_mask.DetachDstBitmap();
                _aggsx_mask = null;

                _maskPixelBlender = null; //remove blender
                _maskPixelBlenderPerCompo = null;
            }
            if (_alphaBitmap != null)
            {
                _alphaBitmap.Dispose();
                _alphaBitmap = null;
            }

        }
        void UpdateTargetBuffer(TargetBufferName value)
        {
            //
            _targetBufferName = value;

            if (_aggsx.DestBitmap != null)
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
                //TempMemPtr tmp = MemBitmap.GetBufferPtr(_aggsx.DestBitmap);
                //unsafe
                //{
                //    _bxt = new BitmapBuffer(
                //       _aggsx.Width,
                //       _aggsx.Height,
                //        tmp.Ptr,
                //        tmp.LengthInBytes);
                //}
            }
        }

        public TargetBufferName TargetBufferName
        {
            get => _targetBufferName;
            set
            {
                if (_targetBufferName == value) { return; }
                //
                UpdateTargetBuffer(value);
            }
        }
        public bool EnableBuiltInMaskComposite
        {
            get => _enableBuiltInMaskComposite;
            set
            {
                if (_enableBuiltInMaskComposite == value) { return; }
                //
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

        public override void Fill(Region rgn)
        {
            var region = rgn as CpuBlitRegion;
            if (region == null) return;
            switch (region.Kind)
            {
                case CpuBlitRegion.CpuBlitRegionKind.BitmapBasedRegion:
                    {
                        var bmpRgn = (PixelFarm.PathReconstruction.BitmapBasedRegion)region;
                        //for bitmap that is used to be a region...
                        //our convention is ...
                        //  non-region => black
                        //  region => white                        
                        //(same as the Typography GlyphTexture)
                        MemBitmap rgnBitmap = bmpRgn.GetRegionBitmap();
                        DrawImage(rgnBitmap);
                    }
                    break;
                case CpuBlitRegion.CpuBlitRegionKind.VxsRegion:
                    {
                        //fill 'hole' of the region
                        var vxsRgn = (PixelFarm.PathReconstruction.VxsRegion)region;
                        Fill(vxsRgn.GetVxs());
                    }
                    break;
                case CpuBlitRegion.CpuBlitRegionKind.MixedRegion:
                    {
                        var mixedRgn = (PixelFarm.PathReconstruction.MixedRegion)region;
                    }
                    break;
            }

        }
        public override void Draw(Region rgn)
        {
            var region = rgn as PixelFarm.CpuBlit.CpuBlitRegion;
            if (region == null) return;
            switch (region.Kind)
            {
                case CpuBlitRegion.CpuBlitRegionKind.BitmapBasedRegion:
                    {
                        var bmpRgn = (PixelFarm.PathReconstruction.BitmapBasedRegion)region;
                        //check if it has outline data or not
                        //if not then just return


                    }
                    break;
                case CpuBlitRegion.CpuBlitRegionKind.VxsRegion:
                    {
                        //draw outline of the region
                        var vxsRgn = (PixelFarm.PathReconstruction.VxsRegion)region;
                        Draw(vxsRgn.GetVxs());
                    }
                    break;
                case CpuBlitRegion.CpuBlitRegionKind.MixedRegion:
                    {
                        var mixedRgn = (PixelFarm.PathReconstruction.MixedRegion)region;
                    }
                    break;
            }
        }

    }

    public enum TargetBufferName
    {
        Unknown,
        Default,
        AlphaMask
    }
}