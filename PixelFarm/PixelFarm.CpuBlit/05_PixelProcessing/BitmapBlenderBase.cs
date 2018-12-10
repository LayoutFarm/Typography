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


namespace PixelFarm.CpuBlit.PixelProcessing
{

    /// <summary>
    /// base class for access(read/write/blend) pixel buffer
    /// </summary>
    public abstract class BitmapBlenderBase : IBitmapBlender
    {
        const int BASE_MASK = 255;
        //--------------------------------------------
        //look up table , for random access to specific point of image buffer
        //we pre-calculate the offset of each row and column
        int[] _yTableArray;
        int[] _xTableArray;
        //--------------------------------------------
        PixelBlenderBGRA _defaultPixelBlender;

        IntPtr _raw_buffer32;
        int _rawBufferLenInBytes;

        //--------------------------------------------
        // Pointer to first pixel depending on strideInBytes and image position         
        protected internal int _int32ArrayStartPixelAt;
        int _width;  // in pixels
        int _height; // in pixels
        int _strideInBytes; // Number of bytes per row,  Can be < 0
        int _m_DistanceInBytesBetweenPixelsInclusive;
        int _bitDepth;

        PixelBlender32 _outputPxBlender;

        public IntPtr GetInternalBufferPtr32
        {
            get
            {
                return _raw_buffer32;
            }
        }
        public Imaging.TempMemPtr GetBufferPtr()
        {
            return new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes);
        }
        protected void SetBufferToNull()
        {
            _raw_buffer32 = IntPtr.Zero;
            _rawBufferLenInBytes = 0;
        }

        protected void SetBuffer(Imaging.TempMemPtr tmpMem)
        {
            _raw_buffer32 = tmpMem.Ptr;
            _rawBufferLenInBytes = tmpMem.LengthInBytes;
        }

        public abstract void WriteBuffer(int[] newbuffer);

        protected void Attach(MemBitmap bmp, PixelBlender32 pixelBlender = null)
        {
            if (pixelBlender == null)
            {
                //use default pixel blender ?
                if (_defaultPixelBlender == null)
                {
                    _defaultPixelBlender = new PixelBlenderBGRA();
                }
                pixelBlender = _defaultPixelBlender;
            }
            Attach(bmp.Width, bmp.Height, bmp.BitDepth, MemBitmap.GetBufferPtr(bmp), pixelBlender);
        }
        /// <summary>
        /// attach image buffer and its information to the reader
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="bitsPerPixel"></param>
        /// <param name="imgbuffer"></param>
        /// <param name="outputPxBlender"></param>
        protected void Attach(int width, int height, int bitsPerPixel, CpuBlit.Imaging.TempMemPtr imgbuffer, PixelBlender32 outputPxBlender)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException("You must have a width and height > than 0.");
            }
            if (bitsPerPixel != 32 && bitsPerPixel != 24 && bitsPerPixel != 8)
            {
                throw new Exception("Unsupported bits per pixel.");
            }
            //
            //
            int bytesPerPixel = (bitsPerPixel + 7) / 8;
            int stride = 4 * ((width * bytesPerPixel + 3) / 4);

#if DEBUG
            if (bytesPerPixel == 0)
            {

            }
