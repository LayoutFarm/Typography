//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
namespace NRasterizer
{
    [Flags]
    enum Flag : byte
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
        readonly short[] _xs;
        readonly short[] _ys;
        readonly ushort[] _contourEndPoints;
        readonly Bounds _bounds;
        readonly bool[] _onCurves;
        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], Bounds.Zero);
        //internal Glyph(short[] xs, short[] ys, Flag[] flags, ushort[] contourEndPoints, Bounds bounds)
        //{
        //    _xs = xs;
        //    _ys = ys;
        //    _contourEndPoints = contourEndPoints;
        //    _bounds = bounds;

        //}
        public Glyph(short[] xs, short[] ys, bool[] onCurves, ushort[] contourEndPoints, Bounds bounds)
        {
            _xs = xs;
            _ys = ys;
            _onCurves = onCurves;
            _contourEndPoints = contourEndPoints;
            _bounds = bounds;
        }
        internal short[] Xs { get { return _xs; } }
        internal short[] Ys { get { return _ys; } }
        public Bounds Bounds { get { return _bounds; } }
        public ushort[] EndPoints { get { return _contourEndPoints; } }
        public bool[] OnCurves { get { return _onCurves; } }
        public FtPoint[] GetPoints()
        {

            int j = _xs.Length;
            FtPoint[] points = new FtPoint[j];
            for (int i = 0; i < j; ++i)
            {
                points[i] = new FtPoint(_xs[i], _ys[i]);
            }
            return points;
        }
    }
}
