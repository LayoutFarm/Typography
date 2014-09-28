using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{
    internal class Glyf
    {
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

            return new Glyph(xs, ys, flags.Select(f => f.HasFlag(Flag.OnCurve)).ToArray(), endPoints, bounds);
        }

        private static Glyph ReadCompositeGlyph(BinaryReader input, int count, Bounds bounds)
        {
            // TODO: Parse composite glyphs
            return Glyph.Empty;
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
                    var bounds = BoundsReader.ReadFrom(input);
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
