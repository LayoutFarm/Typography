//Apache2, 2016-present, WinterDev

using System.Collections.Generic;
using System.IO;

namespace Typography.OpenFont.Tables
{
    public class ScriptList : Dictionary<string, ScriptTable>
    {
        // https://www.microsoft.com/typography/otspec/chapter2.htm
        // The ScriptList identifies the scripts in a font,
        // each of which is represented by a Script table that contains script and language-system data.
        // Language system tables reference features, which are defined in the FeatureList.
        // Each feature table references the lookup data defined in the LookupList that describes how, when, and where to implement the feature.
        private ScriptList() { }
        public new ScriptTable? this[string tagName]
        {
            get { return TryGetValue(tagName, out ScriptTable? ret) ? ret : null; }
        }

        public static ScriptList CreateFrom(BinaryReader reader, long beginAt)
        {
            // https://www.microsoft.com/typography/otspec/chapter2.htm
            //
            // ScriptList table
            // Type    Name                      Description
            // uint16  ScriptCount               Number of ScriptRecords
            // struct  ScriptRecord[ScriptCount] Array of ScriptRecords
            //                                   -listed alphabetically by ScriptTag
            // ScriptRecord
            // Type      Name       Description
            // Tag       ScriptTag  4-byte ScriptTag identifier
            // Offset16  Script     Offset to Script table-from beginning of ScriptList

            reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);
            ushort scriptCount = reader.ReadUInt16();

            ScriptList scriptList = new ScriptList();

            // Read records (tags and table offsets)
            uint[] scriptTags = new uint[scriptCount];
            ushort[] scriptOffsets = new ushort[scriptCount];
            for (int i = 0; i < scriptCount; ++i)
            {
                scriptTags[i] = reader.ReadUInt32();
                scriptOffsets[i] = reader.ReadUInt16();
            }

            // Read each table and add it to the dictionary
            for (int i = 0; i < scriptCount; ++i)
            {
                ScriptTable scriptTable = ScriptTable.CreateFrom(reader, beginAt + scriptOffsets[i], scriptTags[i]);

                scriptList.Add(Utils.TagToString(scriptTags[i]), scriptTable);
            }

            return scriptList;
        }
    }
}
