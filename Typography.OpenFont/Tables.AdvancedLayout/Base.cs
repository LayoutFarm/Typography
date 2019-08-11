//Apache2, 2016-present, WinterDev
//https://www.microsoft.com/typography/otspec/base.htm
//BASE - Baseline Table
//The Baseline table (BASE) provides information used to align glyphs of different scripts and sizes in a line of text, 
//whether the glyphs are in the same font or in different fonts.
//To improve text layout, the Baseline table also provides minimum (min) and maximum (max) glyph extent values for each script,
//language system, or feature in a font.

using System.IO;

namespace Typography.OpenFont.Tables
{
    class BASE : TableEntry
    {
        public const string _N = "BASE";
        public override string Name => _N;

        protected override void ReadContentFrom(BinaryReader reader)
        {
            //BASE Header

            //The BASE table begins with a header that starts with a version number.
            //Two versions are defined. 
            //Version 1.0 contains offsets to horizontal and vertical Axis tables(HorizAxis and VertAxis). 
            //Version 1.1 also includes an offset to an Item Variation Store table.

            //Each Axis table stores all baseline information and min / max extents for one layout direction.
            //The HorizAxis table contains Y values for horizontal text layout;
            //the VertAxis table contains X values for vertical text layout.


            // A font may supply information for both layout directions.
            //If a font has values for only one text direction, 
            //the Axis table offset value for the other direction will be set to NULL.

            //The optional Item Variation Store table is used in variable fonts to provide variation data 
            //for BASE metric values within the Axis tables.


            // BASE Header, Version 1.0
            //Type      Name                Description
            //uint16    majorVersion        Major version of the BASE table, = 1
            //uint16    minorVersion        Minor version of the BASE table, = 0
            //Offset16  horizAxisOffset     Offset to horizontal Axis table, from beginning of BASE table(may be NULL)
            //Offset16  vertAxisOffset      Offset to vertical Axis table, from beginning of BASE table(may be NULL)

            //BASE Header, Version 1.1
            //Type      Name                Description
            //uint16    majorVersion        Major version of the BASE table, = 1
            //uint16    minorVersion        Minor version of the BASE table, = 1
            //Offset16  horizAxisOffset     Offset to horizontal Axis table, from beginning of BASE table(may be NULL)
            //Offset16  vertAxisOffset      Offset to vertical Axis table, from beginning of BASE table(may be NULL)
            //Offset32  itemVarStoreOffset  Offset to Item Variation Store table, from beginning of BASE table(may be null)


            long tableStartAt = reader.BaseStream.Position;

            ushort majorVersion = reader.ReadUInt16();
            ushort minorVersion = reader.ReadUInt16();
            ushort horizAxisOffset = reader.ReadUInt16();
            ushort vertAxisOffset = reader.ReadUInt16();
            uint itemVarStoreOffset = 0;
            if (minorVersion == 1)
            {
                itemVarStoreOffset = reader.ReadUInt32();
            }

            //Axis Tables: HorizAxis and VertAxis 

            if (horizAxisOffset > 0)
            {
                reader.BaseStream.Position = tableStartAt + horizAxisOffset;
                ReadAxisTable(reader);

            }
            if (vertAxisOffset > 0)
            {
                reader.BaseStream.Position = tableStartAt + vertAxisOffset;
                ReadAxisTable(reader);
            }
            ///....TODO: developing ....
        }
        void ReadAxisTable(BinaryReader reader)
        {
            //An Axis table is used to render scripts either horizontally or vertically. 
            //It consists of offsets, measured from the beginning of the Axis table,
            //to a BaseTagList and a BaseScriptList:

            //The BaseScriptList enumerates all scripts rendered in the text layout direction.
            //The BaseTagList enumerates all baselines used to render the scripts in the text layout direction.
            //If no baseline data is available for a text direction,
            //the offset to the corresponding BaseTagList may be set to NULL.

            //Axis Table
            //Type        Name                      Description
            //Offset16    baseTagListOffset         Offset to BaseTagList table, from beginning of Axis table(may be NULL)
            //Offset16    baseScriptListOffset      Offset to BaseScriptList table, from beginning of Axis table

            long axisTableStartAt = reader.BaseStream.Position;

            ushort baseTagListOffset = reader.ReadUInt16();
            ushort baseScriptListOffset = reader.ReadUInt16();

            if (baseTagListOffset > 0)
            {
                reader.BaseStream.Position = axisTableStartAt + baseTagListOffset;
                ReadBaseTagList(reader);
            }
            if (baseScriptListOffset > 0)
            {
                reader.BaseStream.Position = axisTableStartAt + baseScriptListOffset;
                ReadBaseScriptList(reader);
            }
        }

