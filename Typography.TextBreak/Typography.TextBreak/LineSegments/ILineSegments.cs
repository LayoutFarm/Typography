//MIT, 2016-present, WinterDev 
using Typography.OpenFont;

namespace Typography.TextBreak
{
    public class SpanBreakInfo
    {
        internal SpanBreakInfo(UnicodeRangeInfo unicodeRange, bool isRightToLeft, uint scriptTag, uint langTag = 0)
        {
            UnicodeRange = unicodeRange; //can be null
            RightToLeft = isRightToLeft;
            ScriptTag = scriptTag;
            LangTag = 0;
        }
        internal SpanBreakInfo(bool isRightToLeft, uint scriptTag, uint langTag = 0)
        {
            UnicodeRange = null;
            RightToLeft = isRightToLeft;
            ScriptTag = scriptTag;
            LangTag = 0;
        }
        public UnicodeRangeInfo UnicodeRange { get; }
        public bool RightToLeft { get; }
        public uint ScriptTag { get; }
        public uint LangTag { get; }
#if DEBUG
        public override string ToString()
        {
            return Typography.OpenFont.TagUtils.TagToString(ScriptTag) + ":" + Typography.OpenFont.TagUtils.TagToString(LangTag);
        }
#endif
    }

}