using NRasterizer.IO;
using NRasterizer.Tables;
using System;
using System.Collections.Generic;
using System.IO;


namespace NRasterizer
{
    public class OpenTypeReader
    {
        private TableEntry FindTable(IEnumerable<TableEntry> tables, string tableName)
        {
            foreach (TableEntry te in tables)
            {
                if (te.Tag == tableName)
                {
                    return te;
                }
            }
            return null;
        }
        public Typeface Read(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (BinaryReader input = new ByteOrderSwappingBinaryReader(stream))
            {
                uint version = input.ReadUInt32();
                ushort tableCount = input.ReadUInt16();
                ushort searchRange = input.ReadUInt16();
                ushort entrySelector = input.ReadUInt16();
                ushort rangeShift = input.ReadUInt16();

                var tables = new List<TableEntry>(tableCount);
                for (int i = 0; i < tableCount; i++)
                {
                    tables.Add(TableEntry.ReadFrom(input));
                }

                Head header = Head.From(FindTable(tables, "head"));
                MaxProfile maximumProfile = MaxProfile.From(FindTable(tables, "maxp"));
                GlyphLocations glyphLocations = new GlyphLocations(FindTable(tables, "loca"), maximumProfile.GlyphCount, header.WideGlyphLocations);
                List<Glyph> glyphs = Glyf.From(FindTable(tables, "glyf"), glyphLocations);
                List<CharacterMap> cmaps = CmapReader.From(FindTable(tables, "cmap"));

                var horizontalHeader = HorizontalHeader.From(FindTable(tables, "hhea"));
                var horizontalMetrics = HorizontalMetrics.From(FindTable(tables, "hmtx"),
                    horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount);

                return new Typeface(header.Bounds, header.UnitsPerEm, glyphs, cmaps, horizontalMetrics);
            }
        }

    }
}
