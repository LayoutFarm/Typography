//MIT, 2016-present, WinterDev
//----------------------------------- 

using System.Collections.Generic;

using PixelFarm.Drawing.Fonts;

using PixelFarm.Contours;

namespace Typography.Rendering
{

    public class SimpleFontAtlasBuilder
    {
        PixelFarm.CpuBlit.MemBitmap _latestGenGlyphImage;
        Dictionary<ushort, CacheGlyph> _glyphs = new Dictionary<ushort, CacheGlyph>();

        public SimpleFontAtlasBuilder()
        {
            SpaceCompactOption = CompactOption.BinPack; //default
            MaxAtlasWidth = 800;
        }
        public int MaxAtlasWidth { get; set; }
        public PixelFarm.Drawing.BitmapAtlas.TextureKind TextureKind { get; private set; }
        public float FontSizeInPoints { get; private set; }
        public string FontFilename { get; set; }
        public int FontKey { get; set; }
        public CompactOption SpaceCompactOption { get; set; }
        //
        public enum CompactOption
        {
            None,
            BinPack,
            ArrangeByHeight
        }

        /// <summary>
        /// add or replace
        /// </summary>
        /// <param name="glyphIndex"></param>
        /// <param name="img"></param>
        public void AddGlyph(ushort glyphIndex, GlyphImage img)
        {
            _glyphs[glyphIndex] = new CacheGlyph(glyphIndex, img);
        }

        public void SetAtlasInfo(PixelFarm.Drawing.BitmapAtlas.TextureKind textureKind, float fontSizeInPts)
        {
            this.TextureKind = textureKind;
            this.FontSizeInPoints = fontSizeInPts;
        }
        public PixelFarm.CpuBlit.MemBitmap BuildSingleImage()
        {
            //1. add to list 
            var glyphList = new List<CacheGlyph>(_glyphs.Count);
            foreach (CacheGlyph glyphImg in _glyphs.Values)
            {
                //sort data
                glyphList.Add(glyphImg);
            }

            int totalMaxLim = MaxAtlasWidth;
            int maxRowHeight = 0;
            int currentY = 0;
            int currentX = 0;

            switch (this.SpaceCompactOption)
            {
                default:
                    throw new System.NotSupportedException();
                case CompactOption.BinPack:
                    {
                        //2. sort by glyph width
                        glyphList.Sort((a, b) =>
                        {
                            return a.img.Width.CompareTo(b.img.Width);
                        });
                        //3. layout 
                        for (int i = glyphList.Count - 1; i >= 0; --i)
                        {
                            CacheGlyph g = glyphList[i];
                            if (g.img.Height > maxRowHeight)
                            {
                                maxRowHeight = g.img.Height;
                            }
                            if (currentX + g.img.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.img.Width, g.img.Height);
                            currentX += g.img.Width;
                        }

                    }
                    break;
                case CompactOption.ArrangeByHeight:
                    {
                        //2. sort by height
                        glyphList.Sort((a, b) =>
                        {
                            return a.img.Height.CompareTo(b.img.Height);
                        });
                        //3. layout 
                        int glyphCount = glyphList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            CacheGlyph g = glyphList[i];
                            if (g.img.Height > maxRowHeight)
                            {
                                maxRowHeight = g.img.Height;
                            }
                            if (currentX + g.img.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                                maxRowHeight = g.img.Height;//reset, after start new row
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.img.Width, g.img.Height);
                            currentX += g.img.Width;
                        }

                    }
                    break;
                case CompactOption.None:
                    {
                        //3. layout 
                        int glyphCount = glyphList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            CacheGlyph g = glyphList[i];
                            if (g.img.Height > maxRowHeight)
                            {
                                maxRowHeight = g.img.Height;
                            }
                            if (currentX + g.img.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                                maxRowHeight = g.img.Height;//reset, after start new row
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.img.Width, g.img.Height);
                            currentX += g.img.Width;
                        }
                    }
                    break;
            }

            currentY += maxRowHeight;
            int imgH = currentY;
            // -------------------------------
            //compact image location
            // TODO: review performance here again***

