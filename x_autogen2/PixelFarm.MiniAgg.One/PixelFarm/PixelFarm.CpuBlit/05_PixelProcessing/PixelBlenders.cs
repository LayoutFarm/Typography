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
#define USE_BLENDER

using System;
using PixelFarm.Drawing;


namespace PixelFarm.CpuBlit.PixelProcessing
{
    /// <summary>
    /// look up table helper for clamp value from 9 bits to 8 bits
    /// </summary>
    static class ClampFrom9To8Bits
    {
        internal static readonly byte[] _ = new byte[1 << 9];
        static ClampFrom9To8Bits()
        {
            //this is a clamp table
            //9 bits to 8 bits
            //if we don't use this clamp table
            for (int i = _.Length - 1; i >= 0; --i)
            {
                _[i] = (byte)Math.Min(i, 255);
            }
        }
    }


    public class PixelBlenderBGRA : PixelBlender32
    {
        //from https://microsoft.github.io/Win2D/html/PremultipliedAlpha.htm
        //1. Straight alpha
        //result = (source.RGB* source.A) + (dest.RGB* (1 - source.A))
        //---
        //2. Premultiplied alpha
        //result = source.RGB + (dest.RGB * (1 - source.A))
        //---
        //3. Converting between alpha formats
        //3.1 from straight to premult
        //premultiplied.R = (byte) (straight.R* straight.A / 255);
        //premultiplied.G = (byte) (straight.G* straight.A / 255);
        //premultiplied.B = (byte) (straight.B* straight.A / 255);
        //premultiplied.A = straight.A;
        //3.2 from premult to strait
        //straight.R = premultiplied.R  * ((1/straight.A) * 255);
        //straight.G = premultiplied.G  * ((1/straight.A) * 255);
        //straight.B = premultiplied.B  * ((1/straight.A) * 255);
        //straight.A = premultiplied.A;

        bool _enableGamma;
        float _gammaValue;
        public PixelBlenderBGRA() { }
      
        public bool EnableGamma
        {
            get { return _enableGamma; }
            set
            {

                if (value != _enableGamma)
                {

                }
                this._enableGamma = value;
            }
        }
        public float GammaValue
        {
            get { return _gammaValue; }
            set
            {
                _gammaValue = value;
                //TODO: 
                //get new gamma table
            }
        }


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
        internal override unsafe void BlendPixel32(int* ptr, Color sc)
        {
            BlendPixel32Internal(ptr, sc);
        }


