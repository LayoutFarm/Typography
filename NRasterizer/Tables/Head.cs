using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tables
{
    internal class Head
    {
        private readonly short _indexToLocFormat;

        public bool WideGlyphLocations { get { return _indexToLocFormat > 0; } }

        private Head(BinaryReader input)
        {            
            var version = input.ReadUInt32(); // 0x00010000 for version 1.0.
            var fontRevision = input.ReadUInt32();
            var checkSumAdjustment = input.ReadUInt32();
            var magicNumber = input.ReadUInt32();
            if (magicNumber != 0x5F0F3CF5) throw new ApplicationException("Invalid magic number!" + magicNumber.ToString("x"));

            var flags = input.ReadUInt16();
            var unitsPerEm = input.ReadUInt16(); // valid is 16 to 16384
            var created = input.ReadUInt64(); //  International date (8-byte field). (?)
            var modified = input.ReadUInt64();
            
            // bounding box for all glyphs
            var xMin = input.ReadInt16();
            var yMin = input.ReadInt16();
            var xMax = input.ReadInt16();
            var yMax = input.ReadInt16();
            
            var macStyle = input.ReadUInt16();
            var lowestRecPPEM = input.ReadUInt16();
            var fontDirectionHint = input.ReadInt16();
            _indexToLocFormat = input.ReadInt16(); // 0 for 16-bit offsets, 1 for 32-bit.
            var glyphDataFormat = input.ReadInt16(); // 0
        }

        internal static Head From(TableEntry table)
        {
            return new Head(table.GetDataReader());
        }
    }
}
