//Apache2, 2016-present, WinterDev, Sam Hocevar <sam@hocevar.net>

using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    // https://www.microsoft.com/typography/otspec/GPOS.htm
    public partial class GPOS : GlyphShapingTableEntry
    {
        public const string Name = "GPOS";
        internal GPOS(TableHeader header, BinaryReader input) : base(header, input) { }
        //
        protected override void ReadLookupTable(BinaryReader reader, long lookupTablePos,
                                                ushort lookupType, ushort lookupFlags,
                                                ushort[] subTableOffsets, ushort markFilteringSet)
        {
            LookupTable lookupTable = new LookupTable(lookupType, lookupFlags, markFilteringSet);
            foreach (long subTableOffset in subTableOffsets)
            {
                LookupSubTable subTable = lookupTable.ReadSubTable(reader, lookupTablePos + subTableOffset);
                subTable.OwnerGPos = this;
                lookupTable.SubTables.Add(subTable);
            }

#if DEBUG
            lookupTable.dbugLkIndex = LookupList.Count;
#endif

            LookupList.Add(lookupTable);
        }

        protected override void ReadFeatureVariations(BinaryReader reader, long featureVariationsBeginAt)
        {
            Utils.WarnUnimplemented("GPOS feature variations");
        }

        private List<LookupTable> _lookupList = new List<LookupTable>();

        public IList<LookupTable> LookupList => _lookupList;

        public abstract class LookupSubTable
        {
            public GPOS? OwnerGPos;

            public abstract void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len);
        }

        /// <summary>
        /// Subtable for unhandled/unimplemented features
        /// </summary>
        public class UnImplementedLookupSubTable : LookupSubTable
        {
            readonly string _msg;
            public UnImplementedLookupSubTable(string message)
            {
                _msg = message;
                Utils.WarnUnimplemented(message);
            }
            public override string ToString()
            {
                return _msg;
            }
            public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
            }
        }

        /// <summary>
        /// sub table of a lookup list
        /// </summary>
        public partial class LookupTable
        {
#if DEBUG
            public int dbugLkIndex;
#endif

            public ushort lookupType { get; private set; }
            public readonly ushort lookupFlags;
            public readonly ushort markFilteringSet;
            //--------------------------
            List<LookupSubTable> _subTables = new List<LookupSubTable>();
            public LookupTable(ushort lookupType, ushort lookupFlags, ushort markFilteringSet)
            {
                this.lookupType = lookupType;
                this.lookupFlags = lookupFlags;
                this.markFilteringSet = markFilteringSet;
            }
            public void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
            {
                foreach (LookupSubTable subTable in SubTables)
                {
                    subTable.DoGlyphPosition(inputGlyphs, startAt, len);
                    //update len
                    len = inputGlyphs.Count;
                }
            }
            public IList<LookupSubTable> SubTables { get { return _subTables; } }

#if DEBUG
            public override string ToString()
            {
                return lookupType.ToString();
            }
#endif
            public LookupSubTable ReadSubTable(BinaryReader reader, long subTableStartAt)
            {
                switch (lookupType)
                {
                    case 1: return ReadLookupType1(reader, subTableStartAt);
                    case 2: return ReadLookupType2(reader, subTableStartAt);
                    case 3: return ReadLookupType3(reader, subTableStartAt);
                    case 4: return ReadLookupType4(reader, subTableStartAt);
                    case 5: return ReadLookupType5(reader, subTableStartAt);
                    case 6: return ReadLookupType6(reader, subTableStartAt);
                    case 7: return ReadLookupType7(reader, subTableStartAt);
                    case 8: return ReadLookupType8(reader, subTableStartAt);
                    case 9: return ReadLookupType9(reader, subTableStartAt);
                }

                return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Type {0}", lookupType));
            }

            class LkSubTableType1 : LookupSubTable
            {
                ValueRecord? _singleValue;
                ValueRecord?[]? _multiValues;
                public LkSubTableType1(ValueRecord? singleValue, CoverageTable coverage)
                {
                    this.Format = 1;
                    _singleValue = singleValue;
                    CoverageTable = coverage;
                }
                public LkSubTableType1(ValueRecord?[] valueRecords, CoverageTable coverage)
                {
                    this.Format = 2;
                    _multiValues = valueRecords;
                    CoverageTable = coverage;
                }
                public int Format { get; private set; }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 1");
                }
            }

            /// <summary>
            /// Lookup Type 1: Single Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            static LookupSubTable ReadLookupType1(BinaryReader reader, long subTableStartAt)
            {
                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                switch (format)
                {
                    default: throw new NotSupportedException();
                    case 1:
                        {
                            //Single Adjustment Positioning: Format 1
                            //Value 	    Type 	        Description
                            //uint16 	    PosFormat 	    Format identifier-format = 1
                            //Offset16 	    Coverage 	    Offset to Coverage table-from beginning of SinglePos subtable
                            //uint16 	    ValueFormat     Defines the types of data in the ValueRecord
                            //ValueRecord 	Value 	        Defines positioning value(s)-applied to all glyphs in the Coverage table 
                            ushort coverage = reader.ReadUInt16();
                            ushort valueFormat = reader.ReadUInt16();
                            return new LkSubTableType1(ValueRecord.CreateFrom(reader, valueFormat),
                                CoverageTable.CreateFrom(reader, subTableStartAt + coverage));
                        }
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
                            var values = new ValueRecord?[valueCount];
                            for (int n = 0; n < valueCount; ++n)
                            {
                                values[n] = ValueRecord.CreateFrom(reader, valueFormat);
                            }
                            return new LkSubTableType1(values,
                                CoverageTable.CreateFrom(reader, subTableStartAt + coverage));
                        }
                }
            }

            /// <summary>
            /// Lookup Type 2, Format1: Pair Adjustment Positioning Subtable
            /// </summary>
            class LkSubTableType2Fmt1 : LookupSubTable
            {
                internal PairSetTable[] _pairSetTables;
                public LkSubTableType2Fmt1(PairSetTable[] pairSetTables, CoverageTable coverage)
                {
                    _pairSetTables = pairSetTables;
                    CoverageTable = coverage;
                }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    //find marker
                    CoverageTable covTable = this.CoverageTable;
                    int lim = inputGlyphs.Count - 1;
                    for (int i = 0; i < lim; ++i)
                    {
                        int firstGlyphFound = covTable.FindPosition(inputGlyphs.GetGlyph(i, out ushort glyph_advW));
                        if (firstGlyphFound > -1)
                        {
                            //test this with Palatino A-Y sequence
                            PairSetTable pairSet = _pairSetTables[firstGlyphFound];

                            //check second glyph  
                            ushort second_glyph_index = inputGlyphs.GetGlyph(i + 1, out ushort second_glyph_w);

                            if (pairSet.FindPairSet(second_glyph_index, out PairSet foundPairSet))
                            {
                                ValueRecord? v1 = foundPairSet.value1;
                                ValueRecord? v2 = foundPairSet.value2;
                                //TODO: recheck for vertical writing ... (YAdvance)
                                if (v1 != null)
                                {
                                    inputGlyphs.AppendGlyphAdvance(i, v1.XAdvance, 0);
                                }

                                if (v2 != null)
                                {
                                    inputGlyphs.AppendGlyphAdvance(i + 1, v2.XAdvance, 0);
                                }

                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Lookup Type2, Format2: Class pair adjustment
            /// </summary>
            class LkSubTableType2Fmt2 : LookupSubTable
            {
                //Format 2 defines a pair as a set of two glyph classes and modifies the positions of all the glyphs in a class
                internal readonly Lk2Class1Record[] _class1records;
                internal readonly ClassDefTable _class1Def;
                internal readonly ClassDefTable _class2Def;

                public LkSubTableType2Fmt2(Lk2Class1Record[] class1records, ClassDefTable class1Def, ClassDefTable class2Def, CoverageTable coverage)
                {
                    _class1records = class1records;
                    _class1Def = class1Def;
                    _class2Def = class2Def;
                    CoverageTable = coverage;
                }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {

                    //coverage
                    //The Coverage table lists the indices of the first glyphs that may appear in each glyph pair.
                    //More than one pair may begin with the same glyph, 
                    //but the Coverage table lists the glyph index only once

                    CoverageTable covTable = this.CoverageTable;
                    int lim = inputGlyphs.Count - 1;
                    for (int i = 0; i < lim; ++i) //start at 0
                    {
                        ushort glyph1_index = inputGlyphs.GetGlyph(i, out ushort glyph_advW);
                        int record1Index = covTable.FindPosition(glyph1_index);
                        if (record1Index > -1)
                        {
                            int class1_no = _class1Def.GetClassValue(glyph1_index);
                            if (class1_no > -1)
                            {
                                ushort glyph2_index = inputGlyphs.GetGlyph(i + 1, out ushort glyph_advW2);
                                int class2_no = _class2Def.GetClassValue(glyph2_index);

                                if (class2_no > -1)
                                {
                                    Lk2Class1Record class1Rec = _class1records[class1_no];
                                    //TODO: recheck for vertical writing ... (YAdvance)
                                    Lk2Class2Record pair = class1Rec.class2Records[class2_no];

                                    ValueRecord? v1 = pair.value1;
                                    ValueRecord? v2 = pair.value2;

                                    if (v1 != null && v1.XAdvance != 0)
                                    {
                                        inputGlyphs.AppendGlyphAdvance(i, v1.XAdvance, 0);
                                    }

                                    if (v2 != null)
                                    {
                                        inputGlyphs.AppendGlyphAdvance(i + 1, v2.XAdvance, 0);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            struct Lk2Class1Record
            {
                // a Class1Record enumerates all pairs that contain a particular class as a first component.
                //The Class1Record array stores all Class1Records according to class value.

                //Note: Class1Records are not tagged with a class value identifier.
                //Instead, the index value of a Class1Record in the array defines the class value represented by the record.
                //For example, the first Class1Record enumerates pairs that begin with a Class 0 glyph,
                //the second Class1Record enumerates pairs that begin with a Class 1 glyph, and so on.

                //Each Class1Record contains an array of Class2Records (Class2Record), which also are ordered by class value. 
                //One Class2Record must be declared for each class in the ClassDef2 table, including Class 0.
                //--------------------------------
                //Class1Record
                //Value 	Type 	Description
                //struct 	Class2Record[Class2Count] 	Array of Class2 records-ordered by Class2
                //--------------------------------
                public readonly Lk2Class2Record[] class2Records;
                public Lk2Class1Record(Lk2Class2Record[] class2Records)
                {
                    this.class2Records = class2Records;
                }
                //#if DEBUG
                //                public override string ToString()
                //                {
                //                    System.Text.StringBuilder stbuilder = new System.Text.StringBuilder();
                //                    for (int i = 0; i < class2Records.Length; ++i)
                //                    {
                //                        Lk2Class2Record rec = class2Records[i];
                //                        string str = rec.ToString();

                //                        if (str != "value1:,value2:")
                //                        {
                //                            //skip
                //                            stbuilder.Append("i=" + i + "=>" + str + "    ");
                //                        }
                //                    }
                //                    return stbuilder.ToString();
                //                    //return base.ToString();
                //                }
                //#endif
            }

            class Lk2Class2Record
            {
                //A Class2Record consists of two ValueRecords,
                //one for the first glyph in a class pair (Value1) and one for the second glyph (Value2).
                //If the PairPos subtable has a value of zero (0) for ValueFormat1 or ValueFormat2, 
                //the corresponding record (ValueRecord1 or ValueRecord2) will be empty.

                //Class2Record
                //--------------------------------
                //Value 	    Type 	Description
                //ValueRecord 	Value1 	Positioning for first glyph-empty if ValueFormat1 = 0
                //ValueRecord 	Value2 	Positioning for second glyph-empty if ValueFormat2 = 0
                //--------------------------------
                public readonly ValueRecord? value1;//null= empty
                public readonly ValueRecord? value2;//null= empty

                public Lk2Class2Record(ValueRecord? value1, ValueRecord? value2)
                {
                    this.value1 = value1;
                    this.value2 = value2;
                }

#if DEBUG
                public override string ToString()
                {
                    return "value1:" + (value1?.ToString()) + ",value2:" + value2?.ToString();
                }
#endif
            }

            /// <summary>
            ///  Lookup Type 2: Pair Adjustment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            static LookupSubTable ReadLookupType2(BinaryReader reader, long subTableStartAt)
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


                //Class2Record
                //--------------------------------
                //Value 	    Type 	Description
                //ValueRecord 	Value1 	Positioning for first glyph-empty if ValueFormat1 = 0
                //ValueRecord 	Value2 	Positioning for second glyph-empty if ValueFormat2 = 0
                //--------------------------------

                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                switch (format)
                {
                    default:
                        return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Table Type 2 Format {0}", format));
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
                            return new LkSubTableType2Fmt1(pairSetTables,
                                CoverageTable.CreateFrom(reader, subTableStartAt + coverage));
                        }
                    case 2:
                        {
                            ushort coverage = reader.ReadUInt16();
                            ushort value1Format = reader.ReadUInt16();
                            ushort value2Format = reader.ReadUInt16();
                            ushort classDef1_offset = reader.ReadUInt16();
                            ushort classDef2_offset = reader.ReadUInt16();
                            ushort class1Count = reader.ReadUInt16();
                            ushort class2Count = reader.ReadUInt16();

                            Lk2Class1Record[] class1Records = new Lk2Class1Record[class1Count];
                            for (int c1 = 0; c1 < class1Count; ++c1)
                            {
                                //for each c1 record

                                Lk2Class2Record[] class2Records = new Lk2Class2Record[class2Count];
                                for (int c2 = 0; c2 < class2Count; ++c2)
                                {
                                    class2Records[c2] = new Lk2Class2Record(
                                          ValueRecord.CreateFrom(reader, value1Format),
                                          ValueRecord.CreateFrom(reader, value2Format));
                                }
                                class1Records[c1] = new Lk2Class1Record(class2Records);
                            }

                            return new LkSubTableType2Fmt2(class1Records,
                                ClassDefTable.CreateFrom(reader, subTableStartAt + classDef1_offset),
                                ClassDefTable.CreateFrom(reader, subTableStartAt + classDef2_offset),
                                CoverageTable.CreateFrom(reader, subTableStartAt + coverage));
                        }
                }
            }

            /// <summary>
            /// Lookup Type 3: Cursive Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            static LookupSubTable ReadLookupType3(BinaryReader reader, long subTableStartAt)
            {
                // TODO: implement this

                return new UnImplementedLookupSubTable("GPOS Lookup Table Type 3");
            }

            /// <summary>
            /// Lookup Type 4: MarkToBase Attachment Positioning, or called (MarkBasePos) table
            /// </summary>
            class LkSubTableType4 : LookupSubTable
            {
                public LkSubTableType4(CoverageTable markCoverageTable, CoverageTable baseCoverageTable, MarkArrayTable markArrayTable, BaseArrayTable baseArrayTable)
                {
                    MarkCoverageTable = markCoverageTable;
                    BaseCoverageTable = baseCoverageTable;
                    MarkArrayTable = markArrayTable;
                    BaseArrayTable = baseArrayTable;
                }

                public CoverageTable MarkCoverageTable { get; set; }
                public CoverageTable BaseCoverageTable { get; set; }
                public MarkArrayTable MarkArrayTable { get; set; }
                public BaseArrayTable BaseArrayTable { get; set; }

                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    int xpos = 0;
                    //find marker
                    int j = inputGlyphs.Count;

                    startAt = Math.Max(startAt, 1);

                    for (int i = startAt; i < j; ++i) //start at 1
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
                    List<ushort> expandedMarks = new List<ushort>(MarkCoverageTable.GetExpandedValueIter());
                    if (expandedMarks.Count != MarkArrayTable.dbugGetAnchorCount())
                    {
                        throw new NotSupportedException();
                    }
                    //--------------------------
                    List<ushort> expandedBase = new List<ushort>(BaseCoverageTable.GetExpandedValueIter());
                    if (expandedBase.Count != BaseArrayTable.dbugGetRecordCount())
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
            static LookupSubTable ReadLookupType4(BinaryReader reader, long subTableStartAt)
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
                //Value 	Type 	        Description
                //uint16 	PosFormat 	    Format identifier-format = 1
                //Offset16 	MarkCoverage 	Offset to MarkCoverage table-from beginning of MarkBasePos subtable ( all the mark glyphs referenced in the subtable)
                //Offset16 	BaseCoverage 	Offset to BaseCoverage table-from beginning of MarkBasePos subtable (all the base glyphs referenced in the subtable)
                //uint16 	ClassCount 	    Number of classes defined for marks
                //Offset16 	MarkArray 	    Offset to MarkArray table-from beginning of MarkBasePos subtable
                //Offset16 	BaseArray 	    Offset to BaseArray table-from beginning of MarkBasePos subtable
                //----------------------------------------------

                //The BaseArray table consists of an array (BaseRecord) and count (BaseCount) of BaseRecords. 
                //The array stores the BaseRecords in the same order as the BaseCoverage Index. 
                //Each base glyph in the BaseCoverage table has a BaseRecord.

                //BaseArray table
                //Value 	Type 	Description
                //uint16 	BaseCount 	Number of BaseRecords
                //struct 	BaseRecord[BaseCount] 	Array of BaseRecords-in order of BaseCoverage Index

                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                if (format != 1)
                {
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 4 Format {0}", format));
                }
                ushort markCoverageOffset = reader.ReadUInt16(); //offset from
                ushort baseCoverageOffset = reader.ReadUInt16();
                ushort markClassCount = reader.ReadUInt16();
                ushort markArrayOffset = reader.ReadUInt16();
                ushort baseArrayOffset = reader.ReadUInt16();

                //read mark array table
                var lookupType4 = new LkSubTableType4(
                    CoverageTable.CreateFrom(reader, subTableStartAt + markCoverageOffset),
                    CoverageTable.CreateFrom(reader, subTableStartAt + baseCoverageOffset),
                    MarkArrayTable.CreateFrom(reader, subTableStartAt + markArrayOffset),
                    BaseArrayTable.CreateFrom(reader, subTableStartAt + baseArrayOffset, markClassCount));
#if DEBUG
                lookupType4.dbugTest();
#endif
                return lookupType4;
            }

            class LkSubTableType5 : LookupSubTable
            {
                public LkSubTableType5(CoverageTable markCoverage, CoverageTable ligatureCoverage, MarkArrayTable markArrayTable, LigatureArrayTable ligatureArrayTable)
                {
                    MarkCoverage = markCoverage;
                    LigatureCoverage = ligatureCoverage;
                    MarkArrayTable = markArrayTable;
                    LigatureArrayTable = ligatureArrayTable;
                }

                public CoverageTable MarkCoverage { get; set; }
                public CoverageTable LigatureCoverage { get; set; }
                public MarkArrayTable MarkArrayTable { get; set; }
                public LigatureArrayTable LigatureArrayTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 5");
                }
            }

            /// <summary>
            /// Lookup Type 5: MarkToLigature Attachment Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            static LookupSubTable ReadLookupType5(BinaryReader reader, long subTableStartAt)
            {
                //uint16 	PosFormat 	        Format identifier-format = 1
                //Offset16 	MarkCoverage 	    Offset to Mark Coverage table-from beginning of MarkLigPos subtable
                //Offset16 	LigatureCoverage 	Offset to Ligature Coverage table-from beginning of MarkLigPos subtable
                //uint16 	ClassCount 	        Number of defined mark classes
                //Offset16 	MarkArray 	        Offset to MarkArray table-from beginning of MarkLigPos subtable
                //Offset16 	LigatureArray 	    Offset to LigatureArray table-from beginning of MarkLigPos subtable

                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                if (format != 1)
                {
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 5 Format {0}", format));
                }
                ushort markCoverageOffset = reader.ReadUInt16(); //from beginning of MarkLigPos subtable
                ushort ligatureCoverageOffset = reader.ReadUInt16();
                ushort classCount = reader.ReadUInt16();
                ushort markArrayOffset = reader.ReadUInt16();
                ushort ligatureArrayOffset = reader.ReadUInt16();
                //-----------------------
                var markCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + markCoverageOffset);
                var ligatureCoverage = CoverageTable.CreateFrom(reader, subTableStartAt + ligatureCoverageOffset);
                var markArrayTable = MarkArrayTable.CreateFrom(reader, subTableStartAt + markArrayOffset);

                reader.BaseStream.Seek(subTableStartAt + ligatureArrayOffset, SeekOrigin.Begin);
                var ligatureArrayTable = new LigatureArrayTable();
                ligatureArrayTable.ReadFrom(reader, classCount);
                return new LkSubTableType5(markCoverage, ligatureCoverage, markArrayTable, ligatureArrayTable);
            }

            //-----------------------------------------------------------------
            //https://docs.microsoft.com/en-us/typography/opentype/otspec180/gpos#lookup-type-6--marktomark-attachment-positioning-subtable
            /// <summary>
            /// Lookup Type 6: MarkToMark Attachment
            /// defines the position of one mark relative to another mark 
            /// </summary>
            class LkSubTableType6 : LookupSubTable
            {
                public LkSubTableType6(CoverageTable markCoverage1, CoverageTable markCoverage2, MarkArrayTable mark1ArrayTable, Mark2ArrayTable mark2ArrayTable)
                {
                    MarkCoverage1 = markCoverage1;
                    MarkCoverage2 = markCoverage2;
                    Mark1ArrayTable = mark1ArrayTable;
                    Mark2ArrayTable = mark2ArrayTable;
                }

                public CoverageTable MarkCoverage1 { get; set; }
                public CoverageTable MarkCoverage2 { get; set; }
                public MarkArrayTable Mark1ArrayTable { get; set; }
                public Mark2ArrayTable Mark2ArrayTable { get; set; } // Mark2 attachment points used to attach Mark1 glyphs to a specific Mark2 glyph. 


                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    //The attaching mark is Mark1, 
                    //and the base mark being attached to is Mark2.

                    //The Mark2 glyph (that combines with a Mark1 glyph) is the glyph preceding the Mark1 glyph in glyph string order 
                    //(skipping glyphs according to LookupFlags)

                    //@prepare: we must found mark2 glyph before mark1
#if DEBUG
                    if (len == 3 || len == 4)
                    {

                    }
#endif
                    //find marker
                    startAt = Math.Max(startAt, 1);
                    int lim = Math.Min(startAt + len, inputGlyphs.Count);

                    for (int i = startAt; i < lim; ++i)
                    {
                        ushort glyph_adv_w;
                        int mark1Found = MarkCoverage1.FindPosition(inputGlyphs.GetGlyph(i, out glyph_adv_w));
                        if (mark1Found > -1)
                        {
                            //this is mark glyph
                            //then-> look back for base mark (mark2)
                            ushort prev_pos_adv_w;
                            int mark2Found = MarkCoverage2.FindPosition(inputGlyphs.GetGlyph(i - 1, out prev_pos_adv_w));
                            if (mark2Found > -1)
                            {
                                int mark1ClassId = this.Mark1ArrayTable.GetMarkClass(mark1Found);
                                AnchorPoint mark2BaseAnchor = this.Mark2ArrayTable.GetAnchorPoint(mark2Found, mark1ClassId);
                                AnchorPoint mark1Anchor = this.Mark1ArrayTable.GetAnchorPoint(mark1Found);

                                // TODO: review here
                                if (mark1Anchor.ycoord < 0)
                                {
                                    //temp HACK!
                                    //eg. น้ำ in Tahoma 
                                    
                                    inputGlyphs.AppendGlyphOffset(i - 1 /*PREV*/, mark1Anchor.xcoord, mark1Anchor.ycoord);                                     
                                }
                                else
                                {
                                    //short offset_x, offset_y;
                                    //inputGlyphs.GetOffset(i - 1/*PREV*/, out offset_x, out offset_y);
                                    //inputGlyphs.AppendGlyphOffset(
                                    //     i,
                                    //     (short)(offset_x + mark2BaseAnchor.xcoord - mark1Anchor.xcoord),
                                    //     (short)(offset_y + mark2BaseAnchor.ycoord - mark1Anchor.ycoord));


                                    //TEMP hack
                                    short offset_x, offset_y;
                                    inputGlyphs.GetOffset(i - 1/*PREV*/, out offset_x, out offset_y);
                                    inputGlyphs.AppendGlyphOffset(
                                         i,
                                         (short)(offset_x + mark2BaseAnchor.xcoord - mark1Anchor.xcoord),
                                         (short)(offset_y + mark1Anchor.ycoord - mark2BaseAnchor.ycoord));
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
            static LookupSubTable ReadLookupType6(BinaryReader reader, long subTableStartAt)
            {
                // uint16     PosFormat      Format identifier-format = 1
                // Offset16   Mark1Coverage  Offset to Combining Mark Coverage table-from beginning of MarkMarkPos subtable
                // Offset16   Mark2Coverage  Offset to Base Mark Coverage table-from beginning of MarkMarkPos subtable
                // uint16     ClassCount     Number of Combining Mark classes defined
                // Offset16   Mark1Array     Offset to MarkArray table for Mark1-from beginning of MarkMarkPos subtable
                // Offset16   Mark2Array     Offset to Mark2Array table for Mark2-from beginning of MarkMarkPos subtable

                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                if (format != 1)
                {
                    return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 6 Format {0}", format));
                }
                ushort mark1CoverageOffset = reader.ReadUInt16();
                ushort mark2CoverageOffset = reader.ReadUInt16();
                ushort classCount = reader.ReadUInt16();
                ushort mark1ArrayOffset = reader.ReadUInt16();
                ushort mark2ArrayOffset = reader.ReadUInt16();
                //
                return new LkSubTableType6(
                    CoverageTable.CreateFrom(reader, subTableStartAt + mark1CoverageOffset),
                    CoverageTable.CreateFrom(reader, subTableStartAt + mark2CoverageOffset),
                    MarkArrayTable.CreateFrom(reader, subTableStartAt + mark1ArrayOffset),
                    Mark2ArrayTable.CreateFrom(reader, subTableStartAt + mark2ArrayOffset, classCount));
            }

            /// <summary>
            /// Lookup Type 7: Contextual Positioning Subtables
            /// </summary>
            /// <param name="reader"></param>
            static LookupSubTable ReadLookupType7(BinaryReader reader, long subTableStartAt)
            {
                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                switch (format)
                {
                    default:
                        return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Sub Table Type 7 Format {0}", format));
                    case 1:
                        {
                            //Context Positioning Subtable: Format 1
                            //ContextPosFormat1 subtable: Simple context positioning
                            //Value 	Type 	            Description
                            //uint16 	PosFormat 	        Format identifier-format = 1
                            //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                            //uint16 	PosRuleSetCount 	Number of PosRuleSet tables
                            //Offset16 	PosRuleSet[PosRuleSetCount]
                            //
                            ushort coverageOffset = reader.ReadUInt16();
                            ushort posRuleSetCount = reader.ReadUInt16();
                            ushort[] posRuleSetOffsets = Utils.ReadUInt16Array(reader, posRuleSetCount);

                            var posRuleSetTables = CreateMultiplePosRuleSetTables(subTableStartAt, posRuleSetOffsets, reader);
                            var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                            return new LkSubTableType7Fmt1(posRuleSetTables, coverageTable);
                        }
                    case 2:
                        {
                            //Context Positioning Subtable: Format 2
                            //uint16 	PosFormat 	        Format identifier-format = 2
                            //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                            //Offset16 	ClassDef 	        Offset to ClassDef table-from beginning of ContextPos subtable
                            //uint16 	PosClassSetCnt      Number of PosClassSet tables
                            //Offset16 	PosClassSet[PosClassSetCnt] 	Array of offsets to PosClassSet tables-from beginning of ContextPos subtable-ordered by class-may be NULL

                            ushort coverageOffset = reader.ReadUInt16();
                            ushort classDefOffset = reader.ReadUInt16();
                            ushort posClassSetCount = reader.ReadUInt16();
                            ushort[] posClassSetOffsets = Utils.ReadUInt16Array(reader, posClassSetCount);

                            PosClassSetTable[] posClassSetTables = new PosClassSetTable[posClassSetCount];
                            for (int n = 0; n < posClassSetCount; ++n)
                            {
                                posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, subTableStartAt + posClassSetOffsets[n]);
                            }
                            var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                            return new LkSubTableType7Fmt2(classDefOffset, posClassSetTables, coverageTable);
                        }
                    case 3:
                        {
                            //ContextPosFormat3 subtable: Coverage-based context glyph positioning
                            //Value 	Type 	    Description
                            //uint16 	PosFormat 	Format identifier-format = 3
                            //uint16 	GlyphCount 	Number of glyphs in the input sequence
                            //uint16 	PosCount 	Number of PosLookupRecords
                            //Offset16 	Coverage[GlyphCount] 	Array of offsets to Coverage tables-from beginning of ContextPos subtable
                            //struct 	PosLookupRecord[PosCount] Array of positioning lookups-in design order
                            ushort glyphCount = reader.ReadUInt16();
                            ushort posCount = reader.ReadUInt16();
                            //read each lookahead record
                            ushort[] coverageOffsets = Utils.ReadUInt16Array(reader, glyphCount);
                            var posLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);
                            var coverageTables = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, coverageOffsets, reader);
                            return new LkSubTableType7Fmt3(posLookupRecords, coverageTables);
                        }
                }
            }

            class LkSubTableType7Fmt1 : LookupSubTable
            {
                public LkSubTableType7Fmt1(PosRuleSetTable[] posRuleSetTables , CoverageTable coverageTable)
                {
                    PosRuleSetTables = posRuleSetTables;
                    CoverageTable = coverageTable;
                }

                public PosRuleSetTable[] PosRuleSetTables { get; set; }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 7 Format 1");
                }
            }

            class LkSubTableType7Fmt2 : LookupSubTable
            {
                public LkSubTableType7Fmt2(ushort classDefOffset, PosClassSetTable[] posClassSetTables, CoverageTable coverageTable)
                {
                    ClassDefOffset = classDefOffset;
                    PosClassSetTables = posClassSetTables;
                    CoverageTable = coverageTable;
                }

                public ushort ClassDefOffset { get; set; }
                public PosClassSetTable[] PosClassSetTables { get; set; }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 7 Format 2");
                }

            }
            class LkSubTableType7Fmt3 : LookupSubTable
            {
                public LkSubTableType7Fmt3(PosLookupRecord[] posLookupRecords, CoverageTable[] coverageTables)
                {
                    PosLookupRecords = posLookupRecords;
                    CoverageTables = coverageTables;
                }

                public PosLookupRecord[] PosLookupRecords { get; set; }
                public CoverageTable[] CoverageTables { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 7 Format 3");
                }
            }
            //----------------------------------------------------------------
            class LkSubTableType8Fmt1 : LookupSubTable
            {
                public LkSubTableType8Fmt1(PosRuleSetTable[] posRuleSetTables, CoverageTable coverageTable)
                {
                    PosRuleSetTables = posRuleSetTables;
                    CoverageTable = coverageTable;
                }

                public PosRuleSetTable[] PosRuleSetTables { get; set; }
                public CoverageTable CoverageTable { get; set; }
                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 8 Format 1");
                }
            }

            class LkSubTableType8Fmt2 : LookupSubTable
            {
                ushort[] chainPosClassSetOffsetArray;

                public LkSubTableType8Fmt2(ushort[] chainPosClassSetOffsetArray, ushort backtrackClassDefOffset, ushort inputClassDefOffset, ushort lookaheadClassDefOffset, PosClassSetTable[] posClassSetTables, CoverageTable coverageTable)
                {
                    this.chainPosClassSetOffsetArray = chainPosClassSetOffsetArray;
                    BacktrackClassDefOffset = backtrackClassDefOffset;
                    InputClassDefOffset = inputClassDefOffset;
                    LookaheadClassDefOffset = lookaheadClassDefOffset;
                    PosClassSetTables = posClassSetTables;
                    CoverageTable = coverageTable;
                }

                public ushort BacktrackClassDefOffset { get; set; }
                public ushort InputClassDefOffset { get; set; }
                public ushort LookaheadClassDefOffset { get; set; }
                public PosClassSetTable[] PosClassSetTables { get; set; }
                public CoverageTable CoverageTable { get; set; }

                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 8 Format 2");
                }
            }
            class LkSubTableType8Fmt3 : LookupSubTable
            {
                public LkSubTableType8Fmt3(PosLookupRecord[] posLookupRecords, CoverageTable[] backtrackCoverages, CoverageTable[] inputGlyphCoverages, CoverageTable[] lookaheadCoverages)
                {
                    PosLookupRecords = posLookupRecords;
                    BacktrackCoverages = backtrackCoverages;
                    InputGlyphCoverages = inputGlyphCoverages;
                    LookaheadCoverages = lookaheadCoverages;
                }

                public PosLookupRecord[] PosLookupRecords { get; set; }
                public CoverageTable[] BacktrackCoverages { get; set; }
                public CoverageTable[] InputGlyphCoverages { get; set; }
                public CoverageTable[] LookaheadCoverages { get; set; }

                //Chaining Context Positioning Format 3: Coverage-based Chaining Context Glyph Positioning
                //USHORT 	PosFormat 	                    Format identifier-format = 3
                //USHORT 	BacktrackGlyphCount 	        Number of glyphs in the backtracking sequence
                //Offset 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                //USHORT 	InputGlyphCount 	            Number of glyphs in input sequence
                //Offset 	Coverage[InputGlyphCount] 	    Array of offsets to coverage tables in input sequence, in glyph sequence order
                //USHORT 	LookaheadGlyphCount 	        Number of glyphs in lookahead sequence
                //Offset 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                //USHORT 	PosCount 	                    Number of PosLookupRecords
                //struct 	PosLookupRecord[PosCount] 	    Array of PosLookupRecords,in design order


                public override void DoGlyphPosition(IGlyphPositions inputGlyphs, int startAt, int len)
                {
                    Utils.WarnUnimplemented("GPOS Lookup Sub Table Type 8 Format 3");
                }
            }

            /// <summary>
            /// LookupType 8: Chaining Contextual Positioning Subtable
            /// </summary>
            /// <param name="reader"></param>
            LookupSubTable ReadLookupType8(BinaryReader reader, long subTableStartAt)
            {
                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);

                ushort format = reader.ReadUInt16();
                switch (format)
                {
                    default:
                        return new UnImplementedLookupSubTable(string.Format("GPOS Lookup Table Type 8 Format {0}", format));
                    case 1:
                        {
                            //Chaining Context Positioning  Format 1: Simple Chaining Context Glyph Positioning
                            //uint16 	PosFormat 	        Format identifier-format = 1
                            //Offset16 	Coverage 	        Offset to Coverage table-from beginning of ContextPos subtable
                            //uint16 	ChainPosRuleSetCount 	Number of ChainPosRuleSet tables
                            //Offset16 	ChainPosRuleSet[ChainPosRuleSetCount] 	Array of offsets to ChainPosRuleSet tables-from beginning of ContextPos subtable-ordered by Coverage Index

                            ushort coverageOffset = reader.ReadUInt16();
                            ushort chainPosRuleSetCount = reader.ReadUInt16();
                            ushort[] chainPosRuleSetOffsetList = Utils.ReadUInt16Array(reader, chainPosRuleSetCount);

                            var posRuleSetTables = CreateMultiplePosRuleSetTables(subTableStartAt, chainPosRuleSetOffsetList, reader);
                            var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                            return new LkSubTableType8Fmt1(posRuleSetTables, coverageTable);
                        }
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
                            ushort inputClassDefOffset = reader.ReadUInt16();
                            ushort lookaheadClassDefOffset = reader.ReadUInt16();
                            ushort chainPosClassSetCount = reader.ReadUInt16();
                            ushort[] chainPosClassSetOffsetArray = Utils.ReadUInt16Array(reader, chainPosClassSetCount);

                            //----------
                            PosClassSetTable[] posClassSetTables = new PosClassSetTable[chainPosClassSetCount];
                            for (int n = 0; n < chainPosClassSetCount; ++n)
                            {
                                posClassSetTables[n] = PosClassSetTable.CreateFrom(reader, subTableStartAt + chainPosClassSetOffsetArray[n]);
                            }
                            var coverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                            return new LkSubTableType8Fmt2(chainPosClassSetOffsetArray, backTrackClassDefOffset, inputClassDefOffset, lookaheadClassDefOffset, posClassSetTables, coverageTable);
                        }
                    case 3:
                        {
                            //Chaining Context Positioning Format 3: Coverage-based Chaining Context Glyph Positioning
                            //uint16 	PosFormat 	                    Format identifier-format = 3
                            //uint16 	BacktrackGlyphCount 	        Number of glyphs in the backtracking sequence
                            //Offset16 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                            //uint16 	InputGlyphCount 	            Number of glyphs in input sequence
                            //Offset16 	Coverage[InputGlyphCount] 	    Array of offsets to coverage tables in input sequence, in glyph sequence order
                            //uint16 	LookaheadGlyphCount 	        Number of glyphs in lookahead sequence
                            //Offset16 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                            //uint16 	PosCount 	                    Number of PosLookupRecords
                            //struct 	PosLookupRecord[PosCount] 	    Array of PosLookupRecords,in design order

                            ushort backtrackGlyphCount = reader.ReadUInt16();
                            ushort[] backtrackCoverageOffsets = Utils.ReadUInt16Array(reader, backtrackGlyphCount);
                            ushort inputGlyphCount = reader.ReadUInt16();
                            ushort[] inputGlyphCoverageOffsets = Utils.ReadUInt16Array(reader, inputGlyphCount);

                            ushort lookaheadGlyphCount = reader.ReadUInt16();
                            ushort[] lookaheadCoverageOffsets = Utils.ReadUInt16Array(reader, lookaheadGlyphCount);

                            ushort posCount = reader.ReadUInt16();
                            var posLookupRecords = CreateMultiplePosLookupRecords(reader, posCount);

                            var backtrackCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, backtrackCoverageOffsets, reader);
                            var inputGlyphCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, inputGlyphCoverageOffsets, reader);
                            var lookaheadCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, lookaheadCoverageOffsets, reader);

                            return new LkSubTableType8Fmt3(posLookupRecords, backtrackCoverages, inputGlyphCoverages, lookaheadCoverages);
                        }
                }
            }

            /// <summary>
            /// LookupType 9: Extension Positioning
            /// </summary>
            /// <param name="reader"></param>
            LookupSubTable ReadLookupType9(BinaryReader reader, long subTableStartAt)
            {
                reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                ushort format = reader.ReadUInt16();
                ushort extensionLookupType = reader.ReadUInt16();
                uint extensionOffset = reader.ReadUInt32();
                if (extensionLookupType == 9)
                {
                    throw new NotSupportedException();
                }
                // Simply read the lookup table again with updated offsets
                lookupType = extensionLookupType;
                LookupSubTable subTable = ReadSubTable(reader, subTableStartAt + extensionOffset);
                // FIXME: this is a bit hackish, try to find a better construct
                lookupType = 9;
                return subTable;
            }
        }
    }
}