        internal override void BlendPixels(
            int[] dstBuffer, int arrayElemOffset,
            Color[] srcColors, int srcColorOffset,
            byte[] covers, int coversIndex, bool firstCoverForAll, int count)
        {
            if (firstCoverForAll)
            {
                int cover = covers[coversIndex];
                if (cover == 255)
                {
                    //version 1
                    //do
                    //{
                    //    BlendPixel(destBuffer, bufferOffset, sourceColors[sourceColorsOffset++]);
                    //    bufferOffset += 4;
                    //}
                    //while (--count != 0);

                    //version 2
                    //unsafe
                    //{
                    //    fixed (byte* head = &destBuffer[bufferOffset])
                    //    {
                    //        int* header2 = (int*)(IntPtr)head;
                    //        do
                    //        {
                    //            Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++]);
                    //            header2++;//move next
                    //        }
                    //        while (--count != 0);
                    //    }
                    //}
                    //------------------------------
                    //version 3: similar to version 2, but have a plan
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
                    ////version 1
                    //do
                    //{
                    //    BlendPixel(destBuffer, bufferOffset, sourceColors[sourceColorsOffset].NewFromChangeCoverage(cover));
                    //    bufferOffset += 4;
                    //    ++sourceColorsOffset;
                    //}
                    //while (--count != 0);

                    ////version 2 
                    //unsafe
                    //{
                    //    fixed (byte* head = &destBuffer[bufferOffset])
                    //    {
                    //        int* header2 = (int*)(IntPtr)head;
                    //        do
                    //        {

                    //            //Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++].NewFromChangeCoverage(cover));
                    //            Blend32PixelInternal(header2, sourceColors[sourceColorsOffset++], cover);
                    //            header2++;//move next
                    //        }
                    //        while (--count != 0);
                    //    }
                    //}
                    //------------------------------
                    //version 3: similar to version 2, but have a plan
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
        internal override void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor)
        {
            unsafe
            {
                unchecked
                {
                    fixed (int* ptr = &dstBuffer[arrayOffset])
                    {
                        //TODO: consider use memcpy() impl*** 
                        *ptr = srcColor.ToARGB();
                    }
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
                        int* ptr = ptr_byte;
                        int argb = srcColor.ToARGB();

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
            int src_a = (byte)((srcColor.alpha * coverageValue + 255) >> 8);
            //after apply the alpha
            unchecked
            {
                if (src_a == 255)
                {
                    *dstPtr = srcColor.ToARGB(); //just copy
                }
                else
                {
                    int dest = *dstPtr;
                    //separate each component
                    byte a = (byte)((dest >> 24) & 0xff);
                    byte r = (byte)((dest >> 16) & 0xff);
                    byte g = (byte)((dest >> 8) & 0xff);
                    byte b = (byte)((dest) & 0xff);


                    *dstPtr =
                     ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> ColorEx.BASE_SHIFT)) << 24) |
                     ((byte)(((srcColor.red - r) * src_a + (r << ColorEx.BASE_SHIFT)) >> ColorEx.BASE_SHIFT) << 16) |
                     ((byte)(((srcColor.green - g) * src_a + (g << ColorEx.BASE_SHIFT)) >> (int)ColorEx.BASE_SHIFT) << 8) |
                     ((byte)(((srcColor.blue - b) * src_a + (b << ColorEx.BASE_SHIFT)) >> ColorEx.BASE_SHIFT));
                }
            }

        }
        static unsafe void BlendPixel32Internal(int* dstPtr, Color srcColor)
        {
            unchecked
            {
                if (srcColor.alpha == 255)
                {
                    *dstPtr = srcColor.ToARGB(); //just copy
                }
                else
                {
                    int dest = *dstPtr;
                    //separate each component
                    byte a = (byte)((dest >> 24) & 0xff);
                    byte r = (byte)((dest >> 16) & 0xff);
                    byte g = (byte)((dest >> 8) & 0xff);
                    byte b = (byte)((dest) & 0xff);

                    byte src_a = srcColor.alpha;

                    *dstPtr =
                     ((byte)((src_a + a) - ((src_a * a + BASE_MASK) >> ColorEx.BASE_SHIFT)) << 24) |
                     ((byte)(((srcColor.red - r) * src_a + (r << ColorEx.BASE_SHIFT)) >> ColorEx.BASE_SHIFT) << 16) |
                     ((byte)(((srcColor.green - g) * src_a + (g << ColorEx.BASE_SHIFT)) >> ColorEx.BASE_SHIFT) << 8) |
                     ((byte)(((srcColor.blue - b) * src_a + (b << ColorEx.BASE_SHIFT)) >> ColorEx.BASE_SHIFT));
                }
            }
        }


    }



    //        /// <summary>
    //        /// pre-multiplied alpha rgba
    //        /// </summary>
    //        public sealed class PixelBlenderPreMultBGRA : PixelBlenderBGRABase, IPixelBlender
    //        {
    //            //from https://microsoft.github.io/Win2D/html/PremultipliedAlpha.htm
    //            //1. Straight alpha
    //            //result = (source.RGB* source.A) + (dest.RGB* (1 - source.A))
    //            //---
    //            //2. Premultiplied alpha
    //            //result = source.RGB + (dest.RGB * (1 - source.A))
    //            //---
    //            //3. Converting between alpha formats
    //            //3.1 from straight to premult
    //            //premultiplied.R = (byte) (straight.R* straight.A / 255);
    //            //premultiplied.G = (byte) (straight.G* straight.A / 255);
    //            //premultiplied.B = (byte) (straight.B* straight.A / 255);
    //            //premultiplied.A = straight.A;
    //            //3.2 from premult to strait
    //            //straight.R = premultiplied.R  * ((1/straight.A) * 255);
    //            //straight.G = premultiplied.G  * ((1/straight.A) * 255);
    //            //straight.B = premultiplied.B  * ((1/straight.A) * 255);
    //            //straight.A = premultiplied.A;
    //            public PixelBlenderPreMultBGRA()
    //            {
    //            }

    //            public Color PixelToColorRGBA(byte[] buffer, int bufferOffset)
    //            {
    //                //TODO: review here, this may not correct for pre-multiplied alpha RGB
    //                return new Color(buffer[bufferOffset + CO.A],
    //                    buffer[bufferOffset + CO.R],
    //                    buffer[bufferOffset + CO.G],
    //                    buffer[bufferOffset + CO.B]);
    //            }

