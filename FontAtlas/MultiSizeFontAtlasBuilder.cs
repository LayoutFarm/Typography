//MIT, 2019-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;
using System.IO;
using Typography.Rendering;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;

namespace PixelFarm.Drawing.Fonts
{
    public class MultiSizeFontAtlasBuilder
    {

        List<SimpleFontAtlasInfo> _simpleFontInfoList = new List<SimpleFontAtlasInfo>();
        class SimpleFontAtlasInfo
        {
            public int fontKey;
            public string simpleFontAtlasFile;
            public string imgFile;
            public FontAtlasFile fontAtlasFile;
            public Dictionary<ushort, TextureGlyphMapData> NewCloneLocations;
            public RequestFont reqFont;
            public PixelFarm.Drawing.BitmapAtlas.TextureKind textureKind;
        }
        public void AddSimpleFontAtlasFile(RequestFont reqFont,
            string simpleFontAtlasFile, string imgFile, PixelFarm.Drawing.BitmapAtlas.TextureKind textureKind)
        {

            var fontAtlasFile = new FontAtlasFile();
            using (FileStream fs = new FileStream(simpleFontAtlasFile, FileMode.Open))
            {
                fontAtlasFile.Read(fs);
            }

            var simpleFontAtlasInfo = new SimpleFontAtlasInfo()
            {
                reqFont = reqFont,
                simpleFontAtlasFile = simpleFontAtlasFile,
                imgFile = imgFile,
                fontAtlasFile = fontAtlasFile,
                textureKind = textureKind

            };
            _simpleFontInfoList.Add(simpleFontAtlasInfo);
        }
        public void BuildMultiFontSize(string multiFontSizrAtlasFilename, string imgOutputFilename)
        {
            //merge to the new one
            //1. ensure same atlas width
            int atlasW = 0;
            int j = _simpleFontInfoList.Count;
            int totalHeight = 0;

            const int interAtlasSpace = 2;
            for (int i = 0; i < j; ++i)
            {
                SimpleFontAtlasInfo atlasInfo = _simpleFontInfoList[i];
                SimpleFontAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];
                totalHeight += fontAtlas.Height + interAtlasSpace;
                if (i == 0)
                {
                    atlasW = fontAtlas.Width;
                }
                else
                {
                    if (atlasW != fontAtlas.Width)
                    {
                        throw new NotSupportedException();
                    }
                }
            }
            //--------------------------------------------
            //in this version, the glyph offsetY is measure from bottom***
            int[] offsetFromBottoms = new int[j];
            int offsetFromBottom = interAtlasSpace;//start offset 
            for (int i = j - 1; i >= 0; --i)
            {
                SimpleFontAtlasInfo atlasInfo = _simpleFontInfoList[i];
                SimpleFontAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];
                offsetFromBottoms[i] = offsetFromBottom;
                offsetFromBottom += fontAtlas.Height + interAtlasSpace;
            }
            //--------------------------------------------
            //merge all img to one
            int top = 0;
            using (PixelFarm.CpuBlit.MemBitmap memBitmap = new CpuBlit.MemBitmap(atlasW, totalHeight))
            {
                PixelFarm.CpuBlit.AggPainter painter = PixelFarm.CpuBlit.AggPainter.Create(memBitmap);
                for (int i = 0; i < j; ++i)
                {
                    SimpleFontAtlasInfo atlasInfo = _simpleFontInfoList[i];
                    FontAtlasFile atlasFile = atlasInfo.fontAtlasFile;
                    SimpleFontAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];

                    atlasInfo.NewCloneLocations = SimpleFontAtlas.CloneLocationWithOffset(fontAtlas, 0, offsetFromBottoms[i]);

                    using (System.IO.Stream fontImgStream = PixelFarm.Platforms.StorageService.Provider.ReadDataStream(atlasInfo.imgFile))
                    using (PixelFarm.CpuBlit.MemBitmap atlasBmp = PixelFarm.CpuBlit.MemBitmap.LoadBitmap(fontImgStream))
                    {
                        painter.DrawImage(atlasBmp, 0, top);
                        top += atlasBmp.Height + interAtlasSpace;
                    }

                }
                memBitmap.SaveImage(imgOutputFilename);
            }
            //--------------------------------------------
            //save merged font atlas
            using (FileStream fs = new FileStream(multiFontSizrAtlasFilename, FileMode.Create))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                //-----------
                //overview
                //total img info
                FontAtlasFile fontAtlasFile = new FontAtlasFile();
                fontAtlasFile.StartWrite(fs);

                //1. simple atlas count
                fontAtlasFile.WriteOverviewMultiSizeFontInfo((ushort)j);
                //2. 
                for (int i = 0; i < j; ++i)
                {
                    SimpleFontAtlasInfo atlasInfo = _simpleFontInfoList[i];
                    RequestFont reqFont = atlasInfo.reqFont;
                    fontAtlasFile.WriteOverviewFontInfo(reqFont.Name, reqFont.FontKey, reqFont.SizeInPoints);//size in points

                    fontAtlasFile.WriteTotalImageInfo(
                        (ushort)atlasW,
                        (ushort)top,
                        4,
                        atlasInfo.textureKind);
                    //
                    //
                    fontAtlasFile.WriteGlyphList(atlasInfo.NewCloneLocations);
                }
                fontAtlasFile.EndWrite();
            }
        }


    }

}