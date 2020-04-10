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

    public interface IPreviewFontInfo {
        public string? Name { get; }
    }
    public class PreviewFontInfo : IPreviewFontInfo
    {
        public string? Name { get; }
        public string? SubFamilyName { get; }
        public string? TypographicFamilyName { get; }
        public string? TypographicSubFamilyName { get; }
        public Extensions.TranslatedOS2FontStyle OS2TranslatedStyle { get; }
        public ushort Weight { get; }

        public PreviewFontInfo(string? fontName, string? fontSubFam,
            string? tFamilyName, string? tSubFamilyName,
            ushort weight,
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
        }
        public int ActualStreamOffset { get; internal set; }
        public bool IsWebFont { get; internal set; }
#if DEBUG
        public override string ToString() => Name + ", " + SubFamilyName + ", " + OS2TranslatedStyle;
#endif
    }
    public class PreviewFontCollectionInfo : IPreviewFontInfo
    {
        public PreviewFontCollectionInfo(string fontName, PreviewFontInfo[] members)
        {
            Name = fontName;
            Fonts = members;
        }
        public string? Name { get; }
        public PreviewFontInfo[] Fonts { get; }
#if DEBUG
        public override string ToString() => Name ?? "";
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

            public FontCollectionHeader(ushort majorVersion, ushort minorVersion, uint numFonts, int[] offsetTables)
            {
                this.majorVersion = majorVersion;
                this.minorVersion = minorVersion;
                this.numFonts = numFonts;
                this.offsetTables = offsetTables;
            }
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
                if (member.Name is null) continue;
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
        public IPreviewFontInfo? ReadPreview(Stream stream)
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
                    return new PreviewFontCollectionInfo(BuildTtcfName(members), members);
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

            var majorVersion = input.ReadUInt16();
            var minorVersion = input.ReadUInt16();
            uint numFonts = input.ReadUInt32();
            int[] offsetTables = new int[numFonts];
            for (uint i = 0; i < numFonts; ++i)
            {
                offsetTables[i] = input.ReadInt32();
            }

            var ttcHeader = new FontCollectionHeader(majorVersion, minorVersion, numFonts, offsetTables);

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
                var unreadEntry = new UnreadTableEntry(ReadTableHeader(input));
                tables.AddEntry(unreadEntry.Name, unreadEntry);
            }
            return ReadPreviewFontInfo(tables, input);
        }
        public Typeface? Read(Stream stream, int streamStartOffset = 0, ReadFlags readFlags = ReadFlags.Full)
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
                    var unreadEntry = new UnreadTableEntry(ReadTableHeader(input));
                    tables.AddEntry(unreadEntry.Name, unreadEntry);
                }
                //------------------------------------------------------------------ 
                return ReadTableEntryCollection(tables, input);
            }
        }

        internal PreviewFontInfo ReadPreviewFontInfo(TableEntryCollection tables, BinaryReader input)
        {
            var os2Table = ReadTableMandatory(tables, input, OS2Table.Name, (h, r) => new OS2Table(h, r));
            var nameEntry = ReadTableMandatory(tables, input, NameEntry.Name, (h, r) => new NameEntry(h, r));

            return new PreviewFontInfo(
              nameEntry.FontName,
              nameEntry.FontSubFamily,
              nameEntry.TypographicFamilyName,
              nameEntry.TypographyicSubfamilyName,
              os2Table.usWeightClass,
              Extensions.TypefaceExtensions.TranslatedOS2FontStyle(os2Table));
        }
        internal Typeface ReadTableEntryCollection(TableEntryCollection tables, BinaryReader input)
        {
            // 8 mandatory tables: https://docs.microsoft.com/en-us/typography/opentype/spec/otff#required-tables
            // Here, post is not treated as mandatory -> 7 mandatory tables
            var os2Table = ReadTableMandatory(tables, input, OS2Table.Name, (h, r) => new OS2Table(h, r));
            var nameEntry = ReadTableMandatory(tables, input, NameEntry.Name, (h, r) => new NameEntry(h, r));

            var header = ReadTableMandatory(tables, input, Head.Name, (h, r) => new Head(h, r));
            var maximumProfile = ReadTableMandatory(tables, input, MaxProfile.Name, (h, r) => new MaxProfile(h, r));
            var horizontalHeader = ReadTableMandatory(tables, input, HorizontalHeader.Name, (h, r) => new HorizontalHeader(h, r));
            var horizontalMetrics = ReadTableMandatory(tables, input, HorizontalMetrics.Name,
                (h, r) => new HorizontalMetrics(horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount, h, r));

            //---
            var postTable = ReadTableIfExists(tables, input, PostTable.Name, (h, r) => new PostTable(h, r));
            var cff = ReadTableIfExists(tables, input, CFFTable.Name, (h, r) => new CFFTable(h, r));

            //--------------
            var cmaps = ReadTableMandatory(tables, input, Cmap.Name, (h, r) => new Cmap(h, r));
            var glyphLocations = ReadTableIfExists(tables, input, GlyphLocations.Name,
                (h, r) => new GlyphLocations(maximumProfile.GlyphCount, header.WideGlyphLocations, h, r));

            var glyf = glyphLocations != null ? ReadTableIfExists(tables, input, Glyf.Name, (h, r) => new Glyf(glyphLocations, h, r)) : null;
            //--------------
            var gaspTable = ReadTableIfExists(tables, input, Gasp.Name, (h, r) => new Gasp(h, r));
            var vdmx = ReadTableIfExists(tables, input, VerticalDeviceMetrics.Name, (h, r) => new VerticalDeviceMetrics(h, r));
            //--------------


            var kern = ReadTableIfExists(tables, input, Kern.Name, (h, r) => new Kern(h, r)); //deprecated
            //--------------
            //advanced typography
            var gdef = ReadTableIfExists(tables, input, GDEF.Name, (h, r) => new GDEF(h, r));
            var gsub = ReadTableIfExists(tables, input, GSUB.Name, (h, r) => new GSUB(h, r));
            var gpos = ReadTableIfExists(tables, input, GPOS.Name, (h, r) => new GPOS(h, r));
            var baseTable = ReadTableIfExists(tables, input, BASE.Name, (h, r) => new BASE(h, r));
            var jstf = ReadTableIfExists(tables, input, JSTF.Name, (h, r) => new JSTF(h, r));

            var colr = ReadTableIfExists(tables, input, COLR.Name, (h, r) => new COLR(h, r));
            var cpal = ReadTableIfExists(tables, input, CPAL.Name, (h, r) => new CPAL(h, r));
            var vhea = ReadTableIfExists(tables, input, VerticalHeader.Name, (h, r) => new VerticalHeader(h, r));
            if (vhea != null)
            {
                var vmtx = ReadTableIfExists(tables, input, VerticalMetrics.Name,
                    (h, r) => new VerticalMetrics(vhea.NumOfLongVerMetrics, h, r));
            }

            var stat = ReadTableIfExists(tables, input, STAT.Name, (h, r) => new STAT(h, r));
            if (stat != null)
            {
                var fvar = ReadTableIfExists(tables, input, FVar.Name, (h, r) => new FVar(h, r));
                if (fvar != null)
                {
                    var gvar = ReadTableIfExists(tables, input, GVar.Name, (h, r) => new GVar(h, r));
                    var cvar = ReadTableIfExists(tables, input, CVar.Name, (h, r) => new CVar(h, r));
                    var hvar = ReadTableIfExists(tables, input, HVar.Name, (h, r) => new HVar(h, r));
                    var mvar = ReadTableIfExists(tables, input, MVar.Name, (h, r) => new MVar(h, r));
                    var avar = ReadTableIfExists(tables, input, AVar.Name, (h, r) => new AVar(h, r));
                }
            }


            //test math table
            var mathtable = ReadTableIfExists(tables, input, MathTable.Name, (h, r) => new MathTable(h, r));

            //---------------------------------------------
            //about truetype instruction init 

            //--------------------------------------------- 
            Typeface typeface;
            bool isPostScriptOutline = false;
            bool isBitmapFont = false;
            if (glyf == null)
            {
                //check if this is cff table ?
                if (cff == null)
                {

                    //check  cbdt/cblc ?
                    var cblcTable = ReadTableIfExists(tables, input, CBLC.Name, (h, r) => new CBLC(h, r));
                    if (cblcTable != null)
                    {
                        var cbdtTable = ReadTableIfExists(tables, input, CBDT.Name, (h, r) => new CBDT(h, r));
                        if (cbdtTable is null) throw new NotImplementedException($"{CBLC.Name} exists but not {CBDT.Name}");
                        //read cbdt 
                        //bitmap font 

                        BitmapFontGlyphSource bmpFontGlyphSrc = new BitmapFontGlyphSource(cblcTable, cbdtTable);
                        Glyph[] glyphs = bmpFontGlyphSrc.BuildGlyphList();


                        typeface = new Typeface(
                            os2Table,
                            nameEntry,
                            header,
                            maximumProfile,
                            horizontalHeader,
                            horizontalMetrics,
                            cmaps,
                            glyphs)
                        {
                            BitmapFontGlyphSource = bmpFontGlyphSrc
                        };
                        isBitmapFont = true;
                    }
                    else
                    {
                        //TODO:
                        var fontBmpTable = ReadTableIfExists(tables, input, EBLC.Name, (h, r) => new EBLC(h, r));
                        throw new NotSupportedException();
                    }
                }
                else
                {
                    //...  
                    //PostScript outline font 
                    isPostScriptOutline = true;
                    if (cff.Cff1FontSet is null)
                        throw new System.NotImplementedException("CFF2 not implemented");
                    var cff1font = cff.Cff1FontSet._fonts[0];
                    typeface = new Typeface(
                            os2Table,
                            nameEntry,
                            header,
                            maximumProfile,
                            horizontalHeader,
                            horizontalMetrics,
                            cmaps,
                            cff1font._glyphs)
                    {
                        CffTable = cff
                    };
                }
            }
            else
            {
                typeface = new Typeface(
                            os2Table,
                            nameEntry,
                            header,
                            maximumProfile,
                            horizontalHeader,
                            horizontalMetrics,
                            cmaps,
                            glyf.Glyphs);
            }

            //----------------------------
            typeface.KernTable = kern;
            typeface.GaspTable = gaspTable;
            //----------------------------

            if (!isPostScriptOutline && !isBitmapFont)
            {
                var fpgmTable = ReadTableIfExists(tables, input, FpgmTable.Name, (h, r) => new FpgmTable(h, r));
                //control values table
                var cvtTable = ReadTableIfExists(tables, input, CvtTable.Name, (h, r) => new CvtTable(h, r));
                if (cvtTable != null)
                {
                    typeface.ControlValues = cvtTable._controlValues;
                }
                if (fpgmTable != null)
                {
                    typeface.FpgmProgramBuffer = fpgmTable._programBuffer;
                }
                var propProgramTable = ReadTableIfExists(tables, input, PrepTable.Name, (h, r) => new PrepTable(h, r));
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
                var svgTable = ReadTableIfExists(tables, input, SvgTable.Name, (h, r) => new SvgTable(h, r));
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
        internal delegate T TableReader<T>(TableHeader header, BinaryReader reader);
        static T ReadTableMandatory<T>(TableEntryCollection tables, BinaryReader reader, string name, TableReader<T> tableReader)
            where T : TableEntry =>
            ReadTableIfExists(tables, reader, name, tableReader) ?? throw new NotSupportedException("Missing mandatory table in font file: " + name);
        static T? ReadTableIfExists<T>(TableEntryCollection tables, BinaryReader reader, string name, TableReader<T> tableReader)
            where T : notnull, TableEntry
        {
            if (tables.TryGetTable(name, out TableEntry? found))
            {
                //found table name
                //check if we have read this table or not
                if (found is UnreadTableEntry unreadTableEntry)
                {
                    //set header before actual read
                    T resultTable =
                        unreadTableEntry.HasCustomContentReader
                        ? unreadTableEntry.CreateTableEntry(reader, tableReader)
                        : tableReader(found.Header, reader);
                    //then replace
                    tables.ReplaceTable(name, resultTable);
                    return resultTable;
                }
                else
                {
                    //we have read this table
                    throw new InvalidOperationException("Table cannot be read more than once: " + name);
                }
            }
            //not found
            return null;
        }


    }

}
