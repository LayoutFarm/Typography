//MIT, 2014-2017, WinterDev

using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Typography.OpenFont;
using Typography.TextLayout;
using Typography.Rendering;

namespace SampleWinForms.UI
{
    public enum UIMouseButtons
    {
        Left,
        Right,
        Middle,
        None
    }
    public enum UIKeys
    {
        Modifiers = -65536,
        //
        None = 0,
        //
        LButton = 1,
        //
        RButton = 2,
        //
        Cancel = 3,
        //
        MButton = 4,
        //
        XButton1 = 5,
        //
        XButton2 = 6,
        //
        Back = 8,
        //
        Tab = 9,
        //
        LineFeed = 10,
        //
        Clear = 12,
        //
        Enter = 13,
        //
        Return = 13,
        //
        ShiftKey = 16,
        //
        ControlKey = 17,
        //
        Menu = 18,
        //
        Pause = 19,
        //
        CapsLock = 20,
        //
        Capital = 20,
        //
        KanaMode = 21,
        //
        HanguelMode = 21,
        //
        HangulMode = 21,
        //
        JunjaMode = 23,
        //
        FinalMode = 24,
        //
        KanjiMode = 25,
        //
        HanjaMode = 25,
        //
        Escape = 27,
        //
        IMEConvert = 28,
        //
        IMENonconvert = 29,
        //
        IMEAceept = 30,
        //
        IMEAccept = 30,
        //
        IMEModeChange = 31,
        //
        Space = 32,
        //
        Prior = 33,
        //
        PageUp = 33,
        //
        Next = 34,
        //
        PageDown = 34,
        //
        End = 35,
        //
        Home = 36,
        //
        Left = 37,
        //
        Up = 38,
        //
        Right = 39,
        //
        Down = 40,
        //
        Select = 41,
        //
        Print = 42,
        //
        Execute = 43,
        //
        PrintScreen = 44,
        //
        Snapshot = 44,
        //
        Insert = 45,
        //
        Delete = 46,
        //
        Help = 47,
        //
        D0 = 48,
        //
        D1 = 49,
        //
        D2 = 50,
        //
        D3 = 51,
        //
        D4 = 52,
        //
        D5 = 53,
        //
        D6 = 54,
        //
        D7 = 55,
        //
        D8 = 56,
        //
        D9 = 57,
        //
        A = 65,
        //
        B = 66,
        //
        C = 67,
        //
        D = 68,
        //
        E = 69,
        //
        F = 70,
        //
        G = 71,
        //
        H = 72,
        //
        I = 73,
        //
        J = 74,
        //
        K = 75,
        //
        L = 76,
        //
        M = 77,
        //
        N = 78,
        //
        O = 79,
        //
        P = 80,
        //
        Q = 81,
        //
        R = 82,
        //
        S = 83,
        //
        T = 84,
        //
        U = 85,
        //
        V = 86,
        //
        W = 87,
        //
        X = 88,
        //
        Y = 89,
        //
        Z = 90,
        //
        LWin = 91,
        //
        RWin = 92,
        //
        Apps = 93,
        //
        Sleep = 95,
        //
        NumPad0 = 96,
        //
        NumPad1 = 97,
        //
        NumPad2 = 98,
        //
        NumPad3 = 99,
        //
        NumPad4 = 100,
        //
        NumPad5 = 101,
        //
        NumPad6 = 102,
        //
        NumPad7 = 103,
        //
        NumPad8 = 104,
        //
        NumPad9 = 105,
        //
        Multiply = 106,
        //
        Add = 107,
        //
        Separator = 108,
        //
        Subtract = 109,
        //
        Decimal = 110,
        //
        Divide = 111,
        //
        F1 = 112,
        //
        F2 = 113,
        //
        F3 = 114,
        //
        F4 = 115,
        //
        F5 = 116,
        //
        F6 = 117,
        //
        F7 = 118,
        //
        F8 = 119,
        //
        F9 = 120,
        //
        F10 = 121,
        //
        F11 = 122,
        //
        F12 = 123,
        //
        F13 = 124,
        //
        F14 = 125,
        //
        F15 = 126,
        //
        F16 = 127,
        //
        F17 = 128,
        //
        F18 = 129,
        //
        F19 = 130,
        //
        F20 = 131,
        //
        F21 = 132,
        //
        F22 = 133,
        //
        F23 = 134,
        //
        F24 = 135,
        //
        NumLock = 144,
        //
        Scroll = 145,
        //
        LShiftKey = 160,
        //
        RShiftKey = 161,
        //
        LControlKey = 162,
        //
        RControlKey = 163,
        //
        LMenu = 164,
        //
        RMenu = 165,
        //
        BrowserBack = 166,
        //
        BrowserForward = 167,
        //
        BrowserRefresh = 168,
        //
        BrowserStop = 169,
        //
        BrowserSearch = 170,
        //
        BrowserFavorites = 171,
        //
        BrowserHome = 172,
        //
        VolumeMute = 173,
        //
        VolumeDown = 174,
        //
        VolumeUp = 175,
        //
        MediaNextTrack = 176,
        //
        MediaPreviousTrack = 177,
        //
        MediaStop = 178,
        //
        MediaPlayPause = 179,
        //
        LaunchMail = 180,
        //
        SelectMedia = 181,
        //
        LaunchApplication1 = 182,
        //
        LaunchApplication2 = 183,
        //
        Oem1 = 186,
        //
        OemSemicolon = 186,
        //
        Oemplus = 187,
        //
        Oemcomma = 188,
        //
        OemMinus = 189,
        //
        OemPeriod = 190,
        //
        OemQuestion = 191,
        //
        Oem2 = 191,
        //
        Oemtilde = 192,
        //
        Oem3 = 192,
        //
        Oem4 = 219,
        //
        OemOpenBrackets = 219,
        //
        OemPipe = 220,
        //
        Oem5 = 220,
        //
        Oem6 = 221,
        //
        OemCloseBrackets = 221,
        //
        Oem7 = 222,
        //
        OemQuotes = 222,
        //
        Oem8 = 223,
        //
        Oem102 = 226,
        //
        OemBackslash = 226,
        //
        ProcessKey = 229,
        //
        Packet = 231,
        //
        Attn = 246,
        //
        Crsel = 247,
        //
        Exsel = 248,
        //
        EraseEof = 249,
        //
        Play = 250,
        //
        Zoom = 251,
        //
        NoName = 252,
        //
        Pa1 = 253,
        //
        OemClear = 254,
        //
        KeyCode = 65535,
        //
        Shift = 65536,
        //
        Control = 131072,
        //
        Alt = 262144,
    }
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

