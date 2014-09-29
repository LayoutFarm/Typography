using System.Collections.Generic;

namespace NRasterizer
{
    public struct Line
    {
        public Line(short x0, short y0, short x1, short y1, bool on)
        {
            this.x0 = x0;
            this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            this.on = on;
        }
        public short x0;
        public short y0;
        public short x1;
        public short y1;
        public bool on;
    }

    public class Glyph
    {
        private readonly short[] _x;
        private readonly short[] _y;
        private readonly bool[] _on;
        private readonly ushort[] _contourEndPoints;
        private readonly Bounds _bounds;

        public static readonly Glyph Empty = new Glyph(new short[0], new short[0], new bool[0], new ushort[0], Bounds.Zero);
        
        public Glyph(short[] x, short[] y, bool[] on, ushort[] contourEndPoints, Bounds bounds)
        {
            _x = x;
            _y = y;
            _on = on;
            _contourEndPoints = contourEndPoints;
            _bounds = bounds;
        }

        public Bounds Bounds { get { return _bounds; } }

        public int ContourCount { get { return _contourEndPoints.Length; } }

        // Returns indices to the X/Y/On arrays
        public IEnumerable<Line> GetContourIterator(int contourIndex)
        {
            var begin = GetContourBegin(contourIndex);
            var end = GetContourEnd(contourIndex);
            for (int i = begin; i < end; i++)
            {
                yield return new Line(_x[i], _y[i], _x[i+1], _y[i+1], _on[i]);
            }
            yield return new Line(_x[end], _y[end], _x[begin], _y[begin], _on[end]);
        }

        private int GetContourBegin(int contourIndex)
        {
            if (contourIndex == 0) return 0;
            return _contourEndPoints[contourIndex - 1];
        }

        private int GetContourEnd(int contourIndex)
        {
            return _contourEndPoints[contourIndex];
        }

        public short[] X { get { return _x; } }
        public short[] Y { get { return _y; } }
        public bool[] On { get { return _on; } }
    }
}
