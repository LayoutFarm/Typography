//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using System.Drawing;

using System.IO;
using System.Windows.Forms;

using PixelFarm.CpuBlit;
using PixelFarm.CpuBlit.BitmapAtlas;
using PixelFarm.Contours;

using Typography.OpenFont;
using Typography.OpenFont.Trimable;
using Typography.TextLayout;
using Typography.Contours;
using Typography.WebFont;

using BrotliSharpLib;


namespace SampleWinForms
{
    public partial class FormTestTrimableFeature : Form
    {
        public FormTestTrimableFeature()
        {
            InitializeComponent();
        }

        private void FormTestTrimableFeature_Load(object sender, EventArgs e)
        {

        }
        static void TestLoadAndReload(string filename)
        {

            Typeface typeface = null;
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                //read in full mode
                OpenFontReader openFontReader = new OpenFontReader();
                typeface = openFontReader.Read(fs);
            }

            //before
            bool hasColor1 = typeface.HasColorTable();
            bool hasSvg1 = typeface.HasSvgTable();
            bool hasCff1 = typeface.IsCffFont;

            TrimMode glyphMode1 = typeface.GetTrimMode();
            RestoreTicket ticket = typeface.TrimDown();

            //after reload with a fewer version
            //test get glyph again, you will get a new instance of glyph (with the same glyph index)
            Glyph g1 = typeface.GetGlyph(1);

            bool hasColor2 = typeface.HasColorTable();
            bool hasSvg2 = typeface.HasSvgTable();
            bool hasCff2 = typeface.IsCffFont;

            TrimMode glyphMode2 = typeface.GetTrimMode();

            //can we load glyph detail again?
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            {
                typeface.RestoreUp(ticket, fs);
            }
        }
        //-------
        //test incorrect restore
        void TestIncorrectRestore(string file1, string file2)
        {

            Typeface typeface = null;
            using (FileStream fs = new FileStream(file1, FileMode.Open))
            {
                //read in full mode
                OpenFontReader openFontReader = new OpenFontReader();
                typeface = openFontReader.Read(fs);
            }

            RestoreTicket ticket = typeface.TrimDown();
            //test restore with incorrect file
            using (FileStream fs = new FileStream(file2, FileMode.Open))
            {
                typeface.RestoreUp(ticket, fs);
            }
        }

        private void cmdTestReloadGlyphs_Click(object sender, EventArgs e)
        {
            // string filename = "Test/SourceSansPro-Regular.ttf";
            string[] files = new string[]
            {
                "Test/TwitterColorEmoji-SVGinOT.ttf" ,//svg in opentype font
                "Test/NotoColorEmoji.ttf",   //embeded bitmap font
                "Test/SourceSansPro-Regular.ttf", //ttf
                "Test/latinmodern-math.otf", //otf (cff)
                "Test/Sarabun-Regular.woff2",
                "Test/Sarabun-Regular.woff",
            };


            TestIncorrectRestore(files[0], files[1]);

            foreach (string filename in files)
            {
                TestLoadAndReload(filename);
            } 
            
        }
    }
}
