//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
namespace NRasterizer
{
    [Flags]
    public enum Flag : byte
    {
        OnCurve = 1,
        XByte = 1 << 1,
        YByte = 1 << 2,
        Repeat = 1 << 3,
        XSignOrSame = 1 << 4,
        YSignOrSame = 1 << 5
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
        public ushort[] EndPoints { get { return _contourEndPoints; } }
        public Flag[] Flags { get { return _flags; } }
        public FtPoint[] GetPoints()
        {

            int j = _x.Length;
            FtPoint[] points = new FtPoint[j];
            for (int i = 0; i < j; ++i)
            {
                points[i] = new FtPoint(_x[i], _y[i]);
            }
            return points;
        }
    }
}
