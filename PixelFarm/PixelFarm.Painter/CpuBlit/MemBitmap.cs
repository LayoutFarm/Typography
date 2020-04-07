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
using PixelFarm.Drawing.Internal;
using PixelFarm.CpuBlit.VertexProcessing;
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
    /// agg buffer's pixel format
    /// </summary>
    public enum PixelFormat
    {
        ARGB32,
        RGB24,
        GrayScale8,
    }
    /// <summary>
    /// 32 bpp native memory bitmap
    /// </summary>
    public sealed class MemBitmap : Image, IBitmapSrc
    {

        readonly int _width;
        readonly int _height;

        readonly int _strideBytes;
        readonly int _bitDepth;
        readonly PixelFormat _pixelFormat;
        readonly bool _pixelBufferFromExternalSrc;

        IntPtr _pixelBuffer;
        int _pixelBufferInBytes;
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
                _pixelFormat = PixelFormat.ARGB32, //***
                out _bitDepth,
                out int bytesPerPixel);

            _pixelBufferInBytes = width * height * 4;
            _pixelBufferFromExternalSrc = true; //*** we receive ptr from external ***
            _pixelBuffer = externalNativeInt32Ptr;

#if DEBUG
            dbugMemBitmapMonitor.dbugRegisterMemBitmap(this, width + "x" + height + ": " + DateTime.Now.ToString("u"));
