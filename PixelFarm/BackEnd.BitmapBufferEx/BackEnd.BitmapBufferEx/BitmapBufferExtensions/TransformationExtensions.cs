//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx

//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of transformation extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 18:18:24 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113191 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapTransformationExtensions.cs $
//   Id:                $Id: WriteableBitmapTransformationExtensions.cs 113191 2015-03-05 17:18:24Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//


using System;
namespace BitmapBufferEx
{
    /// <summary>
    /// Collection of transformation extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {

        /// <summary>
        /// The interpolation method.
        /// </summary>
        public enum Interpolation
        {
            /// <summary>
            /// The nearest neighbor algorithm simply selects the color of the nearest pixel.
            /// </summary>
            NearestNeighbor = 0,

            /// <summary>
            /// Linear interpolation in 2D using the average of 3 neighboring pixels.
            /// </summary>
            Bilinear,
        }

        /// <summary>
        /// The mode for flipping.
        /// </summary>
        public enum FlipMode
        {
            /// <summary>
            /// Flips the image vertical (around the center of the y-axis).
            /// </summary>
            Vertical,

            /// <summary>
            /// Flips the image horizontal (around the center of the x-axis).
            /// </summary>
            Horizontal
        }




        /// <summary>
        /// Creates a new cropped WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the rectangle that defines the crop region.</param>
        /// <param name="y">The y coordinate of the rectangle that defines the crop region.</param>
        /// <param name="width">The width of the rectangle that defines the crop region.</param>
        /// <param name="height">The height of the rectangle that defines the crop region.</param>
        /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
        public static BitmapBuffer Crop(this BitmapBuffer bmp, int x, int y, int width, int height)
        {
            using (BitmapContext srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int srcWidth = srcContext.Width;
                int srcHeight = srcContext.Height;

                // If the rectangle is completely out of the bitmap
                if (x > srcWidth || y > srcHeight)
                {
                    return BitmapBufferFactory.New(0, 0);
                }

                // Clamp to boundaries
                if (x < 0) x = 0;
                if (x + width > srcWidth) width = srcWidth - x;
                if (y < 0) y = 0;
                if (y + height > srcHeight) height = srcHeight - y;

                // Copy the pixels line by line using fast BlockCopy
                BitmapBuffer result = BitmapBufferFactory.New(width, height);
                using (BitmapContext destContext = result.GetBitmapContext())
                {
                    for (int line = 0; line < height; line++)
                    {
                        int srcOff = ((y + line) * srcWidth + x) * ARGB_SIZE;
                        int dstOff = line * width * ARGB_SIZE;
                        BitmapContext.BlockCopy(srcContext, srcOff, destContext, dstOff, width * ARGB_SIZE);
                    } 
                    return result;
                }
            }
        }

#if DEBUG
        /// <summary>
        /// Creates a new cropped WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="region">The rectangle that defines the crop region.</param>
        /// <returns>A new WriteableBitmap that is a cropped version of the input.</returns>
        public static BitmapBuffer Crop(this BitmapBuffer bmp, RectD region)
        {
            return bmp.Crop((int)region.X, (int)region.Y, (int)region.Width, (int)region.Height);
        }
#endif

        /// <summary>
        /// Creates a new resized WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new WriteableBitmap that is a resized version of the input.</returns>
        public static BitmapBuffer Resize(this BitmapBuffer bmp, int width, int height, Interpolation interpolation)
        {
            using (BitmapContext srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int[] pd = Resize(srcContext, srcContext.Width, srcContext.Height, width, height, interpolation);

                BitmapBuffer result = BitmapBufferFactory.New(width, height);
                using (BitmapContext dstContext = result.GetBitmapContext())
                {
                    BitmapContext.BlockCopy(pd, 0, dstContext, 0, ARGB_SIZE * pd.Length);
                }
                return result;
            }
        }

