//MIT, 2017, WinterDev
//MIT, 2015, Michael Popoloski, WinterDev

using System;
using System.IO;

namespace Typography.OpenFont.Tables
{
    /// <summary>
    /// EBLC : Embedded bitmap location data
    /// </summary>
    class EBLCTable : TableEntry
    {
        //from https://www.microsoft.com/typography/otspec/eblc.htm
        //EBLC - Embedded Bitmap Location Table
        //----------------------------------------------
        //The EBLC provides embedded bitmap locators.It is used together with the EDBTtable, which provides embedded, monochrome or grayscale bitmap glyph data, and the EBSC table, which provided embedded bitmap scaling information.
        //OpenType embedded bitmaps are called 'sbits' (for “scaler bitmaps”). A set of bitmaps for a face at a given size is called a strike.
        //The 'EBLC' table identifies the sizes and glyph ranges of the sbits, and keeps offsets to glyph bitmap data in indexSubTables.The 'EBDT' table then stores the glyph bitmap data, also in a number of different possible formats.Glyph metrics information may be stored in either the 'EBLC' or 'EBDT' table, depending upon the indexSubTable and glyph bitmap formats. The 'EBSC' table identifies sizes that will be handled by scaling up or scaling down other sbit sizes.
        //The 'EBLC' table uses the same format as the Apple Apple Advanced Typography (AAT) 'bloc' table.
        //The 'EBLC' table begins with a header containing the table version and number of strikes.An OpenType font may have one or more strikes embedded in the 'EBDT' table.
        //----------------------------------------------
        //eblcHeader 
        //----------------------------------------------
        //Type      Name            Description
        //uint16    majorVersion    Major version of the EBLC table, = 2.
        //uint16    minorVersion    Minor version of the EBLC table, = 0.
        //uint32    numSizes        Number of bitmapSizeTables
        //----------------------------------------------
        //Note that the first version of the EBLC table is 2.0.
        //The eblcHeader is followed immediately by the bitmapSizeTable array(s). 
        //The numSizes in the eblcHeader indicates the number of bitmapSizeTables in the array.
        //Each strike is defined by one bitmapSizeTable.

