//MIT, 2020-present, WinterDev
using System;
using System.IO;

using PixelFarm.CpuBlit;
using Typography.OpenFont;
using PixelFarm.CpuBlit.BitmapAtlas;

namespace SampleWinForms
{
    struct FontAtlasBuilderHelper
    {
        public string TextureInfoFilename { get; set; }
        public string OutputImgFilename { get; set; }

#if DEBUG
        public long dbugBuildTimeMillisec { get; set; }
#endif
        public void Build(
            GlyphTextureBitmapGenerator glyphTextureGen,
            Typeface typeface, float fontSizeInPoints,
            TextureKind textureKind,
            GlyphTextureBuildDetail[] buildDetails,
            int fontKey)
        {
#if DEBUG
            //overall, glyph atlas generation time
            System.Diagnostics.Stopwatch dbugStopWatch = new System.Diagnostics.Stopwatch();
            dbugStopWatch.Start();
#endif

            SimpleBitmapAtlasBuilder atlasBuilder = glyphTextureGen.CreateTextureFontFromBuildDetail(typeface,
                fontSizeInPoints,
                textureKind,
                buildDetails);

            //3. set information before write to font-info
            atlasBuilder.SpaceCompactOption = SimpleBitmapAtlasBuilder.CompactOption.ArrangeByHeight;
            atlasBuilder.FontFilename = typeface.Name;
            atlasBuilder.FontKey = fontKey;

            //4. merge all glyph in the builder into a single image
            using (MemBitmap totalGlyphsImg = atlasBuilder.BuildSingleImage(true))
            {
                
                if (TextureInfoFilename == null)
                {
                    string textureName = typeface.Name.ToLower() + "_" + fontKey;
                    string output_imgFilename = textureName + ".png";

                    TextureInfoFilename = textureName;
                    OutputImgFilename = output_imgFilename;
                }


                //5. save atlas info to disk
                using (FileStream fs = new FileStream(TextureInfoFilename + ".info", FileMode.Create))
                {
                    atlasBuilder.SaveAtlasInfo(fs);
                }

                //6. save total-glyph-image to disk
                totalGlyphsImg.SaveImage(OutputImgFilename);
            }

#if DEBUG
            dbugStopWatch.Stop();
            dbugBuildTimeMillisec = dbugStopWatch.ElapsedMilliseconds;
#endif

        }

    }


}