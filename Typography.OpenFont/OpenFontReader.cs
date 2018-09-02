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

    public struct PreviewFontInfo
    {
        public readonly string fontName;
        public readonly string fontSubFamily;
        public readonly Extensions.TranslatedOS2FontStyle OS2TranslatedStyle;


        public PreviewFontInfo(string fontName, string fontSubFam,
            Extensions.TranslatedOS2FontStyle os2TranslatedStyle = Extensions.TranslatedOS2FontStyle.UNSET)
        {
            this.fontName = fontName;
            this.fontSubFamily = fontSubFam;
            OS2TranslatedStyle = os2TranslatedStyle;
        }
#if DEBUG
        public override string ToString()
        {
            return fontName + ", " + fontSubFamily + ", " + OS2TranslatedStyle;
        }
#endif
    }


    public class OpenFontReader
    {
        /// <summary>
        /// read only name entry
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public PreviewFontInfo ReadPreview(Stream stream)
        {
            var little = BitConverter.IsLittleEndian;
            using (var input = new ByteOrderSwappingBinaryReader(stream))
            {
                ushort majorVersion = input.ReadUInt16();
                ushort minorVersion = input.ReadUInt16();
                ushort tableCount = input.ReadUInt16();
                ushort searchRange = input.ReadUInt16();
                ushort entrySelector = input.ReadUInt16();
                ushort rangeShift = input.ReadUInt16();
                var tables = new TableEntryCollection();
                for (int i = 0; i < tableCount; i++)
                {
                    tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
                }


                NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());
                OS2Table os2Table = ReadTableIfExists(tables, input, new OS2Table());

                return new PreviewFontInfo(
                    nameEntry.FontName,
                    nameEntry.FontSubFamily,
                    Extensions.TypefaceExtensions.TranslatedOS2FontStyle(os2Table)
                    );
            }
        }


        public Typeface Read(Stream stream, ReadFlags readFlags = ReadFlags.Full)
        {
            bool little = BitConverter.IsLittleEndian;
            using (var input = new ByteOrderSwappingBinaryReader(stream))
            {
                ushort majorVersion = input.ReadUInt16();
                ushort minorVersion = input.ReadUInt16();
                ushort tableCount = input.ReadUInt16();
                ushort searchRange = input.ReadUInt16();
                ushort entrySelector = input.ReadUInt16();
                ushort rangeShift = input.ReadUInt16();
                var tables = new TableEntryCollection();
                for (int i = 0; i < tableCount; i++)
                {
                    tables.AddEntry(new UnreadTableEntry(ReadTableHeader(input)));
                }
                //------------------------------------------------------------------ 
                OS2Table os2Table = ReadTableIfExists(tables, input, new OS2Table());
                NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());

                Head header = ReadTableIfExists(tables, input, new Head());
                MaxProfile maximumProfile = ReadTableIfExists(tables, input, new MaxProfile());
                HorizontalHeader horizontalHeader = ReadTableIfExists(tables, input, new HorizontalHeader());
                HorizontalMetrics horizontalMetrics = ReadTableIfExists(tables, input, new HorizontalMetrics(horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount));

                //---
                PostTable postTable = ReadTableIfExists(tables, input, new PostTable());
                CFFTable ccf = ReadTableIfExists(tables, input, new CFFTable());

                //--------------
                Cmap cmaps = ReadTableIfExists(tables, input, new Cmap());
                GlyphLocations glyphLocations = ReadTableIfExists(tables, input, new GlyphLocations(maximumProfile.GlyphCount, header.WideGlyphLocations));

                Glyf glyf = ReadTableIfExists(tables, input, new Glyf(glyphLocations));
                //--------------
                Gasp gaspTable = ReadTableIfExists(tables, input, new Gasp());
                VerticalDeviceMetrics vdmx = ReadTableIfExists(tables, input, new VerticalDeviceMetrics());
                //--------------


                Kern kern = ReadTableIfExists(tables, input, new Kern());
                //--------------
                //advanced typography
                GDEF gdef = ReadTableIfExists(tables, input, new GDEF());
                GSUB gsub = ReadTableIfExists(tables, input, new GSUB());
                GPOS gpos = ReadTableIfExists(tables, input, new GPOS());
                BASE baseTable = ReadTableIfExists(tables, input, new BASE());
                COLR colr = ReadTableIfExists(tables, input, new COLR());
                CPAL cpal = ReadTableIfExists(tables, input, new CPAL());
                VerticalHeader vhea = ReadTableIfExists(tables, input, new VerticalHeader());
                if (vhea != null)
                {
                    VerticalMetrics vmtx = ReadTableIfExists(tables, input, new VerticalMetrics(vhea.NumOfLongVerMetrics));
                }



                //test math table
                MathTable mathtable = ReadTableIfExists(tables, input, new MathTable());
                EBLCTable fontBmpTable = ReadTableIfExists(tables, input, new EBLCTable());
                //---------------------------------------------
                //about truetype instruction init 

                //--------------------------------------------- 
                Typeface typeface = null;
                bool isPostScriptOutline = false;
                if (glyf == null)
                {
                    //check if this is cff table ?
                    if (ccf == null)
                    {
                        //TODO: review here
                        throw new NotSupportedException();
                    }
                    //...  
                    //PostScript outline font 
                    isPostScriptOutline = true;
                    typeface = new Typeface(
                          nameEntry,
                          header.Bounds,
                          header.UnitsPerEm,
                          ccf,
                          horizontalMetrics,
                          os2Table);


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

                if (!isPostScriptOutline)
                {
                    FpgmTable fpgmTable = ReadTableIfExists(tables, input, new FpgmTable());
                    //control values table
                    CvtTable cvtTable = ReadTableIfExists(tables, input, new CvtTable());
                    if (cvtTable != null)
                    {
                        typeface.ControlValues = cvtTable.controlValues;
                    }
                    if (fpgmTable != null)
                    {
                        typeface.FpgmProgramBuffer = fpgmTable.programBuffer;
                    }
                    PrepTable propProgramTable = ReadTableIfExists(tables, input, new PrepTable());
                    if (propProgramTable != null)
                    {
                        typeface.PrepProgramBuffer = propProgramTable.programBuffer;
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


                //test
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
                return typeface;
            }
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

            TableEntry found;
            if (tables.TryGetTable(resultTable.Name, out found))
            {
                //found table name
                //check if we have read this table or not
                if (found is UnreadTableEntry)
                {
                    //set header before actal read
                    resultTable.Header = found.Header;
                    resultTable.LoadDataFrom(reader);
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
