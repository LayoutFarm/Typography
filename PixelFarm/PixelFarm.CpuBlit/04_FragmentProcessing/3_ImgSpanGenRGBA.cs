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
#define USE_UNSAFE_CODE

using System;
using img_subpix_const = PixelFarm.CpuBlit.Imaging.ImageFilterLookUpTable.ImgSubPixConst;

namespace PixelFarm.CpuBlit.FragmentProcessing
{
    // it should be easy to write a 90 rotating or mirroring filter too. LBB 2012/01/14
    class ImgSpanGenRGBA_NN_StepXBy1 : ImgSpanGen
    {

        //a span generator generates output color spans => 

        const int BASE_SHITF = 8;
        const int BASE_SCALE = (int)(1 << BASE_SHITF);
        const int BASE_MASK = BASE_SCALE - 1;
        IBitmapSrc _bmpSrc;
        public ImgSpanGenRGBA_NN_StepXBy1()
        {

        }
        public void SetSrcBitmap(IBitmapSrc src)
        {
            if (src.BitDepth != 32)
            {
                throw new NotSupportedException("The source is expected to be 32 bit.");
            }
            _bmpSrc = src;
        }
        public void ReleaseSrcBitmap()
        {
            _bmpSrc = null;
        }
        public sealed override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
            ISpanInterpolator spanInterpolator = Interpolator;
            spanInterpolator.Begin(x + dx, y + dy, len);
            int x_hr;
            int y_hr;
            spanInterpolator.GetCoord(out x_hr, out y_hr);
            int x_lr = x_hr >> img_subpix_const.SHIFT;
            int y_lr = y_hr >> img_subpix_const.SHIFT;

            int bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr);

