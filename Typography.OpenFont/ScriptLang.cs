//MIT, 2016-present, WinterDev
using System;
namespace Typography.OpenFont
{

    //NOTE: readmore about language tag, https://tools.ietf.org/html/bcp47
    //...The language of an information item or a user's language preferences
    //often need to be identified so that appropriate processing can be
    //applied. 
    //...
    //...
    //One means of indicating the language used is by labeling the
    //information content with an identifier or "tag".  These tags can also
    //be used to specify the user's preferences when selecting information
    //content or to label additional attributes of content and associated
    //resources. 

    //..
    //..The Language Tag
    //..

    //Language tags are used to help identify languages, whether spoken,
    //written, signed, or otherwise signaled, for the purpose of
    //communication.This includes constructed and artificial languages
    //but excludes languages not intended primarily for human
    //communication, such as programming languages.


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
