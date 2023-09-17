//MIT, 2014-2016, WinterDev
//-----------------------------------
//use FreeType and HarfBuzz wrapper
//native dll lib
//plan?: port  them to C#  :)
//-----------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
namespace Typography.TextBreak.ICU
{

    /// <summary>
    /// text breaker with icu4c
    /// </summary>
    public class NativeTextBreaker : TextBreaker
    {
        string _locale;
        byte[] _localebuff;
        public NativeTextBreaker(TextBreakKind breakKind, string locale)
        {
            this.BreakKind = breakKind;
            this._locale = locale;
            _localebuff = System.Text.Encoding.ASCII.GetBytes(locale);
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
                    IntPtr nativeIter = Icu4c.ubrk_open(type, _localebuff, h, len, out errCode);
                    int cur = Icu4c.ubrk_first(nativeIter);
                    while (cur != DONE)
                    {
                        int next = Icu4c.ubrk_next(nativeIter);
                        int status = Icu4c.ubrk_getRuleStatus(nativeIter);
                        if (next != DONE && AddToken(type, status))
                        {
                            onbreak(new SplitBound(cur, next - cur));
                        }
                        cur = next;
                    }
                    Icu4c.ubrk_close(nativeIter);
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


        static bool s_init = false;
        public static void LoadLib(string iculibDir, int version)
        {
            //TODO:
            if (s_init) { return; }

            s_init = true;
            Icu4c.Load(iculibDir, version);
            unsafe
            {
                //check version
                byte[] version_buffer = new byte[20];
                fixed (byte* version_buffer_ptr = &version_buffer[0])
                {
                    Icu4c.u_getVersion(version_buffer_ptr);
                }
                s_versionMajor = version_buffer[0];
                s_versionMinor = version_buffer[1];
            }
        }

        static int s_versionMajor;
        static int s_versionMinor;
        public static void GetVersion(out int major, out int minor)
        {
            major = s_versionMajor;
            minor = s_versionMinor;
        }
        static InMemoryIcuDataHolder s_dataHolder;

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

            if (s_dataHolder == null)
            {
                s_dataHolder = new InMemoryIcuDataHolder(icudatafile);
                s_dataHolder.Use();
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





    class InMemoryIcuDataHolder : IDisposable
    {
        IntPtr _unmanagedICUMemData;
        public InMemoryIcuDataHolder(string loadIcuDataFromFile)
        {
            byte[] inMemoryICUData = System.IO.File.ReadAllBytes(loadIcuDataFromFile);
            _unmanagedICUMemData = System.Runtime.InteropServices.Marshal.AllocHGlobal(inMemoryICUData.Length);
            System.Runtime.InteropServices.Marshal.Copy(inMemoryICUData, 0, _unmanagedICUMemData, inMemoryICUData.Length);
        }
        public void Use()
        {
            int errCode;
            if (_unmanagedICUMemData != IntPtr.Zero)
            {
                Icu4c.urk_setCommonData(_unmanagedICUMemData, out errCode);
            }

        }
        public void Dispose()
        {
            if (_unmanagedICUMemData != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_unmanagedICUMemData);
                _unmanagedICUMemData = IntPtr.Zero;
            }
        }
    }


    static class Icu4c
    {
        static IntPtr s_icuuclib;
        internal static udata_setCommonData urk_setCommonData;
        internal static ubrk_open ubrk_open;
        internal static ubrk_close ubrk_close;
        internal static ubrk_first ubrk_first;
        internal static ubrk_next ubrk_next;
        internal static ubrk_getRuleStatus ubrk_getRuleStatus;
        internal static u_getVersion u_getVersion;


        public static void Load(string icuLibDirectory, int version)
        {
            string cur_dir = Directory.GetCurrentDirectory(); //save
            Directory.SetCurrentDirectory(icuLibDirectory);
            s_icuuclib = LoadLibrary($"icuuc{version}.dll");

#if DEBUG
            if (s_icuuclib == IntPtr.Zero)
            {

            }
#endif

            GetFuncPtr(s_icuuclib, $"udata_setCommonData_{version}", out urk_setCommonData);
            GetFuncPtr(s_icuuclib, $"ubrk_open_{version}", out ubrk_open);
            GetFuncPtr(s_icuuclib, $"ubrk_close_{version}", out ubrk_close);
            GetFuncPtr(s_icuuclib, $"ubrk_first_{version}", out ubrk_first);
            GetFuncPtr(s_icuuclib, $"ubrk_next_{version}", out ubrk_next);
            GetFuncPtr(s_icuuclib, $"ubrk_getRuleStatus_{version}", out ubrk_getRuleStatus);
            GetFuncPtr(s_icuuclib, $"u_getVersion_{version}", out u_getVersion);
            Directory.SetCurrentDirectory(cur_dir); //restore    
        }

        public static void Unload()
        {
            FreeLibrary(s_icuuclib);
            s_icuuclib = IntPtr.Zero;
            urk_setCommonData = null;
            ubrk_open = null;
            ubrk_close = null;
            ubrk_first = null;
            ubrk_next = null;
            ubrk_getRuleStatus = null;
        }
        static bool GetFuncPtr<T>(IntPtr module_ptr, string funcName, out T funcptr)
        {
            IntPtr ptr = GetProcAddress(module_ptr, funcName);       //win32
            if (ptr == IntPtr.Zero)
            {
                funcptr = default;
                return false;
            }
            funcptr = (T)(object)System.Runtime.InteropServices.Marshal.GetDelegateForFunctionPointer(ptr, typeof(T));
            return true;
        }


        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FreeLibrary(IntPtr hModule);
        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
    }

    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void udata_setCommonData(IntPtr data, out int err);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate IntPtr ubrk_open(UBreakIteratorType iterType, byte[] locale, char* startChar, int len, out int err);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate void ubrk_close(IntPtr nativeBreakIter);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int ubrk_first(IntPtr nativeBreakIter);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int ubrk_next(IntPtr nativeBreakIter);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    delegate int ubrk_getRuleStatus(IntPtr nativeBreakIter);
    [System.Runtime.InteropServices.UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    unsafe delegate void u_getVersion(/*icu version max len =20*/byte* version);  //see uversion.h

}