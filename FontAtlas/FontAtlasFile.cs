//MIT, 2018-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
using Typography.Rendering;
namespace PixelFarm.Drawing.Fonts
{


    public class FontAtlasFile
    {
        //Typography's custom font atlas file        
        SimpleFontAtlas _atlas;
        enum FontTextureObjectKind : ushort
        {
            End,
            TotalImageInfo,
            GlyphList,
            OverviewFontInfo,
        }
        public SimpleFontAtlas Result
        {
            get { return _atlas; }
        }

        public void Read(Stream inputStream)
        {
            //custom font atlas file             
            _atlas = new SimpleFontAtlas();
            using (BinaryReader reader = new BinaryReader(inputStream, System.Text.Encoding.UTF8))
            {
                //1. version
                ushort fileversion = reader.ReadUInt16();
                bool stop = false;
                while (!stop)
                {
                    //2. read object kind
                    FontTextureObjectKind objKind = (FontTextureObjectKind)reader.ReadUInt16();
                    switch (objKind)
                    {
                        default: throw new NotSupportedException();
                        case FontTextureObjectKind.OverviewFontInfo:
                            ReadOverviewFontInfo(reader);
                            break;
                        case FontTextureObjectKind.End:
                            stop = true;
                            break;
                        case FontTextureObjectKind.GlyphList:
                            ReadGlyphList(reader);
                            break;
                        case FontTextureObjectKind.TotalImageInfo:
                            ReadTotalImageInfo(reader);
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
                //1. codepoint
                var glyphMap = new TextureGlyphMapData();
                ushort glyphIndex = reader.ReadUInt16();
                //2. area, left,top,width,height

                glyphMap.Left = reader.ReadUInt16();
                glyphMap.Top = reader.ReadUInt16();
                glyphMap.Width = reader.ReadUInt16();
                glyphMap.Height = reader.ReadUInt16();
                //3. border x,y
                int borderXY = reader.ReadUInt16();
                glyphMap.BorderX = borderXY & 0xff;
                glyphMap.BorderY = borderXY >> 8;
                //---------------------------------------
                //4. texture offset
                glyphMap.TextureXOffset = reader.ReadSingle();
                glyphMap.TextureYOffset = reader.ReadSingle();

                //---------------------------------------
                _atlas.AddGlyph(glyphIndex, glyphMap);
            }
        }

        void ReadOverviewFontInfo(BinaryReader reader)
        {
            _atlas.FontFilename = reader.ReadString();
            _atlas.OriginalFontSizePts = reader.ReadSingle();
        }

        //------------------------------------------------------------
        BinaryWriter _writer;
        public void StartWrite(Stream outputStream)
        {
            _writer = new BinaryWriter(outputStream, System.Text.Encoding.UTF8);
            //version            
            _writer.Write((ushort)1);
        }
        public void EndWrite()
        {
            //write end marker
            _writer.Write((ushort)FontTextureObjectKind.End);
            //
            _writer.Flush();
            _writer = null;
        }

        public void WriteOverviewFontInfo(string fontFileName, float sizeInPt)
        {
            _writer.Write((ushort)FontTextureObjectKind.OverviewFontInfo);
            if (fontFileName == null)
            {
                fontFileName = "";
            }
            _writer.Write(fontFileName);
            _writer.Write(sizeInPt);
        }
        public void WriteTotalImageInfo(ushort width, ushort height, byte colorComponent, TextureKind textureKind)
        {
            _writer.Write((ushort)FontTextureObjectKind.TotalImageInfo);
            _writer.Write(width);
            _writer.Write(height);
            _writer.Write(colorComponent);
            _writer.Write((byte)textureKind);
        }
        public void WriteGlyphList(Dictionary<ushort, CacheGlyph> glyphs)
        {
            _writer.Write((ushort)FontTextureObjectKind.GlyphList);
            //total number
            int totalNum = glyphs.Count;
#if DEBUG
            if (totalNum >= ushort.MaxValue)
            {
                throw new NotSupportedException();
            }
#endif

            _writer.Write((ushort)totalNum);
            // 
            foreach (CacheGlyph g in glyphs.Values)
            {
                //1. code point
                _writer.Write((ushort)g.glyphIndex);
                //2. area, left,top,width,height
                _writer.Write((ushort)g.area.Left);
                _writer.Write((ushort)g.area.Top);
                _writer.Write((ushort)g.area.Width);
                _writer.Write((ushort)g.area.Height);

                //3. border x,y

                if ((g.borderX > byte.MaxValue) || (g.borderY > byte.MaxValue))
                {
                    throw new NotSupportedException();
                }

                _writer.Write((ushort)(((g.borderY & 0xff) << 8) | (g.borderX & 0xff)));

                //4. texture offset                
                GlyphImage img = g.img;
                _writer.Write((float)img.TextureOffsetX);
                _writer.Write((float)img.TextureOffsetY);


            }
        }
    }

}