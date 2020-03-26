//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License


namespace Typography.TextBreak
{


    public class LaoDictionaryBreakingEngine : DictionaryBreakingEngine
    {
        CustomDic _customDic;
        public LaoDictionaryBreakingEngine(CustomDic customDic)
        {
            _customDic = customDic;
        }
        //
        protected override CustomDic CurrentCustomDic => _customDic;
        //
        public override bool CanBeStartChar(char c)
        {
            return s_canbeStartChars[c - _customDic.FirstChar];
        }
        protected override WordGroup? GetWordGroupForFirstChar(char c)
        {
            return _customDic.GetWordGroupForFirstChar(c);
        }

        //0E80–0EFF 
        public override char FirstUnicodeChar => s_firstChar;
        public override char LastUnicodeChar => s_lastChar;
        //
        static bool[] s_canbeStartChars;
        const char s_firstChar = (char)0x0E80;
        const char s_lastChar = (char)0x0EFF;
        //
        public static char FirstChar => s_firstChar;
        public static char LastChar => s_lastChar;
        //


        static LaoDictionaryBreakingEngine()
        {
            char[] laoCantStartWithChars = new char[] {
                (char)0x0EB0, //
                (char)0x0EB1, //
                (char)0x0EB2,
                (char)0x0EB3,
                (char)0x0EB4,
                (char)0x0EB5,
                (char)0x0EB6,
                (char)0x0EB7,
                (char)0x0EB8,
                (char)0x0EB9,
                (char)0x0EBB,
                (char)0x0EBC,
                //
                (char)0x0EC8,
                (char)0x0EC9,
                (char)0x0ECA,
                (char)0x0ECB,
                (char)0x0ECC,
                (char)0x0ECD,
            };
            //-------------------------------------------------------

            s_canbeStartChars = new bool[s_lastChar - s_firstChar + 1];
            for (int i = s_canbeStartChars.Length - 1; i >= 0; --i)
            {
                s_canbeStartChars[i] = true;
            }
            //------------------------------------------------
            for (int i = laoCantStartWithChars.Length - 1; i >= 0; --i)
            {
                int shiftedIndex = laoCantStartWithChars[i] - s_firstChar;
                //some char can't be start char
                s_canbeStartChars[shiftedIndex] = false;
            }

        }
    }


}