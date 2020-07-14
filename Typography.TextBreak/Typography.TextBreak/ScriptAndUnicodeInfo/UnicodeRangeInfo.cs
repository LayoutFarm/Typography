//MIT, 2016-present, WinterDev 

namespace Typography.OpenFont
{

   
    public class UnicodeRangeInfo
    {
        /// <summary>
        /// begin code point
        /// </summary>
        public int StarCodepoint { get; }
        /// <summary>
        /// end codepoint
        /// </summary>
        public int EndCodepoint { get; }

        public string Name { get; }

        internal UnicodeRangeInfo(int startAt, int endAt, string name)
        {
            StarCodepoint = startAt;
            EndCodepoint = endAt;
            Name = name;
        }
        public bool IsInRange(int codepoint) => codepoint >= StarCodepoint && codepoint <= EndCodepoint;
        public override string ToString() => Name;

    }

  



}
