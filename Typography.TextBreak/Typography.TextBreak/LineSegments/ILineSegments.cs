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
        public SpanBreakInfo(bool isRightToLeft, int sampleCodePoint, string scriptTag)
        {
            RightToLeft = isRightToLeft;
            SampleCodePoint = sampleCodePoint;
            ScriptTag = scriptTag;
        }
        public int SampleCodePoint { get; }
        public bool RightToLeft { get; }
        public string ScriptTag { get; }
        public object ResolvedScriptLang { get; set; }
    }


}