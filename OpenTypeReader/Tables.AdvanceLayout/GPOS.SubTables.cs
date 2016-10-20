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
        }

    }
}