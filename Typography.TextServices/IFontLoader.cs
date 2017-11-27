//MIT, 2017, WinterDev
namespace Typography.TextService
{
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
    }
}