            _visualLine.Draw(g);
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



    class Line
    {
        //left most =0

        int _caretCharIndex = 0;//default
        internal List<char> _charBuffer = new List<char>();
        bool _contentChanged = true;

        /// <summary>
        /// add char at current pos
        /// </summary>
        /// <param name="c"></param>
        public void AddChar(char c)
        {
            //add char at cursor index
            int count = _charBuffer.Count;

            if (_caretCharIndex == count)
            {
                //at the end                
                _charBuffer.Add(c);
                _caretCharIndex++;
            }
            else if (_caretCharIndex < count)
            {
                _charBuffer.Insert(_caretCharIndex, c);
                _caretCharIndex++;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            _contentChanged = true;
        }
        public void DoBackspace()
        {
            int count = _charBuffer.Count;
            if (count == 0)
            {
                _caretCharIndex = 0;
                return;
            }

            //---------------------------------
            if (_caretCharIndex == count)
            {
                //end
                _charBuffer.RemoveAt(count - 1);
                _caretCharIndex--;
            }
            else if (_caretCharIndex < count)
            {
                _charBuffer.RemoveAt(_caretCharIndex);
                _caretCharIndex--;
            }
            else
            {
                throw new System.NotSupportedException();
            }
            _contentChanged = true;
        }
        public void DoDelete()
        {
            //simulate by do right + backspace
            int count = _charBuffer.Count;
            if (_caretCharIndex == count)
            {
                //caret is on the end
                //just return
                return;
            }
            DoRight();
            DoBackspace();
        }
        public void DoLeft()
        {
            int count = _charBuffer.Count;
            if (count == 0)
            {
                _caretCharIndex = 0;
                return;
            }
            else if (_caretCharIndex > 0)
            {
                //this is on the end
                _caretCharIndex--;
            }
            else
            {

            }

        }
        public void DoRight()
        {
            int count = _charBuffer.Count;
            if (count == 0)
            {
                return;
            }
            else if (_caretCharIndex < count)
            {
                //this is on the end
                _caretCharIndex++;
            }
            else
            {

            }
        }
        public void DoHome()
        {
            _caretCharIndex = 0;
        }
        public void DoEnd()
        {
            _caretCharIndex = _charBuffer.Count;
        }
        public int CharCount
        {
            get { return 0; }
        }
        public bool ContentChanged { get { return _contentChanged; } }
        public int CaretCharIndex { get { return _caretCharIndex; } }
        public void SetCaretCharIndex(int newindex)
        {
            if (newindex >= 0 && newindex <= _charBuffer.Count)
            {
                _caretCharIndex = newindex;
            }
        }
    }


