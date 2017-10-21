//MIT, 2017, WinterDev
namespace PixelFarm.Drawing.Fonts
{
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
    }
}