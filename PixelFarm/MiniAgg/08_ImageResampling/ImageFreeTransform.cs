//MIT, 2014-2017, WinterDev
//---------------------------------------------
//some code from CodeProject: 'Free Image Transformation'
//YLS CS 
//license : CPOL

using System;
using PixelFarm.VectorMath;
namespace PixelFarm.Agg.Imaging
{

    public class FreeTransform
    {
        PointF[] vertex = new PointF[4];
        Vector AB, BC, CD, DA;
        RectInt rect = new RectInt();
        MyImageReaderWriter srcCB;
        ActualImage srcImageInput;
        int srcW = 0;
        int srcH = 0;
        public FreeTransform()
        {
        }
        public ActualImage Bitmap
        {
            get
            {
                return GetTransformedBitmap();
            }
            set
            {
                if (value == null)
                {
                    return;
                }
                try
                {
                    this.srcImageInput = value;
                    this.srcCB = new MyImageReaderWriter();
                    srcCB.ReloadImage(value);
                    srcH = value.Height;
                    srcW = value.Width;
                }
                catch
                {
                    srcW = 0; srcH = 0;
                }
            }
        }

        public Point ImageLocation
        {
            //left bottom?
            get { return new Point(rect.Left, rect.Bottom); }
        }

        bool isBilinear = false;
        public bool IsBilinearInterpolation
        {
            get { return isBilinear; }
            set { isBilinear = value; }
        }

        public int ImageWidth
        {
            get { return rect.Width; }
        }

        public int ImageHeight
        {
            get { return rect.Height; }
        }

        public PointF VertexLeftTop
        {
            set { vertex[0] = value; setVertex(); }
            get { return vertex[0]; }
        }

        public PointF VertexTopRight
        {
            get { return vertex[1]; }
            set { vertex[1] = value; setVertex(); }
        }

        public PointF VertexRightBottom
        {
            get { return vertex[2]; }
            set
            {
                vertex[2] = value;
                setVertex();
            }
        }

        public PointF VertexBottomLeft
        {
            get { return vertex[3]; }
            set { vertex[3] = value; setVertex(); }
        }

        public PointF[] FourCorners
        {
            get { return vertex; }
            set { vertex = value; setVertex(); }
        }