    class VisualLine
    {

        Line _line;
        DevGdiTextPrinter _printer;
        List<GlyphPlan> _glyphPlans = new List<GlyphPlan>();
        float toPxScale = 1;
        public void BindLine(Line line)
        {
            this._line = line;
        }
        public void BindPrinter(DevGdiTextPrinter printer)
        {
            _printer = printer;
        }
        public float X { get; set; }
        public float Y { get; set; }

        public void SetCharIndexFromPos(float x, float y)
        {
            int j = _glyphPlans.Count;
            float accum_x = 0;
            for (int i = 0; i < j; ++i)
            {
                float thisGlyphW = _glyphPlans[i].advX * toPxScale;
                accum_x += thisGlyphW;
                if (accum_x > x)
                {
                    //TODO: review here 
                    //for some glyph that has been substitued 
                    //glyph may not match with actual user char in the _line    

                    float xoffset_on_glyph = (x - (accum_x - thisGlyphW));
                    if (xoffset_on_glyph >= (thisGlyphW / 3))
                    {
                        _line.SetCaretCharIndex(i + 1);
                    }
                    else
                    {
                        _line.SetCaretCharIndex(i);
                    }
                    //stop
                    break;
                }

            }

        }
        public void Draw(Graphics g)
        {
            if (_line.ContentChanged)
            {
                char[] textBuffer = _line._charBuffer.ToArray();
                _glyphPlans.Clear();
                _printer.PrintGlyphPlans(_glyphPlans, textBuffer, 0, textBuffer.Length);
                toPxScale = _printer.CurrentTypeface.CalculateFromPointToPixelScale(_printer.FontSizeInPoints);
            }

            if (_glyphPlans.Count > 0)
            {
                _printer.DrawString(g, _glyphPlans, X, Y);
                //draw caret 
                //not blink in this version
                int caret_index = _line.CaretCharIndex;
                //find caret pos based on glyph plan
                //TODO: check when do gsub (glyph number may not match with user char number)                 
                float xpos = X;
                if (caret_index == 0)
                {
                    g.DrawLine(Pens.Red, xpos, this.Y, xpos, this.Y + _printer.FontAscendingPx);
                }
                else
                {
                    GlyphPlan p = _glyphPlans[caret_index - 1];
                    xpos += ((p.x + p.advX) * toPxScale);
                    g.DrawLine(Pens.Red, xpos, this.Y, xpos, this.Y + _printer.FontAscendingPx);
                }
            }
            else
            {
                float xpos = X;
                g.DrawLine(Pens.Red, xpos, this.Y, xpos, this.Y + _printer.FontAscendingPx);
            }


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