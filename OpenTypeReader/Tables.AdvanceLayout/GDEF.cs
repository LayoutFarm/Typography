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



    class GDEF : TableEntry
    {
        long tableStartAt;

        public override string Name
        {
            get { return "GDEF"; }
        }

        protected override void ReadContentFrom(BinaryReader reader)
        {
            tableStartAt = reader.BaseStream.Position;
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

            //common to 1.0, 1.2, 1.3...
            this.MajorVersion = reader.ReadUInt16();
            this.MinorVersion = reader.ReadUInt16();
            //
            short glyphClassDefOffset = reader.ReadInt16();
            short attachListOffset = reader.ReadInt16();
            short ligCaretListOffset = reader.ReadInt16();
            short markAttachClassDefOffset = reader.ReadInt16();
            short markGlyphSetsDefOffset = 0;
            uint itemVarStoreOffset = 0;
            //
            switch (MinorVersion)
            {
                default: throw new NotSupportedException();
                case 0: break;
                case 1:
                    markGlyphSetsDefOffset = reader.ReadInt16();
                    break;
                case 3:
                    markGlyphSetsDefOffset = reader.ReadInt16();
                    itemVarStoreOffset = reader.ReadUInt32();
                    break;
            }
            //---------------


            this.GlyphClassDef = (glyphClassDefOffset == 0) ? null : ClassDefTable.CreateFrom(reader, tableStartAt + glyphClassDefOffset);
            this.AttachmentListTable = (attachListOffset == 0) ? null : AttachmentListTable.CreateFrom(reader, tableStartAt + attachListOffset);
            this.LigCaretList = (ligCaretListOffset == 0) ? null : LigCaretList.CreateFrom(reader, tableStartAt + ligCaretListOffset);

            //A Mark Attachment Class Definition Table defines the class to which a mark glyph may belong.
            //This table uses the same format as the Class Definition table (for details, see the chapter, Common Table Formats ).
            this.MarkAttachmentClassDef = (markAttachClassDefOffset == 0) ? null : ClassDefTable.CreateFrom(reader, tableStartAt + markAttachClassDefOffset);
            this.MarkGlyphSetsTable = (markGlyphSetsDefOffset == 0) ? null : MarkGlyphSetsTable.CreateFrom(reader, tableStartAt + markGlyphSetsDefOffset);

            if (itemVarStoreOffset != 0)
            {
                //not support
                throw new NotSupportedException();
                reader.BaseStream.Seek(this.Header.Offset + itemVarStoreOffset, SeekOrigin.Begin);
            }
        }
        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set; }
        public ClassDefTable GlyphClassDef { get; private set; }
        public AttachmentListTable AttachmentListTable { get; private set; }
        public LigCaretList LigCaretList { get; private set; }
        public ClassDefTable MarkAttachmentClassDef { get; private set; }
        public MarkGlyphSetsTable MarkGlyphSetsTable { get; private set; }

    }
}