//MIT, 2016,  WinterDev
using System;
using System.Drawing;
using System.Windows.Forms;
using NRasterizer;
using System.IO;
using PixelFarm.Agg;
using PixelFarm.Agg.VertexSource;

namespace Sample2
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
            var reader = new OpenTypeReader();
            char testChar = 'A';
            int size = 72;
            int resolution = 72;

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                //1. read typeface from font file
                Typeface typeFace = reader.Read(fs);
                //2. glyph-to-vxs builder
                var builder = new GlyphVxsBuilder(typeFace);
                VertexStore vxs1 = builder.CreateVxs(testChar, size, resolution);
                //----------------
                //3. do mini translate
                var mat = PixelFarm.Agg.Transform.Affine.NewMatix(
                     new PixelFarm.Agg.Transform.AffinePlan(
                         PixelFarm.Agg.Transform.AffineMatrixCommand.Translate, 10, 10));
                vxs1 = mat.TransformToVxs(vxs1);
                //----------------
                //4. flatten all curves 
                VertexStore vxs = curveFlattener.MakeVxs(vxs1);
                //---------------- 
                //5. use PixelFarm's Agg to render to bitmap...
                //5.1 clear background
                p.Clear(PixelFarm.Drawing.Color.White);
                //5.2 
                p.FillColor = PixelFarm.Drawing.Color.Black;
                //5.3
                p.Fill(vxs);
                //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap
                BitmapHelper.CopyToWindowsBitmap(destImg, winBmp, new RectInt(0, 0, 300, 300));
                //--------------- 
                //7. just render our bitmap
                g.Clear(Color.White);
                g.DrawImage(winBmp, new Point(10, 0));
            }
        }
    }
}
