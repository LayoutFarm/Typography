//MIT, 2014-2017, WinterDev

using System.Drawing;
using PixelFarm.Agg;
using PixelFarm.Drawing.Fonts;
namespace SampleWinForms.UI
{

    class SampleTextBoxControllerForPixelFarm : SampleTextBoxController
    {
        Graphics g;
        DevVxsTextPrinter _printer;
        ActualImage destImg;
        ImageGraphics2D imgGfx2d;
        AggCanvasPainter p;
        Bitmap winBmp;
        //
        public SampleTextBoxControllerForPixelFarm()
        {
        }
        public void BindHostGraphics(Graphics hostControlGraphics)
        {
            g = hostControlGraphics;
            //
            destImg = new ActualImage(400, 300, PixelFormat.ARGB32);
            imgGfx2d = new ImageGraphics2D(destImg); //no platform
            p = new AggCanvasPainter(imgGfx2d);
            winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);


        }
        public override void UpdateOutput()
        {
            //TODO: review here again 
            //----------
            //set some Gdi+ props... 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            g.Clear(Color.White);
            //credit:
            //http://stackoverflow.com/questions/1485745/flip-coordinates-when-drawing-to-control
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly   

            p.FillColor = PixelFarm.Drawing.Color.Black;
            p.Clear(PixelFarm.Drawing.Color.White);

            _printer.TargetCanvasPainter = p;
            _visualLine.Draw();


            //----------
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly                          

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap

            PixelFarm.Agg.Imaging.BitmapHelper.CopyToGdiPlusBitmapSameSize(p.Graphics.DestActualImage, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(0, 20));
#if DEBUG
            //draw latest mousedown (x,y)
            g.FillRectangle(Brushes.Green, _mousedown_X, _mousedown_Y, 5, 5);
#endif
        }
        public DevVxsTextPrinter TextPrinter
        {
            get { return _printer; }
            set
            {
                _printer = value;
                _visualLine.BindPrinter(value);
            }
        }
    }
}