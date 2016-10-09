//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.IO;

namespace NRasterizer.Tables
{
    class Head
    {
        readonly short _indexToLocFormat;
        readonly Bounds _bounds;
        readonly ushort _unitsPerEm;

        public Bounds Bounds { get { return _bounds; } }
        public bool WideGlyphLocations { get { return _indexToLocFormat > 0; } }

        private Head(BinaryReader input)
        {
            uint version = input.ReadUInt32(); // 0x00010000 for version 1.0.
            uint fontRevision = input.ReadUInt32();
            uint checkSumAdjustment = input.ReadUInt32();
            uint magicNumber = input.ReadUInt32();
            if (magicNumber != 0x5F0F3CF5) throw new ApplicationException("Invalid magic number!" + magicNumber.ToString("x"));

            ushort flags = input.ReadUInt16();
            _unitsPerEm = input.ReadUInt16(); // valid is 16 to 16384
            ulong created = input.ReadUInt64(); //  International date (8-byte field). (?)
            ulong modified = input.ReadUInt64();

            // bounding box for all glyphs
            _bounds = BoundsReader.ReadFrom(input);

            ushort macStyle = input.ReadUInt16();
            ushort lowestRecPPEM = input.ReadUInt16();
            short fontDirectionHint = input.ReadInt16();
            _indexToLocFormat = input.ReadInt16(); // 0 for 16-bit offsets, 1 for 32-bit.
            short glyphDataFormat = input.ReadInt16(); // 0
        }

        internal static Head From(TableEntry table)
        {
            return new Head(table.GetDataReader());
        }

        public ushort UnitsPerEm { get { return _unitsPerEm; } }
    }
}
