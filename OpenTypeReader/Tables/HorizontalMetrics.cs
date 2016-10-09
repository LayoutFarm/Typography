//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
namespace NRasterizer.Tables
{
    class HorizontalMetrics
    {
        readonly List<ushort> _advanceWidths;
        readonly List<short> _leftSideBearings;

        private HorizontalMetrics(BinaryReader input, UInt16 count, UInt16 numGlyphs)
        {
            _advanceWidths = new List<ushort>(numGlyphs);
            _leftSideBearings = new List<short>(numGlyphs);

            for (int i = 0; i < count; i++)
            {
                _advanceWidths.Add(input.ReadUInt16());
                _leftSideBearings.Add(input.ReadInt16());
            }

            ushort advanceWidth = _advanceWidths[count - 1];
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
