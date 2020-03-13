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

namespace SampleWinForms
{
    public partial class FormFontAtlas : Form
    {
        Typeface _typeface;

        List<UIFontScriptOpt> _availableScripts = new List<UIFontScriptOpt>();



        class TextureKindAndDescription
        {
            public TextureKindAndDescription(PixelFarm.Drawing.BitmapAtlas.TextureKind kind, string desc)
            {
                Kind = kind;
                Description = desc;
            }

            public ushort TechniqueDetail { get; set; }
            public PixelFarm.Drawing.BitmapAtlas.TextureKind Kind { get; }
            public string Description { get; }

            public override string ToString()
            {
                return Description;
            }
        }

        public FormFontAtlas()
        {
            InitializeComponent();

            uiFontAtlasFileViewer1.BringToFront();

            this.cmbTextureKind.Items.Add(new TextureKindAndDescription(PixelFarm.Drawing.BitmapAtlas.TextureKind.StencilLcdEffect, "StencilLcd"));
            this.cmbTextureKind.Items.Add(new TextureKindAndDescription(PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf, "Msdf"));

            //msdf3 is our extension to the original Msdf technique
            this.cmbTextureKind.Items.Add(new TextureKindAndDescription(PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf, "Msdf3") { TechniqueDetail = 3 });

            this.cmbTextureKind.SelectedIndex = 0;//default
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
                            UIFontScriptOpt customUIFontScript = new UIFontScriptOpt();
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

            if (!float.TryParse(txtSelectedFontSize.Text, out float fontSizeInPoints))
            {
                MessageBox.Show("err: selected font size " + txtSelectedFontSize.Text);
                return;
            }


            //1. create glyph-texture-bitmap generator
            var glyphTextureGen = new GlyphTextureBitmapGenerator();

            //2. generate the glyphs
            TextureKindAndDescription textureKindAndDesc = (TextureKindAndDescription)this.cmbTextureKind.SelectedItem;
            if (textureKindAndDesc.Kind == PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf)
            {
                glyphTextureGen.MsdfGenVersion = textureKindAndDesc.TechniqueDetail;
            }

            SimpleFontAtlasBuilder atlasBuilder = glyphTextureGen.CreateTextureFontFromInputChars(
               _typeface,
               fontSizeInPoints,
               textureKindAndDesc.Kind,
               GetUniqueChars()
            );

            //3. set information before write to font-info
            atlasBuilder.SpaceCompactOption = SimpleFontAtlasBuilder.CompactOption.ArrangeByHeight;


            //4. merge all glyph in the builder into a single image
            MemBitmap totalGlyphsImg = atlasBuilder.BuildSingleImage();
            string fontTextureImg = "test_glyph_atlas.png";

            //5. save to png
            totalGlyphsImg.SaveImage(fontTextureImg);
            //-----------------------------------------------


            //let view result...
            SimpleUtils.DisposeExistingPictureBoxImage(picOutput);

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
            //-------------------------------------------------------------------------------

            //setting...
            Typeface typeface = _typeface;
            if (!float.TryParse(txtSelectedFontSize.Text, out float fontSizeInPoints))
            {
                MessageBox.Show("err: selected font size " + txtSelectedFontSize.Text);
                return;
            }

            //create request font, indeed we need its 'FontKey'
            PixelFarm.Drawing.RequestFont reqFont = new PixelFarm.Drawing.RequestFont(
                typeface.Name,
                fontSizeInPoints,
                 PixelFarm.Drawing.FontStyle.Regular
                );


            //user may want only some script lang (not all script in the font)            
            //and each script user may want different glyph-gen technique.
            //so we use GlyphTextureBuildDetail to describe that information.

            List<GlyphTextureBuildDetail> buildDetails = new List<GlyphTextureBuildDetail>();

            foreach (UIFontScriptOpt scriptLangUI in _availableScripts)
            {
                if (scriptLangUI.Selected)
                {
                    buildDetails.Add(new GlyphTextureBuildDetail()
                    {
                        ScriptLang = scriptLangUI.ScriptLang,
                        DoFilter = scriptLangUI.DoFilter,
                        HintTechnique = scriptLangUI.HintTechnique,
                    });
                }
            }

            if (buildDetails.Count == 0)
            {
                MessageBox.Show("please select some script");
                return;
            }

            //-------------------------------------------------------------------------------


            //1. create glyph-texture-bitmap generator
            var glyphTextureGen = new GlyphTextureBitmapGenerator();

            //2. generate the glyphs
            TextureKindAndDescription textureKindAndDesc = (TextureKindAndDescription)this.cmbTextureKind.SelectedItem;
            if (textureKindAndDesc.Kind == PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf)
            {
                glyphTextureGen.MsdfGenVersion = textureKindAndDesc.TechniqueDetail;
            }

#if DEBUG
            //overall, glyph atlas generation time
            System.Diagnostics.Stopwatch dbugStopWatch = new System.Diagnostics.Stopwatch();
            dbugStopWatch.Start();
#endif
            SimpleFontAtlasBuilder atlasBuilder = glyphTextureGen.CreateTextureFontFromBuildDetail(typeface,
                fontSizeInPoints,
                textureKindAndDesc.Kind,
                buildDetails.ToArray());

            //3. set information before write to font-info
            atlasBuilder.SpaceCompactOption = SimpleFontAtlasBuilder.CompactOption.ArrangeByHeight;
            atlasBuilder.FontFilename = typeface.Name;
            atlasBuilder.FontKey = reqFont.FontKey;

            //4. merge all glyph in the builder into a single image
            MemBitmap totalGlyphsImg = atlasBuilder.BuildSingleImage();

#if DEBUG
            dbugStopWatch.Stop();
            this.Text += ", finished: build time(ms)=" + dbugStopWatch.ElapsedMilliseconds;
            System.Diagnostics.Debug.WriteLine("font atlas build time (ms): " + dbugStopWatch.ElapsedMilliseconds);
#endif

            string textureName = typeface.Name.ToLower() + "_" + reqFont.FontKey;
            string output_imgFilename = textureName + ".png";

            //5. save atlas info to disk
            using (FileStream fs = new FileStream(textureName + ".info", FileMode.Create))
            {
                atlasBuilder.SaveAtlasInfo(fs);
            }

            //6. save total-glyph-image to disk
            totalGlyphsImg.SaveImage(output_imgFilename);

            ///------------------------------------------------
            //lets view result ...

            SimpleUtils.DisposeExistingPictureBoxImage(picOutput);

            uiFontAtlasFileViewer1.LoadFontAtlasFile(textureName + ".info", textureName + ".png");

            this.picOutput.Image = new Bitmap(output_imgFilename);
            this.lblOutput.Text = "Output: " + output_imgFilename;

            ////read .info back and convert to base64
            //byte[] atlas_info_content = File.ReadAllBytes(textureName + ".info");
            //string base64 = Convert.ToBase64String(atlas_info_content);
            ////create atlas
            //SimpleFontAtlas fontAtlas = atlasBuilder.CreateSimpleFontAtlas();
            //fontAtlas.TotalGlyph = totalGlyphsImg;

        }


        private void cmdBuildFromSelectedScriptLangs_Click(object sender, EventArgs e)
        {
            BuiltAtlas_FromUserOptions();
        }

        private void cmdShowAtlasViewer_Click(object sender, EventArgs e)
        {

        }

        private void chkShowAtlasViewer_CheckedChanged(object sender, EventArgs e)
        {
            uiFontAtlasFileViewer1.Visible = chkShowAtlasViewer.Checked;
        }
    }
}
