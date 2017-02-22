//MIT, 2016-2017, WinterDev
using System.IO;
using System.Drawing;
using System.Windows.Forms;

using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        string _currentSelectedFontFile;
        int fontSizeInPoint = 14;//default
        public Form1()
        {
            InitializeComponent();
            //----------
            button1.Click += (s, e) => UpdateRenderOutput();
            //simple load test fonts from local test dir
            //and send it into test list
            chkFillBackground.Checked = true;
            chkBorder.CheckedChanged += (s, e) => UpdateRenderOutput();
            chkFillBackground.CheckedChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbPositionTech.Items.Add(PositionTecnhique.OpenFont);
            cmbPositionTech.Items.Add(PositionTecnhique.Kerning);
            cmbPositionTech.Items.Add(PositionTecnhique.None);
            cmbPositionTech.SelectedIndex = 0;
            cmbPositionTech.SelectedIndexChanged += (s, e) => UpdateRenderOutput();
            //----------
            cmbHintTechnique.Items.Add(HintTechnique.None);
            cmbHintTechnique.Items.Add(HintTechnique.TrueTypeInstruction);
            cmbHintTechnique.Items.Add(HintTechnique.TrueTypeInstruction_VerticalOnly);
            cmbHintTechnique.Items.Add(HintTechnique.CustomAutoFit);
            cmbHintTechnique.SelectedIndex = 0;
            cmbHintTechnique.SelectedIndexChanged += (s, e) => UpdateRenderOutput();

            txtInputChar.TextChanged += (s, e) => UpdateRenderOutput();
            //
            int selectedFileIndex = -1;
            //string selectedFontFileName = "pala.ttf";
            string selectedFontFileName = "tahoma.ttf";
            //string selectedFontFileName="cambriaz.ttf";
            //string selectedFontFileName="CompositeMS2.ttf"; 
            int fileIndexCount = 0;

            foreach (string file in Directory.GetFiles("..\\..\\..\\TestFonts", "*.ttf"))
            {
                var tmpLocalFile = new TempLocalFontFile(file);
                lstFontList.Items.Add(tmpLocalFile);
                if (selectedFileIndex < 0 && tmpLocalFile.OnlyFileName == selectedFontFileName)
                {
                    selectedFileIndex = fileIndexCount;
                    _currentSelectedFontFile = file;
                }
                fileIndexCount++;
            }
            if (selectedFileIndex < 0) { selectedFileIndex = 0; }
            lstFontList.SelectedIndex = selectedFileIndex;
            lstFontList.SelectedIndexChanged += (s, e) =>
            {
                _currentSelectedFontFile = ((TempLocalFontFile)lstFontList.SelectedItem).actualFileName;
                UpdateRenderOutput();
            };
            //----------
            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72,240,300,360
                });
            lstFontSizes.SelectedIndex = 0;
            lstFontSizes.SelectedIndexChanged += (s, e) =>
            {
                //new font size
                fontSizeInPoint = (int)lstFontSizes.SelectedItem;
                UpdateRenderOutput();
            };
        }
        void UpdateRenderOutput()
        {
            //render glyph with gdi path
            if (g == null)
            {

                g = this.CreateGraphics();
            }
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            var reader = new OpenFontReader();
            char testChar = txtInputChar.Text[0];//only 1 char 
            int resolution = 96;
            //1. read typeface from font file
            using (var fs = new FileStream(_currentSelectedFontFile, FileMode.Open))
            {
                Typeface typeFace = reader.Read(fs);
                RenderWithGdiPlusPath(typeFace, testChar, fontSizeInPoint, resolution);
            }
        }
        void RenderWithGdiPlusPath(Typeface typeface, char testChar, float sizeInPoint, int resolution)
        {

            //render glyph path with Gdi+ path 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //////credit:
            //////http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly  


            //----------------------------------------------------
            var builder = new MyGlyphPathBuilder(typeface);
            var hintTech = (HintTechnique)cmbHintTechnique.SelectedItem;
            builder.UseTrueTypeInstructions = false;//reset
            builder.UseVerticalHinting = false;//reset
            switch (hintTech)
            {
                case HintTechnique.TrueTypeInstruction:
                    builder.UseTrueTypeInstructions = true;
                    break;
                case HintTechnique.TrueTypeInstruction_VerticalOnly:
                    builder.UseTrueTypeInstructions = true;
                    builder.UseVerticalHinting = true;
                    break;
                case HintTechnique.CustomAutoFit:
                    //custom agg autofit 
                    break;
            }
            //---------------------------------------------------- 
            builder.Build(testChar, sizeInPoint);
            var gdiPathBuilder = new GlyphPathBuilderGdi();
            builder.ReadShapes(gdiPathBuilder);
            float pxScale = builder.GetPixelScale();

            System.Drawing.Drawing2D.GraphicsPath path = gdiPathBuilder.ResultGraphicPath;
            path.Transform(
                new System.Drawing.Drawing2D.Matrix(
                    pxScale, 0,
                    0, pxScale,
                    0, 0
                ));

            if (chkFillBackground.Checked)
            {
                g.FillPath(Brushes.Black, path);
            }
            if (chkBorder.Checked)
            {
                g.DrawPath(Pens.Green, path);
            }
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            
        }

        private void cmdBuildMsdfTexture_Click(object sender, System.EventArgs e)
        {
            string sampleFontFile = @"..\..\..\TestFonts\tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                255,
                "d:\\WImageTest\\sample_msdf.png");
        }
        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample
            var reader = new OpenFontReader();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeface = reader.Read(fs);
                //sample: create sample msdf texture 
                //-------------------------------------------------------------
                var builder = new MyGlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder2();
                var msdfBuilder = new MsdfGlyphGen();

                for (ushort n = startGlyphIndex; n <= endGlyphIndex; ++n)
                {
                    //build glyph
                    builder.BuildFromGlyphIndex(n, sizeInPoint);

                    var msdfGlyphGen = new MsdfGlyphGen();
                    var actualImg = msdfGlyphGen.CreateMsdfImage(
                        builder.GetOutputPoints(),
                        builder.GetOutputContours(),
                        builder.GetPixelScale());
                    atlasBuilder.AddGlyph((int)n, actualImg);

                    //using (Bitmap bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    //{
                    //    var bmpdata = bmp.LockBits(new Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    //    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                    //    bmp.UnlockBits(bmpdata);
                    //    bmp.Save("d:\\WImageTest\\a001_xn2_" + n + ".png");
                    //}
                }

                var glyphImg2 = atlasBuilder.BuildSingleImage();
                using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    int[] intBuffer = glyphImg2.GetImageBuffer();

                    System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save("d:\\WImageTest\\a_total.png");
                }
                atlasBuilder.SaveFontInfo("d:\\WImageTest\\a_info.xml");
            }
        }
    }
}
