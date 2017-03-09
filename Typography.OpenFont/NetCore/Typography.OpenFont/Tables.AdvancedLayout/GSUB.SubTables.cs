//Apache2, 2016-2017,  WinterDev

using System.Collections.Generic;
namespace Typography.OpenFont.Tables
{

    partial class GSUB : TableEntry
    {
        /// <summary>
        /// base class of lookup sub table
        /// </summary>
        public abstract class LookupSubTable
        {
            public abstract void DoSubtitution(List<ushort> glyphIndices, int startAt, int len);
            public GSUB OwnerGSub;
        }
    }
}