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
        FilterMan _filterMan = new FilterMan();

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


    }
}