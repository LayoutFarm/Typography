//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// MIT, Anti-Grain Geometry - Version 2.4
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
using PixelFarm.CpuBlit.Imaging;

using subpix_const = PixelFarm.CpuBlit.Imaging.ImageFilterLookUpTable.ImgSubPixConst;
using filter_const = PixelFarm.CpuBlit.Imaging.ImageFilterLookUpTable.ImgFilterConst;


namespace PixelFarm.CpuBlit.FragmentProcessing
{
    public static class ISpanInterpolatorExtensions
    {
        public static void TranslateBeginCoord(this ISpanInterpolator interpolator,
            double inX, double inY,
            out int outX, out int outY,
            int shift)
        {
            interpolator.Begin(inX, inY, 1);
            interpolator.GetCoord(out int x_hr, out int y_hr);
            //get translate version 
            outX = x_hr >> shift;
            outY = y_hr >> shift;
        }

        public static void SubPixTranslateBeginCoord(this ISpanInterpolator interpolator,
            double inX, double inY,
            out int outX, out int outY)
        {
            interpolator.Begin(inX, inY, 1);
            interpolator.GetCoord(out int x_hr, out int y_hr);
            //get translate version 
            outX = x_hr >> subpix_const.SHIFT;
            outY = y_hr >> subpix_const.SHIFT;
        }

        public static void SubPixGetTranslatedCoord(this ISpanInterpolator interpolator, out int outX, out int outY)
        {
            interpolator.GetCoord(out int x_hr, out int y_hr);
            outX = x_hr >> subpix_const.SHIFT;
            outY = y_hr >> subpix_const.SHIFT;
        }
    }

    // it should be easy to write a 90 rotating or mirroring filter too. LBB 2012/01/14
    /// <summary>
    /// Nearest Neighbor,StepXBy1
    /// </summary>
    class ImgSpanGenRGBA_NN_StepXBy1 : ImgSpanGen
    {
        //NN: nearest neighbor
        public ImgSpanGenRGBA_NN_StepXBy1()
        {

        }
        public sealed override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
            //ISpanInterpolator spanInterpolator = Interpolator;
            //spanInterpolator.Begin(x + dx, y + dy, len);

            //spanInterpolator.GetCoord(out int x_hr, out int y_hr);
            //int x_lr = x_hr >> subpix_const.SHIFT;
            //int y_lr = y_hr >> subpix_const.SHIFT;

