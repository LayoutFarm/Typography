//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System.IO;
using System.Collections.Generic;

namespace Typography.TextBreak
{
    public static class CustomBreakerBuilder
    {
        static ThaiDictionaryBreakingEngine thaiDicBreakingEngine;
        static LaoDictionaryBreakingEngine laoDicBreakingEngine;

        static bool isInit;
        static DictionaryProvider s_dicProvider;

        static void InitAllDics()
        {
            if (thaiDicBreakingEngine == null)
            {
                var customDic = new CustomDic();
                thaiDicBreakingEngine = new ThaiDictionaryBreakingEngine();
                thaiDicBreakingEngine.SetDictionaryData(customDic);//add customdic to the breaker
                customDic.SetCharRange(thaiDicBreakingEngine.FirstUnicodeChar, thaiDicBreakingEngine.LastUnicodeChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("thai"));
            }

            if (laoDicBreakingEngine == null)
            {
                var customDic = new CustomDic();
                laoDicBreakingEngine = new LaoDictionaryBreakingEngine();
                laoDicBreakingEngine.SetDictionaryData(customDic);//add customdic to the breaker
                customDic.SetCharRange(laoDicBreakingEngine.FirstUnicodeChar, laoDicBreakingEngine.LastUnicodeChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("lao"));
            }
        }


        //public static void Setup(string dataDir)
        //{
        //    Setup(new IcuSimpleTextFileDictionaryProvider() { DataDir = dataDir });
        //}
        public static void Setup(DictionaryProvider dicProvider)
        {
            if (isInit) return;

            s_dicProvider = dicProvider;
            InitAllDics();
            isInit = true;
        }

        public static CustomBreaker NewCustomBreaker()
        {
            if (!isInit)
            {
                if (s_dicProvider == null)
                {
                    //no dictionary provider
                    return null;
                }
                InitAllDics();
                isInit = true;
            }
            var breaker = new CustomBreaker();
            breaker.AddBreakingEngine(thaiDicBreakingEngine);
            breaker.AddBreakingEngine(laoDicBreakingEngine);
            return breaker;
        }
    }

    public abstract class DictionaryProvider
    {
        public abstract IEnumerable<string> GetSortedUniqueWordList(string dicName);
    }



   
}