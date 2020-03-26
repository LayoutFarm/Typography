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
        public const string Name = "cvar";
        internal CVar(TableHeader header, BinaryReader reader) : base(header, reader)
        {

        }
    }
}