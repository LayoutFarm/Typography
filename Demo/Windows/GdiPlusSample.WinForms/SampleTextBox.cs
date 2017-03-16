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
// 
namespace SampleWinForms
{
    public partial class SampleTextBox : UserControl
    {
        //-----------------
        //Sample code only.
        //so just 1 text run :)
        //-----------------

        TextRun _sampleTextRun;
        TextBoxEventManager _eventManager;
        Graphics _g;
        public SampleTextBox()
        {
            InitializeComponent();
            _eventManager = new TextBoxEventManager();
            _eventManager.Bind(this);
        }
        protected override void OnLoad(EventArgs e)
        {
            _g = this.CreateGraphics();
            base.OnLoad(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            _g.Clear(Color.White);
            ////we handle paint here
            ////-------------------------------------------------- 
            ////we test the text run, selection, 
            //this._sampleTextRun = textRun;
            ////-------------------------------------------------- 

            ////set presentation elements
            //textRun.SetGlyphPlan(userGlyphPlans, 0, userGlyphPlans.Count);
            ////-------------------------------------------------- 
            base.OnPaint(e);
        }


        class TextBoxEventManager
        {
            UserControl _hostControl;
            bool _isMouseDown;

            public void Bind(UserControl hostControl)
            {
                _hostControl = hostControl;
                _hostControl.KeyDown += _hostControl_KeyDown;
                _hostControl.KeyUp += _hostControl_KeyUp;
                _hostControl.KeyPress += _hostControl_KeyPress;
                //mouse
                _hostControl.MouseDown += _hostControl_MouseDown;
                _hostControl.MouseUp += _hostControl_MouseUp;
                _hostControl.MouseMove += _hostControl_MouseMove;
                _hostControl.DoubleClick += _hostControl_DoubleClick;
            }

            private void _hostControl_DoubleClick(object sender, System.EventArgs e)
            {

            }

            private void _hostControl_MouseMove(object sender, MouseEventArgs e)
            {
                if (_isMouseDown)
                {
                    //dragging ...

                }
            }

            private void _hostControl_MouseUp(object sender, MouseEventArgs e)
            {
                _isMouseDown = false;
            }

            private void _hostControl_MouseDown(object sender, MouseEventArgs e)
            {
                _isMouseDown = true;
                //find mouse down position
                
            }
            private void _hostControl_KeyPress(object sender, KeyPressEventArgs e)
            {
                //sample only


            }
            private void _hostControl_KeyUp(object sender, KeyEventArgs e)
            {

            }

            private void _hostControl_KeyDown(object sender, KeyEventArgs e)
            {

            }
        }



        class TextRun
        {
            char[] _srcTextBuffer;
            int _startAt;
            int _len;

            GlyphPlanListCache _glyphPlanListCache;

            public TextRun(char[] srcTextBuffer, int startAt, int len)
            {
                this._srcTextBuffer = srcTextBuffer;
                this._startAt = startAt;
                this._len = len;
            }
            public void SetGlyphPlan(List<GlyphPlan> glyphPlans, int startAt, int len)
            {
                _glyphPlanListCache = new GlyphPlanListCache(glyphPlans, startAt, len);
            }
            struct GlyphPlanListCache
            {
                public readonly List<GlyphPlan> glyphPlans;
                public readonly int startAt;
                public readonly int len;
                public GlyphPlanListCache(List<GlyphPlan> glyphPlans, int startAt, int len)
                {
                    this.glyphPlans = glyphPlans;
                    this.startAt = startAt;
                    this.len = len;
                }

            }
        }
    }
}
