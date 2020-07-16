//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {

        public static Typography.FontManagement.TypefaceStyle ConvToInstalledFontStyle(this RequestFontStyle style)
        {
            switch (style)
            {
                default:
                case RequestFontStyle.Regular: return Typography.FontManagement.TypefaceStyle.Regular;

                case RequestFontStyle.Italic: return Typography.FontManagement.TypefaceStyle.Italic;
                case RequestFontStyle.Oblique: return Typography.FontManagement.TypefaceStyle.Italic;
            }

        }
    }

}