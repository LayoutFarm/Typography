//MIT, 2019-present, WinterDev
using System;
using System.IO;

namespace Typography.OpenFont.Tables
{
    //https://www.microsoft.com/typography/otspec/gvar.htm

    class GVar : TableEntry
    {
        public const string Name = "gvar";

        public ushort axisCount;
        //
        internal GVar(TableHeader header, BinaryReader reader) : base(header, reader)
        {
            //'gvar' header

            //The glyph variations table header format is as follows:

            //'gvar' header:
            //Type              Name                Description
            //uint16            majorVersion        Major version number of the glyph variations table — set to 1.
            //uint16            minorVersion        Minor version number of the glyph variations table — set to 0.
            //uint16            axisCount           The number of variation axes for this font.
            //                                      This must be the same number as axisCount in the 'fvar' table.
            //uint16            sharedTupleCount    The number of shared tuple records.
            //                                      Shared tuple records can be referenced within glyph variation data tables for multiple glyphs,
            //                                      as opposed to other tuple records stored directly within a glyph variation data table.
            //Offset32          sharedTuplesOffset  Offset from the start of this table to the shared tuple records.
            //uint16            glyphCount          The number of glyphs in this font.This must match the number of glyphs stored elsewhere in the font.
            //uint16            flags               Bit-field that gives the format of the offset array that follows.
            //                                      If bit 0 is clear, the offsets are uint16; 
            //                                      if bit 0 is set, the offsets are uint32.
            //Offset32          glyphVariationDataArrayOffset   Offset from the start of this table to the array of GlyphVariationData tables.
            //
            //Offset16-
            //-or- Offset32     glyphVariationDataOffsets[glyphCount + 1] Offsets from the start of the GlyphVariationData array to each GlyphVariationData table.
            //     
            //-------------


            long beginAt = reader.BaseStream.Position;

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            axisCount = reader.ReadUInt16();
            ushort sharedTupleCount = reader.ReadUInt16();
            uint sharedTuplesOffset = reader.ReadUInt32();
            ushort glyphCount = reader.ReadUInt16();
            ushort flags = reader.ReadUInt16();
            uint glyphVariationDataArrayOffset = reader.ReadUInt32();

            uint[]? glyphVariationDataOffsets = null;
            if ((flags & 0x1) == 0)
            {
                //bit 0 is clear-> use Offset16
                glyphVariationDataOffsets = reader.ReadUInt16ArrayAsUInt32Array(glyphCount + 1);
                //
                //***If the short format (Offset16) is used for offsets, 
                //the value stored is the offset divided by 2.
                //Hence, the actual offset for the location of the GlyphVariationData table within the font 
                //will be the value stored in the offsets array multiplied by 2.

                for (int i = 0; i < glyphVariationDataOffsets.Length; ++i)
                {
                    glyphVariationDataOffsets[i] *= 2;
                }
            }
            else
            {
                //Offset32
                glyphVariationDataOffsets = reader.ReadUInt32Array(glyphCount + 1);
            }

            reader.BaseStream.Position = beginAt + sharedTuplesOffset;
            ReadShareTupleArray(reader, sharedTupleCount);
            //             
            reader.BaseStream.Position = beginAt + glyphVariationDataArrayOffset;
            ReadGlyphVariationData(reader, glyphVariationDataOffsets);
        }
        void ReadShareTupleArray(BinaryReader reader, ushort sharedTupleCount)
        {
            //-------------
            //Shared tuples array
            //-------------
            //The shared tuples array provides a set of variation-space positions
            //that can be referenced by variation data for any glyph. 
            //The shared tuples array follows the GlyphVariationData offsets array
            //at the end of the 'gvar' header.
            //This data is simply an array of tuple records, each representing a position in the font’s variation space.

            //Shared tuples array:
            //Type            Name                            Description
            //TupleRecord     sharedTuples[sharedTupleCount]  Array of tuple records shared across all glyph variation data tables.

            //Tuple records that are in the shared array or
            //that are contained directly within a given glyph variation data table 
            //use 2.14 values to represent normalized coordinate values.
            //See the Common Table Formats chapter for details.

            //

            //Tuple Records
            //https://docs.microsoft.com/en-us/typography/opentype/spec/otvarcommonformats
            //The tuple variation store formats make reference to regions within the font’s variation space using tuple records. 
            //These references identify positions in terms of normalized coordinates, which use F2DOT14 values.
            //Tuple record(F2DOT14):
            //Type Name    Description
            //F2DOT14     coordinates[axisCount]  Coordinate array specifying a position within the font’s variation space.
            //                                    The number of elements must match the axisCount specified in the 'fvar' table.


            TupleRecord[] tubleRecords = new TupleRecord[sharedTupleCount];
            for (int i = 0; i < sharedTupleCount; ++i)
            {
                TupleRecord rec = new TupleRecord();
                float[] coords = new float[axisCount];
                rec.coords = coords;
                for (int n = 0; n < axisCount; ++n)
                {
                    coords[n] = reader.ReadF2Dot14();
                }
                tubleRecords[i] = rec;
            }

        }
        void ReadGlyphVariationData(BinaryReader reader, uint[] glyphVariationDataOffsets)
        {

            //------------
            //The glyphVariationData table array
            //The glyphVariationData table array follows the 'gvar' header and shared tuples array.
            //Each glyphVariationData table describes the variation data for a single glyph in the font.

            //GlyphVariationData header:
            //Type                  Name                    Description
            //uint16                tupleVariationCount     A packed field.
            //                                              The high 4 bits are flags, 
            //                                              and the low 12 bits are the number of tuple variation tables for this glyph.
            //                                              The number of tuple variation tables can be any number between 1 and 4095.
            //Offset16              dataOffset              Offset from the start of the GlyphVariationData table to the serialized data
            //TupleVariationHeader  tupleVariationHeaders[tupleCount]   Array of tuple variation headers.


            long beginAt = reader.BaseStream.Position;
            ushort tupleVariationCount = reader.ReadUInt16();
            ushort dataOffset = reader.ReadUInt16();
            int flags = tupleVariationCount >> 12; //uppper 4 bits
            int tupleCount = tupleVariationCount & 0xFFF;//low 12 bits are the number of tuple variation tables for this glyph

            TupleVariationHeader[] headers = new TupleVariationHeader[tupleCount];
            for (int i = 0; i < tupleCount; ++i)
            {
               
                TupleVariationHeader header = new TupleVariationHeader();
                header.variableDataSize = (short)reader.ReadUInt16();
                header.tupleIndex = reader.ReadUInt16();

                TupleIndexFormat format = (TupleIndexFormat)(header.tupleIndex >> 8); //The high 4 bits are flags(see below).
                int indexToSharedTubleRecArrat = header.tupleIndex & 0x0FFF; // The low 12 bits are an index into a shared tuple records array.
                if ((format & TupleIndexFormat.EMBEDDED_PEAK_TUPLE) != 0)
                {
                    //read peakTuple
                }
                if ((format & TupleIndexFormat.INTERMEDIATE_REGION) != 0)
                {
                    //read start and end tuple
                }
                headers[i] = header;
            }
        }

      
        
    }
}



