using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NRasterizer.IO;
using NRasterizer.Tables;

namespace NRasterizer
{
    public class OpenTypeReader
    {
        public Typeface Read(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (BinaryReader input = new ByteOrderSwappingBinaryReader(stream))
            {
                UInt32 version = input.ReadUInt32();
                UInt16 tableCount = input.ReadUInt16();
                UInt16 searchRange = input.ReadUInt16();
                UInt16 entrySelector = input.ReadUInt16();
                UInt16 rangeShift = input.ReadUInt16();

                var tables = new List<TableEntry>(tableCount);
                for (int i = 0; i < tableCount; i++)
                {
                    tables.Add(TableEntry.ReadFrom(input));
                }

                var header = Head.From(tables.Single(t => t.Tag == "head"));
                var maximumProfile = MaxProfile.From(tables.Single(t => t.Tag == "maxp"));
                var glyphLocations = new GlyphLocations(tables.Single(t => t.Tag == "loca"), maximumProfile.GlyphCount, header.WideGlyphLocations);
                var glyphs = Glyf.From(tables.Single(t => t.Tag == "glyf"), glyphLocations);

                var cmaps = CharacterMap.From(tables.Single(t => t.Tag == "cmap"));

                // Debug
                for (uint c = 0; c < 0x00A8; c++)
                {
                    var index = cmaps[0].CharacterToGlyphIndex(c);
                    Console.WriteLine("0x{0:x4} -> {1}", c, index);
                }

                return new Typeface(header.Bounds, glyphs, cmaps);
            }
        }

    }
}
