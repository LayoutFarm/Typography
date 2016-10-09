//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System.IO;
namespace NRasterizer.Tables
{
    public class MaxProfile
    {
        ushort _gylphCount;

        private MaxProfile(BinaryReader input)
        {
            uint version = input.ReadUInt32(); // 0x00010000 == 1.0
            _gylphCount = input.ReadUInt16();
            ushort maxPointsPerGlyph = input.ReadUInt16();
            ushort maxContoursPerGlyph = input.ReadUInt16();
            ushort maxPointsPerCompositeGlyph = input.ReadUInt16();
            ushort maxContoursPerCompositeGlyph = input.ReadUInt16();
            ushort maxZones = input.ReadUInt16();
            ushort maxTwilightPoints = input.ReadUInt16();
            ushort maxStorage = input.ReadUInt16();
            ushort maxFunctionDefs = input.ReadUInt16();
            ushort maxInstructionDefs = input.ReadUInt16();
            ushort maxStackElements = input.ReadUInt16();
            ushort maxSizeOfInstructions = input.ReadUInt16();
            ushort maxComponentElements = input.ReadUInt16();
            ushort maxComponentDepth = input.ReadUInt16();
        }

        public ushort GlyphCount { get { return _gylphCount; } }

        internal static MaxProfile From(TableEntry table)
        {
            return new MaxProfile(table.GetDataReader());
        }

    }
}
