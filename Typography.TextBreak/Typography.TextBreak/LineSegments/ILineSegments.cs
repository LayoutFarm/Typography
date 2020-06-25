//MIT, 2016-present, WinterDev 
namespace Typography.TextBreak
{ 
    public class SpanBreakInfo
    {
        
        internal SpanBreakInfo(bool isRightToLeft, uint scriptTag, uint langTag = 0)
        {
            RightToLeft = isRightToLeft;
            ScriptTag = scriptTag;
            LangTag = 0;
        }
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