//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using System.Windows.Forms;

using Typography.OpenFont;
using Typography.Rendering;
using Typography.Contours;

using PixelFarm.Drawing;
using PixelFarm.CpuBlit;
using PixelFarm.Drawing.Fonts;
using PixelFarm.Contours;

namespace SampleWinForms
{
    public partial class FormMsdfTest2 : Form
    {
        LayoutFarm.OpenFontTextService _textServices;
        public FormMsdfTest2()
        {
            InitializeComponent();
            //
            _textServices = new LayoutFarm.OpenFontTextService();

        }

        private void cmdTestMsdfGen_Click(object sender, EventArgs e)
        {

            //samples...
            //1. create texture from specific glyph index range
            string sampleFontFile = (string)lstFontFiles.SelectedItem;

            if (!int.TryParse(txtFontSize.Text, out int fontSize))
            {
                fontSize = 18;
            }

            if (!ushort.TryParse(txtGlyphIndexStart.Text, out ushort glyphIndexStart))
            {
                glyphIndexStart = 0;
            }

            if (!ushort.TryParse(txtGlyphIndexStop.Text, out ushort glyphIndexStop))
            {
                glyphIndexStop = 1;
            }


            CreateSampleMsdfTextureFont(
                sampleFontFile,
                fontSize,
                glyphIndexStart,
                glyphIndexStop,
                "sample_msdf.png");
            //---------------------------------------------------------
            //2. for debug, create from some unicode chars
            //
            //CreateSampleMsdfTextureFont(
            //   sampleFontFile,
            //   18,
            //  new char[] { 'I' },
            //  "sample_msdf.png");
            //---------------------------------------------------------
            ////3.
            //GlyphTranslatorToContour tx = new GlyphTranslatorToContour();
            //tx.BeginRead(1);
            ////tx.MoveTo(10, 10);
            ////tx.LineTo(25, 25);
            ////tx.LineTo(15, 10);
            //tx.MoveTo(3.84f, 0);
            //tx.LineTo(1.64f, 0);
            //tx.LineTo(1.64f, 18.23f);
            //tx.LineTo(3.84f, 18.23f);
            //tx.CloseContour();
            //tx.EndRead();
            ////
            //CreateSampleMsdfImg(tx, "d:\\WImageTest\\tx_contour2.bmp");
        }

        class GlyphContourBuilder2 : IGlyphTranslator
        {
            ContourBuilder _c;
            public GlyphContourBuilder2(ContourBuilder c)
            {
                _c = c;
            }
            public void BeginRead(int contourCount) => _c.BeginRead(contourCount);

            public void CloseContour() => _c.CloseContour();

            public void Curve3(float x1, float y1, float x2, float y2) => _c.Curve3(x1, y1, x2, y2);

            public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3) => _c.Curve4(x1, y1, x2, y2, x3, y3);

            public void EndRead() => _c.EndRead();

            public void LineTo(float x1, float y1) => _c.LineTo(x1, y1);

            public void MoveTo(float x0, float y0) => _c.MoveTo(x0, y0);
        }

        static void CreateSampleMsdfTextureFont(string fontfile, float sizeInPoint, ushort startGlyphIndex, ushort endGlyphIndex, string outputFile)
        {
            //sample

            var reader = new OpenFontReader();

            Typeface typeface = null;
            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                typeface = reader.Read(fs);
            }

            //sample: create sample msdf texture 
            //-------------------------------------------------------------
            var builder = new GlyphOutlineBuilder(typeface);
            //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
            //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
            //-------------------------------------------------------------
            RequestFont reqFont = new RequestFont(typeface.Name, sizeInPoint);

            var atlasBuilder = new SimpleFontAtlasBuilder();
            atlasBuilder.FontFilename = System.IO.Path.GetFileName(fontfile);
            atlasBuilder.FontKey = reqFont.FontKey;
            //create temp folder for each glyph

            string tempFolderName = "tmp_msdf";
            if (Directory.Exists(tempFolderName))
            {
                //DANGER!
                Directory.Delete(tempFolderName, true);
            }
            Directory.CreateDirectory(tempFolderName);

            if (endGlyphIndex < 1)
            {
                endGlyphIndex = (ushort)(typeface.GlyphCount - 1);
            }


            for (ushort gindex = startGlyphIndex; gindex <= endGlyphIndex; ++gindex)
            {
                //build glyph
                builder.BuildFromGlyphIndex(gindex, sizeInPoint);

                var glyphContourBuilder = new ContourBuilder();
                //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                var genParams = new MsdfGenParams();
                builder.ReadShapes(new GlyphContourBuilder2(glyphContourBuilder));
                //genParams.shapeScale = 1f / 64; //we scale later (as original C++ code use 1/64)
                GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphContourBuilder, genParams);
                atlasBuilder.AddGlyph(gindex, glyphImg);

