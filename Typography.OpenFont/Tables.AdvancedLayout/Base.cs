//Apache2, 2016-present, WinterDev
//https://www.microsoft.com/typography/otspec/base.htm
//BASE - Baseline Table
//The Baseline table (BASE) provides information used to align glyphs of different scripts and sizes in a line of text, 
//whether the glyphs are in the same font or in different fonts.
//To improve text layout, the Baseline table also provides minimum (min) and maximum (max) glyph extent values for each script,
//language system, or feature in a font.

using System.IO;

namespace Typography.OpenFont.Tables
{
    class BASE : TableEntry
    {
        public const string _N = "BASE";
        public override string Name => _N;

        protected override void ReadContentFrom(BinaryReader reader)
        {
            //BASE Header

            //The BASE table begins with a header that starts with a version number.
            //Two versions are defined. 
            //Version 1.0 contains offsets to horizontal and vertical Axis tables(HorizAxis and VertAxis). 
            //Version 1.1 also includes an offset to an Item Variation Store table.

            //Each Axis table stores all baseline information and min / max extents for one layout direction.
            //The HorizAxis table contains Y values for horizontal text layout;
            //the VertAxis table contains X values for vertical text layout.


            // A font may supply information for both layout directions.
            //If a font has values for only one text direction, 
            //the Axis table offset value for the other direction will be set to NULL.

            //The optional Item Variation Store table is used in variable fonts to provide variation data 
            //for BASE metric values within the Axis tables.


            // BASE Header, Version 1.0
            //Type      Name                Description
            //uint16    majorVersion        Major version of the BASE table, = 1
            //uint16    minorVersion        Minor version of the BASE table, = 0
            //Offset16  horizAxisOffset     Offset to horizontal Axis table, from beginning of BASE table(may be NULL)
            //Offset16  vertAxisOffset      Offset to vertical Axis table, from beginning of BASE table(may be NULL)

            //BASE Header, Version 1.1
            //Type      Name                Description
            //uint16    majorVersion        Major version of the BASE table, = 1
            //uint16    minorVersion        Minor version of the BASE table, = 1
            //Offset16  horizAxisOffset     Offset to horizontal Axis table, from beginning of BASE table(may be NULL)
            //Offset16  vertAxisOffset      Offset to vertical Axis table, from beginning of BASE table(may be NULL)
            //Offset32  itemVarStoreOffset  Offset to Item Variation Store table, from beginning of BASE table(may be null)


            long tableStartAt = reader.BaseStream.Position;

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            ushort horizAxisOffset = reader.ReadUInt16();
            ushort vertAxisOffset = reader.ReadUInt16();
            uint itemVarStoreOffset = 0;
            if (minorVersion == 1)
            {
                itemVarStoreOffset = reader.ReadUInt32();
            }

            ///....TODO: developing ....
        }
    }
}