        public override string Name
        {
            get { return "EBLC"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            // load each strike table
            int beginPos = (int)reader.BaseStream.Position;
            //
            ushort versionMajor = reader.ReadUInt16();
            ushort versionMinor = reader.ReadUInt16();
            uint numSizes = reader.ReadUInt32();

            if (numSizes > MaxBitmapStrikes)
                throw new Exception("Too many bitmap strikes in font.");

            //----------------
            var sizeTableHeaders = new BitmapSizeTable[numSizes];


            //int skipLen = sizeof(uint) + sizeof(ushort) * 2 + 12 * 2;
            for (int i = 0; i < numSizes; i++)
            {

                //bitmapSizeTable

                //Type      Name                         Description
                //Offset32  indexSubTableArrayOffset     offset to index subtable from beginning of EBLC.
                //uint32    indexTablesSize              number of bytes in corresponding index subtables and array
                //uint32    numberOfIndexSubTables       an index subtable for each range or format change
                //uint32    colorRef                     not used; set to 0.
                //sbitLineMetrics hori                   line metrics for text rendered horizontally
                //sbitLineMetrics vert                   line metrics for text rendered vertically
                //uint16    startGlyphIndex              lowest glyph index for this size
                //uint16    endGlyphIndex                highest glyph index for this size
                //uint8     ppemX                        horizontal pixels per Em
                //uint8     ppemY                        vertical pixels per Em
                //uint8     bitDepth                     the Microsoft rasterizer v.1.7 or greater supports the following bitDepth values, as described below: 1, 2, 4, and 8.
                //int8      flags                        vertical or horizontal(see bitmapFlags)

                //
                //sbitLineMetrics  (12 bytes)
                //Type    Name
                //int8    ascender
                //int8    descender
                //uint8   widthMax
                //int8    caretSlopeNumerator
                //int8    caretSlopeDenominator
                //int8    caretOffset
                //int8    minOriginSB
                //int8    minAdvanceSB
                //int8    maxBeforeBL
                //int8    minAfterBL
                //int8    pad1
                //int8    pad2

                //
                BitmapSizeTable bmpsizeTable = new BitmapSizeTable();
                bmpsizeTable.SubTableOffset = reader.ReadUInt32();
                bmpsizeTable.SubTableSize = reader.ReadUInt32();
                bmpsizeTable.SubTableCount = reader.ReadUInt32();
                reader.ReadUInt32();//not use, colorRef The colorRef and bitDepth fields are reserved for future enhancements 

                //
                //metrics entries 
                reader.BaseStream.Position += (12 * 2); //skip line matric

                //
                bmpsizeTable.startGlyph = reader.ReadUInt16();
                bmpsizeTable.endGlyph = reader.ReadUInt16();
                bmpsizeTable.PpemX = reader.ReadByte();
                bmpsizeTable.PpemY = reader.ReadByte();
                bmpsizeTable.BitDepth = reader.ReadByte(); //The colorRef and bitDepth fields are reserved for future enhancements. For monochrome bitmaps they should have the values colorRef=0 and bitDepth=1.

                //
                //The 'flags' byte contains two bits to indicate the direction of small glyph metrics: horizontal or vertical.The remaining bits are reserved.
                bmpsizeTable.Flags = (BitmapSizeFlags)reader.ReadByte();

                sizeTableHeaders[i] = bmpsizeTable; //save
            }

            // read index subtables
            var indexSubTables = new IndexSubTable[numSizes];
            for (int i = 0; i < numSizes; i++)
            {
                reader.BaseStream.Seek(beginPos + sizeTableHeaders[i].SubTableOffset, SeekOrigin.Begin);
                //--------------------------------------------
                //indexSubTableArray
                //Type      Name                                Description
                //uint16    firstGlyphIndex                     first glyph code of this range
                //uint16    lastGlyphIndex                      last glyph code of this range(inclusive)
                //Offset32  additionalOffsetToIndexSubtable     add to indexSubTableArrayOffset to get offset from beginning of 'EBLC'
                //--------------------------------------------

                indexSubTables[i] = new IndexSubTable
                {
                    FirstGlyph = reader.ReadUInt16(),
                    LastGlyph = reader.ReadUInt16(),
                    Offset = reader.ReadUInt32()
                };
            }

            // read the actual data for each strike table
            for (int i = 0; i < numSizes; i++)
            {
                // read the subtable header

                reader.BaseStream.Seek(beginPos + sizeTableHeaders[i].SubTableOffset + indexSubTables[i].Offset, SeekOrigin.Begin);
                //--------------------------------------------
                //indexSubHeader
                //--------------------------------------------
                //Type          Name            Description
                //uint16        indexFormat     format of this indexSubTable
                //uint16        imageFormat     format of 'EBDT' image data
                //Offset32      imageDataOffset offset to image data in 'EBDT' table
                ushort indexFormat = reader.ReadUInt16();
                ushort imageFormat = reader.ReadUInt16();
                uint imageDataOffset = reader.ReadUInt32();
                //There are currently five different formats used for the indexSubTable, 
                //depending upon the size and type of bitmap data in the glyph code range. 
                //Apple 'bloc' tables support only formats 1 through 3.
                //The choice of which indexSubTable format to use is up to the font manufacturer, 
                //but should be made with the aim of minimizing the size of the font file.
                //Ranges of glyphs with variable metrics -that is, where glyphs may differ from each other in bounding box height, 
                //width, side bearings or advance - must use format 1, 3 or 4.Ranges of glyphs with constant metrics can save space by using format 2 or 5, which keep a single copy of the metrics information in the indexSubTable rather than a copy per glyph in the 'EBDT' table.In some monospaced fonts it makes sense to store extra white space around some of the glyphs to keep all metrics identical, thus permitting the use of format 2 or 5.
                //Structures for each indexSubTable format are listed below.

                //TODO: impl this
            }
        }
        struct BitmapSizeTable
        {
            public uint SubTableOffset;
            public uint SubTableSize;
            public uint SubTableCount;
            public ushort startGlyph;
            public ushort endGlyph;

            public byte PpemX;
            public byte PpemY;
            public byte BitDepth;
            //bitDepth
            //Value   Description
            //1	      black/white
            //2	      4 levels of gray
            //4	      16 levels of gray
            //8	      256 levels of gray

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
            None = 0,
            Horizontal = 1,
            Vertical = 2
        }

        const int MaxBitmapStrikes = 1024;
    }
}