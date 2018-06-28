//MIT, 2017-present, WinterDev
namespace Typography.TextServices
{
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
    }
}