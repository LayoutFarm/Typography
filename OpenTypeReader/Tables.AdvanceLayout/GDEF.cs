//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//https://www.microsoft.com/typography/otspec/GDEF.htm
//GDEF — Glyph Definition Table

//The Glyph Definition (GDEF) table contains six types of information in six independent tables:

//    The GlyphClassDef table classifies the different types of glyphs in the font.
//    The AttachmentList table identifies all attachment points on the glyphs, which streamlines data access and bitmap caching.
//    The LigatureCaretList table contains positioning data for ligature carets, which the text-processing client uses on screen to select and highlight the individual components of a ligature glyph.
//    The MarkAttachClassDef table classifies mark glyphs, to help group together marks that are positioned similarly.
//    The MarkGlyphSetsTable allows the enumeration of an arbitrary number of glyph sets that can be used as an extension of the mark attachment class definition to allow lookups to filter mark glyphs by arbitrary sets of marks.
//    The ItemVariationStore table is used in variable fonts to contain variation data used for adjustment of values in the GDEF, GPOS or JSTF tables.

//The GSUB and GPOS tables may reference certain GDEF table information used for processing of lookup tables. See, for example, the LookupFlag bit enumeration in “OpenType Layout Common Table Formats”.

//In variable fonts, the GDEF, GPOS and JSTF tables may all reference variation data within the ItemVariationStore table contained within the GDEF table. See below for further discussion of variable fonts and the ItemVariationStore table.


namespace NRasterizer.Tables
{
    //

    /// <summary>
    /// 
    /// </summary>
    class GDEF : TableEntry
    {
        long gdefTableStartAt;
        ScriptList scriptList = new ScriptList();
        FeatureList featureList = new FeatureList();
        public override string Name
        {
            get { return "GDEF"; }
        }

        protected override void ReadContentFrom(BinaryReader reader)
        {
            gdefTableStartAt = reader.BaseStream.Position;
            //-----------------------------------------
            //GDEF Header, Version 1.0
            //Type 	Name 	Description
            //USHORT 	MajorVersion 	Major version of the GDEF table, = 1
            //USHORT 	MinorVersion 	Minor version of the GDEF table, = 0
            //Offset 	GlyphClassDef 	Offset to class definition table for glyph type, from beginning of GDEF header (may be NULL)
            //Offset 	AttachList 	Offset to list of glyphs with attachment points, from beginning of GDEF header (may be NULL)
            //Offset 	LigCaretList 	Offset to list of positioning points for ligature carets, from beginning of GDEF header (may be NULL)
            //Offset 	MarkAttachClassDef 	Offset to class definition table for mark attachment type, from beginning of GDEF header (may be NULL)
            //GDEF Header, Version 1.2
            //Type 	Name 	Description
            //USHORT 	MajorVersion 	Major version of the GDEF table, = 1
            //USHORT 	MinorVersion 	Minor version of the GDEF table, = 2
            //Offset 	GlyphClassDef 	Offset to class definition table for glyph type, from beginning of GDEF header (may be NULL)
            //Offset 	AttachList 	Offset to list of glyphs with attachment points, from beginning of GDEF header (may be NULL)
            //Offset 	LigCaretList 	Offset to list of positioning points for ligature carets, from beginning of GDEF header (may be NULL)
            //Offset 	MarkAttachClassDef 	Offset to class definition table for mark attachment type, from beginning of GDEF header (may be NULL)
            //Offset 	MarkGlyphSetsDef 	Offset to the table of mark set definitions, from beginning of GDEF header (may be NULL)
            //GDEF Header, Version 1.3
            //Type 	Name 	Description
            //USHORT 	MajorVersion 	Major version of the GDEF table, = 1
            //USHORT 	MinorVersion 	Minor version of the GDEF table, = 3
            //Offset 	GlyphClassDef 	Offset to class definition table for glyph type, from beginning of GDEF header (may be NULL)
            //Offset 	AttachList 	Offset to list of glyphs with attachment points, from beginning of GDEF header (may be NULL)
            //Offset 	LigCaretList 	Offset to list of positioning points for ligature carets, from beginning of GDEF header (may be NULL)
            //Offset 	MarkAttachClassDef 	Offset to class definition table for mark attachment type, from beginning of GDEF header (may be NULL)
            //Offset 	MarkGlyphSetsDef 	Offset to the table of mark set definitions, from beginning of GDEF header (may be NULL)
            //ULONG 	ItemVarStore 	Offset to the Item Variation Store table, from beginning of GDEF header (may be NULL)

            this.MajorVersion = reader.ReadUInt16();
            this.MinorVersion = reader.ReadUInt16();
            //
            short glyphClassDefOffset = reader.ReadInt16();
            short attachListOffset = reader.ReadInt16();
            short ligCaretList = reader.ReadInt16();
            short markAttachClassDef = reader.ReadInt16();
            short markGlyphSetsDef = 0;
            uint itemVarStore = 0;
            switch (MinorVersion)
            {
                default: throw new NotSupportedException();
                case 0: break;
                case 1:
                    markGlyphSetsDef = reader.ReadInt16();
                    break;
                case 3:
                    markGlyphSetsDef = reader.ReadInt16();
                    itemVarStore = reader.ReadUInt32();
                    break;
            }
            //---------------
            reader.BaseStream.Seek(this.Header.Offset + glyphClassDefOffset, SeekOrigin.Begin);


            reader.BaseStream.Seek(this.Header.Offset + attachListOffset, SeekOrigin.Begin);


            reader.BaseStream.Seek(this.Header.Offset + ligCaretList, SeekOrigin.Begin);


            reader.BaseStream.Seek(this.Header.Offset + markAttachClassDef, SeekOrigin.Begin);

            if (markGlyphSetsDef != 0)
            {
                reader.BaseStream.Seek(this.Header.Offset + markGlyphSetsDef, SeekOrigin.Begin);
            }
            if (itemVarStore != 0)
            {
                reader.BaseStream.Seek(this.Header.Offset + itemVarStore, SeekOrigin.Begin);
            }
        }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }

    }
}