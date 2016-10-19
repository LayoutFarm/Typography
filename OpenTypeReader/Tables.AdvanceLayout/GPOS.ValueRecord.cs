//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{
    partial class GPOS
    {
        //https://www.microsoft.com/typography/otspec/GPOS.htm

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
    }
}