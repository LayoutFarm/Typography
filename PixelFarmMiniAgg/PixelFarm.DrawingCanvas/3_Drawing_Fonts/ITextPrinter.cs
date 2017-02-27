////MIT, 2014-2017, WinterDev


namespace PixelFarm.Drawing.Fonts
{
    public interface ITextPrinter
    {
      
        void DrawString(string text, double x, double y);
        void DrawString(char[] text, double x, double y);
        void ChangeFont(RequestFont font);
        void ChangeFontColor(Color fontColor);
    }
}