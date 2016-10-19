//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{

    abstract class LookupSubTable
    {
        public int Format
        {
            get;
            protected set;
        }
        public ushort CoverageOffset
        {
            get;
            protected set;
        }
        public CoverageTable CoverageTable
        {
            get;
            set;
        }

    }

    /// <summary>
    ///  for lookup table type 1, format1
    /// </summary>
    class LookupSubTableT1F1 : LookupSubTable
    {
        public LookupSubTableT1F1(ushort coverageOffset, short deltaGlyph)
        {
            this.Format = 1;
            this.CoverageOffset = coverageOffset;
            this.DeltaGlyph = deltaGlyph;
        }
        public short DeltaGlyph
        {
            //format1
            get;
            private set;
        }
    }
    /// <summary>
    /// for lookup table type 1, format2
    /// </summary>
    class LookupSubTableT1F2 : LookupSubTable
    {
        public LookupSubTableT1F2(ushort coverageOffset, ushort[] substitueGlyphs)
        {
            this.Format = 2;
            this.CoverageOffset = coverageOffset;
            this.SubstitueGlyphs = substitueGlyphs;
        }
        public ushort[] SubstitueGlyphs
        {
            get;
            private set;
        }
    }
}