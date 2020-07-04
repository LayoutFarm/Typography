//MIT, 2016-present, WinterDev

using System.Collections.Generic;

namespace Typography.OpenFont
{

    //https://docs.microsoft.com/en-us/typography/opentype/spec/os2#ulunicoderange1-bits-031ulunicoderange2-bits-3263ulunicoderange3-bits-6495ulunicoderange4-bits-96127     

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

    public readonly struct BitposAndAssciatedUnicodeRanges
    {
        public readonly int Bitpos;
        public readonly UnicodeRangeInfo[] Ranges;
        public BitposAndAssciatedUnicodeRanges(int bitpos, UnicodeRangeInfo[] ranges)
        {
            Bitpos = bitpos;
            Ranges = ranges;
        }
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Bitpos + ",");
            for (int i = 0; i < Ranges.Length; ++i)
            {
                sb.Append(Ranges[i].ToString());
            }
            return sb.ToString();
        }
        public bool IsInRange(int codepoint)
        {
            for (int i = 0; i < Ranges.Length; ++i)
            {
                if (Ranges[i].IsInRange(codepoint))
                {
                    return true;
                }
            }
            return false;
        }
    }



}