#endif
        }
        public BitmapBufferFormat BufferPixelFormat
        {
            get
            {
                if (PixelFormat == CpuBlit.PixelFormat.ARGB32)
                {
                    return BitmapBufferFormat.BGRA;//on windows
                }
                else
                {
                    return BitmapBufferFormat.BGR;//on Windows
                }
            }
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

        public override bool IsReferenceImage => false;
        public CpuBlit.PixelFormat PixelFormat => _pixelFormat;
        //
        public int Stride => _strideBytes;
        public int BitDepth => _bitDepth;
        //
        public bool IsBigEndian { get; set; }
        public static TempMemPtr GetBufferPtr(MemBitmap bmp)
        {
            return new TempMemPtr(bmp._pixelBuffer, bmp._pixelBufferInBytes);
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
        public static unsafe MemBitmap CreateFromCopy(int width, int height, IntPtr totalBuffer, int totalLen, bool doFlipY = false)
        {

            var bmp = new MemBitmap(width, height);
#if DEBUG
            bmp._dbugNote = "MemBitmap.CreateFromCopy";
#endif
            //System.Runtime.InteropServices.Marshal.Copy(totalBuffer, bmp._pixelBuffer, 0, totalLen);
            MemMx.memcpy((byte*)(bmp._pixelBuffer), (byte*)totalBuffer, totalLen);
            //if (doFlipY)
            //{
            //    //flip vertical Y  
            //    int[] totalBufferFlipY = new int[totalBuffer.Length];
            //    int srcRowIndex = height - 1;
            //    int strideInBytes = width * 4;//32 bpp
            //    for (int i = 0; i < height; ++i)
            //    {
            //        //copy each row from src to dst
            //        System.Buffer.BlockCopy(totalBuffer, strideInBytes * srcRowIndex, totalBufferFlipY, strideInBytes * i, strideInBytes);
            //        srcRowIndex--;
            //    }
            //    totalBuffer = totalBufferFlipY;
            //}
            //unsafe
            //{

            //}
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


        public static int CalculateStride(int width, CpuBlit.PixelFormat format)
        {
            int bitDepth, bytesPerPixel;
            return CalculateStride(width, format, out bitDepth, out bytesPerPixel);
        }
        public static int CalculateStride(int width, CpuBlit.PixelFormat format, out int bitDepth, out int bytesPerPixel)
        {
            //stride calcuation helper

            switch (format)
            {
                case CpuBlit.PixelFormat.ARGB32:
                    {
                        bitDepth = 32;
                        bytesPerPixel = (bitDepth + 7) / 8;
                        return width * (32 / 8);
                    }
                case CpuBlit.PixelFormat.GrayScale8:
                    {
                        bitDepth = 8; //bit per pixel
                        bytesPerPixel = (bitDepth + 7) / 8;
                        return 4 * ((width * bytesPerPixel + 3) / 4);
                    }
                case CpuBlit.PixelFormat.RGB24:
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

                using (TempMemPtr pixBuffer = MemBitmap.GetBufferPtr(memBmp))
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
        Q1Rect IBitmapSrc.GetBounds() => new Q1Rect(0, 0, _width, _height);
        int IBitmapSrc.Width => _width;
        int IBitmapSrc.Height => _height;
        int IBitmapSrc.Stride => _strideBytes;
        //
        int IBitmapSrc.BytesBetweenPixelsInclusive => 4;
        int IBitmapSrc.BitDepth => _bitDepth;
        //
        int IBitmapSrc.GetBufferOffsetXY32(int x, int y) => (y * _width) + x;
        TempMemPtr IBitmapSrc.GetBufferPtr()
        {
            return new TempMemPtr(_pixelBuffer, _pixelBufferInBytes);
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
                //TODO: use CO with color shift***
                int* pxBuff = (int*)_pixelBuffer;
                int pixelValue = pxBuff[y * _width + x];
                return new Color(
                  (byte)(pixelValue >> 24),
                  (byte)(pixelValue >> 16),
                  (byte)(pixelValue >> 8),
                  (byte)(pixelValue));
            }
        }
        public static MemBitmap LoadBitmap(string filename)
        {
            return MemBitmapExtensions.DefaultMemBitmapIO.LoadImage(filename);
        }
        public static MemBitmap LoadBitmap(System.IO.Stream input)
        {
            return MemBitmapExtensions.LoadImage(input);
        }
    }

    public interface IBitmapSrc
    {
        int BitDepth { get; }
        int Width { get; }
        int Stride { get; }
        int Height { get; }

        Q1Rect GetBounds();

        int GetBufferOffsetXY32(int x, int y);

        TempMemPtr GetBufferPtr();


        int BytesBetweenPixelsInclusive { get; }
        void WriteBuffer(int[] newBuffer);
        Color GetPixel(int x, int y);
    }

    public static class MemBitmapExtensions
    {


        public static int[] CopyImgBuffer(this MemBitmap memBmp, int width, int height)
        {
            //calculate stride for the width

            int destStride = MemBitmap.CalculateStride(width, PixelFormat.ARGB32);
            int newBmpW = destStride / 4;
            int[] buff2 = new int[newBmpW * height];
            unsafe
            {

                using (TempMemPtr srcBufferPtr = MemBitmap.GetBufferPtr(memBmp))
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


        public static MemBitmap CopyImgBuffer(this MemBitmap src, int srcX, int srcY, int srcW, int srcH)
        {
            //simple copy

#if DEBUG
            Rectangle orgSourceRect = new Rectangle(0, 0, src.Width, src.Height);
            Rectangle requestRect = new Rectangle(srcX, srcY, srcW, srcH);
#endif

            Rectangle toCopyRect = Rectangle.Intersect(
                                   new Rectangle(0, 0, src.Width, src.Height),//orgSourceRect
                                   new Rectangle(srcX, srcY, srcW, srcH));//reqstRect

            if (toCopyRect.Width == 0 || toCopyRect.Height == 0)
            {
                return null;
            }
            //-----
            MemBitmap copyBmp = new MemBitmap(toCopyRect.Width, toCopyRect.Height);
            unsafe
            {
                using (TempMemPtr srcBufferPtr = MemBitmap.GetBufferPtr(src))
                using (TempMemPtr dstBufferPtr = MemBitmap.GetBufferPtr(copyBmp))
                {

                    int* srcPtr = (int*)srcBufferPtr.Ptr;
                    int* dstPtr = (int*)dstBufferPtr.Ptr;
                    int lineEnd = srcY + srcH;
                    int orgSrcW = src.Width;
                    for (int line = toCopyRect.Top; line < toCopyRect.Bottom; ++line)
                    {
                        NativeMemMx.memcpy((byte*)dstPtr, (byte*)(srcPtr + ((line * orgSrcW) + toCopyRect.Left)), toCopyRect.Width * 4);
                        dstPtr += toCopyRect.Width;
                    }
                }
            }

            return copyBmp;
        }

        /// <summary>
        /// swap from gles ARGB to ABGR (Gdi)
        /// </summary>
        /// <param name="src"></param>
        public static void SwapArgbToAbgr(this MemBitmap src)
        {
            //TODO:
        }
        //public static void InvertColor(this MemBitmap memBmp)
        //{
        //    //temp fix
        //    unsafe
        //    {
        //        Imaging.TempMemPtr tmp = MemBitmap.GetBufferPtr(memBmp);
        //        int* buffer = (int*)tmp.Ptr;
        //        int len32 = tmp.LengthInBytes / 4;
        //        unsafe
        //        {
        //            {
        //                int* head_i32 = (int*)buffer;
        //                for (int n = len32 - 1; n >= 0; --n)
        //                {
        //                    int value = *head_i32;
        //                    int r = (value >> CO.R_SHIFT) & 0xff;
        //                    int g = (value >> CO.G_SHIFT) & 0xff;
        //                    int b = (value >> CO.B_SHIFT) & 0xff;
        //                    int a = (value >> CO.A_SHIFT) & 0xff;

        //                    *head_i32 = ((255 - r) << CO.R_SHIFT) | ((255 - g) << CO.G_SHIFT) | ((255 - b) << CO.B_SHIFT) | ((255 - a) << CO.A_SHIFT);
        //                    head_i32++;
        //                }
        //            }
        //        } 
        //    }
        //}
        internal static void Clear(PixelFarm.CpuBlit.TempMemPtr tmp, Color color)
        {
            unsafe
            {
                int* buffer = (int*)tmp.Ptr;
                int len32 = tmp.LengthInBytes / 4;

                //------------------------------
                //fast clear buffer
                //skip clipping ****
                //TODO: reimplement clipping***
                //------------------------------ 
                if (color == Color.White)
                {
                    //fast cleat with white color
                    int n = len32;
                    unsafe
                    {
                        //fixed (void* head = &buffer[0])
                        {
                            uint* head_i32 = (uint*)buffer;
                            for (int i = n - 1; i >= 0; --i)
                            {
                                *head_i32 = 0xffffffff; //white (ARGB)
                                head_i32++;
                            }
                        }
                    }
                }
                else if (color == Color.Black)
                {
                    //fast clear with black color
                    int n = len32;
                    unsafe
                    {
                        //fixed (void* head = &buffer[0])
                        {
                            uint* head_i32 = (uint*)buffer;
                            for (int i = n - 1; i >= 0; --i)
                            {
                                *head_i32 = 0xff000000; //black (ARGB)
                                head_i32++;
                            }
                        }
                    }
                }
                else if (color == Color.Empty)
                {
                    int n = len32;
                    unsafe
                    {
                        //fixed (void* head = &buffer[0])
                        {
                            uint* head_i32 = (uint*)buffer;
                            for (int i = n - 1; i >= 0; --i)
                            {
                                *head_i32 = 0x00000000; //empty
                                head_i32++;
                            }
                        }
                    }
                }
                else
                {
                    //other color
                    //#if WIN32
                    //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                    //#else
                    //                            uint colorARGB = (uint)((color.alpha << 24) | ((color.blue << 16) | (color.green << 8) | color.red));
                    //#endif

                    //ARGB
                    uint colorARGB = (uint)((color.A << CO.A_SHIFT) | ((color.R << CO.R_SHIFT) | (color.B << CO.G_SHIFT) | color.B << CO.B_SHIFT));
                    int n = len32;
                    unsafe
                    {
                        //fixed (void* head = &buffer[0])
                        {
                            uint* head_i32 = (uint*)buffer;
                            for (int i = n - 1; i >= 0; --i)
                            {
                                *head_i32 = colorARGB;
                                head_i32++;
                            }
                        }
                    }
                }
            }
        }
        public static void Clear(this MemBitmap bmp, Color color)
        {
            Clear(MemBitmap.GetBufferPtr(bmp), color);
        }
        /// <summary>
        /// create thumbnail img with super-sampling technique,(Expensive, High quality thumb)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dst"></param>
        public static MemBitmap CreateThumbnailWithSuperSamplingTechnique(this MemBitmap source, float scaleRatio)
        {

            // Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN
            //in this version new image MUST smaller than the original one ***
            if (scaleRatio >= 1 || scaleRatio < 0) return null;

            //create new bitmap
            int newBmpW = (int)Math.Round(source.Width * scaleRatio);
            int newBmpH = (int)Math.Round(source.Height * scaleRatio);

            MemBitmap thumbBitmap = new MemBitmap(newBmpW, newBmpH); //***
            IBitmapSrc source_1 = (IBitmapSrc)source;

            unsafe
            {

                Rectangle dstRoi2 = new Rectangle(0, 0, newBmpW, newBmpH);

                int dstWidth = dstRoi2.Width;
                int dstHeight = dstRoi2.Height;

                int srcH = source.Height;
                int srcW = source.Width;

                TempMemPtr dstMemPtr = MemBitmap.GetBufferPtr(thumbBitmap);
                int dstStrideInt32 = newBmpW;

                for (int dstY = dstRoi2.Top; dstY < dstRoi2.Bottom; ++dstY)
                {
                    //from dst  => find proper source (y)

                    //double srcTop = (double)(dstY * srcH) / (double)dstHeight;
                    double srcTop = (double)(dstY * srcH) / (double)dstHeight;
                    double srcTopFloor = Math.Floor(srcTop);
                    double srcTopWeight = 1 - (srcTop - srcTopFloor);
                    int srcTopInt = (int)srcTopFloor;

                    //double srcBottom = (double)((dstY + 1) * srcH) / (double)dstHeight;
                    double srcBottom = (double)((dstY + 1) * srcH) / (double)dstHeight;
                    double srcBottomFloor = Math.Floor(srcBottom - 0.00001);
                    double srcBottomWeight = srcBottom - srcBottomFloor;
                    int srcBottomInt = (int)srcBottomFloor;


                    int* srcBuffer = (int*)(MemBitmap.GetBufferPtr(source)).Ptr;
                    int srcStrideInt32 = source.Width;//***

                    int* dstAddr = (int*)dstMemPtr.Ptr + (dstStrideInt32 * dstY); //begin at

                    for (int dstX = dstRoi2.Left; dstX < dstRoi2.Right; ++dstX)
                    {
                        //from dst=> find proper source (x)

                        double srcLeft = (double)(dstX * srcW) / (double)dstWidth;
                        double srcLeftFloor = Math.Floor(srcLeft);
                        double srcLeftWeight = 1 - (srcLeft - srcLeftFloor);
                        int srcLeftInt = (int)srcLeftFloor;

                        double srcRight = (double)((dstX + 1) * srcW) / (double)dstWidth;
                        double srcRightFloor = Math.Floor(srcRight - 0.00001);
                        double srcRightWeight = srcRight - srcRightFloor;
                        int srcRightInt = (int)srcRightFloor;

                        double blueSum = 0;
                        double greenSum = 0;
                        double redSum = 0;
                        double alphaSum = 0;

                        //now we know (left,top) of source that we want
                        //then ask the pixel value from source at that pos

                        //(1) left fractional edge
                        {
                            //PaintFx.ColorBgra* srcLeftPtr = source.GetPointAddressUnchecked(srcLeftInt, srcTopInt + 1);
                            int* srcLeftColorAddr = srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt, srcTopInt + 1);

                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                int srcColor = *srcLeftColorAddr;
                                double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * srcLeftWeight;

                                blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                                greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                                redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                                alphaSum += a_w;

                                //move to next row
                                srcLeftColorAddr += srcStrideInt32;
                                //srcLeftPtr = (ColorBgra*)((byte*)srcLeftPtr + source._stride);
                            }
                        }
                        //
                        {
                            //(2) right fractional edge
                            //ColorBgra* srcRightPtr = source.GetPointAddressUnchecked(srcRightInt, srcTopInt + 1);
                            int* srcRightColorAddr = srcBuffer + source_1.GetBufferOffsetXY32(srcRightInt, srcTopInt + 1);

                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                int srcColor = *srcRightColorAddr;
                                double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * srcRightWeight;

                                blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                                greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                                redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                                alphaSum += a_w;

                                //srcRightPtr = (ColorBgra*)((byte*)srcRightPtr + source._stride); 
                                srcRightColorAddr += srcStrideInt32; //move to next row
                            }
                        }
                        // 
                        {
                            //(3) top fractional edge   
                            //ColorBgra* srcTopPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcTopInt);
                            int* srcTopColorAddr = srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt + 1, srcTopInt);

                            for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                            {
                                int srcColor = *srcTopColorAddr;
                                double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * srcTopWeight;

                                blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                                greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                                redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                                alphaSum += a_w;

                                //move to next column
                                //++srcTopPtr;
                                ++srcTopColorAddr;
                            }
                        }
                        //
                        {
                            //(4) bottom fractional edge
                            //ColorBgra* srcBottomPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcBottomInt); 
                            int* srcBottomColorAddr = srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt + 1, srcBottomInt);

                            for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                            {
                                int srcColor = *srcBottomColorAddr;
                                double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * srcBottomWeight;

                                blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                                greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                                redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                                alphaSum += a_w;

                                //++srcBottomPtr;
                                //move to next column
                                //++srcTopPtr;
                                ++srcBottomColorAddr;
                            }
                        }
                        {
                            //(5) center area
                            for (int srcY = srcTopInt + 1; srcY < srcBottomInt; ++srcY)
                            {
                                //ColorBgra* srcPtr = source.GetPointAddressUnchecked(srcLeftInt + 1, srcY); 
                                int* srcColorAddr = srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt + 1, srcY);

                                for (int srcX = srcLeftInt + 1; srcX < srcRightInt; ++srcX)
                                {
                                    int srcColor = *srcColorAddr;

                                    int a = ((srcColor >> CO.A_SHIFT) & 0xff);
                                    blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a;
                                    greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a;
                                    redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a;
                                    alphaSum += a;

                                    ++srcColorAddr;
                                }
                            }
                        }


                        //(6) four corner pixels
                        {
                            //6.1 
                            //ColorBgra srcTL = source.GetPoint(srcLeftInt, srcTopInt); 
                            int srcColor = *(srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt, srcTopInt));

                            double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * (srcTopWeight * srcLeftWeight);

                            blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                            greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                            redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                            alphaSum += a_w;
                        }

                        {
                            //6.2
                            //ColorBgra srcTR = source.GetPoint(srcRightInt, srcTopInt);
                            //double srcTRA = srcTR.A;
                            //blueSum += srcTR.B * (srcTopWeight * srcRightWeight) * srcTRA;
                            //greenSum += srcTR.G * (srcTopWeight * srcRightWeight) * srcTRA;
                            //redSum += srcTR.R * (srcTopWeight * srcRightWeight) * srcTRA;
                            //alphaSum += srcTR.A * (srcTopWeight * srcRightWeight); 

                            int srcColor = *(srcBuffer + source_1.GetBufferOffsetXY32(srcRightInt, srcTopInt));
                            double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * (srcTopWeight * srcRightWeight);

                            blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                            greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                            redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                            alphaSum += a_w;
                        }


                        {
                            //(6.3)
                            int srcColor = *(srcBuffer + source_1.GetBufferOffsetXY32(srcLeftInt, srcBottomInt));
                            double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * (srcBottomWeight * srcLeftWeight);

                            blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                            greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                            redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                            alphaSum += a_w; //without a


                            //ColorBgra srcBL = source.GetPoint(srcLeftInt, srcBottomInt);
                            //double srcBLA = srcBL.A;
                            //blueSum += srcBL.B * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            //greenSum += srcBL.G * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            //redSum += srcBL.R * (srcBottomWeight * srcLeftWeight) * srcBLA;
                            //alphaSum += srcBL.A * (srcBottomWeight * srcLeftWeight);
                        }

                        {
                            //(6.4)

                            //ColorBgra srcBR = source.GetPoint(srcRightInt, srcBottomInt);
                            //double srcBRA = srcBR.A;
                            //blueSum += srcBR.B * (srcBottomWeight * srcRightWeight) * srcBRA;
                            //greenSum += srcBR.G * (srcBottomWeight * srcRightWeight) * srcBRA;
                            //redSum += srcBR.R * (srcBottomWeight * srcRightWeight) * srcBRA;
                            //alphaSum += srcBR.A * (srcBottomWeight * srcRightWeight);

                            int srcColor = *(srcBuffer + source_1.GetBufferOffsetXY32(srcRightInt, srcBottomInt));
                            double a_w = ((srcColor >> CO.A_SHIFT) & 0xff) * (srcBottomWeight * srcRightWeight);

                            blueSum += ((srcColor >> CO.B_SHIFT) & 0xff) * a_w;
                            greenSum += ((srcColor >> CO.G_SHIFT) & 0xff) * a_w;
                            redSum += ((srcColor >> CO.R_SHIFT) & 0xff) * a_w;
                            alphaSum += a_w;

                        }


                        double area = (srcRight - srcLeft) * (srcBottom - srcTop);

                        double alpha = alphaSum / area;
                        double blue;
                        double green;
                        double red;

                        if (alpha == 0)
                        {
                            blue = 0;
                            green = 0;
                            red = 0;
                        }
                        else
                        {
                            blue = blueSum / alphaSum;
                            green = greenSum / alphaSum;
                            red = redSum / alphaSum;
                        }

                        // add 0.5 so that rounding goes in the direction we want it to
                        blue += 0.5;
                        green += 0.5;
                        red += 0.5;
                        alpha += 0.5;


                        //***
                        //dstPtr->Bgra = (uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                        //++dstPtr;
                        *dstAddr = ((byte)alpha) << CO.A_SHIFT |
                                   ((byte)blue) << CO.B_SHIFT |
                                   ((byte)green) << CO.G_SHIFT |
                                   ((byte)red) << CO.R_SHIFT;

                        //(uint)blue + ((uint)green << 8) + ((uint)red << 16) + ((uint)alpha << 24);
                        ++dstAddr;
                    }
                }
            }
            return thumbBitmap;
        }


        public static MemBitmapIO DefaultMemBitmapIO { get; set; }


        public static MemBitmap LoadImage(System.IO.Stream stream)
        {
            //user need to provider load img func handler
            return DefaultMemBitmapIO.LoadImage(stream);
        }
        public static PixelFarm.CpuBlit.MemBitmap ScaleImage(this PixelFarm.CpuBlit.MemBitmap bmp, float x_scale, float y_scale)
        {
            return DefaultMemBitmapIO.ScaleImage(bmp, x_scale, y_scale);
        }


        public static void SaveImage(this MemBitmap source, string filename, MemBitmapIO.OutputImageFormat outputFormat = MemBitmapIO.OutputImageFormat.Default, object saveParameters = null)
        {
            //save image with default parameter 
            if (outputFormat == MemBitmapIO.OutputImageFormat.Default)
            {
                string ext = System.IO.Path.GetExtension(filename).ToLower();
                switch (ext)
                {
                    case ".png":
                        outputFormat = MemBitmapIO.OutputImageFormat.Png;
                        break;
                    case ".jpg":
                    case ".jpeg":
                        outputFormat = MemBitmapIO.OutputImageFormat.Jpeg;
                        break;
                }
            }

            DefaultMemBitmapIO.SaveImage(source, filename, outputFormat, saveParameters);
        }
        public static void SaveImage(this MemBitmap source,
            System.IO.Stream output,
            MemBitmapIO.OutputImageFormat outputFormat = MemBitmapIO.OutputImageFormat.Default, object saveParameters = null)
        {
            DefaultMemBitmapIO.SaveImage(source, output, outputFormat, saveParameters);

            ////save image with default parameter 
            //if (outputFormat == MemBitmapIO.OutputImageFormat.Default)
            //{
            //    string ext = System.IO.Path.GetExtension(filename).ToLower();
            //    switch (ext)
            //    {
            //        case ".png":
            //            outputFormat = MemBitmapIO.OutputImageFormat.Png;
            //            break;
            //        case ".jpg":
            //        case ".jpeg":
            //            outputFormat = MemBitmapIO.OutputImageFormat.Jpeg;
            //            break;
            //    }
            //}

            //DefaultMemBitmapIO.SaveImage(source, filename, outputFormat, saveParameters);
        }
    }

    public abstract class MemBitmapIO
    {
        public enum OutputImageFormat
        {
            Default,
            Png,
            Jpeg,
        }

        public abstract MemBitmap LoadImage(string filename);
        public abstract MemBitmap LoadImage(System.IO.Stream input);
        public abstract void SaveImage(MemBitmap bitmap, System.IO.Stream output, OutputImageFormat outputFormat, object saveParameters);
        public abstract void SaveImage(MemBitmap bitmap, string filename, OutputImageFormat outputFormat, object saveParameters);
        public abstract PixelFarm.CpuBlit.MemBitmap ScaleImage(PixelFarm.CpuBlit.MemBitmap bmp, float x_scale, float y_scale);
    }
}