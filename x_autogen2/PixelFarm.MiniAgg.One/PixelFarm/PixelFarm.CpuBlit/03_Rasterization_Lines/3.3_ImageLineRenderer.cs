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
#if true
using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.PixelProcessing;
namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    /*
    //========================================================line_image_scale
    public class line_image_scale
    {
        IImage m_source;
        double        m_height;
        double        m_scale;

        public line_image_scale(IImage src, double height)
        {
            m_source = (src);
            m_height = (height);
            m_scale = (src.height() / height);
        }

        public double width()  { return m_source.width(); }
        public double height() { return m_height; }

        public RGBA_Bytes pixel(int x, int y) 
        { 
            double src_y = (y + 0.5) * m_scale - 0.5;
            int h  = m_source.height() - 1;
            int y1 = ufloor(src_y);
            int y2 = y1 + 1;
            RGBA_Bytes pix1 = (y1 < 0) ? new no_color() : m_source.pixel(x, y1);
            RGBA_Bytes pix2 = (y2 > h) ? no_color() : m_source.pixel(x, y2);
            return pix1.gradient(pix2, src_y - y1);
        }
    };

     */

    //======================================================line_image_pattern
    public class LineImagePattern
    {
        IPatternFilter m_filter;
        int m_dilation;
        int m_dilation_hr;
        PixelProcessing.SubBitmapBlender m_buf;
        int[] m_data = null;
        int m_DataSizeInBytes = 0;
        int m_width;
        int m_height;
        int m_width_hr;
        int m_half_height_hr;
        int m_offset_y_hr;
        //--------------------------------------------------------------------
        public LineImagePattern(IPatternFilter filter)
        {
            m_filter = filter;
            m_dilation = (filter.Dilation + 1);
            m_dilation_hr = (m_dilation << LineAA.SUBPIXEL_SHIFT);
            m_width = (0);
            m_height = (0);
            m_width_hr = (0);
            m_half_height_hr = (0);
            m_offset_y_hr = (0);
        }
        public BitmapBlenderBase MyBuffer
        {
            get
            {
                throw new NotSupportedException();
            }
        }
        ~LineImagePattern()
        {
            //TODO: review here, remove finalizer
            if (m_DataSizeInBytes > 0)
            {
                m_data = null;
            }
        }

        // Create
        //--------------------------------------------------------------------
        public LineImagePattern(IPatternFilter filter, LineImagePattern src)
        {
            m_filter = (filter);
            m_dilation = (filter.Dilation + 1);
            m_dilation_hr = (m_dilation << LineAA.SUBPIXEL_SHIFT);
            m_width = 0;
            m_height = 0;
            m_width_hr = 0;
            m_half_height_hr = 0;
            m_offset_y_hr = (0);
            Create(src.MyBuffer);
        }

        // Create
        //--------------------------------------------------------------------
        public void Create(IBitmapSrc src)
        {
            // we are going to create a dialated image for filtering
            // we add m_dilation pixels to every side of the image and then copy the image in the x
            // dirrection into each end so that we can sample into this image to get filtering on x repeating
            // if the original image look like this
            //
            // 123456 
            //
            // the new image would look like this
            //
            // 0000000000
            // 0000000000
            // 5612345612
            // 0000000000
            // 0000000000

            m_height = (int)AggMath.uceil(src.Height);
            m_width = (int)AggMath.uceil(src.Width);
            m_width_hr = (int)AggMath.uround(src.Width * LineAA.SUBPIXEL_SCALE);
            m_half_height_hr = (int)AggMath.uround(src.Height * LineAA.SUBPIXEL_SCALE / 2);
            m_offset_y_hr = m_dilation_hr + m_half_height_hr - LineAA.SUBPIXEL_SCALE / 2;
            m_half_height_hr += LineAA.SUBPIXEL_SCALE / 2;
            int bufferWidth = m_width + m_dilation * 2;
            int bufferHeight = m_height + m_dilation * 2;
            int bytesPerPixel = src.BitDepth / 8;
            int newSizeInBytes = bufferWidth * bufferHeight * bytesPerPixel;
            if (m_DataSizeInBytes < newSizeInBytes)
            {
                m_DataSizeInBytes = newSizeInBytes;
                m_data = new int[m_DataSizeInBytes / 4];
            }


            m_buf = new PixelProcessing.SubBitmapBlender(m_data, 0, bufferWidth, bufferHeight, bufferWidth * bytesPerPixel, src.BitDepth, bytesPerPixel);

            unsafe
            {
                CpuBlit.Imaging.TempMemPtr destMemPtr = m_buf.GetBufferPtr();
                CpuBlit.Imaging.TempMemPtr srcMemPtr = src.GetBufferPtr();

                int* destBuffer = (int*)destMemPtr.Ptr;
                int* srcBuffer = (int*)srcMemPtr.Ptr;
                // copy the image into the middle of the dest
                for (int y = 0; y < m_height; y++)
                {
                    for (int x = 0; x < m_width; x++)
                    {
                        int sourceOffset = src.GetBufferOffsetXY32(x, y);
                        int destOffset = m_buf.GetBufferOffsetXY32(m_dilation, y + m_dilation);
                        destBuffer[destOffset] = srcBuffer[sourceOffset]; 
                    }
                }

                // copy the first two pixels form the end into the begining and from the begining into the end
                for (int y = 0; y < m_height; y++)
                {
                    int s1Offset = src.GetBufferOffsetXY32(0, y);
                    int d1Offset = m_buf.GetBufferOffsetXY32(m_dilation + m_width, y);
                    int s2Offset = src.GetBufferOffsetXY32(m_width - m_dilation, y);
                    int d2Offset = m_buf.GetBufferOffsetXY32(0, y);

                    for (int x = 0; x < m_dilation; x++)
                    {
                        destBuffer[d1Offset++] = srcBuffer[s1Offset++];
                        destBuffer[d2Offset++] = srcBuffer[s2Offset++]; 
                    }
                }

                srcMemPtr.Release();
                destMemPtr.Release();
            }



        }

        //--------------------------------------------------------------------
        public int PatternWidth { get { return m_width_hr; } }
        public int LineWidth { get { return m_half_height_hr; } }
        public double Width { get { return m_height; } }

        //--------------------------------------------------------------------
        public void Pixel(Color[] destBuffer, int destBufferOffset, int x, int y)
        {
            m_filter.SetPixelHighRes(m_buf, destBuffer, destBufferOffset,
                                     x % m_width_hr + m_dilation_hr,
                                     y + m_offset_y_hr);
        }

        //--------------------------------------------------------------------
        public IPatternFilter PatternFilter { get { return m_filter; } }
    }

    /*

    //=================================================line_image_pattern_pow2
    public class line_image_pattern_pow2 : 
        line_image_pattern<IPatternFilter>
    {
        uint m_mask;

        //--------------------------------------------------------------------
        public line_image_pattern_pow2(IPatternFilter filter) :
            line_image_pattern<IPatternFilter>(filter), m_mask(line_subpixel_mask) {}

        //--------------------------------------------------------------------
        public line_image_pattern_pow2(IPatternFilter filter, ImageBuffer src) :
            line_image_pattern<IPatternFilter>(filter), m_mask(line_subpixel_mask)
        {
            create(src);
        }

        //--------------------------------------------------------------------
        public void create(ImageBuffer src)
        {
            line_image_pattern<IPatternFilter>::create(src);
            m_mask = 1;
            while(m_mask < base_type::m_width) 
            {
                m_mask <<= 1;
                m_mask |= 1;
            }
            m_mask <<= line_subpixel_shift - 1;
            m_mask |=  line_subpixel_mask;
            base_type::m_width_hr = m_mask + 1;
        }

        //--------------------------------------------------------------------
        public void pixel(RGBA_Bytes* p, int x, int y)
        {
            base_type::m_filter->pixel_high_res(
                    base_type::m_buf.rows(), 
                    p,
                    (x & m_mask) + base_type::m_dilation_hr,
                    y + base_type::m_offset_y_hr);
        }
    };
     */

    //===================================================distance_interpolator4


