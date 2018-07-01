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
// The Stack Blur Algorithm was invented by Mario Klingemann, 
// mario@quasimondo.com and described here:
// http://incubator.quasimondo.com/processing/fast_blur_deluxe.php
// (search phrase "Stackblur: Fast But Goodlooking"). 
// The major improvement is that there's no more division table
// that was very expensive to create for large blur radii. Instead, 
// for 8-bit per channel and radius not exceeding 254 the division is 
// replaced by multiplication and shift. 
//
//----------------------------------------------------------------------------

using System;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit.PixelProcessing;

namespace PixelFarm.CpuBlit.Imaging
{
    //==============================================================stack_blur
    public class StackBlur
    {
        public void Blur(BitmapBlenderBase img, int rx, int ry)
        {
            switch (img.BitDepth)
            {
                case 24:
                    StackBlurRGB24(img, rx, ry);
                    break;
                case 32:
                    StackBlurRGBA32(img, rx, ry);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        void StackBlurRGB24(BitmapBlenderBase img, int rx, int ry)
        {
            throw new NotImplementedException();
#if false
            //typedef typename Img::color_type color_type;
            //typedef typename Img::order_type order_type;

            int x, y, xp, yp, i;
            int stack_ptr;
            int stack_start;

            byte* src_pix_ptr;
                  byte* dst_pix_ptr;
            color_type*  stack_pix_ptr;

            int sum_r;
            int sum_g;
            int sum_b;
            int sum_in_r;
            int sum_in_g;
            int sum_in_b;
            int sum_out_r;
            int sum_out_g;
            int sum_out_b;

            int w   = img.width();
            int h   = img.height();
            int wm  = w - 1;
            int hm  = h - 1;

            int div;
            int mul_sum;
            int shr_sum;

            pod_vector<color_type> stack;

            if(rx > 0)
            {
                if(rx > 254) rx = 254;
                div = rx * 2 + 1;
                mul_sum = stack_blur_tables.g_stack_blur8_mul[rx];
                shr_sum = stack_blur_tables.g_stack_blur8_shr[rx];
                stack.allocate(div);

                for(y = 0; y < h; y++)
                {
                    sum_r = 
                    sum_g = 
                    sum_b = 
                    sum_in_r = 
                    sum_in_g = 
                    sum_in_b = 
                    sum_out_r = 
                    sum_out_g = 
                    sum_out_b = 0;

                    src_pix_ptr = img.pix_ptr(0, y);
                    for(i = 0; i <= rx; i++)
                    {
                        stack_pix_ptr    = &stack[i];
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
                        sum_r           += src_pix_ptr[R] * (i + 1);
                        sum_g           += src_pix_ptr[G] * (i + 1);
                        sum_b           += src_pix_ptr[B] * (i + 1);
                        sum_out_r       += src_pix_ptr[R];
                        sum_out_g       += src_pix_ptr[G];
                        sum_out_b       += src_pix_ptr[B];
                    }
                    for(i = 1; i <= rx; i++)
                    {
                        if(i <= wm) src_pix_ptr += Img::pix_width; 
                        stack_pix_ptr = &stack[i + rx];
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
                        sum_r           += src_pix_ptr[R] * (rx + 1 - i);
                        sum_g           += src_pix_ptr[G] * (rx + 1 - i);
                        sum_b           += src_pix_ptr[B] * (rx + 1 - i);
                        sum_in_r        += src_pix_ptr[R];
                        sum_in_g        += src_pix_ptr[G];
                        sum_in_b        += src_pix_ptr[B];
                    }

                    stack_ptr = rx;
                    xp = rx;
                    if(xp > wm) xp = wm;
                    src_pix_ptr = img.pix_ptr(xp, y);
                    dst_pix_ptr = img.pix_ptr(0, y);
                    for(x = 0; x < w; x++)
                    {
                        dst_pix_ptr[R] = (sum_r * mul_sum) >> shr_sum;
                        dst_pix_ptr[G] = (sum_g * mul_sum) >> shr_sum;
                        dst_pix_ptr[B] = (sum_b * mul_sum) >> shr_sum;
                        dst_pix_ptr   += Img::pix_width;

                        sum_r -= sum_out_r;
                        sum_g -= sum_out_g;
                        sum_b -= sum_out_b;
           
                        stack_start = stack_ptr + div - rx;
                        if(stack_start >= div) stack_start -= div;
                        stack_pix_ptr = &stack[stack_start];

                        sum_out_r -= stack_pix_ptr->r;
                        sum_out_g -= stack_pix_ptr->g;
                        sum_out_b -= stack_pix_ptr->b;

                        if(xp < wm) 
                        {
                            src_pix_ptr += Img::pix_width;
                            ++xp;
                        }
            
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
            
                        sum_in_r += src_pix_ptr[R];
                        sum_in_g += src_pix_ptr[G];
                        sum_in_b += src_pix_ptr[B];
                        sum_r    += sum_in_r;
                        sum_g    += sum_in_g;
                        sum_b    += sum_in_b;
            
                        ++stack_ptr;
                        if(stack_ptr >= div) stack_ptr = 0;
                        stack_pix_ptr = &stack[stack_ptr];

                        sum_out_r += stack_pix_ptr->r;
                        sum_out_g += stack_pix_ptr->g;
                        sum_out_b += stack_pix_ptr->b;
                        sum_in_r  -= stack_pix_ptr->r;
                        sum_in_g  -= stack_pix_ptr->g;
                        sum_in_b  -= stack_pix_ptr->b;
                    }
                }
            }

            if(ry > 0)
            {
                if(ry > 254) ry = 254;
                div = ry * 2 + 1;
                mul_sum = stack_blur_tables.g_stack_blur8_mul[ry];
                shr_sum = stack_blur_tables.g_stack_blur8_shr[ry];
                stack.allocate(div);

                int stride = img.stride();
                for(x = 0; x < w; x++)
                {
                    sum_r = 
                    sum_g = 
                    sum_b = 
                    sum_in_r = 
                    sum_in_g = 
                    sum_in_b = 
                    sum_out_r = 
                    sum_out_g = 
                    sum_out_b = 0;

                    src_pix_ptr = img.pix_ptr(x, 0);
                    for(i = 0; i <= ry; i++)
                    {
                        stack_pix_ptr    = &stack[i];
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
                        sum_r           += src_pix_ptr[R] * (i + 1);
                        sum_g           += src_pix_ptr[G] * (i + 1);
                        sum_b           += src_pix_ptr[B] * (i + 1);
                        sum_out_r       += src_pix_ptr[R];
                        sum_out_g       += src_pix_ptr[G];
                        sum_out_b       += src_pix_ptr[B];
                    }
                    for(i = 1; i <= ry; i++)
                    {
                        if(i <= hm) src_pix_ptr += stride; 
                        stack_pix_ptr = &stack[i + ry];
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
                        sum_r           += src_pix_ptr[R] * (ry + 1 - i);
                        sum_g           += src_pix_ptr[G] * (ry + 1 - i);
                        sum_b           += src_pix_ptr[B] * (ry + 1 - i);
                        sum_in_r        += src_pix_ptr[R];
                        sum_in_g        += src_pix_ptr[G];
                        sum_in_b        += src_pix_ptr[B];
                    }

                    stack_ptr = ry;
                    yp = ry;
                    if(yp > hm) yp = hm;
                    src_pix_ptr = img.pix_ptr(x, yp);
                    dst_pix_ptr = img.pix_ptr(x, 0);
                    for(y = 0; y < h; y++)
                    {
                        dst_pix_ptr[R] = (sum_r * mul_sum) >> shr_sum;
                        dst_pix_ptr[G] = (sum_g * mul_sum) >> shr_sum;
                        dst_pix_ptr[B] = (sum_b * mul_sum) >> shr_sum;
                        dst_pix_ptr += stride;

                        sum_r -= sum_out_r;
                        sum_g -= sum_out_g;
                        sum_b -= sum_out_b;
           
                        stack_start = stack_ptr + div - ry;
                        if(stack_start >= div) stack_start -= div;

                        stack_pix_ptr = &stack[stack_start];
                        sum_out_r -= stack_pix_ptr->r;
                        sum_out_g -= stack_pix_ptr->g;
                        sum_out_b -= stack_pix_ptr->b;

                        if(yp < hm) 
                        {
                            src_pix_ptr += stride;
                            ++yp;
                        }
            
                        stack_pix_ptr->r = src_pix_ptr[R];
                        stack_pix_ptr->g = src_pix_ptr[G];
                        stack_pix_ptr->b = src_pix_ptr[B];
            
                        sum_in_r += src_pix_ptr[R];
                        sum_in_g += src_pix_ptr[G];
                        sum_in_b += src_pix_ptr[B];
                        sum_r    += sum_in_r;
                        sum_g    += sum_in_g;
                        sum_b    += sum_in_b;
            
                        ++stack_ptr;
                        if(stack_ptr >= div) stack_ptr = 0;
                        stack_pix_ptr = &stack[stack_ptr];

                        sum_out_r += stack_pix_ptr->r;
                        sum_out_g += stack_pix_ptr->g;
                        sum_out_b += stack_pix_ptr->b;
                        sum_in_r  -= stack_pix_ptr->r;
                        sum_in_g  -= stack_pix_ptr->g;
                        sum_in_b  -= stack_pix_ptr->b;
                    }
                }
            }
#endif
        }


        class BlurStack
        {
            public int r;
            public int g;
            public int b;
            public int a;
            public BlurStack() { }
            public BlurStack(byte r, byte g, byte b, byte a)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
            }
        }

        class CircularBlurStack
        {
            int currentHeadIndex;
            int currentTailIndex;
            int size;
            BlurStack[] blurValues;
            public CircularBlurStack(int size)
            {
                this.size = size;
                this.blurValues = new BlurStack[size];
                this.currentHeadIndex = 0;
                this.currentTailIndex = size - 1;
                for (int i = size - 1; i >= 0; --i)
                {
                    blurValues[i] = new BlurStack();
                }
            }
            public void Prepare(int count, int r, int g, int b, int a)
            {
                this.currentHeadIndex = 0;
                this.currentTailIndex = size - 1;
                for (int i = 0; i < count; ++i)
                {
                    blurValues[i] = new BlurStack((byte)r, (byte)g, (byte)b, (byte)a);
                    this.Next();
                }
            }
            public void ResetHeadTailPosition()
            {
                this.currentHeadIndex = 0;
                this.currentTailIndex = size - 1;
            }
            public void Next()
            {
                //--------------------------
                if (currentHeadIndex + 1 < size)
                {
                    currentHeadIndex++;
                }
                else
                {
                    currentHeadIndex = 0;
                }
                //--------------------------

                if (currentTailIndex + 1 < size)
                {
                    currentTailIndex++;
                }
                else
                {
                    currentTailIndex = 0;
                }
            }
            public BlurStack CurrentHeadColor
            {
                get
                {
                    return this.blurValues[this.currentHeadIndex];
                }
            }
            public BlurStack CurrentTailColor
            {
                get
                {
                    return this.blurValues[this.currentTailIndex];
                }
            }
        }

        void StackBlurRGBA32(BitmapBlenderBase img, int radius, int ry)
        {
            int width = img.Width; 
            int height = img.Height;

            //TODO: review here again
            //need to copy ?

            int[] srcBuffer = new int[width * height];
            BitmapBlenderBase.CopySubBufferToInt32Array(img, 0, 0, width, height, srcBuffer);
            StackBlurARGB.FastBlur32ARGB(srcBuffer, srcBuffer, img.Width, img.Height, radius);
            int i = 0;
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    //TODO: review here again=>
                    //find a better way to set pixel...
                    int dest = srcBuffer[i];
                    img.SetPixel(x, y,
                         Color.FromArgb(
                           (byte)((dest >> 16) & 0xff),
                           (byte)((dest >> 8) & 0xff),
                           (byte)((dest) & 0xff)));
                    i++;
                }
            }
        }
    }




    public abstract class RecursizeBlurCalculator
    {
        public double r, g, b, a;
        public abstract RecursizeBlurCalculator CreateNew();
        public abstract void FromPix(Color c);
        public abstract void Calc(double b1, double b2, double b3, double b4,
            RecursizeBlurCalculator c1, RecursizeBlurCalculator c2, RecursizeBlurCalculator c3, RecursizeBlurCalculator c4);
        public abstract void ToPix(ref Color c);
    }

    //===========================================================recursive_blur
    public sealed class RecursiveBlur
    {
        ArrayList<RecursizeBlurCalculator> m_sum1;
        ArrayList<RecursizeBlurCalculator> m_sum2;
        ArrayList<Color> m_buf;
        RecursizeBlurCalculator m_RecursizeBlurCalculatorFactory;
        public RecursiveBlur(RecursizeBlurCalculator recursizeBluerCalculatorFactory)
        {
            m_sum1 = new ArrayList<RecursizeBlurCalculator>();
            m_sum2 = new ArrayList<RecursizeBlurCalculator>();
            m_buf = new ArrayList<Color>();
            m_RecursizeBlurCalculatorFactory = recursizeBluerCalculatorFactory;
        }
        public void BlurX(IBitmapBlender img, double radius)
        {
            if (radius < 0.62) return;
            if (img.Width < 3) return;
            double s = (double)(radius * 0.5);
            double q = (double)((s < 2.5) ?
                                    3.97156 - 4.14554 * Math.Sqrt(1 - 0.26891 * s) :
                                    0.98711 * s - 0.96330);
            double q2 = (double)(q * q);
            double q3 = (double)(q2 * q);
            double b0 = (double)(1.0 / (1.578250 +
                                            2.444130 * q +
                                            1.428100 * q2 +
                                            0.422205 * q3));
            double b1 = (double)(2.44413 * q +
                                      2.85619 * q2 +
                                      1.26661 * q3);
            double b2 = (double)(-1.42810 * q2 +
                                     -1.26661 * q3);
            double b3 = (double)(0.422205 * q3);
            double b = (double)(1 - (b1 + b2 + b3) * b0);
            b1 *= b0;
            b2 *= b0;
            b3 *= b0;
            int w = img.Width;
            int h = img.Height;
            int wm = (int)w - 1;
            int x, y;
            int startCreatingAt = (int)m_sum1.Count;
            m_sum1.AdjustSize(w);
            m_sum2.AdjustSize(w);
            m_buf.Allocate(w);
            RecursizeBlurCalculator[] Sum1Array = m_sum1.Array;
            RecursizeBlurCalculator[] Sum2Array = m_sum2.Array;
            Color[] BufferArray = m_buf.Array;
            for (int i = startCreatingAt; i < w; i++)
            {
                Sum1Array[i] = m_RecursizeBlurCalculatorFactory.CreateNew();
                Sum2Array[i] = m_RecursizeBlurCalculatorFactory.CreateNew();
            }

            for (y = 0; y < h; y++)
            {

                //TODO: review get pixel here...
                RecursizeBlurCalculator c = m_RecursizeBlurCalculatorFactory;
                c.FromPix(img.GetPixel(0, y));
                Sum1Array[0].Calc(b, b1, b2, b3, c, c, c, c);
                c.FromPix(img.GetPixel(1, y));
                Sum1Array[1].Calc(b, b1, b2, b3, c, Sum1Array[0], Sum1Array[0], Sum1Array[0]);
                c.FromPix(img.GetPixel(2, y));
                Sum1Array[2].Calc(b, b1, b2, b3, c, Sum1Array[1], Sum1Array[0], Sum1Array[0]);
                for (x = 3; x < w; ++x)
                {
                    c.FromPix(img.GetPixel(x, y));
                    Sum1Array[x].Calc(b, b1, b2, b3, c, Sum1Array[x - 1], Sum1Array[x - 2], Sum1Array[x - 3]);
                }

                Sum2Array[wm].Calc(b, b1, b2, b3, Sum1Array[wm], Sum1Array[wm], Sum1Array[wm], Sum1Array[wm]);
                Sum2Array[wm - 1].Calc(b, b1, b2, b3, Sum1Array[wm - 1], Sum2Array[wm], Sum2Array[wm], Sum2Array[wm]);
                Sum2Array[wm - 2].Calc(b, b1, b2, b3, Sum1Array[wm - 2], Sum2Array[wm - 1], Sum2Array[wm], Sum2Array[wm]);
                Sum2Array[wm].ToPix(ref BufferArray[wm]);
                Sum2Array[wm - 1].ToPix(ref BufferArray[wm - 1]);
                Sum2Array[wm - 2].ToPix(ref BufferArray[wm - 2]);
                for (x = wm - 3; x >= 0; --x)
                {
                    Sum2Array[x].Calc(b, b1, b2, b3, Sum1Array[x], Sum2Array[x + 1], Sum2Array[x + 2], Sum2Array[x + 3]);
                    Sum2Array[x].ToPix(ref BufferArray[x]);
                }

                img.CopyColorHSpan(0, y, w, BufferArray, 0);
            }
        }

        public void BlurY(IBitmapBlender img, double radius)
        {
            FormatTransposer img2 = new FormatTransposer(img);
            BlurX(img2, radius);
        }

        public void Blur(IBitmapBlender img, double radius)
        {
            BlurX(img, radius);
            BlurY(img, radius);
        }
    }

    //=================================================recursive_blur_calc_rgb
    public sealed class RecursiveBlurCalcRGB : RecursizeBlurCalculator
    {
        public override RecursizeBlurCalculator CreateNew()
        {
            return new RecursiveBlurCalcRGB();
        }

        public override void FromPix(Color c)
        {
            r = c.red;
            g = c.green;
            b = c.blue;
        }

        public override void Calc(double b1, double b2, double b3, double b4,
            RecursizeBlurCalculator c1, RecursizeBlurCalculator c2, RecursizeBlurCalculator c3, RecursizeBlurCalculator c4)
        {
            r = b1 * c1.r + b2 * c2.r + b3 * c3.r + b4 * c4.r;
            g = b1 * c1.g + b2 * c2.g + b3 * c3.g + b4 * c4.g;
            b = b1 * c1.b + b2 * c2.b + b3 * c3.b + b4 * c4.b;
        }

        public override void ToPix(ref Color c)
        {
            //c.red = (byte)AggMath.uround(r);
            //c.green = (byte)AggMath.uround(g);
            //c.blue = (byte)AggMath.uround(b);

            c = new Color(c.alpha,
                (byte)AggMath.uround(r),
                (byte)AggMath.uround(g),
                (byte)AggMath.uround(b)
                );

        }
    }

    //=================================================recursive_blur_calc_rgba
    public sealed class RecursiveBlurCalcRGBA : RecursizeBlurCalculator
    {
        public override RecursizeBlurCalculator CreateNew()
        {
            return new RecursiveBlurCalcRGBA();
        }

        public override void FromPix(Color c)
        {
            r = c.red;
            g = c.green;
            b = c.blue;
            a = c.alpha;
        }

        public override void Calc(double b1, double b2, double b3, double b4,
            RecursizeBlurCalculator c1, RecursizeBlurCalculator c2, RecursizeBlurCalculator c3, RecursizeBlurCalculator c4)
        {
            r = b1 * c1.r + b2 * c2.r + b3 * c3.r + b4 * c4.r;
            g = b1 * c1.g + b2 * c2.g + b3 * c3.g + b4 * c4.g;
            b = b1 * c1.b + b2 * c2.b + b3 * c3.b + b4 * c4.b;
            a = b1 * c1.a + b2 * c2.a + b3 * c3.a + b4 * c4.a;
        }

        public override void ToPix(ref Color c)
        {
            c = new Color(
                (byte)AggMath.uround(a),
                (byte)AggMath.uround(r),
                (byte)AggMath.uround(g),
                (byte)AggMath.uround(b)
                );
        }
    }

    //================================================recursive_blur_calc_gray
    public sealed class RecursiveBlurCalcGray : RecursizeBlurCalculator
    {
        public override RecursizeBlurCalculator CreateNew()
        {
            return new RecursiveBlurCalcGray();
        }

        public override void FromPix(Color c)
        {
            r = c.red;
        }

        public override void Calc(double b1, double b2, double b3, double b4,
            RecursizeBlurCalculator c1, RecursizeBlurCalculator c2, RecursizeBlurCalculator c3, RecursizeBlurCalculator c4)
        {
            r = b1 * c1.r + b2 * c2.r + b3 * c3.r + b4 * c4.r;
        }

        public override void ToPix(ref Color c)
        {
            c = new Color(c.A,
                (byte)AggMath.uround(r),
                c.G,
                c.B
                );
        }
    }
}
