//BSD, 2014-2016, WinterDev
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
using img_subpix_const = PixelFarm.Agg.ImageFilterLookUpTable.ImgSubPixConst;
using img_filter_const = PixelFarm.Agg.ImageFilterLookUpTable.ImgFilterConst;
namespace PixelFarm.Agg.Imaging
{
    // it should be easy to write a 90 rotating or mirroring filter too. LBB 2012/01/14
    class ImgSpanGenRGB_NNStepXby1 : ImgSpanGen
    {
        const int BASE_SHIFT = 8;
        const int BASE_SCALE = (int)(1 << BASE_SHIFT);
        const int BASE_MASK = BASE_SCALE - 1;
        ImageReaderWriterBase srcRW;
        public ImgSpanGenRGB_NNStepXby1(IImageReaderWriter src, ISpanInterpolator spanInterpolator)
            : base(spanInterpolator)
        {
            this.srcRW = (ImageReaderWriterBase)src;
            if (srcRW.BitDepth != 24)
            {
                throw new NotSupportedException("The source is expected to be 32 bit.");
            }
        }
        public override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
            ISpanInterpolator spanInterpolator = Interpolator;
            spanInterpolator.Begin(x + dx, y + dy, len);
            int x_hr;
            int y_hr;
            spanInterpolator.GetCoord(out x_hr, out y_hr);
            int x_lr = x_hr >> img_subpix_const.SHIFT;
            int y_lr = y_hr >> img_subpix_const.SHIFT;
            int bufferIndex = srcRW.GetBufferOffsetXY(x_lr, y_lr);
            byte[] srcBuff = srcRW.GetBuffer();
            Drawing.Color color = Drawing.Color.White;
            do
            {
                color.blue = srcBuff[bufferIndex++];
                color.green = srcBuff[bufferIndex++];
                color.red = srcBuff[bufferIndex++];
                outputColors[startIndex++] = color;
            } while (--len != 0);
        }
    }




    //=====================================span_image_filter_rgb_bilinear_clip
    class ImgSpanGenRGB_BilinearClip : ImgSpanGen
    {
        Drawing.Color m_bgcolor;
        const int BASE_SHIFT = 8;
        const int BASE_SCALE = (int)(1 << BASE_SHIFT);
        const int BASE_MASK = BASE_SCALE - 1;
        ImageReaderWriterBase srcRW;
        //--------------------------------------------------------------------
        public ImgSpanGenRGB_BilinearClip(IImageReaderWriter src,
                                          Drawing.Color back_color,
                                          ISpanInterpolator inter)
            : base(inter)
        {
            m_bgcolor = back_color;
            srcRW = (ImageReaderWriterBase)src;
        }

        public Drawing.Color BackgroundColor
        {
            get { return this.m_bgcolor; }
            set { this.m_bgcolor = value; }
        }
        public override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
            ISpanInterpolator spanInterpolator = base.Interpolator;
            spanInterpolator.Begin(x + base.dx, y + base.dy, len);
            int accColor0, accColor1, accColor2;
            int sourceAlpha;
            int back_r = m_bgcolor.red;
            int back_g = m_bgcolor.green;
            int back_b = m_bgcolor.blue;
            int back_a = m_bgcolor.alpha;
            int bufferIndex;
            int maxx = (int)srcRW.Width - 1;
            int maxy = (int)srcRW.Height - 1;
            byte[] srcBuffer = srcRW.GetBuffer();
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
                        accColor0 =
                        accColor1 =
                        accColor2 = img_subpix_const.SCALE * img_subpix_const.SCALE / 2;
                        x_hr &= img_subpix_const.MASK;
                        y_hr &= img_subpix_const.MASK;
                        bufferIndex = srcRW.GetBufferOffsetXY(x_lr, y_lr);
                        weight = ((img_subpix_const.SCALE - x_hr) *
                                 (img_subpix_const.SCALE - y_hr));
                        accColor0 += weight * srcBuffer[bufferIndex + CO.R];
                        accColor1 += weight * srcBuffer[bufferIndex + CO.G];
                        accColor2 += weight * srcBuffer[bufferIndex + CO.B];
                        bufferIndex += 3;
                        weight = (x_hr * (img_subpix_const.SCALE - y_hr));
                        accColor0 += weight * srcBuffer[bufferIndex + CO.R];
                        accColor1 += weight * srcBuffer[bufferIndex + CO.G];
                        accColor2 += weight * srcBuffer[bufferIndex + CO.B];
                        y_lr++;
                        bufferIndex = srcRW.GetBufferOffsetXY(x_lr, y_lr);
                        weight = ((img_subpix_const.SCALE - x_hr) * y_hr);
                        accColor0 += weight * srcBuffer[bufferIndex + CO.R];
                        accColor1 += weight * srcBuffer[bufferIndex + CO.G];
                        accColor2 += weight * srcBuffer[bufferIndex + CO.B];
                        bufferIndex += 3;
                        weight = (x_hr * y_hr);
                        accColor0 += weight * srcBuffer[bufferIndex + CO.R];
                        accColor1 += weight * srcBuffer[bufferIndex + CO.G];
                        accColor2 += weight * srcBuffer[bufferIndex + CO.B];
                        accColor0 >>= img_subpix_const.SHIFT * 2;
                        accColor1 >>= img_subpix_const.SHIFT * 2;
                        accColor2 >>= img_subpix_const.SHIFT * 2;
                        sourceAlpha = BASE_MASK;
                    }
                    else
                    {
                        if (x_lr < -1 || y_lr < -1 ||
                           x_lr > maxx || y_lr > maxy)
                        {
                            accColor0 = back_r;
                            accColor1 = back_g;
                            accColor2 = back_b;
                            sourceAlpha = back_a;
                        }
                        else
                        {
                            accColor0 =
                            accColor1 =
                            accColor2 = img_subpix_const.SCALE * img_subpix_const.SCALE / 2;
                            sourceAlpha = img_subpix_const.SCALE * img_subpix_const.SCALE / 2;
                            x_hr &= img_subpix_const.MASK;
                            y_hr &= img_subpix_const.MASK;
                            weight = ((img_subpix_const.SCALE - x_hr) * (img_subpix_const.SCALE - y_hr));
                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                            {
                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref sourceAlpha,
                                   srcRW.GetBuffer(),
                                   srcRW.GetBufferOffsetXY(x_lr, y_lr),
                                   weight);
                            }
                            else
                            {
                                accColor0 += back_r * weight;
                                accColor1 += back_g * weight;
                                accColor2 += back_b * weight;
                                sourceAlpha += back_a * weight;
                            }
                            x_lr++;
                            weight = (x_hr * (img_subpix_const.SCALE - y_hr));
                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                            {
                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref sourceAlpha,
                                   srcRW.GetBuffer(),
                                   srcRW.GetBufferOffsetXY(x_lr, y_lr),
                                   weight);
                            }
                            else
                            {
                                accColor0 += back_r * weight;
                                accColor1 += back_g * weight;
                                accColor2 += back_b * weight;
                                sourceAlpha += back_a * weight;
                            }

                            x_lr--;
                            y_lr++;
                            weight = ((img_subpix_const.SCALE - x_hr) * y_hr);
                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                            {
                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref sourceAlpha,
                                    srcRW.GetBuffer(),
                                    srcRW.GetBufferOffsetXY(x_lr, y_lr),
                                    weight);
                            }
                            else
                            {
                                accColor0 += back_r * weight;
                                accColor1 += back_g * weight;
                                accColor2 += back_b * weight;
                                sourceAlpha += back_a * weight;
                            }

                            x_lr++;
                            weight = (x_hr * y_hr);
                            if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                            {
                                BlendInFilterPixel(ref accColor0, ref accColor1, ref accColor2, ref sourceAlpha,
                                 srcRW.GetBuffer(),
                                 srcRW.GetBufferOffsetXY(x_lr, y_lr),
                                 weight);
                            }
                            else
                            {
                                accColor0 += back_r * weight;
                                accColor1 += back_g * weight;
                                accColor2 += back_b * weight;
                                sourceAlpha += back_a * weight;
                            }
                            accColor0 >>= img_subpix_const.SHIFT * 2;
                            accColor1 >>= img_subpix_const.SHIFT * 2;
                            accColor2 >>= img_subpix_const.SHIFT * 2;
                            sourceAlpha >>= img_subpix_const.SHIFT * 2;
                        }
                    }

                    outputColors[startIndex].red = (byte)accColor0;
                    outputColors[startIndex].green = (byte)accColor1;
                    outputColors[startIndex].blue = (byte)accColor2;
                    outputColors[startIndex].alpha = (byte)sourceAlpha;
                    startIndex++;
                    spanInterpolator.Next();
                } while (--len != 0);
            }
        }

        static void BlendInFilterPixel(ref int accColor0, ref int accColor1, ref int accColor2, ref int sourceAlpha,
             byte[] srcBuffer, int bufferIndex, int weight)
        {
            unchecked
            {
                accColor0 += weight * srcBuffer[bufferIndex + CO.R];
                accColor1 += weight * srcBuffer[bufferIndex + CO.G];
                accColor2 += weight * srcBuffer[bufferIndex + CO.B];
                sourceAlpha += weight * BASE_MASK;
            }
        }
    }
}
