//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;

using Typography.OpenFont.IO;
using Typography.OpenFont.Tables;

namespace Typography.OpenFont
{
    [Flags]
    public enum ReadFlags
    {
        Full = 0,
        Name = 1,
        Matrix = 1 << 2,
        AdvancedLayout = 1 << 3,
        Variation = 1 << 4
    }


    public class PreviewFontInfo
    {
        public readonly string Name;
        public readonly string SubFamilyName;
        public readonly string TypographicFamilyName;
        public readonly string TypographicSubFamilyName;
        public readonly Extensions.TranslatedOS2FontStyle OS2TranslatedStyle;
        public readonly ushort Weight;
        PreviewFontInfo[] _ttcfMembers;


        internal PreviewFontInfo(string fontName, string fontSubFam,
            string tFamilyName, string tSubFamilyName,
            ushort weight,
            Languages langs,
            Extensions.TranslatedOS2FontStyle os2TranslatedStyle = Extensions.TranslatedOS2FontStyle.UNSET)
        {
            Name = fontName;
            SubFamilyName = fontSubFam;
            TypographicFamilyName = tFamilyName;
            TypographicSubFamilyName = tSubFamilyName;

#if DEBUG
            //please note that some fontName != typographicFontName
            //this may effect how to search a font
            if (fontName != tFamilyName && tFamilyName != null)
            {

            }
            if (fontSubFam != tSubFamilyName && tSubFamilyName != null)
            {

            }
#endif
            Weight = weight;
            OS2TranslatedStyle = os2TranslatedStyle;
            Languages = langs;
        }
        internal PreviewFontInfo(string fontName, PreviewFontInfo[] ttcfMembers)
        {
            Name = fontName;
            SubFamilyName = "";
            _ttcfMembers = ttcfMembers;
            Languages = new Languages();
        }
        public int ActualStreamOffset { get; internal set; }
        public bool IsWebFont { get; internal set; }
        public bool IsFontCollection => _ttcfMembers != null;

        public string PostScriptName { get; internal set; }
        public string UniqueFontIden { get; internal set; }
        public string VersionString { get; internal set; }
        public Languages Languages { get; }

        /// <summary>
        /// get font collection's member count
        /// </summary>
        public int MemberCount => _ttcfMembers.Length;
        /// <summary>
        /// get font collection's member
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PreviewFontInfo GetMember(int index) => _ttcfMembers[index];
#if DEBUG
        public override string ToString()
        {
            return (IsFontCollection) ? Name : Name + ", " + SubFamilyName + ", " + OS2TranslatedStyle;
        }
#endif
    }


    static class KnownFontFiles
    {
        public static bool IsTtcf(ushort u1, ushort u2)
        {
            //https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
            //check if 1st 4 bytes is ttcf or not  
            return (((u1 >> 8) & 0xff) == (byte)'t') &&
                   (((u1) & 0xff) == (byte)'t') &&
                   (((u2 >> 8) & 0xff) == (byte)'c') &&
                   (((u2) & 0xff) == (byte)'f');
        }
        public static bool IsWoff(ushort u1, ushort u2)
        {
            return (((u1 >> 8) & 0xff) == (byte)'w') && //0x77
                  (((u1) & 0xff) == (byte)'O') && //0x4f 
                  (((u2 >> 8) & 0xff) == (byte)'F') && // 0x46
                  (((u2) & 0xff) == (byte)'F'); //0x46 
        }
        public static bool IsWoff2(ushort u1, ushort u2)
        {
            return (((u1 >> 8) & 0xff) == (byte)'w') &&//0x77
            (((u1) & 0xff) == (byte)'O') &&  //0x4f 
            (((u2 >> 8) & 0xff) == (byte)'F') && //0x46
            (((u2) & 0xff) == (byte)'2'); //0x32 
        }
    }





    public class OpenFontReader
    {

        public OpenFontReader()
        {

        }

        class FontCollectionHeader
        {
            public ushort majorVersion;
            public ushort minorVersion;
            public uint numFonts;
            public int[] offsetTables;
            //
            //if version 2
            public uint dsigTag;
            public uint dsigLength;
            public uint dsigOffset;
        }

