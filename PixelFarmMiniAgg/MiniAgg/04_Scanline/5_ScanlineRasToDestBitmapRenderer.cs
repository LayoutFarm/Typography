//BSD, 2014-2016, WinterDev

//MatterHackers
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
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
using PixelFarm.Agg.Imaging;
namespace PixelFarm.Agg
{
    public enum ScanlineRenderMode
    {
        Default,
        Custom,
        SubPixelRendering
    }




    /// <summary>
    /// scanline rasterizer TO DESTINATION bitmap
    /// </summary>  
    public class ScanlineRasToDestBitmapRenderer
    {

        //agg lcd test
        //lcd_distribution_lut<ggo_gray8> lut(1.0/3.0, 2.0/9.0, 1.0/9.0);
        //lcd_distribution_lut<ggo_gray8> lut(0.5, 0.25, 0.125);
        /// <summary>
        /// grey scale 8, lcd lookup table
        /// </summary>
        static readonly LcdDistributionLut g8LcdLut = new LcdDistributionLut(GrayLevels.Gray8, 0.5, 0.25, 0.125);
        //
        const float cover_1_3 = 255f / 3f;
        const float cover_2_3 = cover_1_3 * 2f;
        ArrayList<Color> tempSpanColors = new ArrayList<Color>();
        public ScanlineRasToDestBitmapRenderer()
        {
        }
        public ScanlineRenderMode ScanlineRenderMode
        {
            get;
            set;
        }


        static float mix(float farColor, float nearColor, float weight)
        {
            //from ...
            //opengl es2 mix function              
            return farColor * (1f - weight) + (nearColor * weight);
        }

        public class ForwardTemporaryBuffer
        {
            //reuseable for subpixel grey scale buffer

            byte[] byteBuffer = new byte[5];
            int writeIndex = 0;
            int readIndex = 0;

            /// <summary>
            /// accumulate byte data 
            /// </summary>
            /// <param name="v0"></param>
            /// <param name="v1"></param>
            /// <param name="v2"></param>
            public void WriteAccum(byte v0, byte v1, byte v2)
            {
                byte v3 = v1, v4 = v0;
                //indeed we can use loop for this,
                //but in this case we just switch it
                switch (writeIndex)
                {
                    default: throw new NotSupportedException();
                    case 0:
                        byteBuffer[0] += v0; byteBuffer[1] += v1; byteBuffer[2] += v2;
                        byteBuffer[3] += v3; byteBuffer[4] += v4;
                        writeIndex = 1;
                        break;
                    case 1:
                        byteBuffer[1] += v0; byteBuffer[2] += v1; byteBuffer[3] += v2;
                        byteBuffer[4] += v3; byteBuffer[0] += v4;
                        writeIndex = 2;
                        break;
                    case 2:
                        byteBuffer[2] += v0; byteBuffer[3] += v1; byteBuffer[4] += v2;
                        byteBuffer[0] += v3; byteBuffer[1] += v4;
                        writeIndex = 3;
                        break;
                    case 3:
                        byteBuffer[3] += v0; byteBuffer[4] += v1; byteBuffer[0] += v2;
                        byteBuffer[1] += v3; byteBuffer[2] += v4;
                        writeIndex = 4;
                        break;
                    case 4:
                        byteBuffer[4] += v0; byteBuffer[0] += v1; byteBuffer[1] += v2;
                        byteBuffer[2] += v3; byteBuffer[3] += v4;
                        writeIndex = 0;
                        break;
                }
            }
            public void Reset()
            {
                writeIndex = 0;
                readIndex = 0;
                byteBuffer[0] = byteBuffer[1] = byteBuffer[2] = byteBuffer[3] = byteBuffer[4] = 0;
            }
            public void ReadNext(out byte v0, out byte v1, out byte v2, out byte v3, out byte v4)
            {
                //read from current read index
                //
                //indeed we can use loop for this,
                //but in this case we just switch it

                switch (readIndex)
                {
                    default: throw new NotSupportedException();
                    case 0:
                        v0 = byteBuffer[0]; v1 = byteBuffer[1]; v2 = byteBuffer[2];
                        v3 = byteBuffer[3]; v4 = byteBuffer[4];
                        readIndex = 1;
                        byteBuffer[0] = 0;//clear for next accum 
                        break;
                    case 1:
                        v0 = byteBuffer[1]; v1 = byteBuffer[2]; v2 = byteBuffer[3];
                        v3 = byteBuffer[4]; v4 = byteBuffer[0];
                        readIndex = 2;
                        byteBuffer[1] = 0;//clear for next accum 
                        break;
                    case 2:
                        v0 = byteBuffer[2]; v1 = byteBuffer[3]; v2 = byteBuffer[4];
                        v3 = byteBuffer[0]; v4 = byteBuffer[1];
                        readIndex = 3;
                        byteBuffer[2] = 0;//clear for next accum 
                        break;
                    case 3:
                        v0 = byteBuffer[3]; v1 = byteBuffer[4]; v2 = byteBuffer[0];
                        v3 = byteBuffer[1]; v4 = byteBuffer[2];
                        readIndex = 4;
                        byteBuffer[3] = 0;//clear for next accum 
                        break;
                    case 4:
                        v0 = byteBuffer[4]; v1 = byteBuffer[0]; v2 = byteBuffer[1];
                        v3 = byteBuffer[2]; v4 = byteBuffer[3];
                        readIndex = 0;
                        byteBuffer[4] = 0;//clear for next accum 
                        break;
                }
            }

        }

