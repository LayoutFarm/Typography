//MIT, 2020-present, WinterDev
using System;
using System.Collections.Generic;
using System.ComponentModel;

using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using PixelFarm.Drawing.Fonts;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;
using Typography.Rendering;
using Typography.TextLayout;

namespace SampleWinForms
{
    public partial class FormFontAtlas : Form
    {
        Typeface _typeface;

        List<CustomUIFontScriptOpt> _availableScripts = new List<CustomUIFontScriptOpt>();
        public FormFontAtlas()
        {
            InitializeComponent();
        }

        public void SetFont(Typeface typeface, float fontSizeInPoints)
        {
            _typeface = typeface;
            
            this.Text = typeface.Name + ", size=" + fontSizeInPoints + "pts";
            this.txtSelectedFontSize.Text = fontSizeInPoints.ToString();

            this.flowLayoutPanel1.Controls.Clear();
            _availableScripts.Clear();

            foreach (ScriptLang scriptLang in ScriptLangs.GetRegiteredScriptLangIter())
            {
                if (ScriptLangs.TryGetUnicodeLangBitsArray(scriptLang.shortname, out UnicodeLangBits[] unicodeLangs))
                {
                    foreach (UnicodeLangBits unicodeLang in unicodeLangs)
                    {
                        if (typeface.DoesSupportUnicode(unicodeLang))
                        {
                            //
                            CustomUIFontScriptOpt customUIFontScript = new CustomUIFontScriptOpt();
                            customUIFontScript.SetInfo(scriptLang, unicodeLang);
                            _availableScripts.Add(customUIFontScript);

                            this.flowLayoutPanel1.Controls.Add(customUIFontScript);
                        }
                    }
                }

            }
        }

        private void FormFontAtlas_Load(object sender, EventArgs e)
        {

        }

        private void cmdBuildAtlasFromText_Click(object sender, EventArgs e)
        {
            BuildAtlas_FromInputChars();
        }

        char[] GetUniqueChars()
        {
            //
            char[] sampleChars = this.txtSampleChars.Text.ToCharArray();
            //ensure all chars are unique
            Dictionary<char, bool> uniqueDic = new Dictionary<char, bool>();
            foreach (char c in sampleChars)
            {
                uniqueDic[c] = true;
            }
            char[] uniqueChars = new char[uniqueDic.Count];

            int i = 0;
            foreach (char k in uniqueDic.Keys)
            {
                uniqueChars[i] = k;
                i++;
            }
            return uniqueChars;
        }
        void BuildAtlas_FromInputChars()
        {

            GlyphImage totalGlyphsImg = null;
            SimpleFontAtlasBuilder atlasBuilder = null;
            var glyphTextureGen = new GlyphTextureBitmapGenerator();

            if (!float.TryParse(txtSelectedFontSize.Text, out float fontSizeInPoints))
            {
                MessageBox.Show("err: selected font size " + txtSelectedFontSize.Text);
                return;
            }


            glyphTextureGen.CreateTextureFontFromInputChars(
               _typeface,
               fontSizeInPoints,
               PixelFarm.Drawing.BitmapAtlas.TextureKind.StencilLcdEffect,
               GetUniqueChars(),
               (glyphIndex, glyphImage, outputAtlasBuilder) =>
               {
                   if (outputAtlasBuilder != null)
                   {
                       //finish
                       atlasBuilder = outputAtlasBuilder;
                   }
               }
            );

            atlasBuilder.SpaceCompactOption = SimpleFontAtlasBuilder.CompactOption.ArrangeByHeight;
            //
            totalGlyphsImg = atlasBuilder.BuildSingleImage();
            string fontTextureImg = "test_glyph_atlas.png";

            //create atlas
            SimpleFontAtlas fontAtlas = atlasBuilder.CreateSimpleFontAtlas();
            fontAtlas.TotalGlyph = totalGlyphsImg;
            //copy to Gdi+ and save
            //TODO: use helper method
            SaveGlyphImageToPngFile(totalGlyphsImg, fontTextureImg);

            this.lblOutput.Text = "output: " + fontTextureImg;
            this.picOutput.Image = new Bitmap(fontTextureImg);
#if DEBUG
            //save glyph image for debug
            //PixelFarm.Agg.ActualImage.SaveImgBufferToPngFile(
            //    totalGlyphsImg.GetImageBuffer(),
            //    totalGlyphsImg.Width * 4,
            //    totalGlyphsImg.Width, totalGlyphsImg.Height,
            //    "total_" + reqFont.Name + "_" + reqFont.SizeInPoints + ".png");
            ////save image to cache
            //SaveImgBufferToFile(totalGlyphsImg, fontTextureImg);
#endif

            //cache the atlas
            //_createdAtlases.Add(fontKey, fontAtlas);
            ////
            ////calculate some commonly used values
            //fontAtlas.SetTextureScaleInfo(
            //    resolvedTypeface.CalculateScaleToPixelFromPointSize(fontAtlas.OriginalFontSizePts),
            //    resolvedTypeface.CalculateScaleToPixelFromPointSize(reqFont.SizeInPoints));
            ////TODO: review here, use scaled or unscaled values
            //fontAtlas.SetCommonFontMetricValues(
            //    resolvedTypeface.Ascender,
            //    resolvedTypeface.Descender,
            //    resolvedTypeface.LineGap,
            //    resolvedTypeface.CalculateRecommendLineSpacing());

            ///
        }


