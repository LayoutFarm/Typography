//Apache2, 2016-2017, WinterDev, Sam Hocevar <sam@hocevar.net>
using System;
using System.Collections.Generic;
using System.IO;
namespace Typography.OpenFont.Tables
{

    //from https://www.microsoft.com/typography/otspec/otff.htm#otttables
    //Data Types

    //The following data types are used in the OpenType font file.All OpenType fonts use Motorola-style byte ordering (Big Endian):
    //Data      Type   Description
    //uint8     8-bit unsigned integer.
    //int8	    8-bit signed integer.
    //uint16	16-bit unsigned integer.
    //int16	    16-bit signed integer.
    //uint24	24-bit unsigned integer.
    //uint32	32-bit unsigned integer.
    //int32	    32-bit signed integer.
    //Fixed	    32-bit signed fixed-point number(16.16)
    //FWORD     int16 that describes a quantity in font design units.
    //UFWORD    uint16 that describes a quantity in font design units.
    //F2DOT14   16 - bit signed fixed number with the low 14 bits of fraction(2.14).
    //LONGDATETIME   Date represented in number of seconds since 12:00 midnight, January 1, 1904.The value is represented as a signed 64 - bit integer.
    //Tag Array of four uint8s(length = 32 bits) used to identify a script, language system, feature, or baseline
    //Offset16   Short offset to a table, same as uint16, NULL offset = 0x0000
    //Offset32   Long offset to a table, same as uint32, NULL offset = 0x00000000
    //------- 


    //https://www.microsoft.com/typography/otspec/GPOS.htm

    public partial class GPOS : TableEntry
    {
        long gposTableStartAt;
        ScriptList scriptList = new ScriptList();
        FeatureList featureList = new FeatureList();
        List<LookupTable> lookupRecords = new List<LookupTable>();
        public override string Name
        {
            get { return "GPOS"; }
        }
        public ScriptList ScriptList { get { return scriptList; } }
        public FeatureList FeatureList { get { return featureList; } }
        public LookupTable GetLookupTable(int index)
        {
            return lookupRecords[index];
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            gposTableStartAt = reader.BaseStream.Position;
            //-------------------------------------------
            // GPOS Header
            //The GPOS table begins with a header that contains a version number for the table. Two versions are defined. 
            //Version 1.0 contains offsets to three tables: ScriptList, FeatureList, and LookupList. 
            //Version 1.1 also includes an offset to a FeatureVariations table.
            //For descriptions of these tables, see the chapter, OpenType Layout Common Table Formats .
            //Example 1 at the end of this chapter shows a GPOS Header table definition.
            //GPOS Header, Version 1.0
            //Value 	Type 	Description
            //uint16 	MajorVersion 	Major version of the GPOS table, = 1
            //uint16 	MinorVersion 	Minor version of the GPOS table, = 0
            //Offset16 	ScriptList 	Offset to ScriptList table, from beginning of GPOS table
            //Offset16 	FeatureList 	Offset to FeatureList table, from beginning of GPOS table
            //Offset16 	LookupList 	Offset to LookupList table, from beginning of GPOS table

            //GPOS Header, Version 1.1
            //Value 	Type 	Description
            //uint16 	MajorVersion 	Major version of the GPOS table, = 1
            //uint16 	MinorVersion 	Minor version of the GPOS table, = 1
            //Offset16 	ScriptList 	Offset to ScriptList table, from beginning of GPOS table
            //Offset16 	FeatureList 	Offset to FeatureList table, from beginning of GPOS table
            //Offset16 	LookupList 	Offset to LookupList table, from beginning of GPOS table
            //Offset32 	FeatureVariations 	Offset to FeatureVariations table, from beginning of GPOS table (may be NULL) 

            this.MajorVersion = reader.ReadUInt16();
            this.MinorVersion = reader.ReadUInt16();

            ushort scriptListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort featureListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort lookupListOffset = reader.ReadUInt16();//from beginning of GSUB table
            uint featureVariations = (MinorVersion == 1) ? reader.ReadUInt32() : 0;//from beginning of GSUB table

            //-----------------------
            //1. scriptlist             
            scriptList = ScriptList.CreateFrom(reader, gposTableStartAt + scriptListOffset);
            //-----------------------
            //2. feature list             
            featureList = FeatureList.CreateFrom(reader, gposTableStartAt + featureListOffset);
            //-----------------------
            //3. lookup list

            ReadLookupListTable(reader, gposTableStartAt + lookupListOffset);
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
        void ReadLookupListTable(BinaryReader reader, long lookupListBeginAt)
        {

            reader.BaseStream.Seek(lookupListBeginAt, SeekOrigin.Begin);
            //

            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //-----------------------
            //LookupList table
            //-----------------------
            //Type 	    Name 	        Description
            //uint16 	LookupCount 	Number of lookups in this table
            //Offset16 	Lookup[LookupCount] 	Array of offsets to Lookup tables-from beginning of LookupList -zero based (first lookup is Lookup index = 0)
            //-----------------------
            //
            //
            //Lookup Table
            //A Lookup table (Lookup) defines the specific conditions, type, 
            //and results of a substitution or positioning action that is used to implement a feature. 
            //For example, a substitution operation requires a list of target glyph indices to be replaced, 
            //a list of replacement glyph indices, and a description of the type of substitution action.
            //Each Lookup table may contain only one type of information (LookupType),
            //determined by whether the lookup is part of a GSUB or GPOS table. GSUB supports eight LookupTypes, 
            //and GPOS supports nine LookupTypes (for details about LookupTypes, see the GSUB and GPOS chapters of the document).

            //Each LookupType is defined with one or more subtables, 
            //and each subtable definition provides a different representation format.
            //The format is determined by the content of the information required for an operation and by required storage efficiency.
            //When glyph information is best presented in more than one format,
            //a single lookup may contain more than one subtable, as long as all the subtables are the same LookupType. 
            //For example, within a given lookup, a glyph index array format may best represent one set of target glyphs,
            //whereas a glyph index range format may be better for another set of target glyphs.

            //During text processing, a client applies a lookup to each glyph in the string before moving to the next lookup. 
            //A lookup is finished for a glyph after the client makes the substitution/positioning operation.
            //To move to the “next” glyph, the client will typically skip all the glyphs that participated in the lookup operation: glyphs 
            //that were substituted/positioned as well as any other glyphs that formed a context for the operation. However, in the case of pair positioning operations (i.e., kerning), the “next” glyph in a sequence may be the second glyph of the positioned pair (see pair positioning lookup for details).

            //A Lookup table contains a LookupType, specified as an integer, that defines the type of information stored in the lookup.
            //The LookupFlag specifies lookup qualifiers that assist a text-processing client in substituting or positioning glyphs.
            //The SubTableCount specifies the total number of SubTables. 
            //The SubTable array specifies offsets, measured from the beginning of the Lookup table, to each SubTable enumerated in the SubTable array.
            //
            //Lookup table
            //--------------------------------
            //Type  	Name 	        Description
            //unit16 	LookupType  	Different enumerations for GSUB and GPOS
            //unit16 	LookupFlag 	    Lookup qualifiers
            //unit16 	SubTableCount 	Number of SubTables for this lookup
            //Offset16 	SubTable[SubTableCount] 	Array of offsets to SubTables-from beginning of Lookup table
            //unit16 	MarkFilteringSet   Index (base 0) into GDEF mark glyph sets structure. 
            //                             *** This field is only present if bit UseMarkFilteringSet of lookup flags is set.
            //--------------------------------
            lookupRecords.Clear();
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupTableOffsets = Utils.ReadUInt16Array(reader, lookupCount);

            //----------------------------------------------
            //load each sub table
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            for (int i = 0; i < lookupCount; ++i)
            {
                long lookupTablePos = lookupListBeginAt + lookupTableOffsets[i];
                reader.BaseStream.Seek(lookupTablePos, SeekOrigin.Begin);

                ushort lookupType = reader.ReadUInt16();//Each Lookup table may contain only one type of information (LookupType)
                ushort lookupFlags = reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                //Each LookupType is defined with one or more subtables, and each subtable definition provides a different representation format
                //
                ushort[] subTableOffsets = Utils.ReadUInt16Array(reader, subTableCount);

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
                LookupTable lookupTable = lookupRecords[i];
                //set origin
                reader.BaseStream.Seek(lookupListBeginAt + lookupTableOffsets[i], SeekOrigin.Begin);
                lookupTable.ReadRecordContent(reader);
                foreach (var subT in lookupTable.SubTables)
                {
                    subT.OwnerGPos = this;
                }
            }
        }

        public abstract class LookupSubTable
        {
            public GPOS OwnerGPos;

            public abstract void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len);
        }

