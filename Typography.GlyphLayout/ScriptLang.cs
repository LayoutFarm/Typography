//MIT, 2016-present, WinterDev

namespace Typography.OpenFont
{
    /// <summary>
    /// script tag and syslang tag for GSUB, GPOS
    /// </summary>
    public sealed class ScriptLang
    {
        /// <summary>
        /// script tag
        /// </summary>
        public readonly string scriptTag;
        /// <summary>
        /// syslang tag
        /// </summary>
        public readonly string sysLangTag;

        public ScriptLang(string scriptTag, string sysLangTag = "")
        {
            this.scriptTag = scriptTag;
            this.sysLangTag = sysLangTag;
        }
        public override string ToString()
        {
            return this.scriptTag + ":" + scriptTag;

        }

    }
}
