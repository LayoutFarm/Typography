//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{

    class GPOS : TableEntry
    {
        long gposTableStartAt;
        ScriptList scriptList = new ScriptList();
        FeatureList featureList = new FeatureList();
        List<LookupTable> lookupRecords = new List<LookupTable>();
        public override string Name
        {
            get { return "GPOS"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            gposTableStartAt = reader.BaseStream.Position;
            //-------------------------------------------
            // GPOS Header
            //The GPOS table begins with a header that contains a version number for the table. Two versions are defined. Version 1.0 contains offsets to three tables: ScriptList, FeatureList, and LookupList. Version 1.1 also includes an offset to a FeatureVariations table. For descriptions of these tables, see the chapter, OpenType Layout Common Table Formats . Example 1 at the end of this chapter shows a GPOS Header table definition.
            //GPOS Header, Version 1.0
            //Value 	Type 	Description
            //USHORT 	MajorVersion 	Major version of the GPOS table, = 1
            //USHORT 	MinorVersion 	Minor version of the GPOS table, = 0
            //Offset 	ScriptList 	Offset to ScriptList table, from beginning of GPOS table
            //Offset 	FeatureList 	Offset to FeatureList table, from beginning of GPOS table
            //Offset 	LookupList 	Offset to LookupList table, from beginning of GPOS table

            //GPOS Header, Version 1.1
            //Value 	Type 	Description
            //USHORT 	MajorVersion 	Major version of the GPOS table, = 1
            //USHORT 	MinorVersion 	Minor version of the GPOS table, = 1
            //Offset 	ScriptList 	Offset to ScriptList table, from beginning of GPOS table
            //Offset 	FeatureList 	Offset to FeatureList table, from beginning of GPOS table
            //Offset 	LookupList 	Offset to LookupList table, from beginning of GPOS table
            //ULONG 	FeatureVariations 	Offset to FeatureVariations table, from beginning of GPOS table (may be NULL) 

            this.MajorVersion = reader.ReadUInt16();
            this.MinorVersion = reader.ReadUInt16();

            ushort scriptListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort featureListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort lookupListOffset = reader.ReadUInt16();//from beginning of GSUB table
            uint featureVariations = (MinorVersion == 1) ? reader.ReadUInt32() : 0;//from beginning of GSUB table

            //-----------------------
            //1. scriptlist
            reader.BaseStream.Seek(this.Header.Offset + scriptListOffset, SeekOrigin.Begin);
            scriptList.ReadFrom(reader);
            //-----------------------
            //2. feature list
            reader.BaseStream.Seek(this.Header.Offset + featureListOffset, SeekOrigin.Begin);
            featureList.ReadFrom(reader);
            //-----------------------
            //3. lookup list
            reader.BaseStream.Seek(this.Header.Offset + lookupListOffset, SeekOrigin.Begin);
            ReadLookupListTable(reader);
            //-----------------------
            //4. feature variations
            if (featureVariations > 0)
            {
                reader.BaseStream.Seek(this.Header.Offset + featureVariations, SeekOrigin.Begin);
                ReadFeaureVariations(reader);
            }

        }
        public ushort MajorVersion { get; private set; }
        public ushort MinorVersion { get; private set; }
        void ReadFeaureVariations(BinaryReader reader)
        {
            throw new NotImplementedException();
        }
        void ReadLookupListTable(BinaryReader reader)
        {
            long lookupListHeadPos = reader.BaseStream.Position;
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //LookupList table
            //Type 	Name 	Description
            //USHORT 	LookupCount 	Number of lookups in this table
            //Offset 	Lookup[LookupCount] 	Array of offsets to Lookup tables-from beginning of LookupList -zero based (first lookup is Lookup index = 0)
            //Lookup Table

            //A Lookup table (Lookup) defines the specific conditions, type, and results of a substitution or positioning action that is used to implement a feature. For example, a substitution operation requires a list of target glyph indices to be replaced, a list of replacement glyph indices, and a description of the type of substitution action.

            //Each Lookup table may contain only one type of information (LookupType), determined by whether the lookup is part of a GSUB or GPOS table. GSUB supports eight LookupTypes, and GPOS supports nine LookupTypes (for details about LookupTypes, see the GSUB and GPOS chapters of the document).

            //Each LookupType is defined with one or more subtables, and each subtable definition provides a different representation format. The format is determined by the content of the information required for an operation and by required storage efficiency. When glyph information is best presented in more than one format, a single lookup may contain more than one subtable, as long as all the subtables are the same LookupType. For example, within a given lookup, a glyph index array format may best represent one set of target glyphs, whereas a glyph index range format may be better for another set of target glyphs.

            //During text processing, a client applies a lookup to each glyph in the string before moving to the next lookup. A lookup is finished for a glyph after the client makes the substitution/positioning operation. To move to the “next” glyph, the client will typically skip all the glyphs that participated in the lookup operation: glyphs that were substituted/positioned as well as any other glyphs that formed a context for the operation. However, in the case of pair positioning operations (i.e., kerning), the “next” glyph in a sequence may be the second glyph of the positioned pair (see pair positioning lookup for details).

            //A Lookup table contains a LookupType, specified as an integer, that defines the type of information stored in the lookup. The LookupFlag specifies lookup qualifiers that assist a text-processing client in substituting or positioning glyphs. The SubTableCount specifies the total number of SubTables. The SubTable array specifies offsets, measured from the beginning of the Lookup table, to each SubTable enumerated in the SubTable array.
            //Lookup table
            //Type 	Name 	Description
            //USHORT 	LookupType 	Different enumerations for GSUB and GPOS
            //USHORT 	LookupFlag 	Lookup qualifiers
            //USHORT 	SubTableCount 	Number of SubTables for this lookup
            //Offset 	SubTable
            //[SubTableCount] 	Array of offsets to SubTables-from beginning of Lookup table
            //unit16 	MarkFilteringSet
            lookupRecords.Clear();
            ushort lookupCount = reader.ReadUInt16();
            int[] subTableOffset = new int[lookupCount];
            for (int i = 0; i < lookupCount; ++i)
            {
                subTableOffset[i] = reader.ReadUInt16();
            }
            //----------------------------------------------
            //load each sub table
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            for (int i = 0; i < lookupCount; ++i)
            {
                long lookupTablePos = lookupListHeadPos + subTableOffset[i];
                reader.BaseStream.Seek(lookupTablePos, SeekOrigin.Begin);

                ushort lookupType = reader.ReadUInt16();//Each Lookup table may contain only one type of information (LookupType)
                ushort lookupFlags = reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                //Each LookupType is defined with one or more subtables, and each subtable definition provides a different representation format
                //
                ushort[] subTableOffsets = new ushort[subTableCount];
                for (int m = 0; m < subTableCount; ++m)
                {
                    subTableOffsets[m] = reader.ReadUInt16();
                }

                ushort markFilteringSet =
                    ((lookupFlags & 0x0010) == 0x0010) ? reader.ReadUInt16() : (ushort)0;

                lookupRecords.Add(
                    new LookupTable(
                        lookupTablePos,
                        lookupType,
                        lookupFlags,
                        subTableCount,
                        subTableOffsets,//Array of offsets to SubTables-from beginning of Lookup table
                        markFilteringSet));
            }
            //----------------------------------------------
            //read each lookup record content ...
            for (int i = 0; i < lookupCount; ++i)
            {
                LookupTable lookupRecord = lookupRecords[i];
                //set origin
                reader.BaseStream.Seek(lookupListHeadPos + subTableOffset[i], SeekOrigin.Begin);
                lookupRecord.ReadRecordContent(reader);
            }

        }
        /// <summary>
        /// sub table of a lookup list
        /// </summary>
        class LookupTable
        {
            //--------------------------
            long lookupTablePos;
            //--------------------------
            public readonly ushort lookupType;
            public readonly ushort lookupFlags;
            public readonly ushort subTableCount;
            public readonly ushort[] subTableOffsets;
            public readonly ushort markFilteringSet;
            //--------------------------
            List<LookupSubTable> subTables = new List<LookupSubTable>();
            public LookupTable(
                long lookupTablePos,
                ushort lookupType,
                ushort lookupFlags,
                ushort subTableCount,
                ushort[] subTableOffsets,
                ushort markFilteringSet
                 )
            {
                this.lookupTablePos = lookupTablePos;
                this.lookupType = lookupType;
                this.lookupFlags = lookupFlags;
                this.subTableCount = subTableCount;
                this.subTableOffsets = subTableOffsets;
                this.markFilteringSet = markFilteringSet;
            }
#if DEBUG
            public override string ToString()
            {
                return lookupType.ToString();
            }
#endif
            public void ReadRecordContent(BinaryReader reader)
            {
                switch (lookupType)
                {
                    default: throw new NotSupportedException();
                    case 1:
                        ReadLookupType1(reader);
                        break;
                    case 2:
                        ReadLookupType2(reader);
                        break;
                    case 3:
                        ReadLookupType3(reader);
                        break;
                    case 4:
                        ReadLookupType4(reader);
                        break;
                    case 5:
                        ReadLookupType5(reader);
                        break;
                    case 6:
                        ReadLookupType6(reader);
                        break;
                    case 7:
                        ReadLookupType7(reader);
                        break;
                    case 8:
                        ReadLookupType8(reader);
                        break;
                    case 9:
                        ReadLookupType9(reader);
                        break;
                }
            }
            /// <summary>
            /// Lookup Type 1: Single Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType1(BinaryReader reader)
            {
                long thisLoookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;


                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    reader.BaseStream.Seek(lookupTablePos + subTableOffsets[i], SeekOrigin.Begin);

                    //-----------------------
                    LookupSubTable subTable = null;
                    ushort format = reader.ReadUInt16();
                    ushort coverage = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                //Single Adjustment Positioning: Format 1

                            } break;
                        case 2:
                            {
                                //Single Adjustment Positioning: Format 2

                            }
                            break;
                    }
                    //    subTable.CoverageTable = CoverageTable.ReadFrom(reader);
                    this.subTables.Add(subTable);
                }
            }
            /// <summary>
            ///  Lookup Type 2: Pair Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType2(BinaryReader reader)
            {

                Console.WriteLine("skip lookup2");

            }

            /// <summary>
            /// Lookup Type 3: Cursive Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType3(BinaryReader reader)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// Lookup Type 4: MarkToBase Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType4(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 4");
                //throw new NotImplementedException();
            }
            /// <summary>
            /// Lookup Type 5: MarkToLigature Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType5(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 4");
            }
            /// <summary>
            /// Lookup Type 6: MarkToMark Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType6(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 6");
                //throw new NotImplementedException();
            }
            /// <summary>
            /// Lookup Type 7: Contextual Positioning Subtables
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType7(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 7");
            }
            /// <summary>
            /// LookupType 8: Chaining Contextual Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType8(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 8");
            }
            /// <summary>
            /// LookupType 9: Extension Positioning
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType9(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 9");
            }
        }
    }

}