        static string ConvertToTagString(byte[] iden_tag_bytes)
        {
            return new string(new char[] {
                 (char)iden_tag_bytes[0] ,
                 (char)iden_tag_bytes[1],
                 (char)iden_tag_bytes[2],
                 (char)iden_tag_bytes[3]});
        }
        void ReadBaseTagList(BinaryReader reader)
        {
            //BaseTagList Table

            //The BaseTagList table identifies the baselines for all scripts in the font that are rendered in the same text direction. 
            //Each baseline is identified with a 4-byte baseline tag. 
            //The Baseline Tags section of the OpenType Layout Tag Registry lists currently registered baseline tags.
            //The BaseTagList can define any number of baselines, and it may include baseline tags for scripts supported in other fonts.

            //Each script in the BaseScriptList table must designate one of these BaseTagList baselines as its default,
            //which the OpenType Layout Services use to align all glyphs in the script. 
            //Even though the BaseScriptList and the BaseTagList are defined independently of one another, 
            //the BaseTagList typically includes a tag for each different default baseline needed to render the scripts in the layout direction.
            //If some scripts use the same default baseline, the BaseTagList needs to list the common baseline tag only once.

            //The BaseTagList table consists of an array of baseline identification tags (baselineTags),
            //listed alphabetically, and a count of the total number of baseline Tags in the array (baseTagCount).

            //BaseTagList table
            //Type 	    Name 	                    Description
            //uint16 	baseTagCount 	            Number of baseline identification tags in this text direction — may be zero (0)
            //Tag 	    baselineTags[baseTagCount] 	Array of 4-byte baseline identification tags — must be in alphabetical order

            //see baseline tag =>  https://docs.microsoft.com/en-us/typography/opentype/spec/baselinetags
            ushort baseTagCount = reader.ReadUInt16();
            for (int i = 0; i < baseTagCount; ++i)
            {
                string tagString = ConvertToTagString(reader.ReadBytes(4));

            }
        }
        void ReadBaseScriptList(BinaryReader reader)
        {
            //BaseScriptList Table

            //The BaseScriptList table identifies all scripts in the font that are rendered in the same layout direction. 
            //If a script is not listed here, then
            //the text-processing client will render the script using the layout information specified for the entire font.

            //For each script listed in the BaseScriptList table,
            //a BaseScriptRecord must be defined that identifies the script and references its layout data.
            //BaseScriptRecords are stored in the baseScriptRecords array, ordered alphabetically by the baseScriptTag in each record.
            //The baseScriptCount specifies the total number of BaseScriptRecords in the array.

            //BaseScriptList table
            //Type  	        Name 	                            Description
            //uint16 	        baseScriptCount 	                Number of BaseScriptRecords defined
            //BaseScriptRecord 	baseScriptRecords[baseScriptCount] 	Array of BaseScriptRecords, in alphabetical order by baseScriptTag

            long baseScriptListStartAt = reader.BaseStream.Position;
            ushort baseScriptCount = reader.ReadUInt16();
            BaseScriptRecord[] records = new BaseScriptRecord[baseScriptCount];
            for (int i = 0; i < baseScriptCount; ++i)
            {
                //BaseScriptRecord

                //A BaseScriptRecord contains a script identification tag (baseScriptTag), 
                //which must be identical to the ScriptTag used to define the script in the ScriptList of a GSUB or GPOS table. 
                //Each record also must include an offset to a BaseScript table that defines the baseline and min/max extent data for the script.             

                //BaseScriptRecord
                //Type 	    Name 	            Description
                //Tag 	    baseScriptTag 	    4-byte script identification tag
                //Offset16 	baseScriptOffset 	Offset to BaseScript table, from beginning of BaseScriptList             
                records[i] = new BaseScriptRecord(ConvertToTagString(reader.ReadBytes(4)), reader.ReadUInt16());
            }
            for (int i = 0; i < baseScriptCount; ++i)
            {
                BaseScriptRecord baseScriptRecord = records[i];
                reader.BaseStream.Position = baseScriptListStartAt + baseScriptRecord.baseScriptOffset;
                ReadBaseScriptTable(reader);
            }
        }
        struct BaseScriptRecord
        {
            public readonly string baseScriptTag;
            public readonly ushort baseScriptOffset;
            public BaseScriptRecord(string scriptTag, ushort offset)
            {
                this.baseScriptTag = scriptTag;
                this.baseScriptOffset = offset;
            }
        }
        struct BaseLangSysRecord
        {
            public readonly string baseScriptTag;
            public readonly ushort baseScriptOffset;
            public BaseLangSysRecord(string scriptTag, ushort offset)
            {
                this.baseScriptTag = scriptTag;
                this.baseScriptOffset = offset;
            }
        }

