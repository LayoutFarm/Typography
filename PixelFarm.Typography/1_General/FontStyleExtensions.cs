//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {
        public static Typography.FontManagement.TypefaceStyle ConvToInstalledFontStyle(this FontStyle style)
        {
            Typography.FontManagement.TypefaceStyle installedStyle = Typography.FontManagement.TypefaceStyle.Regular;//regular
            switch (style)
            {
                default: break;
                case FontStyle.Bold:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Bold;
                    break;
                case FontStyle.Italic:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Italic;
                    break;
                case FontStyle.Bold | FontStyle.Italic:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Italic;//??? WHY????
                    break;
                case FontStyle.Others:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Others;
                    break;
            }

            return installedStyle;
        }
    }

}