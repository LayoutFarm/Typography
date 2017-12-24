//MIT, 2017, WinterDev
namespace Typography.TextServices
{
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
    }
}