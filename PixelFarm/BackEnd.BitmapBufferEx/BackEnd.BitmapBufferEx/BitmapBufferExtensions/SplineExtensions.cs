//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of draw spline extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-05 18:18:24 +0100 (Do, 05 Mrz 2015) $
//   Changed in:        $Revision: 113191 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapSplineExtensions.cs $
//   Id:                $Id: WriteableBitmapSplineExtensions.cs 113191 2015-03-05 17:18:24Z unknown $
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
    /// Collection of draw spline extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {

        const float STEP_FACTOR = 2f; 


        /// <summary>
        /// Draws a cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color.</param>
        public static void DrawBezier(this BitmapBuffer bmp, int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2, ColorInt color)
        {
         
            bmp.DrawBezier(x1, y1, cx1, cy1, cx2, cy2, x2, y2, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a cubic Beziér spline defined by start, end and two control points.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the start point.</param>
        /// <param name="y1">The y-coordinate of the start point.</param>
        /// <param name="cx1">The x-coordinate of the 1st control point.</param>
        /// <param name="cy1">The y-coordinate of the 1st control point.</param>
        /// <param name="cx2">The x-coordinate of the 2nd control point.</param>
        /// <param name="cy2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x2">The x-coordinate of the end point.</param>
        /// <param name="y2">The y-coordinate of the end point.</param>
        /// <param name="color">The color.</param>
        public static void DrawBezier(this BitmapBuffer bmp, int x1, int y1, int cx1, int cy1, int cx2, int cy2, int x2, int y2, int color)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int minX = Math.Min(x1, Math.Min(cx1, Math.Min(cx2, x2)));
            int minY = Math.Min(y1, Math.Min(cy1, Math.Min(cy2, y2)));
            int maxX = Math.Max(x1, Math.Max(cx1, Math.Max(cx2, x2)));
            int maxY = Math.Max(y1, Math.Max(cy1, Math.Max(cy2, y2)));

            // Get slope
            int lenx = maxX - minX;
            int len = maxY - minY;
            if (lenx > len)
            {
                len = lenx;
            }

            // Prevent division by zero
            if (len != 0)
            {
                using (BitmapContext context = bmp.GetBitmapContext())
                {
                    // Use refs for faster access (really important!) speeds up a lot!
                    int w = context.Width;
                    int h = context.Height;

                    // Init vars
                    float step = STEP_FACTOR / len;
                    int tx1 = x1;
                    int ty1 = y1;
                    int tx2, ty2;

                    // Interpolate
                    for (float t = step; t <= 1; t += step)
                    {
                        float tSq = t * t;
                        float t1 = 1 - t;
                        float t1Sq = t1 * t1;

                        tx2 = (int)(t1 * t1Sq * x1 + 3 * t * t1Sq * cx1 + 3 * t1 * tSq * cx2 + t * tSq * x2);
                        ty2 = (int)(t1 * t1Sq * y1 + 3 * t * t1Sq * cy1 + 3 * t1 * tSq * cy2 + t * tSq * y2);

                        // Draw line
                        DrawLine(context, w, h, tx1, ty1, tx2, ty2, color);
                        tx1 = tx2;
                        ty1 = ty2;
                    }

                    // Prevent rounding gap
                    DrawLine(context, w, h, tx1, ty1, x2, y2, color);
                }
            }
        }

        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawBeziers(this BitmapBuffer bmp, int[] points, ColorInt color)
        {
            
            bmp.DrawBeziers(points, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a series of cubic Beziér splines each defined by start, end and two control points. 
        /// The ending point of the previous curve is used as starting point for the next. 
        /// Therefore the initial curve needs four points and the subsequent 3 (2 control and 1 end point).
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, cx1, cy1, cx2, cy2, x2, y2, cx3, cx4 ..., xn, yn).</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawBeziers(this BitmapBuffer bmp, int[] points, int color)
        {
            int x1 = points[0];
            int y1 = points[1];
            int x2, y2;
            for (int i = 2; i + 5 < points.Length; i += 6)
            {
                x2 = points[i + 4];
                y2 = points[i + 5];
                bmp.DrawBezier(x1, y1, points[i], points[i + 1], points[i + 2], points[i + 3], x2, y2, color);
                x1 = x2;
                y1 = y2;
            }
        }



        /// <summary>
        /// Draws a segment of a Cardinal spline (cubic) defined by four control points.
        /// </summary>
        /// <param name="x1">The x-coordinate of the 1st control point.</param>
        /// <param name="y1">The y-coordinate of the 1st control point.</param>
        /// <param name="x2">The x-coordinate of the 2nd control point.</param>
        /// <param name="y2">The y-coordinate of the 2nd control point.</param>
        /// <param name="x3">The x-coordinate of the 3rd control point.</param>
        /// <param name="y3">The y-coordinate of the 3rd control point.</param>
        /// <param name="x4">The x-coordinate of the 4th control point.</param>
        /// <param name="y4">The y-coordinate of the 4th control point.</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color.</param>
        /// <param name="context">The pixel context.</param>
        /// <param name="w">The width of the bitmap.</param>
        /// <param name="h">The height of the bitmap.</param> 
        private static void DrawCurveSegment(int x1, int y1, int x2, int y2, int x3, int y3, int x4, int y4, float tension, int color, BitmapContext context, int w, int h)
        {
            // Determine distances between controls points (bounding rect) to find the optimal stepsize
            int minX = Math.Min(x1, Math.Min(x2, Math.Min(x3, x4)));
            int minY = Math.Min(y1, Math.Min(y2, Math.Min(y3, y4)));
            int maxX = Math.Max(x1, Math.Max(x2, Math.Max(x3, x4)));
            int maxY = Math.Max(y1, Math.Max(y2, Math.Max(y3, y4)));

            // Get slope
            int lenx = maxX - minX;
            int len = maxY - minY;
            if (lenx > len)
            {
                len = lenx;
            }

            // Prevent division by zero
            if (len != 0)
            {
                // Init vars
                float step = STEP_FACTOR / len;
                int tx1 = x2;
                int ty1 = y2;
                int tx2, ty2;

                // Calculate factors
                float sx1 = tension * (x3 - x1);
                float sy1 = tension * (y3 - y1);
                float sx2 = tension * (x4 - x2);
                float sy2 = tension * (y4 - y2);
                float ax = sx1 + sx2 + 2 * x2 - 2 * x3;
                float ay = sy1 + sy2 + 2 * y2 - 2 * y3;
                float bx = -2 * sx1 - sx2 - 3 * x2 + 3 * x3;
                float by = -2 * sy1 - sy2 - 3 * y2 + 3 * y3;

                // Interpolate
                for (float t = step; t <= 1; t += step)
                {
                    float tSq = t * t;

                    tx2 = (int)(ax * tSq * t + bx * tSq + sx1 * t + x2);
                    ty2 = (int)(ay * tSq * t + by * tSq + sy1 * t + y2);

                    // Draw line
                    DrawLine(context, w, h, tx1, ty1, tx2, ty2, color);
                    tx1 = tx2;
                    ty1 = ty2;
                }

                // Prevent rounding gap
                DrawLine(context, w, h, tx1, ty1, x3, y3, color);
            }
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawCurve(this BitmapBuffer bmp, int[] points, float tension, ColorInt color)
        {
         
            bmp.DrawCurve(points, tension, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawCurve(this BitmapBuffer bmp, int[] points, float tension, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                // First segment
                DrawCurveSegment(points[0], points[1], points[0], points[1], points[2], points[3], points[4], points[5], tension, color, context, w, h);

                // Middle segments
                int i;
                for (i = 2; i < points.Length - 4; i += 2)
                {
                    DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 4], points[i + 5], tension, color, context, w, h);
                }

                // Last segment
                DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 2], points[i + 3], tension, color, context, w, h);
            }
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawCurveClosed(this BitmapBuffer bmp, int[] points, float tension, ColorInt color)
        {
            
            bmp.DrawCurveClosed(points, tension, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a closed Cardinal spline (cubic) defined by a point collection. 
        /// The cardinal spline passes through each point in the collection.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points for the curve in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, x3, y3, x4, y4, x1, x2 ..., xn, yn).</param>
        /// <param name="tension">The tension of the curve defines the shape. Usually between 0 and 1. 0 would be a straight line.</param>
        /// <param name="color">The color for the spline.</param>
        public static void DrawCurveClosed(this BitmapBuffer bmp, int[] points, float tension, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;

                int pn = points.Length;

                // First segment
                DrawCurveSegment(points[pn - 2], points[pn - 1], points[0], points[1], points[2], points[3], points[4], points[5], tension, color, context, w, h);

                // Middle segments
                int i;
                for (i = 2; i < pn - 4; i += 2)
                {
                    DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[i + 4], points[i + 5], tension, color, context, w, h);
                }

                // Last segment
                DrawCurveSegment(points[i - 2], points[i - 1], points[i], points[i + 1], points[i + 2], points[i + 3], points[0], points[1], tension, color, context, w, h);

                // Last-to-First segment
                DrawCurveSegment(points[i], points[i + 1], points[i + 2], points[i + 3], points[0], points[1], points[2], points[3], tension, color, context, w, h);
            }
        }
    }
}