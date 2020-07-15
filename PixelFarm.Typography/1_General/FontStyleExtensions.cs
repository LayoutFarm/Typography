//MIT, 2016-present, WinterDev 


namespace PixelFarm.Drawing
{
    static class FontStyleExtensions
    {
        public static Typography.FontManagement.TypefaceStyle ConvToInstalledFontStyle(this CssFontStyle style)
        {
            switch (style)
            {
                default: return Typography.FontManagement.TypefaceStyle.Others;
                case CssFontStyle.Regular: return Typography.FontManagement.TypefaceStyle.Regular;
                case CssFontStyle.Italic: return Typography.FontManagement.TypefaceStyle.Italic;
            }
        }
    }

}