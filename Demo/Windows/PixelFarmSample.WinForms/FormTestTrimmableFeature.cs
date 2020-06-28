//MIT, 2016-present, WinterDev
using System;
using System.IO;
using System.Windows.Forms;

using Typography.OpenFont;
using Typography.OpenFont.Trimmable;

namespace SampleWinForms
{
    public partial class FormTestTrimmableFeature : Form
    {
        public FormTestTrimmableFeature()
        {
            InitializeComponent();
        }

        private void FormTestTrimmableFeature_Load(object sender, EventArgs e)
        {

        }
        static void TestLoadAndReload(string filename)
        {

            //Trimmable feature tests:           
            //[A] read the font file as usual => get full information about the font 
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
            Glyph g1_1 = typeface.GetGlyph(1);

            //---------------------------------------------------------
            //[B] if you create paths from glyphs, or atlas from glyph 
            //   and you don't want any glyph-building-detail (eg. to reduce memory usuage)
            //   but you still want to use the typeface for text-layout
            //   you can trim it down
            RestoreTicket ticket = typeface.TrimDown();//***

            //[C] you can GetGlyph() but this is ANOTHER NEW GLYPH
            //without building instruction( eg. no cff,ttf,svg data,bitmap)
            //

            Glyph g1_2 = typeface.GetGlyph(1);

            //** if you cache the the old version of 'full-info' glyph**
            // the info is still cache on the old glyph and it can be used as 'full-info' glyph
            // TrimDown() DOES NOT go to delete that glyph.

            bool hasColor2 = typeface.HasColorTable();
            bool hasSvg2 = typeface.HasSvgTable();
            bool hasCff2 = typeface.IsCffFont;

            TrimMode glyphMode2 = typeface.GetTrimMode();
            //---------------------------------------------------------

            //[D] can we load glyph detail again?
            //yes=> this need 'ticket' from latest TrimDown()
            //if you don't have it, you can't restore it.           

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
                "Test/Sarabun-Regular.woff2", //woff2
                "Test/Sarabun-Regular.woff", //woff
            };

            TestIncorrectRestore(files[0], files[1]);

            foreach (string filename in files)
            {
                TestLoadAndReload(filename);
            }

        }
    }
}
