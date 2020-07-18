//MIT, 2016-present, WinterDev 

namespace Typography.OpenFont
{

   
    public class UnicodeRangeInfo
    {
        /// <summary>
        /// begin code point
        /// </summary>
        public int StartCodepoint { get; }
        /// <summary>
        /// end codepoint
        /// </summary>
        public int EndCodepoint { get; }

        public string Name { get; }

        internal UnicodeRangeInfo(int startAt, int endAt, string name)
        {
            StartCodepoint = startAt;
            EndCodepoint = endAt;
            Name = name;
        }
        public bool IsInRange(int codepoint) => codepoint >= StartCodepoint && codepoint <= EndCodepoint;
        public override string ToString() => Name;

    }

  



}
