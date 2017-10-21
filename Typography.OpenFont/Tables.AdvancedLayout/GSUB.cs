//Apache2, 2016-2017,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{

    ////////////////////////////////////////////////////////////////
    //https://www.microsoft.com/typography/developers/opentype/detail.htm 
    //GSUB Table
    //The GSUB table contains substitution lookups that map GIDs to GIDs and associate these mappings with particular OpenType Layout features. The OpenType specification currently supports six different GSUB lookup types:

    //    1. Single        Replaces one glyph with one glyph.
    //    2. Multiple      Replaces one glyph with more than one glyph.
    //    3. Alternate     Replaces one glyph with one of many glyphs.
    //    4. Ligature      Replaces multiple glyphs with one glyph.
    //    5. Context       Replaces one or more glyphs in context.
    //    6. Chaining Context   Replaces one or more glyphs in chained context. 

    //Although these lookups are defined by the font developer, 
    //it is important for application developers to understand that some features require relatively complex UI support.
    //In particular, OTL features using type 3 lookups may require the application to present options
    //to the user (an example of this is provided in the discussion of OTLS in Part One). 
    //In addition, some registered features allow more than one lookup type to be employed, 
    //so application developers cannot rely on supporting only some lookup types.
    //Similarly, features may have both GSUB and GPOS solutions—e.g. the 'Case-Sensitive Forms' feature—so applications 
    //that want to support these features should avoid limiting their support to only one of these tables. 
    //In setting priorities for feature support,
    //it is important to consider the possible interaction of features and to provide users with powerful sets of typographic tools that work together. 

    ////////////////////////////////////////////////////////////////

    public partial class GSUB : TableEntry
    {
        //from https://www.microsoft.com/typography/otspec/GSUB.htm

        ScriptList scriptList = new ScriptList();
        FeatureList featureList = new FeatureList();
        List<LookupTable> lookupList = new List<LookupTable>();

        long gsubTableStartAt;
        public override string Name
        {
            get { return "GSUB"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            //----------
            gsubTableStartAt = reader.BaseStream.Position;
            //----------
            //1. header
            //GSUB Header

            //The GSUB table begins with a header that contains a version number for the table (Version) and
            //offsets to a three tables: ScriptList, FeatureList, and LookupList. 
            //For descriptions of each of these tables, see the chapter, 
            //OpenType Common Table Formats. Example 1 at the end of this chapter shows a GSUB Header table definition.

            //GSUB Header, Version 1.0
            //Type 	Name 	Description
            //uint16 	MajorVersion 	Major version of the GSUB table, = 1
            //uint16 	MinorVersion 	Minor version of the GSUB table, = 0
            //Offset16 	ScriptList 	Offset to ScriptList table, from beginning of GSUB table
            //Offset16 	FeatureList 	Offset to FeatureList table, from beginning of GSUB table
            //Offset16 	LookupList 	Offset to LookupList table, from beginning of GSUB table

            //GSUB Header, Version 1.1
            //Type 	Name 	Description
            //uint16 	MajorVersion 	Major version of the GSUB table, = 1
            //uint16 	MinorVersion 	Minor version of the GSUB table, = 1
            //Offset16 	ScriptList 	Offset to ScriptList table, from beginning of GSUB table
            //Offset16 	FeatureList 	Offset to FeatureList table, from beginning of GSUB table
            //Offset16 	LookupList 	Offset to LookupList table, from beginning of GSUB table
            //Offset32 	FeatureVariations 	Offset to FeatureVariations table, from beginning of the GSUB table (may be NULL)
            //--------------------
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
            ushort scriptListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort featureListOffset = reader.ReadUInt16();//from beginning of GSUB table
            ushort lookupListOffset = reader.ReadUInt16();//from beginning of GSUB table
            uint featureVariations = (MinorVersion == 1) ? reader.ReadUInt32() : 0;//from beginning of GSUB table
            //-----------------------
            //1. scriptlist 
            scriptList = ScriptList.CreateFrom(reader, gsubTableStartAt + scriptListOffset);
            //-----------------------
            //2. feature list             
            featureList = FeatureList.CreateFrom(reader, gsubTableStartAt + featureListOffset);
            //-----------------------
            //3. lookup list 
            ReadLookupListTable(reader, gsubTableStartAt + lookupListOffset);
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

        public ScriptList ScriptList { get { return scriptList; } }
        public FeatureList FeatureList { get { return featureList; } }
        public IList<LookupTable> LookupList { get { return lookupList; } }


        void ReadLookupListTable(BinaryReader reader, long lookupListHeadPos)
        {
            reader.BaseStream.Seek(lookupListHeadPos, SeekOrigin.Begin);
            //
            //------
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //------------------------------
            //LookupList table
            //------------------------------
            //Type 	    Name 	            Description
            //uint16 	LookupCount 	    Number of lookups in this table
            //Offset16 	Lookup[LookupCount] Array of offsets to Lookup tables-from beginning of LookupList -zero based (first lookup is Lookup index = 0)
            //------------------------------
            //Lookup Table
            //A Lookup table (Lookup) defines the specific conditions, type, and results of a substitution or positioning action that is used to implement a feature. For example, a substitution operation requires a list of target glyph indices to be replaced, a list of replacement glyph indices, and a description of the type of substitution action.

            //Each Lookup table may contain only one type of information (LookupType), determined by whether the lookup is part of a GSUB or GPOS table. GSUB supports eight LookupTypes, and GPOS supports nine LookupTypes (for details about LookupTypes, see the GSUB and GPOS chapters of the document).

            //Each LookupType is defined with one or more subtables, and each subtable definition provides a different representation format. The format is determined by the content of the information required for an operation and by required storage efficiency. When glyph information is best presented in more than one format, a single lookup may contain more than one subtable, as long as all the subtables are the same LookupType. For example, within a given lookup, a glyph index array format may best represent one set of target glyphs, whereas a glyph index range format may be better for another set of target glyphs.

            //During text processing, a client applies a lookup to each glyph in the string before moving to the next lookup. A lookup is finished for a glyph after the client makes the substitution/positioning operation. To move to the “next” glyph, the client will typically skip all the glyphs that participated in the lookup operation: glyphs that were substituted/positioned as well as any other glyphs that formed a context for the operation. However, in the case of pair positioning operations (i.e., kerning), the “next” glyph in a sequence may be the second glyph of the positioned pair (see pair positioning lookup for details).

            //A Lookup table contains a LookupType, specified as an integer, that defines the type of information stored in the lookup. The LookupFlag specifies lookup qualifiers that assist a text-processing client in substituting or positioning glyphs. The SubTableCount specifies the total number of SubTables. The SubTable array specifies offsets, measured from the beginning of the Lookup table, to each SubTable enumerated in the SubTable array.
            //------------------------------
            //Lookup table
            //------------------------------
            //Type 	    Name 	        Description
            //uint16 	LookupType 	    Different enumerations for GSUB and GPOS
            //uint16 	LookupFlag 	    Lookup qualifiers
            //uint16 	SubTableCount 	Number of SubTables for this lookup
            //Offset16 	SubTable[SubTableCount] 	Array of offsets to SubTables-from beginning of Lookup table
            //unit16 	MarkFilteringSet
            //------------------------------
            ushort lookupCount = reader.ReadUInt16();
            ushort[] lookupOffsets = Utils.ReadUInt16Array(reader, lookupCount);
            //----------------------------------------------
            //load each sub table
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            for (int i = 0; i < lookupCount; ++i)
            {
                long lookupTablePos = lookupListHeadPos + lookupOffsets[i];
                reader.BaseStream.Seek(lookupTablePos, SeekOrigin.Begin);

                ushort lookupType = reader.ReadUInt16();//Each Lookup table may contain only one type of information (LookupType)
                ushort lookupFlags = reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                //Each LookupType is defined with one or more subtables, and each subtable definition provides a different representation format
                //
                uint[] subTableOffsets = Utils.ReadUInt16ArrayAsUInt32Array(reader, subTableCount);

                ushort markFilteringSet =
                    ((lookupFlags & 0x0010) == 0x0010) ? reader.ReadUInt16() : (ushort)0;

                //
                //https://www.microsoft.com/typography/otspec/gsub.htm#ES
                //LookupType 7: Extension Substitution
                //This lookup provides a mechanism whereby any other lookup type's subtables are stored 
                //at a 32-bit offset location in the 'GSUB' table. 
                //This is needed if the total size of the subtables exceeds the 16-bit
                //limits of the various other offsets in the 'GSUB' table. 
                //In this specification,
                //the subtable stored at the 32-bit offset location is termed the “extension” subtable.
                //---
                //This subtable type uses one format: ExtensionSubstFormat1.
                //7.1 Extension Substitution Subtable Format 1
                //Type Name    Description
                //uint16  substFormat Format identifier.Set to 1.
                //uint16 extensionLookupType     Lookup type of subtable referenced by extensionOffset(that is, the extension subtable).
                //Offset32 extensionOffset     Offset to the extension subtable, of lookup type extensionLookupType, relative to the start of the ExtensionSubstFormat1 subtable.
                //---
                //
                // Substitution Lookup Record
                //
                // When an OpenType layout engine encounters a LookupType 7 Lookup table, it shall:
                //
                // Proceed as though the Lookup table's LookupType field were set to the ExtensionLookupType of the subtables.
                // Proceed as though each extension subtable referenced by ExtensionOffset replaced the LookupType 7 subtable that referenced it.

                if (lookupType == 7)
                {
                    for (int j = 0; j < subTableCount; ++j)
                    {
                        reader.BaseStream.Seek(lookupTablePos + subTableOffsets[j], SeekOrigin.Begin);
                        ushort version = reader.ReadUInt16(); // must be 1
                        lookupType = reader.ReadUInt16(); // must all be the same and != 7
                        subTableOffsets[j] += reader.ReadUInt32();
                    }
                }


                lookupList.Add(
                    new LookupTable(
                        this,
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
                LookupTable lookupRecord = lookupList[i];
                //set origin
                reader.BaseStream.Seek(lookupListHeadPos + lookupOffsets[i], SeekOrigin.Begin);
                lookupRecord.ReadRecordContent(reader);

                //assign gsub owner**
                foreach (LookupSubTable subtable in lookupRecord.SubTables)
                {
                    subtable.OwnerGSub = this;
                }
            }
            //----------------------------------------------
        }
        void ReadFeaureVariations(BinaryReader reader)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// sub table of a lookup list
        /// </summary>
        public class LookupTable
        {
            //--------------------------
            long lookupTablePos;
            //--------------------------
            public readonly ushort lookupType;
            public readonly ushort lookupFlags;
            public readonly ushort subTableCount;

            //the original offset is ushort[],
            //but sometimes it is extended to 32 bits by 
            //LookupType 7: Extension Substitution
            //so we extend this to uint[] 

            public readonly uint[] subTableOffsets;
            public readonly ushort markFilteringSet;
            //--------------------------
            List<LookupSubTable> subTables = new List<LookupSubTable>();
            GSUB ownerGsubTable;
            public LookupTable(
                GSUB owner,
                long lookupTablePos,
                ushort lookupType,
                ushort lookupFlags,
                ushort subTableCount,
                uint[] subTableOffsets,
                ushort markFilteringSet
                 )
            {
                this.ownerGsubTable = owner;
                this.lookupTablePos = lookupTablePos;
                this.lookupType = lookupType;
                this.lookupFlags = lookupFlags;
                this.subTableCount = subTableCount;
                this.subTableOffsets = subTableOffsets;
                this.markFilteringSet = markFilteringSet;
            }
            public GSUB GSubTable
            {
                get { return this.ownerGsubTable; }
            }
            public List<LookupSubTable> SubTables
            {
                get { return subTables; }
            }
            public void DoSubstitution(IGlyphIndexList inputGlyphs, int startAt, int len)
            {
                int j = subTables.Count;
                for (int i = 0; i < j; ++i)
                {
                    subTables[i].DoSubtitution(inputGlyphs, startAt, len);
                }
            }
#if DEBUG
            public override string ToString()
            {
                return lookupType.ToString();
            }
#endif

            public string ForUseWithFeatureId
            {
                get;
                set;
            }
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
                }
            }

            /// <summary>
            ///  for lookup table type 1, format1
            /// </summary>
            class LkSubTableT1Fmt1 : LookupSubTable
            {
                public LkSubTableT1Fmt1(ushort coverageOffset, short deltaGlyph)
                {
                    this.CoverageOffset = coverageOffset;
                    this.DeltaGlyph = deltaGlyph;
                }
                public ushort CoverageOffset { get; set; }
                /// <summary>
                /// Add to original GlyphID to get substitute GlyphID
                /// </summary>
                public short DeltaGlyph
                {
                    //format1
                    get;
                    private set;
                }
                public CoverageTable CoverageTable
                {
                    get;
                    set;
                }

                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    throw new NotImplementedException();
                }
            }
            /// <summary>
            /// for lookup table type 1, format2
            /// </summary>
            class LkSubTableT1Fmt2 : LookupSubTable
            {
                public LkSubTableT1Fmt2(ushort coverageOffset, ushort[] substitueGlyphs)
                {
                    this.CoverageOffset = coverageOffset;
                    this.SubstitueGlyphs = substitueGlyphs;
                }
                public ushort CoverageOffset { get; set; }
                /// <summary>
                /// It provides an array of output glyph indices (Substitute) explicitly matched to the input glyph indices specified in the Coverage table
                /// </summary>
                public ushort[] SubstitueGlyphs
                {
                    get;
                    private set;
                }
                public CoverageTable CoverageTable
                {
                    get;
                    set;
                }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    int endBefore = startAt + len;
                    for (int i = startAt; i < endBefore; ++i)
                    {
                        int foundAt = CoverageTable.FindPosition(glyphIndices[i]);
                        if (foundAt > -1)
                        {
                            glyphIndices.Replace(i, SubstitueGlyphs[foundAt]);
                        }
                    }
                }
            }


            /// <summary>
            /// LookupType 1: Single Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType1(BinaryReader reader)
            {


                //---------------------
                //LookupType 1: Single Substitution Subtable
                //Single substitution (SingleSubst) subtables tell a client to replace a single glyph with another glyph. 
                //The subtables can be either of two formats. 
                //Both formats require two distinct sets of glyph indices: one that defines input glyphs (specified in the Coverage table), 
                //and one that defines the output glyphs. Format 1 requires less space than Format 2, but it is less flexible.
                //------------------------------------
                // 1.1 Single Substitution Format 1
                //------------------------------------
                //Format 1 calculates the indices of the output glyphs, 
                //which are not explicitly defined in the subtable. 
                //To calculate an output glyph index, Format 1 adds a constant delta value to the input glyph index.
                //For the substitutions to occur properly, the glyph indices in the input and output ranges must be in the same order. 
                //This format does not use the Coverage Index that is returned from the Coverage table.

                //The SingleSubstFormat1 subtable begins with a format identifier (SubstFormat) of 1. 
                //An offset references a Coverage table that specifies the indices of the input glyphs.
                //DeltaGlyphID is the constant value added to each input glyph index to calculate the index of the corresponding output glyph.

                //Example 2 at the end of this chapter uses Format 1 to replace standard numerals with lining numerals. 

                //---------------------------------
                //SingleSubstFormat1 subtable: Calculated output glyph indices
                //---------------------------------
                //Type 	    Name 	        Description
                //uint16 	SubstFormat 	Format identifier-format = 1
                //Offset16 	Coverage 	    Offset to Coverage table-from beginning of Substitution table
                //uint16 	DeltaGlyphID 	Add to original GlyphID to get substitute GlyphID

                //------------------------------------
                //1.2 Single Substitution Format 2
                //------------------------------------
                //Format 2 is more flexible than Format 1, but requires more space. 
                //It provides an array of output glyph indices (Substitute) explicitly matched to the input glyph indices specified in the Coverage table.
                //The SingleSubstFormat2 subtable specifies a format identifier (SubstFormat), an offset to a Coverage table that defines the input glyph indices,
                //a count of output glyph indices in the Substitute array (GlyphCount), and a list of the output glyph indices in the Substitute array (Substitute).
                //The Substitute array must contain the same number of glyph indices as the Coverage table. To locate the corresponding output glyph index in the Substitute array, this format uses the Coverage Index returned from the Coverage table.

                //Example 3 at the end of this chapter uses Format 2 to substitute vertically oriented glyphs for horizontally oriented glyphs. 
                //---------------------------------
                //SingleSubstFormat2 subtable: Specified output glyph indices
                //---------------------------------
                //Type 	    Name 	        Description
                //USHORT 	SubstFormat 	Format identifier-format = 2
                //Offset 	Coverage 	    Offset to Coverage table-from beginning of Substitution table
                //USHORT 	GlyphCount 	    Number of GlyphIDs in the Substitute array
                //GlyphID 	Substitute[GlyphCount] 	Array of substitute GlyphIDs-ordered by Coverage Index 
                //---------------------------------

                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //-----------------------

                    ushort format = reader.ReadUInt16();
                    ushort coverage = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                short deltaGlyph = reader.ReadInt16();
                                var subTable = new LkSubTableT1Fmt1(coverage, deltaGlyph);
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                this.subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                ushort glyphCount = reader.ReadUInt16();
                                ushort[] substitueGlyphs = Utils.ReadUInt16Array(reader, glyphCount); // 	Array of substitute GlyphIDs-ordered by Coverage Index                                 
                                var subTable = new LkSubTableT1Fmt2(coverage, substitueGlyphs);
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                this.subTables.Add(subTable);
                            }
                            break;
                    }
                }
            }


            class LkSubTableT2 : LookupSubTable
            {

                public CoverageTable CoverageTable { get; set; }
                public SequenceTable[] SeqTables { get; set; }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    int lim = startAt + len;
                    for (int i = startAt; i < lim; ++i)
                    {
                        int foundPos = CoverageTable.FindPosition(glyphIndices[i]);
                        if (foundPos > -1)
                        {
                            SequenceTable seqTable = SeqTables[foundPos];
                            //replace current glyph index with new seq
                            int new_seqCount = seqTable.substitueGlyphs.Length;
                            glyphIndices.Replace(i, seqTable.substitueGlyphs);
                            lim += (new_seqCount - 1);
                            i += (new_seqCount - 1);
                        }
                    }
                }
            }
            struct SequenceTable
            {
                public ushort[] substitueGlyphs;
                public SequenceTable(ushort[] substitueGlyphs)
                {
                    this.substitueGlyphs = substitueGlyphs;
                }
            }
            /// <summary>
            /// LookupType 2: Multiple Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType2(BinaryReader reader)
            {

                //LookupType 2: Multiple Substitution Subtable 
                //A Multiple Substitution (MultipleSubst) subtable replaces a single glyph with more than one glyph, 
                //as when multiple glyphs replace a single ligature. 
                //The subtable has a single format: MultipleSubstFormat1. The subtable specifies a format identifier (SubstFormat), an offset to a Coverage table that defines the input glyph indices, a count of offsets in the Sequence array (SequenceCount), and an array of offsets to Sequence tables that define the output glyph indices (Sequence). The Sequence table offsets are ordered by the Coverage Index of the input glyphs.

                //For each input glyph listed in the Coverage table, a Sequence table defines the output glyphs. Each Sequence table contains a count of the glyphs in the output glyph sequence (GlyphCount) and an array of output glyph indices (Substitute).

                //    Note: The order of the output glyph indices depends on the writing direction of the text. For text written left to right, the left-most glyph will be first glyph in the sequence. Conversely, for text written right to left, the right-most glyph will be first.

                //The use of multiple substitution for deletion of an input glyph is prohibited. GlyphCount should always be greater than 0. 
                //Example 4 at the end of this chapter shows how to replace a single ligature with three glyphs. 

                //----------------------
                //MultipleSubstFormat1 subtable: Multiple output glyphs
                //----------------------
                //Type 	    Name 	                Description
                //uint16 	SubstFormat 	        Format identifier-format = 1
                //Offset16 	Coverage    	        Offset to Coverage table-from beginning of Substitution table
                //uint16 	SequenceCount 	        Number of Sequence table offsets in the Sequence array
                //Offset16 	Sequence[SequenceCount] Array of offsets to Sequence tables-from beginning of Substitution table-ordered by Coverage Index
                ////----------------------
                //Sequence table
                //Type 	    Name 	                Description
                //uint16 	GlyphCount 	            Number of glyph IDs  in the Substitute array. This should always be greater than 0.
                //uint16 	Substitute[GlyphCount]  String of glyph IDs  to substitute
                //----------------------
                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                ushort coverageOffset = reader.ReadUInt16();
                                ushort seqCount = reader.ReadUInt16();
                                ushort[] seqOffsets = Utils.ReadUInt16Array(reader, seqCount);

                                var subTable = new LkSubTableT2();
                                subTable.SeqTables = new SequenceTable[seqCount];
                                for (int n = 0; n < seqCount; ++n)
                                {
                                    reader.BaseStream.Seek(subTableStartAt + seqOffsets[n], SeekOrigin.Begin);
                                    ushort glyphCount = reader.ReadUInt16();
                                    subTable.SeqTables[n] = new SequenceTable(
                                        Utils.ReadUInt16Array(reader, glyphCount));
                                }
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);

                                this.subTables.Add(subTable);
                            }
                            break;
                    }
                    //------------------------------------------------------------- 
                }
            }
            /// <summary>
            /// LookupType 3: Alternate Substitution Subtable
            /// </summary>
            class LkSubTableT3 : LookupSubTable
            {
                public CoverageTable CoverageTable
                {
                    get;
                    set;
                }
                public AlternativeSetTable[] AlternativeSetTables { get; set; }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    //Coverage table containing the indices of glyphs with alternative forms(Coverage),
                    int end = startAt + len;
                    for (int i = startAt; i < end; ++i)
                    {
                        int iscovered = this.CoverageTable.FindPosition(glyphIndices[i]);
                    }
                    //this.CoverageTable.FindPosition()
                    // Console.WriteLine("lksubtable3 is not  implemented");
                }
            }
            /// <summary>
            /// LookupType 3: Alternate Substitution Subtable 
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType3(BinaryReader reader)
            {
                //LookupType 3: Alternate Substitution Subtable

                //An Alternate Substitution (AlternateSubst)subtable identifies any number of aesthetic alternatives
                //from which a user can choose a glyph variant to replace the input glyph.

                //For example, if a font contains four variants of the ampersand symbol,
                //the cmap table will specify the index of one of the four glyphs as the default glyph index, 
                //and an AlternateSubst subtable will list the indices of the other three glyphs as alternatives.
                //A text - processing client would then have the option of replacing the default glyph with any of the three alternatives.

                //The subtable has one format: AlternateSubstFormat1.
                //The subtable contains a format identifier (SubstFormat),
                //    an offset to a Coverage table containing the indices of glyphs with alternative forms(Coverage),
                //    a count of offsets to AlternateSet tables(AlternateSetCount), 
                //    and an array of offsets to AlternateSet tables(AlternateSet).

                //For each glyph, an AlternateSet subtable contains a count of the alternative glyphs(GlyphCount) and
                //   an array of their glyph indices(Alternate).
                //Because all the glyphs are functionally equivalent, they can be in any order in the array.

                //Example 5 at the end of this chapter shows how to replace the default ampersand glyph with alternative glyphs.

                //-----------------------
                //AlternateSubstFormat1 subtable: Alternative output glyphs
                //-----------------------
                //Type          Name                Description
                //uint16        SubstFormat         Format identifier - format = 1
                //Offset16      Coverage            Offset to Coverage table - from beginning of Substitution table
                //uint16        AlternateSetCount   Number of AlternateSet tables
                //Offset16      AlternateSet[AlternateSetCount] Array of offsets to AlternateSet tables - from beginning of Substitution table - ordered by Coverage Index
                //
                //AlternateSet table
                //Type    Name    Description
                //uint16  GlyphCount  Number of glyph IDs in the Alternate array
                //uint16  Alternate[GlyphCount]   Array of alternate glyph IDs -in arbitrary order
                //-----------------------

                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    //
                    ushort format = reader.ReadUInt16(); //The subtable has one format: AlternateSubstFormat1.
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                ushort coverageOffset = reader.ReadUInt16();
                                ushort alternativeSetCount = reader.ReadUInt16();
                                ushort[] alternativeTableOffsets = Utils.ReadUInt16Array(reader, alternativeSetCount);

                                LkSubTableT3 subTable = new LkSubTableT3();
                                AlternativeSetTable[] alternativeSetTables = new AlternativeSetTable[alternativeSetCount];
                                subTable.AlternativeSetTables = alternativeSetTables;
                                for (int n = 0; n < alternativeSetCount; ++n)
                                {
                                    alternativeSetTables[n] = AlternativeSetTable.CreateFrom(reader, subTableStartAt + alternativeTableOffsets[n]);
                                }
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);

                                this.subTables.Add(subTable);
                            }
                            break;
                    }
                }
            }

            class AlternativeSetTable
            {
                public ushort[] alternativeGlyphIds;
                public static AlternativeSetTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    // 
                    AlternativeSetTable altTable = new AlternativeSetTable();
                    ushort glyphCount = reader.ReadUInt16();
                    altTable.alternativeGlyphIds = Utils.ReadUInt16Array(reader, glyphCount);
                    return altTable;
                }
            }

            class LkSubTableT4 : LookupSubTable
            {
                public CoverageTable CoverageTable { get; set; }
                public LigatureSetTable[] LigatureSetTables { get; set; }

                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    //check coverage
                    int lim = startAt + len;
                    for (int c = startAt; c < lim; ++c)
                    {
                        ushort glyphIndex = glyphIndices[c];
                        int foundPos = this.CoverageTable.FindPosition(glyphIndex);
                        if (foundPos > -1)
                        {
                            LigatureSetTable ligTable = LigatureSetTables[foundPos];
                            LigatureTable[] ligs = ligTable.Ligatures;
                            int j = ligs.Length;
                            for (int i = 0; i < j; ++i)
                            {
                                LigatureTable lig = ligs[i];
                                int remainingLen = lim - (c + 1);
                                int compLen = lig.ComponentGlyphs.Length;
                                if (compLen > remainingLen)
                                {   //skip tp next component
                                    continue;
                                }
                                bool allMatched = true;
                                int tmp_i = c + 1;
                                for (int p = 0; p < compLen; ++p)
                                {
                                    if (glyphIndices[tmp_i] != lig.ComponentGlyphs[p])
                                    {
                                        allMatched = false;
                                        break; //exit from loop
                                    }
                                }
                                if (allMatched)
                                {
                                    //remove all match and replace with selected glyph                                     
                                    glyphIndices.Replace(c, compLen + 1, lig.GlyphId);
                                    lim -= compLen;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            class LigatureSetTable
            {
                //LigatureSet table: All ligatures beginning with the same glyph
                //Type 	    Name 	        Description
                //uint16 	LigatureCount 	Number of Ligature tables
                //Offset16 	Ligature[LigatureCount] 	Array of offsets to Ligature tables-from beginning of LigatureSet table-ordered by preference

                public LigatureTable[] Ligatures { get; set; }
                public static LigatureSetTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    LigatureSetTable ligSetTable = new LigatureSetTable();
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    //
                    ushort ligCount = reader.ReadUInt16(); //Number of Ligature tables
                    ushort[] ligOffsets = Utils.ReadUInt16Array(reader, ligCount);
                    //
                    LigatureTable[] ligTables = ligSetTable.Ligatures = new LigatureTable[ligCount];
                    for (int i = 0; i < ligCount; ++i)
                    {
                        ligTables[i] = LigatureTable.CreateFrom(reader, beginAt + ligOffsets[i]);
                    }
                    return ligSetTable;
                }

            }
            struct LigatureTable
            {
                //uint16 	LigGlyph 	GlyphID of ligature to substitute
                //uint16 	CompCount 	Number of components in the ligature
                //uint16 	Component[CompCount - 1] 	Array of component GlyphIDs-start with the second component-ordered in writing direction
                /// <summary>
                /// output glyph
                /// </summary>
                public ushort GlyphId { get; set; }
                /// <summary>
                /// ligature component start with second ordered glyph
                /// </summary>
                public ushort[] ComponentGlyphs { get; set; }
                public static LigatureTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    // 
                    LigatureTable ligTable = new LigatureTable();
                    ligTable.GlyphId = reader.ReadUInt16();
                    ushort compCount = reader.ReadUInt16();
                    ligTable.ComponentGlyphs = Utils.ReadUInt16Array(reader, compCount - 1);
                    return ligTable;
                }
#if DEBUG
                public override string ToString()
                {
                    var stbuilder = new System.Text.StringBuilder();
                    int j = ComponentGlyphs.Length;
                    stbuilder.Append("output:" + GlyphId + ",{");

                    for (int i = 0; i < j; ++i)
                    {
                        if (i > 0)
                        {
                            stbuilder.Append(',');
                        }
                        stbuilder.Append(ComponentGlyphs[i]);
                    }
                    stbuilder.Append("}");
                    return stbuilder.ToString();
                }
#endif
            }
            /// <summary>
            /// LookupType 4: Ligature Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType4(BinaryReader reader)
            {
                //LookupType 4: Ligature Substitution Subtable

                //A Ligature Substitution (LigatureSubst) subtable identifies ligature substitutions where a single glyph 
                //replaces multiple glyphs. One LigatureSubst subtable can specify any number of ligature substitutions.

                //The subtable uses a single format: LigatureSubstFormat1. 
                //It contains a format identifier (SubstFormat),
                //a Coverage table offset (Coverage), a count of the ligature sets defined in this table (LigSetCount),
                //and an array of offsets to LigatureSet tables (LigatureSet).
                //The Coverage table specifies only the index of the first glyph component of each ligature set.

                //-----------------------------
                //LigatureSubstFormat1 subtable: All ligature substitutions in a script
                //-----------------------------
                //Type 	    Name 	        Description
                //uint16 	SubstFormat 	Format identifier-format = 1
                //Offset16 	Coverage 	    Offset to Coverage table-from beginning of Substitution table
                //uint16 	LigSetCount 	Number of LigatureSet tables
                //Offset16 	LigatureSet[LigSetCount] 	Array of offsets to LigatureSet tables-from beginning of Substitution table-ordered by Coverage Index
                //-----------------------------

                //A LigatureSet table, one for each covered glyph, 
                //specifies all the ligature strings that begin with the covered glyph.
                //For example, if the Coverage table lists the glyph index for a lowercase “f,”
                //then a LigatureSet table will define the “ffl,” “fl,” “ffi,” “fi,” and “ff” ligatures.
                //If the Coverage table also lists the glyph index for a lowercase “e,” 
                //then a different LigatureSet table will define the “etc” ligature.

                //A LigatureSet table consists of a count of the ligatures that begin with
                //the covered glyph (LigatureCount) and an array of offsets to Ligature tables,
                //which define the glyphs in each ligature (Ligature). 
                //The order in the Ligature offset array defines the preference for using the ligatures.
                //For example, if the “ffl” ligature is preferable to the “ff” ligature, then the Ligature array would list the offset to the “ffl” Ligature table before the offset to the “ff” Ligature table.
                //-----------------------------
                //LigatureSet table: All ligatures beginning with the same glyph
                //-----------------------------
                //Type 	Name 	Description
                //uint16 	LigatureCount 	Number of Ligature tables
                //Offset16 	Ligature[LigatureCount] 	Array of offsets to Ligature tables-from beginning of LigatureSet table-ordered by preference
                //-----------------------------

                //For each ligature in the set, a Ligature table specifies the GlyphID of the output ligature glyph (LigGlyph);
                // count of the total number of component glyphs in the ligature, including the first component (CompCount); 
                //and an array of GlyphIDs for the components (Component).
                //The array starts with the second component glyph (array index = 1) in the ligature 
                //because the first component glyph is specified in the Coverage table.

                //    Note: The Component array lists GlyphIDs according to the writing direction of the text.
                //For text written right to left, the right-most glyph will be first. 
                //Conversely, for text written left to right, the left-most glyph will be first.

                //Example 6 at the end of this chapter shows how to replace a string of glyphs with a single ligature.
                //-----------------------------
                //Ligature table: Glyph components for one ligature
                //-----------------------------
                //Type 	    Name 	    Description
                //uint16 	LigGlyph 	GlyphID of ligature to substitute
                //uint16 	CompCount 	Number of components in the ligature
                //uint16 	Component[CompCount - 1] 	Array of component GlyphIDs-start with the second component-ordered in writing direction
                //-----------------------------
                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                ushort coverageOffset = reader.ReadUInt16();
                                ushort ligSetCount = reader.ReadUInt16();
                                ushort[] ligSetOffsets = Utils.ReadUInt16Array(reader, ligSetCount);
                                LkSubTableT4 subTable = new LkSubTableT4();
                                LigatureSetTable[] ligSetTables = subTable.LigatureSetTables = new LigatureSetTable[ligSetCount];
                                for (int n = 0; n < ligSetCount; ++n)
                                {
                                    ligSetTables[n] = LigatureSetTable.CreateFrom(reader, subTableStartAt + ligSetOffsets[n]);
                                }
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverageOffset);
                                this.subTables.Add(subTable);

                            }
                            break;
                    }
                }
            }
            /// <summary>
            /// LookupType 5: Contextual Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType5(BinaryReader reader)
            {
                throw new NotImplementedException();
            }
            class ChainSubRuleSetTable
            {
                //ChainSubRuleSet table: All contexts beginning with the same glyph
                //-------------------------------------------------------------------------
                //Type  	Name 	                            Description
                //-------------------------------------------------------------------------
                //uint16 	ChainSubRuleCount 	                Number of ChainSubRule tables
                //Offset16 	ChainSubRule[ChainSubRuleCount] 	Array of offsets to ChainSubRule tables-from beginning of ChainSubRuleSet table-ordered by preference
                //-------------------------------------------------------------------------
                //
                //A ChainSubRule table consists of a count of the glyphs to be matched in the backtrack,
                //input, and lookahead context sequences, including the first glyph in each sequence, 
                //and an array of glyph indices that describe each portion of the contexts. 
                //The Coverage table specifies the index of the first glyph in each context,
                //and each array begins with the second glyph (array index = 1) in the context sequence.

                // Note: All arrays list the indices in the order the corresponding glyphs appear in the text. 
                //For text written from right to left, the right-most glyph will be first; conversely, 
                //for text written from left to right, the left-most glyph will be first.

                ChainSubRuleSubTable[] chainSubRuleSubTables;
                public static ChainSubRuleSetTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    //---
                    ChainSubRuleSetTable table = new ChainSubRuleSetTable();
                    ushort subRuleCount = reader.ReadUInt16();
                    ushort[] subRuleOffsets = Utils.ReadUInt16Array(reader, subRuleCount);
                    ChainSubRuleSubTable[] chainSubRuleSubTables = table.chainSubRuleSubTables = new ChainSubRuleSubTable[subRuleCount];
                    for (int i = 0; i < subRuleCount; ++i)
                    {
                        chainSubRuleSubTables[i] = ChainSubRuleSubTable.CreateFrom(reader, beginAt + subRuleOffsets[i]);
                    }

                    return table;
                }
            }
            //---------------------
            //SubstLookupRecord
            //---------------------
            //Type 	    Name 	            Description
            //uint16 	SequenceIndex 	    Index into current glyph sequence-first glyph = 0
            //uint16 	LookupListIndex 	Lookup to apply to that position-zero-based
            //---------------------
            //The SequenceIndex in a SubstLookupRecord must take into consideration the order 
            //in which lookups are applied to the entire glyph sequence.
            //Because multiple substitutions may occur per context,
            //the SequenceIndex and LookupListIndex refer to the glyph sequence after the text-processing client has applied any previous lookups.
            //In other words, the SequenceIndex identifies the location for the substitution at the time that the lookup is to be applied.
            //For example, consider an input glyph sequence of four glyphs.
            //The first glyph does not have a substitute, but the middle 
            //two glyphs will be replaced with a ligature, and a single glyph will replace the fourth glyph:

            //    The first glyph is in position 0. No lookups will be applied at position 0, so no SubstLookupRecord is defined.
            //    The SubstLookupRecord defined for the ligature substitution specifies the SequenceIndex as position 1,
            //which is the position of the first-glyph component in the ligature string. After the ligature replaces the glyphs in positions 1 and 2, however,
            //the input glyph sequence consists of only three glyphs, not the original four.
            //    To replace the last glyph in the sequence,
            //the SubstLookupRecord defines the SequenceIndex as position 2 instead of position 3. 
            //This position reflects the effect of the ligature substitution applied before this single substitution.

            //    Note: This example assumes that the LookupList specifies the ligature substitution lookup before the single substitution lookup.

            struct SubstLookupRecord
            {
                public readonly ushort sequenceIndex;
                public readonly ushort lookupListIndex;
                public SubstLookupRecord(ushort seqIndex, ushort lookupListIndex)
                {
                    this.sequenceIndex = seqIndex;
                    this.lookupListIndex = lookupListIndex;
                }
                public static SubstLookupRecord[] CreateSubstLookupRecords(BinaryReader reader, ushort ncount)
                {
                    SubstLookupRecord[] results = new SubstLookupRecord[ncount];
                    for (int i = 0; i < ncount; ++i)
                    {
                        results[i] = new SubstLookupRecord(reader.ReadUInt16(), reader.ReadUInt16());
                    }
                    return results;
                }
            }
            class ChainSubRuleSubTable
            {

                //A ChainSubRule table also contains a count of the substitutions to be performed on the input glyph sequence (SubstCount)
                //and an array of SubstitutionLookupRecords (SubstLookupRecord). 
                //Each record specifies a position in the input glyph sequence and a LookupListIndex to the substitution lookup that is applied at that position.
                //The array should list records in design order, or the order the lookups should be applied to the entire glyph sequence.

                //ChainSubRule subtable
                //Type 	Name 	Description
                //uint16 	BacktrackGlyphCount 	Total number of glyphs in the backtrack sequence (number of glyphs to be matched before the first glyph)
                //uint16 	Backtrack[BacktrackGlyphCount] 	Array of backtracking GlyphID's (to be matched before the input sequence)
                //uint16 	InputGlyphCount 	Total number of glyphs in the input sequence (includes the first glyph)
                //uint16 	Input[InputGlyphCount - 1] 	Array of input GlyphIDs (start with second glyph)
                //uint16 	LookaheadGlyphCount 	Total number of glyphs in the look ahead sequence (number of glyphs to be matched after the input sequence)
                //uint16 	LookAhead[LookAheadGlyphCount] 	Array of lookahead GlyphID's (to be matched after the input sequence)
                //uint16 	SubstCount 	Number of SubstLookupRecords
                //struct 	SubstLookupRecord[SubstCount] 	Array of SubstLookupRecords (in design order)

                ushort[] backTrackingGlyphs;
                ushort[] inputGlyphs;
                ushort[] lookaheadGlyphs;
                SubstLookupRecord[] substLookupRecords;
                public static ChainSubRuleSubTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    //
                    //------------
                    ChainSubRuleSubTable subRuleTable = new ChainSubRuleSubTable();
                    ushort backtrackGlyphCount = reader.ReadUInt16();
                    subRuleTable.backTrackingGlyphs = Utils.ReadUInt16Array(reader, backtrackGlyphCount);
                    //--------
                    ushort inputGlyphCount = reader.ReadUInt16();
                    subRuleTable.inputGlyphs = Utils.ReadUInt16Array(reader, inputGlyphCount - 1);//*** start with second glyph, so -1
                    //----------
                    ushort lookaheadGlyphCount = reader.ReadUInt16();
                    subRuleTable.lookaheadGlyphs = Utils.ReadUInt16Array(reader, lookaheadGlyphCount);
                    //------------
                    ushort substCount = reader.ReadUInt16();
                    subRuleTable.substLookupRecords = SubstLookupRecord.CreateSubstLookupRecords(reader, substCount);

                    return subRuleTable;
                }

            }


            class ChainSubClassSet
            {

                //----------------------------------
                //ChainSubRuleSet table: All contexts beginning with the same glyph
                //----------------------------------
                //Type 	    Name 	                Description
                //uint16 	ChainSubClassRuleCnt 	Number of ChainSubClassRule tables
                //Offset16 	ChainSubClassRule[ChainSubClassRuleCount] 	Array of offsets to ChainSubClassRule tables-from beginning of ChainSubClassSet-ordered by preference
                //----------------------------------
                //For each context, a ChainSubClassRule table contains a count of the glyph classes in the context sequence (GlyphCount),
                //including the first class. 
                //A Class array lists the classes, beginning with the second class (array index = 1), that follow the first class in the context.

                //Note: Text order depends on the writing direction of the text. For text written from right to left, the right-most class will be first. Conversely, for text written from left to right, the left-most class will be first.

                //The values specified in the Class array are the values defined in the ClassDef table. 
                //The first class in the sequence,
                //Class 2, is identified in the ChainContextSubstFormat2 table by the ChainSubClassSet array index of the corresponding ChainSubClassSet.

                //A ChainSubClassRule also contains a count of the substitutions to be performed on the context (SubstCount) and an array of SubstLookupRecords (SubstLookupRecord) that supply the substitution data. For each position in the context that requires a substitution, a SubstLookupRecord specifies a LookupList index and a position in the input glyph sequence where the lookup is applied. The SubstLookupRecord array lists SubstLookupRecords in design order-that is, the order in which lookups should be applied to the entire glyph sequence.


                ChainSubClassRuleTable[] subClassRuleTables;
                public static ChainSubClassSet CreateFrom(BinaryReader reader, long beginAt)
                {
                    reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
                    //
                    ChainSubClassSet chainSubClassSet = new ChainSubClassSet();
                    ushort count = reader.ReadUInt16();
                    ushort[] subClassRuleOffsets = Utils.ReadUInt16Array(reader, count);

                    ChainSubClassRuleTable[] subClassRuleTables = chainSubClassSet.subClassRuleTables = new ChainSubClassRuleTable[count];
                    for (int i = 0; i < count; ++i)
                    {
                        subClassRuleTables[i] = ChainSubClassRuleTable.CreateFrom(reader, beginAt + subClassRuleOffsets[i]);
                    }
                    return chainSubClassSet;
                }
            }
            class ChainSubClassRuleTable
            {
                //ChainSubClassRule table: Chaining context definition for one class
                //Type 	Name 	Description
                //USHORT 	BacktrackGlyphCount 	Total number of glyphs in the backtrack sequence (number of glyphs to be matched before the first glyph)
                //USHORT 	Backtrack[BacktrackGlyphCount] 	Array of backtracking classes(to be matched before the input sequence)
                //USHORT 	InputGlyphCount 	Total number of classes in the input sequence (includes the first class)
                //USHORT 	Input[InputGlyphCount - 1] 	Array of input classes(start with second class; to be matched with the input glyph sequence)
                //USHORT 	LookaheadGlyphCount 	Total number of classes in the look ahead sequence (number of classes to be matched after the input sequence)
                //USHORT 	LookAhead[LookAheadGlyphCount] 	Array of lookahead classes(to be matched after the input sequence)
                //USHORT 	SubstCount 	Number of SubstLookupRecords
                //struct 	SubstLookupRecord[SubstCount] 	Array of SubstLookupRecords (in design order)

                ushort[] backtrakcingClassDefs;
                ushort[] inputClassDefs;
                ushort[] lookaheadClassDefs;
                SubstLookupRecord[] subsLookupRecords;
                public static ChainSubClassRuleTable CreateFrom(BinaryReader reader, long beginAt)
                {
                    ChainSubClassRuleTable subClassRuleTable = new ChainSubClassRuleTable();
                    ushort backtrackingCount = reader.ReadUInt16();
                    subClassRuleTable.backtrakcingClassDefs = Utils.ReadUInt16Array(reader, backtrackingCount);
                    ushort inputGlyphCount = reader.ReadUInt16();
                    subClassRuleTable.inputClassDefs = Utils.ReadUInt16Array(reader, inputGlyphCount - 1);//** -1
                    ushort lookaheadGlyphCount = reader.ReadUInt16();
                    subClassRuleTable.lookaheadClassDefs = Utils.ReadUInt16Array(reader, lookaheadGlyphCount);
                    ushort substCount = reader.ReadUInt16();
                    subClassRuleTable.subsLookupRecords = SubstLookupRecord.CreateSubstLookupRecords(reader, substCount);

                    return subClassRuleTable;
                }
            }

            //-------------------------------------------------------------
            class LkSubTableT6Fmt1 : LookupSubTable
            {

                public CoverageTable CoverageTable { get; set; }
                public ChainSubRuleSetTable[] SubRuleSets { get; set; }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    throw new NotImplementedException();
                }
            }


            class LkSubTableT6Fmt2 : LookupSubTable
            {

                public CoverageTable CoverageTable { get; set; }
                public ClassDefTable BacktrackClassDef { get; set; }
                public ClassDefTable InputClassDef { get; set; }
                public ClassDefTable LookaheadClassDef { get; set; }
                public ChainSubClassSet[] ChainSubClassSets { get; set; }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {
                    throw new NotImplementedException();
                }
            }


            class LkSubTableT6Fmt3 : LookupSubTable
            {

                public CoverageTable[] BacktrackingCoverages { get; set; }
                public CoverageTable[] InputCoverages { get; set; }
                public CoverageTable[] LookaheadCoverages { get; set; }
                public SubstLookupRecord[] SubstLookupRecords { get; set; }
                public override void DoSubtitution(IGlyphIndexList glyphIndices, int startAt, int len)
                {

                    int endBefore = startAt + len;
                    for (int i = startAt; i < endBefore; ++i)
                    {
                        ushort cur_glyphIndex = glyphIndices[i];
                        //check if this is in input coverage or not
                        if (CoverageTable.IsInRange(InputCoverages, cur_glyphIndex))
                        {
                            //check back tracking or look ahead
                            if (BacktrackingCoverages.Length > 0 && LookaheadCoverages.Length > 0)
                            {
                                throw new NotSupportedException();
                            }
                            if (BacktrackingCoverages.Length > 0)
                            {

                                if (i > 0)
                                {
                                    //has next glyph
                                    if (CoverageTable.IsInRange(BacktrackingCoverages, glyphIndices[i - 1]))
                                    {
                                        //match!, then
                                        //do substitution
                                        ushort replaceAt = SubstLookupRecords[0].sequenceIndex;
                                        ushort lookupIndex = SubstLookupRecords[0].lookupListIndex;
#if DEBUG
                                        if (replaceAt != 0)
                                        {

                                        }
#endif
                                        LookupTable anotherLookup = this.OwnerGSub.LookupList[lookupIndex];
                                        anotherLookup.DoSubstitution(glyphIndices, i + replaceAt, 1);//?          
                                        //****
                                        continue;
                                    }
                                }
                            }
                            if (LookaheadCoverages.Length > 0)
                            {
                                if (i < len - 1)
                                {
                                    //has next glyph
                                    if (CoverageTable.IsInRange(LookaheadCoverages, glyphIndices[i + 1]))
                                    {
                                        //match!, then
                                        //do substitution
                                        ushort replaceAt = SubstLookupRecords[0].sequenceIndex;
                                        ushort lookupIndex = SubstLookupRecords[0].lookupListIndex;
#if DEBUG
                                        if (replaceAt != 0)
                                        {

                                        }
#endif
                                        LookupTable anotherLookup = this.OwnerGSub.LookupList[lookupIndex];
                                        anotherLookup.DoSubstitution(glyphIndices, i + replaceAt, 1);//?                                         
                                        //****
                                        continue;
                                    }
                                }
                            }
                        }
                        //-----------------
                        //no substituion occurs
                        //just pass t 
                    }
                }
            }

            /// <summary>
            /// LookupType 6: Chaining Contextual Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType6(BinaryReader reader)
            {
                //LookupType 6: Chaining Contextual Substitution Subtable
                //A Chaining Contextual Substitution subtable (ChainContextSubst) describes glyph substitutions in context with an ability to look back and/or look ahead
                //in the sequence of glyphs. 
                //The design of the Chaining Contextual Substitution subtable is parallel to that of the Contextual Substitution subtable,
                //including the availability of three formats for handling sequences of glyphs, glyph classes, or glyph sets. Each format can describe one or more backtrack,
                //input, and lookahead sequences and one or more substitutions for each sequence.
                //-----------------------
                //TODO: impl here

                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    ushort format = reader.ReadUInt16();
                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                //6.1 Chaining Context Substitution Format 1: Simple Chaining Context Glyph Substitution 
                                //-------------------------------
                                //ChainContextSubstFormat1 subtable: Simple context glyph substitution
                                //-------------------------------
                                //Type  	Name 	        Description
                                //uint16 	SubstFormat 	Format identifier-format = 1
                                //Offset16 	Coverage 	    Offset to Coverage table-from beginning of Substitution table
                                //uint16 	ChainSubRuleSetCount 	Number of ChainSubRuleSet tables-must equal GlyphCount in Coverage table
                                //Offset16 	ChainSubRuleSet[ChainSubRuleSetCount] 	Array of offsets to ChainSubRuleSet tables-from beginning of Substitution table-ordered by Coverage Index
                                //-------------------------------

                                var subTable = new LkSubTableT6Fmt1();
                                ushort coverage = reader.ReadUInt16();
                                ushort chainSubRulesetCount = reader.ReadUInt16();
                                ushort[] chainSubRulesetOffsets = Utils.ReadUInt16Array(reader, chainSubRulesetCount);
                                ChainSubRuleSetTable[] subRuleSets = subTable.SubRuleSets = new ChainSubRuleSetTable[chainSubRulesetCount];
                                for (int n = 0; n < chainSubRulesetCount; ++n)
                                {
                                    subRuleSets[n] = ChainSubRuleSetTable.CreateFrom(reader, subTableStartAt + chainSubRulesetOffsets[n]);
                                }
                                //----------------------------
                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                this.subTables.Add(subTable);
                            }
                            break;
                        case 2:
                            {
                                //-------------------
                                //ChainContextSubstFormat2 subtable: Class-based chaining context glyph substitution
                                //-------------------
                                //Type 	    Name 	            Description
                                //uint16 	SubstFormat 	    Format identifier-format = 2
                                //Offset16 	Coverage 	        Offset to Coverage table-from beginning of Substitution table
                                //Offset16 	BacktrackClassDef 	Offset to glyph ClassDef table containing backtrack sequence data-from beginning of Substitution table
                                //Offset16 	InputClassDef 	    Offset to glyph ClassDef table containing input sequence data-from beginning of Substitution table
                                //Offset16 	LookaheadClassDef 	Offset to glyph ClassDef table containing lookahead sequence data-from beginning of Substitution table
                                //uint16 	ChainSubClassSetCnt 	Number of ChainSubClassSet tables
                                //Offset16 	ChainSubClassSet[ChainSubClassSetCnt] 	Array of offsets to ChainSubClassSet tables-from beginning of Substitution table-ordered by input class-may be NULL
                                //-------------------
                                var subTable = new LkSubTableT6Fmt2();
                                ushort coverage = reader.ReadUInt16();
                                ushort backtrackClassDefOffset = reader.ReadUInt16();
                                ushort inputClassDefOffset = reader.ReadUInt16();
                                ushort lookaheadClassDefOffset = reader.ReadUInt16();
                                ushort chainSubClassSetCount = reader.ReadUInt16();
                                ushort[] chainSubClassSetOffsets = Utils.ReadUInt16Array(reader, chainSubClassSetCount);
                                //
                                subTable.BacktrackClassDef = ClassDefTable.CreateFrom(reader, subTableStartAt + backtrackClassDefOffset);
                                subTable.InputClassDef = ClassDefTable.CreateFrom(reader, subTableStartAt + inputClassDefOffset);
                                subTable.LookaheadClassDef = ClassDefTable.CreateFrom(reader, subTableStartAt + lookaheadClassDefOffset);
                                if (chainSubClassSetCount != 0)
                                {
                                    ChainSubClassSet[] chainSubClassSets = subTable.ChainSubClassSets = new ChainSubClassSet[chainSubClassSetCount];
                                    for (int n = 0; n < chainSubClassSetCount; ++n)
                                    {
                                        chainSubClassSets[n] = ChainSubClassSet.CreateFrom(reader, subTableStartAt + chainSubClassSetOffsets[i]);
                                    }
                                }

                                subTable.CoverageTable = CoverageTable.CreateFrom(reader, subTableStartAt + coverage);
                                this.subTables.Add(subTable);
                            }
                            break;
                        case 3:
                            {
                                //-------------------
                                //6.3 Chaining Context Substitution Format 3: Coverage-based Chaining Context Glyph Substitution
                                //-------------------
                                //uint16 	BacktrackGlyphCount 	Number of glyphs in the backtracking sequence
                                //Offset16 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                                //uint16 	InputGlyphCount 	Number of glyphs in input sequence
                                //Offset16 	Coverage[InputGlyphCount] 	Array of offsets to coverage tables in input sequence, in glyph sequence order
                                //uint16 	LookaheadGlyphCount 	Number of glyphs in lookahead sequence
                                //Offset16 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                                //uint16 	SubstCount 	Number of SubstLookupRecords
                                //struct 	SubstLookupRecord[SubstCount] 	Array of SubstLookupRecords, in design order
                                //-------------------
                                LkSubTableT6Fmt3 subTable = new LkSubTableT6Fmt3();
                                ushort backtrackingGlyphCount = reader.ReadUInt16();
                                ushort[] backtrackingCoverageOffsets = Utils.ReadUInt16Array(reader, backtrackingGlyphCount);
                                ushort inputGlyphCount = reader.ReadUInt16();
                                ushort[] inputGlyphCoverageOffsets = Utils.ReadUInt16Array(reader, inputGlyphCount);
                                ushort lookAheadGlyphCount = reader.ReadUInt16();
                                ushort[] lookAheadCoverageOffsets = Utils.ReadUInt16Array(reader, lookAheadGlyphCount);
                                ushort substCount = reader.ReadUInt16();
                                subTable.SubstLookupRecords = SubstLookupRecord.CreateSubstLookupRecords(reader, substCount);
                                //
                                subTable.BacktrackingCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, backtrackingCoverageOffsets, reader);
                                subTable.InputCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, inputGlyphCoverageOffsets, reader);
                                subTable.LookaheadCoverages = CoverageTable.CreateMultipleCoverageTables(subTableStartAt, lookAheadCoverageOffsets, reader);


                                subTables.Add(subTable);
                            }
                            break;
                    }
                    //------------------------------------------------------------- 
                }
            }
            /// <summary>
            /// LookupType 7: Extension Substitution
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType7(BinaryReader reader)
            {

                //LookupType 7: Extension Substitution

                //This lookup provides a mechanism whereby any other lookup type's subtables are stored at a 32-bit offset location in the 'GSUB' table. 
                //This is needed if the total size of the subtables exceeds the 16-bit limits of the various other offsets in the 'GSUB' table.
                //In this specification, the subtable stored at the 32-bit offset location is termed the “extension” subtable.
                //----------------------------
                //ExtensionSubstFormat1 subtable
                //----------------------------
                //Type      Name                Description
                //uint16    SubstFormat         Format identifier.Set to 1.
                //uint16    ExtensionLookupType Lookup type of subtable referenced by ExtensionOffset (i.e.the extension subtable).
                //Offset32     ExtensionOffset     Offset to the extension subtable, of lookup type ExtensionLookupType, relative to the start of the ExtensionSubstFormat1 subtable.
                //----------------------------
                //ExtensionLookupType must be set to any lookup type other than 7.
                //All subtables in a LookupType 7 lookup must have the same ExtensionLookupType.
                //All offsets in the extension subtables are set in the usual way, 
                //i.e.relative to the extension subtables themselves.

                //When an OpenType layout engine encounters a LookupType 7 Lookup table, it shall:

                //Proceed as though the Lookup table's LookupType field were set to the ExtensionLookupType of the subtables.
                //Proceed as though each extension subtable referenced by ExtensionOffset replaced the LookupType 7 subtable that referenced it.

                //Substitution Lookup Record

                //All contextual substitution subtables specify the substitution data in a Substitution Lookup Record (SubstLookupRecord).
                //Each record contains a SequenceIndex, 
                //which indicates the position where the substitution will occur in the glyph sequence.
                //In addition, a LookupListIndex identifies the lookup to be applied at the glyph position specified by the SequenceIndex.

                //The contextual substitution subtables defined in Examples 7, 8, and 9 at the end of this chapter show SubstLookupRecords.
                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    long subTableStartAt = lookupTablePos + subTableOffsets[i];
                    reader.BaseStream.Seek(subTableStartAt, SeekOrigin.Begin);
                    ushort format = reader.ReadUInt16();
                    ushort extensionLookupType = reader.ReadUInt16();
                    uint extensionOffset = reader.ReadUInt32();
                    if (extensionLookupType == 7)
                    {
                        throw new NotSupportedException();
                    }
                }
                //TODO: impl more , this is not complete!
                return;
                throw new NotImplementedException();
            }

            /// <summary>
            /// LookupType 8: Reverse Chaining Contextual Single Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType8(BinaryReader reader)
            {
                throw new NotImplementedException();
            }
        }

    }
}