//MIT, 2018-present, WinterDev

using System;
using System.Collections.Generic;
using System.IO;

namespace PixelFarm.CpuBlit.BitmapAtlas
{

    public class BitmapAtlasFile
    {
        //one atlas file may contain more than 1 simple-atlas


        SimpleBitmapAtlas _atlas;

        enum ObjectKind : ushort
        {
            End,
            TotalImageInfo,
            GlyphList,
            OverviewFontInfo,
            FontScriptTags,

            OverviewMultiSizeFontInfo,
            OverviewBitmapInfo,
            ImgUrlDic,
        }


        public List<SimpleBitmapAtlas> AtlasList { get; private set; }

        public void Read(Stream inputStream)
        {
            //custom font atlas file                        
            AtlasList = new List<SimpleBitmapAtlas>();
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
                            AtlasList.Add(_atlas);
                            ReadOverviewFontInfo(reader);
                            break;
                        case ObjectKind.FontScriptTags:
                            ReadScriptTags(reader);
                            break;
                        case ObjectKind.End:
                            stop = true;
                            break;
                        case ObjectKind.GlyphList:
                            ReadAtlasItems(reader);
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
        void ReadAtlasItems(BinaryReader reader)
        {
            ushort itemCount = reader.ReadUInt16();
            for (int i = 0; i < itemCount; ++i)
            {
                //read each glyph map info

                var item = new AtlasItem(reader.ReadUInt16());

                //2. area
                item.Left = reader.ReadUInt16();
                item.Top = reader.ReadUInt16();
                item.Width = reader.ReadUInt16();
                item.Height = reader.ReadUInt16();

                //3. texture offset
                item.TextureXOffset = reader.ReadInt16();
                item.TextureYOffset = reader.ReadInt16();

                _atlas.AddAtlasItem(item);
            }
        }

        void ReadOverviewFontInfo(BinaryReader reader)
        {
            //read str len 
            _atlas.FontName = ReadLengthPrefixUtf8String(reader);
            int old_random_num = reader.ReadInt32();//unused 
            _atlas.SizeInPts = reader.ReadSingle();
        }
        void ReadScriptTags(BinaryReader reader)
        {
            ushort count = reader.ReadUInt16();
            for (int i = 0; i < count; ++i)
            {
                _atlas.ScriptTags.Add(reader.ReadUInt32());
            }
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
        internal void WriteOverviewFontInfo(string fontName, int fontKey, float sizeInPt)
        {
            _writer.Write((ushort)ObjectKind.OverviewFontInfo);
            WriteLengthPrefixUtf8String(fontName);
            _writer.Write(fontKey); //
            _writer.Write(sizeInPt); //
        }
        internal void WriteScriptTags(uint[] scriptTags)
        {
            if (scriptTags != null && scriptTags.Length > 0)
            {
                _writer.Write((ushort)ObjectKind.FontScriptTags);
                _writer.Write((ushort)scriptTags.Length);
                for (int i = 0; i < scriptTags.Length; ++i)
                {
                    _writer.Write(scriptTags[i]);
                }
            }
        }
        internal void WriteTotalImageInfo(ushort width, ushort height, byte colorComponent, TextureKind textureKind)
        {
            _writer.Write((ushort)ObjectKind.TotalImageInfo);
            _writer.Write(width);
            _writer.Write(height);
            _writer.Write(colorComponent);
            _writer.Write((byte)textureKind);
        }
        internal void WriteAtlasItems(Dictionary<ushort, BitmapAtlasItemSource> items)
        {
            int totalNum = items.Count;
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
            foreach (BitmapAtlasItemSource g in items.Values)
            {
                //1. unique uint16 name
                _writer.Write((ushort)g.UniqueInt16Name);

                //2. area
                _writer.Write((ushort)g.Area.Left);
                _writer.Write((ushort)g.Area.Top);
                _writer.Write((ushort)g.Area.Width);
                _writer.Write((ushort)g.Area.Height);

                //3. texture offset                

                _writer.Write((short)g.TextureXOffset);
                _writer.Write((short)g.TextureYOffset);
            }
        }
        //--------------------
        internal void WriteAtlasItems(Dictionary<ushort, AtlasItem> items)
        {
            //total number
            int totalNum = items.Count;
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
            foreach (var kp in items)
            {
                ushort uniqueName = kp.Key;
                AtlasItem g = kp.Value;
                //1. unique uint16 name
                _writer.Write((ushort)uniqueName);

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