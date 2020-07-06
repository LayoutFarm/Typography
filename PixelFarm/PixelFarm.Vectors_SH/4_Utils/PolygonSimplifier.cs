//The MIT License(MIT)

//Copyright(c) 2018 Rohaan Hamid

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in
//all copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//THE SOFTWARE.

//from https://github.com/rohaanhamid/simplify-csharp
//--------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;


namespace PixelFarm.CpuBlit.VertexProcessing
{
    //for .net 2.0
    public delegate R Func<R>();
    public delegate R Func<T, R>(T t);
    public delegate R Func<T1, T2, R>(T1 t, T2 t2);
    // 

    public class SimplificationHelpers
    {
        public static void Simplify<T>(
            IList<T> points,
            Func<T, T, Boolean> equalityChecker,
            Func<T, double> xExtractor,
            Func<T, double> yExtractor,
            Func<T, double> zExtractor,
            IList<T> output,
            double tolerance = 1.0,
            bool highestQuality = false)
        {
            var simplifier3D = new Simplifier3D<T>(equalityChecker, xExtractor, yExtractor, zExtractor);
            simplifier3D.Simplify(points, tolerance, highestQuality, output);
        }

        public static void Simplify<T>(
            IList<T> points,
            Func<T, T, Boolean> equalityChecker,
            Func<T, double> xExtractor,
            Func<T, double> yExtractor,
            IList<T> output,
            double tolerance = 1.0,
            bool highestQuality = false)
        {
            var simplifier2D = new Simplifier2D<T>(equalityChecker, xExtractor, yExtractor);
            simplifier2D.Simplify(points, tolerance, highestQuality, output);
        }
    }

    public class Simplifier3D<T> : BaseSimplifier<T>
    {
        readonly Func<T, double> _xExtractor;
        readonly Func<T, double> _yExtractor;
        readonly Func<T, double> _zExtractor;

        public Simplifier3D(Func<T, T, Boolean> equalityChecker,
            Func<T, double> xExtractor, Func<T, double> yExtractor, Func<T, double> zExtractor) :
            base(equalityChecker)
        {
            _xExtractor = xExtractor;
            _yExtractor = yExtractor;
            _zExtractor = zExtractor;
        }

        protected override double GetSquareDistance(T p1, T p2)
        {
            double dx = _xExtractor(p1) - _xExtractor(p2);
            double dy = _yExtractor(p1) - _yExtractor(p2);
            double dz = _zExtractor(p1) - _zExtractor(p2);

            return dx * dx + dy * dy + dz * dz;
        }

        protected override double GetSquareSegmentDistance(T p0, T p1, T p2)
        {
            double x0, y0, z0, x1, y1, z1, x2, y2, z2, dx, dy, dz, t;

            x1 = _xExtractor(p1);
            y1 = _yExtractor(p1);
            z1 = _zExtractor(p1);
            x2 = _xExtractor(p2);
            y2 = _yExtractor(p2);
            z2 = _zExtractor(p2);
            x0 = _xExtractor(p0);
            y0 = _yExtractor(p0);
            z0 = _zExtractor(p0);

            dx = x2 - x1;
            dy = y2 - y1;
            dz = z2 - z1;

            if (dx != 0.0d || dy != 0.0d || dz != 0.0d)
            {
                t = ((x0 - x1) * dx + (y0 - y1) * dy + (z0 - z1) * dz)
                        / (dx * dx + dy * dy + dz * dz);

                if (t > 1.0d)
                {
                    x1 = x2;
                    y1 = y2;
                    z1 = z2;
                }
                else if (t > 0.0d)
                {
                    x1 += dx * t;
                    y1 += dy * t;
                    z1 += dz * t;
                }
            }

            dx = x0 - x1;
            dy = y0 - y1;
            dz = z0 - z1;

            return dx * dx + dy * dy + dz * dz;
        }
    }


    public class Simplifier2D<T> : BaseSimplifier<T>
    {
        readonly Func<T, double> _xExtractor;
        readonly Func<T, double> _yExtractor;

        public Simplifier2D(Func<T, T, Boolean> equalityChecker,
            Func<T, double> xExtractor, Func<T, double> yExtractor) :
            base(equalityChecker)
        {
            _xExtractor = xExtractor;
            _yExtractor = yExtractor;
        }

        protected override double GetSquareDistance(T p1, T p2)
        {
            double dx = _xExtractor(p1) - _xExtractor(p2);
            double dy = _yExtractor(p1) - _yExtractor(p2);

            return dx * dx + dy * dy;
        }

