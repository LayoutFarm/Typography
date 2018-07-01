/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//Apache2, 2017-present, WinterDev
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text;
using PixelFarm.Drawing;

namespace PaintFx
{
    /// <summary>
    /// Defines miscellaneous constants and static functions.
    /// </summary>
    /// // TODO: refactor into mini static classes
    public sealed class PixelUtils
    {
        private PixelUtils()
        {
        }

        internal static bool IsNumber(float x)
        {
            return x >= float.MinValue && x <= float.MaxValue;
        }

        internal static bool IsNumber(double x)
        {
            return x >= double.MinValue && x <= double.MaxValue;
        }

        internal static int Min(int val0, params int[] vals)
        {
            int min = val0;

            for (int i = 0; i < vals.Length; ++i)
            {
                if (vals[i] < min)
                {
                    min = vals[i];
                }
            }

            return min;
        }

        internal static int Max(int val0, params int[] vals)
        {
            int max = val0;

            for (int i = 0; i < vals.Length; ++i)
            {
                if (vals[i] > max)
                {
                    max = vals[i];
                }
            }

            return max;
        }

        public static PointF[] GetRgssOffsets(int quality)
        {
            unsafe
            {
                int sampleCount = quality * quality;
                PointF[] samplesArray = new PointF[sampleCount];

                fixed (PointF* pSamplesArray = samplesArray)
                {
                    GetRgssOffsets(pSamplesArray, sampleCount, quality);
                }

                return samplesArray;
            }
        }

        public static unsafe void GetRgssOffsets(PointF* samplesArray, int sampleCount, int quality)
        {
            if (sampleCount < 1)
            {
                throw new ArgumentOutOfRangeException("sampleCount", "sampleCount must be [0, int.MaxValue]");
            }

            if (sampleCount != quality * quality)
            {
                throw new ArgumentOutOfRangeException("sampleCount != (quality * quality)");
            }

            if (sampleCount == 1)
            {
                samplesArray[0] = new PointF(0.0f, 0.0f);
            }
            else
            {
                for (int i = 0; i < sampleCount; ++i)
                {
                    double y = (i + 1d) / (sampleCount + 1d);
                    double x = y * quality;

                    x -= (int)x;

                    samplesArray[i] = new PointF((float)(x - 0.5d), (float)(y - 0.5d));
                }
            }
        }
        public static Size ComputeThumbnailSize(Size originalSize, int maxEdgeLength)
        {
            Size thumbSize;

            if (originalSize.Width > originalSize.Height)
            {
                int longSide = Math.Min(originalSize.Width, maxEdgeLength);
                thumbSize = new Size(longSide, Math.Max(1, (originalSize.Height * longSide) / originalSize.Width));
            }
            else if (originalSize.Height > originalSize.Width)
            {
                int longSide = Math.Min(originalSize.Height, maxEdgeLength);
                thumbSize = new Size(Math.Max(1, (originalSize.Width * longSide) / originalSize.Height), longSide);
            }
            else // if (docSize.Width == docSize.Height)
            {
                int longSide = Math.Min(originalSize.Width, maxEdgeLength);
                thumbSize = new Size(longSide, longSide);
            }

            return thumbSize;
        }

        public static readonly Color TransparentKey = Color.FromArgb(192, 192, 192);



        private static bool allowGCFullCollect = true;
        public static bool AllowGCFullCollect
        {
            get
            {
                return allowGCFullCollect;
            }

            set
            {
                allowGCFullCollect = value;
            }
        }

