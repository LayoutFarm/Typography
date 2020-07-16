//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {

        public static Typography.FontManagement.TypefaceStyle ConvToInstalledFontStyle(this NewCssFontStyle style)
        {
            switch (style)
            {
                default:
                case NewCssFontStyle.Regular: return Typography.FontManagement.TypefaceStyle.Regular;

                case NewCssFontStyle.Italic: return Typography.FontManagement.TypefaceStyle.Italic;
                case NewCssFontStyle.Oblique: return Typography.FontManagement.TypefaceStyle.Italic;
            }

        }
    }

}