//MIT, 2014-present, WinterDev

using System.Drawing;
using PixelFarm.CpuBlit;
namespace SampleWinForms.UI
{

    class SampleTextBoxControllerForPixelFarm : SampleTextBoxController
    {
        Graphics g;
        PixelFarm.Drawing.VxsTextPrinter _printer;
        MemBitmap destImg;

        AggPainter p;
        Bitmap winBmp;
        VisualLine _visualLine;
        PixelFarm.Drawing.OpenFontTextService _openFontTextServices;
        public SampleTextBoxControllerForPixelFarm()
        {
        }
        public bool ReadyToRender { get; set; }
        public void BindHostGraphics(Graphics hostControlGraphics)
        {
            g = hostControlGraphics;
            //
            destImg = new MemBitmap(400, 300);
            p = AggPainter.Create(destImg);
            winBmp = new Bitmap(400, 300, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            _openFontTextServices = new PixelFarm.Drawing.OpenFontTextService();

            _printer = new PixelFarm.Drawing.VxsTextPrinter(p, _openFontTextServices);
            _visualLine = new VisualLine();
            _visualLine.BindLine(_line);
            _visualLine.Y = 100;
        }
        public override void HostInvokeMouseDown(int xpos, int ypos, UIMouseButtons button)
        {
            _visualLine.SetCharIndexFromPos(xpos, ypos);
            base.HostInvokeMouseDown(xpos, ypos, button);
        }
        public override void UpdateOutput()
        {
            if (!ReadyToRender) return;
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

            // _printer.TargetCanvasPainter = p;
            _visualLine.Draw();


            //----------
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly                          

            //6. use this util to copy image from Agg actual image to System.Drawing.Bitmap

            PixelFarm.CpuBlit.BitmapHelper.CopyToGdiPlusBitmapSameSize(p.RenderSurface.DestBitmap, winBmp);
            //--------------- 
            //7. just render our bitmap
            g.Clear(Color.White);
            g.DrawImage(winBmp, new Point(0, 20));
#if DEBUG
            //draw latest mousedown (x,y)
            g.FillRectangle(Brushes.Green, _mousedown_X, _mousedown_Y, 5, 5);
#endif
        }
        public PixelFarm.Drawing.VxsTextPrinter TextPrinter
        {
            get => _printer;
            set
            {
                _printer = value;
                _visualLine.BindPrinter(value);
            }
        }
    }
}