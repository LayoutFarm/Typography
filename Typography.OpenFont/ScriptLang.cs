//MIT, 2016-present, WinterDev
using System;
namespace Typography.OpenFont
{
     
    /// <summary>
    /// script tag and lang_feature tag request for GSUB, GPOS
    /// </summary>
    public struct ScriptLang
    {
        /// <summary>
        /// script tag
        /// </summary>
        public readonly uint scriptTag;
        /// <summary>
        /// syslang tag
        /// </summary>
        public readonly uint sysLangTag;

        public ScriptLang(uint scriptTag, uint sysLangTag = 0)
        {
            this.scriptTag = scriptTag;
            this.sysLangTag = sysLangTag;
        }
        public ScriptLang(string scriptTag, string sysLangTag = null)
        {
            this.sysLangTag = (sysLangTag == null) ? 0 : StringToTag(sysLangTag);
            this.scriptTag = StringToTag(scriptTag);
        }
#if DEBUG
        public override string ToString()
        {
            return TagToString(this.scriptTag) + ":" + TagToString(sysLangTag);
        }
#endif
        public bool IsEmpty() => scriptTag == 0 && sysLangTag == 0;
        static byte GetByte(char c)
        {
            if (c >= 0 && c < 256)
            {
                return (byte)c;
            }
            return 0;
        }
        static uint StringToTag(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length != 4)
            {
                return 0;
            }

            char[] buff = str.ToCharArray();
            byte b0 = GetByte(buff[0]);
            byte b1 = GetByte(buff[1]);
            byte b2 = GetByte(buff[2]);
            byte b3 = GetByte(buff[3]);

            return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
        }
        static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        public string GetScriptTagString() => TagToString(this.scriptTag);
        public string GetLangTagString() => TagToString(this.sysLangTag);

    }
}
