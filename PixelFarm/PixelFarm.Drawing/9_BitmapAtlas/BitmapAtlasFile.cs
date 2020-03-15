//MIT, 2018-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;

namespace PixelFarm.CpuBlit.BitmapAtlas
{


    public class BitmapAtlasFile
    {
        SimpleBitmapAtlas _atlas;

        enum ObjectKind : ushort
        {
            End,
            TotalImageInfo,
            GlyphList,
            OverviewFontInfo,
            OverviewMultiSizeFontInfo,

            OverviewBitmapInfo,
            ImgUrlDic,
        }


        public List<SimpleBitmapAtlas> ResultSimpleFontAtlasList { get; private set; }

        public void Read(Stream inputStream)
        {
            //custom font atlas file                        
            ResultSimpleFontAtlasList = new List<SimpleBitmapAtlas>();
            using (BinaryReader reader = new BinaryReader(inputStream, System.Text.Encoding.UTF8))
            {
                //1. version
                ushort fileversion = reader.ReadUInt16();
                bool stop = false;
                int listCount = 0;

                while (!stop)
                {
                    //2. read object kind
                    ObjectKind objKind = (ObjectKind)reader.ReadUInt16();
                    switch (objKind)
                    {
                        default: throw new NotSupportedException();
                        case ObjectKind.OverviewMultiSizeFontInfo:
                            listCount = reader.ReadUInt16();
                            break;
                        case ObjectKind.OverviewFontInfo:
                            //start new atlas
                            _atlas = new SimpleBitmapAtlas();
                            ResultSimpleFontAtlasList.Add(_atlas);
                            ReadOverviewFontInfo(reader);
                            break;
                        case ObjectKind.End:
                            stop = true;
                            break;
                        case ObjectKind.GlyphList:
                            ReadGlyphList(reader);
                            break;
                        case ObjectKind.TotalImageInfo:
                            ReadTotalImageInfo(reader);
                            break;
                        case ObjectKind.ImgUrlDic:
                            ReadImgUrlDict(reader);
                            break;
                    }
                }

            }
        }

        void ReadTotalImageInfo(BinaryReader reader)
        {
            //read total
            //this version compose of width and height
            _atlas.Width = reader.ReadUInt16();
            _atlas.Height = reader.ReadUInt16();
            byte colorComponent = reader.ReadByte(); //1 or 4
            _atlas.TextureKind = (TextureKind)reader.ReadByte();
        }
        void ReadGlyphList(BinaryReader reader)
        {
            ushort glyphCount = reader.ReadUInt16();
            for (int i = 0; i < glyphCount; ++i)
            {
                //read each glyph map info

                var glyphMap = new TextureGlyphMapData();

                //1. glyph index
                ushort glyphIndex = reader.ReadUInt16();

                //2. area
                glyphMap.Left = reader.ReadUInt16();
                glyphMap.Top = reader.ReadUInt16();
                glyphMap.Width = reader.ReadUInt16();
                glyphMap.Height = reader.ReadUInt16();

                //3. texture offset
                glyphMap.TextureXOffset = reader.ReadInt16();
                glyphMap.TextureYOffset = reader.ReadInt16();

                _atlas.AddGlyph(glyphIndex, glyphMap);
            }
        }

