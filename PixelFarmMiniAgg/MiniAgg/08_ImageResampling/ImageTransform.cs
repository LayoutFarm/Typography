//2014-2016, BSD WinterDev

namespace PixelFarm.Agg.Image
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
        public static double getValue(double[] p, double x)
        {
            return p[1] + 0.5 * x * (p[2] - p[0] + x * (2.0 * p[0] - 5.0 * p[1] + 4.0 * p[2] - p[3] + x * (3.0 * (p[1] - p[2]) + p[3] - p[0])));
        }
        public static double getValue2(double p0, double p1, double p2, double p3, double x)
        {
            return p1 + 0.5 * x * (p2 - p0 + x * (2.0 * p0 - 5.0 * p1 + 4.0 * p2 - p3 +
                    x * (3.0 * (p1 - p2) + p3 - p0)));
        }
    }
    public class BicubicInterpolator2 : CubicInterpolator2
    {
        public double getValue(double[][] p, double x, double y)
        {
            var am = p[0];
            var a1 = getValue(p[0], y);
            var a2 = getValue(p[1], y);
            var a3 = getValue(p[2], y);
            var a4 = getValue(p[3], y);
            return getValue2(a1, a2, a3, a4, x);
        }
        public double getValueBytes(byte[] sqPixs, double x, double y)
        {
            var a1 = getValue2(sqPixs[0], sqPixs[1], sqPixs[2], sqPixs[3], x);
            var a2 = getValue2(sqPixs[4], sqPixs[5], sqPixs[6], sqPixs[7], x);
            var a3 = getValue2(sqPixs[8], sqPixs[9], sqPixs[10], sqPixs[11], x);
            var a4 = getValue2(sqPixs[12], sqPixs[13], sqPixs[14], sqPixs[15], x);
            return getValue2(a1, a2, a3, a4, y);
        }
    }
    public struct MyColor
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public MyColor(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public override string ToString()
        {
            return "b:" + b + ",g:" + g + ",r:" + r + ",a:" + a;
        }
    }
    public class BufferReader4
    {
        //matrix four ,four reader
        byte[] buffer;
        int stride;
        int width;
        int height;
        int cX;
        int cY;
        public BufferReader4(byte[] buffer, int stride, int width, int height)
        {
            this.buffer = buffer;
            this.stride = stride;
            this.width = width;
            this.height = height;
        }
        public void SetStartPixel(int x, int y)
        {
            cX = x;
            cY = y;
        }

        public MyColor ReadOnePixel()
        {
            int byteIndex = ((cY * stride) + cX * 4);
            byte b = buffer[byteIndex];
            byte g = buffer[byteIndex + 1];
            byte r = buffer[byteIndex + 2];
            byte a = buffer[byteIndex + 3];
            return new MyColor(r, g, b, a);
        }
        public void Read4(MyColor[] outputBuffer)
        {
            byte b, g, r, a;
            int m = 0;
            int tmpY = this.cY;
            int byteIndex = ((tmpY * stride) + cX * 4);
            b = buffer[byteIndex];
            g = buffer[byteIndex + 1];
            r = buffer[byteIndex + 2];
            a = buffer[byteIndex + 3];
            outputBuffer[m] = new MyColor(r, g, b, a);
            byteIndex += 4;
            //-----------------------------------
            b = buffer[byteIndex];
            g = buffer[byteIndex + 1];
            r = buffer[byteIndex + 2];
            a = buffer[byteIndex + 3];
            outputBuffer[m + 1] = new MyColor(r, g, b, a);
            byteIndex += 4;
            //------------------------------------
            //newline
            tmpY++;
            byteIndex = (tmpY * stride) + (cX * 4);
            //------------------------------------
            b = buffer[byteIndex];
            g = buffer[byteIndex + 1];
            r = buffer[byteIndex + 2];
            a = buffer[byteIndex + 3];
            outputBuffer[m + 2] = new MyColor(r, g, b, a);
            byteIndex += 4;
            //------------------------------------
            b = buffer[byteIndex];
            g = buffer[byteIndex + 1];
            r = buffer[byteIndex + 2];
            a = buffer[byteIndex + 3];
            outputBuffer[m + 3] = new MyColor(r, g, b, a);
            byteIndex += 4;
        }
        public void Read16(MyColor[] outputBuffer)
        {
            //bgra to argb
            //16 px 
            byte b, g, r, a;
            int m = 0;
            int tmpY = this.cY - 1;
            int byteIndex = ((tmpY * stride) + cX * 4);
            byteIndex -= 4;//step back
            //-------------------------------------------------             
            for (int n = 0; n < 4; ++n)
            {
                //0
                b = buffer[byteIndex];
                g = buffer[byteIndex + 1];
                r = buffer[byteIndex + 2];
                a = buffer[byteIndex + 3];
                outputBuffer[m] = new MyColor(r, g, b, a);
                byteIndex += 4;
                //------------------------------------------------
                //1
                b = buffer[byteIndex];
                g = buffer[byteIndex + 1];
                r = buffer[byteIndex + 2];
                a = buffer[byteIndex + 3];
                outputBuffer[m + 1] = new MyColor(r, g, b, a);
                byteIndex += 4;
                //------------------------------------------------
                //2
                b = buffer[byteIndex];
                g = buffer[byteIndex + 1];
                r = buffer[byteIndex + 2];
                a = buffer[byteIndex + 3];
                outputBuffer[m + 2] = new MyColor(r, g, b, a);
                byteIndex += 4;
                //------------------------------------------------
                //3
                b = buffer[byteIndex];
                g = buffer[byteIndex + 1];
                r = buffer[byteIndex + 2];
                a = buffer[byteIndex + 3];
                outputBuffer[m + 3] = new MyColor(r, g, b, a);
                byteIndex += 4;
                //------------------------------------------------
                m += 4;
                //go next row
                tmpY++;
                byteIndex = (tmpY * stride) + (cX * 4);
                byteIndex -= 4;
            }
        }
        public void MoveNext()
        {
            //move next and automatic gonext line
            cX++;
        }

        public int CurrentX { get { return this.cX; } }
        public int CurrentY { get { return this.cY; } }
        public int Height { get { return this.height; } }
        public int Width { get { return this.width; } }
    }
    //------------------------------------------------------------------------
}