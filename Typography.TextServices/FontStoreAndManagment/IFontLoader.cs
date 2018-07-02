//MIT, 2017-present, WinterDev
namespace Typography.FontManagement
{
    public interface IFontLoader
    {
        InstalledFont GetFont(string fontName, InstalledFontStyle style);
    }
}