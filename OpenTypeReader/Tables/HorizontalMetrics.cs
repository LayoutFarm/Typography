//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
namespace NRasterizer.Tables
{
    class HorizontalMetrics : TableEntry
    {
        List<ushort> _advanceWidths;
        List<short> _leftSideBearings;
        int _count;
        int _numGlyphs;
        public HorizontalMetrics(UInt16 count, UInt16 numGlyphs)
        {
            _advanceWidths = new List<ushort>(numGlyphs);
            _leftSideBearings = new List<short>(numGlyphs);
            _count = count;
            _numGlyphs = numGlyphs;
        }
        public override string Name
        {
            get { return "hmtx"; }
        }
        public ushort GetAdvanceWidth(int index)
        {
            return _advanceWidths[index];
        }
        protected override void ReadContentFrom(BinaryReader input)
        {
            int count = _count;
            int numGlyphs = _numGlyphs;
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
    }
}
