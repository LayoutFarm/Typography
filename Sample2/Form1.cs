using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using NRasterizer;
using System.IO;

namespace Sample2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ReadAndRender(@"..\..\segoeui.ttf");
        }
        GraphicsPath gfxPath;
        void ReadAndRender(string fontfile)
        {
            var reader = new OpenTypeReader();
            string text = "B";
            int size = 48;
            gfxPath = new GraphicsPath();

            using (var fs = new FileStream(fontfile, FileMode.Open))
            {
                Typeface typeFace = reader.Read(fs);
                var r = new Rasterizer2(typeFace);
                //fill into gfx path               
                r.Rasterize(gfxPath, text, size, 72, false);
                //render to screen
                Graphics g = this.CreateGraphics();
                g.FillPath(Brushes.Black, gfxPath);
            }
        }
    }
}
