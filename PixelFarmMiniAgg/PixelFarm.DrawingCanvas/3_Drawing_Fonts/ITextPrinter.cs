////MIT, 2014-2017, WinterDev

namespace PixelFarm.Drawing
{
    public interface IFonts
    {
        float MeasureWhitespace(RequestFont f);
        Size MeasureString(char[] str, int startAt, int len, RequestFont font);
        Size MeasureString(char[] str, int startAt, int len, RequestFont font,
            float maxWidth, out int charFit,
            out int charFitWidth);
        void CalculateGlyphAdvancePos(char[] str, int startAt,
            int len, RequestFont font, int[] glyphXAdvances);
        void Dispose();
    }

}
namespace PixelFarm.Drawing.Fonts
{
    public interface ITextPrinter
    {

        void DrawString(string text, double x, double y);
        void DrawString(char[] text, int startAt, int len, double x, double y);
        void ChangeFont(RequestFont font);
        void ChangeFontColor(Color fontColor);
    }
}