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
////          mcseemagg@yahoo.com
////          http://www.antigrain.com
////----------------------------------------------------------------------------
////
//// Adaptation for high precision colors has been sponsored by 
//// Liberty Technology Systems, Inc., visit http://lib-sys.com
////
//// Liberty Technology Systems, Inc. is the provider of
//// PostScript and PDF technology for software developers.
//// 
////----------------------------------------------------------------------------
//#define USE_BLENDER
////using ColorT = MatterHackers.Agg.order_bgra;

//using System;

//using MatterHackers.Agg;
////using Mono.Simd;

//namespace MatterHackers.Agg.Image
//{
//public class BlenderBaseBGRAFloat
//{
//    public int NumPixelBits { get { return 128; } }

//    public const byte BASE_MASK = 255;
//}
//public sealed class BlenderBGRAFloat : BlenderBaseBGRAFloat, IRecieveBlenderFloat
//{
//    public ColorRGBAf PixelToColorRGBA_Floats(float[] buffer, int bufferOffset)
//    {
//        return new ColorRGBAf(buffer[bufferOffset + ImageBase.OrderR], buffer[bufferOffset + ImageBase.OrderG], buffer[bufferOffset + ImageBase.OrderB], buffer[bufferOffset + ImageBase.OrderA]);
//    }

//    public void CopyPixels(float[] buffer, int bufferOffset, ColorRGBAf sourceColor, int count)
//    {
//        do
//        {
//            buffer[bufferOffset + ImageBase.OrderR] = sourceColor.red;
//            buffer[bufferOffset + ImageBase.OrderG] = sourceColor.green;
//            buffer[bufferOffset + ImageBase.OrderB] = sourceColor.blue;
//            buffer[bufferOffset + ImageBase.OrderA] = sourceColor.alpha;
//            bufferOffset += 4;
//        }
//        while (--count != 0);
//    }

//    public void BlendPixel(float[] buffer, int bufferOffset, ColorRGBAf sourceColor)
//    {
//        if (sourceColor.alpha == 1)
//        {
//            buffer[bufferOffset + ImageBase.OrderR] = (byte)(sourceColor.red);
//            buffer[bufferOffset + ImageBase.OrderG] = (byte)(sourceColor.green);
//            buffer[bufferOffset + ImageBase.OrderB] = (byte)(sourceColor.blue);
//            buffer[bufferOffset + ImageBase.OrderA] = (byte)(sourceColor.alpha);
//        }
//        else
//        {
//            float r = buffer[bufferOffset + ImageBase.OrderR];
//            float g = buffer[bufferOffset + ImageBase.OrderG];
//            float b = buffer[bufferOffset + ImageBase.OrderB];
//            float a = buffer[bufferOffset + ImageBase.OrderA];
//            buffer[bufferOffset + ImageBase.OrderR] = (sourceColor.red - r) * sourceColor.alpha + r;
//            buffer[bufferOffset + ImageBase.OrderG] = (sourceColor.green - g) * sourceColor.alpha + g;
//            buffer[bufferOffset + ImageBase.OrderB] = (sourceColor.blue - b) * sourceColor.alpha + b;
//            buffer[bufferOffset + ImageBase.OrderA] = (sourceColor.alpha + a) - sourceColor.alpha * a;
//        }
//    }

//    public void BlendPixels(float[] destBuffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset,
//        byte[] covers, int coversIndex, bool firstCoverForAll, int count)
//    {
//        throw new NotImplementedException();
//    }
//};

/*
    //=========================================================multiplier_rgba
    template<class ColorT, class Order> struct multiplier_rgba
    {
        typedef typename ColorT::value_type value_type;
        typedef typename ColorT::calc_type calc_type;

        //--------------------------------------------------------------------
        static void premultiply(value_type* p)
        {
            calc_type a = p[Order::A];
            if(a < ColorT::base_mask)
            {
                if(a == 0)
                {
                    p[Order::R] = p[Order::G] = p[Order::B] = 0;
                    return;
                }
                p[Order::R] = value_type((p[Order::R] * a + ColorT::base_mask) >> ColorT::base_shift);
                p[Order::G] = value_type((p[Order::G] * a + ColorT::base_mask) >> ColorT::base_shift);
                p[Order::B] = value_type((p[Order::B] * a + ColorT::base_mask) >> ColorT::base_shift);
            }
        }


        //--------------------------------------------------------------------
        static void demultiply(value_type* p)
        {
            calc_type a = p[Order::A];
            if(a < ColorT::base_mask)
            {
                if(a == 0)
                {
                    p[Order::R] = p[Order::G] = p[Order::B] = 0;
                    return;
                }
                calc_type r = (calc_type(p[Order::R]) * ColorT::base_mask) / a;
                calc_type g = (calc_type(p[Order::G]) * ColorT::base_mask) / a;
                calc_type b = (calc_type(p[Order::B]) * ColorT::base_mask) / a;
                p[Order::R] = value_type((r > ColorT::base_mask) ? ColorT::base_mask : r);
                p[Order::G] = value_type((g > ColorT::base_mask) ? ColorT::base_mask : g);
                p[Order::B] = value_type((b > ColorT::base_mask) ? ColorT::base_mask : b);
            }
        }
    };

    //=====================================================apply_gamma_dir_rgba
    template<class ColorT, class Order, class GammaLut> class apply_gamma_dir_rgba
    {
    public:
        typedef typename ColorT::value_type value_type;

        apply_gamma_dir_rgba(const GammaLut& gamma) : m_gamma(gamma) {}

        void operator () (value_type* p)
        {
            p[Order::R] = m_gamma.dir(p[Order::R]);
            p[Order::G] = m_gamma.dir(p[Order::G]);
            p[Order::B] = m_gamma.dir(p[Order::B]);
        }

    private:
        const GammaLut& m_gamma;
    };

    //=====================================================apply_gamma_inv_rgba
    template<class ColorT, class Order, class GammaLut> class apply_gamma_inv_rgba
    {
    public:
        typedef typename ColorT::value_type value_type;

        apply_gamma_inv_rgba(const GammaLut& gamma) : m_gamma(gamma) {}

        void operator () (value_type* p)
        {
            p[Order::R] = m_gamma.inv(p[Order::R]);
            p[Order::G] = m_gamma.inv(p[Order::G]);
            p[Order::B] = m_gamma.inv(p[Order::B]);
        }

    private:
        const GammaLut& m_gamma;
    };
    

    //=============================================================blender_rgba
    template<class ColorT, class Order> struct blender_rgba
    {
        typedef ColorT color_type;
        typedef Order order_type;
        typedef typename color_type::value_type value_type;
        typedef typename color_type::calc_type calc_type;
        enum base_scale_e 
        { 
            base_shift = color_type::base_shift,
            base_mask  = color_type::base_mask
        };

        //--------------------------------------------------------------------
        static void blend_pix(value_type* p, 
                                         int cr, int cg, int cb,
                                         int alpha, 
                                         int cover=0)
        {
            calc_type r = p[Order::R];
            calc_type g = p[Order::G];
            calc_type b = p[Order::B];
            calc_type a = p[Order::A];
            p[Order::R] = (value_type)(((cr - r) * alpha + (r << base_shift)) >> base_shift);
            p[Order::G] = (value_type)(((cg - g) * alpha + (g << base_shift)) >> base_shift);
            p[Order::B] = (value_type)(((cb - b) * alpha + (b << base_shift)) >> base_shift);
            p[Order::A] = (value_type)((alpha + a) - ((alpha * a + base_mask) >> base_shift));
        }
    };
 */


//public interface IRecieveBlenderFloat
//{
//    int NumPixelBits { get; }

//    ColorRGBAf PixelToColorRGBA_Floats(float[] buffer, int bufferOffset);
//    void CopyPixels(float[] buffer, int bufferOffset, ColorRGBAf sourceColor, int count);

//    void BlendPixel(float[] buffer, int bufferOffset, ColorRGBAf sourceColor);
//    void BlendPixels(float[] buffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset,
//        byte[] sourceCovers, int sourceCoversOffset, bool firstCoverForAll, int count);
//}
//public sealed class BlenderPreMultBGRAFloat : BlenderBaseBGRAFloat, IRecieveBlenderFloat
//{
//    public ColorRGBAf PixelToColorRGBA_Floats(float[] buffer, int bufferOffset)
//    {
//        throw new NotImplementedException();
//        //return new RGBA_(buffer[bufferOffset + ImageBuffer.OrderR], buffer[bufferOffset + ImageBuffer.OrderG], buffer[bufferOffset + ImageBuffer.OrderB], buffer[bufferOffset + ImageBuffer.OrderA]);
//    }

//    public void SetPixels(float[] buffer, int bufferOffset, ColorRGBAf sourceColor, int count)
//    {
//        do
//        {
//            buffer[bufferOffset + ImageBase.OrderR] = sourceColor.red;
//            buffer[bufferOffset + ImageBase.OrderG] = sourceColor.green;
//            buffer[bufferOffset + ImageBase.OrderB] = sourceColor.blue;
//            buffer[bufferOffset + ImageBase.OrderA] = sourceColor.alpha;
//            bufferOffset += 4;
//        }
//        while (--count != 0);
//    }