            int totalImgWidth = totalMaxLim;
            if (SpaceCompactOption == CompactOption.BinPack) //again here?
            {
                totalImgWidth = 0;//reset
                //use bin packer
                BinPacker binPacker = new BinPacker(totalMaxLim, currentY);
                for (int i = glyphList.Count - 1; i >= 0; --i)
                {
                    CacheGlyph g = glyphList[i];
                    BinPackRect newRect = binPacker.Insert(g.img.Width, g.img.Height);
                    g.area = new Rectangle(newRect.X, newRect.Y, g.img.Width, g.img.Height);


                    //recalculate proper max midth again, after arrange and compact space
                    if (newRect.Right > totalImgWidth)
                    {
                        totalImgWidth = newRect.Right;
                    }
                }
            }
            // ------------------------------- 
            //4. create a mergeBmpBuffer
            //please note that original glyph image is head-down (Y axis)
            //so we will flip-Y axis again in step 5.
            int[] mergeBmpBuffer = new int[totalImgWidth * imgH];
            if (SpaceCompactOption == CompactOption.BinPack) //again here?
            {
                for (int i = glyphList.Count - 1; i >= 0; --i)
                {
                    CacheGlyph g = glyphList[i];
                    //copy glyph image buffer to specific area of final result buffer
                    GlyphImage img = g.img;
                    CopyToDest(img.GetImageBuffer(), img.Width, img.Height, mergeBmpBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }
            }
            else
            {
                int glyphCount = glyphList.Count;
                for (int i = 0; i < glyphCount; ++i)
                {
                    CacheGlyph g = glyphList[i];
                    //copy glyph image buffer to specific area of final result buffer
                    GlyphImage img = g.img;
                    CopyToDest(img.GetImageBuffer(), img.Width, img.Height, mergeBmpBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }
            }

            //5. since the mergeBmpBuffer is head-down
            //we will flipY axis again to head-up, the head-up img is easy to read and debug

            int[] totalBufferFlipY = new int[mergeBmpBuffer.Length];
            int srcRowIndex = imgH - 1;
            int strideInBytes = totalImgWidth * 4;//32 argb

            for (int i = 0; i < imgH; ++i)
            {
                //copy each row from src to dst
                System.Buffer.BlockCopy(mergeBmpBuffer, strideInBytes * srcRowIndex, totalBufferFlipY, strideInBytes * i, strideInBytes);
                srcRowIndex--;
            }

            //flipY on atlas info too
            for (int i = 0; i < glyphList.Count; ++i)
            {
                CacheGlyph g = glyphList[i];
                Rectangle rect = g.area;
                g.area = new Rectangle(rect.X, imgH - (rect.Y + rect.Height), rect.Width, rect.Height);
            }


            //***
            //6. generate final output
            //TODO: rename GlyphImage to another name to distinquist
            //between small glyph and a large one

            return _latestGenGlyphImage = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(totalImgWidth, imgH, totalBufferFlipY);

        }
        public void SaveAtlasInfo(System.IO.Stream outputStream)
        {

            if (_latestGenGlyphImage == null)
            {
                throw new System.Exception("");
            }

            FontAtlasFile fontAtlasFile = new FontAtlasFile();
            fontAtlasFile.StartWrite(outputStream);
            fontAtlasFile.WriteOverviewFontInfo(FontFilename, FontKey, FontSizeInPoints);

            fontAtlasFile.WriteTotalImageInfo(
                (ushort)_latestGenGlyphImage.Width,
                (ushort)_latestGenGlyphImage.Height, 4,
                this.TextureKind);
            //
            //
            fontAtlasFile.WriteGlyphList(_glyphs);
            fontAtlasFile.EndWrite();
        }

        public SimpleFontAtlas CreateSimpleFontAtlas()
        {
            SimpleFontAtlas simpleFontAtlas = new SimpleFontAtlas();
            simpleFontAtlas.TextureKind = this.TextureKind;
            simpleFontAtlas.OriginalFontSizePts = this.FontSizeInPoints;
            foreach (CacheGlyph cacheGlyph in _glyphs.Values)
            {

                Rectangle area = cacheGlyph.area;
                TextureGlyphMapData glyphData = new TextureGlyphMapData();

                glyphData.Width = cacheGlyph.img.Width;
                glyphData.Left = area.X;
                glyphData.Top = area.Top;
                glyphData.Height = area.Height;

                glyphData.TextureXOffset = cacheGlyph.img.TextureOffsetX;
                glyphData.TextureYOffset = cacheGlyph.img.TextureOffsetY;


                simpleFontAtlas.AddGlyph(cacheGlyph.glyphIndex, glyphData);
            }

            return simpleFontAtlas;
        }

        public List<SimpleFontAtlas> LoadFontAtlasInfo(System.IO.Stream dataStream)
        {
            FontAtlasFile atlasFile = new FontAtlasFile();
            //read font atlas from stream data
            atlasFile.Read(dataStream);
            return atlasFile.ResultSimpleFontAtlasList;
        }

        static void CopyToDest(int[] srcPixels, int srcW, int srcH, int[] targetPixels, int targetX, int targetY, int totalTargetWidth)
        {
            int srcIndex = 0;
            unsafe
            {

                for (int r = 0; r < srcH; ++r)
                {
                    //for each row 
                    int targetP = ((targetY + r) * totalTargetWidth) + targetX;
                    for (int c = 0; c < srcW; ++c)
                    {
                        targetPixels[targetP] = srcPixels[srcIndex];
                        srcIndex++;
                        targetP++;
                    }
                }
            }
        }
    }
}