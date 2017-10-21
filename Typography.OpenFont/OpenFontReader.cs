//Apache2, 2017, WinterDev
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
        public PreviewFontInfo(string fontName, string fontSubFam)
        {
            this.fontName = fontName;
            this.fontSubFamily = fontSubFam;
        }

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

                //translate...
                NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());
                return new PreviewFontInfo(nameEntry.FontName, nameEntry.FontSubFamily);
            }
        }


        public Typeface Read(Stream stream, ReadFlags readFlags = ReadFlags.Full)
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
                //------------------------------------------------------------------ 
                OS2Table os2Table = ReadTableIfExists(tables, input, new OS2Table());
                NameEntry nameEntry = ReadTableIfExists(tables, input, new NameEntry());


                Head header = ReadTableIfExists(tables, input, new Head());
                MaxProfile maximumProfile = ReadTableIfExists(tables, input, new MaxProfile());
                HorizontalHeader horizontalHeader = ReadTableIfExists(tables, input, new HorizontalHeader());
                HorizontalMetrics horizontalMetrics = ReadTableIfExists(tables, input, new HorizontalMetrics(horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount));

                //--------------
                Cmap cmaps = ReadTableIfExists(tables, input, new Cmap());
                GlyphLocations glyphLocations = ReadTableIfExists(tables, input, new GlyphLocations(maximumProfile.GlyphCount, header.WideGlyphLocations));
                Glyf glyf = ReadTableIfExists(tables, input, new Glyf(glyphLocations));
                //--------------
                Gasp gaspTable = ReadTableIfExists(tables, input, new Gasp());
                VerticalDeviceMatrics vdmx = ReadTableIfExists(tables, input, new VerticalDeviceMatrics());
                //--------------
                PostTable postTable = ReadTableIfExists(tables, input, new PostTable());
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
                    VerticalMatric vmtx = ReadTableIfExists(tables, input, new VerticalMatric(vhea.NumOfLongVerMatrics));
                }

                EBLCTable fontBmpTable = ReadTableIfExists(tables, input, new EBLCTable());
                //---------------------------------------------
                //about truetype instruction init 

                //--------------------------------------------- 
                var typeface = new Typeface(
                    nameEntry,
                    header.Bounds,
                    header.UnitsPerEm,
                    glyf.Glyphs,
                    cmaps.CharMaps,
                    horizontalMetrics,
                    os2Table);
                //----------------------------
                typeface.KernTable = kern;
                typeface.GaspTable = gaspTable;
                typeface.MaxProfile = maximumProfile;
                //----------------------------
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
                //-------------------------
                typeface.LoadOpenFontLayoutInfo(
                    gdef,
                    gsub,
                    gpos,
                    baseTable,
                    colr,
                    cpal);
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
                    //then reaplce
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
