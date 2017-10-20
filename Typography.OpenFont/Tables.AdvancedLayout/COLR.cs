// Copyright © 2017 Sam Hocevar <sam@hocevar.net>
// Apache2

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    public class COLR : TableEntry
    {
        public override string Name { get { return "COLR"; } }

        // Read the COLR table
        // https://www.microsoft.com/typography/otspec/colr.htm
        protected override void ReadContentFrom(BinaryReader reader)
        {
            long offset = reader.BaseStream.Position;

            ushort version = reader.ReadUInt16();
            ushort glyphCount = reader.ReadUInt16();
            uint glyphsOffset = reader.ReadUInt32();
            uint layersOffset = reader.ReadUInt32();
            ushort layersCount = reader.ReadUInt16();
            GlyphLayers = new ushort[layersCount];
            GlyphPalettes = new ushort[layersCount];

            reader.BaseStream.Seek(offset + glyphsOffset, SeekOrigin.Begin);
            for (int i = 0; i < glyphCount; ++i)
            {
                ushort gid = reader.ReadUInt16();
                LayerIndices[gid] = reader.ReadUInt16();
                LayerCounts[gid] = reader.ReadUInt16();
            }

            reader.BaseStream.Seek(offset + layersOffset, SeekOrigin.Begin);
            for (int i = 0; i < GlyphLayers.Length; ++i)
            {
                GlyphLayers[i] = reader.ReadUInt16();
                GlyphPalettes[i] = reader.ReadUInt16();
            }
        }

        public ushort[] GlyphLayers { get; private set; }
        public ushort[] GlyphPalettes { get; private set; }
        public readonly Dictionary<ushort, ushort> LayerIndices = new Dictionary<ushort, ushort>();
        public readonly Dictionary<ushort, ushort> LayerCounts = new Dictionary<ushort, ushort>();
    }
}