    //            public void CopyPixels(byte[] buffer, int bufferOffset, Color sourceColor, int count)
    //            {
    //                do
    //                {
    //                    buffer[bufferOffset + CO.R] = sourceColor.red;
    //                    buffer[bufferOffset + CO.G] = sourceColor.green;
    //                    buffer[bufferOffset + CO.B] = sourceColor.blue;
    //                    buffer[bufferOffset + CO.A] = sourceColor.alpha;
    //                    bufferOffset += 4;

    //                } while (--count != 0);
    //            }
    //            public void CopyPixel(byte[] buffer, int bufferOffset, Color sourceColor)
    //            {
    //                buffer[bufferOffset + CO.R] = sourceColor.red;
    //                buffer[bufferOffset + CO.G] = sourceColor.green;
    //                buffer[bufferOffset + CO.B] = sourceColor.blue;
    //                buffer[bufferOffset + CO.A] = sourceColor.alpha;
    //            }

    //            public void BlendPixel(byte[] buffer, int bufferOffset, Color sourceColor)
    //            {
    //                //unsafe
    //                {
    //                    int oneOverAlpha = BASE_MASK - sourceColor.alpha;
    //                    unchecked
    //                    {
    //#if false
    //					Vector4i sourceColors = new Vector4i(sourceColor.m_B, sourceColor.m_G, sourceColor.m_R, sourceColor.m_A);
    //					Vector4i destColors = new Vector4i(
    //						pDestBuffer[bufferOffset + ImageBuffer.OrderB],
    //					    pDestBuffer[bufferOffset + ImageBuffer.OrderG],
    //					    pDestBuffer[bufferOffset + ImageBuffer.OrderB],
    //					    pDestBuffer[bufferOffset + ImageBuffer.OrderA]);
    //					Vector4i oneOverAlphaV = new Vector4i(oneOverAlpha, oneOverAlpha, oneOverAlpha, oneOverAlpha);
    //					Vector4i rounding = new Vector4i(255, 255, 255, 255);
    //					Vector4i temp = destColors * oneOverAlphaV + rounding;
    //					temp = temp >> 8;
    //					temp = temp + sourceColors;
    //					Vector8us packed8Final = Vector4i.PackWithUnsignedSaturation(temp, temp);
    //					Vector16b packed16Final = Vector8us.SignedPackWithUnsignedSaturation(packed8Final, packed8Final);
    //					pDestBuffer[bufferOffset + ImageBuffer.OrderR] = packed16Final.V2;
    //					pDestBuffer[bufferOffset + ImageBuffer.OrderG] = packed16Final.V1;
    //					pDestBuffer[bufferOffset + ImageBuffer.OrderB] = packed16Final.V0;
    //					pDestBuffer[bufferOffset + ImageBuffer.OrderA] = 255;

    //#else
    //                        byte r = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.R] * oneOverAlpha + 255) >> 8) + sourceColor.red];
    //                        byte g = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.G] * oneOverAlpha + 255) >> 8) + sourceColor.green];
    //                        byte b = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.B] * oneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                        byte a = buffer[bufferOffset + CO.A];
    //                        buffer[bufferOffset + CO.R] = r;
    //                        buffer[bufferOffset + CO.G] = g;
    //                        buffer[bufferOffset + CO.B] = b;
    //                        buffer[bufferOffset + CO.A] = (byte)(BASE_MASK - ClampFrom9To8Bits._[(oneOverAlpha * (BASE_MASK - a) + 255) >> 8]);
    //#endif
    //                    }
    //                }
    //            }

