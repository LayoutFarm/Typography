//MIT, 2019-present, WinterDev
//----------------------------------- 

using System;
using System.Collections.Generic;
using System.IO;
using PixelFarm.Drawing;

namespace PixelFarm.CpuBlit.BitmapAtlas
{
    /// <summary>
    /// for build multiple font atlas
    /// </summary>
    public class MultiGlyphSizeBitmapAtlasBuilder
    {
        List<TempMerginhAtlasInfo> _atlasList = new List<TempMerginhAtlasInfo>();

        class TempMerginhAtlasInfo
        {
            public int fontKey;
            public string simpleFontAtlasFile;
            public string imgFile;
            public BitmapAtlasFile fontAtlasFile;
            public Dictionary<ushort, TextureGlyphMapData> NewCloneLocations;
            public RequestFont reqFont;
            public TextureKind textureKind;
            public Dictionary<string, ushort> ImgUrlDict;

        }

        public void AddSimpleAtlasFile(RequestFont reqFont,
            string bitmapAtlasFile, string imgFile, TextureKind textureKind)
        {
            //TODO: use 'File' provider to access system file
            var fontAtlasFile = new BitmapAtlasFile();
            using (FileStream fs = new FileStream(bitmapAtlasFile, FileMode.Open))
            {
                fontAtlasFile.Read(fs);
            }

            var simpleFontAtlasInfo = new TempMerginhAtlasInfo()
            {
                reqFont = reqFont,
                simpleFontAtlasFile = bitmapAtlasFile,
                imgFile = imgFile,
                fontAtlasFile = fontAtlasFile,
                textureKind = textureKind

            };
            _atlasList.Add(simpleFontAtlasInfo);
        }
        public void BuildMultiFontSize(string multiFontSizeAtlasFilename, string imgOutputFilename)
        {
            //merge to the new one
            //1. ensure same atlas width
            int atlasW = 0;
            int j = _atlasList.Count;
            int totalHeight = 0;

            const int interAtlasSpace = 2;
            for (int i = 0; i < j; ++i)
            {
                TempMerginhAtlasInfo atlasInfo = _atlasList[i];
                SimpleBitmapAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];
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
                TempMerginhAtlasInfo atlasInfo = _atlasList[i];
                SimpleBitmapAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];
                offsetFromBottoms[i] = offsetFromBottom;
                offsetFromBottom += fontAtlas.Height + interAtlasSpace;
            }
            //--------------------------------------------
            //merge all img to one
            int top = 0;
            using (MemBitmap memBitmap = new MemBitmap(atlasW, totalHeight))
            {
                AggPainter painter = AggPainter.Create(memBitmap);
                for (int i = 0; i < j; ++i)
                {
                    TempMerginhAtlasInfo atlasInfo = _atlasList[i];
                    BitmapAtlasFile atlasFile = atlasInfo.fontAtlasFile;
                    SimpleBitmapAtlas fontAtlas = atlasInfo.fontAtlasFile.ResultSimpleFontAtlasList[0];

                    atlasInfo.NewCloneLocations = SimpleBitmapAtlas.CloneLocationWithOffset(fontAtlas, 0, offsetFromBottoms[i]);
                    atlasInfo.ImgUrlDict = fontAtlas.ImgUrlDict;

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
            //TODO: use 'File' provider to access system file
            using (FileStream fs = new FileStream(multiFontSizeAtlasFilename, FileMode.Create))
            using (BinaryWriter w = new BinaryWriter(fs))
            {
                //-----------
                //overview
                //total img info
                BitmapAtlasFile fontAtlasFile = new BitmapAtlasFile();
                fontAtlasFile.StartWrite(fs);

                //1. simple atlas count
                fontAtlasFile.WriteOverviewMultiSizeFontInfo((ushort)j);
                //2. 
                for (int i = 0; i < j; ++i)
                {
                    TempMerginhAtlasInfo atlasInfo = _atlasList[i];


                    RequestFont reqFont = atlasInfo.reqFont;
                    fontAtlasFile.WriteOverviewFontInfo(reqFont.Name, reqFont.FontKey, reqFont.SizeInPoints);//size in points

                    fontAtlasFile.WriteTotalImageInfo(
                        (ushort)atlasW,
                        (ushort)top,
                        4,
                        atlasInfo.textureKind);
                    //

                    fontAtlasFile.WriteGlyphList(atlasInfo.NewCloneLocations);

                    if (atlasInfo.ImgUrlDict != null)
                    {
                        fontAtlasFile.WriteImgUrlDict(atlasInfo.ImgUrlDict);
                    }
                }
                fontAtlasFile.EndWrite();
            }
        }


    }

}