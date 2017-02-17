//Apache2, 2016-2017, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
 

namespace Typography.OpenType.Tables
{
    class CoverageTable
    {
        //https://www.microsoft.com/typography/otspec/chapter2.htm
        ushort _format;
        ushort[] orderedGlyphIdList; //for format1
        //----------------------------------
        int[] rangeOffsets;
        RangeRecord[] ranges;//for format2
        //----------------------------------

        private CoverageTable()
        {
        }
        public int FindPosition(int glyphIndex)
        {
            switch (_format)
            {
                //should not occur here
                default: throw new NotSupportedException();
                case 1:
                    {
                        //TODO: imple fast search here                       

                        for (int i = orderedGlyphIdList.Length - 1; i >= 0; --i)
                        {
                            ushort gly = orderedGlyphIdList[i];
                            if (gly < glyphIndex)
                            {
                                return -1;//not found
                            }
                            else if (gly == glyphIndex)
                            {
                                return i;
                            }
                        }
                    }
                    break;
                case 2:
                    {
                        //return 'logical' coverage index

                        for (int i = ranges.Length - 1; i >= 0; --i)
                        {
                            RangeRecord range = ranges[i];
                            if (range.Contains(glyphIndex))
                            {
                                //found
                                //(glyphIndex - range.start) -> local offset
                                //then + overall rangeOffsets 
                                return (glyphIndex - range.start) + rangeOffsets[i];
                            }
                        }
                        //not found in range
                        return -1;
                    }

            }
            return -1;//not found
        }
        public static CoverageTable CreateFrom(BinaryReader reader, long beginAt)
        {
            reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
            //---------------------------------------------------
            var coverageTable = new CoverageTable();
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
                        coverageTable.UpdateRangeOffsets();
                    }
                    break;

            }
            return coverageTable;
        }

        void UpdateRangeOffsets()
        {
            //for format 2
#if DEBUG
            if (this._format != 2)
            {
                throw new NotSupportedException();
            }
#endif
            int j = ranges.Length;
            int total = 0;
            rangeOffsets = new int[j];
            for (int i = 0; i < j; ++i)
            {
                RangeRecord r = ranges[i];
                rangeOffsets[i] = total;
                total += r.Width;
            }

        }

        public static CoverageTable[] CreateMultipleCoverageTables(long initPos, short[] offsets, BinaryReader reader)
        {
            int j = offsets.Length;
            CoverageTable[] results = new CoverageTable[j];
            for (int i = 0; i < j; ++i)
            {
                results[i] = CoverageTable.CreateFrom(reader, initPos + offsets[i]);
            }
            return results;
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
            public bool Contains(int glyphIndex)
            {
                return glyphIndex >= start && glyphIndex <= end;
            }
            public int Width
            {
                get { return end - start + 1; }
            }
#if DEBUG
            public override string ToString()
            {
                return "range: index, " + startCoverageIndex + "[" + start + "," + end + "]";
            }
#endif
        }

        //------------------------------------------------------------------------------------------
        public static bool IsInRange(CoverageTable[] coverageTables, ushort cur_glyphIndex)
        {
            //just test
            //TODO: 
            //reduce loop by make this a dicision table*** 
            int j = coverageTables.Length;
            for (int i = 0; i < j; ++i)
            {
                int found = coverageTables[i].FindPosition(cur_glyphIndex);
                if (found > -1)
                {
                    return true;
                }
            }
            return false;
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