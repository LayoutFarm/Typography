//MIT, 2016-present, WinterDev 
namespace Typography.TextBreak
{
    public interface ILineSegmentList : System.IDisposable
    {
        int Count { get; }
        ILineSegment this[int index] { get; }
    }
    public interface ILineSegment
    {
        int Length { get; }
        int StartAt { get; }
        SpanBreakInfo SpanBreakInfo { get; }
    }

    public struct BreakSpan
    {
        //TODO: review here again***
        public int startAt;
        public ushort len;
        public WordKind wordKind;
        public SpanBreakInfo SpanBreakInfo;
    }

    public class SpanBreakInfo
    {
        public SpanBreakInfo(bool isRightToLeft, uint scriptTag, uint langTag = 0)
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