        //-----------------------------------------------------------------------------------------------
        void BuiltAtlas_FromUserOptions()
        {

            //we can create font atlas by specific script-langs

            GlyphImage totalGlyphsImg = null;
            SimpleFontAtlasBuilder atlasBuilder = null;
            var glyphTextureGen = new GlyphTextureBitmapGenerator();
            //
            Typeface typeface = _typeface;

            if (!float.TryParse(txtSelectedFontSize.Text, out float fontSizeInPoints))
            {
                MessageBox.Show("err: selected font size " + txtSelectedFontSize.Text);
                return;
            }

            PixelFarm.Drawing.RequestFont reqFont = new PixelFarm.Drawing.RequestFont(
                typeface.Name,
                fontSizeInPoints,
                 PixelFarm.Drawing.FontStyle.Regular
                );


            //user may want only some scripts lang (not entire font)
            //so we specific 

            List<GlyphTextureBuildDetail> buildDetails1 = new List<GlyphTextureBuildDetail>();

            foreach (CustomUIFontScriptOpt scriptLangUI in _availableScripts)
            {
                if (scriptLangUI.Selected)
                {
                    buildDetails1.Add(new GlyphTextureBuildDetail()
                    {
                        ScriptLang = scriptLangUI.ScriptLang,
                        DoFilter = scriptLangUI.DoFilter,
                        HintTechnique = scriptLangUI.HintTechnique,
                    });
                }
            }

            GlyphTextureBuildDetail[] buildDetails = buildDetails1.ToArray();

            glyphTextureGen.CreateTextureFontFromBuildDetail(typeface,
                fontSizeInPoints,
                PixelFarm.Drawing.BitmapAtlas.TextureKind.StencilLcdEffect,
                buildDetails,
                (glyphIndex, glyphImage, outputAtlasBuilder) =>
                {
                    if (outputAtlasBuilder != null)
                    {
                        //finish
                        atlasBuilder = outputAtlasBuilder;
                    }
                });


            atlasBuilder.SpaceCompactOption = SimpleFontAtlasBuilder.CompactOption.ArrangeByHeight;
            totalGlyphsImg = atlasBuilder.BuildSingleImage();



            atlasBuilder.FontFilename = typeface.Name;
            atlasBuilder.FontKey = reqFont.FontKey;

            string textureName = typeface.Name.ToLower() + "_" + reqFont.FontKey;

            using (FileStream fs = new FileStream(textureName + ".info", FileMode.Create))
            {
                atlasBuilder.SaveFontInfo(fs);
            }


            string output_imgFilename = textureName + ".png";

            SaveGlyphImageToPngFile(totalGlyphsImg, output_imgFilename);
            this.picOutput.Image = new Bitmap(output_imgFilename);
            this.lblOutput.Text = "Output: " + output_imgFilename;


            ////read .info back and convert to base64
            //byte[] atlas_info_content = File.ReadAllBytes(textureName + ".info");
            //string base64 = Convert.ToBase64String(atlas_info_content);
            ////create atlas
            //SimpleFontAtlas fontAtlas = atlasBuilder.CreateSimpleFontAtlas();
            //fontAtlas.TotalGlyph = totalGlyphsImg;

        }

        static void SaveGlyphImageToPngFile(GlyphImage totalGlyphsImg, string imgFilename)
        {

            //TODO: use helper method 
            using (MemBitmap memBmp = MemBitmap.CreateFromCopy(totalGlyphsImg.Width, totalGlyphsImg.Height, totalGlyphsImg.GetImageBuffer()))
            using (System.Drawing.Bitmap bmp = new Bitmap(memBmp.Width, memBmp.Height))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, memBmp.Width, memBmp.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                var tmpMem = MemBitmap.GetBufferPtr(memBmp);
                unsafe
                {
                    PixelFarm.CpuBlit.NativeMemMx.MemCopy((byte*)bmpdata.Scan0,
                        (byte*)tmpMem.Ptr,
                        tmpMem.LengthInBytes);
                }
                bmp.UnlockBits(bmpdata);

                if (System.IO.File.Exists(imgFilename))
                {
                    System.IO.File.Delete(imgFilename);
                }
                bmp.Save(imgFilename);
            }
        }

        private void cmdBuildFromSelectedScriptLangs_Click(object sender, EventArgs e)
        {
            BuiltAtlas_FromUserOptions();
        }
    }
}
