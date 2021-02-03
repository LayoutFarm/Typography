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
namespace PixelFarm.CpuBlit
{

    public class MemBitmapProxy
    {
        public MemBitmapProxy()
        {
        }
        public void Set(MemBitmap memBmp, int left, int top, int width, int height)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            OriginalBmp = memBmp;
        }
        public MemBitmap OriginalBmp { get; private set; }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    }

    public static class MemBitmapExt
    {
        //---------------
        //helper...
        public static TempMemPtr FromBmp(MemBitmap memBmp)
        {
            return MemBitmap.GetBufferPtr(memBmp);
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


        public static int[] CopyImgBuffer(this MemBitmap memBmp, int width, int height, bool flipY = false)
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

                    if (flipY)
                    {
                        fixed (int* destHead = &buff2[0])
                        {
                            byte* destHead2 = (byte*)destHead;

                            srcBuffer += (height - 1) * srcStride;
                            for (int line = 0; line < height; ++line)
                            {
                                //System.Runtime.InteropServices.Marshal.Copy(srcBuffer, srcIndex, (IntPtr)destHead2, destStride);
                                MemMx.memcpy((byte*)destHead2, srcBuffer + srcIndex, destStride);
                                srcIndex -= srcStride;
                                destHead2 += destStride;
                            }
                        }
                    }
                    else
                    {
                        fixed (int* destHead = &buff2[0])
                        {
                            byte* destHead2 = (byte*)destHead;
                            for (int line = 0; line < height; ++line)
                            {
                                //System.Runtime.InteropServices.Marshal.Copy(srcBuffer, srcIndex, (IntPtr)destHead2, destStride);
                                MemMx.memcpy((byte*)destHead2, srcBuffer + srcIndex, destStride);
                                srcIndex += srcStride;
                                destHead2 += destStride;
                            }
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
                        MemMx.memcpy((byte*)dstPtr, (byte*)(srcPtr + ((line * orgSrcW) + toCopyRect.Left)), toCopyRect.Width * 4);
                        dstPtr += toCopyRect.Width;
                    }
                }
            }

            return copyBmp;
        }

        public static void BlendColorWithMask(Color c, MemBitmapProxy mask, MemBitmap dst, int dstX, int dstY, int srcX, int srcY, int srcW, int srcH)
        {
            //select color from mask bitmap and blend to dst bitmap with specific color     

        }
        public static void BitBlt(MemBitmap src, MemBitmap dst, int dstX, int dstY, int srcX, int srcY, int srcW, int srcH)
        {
            IntPtr src_h = src.GetRawBufferHead();
            IntPtr dst_h = dst.GetRawBufferHead();
            unsafe
            {
                int* src_h1 = (int*)src_h;
                int* dst_h1 = (int*)dst_h;
                //copy line-by-line
                src_h1 += srcY * src.Width;//move to src line
                dst_h1 += dstY * dst.Width;//move to dst line

                if (dstX + srcW > dst.Width)
                {
                    srcW = dst.Width - dstX;
                    if (srcW < 0) { return; }//limit
                }
                if (dstY + srcH > dst.Height)
                {
                    srcH = dst.Height - dstY;
                    if (srcH < 0) return;//limit
                }

                for (int t_count = 0; t_count < srcH; ++t_count)
                {
                    MemMx.memcpy((byte*)(dst_h1 + dstX), (byte*)(src_h1 + srcX), srcW * 4);
                    //move to next line
                    src_h1 += src.Width;
                    dst_h1 += dst.Width;
                }
            }

            src.ReleaseRawBufferHead(src_h);
            dst.ReleaseRawBufferHead(dst_h);

#if DEBUG
            //dst.SaveImage("tmpN1.png");
#endif
        }

        ///// <summary>
        ///// swap from gles ARGB to ABGR (Gdi)
        ///// </summary>
        ///// <param name="src"></param>
        //public static void SwapArgbToAbgr(this MemBitmap src)
        //{
        //    //TODO:
        //}
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

        internal static void Clear(PixelFarm.CpuBlit.TempMemPtr tmp, Color color, int left, int top, int width, int height)
        {
            unsafe
            {
                int* buffer = (int*)tmp.Ptr;
                //------------------------------
                //fast clear buffer
                //skip clipping ****
                //TODO: reimplement clipping***
                //------------------------------  

                unsafe
                {
                    //clear only 1st row 
                    uint* head_i32 = (uint*)buffer;
                    //first line
                    //other color
                    //#if WIN32
                    //  uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                    //#else
                    //  uint colorARGB = (uint)((color.alpha << 24) | ((color.blue << 16) | (color.green << 8) | color.red));
                    //#endif

                    //ARGB
                    uint colorARGB = 0;//empty
                    if (color != Color.Empty)
                    {
                        colorARGB = (uint)((color.A << CO.A_SHIFT) | ((color.R << CO.R_SHIFT) | (color.G << CO.G_SHIFT) | color.B << CO.B_SHIFT));
                    }



                    head_i32 += top * width;//move to first line 
                    //and first line only
                    uint* head_i32_1 = head_i32 + left;
                    for (int i = width - 1; i >= 0; --i)
                    {
                        *head_i32_1 = colorARGB; //black (ARGB)
                        head_i32_1++;
                    }

                    int stride = width * 4;//bytes
                    //and copy to another line
                    head_i32 += width;//move to another line

                    for (int i = height - 2; i >= 0; --i)
                    {
                        MemMx.memcpy((byte*)(head_i32 + left), (byte*)buffer, stride);
                        head_i32 += width;
                    }
                }
            }
        }

        internal static void Clear(PixelFarm.CpuBlit.TempMemPtr tmp, Color color, int width, int height)
        {
            unsafe
            {
                int* buffer = (int*)tmp.Ptr;


                //------------------------------
                //fast clear buffer
                //skip clipping ****
                //TODO: reimplement clipping***
                //------------------------------ 


                unsafe
                {
                    //clear only 1st row 
                    uint* head_i32 = (uint*)buffer;
                    //first line

                    //other color
                    //#if WIN32
                    //  uint colorARGB = (uint)((color.alpha << 24) | ((color.red << 16) | (color.green << 8) | color.blue));
                    //#else
                    //  uint colorARGB = (uint)((color.alpha << 24) | ((color.blue << 16) | (color.green << 8) | color.red));
                    //#endif

                    //ARGB
                    uint colorARGB = 0;//empty
                    if (color != Color.Empty)
                    {
                        colorARGB = (uint)((color.A << CO.A_SHIFT) | ((color.R << CO.R_SHIFT) | (color.G << CO.G_SHIFT) | color.B << CO.B_SHIFT));
                    }

                    //first line only
                    for (int i = width - 1; i >= 0; --i)
                    {
                        *head_i32 = colorARGB; //black (ARGB)
                        head_i32++;
                    }
                    //and copy to another line
                    int stride = width * 4;
                    for (int i = height - 2; i >= 0; --i)
                    {
                        //copy from first line to another line
                        MemMx.memcpy((byte*)head_i32, (byte*)buffer, stride);
                        head_i32 += width;
                    }
                }
            }
        }
        public static void Clear(this MemBitmap bmp, Color color)
        {
            Clear(MemBitmap.GetBufferPtr(bmp), color, bmp.Width, bmp.Height);
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

        public static MemBitmap LoadBitmap(string filename)
        {
            return DefaultMemBitmapIO.LoadImage(filename);
        }
        public static MemBitmap LoadBitmap(System.IO.Stream stream)
        {
            //user need to provider load img func handler
            return DefaultMemBitmapIO.LoadImage(stream);
        }
        public static MemBitmap ScaleImage(this PixelFarm.CpuBlit.MemBitmap bmp, float x_scale, float y_scale)
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
        }

#if DEBUG
        public static bool s_dbugEnableDebugImage;
#endif

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