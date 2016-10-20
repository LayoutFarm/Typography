//Apache2,  2016, WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NRasterizer.Tables
{

    class FeatureList
    {

        FeatureRecord[] featureRecords;
        public static FeatureList CreateFrom(BinaryReader reader, long begin)
        {
            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //FeatureList table         
            //Type 	Name 	Description
            //USHORT 	FeatureCount 	Number of FeatureRecords in this table
            //struct 	FeatureRecord[FeatureCount] 	Array of FeatureRecords-zero-based (first feature has FeatureIndex = 0)-listed alphabetically by FeatureTag
            //FeatureRecord
            //Type 	Name 	Description
            //Tag 	FeatureTag 	4-byte feature identification tag
            //Offset 	Feature 	Offset to Feature table-from beginning of FeatureList
            reader.BaseStream.Seek(begin, SeekOrigin.Begin);
            //
            FeatureList featureList = new FeatureList();
            ushort featureCount = reader.ReadUInt16();
            FeatureRecord[] featureRecords = featureList.featureRecords = new FeatureRecord[featureCount];
            for (int i = 0; i < featureCount; ++i)
            {
                //read script record
                featureRecords[i] = new FeatureRecord(
                    reader.ReadUInt32(), //script tag
                    reader.ReadInt16()); //offset
            }

            return featureList;
        }
        struct FeatureRecord
        {
            public readonly uint scriptTag;//4-byte ScriptTag identifier
            public readonly short offset; //Script Offset to Script table-from beginning of ScriptList
            public FeatureRecord(uint scriptTag, short offset)
            {
                this.scriptTag = scriptTag;
                this.offset = offset;
            }
            public string ScriptName
            {
                get { return Utils.TagToString(scriptTag); }
            }
#if DEBUG
            public override string ToString()
            {
                return ScriptName + "," + offset;
            }
#endif
        }

    }
}