    //            public void BlendPixels(byte[] buffer, int bufferOffset,
    //                Color[] sourceColors, int sourceColorsOffset,
    //                byte[] sourceCovers, int sourceCoversOffset, bool firstCoverForAll, int count)
    //            {
    //                if (firstCoverForAll)
    //                {
    //                    //unsafe
    //                    {
    //                        if (sourceCovers[sourceCoversOffset] == 255)
    //                        {
    //                            for (int i = 0; i < count; i++)
    //                            {
    //#if false
    //                           BlendPixel(pDestBuffer, bufferOffset, sourceColors[sourceColorsOffset]);
    //#else
    //                                Color sourceColor = sourceColors[sourceColorsOffset];
    //                                if (sourceColor.alpha == 255)
    //                                {
    //                                    buffer[bufferOffset + CO.R] = sourceColor.red;
    //                                    buffer[bufferOffset + CO.G] = sourceColor.green;
    //                                    buffer[bufferOffset + CO.B] = sourceColor.blue;
    //                                    buffer[bufferOffset + CO.A] = 255;
    //                                }
    //                                else
    //                                {
    //                                    int OneOverAlpha = BASE_MASK - sourceColor.alpha;
    //                                    unchecked
    //                                    {
    //                                        byte r = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.R] * OneOverAlpha + 255) >> 8) + sourceColor.red];
    //                                        byte g = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.G] * OneOverAlpha + 255) >> 8) + sourceColor.green];
    //                                        byte b = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.B] * OneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                                        byte a = buffer[bufferOffset + CO.A];
    //                                        buffer[bufferOffset + CO.R] = r;
    //                                        buffer[bufferOffset + CO.G] = g;
    //                                        buffer[bufferOffset + CO.B] = b;
    //                                        buffer[bufferOffset + CO.A] = (byte)(BASE_MASK - ClampFrom9To8Bits._[(OneOverAlpha * (BASE_MASK - a) + 255) >> 8]);
    //                                    }
    //                                }
    //#endif
    //                                sourceColorsOffset++;
    //                                bufferOffset += 4;
    //                            }
    //                        }
    //                        else
    //                        {
    //                            for (int i = 0; i < count; i++)
    //                            {
    //                                Color sourceColor = sourceColors[sourceColorsOffset];
    //                                int alpha = (sourceColor.alpha * sourceCovers[sourceCoversOffset] + 255) / 256;
    //                                if (alpha == 0)
    //                                {
    //                                    continue;
    //                                }
    //                                else if (alpha == 255)
    //                                {
    //                                    buffer[bufferOffset + CO.R] = sourceColor.red;
    //                                    buffer[bufferOffset + CO.G] = sourceColor.green;
    //                                    buffer[bufferOffset + CO.B] = sourceColor.blue;
    //                                    buffer[bufferOffset + CO.A] = (byte)alpha;
    //                                }
    //                                else
    //                                {
    //                                    int OneOverAlpha = BASE_MASK - alpha;
    //                                    unchecked
    //                                    {
    //                                        byte r = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.R] * OneOverAlpha + 255) >> 8) + sourceColor.red];
    //                                        byte g = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.G] * OneOverAlpha + 255) >> 8) + sourceColor.green];
    //                                        byte b = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.B] * OneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                                        byte a = buffer[bufferOffset + CO.A];
    //                                        buffer[bufferOffset + CO.R] = r;
    //                                        buffer[bufferOffset + CO.G] = g;
    //                                        buffer[bufferOffset + CO.B] = b;
    //                                        buffer[bufferOffset + CO.A] = (byte)(BASE_MASK - ClampFrom9To8Bits._[(OneOverAlpha * (BASE_MASK - a) + 255) >> 8]);
    //                                    }
    //                                }
    //                                sourceColorsOffset++;
    //                                bufferOffset += 4;
    //                            }
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    for (int i = 0; i < count; i++)
    //                    {
    //                        Color sourceColor = sourceColors[sourceColorsOffset];
    //                        int alpha = (sourceColor.alpha * sourceCovers[sourceCoversOffset] + 255) / 256;
    //                        if (alpha == 255)
    //                        {
    //                            buffer[bufferOffset + CO.R] = (byte)sourceColor.red;
    //                            buffer[bufferOffset + CO.G] = (byte)sourceColor.green;
    //                            buffer[bufferOffset + CO.B] = (byte)sourceColor.blue;
    //                            buffer[bufferOffset + CO.A] = (byte)alpha;
    //                        }
    //                        else if (alpha > 0)
    //                        {
    //                            int OneOverAlpha = BASE_MASK - alpha;
    //                            unchecked
    //                            {
    //                                byte r = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.R] * OneOverAlpha + 255) >> 8) + sourceColor.red];
    //                                byte g = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.G] * OneOverAlpha + 255) >> 8) + sourceColor.green];
    //                                byte b = ClampFrom9To8Bits._[((buffer[bufferOffset + CO.B] * OneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                                byte a = buffer[bufferOffset + CO.A];
    //                                buffer[bufferOffset + CO.R] = r;
    //                                buffer[bufferOffset + CO.G] = g;
    //                                buffer[bufferOffset + CO.B] = b;
    //                                buffer[bufferOffset + CO.A] = (byte)(BASE_MASK - ClampFrom9To8Bits._[(OneOverAlpha * (BASE_MASK - a) + 255) >> 8]);
    //                            }
    //                        }
    //                        sourceColorsOffset++;
    //                        sourceCoversOffset++;
    //                        bufferOffset += 4;
    //                    }
    //                }
    //            }
    //        }