            unsafe
            {
                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = _bmpSrc.GetBufferPtr())
                {
                    int* pSource = (int*)srcBufferPtr.Ptr + bufferIndex;

                    do
                    {
                        int src_value = *pSource;
                        //separate each component 
                        //TODO: review here, color from source buffer
                        //should be in 'pre-multiplied' format.
                        //so it should be converted to 'straight' color by call something like ..'FromPreMult()' 

                        outputColors[startIndex++] = Drawing.Color.FromArgb(
                            (byte)((src_value >> 24) & 0xff), //a
                            (byte)((src_value >> 16) & 0xff), //r
                            (byte)((src_value >> 8) & 0xff), //g
                            (byte)((src_value) & 0xff));//b

                        pSource++;//move next
                    } while (--len != 0);

                }
            }



            //version 1 , incorrect
            //ISpanInterpolator spanInterpolator = Interpolator;
            //spanInterpolator.Begin(x + dx, y + dy, len);
            //int x_hr;
            //int y_hr;
            //spanInterpolator.GetCoord(out x_hr, out y_hr);
            //int x_lr = x_hr >> img_subpix_const.SHIFT;
            //int y_lr = y_hr >> img_subpix_const.SHIFT;
            //int bufferIndex = srcRW.GetBufferOffsetXY(x_lr, y_lr);
            //byte[] srcBuffer = srcRW.GetBuffer();
            //unsafe
            //{
            //    fixed (byte* pSource = srcBuffer)
            //    {
            //        do
            //        {
            //            outputColors[startIndex++] = *(Drawing.Color*)&(pSource[bufferIndex]);
            //            bufferIndex += 4;
            //        } while (--len != 0);
            //    }
            //}
        }
    }



    class ImgSpanGenRGBA_BilinearClip : ImgSpanGen
    {
        const int BASE_SHIFT = 8;
        const int BASE_SCALE = (int)(1 << BASE_SHIFT);
        const int BASE_MASK = BASE_SCALE - 1;
        IBitmapSrc _imgsrc;
        Drawing.Color _bgcolor;
        int _bytesBetweenPixelInclusive;
        bool _mode0 = false;

        public ImgSpanGenRGBA_BilinearClip(Drawing.Color back_color)
        {
            _bgcolor = back_color;
        }

        public void SetSrcBitmap(IBitmapSrc src)
        {
            _imgsrc = src;
            _bytesBetweenPixelInclusive = _imgsrc.BytesBetweenPixelsInclusive;
        }
        public void ReleaseSrcBitmap()
        {
            _imgsrc = null;
        }
        public override void Prepare()
        {
            base.Prepare();

            ISpanInterpolator spanInterpolator = base.Interpolator;

            _mode0 = (spanInterpolator.GetType() == typeof(SpanInterpolatorLinear)
                && ((SpanInterpolatorLinear)spanInterpolator).Transformer.GetType() == typeof(VertexProcessing.Affine)
                && ((VertexProcessing.Affine)((SpanInterpolatorLinear)spanInterpolator).Transformer).IsIdentity());
        }
        public Drawing.Color BackgroundColor
        {
            get => _bgcolor;
            set => _bgcolor = value;
        }

        public sealed override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
#if DEBUG
            int tmp_len = len;
#endif
            unsafe
            {
                //TODO: review here 

                if (_mode0)
                {


                    using (CpuBlit.Imaging.TempMemPtr.FromBmp(_imgsrc, out int* srcBuffer))
                    {
                        int bufferIndex = _imgsrc.GetBufferOffsetXY32(x, y);
                        //unsafe
                        {
#if true
                            do
                            {
                                //TODO: review here, match component?
                                //ORDER IS IMPORTANT!
                                //TODO : use CO (color order instead)
                                int color = srcBuffer[bufferIndex++];

                                //byte b = (byte)srcBuffer[bufferIndex++];
                                //byte g = (byte)srcBuffer[bufferIndex++];
                                //byte r = (byte)srcBuffer[bufferIndex++];
                                //byte a = (byte)srcBuffer[bufferIndex++];

                                //outputColors[startIndex] = Drawing.Color.FromArgb(a, r, g, b);
                                outputColors[startIndex] = Drawing.Color.FromArgb(
                                    (color >> 24) & 0xff, //a
                                    (color >> 16) & 0xff, //r
                                    (color >> 8) & 0xff, //b
                                    (color) & 0xff //b
                                    );

                                ++startIndex;
                            } while (--len != 0);
#else
                        fixed (byte* pSource = &fg_ptr[bufferIndex])
                        {
                            int* pSourceInt = (int*)pSource;
                            fixed (RGBA_Bytes* pDest = &span[spanIndex])
                            {
                                int* pDestInt = (int*)pDest;
                                do
                                {
                                    *pDestInt++ = *pSourceInt++;
                                } while (--len != 0);
                            }
                        }
#endif
                        }
                    }
                }
                else
                {



                    ISpanInterpolator spanInterpolator = base.Interpolator;
                    using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = _imgsrc.GetBufferPtr())
                    {
                        int* srcBuffer = (int*)srcBufferPtr.Ptr;

                        spanInterpolator.Begin(x + base.dx, y + base.dy, len);
                        int accColor0, accColor1, accColor2, accColor3;
                        int back_r = _bgcolor.red;
                        int back_g = _bgcolor.green;
                        int back_b = _bgcolor.blue;
                        int back_a = _bgcolor.alpha;
                        int maxx = _imgsrc.Width - 1;
                        int maxy = _imgsrc.Height - 1;
                        int color = 0;

                        unchecked
                        {
                            do
                            {
                                int x_hr;
                                int y_hr;
                                spanInterpolator.GetCoord(out x_hr, out y_hr);
                                x_hr -= base.dxInt;
                                y_hr -= base.dyInt;
                                int x_lr = x_hr >> img_subpix_const.SHIFT;
                                int y_lr = y_hr >> img_subpix_const.SHIFT;
                                int weight;
                                if (x_lr >= 0 && y_lr >= 0 &&
                                   x_lr < maxx && y_lr < maxy)
                                {
                                    int bufferIndex = _imgsrc.GetBufferOffsetXY32(x_lr, y_lr);


                                    accColor0 =
                                        accColor1 =
                                            accColor2 =
                                                accColor3 = (int)img_subpix_const.SCALE * (int)img_subpix_const.SCALE / 2;

                                    x_hr &= img_subpix_const.MASK;
                                    y_hr &= img_subpix_const.MASK;

                                    //bufferIndex = _imgsrc.GetBufferOffsetXY32(x_lr, y_lr);

                                    weight = ((img_subpix_const.SCALE - x_hr) *
                                             (img_subpix_const.SCALE - y_hr));
                                    if (weight > BASE_MASK)
                                    {
                                        color = srcBuffer[bufferIndex];

                                        accColor3 += weight * ((color >> 24) & 0xff); //a
                                        accColor0 += weight * ((color >> 16) & 0xff); //r
                                        accColor1 += weight * ((color >> 8) & 0xff); //g
                                        accColor2 += weight * ((color) & 0xff); //b 

                                    }

                                    weight = (x_hr * ((int)img_subpix_const.SCALE - y_hr));
                                    if (weight > BASE_MASK)
                                    {
                                        bufferIndex++;
                                        color = srcBuffer[bufferIndex];
                                        //
                                        accColor3 += weight * ((color >> 24) & 0xff); //a
                                        accColor0 += weight * ((color >> 16) & 0xff); //r
                                        accColor1 += weight * ((color >> 8) & 0xff); //g
                                        accColor2 += weight * ((color) & 0xff); //b 
                                    }

                                    weight = (((int)img_subpix_const.SCALE - x_hr) * y_hr);
                                    if (weight > BASE_MASK)
                                    {
                                        ++y_lr;
                                        //
                                        bufferIndex = _imgsrc.GetBufferOffsetXY32(x_lr, y_lr);
                                        color = srcBuffer[bufferIndex];
                                        //
                                        accColor3 += weight * ((color >> 24) & 0xff); //a
                                        accColor0 += weight * ((color >> 16) & 0xff); //r
                                        accColor1 += weight * ((color >> 8) & 0xff); //g
                                        accColor2 += weight * ((color) & 0xff); //b 
                                    }
                                    weight = (x_hr * y_hr);
                                    if (weight > BASE_MASK)
                                    {
                                        bufferIndex++;
                                        color = srcBuffer[bufferIndex];
                                        //
                                        accColor3 += weight * ((color >> 24) & 0xff); //a
                                        accColor0 += weight * ((color >> 16) & 0xff); //r
                                        accColor1 += weight * ((color >> 8) & 0xff); //g
                                        accColor2 += weight * ((color) & 0xff); //b 
                                    }
                                    accColor0 >>= img_subpix_const.SHIFT * 2;
                                    accColor1 >>= img_subpix_const.SHIFT * 2;
                                    accColor2 >>= img_subpix_const.SHIFT * 2;
                                    accColor3 >>= img_subpix_const.SHIFT * 2;
                                }
                                else
                                {
                                    if (x_lr < -1 || y_lr < -1 ||
                                       x_lr > maxx || y_lr > maxy)
                                    {
                                        accColor0 = back_r;
                                        accColor1 = back_g;
                                        accColor2 = back_b;
                                        accColor3 = back_a;
                                    }
                                    else
                                    {
                                        accColor0 =
                                        accColor1 =
                                        accColor2 =
                                        accColor3 = (int)img_subpix_const.SCALE * (int)img_subpix_const.SCALE / 2;
                                        x_hr &= (int)img_subpix_const.MASK;
                                        y_hr &= (int)img_subpix_const.MASK;
                                        weight = (((int)img_subpix_const.SCALE - x_hr) *
                                                 ((int)img_subpix_const.SCALE - y_hr));
                                        if (weight > BASE_MASK)
                                        {

                                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                            {
                                                BlendInFilterPixel(
                                                    ref accColor0, ref accColor1, ref accColor2, ref accColor3,
                                                    srcBuffer,
                                                    _imgsrc.GetBufferOffsetXY32(x_lr, y_lr),
                                                    weight);
                                            }
                                            else
                                            {
                                                accColor0 += back_r * weight;
                                                accColor1 += back_g * weight;
                                                accColor2 += back_b * weight;
                                                accColor3 += back_a * weight;
                                            }

                                        }

                                        x_lr++;
                                        weight = (x_hr * ((int)img_subpix_const.SCALE - y_hr));
                                        if (weight > BASE_MASK)
                                        {
                                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                            {
                                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref accColor3,
                                                    srcBuffer,
                                                    _imgsrc.GetBufferOffsetXY32(x_lr, y_lr),
                                                    weight);
                                            }
                                            else
                                            {
                                                accColor0 += back_r * weight;
                                                accColor1 += back_g * weight;
                                                accColor2 += back_b * weight;
                                                accColor3 += back_a * weight;
                                            }
                                        }

                                        x_lr--;
                                        y_lr++;
                                        weight = (((int)img_subpix_const.SCALE - x_hr) * y_hr);
                                        if (weight > BASE_MASK)
                                        {
                                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                            {
                                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref accColor3,
                                                    srcBuffer,
                                                    _imgsrc.GetBufferOffsetXY32(x_lr, y_lr),
                                                    weight);
                                            }
                                            else
                                            {
                                                accColor0 += back_r * weight;
                                                accColor1 += back_g * weight;
                                                accColor2 += back_b * weight;
                                                accColor3 += back_a * weight;
                                            }
                                        }

                                        x_lr++;
                                        weight = (x_hr * y_hr);
                                        if (weight > BASE_MASK)
                                        {
                                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                            {
                                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref accColor3,
                                                   srcBuffer,
                                                   _imgsrc.GetBufferOffsetXY32(x_lr, y_lr),
                                                   weight);
                                            }
                                            else
                                            {
                                                accColor0 += back_r * weight;
                                                accColor1 += back_g * weight;
                                                accColor2 += back_b * weight;
                                                accColor3 += back_a * weight;
                                            }
                                        }

                                        accColor0 >>= img_subpix_const.SHIFT * 2;
                                        accColor1 >>= img_subpix_const.SHIFT * 2;
                                        accColor2 >>= img_subpix_const.SHIFT * 2;
                                        accColor3 >>= img_subpix_const.SHIFT * 2;
                                    }
                                }

#if DEBUG
                                if (startIndex >= outputColors.Length)
                                {

                                }
#endif
                                outputColors[startIndex] = PixelFarm.Drawing.Color.FromArgb(
                                    (byte)accColor3,
                                    (byte)accColor0,
                                    (byte)accColor1,
                                    (byte)accColor2
                                    );

                                //outputColors[startIndex].red = (byte)accColor0;
                                //outputColors[startIndex].green = (byte)accColor1;
                                //outputColors[startIndex].blue = (byte)accColor2;
                                //outputColors[startIndex].alpha = (byte)accColor3;
                                ++startIndex;
                                spanInterpolator.Next();
                            } while (--len != 0);
                        }
                    }
                }

            }
        }

        static unsafe void BlendInFilterPixel(
            ref int accColor0, ref int accColor1,
            ref int accColor2, ref int accColor3,
            int* srcBuffer, int bufferIndex, int weight)
        {
            //accColor0 = back_r;
            //accColor1 = back_g;
            //accColor2 = back_b;
            //accColor3 = back_a;
            unchecked
            {
                int color = srcBuffer[bufferIndex];
                //
                accColor0 += weight * (color & 0xff);
                accColor1 += weight * ((color >> 8) & 0xff);
                accColor2 += weight * ((color >> 16) & 0xff);
                accColor3 += weight * ((color >> 24) & 0xff);
            }
        }

    }
}