        static string BuildTtcfName(PreviewFontInfo[] members)
        {
            //THIS IS MY CONVENTION for TrueType collection font name
            //you can change this to fit your need.

            var stbuilder = new System.Text.StringBuilder();
            stbuilder.Append("TTCF: " + members.Length);
            var uniqueNames = new System.Collections.Generic.Dictionary<string, bool>();
            for (uint i = 0; i < members.Length; ++i)
            {
                PreviewFontInfo member = members[i];
                if (!uniqueNames.ContainsKey(member.Name))
                {
                    uniqueNames.Add(member.Name, true);
                    stbuilder.Append("," + member.Name);
                }
            }
            return stbuilder.ToString();
        }


        /// <summary>
        /// read only name entry
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public PreviewFontInfo ReadPreview(Stream stream)
        {
            //var little = BitConverter.IsLittleEndian;
            using (var input = new ByteOrderSwappingBinaryReader(stream))
            {
                ushort majorVersion = input.ReadUInt16();
                ushort minorVersion = input.ReadUInt16();

                if (KnownFontFiles.IsTtcf(majorVersion, minorVersion))
                {
                    //this font stream is 'The Font Collection'
                    FontCollectionHeader ttcHeader = ReadTTCHeader(input);
                    PreviewFontInfo[] members = new PreviewFontInfo[ttcHeader.numFonts];
                    for (uint i = 0; i < ttcHeader.numFonts; ++i)
                    {
                        input.BaseStream.Seek(ttcHeader.offsetTables[i], SeekOrigin.Begin);
                        PreviewFontInfo member = members[i] = ReadActualFontPreview(input, false);
                        member.ActualStreamOffset = ttcHeader.offsetTables[i];
                    }
                    return new PreviewFontInfo(BuildTtcfName(members), members);
                }
                else if (KnownFontFiles.IsWoff(majorVersion, minorVersion))
                {
                    //check if we enable woff or not
                    WebFont.WoffReader woffReader = new WebFont.WoffReader();
                    input.BaseStream.Position = 0;
                    return woffReader.ReadPreview(input);
                }
                else if (KnownFontFiles.IsWoff2(majorVersion, minorVersion))
                {
                    //check if we enable woff2 or not
                    WebFont.Woff2Reader woffReader = new WebFont.Woff2Reader();
                    input.BaseStream.Position = 0;
                    return woffReader.ReadPreview(input);
                }
                else
                {
                    return ReadActualFontPreview(input, true);//skip version data (majorVersion, minorVersion)
                }
            }
        }
        FontCollectionHeader ReadTTCHeader(ByteOrderSwappingBinaryReader input)
        {
            //https://docs.microsoft.com/en-us/typography/opentype/spec/otff#ttc-header
            //TTC Header Version 1.0:
            //Type 	    Name 	        Description
            //TAG 	    ttcTag 	        Font Collection ID string: 'ttcf' (used for fonts with CFF or CFF2 outlines as well as TrueType outlines)
            //uint16 	majorVersion 	Major version of the TTC Header, = 1.
            //uint16 	minorVersion 	Minor version of the TTC Header, = 0.
            //uint32 	numFonts 	    Number of fonts in TTC
            //Offset32 	offsetTable[numFonts] 	Array of offsets to the OffsetTable for each font from the beginning of the file

            //TTC Header Version 2.0:
            //Type 	    Name 	        Description
            //TAG 	    ttcTag 	        Font Collection ID string: 'ttcf'
            //uint16 	majorVersion 	Major version of the TTC Header, = 2.
            //uint16 	minorVersion 	Minor version of the TTC Header, = 0.
            //uint32 	numFonts 	    Number of fonts in TTC
            //Offset32 	offsetTable[numFonts] 	Array of offsets to the OffsetTable for each font from the beginning of the file
            //uint32 	dsigTag 	    Tag indicating that a DSIG table exists, 0x44534947 ('DSIG') (null if no signature)
            //uint32 	dsigLength 	    The length (in bytes) of the DSIG table (null if no signature)
            //uint32 	dsigOffset 	    The offset (in bytes) of the DSIG table from the beginning of the TTC file (null if no signature)

            var ttcHeader = new FontCollectionHeader();

            ttcHeader.majorVersion = input.ReadUInt16();
            ttcHeader.minorVersion = input.ReadUInt16();
            uint numFonts = input.ReadUInt32();
            int[] offsetTables = new int[numFonts];
            for (uint i = 0; i < numFonts; ++i)
            {
                offsetTables[i] = input.ReadInt32();
            }

            ttcHeader.numFonts = numFonts;
            ttcHeader.offsetTables = offsetTables;
            //
            if (ttcHeader.majorVersion == 2)
            {
                ttcHeader.dsigTag = input.ReadUInt32();
                ttcHeader.dsigLength = input.ReadUInt32();
                ttcHeader.dsigOffset = input.ReadUInt32();

                if (ttcHeader.dsigTag == 0x44534947)
                {
                    //Tag indicating that a DSIG table exists
                    //TODO: goto DSIG add read signature
                }
            }
            return ttcHeader;
        }
        PreviewFontInfo ReadActualFontPreview(ByteOrderSwappingBinaryReader input, bool skipVersionData)
        {
            if (!skipVersionData)
            {
                ushort majorVersion = input.ReadUInt16();
                ushort minorVersion = input.ReadUInt16();
            }

            ushort tableCount = input.ReadUInt16();
            ushort searchRange = input.ReadUInt16();
            ushort entrySelector = input.ReadUInt16();
            ushort rangeShift = input.ReadUInt16();

            var tables = new TableEntryCollection();
            for (int i = 0; i < tableCount; i++)
            {
                tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
            }
            return ReadPreviewFontInfo(tables, input);
        }
        public Typeface Read(Stream stream, int streamStartOffset = 0, ReadFlags readFlags = ReadFlags.Full)
        {
            //bool little = BitConverter.IsLittleEndian; 

            if (streamStartOffset > 0)
            {
                //eg. for ttc
                stream.Seek(streamStartOffset, SeekOrigin.Begin);
            }
            using (var input = new ByteOrderSwappingBinaryReader(stream))
            {
                ushort majorVersion = input.ReadUInt16();
                ushort minorVersion = input.ReadUInt16();

                if (KnownFontFiles.IsTtcf(majorVersion, minorVersion))
                {
                    //this font stream is 'The Font Collection'                    
                    //To read content of ttc=> one must specific the offset
                    //so use read preview first=> you will know that what are inside the ttc.                    

                    return null;
                }
                else if (KnownFontFiles.IsWoff(majorVersion, minorVersion))
                {
                    //check if we enable woff or not
                    WebFont.WoffReader woffReader = new WebFont.WoffReader();
                    input.BaseStream.Position = 0;
                    return woffReader.Read(input);
                }
                else if (KnownFontFiles.IsWoff2(majorVersion, minorVersion))
                {
                    //check if we enable woff2 or not
                    WebFont.Woff2Reader woffReader = new WebFont.Woff2Reader();
                    input.BaseStream.Position = 0;
                    return woffReader.Read(input);
                }
                //-----------------------------------------------------------------


                ushort tableCount = input.ReadUInt16();
                ushort searchRange = input.ReadUInt16();
                ushort entrySelector = input.ReadUInt16();
                ushort rangeShift = input.ReadUInt16();
                //------------------------------------------------------------------ 
                var tables = new TableEntryCollection();
                for (int i = 0; i < tableCount; i++)
                {
                    tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
                }
                //------------------------------------------------------------------ 
                return ReadTableEntryCollection(tables, input);
            }
        }

