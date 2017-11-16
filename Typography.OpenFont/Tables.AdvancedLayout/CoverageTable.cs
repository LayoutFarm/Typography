//Apache2, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    class CoverageTable
    {
        //https://www.microsoft.com/typography/otspec/chapter2.htm
        ushort _format;
        ushort[] orderedGlyphIdList; //for format1
        //----------------------------------
        RangeRecord[] ranges;//for format2
        //----------------------------------

        public int FindPosition(ushort glyphIndex)
        {
            switch (_format)
            {
                //should not occur here
                default: throw new NotSupportedException();
                case 1:
                    // "The glyph indices must be in numerical order for binary searching of the list"
                    // (https://www.microsoft.com/typography/otspec/chapter2.htm#coverageFormat1)
                    return Array.BinarySearch(orderedGlyphIdList, glyphIndex);
                case 2:
                    // Ranges must be in glyph ID order, and they must be distinct, with no overlapping.
                    // [...] quick calculation of the Coverage Index for any glyph in any range using the
                    // formula: Coverage Index (glyphID) = startCoverageIndex + glyphID - startGlyphID.
                    // (https://www.microsoft.com/typography/otspec/chapter2.htm#coverageFormat2)
                    {
                        int n = Array.BinarySearch(ranges, glyphIndex);
                        return n < 0 ? -1 : ranges[n].startCoverageIndex + glyphIndex - ranges[n].start;
                    }
            }
        }

        public static CoverageTable CreateFrom(BinaryReader reader, long beginAt)
        {
            reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
            //---------------------------------------------------
            var coverageTable = new CoverageTable();

            //CoverageFormat1 table: Individual glyph indices
            //Type      Name                       Description
            //uint16    CoverageFormat             Format identifier-format = 1
            //uint16    GlyphCount  	           Number of glyphs in the GlyphArray
            //uint16    GlyphArray[GlyphCount] 	   Array of glyph IDs — in numerical order
            //---------------------------------
            //CoverageFormat2 table: Range of glyphs 
            //Type      Name                       Description           
            //uint16    CoverageFormat             Format identifier-format = 2
            //uint16    RangeCount                 Number of RangeRecords
            //struct    RangeRecord[RangeCount]    Array of glyph ranges — ordered by StartGlyphID.
            //------------
            //RangeRecord
            //----------
            //Type      Name                Description
            //uint16    StartGlyphID        First glyph ID in the range
            //uint16    EndGlyphID          Last glyph ID in the range
            //uint16    StartCoverageIndex  Coverage Index of first glyph ID in range
            //----------

            switch (coverageTable._format = reader.ReadUInt16())
            {
                default:
                    throw new NotSupportedException();
                case 1:    //CoverageFormat1 table: Individual glyph indices
                    {
                        ushort glyphCount = reader.ReadUInt16();
                        coverageTable.orderedGlyphIdList = Utils.ReadUInt16Array(reader, glyphCount);
                    }
                    break;
                case 2:  //CoverageFormat2 table: Range of glyphs
                    {

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

        public static CoverageTable[] CreateMultipleCoverageTables(long initPos, ushort[] offsets, BinaryReader reader)
        {
            int j = offsets.Length;
            CoverageTable[] results = new CoverageTable[j];
            for (int i = 0; i < j; ++i)
            {
                results[i] = CoverageTable.CreateFrom(reader, initPos + offsets[i]);
            }
            return results;
        }

        struct RangeRecord : IComparable
        {
            //------------
            //RangeRecord
            //----------
            //Type      Name                Description
            //uint16    StartGlyphID        First glyph ID in the range
            //uint16    EndGlyphID          Last glyph ID in the range
            //uint16    StartCoverageIndex  Coverage Index of first glyph ID in range
            //----------
            public readonly ushort start;
            public readonly ushort end;
            public readonly ushort startCoverageIndex;
            public RangeRecord(ushort start, ushort end, ushort startCoverageIndex)
            {
                this.start = start;
                this.end = end;
                this.startCoverageIndex = startCoverageIndex;
            }
            public bool Contains(ushort glyphIndex)
            {
                return glyphIndex >= start && glyphIndex <= end;
            }
            public int FindPosition(ushort glyphIndex)
            {
                return Contains(glyphIndex) ? glyphIndex - start : -1;
            }

            // This special comparator allows for binary searching inside all the ranges.
            public int CompareTo(object obj)
            {
                if (obj is ushort)
                {
                    ushort n = (ushort)obj;
                    return n < start ? 1 : n > end ? -1 : 0;
                }
                throw new NotImplementedException();
            }

#if DEBUG
            public override string ToString()
            {
                return "range: index, " + startCoverageIndex + "[" + start + "," + end + "]";
            }
#endif
        }

#if DEBUG
        public ushort[] dbugGetExpandedGlyphs()
        {
            switch (_format)
            {
                default:
                    throw new NotSupportedException();
                case 1:
                    return orderedGlyphIdList;
                case 2:
                    {
                        List<ushort> list = new List<ushort>();
                        int j = ranges.Length;
                        for (int i = 0; i < j; ++i)
                        {
                            RangeRecord range = ranges[i];
                            for (ushort n = range.start; n <= range.end; ++n)
                            {
                                list.Add(n);
                            }
                        }
                        return list.ToArray();
                    }

            }

        }
#endif
    }

}