                using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                {
                    int[] buffer = glyphImg.GetImageBuffer();
                    var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg.Width, glyphImg.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                    System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                    bmp.UnlockBits(bmpdata);
                    bmp.Save(tempFolderName + "//glyph_" + gindex + ".png");
                }
            }

            GlyphImage glyphImg2 = atlasBuilder.BuildSingleImage();
            using (Bitmap bmp = new Bitmap(glyphImg2.Width, glyphImg2.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg2.Width, glyphImg2.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                int[] intBuffer = glyphImg2.GetImageBuffer();

                System.Runtime.InteropServices.Marshal.Copy(intBuffer, 0, bmpdata.Scan0, intBuffer.Length);
                bmp.UnlockBits(bmpdata);
                bmp.Save(outputFile);
            }

            string saveToFile = "a_info.bin";
            using (System.IO.FileStream saveFs = new FileStream(saveToFile, FileMode.Create))
            {
                atlasBuilder.SaveFontInfo(saveFs);
                saveFs.Flush();
                saveFs.Close();
            }

            //
            //-----------
            //test read texture info back
            var atlasBuilder2 = new SimpleFontAtlasBuilder();
            using (System.IO.FileStream readFromFs = new FileStream(saveToFile, FileMode.Open))
            {
                var readbackFontAtlas = atlasBuilder2.LoadFontAtlasInfo(readFromFs);
            }

        }

        static void CreateSampleMsdfImg(ContourBuilder tx, string outputFile)
        {
            //sample

            MsdfGenParams msdfGenParams = new MsdfGenParams();
            GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(tx, msdfGenParams);
            int w = glyphImg.Width;
            int h = glyphImg.Height;
            using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                int[] imgBuffer = glyphImg.GetImageBuffer();
                System.Runtime.InteropServices.Marshal.Copy(imgBuffer, 0, bmpdata.Scan0, imgBuffer.Length);
                bmp.UnlockBits(bmpdata);
                bmp.Save(outputFile);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {

            //1. 
            MsdfGenParams msdfGenParams = new MsdfGenParams();
            //GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(tx, msdfGenParams);
            Msdfgen.Shape shape1 = new Msdfgen.Shape();
            //
            Msdfgen.Contour cnt = new Msdfgen.Contour();
            //cnt.AddLine(0, 0, 50, 0);
            //cnt.AddLine(50, 0, 50, 50);
            //cnt.AddLine(50, 50, 0, 50);
            //cnt.AddLine(0, 50, 0, 0);
            //cnt.AddLine(10, 20, 50, 0);
            //cnt.AddLine(50, 0, 80, 20);
            //cnt.AddLine(80, 20, 50, 60);
            //cnt.AddLine(50, 60, 10, 20);

            //for msdf we draw shape clock-wise 
            cnt.AddLine(10, 20, 50, 60);
            cnt.AddLine(50, 60, 80, 20);
            cnt.AddLine(80, 20, 50, 0);
            cnt.AddLine(50, 0, 10, 20);
            shape1.contours.Add(cnt);
            //
            //
            var genParams = new MsdfGenParams();
            GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(shape1, genParams);

            using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                int[] buffer = glyphImg.GetImageBuffer();

                var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg.Width, glyphImg.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                bmp.UnlockBits(bmpdata);
                bmp.Save("msdf_shape.png");
                //
            }


        }


        class LocalFileStorageProvider : PixelFarm.Platforms.StorageServiceProvider
        {


            readonly string _baseDir;
            public LocalFileStorageProvider(string baseDir, bool disableAbsolutePath = false)
            {
                _baseDir = baseDir;
                DisableAbsolutePath = disableAbsolutePath;
            }
            public string BaseDir => _baseDir;

            public bool DisableAbsolutePath { get; }
            public override string[] GetDataNameList(string dir)
            {
                if (Path.IsPathRooted(dir))
                {
                    if (DisableAbsolutePath) return null;
                }
                else
                {
                    dir = Path.Combine(_baseDir, dir);
                }
                return System.IO.Directory.GetFiles(dir);
            }
            public override string[] GetDataDirNameList(string dir)
            {
                if (Path.IsPathRooted(dir))
                {
                    if (DisableAbsolutePath) return null;
                }
                else
                {
                    dir = Path.Combine(_baseDir, dir);
                }
                return System.IO.Directory.GetFiles(dir);
            }
            public override bool DataExists(string dataName)
            {
                //implement with file 

                if (Path.IsPathRooted(dataName))
                {
                    if (DisableAbsolutePath) return false;
                }
                else
                {
                    dataName = Path.Combine(_baseDir, dataName);
                }

                return System.IO.File.Exists(dataName);
            }
            public override byte[] ReadData(string dataName)
            {

                if (Path.IsPathRooted(dataName))
                {
                    if (DisableAbsolutePath) return null;
                }
                else
                {
                    dataName = Path.Combine(_baseDir, dataName);
                }

                return System.IO.File.ReadAllBytes(dataName);
            }
            public override void SaveData(string dataName, byte[] content)
            {

                if (Path.IsPathRooted(dataName))
                {
                    if (DisableAbsolutePath) return;
                }
                else
                {
                    dataName = Path.Combine(_baseDir, dataName);
                }

#if !__MOBILE__
                //TODO: review here, save data on android
                System.IO.File.WriteAllBytes(dataName, content);
#endif
            }
        }


        private void button1_Click_1(object sender, EventArgs e)
        {

            PixelFarm.CpuBlit.MemBitmapExtensions.DefaultMemBitmapIO = new PixelFarm.Drawing.WinGdi.GdiBitmapIO();


            var storageProvider = new LocalFileStorageProvider("", true);
            PixelFarm.Platforms.StorageService.RegisterProvider(storageProvider);


            var bmpFontMx = new BitmapFontManager<MemBitmap>(
                 _textServices,
                 atlas =>
                 {
                     GlyphImage totalGlyphImg = atlas.TotalGlyph;
                     return MemBitmap.CreateFromCopy(totalGlyphImg.Width, totalGlyphImg.Height, totalGlyphImg.GetImageBuffer());
                 }
             );

            string multiSizeFontAtlasFilename = "tahoma_set1.multisize_fontAtlas";
            string totalImgAtlasFilename = "tahoma_set1.multisize_fontAtlas.png";
            //in this version, mutlsize font texture must use the same typeface
            {
                MultiSizeFontAtlasBuilder multiSizeFontAtlasBuilder = new MultiSizeFontAtlasBuilder();
                {
                    bmpFontMx.TextureKindForNewFont = PixelFarm.Drawing.BitmapAtlas.TextureKind.StencilLcdEffect;
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 10), bmpFontMx);
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 11), bmpFontMx);
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 12), bmpFontMx);
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 13), bmpFontMx);
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 14), bmpFontMx);

                    bmpFontMx.TextureKindForNewFont = PixelFarm.Drawing.BitmapAtlas.TextureKind.Msdf;
                    AddExistingOrCreateNewSimpleFontAtlas(multiSizeFontAtlasBuilder, new RequestFont("tahoma", 24), bmpFontMx);
                }

                //-------
                multiSizeFontAtlasBuilder.BuildMultiFontSize(multiSizeFontAtlasFilename, totalImgAtlasFilename);

            }
            {
                //test load the font altas back
                FontAtlasFile atlasFile = new FontAtlasFile();
                using (FileStream fs = new FileStream(multiSizeFontAtlasFilename, FileMode.Open))
                {
                    atlasFile.Read(fs);
                }

            }

        }
        void AddExistingOrCreateNewSimpleFontAtlas(
            MultiSizeFontAtlasBuilder multisizeFontAtlasBuilder,
            RequestFont reqFont,
            BitmapFontManager<MemBitmap> bmpFontMx)
        {
            int fontKey = reqFont.FontKey;

            string fontTextureFile = reqFont.Name + "_" + fontKey;
            string resolveFontTextureFile = fontTextureFile + ".info";
            string fontTextureInfoFile = resolveFontTextureFile;
            string fontTextureImgFilename = fontTextureInfoFile + ".png";

            _textServices.ResolveTypeface(reqFont); //resolve for 'actual' font
            if (PixelFarm.Platforms.StorageService.Provider.DataExists(resolveFontTextureFile) &&
                File.Exists(fontTextureImgFilename))
            {
                multisizeFontAtlasBuilder.AddSimpleFontAtlasFile(reqFont,
                    resolveFontTextureFile,
                    fontTextureImgFilename,
                    bmpFontMx.TextureKindForNewFont
                    );
            }
            else
            {
                //create a new one 
                PixelFarm.Drawing.Fonts.SimpleFontAtlas fontAtlas = bmpFontMx.GetFontAtlas(reqFont, out MemBitmap fontBmp);
                bmpFontMx.GetFontAtlas(reqFont, out fontBmp);
                multisizeFontAtlasBuilder.AddSimpleFontAtlasFile(reqFont,
                    resolveFontTextureFile,
                    fontTextureImgFilename,
                    bmpFontMx.TextureKindForNewFont);
            }

        }

        private void FormMsdfTest2_Load(object sender, EventArgs e)
        {
            string[] fontfiles = Directory.GetFiles("Test");//example only, you can change this
            lstFontFiles.Items.AddRange(fontfiles);
            if (fontfiles.Length > 0)
            {
                lstFontFiles.SelectedIndex = 0;
            }
        }

        //public static Msdfgen.Shape CreateMsdfShape(GlyphContourBuilder glyphToContour, float pxScale)
        //{
        //    List<GlyphContour> cnts = glyphToContour.GetContours();
        //    List<GlyphContour> newFitContours = new List<GlyphContour>();
        //    int j = cnts.Count;
        //    for (int i = 0; i < j; ++i)
        //    {
        //        newFitContours.Add(
        //            CreateFitContour(
        //                cnts[i], pxScale, false, true));
        //    }
        //    return CreateMsdfShape(newFitContours);
        //}
    }
}