    //public sealed class PixelBlenderGammaBGRA : PixelBlenderBGRABase, IPixelBlender
    //{
    //    GammaLookUpTable m_gamma;
    //    static Dictionary<float, GammaLookUpTable> gammaTablePool = new Dictionary<float, GammaLookUpTable>();
    //    public PixelBlenderGammaBGRA(float gammaValue)
    //    {
    //        //TODO: review caching here
    //        GammaLookUpTable found;
    //        if (!gammaTablePool.TryGetValue(gammaValue, out found))
    //        {
    //            found = new GammaLookUpTable(gammaValue);
    //            gammaTablePool.Add(gammaValue, found);
    //        }

    //        this.m_gamma = found;
    //    }
    //    public Color PixelToColorRGBA(byte[] buffer, int bufferOffset)
    //    {
    //        return new Color(
    //            buffer[bufferOffset + CO.A],
    //            buffer[bufferOffset + CO.R],
    //            buffer[bufferOffset + CO.G],
    //            buffer[bufferOffset + CO.B]
    //            );
    //    }

    //    public void CopyPixels(byte[] buffer, int bufferOffset, Color sourceColor, int count)
    //    {
    //        do
    //        {
    //            buffer[bufferOffset + CO.R] = m_gamma.inv(sourceColor.red);
    //            buffer[bufferOffset + CO.G] = m_gamma.inv(sourceColor.green);
    //            buffer[bufferOffset + CO.B] = m_gamma.inv(sourceColor.blue);
    //            buffer[bufferOffset + CO.A] = m_gamma.inv(sourceColor.alpha);
    //            bufferOffset += 4;
    //        }
    //        while (--count != 0);
    //    }

    //    public void CopyPixel(byte[] buffer, int bufferOffset, Color sourceColor)
    //    {
    //        buffer[bufferOffset + CO.R] = m_gamma.inv(sourceColor.red);
    //        buffer[bufferOffset + CO.G] = m_gamma.inv(sourceColor.green);
    //        buffer[bufferOffset + CO.B] = m_gamma.inv(sourceColor.blue);
    //        buffer[bufferOffset + CO.A] = m_gamma.inv(sourceColor.alpha);
    //    }
    //    public void BlendPixel(byte[] buffer, int bufferOffset, Color sourceColor)
    //    {
    //        unchecked
    //        {
    //            byte r = buffer[bufferOffset + CO.R];
    //            byte g = buffer[bufferOffset + CO.G];
    //            byte b = buffer[bufferOffset + CO.B];
    //            byte a = buffer[bufferOffset + CO.A];
    //            buffer[bufferOffset + CO.R] = m_gamma.inv((byte)(((sourceColor.red - r) * sourceColor.alpha + (r << (int)ColorEx.BASE_SHIFT)) >> (int)ColorEx.BASE_SHIFT));
    //            buffer[bufferOffset + CO.G] = m_gamma.inv((byte)(((sourceColor.green - g) * sourceColor.alpha + (g << (int)ColorEx.BASE_SHIFT)) >> (int)ColorEx.BASE_SHIFT));
    //            buffer[bufferOffset + CO.B] = m_gamma.inv((byte)(((sourceColor.blue - b) * sourceColor.alpha + (b << (int)ColorEx.BASE_SHIFT)) >> (int)ColorEx.BASE_SHIFT));
    //            buffer[CO.A] = (byte)((sourceColor.alpha + a) - ((sourceColor.alpha * a + BASE_MASK) >> (int)ColorEx.BASE_SHIFT));
    //        }
    //    }

