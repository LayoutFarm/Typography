using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NRasterizer;
using System.IO;
using PixelFarm.Agg;

namespace Sample2
{
    public partial class Form1 : Form
    {
        Graphics g;
        AggCanvasPainter p;
        ImageGraphics2D imgGfx2d;
        ActualImage destImg;
        Bitmap winBmp;
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
            ReadAndRender(@"..\..\segoeui.ttf");
        }

        void ReadAndRender(string fontfile)
        {
            var reader = new OpenTypeReader();
            string text = "C";
            int size = 72;
            //gfxPath = new GraphicsPath();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                Typeface typeFace = reader.Read(fs);
                var r = new Rasterizer2(typeFace);
                //fill into gfx path               
                r.Rasterize(text, size, 72, false);
                //render to screen
                VertexStore vxs = r.MakeVxs();
                p.Clear(PixelFarm.Drawing.Color.White);
                p.FillColor = PixelFarm.Drawing.Color.Black;
                p.Fill(vxs);
                BitmapHelper.CopyToWindowsBitmap(destImg, winBmp, new RectInt(0, 0, 300, 300));
                g.Clear(Color.White);
                g.DrawImage(winBmp, new Point(0, 0));
                //g.FillPath(Brushes.Black, gfxPath);
            }
        }
    }
}
