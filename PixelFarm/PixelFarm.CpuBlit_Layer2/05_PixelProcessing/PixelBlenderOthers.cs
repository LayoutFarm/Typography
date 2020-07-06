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
//
// Adaptation for high precision colors has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;
using CO = PixelFarm.Drawing.Internal.CO;

namespace PixelFarm.CpuBlit.PixelProcessing
{

    /// <summary>
    /// change destination alpha change with red color from source
    /// </summary>
    public class PixelBlenderGrey : PixelBlender32
    {

        internal override void BlendPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                fixed (int* head = &dstBuffer[arrayOffset])
                {
                    BlendPixel32Internal(head, srcColor);
                }
            }
        }

        internal override unsafe void BlendPixel32(int* dstPtr, Color srcColor)
        {
            BlendPixel32Internal(dstPtr, srcColor);
        }

        internal override void BlendPixels(int[] dstBuffer,
            int arrayElemOffset,
            Color[] srcColors,
            int srcColorOffset,
            byte[] covers,
            int coversIndex,
            bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                                //---------
                                //2
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
                else
                {

                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                                //2.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (int* dstHead = &dstBuffer[arrayElemOffset])
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset]);
                            }
                            else
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset].NewFromChangeCoverage(cover));
                            }
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }

            }
        }


        internal override void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {

                fixed (int* ptr = &dstBuffer[arrayOffset])
                {
                    //TODO: consider use memcpy() impl*** 
                    *ptr = srcColor.ToGrayValueARGB();
                }

            }

        }

        internal override void CopyPixels(int[] dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                unchecked
                {
                    fixed (int* ptr_byte = &dstBuffer[arrayOffset])
                    {
                        //TODO: consider use memcpy() impl***

                        //byte y = (byte)(((srcColor.R * 77) + (srcColor.G * 151) + (srcColor.B * 28)) >> 8);
                        //srcColor = new Color(srcColor.A, y, y, y);

                        int* ptr = ptr_byte;
                        int argb = srcColor.ToGrayValueARGB();

                        //---------
                        if ((count % 2) != 0)
                        {
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                        }

                        while (count > 0)
                        {
                            //-----------
                            //1.
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                            //-----------
                            //2
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                        }

                    }
                }
            }
        }

        static unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor, int coverageValue)
        {

            //calculate new alpha


            int src_a = (byte)((srcColor.A * coverageValue + 255) >> 8);
            //after apply the alpha
            unchecked
            {
                if (src_a == 255)
                {
                    *dstPtr = srcColor.ToGrayValueARGB(); //just copy
                }
                else
                {
                    byte y = (byte)(((srcColor.R * 77) + (srcColor.G * 151) + (srcColor.B * 28)) >> 8);
                    srcColor = new Color(srcColor.A, y, y, y);

                    int dest = *dstPtr;

                    //separate each component
                    byte a = (byte)((dest >> CO.A_SHIFT) & 0xff);
                    byte r = (byte)((dest >> CO.R_SHIFT) & 0xff);
                    byte g = (byte)((dest >> CO.G_SHIFT) & 0xff);
                    byte b = (byte)((dest >> CO.B_SHIFT) & 0xff);


                    *dstPtr =
                     ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                     ((byte)(((srcColor.R - r) * src_a + (r << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.R_SHIFT) |
                     ((byte)(((srcColor.G - g) * src_a + (g << CO.BASE_SHIFT)) >> (int)CO.BASE_SHIFT) << CO.G_SHIFT) |
                     ((byte)(((srcColor.B - b) * src_a + (b << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT);
                }
            }
        }
        static unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor)
        {
            unchecked
            {

                //convert srcColor to grey-scale image


                if (srcColor.A == 255)
                {
                    *dstPtr = srcColor.ToGrayValueARGB(); //just copy
                }
                else
                {
                    byte y = (byte)(((srcColor.R * 77) + (srcColor.G * 151) + (srcColor.B * 28)) >> 8);
                    srcColor = new Color(srcColor.A, y, y, y);

                    int dest = *dstPtr;

                    //separate each component
                    byte a = (byte)((dest >> CO.A_SHIFT) & 0xff);
                    byte r = (byte)((dest >> CO.R_SHIFT) & 0xff);
                    byte g = (byte)((dest >> CO.G_SHIFT) & 0xff);
                    byte b = (byte)((dest >> CO.B_SHIFT) & 0xff);

                    byte src_a = srcColor.A;

                    *dstPtr =
                     ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                     ((byte)(((srcColor.R - r) * src_a + (r << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT) |
                     ((byte)(((srcColor.G - g) * src_a + (g << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.G_SHIFT) |
                     ((byte)(((srcColor.B - b) * src_a + (b << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT);
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* ptr = (int*)dstBuffer.Ptr;
                int* head = &ptr[arrayOffset];
                {
                    BlendPixel32Internal(head, srcColor);
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dstBuffer1, int arrayElemOffset, Color[] srcColors, int srcColorOffset, byte[] covers, int coversIndex, bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dstBuffer1.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                                //---------
                                //2
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
                else
                {

                    unsafe
                    {
                        int* dstBuffer = (int*)dstBuffer1.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                                //2.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    int* dstBuffer = (int*)dstBuffer1.Ptr;
                    int* dstHead = &dstBuffer[arrayElemOffset];
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset]);
                            }
                            else
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset].NewFromChangeCoverage(cover));
                            }
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }

            }
        }

        internal override void CopyPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                int* ptr1 = (int*)dstBuffer.Ptr;
                int* ptr_byte = &ptr1[arrayOffset];
                {
                    //TODO: consider use memcpy() impl*** 
                    int* ptr = ptr_byte;
                    int argb = srcColor.ToGrayValueARGB();

                    //---------
                    if ((count % 2) != 0)
                    {
                        *ptr = argb;
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------
                        //1.
                        *ptr = argb;
                        ptr++; //move next
                        count--;
                        //-----------
                        //2
                        *ptr = argb;
                        ptr++; //move next
                        count--;
                    }

                }

            }
        }

        internal override void CopyPixel(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* ptr1 = (int*)dstBuffer.Ptr;
                int* ptr = &ptr1[arrayOffset];
                {
                    //TODO: consider use memcpy() impl***  
                    *ptr = srcColor.ToGrayValueARGB();
                }

            }
        }
    }


    static class ColorExtensions
    {
        /// <summary>
        /// with ratio, R77,B151,B28
        /// </summary>
        /// <param name="srcColor"></param>
        /// <returns></returns>
        public static int ToGrayValueARGB(this in Color srcColor)
        {
            byte y = (byte)(((srcColor.R * 77) + (srcColor.G * 151) + (srcColor.B * 28)) >> 8);
            return ((srcColor.A << 24) | (y << 16) | (y << 8) | y);
        }
    }
    public abstract class CustomPixelBlender : PixelBlender32
    {

        internal sealed override void BlendPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                fixed (int* head = &dstBuffer[arrayOffset])
                {
                    BlendPixel32Internal(head, srcColor);
                }
            }
        }

        internal sealed override unsafe void BlendPixel32(int* dstPtr, Color srcColor)
        {
            BlendPixel32Internal(dstPtr, srcColor);
        }

        internal sealed override void BlendPixels(int[] dstBuffer,
            int arrayElemOffset,
            Color[] srcColors,
            int srcColorOffset,
            byte[] covers,
            int coversIndex,
            bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                                //---------
                                //2
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
                else
                {

                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                                //2.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (int* dstHead = &dstBuffer[arrayElemOffset])
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset]);
                            }
                            else
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset].NewFromChangeCoverage(cover));
                            }
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }

            }
        }


        internal sealed override void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {

                fixed (int* ptr = &dstBuffer[arrayOffset])
                {
                    //TODO: consider use memcpy() impl***  
                    *ptr = srcColor.ToGrayValueARGB();
                }

            }

        }

        internal sealed override void CopyPixels(int[] dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                unchecked
                {
                    fixed (int* ptr_byte = &dstBuffer[arrayOffset])
                    {
                        //TODO: consider use memcpy() impl***

                        int* ptr = ptr_byte;
                        int argb = srcColor.ToGrayValueARGB();

                        //---------
                        if ((count % 2) != 0)
                        {
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                        }

                        while (count > 0)
                        {
                            //-----------
                            //1.
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                            //-----------
                            //2
                            *ptr = argb;
                            ptr++; //move next
                            count--;
                        }

                    }
                }
            }

        }

        protected abstract unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor, int coverageValue);

        protected abstract unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor);

        internal sealed override void BlendPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* ptr = (int*)dstBuffer.Ptr;
                int* head = &ptr[arrayOffset];
                {
                    BlendPixel32Internal(head, srcColor);
                }
            }
        }

        internal sealed override void BlendPixels(TempMemPtr dstBuffer1, int arrayElemOffset, Color[] srcColors, int srcColorOffset, byte[] covers, int coversIndex, bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dstBuffer1.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                                //---------
                                //2
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                header2++;//move next
                                count--;
                            }
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dstBuffer1.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = (int*)(IntPtr)head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                                //2.
                                BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    int* dstBuffer = (int*)dstBuffer1.Ptr;
                    int* dstHead = &dstBuffer[arrayElemOffset];
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset]);
                            }
                            else
                            {
                                BlendPixel32Internal(dstBufferPtr, srcColors[srcColorOffset].NewFromChangeCoverage(cover));
                            }
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }

            }
        }

        internal sealed override void CopyPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                int* ptr1 = (int*)dstBuffer.Ptr;
                int* ptr_byte = &ptr1[arrayOffset];
                {
                    int* ptr = ptr_byte;
                    //---------
                    if ((count % 2) != 0)
                    {
                        BlendPixel32Internal(ptr, srcColor);
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------
                        //1. 
                        BlendPixel32Internal(ptr, srcColor);
                        ptr++; //move next
                        count--;
                        //-----------
                        //2 
                        BlendPixel32Internal(ptr, srcColor);
                        ptr++; //move next
                        count--;
                    }
                }
            }
        }

        internal sealed override void CopyPixel(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* ptr1 = (int*)dstBuffer.Ptr;
                int* ptr = &ptr1[arrayOffset];
                {
                    BlendPixel32Internal(ptr, srcColor);
                }
            }
        }
    }







    /// <summary>
    /// apply mask to srcColor before send it to dest bmp
    /// </summary>
    public class PixelBlenderWithMask : PixelBlender32
    {
        TempMemPtr _maskInnerBuffer;
        int _mask_shift;//default
        PixelBlenderColorComponent _selectedMaskComponent;
        public PixelBlenderWithMask()
        {
            SelectedMaskComponent = PixelBlenderColorComponent.R; //default
        }

        /// <summary>
        /// set mask image, please note that size of mask must be the same size of the dest buffer
        /// </summary>
        /// <param name="maskBmp"></param>
        public void SetMaskBitmap(MemBitmap maskBmp)
        {
            //please note that size of mask must be the same size of the dest buffer
            _maskInnerBuffer = MemBitmap.GetBufferPtr(maskBmp);
        }

        public PixelBlenderColorComponent SelectedMaskComponent
        {
            get => _selectedMaskComponent;
            set
            {
                _selectedMaskComponent = value;
                switch (value)
                {
                    default: throw new NotSupportedException();
                    case PixelBlenderColorComponent.A:
                        _mask_shift = CO.A_SHIFT;
                        break;
                    case PixelBlenderColorComponent.R:
                        _mask_shift = CO.R_SHIFT;
                        break;
                    case PixelBlenderColorComponent.G:
                        _mask_shift = CO.G_SHIFT;
                        break;
                    case PixelBlenderColorComponent.B:
                        _mask_shift = CO.B_SHIFT;
                        break;
                }
            }
        }

        Color NewColorFromMask(Color srcColor, int arrayOffset)
        {
            unsafe
            {
                int* buff = (int*)_maskInnerBuffer.Ptr;
                return srcColor.NewFromChangeCoverage((byte)((buff[arrayOffset]) >> _mask_shift));
            }

        }
        internal override void BlendPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                fixed (int* head = &dstBuffer[arrayOffset])
                {
                    BlendPixel32Internal(head, NewColorFromMask(srcColor, arrayOffset));
                }
            }
        }

        internal override unsafe void BlendPixel32(int* dstPtr, Color srcColor)
        {
            BlendPixel32Internal(dstPtr, srcColor);
        }

        internal override void BlendPixels(int[] dstBuffer,
            int arrayElemOffset,
            Color[] srcColors,
            int srcColorOffset,
            byte[] covers,
            int coversIndex,
            bool firstCoverForAll, int count)
        {

            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {

                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                                //---------
                                //2
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }

                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                                //2.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                            }

                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (int* dstHead = &dstBuffer[arrayElemOffset])
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset], arrayElemOffset));
                            }
                            else
                            {
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset].NewFromChangeCoverage(cover), arrayElemOffset));
                            }


                            arrayElemOffset++;
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }
            }
        }
        internal override void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                unchecked
                {
                    fixed (int* ptr = &dstBuffer[arrayOffset])
                    {
                        BlendPixel32(ptr, NewColorFromMask(srcColor, arrayOffset));
                    }
                }
            }
        }

        internal override void CopyPixels(int[] dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                fixed (int* ptr_byte = &dstBuffer[arrayOffset])
                {
                    //TODO: consider use memcpy() impl***
                    int* ptr = ptr_byte;

                    Color newColor = NewColorFromMask(srcColor, arrayOffset);

                    //---------
                    if ((count % 2) != 0)
                    {
                        BlendPixel32(ptr, newColor);

                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------

                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                        //-----------

                        //2
                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                }
            }
        }

        static unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor)
        {
            unchecked
            {
                if (srcColor.A == 255)
                {
                    *dstPtr = srcColor.ToARGB(); //just copy
                }
                else
                {
                    int dest = *dstPtr;
                    //separate each component
                    byte a = (byte)((dest >> CO.A_SHIFT) & 0xff);
                    byte r = (byte)((dest >> CO.R_SHIFT) & 0xff);
                    byte g = (byte)((dest >> CO.G_SHIFT) & 0xff);
                    byte b = (byte)((dest >> CO.B_SHIFT) & 0xff);

                    byte src_a = srcColor.A;

                    *dstPtr =
                     ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                     ((byte)(((srcColor.R - r) * src_a + (r << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.R_SHIFT) |
                     ((byte)(((srcColor.G - g) * src_a + (g << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.G_SHIFT) |
                     ((byte)(((srcColor.B - b) * src_a + (b << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT);
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dst, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* dstBuffer = (int*)dst.Ptr;
                int* head = &dstBuffer[arrayOffset];
                {
                    BlendPixel32Internal(head, NewColorFromMask(srcColor, arrayOffset));
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dst, int arrayElemOffset, Color[] srcColors, int srcColorOffset, byte[] covers, int coversIndex, bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dst.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                                //---------
                                //2
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dst.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                                //2.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                            }
                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    int* dstBuffer = (int*)dst.Ptr;
                    int* head = &dstBuffer[arrayElemOffset];
                    {
                        int* dstBufferPtr = head;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset], arrayElemOffset));
                            }
                            else
                            {
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset].NewFromChangeCoverage(cover), arrayElemOffset));
                            }


                            arrayElemOffset++;
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }
            }
        }

        internal override void CopyPixels(TempMemPtr dst, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                int* dstBuffer = (int*)dst.Ptr;
                int* ptr_byte = &dstBuffer[arrayOffset];
                {
                    //TODO: consider use memcpy() impl***
                    int* ptr = ptr_byte;

                    Color newColor = NewColorFromMask(srcColor, arrayOffset);

                    //---------
                    if ((count % 2) != 0)
                    {
                        BlendPixel32(ptr, newColor);

                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------

                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                        //-----------

                        //2
                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                }

            }
        }

        internal override void CopyPixel(TempMemPtr dst, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* dstBuffer = (int*)dst.Ptr;
                int* ptr = &dstBuffer[arrayOffset];
                {
                    BlendPixel32(ptr, NewColorFromMask(srcColor, arrayOffset));
                }

            }
        }
    }

    public enum PixelBlenderColorComponent
    {
        A, //24
        R, //16
        G, //8
        B  //0
    }

    public enum EnableOutputColorComponent
    {
        EnableAll, //=0

        A,
        R,
        G,
        B
    }


    //TODO: review this again ...
    /// <summary>
    /// only apply to some dest color component
    /// </summary>
    public class PixelBlenderPerColorComponentWithMask : PixelBlender32
    {
        TempMemPtr _maskInnerBuffer;
        int _mask_shift;//default

        PixelBlenderColorComponent _selectedMaskComponent;
        EnableOutputColorComponent _selectedDestMaskComponent;

        public PixelBlenderPerColorComponentWithMask()
        {
            SelectedMaskComponent = PixelBlenderColorComponent.R; //default
            EnableOutputColorComponent = EnableOutputColorComponent.EnableAll;
        }
        /// <summary>
        /// set mask image, please note that size of mask must be the same size of the dest buffer
        /// </summary>
        /// <param name="maskBmp"></param>
        public void SetMaskBitmap(MemBitmap maskBmp)
        {
            //in this version
            //please note that size of mask must be the same size of the dest buffer
            _maskInnerBuffer = MemBitmap.GetBufferPtr(maskBmp);
        }

        public EnableOutputColorComponent EnableOutputColorComponent
        {
            get => _selectedDestMaskComponent;
            set => _selectedDestMaskComponent = value;
        }
        public PixelBlenderColorComponent SelectedMaskComponent
        {
            get => _selectedMaskComponent;
            set
            {
                _selectedMaskComponent = value;
                switch (value)
                {
                    default: throw new NotSupportedException();
                    case PixelBlenderColorComponent.A:
                        _mask_shift = CO.A_SHIFT;
                        break;
                    case PixelBlenderColorComponent.R:
                        _mask_shift = CO.R_SHIFT;
                        break;
                    case PixelBlenderColorComponent.G:
                        _mask_shift = CO.G_SHIFT;
                        break;
                    case PixelBlenderColorComponent.B:
                        _mask_shift = CO.B_SHIFT;
                        break;
                }
            }
        }
        /// <summary>
        /// new color output after applying with mask
        /// </summary>
        /// <param name="srcColor"></param>
        /// <param name="arrayOffset"></param>
        /// <returns></returns>
        Color NewColorFromMask(Color srcColor, int arrayOffset)
        {
            unsafe
            {
                int* ptr = (int*)_maskInnerBuffer.Ptr;
                return srcColor.NewFromChangeCoverage((byte)((ptr[arrayOffset]) >> _mask_shift));
            }
        }
        internal override void BlendPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                fixed (int* head = &dstBuffer[arrayOffset])
                {
                    BlendPixel32Internal(head, NewColorFromMask(srcColor, arrayOffset), _selectedDestMaskComponent);
                }
            }
        }

        internal override unsafe void BlendPixel32(int* dstPtr, Color srcColor)
        {
            BlendPixel32Internal(dstPtr, srcColor, _selectedDestMaskComponent);
        }

        internal override void BlendPixels(int[] dstBuffer,
            int arrayElemOffset,
            Color[] srcColors,
            int srcColorOffset,
            byte[] covers,
            int coversIndex,
            bool firstCoverForAll, int count)
        {

            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {

                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                                //---------
                                //2
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        fixed (int* head = &dstBuffer[arrayElemOffset])
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                                //2.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                            }
                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    fixed (int* dstHead = &dstBuffer[arrayElemOffset])
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset], arrayElemOffset));
                            }
                            else
                            {
                                BlendPixel(dstBuffer, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset].NewFromChangeCoverage(cover), arrayElemOffset));
                            }

                            arrayElemOffset++;
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }
            }
        }
        internal override void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                fixed (int* ptr = &dstBuffer[arrayOffset])
                {
                    BlendPixel32(ptr, NewColorFromMask(srcColor, arrayOffset));
                }
            }
        }

        internal override void CopyPixels(int[] dstBuffer, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                fixed (int* ptr_byte = &dstBuffer[arrayOffset])
                {
                    //TODO: consider use memcpy() impl***
                    int* ptr = ptr_byte;

                    Color newColor = NewColorFromMask(srcColor, arrayOffset);

                    //---------
                    if ((count % 2) != 0)
                    {
                        BlendPixel32(ptr, newColor);

                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------

                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                        //-----------

                        //2
                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }
                }
            }
        }

        static unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor, EnableOutputColorComponent enableCompo)
        {
            unchecked
            {
                if (srcColor.A == 255)
                {
                    *dstPtr = srcColor.ToARGB(); //just copy
                }
                else
                {
                    int dest = *dstPtr;
                    //separate each component
                    byte a = (byte)((dest >> CO.A_SHIFT) & 0xff);
                    byte r = (byte)((dest >> CO.R_SHIFT) & 0xff);
                    byte g = (byte)((dest >> CO.G_SHIFT) & 0xff);
                    byte b = (byte)((dest >> CO.B_SHIFT) & 0xff);

                    byte src_a = srcColor.A;

                    switch (enableCompo)
                    {
                        case EnableOutputColorComponent.EnableAll:
                            {
                                *dstPtr =
                                 ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                                 ((byte)(((srcColor.R - r) * src_a + (r << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.R_SHIFT) |
                                 ((byte)(((srcColor.G - g) * src_a + (g << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.G_SHIFT) |
                                 ((byte)(((srcColor.B - b) * src_a + (b << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT);
                            }
                            break;
                        case EnableOutputColorComponent.R:
                            {
                                *dstPtr =
                                   ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                                   ((byte)(((srcColor.R - r) * src_a + (r << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.R_SHIFT) |
                                    (g << CO.G_SHIFT) |
                                    (b << CO.B_SHIFT);
                            }
                            break;
                        case EnableOutputColorComponent.G:
                            {
                                *dstPtr =
                                ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                                (r << CO.R_SHIFT) |
                                ((byte)(((srcColor.G - g) * src_a + (g << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.G_SHIFT) |
                                (b << CO.B_SHIFT);

                            }
                            break;
                        case EnableOutputColorComponent.B:
                            {
                                *dstPtr =
                                 ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> CO.BASE_SHIFT)) << CO.A_SHIFT) |
                                 (r << CO.R_SHIFT) |
                                 (g << CO.G_SHIFT) |
                                 ((byte)(((srcColor.B - b) * src_a + (b << CO.BASE_SHIFT)) >> CO.BASE_SHIFT) << CO.B_SHIFT);
                            }
                            break;
                    }
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* dst = (int*)dstBuffer.Ptr;
                int* head = &dst[arrayOffset];
                {
                    BlendPixel32Internal(head, NewColorFromMask(srcColor, arrayOffset), _selectedDestMaskComponent);
                }
            }
        }

        internal override void BlendPixels(TempMemPtr dst, int arrayElemOffset, Color[] srcColors,
            int srcColorOffset, byte[] covers, int coversIndex, bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dst.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }

                            //now count is even number
                            while (count > 0)
                            {
                                //now count is even number
                                //---------
                                //1
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                                //---------
                                //2
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++]);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                header2++;//move next
                                count--;
                                arrayElemOffset++;
                            }
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        int* dstBuffer = (int*)dst.Ptr;
                        int* head = &dstBuffer[arrayElemOffset];
                        {
                            int* header2 = head;

                            if (count % 2 != 0)
                            {
                                //odd
                                //
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;
                                header2++;//move next
                                count--;
                            }
                            while (count > 0)
                            {
                                //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                                //1.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                                //2.
                                //BlendPixel32Internal(header2, srcColors[srcColorOffset++], cover);
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset++], arrayElemOffset));
                                arrayElemOffset++;

                                header2++;//move next
                                count--;
                            }
                        }
                    }
                }
            }
            else
            {
                unsafe
                {
                    int* dstBuffer = (int*)dst.Ptr;
                    int* dstHead = &dstBuffer[arrayElemOffset];
                    {
                        int* dstBufferPtr = dstHead;
                        do
                        {
                            //cover may diff in each loop
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset], arrayElemOffset));
                            }
                            else
                            {
                                BlendPixels(dst, arrayElemOffset, NewColorFromMask(srcColors[srcColorOffset].NewFromChangeCoverage(cover), arrayElemOffset));
                            }

                            arrayElemOffset++;
                            dstBufferPtr++;
                            ++srcColorOffset;
                        }
                        while (--count != 0);
                    }
                }
            }
        }

        internal override void CopyPixels(TempMemPtr dst, int arrayOffset, Color srcColor, int count)
        {
            unsafe
            {
                int* dstBuffer = (int*)dst.Ptr;
                int* ptr_byte = &dstBuffer[arrayOffset];
                {
                    //TODO: consider use memcpy() impl***
                    int* ptr = ptr_byte;

                    Color newColor = NewColorFromMask(srcColor, arrayOffset);

                    //---------
                    if ((count % 2) != 0)
                    {
                        BlendPixel32(ptr, newColor);

                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }

                    while (count > 0)
                    {
                        //-----------

                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                        //-----------

                        //2
                        newColor = NewColorFromMask(srcColor, arrayOffset);

                        //1.
                        BlendPixel32(ptr, newColor);
                        arrayOffset++;//move next
                        ptr++; //move next
                        count--;
                    }
                }
            }
        }

        internal override void CopyPixel(TempMemPtr dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                int* dst = (int*)dstBuffer.Ptr;
                int* ptr = &dst[arrayOffset];
                {
                    BlendPixel32(ptr, NewColorFromMask(srcColor, arrayOffset));
                }
            }
        }
    }


}
