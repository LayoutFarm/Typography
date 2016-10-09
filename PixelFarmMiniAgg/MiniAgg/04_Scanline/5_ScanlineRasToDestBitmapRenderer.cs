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

using PixelFarm.Drawing;
using PixelFarm.Agg.Image;
namespace PixelFarm.Agg
{
    public enum ScanlineRenderMode
    {
        Default,
        Custom,
        SubPixelRendering
    }

    /// <summary>
    /// to bitmap
    /// </summary>  
    public class ScanlineRasToDestBitmapRenderer
    {
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
        void SubPixRender(IImageReaderWriter dest, Scanline scanline, Color color)
        {
            byte[] covers = scanline.GetCovers();
            int num_spans = scanline.SpanCount;
            int y = scanline.Y;
            byte[] buffer = dest.GetBuffer();
            IPixelBlender blender = dest.GetRecieveBlender();
            int last_x = int.MinValue;
            int bufferOffset = 0;
            //------------------------------------------
            Color bgColor = Color.White;
            float cb_R = bgColor.R / 255f;
            float cb_G = bgColor.G / 255f;
            float cb_B = bgColor.B / 255f;
            float cf_R = color.R / 255f;
            float cf_G = color.G / 255f;
            float cf_B = color.B / 255f;
            //------------------------------------------
            int prevCover = -1;
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
                    //------------------------------------------- 
                    if (coverageValue >= 255)
                    {
                        //100% cover
                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                        Color todrawColor = Color.FromArgb(a, Color.FromArgb(color.R, color.G, color.B));
                        while (num_pix > 0)
                        {
                            blender.BlendPixel(buffer, bufferOffset, todrawColor);
                            bufferOffset += 4; //1 pixel 4 bytes
                            --num_pix;
                        }
                    }
                    else
                    {
                        int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                        Color newc = Color.FromArgb(color.R, color.G, color.B);
                        Color todrawColor = Color.FromArgb(a, newc);
                        while (num_pix > 0)
                        {
                            blender.BlendPixel(buffer, bufferOffset, todrawColor);
                            bufferOffset += 4; //1 pixel 4 bytes
                            --num_pix;
                        }
                    }
                    prevCover = coverageValue;
                }
                else
                {
                    int coverIndex = span.cover_index;
                    last_x += (num_pix - 1);
                    while (num_pix > 0)
                    {
                        int coverageValue = covers[coverIndex++];
                        if (coverageValue >= 255)
                        {
                            //100% cover
                            Color newc = Color.FromArgb(color.R, color.G, color.B);
                            int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                            blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, newc));
                            prevCover = coverageValue;
                        }
                        else
                        {
                            //check direction : 

                            bool isUpHill = coverageValue >= prevCover;
                            //if (isUpHill != ((coverageValue % 2) > 0))
                            //{
                            //}
                            //---------------------------- 
                            byte c_r = 0, c_g = 0, c_b = 0;
                            //----------------------------
                            //assume lcd screen is RGB
                            float subpix_percent = ((float)(coverageValue) / 256f);
                            if (coverageValue < cover_1_3)
                            {
                                //assume LCD color arrangement is BGR                            
                                if (isUpHill)
                                {
                                    c_r = bgColor.R;
                                    c_g = bgColor.G;
                                    c_b = (byte)(mix(cb_B, cf_B, subpix_percent) * 255);
                                }
                                else
                                {
                                    c_r = (byte)(mix(cb_R, cf_R, subpix_percent) * 255);
                                    c_g = bgColor.G;
                                    c_b = bgColor.B;
                                }

                                int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                                blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
                            }
                            else if (coverageValue < cover_2_3)
                            {
                                if (isUpHill)
                                {
                                    c_r = bgColor.R;
                                    c_g = (byte)(mix(cb_G, cf_G, subpix_percent) * 255);
                                    c_b = (byte)(mix(cb_B, cf_B, 1) * 255);
                                }
                                else
                                {
                                    c_r = (byte)(mix(cb_R, cf_R, 1) * 255);
                                    c_g = (byte)(mix(cb_G, cf_G, subpix_percent) * 255);
                                    c_b = bgColor.B;
                                }

                                int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                                blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
                            }
                            else
                            {
                                //cover > 2/3 but not full 
                                if (isUpHill)
                                {
                                    c_r = (byte)(mix(cb_R, cf_R, subpix_percent) * 255);
                                    c_g = (byte)(mix(cb_G, cf_G, 1) * 255);
                                    c_b = (byte)(mix(cb_B, cf_B, 1) * 255);
                                }
                                else
                                {
                                    c_r = (byte)(mix(cb_R, cf_R, 1) * 255);
                                    c_g = (byte)(mix(cb_G, cf_G, 1) * 255);
                                    c_b = (byte)(mix(cb_B, cf_B, subpix_percent) * 255);
                                }

                                int a = ((coverageValue + 1) * color.Alpha0To255) >> 8;
                                blender.BlendPixel(buffer, bufferOffset, Color.FromArgb(a, Color.FromArgb(c_r, c_g, c_b)));
                            }
                        }
                        bufferOffset += 4; //1 pixel 4 bits 
                        --num_pix;
                        prevCover = coverageValue;
                    }
                }
            }
        }
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
}