#if true
#if false
    //==================================================line_interpolator_image
    public class line_interpolator_image
    {
        line_parameters m_lp;
        dda2_line_interpolator m_li;
        distance_interpolator4 m_di; 
        IImageByte m_ren;
        int m_plen;
        int m_x;
        int m_y;
        int m_old_x;
        int m_old_y;
        int m_width;
        int m_max_extent;
        int m_start;
        int m_step;
        int[] m_dist_pos = new int[max_half_width + 1];
        RGBA_Bytes[] m_colors = new RGBA_Bytes[max_half_width * 2 + 4];

        //---------------------------------------------------------------------
        public const int max_half_width = 64;

        //---------------------------------------------------------------------
        public line_interpolator_image(renderer_outline_aa ren, line_parameters lp,
                                int sx, int sy, int ex, int ey, 
                                int pattern_start,
                                double scale_x)
        {
            throw new NotImplementedException();
/*
            m_lp=(lp);
            m_li = new dda2_line_interpolator(lp.vertical ? LineAABasics.line_dbl_hr(lp.x2 - lp.x1) :
                               LineAABasics.line_dbl_hr(lp.y2 - lp.y1),
                 lp.vertical ? Math.Abs(lp.y2 - lp.y1) :
                               Math.Abs(lp.x2 - lp.x1) + 1);
            m_di = new distance_interpolator4(lp.x1, lp.y1, lp.x2, lp.y2, sx, sy, ex, ey, lp.len, scale_x,
                 lp.x1 & ~LineAABasics.line_subpixel_mask, lp.y1 & ~LineAABasics.line_subpixel_mask);
            m_ren=ren;
            m_x = (lp.x1 >> LineAABasics.line_subpixel_shift);
            m_y = (lp.y1 >> LineAABasics.line_subpixel_shift);
            m_old_x=(m_x);
            m_old_y=(m_y);
            m_count = ((lp.vertical ? Math.Abs((lp.y2 >> LineAABasics.line_subpixel_shift) - m_y) :
                                   Math.Abs((lp.x2 >> LineAABasics.line_subpixel_shift) - m_x)));
            m_width=(ren.subpixel_width());
            //m_max_extent(m_width >> (LineAABasics.line_subpixel_shift - 2));
            m_max_extent = ((m_width + LineAABasics.line_subpixel_scale) >> LineAABasics.line_subpixel_shift);
            m_start=(pattern_start + (m_max_extent + 2) * ren.pattern_width());
            m_step=(0);

            dda2_line_interpolator li = new dda2_line_interpolator(0, lp.vertical ? 
                                              (lp.dy << LineAABasics.line_subpixel_shift) :
                                              (lp.dx << LineAABasics.line_subpixel_shift),
                                           lp.len);

            uint i;
            int stop = m_width + LineAABasics.line_subpixel_scale * 2;
            for(i = 0; i < max_half_width; ++i)
            {
                m_dist_pos[i] = li.y();
                if(m_dist_pos[i] >= stop) break;
                ++li;
            }
            m_dist_pos[i] = 0x7FFF0000;

            int dist1_start;
            int dist2_start;
            int npix = 1;

            if(lp.vertical)
            {
                do
                {
                    --m_li;
                    m_y -= lp.inc;
                    m_x = (m_lp.x1 + m_li.y()) >> LineAABasics.line_subpixel_shift;

                    if(lp.inc > 0) m_di.dec_y(m_x - m_old_x);
                    else           m_di.inc_y(m_x - m_old_x);

                    m_old_x = m_x;

                    dist1_start = dist2_start = m_di.dist_start(); 

                    int dx = 0;
                    if(dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start += m_di.dy_start();
                        dist2_start -= m_di.dy_start();
                        if(dist1_start < 0) ++npix;
                        if(dist2_start < 0) ++npix;
                        ++dx;
                    }
                    while(m_dist_pos[dx] <= m_width);
                    if(npix == 0) break;

                    npix = 0;
                }
                while(--m_step >= -m_max_extent);
            }
            else
            {
                do
                {
                    --m_li;

                    m_x -= lp.inc;
                    m_y = (m_lp.y1 + m_li.y()) >> LineAABasics.line_subpixel_shift;

                    if(lp.inc > 0) m_di.dec_x(m_y - m_old_y);
                    else           m_di.inc_x(m_y - m_old_y);

                    m_old_y = m_y;

                    dist1_start = dist2_start = m_di.dist_start(); 

                    int dy = 0;
                    if(dist1_start < 0) ++npix;
                    do
                    {
                        dist1_start -= m_di.dx_start();
                        dist2_start += m_di.dx_start();
                        if(dist1_start < 0) ++npix;
                        if(dist2_start < 0) ++npix;
                        ++dy;
                    }
                    while(m_dist_pos[dy] <= m_width);
                    if(npix == 0) break;

                    npix = 0;
                }
                while(--m_step >= -m_max_extent);
            }
            m_li.adjust_forward();
            m_step -= m_max_extent;
 */
        }

        //---------------------------------------------------------------------
        public bool step_hor()
        {
            throw new NotImplementedException();
/*
            ++m_li;
            m_x += m_lp.inc;
            m_y = (m_lp.y1 + m_li.y()) >> LineAABasics.line_subpixel_shift;

            if(m_lp.inc > 0) m_di.inc_x(m_y - m_old_y);
            else             m_di.dec_x(m_y - m_old_y);

            m_old_y = m_y;

            int s1 = m_di.dist() / m_lp.len;
            int s2 = -s1;

            if(m_lp.inc < 0) s1 = -s1;

            int dist_start;
            int dist_pict;
            int dist_end;
            int dy;
            int dist;

            dist_start = m_di.dist_start();
            dist_pict  = m_di.dist_pict() + m_start;
            dist_end   = m_di.dist_end();
            RGBA_Bytes* p0 = m_colors + max_half_width + 2;
            RGBA_Bytes* p1 = p0;

            int npix = 0;
            p1->clear();
            if(dist_end > 0)
            {
                if(dist_start <= 0)
                {
                    m_ren.pixel(p1, dist_pict, s2);
                }
                ++npix;
            }
            ++p1;

            dy = 1;
            while((dist = m_dist_pos[dy]) - s1 <= m_width)
            {
                dist_start -= m_di.dx_start();
                dist_pict  -= m_di.dx_pict();
                dist_end   -= m_di.dx_end();
                p1->clear();
                if(dist_end > 0 && dist_start <= 0)
                {   
                    if(m_lp.inc > 0) dist = -dist;
                    m_ren.pixel(p1, dist_pict, s2 - dist);
                    ++npix;
                }
                ++p1;
                ++dy;
            }

            dy = 1;
            dist_start = m_di.dist_start();
            dist_pict  = m_di.dist_pict() + m_start;
            dist_end   = m_di.dist_end();
            while((dist = m_dist_pos[dy]) + s1 <= m_width)
            {
                dist_start += m_di.dx_start();
                dist_pict  += m_di.dx_pict();
                dist_end   += m_di.dx_end();
                --p0;
                p0->clear();
                if(dist_end > 0 && dist_start <= 0)
                {   
                    if(m_lp.inc > 0) dist = -dist;
                    m_ren.pixel(p0, dist_pict, s2 + dist);
                    ++npix;
                }
                ++dy;
            }
            m_ren.blend_color_vspan(m_x, 
                                    m_y - dy + 1, 
                                    (uint)(p1 - p0), 
                                    p0); 
            return npix && ++m_step < m_count;
 */
        }

        //---------------------------------------------------------------------
        public bool step_ver()
        {
            throw new NotImplementedException();
/*
            ++m_li;
            m_y += m_lp.inc;
            m_x = (m_lp.x1 + m_li.y()) >> LineAABasics.line_subpixel_shift;

            if(m_lp.inc > 0) m_di.inc_y(m_x - m_old_x);
            else             m_di.dec_y(m_x - m_old_x);

            m_old_x = m_x;

            int s1 = m_di.dist() / m_lp.len;
            int s2 = -s1;

            if(m_lp.inc > 0) s1 = -s1;

            int dist_start;
            int dist_pict;
            int dist_end;
            int dist;
            int dx;

            dist_start = m_di.dist_start();
            dist_pict  = m_di.dist_pict() + m_start;
            dist_end   = m_di.dist_end();
            RGBA_Bytes* p0 = m_colors + max_half_width + 2;
            RGBA_Bytes* p1 = p0;

            int npix = 0;
            p1->clear();
            if(dist_end > 0)
            {
                if(dist_start <= 0)
                {
                    m_ren.pixel(p1, dist_pict, s2);
                }
                ++npix;
            }
            ++p1;

            dx = 1;
            while((dist = m_dist_pos[dx]) - s1 <= m_width)
            {
                dist_start += m_di.dy_start();
                dist_pict  += m_di.dy_pict();
                dist_end   += m_di.dy_end();
                p1->clear();
                if(dist_end > 0 && dist_start <= 0)
                {   
                    if(m_lp.inc > 0) dist = -dist;
                    m_ren.pixel(p1, dist_pict, s2 + dist);
                    ++npix;
                }
                ++p1;
                ++dx;
            }

            dx = 1;
            dist_start = m_di.dist_start();
            dist_pict  = m_di.dist_pict() + m_start;
            dist_end   = m_di.dist_end();
            while((dist = m_dist_pos[dx]) + s1 <= m_width)
            {
                dist_start -= m_di.dy_start();
                dist_pict  -= m_di.dy_pict();
                dist_end   -= m_di.dy_end();
                --p0;
                p0->clear();
                if(dist_end > 0 && dist_start <= 0)
                {   
                    if(m_lp.inc > 0) dist = -dist;
                    m_ren.pixel(p0, dist_pict, s2 - dist);
                    ++npix;
                }
                ++dx;
            }
            m_ren.blend_color_hspan(m_x - dx + 1, 
                                    m_y, 
                                    (uint)(p1 - p0), 
                                    p0);
            return npix && ++m_step < m_count;
 */
        }


        //---------------------------------------------------------------------
        public int  pattern_end() { return m_start + m_di.len(); }

        //---------------------------------------------------------------------
        public bool vertical() { return m_lp.vertical; }
        public int  width() { return m_width; }
    }
