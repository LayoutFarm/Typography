//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;
namespace Typography.OpenFont.Tables
{
    class Head : TableEntry
    {
        public const string Name = "head";
        //
        short _indexToLocFormat;
        Bounds _bounds;
        public Head(TableHeader header, BinaryReader input) : base(header, input)
        {
            Version = input.ReadUInt32(); // 0x00010000 for version 1.0.
            FontRevision = input.ReadUInt32();
            CheckSumAdjustment = input.ReadUInt32();
            MagicNumber = input.ReadUInt32();
            if (MagicNumber != 0x5F0F3CF5) throw new Exception("Invalid magic number! " + MagicNumber.ToString("x"));
            Flags = input.ReadUInt16();
            UnitsPerEm = input.ReadUInt16(); // valid is 16 to 16384
            Created = input.ReadUInt64(); //  International date (8-byte field). (?)
            Modified = input.ReadUInt64();
            // bounding box for all glyphs
            _bounds = Utils.ReadBounds(input);
            MacStyle = input.ReadUInt16();
            LowestRecPPEM = input.ReadUInt16();
            FontDirectionHint = input.ReadInt16();
            _indexToLocFormat = input.ReadInt16(); // 0 for 16-bit offsets, 1 for 32-bit.
            GlyphDataFormat = input.ReadInt16(); // 0
        }

        public uint Version { get; private set; }
        public uint FontRevision { get; private set; }
        public uint CheckSumAdjustment { get; private set; }
        public uint MagicNumber { get; private set; }
        public ushort Flags { get; private set; }
        public ushort UnitsPerEm { get; private set; }
        public ulong Created { get; private set; }
        public ulong Modified { get; private set; }
        public Bounds Bounds => _bounds;
        public ushort MacStyle { get; private set; }
        public ushort LowestRecPPEM { get; private set; }
        public short FontDirectionHint { get; private set; }
        public bool WideGlyphLocations => _indexToLocFormat > 0;
        public short GlyphDataFormat { get; private set; }

    }
}
