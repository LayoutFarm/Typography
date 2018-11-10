//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.Imaging
{
    /// <summary>
    /// agg buffer's pixel format
    /// </summary>
    public enum PixelFormat
    {
        ARGB32,
        RGB24,
        GrayScale8,
    }

    public struct TempMemPtr : IDisposable
    {
        int _lenInBytes; //in bytes 
        IntPtr _nativeBuffer;
        bool _isOwner;

        public TempMemPtr(IntPtr nativeBuffer32, int lenInBytes, bool isOwner = false)
        {
            this._lenInBytes = lenInBytes;
            _nativeBuffer = nativeBuffer32;
            _isOwner = isOwner;
        }
        public int LengthInBytes
        {
            get { return _lenInBytes; }
        }

        public IntPtr Ptr
        {
            get
            {
                return _nativeBuffer;
            }
        }
        public void Dispose()
        {
            if (_isOwner)
            {
                //destroy in
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_nativeBuffer);
                _nativeBuffer = IntPtr.Zero;
            }

        }


        //---------------
        //helper...
        public static TempMemPtr FromBmp(ActualBitmap actualBmp)
        {
            return ActualBitmap.GetBufferPtr(actualBmp);
        }
        public unsafe static TempMemPtr FromBmp(IBitmapSrc actualBmp, out int* headPtr)
        {
            TempMemPtr ptr = actualBmp.GetBufferPtr();
            headPtr = (int*)ptr.Ptr;
            return ptr;
        }

        public unsafe static TempMemPtr FromBmp(ActualBitmap actualBmp, out int* headPtr)
        {
            TempMemPtr ptr = ActualBitmap.GetBufferPtr(actualBmp);
            headPtr = (int*)ptr.Ptr;
            return ptr;
        }
        public unsafe static TempMemPtr FromBmp(ActualBitmap actualBmp, out byte* headPtr)
        {
            TempMemPtr ptr = ActualBitmap.GetBufferPtr(actualBmp);
            headPtr = (byte*)ptr.Ptr;
            return ptr;
        }
    }
}
namespace PixelFarm.CpuBlit
{

    public sealed class ActualBitmap : Image, IBitmapSrc
    {


        int width;
        int height;

        int _strideBytes;
        int bitDepth;
        CpuBlit.Imaging.PixelFormat pixelFormat;


        IntPtr _pixelBuffer;
        int _pixelBufferInBytes;
        bool _pixelBufferFromExternalSrc;

