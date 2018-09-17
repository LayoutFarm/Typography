//MIT, 2014-present, WinterDev 

namespace PixelFarm.Drawing
{
    public interface ILineSegmentList
    {
        int Count { get; }
        ILineSegment this[int index] { get; }

    }
    public interface ILineSegment
    {

        int Length { get; }
        int StartAt { get; }
    }

    //implement this interface to handler font measurement/ glyph layout position
    //see current implementation in Gdi32IFonts and OpenFontIFonts
    public interface ITextService
    {

        float MeasureWhitespace(RequestFont f);
        float MeasureBlankLineHeight(RequestFont f);
        //
        bool SupportsWordBreak { get; }

        ILineSegmentList BreakToLineSegments(System.ReadOnlySpan<char> textBufferSpan);
        //
        Size MeasureString(System.ReadOnlySpan<char> textBufferSpan, RequestFont font);

        void MeasureString(System.ReadOnlySpan<char> textBufferSpan, RequestFont font, int maxWidth, out int charFit, out int charFitWidth);

        void CalculateUserCharGlyphAdvancePos(System.ReadOnlySpan<char> textBufferSpan, 
            RequestFont font,
            int[] outputXAdvances,
            out int outputTotalW,
            out int lineHeight);

        void CalculateUserCharGlyphAdvancePos(System.ReadOnlySpan<char> textBufferSpan, ILineSegmentList lineSegs,
            RequestFont font, int[] outputXAdvances, out int outputTotalW, out int lineHeight);
    }



    /// <summary>
    /// for printing a string to target canvas
    /// </summary>
    public interface ITextPrinter
    {
        bool StartDrawOnLeftTop { get; set; }
        void DrawString(System.ReadOnlySpan<char> charBuffer, double x, double y);
        /// <summary>
        /// render from RenderVxFormattedString object to specific pos
        /// </summary>
        /// <param name="renderVx"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void DrawString(RenderVxFormattedString renderVx, double x, double y);
        //-------------
        void PrepareStringForRenderVx(RenderVxFormattedString renderVx, char[] text, int startAt, int len);
        void ChangeFont(RequestFont font);
        //-------------
        void ChangeFillColor(Color fillColor);
        void ChangeStrokeColor(Color strokColor);
    }

    public static class ITextPrinterExtensions
    {
        public static void DrawString(this ITextPrinter textPrinter, string text, double x, double y)
        {
            char[] textBuffer = text.ToCharArray();
            textPrinter.DrawString(textBuffer, x, y);
        }
    }

}