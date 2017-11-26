//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//-----------------------------------
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices; 
namespace Typography.TextBreak.ICU
{
    static class NativeTextBreakerLib
    {
        const string myfontLib = NativeDLL.MyFtLibName;

        [DllImport(myfontLib, CallingConvention = CallingConvention.Cdecl)]
        public static extern void MyFtLibGetFullVersion(out int major, out int minor, out int revision);
        [DllImport(myfontLib, CharSet = CharSet.Ansi)]
        public static extern void MyFt_IcuSetDataDir(string datadir);
        [DllImport(myfontLib)]
        public static unsafe extern void MyFt_IcuSetData(void* data, out int err);
        [DllImport(myfontLib, CharSet = CharSet.Unicode)]
        public static unsafe extern IntPtr MtFt_UbrkOpen(UBreakIteratorType iterType, byte[] locale, char* startChar, int len, out int err);
        [DllImport(myfontLib)]
        public static extern void MtFt_UbrkClose(IntPtr naitveBreakIter);
        [DllImport(myfontLib)]
        public static extern int MtFt_UbrkFirst(IntPtr nativeBreakIter);
        [DllImport(myfontLib)]
        public static extern int MtFt_UbrkNext(IntPtr nativeBreakIter);
        [DllImport(myfontLib)]
        public static extern int MtFt_UbrkGetRuleStatus(IntPtr nativeBreakIter);
    }





    /// <summary>
    /// text breaker with icu4c
    /// </summary>
    public class NativeTextBreaker : TextBreaker
    {
        string locale;
        byte[] localebuff;
        public NativeTextBreaker(TextBreakKind breakKind, string locale)
        {
            this.BreakKind = breakKind;
            this.locale = locale;
            localebuff = System.Text.Encoding.ASCII.GetBytes(locale);
        }
        public override void DoBreak(char[] input, int start, int len, OnBreak onbreak)
        {
            //1. 
            UBreakIteratorType type = UBreakIteratorType.WORD;
            switch (BreakKind)
            {
                default:
                case TextBreakKind.Word:
                    type = UBreakIteratorType.WORD;
                    break;
                case TextBreakKind.Sentence:
                    type = UBreakIteratorType.SENTENCE;
                    break;
            }
            //------------------------ 
            int errCode = 0;
            //break all string  
            unsafe
            {
                fixed (char* h = &input[start])
                {
                    IntPtr nativeIter = NativeTextBreakerLib.MtFt_UbrkOpen(type, localebuff, h, len, out errCode);
                    int cur = NativeTextBreakerLib.MtFt_UbrkFirst(nativeIter);
                    while (cur != DONE)
                    {
                        int next = NativeTextBreakerLib.MtFt_UbrkNext(nativeIter);
                        int status = NativeTextBreakerLib.MtFt_UbrkGetRuleStatus(nativeIter);
                        if (next != DONE && AddToken(type, status))
                        {
                            onbreak(new SplitBound(cur, next - cur));
                        }
                        cur = next;
                    }
                    NativeTextBreakerLib.MtFt_UbrkClose(nativeIter);
                }
            }
        }

        const int DONE = -1;
        static bool AddToken(UBreakIteratorType type, int status)
        {
            switch (type)
            {
                case UBreakIteratorType.CHARACTER:
                    return true;
                case UBreakIteratorType.LINE:
                case UBreakIteratorType.SENTENCE:
                    return true;
                case UBreakIteratorType.WORD:
                    return status < (int)UWordBreak.NONE || status >= (int)UWordBreak.NONE_LIMIT;
            }
            return false;
        }

        //this is text breaker impl with ICU lib
        static InMemoryIcuDataHolder dataHolder;

        static string s_icuDataFile;
        static bool s_isDataLoaded;
        static object s_dataLoadLock = new object();
        public static void SetICUDataFile(string icudatafile)
        {
            lock (s_dataLoadLock)
            {
                if (s_isDataLoaded)
                {
                    return;
                }
            }
            if (s_isDataLoaded)
            {
                return;
            }
            s_isDataLoaded = true;
            s_icuDataFile = icudatafile;
            //----------
            int major, minor, revision;
            NativeTextBreakerLib.MyFtLibGetFullVersion(out major, out minor, out revision);
            if (dataHolder == null)
            {
                //dataHolder = new InMemoryIcuDataHolder(@"d:\WImageTest\icudt57l\icudt57l.dat");
                dataHolder = new InMemoryIcuDataHolder(icudatafile);
                dataHolder.Use();
            }
        }

    }
    //------
    /// <summary>
    /// The possible types of text boundaries.
    /// </summary>
    enum UBreakIteratorType
    {
        /// <summary>Character breaks.</summary>
        CHARACTER = 0,
        /// <summary>Word breaks.</summary>
        WORD,
        /// <summary>Line breaks.</summary>
        LINE,
        /// <summary>Sentence breaks.</summary>
        SENTENCE,
        // <summary>Title Case breaks.</summary>
        // obsolete. Use WORD instead.
        //TITLE
    }

    enum UWordBreak
    {
        /// <summary>
        /// Tag value for "words" that do not fit into any of other categories.
        /// Includes spaces and most punctuation.
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Upper bound for tags for uncategorized words.
        /// </summary>
        NONE_LIMIT = 100,
        NUMBER = 100,
        NUMBER_LIMIT = 200,
        LETTER = 200,
        LETTER_LIMIT = 300,
        KANA = 300,
        KANA_LIMIT = 400,
        IDEO = 400,
        IDEO_LIMIT = 500,
    }

    enum ULineBreakTag
    {
        SOFT = 0,
        SOFT_LIMIT = 100,
        HARD = 100,
        HARD_LIMIT = 200,
    }

    enum USentenceBreakTag
    {
        TERM = 0,
        TERM_LIMIT = 100,
        SEP = 100,
        SEP_LIMIT = 200,
    }
    //------
    class InMemoryIcuDataHolder : IDisposable
    {
        IntPtr unmanagedICUMemData;
        public InMemoryIcuDataHolder(string loadIcuDataFromFile)
        {
            byte[] inMemoryICUData = System.IO.File.ReadAllBytes(loadIcuDataFromFile);
            unmanagedICUMemData = System.Runtime.InteropServices.Marshal.AllocHGlobal(inMemoryICUData.Length);
            System.Runtime.InteropServices.Marshal.Copy(inMemoryICUData, 0, unmanagedICUMemData, inMemoryICUData.Length);
        }
        public void Use()
        {
            int errCode;
            unsafe
            {
                NativeTextBreakerLib.MyFt_IcuSetData((void*)unmanagedICUMemData, out errCode);
            }
        }
        public void Dispose()
        {
            if (unmanagedICUMemData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(unmanagedICUMemData);
                unmanagedICUMemData = IntPtr.Zero;
            }
        }
    }


#if DEBUG
    public static class dbugTestMyFtLib
    {
        static InMemoryIcuDataHolder dataHolder;

        public static void Test1()
        {


            int major, minor, revision;
            NativeTextBreakerLib.MyFtLibGetFullVersion(out major, out minor, out revision);
            NativeTextBreaker.SetICUDataFile(@"d:\WImageTest\icudt57l\icudt57l.dat");

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