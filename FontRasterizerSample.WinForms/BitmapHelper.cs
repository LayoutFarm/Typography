//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;
using NRasterizer;

using System.Drawing;
using System.Drawing.Imaging;

namespace PixelFarm.Agg
{
    

    public static class BitmapHelper
    {
        public static void CopyToWindowsBitmap(ImageReaderWriterBase backingImageBufferByte,
            Bitmap windowsBitmap,
            RectInt rect)
        {
            int offset = 0;
            byte[] buffer = backingImageBufferByte.GetBuffer();
            BitmapHelper.CopyToWindowsBitmap(buffer, offset,
                backingImageBufferByte.Stride, backingImageBufferByte.Height,
                backingImageBufferByte.BitDepth,
                windowsBitmap, rect);
        }
        public static void CopyToWindowsBitmap(ActualImage backingImageBufferByte,
           Bitmap windowsBitmap,
           RectInt rect)
        {
            int offset = 0;
            byte[] buffer = backingImageBufferByte.GetBuffer();
            BitmapHelper.CopyToWindowsBitmap(buffer, offset,
                backingImageBufferByte.Stride, backingImageBufferByte.Height,
                backingImageBufferByte.BitDepth,
                windowsBitmap, rect);
        }
        public static void CopyToWindowsBitmap(byte[] buffer, int offset,
          int sBackBufferStrideInBytes, int sHeight,
          int bitDepth,
          Bitmap windowsBitmap,
          RectInt rect)
        {
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          windowsBitmap.Width,
                          windowsBitmap.Height),
                          ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            int backBufferStrideInInts = sBackBufferStrideInBytes / 4;
            int backBufferHeight = sHeight;
            int backBufferHeightMinusOne = backBufferHeight - 1;
            int bitmapDataStride = bitmapData1.Stride;
            switch (bitDepth)
            {
                case 24:
                    {
                        throw new NotSupportedException();
                        //unsafe
                        //{
                        //    byte* bitmapDataScan0 = (byte*)bitmapData1.Scan0;
                        //    fixed (byte* pSourceFixed = &buffer[offset])
                        //    {
                        //        byte* pSource = pSourceFixed;
                        //        byte* pDestBuffer = bitmapDataScan0 + bitmapDataStride * backBufferHeightMinusOne;
                        //        for (int y = 0; y < backBufferHeight; y++)
                        //        {
                        //            int* pSourceInt = (int*)pSource;
                        //            int* pDestBufferInt = (int*)pDestBuffer;
                        //            for (int x = 0; x < backBufferStrideInInts; x++)
                        //            {
                        //                pDestBufferInt[x] = pSourceInt[x];
                        //            }
                        //            for (int x = backBufferStrideInInts * 4; x < sBackBufferStrideInBytes; x++)
                        //            {
                        //                pDestBuffer[x] = pSource[x];
                        //            }
                        //            pDestBuffer -= bitmapDataStride;
                        //            pSource += sBackBufferStrideInBytes;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case 32:
                    {
                        unsafe
                        {
                            byte* bitmapDataScan0 = (byte*)bitmapData1.Scan0;
                            fixed (byte* pSourceFixed = &buffer[offset])
                            {
                                byte* pSource = pSourceFixed;
                                byte* pDestBuffer = bitmapDataScan0 + bitmapDataStride * backBufferHeightMinusOne;
                                int rect_bottom = rect.Bottom;
                                int rect_top = rect.Top;
                                int rect_left = rect.Left;
                                int rect_right = rect.Right;
                                for (int y = rect_bottom; y < rect_top; y++)
                                {
                                    int* pSourceInt = (int*)pSource;
                                    pSourceInt += (sBackBufferStrideInBytes * y / 4);
                                    int* pDestBufferInt = (int*)pDestBuffer;
                                    pDestBufferInt -= (bitmapDataStride * y / 4);
                                    for (int x = rect_left; x < rect_right; x++)
                                    {
                                        pDestBufferInt[x] = pSourceInt[x];
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            windowsBitmap.UnlockBits(bitmapData1);
        }
        public static void CopyToWindowsBitmapSameSize(
            ActualImage actualImage,
            Bitmap windowsBitmap)
        {
            int h = windowsBitmap.Height;
            int w = windowsBitmap.Width;
            byte[] buffer = actualImage.GetBuffer();
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          w,
                          h),
                          System.Drawing.Imaging.ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            IntPtr scan0 = bitmapData1.Scan0;
            int stride = bitmapData1.Stride;
            unsafe
            {
                fixed (byte* bufferH = &buffer[0])
                {
                    byte* target = (byte*)scan0;
                    for (int y = h; y > 0; --y)
                    {
                        byte* src = bufferH + ((y - 1) * stride);
                        for (int n = stride - 1; n >= 0; --n)
                        {
                            *target = *src;
                            target++;
                            src++;
                        }
                    }
                }
            }

            windowsBitmap.UnlockBits(bitmapData1);
        }

        public static void CopyFromWindowsBitmapSameSize(
           Bitmap windowsBitmap,
           ActualImage actualImage)
        {
            int h = windowsBitmap.Height;
            int w = windowsBitmap.Width;
            byte[] buffer = actualImage.GetBuffer();
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          w,
                          h),
                          System.Drawing.Imaging.ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            IntPtr scan0 = bitmapData1.Scan0;
            int stride = bitmapData1.Stride;

            //TODO: review here 
            //use buffer copy

            unsafe
            {
                //target
                fixed (byte* targetH = &buffer[0])
                {
                    byte* src = (byte*)scan0;
                    for (int y = h; y > 0; --y)
                    {
                        byte* target = targetH + ((y - 1) * stride);
                        for (int n = stride - 1; n >= 0; --n)
                        {
                            *target = *src;
                            target++;
                            src++;
                        }
                    }
                }
            }
            windowsBitmap.UnlockBits(bitmapData1);
        }
    }
}