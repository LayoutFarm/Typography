using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace NRasterizer.Tables
{
    public class MaxProfile
    {
        private ushort _gylphCount;

        private MaxProfile(BinaryReader input)
        {
            var version = input.ReadUInt32(); // 0x00010000 == 1.0
            _gylphCount = input.ReadUInt16();
            var maxPointsPerGlyph = input.ReadUInt16();
            var maxContoursPerGlyph = input.ReadUInt16();
            var maxPointsPerCompositeGlyph = input.ReadUInt16();
            var maxContoursPerCompositeGlyph = input.ReadUInt16();
            var maxZones = input.ReadUInt16();
            var maxTwilightPoints = input.ReadUInt16();
            var maxStorage = input.ReadUInt16();
            var maxFunctionDefs = input.ReadUInt16();
            var maxInstructionDefs = input.ReadUInt16();
            var maxStackElements = input.ReadUInt16();
            var maxSizeOfInstructions = input.ReadUInt16();
            var maxComponentElements = input.ReadUInt16();
            var maxComponentDepth = input.ReadUInt16();
        }

        public ushort GlyphCount { get { return _gylphCount; } }

        internal static MaxProfile From(TableEntry table)
        {
            return new MaxProfile(table.GetDataReader());
        }

    }
}