        internal PreviewFontInfo ReadPreviewFontInfo(TableEntryCollection tables, BinaryReader input)
        {
            NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());
            OS2Table os2Table = ReadTableIfExists(tables, input, new OS2Table());

            //for preview, read ONLY  script list from gsub and gpos (set OnlyScriptList).
            Meta metaTable = ReadTableIfExists(tables, input, new Meta());
            GSUB gsub = ReadTableIfExists(tables, input, new GSUB() { OnlyScriptList = true });
            GPOS gpos = ReadTableIfExists(tables, input, new GPOS() { OnlyScriptList = true });
            //gsub and gpos contains actual script_list that are in the typeface

            Languages langs = new Languages();
            langs.Update(os2Table, metaTable, gsub, gpos);

            return new PreviewFontInfo(
              nameEntry.FontName,
              nameEntry.FontSubFamily,
              nameEntry.TypographicFamilyName,
              nameEntry.TypographyicSubfamilyName,
              os2Table.usWeightClass,
              langs,
              Extensions.TypefaceExtensions.TranslatedOS2FontStyle(os2Table))
            {
                PostScriptName = nameEntry.PostScriptName,
                UniqueFontIden = nameEntry.UniqueFontIden,
                VersionString = nameEntry.VersionString
            };

        }
        internal Typeface ReadTableEntryCollection(TableEntryCollection tables, BinaryReader input)
        {

            OS2Table os2Table = ReadTableIfExists(tables, input, new OS2Table());
            Meta meta = ReadTableIfExists(tables, input, new Meta());
            NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());

