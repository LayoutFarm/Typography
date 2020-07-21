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
    public readonly struct LineSegment : ILineSegment
    {
        public LineSegment(int startAt, int len, WordKind wordKind, SpanBreakInfo breakInfo)
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

    public class LineSegmentList<T> : ILineSegmentList
        where T : ILineSegment
    {
        readonly List<T> _segments = new List<T>();
        public LineSegmentList()
        {
        }
        public void AddLineSegment(T lineSegment) => _segments.Add(lineSegment);

        public void Clear() => _segments.Clear();

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


    public class LayoutWordVisitor : WordVisitor
    {
        LineSegmentList<LineSegment> _lineSegs;
#if DEBUG
        public LayoutWordVisitor() { }
#endif
        public void SetLineSegmentList(LineSegmentList<LineSegment> lineSegs)
        {
            _lineSegs = lineSegs;
        }
        protected override void OnBreak()
        {
            _lineSegs.AddLineSegment(new LineSegment(
                this.LatestSpanStartAt,
                this.LatestSpanLen,
                this.LatestWordKind,
                this.SpanBreakInfo));
        }
    }


}