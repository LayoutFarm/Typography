//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.IO;

namespace NRasterizer.Tables
{
    class HorizontalHeader
    {
        readonly UInt16 _numerOfHorizontalMetrics;

        public HorizontalHeader(BinaryReader input)
        {
            uint version = input.ReadUInt32();
            short ascender = input.ReadInt16();
            short descent = input.ReadInt16();
            short lineGap = input.ReadInt16();

            ushort advanceWidthMax = input.ReadUInt16();
            short minLeftSideBearing = input.ReadInt16();
            short minRightSideBearing = input.ReadInt16();
            short maxXExtent = input.ReadInt16();
            short caretSlopeRise = input.ReadInt16();
            short caretSlopeRun = input.ReadInt16();
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            Reserved(input.ReadInt16());
            short metricDataFormat = input.ReadInt16(); // 0
            _numerOfHorizontalMetrics = input.ReadUInt16();
        }

        public UInt16 HorizontalMetricsCount
        {
            get { return _numerOfHorizontalMetrics; }
        }

        void Reserved(short zero)
        {
            // should be zero
        }

        public static HorizontalHeader From(TableEntry table)
        {
            return new HorizontalHeader(table.GetDataReader());
        }
    }
}
