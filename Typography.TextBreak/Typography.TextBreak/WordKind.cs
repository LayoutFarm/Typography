//MIT, 2016-present, WinterDev
//some code from ICU project with BSD license

namespace Typography.TextBreak
{



    public enum WordKind : byte
    {
        Unknown,
        //
        Whitespace, //' ' 
        Tab, //'\t'
        OtherWhitespace, //other whitespace
        NewLine,
        Number,
        Punc,
        Control,

        SurrogatePair,
        TextIncomplete,
        Text,
    }



}
