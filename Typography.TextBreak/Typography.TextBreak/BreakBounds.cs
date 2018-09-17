//MIT, 2016-present, WinterDev
//some code from ICU project with BSD license

namespace Typography.TextBreak
{
    public ref struct BreakBounds
    {
        public int startIndex;
        public int length;
        public bool stopNext;
        public WordKind kind;
    }

    public enum WordKind : byte
    {
        Unknown,
        //
        Whitespace,
        NewLine,
        Number,
        Punc,
        //
        Text,
        TextIncomplete,
        Control
    }
    public readonly struct BreakAtInfo
    {
        public static readonly BreakAtInfo Empty = new BreakAtInfo(0, WordKind.Unknown);
        public readonly int breakAt;
        public readonly WordKind wordKind;
        public BreakAtInfo(int breakAt, WordKind w)
        {
            this.breakAt = breakAt;
            this.wordKind = w;
        }
    }

}