        public static void GCFullCollect()
        {
            if (AllowGCFullCollect)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static int defaultSimplificationFactor = 50;
        public static int DefaultSimplificationFactor
        {
            get
            {
                return defaultSimplificationFactor;
            }

            set
            {
                defaultSimplificationFactor = value;
            }
        }



        public static void SplitRectangle(Rectangle rect, Rectangle[] rects)
        {
            int height = rect.Height;

            for (int i = 0; i < rects.Length; ++i)
            {
                Rectangle newRect = Rectangle.FromLTRB(rect.Left,
                                                       rect.Top + ((height * i) / rects.Length),
                                                       rect.Right,
                                                       rect.Top + ((height * (i + 1)) / rects.Length));

                rects[i] = newRect;
            }
        }



        /// <summary>
        /// Rounds an integer to the smallest power of 2 that is greater
        /// than or equal to it.
        /// </summary>
        public static int Log2RoundUp(int x)
        {
            if (x == 0)
            {
                return 1;
            }

            if (x == 1)
            {
                return 1;
            }

            return 1 << (1 + HighestBit(x - 1));
        }

        private static int HighestBit(int x)
        {
            if (x == 0)
            {
                return 0;
            }

            int b = 0;
            int hi = 0;

            while (b <= 30)
            {
                if ((x & (1 << b)) != 0)
                {
                    hi = b;
                }

                ++b;
            }

            return hi;
        }

        private int CountBits(int x)
        {
            uint y = (uint)x;
            int count = 0;

            for (int bit = 0; bit < 32; ++bit)
            {
                if ((y & ((uint)1 << bit)) != 0)
                {
                    ++count;
                }
            }

            return count;
        }

        public static string RemoveSpaces(string s)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in s)
            {
                if (!char.IsWhiteSpace(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        public static int Max(int[,] array)
        {
            int max = int.MinValue;

            for (int i = array.GetLowerBound(0); i <= array.GetUpperBound(0); ++i)
            {
                for (int j = array.GetLowerBound(1); j <= array.GetUpperBound(1); ++j)
                {
                    if (array[i, j] > max)
                    {
                        max = array[i, j];
                    }
                }
            }

            return max;
        }

        public static int Sum(int[][] array)
        {
            int sum = 0;

            for (int i = 0; i < array.Length; ++i)
            {
                int[] row = array[i];

                for (int j = 0; j < row.Length; ++j)
                {
                    sum += row[j];
                }
            }

            return sum;
        }

        public static Point GetRectangleCenter(Rectangle rect)
        {
            return new Point((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        }

        public static PointF GetRectangleCenter(RectangleF rect)
        {
            return new PointF((rect.Left + rect.Right) / 2, (rect.Top + rect.Bottom) / 2);
        }

        public static Scanline[] GetRectangleScans(Rectangle rect)
        {
            Scanline[] scans = new Scanline[rect.Height];

            for (int y = 0; y < rect.Height; ++y)
            {
                scans[y] = new Scanline(rect.X, rect.Y + y, rect.Width);
            }

            return scans;
        }

        public static Scanline[] GetRegionScans(Rectangle[] regions)
        {
            int scanCount = 0;

            int rgnLen = regions.Length;
            for (int i = 0; i < rgnLen; ++i)
            {
                scanCount += regions[i].Height;
            }

            Scanline[] scans = new Scanline[scanCount];
            int scanIndex = 0;


            for (int i = 0; i < rgnLen; ++i)
            {
                Rectangle rect = regions[i];
                int h = rect.Height;
                int w = rect.Width;
                for (int y = 0; y < h; ++y)
                {
                    scans[scanIndex] = new Scanline(rect.X, rect.Y + y, w);
                    ++scanIndex;
                }

            }


            return scans;
        }

        public static Rectangle[] ScanlinesToRectangles(Scanline[] scans)
        {
            return ScanlinesToRectangles(scans, 0, scans.Length);
        }

        public static Rectangle[] ScanlinesToRectangles(Scanline[] scans, int startIndex, int length)
        {
            Rectangle[] rects = new Rectangle[length];

            for (int i = 0; i < length; ++i)
            {
                Scanline scan = scans[i + startIndex];
                rects[i] = new Rectangle(scan.X, scan.Y, scan.Length, 1);
            }

            return rects;
        }

        /// <summary>
        /// Found on Google Groups when searching for "Region.Union" while looking
        /// for bugs:
        /// ---
        /// Hello,
        /// 
        /// I did not run your code, but I know Region.Union is flawed in both 1.0 and
        /// 1.1, so I assume it is in the gdi+ unmanged code dll.  The best workaround,
        /// in terms of speed, is to use a PdnGraphicsPath, but it must be a path with
        /// FillMode = FillMode.Winding. You add the rectangles to the path, then you do
        /// union onto an empty region with the path. The important point is to do only
        /// one union call on a given empty region. We created a "super region" object
        /// to hide all these bugs and optimize clipping operations. In fact, it is much
        /// faster to use the path than to call Region.Union for each rectangle.
        /// 
        /// Too bad about Region.Union. A lot of people will hit this bug, as it is
        /// essential in high-performance animation.
        /// 
        /// Regards,
        /// Frank Hileman
        /// Prodige Software Corporation
        /// ---
        /// </summary>
        /// <param name="rects"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        ///// <returns></returns>
        //public static PdnRegion RectanglesToRegion(RectangleF[] rectsF, int startIndex, int length)
        //{
        //    PdnRegion region;

        //    if (rectsF == null || rectsF.Length == 0 || length == 0)
        //    {
        //        region = PdnRegion.CreateEmpty();
        //    }
        //    else
        //    {
        //        using (PdnGraphicsPath path = new PdnGraphicsPath())
        //        {
        //            path.FillMode = FillMode.Winding;

        //            if (startIndex == 0 && length == rectsF.Length)
        //            {
        //                path.AddRectangles(rectsF);
        //            }
        //            else
        //            {
        //                for (int i = startIndex; i < startIndex + length; ++i)
        //                {
        //                    path.AddRectangle(rectsF[i]);
        //                }
        //            }

        //            region = new PdnRegion(path);
        //        }
        //    }

        //    return region;
        //}

        //public static PdnRegion RectanglesToRegion(RectangleF[] rectsF)
        //{
        //    return RectanglesToRegion(rectsF, 0, rectsF != null ? rectsF.Length : 0);
        //}

        //public static PdnRegion RectanglesToRegion(RectangleF[] rectsF1, RectangleF[] rectsF2, params RectangleF[][] rectsFA)
        //{
        //    using (PdnGraphicsPath path = new PdnGraphicsPath())
        //    {
        //        path.FillMode = FillMode.Winding;

        //        if (rectsF1 != null && rectsF1.Length > 0)
        //        {
        //            path.AddRectangles(rectsF1);
        //        }

        //        if (rectsF2 != null && rectsF2.Length > 0)
        //        {
        //            path.AddRectangles(rectsF2);
        //        }

        //        foreach (RectangleF[] rectsF in rectsFA)
        //        {
        //            if (rectsF != null && rectsF.Length > 0)
        //            {
        //                path.AddRectangles(rectsF);
        //            }
        //        }

        //        return new PdnRegion(path);
        //    }
        //}

        //public static PdnRegion RectanglesToRegion(Rectangle[] rects, int startIndex, int length)
        //{
        //    PdnRegion region;

        //    if (length == 0)
        //    {
        //        region = PdnRegion.CreateEmpty();
        //    }
        //    else
        //    {
        //        using (PdnGraphicsPath path = new PdnGraphicsPath())
        //        {
        //            path.FillMode = FillMode.Winding;
        //            if (startIndex == 0 && length == rects.Length)
        //            {
        //                path.AddRectangles(rects);
        //            }
        //            else
        //            {
        //                for (int i = startIndex; i < startIndex + length; ++i)
        //                {
        //                    path.AddRectangle(rects[i]);
        //                }
        //            }

        //            region = new PdnRegion(path);
        //            path.Dispose();
        //        }
        //    }

        //    return region;
        //}

        //public static PdnRegion RectanglesToRegion(Rectangle[] rects)
        //{
        //    return RectanglesToRegion(rects, 0, rects.Length);
        //}

        public static int GetRegionArea(RectangleF[] rects)
        {
            int area = 0;

            foreach (RectangleF rectF in rects)
            {
                Rectangle rect = Rectangle.Truncate(rectF);
                area += rect.Width * rect.Height;
            }

            return area;
        }

        public static RectangleF RectangleFromCenter(PointF center, float halfSize)
        {
            RectangleF ret = new RectangleF(center.X, center.Y, 0, 0);
            ret.Inflate(halfSize, halfSize);
            return ret;
        }

        public static List<PointF> PointListToPointFList(List<Point> ptList)
        {
            List<PointF> ret = new List<PointF>(ptList.Count);

            for (int i = 0; i < ptList.Count; ++i)
            {
                ret.Add((PointF)ptList[i]);
            }

            return ret;
        }

        public static PointF[] PointArrayToPointFArray(Point[] ptArray)
        {
            PointF[] ret = new PointF[ptArray.Length];

            for (int i = 0; i < ret.Length; ++i)
            {
                ret[i] = ptArray[i];
            }

            return ret;
        }

        public static Rectangle[] InflateRectangles(Rectangle[] rects, int amount)
        {
            Rectangle[] inflated = new Rectangle[rects.Length];

            for (int i = 0; i < rects.Length; ++i)
            {
                inflated[i] = Rectangle.Inflate(rects[i], amount, amount);
            }

            return inflated;
        }

        public static void InflateRectanglesInPlace(Rectangle[] rects, int amount)
        {
            for (int i = 0; i < rects.Length; ++i)
            {
                rects[i].Inflate(amount, amount);
            }
        }

        public static RectangleF[] InflateRectangles(RectangleF[] rectsF, int amount)
        {
            RectangleF[] inflated = new RectangleF[rectsF.Length];

            for (int i = 0; i < rectsF.Length; ++i)
            {
                inflated[i] = RectangleF.Inflate(rectsF[i], amount, amount);
            }

            return inflated;
        }

        public static void InflateRectanglesInPlace(RectangleF[] rectsF, float amount)
        {
            for (int i = 0; i < rectsF.Length; ++i)
            {
                rectsF[i].Inflate(amount, amount);
            }
        }

        public static Rectangle PointsToConstrainedRectangle(Point a, Point b)
        {
            Rectangle rect = PixelUtils.PointsToRectangle(a, b);
            int minWH = Math.Min(rect.Width, rect.Height);

            rect.Width = minWH;
            rect.Height = minWH;

            if (rect.Y != a.Y)
            {
                rect.Location = new Point(rect.X, a.Y - minWH);
            }

            if (rect.X != a.X)
            {
                rect.Location = new Point(a.X - minWH, rect.Y);
            }

            return rect;
        }

        public static RectangleF PointsToConstrainedRectangle(PointF a, PointF b)
        {
            RectangleF rect = PixelUtils.PointsToRectangle(a, b);
            float minWH = Math.Min(rect.Width, rect.Height);

            rect.Width = minWH;
            rect.Height = minWH;

            if (rect.Y != a.Y)
            {
                rect.Location = new PointF(rect.X, a.Y - minWH);
            }

            if (rect.X != a.X)
            {
                rect.Location = new PointF(a.X - minWH, rect.Y);
            }

            return rect;
        }

        /// <summary>
        /// Takes two points and creates a bounding rectangle from them.
        /// </summary>
        /// <param name="a">One corner of the rectangle.</param>
        /// <param name="b">The other corner of the rectangle.</param>
        /// <returns>A Rectangle instance that bounds the two points.</returns>
        public static Rectangle PointsToRectangle(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            int width = Math.Abs(a.X - b.X) + 1;
            int height = Math.Abs(a.Y - b.Y) + 1;

            return new Rectangle(x, y, width, height);
        }

        public static RectangleF PointsToRectangle(PointF a, PointF b)
        {
            float x = Math.Min(a.X, b.X);
            float y = Math.Min(a.Y, b.Y);
            float width = Math.Abs(a.X - b.X) + 1;
            float height = Math.Abs(a.Y - b.Y) + 1;

            return new RectangleF(x, y, width, height);
        }

        public static Rectangle PointsToRectangleExclusive(Point a, Point b)
        {
            int x = Math.Min(a.X, b.X);
            int y = Math.Min(a.Y, b.Y);
            int width = Math.Abs(a.X - b.X);
            int height = Math.Abs(a.Y - b.Y);

            return new Rectangle(x, y, width, height);
        }

        public static RectangleF PointsToRectangleExclusive(PointF a, PointF b)
        {
            float x = Math.Min(a.X, b.X);
            float y = Math.Min(a.Y, b.Y);
            float width = Math.Abs(a.X - b.X);
            float height = Math.Abs(a.Y - b.Y);

            return new RectangleF(x, y, width, height);
        }

        public static RectangleF[] PointsToRectangles(PointF[] pointsF)
        {
            if (pointsF.Length == 0)
            {
                return new RectangleF[] { };
            }

            if (pointsF.Length == 1)
            {
                return new RectangleF[] { new RectangleF(pointsF[0].X, pointsF[0].Y, 1, 1) };
            }

            RectangleF[] rectsF = new RectangleF[pointsF.Length - 1];

            for (int i = 0; i < pointsF.Length - 1; ++i)
            {
                rectsF[i] = PointsToRectangle(pointsF[i], pointsF[i + 1]);
            }

            return rectsF;
        }

        public static Rectangle[] PointsToRectangles(Point[] points)
        {
            if (points.Length == 0)
            {
                return new Rectangle[] { };
            }

            if (points.Length == 1)
            {
                return new Rectangle[] { new Rectangle(points[0].X, points[0].Y, 1, 1) };
            }

            Rectangle[] rects = new Rectangle[points.Length - 1];

            for (int i = 0; i < points.Length - 1; ++i)
            {
                rects[i] = PointsToRectangle(points[i], points[i + 1]);
            }

            return rects;
        }

        /// <summary>
        /// Converts a RectangleF to RectangleF by rounding down the Location and rounding
        /// up the Size.
        /// </summary>
        public static Rectangle RoundRectangle(RectangleF rectF)
        {
            float left = (float)Math.Floor(rectF.Left);
            float top = (float)Math.Floor(rectF.Top);
            float right = (float)Math.Ceiling(rectF.Right);
            float bottom = (float)Math.Ceiling(rectF.Bottom);

            return Rectangle.Truncate(RectangleF.FromLTRB(left, top, right, bottom));
        }

        public static Stack Reverse(Stack reverseMe)
        {
            Stack reversed = new Stack();

            foreach (object o in reverseMe)
            {
                reversed.Push(o);
            }

            return reversed;
        }

        /// <summary>
        /// Allows you to find the bounding box for a "region" that is described as an
        /// array of bounding boxes.
        /// </summary>
        /// <param name="rectsF">The "region" you want to find a bounding box for.</param>
        /// <returns>A RectangleF structure that surrounds the Region.</returns>
        public static RectangleF GetRegionBounds(RectangleF[] rectsF, int startIndex, int length)
        {
            if (rectsF.Length == 0)
            {
                return RectangleF.Empty;
            }

            float left = rectsF[startIndex].Left;
            float top = rectsF[startIndex].Top;
            float right = rectsF[startIndex].Right;
            float bottom = rectsF[startIndex].Bottom;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                RectangleF rectF = rectsF[i];

                if (rectF.Left < left)
                {
                    left = rectF.Left;
                }

                if (rectF.Top < top)
                {
                    top = rectF.Top;
                }

                if (rectF.Right > right)
                {
                    right = rectF.Right;
                }

                if (rectF.Bottom > bottom)
                {
                    bottom = rectF.Bottom;
                }
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static RectangleF GetTraceBounds(PointF[] pointsF, int startIndex, int length)
        {
            if (pointsF.Length == 0)
            {
                return RectangleF.Empty;
            }

            float left = pointsF[startIndex].X;
            float top = pointsF[startIndex].Y;
            float right = 1 + pointsF[startIndex].X;
            float bottom = 1 + pointsF[startIndex].Y;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                PointF pointF = pointsF[i];

                if (pointF.X < left)
                {
                    left = pointF.X;
                }

                if (pointF.Y < top)
                {
                    top = pointF.Y;
                }

                if (pointF.X > right)
                {
                    right = pointF.X;
                }

                if (pointF.Y > bottom)
                {
                    bottom = pointF.Y;
                }
            }

            return RectangleF.FromLTRB(left, top, right, bottom);
        }

        public static Rectangle GetTraceBounds(Point[] points, int startIndex, int length)
        {
            if (points.Length == 0)
            {
                return Rectangle.Empty;
            }

            int left = points[startIndex].X;
            int top = points[startIndex].Y;
            int right = 1 + points[startIndex].X;
            int bottom = 1 + points[startIndex].Y;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                Point point = points[i];

                if (point.X < left)
                {
                    left = point.X;
                }

                if (point.Y < top)
                {
                    top = point.Y;
                }

                if (point.X > right)
                {
                    right = point.X;
                }

                if (point.Y > bottom)
                {
                    bottom = point.Y;
                }
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        /// <summary>
        /// Allows you to find the bounding box for a "region" that is described as an
        /// array of bounding boxes.
        /// </summary>
        /// <param name="rectsF">The "region" you want to find a bounding box for.</param>
        /// <returns>A RectangleF structure that surrounds the Region.</returns>
        public static Rectangle GetRegionBounds(Rectangle[] rects, int startIndex, int length)
        {
            if (rects.Length == 0)
            {
                return Rectangle.Empty;
            }

            int left = rects[startIndex].Left;
            int top = rects[startIndex].Top;
            int right = rects[startIndex].Right;
            int bottom = rects[startIndex].Bottom;

            for (int i = startIndex + 1; i < startIndex + length; ++i)
            {
                Rectangle rect = rects[i];

                if (rect.Left < left)
                {
                    left = rect.Left;
                }

                if (rect.Top < top)
                {
                    top = rect.Top;
                }

                if (rect.Right > right)
                {
                    right = rect.Right;
                }

                if (rect.Bottom > bottom)
                {
                    bottom = rect.Bottom;
                }
            }

            return Rectangle.FromLTRB(left, top, right, bottom);
        }

        public static RectangleF GetRegionBounds(RectangleF[] rectsF)
        {
            return GetRegionBounds(rectsF, 0, rectsF.Length);
        }

        public static Rectangle GetRegionBounds(Rectangle[] rects)
        {
            return GetRegionBounds(rects, 0, rects.Length);
        }

        private static float DistanceSquared(RectangleF[] rectsF, int indexA, int indexB)
        {
            PointF centerA = new PointF(rectsF[indexA].Left + (rectsF[indexA].Width / 2), rectsF[indexA].Top + (rectsF[indexA].Height / 2));
            PointF centerB = new PointF(rectsF[indexB].Left + (rectsF[indexB].Width / 2), rectsF[indexB].Top + (rectsF[indexB].Height / 2));

            return ((centerA.X - centerB.X) * (centerA.X - centerB.X)) +
                ((centerA.Y - centerB.Y) * (centerA.Y - centerB.Y));
        }
        public static Rectangle[] SimplifyRegion(Rectangle[] rects, int complexity)
        {
            if (complexity == 0 || rects.Length < complexity)
            {
                return (Rectangle[])rects.Clone();
            }

            Rectangle[] boxes = new Rectangle[complexity];

            for (int i = 0; i < complexity; ++i)
            {
                int startIndex = (i * rects.Length) / complexity;
                int length = Math.Min(rects.Length, ((i + 1) * rects.Length) / complexity) - startIndex;
                boxes[i] = GetRegionBounds(rects, startIndex, length);
            }

            return boxes;
        }


        public static RectangleF[] SimplifyTrace(PointF[] pointsF, int complexity)
        {
            if (complexity == 0 ||
                (pointsF.Length - 1) < complexity)
            {
                return PointsToRectangles(pointsF);
            }

            RectangleF[] boxes = new RectangleF[complexity];
            int parLength = pointsF.Length - 1; // "(points as Rectangles).Length"

            for (int i = 0; i < complexity; ++i)
            {
                int startIndex = (i * parLength) / complexity;
                int length = Math.Min(parLength, ((i + 1) * parLength) / complexity) - startIndex;
                boxes[i] = GetTraceBounds(pointsF, startIndex, length + 1);
            }

            return boxes;
        }

        //public static Rectangle[] SimplifyTrace(PdnGraphicsPath trace, int complexity)
        //{
        //    return SimplifyRegion(TraceToRectangles(trace), complexity);
        //}

        //public static Rectangle[] SimplifyTrace(PdnGraphicsPath trace)
        //{
        //    return SimplifyTrace(trace, DefaultSimplificationFactor);
        //}

        //public static Rectangle[] TraceToRectangles(PdnGraphicsPath trace, int complexity)
        //{
        //    throw new StillNotPortedException();
        //    //int pointCount = trace.PointCount;

        //    //if (pointCount == 0)
        //    //{
        //    //    return new Rectangle[0];
        //    //}

        //    //PointF[] pathPoints = trace.PathPoints;
        //    //byte[] pathTypes = trace.PathTypes;
        //    //int figureStart = 0;

        //    //// first get count of rectangles we'll need
        //    //Rectangle[] rects = new Rectangle[pointCount];

        //    //for (int i = 0; i < pointCount; ++i)
        //    //{
        //    //    byte type = pathTypes[i];

        //    //    Point a = Point.Truncate(pathPoints[i]);
        //    //    Point b;

        //    //    if ((type & (byte)PathPointType.CloseSubpath) != 0)
        //    //    {
        //    //        b = Point.Truncate(pathPoints[figureStart]);
        //    //        figureStart = i + 1;
        //    //    }
        //    //    else
        //    //    {
        //    //        b = Point.Truncate(pathPoints[i + 1]);
        //    //    }

        //    //    rects[i] = PixelUtils.PointsToRectangle(a, b);
        //    //}

        //    //return rects;
        //}

        //public static Rectangle[] TraceToRectangles(PdnGraphicsPath trace)
        //{
        //    return TraceToRectangles(trace, DefaultSimplificationFactor);
        //}

        public static RectangleF[] SimplifyTrace(PointF[] pointsF)
        {
            return SimplifyTrace(pointsF, defaultSimplificationFactor);
        }

        public static Rectangle[] SimplifyAndInflateRegion(Rectangle[] rects, int complexity, int inflationAmount)
        {
            Rectangle[] simplified = SimplifyRegion(rects, complexity);

            for (int i = 0; i < simplified.Length; ++i)
            {
                simplified[i].Inflate(inflationAmount, inflationAmount);
            }

            return simplified;
        }

        public static Rectangle[] SimplifyAndInflateRegion(Rectangle[] rects)
        {
            return SimplifyAndInflateRegion(rects, defaultSimplificationFactor, 1);
        }

        //public static PdnRegion SimplifyAndInflateRegion(PdnRegion region, int complexity, int inflationAmount)
        //{
        //    Rectangle[] rectRegion = SimplifyRegion(region, complexity);

        //    for (int i = 0; i < rectRegion.Length; ++i)
        //    {
        //        rectRegion[i].Inflate(inflationAmount, inflationAmount);
        //    }

        //    return RectanglesToRegion(rectRegion);
        //}

        //public static PdnRegion SimplifyAndInflateRegion(PdnRegion region)
        //{
        //    return SimplifyAndInflateRegion(region, defaultSimplificationFactor, 1);
        //}

        public static RectangleF[] TranslateRectangles(RectangleF[] rectsF, PointF offset)
        {
            RectangleF[] retRectsF = new RectangleF[rectsF.Length];
            int i = 0;

            foreach (RectangleF rectF in rectsF)
            {
                retRectsF[i] = new RectangleF(rectF.X + offset.X, rectF.Y + offset.Y, rectF.Width, rectF.Height);
                ++i;
            }

            return retRectsF;
        }

        public static Rectangle[] TranslateRectangles(Rectangle[] rects, int dx, int dy)
        {
            Rectangle[] retRects = new Rectangle[rects.Length];

            for (int i = 0; i < rects.Length; ++i)
            {
                retRects[i] = new Rectangle(rects[i].X + dx, rects[i].Y + dy, rects[i].Width, rects[i].Height);
            }

            return retRects;
        }

        public static void TranslatePointsInPlace(PointF[] ptsF, float dx, float dy)
        {
            for (int i = 0; i < ptsF.Length; ++i)
            {
                ptsF[i].X += dx;
                ptsF[i].Y += dy;
            }
        }

        public static void TranslatePointsInPlace(Point[] pts, int dx, int dy)
        {
            for (int i = 0; i < pts.Length; ++i)
            {
                pts[i].X += dx;
                pts[i].Y += dy;
            }
        }

        public static Rectangle[] TruncateRectangles(RectangleF[] rectsF)
        {
            Rectangle[] rects = new Rectangle[rectsF.Length];

            for (int i = 0; i < rectsF.Length; ++i)
            {
                rects[i] = Rectangle.Truncate(rectsF[i]);
            }

            return rects;
        }




        /// <summary>
        /// The Sutherland-Hodgman clipping alrogithm.
        /// http://ezekiel.vancouver.wsu.edu/~cs442/lectures/clip/clip/index.html
        /// 
        /// # Clipping a convex polygon to a convex region (e.g., rectangle) will always produce a convex polygon (or no polygon if completely outside the clipping region).
        /// # Clipping a concave polygon to a rectangle may produce several polygons (see figure above) or, as the following algorithm does, produce a single, possibly degenerate, polygon.
        /// # Divide and conquer: Clip entire polygon against a single edge (i.e., half-plane). Repeat for each edge in the clipping region.
        ///
        /// The input is a sequence of vertices: {v0, v1, ... vn} given as an array of Points
        /// the result is a sequence of vertices, given as an array of Points. This result may have
        /// less than, equal, more than, or 0 vertices.
        /// </summary>
        /// <param name="vertices"></param>
        /// <returns></returns>
        public static List<PointF> SutherlandHodgman(RectangleF bounds, List<PointF> v)
        {
            List<PointF> p1 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Left, v);
            List<PointF> p2 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Right, p1);
            List<PointF> p3 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Top, p2);
            List<PointF> p4 = SutherlandHodgmanOneAxis(bounds, RectangleEdge.Bottom, p3);

            return p4;
        }

        private enum RectangleEdge
        {
            Left,
            Right,
            Top,
            Bottom
        }

        private static List<PointF> SutherlandHodgmanOneAxis(RectangleF bounds, RectangleEdge edge, List<PointF> v)
        {
            if (v.Count == 0)
            {
                return new List<PointF>();
            }

            List<PointF> polygon = new List<PointF>();

            PointF s = v[v.Count - 1];

            for (int i = 0; i < v.Count; ++i)
            {
                PointF p = v[i];
                bool pIn = IsInside(bounds, edge, p);
                bool sIn = IsInside(bounds, edge, s);

                if (sIn && pIn)
                {
                    // case 1: inside -> inside
                    polygon.Add(p);
                }
                else if (sIn && !pIn)
                {
                    // case 2: inside -> outside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                }
                else if (!sIn && !pIn)
                {
                    // case 3: outside -> outside
                    // emit nothing
                }
                else if (!sIn && pIn)
                {
                    // case 4: outside -> inside
                    polygon.Add(LineIntercept(bounds, edge, s, p));
                    polygon.Add(p);
                }

                s = p;
            }

            return polygon;
        }

        private static bool IsInside(RectangleF bounds, RectangleEdge edge, PointF p)
        {
            switch (edge)
            {
                case RectangleEdge.Left:
                    return !(p.X < bounds.Left);

                case RectangleEdge.Right:
                    return !(p.X >= bounds.Right);

                case RectangleEdge.Top:
                    return !(p.Y < bounds.Top);

                case RectangleEdge.Bottom:
                    return !(p.Y >= bounds.Bottom);

                default:
                    //InvalidEnumArgumentException
                    throw new NotSupportedException("edge");
            }
        }

        private static Point LineIntercept(Rectangle bounds, RectangleEdge edge, Point a, Point b)
        {
            if (a == b)
            {
                return a;
            }

            switch (edge)
            {
                case RectangleEdge.Bottom:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(a.X + (((b.X - a.X) * (bounds.Bottom - a.Y)) / (b.Y - a.Y)), bounds.Bottom);

                case RectangleEdge.Left:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(bounds.Left, a.Y + (((b.Y - a.Y) * (bounds.Left - a.X)) / (b.X - a.X)));

                case RectangleEdge.Right:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(bounds.Right, a.Y + (((b.Y - a.Y) * (bounds.Right - a.X)) / (b.X - a.X)));

                case RectangleEdge.Top:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new Point(a.X + (((b.X - a.X) * (bounds.Top - a.Y)) / (b.Y - a.Y)), bounds.Top);
            }

            throw new ArgumentException("no intercept found");
        }

        private static PointF LineIntercept(RectangleF bounds, RectangleEdge edge, PointF a, PointF b)
        {
            if (a.Equals(b))
            {
                return a;
            }

            switch (edge)
            {
                case RectangleEdge.Bottom:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(a.X + (((b.X - a.X) * (bounds.Bottom - a.Y)) / (b.Y - a.Y)), bounds.Bottom);

                case RectangleEdge.Left:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(bounds.Left, a.Y + (((b.Y - a.Y) * (bounds.Left - a.X)) / (b.X - a.X)));

                case RectangleEdge.Right:
                    if (b.X == a.X)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(bounds.Right, a.Y + (((b.Y - a.Y) * (bounds.Right - a.X)) / (b.X - a.X)));

                case RectangleEdge.Top:
                    if (b.Y == a.Y)
                    {
                        throw new ArgumentException("no intercept found");
                    }

                    return new PointF(a.X + (((b.X - a.X) * (bounds.Top - a.Y)) / (b.Y - a.Y)), bounds.Top);
            }

            throw new ArgumentException("no intercept found");
        }

        public static Point[] GetLinePoints(Point first, Point second)
        {
            Point[] coords = null;

            int x1 = first.X;
            int y1 = first.Y;
            int x2 = second.X;
            int y2 = second.Y;
            int dx = x2 - x1;
            int dy = y2 - y1;
            int dxabs = Math.Abs(dx);
            int dyabs = Math.Abs(dy);
            int px = x1;
            int py = y1;
            int sdx = Math.Sign(dx);
            int sdy = Math.Sign(dy);
            int x = 0;
            int y = 0;

            if (dxabs > dyabs)
            {
                coords = new Point[dxabs + 1];

                for (int i = 0; i <= dxabs; i++)
                {
                    y += dyabs;

                    if (y >= dxabs)
                    {
                        y -= dxabs;
                        py += sdy;
                    }

                    coords[i] = new Point(px, py);
                    px += sdx;
                }
            }
            else
                // had to add in this cludge for slopes of 1 ... wasn't drawing half the line
                if (dxabs == dyabs)
            {
                coords = new Point[dxabs + 1];

                for (int i = 0; i <= dxabs; i++)
                {
                    coords[i] = new Point(px, py);
                    px += sdx;
                    py += sdy;
                }
            }
            else
            {
                coords = new Point[dyabs + 1];

                for (int i = 0; i <= dyabs; i++)
                {
                    x += dxabs;

                    if (x >= dyabs)
                    {
                        x -= dyabs;
                        px += sdx;
                    }

                    coords[i] = new Point(px, py);
                    py += sdy;
                }
            }

            return coords;
        }


        /// <summary>
        /// Returns the Distance between two points
        /// </summary>
        public static float Distance(PointF a, PointF b)
        {
            return Magnitude(new PointF(a.X - b.X, a.Y - b.Y));
        }

        /// <summary>
        /// Returns the Magnitude (distance to origin) of a point
        /// </summary>
        // TODO: In v4.0 codebase, turn this into an extension method
        public static float Magnitude(PointF p)
        {
            return (float)Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        // TODO: In v4.0 codebase, turn this into an extension method
        public static double Clamp(double x, double min, double max)
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        // TODO: In v4.0 codebase, turn this into an extension method
        public static float Clamp(float x, float min, float max)
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        // TODO: In v4.0 codebase, turn this into an extension method
        public static int Clamp(int x, int min, int max)
        {
            if (x < min)
            {
                return min;
            }
            else if (x > max)
            {
                return max;
            }
            else
            {
                return x;
            }
        }

        public static byte ClampToByte(double x)
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }

        public static byte ClampToByte(float x)
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }

        public static byte ClampToByte(int x)
        {
            if (x > 255)
            {
                return 255;
            }
            else if (x < 0)
            {
                return 0;
            }
            else
            {
                return (byte)x;
            }
        }

        public static float Lerp(float from, float to, float frac)
        {
            return (from + frac * (to - from));
        }

        public static double Lerp(double from, double to, double frac)
        {
            return (from + frac * (to - from));
        }

        public static PointF Lerp(PointF from, PointF to, float frac)
        {
            return new PointF(Lerp(from.X, to.X, frac), Lerp(from.Y, to.Y, frac));
        }

        public static int ColorDifference(ColorBgra a, ColorBgra b)
        {
            return (int)Math.Ceiling(Math.Sqrt(ColorDifferenceSquared(a, b)));
        }

        public static int ColorDifferenceSquared(ColorBgra a, ColorBgra b)
        {
            int diffSq = 0, tmp;

            tmp = a.R - b.R;
            diffSq += tmp * tmp;
            tmp = a.G - b.G;
            diffSq += tmp * tmp;
            tmp = a.B - b.B;
            diffSq += tmp * tmp;

            return diffSq / 3;
        }


        /// <summary>
        /// Reads a 16-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 16-bit unsigned integer that was read.</returns>
        public static int ReadUInt16(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            return byte1 + (byte2 << 8);
        }

        public static void WriteUInt16(Stream stream, UInt16 word)
        {
            stream.WriteByte((byte)(word & 0xff));
            stream.WriteByte((byte)(word >> 8));
        }

        public static void WriteUInt24(Stream stream, int uint24)
        {
            stream.WriteByte((byte)(uint24 & 0xff));
            stream.WriteByte((byte)((uint24 >> 8) & 0xff));
            stream.WriteByte((byte)((uint24 >> 16) & 0xff));
        }

        public static void WriteUInt32(Stream stream, UInt32 uint32)
        {
            stream.WriteByte((byte)(uint32 & 0xff));
            stream.WriteByte((byte)((uint32 >> 8) & 0xff));
            stream.WriteByte((byte)((uint32 >> 16) & 0xff));
            stream.WriteByte((byte)((uint32 >> 24) & 0xff));
        }

        /// <summary>
        /// Reads a 24-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 24-bit unsigned integer that was read.</returns>
        public static int ReadUInt24(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            int byte3 = stream.ReadByte();

            if (byte3 == -1)
            {
                return -1;
            }

            return byte1 + (byte2 << 8) + (byte3 << 16);
        }

        /// <summary>
        /// Reads a 32-bit unsigned integer from a Stream in little-endian format.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>-1 on failure, else the 32-bit unsigned integer that was read.</returns>
        public static long ReadUInt32(Stream stream)
        {
            int byte1 = stream.ReadByte();

            if (byte1 == -1)
            {
                return -1;
            }

            int byte2 = stream.ReadByte();

            if (byte2 == -1)
            {
                return -1;
            }

            int byte3 = stream.ReadByte();

            if (byte3 == -1)
            {
                return -1;
            }

            int byte4 = stream.ReadByte();

            if (byte4 == -1)
            {
                return -1;
            }

            return unchecked((long)((uint)(byte1 + (byte2 << 8) + (byte3 << 16) + (byte4 << 24))));
        }

        public static int ReadFromStream(Stream input, byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = input.Read(buffer, offset + totalBytesRead, count - totalBytesRead);

                if (bytesRead == 0)
                {
                    throw new IOException("ran out of data");
                }

                totalBytesRead += bytesRead;
            }

            return totalBytesRead;
        }

        public static long CopyStream(Stream input, Stream output, long maxBytes)
        {
            long bytesCopied = 0;
            byte[] buffer = new byte[4096];

            while (true)
            {
                int bytesRead = input.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    break;
                }
                else
                {
                    int bytesToCopy;

                    if (maxBytes != -1 && (bytesCopied + bytesRead) > maxBytes)
                    {
                        bytesToCopy = (int)(maxBytes - bytesCopied);
                    }
                    else
                    {
                        bytesToCopy = bytesRead;
                    }

                    output.Write(buffer, 0, bytesRead);
                    bytesCopied += bytesToCopy;

                    if (bytesToCopy != bytesRead)
                    {
                        break;
                    }
                }
            }

            return bytesCopied;
        }

        public static long CopyStream(Stream input, Stream output)
        {
            return CopyStream(input, output, -1);
        }

        private struct Edge
        {
            public int miny;   // int
            public int maxy;   // int
            public int x;      // fixed point: 24.8
            public int dxdy;   // fixed point: 24.8

            public Edge(int miny, int maxy, int x, int dxdy)
            {
                this.miny = miny;
                this.maxy = maxy;
                this.x = x;
                this.dxdy = dxdy;
            }
        }

        public static Scanline[] GetScans(Point[] vertices)
        {
            return GetScans(vertices, 0, vertices.Length);
        }

        public static Scanline[] GetScans(Point[] vertices, int startIndex, int length)
        {
            if (length > vertices.Length - startIndex)
            {
                throw new ArgumentException("out of bounds: length > vertices.Length - startIndex");
            }

            int ymax = 0;

            // Build edge table
            Edge[] edgeTable = new Edge[length];
            int edgeCount = 0;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Point top = vertices[i];
                Point bottom = vertices[(((i + 1) - startIndex) % length) + startIndex];
                int dy;

                if (top.Y > bottom.Y)
                {
                    Point temp = top;
                    top = bottom;
                    bottom = temp;
                }

                dy = bottom.Y - top.Y;

                if (dy != 0)
                {
                    edgeTable[edgeCount] = new Edge(top.Y, bottom.Y, top.X << 8, (((bottom.X - top.X) << 8) / dy));
                    ymax = Math.Max(ymax, bottom.Y);
                    ++edgeCount;
                }
            }

            // Sort edge table by miny
            for (int i = 0; i < edgeCount - 1; ++i)
            {
                int min = i;

                for (int j = i + 1; j < edgeCount; ++j)
                {
                    if (edgeTable[j].miny < edgeTable[min].miny)
                    {
                        min = j;
                    }
                }

                if (min != i)
                {
                    Edge temp = edgeTable[min];
                    edgeTable[min] = edgeTable[i];
                    edgeTable[i] = temp;
                }
            }

            // Compute how many scanlines we will be emitting
            int scanCount = 0;
            int activeLow = 0;
            int activeHigh = 0;
            int yscan1 = edgeTable[0].miny;

            // we assume that edgeTable[0].miny == yscan
            while (activeHigh < edgeCount - 1 &&
                   edgeTable[activeHigh + 1].miny == yscan1)
            {
                ++activeHigh;
            }

            while (yscan1 <= ymax)
            {
                // Find new edges where yscan == miny
                while (activeHigh < edgeCount - 1 &&
                       edgeTable[activeHigh + 1].miny == yscan1)
                {
                    ++activeHigh;
                }

                int count = 0;
                for (int i = activeLow; i <= activeHigh; ++i)
                {
                    if (edgeTable[i].maxy > yscan1)
                    {
                        ++count;
                    }
                }

                scanCount += count / 2;
                ++yscan1;

                // Remove edges where yscan == maxy
                while (activeLow < edgeCount - 1 &&
                       edgeTable[activeLow].maxy <= yscan1)
                {
                    ++activeLow;
                }

                if (activeLow > activeHigh)
                {
                    activeHigh = activeLow;
                }
            }

            // Allocate scanlines that we'll return
            Scanline[] scans = new Scanline[scanCount];

            // Active Edge Table (AET): it is indices into the Edge Table (ET)
            int[] active = new int[edgeCount];
            int activeCount = 0;
            int yscan2 = edgeTable[0].miny;
            int scansIndex = 0;

            // Repeat until both the ET and AET are empty
            while (yscan2 <= ymax)
            {
                // Move any edges from the ET to the AET where yscan == miny
                for (int i = 0; i < edgeCount; ++i)
                {
                    if (edgeTable[i].miny == yscan2)
                    {
                        active[activeCount] = i;
                        ++activeCount;
                    }
                }

                // Sort the AET on x
                for (int i = 0; i < activeCount - 1; ++i)
                {
                    int min = i;

                    for (int j = i + 1; j < activeCount; ++j)
                    {
                        if (edgeTable[active[j]].x < edgeTable[active[min]].x)
                        {
                            min = j;
                        }
                    }

                    if (min != i)
                    {
                        int temp = active[min];
                        active[min] = active[i];
                        active[i] = temp;
                    }
                }

                // For each pair of entries in the AET, fill in pixels between their info
                for (int i = 0; i < activeCount; i += 2)
                {
                    Edge el = edgeTable[active[i]];
                    Edge er = edgeTable[active[i + 1]];
                    int startx = (el.x + 0xff) >> 8; // ceil(x)
                    int endx = er.x >> 8;      // floor(x)

                    scans[scansIndex] = new Scanline(startx, yscan2, endx - startx);
                    ++scansIndex;
                }

                ++yscan2;

                // Remove from the AET any edge where yscan == maxy
                int k = 0;
                while (k < activeCount && activeCount > 0)
                {
                    if (edgeTable[active[k]].maxy == yscan2)
                    {
                        // remove by shifting everything down one
                        for (int j = k + 1; j < activeCount; ++j)
                        {
                            active[j - 1] = active[j];
                        }

                        --activeCount;
                    }
                    else
                    {
                        ++k;
                    }
                }

                // Update x for each entry in AET
                for (int i = 0; i < activeCount; ++i)
                {
                    edgeTable[active[i]].x += edgeTable[active[i]].dxdy;
                }
            }

            return scans;
        }


