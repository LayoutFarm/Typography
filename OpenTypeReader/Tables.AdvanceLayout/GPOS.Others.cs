//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//https://www.microsoft.com/typography/otspec/GPOS.htm

namespace NRasterizer.Tables
{
    partial class GPOS
    {


        class PairSetTable
        {
            List<PairSet> pairSets = new List<PairSet>();
            public void ReadFrom(BinaryReader reader, ushort v1format, ushort v2format)
            {
                ushort rowCount = reader.ReadUInt16();
                for (int i = 0; i < rowCount; ++i)
                {
                    //GlyphID 	SecondGlyph 	GlyphID of second glyph in the pair-first glyph is listed in the Coverage table
                    //ValueRecord 	Value1 	Positioning data for the first glyph in the pair
                    //ValueRecord 	Value2 	Positioning data for the second glyph in the pair
                    ushort secondGlyp = reader.ReadUInt16();
                    ValueRecord v1 = ValueRecord.CreateFrom(reader, v1format);
                    ValueRecord v2 = ValueRecord.CreateFrom(reader, v2format);
                    PairSet pset = new PairSet(secondGlyp, v1, v2);
                }
            }
        }


        struct PairSet
        {
            public readonly ushort secondGlyph;//GlyphID of second glyph in the pair-first glyph is listed in the Coverage table
            public readonly ValueRecord value1;//Positioning data for the first glyph in the pair
            public readonly ValueRecord value2;//Positioning data for the second glyph in the pair   
            public PairSet(ushort secondGlyph, ValueRecord v1, ValueRecord v2)
            {
                this.secondGlyph = secondGlyph;
                this.value1 = v1;
                this.value2 = v2;
            }
        }


        class ValueRecord
        {
            //ValueRecord (all fields are optional)
            //Value 	Type 	Description
            //SHORT 	XPlacement 	Horizontal adjustment for placement-in design units
            //SHORT 	YPlacement 	Vertical adjustment for placement, in design units
            //SHORT 	XAdvance 	Horizontal adjustment for advance, in design units (only used for horizontal writing)
            //SHORT 	YAdvance 	Vertical adjustment for advance, in design units (only used for vertical writing)
            //Offset 	XPlaDevice 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal placement, from beginning of PosTable (may be NULL)
            //Offset 	YPlaDevice 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical placement, from beginning of PosTable (may be NULL)
            //Offset 	XAdvDevice 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for horizontal advance, from beginning of PosTable (may be NULL)
            //Offset 	YAdvDevice 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for vertical advance, from beginning of PosTable (may be NULL)

            public short XPlacement;
            public short YPlacement;
            public short XAdvance;
            public short YAdvance;
            public short XPlaDevice;
            public short YPlaDevice;
            public short XAdvDevice;
            public short YAdvDevice;

