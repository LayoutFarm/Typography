using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NRasterizer.Tables
{
    internal class HorizontalMetrics
    {
        private readonly List<ushort> _advanceWidths;
        private readonly List<short> _leftSideBearings;

        private HorizontalMetrics(BinaryReader input, UInt16 count, UInt16 numGlyphs)
        {
            _advanceWidths = new List<ushort>(numGlyphs);
            _leftSideBearings = new List<short>(numGlyphs);

            for (int i = 0; i < count; i++)
            {
                _advanceWidths.Add(input.ReadUInt16());
                _leftSideBearings.Add(input.ReadInt16());
            }

            var advanceWidth = _advanceWidths.Last();
            for (int i = 0; i < numGlyphs - count; i++)
            {
                _advanceWidths.Add(advanceWidth);
                _leftSideBearings.Add(input.ReadInt16());
            }
        }

        public ushort GetAdvanceWidth(int index)
        {
            return _advanceWidths[index];
        }

        public static HorizontalMetrics From(TableEntry table, UInt16 count, UInt16 numGlyphs)
        {
            return new HorizontalMetrics(table.GetDataReader(), count, numGlyphs);
        }
    }
}
