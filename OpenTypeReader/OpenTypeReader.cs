//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System;
using System.IO;
using NRasterizer.IO;
using NRasterizer.Tables;
namespace NRasterizer
{
    public class OpenTypeReader
    {
        /// <summary>
        /// read only name entry
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public InstalledFont ReadPreview(Stream stream)
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
                return new InstalledFont(nameEntry.FontName, nameEntry.FontSubFamily);
            }
        }
        public InstalledFont ReadPreview(string fontFileName)
        {
            using (var fs = new FileStream(fontFileName, FileMode.Open))
            {
                return ReadPreview(fs);
            }
        }

        public Typeface Read(Stream stream)
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
                Head header = ReadTableIfExists(tables, input, new Head());
                MaxProfile maximumProfile = ReadTableIfExists(tables, input, new MaxProfile());
                GlyphLocations glyphLocations = ReadTableIfExists(tables, input, new GlyphLocations(maximumProfile.GlyphCount, header.WideGlyphLocations));
                Glyf glyf = ReadTableIfExists(tables, input, new Glyf(glyphLocations));
                Cmap cmaps = ReadTableIfExists(tables, input, new Cmap());
                HorizontalHeader horizontalHeader = ReadTableIfExists(tables, input, new HorizontalHeader());
                HorizontalMetrics horizontalMetrics = ReadTableIfExists(tables, input, new HorizontalMetrics(horizontalHeader.HorizontalMetricsCount, maximumProfile.GlyphCount));
                Kern kern = ReadTableIfExists(tables, input, new Kern());

                return new Typeface(
                    nameEntry,
                    header.Bounds,
                    header.UnitsPerEm,
                    glyf.Glyphs,
                    cmaps.CharMaps,
                    horizontalMetrics,
                    kern);
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
