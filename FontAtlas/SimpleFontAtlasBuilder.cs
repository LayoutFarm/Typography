//MIT, 2016-2017, WinterDev
//----------------------------------- 

using System.Collections.Generic;
using System.Xml;
using PixelFarm.Drawing.Fonts;
using Typography.Contours;

namespace Typography.Rendering
{
    public enum TextureKind
    {
        AggGrayScale,
        AggSubPixel,
        Msdf
    }
    public class SimpleFontAtlasBuilder
    {
        GlyphImage latestGenGlyphImage;
        Dictionary<int, CacheGlyph> glyphs = new Dictionary<int, CacheGlyph>();
        public TextureKind TextureKind { get; private set; }
        public float FontSizeInPoints { get; private set; }
        public void AddGlyph(int codePoint, GlyphImage img)
        {
            var glyphCache = new CacheGlyph();
            glyphCache.codePoint = codePoint;
            glyphCache.img = img;

            glyphs[codePoint] = glyphCache;
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
                g.area = new Rectangle(newRect.X, newRect.Y,
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
            //save font info as xml 
            //save position of each font
            XmlDocument xmldoc = new XmlDocument();
            XmlElement root = xmldoc.CreateElement("font");
            xmldoc.AppendChild(root);

            if (latestGenGlyphImage == null)
            {
                BuildSingleImage();
            }

            {
                //total img element
                XmlElement totalImgElem = xmldoc.CreateElement("total_img");
                totalImgElem.SetAttribute("w", latestGenGlyphImage.Width.ToString());
                totalImgElem.SetAttribute("h", latestGenGlyphImage.Height.ToString());
                totalImgElem.SetAttribute("compo", "4");
                root.AppendChild(totalImgElem);
            }

            foreach (CacheGlyph g in glyphs.Values)
            {
                XmlElement gElem = xmldoc.CreateElement("glyph");
                //convert char to hex
                string unicode = ("0x" + ((int)g.character).ToString("X"));//code point
                Rectangle area = g.area;
                gElem.SetAttribute("c", g.codePoint.ToString());
                gElem.SetAttribute("uc", unicode);//unicode char
                gElem.SetAttribute("ltwh",
                    area.Left + " " + area.Top + " " + area.Width + " " + area.Height
                    );
                gElem.SetAttribute("borderXY",
                    g.borderX + " " + g.borderY
                    );
                var mat = g.glyphMatrix;
                gElem.SetAttribute("mat",
                    mat.advanceX + " " + mat.advanceY + " " +
                    mat.bboxXmin + " " + mat.bboxXmax + " " +
                    mat.bboxYmin + " " + mat.bboxYmax + " " +
                    mat.img_width + " " + mat.img_height + " " +
                    mat.img_horiAdvance + " " + mat.img_horiBearingX + " " +
                    mat.img_horiBearingY + " " +
                    //-----------------------------
                    mat.img_vertAdvance + " " +
                    mat.img_vertBearingX + " " + mat.img_vertBearingY);

                if (g.character > 50)
                {
                    gElem.SetAttribute("example", g.character.ToString());
                }
                root.AppendChild(gElem);
            }
            //if (embededGlyphsImage)
            //{
            //    XmlElement glyphImgElem = xmldoc.CreateElement("msdf_img");
            //    glyphImgElem.SetAttribute("w", latestGenGlyphImage.Width.ToString());
            //    glyphImgElem.SetAttribute("h", latestGenGlyphImage.Height.ToString());
            //    int[] imgBuffer = latestGenGlyphImage.GetImageBuffer();
            //    glyphImgElem.SetAttribute("buff_len", (imgBuffer.Length * 4).ToString());
            //    //----------------------------------------------------------------------
            //    glyphImgElem.AppendChild(
            //        xmldoc.CreateTextNode(ConvertToBase64(imgBuffer)));
            //    //----------------------------------------------------------------------
            //    root.AppendChild(glyphImgElem);
            //    latestGenGlyphImage.GetImageBuffer();
            //}
            xmldoc.Save(filename);
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
                TextureFontGlyphData glyphData = new TextureFontGlyphData();
                area.Y += area.Height;//*** 

                //set font matrix to glyph font data
                glyphData.Rect = Rectangle.FromLTRB(area.X, area.Top, area.Right, area.Bottom);
                glyphData.AdvanceY = cacheGlyph.glyphMatrix.advanceY;
                glyphData.ImgWidth = cacheGlyph.img.Width;
                glyphData.TextureXOffset = cacheGlyph.img.TextureOffsetX;
                glyphData.TextureYOffset = cacheGlyph.img.TextureOffsetY;

                simpleFontAtlas.AddGlyph(cacheGlyph.codePoint, glyphData);
            }

            return simpleFontAtlas;
        }
        //read font info from xml document
        public SimpleFontAtlas LoadFontInfo(string filename)
        {
            SimpleFontAtlas simpleFontAtlas = new SimpleFontAtlas();
            simpleFontAtlas.TextureKind = this.TextureKind;
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filename);
            //read
            int total_W = 0;
            int total_H = 0;
            {

                foreach (XmlElement xmlelem in xmldoc.GetElementsByTagName("total_img"))
                {
                    simpleFontAtlas.Width = total_W = int.Parse(xmlelem.GetAttribute("w"));
                    simpleFontAtlas.Height = total_H = int.Parse(xmlelem.GetAttribute("h"));
                    //only 1...

                    break;
                }
            }
            foreach (XmlElement glyphElem in xmldoc.GetElementsByTagName("glyph"))
            {
                //read
                string unicodeHex = glyphElem.GetAttribute("uc");
                int codepoint = int.Parse(glyphElem.GetAttribute("c"));
                char c = (char)int.Parse(unicodeHex.Substring(2), System.Globalization.NumberStyles.HexNumber);
                Rectangle area = ParseRect(glyphElem.GetAttribute("ltwh"));
                var glyphData = new TextureFontGlyphData();
                area.Y += area.Height;//*** 
                //glyphData.Rect = Rectangle.c((short)area.X, (short)area.Bottom, (short)area.Right, (short)area.Top);
                glyphData.Rect = Rectangle.FromLTRB(area.X, area.Top, area.Right, area.Bottom);
                float[] borderXY = ParseFloatArray(glyphElem.GetAttribute("borderXY"));
                float[] matrix = ParseFloatArray(glyphElem.GetAttribute("mat"));

                glyphData.BorderX = borderXY[0];
                glyphData.BorderY = borderXY[1];

                glyphData.AdvanceX = matrix[0];
                glyphData.AdvanceY = matrix[1];
                glyphData.BBoxXMin = matrix[2];
                glyphData.BBoxXMax = matrix[3];
                glyphData.BBoxYMin = matrix[4];
                glyphData.BBoxYMax = matrix[5];
                glyphData.ImgWidth = matrix[6];
                glyphData.ImgHeight = matrix[7];
                glyphData.HAdvance = matrix[8];
                glyphData.HBearingX = matrix[9];
                glyphData.HBearingY = matrix[10];
                glyphData.VAdvance = matrix[11];
                glyphData.VBearingX = matrix[12];
                glyphData.VBearingY = matrix[13];
                //--------------- 
                simpleFontAtlas.AddGlyph(codepoint, glyphData);
            }
            return simpleFontAtlas;
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