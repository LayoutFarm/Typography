//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License


namespace Typography.TextBreak
{
    public class KhmerDictionaryBreakingEngine : DictionaryBreakingEngine
    {
        public override char FirstUnicodeChar => throw new System.NotImplementedException();

        public override char LastUnicodeChar => throw new System.NotImplementedException();

        protected override CustomDic CurrentCustomDic => throw new System.NotImplementedException();

        public override bool CanBeStartChar(char c)
        {
            throw new System.NotImplementedException();
        }

        protected override WordGroup GetWordGroupForFirstChar(char c)
        {
            throw new System.NotImplementedException();
        }
    }
}