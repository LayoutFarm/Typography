//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License


using System.Collections.Generic;

namespace Typography.TextBreak
{
    public static class CustomBreakerBuilder
    {
        static ThaiDictionaryBreakingEngine _thaiDicBreakingEngine;
        static LaoDictionaryBreakingEngine _laoDicBreakingEngine;

        static bool s_isInit;
        static DictionaryProvider s_dicProvider;

        static void InitAllDics()
        {
            if (_thaiDicBreakingEngine == null)
            {
                var customDic = new CustomDic();
                _thaiDicBreakingEngine = new ThaiDictionaryBreakingEngine();
                _thaiDicBreakingEngine.SetDictionaryData(customDic);//add customdic to the breaker
                customDic.SetCharRange(_thaiDicBreakingEngine.FirstUnicodeChar, _thaiDicBreakingEngine.LastUnicodeChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("thai"));
            }

            if (_laoDicBreakingEngine == null)
            {
                var customDic = new CustomDic();
                _laoDicBreakingEngine = new LaoDictionaryBreakingEngine();
                _laoDicBreakingEngine.SetDictionaryData(customDic);//add customdic to the breaker
                customDic.SetCharRange(_laoDicBreakingEngine.FirstUnicodeChar, _laoDicBreakingEngine.LastUnicodeChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("lao"));
            }
        }


        public static void Setup(DictionaryProvider dicProvider)
        {
            if (s_isInit) return;

            s_dicProvider = dicProvider;
            InitAllDics();
            s_isInit = true;
        }

        public static CustomBreaker NewCustomBreaker()
        {
            if (!s_isInit)
            {
                if (s_dicProvider == null)
                {
                    //no dictionary provider
                    return null;
                }
                InitAllDics();
                s_isInit = true;
            }
            var breaker = new CustomBreaker();
            breaker.AddBreakingEngine(_thaiDicBreakingEngine);
            breaker.AddBreakingEngine(_laoDicBreakingEngine);
            return breaker;
        }
    }

    public abstract class DictionaryProvider
    {
        public abstract IEnumerable<string> GetSortedUniqueWordList(string dicName);
    }




}