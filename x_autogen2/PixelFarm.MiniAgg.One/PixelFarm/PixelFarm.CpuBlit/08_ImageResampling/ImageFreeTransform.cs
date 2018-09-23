//MIT, 2014-present, WinterDev
//---------------------------------------------
//some code from CodeProject: 'Free Image Transformation'
//YLS CS 
//license : CPOL

using System;
using PixelFarm.VectorMath;
using PixelFarm.CpuBlit.PixelProcessing; 
namespace PixelFarm.CpuBlit.Imaging
{

    public class FreeTransform
    {
        public enum InterpolationMode
        {
            None,
            Bilinear,
            Bicubic
        }

        class MyBitmapBlender : BitmapBlenderBase
        {
            public MyBitmapBlender(ActualBitmap img)
            {
                Attach(img);
            }
            public override void ReplaceBuffer(int[] newbuffer)
            {

            }
        }


        PointF _p0, _p1, _p2, _p3;
        Vector AB, BC, CD, DA;
        PixelFarm.Drawing.Rectangle _destBounds;
        int srcW = 0;
        int srcH = 0;

        IBitmapSrc _srcBmp;

        public FreeTransform()
        {
            Interpolation = InterpolationMode.Bilinear;
        }

        //public IBitmapSrc Bitmap
        //{
        //    get
        //    {
        //        return _srcBmp;
        //    }
        //    set
        //    {
        //        _srcBmp = value;

        //        if (value == null)
        //        {
        //            return;
        //        }
        //        try
        //        {
        //            _srcBmp = value;
        //            srcH = value.Height;
        //            srcW = value.Width;
        //        }
        //        catch
        //        {
        //            srcW = 0; srcH = 0;
        //        }
        //    }
        //}

        public Point ImageLocation
        {
            //left bottom?
            get { return new Point(_destBounds.Left, _destBounds.Bottom); }
        }
        public InterpolationMode Interpolation
        {
            get;
            set;
        }

        public int ImageWidth
        {
            get { return _destBounds.Width; }
        }

        public int ImageHeight
        {
            get { return _destBounds.Height; }
        }

        public PointF VertexLeftTop
        {
            get { return _p0; }
            set { _p0 = value; UpdateVertices(); }

        }

        public PointF VertexRightTop
        {
            get { return _p1; }
            set { _p1 = value; UpdateVertices(); }
        }

        public PointF VertexRightBottom
        {
            get { return _p2; }
            set
            {
                _p2 = value;
                UpdateVertices();
            }
        }
        public PointF VertexBottomLeft
        {
            get { return _p3; }
            set { _p3 = value; UpdateVertices(); }
        }
        public void SetFourCorners(PointF leftTop, PointF rightTop, PointF rightBottom, PointF leftBottom)
        {
            _p0 = leftTop;
            _p1 = rightTop;
            _p2 = rightBottom;
            _p3 = leftBottom;
            UpdateVertices();
        }