        void ReadOverviewFontInfo(BinaryReader reader)
        {
            //read str len 
            _atlas.FontFilename = ReadLengthPrefixUtf8String(reader);
            _atlas.FontKey = reader.ReadInt32();
            _atlas.OriginalFontSizePts = reader.ReadSingle();
        }
        void ReadImgUrlDict(BinaryReader reader)
        {
            Dictionary<string, ushort> imgUrlDict = new Dictionary<string, ushort>();
            ushort count = reader.ReadUInt16();
            for (int i = 0; i < count; ++i)
            {

                ushort urlNameLen = reader.ReadUInt16();
                string urlName = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(urlNameLen));

                ushort index = reader.ReadUInt16();

                imgUrlDict.Add(urlName, index);
            }
            _atlas.ImgUrlDict = imgUrlDict;
        }
        static string ReadLengthPrefixUtf8String(BinaryReader reader)
        {
            ushort utf8BufferLen = reader.ReadUInt16();
            byte[] utf8Buffer = reader.ReadBytes(utf8BufferLen);
            return System.Text.Encoding.UTF8.GetString(utf8Buffer);
        }
        //------------------------------------------------------------
        BinaryWriter _writer;
        internal void StartWrite(Stream outputStream)
        {
            _writer = new BinaryWriter(outputStream, System.Text.Encoding.UTF8);
            //version            
            _writer.Write((ushort)2);
        }
        internal void EndWrite()
        {
            //write end marker
            _writer.Write((ushort)ObjectKind.End);
            //
            _writer.Flush();
            _writer = null;
        }
        internal void WriteImgUrlDict(Dictionary<string, ushort> imgUrlDict)
        {
            _writer.Write((ushort)ObjectKind.ImgUrlDic);
            //write key-value            
            int count = imgUrlDict.Count;
            _writer.Write((ushort)count);//***
            foreach (var kp in imgUrlDict)
            {
                //write string for img url (utf8)
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(kp.Key);
                _writer.Write((ushort)buffer.Length); //***ushort *
                _writer.Write(buffer);
                //
                _writer.Write((ushort)kp.Value);
            }
        }
        internal void WriteOverviewMultiSizeFontInfo(ushort count)
        {
            _writer.Write((ushort)ObjectKind.OverviewMultiSizeFontInfo);
            _writer.Write((ushort)count);
        }
        void WriteLengthPrefixUtf8String(string value)
        {
            byte[] utf8Buffer = System.Text.Encoding.UTF8.GetBytes(value);
            if (utf8Buffer.Length > ushort.MaxValue)
            {
                throw new NotSupportedException();
            }
            _writer.Write((ushort)utf8Buffer.Length);
            _writer.Write(utf8Buffer);
        }
        internal void WriteOverviewFontInfo(string fontFileName, int fontKey, float sizeInPt)
        {

            _writer.Write((ushort)ObjectKind.OverviewFontInfo);
            WriteLengthPrefixUtf8String(fontFileName);
            _writer.Write(fontKey);
            _writer.Write(sizeInPt);
        }
        internal void WriteTotalImageInfo(ushort width, ushort height, byte colorComponent, TextureKind textureKind)
        {
            _writer.Write((ushort)ObjectKind.TotalImageInfo);
            _writer.Write(width);
            _writer.Write(height);
            _writer.Write(colorComponent);
            _writer.Write((byte)textureKind);
        }
        internal void WriteGlyphList(Dictionary<ushort, RelocationAtlasItem> glyphs)
        {
            int totalNum = glyphs.Count;
#if DEBUG
            if (totalNum >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }
#endif

            //kind
            _writer.Write((ushort)ObjectKind.GlyphList);
            //count
            _writer.Write((ushort)totalNum);
            // 
            foreach (RelocationAtlasItem g in glyphs.Values)
            {
                //1. glyph index
                _writer.Write((ushort)g.atlasItem.UniqueInt16Name);

                //2. area
                _writer.Write((ushort)g.area.Left);
                _writer.Write((ushort)g.area.Top);
                _writer.Write((ushort)g.area.Width);
                _writer.Write((ushort)g.area.Height);

                //3. texture offset                
                BitmapAtlasItemSource img = g.atlasItem;
                _writer.Write((short)img.TextureXOffset);
                _writer.Write((short)img.TextureYOffset);
            }
        }
        //--------------------
        internal void WriteGlyphList(Dictionary<ushort, TextureGlyphMapData> glyphs)
        {
            //total number
            int totalNum = glyphs.Count;
#if DEBUG
            if (totalNum >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }
#endif

            _writer.Write((ushort)ObjectKind.GlyphList);
            //count
            _writer.Write((ushort)totalNum);
            // 
            foreach (var kp in glyphs)
            {
                ushort glyphIndex = kp.Key;
                TextureGlyphMapData g = kp.Value;
                //1. glyph index
                _writer.Write((ushort)glyphIndex);

                //2. area, left,top,width,height
                _writer.Write((ushort)g.Left);
                _writer.Write((ushort)g.Top);
                _writer.Write((ushort)g.Width);
                _writer.Write((ushort)g.Height);

                //3. texture offset                
                _writer.Write((short)g.TextureXOffset);//short
                _writer.Write((short)g.TextureYOffset);//short
            }
        }
    }

}