        /// <summary>
        /// sub table of a lookup list
        /// </summary>
        public class LookupTable
        {
            //Lookup table
            //--------------------------------
            //Type  	Name 	        Description
            //unit16 	LookupType  	Different enumerations for GSUB and GPOS
            //unit16 	LookupFlag 	    Lookup qualifiers
            //unit16 	SubTableCount 	Number of SubTables for this lookup
            //Offset16 	SubTable[SubTableCount] 	Array of offsets to SubTables-from beginning of Lookup table
            //unit16 	MarkFilteringSet   Index (base 0) into GDEF mark glyph sets structure. 
            //                             *** This field is only present if bit UseMarkFilteringSet of lookup flags is set.
            //--------------------------------

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
            public void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {

                int j = subTables.Count;
                for (int i = 0; i < j; ++i)
                {

                    subTables[i].DoGlyphPosition(inputGlyphs, startAt, len);
                    //update len
                    len = inputGlyphs.Count;
                }
            }
            public List<LookupSubTable> SubTables { get { return subTables; } }

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
                    default:
                        Utils.WarnUnimplemented("Lookup Type {0}", lookupType);
                        break;
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

            class LkSubTableType1 : LookupSubTable
            {
                ValueRecord singleValue;
                ValueRecord[] multiValues;
                public LkSubTableType1(ValueRecord singleValue)
                {
                    this.Format = 1;
                    this.singleValue = singleValue;
                }
                public LkSubTableType1(ValueRecord[] valueRecords)
                {
                    this.Format = 2;
                    this.multiValues = valueRecords;
                }
                public int Format
                {
                    get;
                    private set;
                }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 1");
                }
            }
            /// <summary>
            /// Lookup Type 1: Single Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType1(BinaryReader reader)
            {
                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = reader.BaseStream.Position + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                //Single Adjustment Positioning: Format 1
                                //Value 	Type 	    Description                                
                                //uint16 	PosFormat 	Format identifier-format = 1
                                //Offset16 	Coverage 	Offset to Coverage table-from beginning of SinglePos subtable
                                //uint16 	ValueFormat 	Defines the types of data in the ValueRecord
                                //ValueRecord 	Value 	Defines positioning value(s)-applied to all glyphs in the Coverage table 
                                ushort coverage = reader.ReadUInt16();
                                ushort valueFormat = reader.ReadUInt16();
                                var subTable = new LkSubTableType1(ValueRecord.CreateFrom(reader, valueFormat));
                                //-------                                 
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                //-------
                                this.subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                //Single Adjustment Positioning: Format 2
                                //Value 	    Type 	        Description
                                //USHORT 	    PosFormat 	    Format identifier-format = 2
                                //Offset16 	    Coverage 	    Offset to Coverage table-from beginning of SinglePos subtable
                                //uint16 	    ValueFormat 	Defines the types of data in the ValueRecord
                                //uint16 	    ValueCount 	    Number of ValueRecords
                                //ValueRecord 	Value[ValueCount] 	Array of ValueRecords-positioning values applied to glyphs
                                ushort coverage = reader.ReadUInt16();
                                ushort valueFormat = reader.ReadUInt16();
                                ushort valueCount = reader.ReadUInt16();
                                var values = new ValueRecord[valueCount];
                                for (int n = 0; n < valueCount; ++n)
                                {
                                    values[n] = ValueRecord.CreateFrom(reader, valueFormat);
                                }
                                var subTable = new LkSubTableType1(values);
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                //-------
                                this.subTables.Add(subTable);
                            }
                            break;
                    }
                }
            }

            /// <summary>
            /// Lookup Type 2, Format1: Pair Adjustment Positioning Subtable
            /// </summary>
            class LkSubTableType2Fmt1 : LookupSubTable
            {
                PairSetTable[] pairSetTables;
                public LkSubTableType2Fmt1(PairSetTable[] pairSetTables)
                {
                    this.pairSetTables = pairSetTables;
                }
                public CoverageTable CoverageTable
                {
                    get;
                    set;
                }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    //find marker   
                    CoverageTable covTable = this.CoverageTable;
                    int lim = inputGlyphs.Count - 1;
                    for (int i = 0; i < lim; ++i) //start at 0
                    {
                        ushort glyph_advW;
                        int firstGlyphFound = covTable.FindPosition(inputGlyphs.GetGlyph(i, out glyph_advW));
                        if (firstGlyphFound > -1)
                        {
                            //test this with Palatino A-Y sequence
                            PairSetTable pairSet = this.pairSetTables[firstGlyphFound];
                            //check second glyph 
                            ushort second_glyph_w;
                            ushort second_glyph_index = inputGlyphs.GetGlyph(i + 1, out second_glyph_w);
                            PairSet foundPairSet;
                            if (pairSet.FindPairSet(second_glyph_index, out foundPairSet))
                            {
                                ValueRecord v1 = foundPairSet.value1;
                                ValueRecord v2 = foundPairSet.value2;
                                //TODO: recheck for vertical writing ...
                                inputGlyphs.AppendGlyphAdvance(i, v1.XAdvance, 0);
                                inputGlyphs.AppendGlyphAdvance(i + 1, v2.XAdvance, 0);
                            }
                        }
                    }

                }
            }
            /// <summary>
            ///  Lookup Type 2: Pair Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType2(BinaryReader reader)
            {
                //A pair adjustment positioning subtable(PairPos) is used to adjust the positions of two glyphs
                //in relation to one another-for instance, 
                //to specify kerning data for pairs of glyphs.
                //
                //Compared to a typical kerning table, however, a PairPos subtable offers more flexiblity and 
                //precise control over glyph positioning.

                //The PairPos subtable can adjust each glyph in a pair independently in both the X and Y directions, 
                //and it can explicitly describe the particular type of adjustment applied to each glyph.
                //
                //PairPos subtables can be either of two formats: 
                //1) one that identifies glyphs individually by index(Format 1),
                //or 2) one that identifies glyphs by class (Format 2).
                //-----------------------------------------------
                //FORMAT1:
                //Format 1 uses glyph indices to access positioning data for one or more specific pairs of glyphs
                //All pairs are specified in the order determined by the layout direction of the text.
                //
                //Note: For text written from right to left, the right - most glyph will be the first glyph in a pair;
                //conversely, for text written from left to right, the left - most glyph will be first.
                //
                //A PairPosFormat1 subtable contains a format identifier(PosFormat) and two ValueFormats:
                //ValueFormat1 applies to the ValueRecord of the first glyph in each pair.
                //ValueRecords for all first glyphs must use ValueFormat1.
                //If ValueFormat1 is set to zero(0), 
                //the corresponding glyph has no ValueRecord and, therefore, should not be repositioned.
                //
                //ValueFormat2 applies to the ValueRecord of the second glyph in each pair.
                //ValueRecords for all second glyphs must use ValueFormat2.
                //If ValueFormat2 is set to null, then the second glyph of the pair is the “next” glyph for which a lookup should be performed.
                //
                //A PairPos subtable also defines an offset to a Coverage table(Coverage) that lists the indices of the first glyphs in each pair.
                //More than one pair can have the same first glyph, but the Coverage table will list that glyph only once.
                //
                //The subtable also contains an array of offsets to PairSet tables(PairSet) and a count of the defined tables(PairSetCount).
                //The PairSet array contains one offset for each glyph listed in the Coverage table and uses the same order as the Coverage Index.

                //-----------------
                //PairPosFormat1 subtable: Adjustments for glyph pairs
                //uint16 	PosFormat 	    Format identifier-format = 1
                //Offset16 	Coverage 	    Offset to Coverage table-from beginning of PairPos subtable-only the first glyph in each pair
                //uint16 	ValueFormat1 	Defines the types of data in ValueRecord1-for the first glyph in the pair -may be zero (0)
                //uint16 	ValueFormat2 	Defines the types of data in ValueRecord2-for the second glyph in the pair -may be zero (0)
                //uint16 	PairSetCount 	Number of PairSet tables
                //Offset16 	PairSetOffset[PairSetCount] Array of offsets to PairSet tables-from beginning of PairPos subtable-ordered by Coverage Index                // 	
                //-----------------
                //
                //PairSet table
                //Value 	Type 	            Description
                //uint16 	PairValueCount 	    Number of PairValueRecords
                //struct 	PairValueRecord[PairValueCount] 	Array of PairValueRecords-ordered by GlyphID of the second glyph
                //-----------------
                //A PairValueRecord specifies the second glyph in a pair (SecondGlyph) and defines a ValueRecord for each glyph (Value1 and Value2). 
                //If ValueFormat1 is set to zero (0) in the PairPos subtable, ValueRecord1 will be empty; similarly, if ValueFormat2 is 0, Value2 will be empty.

                //Example 4 at the end of this chapter shows a PairPosFormat1 subtable that defines two cases of pair kerning.
                //PairValueRecord
                //Value 	    Type 	        Description
                //GlyphID 	    SecondGlyph 	GlyphID of second glyph in the pair-first glyph is listed in the Coverage table
                //ValueRecord 	Value1 	        Positioning data for the first glyph in the pair
                //ValueRecord 	Value2 	        Positioning data for the second glyph in the pair
                //-----------------------------------------------

                //PairPosFormat2 subtable: Class pair adjustment
                //Value 	Type 	            Description
                //uint16 	PosFormat 	        Format identifier-format = 2
                //Offset16 	Coverage 	        Offset to Coverage table-from beginning of PairPos subtable-for the first glyph of the pair
                //uint16 	ValueFormat1 	    ValueRecord definition-for the first glyph of the pair-may be zero (0)
                //uint16 	ValueFormat2 	    ValueRecord definition-for the second glyph of the pair-may be zero (0)
                //Offset16 	ClassDef1 	        Offset to ClassDef table-from beginning of PairPos subtable-for the first glyph of the pair
                //Offset16 	ClassDef2 	        Offset to ClassDef table-from beginning of PairPos subtable-for the second glyph of the pair
                //uint16 	Class1Count 	    Number of classes in ClassDef1 table-includes Class0
                //uint16 	Class2Count 	    Number of classes in ClassDef2 table-includes Class0
                //struct 	Class1Record[Class1Count] 	Array of Class1 records-ordered by Class1

                //Each Class1Record contains an array of Class2Records (Class2Record), which also are ordered by class value. 
                //One Class2Record must be declared for each class in the ClassDef2 table, including Class 0.
                //--------------------------------
                //Class1Record
                //Value 	Type 	Description
                //struct 	Class2Record[Class2Count] 	Array of Class2 records-ordered by Class2
                //--------------------------------

                //A Class2Record consists of two ValueRecords,
                //one for the first glyph in a class pair (Value1) and one for the second glyph (Value2).
                //If the PairPos subtable has a value of zero (0) for ValueFormat1 or ValueFormat2, 
                //the corresponding record (ValueRecord1 or ValueRecord2) will be empty.

                //Example 5 at the end of this chapter demonstrates pair kerning with glyph classes in a PairPosFormat2 subtable.
                //Class2Record
                //--------------------------------
                //Value 	    Type 	Description
                //ValueRecord 	Value1 	Positioning for first glyph-empty if ValueFormat1 = 0
                //ValueRecord 	Value2 	Positioning for second glyph-empty if ValueFormat2 = 0
                //--------------------------------
                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                    //----------------------- 
                    ushort format = reader.ReadUInt16();

                    switch (format)
                    {
                        default:
                            Utils.WarnUnimplemented("Pair Adjustment Positioning Subtable Format {0}", format);
                            break;
                        case 1:
                            {
                                ushort coverage = reader.ReadUInt16();
                                ushort value1Format = reader.ReadUInt16();
                                ushort value2Format = reader.ReadUInt16();
                                ushort pairSetCount = reader.ReadUInt16();
                                ushort[] pairSetOffsetArray = Utils.ReadUInt16Array(reader, pairSetCount);
                                PairSetTable[] pairSetTables = new PairSetTable[pairSetCount];
                                for (int n = 0; n < pairSetCount; ++n)
                                {
                                    reader.BaseStream.Seek(subTableStartAt + pairSetOffsetArray[n], SeekOrigin.Begin);
                                    var pairSetTable = new PairSetTable();
                                    pairSetTable.ReadFrom(reader, value1Format, value2Format);
                                    pairSetTables[n] = pairSetTable;
                                }
                                var subTable = new LkSubTableType2Fmt1(pairSetTables);
                                //coverage        
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                //.... TODO: implement this
                                ushort coverage = reader.ReadUInt16();
                                ushort value1Format = reader.ReadUInt16();
                                ushort value2Format = reader.ReadUInt16();
                                ushort classDef1_offset = reader.ReadUInt16();
                                ushort classDef2_offset = reader.ReadUInt16();
                                ushort class1Count = reader.ReadUInt16();
                                ushort class2Count = reader.ReadUInt16();

                                for (int c1 = 0; c1 < class1Count; ++c1)
                                {
                                    //for each c1 record
                                    for (int c2 = 0; c2 < class2Count; ++c2)
                                    {

                                    }

                                }
                                Utils.WarnUnimplemented("Pair Adjustment Positioning Subtable Format 2");
                            }
                            break;
                    }
                }
            }

            /// <summary>
            /// Lookup Type 3: Cursive Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType3(BinaryReader reader)
            {
                //TODO: implement this
                Utils.WarnUnimplemented("Lookup Table Type 3");
            }
            //-------------------------------------------------------------------------
            /// <summary>
            /// Lookup Type 4:MarkToBase Attachment Positioning, or called (MarkBasePos) table
            /// </summary>
            class LkSubTableType4 : LookupSubTable
            {
                public LkSubTableType4()
                {
                }
                public CoverageTable MarkCoverageTable { get; set; }
                public CoverageTable BaseCoverageTable { get; set; }
                public BaseArrayTable BaseArrayTable { get; set; }
                public MarkArrayTable MarkArrayTable { get; set; }

                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    int xpos = 0;
                    //find marker  

                    int j = inputGlyphs.Count;
                    for (int i = 1; i < j; ++i) //start at 1
                    {

                        ushort glyph_advW;
                        int markFound = MarkCoverageTable.FindPosition(inputGlyphs.GetGlyph(i, out glyph_advW));
                        if (markFound > -1)
                        {
                            //this is mark glyph
                            //then-> look back for base
                            ushort prev_glyph_adv_w;
                            int baseFound = BaseCoverageTable.FindPosition(inputGlyphs.GetGlyph(i - 1, out prev_glyph_adv_w));
                            if (baseFound > -1)
                            {
                                ushort markClass = this.MarkArrayTable.GetMarkClass(markFound);
                                //find anchor on base glyph   
                                AnchorPoint markAnchorPoint = this.MarkArrayTable.GetAnchorPoint(markFound);
                                BaseRecord baseRecord = BaseArrayTable.GetBaseRecords(baseFound);
                                AnchorPoint basePointForMark = baseRecord.anchors[markClass];
                                inputGlyphs.AppendGlyphOffset(
                                    i,
                                    (short)((-prev_glyph_adv_w + basePointForMark.xcoord - markAnchorPoint.xcoord)),
                                    (short)(basePointForMark.ycoord - markAnchorPoint.ycoord)
                                    );
                            }
                        }
                        xpos += glyph_advW;
                    }
                }

