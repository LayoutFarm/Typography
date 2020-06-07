//MIT, 2016-present, WinterDev
//some code from ICU project with BSD license

namespace Typography.TextBreak
{

    
   
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
        Control,
        SurrogatePair,
    }
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