//    public void CopyPixels(float[] buffer, int bufferOffset, ColorRGBAf[] sourceColors, int sourceColorsOffset, int count)
//    {
//        throw new NotImplementedException();
//    }

//    public void CopyPixels(float[] buffer, int bufferOffset, ColorRGBAf sourceColor, int count)
//    {
//        do
//        {
//            buffer[bufferOffset + ImageBase.OrderR] = sourceColor.red;
//            buffer[bufferOffset + ImageBase.OrderG] = sourceColor.green;
//            buffer[bufferOffset + ImageBase.OrderB] = sourceColor.blue;
//            buffer[bufferOffset + ImageBase.OrderA] = sourceColor.alpha;
//            bufferOffset += 4;
//        }
//        while (--count != 0);
//    }

//    public void BlendPixel(float[] buffer, int bufferOffset, ColorRGBAf sourceColor)
//    {
//        if (sourceColor.alpha == 1)
//        {
//            buffer[bufferOffset + ImageBase.OrderR] = (byte)(sourceColor.red);
//            buffer[bufferOffset + ImageBase.OrderG] = (byte)(sourceColor.green);
//            buffer[bufferOffset + ImageBase.OrderB] = (byte)(sourceColor.blue);
//            buffer[bufferOffset + ImageBase.OrderA] = (byte)(sourceColor.alpha);
//        }
//        else
//        {
//            float r = buffer[bufferOffset + ImageBase.OrderR];
//            float g = buffer[bufferOffset + ImageBase.OrderG];
//            float b = buffer[bufferOffset + ImageBase.OrderB];
//            float a = buffer[bufferOffset + ImageBase.OrderA];
//            buffer[bufferOffset + ImageBase.OrderR] = (sourceColor.red - r) * sourceColor.alpha + r;
//            buffer[bufferOffset + ImageBase.OrderG] = (sourceColor.green - g) * sourceColor.alpha + g;
//            buffer[bufferOffset + ImageBase.OrderB] = (sourceColor.blue - b) * sourceColor.alpha + b;
//            buffer[bufferOffset + ImageBase.OrderA] = (sourceColor.alpha + a) - sourceColor.alpha * a;
//        }
//    }

//    public void BlendPixels(float[] pDestBuffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset, int count)
//    {
//    }

//    public void BlendPixels(float[] pDestBuffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset,
//        byte sourceCovers, int count)
//    {
//    }

//    public void BlendPixels(float[] pDestBuffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset,
//        byte[] sourceCovers, int sourceCoversOffset, int count)
//    {
//    }

//    public void BlendPixels(float[] pDestBuffer, int bufferOffset,
//        ColorRGBAf[] sourceColors, int sourceColorsOffset,
//        byte[] sourceCovers, int sourceCoversOffset, bool firstCoverForAll, int count)
//    {
//        if (firstCoverForAll)
//        {
//            //unsafe
//            {
//                if (sourceCovers[sourceCoversOffset] == 255)
//                {
//                    for (int i = 0; i < count; i++)
//                    {
//                        BlendPixel(pDestBuffer, bufferOffset, sourceColors[sourceColorsOffset]);
//                        sourceColorsOffset++;
//                        bufferOffset += 4;
//                    }
//                }
//                else
//                {
//                    for (int i = 0; i < count; i++)
//                    {
//                        ColorRGBAf sourceColor = sourceColors[sourceColorsOffset];
//                        float alpha = (sourceColor.alpha * sourceCovers[sourceCoversOffset] + 255) / 256;
//                        if (alpha == 0)
//                        {
//                            continue;
//                        }
//                        else if (alpha == 255)
//                        {
//                            pDestBuffer[bufferOffset + ImageBase.OrderR] = (byte)sourceColor.red;
//                            pDestBuffer[bufferOffset + ImageBase.OrderG] = (byte)sourceColor.green;
//                            pDestBuffer[bufferOffset + ImageBase.OrderB] = (byte)sourceColor.blue;
//                            pDestBuffer[bufferOffset + ImageBase.OrderA] = (byte)alpha;
//                        }
//                        else
//                        {
//                            float OneOverAlpha = BASE_MASK - alpha;
//                            unchecked
//                            {
//                                float r = pDestBuffer[bufferOffset + ImageBase.OrderR] * OneOverAlpha + sourceColor.red;
//                                float g = pDestBuffer[bufferOffset + ImageBase.OrderG] * OneOverAlpha + sourceColor.green;
//                                float b = pDestBuffer[bufferOffset + ImageBase.OrderB] * OneOverAlpha + sourceColor.blue;
//                                float a = pDestBuffer[bufferOffset + ImageBase.OrderA];
//                                pDestBuffer[bufferOffset + ImageBase.OrderR] = r;
//                                pDestBuffer[bufferOffset + ImageBase.OrderG] = g;
//                                pDestBuffer[bufferOffset + ImageBase.OrderB] = b;
//                                pDestBuffer[bufferOffset + ImageBase.OrderA] = (1.0f - ((OneOverAlpha * (1.0f - a))));
//                            }
//                        }
//                        sourceColorsOffset++;
//                        bufferOffset += 4;
//                    }
//                }
//            }
//        }
//        else
//        {
//            for (int i = 0; i < count; i++)
//            {
//                ColorRGBAf sourceColor = sourceColors[sourceColorsOffset];
//                if (sourceColor.alpha == 1 && sourceCovers[sourceCoversOffset] == 255)
//                {
//                    pDestBuffer[bufferOffset + ImageBase.OrderR] = sourceColor.red;
//                    pDestBuffer[bufferOffset + ImageBase.OrderG] = sourceColor.green;
//                    pDestBuffer[bufferOffset + ImageBase.OrderB] = sourceColor.blue;
//                    pDestBuffer[bufferOffset + ImageBase.OrderA] = 1;
//                }
//                else
//                {
//                    // the cover is known to be less than opaque
//                    float coverFloat = (sourceCovers[sourceCoversOffset] * (1.0f / 255.0f));
//                    float alpha = sourceColor.alpha * coverFloat;
//                    if (coverFloat > 0 && alpha > 0)
//                    {
//                        float OneOverAlpha = 1.0f - alpha;
//                        unchecked
//                        {
//                            // the color is already pre multiplied by the alpha but not by the cover value so we only need to multiply th ecolor by the cover
//                            float r = (pDestBuffer[bufferOffset + ImageBase.OrderR] * OneOverAlpha) + sourceColor.red * coverFloat;
//                            float g = (pDestBuffer[bufferOffset + ImageBase.OrderG] * OneOverAlpha) + sourceColor.green * coverFloat;
//                            float b = (pDestBuffer[bufferOffset + ImageBase.OrderB] * OneOverAlpha) + sourceColor.blue * coverFloat;

//                            float destAlpha = pDestBuffer[bufferOffset + ImageBase.OrderA];
//                            float a = (destAlpha + (1.0f - destAlpha) * sourceColor.alpha * coverFloat);
//                            pDestBuffer[bufferOffset + ImageBase.OrderR] = r;
//                            pDestBuffer[bufferOffset + ImageBase.OrderG] = g;
//                            pDestBuffer[bufferOffset + ImageBase.OrderB] = b;
//                            pDestBuffer[bufferOffset + ImageBase.OrderA] = a;
//                        }
//                    }
//                }
//                sourceColorsOffset++;
//                sourceCoversOffset++;
//                bufferOffset += 4;
//            }
//        }
//    }
//};

