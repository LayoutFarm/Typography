//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
namespace NRasterizer
{
   

    public class Glyph
    {
        readonly short[] _xs;
        readonly short[] _ys;
        readonly ushort[] _contourEndPoints;
        readonly Bounds _bounds;
        readonly bool[] _onCurves;
        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], Bounds.Zero);

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

    }
}