    //    public void BlendPixels(byte[] buffer, int bufferOffset,
    //        Color[] sourceColors, int sourceColorsOffset,
    //        byte[] sourceCovers, int sourceCoversOffset, bool firstCoverForAll, int count)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}


#if DEBUG
    //    public sealed class dbugPixelBlenderPolyColorPreMultBGRA : PixelBlenderBGRABase, IPixelBlender
    //    {

    //        Color polyColor;
    //        public dbugPixelBlenderPolyColorPreMultBGRA(Color polyColor)
    //        {
    //            this.polyColor = polyColor;
    //        }

    //        public Color PixelToColorRGBA(byte[] buffer, int bufferOffset)
    //        {
    //            return new Color(buffer[bufferOffset + CO.A], buffer[bufferOffset + CO.R], buffer[bufferOffset + CO.G], buffer[bufferOffset + CO.B]);
    //        }

    //        public void CopyPixels(byte[] buffer, int bufferOffset, Color sourceColor, int count)
    //        {
    //            for (int i = 0; i < count; i++)
    //            {
    //                buffer[bufferOffset + CO.R] = sourceColor.red;
    //                buffer[bufferOffset + CO.G] = sourceColor.green;
    //                buffer[bufferOffset + CO.B] = sourceColor.blue;
    //                buffer[bufferOffset + CO.A] = sourceColor.alpha;
    //                bufferOffset += 4;
    //            }
    //        }
    //        public void CopyPixel(byte[] buffer, int bufferOffset, Color sourceColor)
    //        {
    //            buffer[bufferOffset + CO.R] = sourceColor.red;
    //            buffer[bufferOffset + CO.G] = sourceColor.green;
    //            buffer[bufferOffset + CO.B] = sourceColor.blue;
    //            buffer[bufferOffset + CO.A] = sourceColor.alpha;
    //        }

    //        public void BlendPixel(byte[] pDestBuffer, int bufferOffset, Color sourceColor)
    //        {
    //            //unsafe
    //            {
    //                int sourceA = (byte)(ClampFrom9To8Bits._[(polyColor.Alpha0To255 * sourceColor.alpha + 255) >> 8]);
    //                int oneOverAlpha = BASE_MASK - sourceA;
    //                unchecked
    //                {
    //                    byte sourceR = ClampFrom9To8Bits._[(polyColor.Alpha0To255 * sourceColor.red + 255) >> 8];
    //                    byte sourceG = ClampFrom9To8Bits._[(polyColor.Alpha0To255 * sourceColor.green + 255) >> 8];
    //                    byte sourceB = ClampFrom9To8Bits._[(polyColor.Alpha0To255 * sourceColor.blue + 255) >> 8];
    //                    byte destR = ClampFrom9To8Bits._[((pDestBuffer[bufferOffset + CO.R] * oneOverAlpha + 255) >> 8) + sourceR];
    //                    byte destG = ClampFrom9To8Bits._[((pDestBuffer[bufferOffset + CO.G] * oneOverAlpha + 255) >> 8) + sourceG];
    //                    byte destB = ClampFrom9To8Bits._[((pDestBuffer[bufferOffset + CO.B] * oneOverAlpha + 255) >> 8) + sourceB];
    //                    // TODO: calculated the correct dest alpha
    //                    //int destA = pDestBuffer[bufferOffset + ImageBuffer.OrderA];

    //                    pDestBuffer[bufferOffset + CO.R] = destR;
    //                    pDestBuffer[bufferOffset + CO.G] = destG;
    //                    pDestBuffer[bufferOffset + CO.B] = destB;
    //                    //pDestBuffer[bufferOffset + ImageBuffer.OrderA] = (byte)(base_mask - m_Saturate9BitToByte[(oneOverAlpha * (base_mask - a) + 255) >> 8]);
    //                }
    //            }
    //        }