        /// <summary>
        /// Creates a new resized bitmap.
        /// </summary>
        /// <param name="srcContext">The source context.</param>
        /// <param name="widthSource">The width of the source pixels.</param>
        /// <param name="heightSource">The height of the source pixels.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new bitmap that is a resized version of the input.</returns>
        public static int[] Resize(BitmapContext srcContext, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
        {
            return Resize(srcContext.Pixels, widthSource, heightSource, width, height, interpolation);
        }

        /// <summary>
        /// Creates a new resized bitmap.
        /// </summary>
        /// <param name="pixels">The source pixels.</param>
        /// <param name="widthSource">The width of the source pixels.</param>
        /// <param name="heightSource">The height of the source pixels.</param>
        /// <param name="width">The new desired width.</param>
        /// <param name="height">The new desired height.</param>
        /// <param name="interpolation">The interpolation method that should be used.</param>
        /// <returns>A new bitmap that is a resized version of the input.</returns>
#if WPF
        public static int[] Resize(int* pixels, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
#else
        public static int[] Resize(int[] pixels, int widthSource, int heightSource, int width, int height, Interpolation interpolation)
#endif
        {
            int[] pd = new int[width * height];
            float xs = (float)widthSource / width;
            float ys = (float)heightSource / height;

            float fracx, fracy, ifracx, ifracy, sx, sy, l0, l1, rf, gf, bf;
            int c, x0, x1, y0, y1;
            byte c1a, c1r, c1g, c1b, c2a, c2r, c2g, c2b, c3a, c3r, c3g, c3b, c4a, c4r, c4g, c4b;
            byte a, r, g, b;

            // Nearest Neighbor
            if (interpolation == Interpolation.NearestNeighbor)
            {
                int srcIdx = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        pd[srcIdx++] = pixels[y0 * widthSource + x0];
                    }
                }
            }

            // Bilinear
            else if (interpolation == Interpolation.Bilinear)
            {
                int srcIdx = 0;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        sx = x * xs;
                        sy = y * ys;
                        x0 = (int)sx;
                        y0 = (int)sy;

                        // Calculate coordinates of the 4 interpolation points
                        fracx = sx - x0;
                        fracy = sy - y0;
                        ifracx = 1f - fracx;
                        ifracy = 1f - fracy;
                        x1 = x0 + 1;
                        if (x1 >= widthSource)
                        {
                            x1 = x0;
                        }
                        y1 = y0 + 1;
                        if (y1 >= heightSource)
                        {
                            y1 = y0;
                        }


                        // Read source color
                        c = pixels[y0 * widthSource + x0];
                        c1a = (byte)(c >> 24);
                        c1r = (byte)(c >> 16);
                        c1g = (byte)(c >> 8);
                        c1b = (byte)(c);

                        c = pixels[y0 * widthSource + x1];
                        c2a = (byte)(c >> 24);
                        c2r = (byte)(c >> 16);
                        c2g = (byte)(c >> 8);
                        c2b = (byte)(c);

                        c = pixels[y1 * widthSource + x0];
                        c3a = (byte)(c >> 24);
                        c3r = (byte)(c >> 16);
                        c3g = (byte)(c >> 8);
                        c3b = (byte)(c);

                        c = pixels[y1 * widthSource + x1];
                        c4a = (byte)(c >> 24);
                        c4r = (byte)(c >> 16);
                        c4g = (byte)(c >> 8);
                        c4b = (byte)(c);


                        // Calculate colors
                        // Alpha
                        l0 = ifracx * c1a + fracx * c2a;
                        l1 = ifracx * c3a + fracx * c4a;
                        a = (byte)(ifracy * l0 + fracy * l1);

                        // Red
                        l0 = ifracx * c1r + fracx * c2r;
                        l1 = ifracx * c3r + fracx * c4r;
                        rf = ifracy * l0 + fracy * l1;

                        // Green
                        l0 = ifracx * c1g + fracx * c2g;
                        l1 = ifracx * c3g + fracx * c4g;
                        gf = ifracy * l0 + fracy * l1;

                        // Blue
                        l0 = ifracx * c1b + fracx * c2b;
                        l1 = ifracx * c3b + fracx * c4b;
                        bf = ifracy * l0 + fracy * l1;

                        // Cast to byte
                        r = (byte)rf;
                        g = (byte)gf;
                        b = (byte)bf;

                        // Write destination
                        pd[srcIdx++] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }
            }
            return pd;
        }

        public enum FastRotateAngle
        {
            Rotate0,
            Rotate90,
            Rotate180,
            Rotate270,
        }

        /// <summary>
        /// Rotates the bitmap in 90° steps clockwise and returns a new rotated WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="angle">The angle in degrees the bitmap should be rotated in 90° steps clockwise.</param>
        /// <returns>A new WriteableBitmap that is a rotated version of the input.</returns>
        public static BitmapBuffer Rotate(this BitmapBuffer bmp, FastRotateAngle angle)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                int[] p = context.Pixels;
                int i = 0;


                switch (angle)
                {
                    default:
                        {
                            return bmp.Clone();
                        }
                    case FastRotateAngle.Rotate90:
                        {
                            var result = BitmapBufferFactory.New(h, w);
                            using (BitmapContext destContext = result.GetBitmapContext())
                            {
                                var rp = destContext.Pixels;
                                for (int x = 0; x < w; x++)
                                {
                                    for (int y = h - 1; y >= 0; y--)
                                    {
                                        int srcInd = y * w + x;
                                        rp[i] = p[srcInd];
                                        i++;
                                    }
                                }
                            }
                            return result;
                        }

                    case FastRotateAngle.Rotate180:
                        {
                            var result = BitmapBufferFactory.New(w, h);
                            using (BitmapContext destContext = result.GetBitmapContext())
                            {
                                var rp = destContext.Pixels;
                                for (int y = h - 1; y >= 0; y--)
                                {
                                    for (int x = w - 1; x >= 0; x--)
                                    {
                                        int srcInd = y * w + x;
                                        rp[i] = p[srcInd];
                                        i++;
                                    }
                                }
                            }
                            return result;
                        }
                    case FastRotateAngle.Rotate270:
                        {
                            var result = BitmapBufferFactory.New(h, w);
                            using (BitmapContext destContext = result.GetBitmapContext())
                            {
                                int[] rp = destContext.Pixels;
                                for (int x = w - 1; x >= 0; x--)
                                {
                                    for (int y = 0; y < h; y++)
                                    {
                                        int srcInd = y * w + x;
                                        rp[i] = p[srcInd];
                                        i++;
                                    }
                                }
                            }
                            return result;
                        }
                }
            }
        }

        /// <summary>
        /// Rotates the bitmap in any degree returns a new rotated WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="angle">Arbitrary angle in 360 Degrees (positive = clockwise).</param>
        /// <param name="crop">if true: keep the size, false: adjust canvas to new size</param>
        /// <returns>A new WriteableBitmap that is a rotated version of the input.</returns>
        public static BitmapBuffer RotateFree(this BitmapBuffer bmp, double angle, bool crop = true)
        {
            // rotating clockwise, so it's negative relative to Cartesian quadrants
            double cnAngle = -1.0 * (Math.PI / 180) * angle;

            // general iterators
            int i, j;
            // calculated indices in Cartesian coordinates
            int x, y;
            double fDistance, fPolarAngle;
            // for use in neighboring indices in Cartesian coordinates
            int iFloorX, iCeilingX, iFloorY, iCeilingY;
            // calculated indices in Cartesian coordinates with trailing decimals
            double fTrueX, fTrueY;
            // for interpolation
            double fDeltaX, fDeltaY;

            // interpolated "top" pixels
            double fTopRed, fTopGreen, fTopBlue, fTopAlpha;

            // interpolated "bottom" pixels
            double fBottomRed, fBottomGreen, fBottomBlue, fBottomAlpha;

            // final interpolated color components
            int iRed, iGreen, iBlue, iAlpha;

            int iCentreX, iCentreY;
            int iDestCentreX, iDestCentreY;
            int iWidth, iHeight, newWidth, newHeight;
            using (var bmpContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {

                iWidth = bmpContext.Width;
                iHeight = bmpContext.Height;

                if (crop)
                {
                    newWidth = iWidth;
                    newHeight = iHeight;
                }
                else
                {
                    double rad = angle / (180 / Math.PI);
                    newWidth = (int)Math.Ceiling(Math.Abs(Math.Sin(rad) * iHeight) + Math.Abs(Math.Cos(rad) * iWidth));
                    newHeight = (int)Math.Ceiling(Math.Abs(Math.Sin(rad) * iWidth) + Math.Abs(Math.Cos(rad) * iHeight));
                }


                iCentreX = iWidth / 2;
                iCentreY = iHeight / 2;

                iDestCentreX = newWidth / 2;
                iDestCentreY = newHeight / 2;

                BitmapBuffer bmBilinearInterpolation = BitmapBufferFactory.New(newWidth, newHeight);

                using (BitmapContext bilinearContext = bmBilinearInterpolation.GetBitmapContext())
                {
                    int[] newp = bilinearContext.Pixels;
                    int[] oldp = bmpContext.Pixels;
                    int oldw = bmpContext.Width;

                    // assigning pixels of destination image from source image
                    // with bilinear interpolation
                    for (i = 0; i < newHeight; ++i)
                    {
                        for (j = 0; j < newWidth; ++j)
                        {
                            // convert raster to Cartesian
                            x = j - iDestCentreX;
                            y = iDestCentreY - i;

                            // convert Cartesian to polar
                            fDistance = Math.Sqrt(x * x + y * y);
                            if (x == 0)
                            {
                                if (y == 0)
                                {
                                    // center of image, no rotation needed
                                    newp[i * newWidth + j] = oldp[iCentreY * oldw + iCentreX];
                                    continue;
                                }
                                if (y < 0)
                                {
                                    fPolarAngle = 1.5 * Math.PI;
                                }
                                else
                                {
                                    fPolarAngle = 0.5 * Math.PI;
                                }
                            }
                            else
                            {
                                fPolarAngle = Math.Atan2(y, x);
                            }

                            // the crucial rotation part
                            // "reverse" rotate, so minus instead of plus
                            fPolarAngle -= cnAngle;

                            // convert polar to Cartesian
                            fTrueX = fDistance * Math.Cos(fPolarAngle);
                            fTrueY = fDistance * Math.Sin(fPolarAngle);

                            // convert Cartesian to raster
                            fTrueX = fTrueX + iCentreX;
                            fTrueY = iCentreY - fTrueY;

                            iFloorX = (int)(Math.Floor(fTrueX));
                            iFloorY = (int)(Math.Floor(fTrueY));
                            iCeilingX = (int)(Math.Ceiling(fTrueX));
                            iCeilingY = (int)(Math.Ceiling(fTrueY));

                            // check bounds
                            if (iFloorX < 0 || iCeilingX < 0 || iFloorX >= iWidth || iCeilingX >= iWidth || iFloorY < 0 ||
                                iCeilingY < 0 || iFloorY >= iHeight || iCeilingY >= iHeight) continue;

                            fDeltaX = fTrueX - iFloorX;
                            fDeltaY = fTrueY - iFloorY;

                            int clrTopLeft = oldp[iFloorY * oldw + iFloorX];
                            int clrTopRight = oldp[iFloorY * oldw + iCeilingX];
                            int clrBottomLeft = oldp[iCeilingY * oldw + iFloorX];
                            int clrBottomRight = oldp[iCeilingY * oldw + iCeilingX];

                            fTopAlpha = (1 - fDeltaX) * ((clrTopLeft >> 24) & 0xFF) + fDeltaX * ((clrTopRight >> 24) & 0xFF);
                            fTopRed = (1 - fDeltaX) * ((clrTopLeft >> 16) & 0xFF) + fDeltaX * ((clrTopRight >> 16) & 0xFF);
                            fTopGreen = (1 - fDeltaX) * ((clrTopLeft >> 8) & 0xFF) + fDeltaX * ((clrTopRight >> 8) & 0xFF);
                            fTopBlue = (1 - fDeltaX) * (clrTopLeft & 0xFF) + fDeltaX * (clrTopRight & 0xFF);

                            // linearly interpolate horizontally between bottom neighbors
                            fBottomAlpha = (1 - fDeltaX) * ((clrBottomLeft >> 24) & 0xFF) + fDeltaX * ((clrBottomRight >> 24) & 0xFF);
                            fBottomRed = (1 - fDeltaX) * ((clrBottomLeft >> 16) & 0xFF) + fDeltaX * ((clrBottomRight >> 16) & 0xFF);
                            fBottomGreen = (1 - fDeltaX) * ((clrBottomLeft >> 8) & 0xFF) + fDeltaX * ((clrBottomRight >> 8) & 0xFF);
                            fBottomBlue = (1 - fDeltaX) * (clrBottomLeft & 0xFF) + fDeltaX * (clrBottomRight & 0xFF);

                            // linearly interpolate vertically between top and bottom interpolated results
                            iRed = (int)(Math.Round((1 - fDeltaY) * fTopRed + fDeltaY * fBottomRed));
                            iGreen = (int)(Math.Round((1 - fDeltaY) * fTopGreen + fDeltaY * fBottomGreen));
                            iBlue = (int)(Math.Round((1 - fDeltaY) * fTopBlue + fDeltaY * fBottomBlue));
                            iAlpha = (int)(Math.Round((1 - fDeltaY) * fTopAlpha + fDeltaY * fBottomAlpha));

                            // make sure color values are valid
                            if (iRed < 0) iRed = 0;
                            if (iRed > 255) iRed = 255;
                            if (iGreen < 0) iGreen = 0;
                            if (iGreen > 255) iGreen = 255;
                            if (iBlue < 0) iBlue = 0;
                            if (iBlue > 255) iBlue = 255;
                            if (iAlpha < 0) iAlpha = 0;
                            if (iAlpha > 255) iAlpha = 255;

                            int a = iAlpha + 1;
                            newp[i * newWidth + j] = (iAlpha << 24)
                                                   | ((byte)((iRed * a) >> 8) << 16)
                                                   | ((byte)((iGreen * a) >> 8) << 8)
                                                   | ((byte)((iBlue * a) >> 8));
                        }
                    }
                    return bmBilinearInterpolation;
                }
            }
        }



        /// <summary>
        /// Flips (reflects the image) either vertical or horizontal.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="flipMode">The flip mode.</param>
        /// <returns>A new WriteableBitmap that is a flipped version of the input.</returns>
        public static BitmapBuffer Flip(this BitmapBuffer bmp, FlipMode flipMode)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                int[] p = context.Pixels;
                int i = 0;
                BitmapBuffer result = BitmapBufferFactory.New(w, h);

                switch (flipMode)
                {
                    default:
                        throw new NotSupportedException();
                    case FlipMode.Vertical:
                        using (BitmapContext destContext = result.GetBitmapContext())
                        {
                            int[] rp = destContext.Pixels;
                            for (int y = h - 1; y >= 0; y--)
                            {
                                for (int x = 0; x < w; x++)
                                {
                                    int srcInd = y * w + x;
                                    rp[i] = p[srcInd];
                                    i++;
                                }
                            }
                        }
                        break;
                    case FlipMode.Horizontal:
                        using (BitmapContext destContext = result.GetBitmapContext())
                        {
                            int[] rp = destContext.Pixels;
                            for (int y = 0; y < h; y++)
                            {
                                for (int x = w - 1; x >= 0; x--)
                                {
                                    int srcInd = y * w + x;
                                    rp[i] = p[srcInd];
                                    i++;
                                }
                            }
                        }
                        break;
                }
                return result;
            }
        }

    }
}