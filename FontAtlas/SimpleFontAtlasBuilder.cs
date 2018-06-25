//MIT, 2016-2017, WinterDev
//----------------------------------- 

using System.Collections.Generic;

using PixelFarm.Drawing.Fonts;
using Typography.Contours;

namespace Typography.Rendering
{
    public enum TextureKind : byte
    {
        AggGrayScale,
        AggSubPixel,
        Msdf
    }
    public class SimpleFontAtlasBuilder
    {
        GlyphImage latestGenGlyphImage;
        Dictionary<ushort, CacheGlyph> glyphs = new Dictionary<ushort, CacheGlyph>();
        public TextureKind TextureKind { get; private set; }
        public float FontSizeInPoints { get; private set; }
        public string FontFilename { get; set; }

        public void AddGlyph(ushort glyphIndex, GlyphImage img)
        {
            var glyphCache = new CacheGlyph();
            glyphCache.glyphIndex = glyphIndex;
            glyphCache.img = img;
            glyphs[glyphIndex] = glyphCache;
        }

        public void SetAtlasInfo(TextureKind textureKind, float fontSizeInPts)
        {
            this.TextureKind = textureKind;
            this.FontSizeInPoints = fontSizeInPts;
        }
        public GlyphImage BuildSingleImage()
        {
            //1. add to list 
            var glyphList = new List<CacheGlyph>(glyphs.Count);
            foreach (CacheGlyph glyphImg in glyphs.Values)
            {
                //sort data
                glyphList.Add(glyphImg);
            }
            //2. sort
            glyphList.Sort((a, b) =>
            {
                return a.img.Width.CompareTo(b.img.Width);
            });
            //3. layout

            int totalMaxLim = 800;
            int maxRowHeight = 0;
            int currentY = 0;
            int currentX = 0;
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
            currentY += maxRowHeight;
            int imgH = currentY;
            //-------------------------------
            //compact image location
            //TODO: review performance here again***
            BinPacker binPacker = new BinPacker(totalMaxLim, currentY);
            for (int i = glyphList.Count - 1; i >= 0; --i)
            {
                CacheGlyph g = glyphList[i];
                BinPackRect newRect = binPacker.Insert(g.img.Width, g.img.Height);
                g.area = new Rectangle(
                    newRect.X, newRect.Y,
                    g.img.Width, g.img.Height);
            }
            //------------------------------- 

            //4. create array that can hold data
            int[] totalBuffer = new int[totalMaxLim * imgH];
            for (int i = glyphList.Count - 1; i >= 0; --i)
            {
                CacheGlyph g = glyphList[i];
                //copy data to totalBuffer
                GlyphImage img = g.img;
                CopyToDest(img.GetImageBuffer(), img.Width, img.Height, totalBuffer, g.area.Left, g.area.Top, totalMaxLim);
            }
            //------------------

            GlyphImage glyphImage = new GlyphImage(totalMaxLim, imgH);
            glyphImage.SetImageBuffer(totalBuffer, true);
            latestGenGlyphImage = glyphImage;
            return glyphImage;

        }

        /// <summary>
        /// save font info into xml document
        /// </summary>
        /// <param name="filename"></param>
        public void SaveFontInfo(string filename)
        {

            if (latestGenGlyphImage == null)
            {
                BuildSingleImage();
            }

            FontAtlasFile fontAtlasFile = new FontAtlasFile();
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Create))
            {
                fontAtlasFile.StartWrite(fs);
                fontAtlasFile.WriteOverviewFontInfo(FontFilename, FontSizeInPoints);

                fontAtlasFile.WriteTotalImageInfo(
                    (ushort)latestGenGlyphImage.Width,
                    (ushort)latestGenGlyphImage.Height, 4,
                    this.TextureKind);
                //
                //
                fontAtlasFile.WriteGlyphList(glyphs);
                fontAtlasFile.EndWrite();
            }

            //foreach (CacheGlyph g in glyphs.Values)
            //{
            //    XmlElement gElem = xmldoc.CreateElement("glyph");
            //    //convert char to hex
            //    string unicode = ("0x" + ((int)g.character).ToString("X"));//code point
            //    Rectangle area = g.area;
            //    gElem.SetAttribute("c", g.codePoint.ToString());
            //    gElem.SetAttribute("uc", unicode);//unicode char
            //    gElem.SetAttribute("ltwh",
            //        area.Left + " " + area.Top + " " + area.Width + " " + area.Height
            //        );
            //    gElem.SetAttribute("borderXY",
            //        g.borderX + " " + g.borderY
            //        );
            //    var mat = g.glyphMatrix;
            //    gElem.SetAttribute("mat",
            //        mat.advanceX + " " + mat.advanceY + " " +
            //        mat.bboxXmin + " " + mat.bboxXmax + " " +
            //        mat.bboxYmin + " " + mat.bboxYmax + " " +
            //        mat.img_width + " " + mat.img_height + " " +
            //        mat.img_horiAdvance + " " + mat.img_horiBearingX + " " +
            //        mat.img_horiBearingY + " " +
            //        //-----------------------------
            //        mat.img_vertAdvance + " " +
            //        mat.img_vertBearingX + " " + mat.img_vertBearingY);

            //    if (g.character > 50)
            //    {
            //        gElem.SetAttribute("example", g.character.ToString());
            //    }
            //    root.AppendChild(gElem);
            //}
            ////if (embededGlyphsImage)
            ////{
            ////    XmlElement glyphImgElem = xmldoc.CreateElement("msdf_img");
            ////    glyphImgElem.SetAttribute("w", latestGenGlyphImage.Width.ToString());
            ////    glyphImgElem.SetAttribute("h", latestGenGlyphImage.Height.ToString());
            ////    int[] imgBuffer = latestGenGlyphImage.GetImageBuffer();
            ////    glyphImgElem.SetAttribute("buff_len", (imgBuffer.Length * 4).ToString());
            ////    //----------------------------------------------------------------------
            ////    glyphImgElem.AppendChild(
            ////        xmldoc.CreateTextNode(ConvertToBase64(imgBuffer)));
            ////    //----------------------------------------------------------------------
            ////    root.AppendChild(glyphImgElem);
            ////    latestGenGlyphImage.GetImageBuffer();
            ////}
            //xmldoc.Save(filename);
        }

        public SimpleFontAtlas CreateSimpleFontAtlas()
        {
            SimpleFontAtlas simpleFontAtlas = new SimpleFontAtlas();
            simpleFontAtlas.TextureKind = this.TextureKind;
            simpleFontAtlas.OriginalFontSizePts = this.FontSizeInPoints;
            foreach (CacheGlyph cacheGlyph in glyphs.Values)
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
        //read font info from xml document
        public SimpleFontAtlas LoadFontInfo(string filename)
        {

            FontAtlasFile atlasFile = new FontAtlasFile();
            using (System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
            {
                //read font atlas from stream data
                atlasFile.Read(fs);
                return atlasFile.Result;
            }
        }

        static float[] ParseFloatArray(string str)
        {
            string[] str_values = str.Split(' ');
            int j = str_values.Length;
            float[] f_values = new float[j];
            for (int i = 0; i < j; ++i)
            {
                f_values[i] = float.Parse(str_values[i]);
            }
            return f_values;
        }
        static Rectangle ParseRect(string str)
        {
            string[] ltwh = str.Split(' ');
            return new Rectangle(
                int.Parse(ltwh[0]),
                int.Parse(ltwh[1]),
                int.Parse(ltwh[2]),
                int.Parse(ltwh[3]));
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