        public ActualBitmap(int width, int height)
        {
            //width and height must >0 
            this.width = width;
            this.height = height;
            int bytesPerPixel;
            this._strideBytes = CalculateStride(width,
                this.pixelFormat = CpuBlit.Imaging.PixelFormat.ARGB32, //***
                out bitDepth,
                out bytesPerPixel);

            //alloc mem ***
            _pixelBuffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(_pixelBufferInBytes = (width * height * 4));
            MemMx.memset_unsafe(_pixelBuffer, 0, _pixelBufferInBytes);
        }
        public ActualBitmap(int width, int height, int[] orgBuffer)
            : this(width, height)
        {
            //copy from managed buffer
            System.Runtime.InteropServices.Marshal.Copy(orgBuffer, 0, _pixelBuffer, _pixelBufferInBytes / 4);
        }
        public ActualBitmap(int width, int height, IntPtr externalNativeInt32Ptr)
            : this(width, height)
        {
            //width and height must >0 
            this.width = width;
            this.height = height;
            int bytesPerPixel;
            this._strideBytes = CalculateStride(width,
                this.pixelFormat = CpuBlit.Imaging.PixelFormat.ARGB32, //***
                out bitDepth,
                out bytesPerPixel);

            _pixelBufferFromExternalSrc = true;
            //alloc mem 
            _pixelBuffer = externalNativeInt32Ptr;
        }
        public override void Dispose()
        {
            if (_pixelBuffer != IntPtr.Zero && !_pixelBufferFromExternalSrc)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_pixelBuffer);
                _pixelBuffer = IntPtr.Zero;
                _pixelBufferInBytes = 0;
            }
        }
        public override int Width
        {
            get { return this.width; }
        }
        public override int Height
        {
            get { return this.height; }
        }
        public override int ReferenceX
        {
            get { return 0; }
        }
        public override int ReferenceY
        {
            get { return 0; }
        }
        public RectInt Bounds
        {
            get { return new RectInt(0, 0, this.width, this.height); }
        }
        public override bool IsReferenceImage
        {
            get { return false; }
        }

        public CpuBlit.Imaging.PixelFormat PixelFormat { get { return this.pixelFormat; } }
        public int Stride { get { return this._strideBytes; } }
        public int BitDepth { get { return this.bitDepth; } }
        public bool IsBigEndian { get; set; }


        public static CpuBlit.Imaging.TempMemPtr GetBufferPtr(ActualBitmap img)
        {
            return new CpuBlit.Imaging.TempMemPtr(img._pixelBuffer, img._pixelBufferInBytes);
        }

        public static void ReplaceBuffer(ActualBitmap img, int[] pixelBuffer)
        {
            System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, 0, img._pixelBuffer, pixelBuffer.Length);
        }
        public static ActualBitmap CreateFromCopy(int width, int height, int[] buffer)
        {
            var img = new ActualBitmap(width, height);
            unsafe
            {
                int* header = (int*)img._pixelBuffer;
                {
                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, (IntPtr)header, buffer.Length);
                }
            }
            return img;
        }
        public override void RequestInternalBuffer(ref ImgBufferRequestArgs buffRequest)
        {
            //TODO: review here 2018-08-26
            if (pixelFormat != CpuBlit.Imaging.PixelFormat.ARGB32)
            {
                throw new NotSupportedException();
            }


            int[] newBuff = new int[_pixelBufferInBytes / 4];
            System.Runtime.InteropServices.Marshal.Copy(_pixelBuffer, newBuff, 0, newBuff.Length);
            buffRequest.OutputBuffer32 = newBuff;
        }


        public static int CalculateStride(int width, CpuBlit.Imaging.PixelFormat format)
        {
            int bitDepth, bytesPerPixel;
            return CalculateStride(width, format, out bitDepth, out bytesPerPixel);
        }
        public static int CalculateStride(int width, CpuBlit.Imaging.PixelFormat format, out int bitDepth, out int bytesPerPixel)
        {
            //stride calcuation helper

            switch (format)
            {
                case CpuBlit.Imaging.PixelFormat.ARGB32:
                    {
                        bitDepth = 32;
                        bytesPerPixel = (bitDepth + 7) / 8;
                        return width * (32 / 8);
                    }
                case CpuBlit.Imaging.PixelFormat.GrayScale8:
                    {
                        bitDepth = 8; //bit per pixel
                        bytesPerPixel = (bitDepth + 7) / 8;
                        return 4 * ((width * bytesPerPixel + 3) / 4);
                    }
                case CpuBlit.Imaging.PixelFormat.RGB24:
                    {
                        bitDepth = 24; //bit per pixel
                        bytesPerPixel = (bitDepth + 7) / 8;
                        return 4 * ((width * bytesPerPixel + 3) / 4);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
        public static int[] CopyImgBuffer(ActualBitmap img)
        {

            int[] buff2 = new int[img.Width * img.Height];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr pixBuffer = ActualBitmap.GetBufferPtr(img))
                {
                    //fixed (byte* header = &pixelBuffer[0])
                    byte* header = (byte*)pixBuffer.Ptr;
                    {
                        System.Runtime.InteropServices.Marshal.Copy((IntPtr)header, buff2, 0, buff2.Length);//length in bytes
                    }
                }
            }

            return buff2;
        }
        int IBitmapSrc.BitDepth
        {
            get
            {
                return this.bitDepth;
            }
        }

        int IBitmapSrc.Width
        {
            get
            {
                return this.width;
            }
        }

        int IBitmapSrc.Height
        {
            get
            {
                return this.height;
            }
        }

        int IBitmapSrc.Stride
        {
            get
            {
                return this._strideBytes;
            }
        }
        int IBitmapSrc.BytesBetweenPixelsInclusive
        {
            get { return 4; }
        }
        RectInt IBitmapSrc.GetBounds()
        {
            return new RectInt(0, 0, width, height);
        }

        CpuBlit.Imaging.TempMemPtr IBitmapSrc.GetBufferPtr()
        {
            return new CpuBlit.Imaging.TempMemPtr(_pixelBuffer, _pixelBufferInBytes);
        }

        int IBitmapSrc.GetBufferOffsetXY32(int x, int y)
        {
            return (y * width) + x;
        }

        void IBitmapSrc.ReplaceBuffer(int[] newBuffer)
        {
            //TODO: review here 2018-08-26
            //pixelBuffer = newBuffer;
            System.Runtime.InteropServices.Marshal.Copy(newBuffer, 0, _pixelBuffer, newBuffer.Length);
        }

        Color IBitmapSrc.GetPixel(int x, int y)
        {
            unsafe
            {
                int* pxBuff = (int*)_pixelBuffer;
                int pixelValue = pxBuff[y * width + x];
                return new Color(
                  (byte)(pixelValue >> 24),
                  (byte)(pixelValue >> 16),
                  (byte)(pixelValue >> 8),
                  (byte)(pixelValue));
            }

        }
    }



    public interface IBitmapSrc
    {
        int BitDepth { get; }
        int Width { get; }
        int Stride { get; }
        int Height { get; }

        RectInt GetBounds();

        int GetBufferOffsetXY32(int x, int y);

        Imaging.TempMemPtr GetBufferPtr();


        int BytesBetweenPixelsInclusive { get; }
        void ReplaceBuffer(int[] newBuffer);
        Color GetPixel(int x, int y);
    }

    public static class ActualBitmapExtensions
    {
        public static int[] CopyImgBuffer(ActualBitmap img, int width)
        {
            //calculate stride for the width

            int destStride = ActualBitmap.CalculateStride(width, CpuBlit.Imaging.PixelFormat.ARGB32);
            int h = img.Height;
            int newBmpW = destStride / 4;

            int[] buff2 = new int[newBmpW * img.Height];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = ActualBitmap.GetBufferPtr(img))
                {
                    byte* srcBuffer = (byte*)srcBufferPtr.Ptr;
                    int srcIndex = 0;
                    int srcStride = img.Stride;
                    fixed (int* destHead = &buff2[0])
                    {
                        byte* destHead2 = (byte*)destHead;
                        for (int line = 0; line < h; ++line)
                        {
                            //System.Runtime.InteropServices.Marshal.Copy(srcBuffer, srcIndex, (IntPtr)destHead2, destStride);
                            NativeMemMx.memcpy((byte*)destHead2, srcBuffer + srcIndex, destStride);
                            srcIndex += srcStride;
                            destHead2 += destStride;
                        }
                    }
                }
            }
            return buff2;
        }

        public static int[] CopyImgBuffer(ActualBitmap src, int srcX, int srcY, int srcW, int srcH)
        {
            //calculate stride for the width 
            int destStride = ActualBitmap.CalculateStride(srcW, CpuBlit.Imaging.PixelFormat.ARGB32);
            int newBmpW = destStride / 4;

            int[] buff2 = new int[newBmpW * srcH];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = ActualBitmap.GetBufferPtr(src))
                {
                    byte* srcBuffer = (byte*)srcBufferPtr.Ptr;
                    int srcIndex = 0;
                    int srcStride = src.Stride;
                    fixed (int* destHead = &buff2[0])
                    {
                        byte* destHead2 = (byte*)destHead;

                        //move to specific src line
                        srcIndex += srcStride * srcY;

                        int lineEnd = srcY + srcH;
                        for (int line = srcY; line < lineEnd; ++line)
                        {
                            //System.Runtime.InteropServices.Marshal.Copy(srcBuffer, srcIndex, (IntPtr)destHead2, destStride);
                            NativeMemMx.memcpy((byte*)destHead2, srcBuffer + srcIndex, destStride);
                            srcIndex += srcStride;
                            destHead2 += destStride;
                        }
                    }
                }
            }

            return buff2;
        }
    }

    public delegate void ImageEncodeDelegate(byte[] img, int pixelWidth, int pixelHeight);
    public delegate void ImageDecodeDelegate(byte[] img);

    public static class ExternalImageService
    {
        static ImageEncodeDelegate s_imgEncodeDel;
        static ImageDecodeDelegate s_imgDecodeDel;

        public static bool HasExternalImgCodec
        {
            get
            {
                return s_imgEncodeDel != null;
            }
        }
        public static void RegisterExternalImageEncodeDelegate(ImageEncodeDelegate imgEncodeDel)
        {
            s_imgEncodeDel = imgEncodeDel;
        }
        public static void RegisterExternalImageDecodeDelegate(ImageDecodeDelegate imgDecodeDel)
        {
            s_imgDecodeDel = imgDecodeDel;
        }
        public static void SaveImage(byte[] img, int pixelWidth, int pixelHeight)
        {
            //temp, save as png image
            if (s_imgEncodeDel != null)
            {
                s_imgEncodeDel(img, pixelWidth, pixelHeight);
            }
        }
    }
}