        public static PointF NormalizeVector(PointF vecF)
        {
            float magnitude = Magnitude(vecF);
            vecF.X /= magnitude;
            vecF.Y /= magnitude;
            return vecF;
        }

        public static PointF NormalizeVector2(PointF vecF)
        {
            float magnitude = Magnitude(vecF);

            if (magnitude == 0)
            {
                vecF.X = 0;
                vecF.Y = 0;
            }
            else
            {
                vecF.X /= magnitude;
                vecF.Y /= magnitude;
            }

            return vecF;
        }

        public static void NormalizeVectors(PointF[] vecsF)
        {
            for (int i = 0; i < vecsF.Length; ++i)
            {
                vecsF[i] = NormalizeVector(vecsF[i]);
            }
        }

        public static PointF RotateVector(PointF vecF, float angleDelta)
        {
            angleDelta *= (float)(Math.PI / 180.0);
            float vecFLen = Magnitude(vecF);
            float vecFAngle = angleDelta + (float)Math.Atan2(vecF.Y, vecF.X);
            vecF.X = (float)Math.Cos(vecFAngle);
            vecF.Y = (float)Math.Sin(vecFAngle);
            return vecF;
        }

        public static void RotateVectors(PointF[] vecFs, float angleDelta)
        {
            for (int i = 0; i < vecFs.Length; ++i)
            {
                vecFs[i] = RotateVector(vecFs[i], angleDelta);
            }
        }

