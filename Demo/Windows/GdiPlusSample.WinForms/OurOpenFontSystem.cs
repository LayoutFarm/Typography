//MIT, 2016-present, WinterDev
using System;
using Typography.OpenFont;
using Typography.Text;
using Typography.FontCollection;
using System.Collections.Generic;

namespace SampleWinForms
{
    static class OurOpenFontSystem
    {
        static bool s_isInit;
        static OpenFontTextService s_textServices;
        static InstalledTypefaceCollection s_installedTypefaceCollection;

        public static void Setup()
        {
            if (s_isInit) return;
            s_isInit = true;

            //1. text breaker
            Typography.TextBreak.CustomBreakerBuilder.Setup(
                new Typography.TextBreak.IcuSimpleTextFileDictionaryProvider()
                {
                    DataDir = "../../../../../Typography.TextBreak/icu62/brkitr"
                });


            //1. create font collection             
            s_installedTypefaceCollection = new InstalledTypefaceCollection();
            //2. set some essential handler
            s_installedTypefaceCollection.SetFontNameDuplicatedHandler((f1, f2) => FontNameDuplicatedDecision.Skip);
            s_installedTypefaceCollection.LoadFontsFromFolder("../../../TestFonts_Err");
            s_installedTypefaceCollection.LoadFontsFromFolder("../../../TestFonts");
            s_installedTypefaceCollection.UpdateUnicodeRanges();

            s_textServices = new OpenFontTextService(s_installedTypefaceCollection);
            //SKIP Woff,Woff2
            //Svg builder
        }

        public static OpenFontTextService OpenFontTextService => s_textServices;

        public static TextServiceClient CreateTextServiceClient()
        {
            if (!s_isInit) { throw new NotSupportedException(); }
            return s_textServices.CreateNewServiceClient();
        }
        public static Typeface ResolveTypeface(InstalledTypeface instTypeface)
        {
            return s_installedTypefaceCollection.ResolveTypeface(instTypeface);
        }
        public static IEnumerable<InstalledTypeface> GetInstalledTypefaceIter()
        {
            if (!s_isInit) { throw new NotSupportedException(); }
            foreach (InstalledTypeface instTypeface in s_installedTypefaceCollection.GetInstalledFontIter())
            {
                yield return instTypeface;
            }
        }
        public static InstalledTypefaceCollection GetFontCollection()
        {
            if (!s_isInit) { throw new NotSupportedException(); }
            return s_installedTypefaceCollection;
        }
    }
}