    //        public void BlendPixels(byte[] pDestBuffer, int bufferOffset,
    //            Color[] sourceColors, int sourceColorsOffset,
    //            byte[] sourceCovers, int sourceCoversOffset, bool firstCoverForAll, int count)
    //        {
    //            if (firstCoverForAll)
    //            {
    //                //unsafe
    //                {
    //                    if (sourceCovers[sourceCoversOffset] == 255)
    //                    {
    //                        for (int i = 0; i < count; i++)
    //                        {
    //                            BlendPixel(pDestBuffer, bufferOffset, sourceColors[sourceColorsOffset]);
    //                            sourceColorsOffset++;
    //                            bufferOffset += 4;
    //                        }
    //                    }
    //                    else
    //                    {
    //                        throw new NotImplementedException("need to consider the polyColor");
    //#if false
    //                        for (int i = 0; i < count; i++)
    //                        {
    //                            RGBA_Bytes sourceColor = sourceColors[sourceColorsOffset];
    //                            int alpha = (sourceColor.alpha * sourceCovers[sourceCoversOffset] + 255) / 256;
    //                            if (alpha == 0)
    //                            {
    //                                continue;
    //                            }
    //                            else if (alpha == 255)
    //                            {
    //                                pDestBuffer[bufferOffset + ImageBuffer.OrderR] = (byte)sourceColor.red;
    //                                pDestBuffer[bufferOffset + ImageBuffer.OrderG] = (byte)sourceColor.green;
    //                                pDestBuffer[bufferOffset + ImageBuffer.OrderB] = (byte)sourceColor.blue;
    //                                pDestBuffer[bufferOffset + ImageBuffer.OrderA] = (byte)alpha;
    //                            }
    //                            else
    //                            {
    //                                int OneOverAlpha = base_mask - alpha;
    //                                unchecked
    //                                {
    //                                    int r = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderR] * OneOverAlpha + 255) >> 8) + sourceColor.red];
    //                                    int g = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderG] * OneOverAlpha + 255) >> 8) + sourceColor.green];
    //                                    int b = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderB] * OneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                                    int a = pDestBuffer[bufferOffset + ImageBuffer.OrderA];
    //                                    pDestBuffer[bufferOffset + ImageBuffer.OrderR] = (byte)r;
    //                                    pDestBuffer[bufferOffset + ImageBuffer.OrderG] = (byte)g;
    //                                    pDestBuffer[bufferOffset + ImageBuffer.OrderB] = (byte)b;
    //                                    pDestBuffer[bufferOffset + ImageBuffer.OrderA] = (byte)(base_mask - m_Saturate9BitToByte[(OneOverAlpha * (base_mask - a) + 255) >> 8]);
    //                                }
    //                            }
    //                            sourceColorsOffset++;
    //                            bufferOffset += 4;
    //                        }
    //#endif
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                throw new NotImplementedException("need to consider the polyColor");
    //#if false
    //                for (int i = 0; i < count; i++)
    //                {
    //                    RGBA_Bytes sourceColor = sourceColors[sourceColorsOffset];
    //                    int alpha = (sourceColor.alpha * sourceCovers[sourceCoversOffset] + 255) / 256;
    //                    if (alpha == 255)
    //                    {
    //                        pDestBuffer[bufferOffset + ImageBuffer.OrderR] = (byte)sourceColor.red;
    //                        pDestBuffer[bufferOffset + ImageBuffer.OrderG] = (byte)sourceColor.green;
    //                        pDestBuffer[bufferOffset + ImageBuffer.OrderB] = (byte)sourceColor.blue;
    //                        pDestBuffer[bufferOffset + ImageBuffer.OrderA] = (byte)alpha;
    //                    }
    //                    else if (alpha > 0)
    //                    {
    //                        int OneOverAlpha = base_mask - alpha;
    //                        unchecked
    //                        {
    //                            int r = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderR] * OneOverAlpha + 255) >> 8) + sourceColor.red];
    //                            int g = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderG] * OneOverAlpha + 255) >> 8) + sourceColor.green];
    //                            int b = m_Saturate9BitToByte[((pDestBuffer[bufferOffset + ImageBuffer.OrderB] * OneOverAlpha + 255) >> 8) + sourceColor.blue];
    //                            int a = pDestBuffer[bufferOffset + ImageBuffer.OrderA];
    //                            pDestBuffer[bufferOffset + ImageBuffer.OrderR] = (byte)r;
    //                            pDestBuffer[bufferOffset + ImageBuffer.OrderG] = (byte)g;
    //                            pDestBuffer[bufferOffset + ImageBuffer.OrderB] = (byte)b;
    //                            pDestBuffer[bufferOffset + ImageBuffer.OrderA] = (byte)(base_mask - m_Saturate9BitToByte[(OneOverAlpha * (base_mask - a) + 255) >> 8]);
    //                        }
    //                    }
    //                    sourceColorsOffset++;
    //                    sourceCoversOffset++;
    //                    bufferOffset += 4;
    //                }
    //#endif
    //            }
    //        }
    //    }
#endif //DEBUG
}

