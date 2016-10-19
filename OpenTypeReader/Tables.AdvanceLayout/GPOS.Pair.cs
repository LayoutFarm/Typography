//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//https://www.microsoft.com/typography/otspec/GPOS.htm

namespace NRasterizer.Tables
{
    partial class GPOS
    {
        class PairSetTable
        {
            public List<PairSet> pairSets = new List<PairSet>();

        }
        struct PairSet
        {
            public readonly ushort secondGlyph;//GlyphID of second glyph in the pair-first glyph is listed in the Coverage table
            public readonly ValueRecord value1;//Positioning data for the first glyph in the pair
            public readonly ValueRecord value2;//Positioning data for the second glyph in the pair   
            public PairSet(ushort secondGlyph, ValueRecord v1, ValueRecord v2)
            {
                this.secondGlyph = secondGlyph;
                this.value1 = v1;
                this.value2 = v2;
            }
        }
    }
}