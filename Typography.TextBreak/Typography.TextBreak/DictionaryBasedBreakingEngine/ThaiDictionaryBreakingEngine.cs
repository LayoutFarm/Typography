//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License



namespace Typography.TextBreak
{


    public class ThaiDictionaryBreakingEngine : DictionaryBreakingEngine
    {
        CustomDic _customDic;
        public void SetDictionaryData(CustomDic customDic)
        {
            this._customDic = customDic;

        }
        protected override CustomDic CurrentCustomDic
        {
            get { return _customDic; }
        }

        public override bool CanBeStartChar(char c)
        {
            return canbeStartChars[c - this.FirstUnicodeChar];

        }
        protected override WordGroup GetWordGroupForFirstChar(char c)
        {
            return _customDic.GetWordGroupForFirstChar(c);
        }

        public override char FirstUnicodeChar
        {
            //0E00-0E7F 

            get { return s_firstChar; }
        }
        public override char LastUnicodeChar
        {
            get
            {
                return s_lastChar;
            }
        }
        //------------------------------------
        //eg thai sara

        static bool[] canbeStartChars;
        const char s_firstChar = (char)0x0E00;
        const char s_lastChar = (char)0xE7F;
        static ThaiDictionaryBreakingEngine()
        {

            char[] cannotStartWithChars = new char[]{
               (char)0x0E30,   (char)0x0E31, (char)0x0E32, (char)0x0E33, (char)0x0E34, (char)0x0E35,
               (char)0x0E36, (char)0x0E37, (char)0x0E38, (char)0x0E39, (char)0x0E3A,
               (char)0x0E45, /*skip(MAI YAMOK)0x0E46,*/ (char)0x0E47, (char)0x0E48, (char)0x0E49, (char)0x0E4A,
               (char)0x0E4B, (char)0x0E4C, (char)0x0E4D, (char)0x0E4E,
            };
            canbeStartChars = new bool[s_lastChar - s_firstChar + 1];
            for (int i = canbeStartChars.Length - 1; i >= 0; --i)
            {
                canbeStartChars[i] = true;
            }
            //------------------------------------------------
            for (int i = cannotStartWithChars.Length - 1; i >= 0; --i)
            {
                int shiftedIndex = cannotStartWithChars[i] - s_firstChar;
                //some char can't be start char
                canbeStartChars[shiftedIndex] = false;
            }

        }
    }


}