#endif            
            //
            SetDimmensionAndFormat(width, height, stride, bitsPerPixel, bitsPerPixel / 8);
            SetUpLookupTables();
            //

            this.OutputPixelBlender = outputPxBlender;
            //

            _raw_buffer32 = imgbuffer.Ptr;
            _rawBufferLenInBytes = imgbuffer.LengthInBytes;

        }


        protected void SetDimmensionAndFormat(int width, int height,
           int strideInBytes,
           int bitDepth,
           int distanceInBytesBetweenPixelsInclusive)
        {
            _width = width;
            _height = height;
            _strideInBytes = strideInBytes;
            _bitDepth = bitDepth;

            if (distanceInBytesBetweenPixelsInclusive > 4)
            {
                throw new System.Exception("It looks like you are passing bits per pixel rather than distance in bytes.");
            }
            if (distanceInBytesBetweenPixelsInclusive < (bitDepth / 8))
            {
                throw new Exception("You do not have enough room between pixels to support your bit depth.");
            }
            _m_DistanceInBytesBetweenPixelsInclusive = distanceInBytesBetweenPixelsInclusive;
            if (strideInBytes < distanceInBytesBetweenPixelsInclusive * width)
            {
                throw new Exception("You do not have enough strideInBytes to hold the width and pixel distance you have described.");
            }
        }

        void CopyFromNoClipping(IBitmapSrc sourceImage, RectInt clippedSourceImageRect, int destXOffset, int destYOffset)
        {
            if (BytesBetweenPixelsInclusive != BitDepth / 8
                || sourceImage.BytesBetweenPixelsInclusive != sourceImage.BitDepth / 8)
            {
                throw new Exception("WIP we only support packed pixel formats at this time.");
            }

            if (BitDepth == sourceImage.BitDepth)
            {
                int lengthInBytes = clippedSourceImageRect.Width * BytesBetweenPixelsInclusive;
                int sourceOffset = sourceImage.GetBufferOffsetXY32(clippedSourceImageRect.Left, clippedSourceImageRect.Bottom);

                unsafe
                {

                    using (CpuBlit.Imaging.TempMemPtr memPtr = sourceImage.GetBufferPtr())
                    using (CpuBlit.Imaging.TempMemPtr destPtr = this.GetBufferPtr())
                    {
                        byte* sourceBuffer = (byte*)memPtr.Ptr;
                        byte* destBuffer = (byte*)destPtr.Ptr;
                        int destOffset = GetBufferOffsetXY32(clippedSourceImageRect.Left + destXOffset, clippedSourceImageRect.Bottom + destYOffset);

                        for (int i = 0; i < clippedSourceImageRect.Height; i++)
                        {
                            MemMx.memmove(destBuffer, destOffset * 4, sourceBuffer, sourceOffset, lengthInBytes);
                            sourceOffset += sourceImage.Stride;
                            destOffset += Stride;
                        }
                    }
                }

            }
            else
            {
                bool haveConversion = true;
                switch (sourceImage.BitDepth)
                {
                    case 24:
                        switch (BitDepth)
                        {
                            case 32:
                                {
                                    //TODO: review here, this may not correct
                                    int numPixelsToCopy = clippedSourceImageRect.Width;
                                    for (int i = clippedSourceImageRect.Bottom; i < clippedSourceImageRect.Top; i++)
                                    {
                                        int sourceOffset = sourceImage.GetBufferOffsetXY32(clippedSourceImageRect.Left, clippedSourceImageRect.Bottom + i);

                                        //byte[] sourceBuffer = sourceImage.GetBuffer();
                                        //byte[] destBuffer = GetBuffer();

                                        using (CpuBlit.Imaging.TempMemPtr srcMemPtr = sourceImage.GetBufferPtr())
                                        using (CpuBlit.Imaging.TempMemPtr destBufferPtr = this.GetBufferPtr())
                                        {

                                            int destOffset = GetBufferOffsetXY32(
                                              clippedSourceImageRect.Left + destXOffset,
                                              clippedSourceImageRect.Bottom + i + destYOffset);
                                            unsafe
                                            {
                                                int* destBuffer = (int*)destBufferPtr.Ptr;
                                                int* sourceBuffer = (int*)srcMemPtr.Ptr;
                                                for (int x = 0; x < numPixelsToCopy; x++)
                                                {
                                                    int color = sourceBuffer[sourceOffset++];

                                                    destBuffer[destOffset++] =
                                                         (255 << 24) | //a
                                                         (color & 0xff0000) | //b
                                                         (color & 0x00ff00) | //g
                                                         (color & 0xff);

                                                    //destBuffer[destOffset++] = sourceBuffer[sourceOffset++];
                                                    //destBuffer[destOffset++] = sourceBuffer[sourceOffset++];
                                                    //destBuffer[destOffset++] = 255;
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            default:
                                haveConversion = false;
                                break;
                        }
                        break;
                    default:
                        haveConversion = false;
                        break;
                }

                if (!haveConversion)
                {
                    throw new NotImplementedException("You need to write the " + sourceImage.BitDepth.ToString() + " to " + BitDepth.ToString() + " conversion");
                }
            }
        }

        public void CopyFrom(IBitmapSrc sourceImage, RectInt sourceImageRect, int destXOffset, int destYOffset)
        {
            RectInt sourceImageBounds = sourceImage.GetBounds();
            RectInt clippedSourceImageRect = new RectInt();
            if (clippedSourceImageRect.IntersectRectangles(sourceImageRect, sourceImageBounds))
            {
                RectInt destImageRect = clippedSourceImageRect;
                destImageRect.Offset(destXOffset, destYOffset);
                RectInt destImageBounds = GetBounds();
                RectInt clippedDestImageRect = new RectInt();
                if (clippedDestImageRect.IntersectRectangles(destImageRect, destImageBounds))
                {
                    // we need to make sure the source is also clipped to the dest. So, we'll copy this back to source and offset it.
                    clippedSourceImageRect = clippedDestImageRect;
                    clippedSourceImageRect.Offset(-destXOffset, -destYOffset);
                    CopyFromNoClipping(sourceImage, clippedSourceImageRect, destXOffset, destYOffset);
                }
            }
        }

        public int Width => _width;
        public int Height => _height;
        public int Stride => _strideInBytes;

        public int BytesBetweenPixelsInclusive => _m_DistanceInBytesBetweenPixelsInclusive;
        public int BitDepth => _bitDepth;
        public RectInt GetBounds() => new RectInt(0, 0, _width, _height);

        /// <summary>
        /// get, set blender of destination image buffer
        /// </summary>
        /// <returns></returns>
        public PixelBlender32 OutputPixelBlender
        {
            get => _outputPxBlender;
            set
            {
#if DEBUG
                if (BitDepth != 0 && value != null && PixelBlender32.NUM_PIXEL_BITS != BitDepth)
                {
                    throw new NotSupportedException("The blender has to support the bit depth of this image.");
                }
#endif
                _outputPxBlender = value;
            }
        }

        protected void SetUpLookupTables()
        {
            _yTableArray = new int[_height];
            _xTableArray = new int[_width];
            unsafe
            {
                fixed (int* first = &_yTableArray[0])
                {
                    //go last
                    int* cur = first + _height - 1;
                    for (int i = _height - 1; i >= 0;)
                    {
                        //--------------------
                        //*cur = i * strideInBytes;
                        *cur = i * _width;
                        --i;
                        cur--;
                        //--------------------
                    }
                }
                fixed (int* first = &_xTableArray[0])
                {
                    //go last
                    int* cur = first + _width - 1;
                    //even
                    for (int i = _width - 1; i >= 0;)
                    {
                        //--------------------
                        //*cur = i * m_DistanceInBytesBetweenPixelsInclusive;
                        *cur = i * 1;
                        --i;
                        cur--;
                        //--------------------
                    }
                }
            }


            if (_yTableArray.Length != _height || _xTableArray.Length != _width)
            {
                // LBB, don't fix this if you don't understand what it's trying to do.
                throw new Exception("The yTable and xTable should be allocated correctly at this point. Figure out what happend.");
            }
        }
        public static void CopySubBufferToInt32Array(BitmapBlenderBase buff, int mx, int my, int w, int h, int[] buffer)
        {
            //TODO: review here, 
            //check pixel format for an image buffer before use
            //if mBuffer is not 32 bits ARGB => this may not correct

            int i = 0;
            unsafe
            {
                int* mBuffer = (int*)buff._raw_buffer32;
                for (int y = my; y < h; ++y)
                {
                    //int xbufferOffset = buff.GetBufferOffsetXY(0, y);
                    int xbuffOffset32 = buff.GetBufferOffsetXY32(0, y);

                    for (int x = mx; x < w; ++x)
                    {
                        //A R G B

                        int val = mBuffer[xbuffOffset32];

                        //TODO: A =?
                        byte r = (byte)((val >> 16) & 0xff);// mBuffer[xbufferOffset + 2];
                        byte g = (byte)((val >> 8) & 0xff);// mBuffer[xbufferOffset + 1];
                        byte b = (byte)((val >> 0) & 0xff);// mBuffer[xbufferOffset];


                        //xbufferOffset += 4;
                        xbuffOffset32++;
                        //
                        buffer[i] = b | (g << 8) | (r << 16);
                        i++;
                    }
                }
            }

        }
        public Color GetPixel(int x, int y)
        {
            unsafe
            {
                return _outputPxBlender.PixelToColorRGBA((int*)_raw_buffer32, GetBufferOffsetXY32(x, y));
            }
        }

        public int GetBufferOffsetXY32Check(int x, int y)
        {

            if (y >= _height || x >= _width)
            {
                return -1;
            }
            return _int32ArrayStartPixelAt + _yTableArray[y] + _xTableArray[x];
        }

        public int GetBufferOffsetXY32(int x, int y)
        {
            return _int32ArrayStartPixelAt + _yTableArray[y] + _xTableArray[x];
        }
        public void SetPixel(int x, int y, Color color)
        {
            _outputPxBlender.CopyPixel(new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes), GetBufferOffsetXY32(x, y), color);
        }

        public void CopyHL(int x, int y, int len, Color sourceColor)
        {

            _outputPxBlender.CopyPixels(new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes), GetBufferOffsetXY32(x, y), sourceColor, len);
        }

        public void CopyVL(int x, int y, int len, Color sourceColor)
        {
            throw new NotImplementedException();
#if false
            int scanWidth = StrideInBytes();
            byte* pDestBuffer = GetPixelPointerXY(x, y);
            do
            {
                m_Blender.CopyPixel(pDestBuffer, sourceColor);
                pDestBuffer = &pDestBuffer[scanWidth];
            }
            while (--len != 0);
#endif
        }
        public void BlendHL(int x1, int y, int x2, Color sourceColor, byte cover)
        {
            if (sourceColor.A == 0) { return; }
            //-------------------------------------------------

            int len = x2 - x1 + 1;

            int bufferOffset = GetBufferOffsetXY32(x1, y);

            int alpha = (((int)(sourceColor.A) * (cover + 1)) >> 8);
            if (alpha == BASE_MASK)
            {
                //full
                //int[] buffer = this.GetBuffer32();
                _outputPxBlender.CopyPixels(this.GetBufferPtr(), bufferOffset, sourceColor, len);

            }
            else
            {
                Color c2 = Color.FromArgb(alpha, sourceColor);

                Imaging.TempMemPtr buffer = this.GetBufferPtr();
                do
                {
                    //copy pixel-by-pixel
                    _outputPxBlender.BlendPixels(buffer, bufferOffset, c2);
                    bufferOffset++;
                }
                while (--len != 0);
            }


            //byte[] buffer = GetBuffer();
            //int bufferOffset = GetBufferOffsetXY(x1, y);
            //int alpha = (((int)(sourceColor.A) * (cover + 1)) >> 8);
            //if (alpha == BASE_MASK)
            //{
            //    //full
            //    _recvBlender32.CopyPixels(buffer, bufferOffset, sourceColor, len);
            //}
            //else
            //{
            //    Color c2 = Color.FromArgb(alpha, sourceColor);
            //    do
            //    {
            //        //copy pixel-by-pixel
            //        _recvBlender32.BlendPixel(buffer, bufferOffset, c2);
            //        bufferOffset += m_DistanceInBytesBetweenPixelsInclusive;
            //    }
            //    while (--len != 0);
            //}


        }

        public void BlendVL(int x, int y1, int y2, Color sourceColor, byte cover)
        {
            throw new NotImplementedException();
#if false
            int ScanWidth = StrideInBytes();
            if (sourceColor.m_A != 0)
            {
                unsafe
                {
                    int len = y2 - y1 + 1;
                    byte* p = GetPixelPointerXY(x, y1);
                    sourceColor.m_A = (byte)(((int)(sourceColor.m_A) * (cover + 1)) >> 8);
                    if (sourceColor.m_A == base_mask)
                    {
                        byte cr = sourceColor.m_R;
                        byte cg = sourceColor.m_G;
                        byte cb = sourceColor.m_B;
                        do
                        {
                            m_Blender.CopyPixel(p, sourceColor);
                            p = &p[ScanWidth];
                        }
                        while (--len != 0);
                    }
                    else
                    {
                        if (cover == 255)
                        {
                            do
                            {
                                m_Blender.BlendPixel(p, sourceColor);
                                p = &p[ScanWidth];
                            }
                            while (--len != 0);
                        }
                        else
                        {
                            do
                            {
                                m_Blender.BlendPixel(p, sourceColor);
                                p = &p[ScanWidth];
                            }
                            while (--len != 0);
                        }
                    }
                }
            }
#endif
        }


        MemBitmap _filterBmp;
        public void SetFilterImage(MemBitmap filterBmp)
        {
            _filterBmp = filterBmp;
        }
        public void BlendSolidHSpan(int x, int y, int len, Color sourceColor, byte[] covers, int coversIndex)
        {
            int colorAlpha = sourceColor.alpha;
            if (colorAlpha != 0)
            {
                Imaging.TempMemPtr buffer = this.GetBufferPtr();
                int bufferOffset32 = GetBufferOffsetXY32(x, y);
                do
                {
                    int alpha = ((colorAlpha) * ((covers[coversIndex]) + 1)) >> 8;
                    if (alpha == BASE_MASK)
                    {
                        _outputPxBlender.CopyPixel(buffer, bufferOffset32, sourceColor);
                    }
                    else
                    {
                        _outputPxBlender.BlendPixels(buffer, bufferOffset32, Color.FromArgb(alpha, sourceColor));
                    }

                    bufferOffset32++;
                    coversIndex++;
                }
                while (--len != 0);
            }
        }

        public void BlendSolidVSpan(int x, int y, int len, Color sourceColor, byte[] covers, int coversIndex)
        {
            if (sourceColor.A != 0)
            {
                int scanWidthInBytes = Stride;
                unchecked
                {

                    int bufferOffset32 = GetBufferOffsetXY32(x, y);
                    int actualW = scanWidthInBytes / 4;
                    Imaging.TempMemPtr dst = new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes);
                    do
                    {
                        //TODO: review here again
                        Color newcolor = sourceColor.NewFromChangeCoverage(covers[coversIndex++]);
                        if (newcolor.alpha == BASE_MASK)
                        {
                            _outputPxBlender.CopyPixel(dst, bufferOffset32, newcolor);
                        }
                        else
                        {
                            _outputPxBlender.BlendPixels(dst, bufferOffset32, newcolor);
                        }
                        bufferOffset32 += actualW;//vertically move to next line ***
                    }
                    while (--len != 0);


                }
            }
        }

        public void CopyColorHSpan(int x, int y, int len, Color[] colors, int colorsIndex)
        {
            int bufferOffset32 = GetBufferOffsetXY32(x, y);
            Imaging.TempMemPtr dst = new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes);
            do
            {
                _outputPxBlender.CopyPixel(dst, bufferOffset32, colors[colorsIndex]);
                ++colorsIndex;
                bufferOffset32++;
            }
            while (--len != 0);
        }

        public void CopyColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex)
        {
            int bufferOffset32 = GetBufferOffsetXY32(x, y);
            int actualW = _strideInBytes / 4;
            Imaging.TempMemPtr dst = new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes);
            do
            {
                _outputPxBlender.CopyPixel(dst, bufferOffset32, colors[colorsIndex]);
                ++colorsIndex;
                bufferOffset32 += actualW; //vertically move to next line ***
            }
            while (--len != 0);
        }
        public void BlendColorHSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {

            int bufferOffset32 = GetBufferOffsetXY32Check(x, y);
            if (bufferOffset32 > -1)
            {
                Imaging.TempMemPtr dst = new Imaging.TempMemPtr(_raw_buffer32, _rawBufferLenInBytes);
                _outputPxBlender.BlendPixels(dst, bufferOffset32, colors, colorsIndex, covers, coversIndex, firstCoverForAll, len);
            }
            else
            {

            }
        }

        public void BlendColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {

            int bufferOffset32 = GetBufferOffsetXY32(x, y);
            int scanWidthBytes = System.Math.Abs(Stride);
            if (!firstCoverForAll)
            {
                unsafe
                {
                    //fixed (int* dstBuffer = &raw_buffer32[0])
                    int* dstBuffer = (int*)_raw_buffer32;
                    {
                        int actualWidth = scanWidthBytes / 4;
                        do
                        {

                            //-----------------------
                            int cover = covers[coversIndex++];
                            if (cover == 255)
                            {
                                _outputPxBlender.BlendPixel32(dstBuffer + bufferOffset32, colors[colorsIndex]);
                            }
                            else
                            {
                                _outputPxBlender.BlendPixel32(dstBuffer + bufferOffset32, colors[colorsIndex].NewFromChangeCoverage(cover));
                            }
                            //-----------------------

                            //bufferOffset += actualWidth;
                            bufferOffset32++;
                            ++colorsIndex;
                        }
                        while (--len != 0);
                    }
                }

            }
            else
            {
                if (covers[coversIndex] == 255)
                {
                    unsafe
                    {
                        //fixed (int* destH = &_raw_buffer32)
                        int* destH = (int*)_raw_buffer32;
                        {
                            int* destBuffer = (int*)destH;
                            int actualWidth = scanWidthBytes / 4;

                            do
                            {
                                _outputPxBlender.BlendPixel32(destBuffer, colors[colorsIndex]);
                                //CopyOrBlend32_BasedOnAlpha(_recvBlender32, m_ByteBuffer, bufferOffset, colors[colorsIndex]);
                                //bufferOffset += scanWidthBytes;
                                ++colorsIndex;
                                destBuffer += actualWidth;
                            }
                            while (--len != 0);
                        }
                    }
                }
                else
                {
                    unsafe
                    {
                        //fixed (int* head = &raw_buffer32[0])
                        int* head = (int*)_raw_buffer32;
                        {

                            int actualWidth = scanWidthBytes / 4;
                            do
                            {
                                //-----------------------
                                byte cover = covers[coversIndex++];

                                if (cover == 255)
                                {
                                    //full cover => so use original color
                                    _outputPxBlender.BlendPixel32(head + bufferOffset32, colors[colorsIndex]);
                                }
                                else
                                {
                                    //not full => use new color (change alpha) 
                                    _outputPxBlender.BlendPixel32(head + bufferOffset32, colors[colorsIndex].NewFromChangeCoverage(cover));
                                }
                                //-----------------------

                                // bufferOffset += actualWidth;
                                ++colorsIndex;
                            }
                            while (--len != 0);
                        }
                    }

                }
            }
        }
        public RectInt GetBoundingRect()
        {
            return new RectInt(0, 0, Width, Height);
        }

