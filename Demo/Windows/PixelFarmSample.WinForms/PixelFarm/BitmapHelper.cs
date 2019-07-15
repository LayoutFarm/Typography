//MIT, 2014-2016, WinterDev

using System;
using System.Drawing;
using System.Drawing.Imaging;
using PixelFarm.CpuBlit.Imaging;

namespace PixelFarm.CpuBlit
{
    static class BitmapHelper
    {
        public static void CopyToGdiPlusGraphics(this AggPainter painter, Graphics g, int xpos = 0, int ypos = 0)
        {
            using (Bitmap gdiBmp = new Bitmap(painter.Width, painter.Height))
            {
                CopyToGdiPlusBitmapSameSize(painter.RenderSurface.DestBitmap, gdiBmp);
                g.DrawImage(gdiBmp, new System.Drawing.Point(10, 0));
            }
        }
        /// <summary>
        /// copy from MemBitmap to hBmpScan0
        /// </summary>
        /// <param name="srcMemBmp"></param>
        /// <param name="dstHScan0"></param>
        public static void CopyToWindowsBitmapSameSize(
           MemBitmap srcMemBmp,
           IntPtr dstHScan0)
        {
            //1st, fast
            //byte[] rawBuffer = ActualImage.GetBuffer(actualImage);
            unsafe
            {
                TempMemPtr memPtr = MemBitmap.GetBufferPtr(srcMemBmp);
                MemMx.memcpy((byte*)dstHScan0, (byte*)memPtr.Ptr, srcMemBmp.Stride * srcMemBmp.Height);
                memPtr.Dispose();
            }
            //System.Runtime.InteropServices.Marshal.Copy(rawBuffer, 0,
            //   hBmpScan0, rawBuffer.Length);
        }



        /////////////////////////////////////////////////////////////////////////////////////

        public static void CopyToGdiPlusBitmapSameSize(
            MemBitmap srcMemBmp,
            Bitmap dstBmp)
        {
            //agg store image buffer head-down
            //when copy to window bmp we here to flip 
            //style1: copy row by row *** (fastest)***
            {
                //System.GC.Collect();
                //System.Diagnostics.Stopwatch sss = new System.Diagnostics.Stopwatch();
                //sss.Start();
                //for (int i = 0; i < 1000; ++i)
                //{
                int h = dstBmp.Height;
                int w = dstBmp.Width;
                BitmapData bitmapData1 = dstBmp.LockBits(
                          new Rectangle(0, 0,
                              w,
                              h),
                              System.Drawing.Imaging.ImageLockMode.ReadWrite,
                              dstBmp.PixelFormat);
                IntPtr scan0 = bitmapData1.Scan0;
                int stride = bitmapData1.Stride;
                //byte[] srcBuffer = ActualImage.GetBuffer(actualImage);
                TempMemPtr srcMemPtr = MemBitmap.GetBufferPtr(srcMemBmp);
                unsafe
                {

                    //byte* bufferH = (byte*)srcMemPtr.Ptr;
                    //{
                    //    byte* target = (byte*)scan0;
                    //    int startRowAt = ((h - 1) * stride);
                    //    for (int y = h; y > 0; --y)
                    //    {
                    //        //byte* src = bufferH + ((y - 1) * stride);
                    //        //System.Runtime.InteropServices.Marshal.Copy(
                    //        //   srcBuffer,//src
                    //        //   startRowAt,
                    //        //   (IntPtr)target,
                    //        //   stride);

                    //        MemMx.memcpy(target, bufferH + startRowAt, stride);
                    //        startRowAt -= stride;
                    //        target += stride;
                    //    }
                    //}

                    byte* bufferH = (byte*)srcMemPtr.Ptr;
                    {
                        byte* target = (byte*)scan0;
                        int startRowAt = 0;
                        for (int y = 0; y < h; ++y)
                        {
                            //byte* src = bufferH + ((y - 1) * stride);
                            //System.Runtime.InteropServices.Marshal.Copy(
                            //   srcBuffer,//src
                            //   startRowAt,
                            //   (IntPtr)target,
                            //   stride);

                            MemMx.memcpy(target, bufferH + startRowAt, stride);
                            startRowAt += stride;
                            target += stride;
                        }
                    }

                }
                srcMemPtr.Dispose();
                dstBmp.UnlockBits(bitmapData1);
                //}
                //sss.Stop();
                //long ms = sss.ElapsedMilliseconds;
            }
            //-----------------------------------
            //style2: copy all, then flip again
            //{
            //    System.GC.Collect();
            //    System.Diagnostics.Stopwatch sss = new System.Diagnostics.Stopwatch();
            //    sss.Start();
            //    for (int i = 0; i < 1000; ++i)
            //    {
            //        byte[] rawBuffer = ActualImage.GetBuffer(actualImage);
            //        var bmpdata = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
            //          System.Drawing.Imaging.ImageLockMode.ReadOnly,
            //         bitmap.PixelFormat);


            //        System.Runtime.InteropServices.Marshal.Copy(rawBuffer, 0,
            //            bmpdata.Scan0, rawBuffer.Length);

            //        bitmap.UnlockBits(bmpdata);
            //        bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //    }

            //    sss.Stop();
            //    long ms = sss.ElapsedMilliseconds; 
            //}
            //-----------------------------------

            //-----------------------------------
            //style3: copy row by row + 
            //{
            //    System.GC.Collect();
            //    System.Diagnostics.Stopwatch sss = new System.Diagnostics.Stopwatch();
            //    sss.Start();
            //    for (int i = 0; i < 1000; ++i)
            //    {
            //        int h = bitmap.Height;
            //        int w = bitmap.Width;
            //        BitmapData bitmapData1 = bitmap.LockBits(
            //                  new Rectangle(0, 0,
            //                      w,
            //                      h),
            //                      System.Drawing.Imaging.ImageLockMode.ReadWrite,
            //                      bitmap.PixelFormat);
            //        IntPtr scan0 = bitmapData1.Scan0;
            //        int stride = bitmapData1.Stride;
            //        byte[] buffer = ActualImage.GetBuffer(actualImage);
            //        unsafe
            //        {
            //            fixed (byte* bufferH = &buffer[0])
            //            {
            //                byte* target = (byte*)scan0;
            //                for (int y = h; y > 0; --y)
            //                {
            //                    byte* src = bufferH + ((y - 1) * stride);
            //                    for (int n = stride - 1; n >= 0; --n)
            //                    {
            //                        *target = *src;
            //                        target++;
            //                        src++;
            //                    }
            //                }
            //            }
            //        }
            //        bitmap.UnlockBits(bitmapData1);
            //    }
            //    sss.Stop();
            //    long ms = sss.ElapsedMilliseconds;
            //} 
        }

