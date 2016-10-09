using System;
using System.Collections.Generic;

namespace NRasterizer
{
    [Flags]
    public enum Flag : byte
    {
        OnCurve = 1,
        XByte = 2,
        YByte = 4,
        Repeat = 8,
        XSignOrSame = 16,
        YSignOrSame = 32
    }
    public struct FtPoint
    {
        readonly short _x;
        readonly short _y;

        public FtPoint(short x, short y)
        {
            _x = x;
            _y = y;

        }

        public short X { get { return _x; } }
        public short Y { get { return _y; } }

        public override string ToString()
        {
            return "(" + _x + "," + _y + ")";
        }
    }
    public struct FtPointD
    {
        readonly double _x;
        readonly double _y;
        public FtPointD(double x, double y)
        {
            _x = x;
            _y = y;
        }
        public FtPointD(FtPoint p)
        {
            _x = p.X;
            _y = p.Y;
        }
        public double x { get { return _x; } }
        public double y { get { return _y; } }
        public override string ToString()
        {
            return "(" + _x + "," + _y + ")";
        }
    }



    public class Glyph
    {
        readonly short[] _x;
        readonly short[] _y;

        readonly Flag[] _flags;
        readonly ushort[] _contourEndPoints;
        readonly Bounds _bounds;

        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new Flag[0], new ushort[0], Bounds.Zero);

        public Glyph(short[] x, short[] y, Flag[] flags, ushort[] contourEndPoints, Bounds bounds)
        {
            _x = x;
            _y = y;
            _flags = flags;
            _contourEndPoints = contourEndPoints;
            _bounds = bounds;
        }

        public Bounds Bounds { get { return _bounds; } }

        public int ContourCount { get { return _contourEndPoints.Length; } }

        private FtPoint At(int pointIndex)
        {
            return new FtPoint(_x[pointIndex], _y[pointIndex]);
        }

        private IEnumerable<FtPoint> GetContourPoints(int contourIndex)
        {
            var begin = GetContourBegin(contourIndex);
            var end = GetContourEnd(contourIndex);
            for (int i = begin; i <= end; i++)
            {
                yield return At(i);
            }
        }
    
        //private IEnumerable<FtPoint> InsertImplicit(IEnumerable<FtPoint> points)
        //{

        //    FtPoint previous = new FtPoint(); //empty point
        //    int count = 0;
        //    foreach (FtPoint p in points)
        //    {
        //        if (count == 0)
        //        {
        //            previous = p;
        //            yield return p;
        //        }
        //        else
        //        {
        //            if (!previous.On && !p.On)
        //            {
        //                // implicit point on curve
        //                yield return new FtPoint((short)((previous.X + p.X) / 2), (short)((previous.Y + p.Y) / 2), true);
        //            }
        //            previous = p;
        //            yield return p;
        //        }
        //        count++;
        //    }
        //    //var previous = points.First();
        //    //yield return previous;
        //    //foreach (var p in points.Skip(1))
        //    //{
        //    //    if (!previous.On && !p.On)
        //    //    {
        //    //        // implicit point on curve
        //    //        yield return new Point((short)((previous.X + p.X) / 2), (short)((previous.Y + p.Y) / 2), true);
        //    //    }
        //    //    previous = p;
        //    //    yield return p;
        //    //}
        //}

        private T Circular<T>(List<T> list, int index)
        {
            return list[index % list.Count];
        }

