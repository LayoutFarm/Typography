//MIT, 2016,  WinterDev
using System;
using System.Collections.Generic;

using System.Drawing;
using System.IO;
using System.Windows.Forms;

using NOpenType;
using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;

namespace SampleWinForms
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter p;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;
        static CurveFlattener curveFlattener = new CurveFlattener();

        public Form1()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);

            cmbRenderChoices.Items.Add(RenderChoice.RenderWithMiniAgg);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithPlugableGlyphRasterizer);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithTypePlanAndMiniAgg);
            cmbRenderChoices.SelectedIndex = 0;
            cmbRenderChoices.SelectedIndexChanged += new EventHandler(cmbRenderChoices_SelectedIndexChanged);


            lstFontSizes.Items.AddRange(
                new object[]{
                    8, 9,
                    10,11,
                    12,
                    14,
                    16,
                    18,20,22,24,26,28,36,48,72
                });
        }


        enum RenderChoice
        {
            RenderWithMiniAgg,
            RenderWithPlugableGlyphRasterizer, //new 
            RenderWithTypePlanAndMiniAgg, //new
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
            this.lstFontSizes.SelectedIndex = 5;

        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (g == null)
            {
                destImg = new ActualImage(400, 300, PixelFarm.Agg.Image.PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg, null); //no platform
                p = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();
            }
            //  ReadAndRender(@"..\..\segoeui.ttf");
            ReadAndRender(@"..\..\tahoma.ttf");
            //ReadAndRender(@"..\..\CompositeMS.ttf");
        }

        float fontSizeInPoint = 14; //default
        void ReadAndRender(string fontfile)
        {
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            var reader = new OpenTypeReader();
            char testChar = txtInputChar.Text[0];//only 1 char

            int resolution = 96;

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeFace = reader.Read(fs);

#if DEBUG
                string inputstr = "ก่นกิ่น";
                List<int> outputGlyphIndice = new List<int>();
                typeFace.Lookup(inputstr.ToCharArray(), outputGlyphIndice);
#endif



                RenderChoice renderChoice = (RenderChoice)this.cmbRenderChoices.SelectedItem;
                switch (renderChoice)
                {
                    case RenderChoice.RenderWithMiniAgg:
                        RenderWithMiniAgg(typeFace, testChar, fontSizeInPoint);
                        break;

                    case RenderChoice.RenderWithPlugableGlyphRasterizer:
                        RenderWithPlugableGlyphRasterizer(typeFace, testChar, fontSizeInPoint, resolution);
                        break;
                    case RenderChoice.RenderWithTypePlanAndMiniAgg:
                        RenderWithTextPrinterAndMiniAgg(typeFace, this.txtInputChar.Text, fontSizeInPoint, resolution);
                        break;
                    default:
                        throw new NotSupportedException();

                }
            }
        }


        static int s_POINTS_PER_INCH = 72; //default value, 
        static int s_PIXELS_PER_INCH = 96; //default value
        public static float ConvEmSizeInPointsToPixels(float emsizeInPoint)
        {
            return (int)(((float)emsizeInPoint / (float)s_POINTS_PER_INCH) * (float)s_PIXELS_PER_INCH);
        }

        //-------------------
        //https://www.microsoft.com/typography/otspec/TTCH01.htm
        //Converting FUnits to pixels
        //Values in the em square are converted to values in the pixel coordinate system by multiplying them by a scale. This scale is:
        //pointSize * resolution / ( 72 points per inch * units_per_em )
        //where pointSize is the size at which the glyph is to be displayed, and resolution is the resolution of the output device.
        //The 72 in the denominator reflects the number of points per inch.
        //For example, assume that a glyph feature is 550 FUnits in length on a 72 dpi screen at 18 point. 
        //There are 2048 units per em. The following calculation reveals that the feature is 4.83 pixels long.
        //550 * 18 * 72 / ( 72 * 2048 ) = 4.83
        //-------------------
        public static float ConvFUnitToPixels(ushort reqFUnit, float fontSizeInPoint, ushort unitPerEm)
        {
            //reqFUnit * scale             
            return reqFUnit * GetFUnitToPixelsScale(fontSizeInPoint, unitPerEm);
        }
        public static float GetFUnitToPixelsScale(float fontSizeInPoint, ushort unitPerEm)
        {
            //reqFUnit * scale             
            return ((fontSizeInPoint * s_PIXELS_PER_INCH) / (s_POINTS_PER_INCH * unitPerEm));
        }

        //from http://www.w3schools.com/tags/ref_pxtoemconversion.asp
        //set default
        // 16px = 1 em
        //-------------------
        //1. conv font design unit to em
        // em = designUnit / unit_per_Em       
        //2. conv font design unit to pixels


        // float scale = (float)(size * resolution) / (pointsPerInch * _typeface.UnitsPerEm);



        void RenderWithMiniAgg(Typeface typeface, char testChar, float sizeInPoint)
        {
            //2. glyph-to-vxs builder
            var builder = new GlyphPathBuilderVxs(typeface);
            builder.Build(testChar, sizeInPoint);
            VertexStore vxs = builder.GetVxs();

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);

            if (chkFillBackground.Checked)
            {
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3
                p.Fill(vxs);
            }
            if (chkBorder.Checked)
            {
                //5.4 
                p.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here...
                //p.StrokeWidth = 2;
                //5.5 
                p.Draw(vxs);
            }
            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            BitmapHelper.CopyToWindowsBitmap(destImg, winBmp, new RectInt(0, 0, 300, 300));
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(10, 0));
        }

        void RenderWithPlugableGlyphRasterizer(Typeface typeface, char testChar, float sizeInPoint, int resolution)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            ////credit:
            ////http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly  

            //2. glyph to gdi path
            var gdiGlyphRasterizer = new NOpenType.CLI.GDIGlyphRasterizer();
            var builder = new GlyphPathBuilder(typeface, gdiGlyphRasterizer);
            builder.Build(testChar, sizeInPoint);


            if (chkFillBackground.Checked)
            {
                gdiGlyphRasterizer.Fill(g, Brushes.Black);
            }
            if (chkBorder.Checked)
            {
                gdiGlyphRasterizer.Draw(g, Pens.Green);
            }
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            

        }
        void RenderWithTextPrinterAndMiniAgg(Typeface typeface, string str, float sizeInPoint, int resolution)
        {
            //1. 
            TextPrinter printer = new TextPrinter();
            printer.EnableKerning = this.chkKern.Checked;
            int len = str.Length;
            GlyphPlan[] glyphPlanList = new GlyphPlan[len];
            printer.Print(typeface, sizeInPoint, str, glyphPlanList);
            //--------------------------

            //5. use PixelFarm's Agg to render to bitmap...
            //5.1 clear background
            p.Clear(PixelFarm.Drawing.Color.White);

            if (chkFillBackground.Checked)
            {
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3 
                int glyphListLen = glyphPlanList.Length;

                float ox = p.OriginX;
                float oy = p.OriginY;
                float cx = 0;
                float cy = 10;
                for (int i = 0; i < glyphListLen; ++i)
                {
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    cx = glyphPlan.x;
                    p.SetOrigin(cx, cy);
                    p.Fill(glyphPlan.vxs);
                }
                p.SetOrigin(ox, oy);

            }
            if (chkBorder.Checked)
            {
                //5.4 
                p.StrokeColor = PixelFarm.Drawing.Color.Green;
                //user can specific border width here...
                //p.StrokeWidth = 2;
                //5.5 
                int glyphListLen = glyphPlanList.Length;
                for (int i = 0; i < glyphListLen; ++i)
                {
                    GlyphPlan glyphPlan = glyphPlanList[i];
                    p.Draw(glyphPlan.vxs);
                }
            }
            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
            BitmapHelper.CopyToWindowsBitmap(destImg, winBmp, new RectInt(0, 0, 300, 300));
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(10, 0));
            //--------------------------

        }
        private void txtInputChar_TextChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }
        void cmbRenderChoices_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        private void lstFontSizes_SelectedIndexChanged(object sender, EventArgs e)
        {
            //new font size
            fontSizeInPoint = (int)lstFontSizes.SelectedItem;
            button1_Click(this, EventArgs.Empty);
        }

        private void chkKern_CheckedChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }


    }
}