        void UpdateVertices()
        {
            float xmin = float.MaxValue;
            float ymin = float.MaxValue;
            float xmax = float.MinValue;
            float ymax = float.MinValue;

            {
                //update 4 corners
                //--------------------------
                xmin = Math.Min(xmin, _p0.X);
                xmax = Math.Max(xmax, _p0.X);
                ymin = Math.Min(ymin, _p0.Y);
                ymax = Math.Max(ymax, _p0.Y);
                //--------------------------
                xmin = Math.Min(xmin, _p1.X);
                xmax = Math.Max(xmax, _p1.X);
                ymin = Math.Min(ymin, _p1.Y);
                ymax = Math.Max(ymax, _p1.Y);
                //--------------------------
                xmin = Math.Min(xmin, _p2.X);
                xmax = Math.Max(xmax, _p2.X);
                ymin = Math.Min(ymin, _p2.Y);
                ymax = Math.Max(ymax, _p2.Y);
                //--------------------------
                xmin = Math.Min(xmin, _p3.X);
                xmax = Math.Max(xmax, _p3.X);
                ymin = Math.Min(ymin, _p3.Y);
                ymax = Math.Max(ymax, _p3.Y);
            }


            _destBounds = new Drawing.Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));
            AB = MyVectorHelper.NewFromTwoPoints(_p0, _p1);
            BC = MyVectorHelper.NewFromTwoPoints(_p1, _p2);
            CD = MyVectorHelper.NewFromTwoPoints(_p2, _p3);
            DA = MyVectorHelper.NewFromTwoPoints(_p3, _p0);
            //-----------------------------------------------------------------------
            // get unit vector
            AB /= AB.Magnitude;
            BC /= BC.Magnitude;
            CD /= CD.Magnitude;
            DA /= DA.Magnitude;
            //-----------------------------------------------------------------------

        }

        bool IsOnPlaneABCD(PointF pt) //  including point on border
        {
            return !MyVectorHelper.IsCCW(pt, _p0, _p1) &&
                   !MyVectorHelper.IsCCW(pt, _p1, _p2) &&
                   !MyVectorHelper.IsCCW(pt, _p2, _p3) &&
                   !MyVectorHelper.IsCCW(pt, _p3, _p0);
        }

        public ActualBitmap GetTransformedBitmap(IBitmapSrc bitmap)
        {
            _srcBmp = bitmap;

            if (bitmap == null)
            {
                return null;
            }

            _srcBmp = bitmap;
            srcH = bitmap.Height;
            srcW = bitmap.Width;
            

            //-------------------
            if (srcH == 0 || srcW == 0) return null;
            switch (this.Interpolation)
            {
                default: throw new NotSupportedException();
                case InterpolationMode.None:
                    return GetTransformedBitmapNoInterpolation();
                case InterpolationMode.Bilinear:
                    return GetTransformedBilinearInterpolation();
                case InterpolationMode.Bicubic:
                    return GetTransformedBicubicInterpolation();
            }


        }

        ActualBitmap GetTransformedBitmapNoInterpolation()
        {
            var destCB = new ActualBitmap(_destBounds.Width, _destBounds.Height);
            var destWriter = new MyBitmapBlender(destCB);
            PointF ptInPlane = new PointF();

            int x1, y1;
            double dab, dbc, dcd, dda;

            int rectWidth = _destBounds.Width;
            int rectHeight = _destBounds.Height;
            Vector ab_vec = this.AB;
            Vector bc_vec = this.BC;
            Vector cd_vec = this.CD;
            Vector da_vec = this.DA;
            int rectLeft = this._destBounds.Left;
            int rectTop = this._destBounds.Top;

            TempMemPtr bufferPtr = _srcBmp.GetBufferPtr();

            unsafe
            {


                BufferReader4 reader = new BufferReader4((int*)bufferPtr.Ptr, _srcBmp.Width, _srcBmp.Height);

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

                        reader.SetStartPixel(x1, y1);
                        destWriter.SetPixel(x, y, reader.Read1());
                        //-------------------------------------
                        dab = Math.Abs((MyVectorHelper.NewFromTwoPoints(_p0, srcPt)).CrossProduct(ab_vec));
                        dbc = Math.Abs((MyVectorHelper.NewFromTwoPoints(_p1, srcPt)).CrossProduct(bc_vec));
                        dcd = Math.Abs((MyVectorHelper.NewFromTwoPoints(_p2, srcPt)).CrossProduct(cd_vec));
                        dda = Math.Abs((MyVectorHelper.NewFromTwoPoints(_p3, srcPt)).CrossProduct(da_vec));
                        ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                        ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                    }
                }
                return destCB;
            }
        }

        unsafe ActualBitmap GetTransformedBilinearInterpolation()
        {
            //4 points sampling
            //weight between four point
            ActualBitmap destCB = new ActualBitmap(_destBounds.Width, _destBounds.Height);
            MyBitmapBlender destWriter = new MyBitmapBlender(destCB);
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = _destBounds.Width;
            int rectHeight = _destBounds.Height;
            Vector ab_vec = this.AB;
            Vector bc_vec = this.BC;
            Vector cd_vec = this.CD;
            Vector da_vec = this.DA;
            int rectLeft = this._destBounds.Left;
            int rectTop = this._destBounds.Top;

            int srcW_lim = srcW - 1;
            int srcH_lim = srcH - 1;


            TempMemPtr bufferPtr = _srcBmp.GetBufferPtr();
            BufferReader4 reader = new BufferReader4((int*)bufferPtr.Ptr, _srcBmp.Width, _srcBmp.Height);


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
                    dab = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p0, srcPt).CrossProduct(ab_vec));
                    dbc = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p1, srcPt).CrossProduct(bc_vec));
                    dcd = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p2, srcPt).CrossProduct(cd_vec));
                    dda = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p3, srcPt).CrossProduct(da_vec));

                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;

                    if (x1 >= 0 && x1 < srcW_lim &&
                        y1 >= 0 && y1 < srcH_lim)
                    {

                        //x2 = (x1 == srcW - 1) ? x1 : x1 + 1;
                        //y2 = (y1 == srcH - 1) ? y1 : y1 + 1;

                        x2 = x1 + 1;
                        y2 = y1 + 1;

                        dx1 = ptInPlane.X - x1;
                        if (dx1 < 0) dx1 = 0;
                        dx1 = 1f - dx1;
                        dx2 = 1f - dx1;
                        //
                        //
                        dy1 = ptInPlane.Y - y1;
                        if (dy1 < 0) dy1 = 0;
                        dy1 = 1f - dy1;
                        dy2 = 1f - dy1;
                        //
                        //
                        dx1y1 = dx1 * dy1;
                        dx1y2 = dx1 * dy2;
                        dx2y1 = dx2 * dy1;
                        dx2y2 = dx2 * dy2;
                        //use 4 points

                        reader.SetStartPixel(x1, y1);


                        Drawing.Color x1y1Color;
                        Drawing.Color x2y1Color;
                        Drawing.Color x1y2Color;
                        Drawing.Color x2y2Color;

                        reader.Read4(out x1y1Color, out x2y1Color, out x1y2Color, out x2y2Color);

                        //Drawing.Color x1y1Color = srcCB.GetPixel(x1, y1);
                        //Drawing.Color x2y1Color = srcCB.GetPixel(x2, y1);
                        //Drawing.Color x1y2Color = srcCB.GetPixel(x1, y2);
                        //Drawing.Color x2y2Color = srcCB.GetPixel(x2, y2);
                        float a = (x1y1Color.alpha * dx1y1) + (x2y1Color.alpha * dx2y1) + (x1y2Color.alpha * dx1y2) + (x2y2Color.alpha * dx2y2);
                        float b = (x1y1Color.blue * dx1y1) + (x2y1Color.blue * dx2y1) + (x1y2Color.blue * dx1y2) + (x2y2Color.blue * dx2y2);
                        float g = (x1y1Color.green * dx1y1) + (x2y1Color.green * dx2y1) + (x1y2Color.green * dx1y2) + (x2y2Color.green * dx2y2);
                        float r = (x1y1Color.red * dx1y1) + (x2y1Color.red * dx2y1) + (x1y2Color.red * dx1y2) + (x2y2Color.red * dx2y2);
                        destWriter.SetPixel(x, y, new Drawing.Color((byte)a, (byte)r, (byte)g, (byte)b));

                    }
                }
            }
            return destCB;
        }
        //ActualBitmap GetTransformedBilinearInterpolation()
        //{
        //    //4 points sampling
        //    //weight between four point
        //    ActualBitmap destCB = new ActualBitmap(rect.Width, rect.Height);
        //    MyBitmapBlender destWriter = new MyBitmapBlender(destCB);
        //    PointF ptInPlane = new PointF();
        //    int x1, x2, y1, y2;
        //    double dab, dbc, dcd, dda;
        //    float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
        //    int rectWidth = rect.Width;
        //    int rectHeight = rect.Height;
        //    Vector ab_vec = this.AB;
        //    Vector bc_vec = this.BC;
        //    Vector cd_vec = this.CD;
        //    Vector da_vec = this.DA;
        //    int rectLeft = this.rect.Left;
        //    int rectTop = this.rect.Top;




        //    for (int y = 0; y < rectHeight; ++y)
        //    {
        //        for (int x = 0; x < rectWidth; ++x)
        //        {
        //            PointF srcPt = new PointF(x, y);
        //            srcPt.Offset(rectLeft, rectTop);
        //            if (!IsOnPlaneABCD(srcPt))
        //            {
        //                continue;
        //            }
        //            //-------------------------------------
        //            dab = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p0, srcPt).CrossProduct(ab_vec));
        //            dbc = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p1, srcPt).CrossProduct(bc_vec));
        //            dcd = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p2, srcPt).CrossProduct(cd_vec));
        //            dda = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p3, srcPt).CrossProduct(da_vec));

        //            ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
        //            ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
        //            x1 = (int)ptInPlane.X;
        //            y1 = (int)ptInPlane.Y;

        //            if (x1 >= 0 && x1 < srcW && y1 >= 0 && y1 < srcH)
        //            {
        //                //bilinear interpolation *** 
        //                x2 = (x1 == srcW - 1) ? x1 : x1 + 1;
        //                y2 = (y1 == srcH - 1) ? y1 : y1 + 1;
        //                dx1 = ptInPlane.X - x1;
        //                if (dx1 < 0) dx1 = 0;
        //                dx1 = 1f - dx1;
        //                dx2 = 1f - dx1;
        //                dy1 = ptInPlane.Y - y1;
        //                if (dy1 < 0) dy1 = 0;
        //                dy1 = 1f - dy1;
        //                dy2 = 1f - dy1;
        //                dx1y1 = dx1 * dy1;
        //                dx1y2 = dx1 * dy2;
        //                dx2y1 = dx2 * dy1;
        //                dx2y2 = dx2 * dy2;
        //                //use 4 points


        //                Drawing.Color x1y1Color = srcCB.GetPixel(x1, y1);
        //                Drawing.Color x2y1Color = srcCB.GetPixel(x2, y1);
        //                Drawing.Color x1y2Color = srcCB.GetPixel(x1, y2);
        //                Drawing.Color x2y2Color = srcCB.GetPixel(x2, y2);
        //                float a = (x1y1Color.alpha * dx1y1) + (x2y1Color.alpha * dx2y1) + (x1y2Color.alpha * dx1y2) + (x2y2Color.alpha * dx2y2);
        //                float b = (x1y1Color.blue * dx1y1) + (x2y1Color.blue * dx2y1) + (x1y2Color.blue * dx1y2) + (x2y2Color.blue * dx2y2);
        //                float g = (x1y1Color.green * dx1y1) + (x2y1Color.green * dx2y1) + (x1y2Color.green * dx1y2) + (x2y2Color.green * dx2y2);
        //                float r = (x1y1Color.red * dx1y1) + (x2y1Color.red * dx2y1) + (x1y2Color.red * dx1y2) + (x2y2Color.red * dx2y2);
        //                destWriter.SetPixel(x, y, new Drawing.Color((byte)a, (byte)b, (byte)g, (byte)r));
        //            }
        //        }
        //    }
        //    return destCB;
        //}
        unsafe ActualBitmap GetTransformedBicubicInterpolation()
        {
            //4 points sampling
            //weight between four point 
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            //float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int destRectWidth = _destBounds.Width;
            int dectRectHeight = _destBounds.Height;
            Vector ab_vec = this.AB;
            Vector bc_vec = this.BC;
            Vector cd_vec = this.CD;
            Vector da_vec = this.DA;


            TempMemPtr bufferPtr = _srcBmp.GetBufferPtr();
            BufferReader4 reader = new BufferReader4((int*)bufferPtr.Ptr, _srcBmp.Width, _srcBmp.Height);

            ActualBitmap destCB = new ActualBitmap(_destBounds.Width, _destBounds.Height);
            MyBitmapBlender destWriter = new MyBitmapBlender(destCB);
            int rectLeft = this._destBounds.Left;
            int rectTop = this._destBounds.Top;

            //***
            PixelFarm.Drawing.Color[] colors = new PixelFarm.Drawing.Color[16];

            int srcW_lim = srcW - 2;
            int srcH_lim = srcH - 2;

            for (int y = 0; y < dectRectHeight; ++y)
            {
                for (int x = 0; x < destRectWidth; ++x)
                {
                    PointF srcPt = new PointF(x, y);
                    srcPt.Offset(rectLeft, 0);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    //-------------------------------------
                    dab = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p0, srcPt).CrossProduct(ab_vec));
                    dbc = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p1, srcPt).CrossProduct(bc_vec));
                    dcd = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p2, srcPt).CrossProduct(cd_vec));
                    dda = Math.Abs(MyVectorHelper.NewFromTwoPoints(_p3, srcPt).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;
                    if (x1 >= 2 && x1 < srcW_lim &&
                        y1 >= 2 && y1 < srcH_lim)
                    {
                        reader.SetStartPixel(x1, y1);
                        //reader.Read16(pixelBuffer);
                        //do interpolate 
                        //find src pixel and approximate   
                        destWriter.SetPixel(x, y,

                              GetApproximateColor_Bicubic(reader,
                                colors,
                                ptInPlane.X,
                                ptInPlane.Y)); //TODO:review here blue switch to red channel
                    }
                }
                //newline
                // startLine += stride2;
                //targetPixelIndex = startLine;
            }

            bufferPtr.Release();
            //------------------------
            //System.Runtime.InteropServices.Marshal.Copy(
            //outputBuffer, 0,
            //bmpdata2.Scan0, outputBuffer.Length);
            //outputbmp.UnlockBits(bmpdata2);
            ////outputbmp.Save("d:\\WImageTest\\n_lion_bicubic.png");
            //return outputbmp;
            return destCB;
        }

        static PixelFarm.Drawing.Color GetApproximateColor_Bicubic(BufferReader4 reader, PixelFarm.Drawing.Color[] colors, double cx, double cy)
        {
            if (reader.CanReadAsBlock())
            {
                //read 4 point sample  
                reader.Read16(colors);
                //
                BicubicInterpolator2.GetInterpolatedColor(colors, cx - ((int)cx) /*xdiff*/, cy - ((int)cy)/*ydiff*/, out PixelFarm.Drawing.Color result);
                return result;
            }
            else
            {
                return reader.ReadOnePixel();
            }
        }
    }
}