
//MIT, 2019-present, WinterDev
using System;
using System.IO;

namespace Typography.OpenFont.Tables
{

    //https://docs.microsoft.com/en-us/typography/opentype/spec/mvar

    /// <summary>
    /// MVAR — Metrics Variations Table
    /// </summary>
    class MVar : TableEntry
    {
        public const string _N = "MVAR";
        public override string Name => _N;
        public MVar()
        {

        }
        protected override void ReadContentFrom(BinaryReader reader)
        {

        }
    }
}