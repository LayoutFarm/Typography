//MIT, 2014-present, WinterDev 

namespace PixelFarm.Drawing
{
    public interface ILineSegmentList : System.IDisposable
    {
        int Count { get; }
        ILineSegment this[int index] { get; }

    }
    public interface ILineSegment
    {

        int Length { get; }
        int StartAt { get; }
    }

    public struct TextBufferSpan
    {
        public readonly int start;
        public readonly int len;

        char[] _rawString;
        public TextBufferSpan(char[] rawCharBuffer)
        {
            _rawString = rawCharBuffer;
            this.len = rawCharBuffer.Length;
            this.start = 0;
        }
        public TextBufferSpan(char[] rawCharBuffer, int start, int len)
        {
            this.start = start;
            this.len = len;
            _rawString = rawCharBuffer;
        }
        public override string ToString()
        {
            return start + ":" + len;
        }


        public char[] GetRawCharBuffer() { return _rawString; }
    }

    //implement this interface to handler font measurement/ glyph layout position
    //see current implementation in Gdi32IFonts and OpenFontIFonts
    public interface ITextService
    {

        float MeasureWhitespace(RequestFont f);
        float MeasureBlankLineHeight(RequestFont f);
        //
        bool SupportsWordBreak { get; }

        ILineSegmentList BreakToLineSegments(ref TextBufferSpan textBufferSpan);
        //
        Size MeasureString(ref TextBufferSpan textBufferSpan, RequestFont font);

        void MeasureString(ref TextBufferSpan textBufferSpan, RequestFont font, int maxWidth, out int charFit, out int charFitWidth);

        void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan,
            RequestFont font,
            int[] outputXAdvances,
            out int outputTotalW,
            out int lineHeight);

        void CalculateUserCharGlyphAdvancePos(ref TextBufferSpan textBufferSpan, ILineSegmentList lineSegs,
            RequestFont font, int[] outputXAdvances, out int outputTotalW, out int lineHeight);
    }



    /// <summary>
    /// for printing a string to target canvas
    /// </summary>
    public interface ITextPrinter
    {
        bool StartDrawOnLeftTop { get; set; }
        void DrawString(char[] text, int startAt, int len, double left, double top);
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="left"></param>
        /// <param name="top"></param>
        void DrawString(RenderVxFormattedString renderVx, double left, double top);
        //-------------
        void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len);
        void ChangeFont(RequestFont font);
        //-------------
        void ChangeFillColor(Color fillColor);
        void ChangeStrokeColor(Color strokColor);
        //-------------
        void MeasureString(char[] buffer, int startAt, int len, out int w, out int h);
    }

    public static class ITextPrinterExtensions
    {
        public static void DrawString(this ITextPrinter textPrinter, string text, double left, double top)
        {
#if DEBUG
            if (text == null)
            {
                return;
            }
#endif
            char[] textBuffer = text.ToCharArray();
            textPrinter.DrawString(textBuffer, 0, textBuffer.Length, left, top);
        }
    }

}