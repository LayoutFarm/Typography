//MIT, 2016,  WinterDev
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using NRasterizer;
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
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithGdiPlusPath);
            cmbRenderChoices.Items.Add(RenderChoice.RenderWithPlugableGlyphRasterizer);
            cmbRenderChoices.SelectedIndex = 0;

            cmbRenderChoices.SelectedIndexChanged += new EventHandler(cmbRenderChoices_SelectedIndexChanged);
        }


        enum RenderChoice
        {
            RenderWithMiniAgg,
            RenderWithGdiPlusPath,
            RenderWithPlugableGlyphRasterizer, //new 
            RenderWithTypePlanAndMiniAgg, //new
        }

        void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Render with PixelFarm";
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (g == null)
            {
                destImg = new ActualImage(300, 300, PixelFarm.Agg.Image.PixelFormat.ARGB32);
                imgGfx2d = new ImageGraphics2D(destImg, null); //no platform
                p = new AggCanvasPainter(imgGfx2d);
                winBmp = new Bitmap(300, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                g = this.CreateGraphics();
            }
            //  ReadAndRender(@"..\..\segoeui.ttf");
            ReadAndRender(@"..\..\tahoma.ttf");
        }
        void ReadAndRender(string fontfile)
        {
            if (string.IsNullOrEmpty(this.txtInputChar.Text))
            {
                return;
            }
            var reader = new OpenTypeReader();
            char testChar = txtInputChar.Text[0];//only 1 char

            int size = 72;
            int resolution = 72;

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeFace = reader.Read(fs);

                RenderChoice renderChoice = (RenderChoice)this.cmbRenderChoices.SelectedItem;
                switch (renderChoice)
                {
                    case RenderChoice.RenderWithMiniAgg:
                        RenderWithMiniAgg(typeFace, testChar, size, resolution);
                        break;
                    case RenderChoice.RenderWithGdiPlusPath:
                        RenderWithGdiPlusPath(typeFace, testChar, size, resolution);
                        break;
                    case RenderChoice.RenderWithPlugableGlyphRasterizer:
                        RenderWithPlugableGlyphRasterizer(typeFace, testChar, size, resolution);
                        break;
                    case RenderChoice.RenderWithTypePlanAndMiniAgg:
                        RenderWithTextPrinter(typeFace, this.txtInputChar.Text, size, resolution);
                        break;
                    default:
                        throw new NotSupportedException();

                }
            }
        }

        void RenderWithMiniAgg(Typeface typeface, char testChar, int size, int resolution)
        {
            //2. glyph-to-vxs builder
            var builder = new GlyphPathBuilderVxs(typeface);
            builder.Build(testChar, size, resolution);
            VertexStore vxs1 = builder.GetVxs();
            //----------------
            //3. do mini translate, scale
            var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                //translate
                 new PixelFarm.Agg.Transform.AffinePlan(
                     PixelFarm.Agg.Transform.AffineMatrixCommand.Translate, 1, 1),
                //scale
                 new PixelFarm.Agg.Transform.AffinePlan(
                     PixelFarm.Agg.Transform.AffineMatrixCommand.Scale, 1, 1)
                     );

            vxs1 = mat.TransformToVxs(vxs1);
            //----------------
            //4. flatten all curves 
            VertexStore vxs = curveFlattener.MakeVxs(vxs1);
            //---------------- 
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
        void RenderWithGdiPlusPath(Typeface typeface, char testChar, int size, int resolution)
        {
            //2. glyph to gdi path
            var builder = new GlyphPathBuilderGdiPlus(typeface);
            builder.Build(testChar, size, resolution);
            System.Drawing.Drawing2D.GraphicsPath gfxPath = builder.GetGraphicsPath();
            //--------------- 
            //3. just render to background-graphics
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);

            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            

            if (chkFillBackground.Checked)
            {
                g.FillPath(Brushes.Black, gfxPath);
            }
            if (chkBorder.Checked)
            {
                g.DrawPath(Pens.Green, gfxPath);
            }

            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly            

        }
        void RenderWithPlugableGlyphRasterizer(Typeface typeface, char testChar, int size, int resolution)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            ////credit:
            ////http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly  

            //2. glyph to gdi path
            var gdiGlyphRasterizer = new NRasterizer.CLI.GDIGlyphRasterizer();
            var builder = new GlyphPathBuilder(typeface, gdiGlyphRasterizer);
            builder.Build(testChar, size, resolution);


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
        void RenderWithTextPrinter(Typeface typeface, string str, int size, int resolution)
        {
            TextPrinter printer = new TextPrinter();
            printer.Print(typeface, str);
        }
        private void txtInputChar_TextChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }

        void cmbRenderChoices_SelectedIndexChanged(object sender, EventArgs e)
        {
            button1_Click(this, EventArgs.Empty);
        }



    }
}
