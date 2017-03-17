//MIT, 2016-2017, WinterDev
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
//
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;
using System;

namespace SampleWinForms
{
    using SampleWinForms.UI;
    partial class SampleTextBox : UserControl
    {
        //-----------------
        //Sample code only.       
        //-----------------

         
        SampleTextBoxController _txtBoxController;
        Graphics _hostControlGraphics;
        public SampleTextBox()
        {
            InitializeComponent();
            _txtBoxController = new SampleTextBoxController();
            _txtBoxController.BindHostGraphics(_hostControlGraphics = this.CreateGraphics());
        }
        public void SetTextPrinter(DevGdiTextPrinter printer)
        {
            _txtBoxController.TextPrinter = printer;
        } 
        protected override void OnKeyDown(KeyEventArgs e)
        {

            _txtBoxController.HostInvokeKeyDown((UIKeys)e.KeyCode);
            base.OnKeyDown(e);
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            _txtBoxController.HostInvokeKeyUp((UIKeys)e.KeyCode);
            base.OnKeyUp(e);
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            _txtBoxController.HostInvokeKeyPress(e.KeyChar);
            base.OnKeyPress(e);
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            _txtBoxController.HostInvokeMouseDown(e.X, e.Y, (UIMouseButtons)e.Button);
            base.OnMouseDown(e);
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            _txtBoxController.HostInvokeMouseMove(e.X, e.Y, (UIMouseButtons)e.Button);
            base.OnMouseMove(e);
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            _txtBoxController.HostInvokeMouseUp(e.X, e.Y, (UIMouseButtons)e.Button);
            base.OnMouseUp(e);
        }
        protected override void OnDoubleClick(EventArgs e)
        {
            _txtBoxController.HostInvokeDoubleClick();
            base.OnDoubleClick(e);
        }
         
        protected override bool ProcessDialogKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    _txtBoxController.HostInvokeKeyDown((UIKeys)keyData);
                    return true;
            }
            return base.ProcessDialogKey(keyData);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!this.DesignMode)
            {
                _txtBoxController.UpdateOutput();
            }
            base.OnPaint(e);
        }



    }
}
