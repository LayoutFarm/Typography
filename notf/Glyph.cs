
using System;
using System.Collections.Generic;
using System.IO;
namespace notf
{
    public class Glyph
    {    
        private static Glyph ReadSimpleGlyph(BinaryReader input, int count)
        {
            Console.WriteLine("Simple!");
            return null;
        }

        private static Glyph ReadCompositeGlyph(BinaryReader input, int count)
        {
            Console.WriteLine("Compound!");
            return null;
        }

        internal static List<Glyph> From(TableEntry table, ushort glyphCount)
        {
            var input = table.GetDataReader();

            var glyphs = new List<Glyph>(glyphCount);
            for (int i=0; i<glyphCount; i++)
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
            return glyphs;
        }
    }
}
