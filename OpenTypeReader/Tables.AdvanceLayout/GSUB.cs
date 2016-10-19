//Apache2, 2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{

    class GSUB : TableEntry
    {
        //from https://www.microsoft.com/typography/otspec/GSUB.htm


        ScriptList scriptList = new ScriptList();
        FeatureList featureList = new FeatureList();
        List<LookupTable> lookupTables = new List<LookupTable>();

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

            //The GSUB table begins with a header that contains a version number for the table (Version) and offsets to a three tables: ScriptList, FeatureList, and LookupList. For descriptions of each of these tables, see the chapter, OpenType Common Table Formats. Example 1 at the end of this chapter shows a GSUB Header table definition.
            //GSUB Header, Version 1.0
            //Type 	Name 	Description
            //USHORT 	MajorVersion 	Major version of the GSUB table, = 1
            //USHORT 	MinorVersion 	Minor version of the GSUB table, = 0
            //Offset 	ScriptList 	Offset to ScriptList table, from beginning of GSUB table
            //Offset 	FeatureList 	Offset to FeatureList table, from beginning of GSUB table
            //Offset 	LookupList 	Offset to LookupList table, from beginning of GSUB table

            //GSUB Header, Version 1.1
            //Type 	Name 	Description
            //USHORT 	MajorVersion 	Major version of the GSUB table, = 1
            //USHORT 	MinorVersion 	Minor version of the GSUB table, = 1
            //Offset 	ScriptList 	Offset to ScriptList table, from beginning of GSUB table
            //Offset 	FeatureList 	Offset to FeatureList table, from beginning of GSUB table
            //Offset 	LookupList 	Offset to LookupList table, from beginning of GSUB table
            //ULONG 	FeatureVariations 	Offset to FeatureVariations table, from beginning of the GSUB table (may be NULL)
            //--------------------
            MajorVersion = reader.ReadUInt16();
            MinorVersion = reader.ReadUInt16();
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
            lookupTables.Clear();
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

                lookupTables.Add(
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
                LookupTable lookupRecord = lookupTables[i];
                //set origin
                reader.BaseStream.Seek(lookupListHeadPos + subTableOffset[i], SeekOrigin.Begin);
                lookupRecord.ReadRecordContent(reader);
            }

        }
        void ReadFeaureVariations(BinaryReader reader)
        {
            throw new NotImplementedException();
        }

        internal struct LookupResult
        {

            public readonly LookupSubTable foundOnTable;
            public readonly int foundAtIndex;
            public LookupResult(LookupSubTable foundOnTable, int foundAtIndex)
            {
                this.foundAtIndex = foundAtIndex;
                this.foundOnTable = foundOnTable;
            }

        }
        /// <summary>
        /// sub table of a lookup list
        /// </summary>
        internal class LookupTable
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
            public int FindGlyphIndex(int glyphIndex)
            {
                //check if input glyphIndex is in coverage area 
                for (int i = subTables.Count - 1; i >= 0; --i)
                {
                    int foundAtIndex = subTables[i].CoverageTable.FindGlyphIndex(glyphIndex);
                    if (foundAtIndex > -1)
                    {
                        //found                        
                        return foundAtIndex;
                    }
                }
                return -1;
            }
            public void FindGlyphIndexAll(int glyphIndex, List<LookupResult> outputResults)
            {
                //check if input glyphIndex is in coverage area 
                for (int i = subTables.Count - 1; i >= 0; --i)
                {
                    int foundAtIndex = subTables[i].CoverageTable.FindGlyphIndex(glyphIndex);
                    if (foundAtIndex > -1)
                    {
                        //found                        
                        outputResults.Add(new LookupResult(subTables[i], i));
                    }
                }

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

                //SingleSubstFormat1 subtable: Calculated output glyph indices
                //Type 	Name 	Description
                //USHORT 	SubstFormat 	Format identifier-format = 1
                //Offset 	Coverage 	Offset to Coverage table-from beginning of Substitution table
                //SHORT 	DeltaGlyphID 	Add to original GlyphID to get substitute GlyphID

                //------------------------------------
                //1.2 Single Substitution Format 2
                //------------------------------------
                //Format 2 is more flexible than Format 1, but requires more space. 
                //It provides an array of output glyph indices (Substitute) explicitly matched to the input glyph indices specified in the Coverage table.
                //The SingleSubstFormat2 subtable specifies a format identifier (SubstFormat), an offset to a Coverage table that defines the input glyph indices,
                //a count of output glyph indices in the Substitute array (GlyphCount), and a list of the output glyph indices in the Substitute array (Substitute).
                //The Substitute array must contain the same number of glyph indices as the Coverage table. To locate the corresponding output glyph index in the Substitute array, this format uses the Coverage Index returned from the Coverage table.

                //Example 3 at the end of this chapter uses Format 2 to substitute vertically oriented glyphs for horizontally oriented glyphs. 
                //SingleSubstFormat2 subtable: Specified output glyph indices
                //Type 	Name 	Description
                //USHORT 	SubstFormat 	Format identifier-format = 2
                //Offset 	Coverage 	Offset to Coverage table-from beginning of Substitution table
                //USHORT 	GlyphCount 	Number of GlyphIDs in the Substitute array
                //GlyphID 	Substitute
                //[GlyphCount] 	Array of substitute GlyphIDs-ordered by Coverage Index 


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
                                short deltaGlyph = reader.ReadInt16();
                                subTable = new LookupSubTableT1F1(coverage, deltaGlyph);
                            } break;
                        case 2:
                            {
                                ushort glyphCount = reader.ReadUInt16();
                                ushort[] substitueGlyphs = new ushort[glyphCount];// 	Array of substitute GlyphIDs-ordered by Coverage Index
                                for (int n = 0; n < glyphCount; ++n)
                                {
                                    substitueGlyphs[n] = reader.ReadUInt16();
                                }
                                subTable = new LookupSubTableT1F2(coverage, substitueGlyphs);
                            }
                            break;
                    }
                    subTable.CoverageTable = CoverageTable.ReadFrom(reader);
                    this.subTables.Add(subTable);
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

                //MultipleSubstFormat1 subtable: Multiple output glyphs
                //Type 	Name 	Description
                //USHORT 	SubstFormat 	Format identifier-format = 1
                //Offset 	Coverage 	Offset to Coverage table-from beginning of Substitution table
                //USHORT 	SequenceCount 	Number of Sequence table offsets in the Sequence array
                //Offset 	Sequence
                //[SequenceCount] 	Array of offsets to Sequence tables-from beginning of Substitution table-ordered by Coverage Index
                //Sequence table
                //Type 	Name 	Description
                //USHORT 	GlyphCount 	Number of GlyphIDs in the Substitute array. This should always be greater than 0.
                //GlyphID 	Substitute
                //[GlyphCount] 
                Console.WriteLine("skip lookup2");

            }
            /// <summary>
            /// LookupType 3: Alternate Substitution Subtable 
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType3(BinaryReader reader)
            {
                throw new NotImplementedException();
            }
            /// <summary>
            /// LookupType 4: Ligature Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType4(BinaryReader reader)
            {
                Console.WriteLine("skip lookup type 4");
                //throw new NotImplementedException();
            }
            /// <summary>
            /// LookupType 5: Contextual Substitution Subtable
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType5(BinaryReader reader)
            {
                throw new NotImplementedException();
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
                Console.WriteLine("not complete lookup type 6!");
                int j = subTableOffsets.Length;
                for (int i = 0; i < j; ++i)
                {
                    //move to read pos
                    reader.BaseStream.Seek(lookupTablePos + subTableOffsets[i], SeekOrigin.Begin);

                    //-----------------------
                    LookupSubTable subTable = null;
                    ushort format = reader.ReadUInt16();

                    switch (format)
                    {
                        default: throw new NotSupportedException();
                        case 1:
                            {
                                //6.1 Chaining Context Substitution Format 1: Simple Chaining Context Glyph Substitution 
                                ushort coverage = reader.ReadUInt16();
                                ushort chainSubRulesetCount = reader.ReadUInt16();
                                short[] chainSubRulesetOffsets = new short[chainSubRulesetCount];
                                for (int n = 0; n < chainSubRulesetCount; ++n)
                                {
                                    chainSubRulesetOffsets[n] = reader.ReadInt16();
                                }

                            } break;
                        case 2:
                            {
                                //6.2 Chaining Context Substitution Format 2: Class-based Chaining Context Glyph Substitution       
                                //USHORT 	BacktrackGlyphCount 	Number of glyphs in the backtracking sequence
                                //Offset 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                                //USHORT 	InputGlyphCount 	Number of glyphs in input sequence
                                //Offset 	Coverage[InputGlyphCount] 	Array of offsets to coverage tables in input sequence, in glyph sequence order
                                //USHORT 	LookaheadGlyphCount 	Number of glyphs in lookahead sequence
                                //Offset 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                                //USHORT 	SubstCount 	Number of SubstLookupRecords
                                //struct 	SubstLookupRecord
                                //[SubstCount] 	Array of SubstLookupRecords, in design order


                                ushort coverage = reader.ReadUInt16(); //Offset to Coverage table-from beginning of Substitution table
                                short backtrackClassDef = reader.ReadInt16(); //Offset to glyph ClassDef table containing backtrack sequence data-from beginning of Substitution table
                                short inputClassDef = reader.ReadInt16();//Offset to glyph ClassDef table containing input sequence data-from beginning of Substitution table
                                short lookAheadClassDef = reader.ReadInt16();//Offset to glyph ClassDef table containing lookahead sequence data-from beginning of Substitution table
                                ushort chainSubclassSetCount = reader.ReadUInt16(); //Number of ChainSubClassSet tables
                                short[] chainSubClassOffsets = new short[chainSubclassSetCount];
                                for (int n = 0; n < chainSubclassSetCount; ++n)
                                {
                                    chainSubClassOffsets[n] = reader.ReadInt16();
                                }
                            }
                            break;
                        case 3:
                            {
                                //6.3 Chaining Context Substitution Format 3: Coverage-based Chaining Context Glyph Substitution
                                //
                                //USHORT 	BacktrackGlyphCount 	Number of glyphs in the backtracking sequence
                                //Offset 	Coverage[BacktrackGlyphCount] 	Array of offsets to coverage tables in backtracking sequence, in glyph sequence order
                                //USHORT 	InputGlyphCount 	Number of glyphs in input sequence
                                //Offset 	Coverage[InputGlyphCount] 	Array of offsets to coverage tables in input sequence, in glyph sequence order
                                //USHORT 	LookaheadGlyphCount 	Number of glyphs in lookahead sequence
                                //Offset 	Coverage[LookaheadGlyphCount] 	Array of offsets to coverage tables in lookahead sequence, in glyph sequence order
                                //USHORT 	SubstCount 	Number of SubstLookupRecords
                                //struct 	SubstLookupRecord
                                //[SubstCount] 	Array of SubstLookupRecords, in design order
                                //

                                ushort backtrackingGlyphCount = reader.ReadUInt16();
                                short[] backtrackingCoverageArray = new short[backtrackingGlyphCount];
                                for (int n = 0; n < backtrackingGlyphCount; ++n)
                                {
                                    backtrackingCoverageArray[n] = reader.ReadInt16();
                                }
                                ushort inputGlyphCount = reader.ReadUInt16();
                                short[] inputGlyphCoverageArray = new short[inputGlyphCount];
                                for (int n = 0; n < inputGlyphCount; ++n)
                                {
                                    inputGlyphCoverageArray[n] = reader.ReadInt16();
                                }
                                ushort lookAheadGlyphCount = reader.ReadUInt16();
                                short[] lookAheadCoverageArray = new short[lookAheadGlyphCount];
                                for (int n = 0; n < lookAheadGlyphCount; ++n)
                                {
                                    lookAheadCoverageArray[n] = reader.ReadInt16();
                                }

                            }
                            break;
                    }
                    //-------------------------------------------------------------


                    
                    this.subTables.Add(subTable);
                }

                //throw new NotImplementedException();
            }
            /// <summary>
            /// LookupType 7: Extension Substitution
            /// </summary>
            /// <param name="reader"></param>
            void ReadLookupType7(BinaryReader reader)
            {
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

        public bool CheckSubstitution(int inputGlyph)
        {
            List<GSUB.LookupResult> foundResults = new List<LookupResult>();
            for (int i = lookupTables.Count - 1; i >= 0; --i)
            {
                LookupTable lookup = lookupTables[i];
                if (lookup.lookupType != 1)
                {
                    //this version, handle only type1
                    //TODO: implement more
                    continue;
                }
                int foundIndex = lookup.FindGlyphIndex(inputGlyph);
                if (foundIndex > -1)
                {
                    //found here 
                    lookup.FindGlyphIndexAll(inputGlyph, foundResults);
                }
            }
            return false;
        }

    }
}