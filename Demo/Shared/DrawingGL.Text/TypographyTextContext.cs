//MIT, 2017, Zou Wei(github/zwcloud)
using System;

namespace DrawingGL.Text
{
    /// <summary>
    /// Text context based on Typography
    /// </summary>
    class TypographyTextContext
    {
        TextPrinter _textPrinter = new TextPrinter();
        string _fontFam;
        internal TypographyTextContext(TextPrinter textPrinter)
        {
            _textPrinter = textPrinter;
        }
        internal TypographyTextContext() : this(new TextPrinter()) { }

        public string FontFamily
        {
            //TODO impl font file resolution
            get { return _fontFam; }
            set
            {
                if (_fontFam != value)
                {
                    //this is not co
                    //TODO: implement font fam resolution
                    _textPrinter.FontFilename = value;
                }
                _fontFam = value;
            }
        }
        public void ReplaceTextPrinter(TextPrinter textPrinter)
        {
            //replace default text printer
            this._textPrinter = textPrinter;
        }
        public Typography.OpenFont.Typeface Typeface
        {
            get { return _textPrinter.CurrentTypeFace; }
        }


        public float FontSize
        {
            get;
            set;
        }

        public FontStretch FontStretch
        {
            get;
            set;
        }

        public FontStyle FontStyle
        {
            get;
            set;
        }

        public FontWeight FontWeight { get; set; }


        public TextAlignment Alignment
        {
            get;
            set;
        }

        public int MaxWidth
        {
            get;
            set;
        }

        public int MaxHeight
        {
            get;
            set;
        }



        public void Update()
        {
            //update all props
        }
        public void GenerateGlyphRuns(TextRun textRun, string str)
        {

            char[] buffer = str.ToCharArray();
            GenerateGlyphRuns(textRun, buffer, 0, buffer.Length);

        }
        public void GenerateGlyphRuns(TextRun textRun, char[] charBuffer, int startAt, int len)
        {
            _textPrinter.FontSizeInPoints = this.FontSize;
            _textPrinter.GenerateGlyphRuns(textRun, charBuffer, startAt, len);
            //System.Collections.Generic.List<GlyphRun> glyphs = textRun._glyphs; 

        }
        public Size Measure(char[] charBuffer, int startAt, int len)
        {
            _textPrinter.FontSizeInPoints = this.FontSize; 
            Typography.TextLayout.MeasuredStringBox mesureStringBox = _textPrinter.Measure(charBuffer, startAt, len);
            return new Size(mesureStringBox.width, mesureStringBox.btbd);
        }

        //TODO
        public uint XyToIndex(float pointX, float pointY, out bool isInside)
        {
            throw new NotImplementedException();
        }

        //TODO
        public void IndexToXY(uint textPosition, bool isTrailingHit, out float pointX, out float pointY, out float height)
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            // No native resource is used.
        }

    }

}
