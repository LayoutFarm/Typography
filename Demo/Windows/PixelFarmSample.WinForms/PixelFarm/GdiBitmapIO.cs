//Apache2, 2014-present, WinterDev

using System;
using PixelFarm.CpuBlit;
using System.IO;
namespace PixelFarm.Drawing.WinGdi
{
    public sealed partial class GdiBitmapIO : MemBitmapIO
    {
        public override MemBitmap LoadImage(string filename)
        {
            //resolve IO dest too!!

            using (System.IO.FileStream fs = new System.IO.FileStream(filename, FileMode.Open))
            {
                return LoadImage(fs);
            }
        }
        public override MemBitmap LoadImage(Stream input)
        {
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(input))
            {
                var bmpData2 = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                MemBitmap memBitmap = new MemBitmap(bmp.Width, bmp.Height);
                unsafe
                {
                    byte* dst = (byte*)MemBitmap.GetBufferPtr(memBitmap).Ptr;
                    PixelFarm.Drawing.Internal.MemMx.memcpy(dst, (byte*)bmpData2.Scan0, bmpData2.Stride * bmpData2.Height);
                }
                return memBitmap;
            }
        }
        public override void SaveImage(MemBitmap bitmap, Stream output, OutputImageFormat outputFormat, object saveParameters)
        {
            //save img to
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)MemBitmap.GetBufferPtr(bitmap).Ptr;
                    PixelFarm.Drawing.Internal.MemMx.memcpy((byte*)bmpdata.Scan0, ptr, bmpdata.Stride * bmp.Height);
                }
                bmp.UnlockBits(bmpdata);
                //save to stream
                System.Drawing.Imaging.ImageFormat format = null;
                switch (outputFormat)
                {
                    case OutputImageFormat.Default:
                        throw new NotSupportedException();
                    case OutputImageFormat.Jpeg:
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case OutputImageFormat.Png:
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                }
                bmp.Save(output, format);
            }
        }

        public override void SaveImage(MemBitmap bitmap, string filename, OutputImageFormat outputFormat, object saveParameters)
        {
            //TODO: resolve filename here!!!

            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)MemBitmap.GetBufferPtr(bitmap).Ptr;
                    PixelFarm.Drawing.Internal.MemMx.memcpy((byte*)bmpdata.Scan0, ptr, bmpdata.Stride * bmp.Height);
                }
                bmp.UnlockBits(bmpdata);

                //save to stream
                System.Drawing.Imaging.ImageFormat format = null;
                switch (outputFormat)
                {
                    case OutputImageFormat.Default:
                        throw new NotSupportedException();
                    case OutputImageFormat.Jpeg:
                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                        break;
                    case OutputImageFormat.Png:
                        format = System.Drawing.Imaging.ImageFormat.Png;
                        break;
                }
                bmp.Save(filename, format);
            }
        }
        public override MemBitmap ScaleImage(MemBitmap bitmap, float x_scale, float y_scale)
        {
            //scale 
            using (System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                unsafe
                {
                    byte* ptr = (byte*)MemBitmap.GetBufferPtr(bitmap).Ptr;
                    PixelFarm.Drawing.Internal.MemMx.memcpy((byte*)bmpdata.Scan0, ptr, bmpdata.Stride * bmp.Height);
                }
                bmp.UnlockBits(bmpdata);

                //
                int new_w = (int)Math.Round(bitmap.Width * x_scale);
                int new_h = (int)Math.Round(bitmap.Height * y_scale);
                if (new_w < 64 && new_h < 64)
                {
                    using (System.Drawing.Bitmap scaledBmp = (System.Drawing.Bitmap)bmp.GetThumbnailImage(new_w, new_h, null, IntPtr.Zero))
                    {
#if DEBUG
                        //scaledBmp.Save("thumb01.png");
#endif
                        //copy
                        var bmpdata2 = scaledBmp.LockBits(new System.Drawing.Rectangle(0, 0, new_w, new_h),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly,//***read
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        MemBitmap newMemBitmap = MemBitmap.CreateFromCopy(new_w, new_h, new_w * new_h * 4, bmpdata2.Scan0);
                        scaledBmp.UnlockBits(bmpdata2);
#if DEBUG
                        //newMemBitmap.SaveImage("thumb01_1.png");
#endif

                        return newMemBitmap;
                    }
                }
                else
                {
                    using (System.Drawing.Bitmap scaledBmp = new System.Drawing.Bitmap(bmp, new_w, new_h))
                    {
                        //copy
                        var bmpdata2 = scaledBmp.LockBits(new System.Drawing.Rectangle(0, 0, new_w, new_h),
                         System.Drawing.Imaging.ImageLockMode.ReadOnly, //***read
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                        MemBitmap newMemBitmap = MemBitmap.CreateFromCopy(new_w, new_h, new_w * new_h * 4, bmpdata2.Scan0);
                        scaledBmp.UnlockBits(bmpdata2);
                        return newMemBitmap;
                    }
                }
            }
        }
    }
}