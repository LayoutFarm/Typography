//-----------------------------------------------------------------------
// Copyright (C) 2002-2004 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//-----------------------------------------------------------------------
//from lcd_font.cpp
//-----------------------------------------------------------------------


using System;
namespace PixelFarm.Agg 
{
    // Sub-pixel energy distribution lookup table.
    // See description by Steve Gibson: http://grc.com/cttech.htm
    // The class automatically normalizes the coefficients
    // in such a way that primary + 2*secondary + 3*tertiary = 1.0
    // Also, the input values are in range of 0...NumLevels, output ones
    // are 0...255
    //--------------------------------- 
    //template<class GgoFormat> class lcd_distribution_lut
    //{
    //public:
    //    lcd_distribution_lut(double prim, double second, double tert)
    //    {
    //        double norm = (255.0 / (GgoFormat::num_levels - 1)) / (prim + second*2 + tert*2);
    //        prim   *= norm;
    //        second *= norm;
    //        tert   *= norm;
    //        for(unsigned i = 0; i < GgoFormat::num_levels; i++)
    //        {
    //            m_primary[i]   = (unsigned char)floor(prim   * i);
    //            m_secondary[i] = (unsigned char)floor(second * i);
    //            m_tertiary[i]  = (unsigned char)floor(tert   * i);
    //        }
    //    }

    //    unsigned primary(unsigned v)   const { return m_primary[v];   }
    //    unsigned secondary(unsigned v) const { return m_secondary[v]; }
    //    unsigned tertiary(unsigned v)  const { return m_tertiary[v];  }

    //    static unsigned ggo_format()
    //    {
    //        return GgoFormat::format;
    //    }

    //private:
    //    unsigned char m_primary[GgoFormat::num_levels];
    //    unsigned char m_secondary[GgoFormat::num_levels];
    //    unsigned char m_tertiary[GgoFormat::num_levels];
    //};

    //    // Possible formats for GetGlyphOutline() and corresponding 
    //// numbers of levels of gray.
    ////---------------------------------
    //struct ggo_gray2 { enum { num_levels = 5,  format = GGO_GRAY2_BITMAP }; };
    //struct ggo_gray4 { enum { num_levels = 17, format = GGO_GRAY4_BITMAP }; };
    //struct ggo_gray8 { enum { num_levels = 65, format = GGO_GRAY8_BITMAP }; };

    public enum GrayLevels
    {
        Gray2,
        Gray4,
        Gray8
    }

    public class LcdDistributionLut
    {
        byte[] m_primary;
        byte[] m_secondary;
        byte[] m_tertiary;
        GrayLevels grayLevel;

        public LcdDistributionLut(GrayLevels grayLevel, double prim, double second, double tert)
        {
            this.grayLevel = grayLevel;
            int numLevel = 0;
            switch (grayLevel)
            {
                default: throw new NotSupportedException();
                case GrayLevels.Gray2: numLevel = 5; break;
                case GrayLevels.Gray4: numLevel = 17; break;
                case GrayLevels.Gray8: numLevel = 65; break;
            }
            m_primary = new byte[numLevel];
            m_secondary = new byte[numLevel];
            m_tertiary = new byte[numLevel];

            double norm = (255.0 / (numLevel - 1)) / (prim + second * 2 + tert * 3);
            prim *= norm;
            second *= norm;
            tert *= norm;
            for (int i = 0; i < numLevel; ++i)
            {
                m_primary[i] = (byte)Math.Floor(prim * i);
                m_secondary[i] = (byte)Math.Floor(second * i);
                m_tertiary[i] = (byte)Math.Floor(tert * i);
            }
        }
        public byte Primary(int index)
        {
            return m_primary[index];
        }
        public byte Secondary(int index)
        {
            return m_secondary[index];
        }
        public byte Tertiary(int index)
        {
            return m_tertiary[index];
        }
    }

    

    // This function prepares the alpha-channel information 
    //// for the glyph averaging the values in accordance with 
    //// the method suggested by Steve Gibson. The function
    //// extends the width by 4 extra pixels, 2 at the beginning 
    //// and 2 at the end. Also, it doesn't align the new width 
    //// to 4 bytes, that is, the output gm.gmBlackBoxX is the 
    //// actual width of the array.
    ////---------------------------------
    //template<class LutType>
    //void prepare_lcd_glyph(const LutType& lut, 
    //                       const unsigned char* gbuf1, 
    //                       const GLYPHMETRICS& gm, 
    //                       unsigned char* gbuf2, 
    //                       GLYPHMETRICS* gm2)
    //{
    //    unsigned src_stride = (gm.gmBlackBoxX + 3) / 4 * 4;
    //    unsigned dst_width  = src_stride + 4;
    //    memset(gbuf2, 0, dst_width * gm.gmBlackBoxY);

    //    for(unsigned y = 0; y < gm.gmBlackBoxY; ++y)
    //    {
    //        const unsigned char* src_ptr = gbuf1 + src_stride * y;
    //        unsigned char* dst_ptr = gbuf2 + dst_width * y;
    //        unsigned x;
    //        for(x = 0; x < gm.gmBlackBoxX; ++x)
    //        {
    //            unsigned v = *src_ptr++;
    //            dst_ptr[0] += lut.tertiary(v);
    //            dst_ptr[1] += lut.secondary(v);
    //            dst_ptr[2] += lut.primary(v);
    //            dst_ptr[3] += lut.secondary(v);
    //            dst_ptr[4] += lut.tertiary(v);
    //            ++dst_ptr;
    //        }
    //    }
    //    gm2->gmBlackBoxX = dst_width;
    //}

    // Blend one span into the R-G-B 24 bit frame buffer
    // For the B-G-R byte order or for 32-bit buffers modify
    // this function accordingly. The general idea is 'span' 
    // contains alpha values for individual color channels in the 
    // R-G-B order, so, for the B-G-R order you will have to 
    // choose values from the 'span' array differently
    //---------------------------------
    //void blend_lcd_span(int x, 
    //                    int y, 
    //                    const unsigned char* span, 
    //                    int width, 
    //                    const rgba& color, 
    //                    unsigned char* rgb24_buf, 
    //                    unsigned rgb24_stride)
    //{
    //    unsigned char* p = rgb24_buf + rgb24_stride * y + x;
    //    unsigned char rgb[3] = { color.r, color.g, color.b };
    //    int i = x % 3;
    //    do
    //    {
    //        int a0 = int(*span++) * color.a;
    //        *p++ = (unsigned char)((((rgb[i++] - *p) * a0) + (*p << 16)) >> 16);
    //        if(i > 2) i = 0;
    //    }
    //    while(--width);
    //}


}