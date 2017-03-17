//MIT, 2014-2017, WinterDev

using System.Drawing;
namespace SampleWinForms.UI
{

    class SampleTextBoxController
    {

        bool _isMouseDown;
        bool _isKeyDown;
        int _mousedown_X;
        int _mousedown_Y;

        Line _line;//controller
        VisualLine _visualLine;
        DevGdiTextPrinter _printer;
        Graphics g;

        public SampleTextBoxController()
        {
            _line = new Line();
            _visualLine = new VisualLine();
            _visualLine.BindLine(_line);
            _visualLine.Y = 100;
        }
        public void BindHostGraphics(Graphics hostControlGraphics)
        {
            g = hostControlGraphics;
        }

        public DevGdiTextPrinter TextPrinter
        {
            get { return _printer; }
            set
            {
                _printer = value;
                _visualLine.BindPrinter(value);
            }
        }
        public void UpdateOutput()
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


            _printer.TargetGraphics = g;
            _visualLine.Draw();
            //----------
            //transform back
            g.ScaleTransform(1.0F, -1.0F);// Flip the Y-Axis 
            g.TranslateTransform(0.0F, -(float)300);// Translate the drawing area accordingly                          

            //----------
#if DEBUG
            //draw latest mousedown (x,y)
            g.FillRectangle(Brushes.Green, _mousedown_X, _mousedown_Y, 5, 5);
#endif
        }

        public void HostInvokeDoubleClick()
        {

        }
        public void HostInvokeKeyDown(UIKeys keys)
        {
            _isKeyDown = true;
            switch (keys)
            {
                case UIKeys.Delete:
                    _line.DoDelete();
                    break;
                case UIKeys.Back:
                    _line.DoBackspace();
                    break;
                case UIKeys.Home:
                    _line.DoHome();
                    break;
                case UIKeys.End:
                    _line.DoEnd();
                    break;
                case UIKeys.Left:
                    _line.DoLeft();
                    break;
                case UIKeys.Right:
                    _line.DoRight();
                    break;
            }
            UpdateOutput();
        }
        public void HostInvokeKeyPress(char keychar)
        {
            //sample only
            if (char.IsControl(keychar))
            {
                return;
            }
            _line.AddChar(keychar);
            UpdateOutput();
        }
        public void HostInvokeKeyUp(UIKeys keys)
        {
            _isKeyDown = false;
        }
        public void HostInvokeMouseDown(int xpos, int ypos, UIMouseButtons button)
        {

            this._mousedown_X = xpos;
            this._mousedown_Y = ypos;
            _visualLine.SetCharIndexFromPos(xpos, ypos);
            _isMouseDown = true;
            UpdateOutput();

        }
        public void HostInvokeMouseUp(int xpos, int ypos, UIMouseButtons button)
        {
            _isMouseDown = false;
        }
        public void HostInvokeMouseMove(int xpos, int ypos, UIMouseButtons button)
        {
            if (_isMouseDown)
            {
                //dragging ...

            }
        }

    }



}