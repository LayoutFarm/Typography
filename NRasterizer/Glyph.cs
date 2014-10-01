using System.Collections.Generic;

namespace NRasterizer
{
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

        internal IEnumerable<Segment> GetContourIterator(int contourIndex,
            int fontX, int fontY,
            float xOffset, float yOffset, float scaleX, float scaleY)
        {
            var begin = GetContourBegin(contourIndex);
            var end = GetContourEnd(contourIndex);
            for (int i = begin; i < end; i += _on[i+1] ? 1 : 2)
            {
                if (_on[i + 1])
                {
                    yield return new Line(
                        (int)(xOffset + (fontX + _x[i]) * scaleX),
                        (int)(yOffset + (fontY + _y[i]) * scaleY),
                        (int)(xOffset + (fontX + _x[i + 1]) * scaleX),
                        (int)(yOffset + (fontY + _y[i + 1]) * scaleY));
                }
                else
                {
                    yield return new Bezier(
                        xOffset + (fontX + _x[i]) * scaleX,
                        yOffset + (fontY + _y[i]) * scaleY,
                        xOffset + (fontX + _x[i + 1]) * scaleX,
                        yOffset + (fontY + _y[i + 1]) * scaleY,
                        xOffset + (fontX + _x[i + 2]) * scaleX,
                        yOffset + (fontY + _y[i + 2]) * scaleY);
                }
            }
            // TODO: What if the last segment if a bezier
            yield return new Line(
                (int)(xOffset + (fontX + _x[end]) * scaleX),
                (int)(yOffset + (fontY + _y[end]) * scaleY),
                (int)(xOffset + (fontX +_x[begin]) * scaleX),
                (int)(yOffset + (fontY + _y[begin]) * scaleY));
        }

        private int GetContourBegin(int contourIndex)
        {
            if (contourIndex == 0) return 0;
            return _contourEndPoints[contourIndex - 1]+1;
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
