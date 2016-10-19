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

    
}