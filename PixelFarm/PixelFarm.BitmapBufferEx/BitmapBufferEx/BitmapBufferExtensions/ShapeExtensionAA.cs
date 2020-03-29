//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-07-20 11:44:36 +0200 (Mo, 20 Jul 2015) $
//   Changed in:        $Revision: 114480 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapShapeExtensions.cs $
//   Id:                $Id: WriteableBitmapShapeExtensions.cs 114480 2015-07-20 09:44:36Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
//

namespace BitmapBufferEx
{
    /// <summary>
    /// Collection of extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {
#if DEBUG
        /// <summary>
        /// Draws a polyline. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void dbugDrawPolylineAa(this BitmapBuffer bmp, int[] points, ColorInt color)
        {
            bmp.dbugDrawPolylineAa(points, color.ToPreMultAlphaColor());
        }

        /// <summary>
        /// Draws a polyline anti-aliased. Add the first point also at the end of the array if the line should be closed.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="points">The points of the polyline in x and y pairs, therefore the array is interpreted as (x1, y1, x2, y2, ..., xn, yn).</param>
        /// <param name="color">The color for the line.</param>
        public static void dbugDrawPolylineAa(this BitmapBuffer bmp, int[] points, int color)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                int w = context.Width;
                int h = context.Height;
                int x1 = points[0];
                int y1 = points[1];

                for (int i = 2; i < points.Length; i += 2)
                {
                    //int x2 = points[i];
                    //int y2 = points[i + 1];

                    DrawLineAa(context, w, h,
                        x1, y1,
                        x1 += points[i], y1 += points[i + 1], //also update x1,y1 
                        color);
                    //x1 = x2;
                    //y1 = y2;
                }
            }
        }
#endif
    }
}