        void ReadBaseScriptTable(BinaryReader reader)
        {
            //BaseScript Table
            //A BaseScript table organizes and specifies the baseline data and min/max extent data for one script. 
            //Within a BaseScript table, the BaseValues table contains baseline information, 
            //and one or more MinMax tables contain min/max extent data
            //....

            //A BaseScript table has four components:
            //...

            long baseScriptTableStartAt = reader.BaseStream.Position;

            //BaseScript Table
            //Type 	                Name 	                                Description
            //Offset16 	            baseValuesOffset 	                    Offset to BaseValues table, from beginning of BaseScript table (may be NULL)
            //Offset16 	            defaultMinMaxOffset 	                Offset to MinMax table, from beginning of BaseScript table (may be NULL)
            //uint16    	        baseLangSysCount 	                    Number of BaseLangSysRecords defined — may be zero (0)
            //BaseLangSysRecord 	baseLangSysRecords[baseLangSysCount] 	Array of BaseLangSysRecords, in alphabetical order by BaseLangSysTag

            ushort baseValueOffset = reader.ReadUInt16();
            ushort defaultMinMaxOffset = reader.ReadUInt16();
            ushort baseLangSysCount = reader.ReadUInt16();
            BaseLangSysRecord[] records = null;

            if (baseLangSysCount > 0)
            {
                records = new BaseLangSysRecord[baseLangSysCount];
                for (int i = 0; i < baseLangSysCount; ++i)
                {
                    //BaseLangSysRecord
                    //A BaseLangSysRecord defines min/max extents for a language system or a language-specific feature.
                    //Each record contains an identification tag for the language system (baseLangSysTag) and an offset to a MinMax table (MinMax) 
                    //that defines extent coordinate values for the language system and references feature-specific extent data.

                    //BaseLangSysRecord
                    //Type 	        Name 	        Description
                    //Tag 	        baseLangSysTag 	4-byte language system identification tag
                    //Offset16 	    minMaxOffset 	Offset to MinMax table, from beginning of BaseScript table
                    records[i] = new BaseLangSysRecord(ConvertToTagString(reader.ReadBytes(4)), reader.ReadUInt16());
                }
            }

            //--------------------
            if (baseValueOffset > 0)
            {
                reader.BaseStream.Position = baseScriptTableStartAt + baseValueOffset;
                ReadBaseValues(reader);
            }
            if (defaultMinMaxOffset > 0)
            {
                reader.BaseStream.Position = baseScriptTableStartAt + defaultMinMaxOffset;
            }
        }
        void ReadBaseValues(BinaryReader reader)
        {
            //A BaseValues table lists the coordinate positions of all baselines named in the baselineTags array of the corresponding BaseTagList and
            //identifies a default baseline for a script.

            //...
            //
            //BaseValues table
            //Type 	    Name 	                    Description
            //uint16 	defaultBaselineIndex 	    Index number of default baseline for this script — equals index position of baseline tag in baselineTags array of the BaseTagList
            //uint16 	baseCoordCount          	Number of BaseCoord tables defined — should equal baseTagCount in the BaseTagList
            //Offset16 	baseCoords[baseCoordCount] 	Array of offsets to BaseCoord tables, from beginning of BaseValues table — order matches baselineTags array in the BaseTagList

            long baseValueTableStartAt = reader.BaseStream.Position;

            //
            ushort defaultBaselineIndex = reader.ReadUInt16();
            ushort baseCoordCount = reader.ReadUInt16();
            ushort[] baseCoords = Utils.ReadUInt16Array(reader, baseCoordCount);

            for (int i = 0; i < baseCoordCount; ++i)
            {
                reader.BaseStream.Position = baseValueTableStartAt + baseCoords[i];
                ReadBaseCoordTable(reader);
            }
        }
        void ReadBaseCoordTable(BinaryReader reader)
        {
            //BaseCoord Tables
            //Within the BASE table, a BaseCoord table defines baseline and min/max extent values.
            //Each BaseCoord table defines one X or Y value:

            //If defined within the HorizAxis table, then the BaseCoord table contains a Y value.
            //If defined within the VertAxis table, then the BaseCoord table contains an X value.

            //All values are defined in design units, which typically are scaled and rounded to the nearest integer when scaling the glyphs. 
            //Values may be negative.

            //----------------------
            //BaseCoord Format 1
            //The first BaseCoord format (BaseCoordFormat1) consists of a format identifier, 
            //followed by a single design unit coordinate that specifies the BaseCoord value. 
            //This format has the benefits of small size and simplicity, 
            //but the BaseCoord value cannot be hinted for fine adjustments at different sizes or device resolutions.

            //BaseCoordFormat1 table: Design units only
            //Type 	    Name 	            Description
            //uint16 	baseCoordFormat 	Format identifier — format = 1
            //int16 	coordinate 	        X or Y value, in design units
            //----------------------

            //BaseCoord Format 2

            //The second BaseCoord format (BaseCoordFormat2) specifies the BaseCoord value in design units, 
            //but also supplies a glyph index and a contour point for reference. During font hinting,
            //the contour point on the glyph outline may move. 
            //The point’s final position after hinting provides the final value for rendering a given font size.

            //Note: Glyph positioning operations defined in the GPOS table do not affect the point’s final position.          

            //BaseCoordFormat2 table: Design units plus contour point
            //Type 	    Name 	            Description
            //uint16 	baseCoordFormat 	Format identifier — format = 2
            //int16 	coordinate 	        X or Y value, in design units
            //uint16 	referenceGlyph 	    Glyph ID of control glyph
            //uint16 	baseCoordPoint 	    Index of contour point on the reference glyph

            //----------------------
            //BaseCoord Format 3

            //The third BaseCoord format (BaseCoordFormat3) also specifies the BaseCoord value in design units, 
            //but, in a non-variable font, it uses a Device table rather than a contour point to adjust the value. 
            //This format offers the advantage of fine-tuning the BaseCoord value for any font size and device resolution. 
            //(For more information about Device tables, see the chapter, Common Table Formats.)

            //In a variable font, BaseCoordFormat3 must be used to reference variation data 
            //to adjust the X or Y value for different variation instances, if needed.
            //In this case, BaseCoordFormat3 specifies an offset to a VariationIndex table,
            //which is a variant of the Device table that is used for referencing variation data.

            // Note: While separate VariationIndex table references are required for each Coordinate value that requires variation, two or more values that require the same variation-data values can have offsets that point to the same VariationIndex table, and two or more VariationIndex tables can reference the same variation data entries.

            // Note: If no VariationIndex table is used for a particular X or Y value (the offset is zero, or a different BaseCoord format is used), then that value is used for all variation instances.



            //BaseCoordFormat3 table: Design units plus Device or VariationIndex table
            //Type 	    Name 	            Description
            //uint16 	baseCoordFormat 	Format identifier — format = 3
            //int16 	coordinate 	        X or Y value, in design units
            //Offset16 	deviceTable 	    Offset to Device table (non-variable font) / Variation Index table (variable font) for X or Y value, from beginning of BaseCoord table (may be NULL).

            ushort baseCoordFormat = reader.ReadUInt16();
            switch (baseCoordFormat)
            {
                default: throw new System.NotSupportedException();
                case 1:
                    {
                        short coordinate = reader.ReadInt16();
                    }
                    break;
                case 2:
                    {
                        short coordinate = reader.ReadInt16();
                        ushort referenceGlyph = reader.ReadUInt16();
                        ushort baseCoordPoint = reader.ReadUInt16();
                    }
                    break;
                case 3:
                    //TODO: implement this...
                    break;
            }


        }
    }
}