//MIT, 2016-2017, WinterDev  

namespace PixelFarm.Drawing.Fonts
{
    public static class ScriptLangConv
    {
        public static Typography.OpenFont.ScriptLang GetOpenFontScriptLang(this RequestFont reqFont)
        {
            return Typography.OpenFont.ScriptLangs.GetRegisteredScriptLang(reqFont.ScriptLang.shortname);
        }
    }
}