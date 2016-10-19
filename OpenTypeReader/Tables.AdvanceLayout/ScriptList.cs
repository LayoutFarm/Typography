//Apache2,  2016, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{

    class ScriptList
    {
        List<ScriptRecord> scriptRecords = new List<ScriptRecord>();

        struct ScriptRecord
        {
            public readonly uint scriptTag;//4-byte ScriptTag identifier
            public readonly ushort offset; //Script Offset to Script table-from beginning of ScriptList
            public ScriptRecord(uint scriptTag, ushort offset)
            {
                this.scriptTag = scriptTag;
                this.offset = offset;
            }
            public string ScriptName
            {
                get { return Utils.TagToString(scriptTag); }
            }
            public override string ToString()
            {
                return ScriptName + "," + offset;
            }
        }

        public void ReadFrom(BinaryReader reader)
        {
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //ScriptList table
            //Type 	Name 	Description
            //USHORT 	ScriptCount 	Number of ScriptRecords
            //struct 	ScriptRecord
            //[ScriptCount] 	Array of ScriptRecords
            //-listed alphabetically by ScriptTag
            //ScriptRecord
            //Type 	Name 	Description
            //Tag 	ScriptTag 	4-byte ScriptTag identifier
            //Offset 	Script 	Offset to Script table-from beginning of ScriptList
            scriptRecords.Clear();
            ushort scriptCount = reader.ReadUInt16();
            for (int i = 0; i < scriptCount; ++i)
            {
                //read script record
                scriptRecords.Add(new ScriptRecord(
                    reader.ReadUInt32(),
                    reader.ReadUInt16()));
            }
        }
    }




}