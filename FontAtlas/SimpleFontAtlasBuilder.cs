//MIT, 2016-present, WinterDev
//----------------------------------- 

using System.Collections.Generic;

using PixelFarm.Drawing.Fonts;
using Typography.Contours;

namespace Typography.Rendering
{

    public class SimpleFontAtlasBuilder
    {
        GlyphImage _latestGenGlyphImage;
        Dictionary<ushort, CacheGlyph> _glyphs = new Dictionary<ushort, CacheGlyph>();

        public SimpleFontAtlasBuilder()
        {
            SpaceCompactOption = CompactOption.BinPack; //default
            MaxAtlasWidth = 800;
        }
        public int MaxAtlasWidth { get; set; }
        public TextureKind TextureKind { get; private set; }
        public float FontSizeInPoints { get; private set; }
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
        /// <param name="glyphIndex"></param>
        /// <param name="img"></param>
        public void AddGlyph(ushort glyphIndex, GlyphImage img)
        {
            var glyphCache = new CacheGlyph();
            glyphCache.glyphIndex = glyphIndex;
            glyphCache.img = img;
            _glyphs[glyphIndex] = glyphCache;
        }

        public void SetAtlasInfo(TextureKind textureKind, float fontSizeInPts)
        {
            this.TextureKind = textureKind;
            this.FontSizeInPoints = fontSizeInPts;
        }
        public GlyphImage BuildSingleImage()
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
                                maxRowHeight = 0;//reset, after start new row
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
                                maxRowHeight = 0;//reset, after start new row
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
                    g.area = new Rectangle(newRect.X, newRect.Y,
                        g.img.Width, g.img.Height);


                    //recalculate proper max midth again, after arrange and compact space
                    if (newRect.Right > totalImgWidth)
                    {
                        totalImgWidth = newRect.Right;
                    }
                }
            }
            // ------------------------------- 
            //4. create array that can hold data  
            int[] totalBuffer = new int[totalImgWidth * imgH];
            if (SpaceCompactOption == CompactOption.BinPack) //again here?
            {
                for (int i = glyphList.Count - 1; i >= 0; --i)
                {
                    CacheGlyph g = glyphList[i];
                    //copy data to totalBuffer
                    GlyphImage img = g.img;
                    CopyToDest(img.GetImageBuffer(), img.Width, img.Height, totalBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }

            }
            else
            {
                int glyphCount = glyphList.Count;
                for (int i = 0; i < glyphCount; ++i)
                {
                    CacheGlyph g = glyphList[i];
                    //copy data to totalBuffer
                    GlyphImage img = g.img;
                    CopyToDest(img.GetImageBuffer(), img.Width, img.Height, totalBuffer, g.area.Left, g.area.Top, totalImgWidth);
                }
            }

            GlyphImage glyphImage = new GlyphImage(totalImgWidth, imgH);
            glyphImage.SetImageBuffer(totalBuffer, true);


            _latestGenGlyphImage = glyphImage;

            return glyphImage;

        }

        public void SaveFontInfo(System.IO.Stream outputStream)
        {

            if (_latestGenGlyphImage == null)
            {
                BuildSingleImage();
            }

            FontAtlasFile fontAtlasFile = new FontAtlasFile();
            fontAtlasFile.StartWrite(outputStream);
            fontAtlasFile.WriteOverviewFontInfo(FontFilename, FontSizeInPoints);

            fontAtlasFile.WriteTotalImageInfo(
                (ushort)_latestGenGlyphImage.Width,
                (ushort)_latestGenGlyphImage.Height, 4,
                this.TextureKind);
            //
            //
            fontAtlasFile.WriteGlyphList(_glyphs);
            fontAtlasFile.EndWrite();

        }
        /// <summary>
        /// save font info into xml document
        /// </summary>
        /// <param name="filename"></param>
        public void SaveFontInfo(string filename)
        {
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                SaveFontInfo(fs);
            }
        }
        public SimpleFontAtlas CreateSimpleFontAtlas()
        {
            SimpleFontAtlas simpleFontAtlas = new SimpleFontAtlas();
            simpleFontAtlas.TextureKind = this.TextureKind;
            simpleFontAtlas.OriginalFontSizePts = this.FontSizeInPoints;
            foreach (CacheGlyph cacheGlyph in _glyphs.Values)
            {
                //convert char to hex
                string unicode = ("0x" + ((int)cacheGlyph.character).ToString("X"));//code point
                Rectangle area = cacheGlyph.area;
                TextureGlyphMapData glyphData = new TextureGlyphMapData();
                area.Y += area.Height;//*** 

                ////set font matrix to glyph font data
                //glyphData.Rect = Rectangle.FromLTRB(area.X, area.Top, area.Right, area.Bottom);
                //glyphData.AdvanceY = cacheGlyph.glyphMatrix.advanceY;

                glyphData.Width = cacheGlyph.img.Width;
                glyphData.Left = area.X;
                glyphData.Top = area.Top;
                glyphData.Height = area.Height;

                glyphData.TextureXOffset = (float)cacheGlyph.img.TextureOffsetX;
                glyphData.TextureYOffset = (float)cacheGlyph.img.TextureOffsetY;
                glyphData.BorderX = cacheGlyph.borderX;
                glyphData.BorderY = cacheGlyph.borderY;


                simpleFontAtlas.AddGlyph(cacheGlyph.glyphIndex, glyphData);
            }

            return simpleFontAtlas;
        }
        
        public SimpleFontAtlas LoadFontInfo(string filename)
        {

            FontAtlasFile atlasFile = new FontAtlasFile();
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
            {
                //read font atlas from stream data
                return LoadFontInfo(fs);
            }
        }
        public SimpleFontAtlas LoadFontInfo(System.IO.Stream dataStream)
        {
            FontAtlasFile atlasFile = new FontAtlasFile();
            //read font atlas from stream data
            atlasFile.Read(dataStream);
            return atlasFile.Result;
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