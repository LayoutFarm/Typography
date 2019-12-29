//MIT, 2018-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;
namespace PixelFarm.Drawing.BitmapAtlas
{
    public class BitmapAtlasFile
    {
        SimpleBitmaptAtlas _atlas;
        enum ObjectKind : ushort
        {
            End,
            TotalImageInfo,
            GlyphList,
            OverviewFontInfo,
            //
            OverviewBitmapInfo,
            BmpItemList,
            ImgUrlDic,
        }
        public SimpleBitmaptAtlas Result => _atlas;

        public void Read(Stream inputStream)
        {
            //custom font atlas file             
            _atlas = new SimpleBitmaptAtlas();
            using (BinaryReader reader = new BinaryReader(inputStream, System.Text.Encoding.UTF8))
            {
                //1. version
                ushort fileversion = reader.ReadUInt16();
                bool stop = false;
                while (!stop)
                {
                    //2. read object kind
                    ObjectKind objKind = (ObjectKind)reader.ReadUInt16();
                    switch (objKind)
                    {
                        default: throw new NotSupportedException();
                        case ObjectKind.OverviewBitmapInfo:
                            ReadOverviewBitmapInfo(reader);
                            break;
                        case ObjectKind.OverviewFontInfo:
                            //ReadOverviewFontInfo(reader);
                            throw new NotSupportedException();
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
                //1. codepoint
                var glyphMap = new BitmapMapData();
                ushort glyphIndex = reader.ReadUInt16();
                //2. area, left,top,width,height

                glyphMap.Left = reader.ReadUInt16();
                glyphMap.Top = reader.ReadUInt16();
                glyphMap.Width = reader.ReadUInt16();
                glyphMap.Height = reader.ReadUInt16();
                //---------------------------------------
                //3. texture offset
                glyphMap.TextureXOffset = reader.ReadInt16();
                glyphMap.TextureYOffset = reader.ReadInt16();

                //---------------------------------------
                _atlas.AddBitmapMapData(glyphIndex, glyphMap);
            }
        }
        void ReadOverviewBitmapInfo(BinaryReader reader)
        {
            ushort utf8StrLen = reader.ReadUInt16();
            _atlas.BitmapFilename = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(utf8StrLen));
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
        //------------------------------------------------------------
        BinaryWriter _writer;
        internal void StartWrite(Stream outputStream)
        {
            _writer = new BinaryWriter(outputStream, System.Text.Encoding.UTF8);
            //version            
            _writer.Write((ushort)1);
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
        internal void WriteOverviewBitmapInfo(string bmpfilename)
        {
            _writer.Write((ushort)ObjectKind.OverviewBitmapInfo);
            if (bmpfilename == null)
            {
                bmpfilename = "";
            }
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(bmpfilename);
            _writer.Write((ushort)buffer.Length); //***ushort *
            _writer.Write(buffer);

        }
        internal void WriteTotalImageInfo(ushort width, ushort height, byte colorComponent, TextureKind textureKind)
        {
            _writer.Write((ushort)ObjectKind.TotalImageInfo);
            _writer.Write(width);
            _writer.Write(height);
            _writer.Write(colorComponent);
            _writer.Write((byte)textureKind);
        }
        internal void WriteGlyphList(Dictionary<ushort, CacheBmp> glyphs)
        {
            _writer.Write((ushort)ObjectKind.GlyphList);
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
            foreach (CacheBmp g in glyphs.Values)
            {
                //1. code point
                _writer.Write((ushort)g.imgIndex);
                //2. area, left,top,width,height
                _writer.Write((ushort)g.area.Left);
                _writer.Write((ushort)g.area.Top);
                _writer.Write((ushort)g.area.Width);
                _writer.Write((ushort)g.area.Height);


                //3. texture offset                
                AtlasItemImage img = g.img;
                _writer.Write((short)img.TextureOffsetX);//short
                _writer.Write((short)img.TextureOffsetY);//short

            }
        }
    }

}