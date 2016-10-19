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

        class LookupType1SubTable : LookupSubTable
        {
            ValueRecord singleValue;
            ValueRecord[] multiValues;
            public LookupType1SubTable(ValueRecord singleValue)
            {
                this.Format = 1;
                this.singleValue = singleValue;
            }
            public LookupType1SubTable(ValueRecord[] valueRecords)
            {
                this.Format = 2;
                this.multiValues = valueRecords;
            }
        }
        class LookupType2SubTable : LookupSubTable
        {
            PairSetTable[] pairSetTables;
            public LookupType2SubTable(PairSetTable[] pairSetTables)
            {
                this.Format = 1;
                this.pairSetTables = pairSetTables;
            }

        }

    }
}