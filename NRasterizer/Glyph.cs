using System.Linq;
using System.Collections.Generic;

namespace NRasterizer
{
    internal class Point
    {
        private readonly short _x;
        private readonly short _y;
        private readonly bool _on;

        public Point(short x, short y, bool on)
        {
            _x = x;
            _y = y;
            _on = on;
        }

        public short X { get { return _x; } }
        public short Y { get { return _y; } }
        public bool On { get { return _on; } }
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

        private Point At(int pointIndex)
        {
            return new Point(_x[pointIndex], _y[pointIndex], _on[pointIndex]);
        }

        private IEnumerable<Point> GetContourPoints(int contourIndex)
        {
            var begin = GetContourBegin(contourIndex);
            var end = GetContourEnd(contourIndex);
            for (int i = begin; i <= end; i++)
            {
                yield return At(i);
            }
        }

        private IEnumerable<Point> InsertImplicit(IEnumerable<Point> points)
        {
            var previous = points.First();
            yield return previous;
            foreach (var p in points.Skip(1))
            {
                if (!previous.On && !p.On)
                {
                    // implicit point on curve
                    yield return new Point((short)((previous.X + p.X) / 2), (short)((previous.Y + p.Y) / 2), true);
                }
                previous = p;
                yield return p;
            }
        }

        private T Circular<T>(List<T> list, int index)
        {
            return list[index % list.Count];
        }

        public IEnumerable<Segment> GetContourIterator(int contourIndex,
            int fontX, int fontY,
            float xOffset, float yOffset, float scaleX, float scaleY)
        {
            var pts = InsertImplicit(GetContourPoints(contourIndex)).ToList();

            var begin = GetContourBegin(contourIndex);
            var end = GetContourEnd(contourIndex);
            for (int i = 0; i < end - begin; i += pts[(i + 1) % pts.Count].On ? 1 : 2)
            {
                if (pts[(i + 1) % pts.Count].On)
                {
                    yield return new Line(
                        (int)(xOffset + (fontX + pts[i].X) * scaleX),
                        (int)(yOffset + (fontY + pts[i].Y) * scaleY),
                        (int)(xOffset + (fontX + Circular(pts, i + 1).X) * scaleX),
                        (int)(yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY));
                }
                else
                {
                    yield return new Bezier(
                        xOffset + (fontX + pts[i].X) * scaleX,
                        yOffset + (fontY + pts[i].Y) * scaleY,
                        xOffset + (fontX + Circular(pts, i + 1).X) * scaleX,
                        yOffset + (fontY + Circular(pts, i + 1).Y) * scaleY,
                        xOffset + (fontX + Circular(pts, i + 2).X) * scaleX,
                        yOffset + (fontY + Circular(pts, i + 2).Y) * scaleY);
                }
            }
            // TODO: What if the last segment if a bezier
            yield return new Line(
                (int)(xOffset + (fontX + _x[end]) * scaleX),
                (int)(yOffset + (fontY + _y[end]) * scaleY),
                (int)(xOffset + (fontX + _x[begin]) * scaleX),
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