        private void setVertex()
        {
            float xmin = float.MaxValue;
            float ymin = float.MaxValue;
            float xmax = float.MinValue;
            float ymax = float.MinValue;
            for (int i = 0; i < 4; i++)
            {
                xmax = Math.Max(xmax, vertex[i].X);
                ymax = Math.Max(ymax, vertex[i].Y);
                xmin = Math.Min(xmin, vertex[i].X);
                ymin = Math.Min(ymin, vertex[i].Y);
            }

            rect = new RectInt((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
            AB = MyVectorHelper.NewFromTwoPoints(vertex[0], vertex[1]);
            BC = MyVectorHelper.NewFromTwoPoints(vertex[1], vertex[2]);
            CD = MyVectorHelper.NewFromTwoPoints(vertex[2], vertex[3]);
            DA = MyVectorHelper.NewFromTwoPoints(vertex[3], vertex[0]);
            // get unit vector
            AB /= AB.Magnitude;
            BC /= BC.Magnitude;
            CD /= CD.Magnitude;
            DA /= DA.Magnitude;
        }

        private bool IsOnPlaneABCD(PointF pt) //  including point on border
        {
            if (!MyVectorHelper.IsCCW(pt, vertex[0], vertex[1]))
            {
                if (!MyVectorHelper.IsCCW(pt, vertex[1], vertex[2]))
                {
                    if (!MyVectorHelper.IsCCW(pt, vertex[2], vertex[3]))
                    {
                        if (MyVectorHelper.IsCCW(pt, vertex[3], vertex[0]))
                            return true;
                    }
                }
            }

            return false;
        }

        ActualImage GetTransformedBitmap()
        {
            if (srcH == 0 || srcW == 0) return null;
            if (isBilinear)
            {
                //return GetTransformedBicubicInterpolation();
                return GetTransformedBicubicInterpolation();
                //return GetTransformedBilinearInterpolation();
            }
            else
            {
                return GetTransformedBitmapNoInterpolation();
            }
        }
        static BicubicInterpolator2 myInterpolator = new BicubicInterpolator2();
        ActualImage GetTransformedBitmapNoInterpolation()
        {
            var destCB = new ActualImage(rect.Width, rect.Height, PixelFormat.ARGB32);
            var destWriter = new MyImageReaderWriter();
            destWriter.ReloadImage(destCB);
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;
            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;
            int rectLeft = this.rect.Left;
            int rectTop = this.rect.Top;
            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {
                    PointF srcPt = new PointF(x, y);
                    srcPt.Offset(rectLeft, rectTop);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;
                    destWriter.SetPixel(x, y, srcCB.GetPixel(x1, y1));
                    //-------------------------------------
                    dab = Math.Abs((MyVectorHelper.NewFromTwoPoints(vertex[0], srcPt)).CrossProduct(ab_vec));
                    dbc = Math.Abs((MyVectorHelper.NewFromTwoPoints(vertex[1], srcPt)).CrossProduct(bc_vec));
                    dcd = Math.Abs((MyVectorHelper.NewFromTwoPoints(vertex[2], srcPt)).CrossProduct(cd_vec));
                    dda = Math.Abs((MyVectorHelper.NewFromTwoPoints(vertex[3], srcPt)).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                }
            }
            return destCB;
        }
        ActualImage GetTransformedBilinearInterpolation()
        {
            //4 points sampling
            //weight between four point
            ActualImage destCB = new ActualImage(rect.Width, rect.Height, PixelFormat.ARGB32);
            MyImageReaderWriter destWriter = new MyImageReaderWriter();
            destWriter.ReloadImage(destCB);
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;
            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;
            int rectLeft = this.rect.Left;
            int rectTop = this.rect.Top;
            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {
                    PointF srcPt = new PointF(x, y);
                    srcPt.Offset(rectLeft, rectTop);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    //-------------------------------------
                    dab = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[0], srcPt).CrossProduct(ab_vec));
                    dbc = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[1], srcPt).CrossProduct(bc_vec));
                    dcd = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[2], srcPt).CrossProduct(cd_vec));
                    dda = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[3], srcPt).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;
                    if (x1 >= 0 && x1 < srcW && y1 >= 0 && y1 < srcH)
                    {
                        //bilinear interpolation *** 
                        x2 = (x1 == srcW - 1) ? x1 : x1 + 1;
                        y2 = (y1 == srcH - 1) ? y1 : y1 + 1;
                        dx1 = ptInPlane.X - (float)x1;
                        if (dx1 < 0) dx1 = 0;
                        dx1 = 1f - dx1;
                        dx2 = 1f - dx1;
                        dy1 = ptInPlane.Y - (float)y1;
                        if (dy1 < 0) dy1 = 0;
                        dy1 = 1f - dy1;
                        dy2 = 1f - dy1;
                        dx1y1 = dx1 * dy1;
                        dx1y2 = dx1 * dy2;
                        dx2y1 = dx2 * dy1;
                        dx2y2 = dx2 * dy2;
                        //use 4 points

                        Drawing.Color x1y1Color = srcCB.GetPixel(x1, y1);
                        Drawing.Color x2y1Color = srcCB.GetPixel(x2, y1);
                        Drawing.Color x1y2Color = srcCB.GetPixel(x1, y2);
                        Drawing.Color x2y2Color = srcCB.GetPixel(x2, y2);
                        float a = (x1y1Color.alpha * dx1y1) + (x2y1Color.alpha * dx2y1) + (x1y2Color.alpha * dx1y2) + (x2y2Color.alpha * dx2y2);
                        float b = (x1y1Color.blue * dx1y1) + (x2y1Color.blue * dx2y1) + (x1y2Color.blue * dx1y2) + (x2y2Color.blue * dx2y2);
                        float g = (x1y1Color.green * dx1y1) + (x2y1Color.green * dx2y1) + (x1y2Color.green * dx1y2) + (x2y2Color.green * dx2y2);
                        float r = (x1y1Color.red * dx1y1) + (x2y1Color.red * dx2y1) + (x1y2Color.red * dx1y2) + (x2y2Color.red * dx2y2);
                        destWriter.SetPixel(x, y, new Drawing.Color((byte)a, (byte)b, (byte)g, (byte)r));
                        //destCB.SetColorPixel(x, y, new ColorRGBA((byte)b, (byte)g, (byte)r, (byte)a));
                    }
                }
            }
            return destCB;
        }
        ActualImage GetTransformedBicubicInterpolation()
        {
            //4 points sampling
            //weight between four point


            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;
            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;
            byte[] buffer = srcCB.GetBuffer();
            int stride = srcCB.Stride;
            int bmpWidth = srcCB.Width;
            int bmpHeight = srcCB.Height;
            BufferReader4 reader = new BufferReader4(buffer, stride, bmpWidth, bmpHeight);
            MyColor[] pixelBuffer = new MyColor[16];
            byte[] sqPixs = new byte[16];
            //Bitmap outputbmp = new Bitmap(rectWidth, rectHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ////-----------------------------------------------
            //var bmpdata2 = outputbmp.LockBits(new Rectangle(0, 0, rectWidth, rectHeight),
            //    System.Drawing.Imaging.ImageLockMode.ReadWrite, outputbmp.PixelFormat);
            ////-----------------------------------------

            ActualImage destCB = new ActualImage(rect.Width, rect.Height, PixelFormat.ARGB32);
            MyImageReaderWriter destWriter = new MyImageReaderWriter();
            destWriter.ReloadImage(destCB);
            //PointF ptInPlane = new PointF();

            //int stride2 = bmpdata2.Stride;
            //byte[] outputBuffer = new byte[stride2 * outputbmp.Height];

            // int targetPixelIndex = 0;
            // int startLine = 0;

            int rectLeft = this.rect.Left;
            int rectTop = this.rect.Top;
            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {
                    PointF srcPt = new PointF(x, y);
                    srcPt.Offset(rectLeft, rectTop);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    //-------------------------------------
                    dab = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[0], srcPt).CrossProduct(ab_vec));
                    dbc = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[1], srcPt).CrossProduct(bc_vec));
                    dcd = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[2], srcPt).CrossProduct(cd_vec));
                    dda = Math.Abs(MyVectorHelper.NewFromTwoPoints(vertex[3], srcPt).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;
                    if (x1 >= 2 && x1 < srcW - 2 && y1 >= 2 && y1 < srcH - 2)
                    {
                        reader.SetStartPixel(x1, y1);
                        reader.Read16(pixelBuffer);
                        //do interpolate

                        //find src pixel and approximate  
                        MyColor color = GetApproximateColor_Bicubic(reader,
                           ptInPlane.X,
                           ptInPlane.Y);
                        //outputBuffer[targetPixelIndex] = (byte)color.b;
                        //outputBuffer[targetPixelIndex + 1] = (byte)color.g;
                        //outputBuffer[targetPixelIndex + 2] = (byte)color.r;
                        //outputBuffer[targetPixelIndex + 3] = (byte)color.a;
                        //targetPixelIndex += 4;

                        destWriter.SetPixel(x, y, new Drawing.Color(color.a, color.b, color.g, color.r)); //TODO:review here blue switch to red channel
                    }
                }
                //newline
                // startLine += stride2;
                //targetPixelIndex = startLine;
            }
            //------------------------
            //System.Runtime.InteropServices.Marshal.Copy(
            //outputBuffer, 0,
            //bmpdata2.Scan0, outputBuffer.Length);
            //outputbmp.UnlockBits(bmpdata2);
            ////outputbmp.Save("d:\\WImageTest\\n_lion_bicubic.png");
            //return outputbmp;
            return destCB;
        }
        static void SeparateByChannel(MyColor[] myColors, byte[] rBuffer, byte[] gBuffer, byte[] bBuffer, byte[] aBuffer)
        {
            for (int i = 0; i < 16; ++i)
            {
                MyColor m = myColors[i];
                rBuffer[i] = m.r;
                gBuffer[i] = m.g;
                bBuffer[i] = m.b;
                aBuffer[i] = m.a;
            }
        }
        static MyColor GetApproximateColor_Bicubic(BufferReader4 reader, double cx, double cy)
        {
            byte[] rBuffer = new byte[16];
            byte[] gBuffer = new byte[16];
            byte[] bBuffer = new byte[16];
            byte[] aBuffer = new byte[16];
            //nearest neighbor
            if (reader.CurrentX > 2 && reader.CurrentY > 2 &&
                reader.CurrentX < reader.Width - 2 &&
                reader.CurrentY < reader.Height - 2)
            {
                //read 4 point sample
                MyColor[] colors = new MyColor[16];
                reader.SetStartPixel((int)cx, (int)cy);
                reader.Read16(colors);
                double x0 = (int)cx;
                double x1 = (int)(cx + 1);
                double xdiff = cx - x0;
                double y0 = (int)cy;
                double y1 = (int)(cy + 1);
                double ydiff = cy - y0;
                SeparateByChannel(colors, rBuffer, gBuffer, bBuffer, aBuffer);
                double result_B = myInterpolator.getValueBytes(bBuffer, xdiff, ydiff);
                double result_G = myInterpolator.getValueBytes(gBuffer, xdiff, ydiff);
                double result_R = myInterpolator.getValueBytes(rBuffer, xdiff, ydiff);
                double result_A = myInterpolator.getValueBytes(aBuffer, xdiff, ydiff);
                //clamp
                if (result_B > 255)
                {
                    result_B = 255;
                }
                else if (result_B < 0)
                {
                    result_B = 0;
                }

                if (result_G > 255)
                {
                    result_G = 255;
                }
                else if (result_G < 0)
                {
                    result_G = 0;
                }

                if (result_R > 255)
                {
                    result_R = 255;
                }
                else if (result_R < 0)
                {
                    result_R = 0;
                }

                if (result_A > 255)
                {
                    result_A = 255;
                }
                else if (result_A < 0)
                {
                    result_A = 0;
                }

                return new MyColor((byte)result_R, (byte)result_G, (byte)result_B, (byte)result_A);
            }
            else
            {
                return reader.ReadOnePixel();
            }
        }
    }
}