        protected override double GetSquareSegmentDistance(T p0, T p1, T p2)
        {
            double x1 = _xExtractor(p1);
            double y1 = _yExtractor(p1);
            double x2 = _xExtractor(p2);
            double y2 = _yExtractor(p2);
            double x0 = _xExtractor(p0);
            double y0 = _yExtractor(p0);

            double dx = x2 - x1;
            double dy = y2 - y1;

            double t;

            if (dx != 0.0d || dy != 0.0d)
            {
                t = ((x0 - x1) * dx + (y0 - y1) * dy)
                        / (dx * dx + dy * dy);

                if (t > 1.0d)
                {
                    x1 = x2;
                    y1 = y2;
                }
                else if (t > 0.0d)
                {
                    x1 += dx * t;
                    y1 += dy * t;
                }
            }

            dx = x0 - x1;
            dy = y0 - y1;

            return dx * dx + dy * dy;
        }
    }

    public abstract class BaseSimplifier<T>
    {
        Func<T, T, Boolean> _equalityChecker;

        private class Range
        {
            public int First { get; }
            public int Last { get; }

            public Range(int first, int last)
            {
                First = first;
                Last = last;
            }
        }

        protected BaseSimplifier(Func<T, T, Boolean> equalityChecker)
        {
            _equalityChecker = equalityChecker;
        }

        /// <summary>
        /// Simplified data points
        /// </summary>
        /// <param name="points">Points to be simplified</param>
        /// <param name="tolerance">Amount of wiggle to be tolerated between coordinates.</param>
        /// <param name="highestQuality">
        /// True for Douglas-Peucker. 
        /// False for Radial-Distance before Douglas-Peucker (Runs Faster)
        /// </param>
        /// <returns>Simplified points</returns>
        public void Simplify(IList<T> points,
                            double tolerance,
                            bool highestQuality,
                            IList<T> output)
        {

            if (points == null || points.Count <= 2)
            {
                //TODO: review here
                //nothing todo,
                foreach (T t in points)
                {
                    output.Add(t);
                }
                return;
            }

            double sqTolerance = tolerance * tolerance;

            if (!highestQuality)
            {
                List<T> tmpOutput = new List<T>();
                SimplifyRadialDistance(points, sqTolerance, tmpOutput);
                points = tmpOutput;
            }
            SimplifyDouglasPeucker(points, sqTolerance, output);
        }

        void SimplifyRadialDistance(IList<T> points, double sqTolerance, IList<T> output)
        {
            T point = default(T);
            T prevPoint = points[0];

            output.Add(prevPoint);
            for (int i = 1; i < points.Count; ++i)
            {
                point = points[i];

                if (GetSquareDistance(point, prevPoint) > sqTolerance)
                {
                    output.Add(point);
                    prevPoint = point;
                }
            }
            if (!_equalityChecker(prevPoint, point))
            {
                output.Add(point);
            }
        }

        void SimplifyDouglasPeucker(IList<T> points, double sqTolerance, IList<T> output)
        {

            BitArray bitArray = new BitArray(points.Count);
            bitArray.Set(0, true);
            bitArray.Set(points.Count - 1, true);

            Stack<Range> stack = new Stack<Range>();
            stack.Push(new Range(0, points.Count - 1));

            while (stack.Count > 0)
            {
                Range range = stack.Pop();

                int index = -1;
                double maxSqDist = 0f;

                // Find index of point with maximum square distance from first and last point
                for (int i = range.First + 1; i < range.Last; ++i)
                {
                    double sqDist = GetSquareSegmentDistance(
                        points[i], points[range.First], points[range.Last]);

                    if (sqDist > maxSqDist)
                    {
                        index = i;
                        maxSqDist = sqDist;
                    }
                }

                if (maxSqDist > sqTolerance)
                {
                    bitArray.Set(index, true);

                    stack.Push(new Range(range.First, index));
                    stack.Push(new Range(index, range.Last));
                }
            }

            //List<T> newPoints = new List<T>(CountNumberOfSetBits(bitArray));

            for (int i = 0; i < bitArray.Count; i++)
            {
                if (bitArray[i])
                {
                    output.Add(points[i]);
                }
            }

            //return newPoints.ToArray();
        }

        int CountNumberOfSetBits(BitArray bitArray)
        {
            int counter = 0;
            for (int i = 0; i < bitArray.Length; i++)
            {
                if (bitArray[i])
                {
                    counter++;
                }
            }
            return counter;
        }

        protected abstract double GetSquareDistance(T p1, T p2);
        protected abstract double GetSquareSegmentDistance(T p0, T p1, T p2);
    }
}