#if DEBUG
                public void dbugTest()
                {
                    //count base covate
                    ushort[] expandedMarks = MarkCoverageTable.dbugGetExpandedGlyphs();
                    if (expandedMarks.Length != MarkArrayTable.dbugGetAnchorCount())
                    {
                        throw new NotSupportedException();
                    }
                    //--------------------------
                    ushort[] expandedBase = BaseCoverageTable.dbugGetExpandedGlyphs();
                    if (expandedBase.Length != BaseArrayTable.dbugGetRecordCount())
                    {
                        throw new NotSupportedException();
                    }

                }
#endif
            }
            /// <summary>
            /// Lookup Type 4: MarkToBase Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType4(BinaryReader reader)
            {
                //The MarkToBase attachment (MarkBasePos) subtable is used to position combining mark glyphs with respect to base glyphs. 
                //For example, the Arabic, Hebrew, and Thai scripts combine vowels, diacritical marks, and tone marks with base glyphs.

                //In the MarkBasePos subtable, every mark glyph has an anchor point and is associated with a class of marks. 
                //Each base glyph then defines an anchor point for each class of marks it uses.

                //For example, assume two mark classes: all marks positioned above base glyphs (Class 0),
                //and all marks positioned below base glyphs (Class 1). 
                //In this case, each base glyph that uses these marks would define two anchor points, 
                //one for attaching the mark glyphs listed in Class 0,
                //and one for attaching the mark glyphs listed in Class 1.

                //To identify the base glyph that combines with a mark,
                //the text-processing client must look backward in the glyph string from the mark to the preceding base glyph.
                //To combine the mark and base glyph, the client aligns their attachment points,
                //positioning the mark with respect to the final pen point (advance) position of the base glyph.

                //The MarkToBase Attachment subtable has one format: MarkBasePosFormat1. 
                //The subtable begins with a format identifier (PosFormat) and
                //offsets to two Coverage tables: one that lists all the mark glyphs referenced in the subtable (MarkCoverage), 
                //and one that lists all the base glyphs referenced in the subtable (BaseCoverage).

                //For each mark glyph in the MarkCoverage table,
                //a record specifies its class and an offset to the Anchor table that describes the mark's attachment point (MarkRecord).
                //A mark class is identified by a specific integer, called a class value.
                //ClassCount specifies the total number of distinct mark classes defined in all the MarkRecords.

                //The MarkBasePosFormat1 subtable also contains an offset to a MarkArray table, 
                //which contains all the MarkRecords stored in an array (MarkRecord) by MarkCoverage Index. 
                //A MarkArray table also contains a count of the defined MarkRecords (MarkCount). 
                //(For details about MarkArrays and MarkRecords, see the end of this chapter.)

                //The MarkBasePosFormat1 subtable also contains an offset to a BaseArray table (BaseArray).

                //MarkBasePosFormat1 subtable: MarkToBase attachment point
                //----------------------------------------------
                //Value 	Type 	Description
                //uint16 	PosFormat 	Format identifier-format = 1
                //Offset16 	MarkCoverage 	Offset to MarkCoverage table-from beginning of MarkBasePos subtable ( all the mark glyphs referenced in the subtable)
                //Offset16 	BaseCoverage 	Offset to BaseCoverage table-from beginning of MarkBasePos subtable (all the base glyphs referenced in the subtable)
                //uint16 	ClassCount 	Number of classes defined for marks
                //Offset16 	MarkArray 	Offset to MarkArray table-from beginning of MarkBasePos subtable
                //Offset16 	BaseArray 	Offset to BaseArray table-from beginning of MarkBasePos subtable
                //----------------------------------------------

                //The BaseArray table consists of an array (BaseRecord) and count (BaseCount) of BaseRecords. 
                //The array stores the BaseRecords in the same order as the BaseCoverage Index. 
                //Each base glyph in the BaseCoverage table has a BaseRecord.

                //BaseArray table
                //Value 	Type 	Description
                //uint16 	BaseCount 	Number of BaseRecords
                //struct 	BaseRecord[BaseCount] 	Array of BaseRecords-in order of BaseCoverage Index
                long thisSubTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subtableStart = thisSubTablePos + subTableOffsets[i]; //beginning of MarkBasePos subtable ***
                    reader.BaseStream.Seek(subtableStart, SeekOrigin.Begin);

                    //----------------------- 
                    ushort format = reader.ReadUInt16();
                    if (format != 1)
                    {
                        throw new NotSupportedException();
                    }
                    ushort markCoverageOffset = reader.ReadUInt16(); //offset from 
                    ushort baseCoverageOffset = reader.ReadUInt16();
                    ushort mark_classCount = reader.ReadUInt16();
                    ushort markArrayOffset = reader.ReadUInt16();
                    ushort baseArrayOffset = reader.ReadUInt16();

                    //---------------------------------------------------------------------------

                    //read mark array table
                    var lookupType4 = new LkSubTableType4();
                    //---------------------------------------------------------------------------                     
                    lookupType4.MarkCoverageTable = CoverageTable.CreateFrom(reader, subtableStart + markCoverageOffset);
                    //---------------------------------------------------------------------------                    
                    lookupType4.BaseCoverageTable = CoverageTable.CreateFrom(reader, subtableStart + baseCoverageOffset);
                    //---------------------------------------------------------------------------                     
                    lookupType4.MarkArrayTable = MarkArrayTable.CreateFrom(reader, subtableStart + markArrayOffset);
                    //---------------------------------------------------------------------------                     
                    lookupType4.BaseArrayTable = BaseArrayTable.CreateFrom(reader, subtableStart + baseArrayOffset, mark_classCount);
                    //---------------------------------------------------------------------------
#if DEBUG
                    lookupType4.dbugTest();
#endif
                    this.subTables.Add(lookupType4);
                }
            }


            class LkSubTableType5 : LookupSubTable
            {
                public CoverageTable MarkCoverage { get; set; }
                public CoverageTable LigatureCoverage { get; set; }
                public MarkArrayTable MarkArrayTable { get; set; }
                public LigatureArrayTable LigatureArrayTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 5");
                }
            }
            /// <summary>
            /// Lookup Type 5: MarkToLigature Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType5(BinaryReader reader)
            {
                //uint16 	PosFormat 	Format identifier-format = 1
                //Offset16 	MarkCoverage 	Offset to Mark Coverage table-from beginning of MarkLigPos subtable
                //Offset16 	LigatureCoverage 	Offset to Ligature Coverage table-from beginning of MarkLigPos subtable
                //uint16 	ClassCount 	Number of defined mark classes
                //Offset16 	MarkArray 	Offset to MarkArray table-from beginning of MarkLigPos subtable
                //Offset16 	LigatureArray 	Offset to LigatureArray table-from beginning of MarkLigPos subtable

                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {

                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    if (format != 1)
                    {
                        Utils.WarnUnimplemented("Lookup Sub Table Type 5 Format {0}", format);
                        return;
                    }
                    ushort markCoverageOffset = reader.ReadUInt16(); //from beginning of MarkLigPos subtable
                    ushort ligatureCoverageOffset = reader.ReadUInt16();
                    ushort classCount = reader.ReadUInt16();
                    ushort markArrayOffset = reader.ReadUInt16();
                    ushort ligatureArrayOffset = reader.ReadUInt16();
                    //-----------------------
                    var subTable = new LkSubTableType5();
                    //-----------------------
                    subTable.MarkCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + markCoverageOffset);
                    //-----------------------
                    subTable.LigatureCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + ligatureCoverageOffset);
                    //-----------------------                     
                    subTable.MarkArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + markArrayOffset);
                    //-----------------------
                    reader.BaseStream.Seek(subTableStartAt + ligatureArrayOffset, SeekOrigin.Begin);
                    var ligatureArrayTable = new LigatureArrayTable();
                    ligatureArrayTable.ReadFrom(reader, classCount);
                    subTable.LigatureArrayTable = ligatureArrayTable;
                    //-----------------------
                    this.subTables.Add(subTable);
                }
            }


            //-----------------------------------------------------------------
            /// <summary>
            /// Lookup Type 6: MarkToMark Attachment
            /// </summary>
            class LkSubTableType6 : LookupSubTable
            {
                public CoverageTable MarkCoverage1 { get; set; }
                public CoverageTable MarkCoverage2 { get; set; }
                public MarkArrayTable Mark1ArrayTable { get; set; }
                public Mark2ArrayTable Mark2ArrayTable { get; set; } // Mark2 attachment points used to attach Mark1 glyphs to a specific Mark2 glyph. 
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    //find marker 
                    if (startAt == 0)
                    {
                        startAt++;
                    }
                    int lim = startAt + len;
                    if (lim > inputGlyphs.Count)
                    {
                        lim = inputGlyphs.Count;
                    }
                    //
                    for (int i = startAt; i < lim; ++i) //start at 1
                    {
                        ushort glyph_adv_w;
                        int markFound = MarkCoverage1.FindPosition(inputGlyphs.GetGlyph(i, out glyph_adv_w));
                        if (markFound > -1)
                        {
                            //this is mark glyph
                            //then-> look back for base 
                            ushort prev_pos_adv_w;
                            int baseFound = MarkCoverage2.FindPosition(inputGlyphs.GetGlyph(i - 1, out prev_pos_adv_w));
                            if (baseFound > -1)
                            {
                                int markClassId = this.Mark1ArrayTable.GetMarkClass(markFound);
                                AnchorPoint mark2BaseAnchor = this.Mark2ArrayTable.GetAnchorPoint(baseFound, markClassId);
                                AnchorPoint mark1Anchor = this.Mark1ArrayTable.GetAnchorPoint(markFound);

                                //TODO: review here 
                                if (mark1Anchor.ycoord < 0)
                                {
                                    //eg. น้ำ
                                    //change yoffset of prev pos 
                                    inputGlyphs.AppendGlyphOffset(i - 1 /*PREV*/, 0, (short)(-mark1Anchor.ycoord));
                                    int actualBasePos = FindActualBaseGlyphBackward(inputGlyphs, i - 1);
                                    if (actualBasePos > -1)
                                    {
                                        short actual_base_offset_x, acutal_base_offset_y;
                                        inputGlyphs.GetOffset(actualBasePos, out actual_base_offset_x, out acutal_base_offset_y);
                                        inputGlyphs.AppendGlyphOffset(
                                            i,
                                            (short)(actual_base_offset_x + mark2BaseAnchor.xcoord - mark1Anchor.xcoord),
                                            0);
                                    }
                                }
                                else
                                {
                                    short offset_x, offset_y;
                                    inputGlyphs.GetOffset(i - 1/*PREV*/, out offset_x, out offset_y);
                                    inputGlyphs.AppendGlyphOffset(
                                         i,
                                         (short)(offset_x + mark2BaseAnchor.xcoord - mark1Anchor.xcoord),
                                         mark1Anchor.ycoord);
                                }
                            }
                        }
                    }
                }
            }
            static int FindActualBaseGlyphBackward(IGlyphPositions inputGlyphs, int startAt)
            {
                for (int i = startAt; i >= 0; --i)
                {
                    if (inputGlyphs.GetGlyphClassKind(i) <= GlyphClassKind.Base)
                    {
                        return i;
                    }
                }
                return -1;
            }
            /// <summary>
            /// Lookup Type 6: MarkToMark Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType6(BinaryReader reader)
            {

                //uint16 	PosFormat 	Format identifier-format = 1
                //Offset16 	Mark1Coverage 	Offset to Combining Mark Coverage table-from beginning of MarkMarkPos subtable
                //Offset16 	Mark2Coverage 	Offset to Base Mark Coverage table-from beginning of MarkMarkPos subtable
                //uint16 	ClassCount 	Number of Combining Mark classes defined
                //Offset16 	Mark1Array 	Offset to MarkArray table for Mark1-from beginning of MarkMarkPos subtable
                //Offset16 	Mark2Array 	Offset to Mark2Array table for Mark2-from beginning of MarkMarkPos subtable

                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    if (format != 1)
                    {
                        Utils.WarnUnimplemented("Lookup Sub Table Type 6 Format {0}", format);
                        return;
                    }
                    ushort mark1CoverageOffset = reader.ReadUInt16();
                    ushort mark2CoverageOffset = reader.ReadUInt16();
                    ushort classCount = reader.ReadUInt16();
                    ushort mark1ArrayOffset = reader.ReadUInt16();
                    ushort mark2ArrayOffset = reader.ReadUInt16();
                    //
                    var subTable = new LkSubTableType6();
                    subTable.MarkCoverage1 = CoverageTable.CreateFrom(reader, subTableStartAt + mark1CoverageOffset);
                    subTable.MarkCoverage2 = CoverageTable.CreateFrom(reader, subTableStartAt + mark2CoverageOffset);
                    subTable.Mark1ArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + mark1ArrayOffset);
                    subTable.Mark2ArrayTable = Mark2ArrayTable.CreateFrom(reader, subTableStartAt + mark2ArrayOffset, classCount);


                    this.subTables.Add(subTable);
                }

            }
            /// <summary>
            /// Lookup Type 7: Contextual Positioning Subtables
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType7(BinaryReader reader)
            {

                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default:
                            Utils.WarnUnimplemented("Lookup Sub Table Type 7 Format {0}", format);
                            return;
                        case 1:
                            {
                                //Context Positioning Subtable: Format 1
                                //ContextPosFormat1 subtable: Simple context positioning
                                //Value 	Type 	Description
                                //uint16 	PosFormat 	Format identifier-format = 1
                                //Offset16 	Coverage 	Offset to Coverage table-from beginning of ContextPos subtable
                                //uint16 	PosRuleSetCount 	Number of PosRuleSet tables
                                //Offset16 	PosRuleSet[PosRuleSetCount]
                                //
                                ushort coverageOffset = reader.ReadUInt16();
                                ushort posRuleSetCount = reader.ReadUInt16();
                                ushort[] posRuleSetOffsets = Utils.ReadUInt16Array(reader, posRuleSetCount);

                                LkSubTableType7Fmt1 subTable = new LkSubTableType7Fmt1();
                                subTable.PosRuleSetTables = CreateMultiplePosRuleSetTables(subTableStartAt, posRuleSetOffsets, reader);
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);

                                //-----------
                                subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                //Context Positioning Subtable: Format 2
                                //uint16 	PosFormat 	Format identifier-format = 2
                                //Offset16 	Coverage 	Offset to Coverage table-from beginning of ContextPos subtable
                                //Offset16 	ClassDef 	Offset to ClassDef table-from beginning of ContextPos subtable
                                //uint16 	PosClassSetCnt 	Number of PosClassSet tables
                                //Offset16 	PosClassSet[PosClassSetCnt] 	Array of offsets to PosClassSet tables-from beginning of ContextPos subtable-ordered by class-may be NULL

                                ushort coverageOffset = reader.ReadUInt16();
                                ushort classDefOffset = reader.ReadUInt16();
                                ushort posClassSetCount = reader.ReadUInt16();
                                ushort[] posClassSetOffsets = Utils.ReadUInt16Array(reader, posClassSetCount);

                                var subTable = new LkSubTableType7Fmt2();
                                subTable.ClassDefOffset = classDefOffset;
                                //---------- 
                                PosClassSetTable[] posClassSetTables = new PosClassSetTable[posClassSetCount];
                                subTable.PosClassSetTables = posClassSetTables;
                                for (int n = 0; n < posClassSetCount; ++n)
                                {
                                    posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, coverageOffset);
                                }
                                //----------                                  
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                                //---------- 
                                subTables.Add(subTable);
                                //----------
                            }
                            break;
                        case 3:
                            {
                                //ContextPosFormat3 subtable: Coverage-based context glyph positioning
                                //Value 	Type 	Description
                                //uint16 	PosFormat 	Format identifier-format = 3
                                //uint16 	GlyphCount 	Number of glyphs in the input sequence
                                //uint16 	PosCount 	Number of PosLookupRecords
                                //Offset16 	Coverage[GlyphCount] 	Array of offsets to Coverage tables-from beginning of ContextPos subtable
                                //struct 	PosLookupRecord[PosCount] Array of positioning lookups-in design order
                                var subTable = new LkSubTableType7Fmt3();
                                ushort glyphCount = reader.ReadUInt16();
                                ushort posCount = reader.ReadUInt16();
                                //read each lookahead record
                                ushort[] coverageOffsets = Utils.ReadUInt16Array(reader, glyphCount);
                                subTable.PosLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);
                                subTable.CoverageTables = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, coverageOffsets, reader);
                                //---------- 
                                subTables.Add(subTable);
                                //----------
                            }
                            break;
                    }
                }
            }



            class LkSubTableType7Fmt1 : LookupSubTable
            {

                public CoverageTable CoverageTable { get; set; }
                public PosRuleSetTable[] PosRuleSetTables { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 7 Format 1");
                }
            }

            class LkSubTableType7Fmt2 : LookupSubTable
            {
                public ushort ClassDefOffset { get; set; }
                public CoverageTable CoverageTable { get; set; }
                public PosClassSetTable[] PosClassSetTables { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 7 Format 2");
                }

            }
            class LkSubTableType7Fmt3 : LookupSubTable
            {
                public CoverageTable[] CoverageTables { get; set; }
                public PosLookupRecord[] PosLookupRecords { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 7 Format 3");
                }
            }
            //----------------------------------------------------------------
            class LkSubTableType8Fmt1 : LookupSubTable
            {

                public CoverageTable CoverageTable { get; set; }
                public PosRuleSetTable[] PosRuleSetTables { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 8 Format 1");
                }
            }

            class LkSubTableType8Fmt2 : LookupSubTable
            {
                ushort[] chainPosClassSetOffsetArray;
                public LkSubTableType8Fmt2(ushort[] chainPosClassSetOffsetArray)
                {
                    this.chainPosClassSetOffsetArray = chainPosClassSetOffsetArray;
                }
                public CoverageTable CoverageTable { get; set; }
                public PosClassSetTable[] PosClassSetTables { get; set; }

                public ushort BacktrackClassDefOffset { get; set; }
                public ushort InputClassDefOffset { get; set; }
                public ushort LookaheadClassDefOffset { get; set; }


                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 8 Format 2");
                }
            }
            class LkSubTableType8Fmt3 : LookupSubTable
            {
                public CoverageTable[] BacktrackCoverages { get; set; }
                public CoverageTable[] InputGlyphCoverages { get; set; }
                public CoverageTable[] LookaheadCoverages { get; set; }
                public PosLookupRecord[] PosLookupRecords { get; set; }

                //Chaining Context Positioning Format 3: Coverage-based Chaining Context Glyph Positioning
                //USHORT 	PosFormat 	Format identifier-format = 3
                //USHORT 	BacktrackGlyphCount 	Number of glyphs in the backtracking sequence
                //Offset 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                //USHORT 	InputGlyphCount 	Number of glyphs in input sequence
                //Offset 	Coverage[InputGlyphCount] 	Array of offsets to coverage tables in input sequence, in glyph sequence order
                //USHORT 	LookaheadGlyphCount 	Number of glyphs in lookahead sequence
                //Offset 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                //USHORT 	PosCount 	Number of PosLookupRecords
                //struct 	PosLookupRecord[PosCount] 	Array of PosLookupRecords,in design order


                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("Lookup Sub Table Type 8 Format 3");
                }
            }

            /// <summary>
            /// LookupType 8: Chaining Contextual Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType8(BinaryReader reader)
            {
                long thisLookupTablePos = reader.BaseStream.Position;
                int j = subTableOffsets.Length;

                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default:
                            Utils.WarnUnimplemented("Lookup Table Type 8 Format {0}", format);
                            return;
                        case 1:
                            {
                                //Chaining Context Positioning Format 1: Simple Chaining Context Glyph Positioning
                                //uint16 	PosFormat 	        Format identifier-format = 1
                                //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                                //uint16 	ChainPosRuleSetCount 	Number of ChainPosRuleSet tables
                                //Offset16 	ChainPosRuleSet[ChainPosRuleSetCount] 	Array of offsets to ChainPosRuleSet tables-from beginning of ContextPos subtable-ordered by Coverage Index

                                ushort coverageOffset = reader.ReadUInt16();
                                ushort chainPosRuleSetCount = reader.ReadUInt16();
                                ushort[] chainPosRuleSetOffsetList = Utils.ReadUInt16Array(reader, chainPosRuleSetCount);

                                LkSubTableType8Fmt1 subTable = new LkSubTableType8Fmt1();

                                subTable.PosRuleSetTables = CreateMultiplePosRuleSetTables(subTableStartAt, chainPosRuleSetOffsetList, reader);
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                                //----------

                                subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                //Chaining Context Positioning Format 2: Class-based Chaining Context Glyph Positioning
                                //uint16 	PosFormat 	                Format identifier-format = 2
                                //Offset16 	Coverage 	                Offset to Coverage table-from beginning of ChainContextPos subtable
                                //Offset16 	BacktrackClassDef 	        Offset to ClassDef table containing backtrack sequence context-from beginning of ChainContextPos subtable
                                //Offset16 	InputClassDef 	            Offset to ClassDef table containing input sequence context-from beginning of ChainContextPos subtable
                                //Offset16 	LookaheadClassDef                   	Offset to ClassDef table containing lookahead sequence context-from beginning of ChainContextPos subtable
                                //uint16 	ChainPosClassSetCnt 	                Number of ChainPosClassSet tables
                                //Offset16 	ChainPosClassSet[ChainPosClassSetCnt] 	Array of offsets to ChainPosClassSet tables-from beginning of ChainContextPos subtable-ordered by input class-may be NULL

                                ushort coverageOffset = reader.ReadUInt16();
                                ushort backTrackClassDefOffset = reader.ReadUInt16();
                                ushort inpuClassDefOffset = reader.ReadUInt16();
                                ushort lookadheadClassDefOffset = reader.ReadUInt16();
                                ushort chainPosClassSetCnt = reader.ReadUInt16();
                                ushort[] chainPosClassSetOffsetArray = Utils.ReadUInt16Array(reader, chainPosClassSetCnt);

                                LkSubTableType8Fmt2 subTable = new LkSubTableType8Fmt2(chainPosClassSetOffsetArray);
                                subTable.BacktrackClassDefOffset = backTrackClassDefOffset;
                                subTable.InputClassDefOffset = inpuClassDefOffset;
                                subTable.LookaheadClassDefOffset = lookadheadClassDefOffset;
                                //----------
                                PosClassSetTable[] posClassSetTables = new PosClassSetTable[chainPosClassSetCnt];
                                subTable.PosClassSetTables = posClassSetTables;
                                for (int n = 0; n < chainPosClassSetCnt; ++n)
                                {
                                    posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, subTableStartAt + chainPosClassSetOffsetArray[n]);
                                }
                                //----------

                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                                //----------  
                                subTables.Add(subTable);
                                //----------
                            }
                            break;
                        case 3:
                            {

                                //Chaining Context Positioning Format 3: Coverage-based Chaining Context Glyph Positioning
                                //uint16 	PosFormat 	Format identifier-format = 3
                                //uint16 	BacktrackGlyphCount 	Number of glyphs in the backtracking sequence
                                //Offset16 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                                //uint16 	InputGlyphCount 	Number of glyphs in input sequence
                                //Offset16 	Coverage[InputGlyphCount] 	Array of offsets to coverage tables in input sequence, in glyph sequence order
                                //uint16 	LookaheadGlyphCount 	Number of glyphs in lookahead sequence
                                //Offset16 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                                //uint16 	PosCount 	Number of PosLookupRecords
                                //struct 	PosLookupRecord[PosCount] 	Array of PosLookupRecords,in design order

                                var subTable = new LkSubTableType8Fmt3();
                                //
                                ushort backtrackGlyphCount = reader.ReadUInt16();
                                ushort[] backtrackCoverageOffsets = Utils.ReadUInt16Array(reader, backtrackGlyphCount);
                                ushort inputGlyphCount = reader.ReadUInt16();
                                ushort[] inputGlyphCoverageOffsets = Utils.ReadUInt16Array(reader, inputGlyphCount);
                                //
                                ushort lookaheadGlyphCount = reader.ReadUInt16();
                                ushort[] lookaheadCoverageOffsets = Utils.ReadUInt16Array(reader, lookaheadGlyphCount);
                                //
                                ushort posCount = reader.ReadUInt16();
                                subTable.PosLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);
                                //--------------

                                subTable.BacktrackCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, backtrackCoverageOffsets, reader);
                                subTable.InputGlyphCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, inputGlyphCoverageOffsets, reader);
                                subTable.LookaheadCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, lookaheadCoverageOffsets, reader);
                                subTables.Add(subTable);

                            }
                            break;
                    }

                }
            }
            /// <summary>
            /// LookupType 9: Extension Positioning
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType9(BinaryReader reader)
            {
                //Console.WriteLine("skip lookup type 9");
            }
        }
    }

}