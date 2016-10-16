//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{
    class GSUB : TableEntry
    {
        //from https://www.microsoft.com/typography/otspec/GSUB.htm

        List<ScriptRecord> scriptRecords = new List<ScriptRecord>();
        List<FeatureRecord> featureRecords = new List<FeatureRecord>();
        List<LookupRecord> lookupRecords = new List<LookupRecord>();

        public override string Name
        {
            get { return "GSUB"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {

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
            ReadScriptListTable(reader);
            //-----------------------
            //2. feature list
            reader.BaseStream.Seek(this.Header.Offset + featureListOffset, SeekOrigin.Begin);
            ReadFeatureListTable(reader);
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

        void ReadScriptListTable(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //ScriptList table
            //Type 	Name 	Description
            //USHORT 	ScriptCount 	Number of ScriptRecords
            //struct 	ScriptRecord
            //[ScriptCount] 	Array of ScriptRecords
            //-listed alphabetically by ScriptTag
            //ScriptRecord
            //Type 	Name 	Description
            //Tag 	ScriptTag 	4-byte ScriptTag identifier
            //Offset 	Script 	Offset to Script table-from beginning of ScriptList
            scriptRecords.Clear();
            ushort scriptCount = reader.ReadUInt16();
            for (int i = 0; i < scriptCount; ++i)
            {
                //read script record
                scriptRecords.Add(new ScriptRecord(
                    reader.ReadUInt32(),
                    reader.ReadUInt16()));
            }
        }

        void ReadFeatureListTable(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //FeatureList table
            //Type 	Name 	Description
            //USHORT 	FeatureCount 	Number of FeatureRecords in this table
            //struct 	FeatureRecord[FeatureCount] 	Array of FeatureRecords-zero-based (first feature has FeatureIndex = 0)-listed alphabetically by FeatureTag
            //FeatureRecord
            //Type 	Name 	Description
            //Tag 	FeatureTag 	4-byte feature identification tag
            //Offset 	Feature 	Offset to Feature table-from beginning of FeatureList
            featureRecords.Clear();
            ushort featureCount = reader.ReadUInt16();
            for (int i = 0; i < featureCount; ++i)
            {
                //read script record
                featureRecords.Add(new FeatureRecord(
                    reader.ReadUInt32(),
                    reader.ReadUInt16()));
            }

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
            for (int i = 0; i < lookupCount; ++i)
            {
                reader.BaseStream.Seek(lookupListHeadPos + subTableOffset[i], SeekOrigin.Begin);

                ushort lookupType = reader.ReadUInt16();
                ushort lookupFlags = reader.ReadUInt16();
                ushort subTableCount = reader.ReadUInt16();
                //
                ushort[] subTableOffsets = new ushort[subTableCount];
                for (int m = 0; m < subTableCount; ++m)
                {
                    subTableOffsets[m] = reader.ReadUInt16();
                }
                ushort markFilteringSet = reader.ReadUInt16();
                lookupRecords.Add(
                    new LookupRecord(lookupType,
                        lookupFlags,
                        subTableCount,
                        subTableOffsets,
                        markFilteringSet));
            }
        }
        void ReadFeaureVariations(BinaryReader reader)
        {

        }

        struct ScriptRecord
        {
            public readonly uint scriptTag;//4-byte ScriptTag identifier
            public readonly ushort offset; //Script Offset to Script table-from beginning of ScriptList
            public ScriptRecord(uint scriptTag, ushort offset)
            {
                this.scriptTag = scriptTag;
                this.offset = offset;
            }
            public string ScriptName
            {
                get { return TagToString(scriptTag); }
            }


            public override string ToString()
            {
                return ScriptName + "," + offset;
            }
        }
        struct FeatureRecord
        {
            public readonly uint scriptTag;//4-byte ScriptTag identifier
            public readonly ushort offset; //Script Offset to Script table-from beginning of ScriptList
            public FeatureRecord(uint scriptTag, ushort offset)
            {
                this.scriptTag = scriptTag;
                this.offset = offset;
            }
            public string ScriptName
            {
                get { return TagToString(scriptTag); }
            }
            public override string ToString()
            {
                return ScriptName + "," + offset;
            }
        }

        struct LookupRecord
        {
            public readonly ushort lookupType;
            public readonly ushort lookupFlags;
            public readonly ushort subTableCount;
            public readonly ushort[] offsets;
            public readonly ushort markFilteringSet;

            public LookupRecord(ushort lookupType,
                ushort lookupFlags,
                ushort subTableCount,
                ushort[] offsets,
                ushort markFilteringSet
                 )
            {
                this.lookupType = lookupType;
                this.lookupFlags = lookupFlags;
                this.subTableCount = subTableCount;
                this.offsets = offsets;
                this.markFilteringSet = markFilteringSet;
            }
        }
        static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return Encoding.ASCII.GetString(bytes);
        }

    }
}