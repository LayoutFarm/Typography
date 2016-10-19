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
            public List<PairSet> pairSets = new List<PairSet>();

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
    }
}