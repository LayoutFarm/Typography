//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 18:18:24 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113191 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapBaseExtensions.cs $
//   Id:                $Id: WriteableBitmapBaseExtensions.cs 113191 2015-03-05 17:18:24Z unknown $
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
    /// Collection of extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {

        internal const int ARGB_SIZE = 4;

        public static void Clear(this BitmapContext context, ColorInt color)
        {
            int colr = color.ToPreMultAlphaColor();
            int[] pixels = context.Pixels;
            int w = context.Width;
            int h = context.Height;
            int len = w * ARGB_SIZE;

            // Fill first line
            for (int x = 0; x < w; x++)
            {
                pixels[x] = colr;
            }

            // Copy first line
            int blockHeight = 1;
            int y = 1;
            while (y < h)
            {
                BitmapContext.BlockCopy(context, 0, context, y * len, blockHeight * len);
                y += blockHeight;
                blockHeight = Math.Min(2 * blockHeight, h - y);
            }
        }

        /// <summary>
        /// Fills the whole WriteableBitmap with a color.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="color">The color used for filling.</param>
        public static void Clear(this BitmapBuffer bmp, ColorInt color)
        {            
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                Clear(context, color);
            }
        }

        /// <summary>
        /// Fills the whole WriteableBitmap with an empty color (0).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        public static void Clear(this BitmapBuffer bmp)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Clear();
            }
        }

        /// <summary>
        /// Clones the specified WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>A copy of the WriteableBitmap.</returns>
        public static BitmapBuffer Clone(this BitmapBuffer bmp)
        {
            using (BitmapContext srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                BitmapBuffer result = BitmapBufferFactory.New(srcContext.Width, srcContext.Height);
                using (var destContext = result.GetBitmapContext())
                {
                    BitmapContext.BlockCopy(srcContext, 0, destContext, 0, srcContext.Length * ARGB_SIZE);
                }
                return result;
            }
        }


#if DEBUG
        ///// <summary>
        ///// Applies the given function to all the pixels of the bitmap in 
        ///// order to set their color.
        ///// </summary>
        ///// <param name="bmp">The WriteableBitmap.</param>
        ///// <param name="func">The function to apply. With parameters x, y and a color as a result</param>
        //public static void dbugForEach(this BitmapBuffer bmp, Func<int, int, ColorInt> func)
        //{
        //    using (var context = bmp.GetBitmapContext())
        //    {
        //        int[] pixels = context.Pixels;
        //        int w = context.Width;
        //        int h = context.Height;
        //        int index = 0;

        //        for (int y = 0; y < h; y++)
        //        {
        //            for (int x = 0; x < w; x++)
        //            {
        //                pixels[index++] = func(x, y).ToPreMultAlphaColor();
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Applies the given function to all the pixels of the bitmap in 
        ///// order to set their color.
        ///// </summary>
        ///// <param name="bmp">The WriteableBitmap.</param>
        ///// <param name="func">The function to apply. With parameters x, y, source color and a color as a result</param>
        //public static void dbugForEach(this BitmapBuffer bmp, Func<int, int, ColorInt, ColorInt> func)
        //{
        //    using (var context = bmp.GetBitmapContext())
        //    {
        //        int[] pixels = context.Pixels;
        //        int w = context.Width;
        //        int h = context.Height;
        //        int index = 0;

        //        for (int y = 0; y < h; y++)
        //        {
        //            for (int x = 0; x < w; x++)
        //            {
        //                int c = pixels[index];

        //                // Premultiplied Alpha!
        //                byte a = (byte)(c >> 24);
        //                // Prevent division by zero
        //                int ai = a;
        //                if (ai == 0)
        //                {
        //                    ai = 1;
        //                }
        //                // Scale inverse alpha to use cheap integer mul bit shift
        //                ai = ((255 << 8) / ai);
        //                ColorInt srcColor = ColorInt.FromArgb(a,
        //                                              (byte)((((c >> 16) & 0xFF) * ai) >> 8),
        //                                              (byte)((((c >> 8) & 0xFF) * ai) >> 8),
        //                                              (byte)((((c & 0xFF) * ai) >> 8)));


        //                pixels[index++] = func(x, y, srcColor).ToPreMultAlphaColor();
        //            }
        //        }
        //    }
        //}


        /// <summary>
        /// Gets the color of the pixel at the x, y coordinate as integer.  
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at x, y.</returns>
        public static int dbugGetPixeli(this BitmapBuffer bmp, int x, int y)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                return context.Pixels[y * context.Width + x];
            }
        }

        /// <summary>
        /// Gets the color of the pixel at the x, y coordinate as a Color struct.  
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The color of the pixel at x, y as a Color struct.</returns>
        public static ColorInt dbugGetPixel(this BitmapBuffer bmp, int x, int y)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int c = context.Pixels[y * context.Width + x];
                byte a = (byte)(c >> 24);

                // Prevent division by zero
                int ai = a;
                if (ai == 0)
                {
                    ai = 1;
                }

                // Scale inverse alpha to use cheap integer mul bit shift
                ai = ((255 << 8) / ai);
                return ColorInt.FromArgb(a,
                                     (byte)((((c >> 16) & 0xFF) * ai) >> 8),
                                     (byte)((((c >> 8) & 0xFF) * ai) >> 8),
                                     (byte)((((c & 0xFF) * ai) >> 8)));
            }
        }
#endif
        /// <summary>
        /// Gets the brightness / luminance of the pixel at the x, y coordinate as byte.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate of the pixel.</param>
        /// <param name="y">The y coordinate of the pixel.</param>
        /// <returns>The brightness of the pixel at x, y.</returns>
        public static byte GetBrightness(this BitmapBuffer bmp, int x, int y)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                // Extract color components
                int c = context.Pixels[y * context.Width + x];
                byte r = (byte)(c >> 16);
                byte g = (byte)(c >> 8);
                byte b = (byte)(c);

                // Convert to gray with constant factors 0.2126, 0.7152, 0.0722
                return (byte)((r * 6966 + g * 23436 + b * 2366) >> 15);
            }
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixeli(this BitmapBuffer bmp, int index, byte r, byte g, byte b)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixel(this BitmapBuffer bmp, int x, int y, byte r, byte g, byte b)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }



        /// <summary>
        /// Sets the color of the pixel including the alpha value and using a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixeli(this BitmapBuffer bmp, int index, byte a, byte r, byte g, byte b)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }

        /// <summary>
        /// Sets the color of the pixel including the alpha value. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixel(this BitmapBuffer bmp, int x, int y, byte a, byte r, byte g, byte b)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = (a << 24) | (r << 16) | (g << 8) | b;
            }
        }



        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this BitmapBuffer bmp, int index, ColorInt color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = color.ToPreMultAlphaColor();
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this BitmapBuffer bmp, int x, int y, ColorInt color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = color.ToPreMultAlphaColor();
            }
        }

        /// <summary>
        /// Sets the color of the pixel using an extra alpha value and a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this BitmapBuffer bmp, int index, byte a, ColorInt color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = color.ToPreMultAlphaColor();
            }
        }

        /// <summary>
        /// Sets the color of the pixel using an extra alpha value. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this BitmapBuffer bmp, int x, int y, byte a, ColorInt color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Add one to use mul and cheap bit shift for multiplicaltion
                context.Pixels[y * context.Width + x] = color.ToPreMultAlphaColor();
            }
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster).  
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this BitmapBuffer bmp, int index, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = color;
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this BitmapBuffer bmp, int x, int y, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = color;
            }
        }
    }
}