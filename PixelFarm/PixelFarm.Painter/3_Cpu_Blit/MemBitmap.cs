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
            _lenInBytes = lenInBytes;
            _nativeBuffer = nativeBuffer32;
            _isOwner = isOwner;
        }
        //
        public int LengthInBytes => _lenInBytes;
        //
        public IntPtr Ptr => _nativeBuffer;
        //
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
        public static TempMemPtr FromBmp(MemBitmap memBmp)
        {
            return MemBitmap.GetBufferPtr(memBmp);
        }
        public unsafe static TempMemPtr FromBmp(IBitmapSrc actualBmp, out int* headPtr)
        {
            TempMemPtr ptr = actualBmp.GetBufferPtr();
            headPtr = (int*)ptr.Ptr;
            return ptr;
        }

        public unsafe static TempMemPtr FromBmp(MemBitmap memBmp, out int* headPtr)
        {
            TempMemPtr ptr = MemBitmap.GetBufferPtr(memBmp);
            headPtr = (int*)ptr.Ptr;
            return ptr;
        }
        public unsafe static TempMemPtr FromBmp(MemBitmap bmp, out byte* headPtr)
        {
            TempMemPtr ptr = MemBitmap.GetBufferPtr(bmp);
            headPtr = (byte*)ptr.Ptr;
            return ptr;
        }
    }
}
namespace PixelFarm.CpuBlit
{



#if DEBUG

    static class dbugMemBitmapMonitor
    {
        class TempMemBitmapMonitor
        {
            //public WeakReference _memBmp;
            public MemBitmap _memBmp;
            public string _detail;
            public TempMemBitmapMonitor(string detail) => _detail = detail;
            public bool CanBeReleased() => InternalNativePtrIsReleased();
            public override string ToString() => (CanBeReleased() ? "!" : "") + _detail;
            public bool InternalNativePtrIsReleased()
            {
                if (_memBmp != null)
                {
                    return _memBmp.IsDisposed();
                }
                //if (_memBmp.IsAlive)
                //{
                //    return ((MemBitmap)_memBmp.Target).IsDisposed();
                //}
                return true;
            }
        }
        static System.Text.StringBuilder s_stbuilder = new System.Text.StringBuilder();
        static object s_regLock = new object();
        static System.Timers.Timer s_tim1;
        static int s_count;
        static System.Collections.Generic.List<TempMemBitmapMonitor> _registerMemBmpList = new System.Collections.Generic.List<TempMemBitmapMonitor>();
        static System.Collections.Generic.List<int> _tempToBeRemovedList = new System.Collections.Generic.List<int>();
        public static int dbugRegisterMemBitmapCount()
        {
            lock (s_regLock)
            {
                return _registerMemBmpList.Count;
            }
        }

        public static void dbugRegisterMemBitmap(MemBitmap memBmp, string detail)
        {
            if (s_tim1 == null)
            {
                s_tim1 = new System.Timers.Timer();
                s_tim1.Interval = 10000; //10 sec
                s_tim1.Elapsed += (s, e) =>
                {
                    //check the report

                    lock (s_regLock)
                    {
                        s_tim1.Enabled = false;
                        s_stbuilder.AppendLine((s_count++).ToString());

                        int reg_count = _registerMemBmpList.Count;
                        _tempToBeRemovedList.Clear();
                        //
                        for (int i = 0; i < reg_count; ++i)
                        {
                            TempMemBitmapMonitor tmpBmpMonitor = _registerMemBmpList[i];

                            if (tmpBmpMonitor.CanBeReleased())
                            {
                                //remove
                                _tempToBeRemovedList.Add(i);
                            }
                            else
                            {
                                s_stbuilder.Append(tmpBmpMonitor._detail);
                                //
                                if (tmpBmpMonitor._memBmp != null && tmpBmpMonitor._memBmp._dbugNote != null)
                                {
                                    s_stbuilder.Append(" ");
                                    s_stbuilder.Append(tmpBmpMonitor._memBmp._dbugNote);
                                }
                                //
                                s_stbuilder.AppendLine();
                            }
                        }
                        for (int i = _tempToBeRemovedList.Count - 1; i >= 0; --i)
                        {
                            _registerMemBmpList.RemoveAt(_tempToBeRemovedList[i]);
                        }

                        _tempToBeRemovedList.Clear();

                        s_stbuilder.AppendLine("remaing : " + _registerMemBmpList.Count);
                        s_stbuilder.AppendLine("---");
                        s_stbuilder.AppendLine();

                        //
                        System.Diagnostics.Debug.Write(s_stbuilder.ToString());
                        //
                        s_stbuilder.Length = 0;//clear
                        s_tim1.Enabled = true;
                    }
                };
                s_tim1.Enabled = true;
            }
            //
            //
            lock (s_regLock)
            {
                _registerMemBmpList.Add(new TempMemBitmapMonitor(detail) { _memBmp = memBmp }); //_memBmp = new WeakReference(memBmp) });
            }
        }
    }
#endif



