//MIT, 2020, WinterDev
namespace Typography.TextBreak
{
    public struct BreakAtInfo
    {
        public readonly int breakAt;
        public readonly WordKind wordKind;
        public BreakAtInfo(int breakAt, WordKind w)
        {
            this.breakAt = breakAt;
            this.wordKind = w;
        }
#if DEBUG
        public override string ToString()
        {
            return breakAt + "," + wordKind;
        }
#endif
    }
}