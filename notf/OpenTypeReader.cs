using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using notf.Tables;

namespace notf
{
    public class OpenTypeReader
    {
        public void Reader(Stream stream)
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
                var glyphs = Glyph.From(tables.Single(t => t.Tag == "glyf"), maximumProfile.GlyphCount);

            }
        }

    }
}