        public FtPoint[] GetPoints(out ushort[] contourEndPoints, out Flag[] flags)
        {
            contourEndPoints = _contourEndPoints;
            flags = this._flags;

            int j = _x.Length;
            FtPoint[] points = new FtPoint[j];
            for (int i = 0; i < j; ++i)
            {
                points[i] = new FtPoint(_x[i], _y[i]);
            }
            return points;
        }
        public IEnumerable<Segment> ContourGetSegmentIter(int contourIndex,
            int fontX, int fontY,
            float xOffset, float yOffset, float scaleX, float scaleY)
        {
            throw new NotSupportedException();
            //var pts = new List<FtPoint>((GetContourPoints(contourIndex)));
            //var begin = GetContourBegin(contourIndex);
            //var end = GetContourEnd(contourIndex);
            //for (int i = 0; i < end - begin; i += pts[(i + 1) % pts.Count].On ? 1 : 2)
            //{
            //    if (pts[(i + 1) % pts.Count].On)
            //    {
            //        yield return new Line(
            //            (int)(xOffset + (fontX + pts[i].X) * scaleX),
            //            (int)(yOffset + (fontY + pts[i].Y) * scaleY),
            //            (int)(xOffset + (fontX + Circular(pts, i + 1).X) * scaleX),
            //            (int)(yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY));
            //    }
            //    else
            //    {
            //        yield return new Bezier(
            //            xOffset + (fontX + pts[i].X) * scaleX,
            //            yOffset + (fontY + pts[i].Y) * scaleY,
            //            xOffset + (fontX + Circular(pts, i + 1).X) * scaleX,
            //            yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY,
            //            xOffset + (fontX + Circular(pts, i + 2).X) * scaleX,
            //            yOffset + (fontY + Circular(pts, i + 2).Y) * scaleY);
            //    }
            //}
            //// TODO: What if the last segment if a bezier
            //yield return new Line(
            //    (int)(xOffset + (fontX + _x[end]) * scaleX),
            //    (int)(yOffset + (fontY + _y[end]) * scaleY),
            //    (int)(xOffset + (fontX + _x[begin]) * scaleX),
            //    (int)(yOffset + (fontY + _y[begin]) * scaleY));
        }
        public IEnumerable<Segment> CountourGetSegmentIter2(int contourIndex,
            int fontX, int fontY,
            float xOffset, float yOffset, float scaleX, float scaleY)
        {
            throw new NotSupportedException();
            //var pts = new List<FtPoint>(InsertImplicit(GetContourPoints(contourIndex)));

            //var begin = GetContourBegin(contourIndex);
            //var end = GetContourEnd(contourIndex);
            //for (int i = 0; i < end - begin; i += pts[(i + 1) % pts.Count].On ? 1 : 2)
            //{
            //    if (pts[(i + 1) % pts.Count].On)
            //    {
            //        yield return new Line(
            //            (int)(xOffset + (fontX + pts[i].X) * scaleX),
            //            (int)(yOffset + (fontY + pts[i].Y) * scaleY),
            //            (int)(xOffset + (fontX + Circular(pts, i + 1).X) * scaleX),
            //            (int)(yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY));
            //    }
            //    else
            //    {
            //        yield return new Bezier(
            //            xOffset + (fontX + pts[i].X) * scaleX,
            //            yOffset + (fontY + pts[i].Y) * scaleY,
            //            xOffset + (fontX + Circular(pts, i + 1).X) * scaleX,
            //            yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY,
            //            xOffset + (fontX + Circular(pts, i + 2).X) * scaleX,
            //            yOffset + (fontY + Circular(pts, i + 2).Y) * scaleY);
            //    }
            //}
            //// TODO: What if the last segment if a bezier
            //yield return new Line(
            //    (int)(xOffset + (fontX + _x[end]) * scaleX),
            //    (int)(yOffset + (fontY + _y[end]) * scaleY),
            //    (int)(xOffset + (fontX + _x[begin]) * scaleX),
            //    (int)(yOffset + (fontY + _y[begin]) * scaleY));
        }
        int GetContourBegin(int contourIndex)
        {
            if (contourIndex == 0) return 0;
            return _contourEndPoints[contourIndex - 1] + 1;
        }

        int GetContourEnd(int contourIndex)
        {
            return _contourEndPoints[contourIndex];
        }

        public short[] X { get { return _x; } }
        public short[] Y { get { return _y; } }

    }
}
