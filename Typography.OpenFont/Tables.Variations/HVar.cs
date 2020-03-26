//MIT, 2019-present, WinterDev
using System;
using System.IO;

namespace Typography.OpenFont.Tables
{

    //https://docs.microsoft.com/en-us/typography/opentype/spec/hvar

    /// <summary>
    /// HVAR — Horizontal Metrics Variations Table
    /// </summary>
    class HVar : TableEntry
    {
        public const string Name = "HVAR";


        ItemVariationStoreTable _itemVartionStore;
        public HVar(TableHeader header, BinaryReader reader) : base(header, reader)
        {
            //The HVAR table is used in variable fonts to provide variations for horizontal glyph metrics values.
            //This can be used to provide variation data for advance widths in the 'hmtx' table.
            //In fonts with TrueType outlines, it can also be used to provide variation data for left and right side
            //bearings obtained from the 'hmtx' table and glyph bounding box.
            long beginAt = reader.BaseStream.Position;

            //Horizontal metrics variations table:
            //Type      Name                        Description
            //uint16    majorVersion                Major version number of the horizontal metrics variations table — set to 1.
            //uint16    minorVersion                Minor version number of the horizontal metrics variations table — set to 0.
            //Offset32  itemVariationStoreOffset    Offset in bytes from the start of this table to the item variation store table.
            //Offset32  advanceWidthMappingOffset   Offset in bytes from the start of this table to the delta-set index mapping for advance widths (may be NULL).
            //Offset32  lsbMappingOffset            Offset in bytes from the start of this table to the delta - set index mapping for left side bearings(may be NULL).
            //Offset32  rsbMappingOffset            Offset in bytes from the start of this table to the delta - set index mapping for right side bearings(may be NULL).            

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            uint itemVariationStoreOffset = reader.ReadUInt32();
            uint advanceWidthMappingOffset = reader.ReadUInt32();
            uint lsbMappingOffset = reader.ReadUInt32();
            uint rsbMappingOffset = reader.ReadUInt32();
            //
            //-----------------------------------------

            //itemVariationStore
            reader.BaseStream.Position = beginAt + itemVariationStoreOffset;
            _itemVartionStore = new ItemVariationStoreTable(reader);
        }
    }
}