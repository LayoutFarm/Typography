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


namespace SampleWinForms
{
    public partial class FormMsdfTest2 : Form
    {
        public FormMsdfTest2()
        {
            InitializeComponent();
        }

        private void cmdTestMsdfGen_Click(object sender, EventArgs e)
        {

            //samples...
            //1. create texture from specific glyph index range
            string sampleFontFile = "../../../TestFonts/tahoma.ttf";
            CreateSampleMsdfTextureFont(
                sampleFontFile,
                18,
                0,
                100,
                "d:\\WImageTest\\sample_msdf.png");
            //---------------------------------------------------------
            //2. for debug, create from some unicode chars
            //
            //CreateSampleMsdfTextureFont(
            //   sampleFontFile,
            //   18,
            //  new char[] { 'I' },
            //  "d:\\WImageTest\\sample_msdf.png");
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
                var builder = new GlyphPathBuilder(typeface);
                //builder.UseTrueTypeInterpreter = this.chkTrueTypeHint.Checked;
                //builder.UseVerticalHinting = this.chkVerticalHinting.Checked;
                //-------------------------------------------------------------
                var atlasBuilder = new SimpleFontAtlasBuilder();


                for (ushort gindex = startGlyphIndex; gindex <= endGlyphIndex; ++gindex)
                {
                    //build glyph
                    builder.BuildFromGlyphIndex(gindex, sizeInPoint);

                    var glyphContourBuilder = new GlyphContourBuilder();
                    //glyphToContour.Read(builder.GetOutputPoints(), builder.GetOutputContours());
                    var genParams = new MsdfGenParams();
                    builder.ReadShapes(glyphContourBuilder);
                    //genParams.shapeScale = 1f / 64; //we scale later (as original C++ code use 1/64)
                    GlyphImage glyphImg = MsdfGlyphGen.CreateMsdfImage(glyphContourBuilder, genParams);
                    atlasBuilder.AddGlyph(gindex, glyphImg);

                    using (Bitmap bmp = new Bitmap(glyphImg.Width, glyphImg.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        int[] buffer = glyphImg.GetImageBuffer();

                        var bmpdata = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glyphImg.Width, glyphImg.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
                        System.Runtime.InteropServices.Marshal.Copy(buffer, 0, bmpdata.Scan0, buffer.Length);
                        bmp.UnlockBits(bmpdata);
                        bmp.Save("d:\\WImageTest\\a001_xn2_" + gindex + ".png");
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

                string saveToFile = "d:\\WImageTest\\a_info.bin";
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
                    var readbackFontAtlas = atlasBuilder2.LoadFontInfo(readFromFs);
                }
            }
        }

        static void CreateSampleMsdfImg(GlyphContourBuilder tx, string outputFile)
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
                bmp.Save("d:\\WImageTest\\msdf_shape.png");
                //
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