            Head header = ReadTableIfExists(tables, input, new Head());
            MaxProfile maximumProfile = ReadTableIfExists(tables, input, new MaxProfile());
            HorizontalHeader horizontalHeader = ReadTableIfExists(tables, input, new HorizontalHeader());
            HorizontalMetrics horizontalMetrics = ReadTableIfExists(tables, input, new HorizontalMetrics(horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount));



            //---
            PostTable postTable = ReadTableIfExists(tables, input, new PostTable());
            CFFTable cff = ReadTableIfExists(tables, input, new CFFTable());

            //--------------
            Cmap cmaps = ReadTableIfExists(tables, input, new Cmap());
            GlyphLocations glyphLocations = ReadTableIfExists(tables, input, new GlyphLocations(maximumProfile.GlyphCount, header.WideGlyphLocations));

            Glyf glyf = ReadTableIfExists(tables, input, new Glyf(glyphLocations));
            //--------------
            Gasp gaspTable = ReadTableIfExists(tables, input, new Gasp());
            VerticalDeviceMetrics vdmx = ReadTableIfExists(tables, input, new VerticalDeviceMetrics());
            //--------------


            Kern kern = ReadTableIfExists(tables, input, new Kern()); //deprecated
            //--------------
            //advanced typography
            GDEF gdef = ReadTableIfExists(tables, input, new GDEF());
            GSUB gsub = ReadTableIfExists(tables, input, new GSUB());
            GPOS gpos = ReadTableIfExists(tables, input, new GPOS());
            BASE baseTable = ReadTableIfExists(tables, input, new BASE());
            JSTF jstf = ReadTableIfExists(tables, input, new JSTF());

            COLR colr = ReadTableIfExists(tables, input, new COLR());
            CPAL cpal = ReadTableIfExists(tables, input, new CPAL());
            VerticalHeader vhea = ReadTableIfExists(tables, input, new VerticalHeader());
            if (vhea != null)
            {
                VerticalMetrics vmtx = ReadTableIfExists(tables, input, new VerticalMetrics(vhea.NumOfLongVerMetrics));
            }

            STAT stat = ReadTableIfExists(tables, input, new STAT());
            if (stat != null)
            {
                FVar fvar = ReadTableIfExists(tables, input, new FVar());
                if (fvar != null)
                {
                    GVar gvar = ReadTableIfExists(tables, input, new GVar());
                    CVar cvar = ReadTableIfExists(tables, input, new CVar());
                    HVar hvar = ReadTableIfExists(tables, input, new HVar());
                    MVar mvar = ReadTableIfExists(tables, input, new MVar());
                    AVar avar = ReadTableIfExists(tables, input, new AVar());
                }
            }


            //test math table
            MathTable mathtable = ReadTableIfExists(tables, input, new MathTable());

            //---------------------------------------------
            //about truetype instruction init 

