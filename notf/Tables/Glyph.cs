using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace notf.Tables
{
    public class Glyph
    {
        private readonly byte[] _instructions;
        private readonly short[] _x;
        private readonly short[] _y;

        public Glyph(byte[] instructions, short[] x, short[] y)
        {
            _instructions = instructions;
            _x = x;
            _y = y;
        }
 
        private static Glyph ReadSimpleGlyph(BinaryReader input, int count)
        {
            var endPoints = new ushort[count];
            for (int i=0; i<count; i++)
            {
                endPoints[i] = input.ReadUInt16();
            }

            var instructionSize = input.ReadUInt16();
            var instructions = input.ReadBytes(instructionSize);

            var pointCount = endPoints[count - 1] + 1; // TODO: count can be zero?

            var xs = new short[pointCount];
            var ys = new short[pointCount];
            var flags = input.ReadBytes(pointCount);
            short x = 0;
            short y = 0;
            for (int i = 0; i < pointCount; i++)
            {


                xs[i] = 0;
                ys[i] = 0;
            }

            return new Glyph(instructions, xs, ys);
        }

        private static Glyph ReadCompositeGlyph(BinaryReader input, int count)
        {
            return null;
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
                    var xMin = input.ReadInt16();
                    var yMin = input.ReadInt16();
                    var xMax = input.ReadInt16();
                    var yMax = input.ReadInt16();
                    if (contoursCount >= 0)
                    {
                        glyphs.Add(ReadSimpleGlyph(input, contoursCount));
                    }
                    else
                    {
                        glyphs.Add(ReadCompositeGlyph(input, -contoursCount));
                    }
                }
                else
                {
                    // Empty glyph
                }
            }
            return glyphs;
        }
    }
}
