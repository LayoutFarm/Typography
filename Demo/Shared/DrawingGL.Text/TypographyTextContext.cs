//MIT, 2017, Zou Wei(github/zwcloud)
using System;

namespace DrawingGL.Text
{
    /// <summary>
    /// Text context based on Typography
    /// </summary>
    class TypographyTextContext
    {

        string _fontFam;
        internal TypographyTextContext() { }
        public string FontFamily
        {
            //TODO impl font file resolution
            get { return _fontFam; }
            set
            {
                _fontFam = value;
            }
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
    }

}
