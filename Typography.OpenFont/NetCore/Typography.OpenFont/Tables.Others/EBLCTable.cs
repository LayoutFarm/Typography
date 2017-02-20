//Apache2, 2017, WinterDev
//MIT, 2015, Michael Popoloski, WinterDev

using System; 
using System.IO; 

namespace Typography.OpenFont.Tables
{
    /// <summary>
    /// FontBitmapTable
    /// </summary>
    class EBLCTable : TableEntry
    {
        public override string Name
        {
            get { return "EBLC"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            // load each strike table
            int beginPos = (int)reader.BaseStream.Position;
            var count = reader.ReadInt32();
            if (count > MaxBitmapStrikes)
                throw new Exception("Too many bitmap strikes in font.");

            var sizeTableHeaders = new BitmapSizeTable[count];
            int skipLen = sizeof(uint) + sizeof(ushort) * 2 + 12 * 2;
            for (int i = 0; i < count; i++)
            {
                sizeTableHeaders[i].SubTableOffset = reader.ReadUInt32();
                sizeTableHeaders[i].SubTableSize = reader.ReadUInt32();
                sizeTableHeaders[i].SubTableCount = reader.ReadUInt32();

                // skip colorRef, metrics entries, start and end glyph indices                 
                reader.BaseStream.Position += skipLen;
                sizeTableHeaders[i].PpemX = reader.ReadByte();
                sizeTableHeaders[i].PpemY = reader.ReadByte();
                sizeTableHeaders[i].BitDepth = reader.ReadByte();
                sizeTableHeaders[i].Flags = (BitmapSizeFlags)reader.ReadByte();
            }

            // read index subtables
            var indexSubTables = new IndexSubTable[count];
            for (int i = 0; i < count; i++)
            {
                reader.BaseStream.Seek(beginPos + sizeTableHeaders[i].SubTableOffset, SeekOrigin.Begin);
                indexSubTables[i] = new IndexSubTable
                {
                    FirstGlyph = reader.ReadUInt16(),
                    LastGlyph = reader.ReadUInt16(),
                    Offset = reader.ReadUInt32()
                };
            }

            // read the actual data for each strike table
            for (int i = 0; i < count; i++)
            {
                // read the subtable header

                reader.BaseStream.Seek(beginPos + sizeTableHeaders[i].SubTableOffset + indexSubTables[i].Offset, SeekOrigin.Begin);
                var indexFormat = reader.ReadUInt16();
                var imageFormat = reader.ReadUInt16();
                var imageDataOffset = reader.ReadUInt32();

            }
        }
        struct BitmapSizeTable
        {
            public uint SubTableOffset;
            public uint SubTableSize;
            public uint SubTableCount;
            public byte PpemX;
            public byte PpemY;
            public byte BitDepth;
            public BitmapSizeFlags Flags;
        }

        struct IndexSubTable
        {
            public ushort FirstGlyph;
            public ushort LastGlyph;
            public uint Offset;
        }

        [Flags]
        enum BitmapSizeFlags
        {
            None,
            Horizontal,
            Vertical
        }

        const int MaxBitmapStrikes = 1024;
    }
}