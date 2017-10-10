// Copyright © 2017 Sam Hocevar <sam@hocevar.net>
// Apache2

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    public class CPAL : TableEntry
    {
        public override string Name { get { return "CPAL"; } }

        // Read the CPAL table
        // https://www.microsoft.com/typography/otspec/cpal.htm
        protected override void ReadContentFrom(BinaryReader reader)
        {
            long offset = reader.BaseStream.Position;

            ushort version = reader.ReadUInt16();
            ushort entryCount = reader.ReadUInt16(); // XXX: unused?
            ushort paletteCount = reader.ReadUInt16();
            Colors = new byte[reader.ReadUInt16()][];
            uint colorsOffset = reader.ReadUInt32();

            Palettes = Utils.ReadUInt16Array(reader, paletteCount);

            reader.BaseStream.Seek(offset + colorsOffset, SeekOrigin.Begin);
            for (int i = 0; i < Colors.Length; ++i)
                Colors[i] = reader.ReadBytes(4);
        }

        public ushort[] Palettes { get; private set; }
        public byte[][] Colors { get; private set; }
    }
}

