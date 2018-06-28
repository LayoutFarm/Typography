//MIT, 2016-2017, WinterDev 

using Typography.TextServices;

namespace PixelFarm.Drawing.Fonts
{
    public static class FontStyleExtensions
    {
        public static InstalledFontStyle ConvToInstalledFontStyle(this FontStyle style)
        {
            InstalledFontStyle installedStyle = InstalledFontStyle.Normal;//regular
            switch (style)
            {
                default: break;
                case FontStyle.Bold:
                    installedStyle = InstalledFontStyle.Bold;
                    break;
                case FontStyle.Italic:
                    installedStyle = InstalledFontStyle.Italic;
                    break;
                case FontStyle.Bold | FontStyle.Italic:
                    installedStyle = InstalledFontStyle.Italic;
                    break;
            }
            return installedStyle;
        }
    }

}