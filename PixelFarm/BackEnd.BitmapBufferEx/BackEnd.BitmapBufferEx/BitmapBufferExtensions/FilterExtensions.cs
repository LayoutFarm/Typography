//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of transformation extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 18:18:24 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113191 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapFilterExtensions.cs $
//   Id:                $Id: WriteableBitmapFilterExtensions.cs 113191 2015-03-05 17:18:24Z unknown $
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
    /// Collection of filter / convolution extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {

        ///<summary>
        /// Gaussian blur kernel with the size 5x5
        ///</summary>
        public static int[,] KernelGaussianBlur5x5 = {
                                                       {1,  4,  7,  4, 1},
                                                       {4, 16, 26, 16, 4},
                                                       {7, 26, 41, 26, 7},
                                                       {4, 16, 26, 16, 4},
                                                       {1,  4,  7,  4, 1}
                                                 };

        ///<summary>
        /// Gaussian blur kernel with the size 3x3
        ///</summary>
        public static int[,] KernelGaussianBlur3x3 = {
                                                       {16, 26, 16},
                                                       {26, 41, 26},
                                                       {16, 26, 16}
                                                    };

        ///<summary>
        /// Sharpen kernel with the size 3x3
        ///</summary>
        public static int[,] KernelSharpen3x3 = {
                                                 { 0, -2,  0},
                                                 {-2, 11, -2},
                                                 { 0, -2,  0}
                                              };




        /// <summary>
        /// Creates a new filtered WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="kernel">The kernel used for convolution.</param>
        /// <returns>A new WriteableBitmap that is a filtered version of the input.</returns>
        public static BitmapBuffer Convolute(this BitmapBuffer bmp, int[,] kernel)
        {
            int kernelFactorSum = 0;
            foreach (int b in kernel)
            {
                kernelFactorSum += b;
            }
            return bmp.Convolute(kernel, kernelFactorSum, 0);
        }

        /// <summary>
        /// Creates a new filtered WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="kernel">The kernel used for convolution.</param>
        /// <param name="kernelFactorSum">The factor used for the kernel summing.</param>
        /// <param name="kernelOffsetSum">The offset used for the kernel summing.</param>
        /// <returns>A new WriteableBitmap that is a filtered version of the input.</returns>
        public static BitmapBuffer Convolute(this BitmapBuffer bmp, int[,] kernel, int kernelFactorSum, int kernelOffsetSum)
        {
            int kh = kernel.GetUpperBound(0) + 1;
            int kw = kernel.GetUpperBound(1) + 1;

            if ((kw & 1) == 0)
            {
                throw new System.InvalidOperationException("Kernel width must be odd!");
            }
            if ((kh & 1) == 0)
            {
                throw new System.InvalidOperationException("Kernel height must be odd!");
            }

            using (BitmapContext srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int w = srcContext.Width;
                int h = srcContext.Height;
                BitmapBuffer result = BitmapBufferFactory.New(w, h);

                using (BitmapContext resultContext = result.GetBitmapContext())
                {
                    int[] pixels = srcContext.Pixels;
                    int[] resultPixels = resultContext.Pixels;
                    int index = 0;
                    int kwh = kw >> 1;
                    int khh = kh >> 1;

                    for (int y = 0; y < h; y++)
                    {
                        for (int x = 0; x < w; x++)
                        {
                            int a = 0;
                            int r = 0;
                            int g = 0;
                            int b = 0;

                            for (int kx = -kwh; kx <= kwh; kx++)
                            {
                                int px = kx + x;
                                // Repeat pixels at borders
                                if (px < 0)
                                {
                                    px = 0;
                                }
                                else if (px >= w)
                                {
                                    px = w - 1;
                                }

                                for (int ky = -khh; ky <= khh; ky++)
                                {
                                    int py = ky + y;
                                    // Repeat pixels at borders
                                    if (py < 0)
                                    {
                                        py = 0;
                                    }
                                    else if (py >= h)
                                    {
                                        py = h - 1;
                                    }

                                    int col = pixels[py * w + px];
                                    int k = kernel[ky + kwh, kx + khh];
                                    a += ((col >> 24) & 0xff) * k;
                                    r += ((col >> 16) & 0xff) * k;
                                    g += ((col >> 8) & 0xff) * k;
                                    b += ((col) & 0xff) * k;
                                }
                            }

                            int ta = ((a / kernelFactorSum) + kernelOffsetSum);
                            int tr = ((r / kernelFactorSum) + kernelOffsetSum);
                            int tg = ((g / kernelFactorSum) + kernelOffsetSum);
                            int tb = ((b / kernelFactorSum) + kernelOffsetSum);

                            // Clamp to byte boundaries
                            byte ba = (byte)((ta > 255) ? 255 : ((ta < 0) ? 0 : ta));
                            byte br = (byte)((tr > 255) ? 255 : ((tr < 0) ? 0 : tr));
                            byte bg = (byte)((tg > 255) ? 255 : ((tg < 0) ? 0 : tg));
                            byte bb = (byte)((tb > 255) ? 255 : ((tb < 0) ? 0 : tb));

                            resultPixels[index++] = (ba << 24) | (br << 16) | (bg << 8) | (bb);
                        }
                    }
                    return result;
                }
            }
        }



        /// <summary>
        /// Creates a new inverted WriteableBitmap and returns it.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>The new inverted WriteableBitmap.</returns>
        public static BitmapBuffer Invert(this BitmapBuffer bmp)
        {
            using (BitmapContext srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                var result = BitmapBufferFactory.New(srcContext.Width, srcContext.Height);
                using (BitmapContext resultContext = result.GetBitmapContext())
                {
                    int[] rp = resultContext.Pixels;
                    int[] p = srcContext.Pixels;
                    int length = srcContext.Length;

                    for (int i = 0; i < length; i++)
                    {
                        // Extract
                        int c = p[i];
                        int a = (c >> 24) & 0xff;
                        int r = (c >> 16) & 0xff;
                        int g = (c >> 8) & 0xff;
                        int b = (c) & 0xff;

                        // Invert
                        r = 255 - r;
                        g = 255 - g;
                        b = 255 - b;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }

                    return result;
                }
            }
        }



        /// <summary>
        /// Creates a new WriteableBitmap which is the grayscaled version of this one and returns it. The gray values are equal to the brightness values. 
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>The new gray WriteableBitmap.</returns>
        public static BitmapBuffer Gray(this BitmapBuffer bmp)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int nWidth = context.Width;
                int nHeight = context.Height;
                int[] px = context.Pixels;
                BitmapBuffer result = BitmapBufferFactory.New(nWidth, nHeight);

                using (BitmapContext dest = result.GetBitmapContext())
                {
                    int[] rp = dest.Pixels;
                    int len = context.Length;
                    for (int i = 0; i < len; i++)
                    {
                        // Extract
                        int c = px[i];
                        int a = (c >> 24) & 0xff;
                        int r = (c >> 16) & 0xff;
                        int g = (c >> 8) & 0xff;
                        int b = (c) & 0xff;

                        // Convert to gray with constant factors 0.2126, 0.7152, 0.0722
                        r = g = b = ((r * 6966 + g * 23436 + b * 2366) >> 15);

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Creates a new WriteableBitmap which is contrast adjusted version of this one and returns it.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="level">Level of contrast as double. [-255.0, 255.0] </param>
        /// <returns>The new WriteableBitmap.</returns>
        public static BitmapBuffer AdjustContrast(this BitmapBuffer bmp, double level)
        {
            int factor = (int)((259.0 * (level + 255.0)) / (255.0 * (259.0 - level)) * 255.0);

            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int nWidth = context.Width;
                int nHeight = context.Height;
                int[] px = context.Pixels;
                BitmapBuffer result = BitmapBufferFactory.New(nWidth, nHeight);

                using (BitmapContext dest = result.GetBitmapContext())
                {
                    int[] rp = dest.Pixels;
                    int len = context.Length;
                    for (int i = 0; i < len; i++)
                    {
                        // Extract
                        int c = px[i];
                        int a = (c >> 24) & 0xff;
                        int r = (c >> 16) & 0xff;
                        int g = (c >> 8) & 0xff;
                        int b = (c) & 0xff;

                        // Adjust contrast based on computed factor
                        //TODO: create lookup table for this
                        r = ((factor * (r - 128)) >> 8) + 128;
                        g = ((factor * (g - 128)) >> 8) + 128;
                        b = ((factor * (b - 128)) >> 8) + 128;

                        // Clamp
                        r = r < 0 ? 0 : r > 255 ? 255 : r;
                        g = g < 0 ? 0 : g > 255 ? 255 : g;
                        b = b < 0 ? 0 : b > 255 ? 255 : b;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Creates a new WriteableBitmap which is brightness adjusted version of this one and returns it.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="nLevel">Level of contrast as double. [-255.0, 255.0] </param>
        /// <returns>The new WriteableBitmap.</returns>
        public static BitmapBuffer AdjustBrightness(this BitmapBuffer bmp, int nLevel)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int nWidth = context.Width;
                int nHeight = context.Height;
                int[] px = context.Pixels;
                BitmapBuffer result = BitmapBufferFactory.New(nWidth, nHeight);

                using (BitmapContext dest = result.GetBitmapContext())
                {
                    int[] rp = dest.Pixels;
                    int len = context.Length;
                    for (int i = 0; i < len; i++)
                    {
                        // Extract
                        int c = px[i];
                        int a = (c >> 24) & 0xff;
                        int r = (c >> 16) & 0xff;
                        int g = (c >> 8) & 0xff;
                        int b = (c) & 0xff;

                        // Brightness adjustment
                        //TODO: create lookup table for this
                        r += nLevel;
                        g += nLevel;
                        b += nLevel;

                        // Clamp                    
                        r = r < 0 ? 0 : r > 255 ? 255 : r;
                        g = g < 0 ? 0 : g > 255 ? 255 : g;
                        b = b < 0 ? 0 : b > 255 ? 255 : b;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Creates a new WriteableBitmap which is gamma adjusted version of this one and returns it.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="value">Value of gamma for adjustment. Original is 1.0.</param>
        /// <returns>The new WriteableBitmap.</returns>
        public static BitmapBuffer AdjustGamma(this BitmapBuffer bmp, double value)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int nWidth = context.Width;
                int nHeight = context.Height;
                int[] srcPixels = context.Pixels;
                BitmapBuffer result = BitmapBufferFactory.New(nWidth, nHeight);

                using (BitmapContext dest = result.GetBitmapContext())
                {
                    int[] rp = dest.Pixels;
                    var gammaCorrection = 1.0 / value;
                    int len = context.Length;
                    for (int i = 0; i < len; i++)
                    {
                        // Extract
                        int c = srcPixels[i];
                        int a = (c >> 24) & 0xff;
                        int r = (c >> 16) & 0xff;
                        int g = (c >> 8) & 0xff;
                        int b = (c) & 0xff;

                        //Gamma adjustment
                        //TODO: create gamma-lookup table for this ***
                        r = (int)(255.0 * Math.Pow((r / 255.0), gammaCorrection));
                        g = (int)(255.0 * Math.Pow((g / 255.0), gammaCorrection));
                        b = (int)(255.0 * Math.Pow((b / 255.0), gammaCorrection));

                        // Clamps
                        r = r < 0 ? 0 : r > 255 ? 255 : r;
                        g = g < 0 ? 0 : g > 255 ? 255 : g;
                        b = b < 0 ? 0 : b > 255 ? 255 : b;

                        // Set
                        rp[i] = (a << 24) | (r << 16) | (g << 8) | b;
                    }
                }

                return result;
            }
        }
    }
}