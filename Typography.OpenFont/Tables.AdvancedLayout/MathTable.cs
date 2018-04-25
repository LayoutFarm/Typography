
//MIT, 2018, WinterDev
using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{

    //https://www.microsoft.com/typography/otspec/math.htm
    class MathTable : TableEntry
    {

        public override string Name
        {
            get { return "MATH"; }
        }
        protected override void ReadContentFrom(BinaryReader reader)
        {
            //eg. latin-modern-math-regular.otf, asana-math.otf

            long beginAt = reader.BaseStream.Position;
            //math table header
            //Type          Name    Description
            //uint16        MajorVersion Major version of the MATH table, = 1.
            //uint16        MinorVersion    Minor version of the MATH table, = 0.
            //Offset16      MathConstants   Offset to MathConstants table -from the beginning of MATH table.
            //Offset16      MathGlyphInfo   Offset to MathGlyphInfo table -from the beginning of MATH table.
            //Offset16      MathVariants    Offset to MathVariants table -from the beginning of MATH table.

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            ushort mathConstants_offset = reader.ReadUInt16();
            ushort mathGlyphInfo_offset = reader.ReadUInt16();
            ushort mathVariants_offset = reader.ReadUInt16();
            //---------------------------------

            reader.BaseStream.Position = beginAt + mathConstants_offset;
            ReadMathConstantsTable(reader);
            //
            reader.BaseStream.Position = beginAt + mathGlyphInfo_offset;
            ReadMathMathGlyphInfoTable(reader);
            //
            reader.BaseStream.Position = beginAt + mathVariants_offset;
            ReadMathMathVariantsTable(reader);
        }
        void ReadMathConstantsTable(BinaryReader reader)
        {

        }
        void ReadMathMathGlyphInfoTable(BinaryReader reader)
        {

        }
        void ReadMathMathVariantsTable(BinaryReader reader)
        {


        }
    }


    //MathValueRecord
    //Type      Name            Description
    //int16     Value           The X or Y value in design units
    //Offset16  DeviceTable     Offset to the device table – from the beginning of parent table.May be NULL. Suggested format for device table is 1.
    struct MathValueRecord
    {
        public short Value;
        public ushort DeviceTable;
    }

    class MathConstantsTable
    {

    }
    class MathGlyphInfoTable
    {

    }
    class MathItalicsCorrectonInfoTable
    {
    }
    class MathTopAccentAttachmentTable
    {

    }
    class ExtendedShapeCoverageTable
    {

    }
    class MathKernInfoTable
    {

    }
    class MathKernInfoRecordTable { }

    class MathKernTable { }

    class MathVariantsTable { }

    class MathGlyphConstructionTable { }
    class GlyphAssemblyTable { }
}