        ForwardTemporaryBuffer _forwardTempBuff = new ForwardTemporaryBuffer();
        void SubPixRender(IImageReaderWriter dest, Scanline scanline, Color color)
        {

            byte[] covers = scanline.GetCovers();
            int num_spans = scanline.SpanCount;
            int y = scanline.Y;
            byte[] buffer = dest.GetBuffer();
            IPixelBlender blender = dest.GetRecieveBlender();
            int last_x = int.MinValue;
            int bufferOffset = 0;
            _forwardTempBuff.Reset();

            for (int i = 1; i <= num_spans; ++i)
            {
                //render span by span  
                ScanlineSpan span = scanline.GetSpan(i);
                if (span.x != last_x + 1)
                {
                    bufferOffset = dest.GetBufferOffsetXY(span.x, y);
                }

                last_x = span.x;
                int num_pix = span.len;
                if (num_pix < 0)
                {
                    //special encode***
                    num_pix = -num_pix; //make it positive value
                    last_x += (num_pix - 1);
                    //long span with coverage
                    int coverageValue = covers[span.cover_index];
                    byte prim1 = g8LcdLut.PrimaryFromCoverage(coverageValue);
                    _forwardTempBuff.WriteAccum(
                        g8LcdLut.TertiaryFromCoverage(coverageValue),
                        g8LcdLut.SecondayFromCoverage(coverageValue),
                        g8LcdLut.PrimaryFromCoverage(coverageValue));

                    //------------------------------------------- 
                    //if (coverageValue >= 255)
                    //{
                    //    //100% cover
                    //    int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                    //    Color todrawColor = Color.FromArgb(a, Color.FromArgb(color.R, color.G, color.B));
                    //    while (num_pix > 0)
                    //    {
                    //        blender.BlendPixel(buffer, bufferOffset, todrawColor);
                    //        bufferOffset += 4; //1 pixel 4 bytes
                    //        --num_pix;
                    //    }
                    //}
                    //else
                    //{
                    int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                    Color newc = Color.FromArgb(color.R, color.G, color.B);
                    Color todrawColor = Color.FromArgb(a, newc);
                    while (num_pix > 0)
                    {
                        blender.BlendPixel(buffer, bufferOffset, todrawColor);
                        bufferOffset += 4; //1 pixel 4 bytes
                        --num_pix;
                    }
                    //}

                }
                else
                {
                    int coverIndex = span.cover_index;
                    last_x += (num_pix - 1);
                    while (num_pix > 0)
                    {
                        int coverageValue = covers[coverIndex++];
                        //if (coverageValue >= 255)
                        //{
                        //    //100% cover
                        //    Color newc = Color.FromArgb(color.R, color.G, color.B);
                        //    int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                        //    blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, newc));

                        //}
                        //else
                        //{
                        //check direction : 

                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                        blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(color.R, color.G, color.B)));
                        //}
                        bufferOffset += 4; //1 pixel 4 bits 
                        --num_pix;

                    }
                }
            }
        }

        //void SubPixRender(IImageReaderWriter dest, Scanline scanline, Color color)
        //{
        //    byte[] covers = scanline.GetCovers();
        //    int num_spans = scanline.SpanCount;
        //    int y = scanline.Y;
        //    byte[] buffer = dest.GetBuffer();
        //    IPixelBlender blender = dest.GetRecieveBlender();
        //    int last_x = int.MinValue;
        //    int bufferOffset = 0;
        //    //------------------------------------------
        //    Color bgColor = Color.White;
        //    float cb_R = bgColor.R / 255f;
        //    float cb_G = bgColor.G / 255f;
        //    float cb_B = bgColor.B / 255f;
        //    float cf_R = color.R / 255f;
        //    float cf_G = color.G / 255f;
        //    float cf_B = color.B / 255f;
        //    //------------------------------------------
        //    int prevCover = -1;
        //    for (int i = 1; i <= num_spans; ++i)
        //    {
        //        //render span by span  
        //        ScanlineSpan span = scanline.GetSpan(i);
        //        if (span.x != last_x + 1)
        //        {
        //            bufferOffset = dest.GetBufferOffsetXY(span.x, y);
        //        }

        //        last_x = span.x;
        //        int num_pix = span.len;
        //        if (num_pix < 0)
        //        {
        //            //special encode***
        //            num_pix = -num_pix; //make it positive value
        //            last_x += (num_pix - 1);
        //            //long span with coverage
        //            int coverageValue = covers[span.cover_index];
        //            //------------------------------------------- 
        //            if (coverageValue >= 255)
        //            {
        //                //100% cover
        //                int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                Color todrawColor = Color.FromArgb(a, Color.FromArgb(color.R, color.G, color.B));
        //                while (num_pix > 0)
        //                {
        //                    blender.BlendPixel(buffer, bufferOffset, todrawColor);
        //                    bufferOffset += 4; //1 pixel 4 bytes
        //                    --num_pix;
        //                }
        //            }
        //            else
        //            {
        //                int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                Color newc = Color.FromArgb(color.R, color.G, color.B);
        //                Color todrawColor = Color.FromArgb(a, newc);
        //                while (num_pix > 0)
        //                {
        //                    blender.BlendPixel(buffer, bufferOffset, todrawColor);
        //                    bufferOffset += 4; //1 pixel 4 bytes
        //                    --num_pix;
        //                }
        //            }
        //            prevCover = coverageValue;
        //        }
        //        else
        //        {
        //            int coverIndex = span.cover_index;
        //            last_x += (num_pix - 1);
        //            while (num_pix > 0)
        //            {
        //                int coverageValue = covers[coverIndex++];
        //                if (coverageValue >= 255)
        //                {
        //                    //100% cover
        //                    Color newc = Color.FromArgb(color.R, color.G, color.B);
        //                    int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                    blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, newc));
        //                    prevCover = coverageValue;
        //                }
        //                else
        //                {
        //                    //check direction : 

        //                    bool isUpHill = coverageValue >= prevCover;
        //                    //if (isUpHill != ((coverageValue % 2) > 0))
        //                    //{
        //                    //}
        //                    //---------------------------- 
        //                    byte c_r = 0, c_g = 0, c_b = 0;
        //                    //----------------------------
        //                    //assume lcd screen is RGB
        //                    float subpix_percent = ((float)(coverageValue) / 256f);
        //                    if (coverageValue < cover_1_3)
        //                    {
        //                        //assume LCD color arrangement is BGR                            
        //                        if (isUpHill)
        //                        {
        //                            c_r = bgColor.R;
        //                            c_g = bgColor.G;
        //                            c_b = (byte)(mix(cb_B, cf_B, subpix_percent) * 255);
        //                        }
        //                        else
        //                        {
        //                            c_r = (byte)(mix(cb_R, cf_R, subpix_percent) * 255);
        //                            c_g = bgColor.G;
        //                            c_b = bgColor.B;
        //                        }

        //                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                        blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
        //                    }
        //                    else if (coverageValue < cover_2_3)
        //                    {
        //                        if (isUpHill)
        //                        {
        //                            c_r = bgColor.R;
        //                            c_g = (byte)(mix(cb_G, cf_G, subpix_percent) * 255);
        //                            c_b = (byte)(mix(cb_B, cf_B, 1) * 255);
        //                        }
        //                        else
        //                        {
        //                            c_r = (byte)(mix(cb_R, cf_R, 1) * 255);
        //                            c_g = (byte)(mix(cb_G, cf_G, subpix_percent) * 255);
        //                            c_b = bgColor.B;
        //                        }

        //                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                        blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
        //                    }
        //                    else
        //                    {
        //                        //cover > 2/3 but not full 
        //                        if (isUpHill)
        //                        {
        //                            c_r = (byte)(mix(cb_R, cf_R, subpix_percent) * 255);
        //                            c_g = (byte)(mix(cb_G, cf_G, 1) * 255);
        //                            c_b = (byte)(mix(cb_B, cf_B, 1) * 255);
        //                        }
        //                        else
        //                        {
        //                            c_r = (byte)(mix(cb_R, cf_R, 1) * 255);
        //                            c_g = (byte)(mix(cb_G, cf_G, 1) * 255);
        //                            c_b = (byte)(mix(cb_B, cf_B, subpix_percent) * 255);
        //                        }

        //                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
        //                        blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
        //                    }
        //                }
        //                bufferOffset += 4; //1 pixel 4 bits 
        //                --num_pix;
        //                prevCover = coverageValue;
        //            }
        //        }
        //    }
        //}
        public void RenderWithColor(IImageReaderWriter dest,
                ScanlineRasterizer sclineRas,
                Scanline scline,
                Color color)
        {
            if (!sclineRas.RewindScanlines()) { return; } //early exit
            //----------------------------------------------- 
            scline.ResetSpans(sclineRas.MinX, sclineRas.MaxX);
            switch (this.ScanlineRenderMode)
            {
                default:
                    {
                        //prev mode  
                        //this mode 
                        while (sclineRas.SweepScanline(scline))
                        {
                            //render solid single scanline
                            int y = scline.Y;
                            int num_spans = scline.SpanCount;
                            byte[] covers = scline.GetCovers();
                            //render each span in the scanline
                            for (int i = 1; i <= num_spans; ++i)
                            {
                                ScanlineSpan span = scline.GetSpan(i);
                                if (span.len > 0)
                                {
                                    //positive len 
                                    dest.BlendSolidHSpan(span.x, y, span.len, color, covers, span.cover_index);
                                }
                                else
                                {
                                    //fill the line, same coverage area
                                    int x = span.x;
                                    int x2 = (x - span.len - 1);
                                    dest.BlendHL(x, y, x2, color, covers[span.cover_index]);
                                }
                            }
                        }
                    }
                    break;
                case Agg.ScanlineRenderMode.SubPixelRendering:
                    {
#if DEBUG
                        int dbugMinScanlineCount = 0;
#endif

                        while (sclineRas.SweepScanline(scline))
                        {
                            SubPixRender(dest, scline, color);
#if DEBUG
                            dbugMinScanlineCount++;
#endif
                        }
                    }
                    break;
                case Agg.ScanlineRenderMode.Custom:
                    {
                        while (sclineRas.SweepScanline(scline))
                        {
                            CustomRenderSingleScanLine(dest, scline, color);
                        }
                    }
                    break;
            }
        }

        public void RenderWithSpan(IImageReaderWriter dest,
                ScanlineRasterizer sclineRas,
                Scanline scline,
                ISpanGenerator spanGenerator)
        {
            if (!sclineRas.RewindScanlines()) { return; } //early exit
            //-----------------------------------------------

            scline.ResetSpans(sclineRas.MinX, sclineRas.MaxX);
            spanGenerator.Prepare();
            if (dest.Stride / 4 > (tempSpanColors.AllocatedSize))
            {
                //if not enough -> alloc more
                tempSpanColors.Clear(dest.Stride / 4);
            }

            Color[] colorArray = tempSpanColors.Array;
            while (sclineRas.SweepScanline(scline))
            {
                //render single scanline 
                int y = scline.Y;
                int num_spans = scline.SpanCount;
                byte[] covers = scline.GetCovers();
                for (int i = 1; i <= num_spans; ++i)
                {
                    ScanlineSpan span = scline.GetSpan(i);
                    int x = span.x;
                    int span_len = span.len;
                    bool firstCoverForAll = false;
                    if (span_len < 0)
                    {
                        span_len = -span_len;
                        firstCoverForAll = true;
                    } //make absolute value

                    //1. generate colors -> store in colorArray
                    spanGenerator.GenerateColors(colorArray, 0, x, y, span_len);
                    //2. blend color in colorArray to destination image
                    dest.BlendColorHSpan(x, y, span_len,
                        colorArray, 0,
                        covers, span.cover_index,
                        firstCoverForAll);
                }
            }
        }
        protected virtual void CustomRenderSingleScanLine(
            IImageReaderWriter dest,
            Scanline scline,
            Color color)
        {
            //implement
        }
    }


    //----------------------------
    public class CustomScanlineRasToDestBitmapRenderer : ScanlineRasToDestBitmapRenderer
    {
    }


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
        GrayLevels grayLevel;
        byte[] m_primary;
        byte[] m_secondary;
        byte[] m_tertiary;

        //--------------------------------
        //coverage to primary,seconday,tertiary
        //this is my extension
        byte[] coverage_primary;
        byte[] coverage_secondary;
        byte[] coverage_tertiary;
        //--------------------------------
        public LcdDistributionLut(GrayLevels grayLevel, double prim, double second, double tert)
        {
            this.grayLevel = grayLevel;
            int numLevel = 0;
            switch (grayLevel)
            {
                default: throw new System.NotSupportedException();
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

            coverage_primary = new byte[256];
            coverage_secondary = new byte[256];
            coverage_tertiary = new byte[256];
            for (int i = 0; i < 256; ++i)
            {
                int toGreyScaleLevel = (byte)(((float)(i + 1) / 256f) * ((float)numLevel - 1));
                coverage_primary[i] = m_primary[toGreyScaleLevel];
                coverage_secondary[i] = m_secondary[toGreyScaleLevel];
                coverage_tertiary[i] = m_tertiary[toGreyScaleLevel];
            }

        }
        public byte Primary(int greyLevelIndex)
        {
            return m_primary[greyLevelIndex];
        }
        public byte Secondary(int greyLevelIndex)
        {
            return m_secondary[greyLevelIndex];
        }
        public byte Tertiary(int greyLevelIndex)
        {
            return m_tertiary[greyLevelIndex];
        }

        //
        public byte PrimaryFromCoverage(int coverage)
        {
            return coverage_primary[coverage];
        }

        public byte SecondayFromCoverage(int coverage)
        {
            return coverage_secondary[coverage];
        }
        public byte TertiaryFromCoverage(int coverage)
        {
            return coverage_tertiary[coverage];
        }


        public static readonly LcdDistributionLut Lut8_1_2 = new LcdDistributionLut(GrayLevels.Gray8, 1f / 2f, 1f / 4f, 1f / 8f);
        public static readonly LcdDistributionLut Lut8_1_3 = new LcdDistributionLut(GrayLevels.Gray8, 1f / 3f, 2f / 9f, 1f / 9f);
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