            public ushort valueFormat;
            public void ReadFrom(BinaryReader reader, ushort valueFormat)
            {
                this.valueFormat = valueFormat;
                if (HasFormat(valueFormat, FMT_XPlacement))
                {
                    this.XPlacement = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_YPlacement))
                {
                    this.YPlacement = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_XAdvance))
                {
                    this.XAdvance = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_YAdvance))
                {
                    this.YAdvance = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_XPlaDevice))
                {
                    this.XPlaDevice = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_YPlaDevice))
                {
                    this.YPlaDevice = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_XAdvDevice))
                {
                    this.XAdvDevice = reader.ReadInt16();
                }
                if (HasFormat(valueFormat, FMT_YAdvDevice))
                {
                    this.YAdvDevice = reader.ReadInt16();
                }
            }
            static bool HasFormat(ushort value, int flags)
            {
                return (value & flags) == flags;
            }
            //Mask 	Name 	Description
            //0x0001 	XPlacement 	Includes horizontal adjustment for placement
            //0x0002 	YPlacement 	Includes vertical adjustment for placement
            //0x0004 	XAdvance 	Includes horizontal adjustment for advance
            //0x0008 	YAdvance 	Includes vertical adjustment for advance
            //0x0010 	XPlaDevice 	Includes Device table (non-variable font) / VariationIndex table (variable font) for horizontal placement
            //0x0020 	YPlaDevice 	Includes Device table (non-variable font) / VariationIndex table (variable font) for vertical placement
            //0x0040 	XAdvDevice 	Includes Device table (non-variable font) / VariationIndex table (variable font) for horizontal advance
            //0x0080 	YAdvDevice 	Includes Device table (non-variable font) / VariationIndex table (variable font) for vertical advance
            //0xFF00 	Reserved 	For future use (set to zero)

            //check bits
            const int FMT_XPlacement = 1;
            const int FMT_YPlacement = 1 << 1;
            const int FMT_XAdvance = 1 << 2;
            const int FMT_YAdvance = 1 << 3;
            const int FMT_XPlaDevice = 1 << 4;
            const int FMT_YPlaDevice = 1 << 5;
            const int FMT_XAdvDevice = 1 << 6;
            const int FMT_YAdvDevice = 1 << 7;

            public static ValueRecord CreateFrom(BinaryReader reader, ushort valueFormat)
            {
                var v = new ValueRecord();
                v.ReadFrom(reader, valueFormat);
                return v;
            }
        }


        class AnchorTable
        {
            //Anchor Table

            //A GPOS table uses anchor points to position one glyph with respect to another.
            //Each glyph defines an anchor point, and the text-processing client attaches the glyphs by aligning their corresponding anchor points.

            //To describe an anchor point, an Anchor table can use one of three formats. 
            //The first format uses design units to specify a location for the anchor point.
            //The other two formats refine the location of the anchor point using contour points (Format 2) or Device tables (Format 3). 
            //In a variable font, the third format uses a VariationIndex table (a variant of a Device table) to 
            //reference variation data for adjustment of the anchor position for the current variation instance, as needed. 

            ushort format;
            public void ReadFrom(BinaryReader reader)
            {
                long anchorTableStartAt = reader.BaseStream.Position;

                switch (this.format = reader.ReadUInt16())
                {
                    default: throw new NotFiniteNumberException();
                    case 1:
                        {
                            // AnchorFormat1 table: Design units only
                            //AnchorFormat1 consists of a format identifier (AnchorFormat) and a pair of design unit coordinates (XCoordinate and YCoordinate)
                            //that specify the location of the anchor point. 
                            //This format has the benefits of small size and simplicity,
                            //but the anchor point cannot be hinted to adjust its position for different device resolutions.
                            //Value 	Type 	Description
                            //USHORT 	AnchorFormat 	Format identifier, = 1
                            //SHORT 	XCoordinate 	Horizontal value, in design units
                            //SHORT 	YCoordinate 	Vertical value, in design units
                            short xcoord = reader.ReadInt16();
                            short ycoord = reader.ReadInt16();

                        } break;
                    case 2:
                        {
                            //Anchor Table: Format 2

                            //Like AnchorFormat1, AnchorFormat2 specifies a format identifier (AnchorFormat) and
                            //a pair of design unit coordinates for the anchor point (Xcoordinate and Ycoordinate).

                            //For fine-tuning the location of the anchor point, AnchorFormat2 also provides an index to a glyph contour point (AnchorPoint) 
                            //that is on the outline of a glyph (AnchorPoint).
                            //Hinting can be used to move the AnchorPoint. In the rendered text,
                            //the AnchorPoint will provide the final positioning data for a given ppem size.

                            //Example 16 at the end of this chapter uses AnchorFormat2.


                            //AnchorFormat2 table: Design units plus contour point
                            //Value 	Type 	Description
                            //USHORT 	AnchorFormat 	Format identifier, = 2
                            //SHORT 	XCoordinate 	Horizontal value, in design units
                            //SHORT 	YCoordinate 	Vertical value, in design units
                            //USHORT 	AnchorPoint 	Index to glyph contour point

                            short xcoord = reader.ReadInt16();
                            short ycoord = reader.ReadInt16();
                            ushort anchorPoint = reader.ReadUInt16();


                        } break;
                    case 3:
                        {

                            //Anchor Table: Format 3

                            //Like AnchorFormat1, AnchorFormat3 specifies a format identifier (AnchorFormat) and 
                            //locates an anchor point (Xcoordinate and Ycoordinate).
                            //And, like AnchorFormat 2, it permits fine adjustments in variable fonts to the coordinate values. 
                            //However, AnchorFormat3 uses Device tables, rather than a contour point, for this adjustment.

                            //With a Device table, a client can adjust the position of the anchor point for any font size and device resolution.
                            //AnchorFormat3 can specify offsets to Device tables for the the X coordinate (XDeviceTable) 
                            //and the Y coordinate (YDeviceTable). 
                            //If only one coordinate requires adjustment, 
                            //the offset to the Device table may be set to NULL for the other coordinate.

                            //In variable fonts, AnchorFormat3 must be used to reference variation data to adjust anchor points for different variation instances,
                            //if needed.
                            //In this case, AnchorFormat3 specifies an offset to a VariationIndex table,
                            //which is a variant of the Device table used for variations.
                            //If no VariationIndex table is used for a particular anchor point X or Y coordinate, 
                            //then that value is used for all variation instances.
                            //While separate VariationIndex table references are required for each value that requires variation,
                            //two or more values that require the same variation-data values can have offsets that point to the same VariationIndex table, and two or more VariationIndex tables can reference the same variation data entries.

                            //Example 17 at the end of the chapter shows an AnchorFormat3 table.


                            //AnchorFormat3 table: Design units plus Device or VariationIndex tables
                            //Value 	Type 	Description
                            //USHORT 	AnchorFormat 	Format identifier, = 3
                            //SHORT 	XCoordinate 	Horizontal value, in design units
                            //SHORT 	YCoordinate 	Vertical value, in design units
                            //Offset 	XDeviceTable 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for X coordinate, from beginning of Anchor table (may be NULL)
                            //Offset 	YDeviceTable 	Offset to Device table (non-variable font) / VariationIndex table (variable font) for Y coordinate, from beginning of Anchor table (may be NULL)

                            short xcoord = reader.ReadInt16();
                            short ycoord = reader.ReadInt16();
                            short xdeviceTableOffset = reader.ReadInt16();
                            short ydeviceTableOffset = reader.ReadInt16();


                        } break;
                }

            }
        }


        class MarkArrayTable
        {
            //Mark Array
            //The MarkArray table defines the class and the anchor point for a mark glyph. 
            //Three GPOS subtables-MarkToBase, MarkToLigature, 
            //and MarkToMark Attachment-use the MarkArray table to specify data for attaching marks.

            //The MarkArray table contains a count of the number of mark records (MarkCount) and an array of those records (MarkRecord).
            //Each mark record defines the class of the mark and an offset to the Anchor table that contains data for the mark.

            //A class value can be 0 (zero), but the MarkRecord must explicitly assign that class value (this differs from the ClassDef table, 
            //in which all glyphs not assigned class values automatically belong to Class 0).
            //The GPOS subtables that refer to MarkArray tables use the class assignments for indexing zero-based arrays that contain data for each mark class.

            // MarkArray table
            //Value 	Type 	Description
            //USHORT 	MarkCount 	Number of MarkRecords
            //struct 	MarkRecord
            //[MarkCount] 	Array of MarkRecords-in Coverage order
            //MarkRecord
            //Value 	Type 	Description
            //USHORT 	Class 	Class defined for this mark
            //Offset 	MarkAnchor 	Offset to Anchor table-from beginning of MarkArray table
            MarkRecord[] records;
            public void ReadFrom(BinaryReader reader)
            {
                ushort markCount = reader.ReadUInt16();
                records = new MarkRecord[markCount];
                for (int i = 0; i < markCount; ++i)
                {
                    records[i] = new MarkRecord(
                        reader.ReadUInt16(),
                        reader.ReadInt16());
                }
            }
        }

        struct MarkRecord
        {
            /// <summary>
            /// Class defined for this mark
            /// </summary>
            public readonly ushort markClass;
            /// <summary>
            /// Offset to Anchor table-from beginning of MarkArray table
            /// </summary>
            public readonly short offset;
            public MarkRecord(ushort markClass, short offset)
            {
                this.markClass = markClass;
                this.offset = offset;
            }
#if DEBUG
            public override string ToString()
            {
                return "class " + markClass + ",offset=" + offset;
            }
#endif
        }

        class Mark2ArrayTable
        {
            ///Mark2Array table
            //Value 	Type 	Description
            //USHORT 	Mark2Count 	Number of Mark2 records
            //struct 	Mark2Record
            //[Mark2Count] 	Array of Mark2 records-in Coverage order

            //Each Mark2Record contains an array of offsets to Anchor tables (Mark2Anchor). The array of zero-based offsets, measured from the beginning of the Mark2Array table, defines the entire set of Mark2 attachment points used to attach Mark1 glyphs to a specific Mark2 glyph. The Anchor tables in the Mark2Anchor array are ordered by Mark1 class value.

            //A Mark2Record declares one Anchor table for each mark class (including Class 0) identified in the MarkRecords of the MarkArray. Each Anchor table specifies one Mark2 attachment point used to attach all the Mark1 glyphs in a particular class to the Mark2 glyph.

            Mark2Record[] mark2Records;
            public void ReadFrom(BinaryReader reader, ushort classCount)
            {
                ushort mark2Count = reader.ReadUInt16();
                mark2Records = new Mark2Record[mark2Count];
                for (int i = 0; i < mark2Count; ++i)
                {
                    mark2Records[i] = new Mark2Record(
                        Utils.ReadInt16Array(reader, classCount));
                }
            }
        }

        struct Mark2Record
        {
            //Mark2Record
            //Value 	Type 	Description
            //Offset 	Mark2Anchor
            //[ClassCount] 	Array of offsets (one per class) to Anchor tables-from beginning of Mark2Array table-zero-based array
            public readonly short[] offsets;
            public Mark2Record(short[] offsets)
            {
                this.offsets = offsets;
            }
        }


        class BaseArrayTable
        {
            BaseRecord[] records;
            public void ReadFrom(BinaryReader reader, ushort classCount)
            {
                ushort baseCount = reader.ReadUInt16();
                records = new BaseRecord[baseCount];
                for (int i = 0; i < baseCount; ++i)
                {
                    records[i] = new BaseRecord(Utils.ReadInt16Array(reader, classCount));
                }
            }
        }
        struct BaseRecord
        {
            public short[] offsets;
            public BaseRecord(short[] offsets)
            {
                this.offsets = offsets;
            }

        }
        class Class1Record
        {

        }
        class Class2Record
        {

        }


        // LigatureArray table
        //Value 	Type 	Description
        //USHORT 	LigatureCount 	Number of LigatureAttach table offsets
        //Offset 	LigatureAttach
        //[LigatureCount] 	Array of offsets to LigatureAttach tables-from beginning of LigatureArray table-ordered by LigatureCoverage Index

        //Each LigatureAttach table consists of an array (ComponentRecord) and count (ComponentCount) of the component glyphs in a ligature. The array stores the ComponentRecords in the same order as the components in the ligature. The order of the records also corresponds to the writing direction of the text. For text written left to right, the first component is on the left; for text written right to left, the first component is on the right.
        //LigatureAttach table
        //Value 	Type 	Description
        //USHORT 	ComponentCount 	Number of ComponentRecords in this ligature
        //struct 	ComponentRecord[ComponentCount] 	Array of Component records-ordered in writing direction

        //A ComponentRecord, one for each component in the ligature, contains an array of offsets to the Anchor tables that define all the attachment points used to attach marks to the component (LigatureAnchor). For each mark class (including Class 0) identified in the MarkArray records, an Anchor table specifies the point used to attach all the marks in a particular class to the ligature base glyph, relative to the component.

        //In a ComponentRecord, the zero-based LigatureAnchor array lists offsets to Anchor tables by mark class. If a component does not define an attachment point for a particular class of marks, then the offset to the corresponding Anchor table will be NULL.

        //Example 8 at the end of this chapter shows a MarkLisPosFormat1 subtable used to attach mark accents to a ligature glyph in the Arabic script.
        //ComponentRecord
        //Value 	Type 	Description
        //Offset 	LigatureAnchor
        //[ClassCount] 	Array of offsets (one per class) to Anchor tables-from beginning of LigatureAttach table-ordered by class-NULL if a component does not have an attachment for a class-zero-based array
        class LigatureArrayTable
        {
            LigatureAttachTable[] ligatures;
            public void ReadFrom(BinaryReader reader, ushort classCount)
            {
                long startPos = reader.BaseStream.Position;
                ushort ligatureCount = reader.ReadUInt16();
                short[] offsets = Utils.ReadInt16Array(reader, ligatureCount);

                ligatures = new LigatureAttachTable[ligatureCount];

                for (int i = 0; i < ligatureCount; ++i)
                {
                    //each ligature table
                    reader.BaseStream.Seek(startPos + offsets[i], SeekOrigin.Begin);
                    ligatures[i] = LigatureAttachTable.ReadFrom(reader, classCount);
                }
            }
        }
        class LigatureAttachTable
        {
            ComponentRecord[] records;
            public static LigatureAttachTable ReadFrom(BinaryReader reader, ushort classCount)
            {
                LigatureAttachTable table = new LigatureAttachTable();
                ushort componentCount = reader.ReadUInt16();
                ComponentRecord[] componentRecs = new ComponentRecord[componentCount];
                table.records = componentRecs;
                for (int i = 0; i < componentCount; ++i)
                {
                    componentRecs[i] = new ComponentRecord(
                        Utils.ReadInt16Array(reader, classCount));
                }
                return table;
            }

        }
        struct ComponentRecord
        {
            public short[] offsets;
            public ComponentRecord(short[] offsets)
            {
                this.offsets = offsets;
            }

        }
    }

}