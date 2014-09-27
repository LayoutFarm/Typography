using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace notf.Tables
{
    public class Bounds
    {
        private readonly short _xmin;
        private readonly short _ymin;
        private readonly short _xmax;
        private readonly short _ymax;

        public static readonly Bounds Zero = new Bounds(0, 0, 0, 0);
        public Bounds(short xmin, short ymin, short xmax, short ymax)
        {
            _xmin = xmin;
            _ymin = ymin;
            _xmax = xmax;
            _ymax = ymax;
        }
        public short XMin { get { return _xmin; } }
        public short YMin { get { return _ymin; } }
        public short XMax { get { return _xmax; } }
        public short YMax { get { return _ymax; } }
    }

    public class Glyph
    {
        private readonly byte[] _instructions;
        private readonly short[] _x;
        private readonly short[] _y;
        private readonly Bounds _bounds;

        public static readonly Glyph Empty = new Glyph(new byte[0], new short[0], new short[0], Bounds.Zero);

        public Glyph(byte[] instructions, short[] x, short[] y, Bounds bounds)
        {
            _instructions = instructions;
            _x = x;
            _y = y;
            _bounds = bounds;
        }

        public short[] X { get { return _x; } }
        public short[] Y { get { return _y; } }
        public int PointCount { get { return _x.Length; } } // or y...

        public Bounds Bounds { get { return _bounds; } }

        [Flags]
        private enum Flag: byte
        {
            OnCurve = 1,
            XByte = 2,
            YByte = 4,
            Repeat = 8,
            XSignOrSame = 16,
            YSignOrSame = 32
        }

        private static Flag[] ReadFlags(BinaryReader input, int flagCount)
        {
            var result = new Flag[flagCount];
            int c = 0;
            int repeatCount = 0;
            var flag = (Flag)0;
            while (c < flagCount)
            {
                if (repeatCount > 0)
                {
                    repeatCount--;
                }
                else
                {
                    flag = (Flag)input.ReadByte();
                    if (flag.HasFlag(Flag.Repeat))
                    {
                        repeatCount = input.ReadByte();
                    }
                }
                result[c++] = flag;
            }
            return result;
        }

        private static short[] ReadCoordinates(BinaryReader input, int pointCount, Flag[] flags, Flag isByte, Flag signOrSame)
        {
            var xs = new short[pointCount];
            int x = 0;
            for (int i = 0; i < pointCount; i++)
            {
                int dx;
                if (flags[i].HasFlag(isByte))
                {
                    var b = input.ReadByte();
                    dx = flags[i].HasFlag(signOrSame) ? b : -b;
                }
                else
                {
                    if (flags[i].HasFlag(signOrSame))
                    {
                        dx = 0;
                    }
                    else
                    {
                        dx = input.ReadInt16();
                    }
                }
                x += dx;
                xs[i] = (short)x; // TODO: overflow?
            }
            return xs;
        }

        private static Glyph ReadSimpleGlyph(BinaryReader input, int count, Bounds bounds)
        {
            var endPoints = new ushort[count];
            for (int i=0; i<count; i++)
            {
                endPoints[i] = input.ReadUInt16();
            }

            var instructionSize = input.ReadUInt16();
            var instructions = input.ReadBytes(instructionSize);

            // TODO: should this take the max points rather?
            var pointCount = endPoints[count - 1] + 1; // TODO: count can be zero?

            var flags = ReadFlags(input, pointCount);
            var xs = ReadCoordinates(input, pointCount, flags, Flag.XByte, Flag.XSignOrSame);
            var ys = ReadCoordinates(input, pointCount, flags, Flag.YByte, Flag.YSignOrSame);

            return new Glyph(instructions, xs, ys, bounds);
        }

        private static Glyph ReadCompositeGlyph(BinaryReader input, int count, Bounds bounds)
        {
            // TODO: Parse composite glyphs
            return Glyph.Empty;
        }

        private static Bounds ReadBounds(BinaryReader input)
        {
            var xMin = input.ReadInt16();
            var yMin = input.ReadInt16();
            var xMax = input.ReadInt16();
            var yMax = input.ReadInt16();
            return new Bounds(xMin, yMin, xMax, yMax);
        }

        internal static List<Glyph> From(TableEntry table, GlyphLocations locations)
        {
            var glyphCount = locations.GlyphCount;

            var glyphs = new List<Glyph>(glyphCount);
            for (int i = 0; i < glyphCount; i++)
            {
                var input = table.GetDataReader();
                input.BaseStream.Seek(locations.Offsets[i], SeekOrigin.Current);

                var length = locations.Offsets[i + 1] - locations.Offsets[i];
                if (length > 0)
                {
                    var contoursCount = input.ReadInt16();
                    var bounds = ReadBounds(input);
                    if (contoursCount >= 0)
                    {
                        glyphs.Add(ReadSimpleGlyph(input, contoursCount, bounds));
                    }
                    else
                    {
                        glyphs.Add(ReadCompositeGlyph(input, -contoursCount, bounds));
                    }
                }
                else
                {
                    glyphs.Add(Glyph.Empty);
                }
            }
            return glyphs;
        }
    }
}
