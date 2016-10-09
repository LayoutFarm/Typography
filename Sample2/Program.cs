using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NRasterizer;

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using PixelFarm.Agg;
using PixelFarm;

namespace Sample2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    public class Rasterizer2
    {
        private readonly Typeface _typeface;
        private const int pointsPerInch = 72;
        GraphicsPath gfxPath;
        public Rasterizer2(Typeface typeface)
        {
            _typeface = typeface;
        }
        const double FT_RESIZE = 64; //essential to be floating point
        PixelFarm.Agg.VertexSource.PathWriter ps = new PixelFarm.Agg.VertexSource.PathWriter();

        public PixelFarm.Agg.VertexStore MakeVxs()
        {
            return ps.Vxs;
        }
        void RenderGlyph(ushort[] contours, FtPoint[] ftpoints, Flag[] flags)
        {

            //outline version
            //-----------------------------
            int npoints = ftpoints.Length;
            List<PixelFarm.VectorMath.Vector2> points = new List<PixelFarm.VectorMath.Vector2>(npoints);
            int startContour = 0;
            int cpoint_index = 0;
            int todoContourCount = contours.Length;
            ps.Clear();

            int controlPointCount = 0;
            while (todoContourCount > 0)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                FtPointD secondControlPoint = new FtPointD();
                FtPointD thirdControlPoint = new FtPointD();
                bool justFromCurveMode = false;

                for (; cpoint_index < nextContour; ++cpoint_index)
                {
                    FtPoint vpoint = ftpoints[cpoint_index];
                    int vtag = (int)flags[cpoint_index];
                    //bool has_dropout = (((vtag >> 2) & 0x1) != 0);
                    //int dropoutMode = vtag >> 3;
                    if ((vtag & 0x1) != 0)
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        ps.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                                    }
                                    break;
                                case 2:
                                    {
                                        ps.Curve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                           thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                           vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                                    }
                                    break;
                                default:
                                    {
                                        throw new NotSupportedException();
                                    }
                            }
                            controlPointCount = 0;
                            justFromCurveMode = false;
                        }
                        else
                        {
                            if (isFirstPoint)
                            {
                                isFirstPoint = false;
                                ps.MoveTo(vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                            }
                            else
                            {
                                ps.LineTo(vpoint.X / FT_RESIZE, vpoint.Y / FT_RESIZE);
                            }

                            //if (has_dropout)
                            //{
                            //    //printf("[%d] on,dropoutMode=%d: %d,y:%d \n", mm, dropoutMode, vpoint.x, vpoint.y);
                            //}
                            //else
                            //{
                            //    //printf("[%d] on,x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                            //}
                        }
                    }
                    else
                    {
                        switch (controlPointCount)
                        {
                            case 0:
                                {   //bit 1 set=> off curve, this is a control point
                                    //if this is a 2nd order or 3rd order control point
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint);
                                    }
                                    else
                                    {
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
                                }
                                break;
                            case 1:
                                {
                                    if (((vtag >> 1) & 0x1) != 0)
                                    {
                                        //printf("[%d] bzc3rd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        thirdControlPoint = new FtPointD(vpoint.X, vpoint.Y);
                                    }
                                    else
                                    {
                                        //we already have prev second control point
                                        //so auto calculate line to 
                                        //between 2 point
                                        FtPointD mid = GetMidPoint(secondControlPoint, vpoint);
                                        //----------
                                        //generate curve3
                                        ps.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                            mid.x / FT_RESIZE, mid.y / FT_RESIZE);
                                        //------------------------
                                        controlPointCount--;
                                        //------------------------
                                        //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                        secondControlPoint = new FtPointD(vpoint);
                                    }
                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException();
                                }
                                break;
                        }

                        controlPointCount++;
                        justFromCurveMode = true;
                    }
                }
                //--------
                //close figure
                //if in curve mode
                if (justFromCurveMode)
                {
                    switch (controlPointCount)
                    {
                        case 0: break;
                        case 1:
                            {
                                ps.Curve3(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                    ps.LastMoveX, ps.LastMoveY);
                            }
                            break;
                        case 2:
                            {
                                ps.Curve4(secondControlPoint.x / FT_RESIZE, secondControlPoint.y / FT_RESIZE,
                                   thirdControlPoint.x / FT_RESIZE, thirdControlPoint.y / FT_RESIZE,
                                   ps.LastMoveX, ps.LastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException(); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                ps.CloseFigure();
                //--------                   
                startContour++;
                todoContourCount--;
            }
        }
        static FtPointD GetMidPoint(FtPoint v1, FtPoint v2)
        {
            return new FtPointD(
                ((double)v1.X + (double)v2.X) / 2d,
                ((double)v1.Y + (double)v2.Y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPointD v2)
        {
            return new FtPointD(
                ((double)v1.x + (double)v2.x) / 2d,
                ((double)v1.y + (double)v2.y) / 2d);
        }
        static FtPointD GetMidPoint(FtPointD v1, FtPoint v2)
        {
            return new FtPointD(
                (v1.x + (double)v2.X) / 2d,
                (v1.y + (double)v2.Y) / 2d);
        }

        void RenderGlyph(Glyph glyph, int resolution, int fx, int fy, int size, int x, int y)
        {
            float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
            ushort[] endPoints;
            Flag[] flags;
            FtPoint[] ftpoints = glyph.GetPoints(out endPoints, out flags);
            RenderGlyph(endPoints, ftpoints, flags);
        }

        public void Rasterize(string text, int size, int resolution, bool toFlags = false)
        {

            int fx = 64;
            int fy = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);
                RenderGlyph(glyph, resolution, fx, fy, size, 0, 70);
                fx += _typeface.GetAdvanceWidth(character);
            }

            //if (toFlags)
            //{
            //    RenderFlags(flags, raster);
            //}
            //else
            //{
            //    RenderScanlines(flags, raster);
            //}
        }

        // TODO: Duplicated code from Rasterize & SetScanFlags
        //public IEnumerable<Segment> GetAllSegments(string text, int size, int resolution)
        //{
        //    int x = 0;
        //    int y = 70;

        //    // 
        //    int fx = 64;
        //    int fy = 0;
        //    foreach (var character in text)
        //    {
        //        var glyph = _typeface.Lookup(character);

        //        float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);
        //        for (int contour = 0; contour < glyph.ContourCount; contour++)
        //        {
        //            var aerg = new List<Segment>(glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale));
        //            foreach (var segment in glyph.GetContourIterator(contour, fx, fy, x, y, scale, -scale))
        //            {
        //                yield return segment;
        //            }
        //        }

        //        fx += _typeface.GetAdvanceWidth(character);
        //    }
        //}
    }

    public static class BitmapHelper
    {
        public static void CopyToWindowsBitmap(ImageReaderWriterBase backingImageBufferByte,
            Bitmap windowsBitmap,
            RectInt rect)
        {
            int offset = 0;
            byte[] buffer = backingImageBufferByte.GetBuffer();
            BitmapHelper.CopyToWindowsBitmap(buffer, offset,
                backingImageBufferByte.Stride, backingImageBufferByte.Height,
                backingImageBufferByte.BitDepth,
                windowsBitmap, rect);
        }
        public static void CopyToWindowsBitmap(ActualImage backingImageBufferByte,
           Bitmap windowsBitmap,
           RectInt rect)
        {
            int offset = 0;
            byte[] buffer = backingImageBufferByte.GetBuffer();
            BitmapHelper.CopyToWindowsBitmap(buffer, offset,
                backingImageBufferByte.Stride, backingImageBufferByte.Height,
                backingImageBufferByte.BitDepth,
                windowsBitmap, rect);
        }
        public static void CopyToWindowsBitmap(byte[] buffer, int offset,
          int sBackBufferStrideInBytes, int sHeight,
          int bitDepth,
          Bitmap windowsBitmap,
          RectInt rect)
        {
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          windowsBitmap.Width,
                          windowsBitmap.Height),
                          ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            int backBufferStrideInInts = sBackBufferStrideInBytes / 4;
            int backBufferHeight = sHeight;
            int backBufferHeightMinusOne = backBufferHeight - 1;
            int bitmapDataStride = bitmapData1.Stride;
            switch (bitDepth)
            {
                case 24:
                    {
                        throw new NotSupportedException();
                        //unsafe
                        //{
                        //    byte* bitmapDataScan0 = (byte*)bitmapData1.Scan0;
                        //    fixed (byte* pSourceFixed = &buffer[offset])
                        //    {
                        //        byte* pSource = pSourceFixed;
                        //        byte* pDestBuffer = bitmapDataScan0 + bitmapDataStride * backBufferHeightMinusOne;
                        //        for (int y = 0; y < backBufferHeight; y++)
                        //        {
                        //            int* pSourceInt = (int*)pSource;
                        //            int* pDestBufferInt = (int*)pDestBuffer;
                        //            for (int x = 0; x < backBufferStrideInInts; x++)
                        //            {
                        //                pDestBufferInt[x] = pSourceInt[x];
                        //            }
                        //            for (int x = backBufferStrideInInts * 4; x < sBackBufferStrideInBytes; x++)
                        //            {
                        //                pDestBuffer[x] = pSource[x];
                        //            }
                        //            pDestBuffer -= bitmapDataStride;
                        //            pSource += sBackBufferStrideInBytes;
                        //        }
                        //    }
                        //}
                    }
                    break;
                case 32:
                    {
                        unsafe
                        {
                            byte* bitmapDataScan0 = (byte*)bitmapData1.Scan0;
                            fixed (byte* pSourceFixed = &buffer[offset])
                            {
                                byte* pSource = pSourceFixed;
                                byte* pDestBuffer = bitmapDataScan0 + bitmapDataStride * backBufferHeightMinusOne;
                                int rect_bottom = rect.Bottom;
                                int rect_top = rect.Top;
                                int rect_left = rect.Left;
                                int rect_right = rect.Right;
                                for (int y = rect_bottom; y < rect_top; y++)
                                {
                                    int* pSourceInt = (int*)pSource;
                                    pSourceInt += (sBackBufferStrideInBytes * y / 4);
                                    int* pDestBufferInt = (int*)pDestBuffer;
                                    pDestBufferInt -= (bitmapDataStride * y / 4);
                                    for (int x = rect_left; x < rect_right; x++)
                                    {
                                        pDestBufferInt[x] = pSourceInt[x];
                                    }
                                }
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            windowsBitmap.UnlockBits(bitmapData1);
        }
        public static void CopyToWindowsBitmapSameSize(
            ActualImage actualImage,
            Bitmap windowsBitmap)
        {
            int h = windowsBitmap.Height;
            int w = windowsBitmap.Width;
            byte[] buffer = actualImage.GetBuffer();
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          w,
                          h),
                          System.Drawing.Imaging.ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            IntPtr scan0 = bitmapData1.Scan0;
            int stride = bitmapData1.Stride;
            unsafe
            {
                fixed (byte* bufferH = &buffer[0])
                {
                    byte* target = (byte*)scan0;
                    for (int y = h; y > 0; --y)
                    {
                        byte* src = bufferH + ((y - 1) * stride);
                        for (int n = stride - 1; n >= 0; --n)
                        {
                            *target = *src;
                            target++;
                            src++;
                        }
                    }
                }
            }

            windowsBitmap.UnlockBits(bitmapData1);
        }

        public static void CopyFromWindowsBitmapSameSize(
           Bitmap windowsBitmap,
           ActualImage actualImage)
        {
            int h = windowsBitmap.Height;
            int w = windowsBitmap.Width;
            byte[] buffer = actualImage.GetBuffer();
            BitmapData bitmapData1 = windowsBitmap.LockBits(
                      new Rectangle(0, 0,
                          w,
                          h),
                          System.Drawing.Imaging.ImageLockMode.ReadWrite,
                          windowsBitmap.PixelFormat);
            IntPtr scan0 = bitmapData1.Scan0;
            int stride = bitmapData1.Stride;

            //TODO: review here 
            //use buffer copy

            unsafe
            {
                //target
                fixed (byte* targetH = &buffer[0])
                {
                    byte* src = (byte*)scan0;
                    for (int y = h; y > 0; --y)
                    {
                        byte* target = targetH + ((y - 1) * stride);
                        for (int n = stride - 1; n >= 0; --n)
                        {
                            *target = *src;
                            target++;
                            src++;
                        }
                    }
                }
            }
            windowsBitmap.UnlockBits(bitmapData1);
        }
    }
}