        public static void CopyFromGdiPlusBitmapSameSize(
           Bitmap srcBmp,
           MemBitmap dstMemBmp)
        {
            int h = srcBmp.Height;
            int w = srcBmp.Width;
         
            BitmapData bitmapData1 = srcBmp.LockBits(
                      new Rectangle(0, 0,
                          w,
                          h),
                          System.Drawing.Imaging.ImageLockMode.ReadWrite,
                          srcBmp.PixelFormat);
            IntPtr scan0 = bitmapData1.Scan0;
            int stride = bitmapData1.Stride;

            //TODO: review here 
            //use buffer copy

            unsafe
            {
                //target 
                TempMemPtr targetMemPtr = MemBitmap.GetBufferPtr(dstMemBmp);
                byte* target = (byte*)targetMemPtr.Ptr;
                int startRowAt = ((h - 1) * stride);
                byte* src = (byte*)scan0;
                for (int y = h; y > 0; --y)
                {
                    // byte* target = targetH + ((y - 1) * stride); 
                    //System.Runtime.InteropServices.Marshal.Copy(
                    //      (IntPtr)src,//src
                    //      targetBuffer, startRowAt, stride);
                    MemMx.memcpy(target + startRowAt, src, stride);
                    startRowAt -= stride;
                    src += stride;
                }
                targetMemPtr.Dispose();
                //////////////////////////////////////////////////////////////////
                //fixed (byte* targetH = &targetBuffer[0])
                //{
                //    byte* src = (byte*)scan0;
                //    for (int y = h; y > 0; --y)
                //    {
                //        byte* target = targetH + ((y - 1) * stride);
                //        for (int n = stride - 1; n >= 0; --n)
                //        {
                //            *target = *src;
                //            target++;
                //            src++;
                //        }
                //    }
                //}
            }
            srcBmp.UnlockBits(bitmapData1);
        }
         
    }
}