#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugGetNewDebugId();
        static int dbugGetNewDebugId()
        {
            return dbugTotalId++;
        }
#endif


        public int[] GetOrgInt32Buffer()
        {
            return null;
            //return this.raw_buffer32;
        }
        //        static unsafe void CopyOrBlend32_BasedOnAlpha(PixelBlenderBGRA recieveBlender,
        //            int* destBuffer,
        //            int arrayOffset,
        //            Color sourceColor)
        //        {
        //            //if (sourceColor.m_A != 0)
        //            {
        //#if false // we blend regardless of the alpha so that we can get Light Opacity working (used this way we have addative and faster blending in one blender) LBB
        //                if (sourceColor.m_A == base_mask)
        //                {
        //                    Blender.CopyPixel(pDestBuffer, sourceColor);
        //                }
        //                else
        //#endif
        //                {
        //                    PixelBlenderBGRA.Blend32PixelInternal(destBuffer + arrayOffset, sourceColor);
        //                }
        //            }
        //        }


        //        static void CopyOrBlend_BasedOnAlpha(IPixelBlender recieveBlender,
        //        byte[] destBuffer,
        //        int bufferOffset,
        //        Color sourceColor)
        //        {
        //            //if (sourceColor.m_A != 0)
        //            {
        //#if false // we blend regardless of the alpha so that we can get Light Opacity working (used this way we have addative and faster blending in one blender) LBB
        //                if (sourceColor.m_A == base_mask)
        //                {
        //                    Blender.CopyPixel(pDestBuffer, sourceColor);
        //                }
        //                else
        //#endif
        //                {
        //                    recieveBlender.BlendPixel(destBuffer, bufferOffset, sourceColor);
        //                }
        //            }
        //        }

        //        static unsafe void CopyOrBlend32_BasedOnAlphaAndCover(IPixelBlender recieveBlender, int[] destBuffer, int arrayElemOffset, Color sourceColor, int cover)
        //        {
        //            if (cover == 255)
        //            {
        //                //CopyOrBlend_BasedOnAlpha(recieveBlender, destBuffer, bufferOffset, sourceColor);

        //                fixed (int* dest = &destBuffer[arrayElemOffset])
        //                {
        //                    recieveBlender.BlendPixel(destBuffer, bufferOffset, sourceColor);
        //                }

        //            }
        //            else
        //            {
        //                //if (sourceColor.m_A != 0)
        //                {

        //#if false // we blend regardless of the alpha so that we can get Light Opacity working (used this way we have addative and faster blending in one blender) LBB
        //                    if (sourceColor.m_A == base_mask)
        //                    {
        //                        Blender.CopyPixel(pDestBuffer, sourceColor);
        //                    }
        //                    else
        //#endif
        //                    {
        //                        recieveBlender.BlendPixel(destBuffer, bufferOffset, sourceColor.NewFromChangeCoverage(cover));
        //                    }
        //                }
        //            }
        //        }

        //public void apply_gamma_inv(GammaLookUpTable g)
        //{
        //    throw new System.NotImplementedException();
        //    //for_each_pixel(apply_gamma_inv_rgba<color_type, order_type, GammaLut>(g));
        //}

        //public bool IsPixelVisible(int x, int y)
        //{
        //    ColorRGBA pixelValue = GetRecieveBlender().PixelToColorRGBA_Bytes(m_ByteBuffer, GetBufferOffsetXY(x, y));
        //    return (pixelValue.Alpha0To255 != 0 || pixelValue.Red0To255 != 0 || pixelValue.Green0To255 != 0 || pixelValue.Blue0To255 != 0);
        //}


        //public override int GetHashCode()
        //{
        //    // This might be hard to make fast and usefull.
        //    return m_ByteBuffer.GetHashCode() ^ bufferOffset.GetHashCode() ^ bufferFirstPixel.GetHashCode();
        //}
        //public byte[] GetPixelPointerY(int y, out int bufferOffset)
        //{
        //    bufferOffset = bufferFirstPixel + yTableArray[y];
        //    return m_ByteBuffer;
        //}
    }







}
