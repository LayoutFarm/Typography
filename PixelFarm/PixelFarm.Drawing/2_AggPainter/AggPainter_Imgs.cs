//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;



namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {
        //image processing,


        void DrawBitmap(MemBitmap memBmp, double left, double top)
        {

            //save, restore later... 
            bool useSubPix = UseLcdEffectSubPixelRendering;
            //before render an image we turn off vxs subpixel rendering
            this.UseLcdEffectSubPixelRendering = false;
            _aggsx.UseSubPixelLcdEffect = false;

            if (_orientation == RenderSurfaceOriginKind.LeftTop)
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
            this.UseLcdEffectSubPixelRendering = useSubPix;
            _aggsx.UseSubPixelLcdEffect = useSubPix;
        }
        void DrawBitmap(MemBitmap memBmp, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {

            //save, restore later... 
            bool useSubPix = UseLcdEffectSubPixelRendering;
            //before render an image we turn off vxs subpixel rendering
            this.UseLcdEffectSubPixelRendering = false;

            if (_orientation == RenderSurfaceOriginKind.LeftTop)
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
            this.UseLcdEffectSubPixelRendering = useSubPix;
        }
        public override void DrawImage(Image img, double left, double top, int srcLeft, int srcTop, int srcW, int srcH)
        {
            if (!(img is MemBitmap memBmp))
            {
                //test with other bitmap 
                return;
            }
            else
            {
                DrawBitmap(memBmp, left, top, srcLeft, srcTop, srcW, srcH);
            }
        }
        public override void DrawImage(Image img, double left, double top)
        {
            if (!(img is MemBitmap memBmp))
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
            if (!(img is MemBitmap memBmp))
            {
                //? TODO
                return;
            }


            //-------------------------------
            bool useSubPix = UseLcdEffectSubPixelRendering; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseLcdEffectSubPixelRendering = false;

            _aggsx.Render(memBmp);
            //restore...
            this.UseLcdEffectSubPixelRendering = useSubPix;
        }
        public override void DrawImage(Image img, in AffineMat aff)
        {
            if (!(img is MemBitmap memBmp))
            {
                //? TODO
                return;
            }

            bool useSubPix = UseLcdEffectSubPixelRendering; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseLcdEffectSubPixelRendering = false;

            _aggsx.Render(memBmp, aff);
            //restore...
            this.UseLcdEffectSubPixelRendering = useSubPix;

        }
        public override void DrawImage(Image img, double left, double top, ICoordTransformer coordTx)
        {
            //draw img with transform coord
            //
            if (!(img is MemBitmap memBmp))
            {
                //? TODO
                return;
            }

            bool useSubPix = UseLcdEffectSubPixelRendering; //save, restore later... 
                                                   //before render an image we turn off vxs subpixel rendering
            this.UseLcdEffectSubPixelRendering = false;

            if (coordTx is Affine aff)
            {
                if (this.OriginX != 0 || this.OriginY != 0)
                {
                    coordTx = aff * Affine.NewTranslation(this.OriginX, this.OriginY);
                }
            }

            //_aggsx.SetScanlineRasOrigin(OriginX, OriginY);

            _aggsx.Render(memBmp, coordTx);

            //_aggsx.SetScanlineRasOrigin(xx, yy);
            //restore...
            this.UseLcdEffectSubPixelRendering = useSubPix;
        }
        public override void ApplyFilter(PixelFarm.Drawing.IImageFilter imgFilter)
        {
            //check if we can use this imgFilter
            if (!(imgFilter is PixelFarm.CpuBlit.PixelProcessing.ICpuBlitImgFilter cpuBlitImgFx)) return;
            // 
            cpuBlitImgFx.SetTarget(_aggsx.DestBitmapBlender);
            imgFilter.Apply();
        }

    }
}