#endif

    //===================================================renderer_outline_image
    //template<class BaseRenderer, class ImagePattern> 
    public class ImageLineRenderer : LineRenderer
    {
        PixelProcessing.IBitmapBlender m_ren;
        LineImagePattern m_pattern;
        int m_start;
        double m_scale_x;
        RectInt m_clip_box;
        bool m_clipping;
        //---------------------------------------------------------------------
        //typedef renderer_outline_image<BaseRenderer, ImagePattern> self_type;

        //---------------------------------------------------------------------
        public ImageLineRenderer(PixelProcessing.IBitmapBlender ren, LineImagePattern patt)
        {
            m_ren = ren;
            m_pattern = patt;
            m_start = (0);
            m_scale_x = (1.0);
            m_clip_box = new RectInt(0, 0, 0, 0);
            m_clipping = (false);
        }

        public void Attach(PixelProcessing.IBitmapBlender ren) { m_ren = ren; }

        //---------------------------------------------------------------------
        public LineImagePattern Pattern
        {
            get { return this.m_pattern; }
            set { m_pattern = value; }
        }


        //---------------------------------------------------------------------
        public void ResetClipping() { m_clipping = false; }

        public void SetClipBox(double x1, double y1, double x2, double y2)
        {
            m_clip_box.Left = LineCoordSat.Convert(x1);
            m_clip_box.Bottom = LineCoordSat.Convert(y1);
            m_clip_box.Right = LineCoordSat.Convert(x2);
            m_clip_box.Top = LineCoordSat.Convert(y2);
            m_clipping = true;
        }

        //---------------------------------------------------------------------
        public double ScaleX
        {
            get { return this.m_scale_x; }
            set { this.m_scale_x = value; }
        }
        public double StartX
        {
            get { return (double)(m_start) / LineAA.SUBPIXEL_SCALE; }
            set { m_start = AggMath.iround(value * LineAA.SUBPIXEL_SCALE); }
        }


        //---------------------------------------------------------------------
        public int SubPixelWidth { get { return m_pattern.LineWidth; } }
        public int PatternWidth { get { return m_pattern.PatternWidth; } }

        public double Width { get { return (double)(SubPixelWidth) / LineAA.SUBPIXEL_SCALE; } }

        public void Pixel(Color[] p, int offset, int x, int y)
        {
            throw new NotImplementedException();
            //m_pattern.pixel(p, x, y);
        }

        public void BlendColorHSpan(int x, int y, uint len, Color[] colors, int colorsOffset)
        {
            throw new NotImplementedException();
            //            m_ren.blend_color_hspan(x, y, len, colors, null, 0);
        }

        public void BlendColorVSpan(int x, int y, uint len, Color[] colors, int colorsOffset)
        {
            throw new NotImplementedException();
            //            m_ren.blend_color_vspan(x, y, len, colors, null, 0);
        }

        public static bool AccurateJoinOnly { get { return true; } }

        public override void SemiDot(CompareFunction cmp, int xc1, int yc1, int xc2, int yc2)
        {
        }

        public override void SemiDotHLine(CompareFunction cmp,
                           int xc1, int yc1, int xc2, int yc2,
                           int x1, int y1, int x2)
        {
        }

        public override void Pie(int xc, int yc, int x1, int y1, int x2, int y2)
        {
        }

        public override void Line0(LineParameters lp)
        {
        }

        public override void Line1(LineParameters lp, int sx, int sy)
        {
        }

        public override void Line2(LineParameters lp, int ex, int ey)
        {
        }

        public void Line3NoClip(LineParameters lp,
                           int sx, int sy, int ex, int ey)
        {
            throw new NotImplementedException();
            /*
                        if(lp.len > LineAABasics.line_max_length)
                        {
                            line_parameters lp1, lp2;
                            lp.divide(lp1, lp2);
                            int mx = lp1.x2 + (lp1.y2 - lp1.y1);
                            int my = lp1.y2 - (lp1.x2 - lp1.x1);
                            line3_no_clip(lp1, (lp.x1 + sx) >> 1, (lp.y1 + sy) >> 1, mx, my);
                            line3_no_clip(lp2, mx, my, (lp.x2 + ex) >> 1, (lp.y2 + ey) >> 1);
                            return;
                        }

                        LineAABasics.fix_degenerate_bisectrix_start(lp, ref sx, ref sy);
                        LineAABasics.fix_degenerate_bisectrix_end(lp, ref ex, ref ey);
                        line_interpolator_image li = new line_interpolator_image(this, lp, 
                                                              sx, sy, 
                                                              ex, ey, 
                                                              m_start, m_scale_x);
                        if(li.vertical())
                        {
                            while(li.step_ver());
                        }
                        else
                        {
                            while(li.step_hor());
                        }
                        m_start += uround(lp.len / m_scale_x);
             */
        }

        public override void Line3(LineParameters lp,
                   int sx, int sy, int ex, int ey)
        {
            throw new NotImplementedException();
            /*
                        if(m_clipping)
                        {
                            int x1 = lp.x1;
                            int y1 = lp.y1;
                            int x2 = lp.x2;
                            int y2 = lp.y2;
                            uint flags = clip_line_segment(&x1, &y1, &x2, &y2, m_clip_box);
                            int start = m_start;
                            if((flags & 4) == 0)
                            {
                                if(flags)
                                {
                                    line_parameters lp2(x1, y1, x2, y2, 
                                                       uround(calc_distance(x1, y1, x2, y2)));
                                    if(flags & 1)
                                    {
                                        m_start += uround(calc_distance(lp.x1, lp.y1, x1, y1) / m_scale_x);
                                        sx = x1 + (y2 - y1); 
                                        sy = y1 - (x2 - x1);
                                    }
                                    else
                                    {
                                        while(Math.Abs(sx - lp.x1) + Math.Abs(sy - lp.y1) > lp2.len)
                                        {
                                            sx = (lp.x1 + sx) >> 1;
                                            sy = (lp.y1 + sy) >> 1;
                                        }
                                    }
                                    if(flags & 2)
                                    {
                                        ex = x2 + (y2 - y1); 
                                        ey = y2 - (x2 - x1);
                                    }
                                    else
                                    {
                                        while(Math.Abs(ex - lp.x2) + Math.Abs(ey - lp.y2) > lp2.len)
                                        {
                                            ex = (lp.x2 + ex) >> 1;
                                            ey = (lp.y2 + ey) >> 1;
                                        }
                                    }
                                    line3_no_clip(lp2, sx, sy, ex, ey);
                                }
                                else
                                {
                                    line3_no_clip(lp, sx, sy, ex, ey);
                                }
                            }
                            m_start = start + uround(lp.len / m_scale_x);
                        }
                        else
                        {
                            line3_no_clip(lp, sx, sy, ex, ey);
                        }
             */
        }
    }
#endif
}
#endif