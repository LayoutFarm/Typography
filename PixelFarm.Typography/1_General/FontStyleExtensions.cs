//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {
        public static Typography.FontManagement.TypefaceStyle ConvToInstalledFontStyle(this OldFontStyle style)
        {
            Typography.FontManagement.TypefaceStyle installedStyle = Typography.FontManagement.TypefaceStyle.Regular;//regular
            switch (style)
            {
                default: break;
                case OldFontStyle.Bold:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Bold;
                    break;
                case OldFontStyle.Italic:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Italic;
                    break;
                case OldFontStyle.Bold | OldFontStyle.Italic:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Italic;//??? WHY????
                    break;
                case OldFontStyle.Others:
                    installedStyle = Typography.FontManagement.TypefaceStyle.Others;
                    break;
            }

            return installedStyle;
        }
    }

}