        public static PointF MultiplyVector(PointF vecF, float scalar)
        {
            return new PointF(vecF.X * scalar, vecF.Y * scalar);
        }

        public static PointF AddVectors(PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }

        public static PointF SubtractVectors(PointF lhs, PointF rhs)
        {
            return new PointF(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static PointF NegateVector(PointF v)
        {
            return new PointF(-v.X, -v.Y);
        }

        //public static float GetAngleOfTransform(Matrix matrix)
        //{
        //    throw new StillNotPortedException();

        //    //PointF[] pts = new PointF[] { new PointF(1.0f, 0.0f) };
        //    //matrix.TransformVectors(pts);
        //    //double atan2 = Math.Atan2(pts[0].Y, pts[0].X);
        //    //double angle = atan2 * (180.0f / Math.PI);

        //    //return (float)angle;
        //}

        //public static bool IsTransformFlipped(Matrix matrix)
        //{
        //    PointF ptX = new PointF(1.0f, 0.0f);
        //    PointF ptXT = PixelUtils.TransformOneVector(matrix, ptX);
        //    double atan2X = Math.Atan2(ptXT.Y, ptXT.X);
        //    double angleX = atan2X * (180.0 / Math.PI);

        //    PointF ptY = new PointF(0.0f, 1.0f);
        //    PointF ptYT = PixelUtils.TransformOneVector(matrix, ptY);
        //    double atan2Y = Math.Atan2(ptYT.Y, ptYT.X);
        //    double angleY = (atan2Y * (180.0 / Math.PI)) - 90.0;

        //    while (angleX < 0)
        //    {
        //        angleX += 360;
        //    }

        //    while (angleY < 0)
        //    {
        //        angleY += 360;
        //    }

        //    double angleDelta = Math.Abs(angleX - angleY);

        //    return angleDelta > 1.0 && angleDelta < 359.0;
        //}

        /// <summary>
        /// Calculates the dot product of two vectors.
        /// </summary>
        public static float DotProduct(PointF lhs, PointF rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y;
        }

        /// <summary>
        /// Calculates the orthogonal projection of y on to u.
        /// yhat = u * ((y dot u) / (u dot u))
        /// z = y - yhat
        /// Section 6.2 (pg. 381) of Linear Algebra and its Applications, Second Edition, by David C. Lay
        /// </summary>
        /// <param name="y">The vector to decompose</param>
        /// <param name="u">The non-zero vector to project y on to</param>
        /// <param name="yhat">The orthogonal projection of y onto u</param>
        /// <param name="yhatLen">The length of yhat such that yhat = yhatLen * u</param>
        /// <param name="z">The component of y orthogonal to u</param>
        /// <remarks>
        /// As a special case, if u=(0,0) the results are all zero.
        /// </remarks>
        public static void GetProjection(PointF y, PointF u, out PointF yhat, out float yhatLen, out PointF z)
        {
            if (u.X == 0 && u.Y == 0)
            {
                yhat = new PointF(0, 0);
                yhatLen = 0;
                z = new PointF(0, 0);
            }
            else
            {
                float yDotU = DotProduct(y, u);
                float uDotU = DotProduct(u, u);
                yhatLen = yDotU / uDotU;
                yhat = MultiplyVector(u, yhatLen);
                z = SubtractVectors(y, yhat);
            }
        }

        public static int GreatestCommonDivisor(int a, int b)
        {
            int r;

            if (a < b)
            {
                r = a;
                a = b;
                b = r;
            }

            do
            {
                r = a % b;
                a = b;
                b = r;
            } while (r != 0);

            return a;
        }

        public static void Swap(ref int a, ref int b)
        {
            int t;

            t = a;
            a = b;
            b = t;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T t;

            t = a;
            a = b;
            b = t;
        }

        public static T[] RepeatArray<T>(T[] array, int repeatCount)
        {
            T[] returnArray = new T[repeatCount * array.Length];

            for (int i = 0; i < repeatCount; ++i)
            {
                for (int j = 0; j < array.Length; ++j)
                {
                    int index = (i * array.Length) + j;
                    returnArray[index] = array[j];
                }
            }

            return returnArray;
        }

        public static byte FastScaleByteByByte(byte a, byte b)
        {
            int r1 = a * b + 0x80;
            int r2 = ((r1 >> 8) + r1) >> 8;
            return (byte)r2;
        }

        public static int FastDivideShortByByte(ushort n, byte d)
        {
            int i = d * 3;
            uint m = masTable[i];
            uint a = masTable[i + 1];
            uint s = masTable[i + 2];

            uint nTimesMPlusA = unchecked((n * m) + a);
            uint shifted = nTimesMPlusA >> (int)s;
            int r = (int)shifted;

            return r;
        }

        // i = z * 3;
        // (x / z) = ((x * masTable[i]) + masTable[i + 1]) >> masTable[i + 2)
        private static readonly uint[] masTable =
        {
            0x00000000, 0x00000000, 0,  // 0
            0x00000001, 0x00000000, 0,  // 1
            0x00000001, 0x00000000, 1,  // 2
            0xAAAAAAAB, 0x00000000, 33, // 3
            0x00000001, 0x00000000, 2,  // 4
            0xCCCCCCCD, 0x00000000, 34, // 5
            0xAAAAAAAB, 0x00000000, 34, // 6
            0x49249249, 0x49249249, 33, // 7
            0x00000001, 0x00000000, 3,  // 8
            0x38E38E39, 0x00000000, 33, // 9
            0xCCCCCCCD, 0x00000000, 35, // 10
            0xBA2E8BA3, 0x00000000, 35, // 11
            0xAAAAAAAB, 0x00000000, 35, // 12
            0x4EC4EC4F, 0x00000000, 34, // 13
            0x49249249, 0x49249249, 34, // 14
            0x88888889, 0x00000000, 35, // 15
            0x00000001, 0x00000000, 4,  // 16
            0xF0F0F0F1, 0x00000000, 36, // 17
            0x38E38E39, 0x00000000, 34, // 18
            0xD79435E5, 0xD79435E5, 36, // 19
            0xCCCCCCCD, 0x00000000, 36, // 20
            0xC30C30C3, 0xC30C30C3, 36, // 21
            0xBA2E8BA3, 0x00000000, 36, // 22
            0xB21642C9, 0x00000000, 36, // 23
            0xAAAAAAAB, 0x00000000, 36, // 24
            0x51EB851F, 0x00000000, 35, // 25
            0x4EC4EC4F, 0x00000000, 35, // 26
            0x97B425ED, 0x97B425ED, 36, // 27
            0x49249249, 0x49249249, 35, // 28
            0x8D3DCB09, 0x00000000, 36, // 29
            0x88888889, 0x00000000, 36, // 30
            0x42108421, 0x42108421, 35, // 31
            0x00000001, 0x00000000, 5,  // 32
            0x3E0F83E1, 0x00000000, 35, // 33
            0xF0F0F0F1, 0x00000000, 37, // 34
            0x75075075, 0x75075075, 36, // 35
            0x38E38E39, 0x00000000, 35, // 36
            0x6EB3E453, 0x6EB3E453, 36, // 37
            0xD79435E5, 0xD79435E5, 37, // 38
            0x69069069, 0x69069069, 36, // 39
            0xCCCCCCCD, 0x00000000, 37, // 40
            0xC7CE0C7D, 0x00000000, 37, // 41
            0xC30C30C3, 0xC30C30C3, 37, // 42
            0x2FA0BE83, 0x00000000, 35, // 43
            0xBA2E8BA3, 0x00000000, 37, // 44
            0x5B05B05B, 0x5B05B05B, 36, // 45
            0xB21642C9, 0x00000000, 37, // 46
            0xAE4C415D, 0x00000000, 37, // 47
            0xAAAAAAAB, 0x00000000, 37, // 48
            0x5397829D, 0x00000000, 36, // 49
            0x51EB851F, 0x00000000, 36, // 50
            0xA0A0A0A1, 0x00000000, 37, // 51
            0x4EC4EC4F, 0x00000000, 36, // 52
            0x9A90E7D9, 0x9A90E7D9, 37, // 53
            0x97B425ED, 0x97B425ED, 37, // 54
            0x94F2094F, 0x94F2094F, 37, // 55
            0x49249249, 0x49249249, 36, // 56
            0x47DC11F7, 0x47DC11F7, 36, // 57
            0x8D3DCB09, 0x00000000, 37, // 58
            0x22B63CBF, 0x00000000, 35, // 59
            0x88888889, 0x00000000, 37, // 60
            0x4325C53F, 0x00000000, 36, // 61
            0x42108421, 0x42108421, 36, // 62
            0x41041041, 0x41041041, 36, // 63
            0x00000001, 0x00000000, 6,  // 64
            0xFC0FC0FD, 0x00000000, 38, // 65
            0x3E0F83E1, 0x00000000, 36, // 66
            0x07A44C6B, 0x00000000, 33, // 67
            0xF0F0F0F1, 0x00000000, 38, // 68
            0x76B981DB, 0x00000000, 37, // 69
            0x75075075, 0x75075075, 37, // 70
            0xE6C2B449, 0x00000000, 38, // 71
            0x38E38E39, 0x00000000, 36, // 72
            0x381C0E07, 0x381C0E07, 36, // 73
            0x6EB3E453, 0x6EB3E453, 37, // 74
            0x1B4E81B5, 0x00000000, 35, // 75
            0xD79435E5, 0xD79435E5, 38, // 76
            0x3531DEC1, 0x00000000, 36, // 77
            0x69069069, 0x69069069, 37, // 78
            0xCF6474A9, 0x00000000, 38, // 79
            0xCCCCCCCD, 0x00000000, 38, // 80
            0xCA4587E7, 0x00000000, 38, // 81
            0xC7CE0C7D, 0x00000000, 38, // 82
            0x3159721F, 0x00000000, 36, // 83
            0xC30C30C3, 0xC30C30C3, 38, // 84
            0xC0C0C0C1, 0x00000000, 38, // 85
            0x2FA0BE83, 0x00000000, 36, // 86
            0x2F149903, 0x00000000, 36, // 87
            0xBA2E8BA3, 0x00000000, 38, // 88
            0xB81702E1, 0x00000000, 38, // 89
            0x5B05B05B, 0x5B05B05B, 37, // 90
            0x2D02D02D, 0x2D02D02D, 36, // 91
            0xB21642C9, 0x00000000, 38, // 92
            0xB02C0B03, 0x00000000, 38, // 93
            0xAE4C415D, 0x00000000, 38, // 94
            0x2B1DA461, 0x2B1DA461, 36, // 95
            0xAAAAAAAB, 0x00000000, 38, // 96
            0xA8E83F57, 0xA8E83F57, 38, // 97
            0x5397829D, 0x00000000, 37, // 98
            0xA57EB503, 0x00000000, 38, // 99
            0x51EB851F, 0x00000000, 37, // 100
            0xA237C32B, 0xA237C32B, 38, // 101
            0xA0A0A0A1, 0x00000000, 38, // 102
            0x9F1165E7, 0x9F1165E7, 38, // 103
            0x4EC4EC4F, 0x00000000, 37, // 104
            0x27027027, 0x27027027, 36, // 105
            0x9A90E7D9, 0x9A90E7D9, 38, // 106
            0x991F1A51, 0x991F1A51, 38, // 107
            0x97B425ED, 0x97B425ED, 38, // 108
            0x2593F69B, 0x2593F69B, 36, // 109
            0x94F2094F, 0x94F2094F, 38, // 110
            0x24E6A171, 0x24E6A171, 36, // 111
            0x49249249, 0x49249249, 37, // 112
            0x90FDBC09, 0x90FDBC09, 38, // 113
            0x47DC11F7, 0x47DC11F7, 37, // 114
            0x8E78356D, 0x8E78356D, 38, // 115
            0x8D3DCB09, 0x00000000, 38, // 116
            0x23023023, 0x23023023, 36, // 117
            0x22B63CBF, 0x00000000, 36, // 118
            0x44D72045, 0x00000000, 37, // 119
            0x88888889, 0x00000000, 38, // 120
            0x8767AB5F, 0x8767AB5F, 38, // 121
            0x4325C53F, 0x00000000, 37, // 122
            0x85340853, 0x85340853, 38, // 123
            0x42108421, 0x42108421, 37, // 124
            0x10624DD3, 0x00000000, 35, // 125
            0x41041041, 0x41041041, 37, // 126
            0x10204081, 0x10204081, 35, // 127
            0x00000001, 0x00000000, 7,  // 128
            0x0FE03F81, 0x00000000, 35, // 129
            0xFC0FC0FD, 0x00000000, 39, // 130
            0xFA232CF3, 0x00000000, 39, // 131
            0x3E0F83E1, 0x00000000, 37, // 132
            0xF6603D99, 0x00000000, 39, // 133
            0x07A44C6B, 0x00000000, 34, // 134
            0xF2B9D649, 0x00000000, 39, // 135
            0xF0F0F0F1, 0x00000000, 39, // 136
            0x077975B9, 0x00000000, 34, // 137
            0x76B981DB, 0x00000000, 38, // 138
            0x75DED953, 0x00000000, 38, // 139
            0x75075075, 0x75075075, 38, // 140
            0x3A196B1F, 0x00000000, 37, // 141
            0xE6C2B449, 0x00000000, 39, // 142
            0xE525982B, 0x00000000, 39, // 143
            0x38E38E39, 0x00000000, 37, // 144
            0xE1FC780F, 0x00000000, 39, // 145
            0x381C0E07, 0x381C0E07, 37, // 146
            0xDEE95C4D, 0x00000000, 39, // 147
            0x6EB3E453, 0x6EB3E453, 38, // 148
            0xDBEB61EF, 0x00000000, 39, // 149
            0x1B4E81B5, 0x00000000, 36, // 150
            0x36406C81, 0x00000000, 37, // 151
            0xD79435E5, 0xD79435E5, 39, // 152
            0xD62B80D7, 0x00000000, 39, // 153
            0x3531DEC1, 0x00000000, 37, // 154
            0xD3680D37, 0x00000000, 39, // 155
            0x69069069, 0x69069069, 38, // 156
            0x342DA7F3, 0x00000000, 37, // 157
            0xCF6474A9, 0x00000000, 39, // 158
            0xCE168A77, 0xCE168A77, 39, // 159
            0xCCCCCCCD, 0x00000000, 39, // 160
            0xCB8727C1, 0x00000000, 39, // 161
            0xCA4587E7, 0x00000000, 39, // 162
            0xC907DA4F, 0x00000000, 39, // 163
            0xC7CE0C7D, 0x00000000, 39, // 164
            0x634C0635, 0x00000000, 38, // 165
            0x3159721F, 0x00000000, 37, // 166
            0x621B97C3, 0x00000000, 38, // 167
            0xC30C30C3, 0xC30C30C3, 39, // 168
            0x60F25DEB, 0x00000000, 38, // 169
            0xC0C0C0C1, 0x00000000, 39, // 170
            0x17F405FD, 0x17F405FD, 36, // 171
            0x2FA0BE83, 0x00000000, 37, // 172
            0xBD691047, 0xBD691047, 39, // 173
            0x2F149903, 0x00000000, 37, // 174
            0x5D9F7391, 0x00000000, 38, // 175
            0xBA2E8BA3, 0x00000000, 39, // 176
            0x5C90A1FD, 0x5C90A1FD, 38, // 177
            0xB81702E1, 0x00000000, 39, // 178
            0x5B87DDAD, 0x5B87DDAD, 38, // 179
            0x5B05B05B, 0x5B05B05B, 38, // 180
            0xB509E68B, 0x00000000, 39, // 181
            0x2D02D02D, 0x2D02D02D, 37, // 182
            0xB30F6353, 0x00000000, 39, // 183
            0xB21642C9, 0x00000000, 39, // 184
            0x1623FA77, 0x1623FA77, 36, // 185
            0xB02C0B03, 0x00000000, 39, // 186
            0xAF3ADDC7, 0x00000000, 39, // 187
            0xAE4C415D, 0x00000000, 39, // 188
            0x15AC056B, 0x15AC056B, 36, // 189
            0x2B1DA461, 0x2B1DA461, 37, // 190
            0xAB8F69E3, 0x00000000, 39, // 191
            0xAAAAAAAB, 0x00000000, 39, // 192
            0x15390949, 0x00000000, 36, // 193
            0xA8E83F57, 0xA8E83F57, 39, // 194
            0x15015015, 0x15015015, 36, // 195
            0x5397829D, 0x00000000, 38, // 196
            0xA655C439, 0xA655C439, 39, // 197
            0xA57EB503, 0x00000000, 39, // 198
            0x5254E78F, 0x00000000, 38, // 199
            0x51EB851F, 0x00000000, 38, // 200
            0x028C1979, 0x00000000, 33, // 201
            0xA237C32B, 0xA237C32B, 39, // 202
            0xA16B312F, 0x00000000, 39, // 203
            0xA0A0A0A1, 0x00000000, 39, // 204
            0x4FEC04FF, 0x00000000, 38, // 205
            0x9F1165E7, 0x9F1165E7, 39, // 206
            0x27932B49, 0x00000000, 37, // 207
            0x4EC4EC4F, 0x00000000, 38, // 208
            0x9CC8E161, 0x00000000, 39, // 209
            0x27027027, 0x27027027, 37, // 210
            0x9B4C6F9F, 0x00000000, 39, // 211
            0x9A90E7D9, 0x9A90E7D9, 39, // 212
            0x99D722DB, 0x00000000, 39, // 213
            0x991F1A51, 0x991F1A51, 39, // 214
            0x4C346405, 0x00000000, 38, // 215
            0x97B425ED, 0x97B425ED, 39, // 216
            0x4B809701, 0x4B809701, 38, // 217
            0x2593F69B, 0x2593F69B, 37, // 218
            0x12B404AD, 0x12B404AD, 36, // 219
            0x94F2094F, 0x94F2094F, 39, // 220
            0x25116025, 0x25116025, 37, // 221
            0x24E6A171, 0x24E6A171, 37, // 222
            0x24BC44E1, 0x24BC44E1, 37, // 223
            0x49249249, 0x49249249, 38, // 224
            0x91A2B3C5, 0x00000000, 39, // 225
            0x90FDBC09, 0x90FDBC09, 39, // 226
            0x905A3863, 0x905A3863, 39, // 227
            0x47DC11F7, 0x47DC11F7, 38, // 228
            0x478BBCED, 0x00000000, 38, // 229
            0x8E78356D, 0x8E78356D, 39, // 230
            0x46ED2901, 0x46ED2901, 38, // 231
            0x8D3DCB09, 0x00000000, 39, // 232
            0x2328A701, 0x2328A701, 37, // 233
            0x23023023, 0x23023023, 37, // 234
            0x45B81A25, 0x45B81A25, 38, // 235
            0x22B63CBF, 0x00000000, 37, // 236
            0x08A42F87, 0x08A42F87, 35, // 237
            0x44D72045, 0x00000000, 38, // 238
            0x891AC73B, 0x00000000, 39, // 239
            0x88888889, 0x00000000, 39, // 240
            0x10FEF011, 0x00000000, 36, // 241
            0x8767AB5F, 0x8767AB5F, 39, // 242
            0x86D90545, 0x00000000, 39, // 243
            0x4325C53F, 0x00000000, 38, // 244
            0x85BF3761, 0x85BF3761, 39, // 245
            0x85340853, 0x85340853, 39, // 246
            0x10953F39, 0x10953F39, 36, // 247
            0x42108421, 0x42108421, 38, // 248
            0x41CC9829, 0x41CC9829, 38, // 249
            0x10624DD3, 0x00000000, 36, // 250
            0x828CBFBF, 0x00000000, 39, // 251
            0x41041041, 0x41041041, 38, // 252
            0x81848DA9, 0x00000000, 39, // 253
            0x10204081, 0x10204081, 36, // 254
            0x80808081, 0x00000000, 39  // 255
        };
    }
}
