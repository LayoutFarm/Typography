//MIT, 2019-present, WinterDev
using System;
using System.IO;

namespace Typography.OpenFont.Tables
{
    //https://docs.microsoft.com/en-us/typography/opentype/spec/cvar

    /// <summary>
    /// cvar — CVT Variations Table
    /// </summary>
    class CVar : TableEntry
    {
        public const string _N = "cvar";
        public override string Name => _N; 
        public CVar()
        {

        }
        protected override void ReadContentFrom(BinaryReader reader)
        {

        }
    }
}