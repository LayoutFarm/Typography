//BSD, 2014-present, WinterDev
//ref:  adapt from snippetsfor.net/CSharp/StackBlur

using System;
namespace PixelFarm.CpuBlit.Imaging
{
    public static class StackBlurARGB
    {
        //-----------------------------------------------
        const int A_SHIFT = 24;
        const int R_SHIFT = 16;
        const int G_SHIFT = 8;
        const int B_SHIFT = 0;
        //------------------------------------------------
        static byte[] PrepareLookupTable(int radius)
        {
            int div = radius + radius + 1;
            var dv = new byte[256 * div];
            for (int i = (256 * div) - 1; i >= 0; --i)
            {
                dv[i] = (byte)(i / div);
            }
            return dv;
        }
        //------------------------------------------------
        static void PrepareHorizontalMinMax(int width, int radius, LimitMinMax[] limitMinMax)
        {
            int widthMinus1 = width - 1;
            for (int x = width - 1; x >= 0; --x)
            {
                limitMinMax[x] = new LimitMinMax(
                    min(x + radius + 1, widthMinus1),
                    max(x - radius, 0));
            }
        }
        static void PrepareVerticalMinMax(int width, int height, int radius, LimitMinMax[] limitMinMax)
        {
            int heightMinus1 = height - 1;
            for (int y = height - 1; y >= 0; --y)
            {
                limitMinMax[y] = new LimitMinMax(
                    min(y + radius + 1, heightMinus1) * width,
                    max(y - radius, 0) * width);
            }
        }


        //---------------------------------------------------------------------------
        static void CalculateSumARGBHorizontal(int[] source,
            int width,
            int pixel_index, int radius,
            out int rsum, out int gsum, out int bsum, out int asum)
        {
            rsum = gsum = bsum = asum = 0;
            for (int i = -radius; i <= 0; ++i)
            {
                //negative and zero  
                int p = source[pixel_index];
                rsum += (p >> R_SHIFT) & 0xff;
                gsum += (p >> G_SHIFT) & 0xff;
                bsum += (p >> B_SHIFT) & 0xff;
                asum += (p >> A_SHIFT) & 0xff;
            }
            int widthMinus1 = width - 1;
            for (int i = 1; i <= radius; ++i)
            {
                //positive side
                int p = source[pixel_index + min(widthMinus1, i)];
                rsum += (p >> R_SHIFT) & 0xff;
                gsum += (p >> G_SHIFT) & 0xff;
                bsum += (p >> B_SHIFT) & 0xff;
                asum += (p >> A_SHIFT) & 0xff;
            }
        }

        public static void FastBlur32ARGB(int[] srcBuffer,
             int[] dest,
             int srcImageWidth,
             int srcImageHeight,
             int radius)
        {
            if (srcImageWidth < 1)
            {
                return;
            }

            //---------------------------------------------
            //assign dimension info and copy buffer 

            int width = srcImageWidth;
            int height = srcImageHeight;
            int wh = width * height;
            var r_buffer = new byte[wh];
            var g_buffer = new byte[wh];
            var b_buffer = new byte[wh];
            var a_buffer = new byte[wh];
            int p1, p2;
            LimitMinMax[] limitMinMax = new LimitMinMax[max(width, height)];
            //------------------------------
            //look up table : depends on radius,  
            var dvLookup = PrepareLookupTable(radius);
            //------------------------------  

            PrepareHorizontalMinMax(width, radius, limitMinMax);
            int px_row_head = 0;
            int pixel_index = 0;
            for (int y = 0; y < height; y++)
            {
                // blur horizontal
                int rsum, gsum, bsum, asum;
                CalculateSumARGBHorizontal(srcBuffer, width, pixel_index, radius, out rsum, out gsum, out bsum, out asum);
                for (int x = 0; x < width; x++)
                {
                    r_buffer[pixel_index] = dvLookup[rsum];
                    g_buffer[pixel_index] = dvLookup[gsum];
                    b_buffer[pixel_index] = dvLookup[bsum];
                    a_buffer[pixel_index] = dvLookup[asum];
                    LimitMinMax lim = limitMinMax[x];
                    p1 = srcBuffer[px_row_head + lim.Min];
                    p2 = srcBuffer[px_row_head + lim.Max];
                    rsum += ((p1 >> R_SHIFT) & 0xff) - ((p2 >> R_SHIFT) & 0xff);
                    gsum += ((p1 >> G_SHIFT) & 0xff) - ((p2 >> G_SHIFT) & 0xff);
                    bsum += ((p1 >> B_SHIFT) & 0xff) - ((p2 >> B_SHIFT) & 0xff);
                    asum += ((p1 >> A_SHIFT) & 0xff) - ((p2 >> A_SHIFT) & 0xff);
                    pixel_index++;
                }
                //go next row
                px_row_head += width;
            }

            PrepareVerticalMinMax(width, height, radius, limitMinMax);
            //-------------------------------------------------------------------
            for (int x = 0; x < width; x++)
            {
                // blur vertical
                int rsum, gsum, bsum, asum;
                rsum = gsum = bsum = asum = 0;
                //-----------------------------
                int yp = -radius * width;
                for (int i = -radius; i <= 0; ++i)
                {
                    pixel_index = x;
                    rsum += r_buffer[pixel_index];
                    gsum += g_buffer[pixel_index];
                    bsum += b_buffer[pixel_index];
                    asum += a_buffer[pixel_index];
                    yp += width;
                }
                for (int i = 1; i <= radius; ++i)
                {
                    pixel_index = yp + x;
                    rsum += r_buffer[pixel_index];
                    gsum += g_buffer[pixel_index];
                    bsum += b_buffer[pixel_index];
                    asum += a_buffer[pixel_index];
                    yp += width;
                }
                //-----------------------------

                pixel_index = x;
                for (int y = 0; y < height; y++)
                {
                    //assign pixel value here
                    dest[pixel_index] = (int)((uint)(dvLookup[rsum] << R_SHIFT) |
                        (uint)(dvLookup[gsum] << G_SHIFT) |
                        (uint)(dvLookup[bsum] << B_SHIFT) |
                        unchecked((uint)(dvLookup[asum] << A_SHIFT)));
                    var limit = limitMinMax[y];
                    p1 = x + limit.Min;
                    p2 = x + limit.Max;
                    rsum += r_buffer[p1] - r_buffer[p2]; //diff between 2 pixels
                    gsum += g_buffer[p1] - g_buffer[p2]; //diff between 2 pixels
                    bsum += b_buffer[p1] - b_buffer[p2]; //diff between 2 pixels
                    asum += a_buffer[p1] - a_buffer[p2];
                    pixel_index += width;
                }
            }
        }



        private static int min(int a, int b) { return Math.Min(a, b); }
        private static int max(int a, int b) { return Math.Max(a, b); }

        struct LimitMinMax
        {
            public int Min;
            public int Max;
            public LimitMinMax(int min, int max)
            {
                this.Min = min;
                this.Max = max;
            }
        }
    }
}