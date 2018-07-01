//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of draw line extension and helper methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-02-24 20:36:41 +0100 (Di, 24 Feb 2015) $
//   Changed in:        $Revision: 112951 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapTransformationExtensions.cs $
//   Id:                $Id: WriteableBitmapTransformationExtensions.cs 112951 2015-02-24 19:36:41Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//
using System;
namespace BitmapBufferEx
{
    public static partial class BitmapBufferExtensions
    {

#if DEBUG
        /// <summary> 
        /// Draws an anti-aliased line with a desired stroke thickness
        /// <param name="context">The context containing the pixels as int RGBA value.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="strokeThickness">The stroke thickness of the line.</param>
        /// </summary>
        public static void DrawLineAa(BitmapContext context, int pixelWidth, int pixelHeight, int x1, int y1, int x2, int y2, int color, int strokeThickness, RectD? clipRect = null)
        {
            AAWidthLine(pixelWidth, pixelHeight, context, x1, y1, x2, y2, strokeThickness, color, clipRect);
        }

        /// <summary> 
        /// Draws an anti-aliased line with a desired stroke thickness
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="strokeThickness">The stroke thickness of the line.</param>
        /// </summary>
        public static void DrawLineAa(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int color, int strokeThickness, RectD? clipRect = null)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                AAWidthLine(bmp.PixelWidth, bmp.PixelHeight, context, x1, y1, x2, y2, strokeThickness, color, clipRect);
            }
        }

        /// <summary> 
        /// Draws an anti-aliased line with a desired stroke thickness
        /// <param name="context">The context containing the pixels as int RGBA value.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="strokeThickness">The stroke thickness of the line.</param>
        /// </summary>
        public static void DrawLineAa(BitmapContext context, int pixelWidth, int pixelHeight, int x1, int y1, int x2, int y2, ColorInt color, int strokeThickness, RectD? clipRect = null)
        {

            AAWidthLine(pixelWidth, pixelHeight, context, x1, y1, x2, y2, strokeThickness, color.ToPreMultAlphaColor(), clipRect);
        }

        /// <summary> 
        /// Draws an anti-aliased line with a desired stroke thickness
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="strokeThickness">The stroke thickness of the line.</param>
        /// </summary>
        public static void DrawLineAa(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, ColorInt color, int strokeThickness, RectD? clipRect = null)
        {

            using (BitmapContext context = bmp.GetBitmapContext())
            {
                AAWidthLine(bmp.PixelWidth, bmp.PixelHeight, context, x1, y1, x2, y2, strokeThickness, color.ToPreMultAlphaColor(), clipRect);
            }
        }

        /// <summary> 
        /// Draws an anti-aliased line, using an optimized version of Gupta-Sproull algorithm 
        /// From http://nokola.com/blog/post/2010/10/14/Anti-aliased-Lines-And-Optimizing-Code-for-Windows-Phone-7e28093First-Look.aspx
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// </summary> 
        public static void DrawLineAa(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, ColorInt color, RectD? clipRect = null)
        {
            bmp.DrawLineAa(x1, y1, x2, y2, color.ToPreMultAlphaColor(), clipRect);
        }

        /// <summary> 
        /// Draws an anti-aliased line, using an optimized version of Gupta-Sproull algorithm 
        /// From http://nokola.com/blog/post/2010/10/14/Anti-aliased-Lines-And-Optimizing-Code-for-Windows-Phone-7e28093First-Look.aspx
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// </summary> 
        public static void DrawLineAa(this BitmapBuffer bmp, int x1, int y1, int x2, int y2, int color, RectD? clipRect = null)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                DrawLineAa(context, context.Width, context.Height, x1, y1, x2, y2, color, clipRect);
            }
        }

        /// <summary> 
        /// Draws an anti-aliased line, using an optimized version of Gupta-Sproull algorithm 
        /// From http://nokola.com/blog/post/2010/10/14/Anti-aliased-Lines-And-Optimizing-Code-for-Windows-Phone-7e28093First-Look.aspx
        /// <param name="context">The context containing the pixels as int RGBA value.</param>
        /// <param name="pixelWidth">The width of one scanline in the pixels array.</param>
        /// <param name="pixelHeight">The height of the bitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color for the line.</param>
        /// </summary> 
        public static void DrawLineAa(BitmapContext context, int pixelWidth, int pixelHeight, int x1, int y1, int x2, int y2, int color, RectD? clipRect = null)
        {
            if ((x1 == x2) && (y1 == y2)) return; // edge case causing invDFloat to overflow, found by Shai Rubinshtein

            // Perform cohen-sutherland clipping if either point is out of the viewport
            if (!CohenSutherlandLineClip(clipRect ?? new RectD(0, 0, pixelWidth, pixelHeight), ref x1, ref y1, ref x2, ref y2)) return;

            if (x1 < 1) x1 = 1;
            if (x1 > pixelWidth - 2) x1 = pixelWidth - 2;
            if (y1 < 1) y1 = 1;
            if (y1 > pixelHeight - 2) y1 = pixelHeight - 2;

            if (x2 < 1) x2 = 1;
            if (x2 > pixelWidth - 2) x2 = pixelWidth - 2;
            if (y2 < 1) y2 = 1;
            if (y2 > pixelHeight - 2) y2 = pixelHeight - 2;

            int addr = y1 * pixelWidth + x1;
            int dx = x2 - x1;
            int dy = y2 - y1;

            int du;
            int dv;
            int u;
            int v;
            int uincr;
            int vincr;

            // Extract color
            int a = (color >> 24) & 0xFF;
            uint srb = (uint)(color & 0x00FF00FF);
            uint sg = (uint)((color >> 8) & 0xFF);

            // By switching to (u,v), we combine all eight octants 
            int adx = dx, ady = dy;
            if (dx < 0) adx = -dx;
            if (dy < 0) ady = -dy;

            if (adx > ady)
            {
                du = adx;
                dv = ady;
                u = x2;
                v = y2;
                uincr = 1;
                vincr = pixelWidth;
                if (dx < 0) uincr = -uincr;
                if (dy < 0) vincr = -vincr;
            }
            else
            {
                du = ady;
                dv = adx;
                u = y2;
                v = x2;
                uincr = pixelWidth;
                vincr = 1;
                if (dy < 0) uincr = -uincr;
                if (dx < 0) vincr = -vincr;
            }

            int uend = u + du;
            int d = (dv << 1) - du;        // Initial value as in Bresenham's 
            int incrS = dv << 1;    // &#916;d for straight increments 
            int incrD = (dv - du) << 1;    // &#916;d for diagonal increments

            double invDFloat = 1.0 / (4.0 * Math.Sqrt(du * du + dv * dv));   // Precomputed inverse denominator 
            double invD2DuFloat = 0.75 - 2.0 * (du * invDFloat);   // Precomputed constant

            const int PRECISION_SHIFT = 10; // result distance should be from 0 to 1 << PRECISION_SHIFT, mapping to a range of 0..1 
            const int PRECISION_MULTIPLIER = 1 << PRECISION_SHIFT;
            int invD = (int)(invDFloat * PRECISION_MULTIPLIER);
            int invD2Du = (int)(invD2DuFloat * PRECISION_MULTIPLIER * a);
            int zeroDot75 = (int)(0.75 * PRECISION_MULTIPLIER * a);

            int invDMulAlpha = invD * a;
            int duMulInvD = du * invDMulAlpha; // used to help optimize twovdu * invD 
            int dMulInvD = d * invDMulAlpha; // used to help optimize twovdu * invD 
            //int twovdu = 0;    // Numerator of distance; starts at 0 
            int twovduMulInvD = 0; // since twovdu == 0 
            int incrSMulInvD = incrS * invDMulAlpha;
            int incrDMulInvD = incrD * invDMulAlpha;

            do
            {
                AlphaBlendNormalOnPremultiplied(context, addr, (zeroDot75 - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
                AlphaBlendNormalOnPremultiplied(context, addr + vincr, (invD2Du + twovduMulInvD) >> PRECISION_SHIFT, srb, sg);
                AlphaBlendNormalOnPremultiplied(context, addr - vincr, (invD2Du - twovduMulInvD) >> PRECISION_SHIFT, srb, sg);

                if (d < 0)
                {
                    // choose straight (u direction) 
                    twovduMulInvD = dMulInvD + duMulInvD;
                    d += incrS;
                    dMulInvD += incrSMulInvD;
                }
                else
                {
                    // choose diagonal (u+v direction) 
                    twovduMulInvD = dMulInvD - duMulInvD;
                    d += incrD;
                    dMulInvD += incrDMulInvD;
                    v++;
                    addr += vincr;
                }
                u++;
                addr += uincr;
            } while (u <= uend);
        }
#endif
    }
}