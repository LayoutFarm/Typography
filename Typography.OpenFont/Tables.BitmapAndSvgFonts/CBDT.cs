//MIT, 2019-present, WinterDev
using System;
using System.IO;
using Typography.OpenFont.Tables.BitmapFonts;

namespace Typography.OpenFont.Tables
{
    //test font=> NotoColorEmoji.ttf

    //from https://docs.microsoft.com/en-us/typography/opentype/spec/cbdt

    //Table structure

    //The CBDT table is used to embed color bitmap glyph data. It is used together with the CBLC table,
    //which provides embedded bitmap locators.
    //The formats of these two tables are backward compatible with the EBDT and EBLC tables
    //used for embedded monochrome and grayscale bitmaps.

    //The CBDT table begins with a header containing simply the table version number.
    //Type 	    Name 	        Description
    //uint16 	majorVersion 	Major version of the CBDT table, = 3.
    //uint16 	minorVersion 	Minor version of the CBDT table, = 0.

    //Note that the first version of the CBDT table is 3.0.

    //The rest of the CBDT table is a collection of bitmap data.
    //The data can be presented in three possible formats,
    //indicated by information in the CBLC table.
    //Some of the formats contain metric information plus image data, 
    //and other formats contain only the image data. Long word alignment is not required for these subtables;
    //byte alignment is sufficient.
    class CBDT : TableEntry, IDisposable
    {
        public const string Name = "CBDT";

        GlyphBitmapDataFmt17 _format17 = new GlyphBitmapDataFmt17();
        GlyphBitmapDataFmt18 _format18 = new GlyphBitmapDataFmt18();
        GlyphBitmapDataFmt19 _format19 = new GlyphBitmapDataFmt19();

        // BinaryReaders also dispose their underlying streams
        IO.ByteOrderSwappingBinaryReader? _binReader; // underlying stream contains image data
        public void Dispose()
        {
            if (_binReader != null)
            {
                ((IDisposable)_binReader).Dispose();
                _binReader = null;
            }
        }
        internal CBDT(TableHeader header, BinaryReader reader) : base(header, reader)
        {

            //we will read this later
            byte[] data = reader.ReadBytes((int)this.Header.Length);//***
            _binReader = new IO.ByteOrderSwappingBinaryReader(new MemoryStream(data));

            //ushort majorVersion = reader.ReadUInt16();
            //ushort minorVersion = reader.ReadUInt16();
            ////--------------
            //this.Header.Length;
        }
        public void FillGlyphInfo(Glyph glyph)
        {
            if (_binReader is null) throw new ObjectDisposedException(nameof(_binReader));
            //int srcOffset, int srcLen, int srcFormat,
            if (!(glyph.BitmapSVGInfo is { } bitmapSvg))
                throw new NotSupportedException("Only Bitmap/SVG glyphs are supported");
            _binReader.BaseStream.Position = bitmapSvg.streamOffset;
            switch (bitmapSvg.imgFormat)
            {
                case 17: _format17.FillGlyphInfo(_binReader, glyph); break;
                case 18: _format18.FillGlyphInfo(_binReader, glyph); break;
                case 19: _format19.FillGlyphInfo(_binReader, glyph); break;
                default:
                    throw new NotSupportedException();
            }
        }
        public void CopyBitmapContent(Glyph glyph, Stream outputStream)
        {
            if (_binReader is null) throw new ObjectDisposedException(nameof(_binReader));
            //1 
            if (!(glyph.BitmapSVGInfo is { } bitmapSvg))
                throw new NotSupportedException("Only Bitmap/SVG glyphs are supported");
            _binReader.BaseStream.Position = bitmapSvg.streamOffset;
            switch (bitmapSvg.imgFormat)
            {
                case 17: _format17.ReadRawBitmap(_binReader, glyph, outputStream); break;
                case 18: _format18.ReadRawBitmap(_binReader, glyph, outputStream); break;
                case 19: _format19.ReadRawBitmap(_binReader, glyph, outputStream); break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}