/*
//======================================================blender_rgba_plain
template<class ColorT, class Order> struct blender_rgba_plain
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e { base_shift = color_type::base_shift };

//--------------------------------------------------------------------
static void blend_pix(value_type* p, 
                                 int cr, int cg, int cb,
                                 int alpha,
                                 int cover=0)
{
    if(alpha == 0) return;
    calc_type a = p[Order::A];
    calc_type r = p[Order::R] * a;
    calc_type g = p[Order::G] * a;
    calc_type b = p[Order::B] * a;
    a = ((alpha + a) << base_shift) - alpha * a;
    p[Order::A] = (value_type)(a >> base_shift);
    p[Order::R] = (value_type)((((cr << base_shift) - r) * alpha + (r << base_shift)) / a);
    p[Order::G] = (value_type)((((cg << base_shift) - g) * alpha + (g << base_shift)) / a);
    p[Order::B] = (value_type)((((cb << base_shift) - b) * alpha + (b << base_shift)) / a);
}
};











//=========================================================comp_op_rgba_clear
template<class ColorT, class Order> struct comp_op_rgba_clear
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

static void blend_pix(value_type* p, 
                                 int, int, int, int,
                                 int cover)
{
    if(cover < 255)
    {
        cover = 255 - cover;
        p[Order::R] = (value_type)((p[Order::R] * cover + 255) >> 8);
        p[Order::G] = (value_type)((p[Order::G] * cover + 255) >> 8);
        p[Order::B] = (value_type)((p[Order::B] * cover + 255) >> 8);
        p[Order::A] = (value_type)((p[Order::A] * cover + 255) >> 8);
    }
    else
    {
        p[0] = p[1] = p[2] = p[3] = 0; 
    }
}
};

//===========================================================comp_op_rgba_src
template<class ColorT, class Order> struct comp_op_rgba_src
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;

static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        int alpha = 255 - cover;
        p[Order::R] = (value_type)(((p[Order::R] * alpha + 255) >> 8) + ((sr * cover + 255) >> 8));
        p[Order::G] = (value_type)(((p[Order::G] * alpha + 255) >> 8) + ((sg * cover + 255) >> 8));
        p[Order::B] = (value_type)(((p[Order::B] * alpha + 255) >> 8) + ((sb * cover + 255) >> 8));
        p[Order::A] = (value_type)(((p[Order::A] * alpha + 255) >> 8) + ((sa * cover + 255) >> 8));
    }
    else
    {
        p[Order::R] = sr;
        p[Order::G] = sg;
        p[Order::B] = sb;
        p[Order::A] = sa;
    }
}
};

//===========================================================comp_op_rgba_dst
template<class ColorT, class Order> struct comp_op_rgba_dst
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;

static void blend_pix(value_type*, 
                                 int, int, int, 
                                 int, int)
{
}
};

//======================================================comp_op_rgba_src_over
template<class ColorT, class Order> struct comp_op_rgba_src_over
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

//   Dca' = Sca + Dca.(1 - Sa)
//   Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    calc_type s1a = base_mask - sa;
    p[Order::R] = (value_type)(sr + ((p[Order::R] * s1a + base_mask) >> base_shift));
    p[Order::G] = (value_type)(sg + ((p[Order::G] * s1a + base_mask) >> base_shift));
    p[Order::B] = (value_type)(sb + ((p[Order::B] * s1a + base_mask) >> base_shift));
    p[Order::A] = (value_type)(sa + p[Order::A] - ((sa * p[Order::A] + base_mask) >> base_shift));
}
};

//======================================================comp_op_rgba_dst_over
template<class ColorT, class Order> struct comp_op_rgba_dst_over
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Dca + Sca.(1 - Da)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    calc_type d1a = base_mask - p[Order::A];
    p[Order::R] = (value_type)(p[Order::R] + ((sr * d1a + base_mask) >> base_shift));
    p[Order::G] = (value_type)(p[Order::G] + ((sg * d1a + base_mask) >> base_shift));
    p[Order::B] = (value_type)(p[Order::B] + ((sb * d1a + base_mask) >> base_shift));
    p[Order::A] = (value_type)(sa + p[Order::A] - ((sa * p[Order::A] + base_mask) >> base_shift));
}
};

//======================================================comp_op_rgba_src_in
template<class ColorT, class Order> struct comp_op_rgba_src_in
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca.Da
// Da'  = Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    calc_type da = p[Order::A];
    if(cover < 255)
    {
        int alpha = 255 - cover;
        p[Order::R] = (value_type)(((p[Order::R] * alpha + 255) >> 8) + ((((sr * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::G] = (value_type)(((p[Order::G] * alpha + 255) >> 8) + ((((sg * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::B] = (value_type)(((p[Order::B] * alpha + 255) >> 8) + ((((sb * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::A] = (value_type)(((p[Order::A] * alpha + 255) >> 8) + ((((sa * da + base_mask) >> base_shift) * cover + 255) >> 8));
    }
    else
    {
        p[Order::R] = (value_type)((sr * da + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sg * da + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sb * da + base_mask) >> base_shift);
        p[Order::A] = (value_type)((sa * da + base_mask) >> base_shift);
    }
}
};

//======================================================comp_op_rgba_dst_in
template<class ColorT, class Order> struct comp_op_rgba_dst_in
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Dca.Sa
// Da'  = Sa.Da 
static void blend_pix(value_type* p, 
                                 int, int, int, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sa = base_mask - ((cover * (base_mask - sa) + 255) >> 8);
    }
    p[Order::R] = (value_type)((p[Order::R] * sa + base_mask) >> base_shift);
    p[Order::G] = (value_type)((p[Order::G] * sa + base_mask) >> base_shift);
    p[Order::B] = (value_type)((p[Order::B] * sa + base_mask) >> base_shift);
    p[Order::A] = (value_type)((p[Order::A] * sa + base_mask) >> base_shift);
}
};

//======================================================comp_op_rgba_src_out
template<class ColorT, class Order> struct comp_op_rgba_src_out
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca.(1 - Da)
// Da'  = Sa.(1 - Da) 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    calc_type da = base_mask - p[Order::A];
    if(cover < 255)
    {
        int alpha = 255 - cover;
        p[Order::R] = (value_type)(((p[Order::R] * alpha + 255) >> 8) + ((((sr * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::G] = (value_type)(((p[Order::G] * alpha + 255) >> 8) + ((((sg * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::B] = (value_type)(((p[Order::B] * alpha + 255) >> 8) + ((((sb * da + base_mask) >> base_shift) * cover + 255) >> 8));
        p[Order::A] = (value_type)(((p[Order::A] * alpha + 255) >> 8) + ((((sa * da + base_mask) >> base_shift) * cover + 255) >> 8));
    }
    else
    {
        p[Order::R] = (value_type)((sr * da + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sg * da + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sb * da + base_mask) >> base_shift);
        p[Order::A] = (value_type)((sa * da + base_mask) >> base_shift);
    }
}
};

//======================================================comp_op_rgba_dst_out
template<class ColorT, class Order> struct comp_op_rgba_dst_out
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Dca.(1 - Sa) 
// Da'  = Da.(1 - Sa) 
static void blend_pix(value_type* p, 
                                 int, int, int, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sa = (sa * cover + 255) >> 8;
    }
    sa = base_mask - sa;
    p[Order::R] = (value_type)((p[Order::R] * sa + base_shift) >> base_shift);
    p[Order::G] = (value_type)((p[Order::G] * sa + base_shift) >> base_shift);
    p[Order::B] = (value_type)((p[Order::B] * sa + base_shift) >> base_shift);
    p[Order::A] = (value_type)((p[Order::A] * sa + base_shift) >> base_shift);
}
};

//=====================================================comp_op_rgba_src_atop
template<class ColorT, class Order> struct comp_op_rgba_src_atop
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca.Da + Dca.(1 - Sa)
// Da'  = Da
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    calc_type da = p[Order::A];
    sa = base_mask - sa;
    p[Order::R] = (value_type)((sr * da + p[Order::R] * sa + base_mask) >> base_shift);
    p[Order::G] = (value_type)((sg * da + p[Order::G] * sa + base_mask) >> base_shift);
    p[Order::B] = (value_type)((sb * da + p[Order::B] * sa + base_mask) >> base_shift);
}
};

//=====================================================comp_op_rgba_dst_atop
template<class ColorT, class Order> struct comp_op_rgba_dst_atop
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Dca.Sa + Sca.(1 - Da)
// Da'  = Sa 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    calc_type da = base_mask - p[Order::A];
    if(cover < 255)
    {
        int alpha = 255 - cover;
        sr = (p[Order::R] * sa + sr * da + base_mask) >> base_shift;
        sg = (p[Order::G] * sa + sg * da + base_mask) >> base_shift;
        sb = (p[Order::B] * sa + sb * da + base_mask) >> base_shift;
        p[Order::R] = (value_type)(((p[Order::R] * alpha + 255) >> 8) + ((sr * cover + 255) >> 8));
        p[Order::G] = (value_type)(((p[Order::G] * alpha + 255) >> 8) + ((sg * cover + 255) >> 8));
        p[Order::B] = (value_type)(((p[Order::B] * alpha + 255) >> 8) + ((sb * cover + 255) >> 8));
        p[Order::A] = (value_type)(((p[Order::A] * alpha + 255) >> 8) + ((sa * cover + 255) >> 8));

    }
    else
    {
        p[Order::R] = (value_type)((p[Order::R] * sa + sr * da + base_mask) >> base_shift);
        p[Order::G] = (value_type)((p[Order::G] * sa + sg * da + base_mask) >> base_shift);
        p[Order::B] = (value_type)((p[Order::B] * sa + sb * da + base_mask) >> base_shift);
        p[Order::A] = (value_type)sa;
    }
}
};

//=========================================================comp_op_rgba_xor
template<class ColorT, class Order> struct comp_op_rgba_xor
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca.(1 - Da) + Dca.(1 - Sa)
// Da'  = Sa + Da - 2.Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type s1a = base_mask - sa;
        calc_type d1a = base_mask - p[Order::A];
        p[Order::R] = (value_type)((p[Order::R] * s1a + sr * d1a + base_mask) >> base_shift);
        p[Order::G] = (value_type)((p[Order::G] * s1a + sg * d1a + base_mask) >> base_shift);
        p[Order::B] = (value_type)((p[Order::B] * s1a + sb * d1a + base_mask) >> base_shift);
        p[Order::A] = (value_type)(sa + p[Order::A] - ((sa * p[Order::A] + base_mask/2) >> (base_shift - 1)));
    }
}
};

//=========================================================comp_op_rgba_plus
template<class ColorT, class Order> struct comp_op_rgba_plus
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca + Dca
// Da'  = Sa + Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type dr = p[Order::R] + sr;
        calc_type dg = p[Order::G] + sg;
        calc_type db = p[Order::B] + sb;
        calc_type da = p[Order::A] + sa;
        p[Order::R] = (dr > base_mask) ? (value_type)base_mask : dr;
        p[Order::G] = (dg > base_mask) ? (value_type)base_mask : dg;
        p[Order::B] = (db > base_mask) ? (value_type)base_mask : db;
        p[Order::A] = (da > base_mask) ? (value_type)base_mask : da;
    }
}
};

//========================================================comp_op_rgba_minus
template<class ColorT, class Order> struct comp_op_rgba_minus
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Dca - Sca
// Da' = 1 - (1 - Sa).(1 - Da)
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type dr = p[Order::R] - sr;
        calc_type dg = p[Order::G] - sg;
        calc_type db = p[Order::B] - sb;
        p[Order::R] = (dr > base_mask) ? 0 : dr;
        p[Order::G] = (dg > base_mask) ? 0 : dg;
        p[Order::B] = (db > base_mask) ? 0 : db;
        p[Order::A] = (value_type)(sa + p[Order::A] - ((sa * p[Order::A] + base_mask) >> base_shift));
        //p[Order::A] = (value_type)(base_mask - (((base_mask - sa) * (base_mask - p[Order::A]) + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_multiply
template<class ColorT, class Order> struct comp_op_rgba_multiply
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca.Dca + Sca.(1 - Da) + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type s1a = base_mask - sa;
        calc_type d1a = base_mask - p[Order::A];
        calc_type dr = p[Order::R];
        calc_type dg = p[Order::G];
        calc_type db = p[Order::B];
        p[Order::R] = (value_type)((sr * dr + sr * d1a + dr * s1a + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sg * dg + sg * d1a + dg * s1a + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sb * db + sb * d1a + db * s1a + base_mask) >> base_shift);
        p[Order::A] = (value_type)(sa + p[Order::A] - ((sa * p[Order::A] + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_screen
template<class ColorT, class Order> struct comp_op_rgba_screen
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = Sca + Dca - Sca.Dca
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type dr = p[Order::R];
        calc_type dg = p[Order::G];
        calc_type db = p[Order::B];
        calc_type da = p[Order::A];
        p[Order::R] = (value_type)(sr + dr - ((sr * dr + base_mask) >> base_shift));
        p[Order::G] = (value_type)(sg + dg - ((sg * dg + base_mask) >> base_shift));
        p[Order::B] = (value_type)(sb + db - ((sb * db + base_mask) >> base_shift));
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_overlay
template<class ColorT, class Order> struct comp_op_rgba_overlay
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// if 2.Dca < Da
//   Dca' = 2.Sca.Dca + Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise
//   Dca' = Sa.Da - 2.(Da - Dca).(Sa - Sca) + Sca.(1 - Da) + Dca.(1 - Sa)
// 
// Da' = Sa + Da - Sa.Da
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a  = base_mask - p[Order::A];
        calc_type s1a  = base_mask - sa;
        calc_type dr   = p[Order::R];
        calc_type dg   = p[Order::G];
        calc_type db   = p[Order::B];
        calc_type da   = p[Order::A];
        calc_type sada = sa * p[Order::A];

        p[Order::R] = (value_type)(((2*dr < da) ? 
            2*sr*dr + sr*d1a + dr*s1a : 
            sada - 2*(da - dr)*(sa - sr) + sr*d1a + dr*s1a + base_mask) >> base_shift);

        p[Order::G] = (value_type)(((2*dg < da) ? 
            2*sg*dg + sg*d1a + dg*s1a : 
            sada - 2*(da - dg)*(sa - sg) + sg*d1a + dg*s1a + base_mask) >> base_shift);

        p[Order::B] = (value_type)(((2*db < da) ? 
            2*sb*db + sb*d1a + db*s1a : 
            sada - 2*(da - db)*(sa - sb) + sb*d1a + db*s1a + base_mask) >> base_shift);

        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};


template<class T> inline T sd_min(T a, T b) { return (a < b) ? a : b; }
template<class T> inline T sd_max(T a, T b) { return (a > b) ? a : b; }

//=====================================================comp_op_rgba_darken
template<class ColorT, class Order> struct comp_op_rgba_darken
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = min(Sca.Da, Dca.Sa) + Sca.(1 - Da) + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a = base_mask - p[Order::A];
        calc_type s1a = base_mask - sa;
        calc_type dr  = p[Order::R];
        calc_type dg  = p[Order::G];
        calc_type db  = p[Order::B];
        calc_type da  = p[Order::A];

        p[Order::R] = (value_type)((sd_min(sr * da, dr * sa) + sr * d1a + dr * s1a + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sd_min(sg * da, dg * sa) + sg * d1a + dg * s1a + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sd_min(sb * da, db * sa) + sb * d1a + db * s1a + base_mask) >> base_shift);
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_lighten
template<class ColorT, class Order> struct comp_op_rgba_lighten
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = max(Sca.Da, Dca.Sa) + Sca.(1 - Da) + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a = base_mask - p[Order::A];
        calc_type s1a = base_mask - sa;
        calc_type dr  = p[Order::R];
        calc_type dg  = p[Order::G];
        calc_type db  = p[Order::B];
        calc_type da  = p[Order::A];

        p[Order::R] = (value_type)((sd_max(sr * da, dr * sa) + sr * d1a + dr * s1a + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sd_max(sg * da, dg * sa) + sg * d1a + dg * s1a + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sd_max(sb * da, db * sa) + sb * d1a + db * s1a + base_mask) >> base_shift);
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_color_dodge
template<class ColorT, class Order> struct comp_op_rgba_color_dodge
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// if Sca.Da + Dca.Sa >= Sa.Da
//   Dca' = Sa.Da + Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise
//   Dca' = Dca.Sa/(1-Sca/Sa) + Sca.(1 - Da) + Dca.(1 - Sa)
//
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a  = base_mask - p[Order::A];
        calc_type s1a  = base_mask - sa;
        calc_type dr   = p[Order::R];
        calc_type dg   = p[Order::G];
        calc_type db   = p[Order::B];
        calc_type da   = p[Order::A];
        long_type drsa = dr * sa;
        long_type dgsa = dg * sa;
        long_type dbsa = db * sa;
        long_type srda = sr * da;
        long_type sgda = sg * da;
        long_type sbda = sb * da;
        long_type sada = sa * da;

        p[Order::R] = (value_type)((srda + drsa >= sada) ? 
            (sada + sr * d1a + dr * s1a + base_mask) >> base_shift :
            drsa / (base_mask - (sr << base_shift) / sa) + ((sr * d1a + dr * s1a + base_mask) >> base_shift));

        p[Order::G] = (value_type)((sgda + dgsa >= sada) ? 
            (sada + sg * d1a + dg * s1a + base_mask) >> base_shift :
            dgsa / (base_mask - (sg << base_shift) / sa) + ((sg * d1a + dg * s1a + base_mask) >> base_shift));

        p[Order::B] = (value_type)((sbda + dbsa >= sada) ? 
            (sada + sb * d1a + db * s1a + base_mask) >> base_shift :
            dbsa / (base_mask - (sb << base_shift) / sa) + ((sb * d1a + db * s1a + base_mask) >> base_shift));

        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};
 * 

//=====================================================comp_op_rgba_color_burn
template<class ColorT, class Order> struct comp_op_rgba_color_burn
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// if Sca.Da + Dca.Sa <= Sa.Da
//   Dca' = Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise
//   Dca' = Sa.(Sca.Da + Dca.Sa - Sa.Da)/Sca + Sca.(1 - Da) + Dca.(1 - Sa)
// 
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a  = base_mask - p[Order::A];
        calc_type s1a  = base_mask - sa;
        calc_type dr   = p[Order::R];
        calc_type dg   = p[Order::G];
        calc_type db   = p[Order::B];
        calc_type da   = p[Order::A];
        long_type drsa = dr * sa;
        long_type dgsa = dg * sa;
        long_type dbsa = db * sa;
        long_type srda = sr * da;
        long_type sgda = sg * da;
        long_type sbda = sb * da;
        long_type sada = sa * da;

        p[Order::R] = (value_type)(((srda + drsa <= sada) ? 
            sr * d1a + dr * s1a :
            sa * (srda + drsa - sada) / sr + sr * d1a + dr * s1a + base_mask) >> base_shift);

        p[Order::G] = (value_type)(((sgda + dgsa <= sada) ? 
            sg * d1a + dg * s1a :
            sa * (sgda + dgsa - sada) / sg + sg * d1a + dg * s1a + base_mask) >> base_shift);

        p[Order::B] = (value_type)(((sbda + dbsa <= sada) ? 
            sb * d1a + db * s1a :
            sa * (sbda + dbsa - sada) / sb + sb * d1a + db * s1a + base_mask) >> base_shift);

        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_hard_light
template<class ColorT, class Order> struct comp_op_rgba_hard_light
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// if 2.Sca < Sa
//    Dca' = 2.Sca.Dca + Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise
//    Dca' = Sa.Da - 2.(Da - Dca).(Sa - Sca) + Sca.(1 - Da) + Dca.(1 - Sa)
// 
// Da'  = Sa + Da - Sa.Da
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a  = base_mask - p[Order::A];
        calc_type s1a  = base_mask - sa;
        calc_type dr   = p[Order::R];
        calc_type dg   = p[Order::G];
        calc_type db   = p[Order::B];
        calc_type da   = p[Order::A];
        calc_type sada = sa * da;

        p[Order::R] = (value_type)(((2*sr < sa) ? 
            2*sr*dr + sr*d1a + dr*s1a : 
            sada - 2*(da - dr)*(sa - sr) + sr*d1a + dr*s1a + base_mask) >> base_shift);

        p[Order::G] = (value_type)(((2*sg < sa) ? 
            2*sg*dg + sg*d1a + dg*s1a : 
            sada - 2*(da - dg)*(sa - sg) + sg*d1a + dg*s1a + base_mask) >> base_shift);

        p[Order::B] = (value_type)(((2*sb < sa) ? 
            2*sb*db + sb*d1a + db*s1a : 
            sada - 2*(da - db)*(sa - sb) + sb*d1a + db*s1a + base_mask) >> base_shift);

        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_soft_light
template<class ColorT, class Order> struct comp_op_rgba_soft_light
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// if 2.Sca < Sa
//   Dca' = Dca.(Sa + (1 - Dca/Da).(2.Sca - Sa)) + Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise if 8.Dca <= Da
//   Dca' = Dca.(Sa + (1 - Dca/Da).(2.Sca - Sa).(3 - 8.Dca/Da)) + Sca.(1 - Da) + Dca.(1 - Sa)
// otherwise
//   Dca' = (Dca.Sa + ((Dca/Da)^(0.5).Da - Dca).(2.Sca - Sa)) + Sca.(1 - Da) + Dca.(1 - Sa)
// 
// Da'  = Sa + Da - Sa.Da 

static void blend_pix(value_type* p, 
                                 int r, int g, int b, 
                                 int a, int cover)
{
    double sr = double(r * cover) / (base_mask * 255);
    double sg = double(g * cover) / (base_mask * 255);
    double sb = double(b * cover) / (base_mask * 255);
    double sa = double(a * cover) / (base_mask * 255);
    if(sa > 0)
    {
        double dr = double(p[Order::R]) / base_mask;
        double dg = double(p[Order::G]) / base_mask;
        double db = double(p[Order::B]) / base_mask;
        double da = double(p[Order::A] ? p[Order::A] : 1) / base_mask;
        if(cover < 255)
        {
            a = (a * cover + 255) >> 8;
        }

        if(2*sr < sa)       dr = dr*(sa + (1 - dr/da)*(2*sr - sa)) + sr*(1 - da) + dr*(1 - sa);
        else if(8*dr <= da) dr = dr*(sa + (1 - dr/da)*(2*sr - sa)*(3 - 8*dr/da)) + sr*(1 - da) + dr*(1 - sa);
        else                dr = (dr*sa + (sqrt(dr/da)*da - dr)*(2*sr - sa)) + sr*(1 - da) + dr*(1 - sa);

        if(2*sg < sa)       dg = dg*(sa + (1 - dg/da)*(2*sg - sa)) + sg*(1 - da) + dg*(1 - sa);
        else if(8*dg <= da) dg = dg*(sa + (1 - dg/da)*(2*sg - sa)*(3 - 8*dg/da)) + sg*(1 - da) + dg*(1 - sa);
        else                dg = (dg*sa + (sqrt(dg/da)*da - dg)*(2*sg - sa)) + sg*(1 - da) + dg*(1 - sa);

        if(2*sb < sa)       db = db*(sa + (1 - db/da)*(2*sb - sa)) + sb*(1 - da) + db*(1 - sa);
        else if(8*db <= da) db = db*(sa + (1 - db/da)*(2*sb - sa)*(3 - 8*db/da)) + sb*(1 - da) + db*(1 - sa);
        else                db = (db*sa + (sqrt(db/da)*da - db)*(2*sb - sa)) + sb*(1 - da) + db*(1 - sa);

        p[Order::R] = (value_type)uround(dr * base_mask);
        p[Order::G] = (value_type)uround(dg * base_mask);
        p[Order::B] = (value_type)uround(db * base_mask);
        p[Order::A] = (value_type)(a + p[Order::A] - ((a * p[Order::A] + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_difference
template<class ColorT, class Order> struct comp_op_rgba_difference
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_scale = color_type::base_scale,
    base_mask  = color_type::base_mask
};

// Dca' = Sca + Dca - 2.min(Sca.Da, Dca.Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type dr = p[Order::R];
        calc_type dg = p[Order::G];
        calc_type db = p[Order::B];
        calc_type da = p[Order::A];
        p[Order::R] = (value_type)(sr + dr - ((2 * sd_min(sr*da, dr*sa) + base_mask) >> base_shift));
        p[Order::G] = (value_type)(sg + dg - ((2 * sd_min(sg*da, dg*sa) + base_mask) >> base_shift));
        p[Order::B] = (value_type)(sb + db - ((2 * sd_min(sb*da, db*sa) + base_mask) >> base_shift));
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_exclusion
template<class ColorT, class Order> struct comp_op_rgba_exclusion
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = (Sca.Da + Dca.Sa - 2.Sca.Dca) + Sca.(1 - Da) + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type d1a = base_mask - p[Order::A];
        calc_type s1a = base_mask - sa;
        calc_type dr = p[Order::R];
        calc_type dg = p[Order::G];
        calc_type db = p[Order::B];
        calc_type da = p[Order::A];
        p[Order::R] = (value_type)((sr*da + dr*sa - 2*sr*dr + sr*d1a + dr*s1a + base_mask) >> base_shift);
        p[Order::G] = (value_type)((sg*da + dg*sa - 2*sg*dg + sg*d1a + dg*s1a + base_mask) >> base_shift);
        p[Order::B] = (value_type)((sb*da + db*sa - 2*sb*db + sb*d1a + db*s1a + base_mask) >> base_shift);
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=====================================================comp_op_rgba_contrast
template<class ColorT, class Order> struct comp_op_rgba_contrast
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};


static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    long_type dr = p[Order::R];
    long_type dg = p[Order::G];
    long_type db = p[Order::B];
    int       da = p[Order::A];
    long_type d2a = da >> 1;
    int s2a = sa >> 1;

    int r = (int)((((dr - d2a) * int((sr - s2a)*2 + base_mask)) >> base_shift) + d2a); 
    int g = (int)((((dg - d2a) * int((sg - s2a)*2 + base_mask)) >> base_shift) + d2a); 
    int b = (int)((((db - d2a) * int((sb - s2a)*2 + base_mask)) >> base_shift) + d2a); 

    r = (r < 0) ? 0 : r;
    g = (g < 0) ? 0 : g;
    b = (b < 0) ? 0 : b;

    p[Order::R] = (value_type)((r > da) ? da : r);
    p[Order::G] = (value_type)((g > da) ? da : g);
    p[Order::B] = (value_type)((b > da) ? da : b);
}
};

//=====================================================comp_op_rgba_invert
template<class ColorT, class Order> struct comp_op_rgba_invert
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = (Da - Dca) * Sa + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    sa = (sa * cover + 255) >> 8;
    if(sa)
    {
        calc_type da = p[Order::A];
        calc_type dr = ((da - p[Order::R]) * sa + base_mask) >> base_shift;
        calc_type dg = ((da - p[Order::G]) * sa + base_mask) >> base_shift;
        calc_type db = ((da - p[Order::B]) * sa + base_mask) >> base_shift;
        calc_type s1a = base_mask - sa;
        p[Order::R] = (value_type)(dr + ((p[Order::R] * s1a + base_mask) >> base_shift));
        p[Order::G] = (value_type)(dg + ((p[Order::G] * s1a + base_mask) >> base_shift));
        p[Order::B] = (value_type)(db + ((p[Order::B] * s1a + base_mask) >> base_shift));
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};

//=================================================comp_op_rgba_invert_rgb
template<class ColorT, class Order> struct comp_op_rgba_invert_rgb
{
typedef ColorT color_type;
typedef Order order_type;
typedef typename color_type::value_type value_type;
typedef typename color_type::calc_type calc_type;
typedef typename color_type::long_type long_type;
enum base_scale_e
{ 
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask
};

// Dca' = (Da - Dca) * Sca + Dca.(1 - Sa)
// Da'  = Sa + Da - Sa.Da 
static void blend_pix(value_type* p, 
                                 int sr, int sg, int sb, 
                                 int sa, int cover)
{
    if(cover < 255)
    {
        sr = (sr * cover + 255) >> 8;
        sg = (sg * cover + 255) >> 8;
        sb = (sb * cover + 255) >> 8;
        sa = (sa * cover + 255) >> 8;
    }
    if(sa)
    {
        calc_type da = p[Order::A];
        calc_type dr = ((da - p[Order::R]) * sr + base_mask) >> base_shift;
        calc_type dg = ((da - p[Order::G]) * sg + base_mask) >> base_shift;
        calc_type db = ((da - p[Order::B]) * sb + base_mask) >> base_shift;
        calc_type s1a = base_mask - sa;
        p[Order::R] = (value_type)(dr + ((p[Order::R] * s1a + base_mask) >> base_shift));
        p[Order::G] = (value_type)(dg + ((p[Order::G] * s1a + base_mask) >> base_shift));
        p[Order::B] = (value_type)(db + ((p[Order::B] * s1a + base_mask) >> base_shift));
        p[Order::A] = (value_type)(sa + da - ((sa * da + base_mask) >> base_shift));
    }
}
};





//======================================================comp_op_table_rgba
template<class ColorT, class Order> struct comp_op_table_rgba
{
typedef typename ColorT::value_type value_type;
typedef void (*comp_op_func_type)(value_type* p, 
                                  int cr, 
                                  int cg, 
                                  int cb,
                                  int ca,
                                  int cover);
static comp_op_func_type g_comp_op_func[];
};

//==========================================================g_comp_op_func
template<class ColorT, class Order> 
typename comp_op_table_rgba<ColorT, Order>::comp_op_func_type
comp_op_table_rgba<ColorT, Order>::g_comp_op_func[] = 
{
comp_op_rgba_clear      <ColorT,Order>::blend_pix,
comp_op_rgba_src        <ColorT,Order>::blend_pix,
comp_op_rgba_dst        <ColorT,Order>::blend_pix,
comp_op_rgba_src_over   <ColorT,Order>::blend_pix,
comp_op_rgba_dst_over   <ColorT,Order>::blend_pix,
comp_op_rgba_src_in     <ColorT,Order>::blend_pix,
comp_op_rgba_dst_in     <ColorT,Order>::blend_pix,
comp_op_rgba_src_out    <ColorT,Order>::blend_pix,
comp_op_rgba_dst_out    <ColorT,Order>::blend_pix,
comp_op_rgba_src_atop   <ColorT,Order>::blend_pix,
comp_op_rgba_dst_atop   <ColorT,Order>::blend_pix,
comp_op_rgba_xor        <ColorT,Order>::blend_pix,
comp_op_rgba_plus       <ColorT,Order>::blend_pix,
comp_op_rgba_minus      <ColorT,Order>::blend_pix,
comp_op_rgba_multiply   <ColorT,Order>::blend_pix,
comp_op_rgba_screen     <ColorT,Order>::blend_pix,
comp_op_rgba_overlay    <ColorT,Order>::blend_pix,
comp_op_rgba_darken     <ColorT,Order>::blend_pix,
comp_op_rgba_lighten    <ColorT,Order>::blend_pix,
comp_op_rgba_color_dodge<ColorT,Order>::blend_pix,
comp_op_rgba_color_burn <ColorT,Order>::blend_pix,
comp_op_rgba_hard_light <ColorT,Order>::blend_pix,
comp_op_rgba_soft_light <ColorT,Order>::blend_pix,
comp_op_rgba_difference <ColorT,Order>::blend_pix,
comp_op_rgba_exclusion  <ColorT,Order>::blend_pix,
comp_op_rgba_contrast   <ColorT,Order>::blend_pix,
comp_op_rgba_invert     <ColorT,Order>::blend_pix,
comp_op_rgba_invert_rgb <ColorT,Order>::blend_pix,
0
};


//==============================================================comp_op_e
enum comp_op_e
{
comp_op_clear,         //----comp_op_clear
comp_op_src,           //----comp_op_src
comp_op_dst,           //----comp_op_dst
comp_op_src_over,      //----comp_op_src_over
comp_op_dst_over,      //----comp_op_dst_over
comp_op_src_in,        //----comp_op_src_in
comp_op_dst_in,        //----comp_op_dst_in
comp_op_src_out,       //----comp_op_src_out
comp_op_dst_out,       //----comp_op_dst_out
comp_op_src_atop,      //----comp_op_src_atop
comp_op_dst_atop,      //----comp_op_dst_atop
comp_op_xor,           //----comp_op_xor
comp_op_plus,          //----comp_op_plus
comp_op_minus,         //----comp_op_minus
comp_op_multiply,      //----comp_op_multiply
comp_op_screen,        //----comp_op_screen
comp_op_overlay,       //----comp_op_overlay
comp_op_darken,        //----comp_op_darken
comp_op_lighten,       //----comp_op_lighten
comp_op_color_dodge,   //----comp_op_color_dodge
comp_op_color_burn,    //----comp_op_color_burn
comp_op_hard_light,    //----comp_op_hard_light
comp_op_soft_light,    //----comp_op_soft_light
comp_op_difference,    //----comp_op_difference
comp_op_exclusion,     //----comp_op_exclusion
comp_op_contrast,      //----comp_op_contrast
comp_op_invert,        //----comp_op_invert
comp_op_invert_rgb,    //----comp_op_invert_rgb

end_of_comp_op_e
};







//====================================================comp_op_adaptor_rgba
template<class ColorT, class Order> struct comp_op_adaptor_rgba
{
typedef Order  order_type;
typedef ColorT color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    comp_op_table_rgba<ColorT, Order>::g_comp_op_func[op]
        (p, (cr * ca + base_mask) >> base_shift, 
            (cg * ca + base_mask) >> base_shift,
            (cb * ca + base_mask) >> base_shift,
             ca, cover);
}
};

//=========================================comp_op_adaptor_clip_to_dst_rgba
template<class ColorT, class Order> struct comp_op_adaptor_clip_to_dst_rgba
{
typedef Order  order_type;
typedef ColorT color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    cr = (cr * ca + base_mask) >> base_shift;
    cg = (cg * ca + base_mask) >> base_shift;
    cb = (cb * ca + base_mask) >> base_shift;
    int da = p[Order::A];
    comp_op_table_rgba<ColorT, Order>::g_comp_op_func[op]
        (p, (cr * da + base_mask) >> base_shift, 
            (cg * da + base_mask) >> base_shift, 
            (cb * da + base_mask) >> base_shift, 
            (ca * da + base_mask) >> base_shift, 
            cover);
}
};

//================================================comp_op_adaptor_rgba_pre
template<class ColorT, class Order> struct comp_op_adaptor_rgba_pre
{
typedef Order  order_type;
typedef ColorT color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    comp_op_table_rgba<ColorT, Order>::g_comp_op_func[op](p, cr, cg, cb, ca, cover);
}
};

//=====================================comp_op_adaptor_clip_to_dst_rgba_pre
template<class ColorT, class Order> struct comp_op_adaptor_clip_to_dst_rgba_pre
{
typedef Order  order_type;
typedef ColorT color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    int da = p[Order::A];
    comp_op_table_rgba<ColorT, Order>::g_comp_op_func[op]
        (p, (cr * da + base_mask) >> base_shift, 
            (cg * da + base_mask) >> base_shift, 
            (cb * da + base_mask) >> base_shift, 
            (ca * da + base_mask) >> base_shift, 
            cover);
}
};

//=======================================================comp_adaptor_rgba
template<class BlenderPre> struct comp_adaptor_rgba
{
typedef typename BlenderPre::order_type order_type;
typedef typename BlenderPre::color_type color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    BlenderPre::blend_pix(p, 
                          (cr * ca + base_mask) >> base_shift, 
                          (cg * ca + base_mask) >> base_shift,
                          (cb * ca + base_mask) >> base_shift,
                          ca, cover);
}
};

//==========================================comp_adaptor_clip_to_dst_rgba
template<class BlenderPre> struct comp_adaptor_clip_to_dst_rgba
{
typedef typename BlenderPre::order_type order_type;
typedef typename BlenderPre::color_type color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    cr = (cr * ca + base_mask) >> base_shift;
    cg = (cg * ca + base_mask) >> base_shift;
    cb = (cb * ca + base_mask) >> base_shift;
    int da = p[ImageBuffer.OrderA];
    BlenderPre::blend_pix(p, 
                          (cr * da + base_mask) >> base_shift, 
                          (cg * da + base_mask) >> base_shift, 
                          (cb * da + base_mask) >> base_shift, 
                          (ca * da + base_mask) >> base_shift, 
                          cover);
}
};

//======================================comp_adaptor_clip_to_dst_rgba_pre
template<class BlenderPre> struct comp_adaptor_clip_to_dst_rgba_pre
{
typedef typename BlenderPre::order_type order_type;
typedef typename BlenderPre::color_type color_type;
typedef typename color_type::value_type value_type;
enum base_scale_e
{  
    base_shift = color_type::base_shift,
    base_mask  = color_type::base_mask 
};

static void blend_pix(int op, value_type* p, 
                                 int cr, int cg, int cb,
                                 int ca,
                                 int cover)
{
    int da = p[ImageBuffer.OrderA];
    BlenderPre::blend_pix(p, 
                          (cr * da + base_mask) >> base_shift, 
                          (cg * da + base_mask) >> base_shift, 
                          (cb * da + base_mask) >> base_shift, 
                          (ca * da + base_mask) >> base_shift, 
                          cover);
}
};

    //================================================pixfmt_custom_blend_rgba
    template<class Blender, class RenBuf> class pixfmt_custom_blend_rgba
    {
    public:
        //typedef RenBuf   rbuf_type;
        //typedef typename rbuf_type::row_data row_data;
        typedef Blender  blender_type;
        typedef typename blender_type::color_type color_type;
        typedef typename blender_type::order_type order_type;
        typedef typename color_type::value_type value_type;
        typedef typename color_type::calc_type calc_type;
        enum base_scale_e
        {
            base_shift = color_type::base_shift,
            base_scale = color_type::base_scale,
            base_mask  = color_type::base_mask,
            pix_width  = sizeof(value_type) * 4 
        };


        //--------------------------------------------------------------------
        pixfmt_custom_blend_rgba() : m_rbuf(0), m_comp_op(3) {}
        explicit pixfmt_custom_blend_rgba(rendering_buffer rb, int comp_op=3) : 
            m_rbuf(&rb),
            m_comp_op(comp_op)
        {}
        void attach(rendering_buffer rb) { m_rbuf = &rb; }

        //--------------------------------------------------------------------
        template<class PixFmt>
        bool attach(PixFmt& pixf, int x1, int y1, int x2, int y2)
        {
            rect_i r(x1, y1, x2, y2);
            if(r.clip(rect_i(0, 0, pixf.width()-1, pixf.height()-1)))
            {
                int stride = pixf.stride();
                m_rbuf->attach(pixf.pix_ptr(r.x1, stride < 0 ? r.y2 : r.y1), 
                               (r.x2 - r.x1) + 1,
                               (r.y2 - r.y1) + 1,
                               stride);
                return true;
            }
            return false;
        }

        //--------------------------------------------------------------------
        int width()  const { return m_rbuf->width();  }
        int height() const { return m_rbuf->height(); }
        int      stride() const { return m_rbuf->stride(); }

        //--------------------------------------------------------------------
              byte* row_ptr(int y)       { return m_rbuf->row_ptr(y); }
        const byte* row_ptr(int y) const { return m_rbuf->row_ptr(y); }
        row_data     row(int y)     const { return m_rbuf->row(y); }

        //--------------------------------------------------------------------
        byte* pix_ptr(int x, int y)
        {
            return m_rbuf->row_ptr(y) + x * pix_width;
        }

        const byte* pix_ptr(int x, int y) const
        {
            return m_rbuf->row_ptr(y) + x * pix_width;
        }

        //--------------------------------------------------------------------
        void comp_op(int op) { m_comp_op = op; }
        int comp_op() const  { return m_comp_op; }

        //--------------------------------------------------------------------
        static void make_pix(byte* p, const color_type& c)
        {
            ((value_type*)p)[ImageBuffer.OrderR] = c.r;
            ((value_type*)p)[ImageBuffer.OrderG] = c.g;
            ((value_type*)p)[ImageBuffer.OrderB] = c.b;
            ((value_type*)p)[ImageBuffer.OrderA] = c.a;
        }

        //--------------------------------------------------------------------
        color_type pixel(int x, int y) const
        {
            const value_type* p = (value_type*)m_rbuf->row_ptr(y) + (x << 2);
            return color_type(p[ImageBuffer.OrderR], 
                              p[ImageBuffer.OrderG], 
                              p[ImageBuffer.OrderB], 
                              p[ImageBuffer.OrderA]);
        }

        //--------------------------------------------------------------------
        unsafe void copy_pixel(int x, int y, byte* c)
        {
            blender_type::blend_pix(
                m_comp_op, 
                (value_type*)m_rbuf->row_ptr(x, y, 1) + (x << 2), 
                c.r, c.g, c.b, c.a, 255);
        }

        //--------------------------------------------------------------------
        void blend_pixel(int x, int y, const color_type& c, byte cover)
        {
            blender_type::blend_pix(
                m_comp_op, 
                (value_type*)m_rbuf->row_ptr(x, y, 1) + (x << 2),
                c.r, c.g, c.b, c.a, 
                cover);
        }

        //--------------------------------------------------------------------
        void copy_hline(int x, int y, int len, const color_type& c)
        {
            value_type* p = (value_type*)m_rbuf->row_ptr(x, y, len) + (x << 2);
            do
            {
                blender_type::blend_pix(m_comp_op, p, c.r, c.g, c.b, c.a, 255);
                p += 4;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void copy_vline(int x, int y, int len, const color_type& c)
        {
            do
            {
                blender_type::blend_pix(
                    m_comp_op, 
                    (value_type*)m_rbuf->row_ptr(x, y++, 1) + (x << 2),
                    c.r, c.g, c.b, c.a, 255);
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_hline(int x, int y, int len, 
                         const color_type& c, byte cover)
        {

            value_type* p = (value_type*)m_rbuf->row_ptr(x, y, len) + (x << 2);
            do
            {
                blender_type::blend_pix(m_comp_op, p, c.r, c.g, c.b, c.a, cover);
                p += 4;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_vline(int x, int y, int len, 
                         const color_type& c, byte cover)
        {

            do
            {
                blender_type::blend_pix(
                    m_comp_op, 
                    (value_type*)m_rbuf->row_ptr(x, y++, 1) + (x << 2), 
                    c.r, c.g, c.b, c.a, 
                    cover);
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_solid_hspan(int x, int y, int len, 
                               const color_type& c, const byte* covers)
        {
            value_type* p = (value_type*)m_rbuf->row_ptr(x, y, len) + (x << 2);
            do 
            {
                blender_type::blend_pix(m_comp_op, 
                                        p, c.r, c.g, c.b, c.a, 
                                        *covers++);
                p += 4;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_solid_vspan(int x, int y, int len, 
                               const color_type& c, const byte* covers)
        {
            do 
            {
                blender_type::blend_pix(
                    m_comp_op, 
                    (value_type*)m_rbuf->row_ptr(x, y++, 1) + (x << 2), 
                    c.r, c.g, c.b, c.a, 
                    *covers++);
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void copy_color_hspan(int x, int y,
                              int len, 
                              const color_type* colors)
        {

            value_type* p = (value_type*)m_rbuf->row_ptr(x, y, len) + (x << 2);
            do 
            {
                p[ImageBuffer.OrderR] = colors->r;
                p[ImageBuffer.OrderG] = colors->g;
                p[ImageBuffer.OrderB] = colors->b;
                p[ImageBuffer.OrderA] = colors->a;
                ++colors;
                p += 4;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void copy_color_vspan(int x, int y,
                              int len, 
                              const color_type* colors)
        {
            do 
            {
                value_type* p = (value_type*)m_rbuf->row_ptr(x, y++, 1) + (x << 2);
                p[ImageBuffer.OrderR] = colors->r;
                p[ImageBuffer.OrderG] = colors->g;
                p[ImageBuffer.OrderB] = colors->b;
                p[ImageBuffer.OrderA] = colors->a;
                ++colors;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_color_hspan(int x, int y, int len, 
                               const color_type* colors, 
                               const byte* covers)
        {
            value_type* p = (value_type*)m_rbuf->row_ptr(x, y, len) + (x << 2);
            do 
            {
                blender_type::blend_pix(m_comp_op, 
                                        p, 
                                        colors->r, 
                                        colors->g, 
                                        colors->b, 
                                        colors->a, 
                                        covers ? *covers++ : cover);
                p += 4;
                ++colors;
            }
            while(--len);
        }

        //--------------------------------------------------------------------
        void blend_color_vspan(int x, int y, int len, 
                               const color_type* colors, 
                               const byte* covers,
                               byte cover)
        {
            do 
            {
                blender_type::blend_pix(
                    m_comp_op, 
                    (value_type*)m_rbuf->row_ptr(x, y++, 1) + (x << 2), 
                    colors->r,
                    colors->g,
                    colors->b,
                    colors->a,
                    covers ? *covers++ : cover);
                ++colors;
            }
            while(--len);

        }

        //--------------------------------------------------------------------
        template<class Function> void for_each_pixel(Function f)
        {
            int y;
            for(y = 0; y < height(); ++y)
            {
                row_data r = m_rbuf->row(y);
                if(r.ptr)
                {
                    int len = r.x2 - r.x1 + 1;
                    value_type* p = 
                        (value_type*)m_rbuf->row_ptr(r.x1, y, len) + (r.x1 << 2);
                    do
                    {
                        f(p);
                        p += 4;
                    }
                    while(--len);
                }
            }
        }

        //--------------------------------------------------------------------
        void premultiply()
        {
            for_each_pixel(multiplier_rgba<color_type, order_type>::premultiply);
        }

        //--------------------------------------------------------------------
        void demultiply()
        {
            for_each_pixel(multiplier_rgba<color_type, order_type>::demultiply);
        }

        //--------------------------------------------------------------------
        template<class GammaLut> void apply_gamma_dir(const GammaLut& g)
        {
            for_each_pixel(apply_gamma_dir_rgba<color_type, order_type, GammaLut>(g));
        }

        //--------------------------------------------------------------------
        template<class GammaLut> void apply_gamma_inv(const GammaLut& g)
        {
            for_each_pixel(apply_gamma_inv_rgba<color_type, order_type, GammaLut>(g));
        }

        //--------------------------------------------------------------------
        template<class RenBuf2> void copy_from(const RenBuf2& from, 
                                               int xdst, int ydst,
                                               int xsrc, int ysrc,
                                               int len)
        {
            const byte* p = from.row_ptr(ysrc);
            if(p)
            {
                memmove(m_rbuf->row_ptr(xdst, ydst, len) + xdst * pix_width, 
                        p + xsrc * pix_width, 
                        len * pix_width);
            }
        }

        //--------------------------------------------------------------------
        template<class SrcPixelFormatRenderer> 
        void blend_from(const SrcPixelFormatRenderer& from, 
                        int xdst, int ydst,
                        int xsrc, int ysrc,
                        int len,
                        byte cover)
        {
            typedef typename SrcPixelFormatRenderer::order_type src_order;
            const value_type* psrc = (const value_type*)from.row_ptr(ysrc);
            if(psrc)
            {
                psrc += xsrc << 2;
                value_type* pdst = 
                    (value_type*)m_rbuf->row_ptr(xdst, ydst, len) + (xdst << 2);

                int incp = 4;
                if(xdst > xsrc)
                {
                    psrc += (len-1) << 2;
                    pdst += (len-1) << 2;
                    incp = -4;
                }

                do 
                {
                    blender_type::blend_pix(m_comp_op, 
                                            pdst, 
                                            psrc[src_order::R],
                                            psrc[src_order::G],
                                            psrc[src_order::B],
                                            psrc[src_order::A],
                                            cover);
                    psrc += incp;
                    pdst += incp;
                }
                while(--len);
            }
        }

        //--------------------------------------------------------------------
        template<class SrcPixelFormatRenderer>
        void blend_from_color(const SrcPixelFormatRenderer& from, 
                              const color_type& color,
                              int xdst, int ydst,
                              int xsrc, int ysrc,
                              int len,
                              byte cover)
        {
            typedef typename SrcPixelFormatRenderer::value_type src_value_type;
            const src_value_type* psrc = (src_value_type*)from.row_ptr(ysrc);
            if(psrc)
            {
                value_type* pdst = 
                    (value_type*)m_rbuf->row_ptr(xdst, ydst, len) + (xdst << 2);
                do 
                {
                    blender_type::blend_pix(m_comp_op,
                                            pdst, 
                                            color.r, color.g, color.b, color.a,
                                            (*psrc * cover + base_mask) >> base_shift);
                    ++psrc;
                    pdst += 4;
                }
                while(--len);
            }
        }

        //--------------------------------------------------------------------
        template<class SrcPixelFormatRenderer>
        void blend_from_lut(const SrcPixelFormatRenderer& from, 
                            const color_type* color_lut,
                            int xdst, int ydst,
                            int xsrc, int ysrc,
                            int len,
                            byte cover)
        {
            typedef typename SrcPixelFormatRenderer::value_type src_value_type;
            const src_value_type* psrc = (src_value_type*)from.row_ptr(ysrc);
            if(psrc)
            {
                value_type* pdst = 
                    (value_type*)m_rbuf->row_ptr(xdst, ydst, len) + (xdst << 2);
                do 
                {
                    const color_type& color = color_lut[*psrc];
                    blender_type::blend_pix(m_comp_op,
                                            pdst, 
                                            color.r, color.g, color.b, color.a,
                                            cover);
                    ++psrc;
                    pdst += 4;
                }
                while(--len);
            }
        }

    private:
        rendering_buffer m_rbuf;
        int m_comp_op;
    };




    //-----------------------------------------------------------------------
    typedef blender_rgba<rgba8, order_rgba> blender_rgba32; //----blender_rgba32
    typedef blender_rgba<rgba8, order_argb> blender_argb32; //----blender_argb32
    typedef blender_rgba<rgba8, order_abgr> blender_abgr32; //----blender_abgr32
    typedef blender_rgba<rgba8, order_bgra> blender_bgra32; //----blender_bgra32

    typedef blender_rgba_pre<rgba8, order_rgba> blender_rgba32_pre; //----blender_rgba32_pre
    typedef blender_rgba_pre<rgba8, order_argb> blender_argb32_pre; //----blender_argb32_pre
    typedef blender_rgba_pre<rgba8, order_abgr> blender_abgr32_pre; //----blender_abgr32_pre
    typedef blender_rgba_pre<rgba8, order_bgra> blender_bgra32_pre; //----blender_bgra32_pre

    typedef blender_rgba_plain<rgba8, order_rgba> blender_rgba32_plain; //----blender_rgba32_plain
    typedef blender_rgba_plain<rgba8, order_argb> blender_argb32_plain; //----blender_argb32_plain
    typedef blender_rgba_plain<rgba8, order_abgr> blender_abgr32_plain; //----blender_abgr32_plain
    typedef blender_rgba_plain<rgba8, order_bgra> blender_bgra32_plain; //----blender_bgra32_plain

    typedef blender_rgba<rgba16, order_rgba> blender_rgba64; //----blender_rgba64
    typedef blender_rgba<rgba16, order_argb> blender_argb64; //----blender_argb64
    typedef blender_rgba<rgba16, order_abgr> blender_abgr64; //----blender_abgr64
    typedef blender_rgba<rgba16, order_bgra> blender_bgra64; //----blender_bgra64

    typedef blender_rgba_pre<rgba16, order_rgba> blender_rgba64_pre; //----blender_rgba64_pre
    typedef blender_rgba_pre<rgba16, order_argb> blender_argb64_pre; //----blender_argb64_pre
    typedef blender_rgba_pre<rgba16, order_abgr> blender_abgr64_pre; //----blender_abgr64_pre
    typedef blender_rgba_pre<rgba16, order_bgra> blender_bgra64_pre; //----blender_bgra64_pre


    //-----------------------------------------------------------------------
    typedef int32u pixel32_type;
    typedef pixfmt_alpha_blend_rgba<blender_rgba32, rendering_buffer, pixel32_type> pixfmt_rgba32; //----pixfmt_rgba32
    typedef pixfmt_alpha_blend_rgba<blender_argb32, rendering_buffer, pixel32_type> pixfmt_argb32; //----pixfmt_argb32
    typedef pixfmt_alpha_blend_rgba<blender_abgr32, rendering_buffer, pixel32_type> pixfmt_abgr32; //----pixfmt_abgr32
    typedef pixfmt_alpha_blend_rgba<blender_bgra32, rendering_buffer, pixel32_type> pixfmt_bgra32; //----pixfmt_bgra32

    typedef pixfmt_alpha_blend_rgba<blender_rgba32_pre, rendering_buffer, pixel32_type> pixfmt_rgba32_pre; //----pixfmt_rgba32_pre
    typedef pixfmt_alpha_blend_rgba<blender_argb32_pre, rendering_buffer, pixel32_type> pixfmt_argb32_pre; //----pixfmt_argb32_pre
    typedef pixfmt_alpha_blend_rgba<blender_abgr32_pre, rendering_buffer, pixel32_type> pixfmt_abgr32_pre; //----pixfmt_abgr32_pre
    typedef pixfmt_alpha_blend_rgba<blender_bgra32_pre, rendering_buffer, pixel32_type> pixfmt_bgra32_pre; //----pixfmt_bgra32_pre

    typedef pixfmt_alpha_blend_rgba<blender_rgba32_plain, rendering_buffer, pixel32_type> pixfmt_rgba32_plain; //----pixfmt_rgba32_plain
    typedef pixfmt_alpha_blend_rgba<blender_argb32_plain, rendering_buffer, pixel32_type> pixfmt_argb32_plain; //----pixfmt_argb32_plain
    typedef pixfmt_alpha_blend_rgba<blender_abgr32_plain, rendering_buffer, pixel32_type> pixfmt_abgr32_plain; //----pixfmt_abgr32_plain
    typedef pixfmt_alpha_blend_rgba<blender_bgra32_plain, rendering_buffer, pixel32_type> pixfmt_bgra32_plain; //----pixfmt_bgra32_plain

    struct  pixel64_type { int16u c[4]; };
    typedef pixfmt_alpha_blend_rgba<blender_rgba64, rendering_buffer, pixel64_type> pixfmt_rgba64; //----pixfmt_rgba64
    typedef pixfmt_alpha_blend_rgba<blender_argb64, rendering_buffer, pixel64_type> pixfmt_argb64; //----pixfmt_argb64
    typedef pixfmt_alpha_blend_rgba<blender_abgr64, rendering_buffer, pixel64_type> pixfmt_abgr64; //----pixfmt_abgr64
    typedef pixfmt_alpha_blend_rgba<blender_bgra64, rendering_buffer, pixel64_type> pixfmt_bgra64; //----pixfmt_bgra64

    typedef pixfmt_alpha_blend_rgba<blender_rgba64_pre, rendering_buffer, pixel64_type> pixfmt_rgba64_pre; //----pixfmt_rgba64_pre
    typedef pixfmt_alpha_blend_rgba<blender_argb64_pre, rendering_buffer, pixel64_type> pixfmt_argb64_pre; //----pixfmt_argb64_pre
    typedef pixfmt_alpha_blend_rgba<blender_abgr64_pre, rendering_buffer, pixel64_type> pixfmt_abgr64_pre; //----pixfmt_abgr64_pre
    typedef pixfmt_alpha_blend_rgba<blender_bgra64_pre, rendering_buffer, pixel64_type> pixfmt_bgra64_pre; //----pixfmt_bgra64_pre
*/
//}