            Interpolator.SubPixTranslateBeginCoord(x + dx, y + dy, out int x_lr, out int y_lr);
            ImgSpanGenRGBA_NN.NN_StepXBy1(_bmpSrc, _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr), outputColors, startIndex, len);
        }
    }


    //==============================================span_image_filter_rgba_nn
    /// <summary>
    /// Nearest Neighbor
    /// </summary>
    public class ImgSpanGenRGBA_NN : ImgSpanGen
    {

        bool _noTransformation = false;
        public override void Prepare()
        {
            base.Prepare();

            _noTransformation = (base.Interpolator is SpanInterpolatorLinear spanInterpolatorLinear &&
               spanInterpolatorLinear.Transformer is VertexProcessing.Affine aff &&
               aff.IsIdentity);
        }
        internal unsafe static void NN_StepXBy1(IBitmapSrc bmpsrc, int srcIndex, Drawing.Color[] outputColors, int dstIndex, int len)
        {
            using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = bmpsrc.GetBufferPtr())
            {
                int* pSource = (int*)srcBufferPtr.Ptr + srcIndex;
                do
                {
                    int srcColor = *pSource;
                    //separate each component 
                    //TODO: review here, color from source buffer
                    //should be in 'pre-multiplied' format.
                    //so it should be converted to 'straight' color by call something like ..'FromPreMult()'  
                    outputColors[dstIndex++] = Drawing.Color.FromArgb(
                          (srcColor >> CO.A_SHIFT) & 0xff, //a
                          (srcColor >> CO.R_SHIFT) & 0xff, //r
                          (srcColor >> CO.G_SHIFT) & 0xff, //g
                          (srcColor >> CO.B_SHIFT) & 0xff);//b 

                    pSource++;//move next

                } while (--len != 0);
            }

        }
        public override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {

            if (_noTransformation)
            {

                //Interpolator.GetCoord(out int x_hr, out int y_hr);
                //int x_lr = x_hr >> subpix_const.SHIFT;
                //int y_lr = y_hr >> subpix_const.SHIFT;

                Interpolator.SubPixTranslateBeginCoord(x + dx, y + dy, out int x_lr, out int y_lr);
                NN_StepXBy1(_bmpSrc, _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr), outputColors, startIndex, len);
            }
            else
            {
                ISpanInterpolator spanInterpolator = Interpolator;
                spanInterpolator.Begin(x + dx, y + dy, len);
                unsafe
                {
                    using (CpuBlit.Imaging.TempMemPtr.FromBmp(_bmpSrc, out int* srcBuffer))
                    {
                        //TODO: if no any transformation,=> skip spanInterpolator (see above example)
                        do
                        {
                            //spanInterpolator.GetCoord(out int x_hr, out int y_hr);
                            //int x_lr = x_hr >> subpix_const.SHIFT;
                            //int y_lr = y_hr >> subpix_const.SHIFT;

                            spanInterpolator.SubPixGetTranslatedCoord(out int x_lr, out int y_lr);

                            int bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr);

                            int srcColor = srcBuffer[bufferIndex++];

                            outputColors[startIndex] = Drawing.Color.FromArgb(
                                  (srcColor >> CO.A_SHIFT) & 0xff, //a
                                  (srcColor >> CO.R_SHIFT) & 0xff, //r
                                  (srcColor >> CO.G_SHIFT) & 0xff, //g
                                  (srcColor >> CO.B_SHIFT) & 0xff);//b 

                            ++startIndex;
                            spanInterpolator.Next();

                        } while (--len != 0);
                    }
                }
            }
        }
    }

    class ImgSpanGenRGBA_BilinearClip : ImgSpanGen
    {

        bool _noTransformation = false;
        public ImgSpanGenRGBA_BilinearClip(Drawing.Color back_color)
        {
            BackgroundColor = back_color;
        }

        public override void Prepare()
        {
            base.Prepare();

            _noTransformation = (base.Interpolator is SpanInterpolatorLinear spanInterpolatorLinear &&
               spanInterpolatorLinear.Transformer is VertexProcessing.Affine aff &&
               aff.IsIdentity);
             
        }
        public sealed override void GenerateColors(Drawing.Color[] outputColors, int startIndex, int x, int y, int len)
        {
#if DEBUG
            int tmp_len = len;
#endif
            unsafe
            {
                //TODO: review here 

                if (_noTransformation)
                {
                    using (CpuBlit.Imaging.TempMemPtr.FromBmp(_bmpSrc, out int* srcBuffer))
                    {
                        int bufferIndex = _bmpSrc.GetBufferOffsetXY32(x, y);
                        do
                        {
                            //TODO: review here, match component?
                            //ORDER IS IMPORTANT!
                            //TODO : use CO (color order instead)
                            int srcColor = srcBuffer[bufferIndex++];
                            outputColors[startIndex] = Drawing.Color.FromArgb(
                              (srcColor >> CO.A_SHIFT) & 0xff, //a
                              (srcColor >> CO.R_SHIFT) & 0xff, //r
                              (srcColor >> CO.G_SHIFT) & 0xff, //g
                              (srcColor >> CO.B_SHIFT) & 0xff);//b 

                            ++startIndex;
                        } while (--len != 0);
                    }
                }
                else
                {
                    //Bilinear interpolation, without lookup table
                    ISpanInterpolator spanInterpolator = base.Interpolator;
                    using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = _bmpSrc.GetBufferPtr())
                    {
                        int* srcBuffer = (int*)srcBufferPtr.Ptr;

                        spanInterpolator.Begin(x + base.dx, y + base.dy, len);

                        //accumulated color component
                        int acc_r, acc_g, acc_b, acc_a;

                        Color bgColor = this.BackgroundColor;
                        int back_r = bgColor.red;
                        int back_g = bgColor.green;
                        int back_b = bgColor.blue;
                        int back_a = bgColor.alpha;
                        int maxx = _bmpSrc.Width - 1;
                        int maxy = _bmpSrc.Height - 1;
                        int srcColor = 0;

                        do
                        {
                            int x_hr;
                            int y_hr;
                            spanInterpolator.GetCoord(out x_hr, out y_hr);
                            x_hr -= base.dxInt;
                            y_hr -= base.dyInt;

                            int x_lr = x_hr >> subpix_const.SHIFT;
                            int y_lr = y_hr >> subpix_const.SHIFT;
                            int weight;

                            if (x_lr >= 0 && y_lr >= 0 &&
                               x_lr < maxx && y_lr < maxy)
                            {
                                int bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr);

                                //accumulated color components
                                acc_r =
                                    acc_g =
                                        acc_b =
                                            acc_a = subpix_const.SCALE * subpix_const.SCALE / 2;

                                x_hr &= subpix_const.MASK;
                                y_hr &= subpix_const.MASK;


                                weight = (subpix_const.SCALE - x_hr) * (subpix_const.SCALE - y_hr);

                                if (weight > BASE_MASK)
                                {
                                    srcColor = srcBuffer[bufferIndex];

                                    acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                    acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                    acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                    acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 

                                }

                                weight = (x_hr * (subpix_const.SCALE - y_hr));

                                if (weight > BASE_MASK)
                                {
                                    bufferIndex++;
                                    srcColor = srcBuffer[bufferIndex];
                                    //
                                    acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                    acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                    acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                    acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                }

                                weight = ((subpix_const.SCALE - x_hr) * y_hr);

                                if (weight > BASE_MASK)
                                {
                                    ++y_lr;
                                    //
                                    bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr);
                                    srcColor = srcBuffer[bufferIndex];
                                    //
                                    acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                    acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                    acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                    acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                }

                                weight = (x_hr * y_hr);

                                if (weight > BASE_MASK)
                                {
                                    bufferIndex++;
                                    srcColor = srcBuffer[bufferIndex];
                                    //
                                    acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                    acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                    acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                    acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                }
                                acc_r >>= subpix_const.SHIFT * 2;
                                acc_g >>= subpix_const.SHIFT * 2;
                                acc_b >>= subpix_const.SHIFT * 2;
                                acc_a >>= subpix_const.SHIFT * 2;
                            }
                            else
                            {
                                if (x_lr < -1 || y_lr < -1 ||
                                   x_lr > maxx || y_lr > maxy)
                                {
                                    acc_r = back_r;
                                    acc_g = back_g;
                                    acc_b = back_b;
                                    acc_a = back_a;
                                }
                                else
                                {

                                    acc_r =
                                       acc_g =
                                          acc_b =
                                            acc_a = subpix_const.SCALE * subpix_const.SCALE / 2;

                                    x_hr &= subpix_const.MASK;
                                    y_hr &= subpix_const.MASK;

                                    weight = (subpix_const.SCALE - x_hr) * (subpix_const.SCALE - y_hr);

                                    if (weight > BASE_MASK)
                                    {

                                        if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                        {
                                            srcColor = srcBuffer[_bmpSrc.GetBufferOffsetXY32(x_lr, y_lr)];
                                            //
                                            acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                            acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                            acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                            acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                        }
                                        else
                                        {
                                            acc_r += back_r * weight;
                                            acc_g += back_g * weight;
                                            acc_b += back_b * weight;
                                            acc_a += back_a * weight;
                                        }

                                    }

                                    x_lr++;
                                    weight = x_hr * (subpix_const.SCALE - y_hr);
                                    if (weight > BASE_MASK)
                                    {
                                        if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                        {

                                            srcColor = srcBuffer[_bmpSrc.GetBufferOffsetXY32(x_lr, y_lr)];
                                            //
                                            acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                            acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                            acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                            acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                        }
                                        else
                                        {
                                            acc_r += back_r * weight;
                                            acc_g += back_g * weight;
                                            acc_b += back_b * weight;
                                            acc_a += back_a * weight;
                                        }
                                    }

                                    x_lr--;
                                    y_lr++;
                                    weight = (subpix_const.SCALE - x_hr) * y_hr;
                                    if (weight > BASE_MASK)
                                    {
                                        if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                        {


                                            srcColor = srcBuffer[_bmpSrc.GetBufferOffsetXY32(x_lr, y_lr)];
                                            //
                                            acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                            acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                            acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                            acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 

                                        }
                                        else
                                        {
                                            acc_r += back_r * weight;
                                            acc_g += back_g * weight;
                                            acc_b += back_b * weight;
                                            acc_a += back_a * weight;
                                        }
                                    }

                                    x_lr++;
                                    weight = (x_hr * y_hr);
                                    if (weight > BASE_MASK)
                                    {
                                        if ((uint)x_lr <= (uint)maxx && (uint)y_lr <= (uint)maxy)
                                        {
                                            srcColor = srcBuffer[_bmpSrc.GetBufferOffsetXY32(x_lr, y_lr)];
                                            //
                                            acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                            acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                            acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                            acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 
                                        }
                                        else
                                        {
                                            acc_r += back_r * weight;
                                            acc_g += back_g * weight;
                                            acc_b += back_b * weight;
                                            acc_a += back_a * weight;
                                        }
                                    }

                                    acc_r >>= subpix_const.SHIFT * 2;
                                    acc_g >>= subpix_const.SHIFT * 2;
                                    acc_b >>= subpix_const.SHIFT * 2;
                                    acc_a >>= subpix_const.SHIFT * 2;
                                }
                            }

#if DEBUG
                            if (startIndex >= outputColors.Length)
                            {

                            }
#endif
                            outputColors[startIndex] = PixelFarm.Drawing.Color.FromArgb(
                                (byte)acc_a,
                                (byte)acc_r,
                                (byte)acc_g,
                                (byte)acc_b
                                );

                            ++startIndex;
                            spanInterpolator.Next();

                        } while (--len != 0);

                    }//using
                }//else
            }//unsafe
        }
    }



    public class ImgSpanGenRGBA_CustomFilter : ImgSpanGen
    {
        //from Agg
        //span_image_filter_rgba
        ImageFilterLookUpTable _lut;
        public ImgSpanGenRGBA_CustomFilter()
        {
        }
        public void SetLookupTable(ImageFilterLookUpTable lut)
        {
            _lut = lut;
        }
        public override void GenerateColors(Color[] outputColors, int startIndex, int x, int y, int len)
        {
            ISpanInterpolator spanInterpolator = this.Interpolator;

            int acc_r, acc_g, acc_b, acc_a;
            int diameter = _lut.Diameter;
            int start = _lut.Start;
            int[] weight_array = _lut.WeightArray;

            int x_count;
            int weight_y;

            unsafe
            {
                using (CpuBlit.Imaging.TempMemPtr srcBufferPtr = _bmpSrc.GetBufferPtr())
                {
                    int* srcBuffer = (int*)srcBufferPtr.Ptr;
                    spanInterpolator.Begin(x + base.dx, y + base.dy, len);

                    do
                    {
                        spanInterpolator.GetCoord(out x, out y);

                        x -= base.dxInt;
                        y -= base.dyInt;

                        int x_hr = x;
                        int y_hr = y;

                        int x_lr = x_hr >> subpix_const.SHIFT;
                        int y_lr = y_hr >> subpix_const.SHIFT;

                        //accumualted color components
                        acc_r =
                           acc_g =
                              acc_b =
                                acc_a = filter_const.SCALE / 2;


                        int x_fract = x_hr & subpix_const.MASK;
                        int y_count = diameter;

                        y_hr = subpix_const.MASK - (y_hr & subpix_const.MASK);
                        int bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, y_lr);

                        int tmp_Y = y_lr;
                        for (; ; )
                        {
                            x_count = diameter;
                            weight_y = weight_array[y_hr];
                            x_hr = subpix_const.MASK - x_fract;

                            //-------------------
                            for (; ; )
                            {
                                int weight = (weight_y * weight_array[x_hr] +
                                              filter_const.SCALE / 2) >>
                                              filter_const.SHIFT;

                                int srcColor = srcBuffer[bufferIndex];

                                acc_a += weight * ((srcColor >> CO.A_SHIFT) & 0xff); //a
                                acc_r += weight * ((srcColor >> CO.R_SHIFT) & 0xff); //r
                                acc_g += weight * ((srcColor >> CO.G_SHIFT) & 0xff); //g
                                acc_b += weight * ((srcColor >> CO.B_SHIFT) & 0xff); //b 

                                if (--x_count == 0) break; //for

                                x_hr += subpix_const.SCALE;
                                bufferIndex++;
                            }
                            //-------------------

                            if (--y_count == 0) break;
                            y_hr += subpix_const.SCALE;

                            tmp_Y++; //move down to next row-> and find start bufferIndex
                            bufferIndex = _bmpSrc.GetBufferOffsetXY32(x_lr, tmp_Y);
                        }

                        acc_r >>= filter_const.SHIFT;
                        acc_g >>= filter_const.SHIFT;
                        acc_b >>= filter_const.SHIFT;
                        acc_a >>= filter_const.SHIFT;

                        unchecked
                        {
                            if ((uint)acc_r > BASE_MASK)
                            {
                                if (acc_r < 0) acc_r = 0;
                                if (acc_r > BASE_MASK) acc_r = BASE_MASK;
                            }

                            if ((uint)acc_g > BASE_MASK)
                            {
                                if (acc_g < 0) acc_g = 0;
                                if (acc_g > BASE_MASK) acc_g = BASE_MASK;
                            }

                            if ((uint)acc_b > BASE_MASK)
                            {
                                if (acc_b < 0) acc_b = 0;
                                if (acc_b > BASE_MASK) acc_b = BASE_MASK;
                            }

                            if ((uint)acc_a > BASE_MASK)
                            {
                                if (acc_a < 0) acc_a = 0;
                                if (acc_a > BASE_MASK) acc_a = BASE_MASK;
                            }
                        }
                        outputColors[startIndex] = PixelFarm.Drawing.Color.FromArgb(
                               (byte)acc_a, //a
                               (byte)acc_r,
                               (byte)acc_g,
                               (byte)acc_b);

                        startIndex++;

                        spanInterpolator.Next();
                    } while (--len != 0);
                }
            }
        }
    }

}