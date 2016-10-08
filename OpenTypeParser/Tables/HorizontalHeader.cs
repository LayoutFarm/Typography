using System;
using System.IO;

namespace NRasterizer.Tables
{
    internal class HorizontalHeader
    {
        private readonly UInt16 _numerOfHorizontalMetrics;

        public HorizontalHeader(BinaryReader input)
        {
            var version = input.ReadUInt32();
            var ascender = input.ReadInt16();
            var descent = input.ReadInt16();
            var lineGap = input.ReadInt16();

            var advanceWidthMax = input.ReadUInt16();
            var minLeftSideBearing = input.ReadInt16();
            var minRightSideBearing = input.ReadInt16();
            var maxXExtent = input.ReadInt16();
            var caretSlopeRise = input.ReadInt16();
            var caretSlopeRun = input.ReadInt16();
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            var metricDataFormat = input.ReadInt16(); // 0
            _numerOfHorizontalMetrics = input.ReadUInt16();
        }

        public UInt16 HorizontalMetricsCount
        {
            get { return _numerOfHorizontalMetrics; }            
        }

        private void Reserved(short zero)
        {
            // should be zero
        }

        public static HorizontalHeader From(TableEntry table)
        {
            return new HorizontalHeader(table.GetDataReader());
        }
    }
}
