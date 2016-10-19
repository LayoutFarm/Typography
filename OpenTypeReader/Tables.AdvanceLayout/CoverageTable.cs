//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{
    class CoverageTable
    {
        //https://www.microsoft.com/typography/otspec/chapter2.htm
        ushort _format;
        RangeRecord[] ranges;
        ushort[] orderedGlyphIdList;
        private CoverageTable()
        {
        }
        public static CoverageTable ReadFrom(BinaryReader reader)
        {
            CoverageTable coverageTable = new CoverageTable();
            //1. format  
            switch (coverageTable._format = reader.ReadUInt16())
            {
                default:
                    throw new NotSupportedException();
                case 1:
                    {
                        //CoverageFormat1 table: Individual glyph indices
                        ushort glyphCount = reader.ReadUInt16();
                        //GlyphID 	GlyphArray[GlyphCount] 	Array of GlyphIDs-in numerical order ***
                        ushort[] orderedGlyphIdList = new ushort[glyphCount];
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            orderedGlyphIdList[i] = reader.ReadUInt16();
                        }
                        coverageTable.orderedGlyphIdList = orderedGlyphIdList;
                    } break;
                case 2:
                    {
                        //CoverageFormat2 table: Range of glyphs
                        ushort rangeCount = reader.ReadUInt16();
                        RangeRecord[] ranges = new RangeRecord[rangeCount];
                        for (int i = 0; i < rangeCount; ++i)
                        {
                            ranges[i] = new RangeRecord(
                                reader.ReadUInt16(),
                                reader.ReadUInt16(),
                                reader.ReadUInt16());
                        }
                        coverageTable.ranges = ranges;
                    }
                    break;

            }
            return coverageTable;
        }


        struct RangeRecord
        {
            //GlyphID 	Start 	First GlyphID in the range
            //GlyphID 	End 	Last GlyphID in the range
            //USHORT 	StartCoverageIndex 	Coverage Index of first GlyphID in range
            public readonly ushort start;
            public readonly ushort end;
            public readonly ushort startCoverageIndex;
            public RangeRecord(ushort start, ushort end, ushort startCoverageIndex)
            {
                this.start = start;
                this.end = end;
                this.startCoverageIndex = startCoverageIndex;
            }
#if DEBUG
            public override string ToString()
            {
                return "range: index, " + startCoverageIndex + "[" + start + "," + end + "]";
            }
#endif
        }
    }

}