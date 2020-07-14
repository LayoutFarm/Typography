//MIT, 2016-present, WinterDev, Sam Hocevar
using System;
using System.Collections.Generic;
using Typography.TextBreak;

namespace Typography.TextLayout
{
    public interface ILineSegmentList
    {
        int Count { get; }
        ILineSegment this[int index] { get; }
    }
    public interface ILineSegment
    {
        int StartAt { get; }
        ushort Length { get; }
    }


    //---------
    public readonly struct TextPrinterLineSegment : ILineSegment
    {
        public TextPrinterLineSegment(int startAt, int len, WordKind wordKind, SpanBreakInfo breakInfo)
        {
            _startAt = startAt;
            _len = (ushort)len; //***
            WordKind = wordKind;
            BreakInfo = breakInfo;
        }

        readonly int _startAt;
        readonly ushort _len;

        public int StartAt => _startAt;
        public ushort Length => _len;
        public readonly WordKind WordKind;
        public readonly SpanBreakInfo BreakInfo;
#if DEBUG
        public override string ToString()
        {
            return StartAt + ":" + Length + (BreakInfo.RightToLeft ? "(rtl)" : "");
        }
#endif
    }

    public class TextPrinterLineSegmentList<T> : ILineSegmentList
        where T : ILineSegment
    {
        readonly List<T> _segments = new List<T>();
        public TextPrinterLineSegmentList()
        {
        }
        public void AddLineSegment(T lineSegment)
        {
            _segments.Add(lineSegment);
        }
        public void Clear()
        {
            _segments.Clear();
        }

        public T GetLineSegment(int index) => _segments[index];
        //
        public ILineSegment this[int index] => _segments[index];
        //
        public int Count => _segments.Count;
        //
        public ILineSegment GetSegment(int index) => _segments[index];
        //
#if DEBUG
        public int dbugStartAt;
        public int dbugLen;
#endif
        public void Dispose()
        {
        }
    }


    public class TextPrinterWordVisitor : WordVisitor
    {
        TextPrinterLineSegmentList<TextPrinterLineSegment> _lineSegs;
#if DEBUG
        public TextPrinterWordVisitor() { }
#endif
        public void SetLineSegmentList(TextPrinterLineSegmentList<TextPrinterLineSegment> lineSegs)
        {
            _lineSegs = lineSegs;
        }
        protected override void OnBreak()
        {
            _lineSegs.AddLineSegment(new TextPrinterLineSegment(
                this.LatestSpanStartAt,
                this.LatestSpanLen,
                this.LatestWordKind,
                this.SpanBreakInfo));
        }
    }


}