    /// <summary>
    /// 32 bpp native memory bitmap
    /// </summary>
    public sealed class MemBitmap : Image, IBitmapSrc
    {

        int _width;
        int _height;

        int _strideBytes;
        int _bitDepth;
        CpuBlit.Imaging.PixelFormat _pixelFormat;
        IntPtr _pixelBuffer;
        int _pixelBufferInBytes;
        bool _pixelBufferFromExternalSrc;
        bool _isDisposed;

#if DEBUG
        public string _dbugNote;
#endif
        public MemBitmap(int width, int height)
            : this(width, height, System.Runtime.InteropServices.Marshal.AllocHGlobal(width * height * 4))
        {
            _pixelBufferFromExternalSrc = false;//** if we alloc then we are the owner of this MemBmp
            MemMx.memset_unsafe(_pixelBuffer, 0, _pixelBufferInBytes); //set 
        }
        public MemBitmap(int width, int height, IntPtr externalNativeInt32Ptr)
        {
            //width and height must >0 
            _width = width;
            _height = height;
            _strideBytes = CalculateStride(width,
                _pixelFormat = CpuBlit.Imaging.PixelFormat.ARGB32, //***
                out _bitDepth,
                out int bytesPerPixel);

            _pixelBufferInBytes = width * height * 4;
            _pixelBufferFromExternalSrc = true; //*** we receive ptr from external ***
            _pixelBuffer = externalNativeInt32Ptr;

#if DEBUG
            dbugMemBitmapMonitor.dbugRegisterMemBitmap(this, width + "x" + height + ": " + DateTime.Now.ToString("u"));
#endif
        }
        public override void Dispose()
        {
            if (_pixelBuffer != IntPtr.Zero && !_pixelBufferFromExternalSrc)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_pixelBuffer);
                _pixelBuffer = IntPtr.Zero;
                _pixelBufferInBytes = 0;
                _isDisposed = true;
            }
        }
        public bool IsDisposed() => _isDisposed;
        //
        public override int Width => _width;
        public override int Height => _height;
        //
        public override int ReferenceX => 0;
        public override int ReferenceY => 0;
        //
        public RectInt Bounds => new RectInt(0, 0, _width, _height);
        public override bool IsReferenceImage => false;
        public CpuBlit.Imaging.PixelFormat PixelFormat => _pixelFormat;
        //
        public int Stride => _strideBytes;
        public int BitDepth => _bitDepth;
        //
        public bool IsBigEndian { get; set; }
        public static CpuBlit.Imaging.TempMemPtr GetBufferPtr(MemBitmap bmp)
        {
            return new CpuBlit.Imaging.TempMemPtr(bmp._pixelBuffer, bmp._pixelBufferInBytes);
        }

        public static void ReplaceBuffer(MemBitmap bmp, int[] pixelBuffer)
        {
            System.Runtime.InteropServices.Marshal.Copy(pixelBuffer, 0, bmp._pixelBuffer, pixelBuffer.Length);
        }
        /// <summary>
        /// create mem bitmap by copy data from managed int32 array pixel data to unmanged side
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="totalBuffer"></param>
        /// <param name="doFlipY"></param>
        /// <returns></returns>
        public static MemBitmap CreateFromCopy(int width, int height, int[] totalBuffer, bool doFlipY = false)
        {

            var bmp = new MemBitmap(width, height);
#if DEBUG
            bmp._dbugNote = "MemBitmap.CreateFromCopy";
#endif
            if (doFlipY)
            {
                //flip vertical Y  
                int[] totalBufferFlipY = new int[totalBuffer.Length];
                int srcRowIndex = height - 1;
                int strideInBytes = width * 4;//32 bpp
                for (int i = 0; i < height; ++i)
                {
                    //copy each row from src to dst
                    System.Buffer.BlockCopy(totalBuffer, strideInBytes * srcRowIndex, totalBufferFlipY, strideInBytes * i, strideInBytes);
                    srcRowIndex--;
                }
                totalBuffer = totalBufferFlipY;
            }
            unsafe
            {
                System.Runtime.InteropServices.Marshal.Copy(totalBuffer, 0, bmp._pixelBuffer, totalBuffer.Length);
            }
            return bmp;
        }
        public static MemBitmap CreateFromCopy(int width, int height, int len, IntPtr anotherNativePixelBuffer)
        {
            var memBmp = new MemBitmap(width, height);
#if DEBUG
            memBmp._dbugNote = "MemBitmap.CreateFromCopy";
#endif
            unsafe
            {
                MemMx.memcpy((byte*)memBmp._pixelBuffer, (byte*)anotherNativePixelBuffer, len);
            }
            return memBmp;
        }

        public static MemBitmap CreateFromCopy(MemBitmap another)
        {

            var memBmp = new MemBitmap(another.Width, another.Height);
#if DEBUG
            memBmp._dbugNote = "MemBitmap.CreateFromCopy";
#endif
            unsafe
            {
                MemMx.memcpy((byte*)memBmp._pixelBuffer, (byte*)another._pixelBuffer, another._pixelBufferInBytes);
            }
            return memBmp;
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
        public static int[] CopyImgBuffer(MemBitmap memBmp)
        {

            int[] buff2 = new int[memBmp.Width * memBmp.Height];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr pixBuffer = MemBitmap.GetBufferPtr(memBmp))
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
        //
        RectInt IBitmapSrc.GetBounds() => new RectInt(0, 0, _width, _height);
        int IBitmapSrc.Width => _width;
        int IBitmapSrc.Height => _height;
        int IBitmapSrc.Stride => _strideBytes;
        //
        int IBitmapSrc.BytesBetweenPixelsInclusive => 4;
        int IBitmapSrc.BitDepth => _bitDepth;
        //
        int IBitmapSrc.GetBufferOffsetXY32(int x, int y) => (y * _width) + x;
        CpuBlit.Imaging.TempMemPtr IBitmapSrc.GetBufferPtr()
        {
            return new CpuBlit.Imaging.TempMemPtr(_pixelBuffer, _pixelBufferInBytes);
        }
        //
        void IBitmapSrc.WriteBuffer(int[] newBuffer)
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
                int pixelValue = pxBuff[y * _width + x];
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
        void WriteBuffer(int[] newBuffer);
        Color GetPixel(int x, int y);
    }

    public static class MemBitmapExtensions
    {
        public static int[] CopyImgBuffer(MemBitmap memBmp, int width, int height)
        {
            //calculate stride for the width

            int destStride = MemBitmap.CalculateStride(width, CpuBlit.Imaging.PixelFormat.ARGB32);
            int newBmpW = destStride / 4;
            int[] buff2 = new int[newBmpW * height];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = MemBitmap.GetBufferPtr(memBmp))
                {
                    byte* srcBuffer = (byte*)srcBufferPtr.Ptr;
                    int srcIndex = 0;
                    int srcStride = memBmp.Stride;
                    fixed (int* destHead = &buff2[0])
                    {
                        byte* destHead2 = (byte*)destHead;
                        for (int line = 0; line < height; ++line)
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

        public static int[] CopyImgBuffer(MemBitmap src, int srcX, int srcY, int srcW, int srcH)
        {
            //calculate stride for the width 
            int destStride = MemBitmap.CalculateStride(srcW, CpuBlit.Imaging.PixelFormat.ARGB32);
            int newBmpW = destStride / 4;

            int[] buff2 = new int[newBmpW * srcH];
            unsafe
            {

                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = MemBitmap.GetBufferPtr(src))
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

        /// <summary>
        /// swap from gles ARGB to ABGR (Gdi)
        /// </summary>
        /// <param name="src"></param>
        public static void SwapArgbToAbgr(MemBitmap src)
        {

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