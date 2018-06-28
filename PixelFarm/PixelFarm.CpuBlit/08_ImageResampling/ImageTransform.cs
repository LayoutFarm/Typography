//BSD, 2014-present, WinterDev
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.Imaging
{
    public class CubicInterpolator
    {
        public static double getValue(double[] p, double x)
        {
            return p[1] + 0.5 * x * (p[2] - p[0] + x * (2.0 * p[0] - 5.0 * p[1] + 4.0 * p[2] - p[3] + x * (3.0 * (p[1] - p[2]) + p[3] - p[0])));
        }
    }
    public class BicubicInterpolator : CubicInterpolator
    {
        private double[] arr = new double[4];
        public double getValue(double[][] p, double x, double y)
        {
            var mm = p[0];
            arr[0] = getValue(p[0], y);
            arr[1] = getValue(p[1], y);
            arr[2] = getValue(p[2], y);
            arr[3] = getValue(p[3], y);
            return getValue(arr, x);
        }
    }
    //------------------------------------------------------------------------
    public class CubicInterpolator2
    {

        public static double getValue2(double p0, double p1, double p2, double p3, double x)
        {
            return p1 + 0.5 * x * (p2 - p0 + x * (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3 +
                    x * (3.0 * (p1 - p2) + p3 - p0)));
        }
        public static double getValue2(byte p0, byte p1, byte p2, byte p3, double x)
        {
            return p1 + 0.5 * x * (p2 - p0 + x * (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3 +
                    x * (3.0 * (p1 - p2) + p3 - p0)));
        }
    }
    public class BicubicInterpolator2 : CubicInterpolator2
    {

        public static void GetInterpolatedColor(Color[] colors, double x, double y,
            out PixelFarm.Drawing.Color outputColor)
        {
            //interpolate by channel        
            double v1, v2, v3, v4, v;
            byte a;
            {
                //row
                v1 = getValue2(colors[0].A, colors[1].A, colors[2].A, colors[3].A, x);
                v2 = getValue2(colors[4].A, colors[5].A, colors[6].A, colors[7].A, x);
                v3 = getValue2(colors[8].A, colors[9].A, colors[10].A, colors[11].A, x);
                v4 = getValue2(colors[12].A, colors[13].A, colors[14].A, colors[15].A, x);
                //columns
                v = getValue2(v1, v2, v3, v4, y);
                //clamp
                if (v > 255)
                {
                    a = 255;
                }
                else if (v < 0)
                {
                    a = 0;
                }
                else
                {
                    a = (byte)v;
                }
            }
            //---------------------------------------------------
            byte r;
            {
                //row
                v1 = getValue2(colors[0].R, colors[1].R, colors[2].R, colors[3].R, x);
                v2 = getValue2(colors[4].R, colors[5].R, colors[6].R, colors[7].R, x);
                v3 = getValue2(colors[8].R, colors[9].R, colors[10].R, colors[11].R, x);
                v4 = getValue2(colors[12].R, colors[13].R, colors[14].R, colors[15].R, x);
                //columns
                v = getValue2(v1, v2, v3, v4, y);
                //clamp
                if (v > 255)
                {
                    r = 255;
                }
                else if (v < 0)
                {
                    r = 0;
                }
                else
                {
                    r = (byte)v;
                }

            }
            //---------------------------------------------------
            byte g;
            {
                //row
                v1 = getValue2(colors[0].G, colors[1].G, colors[2].G, colors[3].G, x);
                v2 = getValue2(colors[4].G, colors[5].G, colors[6].G, colors[7].G, x);
                v3 = getValue2(colors[8].G, colors[9].G, colors[10].G, colors[11].G, x);
                v4 = getValue2(colors[12].G, colors[13].G, colors[14].G, colors[15].G, x);
                //columns
                v = getValue2(v1, v2, v3, v4, y);
                //clamp
                if (v > 255)
                {
                    g = 255;
                }
                else if (v < 0)
                {
                    g = 0;
                }
                else
                {
                    g = (byte)v;
                }
            }
            //---------------------------------------------------
            byte b;
            {
                //b

                v1 = getValue2(colors[0].B, colors[1].B, colors[2].B, colors[3].B, x);
                v2 = getValue2(colors[4].B, colors[5].B, colors[6].B, colors[7].B, x);
                v3 = getValue2(colors[8].B, colors[9].B, colors[10].B, colors[11].B, x);
                v4 = getValue2(colors[12].B, colors[13].B, colors[14].B, colors[15].B, x);
                //columns
                v = getValue2(v1, v2, v3, v4, y);
                //clamp
                if (v > 255)
                {
                    b = 255;
                }
                else if (v < 0)
                {
                    b = 0;
                }
                else
                {
                    b = (byte)v;
                }
            }

            outputColor = new Color(a, r, g, b);
        }
    }

    struct BufferReader4
    {
        //matrix four ,four reader
        unsafe int* buffer; 
        int width;
        int height;
        int cX;
        int cY;
        unsafe public BufferReader4(int* buffer, int width, int height)
        {
            this.buffer = buffer;

            this.width = width;
            this.height = height;
            cX = cY = 0;
        }
        public void SetStartPixel(int x, int y)
        {
            cX = x;
            cY = y;
        }
        static Color FromInt(int value)
        {
            return new Color(
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 0) & 0xff));
        }
        public Color ReadOnePixel()
        {

            unsafe
            {
                return FromInt(buffer[((cY * width) + cX)]);

            }
        }

        public Color Read1()
        {
            unsafe
            {
                return FromInt(buffer[(this.cY * width) + cX]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x0y0"></param>
        /// <param name="x1y0"></param>
        /// <param name="x0y1"></param>
        /// <param name="x1y1"></param>
        public void Read4(out Color x0y0, out Color x1y0, out Color x0y1, out Color x1y1)
        {
            //byte b, g, r, a; 

            int index = (this.cY * width) + cX;
            unsafe
            {
                x0y0 = FromInt(buffer[index]);
                index++;
                //----------------------------------- 
                x1y0 = FromInt(buffer[index]);
                index++;
                //------------------------------------
                //newline
                index = ((this.cY + 1) * width) + cX;
                //------------------------------------ 
                x0y1 = FromInt(buffer[index]);
                index++;
                //------------------------------------ 
                x1y1 = FromInt(buffer[index]);
                index++;
            }

        }
        /// <summary>
        /// read 4 x 4 pixels
        /// </summary>
        /// <param name="outputBuffer"></param>
        public void Read16(Color[] outputBuffer)
        {
            //bgra to argb
            //16 px 
            //byte b, g, r, a;
            int m = 0;
            int tmpY = this.cY - 1;
            int index = (tmpY * width) + cX;
            index--;

            unsafe
            {  //-------------------------------------------------             
                for (int n = 0; n < 4; ++n)
                {
                    outputBuffer[m] = FromInt(buffer[index]);
                    index++;
                    //------------------------------------------------ 
                    outputBuffer[m + 1] = FromInt(buffer[index]);
                    index++;
                    //------------------------------------------------ 
                    outputBuffer[m + 2] = FromInt(buffer[index]);
                    index++;
                    //------------------------------------------------ 
                    outputBuffer[m + 3] = FromInt(buffer[index]);
                    index++;
                    //------------------------------------------------
                    m += 4;
                    //go next row
                    tmpY++;
                    index = (tmpY * width) + cX;
                    index--;
                }
            }
        }
        public bool CanReadAsBlock()
        {
            return cX > 2 && cY > 2 &&
                   cX < width - 2 && cY < height - 2;

        }
    }
    //------------------------------------------------------------------------
}