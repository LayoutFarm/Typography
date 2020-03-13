//MIT, 2016-present, WinterDev
//----------------------------------- 

using System.Collections.Generic;
using PixelFarm.CpuBlit;

namespace PixelFarm.Drawing.BitmapAtlas
{

    public class SimpleBitmapAtlasBuilder
    {
        AtlasItemImage _latestGenGlyphImage;
        Dictionary<ushort, CacheBmp> _items = new Dictionary<ushort, CacheBmp>();
        Dictionary<string, ushort> _imgUrlDict;

        public SimpleBitmapAtlasBuilder()
        {
            SpaceCompactOption = CompactOption.BinPack; //default
            MaxAtlasWidth = 800;
        }
        public int MaxAtlasWidth { get; set; }
        public TextureKind TextureKind { get; private set; }

        public string FontFilename { get; set; }
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
        /// <param name="imgIndex"></param>
        /// <param name="img"></param>
        public void AddAtlasItemImage(ushort imgIndex, AtlasItemImage img)
        {
            var cache = new CacheBmp();
            cache.imgIndex = imgIndex;
            cache.img = img;
            _items[imgIndex] = cache;
        }
        public Dictionary<string, ushort> ImgUrlDict
        {
            get => _imgUrlDict;
            set => _imgUrlDict = value;
        }
        public void SetAtlasInfo(TextureKind textureKind)
        {
            this.TextureKind = textureKind;
        }
        public AtlasItemImage BuildSingleImage()
        {
            //1. add to list 
            var itemList = new List<CacheBmp>(_items.Count);
            foreach (CacheBmp itm in _items.Values)
            {
                //sort data
                itemList.Add(itm);
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
                        itemList.Sort((a, b) => a.img.Width.CompareTo(b.img.Width));

                        //3. layout 
                        for (int i = itemList.Count - 1; i >= 0; --i)
                        {
                            CacheBmp g = itemList[i];
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
                        itemList.Sort((a, b) => a.img.Height.CompareTo(b.img.Height));

                        //3. layout 
                        int glyphCount = itemList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            CacheBmp g = itemList[i];
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
                        int glyphCount = itemList.Count;
                        for (int i = 0; i < glyphCount; ++i)
                        {
                            CacheBmp g = itemList[i];
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
            //TODO: review performance here again***

            int totalImgWidth = totalMaxLim;
            if (SpaceCompactOption == CompactOption.BinPack) //again here?
            {
                totalImgWidth = 0;//reset
                //use bin packer
                BinPacker binPacker = new BinPacker(totalMaxLim, currentY);
                for (int i = itemList.Count - 1; i >= 0; --i)
                {
                    CacheBmp g = itemList[i];
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

            MemBitmap totalBmp = new MemBitmap(totalImgWidth, imgH);
            if (SpaceCompactOption == CompactOption.BinPack) //again here?
            {
                for (int i = itemList.Count - 1; i >= 0; --i)
                {
                    CacheBmp g = itemList[i];
                    //copy data to totalBuffer
                    AtlasItemImage img = g.img;
                    CopyToDest(img.Bitmap, img.Width, img.Height, totalBmp, g.area.Left, g.area.Top, totalImgWidth);
                }
            }
            else
            {
                int glyphCount = itemList.Count;
                for (int i = 0; i < glyphCount; ++i)
                {
                    CacheBmp g = itemList[i];
                    //copy data to totalBuffer
                    AtlasItemImage img = g.img;
                    CopyToDest(img.Bitmap, img.Width, img.Height, totalBmp, g.area.Left, g.area.Top, totalImgWidth);
                }
            }

            //new total glyph img
            AtlasItemImage glyphImage = new AtlasItemImage(totalImgWidth, imgH);
            bool flipY = false;
            if (flipY)
            {

                MemBitmap totalBmp2 = new MemBitmap(totalImgWidth, imgH);
                int srcRowIndex = imgH - 1;
                int strideInBytes = totalImgWidth * 4;
                unsafe
                {
                    //flipY
                    byte* flipYPtr = (byte*)MemBitmap.GetBufferPtr(totalBmp2).Ptr;
                    byte* totalBmpPtr = (byte*)MemBitmap.GetBufferPtr(totalBmp).Ptr;
                    for (int i = 0; i < imgH; ++i)
                    {
                        //copy each row from src to dst
                        //System.Buffer.BlockCopy(totalBuffer, strideInBytes * srcRowIndex, totalBufferFlipY, strideInBytes * i, strideInBytes);
                        PixelFarm.CpuBlit.NativeMemMx.MemCopy((byte*)flipYPtr, (byte*)(totalBmpPtr + (srcRowIndex * strideInBytes)), strideInBytes);
                        srcRowIndex--;
                        flipYPtr += strideInBytes;
                    }
                }
                glyphImage.SetBitmap(totalBmp2, true);
                _latestGenGlyphImage = glyphImage;

                totalBmp.Dispose();
                return glyphImage;
            }
            else
            {
                glyphImage.SetBitmap(totalBmp, true);
                _latestGenGlyphImage = glyphImage;
                return glyphImage;
            }
        }

        public void SaveAtlasInfo(System.IO.Stream outputStream)
        {

            if (_latestGenGlyphImage == null)
            {
                BuildSingleImage();
            }

            BitmapAtlasFile bmpAtlasFile = new BitmapAtlasFile();
            bmpAtlasFile.StartWrite(outputStream);
            bmpAtlasFile.WriteOverviewBitmapInfo(FontFilename);

            if (_imgUrlDict != null)
            {
                //save mapping data from img url to index
                bmpAtlasFile.WriteImgUrlDict(_imgUrlDict);
            }

            bmpAtlasFile.WriteTotalImageInfo(
                (ushort)_latestGenGlyphImage.Width,
                (ushort)_latestGenGlyphImage.Height, 4,
                this.TextureKind);
            //
            //
            bmpAtlasFile.WriteGlyphList(_items);
            bmpAtlasFile.EndWrite();

        }
        /// <summary>
        /// save font info into xml document
        /// </summary>
        /// <param name="filename"></param>
        public void SaveAtlasInfo(string filename)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                SaveAtlasInfo(fs);
            }
        }
        public SimpleBitmaptAtlas CreateSimpleFontAtlas()
        {
            SimpleBitmaptAtlas simpleFontAtlas = new SimpleBitmaptAtlas();
            simpleFontAtlas.TextureKind = this.TextureKind;

            foreach (CacheBmp cacheGlyph in _items.Values)
            {

                Rectangle area = cacheGlyph.area;
                BitmapMapData glyphData = new BitmapMapData();

                glyphData.Width = cacheGlyph.img.Width;
                glyphData.Left = area.X;
                glyphData.Top = area.Top;
                glyphData.Height = area.Height;

                glyphData.TextureXOffset = cacheGlyph.img.TextureOffsetX;
                glyphData.TextureYOffset = cacheGlyph.img.TextureOffsetY;


                simpleFontAtlas.AddBitmapMapData(cacheGlyph.imgIndex, glyphData);
            }

            return simpleFontAtlas;
        }

        public SimpleBitmaptAtlas LoadAtlasInfo(string filename)
        {

            BitmapAtlasFile atlasFile = new BitmapAtlasFile();
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
            {
                //read font atlas from stream data
                return LoadAtlasInfo(fs);
            }
        }
        public SimpleBitmaptAtlas LoadAtlasInfo(System.IO.Stream dataStream)
        {
            BitmapAtlasFile atlasFile = new BitmapAtlasFile();
            //read font atlas from stream data
            atlasFile.Read(dataStream);
            return atlasFile.Result;
        }
        static void CopyToDest(MemBitmap srcBmp, int srcW, int srcH, MemBitmap targetBmp, int targetX, int targetY, int totalTargetWidth)
        {
            int srcIndex = 0;
            unsafe
            {
                //
                var srcMemPtr = MemBitmap.GetBufferPtr(srcBmp);
                var targetMemPtr = MemBitmap.GetBufferPtr(targetBmp);
                int* targetPixels = (int*)targetMemPtr.Ptr;
                int* srcPixels = (int*)srcMemPtr.Ptr;
                //
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