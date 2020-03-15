//MIT, 2016-present, WinterDev
//----------------------------------- 

using System.Collections.Generic;
using PixelFarm.Drawing;
namespace PixelFarm.CpuBlit.BitmapAtlas
{
    public class SimpleBitmapAtlasBuilder
    {
        MemBitmap _latestGenGlyphImage;
        Dictionary<ushort, BitmapAtlasItemSource> _items = new Dictionary<ushort, BitmapAtlasItemSource>();

        public SimpleBitmapAtlasBuilder()
        {
            SpaceCompactOption = CompactOption.BinPack; //default
            MaxAtlasWidth = 800;
        }

        public int MaxAtlasWidth { get; set; }
        public TextureKind TextureKind { get; private set; }

        //information about font
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
        /// <param name="itemIndex"></param>
        /// <param name="img"></param>
        public void AddItemSource(BitmapAtlasItemSource img)
        {
            _items[img.UniqueInt16Name] = img;
        }
        public Dictionary<string, ushort> ImgUrlDict { get; set; }
        public void SetAtlasInfo(TextureKind textureKind, float fontSizeInPts)
        {
            this.TextureKind = textureKind;
            this.FontSizeInPoints = fontSizeInPts;
        }
        public MemBitmap BuildSingleImage(bool flipY)
        {
            //1. add to list 
            var itemList = new List<BitmapAtlasItemSource>(_items.Values);

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
                        itemList.Sort((a, b) => a.Width.CompareTo(b.Width));
                        //3. layout 
                        for (int i = itemList.Count - 1; i >= 0; --i)
                        {
                            BitmapAtlasItemSource g = itemList[i];
                            if (g.Height > maxRowHeight)
                            {
                                maxRowHeight = g.Height;
                            }
                            if (currentX + g.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.Width, g.Height);
                            currentX += g.Width;
                        }

                    }
                    break;
                case CompactOption.ArrangeByHeight:
                    {
                        //2. sort by height
                        itemList.Sort((a, b) => a.Height.CompareTo(b.Height));

                        //3. layout 
                        int glyphCount = itemList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            BitmapAtlasItemSource g = itemList[i];
                            if (g.Height > maxRowHeight)
                            {
                                maxRowHeight = g.Height;
                            }
                            if (currentX + g.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                                maxRowHeight = g.Height;//reset, after start new row
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.Width, g.Height);
                            currentX += g.Width;
                        }

                    }
                    break;
                case CompactOption.None:
                    {
                        //3. layout 
                        int glyphCount = itemList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            BitmapAtlasItemSource g = itemList[i];
                            if (g.Height > maxRowHeight)
                            {
                                maxRowHeight = g.Height;
                            }
                            if (currentX + g.Width > totalMaxLim)
                            {
                                //start new row
                                currentY += maxRowHeight;
                                currentX = 0;
                                maxRowHeight = g.Height;//reset, after start new row
                            }
                            //-------------------
                            g.area = new Rectangle(currentX, currentY, g.Width, g.Height);
                            currentX += g.Width;
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
                for (int i = itemList.Count - 1; i >= 0; --i)
                {
                    BitmapAtlasItemSource g = itemList[i];
                    BinPackRect newRect = binPacker.Insert(g.Width, g.Height);
                    g.area = new Rectangle(newRect.X, newRect.Y, g.Width, g.Height);


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
                for (int i = itemList.Count - 1; i >= 0; --i)
                {
                    BitmapAtlasItemSource g = itemList[i];
                    //copy glyph image buffer to specific area of final result buffer
                    CopyToDest(g.GetImageBuffer(), g.Width, g.Height, mergeBmpBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }
            }
            else
            {
                int itemCount = itemList.Count;
                for (int i = 0; i < itemCount; ++i)
                {
                    BitmapAtlasItemSource g = itemList[i];
                    //copy glyph image buffer to specific area of final result buffer

                    CopyToDest(g.GetImageBuffer(), g.Width, g.Height, mergeBmpBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }
            }

            //5. since the mergeBmpBuffer is head-down
            //we will flipY axis again to head-up, the head-up img is easy to read and debug

            if (flipY)
            {
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
                for (int i = 0; i < itemList.Count; ++i)
                {
                    BitmapAtlasItemSource g = itemList[i];
                    Rectangle rect = g.area;
                    g.area = new Rectangle(rect.X, imgH - (rect.Y + rect.Height), rect.Width, rect.Height);
                }


                //***
                //6. generate final output
                //TODO: rename GlyphImage to another name to distinquist
                //between small glyph and a large one
                return _latestGenGlyphImage = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(totalImgWidth, imgH, totalBufferFlipY);
            }
            else
            {
                return _latestGenGlyphImage = PixelFarm.CpuBlit.MemBitmap.CreateFromCopy(totalImgWidth, imgH, mergeBmpBuffer);
            }
        }
        public void SaveAtlasInfo(System.IO.Stream outputStream)
        {

            if (_latestGenGlyphImage == null)
            {
                throw new System.Exception("");
            }

            BitmapAtlasFile atlasFile = new BitmapAtlasFile();
            atlasFile.StartWrite(outputStream);
            if (FontFilename != null)
            {
                atlasFile.WriteOverviewFontInfo(FontFilename, FontKey, FontSizeInPoints);
            }
            else
            {
                atlasFile.WriteOverviewFontInfo("", 0, 0);
            }

            atlasFile.WriteTotalImageInfo(
                (ushort)_latestGenGlyphImage.Width,
                (ushort)_latestGenGlyphImage.Height, 4,
                this.TextureKind);
            //
            //
            atlasFile.WriteAtlasItems(_items);

            if (ImgUrlDict != null)
            {
                atlasFile.WriteImgUrlDict(ImgUrlDict);
            }
            atlasFile.EndWrite();
        }



        public void SaveAtlasInfo(string outputFilename)
        {
            //TODO: review here, use extension method
            using (System.IO.FileStream fs = new System.IO.FileStream(outputFilename, System.IO.FileMode.Create))
            {
                SaveAtlasInfo(fs);
            }
        }
        public List<SimpleBitmapAtlas> LoadAtlasInfo(string infoFilename)
        {
            //TODO: review here, use extension method
            using (System.IO.FileStream fs = new System.IO.FileStream(infoFilename, System.IO.FileMode.Open))
            {
                return LoadAtlasInfo(fs);
            }
        }

        public SimpleBitmapAtlas CreateSimpleBitmapAtlas()
        {
            SimpleBitmapAtlas atlas = new SimpleBitmapAtlas();
            atlas.TextureKind = this.TextureKind;
            atlas.OriginalFontSizePts = this.FontSizeInPoints;

            foreach (BitmapAtlasItemSource src in _items.Values)
            {
                Rectangle area = src.area;
                AtlasItem glyphData = new AtlasItem();

                glyphData.Width = src.Width;
                glyphData.Left = area.X;
                glyphData.Top = area.Top;
                glyphData.Height = area.Height;

                glyphData.TextureXOffset = src.TextureXOffset;
                glyphData.TextureYOffset = src.TextureYOffset;

                atlas.AddGlyph(src.UniqueInt16Name, glyphData);
            }

            return atlas;
        }

        public List<SimpleBitmapAtlas> LoadAtlasInfo(System.IO.Stream dataStream)
        {
            BitmapAtlasFile atlasFile = new BitmapAtlasFile();
            //read font atlas from stream data
            atlasFile.Read(dataStream);
            return atlasFile.AtlasList;
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