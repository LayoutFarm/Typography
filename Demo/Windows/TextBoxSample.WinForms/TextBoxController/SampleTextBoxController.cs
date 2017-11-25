//MIT, 2014-2017, WinterDev

using System.Drawing;
using Typography.TextLayout;
namespace SampleWinForms.UI
{

    class SampleTextBoxControllerForGdi : SampleTextBoxController
    {
        Graphics g;
        DevGdiTextPrinter _printer;
        VisualLine _visualLine;
        public SampleTextBoxControllerForGdi()
        {
            _visualLine = new VisualLine();
            _visualLine.BindLine(_line);
            _visualLine.Y = 100;
        }
        public void BindHostGraphics(Graphics hostControlGraphics)
        {
            g = hostControlGraphics;
        }
        public override void HostInvokeMouseDown(int xpos, int ypos, UIMouseButtons button)
        {
            _visualLine.SetCharIndexFromPos(xpos, ypos);
            base.HostInvokeMouseDown(xpos, ypos, button);
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
        public DevGdiTextPrinter TextPrinter
        {
            get { return _printer; }
            set
            {
                _printer = value;
                _visualLine.BindPrinter(value);
            }
        }
    }


    abstract class SampleTextBoxController
    {

        bool _isMouseDown;
        bool _isKeyDown;
        protected int _mousedown_X;
        protected int _mousedown_Y;

        protected SmallLine _line;//controller 
        public SampleTextBoxController()
        {
            _line = new SmallLine();
        }


        public abstract void UpdateOutput();
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
        public virtual void HostInvokeMouseDown(int xpos, int ypos, UIMouseButtons button)
        {

            this._mousedown_X = xpos;
            this._mousedown_Y = ypos;

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