using System;
using System.Collections.Generic;
using System.Text;

namespace Typography.TextBreak.ICU
{
#if DEBUG
    public static class dbugTestMyFtLib
    {
        static InMemoryIcuDataHolder dataHolder;

        public static void Test1()
        {
             
            NativeTextBreaker.GetVersion(out int major, out int minor);

            string str = "ABCD EFGH IJKL\0";
            var textBreaker = new NativeTextBreaker(TextBreakKind.Word, "en-US");
            List<SplitBound> tokens = new List<SplitBound>();
            textBreaker.DoBreak(str, splitBound =>
            {
                tokens.Add(splitBound);
            });

        }
        public static void Test2()
        {
            //string str = "ABCD EFGH IJKL\0";
            //var textBreaker = new ManagedTextBreaker(TextBreakKind.Word, "en-US");
            //List<SplitBound> tokens = new List<SplitBound>();
            //textBreaker.DoBreak(str, splitBound =>
            //{
            //    tokens.Add(splitBound);
            //});

        }
    }
#endif
}
