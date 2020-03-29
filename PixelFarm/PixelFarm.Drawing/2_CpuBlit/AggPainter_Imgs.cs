//MIT, 2016-present, WinterDev

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.VertexProcessing;
using PixelFarm.CpuBlit.Imaging;

using BitmapBufferEx;
namespace PixelFarm.CpuBlit
{

    partial class AggPainter
    {
        //image processing,


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
            this.UseSubPixelLcdEffect = useSubPix;
        }
        public override void DrawImage(Image actualImage, double left, double top, int srcX, int srcY, int srcW, int srcH)
        {
            if (!(actualImage is MemBitmap memBmp))
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
            if (!(img is MemBitmap memBmp))
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
            if (!(actualImage is MemBitmap memBmp))
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
            this.UseSubPixelLcdEffect = useSubPix;
        }
        public override void ApplyFilter(PixelFarm.Drawing.IImageFilter imgFilter)
        {
            //check if we can use this imgFilter
            if (!(imgFilter is PixelFarm.CpuBlit.FragmentProcessing.ICpuBlitImgFilter cpuBlitImgFx)) return;
            // 
            cpuBlitImgFx.SetTarget(_aggsx.DestBitmapBlender);
            imgFilter.Apply();
        }

    }
}