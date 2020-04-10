//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License


using System.Collections.Generic;

namespace Typography.TextBreak
{
    public static class CustomBreakerBuilder
    {
        //---------------------------------------------------------
        //user can build their owner custom breaker builder 
        //---------------------------------------------------------

        //custom dic may be shared among breaking engine

        [System.ThreadStatic]
        static CustomDic? s_thaiDic;
        [System.ThreadStatic]
        static CustomDic? s_laoDic;

        [System.ThreadStatic]
        static CustomAbbrvDic? s_enAbbrvDic;


        [System.ThreadStatic]
        static DictionaryProvider? s_dicProvider;

        static void InitAllDics()
        {
            //
            if (s_dicProvider == null) return;
            if (s_thaiDic == null)
            {
                var customDic = new CustomDic();
                customDic.SetCharRange(ThaiDictionaryBreakingEngine.FirstChar, ThaiDictionaryBreakingEngine.LastChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("thai"));
                s_thaiDic = customDic;
            }
            if (s_laoDic == null)
            {
                var customDic = new CustomDic();
                customDic.SetCharRange(LaoDictionaryBreakingEngine.FirstChar, LaoDictionaryBreakingEngine.LastChar);
                customDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("lao"));
                s_laoDic = customDic;
            }


            if (s_enAbbrvDic == null)
            {
                s_enAbbrvDic = new CustomAbbrvDic();
                s_enAbbrvDic.LoadSortedUniqueWordList(s_dicProvider.GetSortedUniqueWordList("abbrv-en"));
            }
        }


        public static void Setup(DictionaryProvider dicProvider)
        {
            if (s_dicProvider != null)
            {
                return;
            }
            //
            s_dicProvider = dicProvider;
            InitAllDics();
        }

        public static CustomBreaker? NewCustomBreaker(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            if (s_thaiDic == null)
            {
                if (s_dicProvider == null)
                {
                    //no dictionary provider
                    return null;
                }
                InitAllDics();
            }
            var breaker = new CustomBreaker(newWordBreakHandler);

            breaker.EngBreakingEngine.EngCustomAbbrvDic = s_enAbbrvDic;//optional 
            breaker.EngBreakingEngine.EnableCustomAbbrv = true;//optional 
            // 
            if (s_thaiDic != null)
            {
                var thBreaker = new ThaiDictionaryBreakingEngine(s_thaiDic);
                //thBreaker.DontMergeLastIncompleteWord = true;
                breaker.AddBreakingEngine(thBreaker);
            }
            //
            if (s_laoDic != null)
            {
                var laoBreak = new LaoDictionaryBreakingEngine(s_laoDic);
                breaker.AddBreakingEngine(laoBreak);
            }
            return breaker;
        }
    }

    public abstract class DictionaryProvider
    {
        public abstract IEnumerable<string> GetSortedUniqueWordList(string dicName);
    }




}