            //--------------------------------------------- 
            Typeface typeface = null;
            bool isPostScriptOutline = false;
            bool isBitmapFont = false;
            if (glyf == null)
            {
                //check if this is cff table ?
                if (cff == null)
                {

                    //check  cbdt/cblc ?
                    CBLC cblcTable = ReadTableIfExists(tables, input, new CBLC());
                    if (cblcTable != null)
                    {
                        CBDT cbdtTable = ReadTableIfExists(tables, input, new CBDT());
                        //read cbdt 
                        //bitmap font 

                        BitmapFontGlyphSource bmpFontGlyphSrc = new BitmapFontGlyphSource(cblcTable, cbdtTable);
                        Glyph[] glyphs = bmpFontGlyphSrc.BuildGlyphList();


                        typeface = new Typeface(
                          nameEntry,
                          header.Bounds,
                          header.UnitsPerEm,
                          bmpFontGlyphSrc,
                          glyphs,
                          horizontalMetrics,
                          os2Table);
                        isBitmapFont = true;
                    }
                    else
                    {
                        //TODO:
                        EBLC fontBmpTable = ReadTableIfExists(tables, input, new EBLC());
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    //...  
                    //PostScript outline font 
                    isPostScriptOutline = true;
                    typeface = new Typeface(
                          nameEntry,
                          header.Bounds,
                          header.UnitsPerEm,
                          cff,
                          horizontalMetrics,
                          os2Table);
                }
            }
            else
            {
                typeface = new Typeface(
                    nameEntry,
                    header.Bounds,
                    header.UnitsPerEm,
                    glyf.Glyphs,
                    horizontalMetrics,
                    os2Table);
            }

            //----------------------------
            typeface.CmapTable = cmaps;
            typeface.KernTable = kern;
            typeface.GaspTable = gaspTable;
            typeface.MaxProfile = maximumProfile;
            typeface.HheaTable = horizontalHeader;

            //----------------------------

            if (!isPostScriptOutline && !isBitmapFont)
            {
                FpgmTable fpgmTable = ReadTableIfExists(tables, input, new FpgmTable());
                //control values table
                CvtTable cvtTable = ReadTableIfExists(tables, input, new CvtTable());
                if (cvtTable != null)
                {
                    typeface.ControlValues = cvtTable._controlValues;
                }
                if (fpgmTable != null)
                {
                    typeface.FpgmProgramBuffer = fpgmTable._programBuffer;
                }
                PrepTable propProgramTable = ReadTableIfExists(tables, input, new PrepTable());
                if (propProgramTable != null)
                {
                    typeface.PrepProgramBuffer = propProgramTable._programBuffer;
                }
            }
            //-------------------------
            typeface.LoadOpenFontLayoutInfo(
                gdef,
                gsub,
                gpos,
                baseTable,
                colr,
                cpal);
            //------------

            {
                SvgTable svgTable = ReadTableIfExists(tables, input, new SvgTable());
                if (svgTable != null)
                {
                    typeface._svgTable = svgTable;
                }
            }

            typeface.PostTable = postTable;
            if (mathtable != null)
            {
                var mathGlyphLoader = new MathGlyphLoader();
                mathGlyphLoader.LoadMathGlyph(typeface, mathtable);

            }
#if DEBUG
            //test
            //int found = typeface.GetGlyphIndexByName("Uacute");
            if (typeface.IsCffFont)
            {
                //optional
                typeface.UpdateAllCffGlyphBounds();
            }
#endif


            typeface.UpdateLangs(meta);

            return typeface;
        }


        static TableHeader ReadTableHeader(BinaryReader input)
        {
            return new TableHeader(
                input.ReadUInt32(),
                input.ReadUInt32(),
                input.ReadUInt32(),
                input.ReadUInt32());
        }
        static T ReadTableIfExists<T>(TableEntryCollection tables, BinaryReader reader, T resultTable)
            where T : TableEntry
        {

            if (tables.TryGetTable(resultTable.Name, out TableEntry found))
            {
                //found table name
                //check if we have read this table or not
                if (found is UnreadTableEntry unreadTableEntry)
                {
                    //set header before actal read
                    resultTable.Header = found.Header;
                    if (unreadTableEntry.HasCustomContentReader)
                    {
                        resultTable = unreadTableEntry.CreateTableEntry(reader, resultTable);
                    }
                    else
                    {
                        resultTable.LoadDataFrom(reader);
                    }
                    //then replace
                    tables.ReplaceTable(resultTable);
                    return resultTable;
                }
                else
                {
                    //we have read this table
                    throw new NotSupportedException();
                }
            }
            //not found
            return null;
        }


    }



}
