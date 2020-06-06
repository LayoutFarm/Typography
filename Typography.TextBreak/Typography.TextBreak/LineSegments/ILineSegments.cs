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
        SpanLayoutInfo SpanLayoutInfo { get; }
    }

    public struct BreakSpan
    {
        //TODO: review here again***
        public int startAt;
        public ushort len;
        public WordKind wordKind;
        public SpanLayoutInfo spanLayoutInfo;
    }

    public class SpanLayoutInfo
    {
        public SpanLayoutInfo(bool isRightToLeft, int sampleCodePoint, string scriptLang)
        {
            RightToLeft = isRightToLeft;
            SampleCodePoint = sampleCodePoint;
            ScriptLang = scriptLang;
        }
        public int SampleCodePoint { get; }
        public bool RightToLeft { get; }
        public string ScriptLang { get; }
        public object ResolvedScriptLang { get; set; }
    }


}