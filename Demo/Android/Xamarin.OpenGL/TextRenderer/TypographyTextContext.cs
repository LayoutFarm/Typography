//MIT, 2017, Zou Wei(github/zwcloud)
using System;
using Typography.Rendering;

namespace Xamarin.OpenGL
{
    /// <summary>
    /// Text context based on Typography
    /// </summary>
    class TypographyTextContext : ITextContext
    {
        private static readonly TextPrinter thePrinter = new TextPrinter();//a unique text-printer

        public TypographyTextContext(string text, string fontFamily, int fontSize,
            FontStretch stretch, FontStyle style, FontWeight weight,
            int maxWidth, int maxHeight,
            TextAlignment alignment)
        {
            thePrinter.FontFilename = fontFamily;//This is inaccurate and just a temp hack: Font-family hasn't been implemented by Typography.

            this.FontSize = fontSize;
            this.Alignment = alignment;
            this.MaxWidth = maxWidth;
            this.MaxHeight = maxHeight;
            this.Text = text;
        }

        #region Implementation of ITextContext

        #region TODO Implement those when Typography is ready.

        public int FontSize
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

        public Rect Rect
        {
            get;
            set;
        }

        #endregion

        public string Text
        {
            get;
            set;
        }

        public void Build(float offsetX, float offsetY, TextMesh textMesh)
        {
            thePrinter.FontSizeInPoints = this.FontSize;
            thePrinter.Draw(textMesh, this.Text, offsetX, offsetY);
        }

        public Size Measure()
        {
            thePrinter.FontSizeInPoints = this.FontSize;
            float width;
            float height;
            thePrinter.Measure(this.Text, 0, this.Text.Length, out width, out height);
            return new Size(width, height);
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

        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            // No native resource is used.
        }

        #endregion
    }

}
