//MIT, 2016-present, WinterDev 
using System;
using System.Collections.Generic;
using System.IO;
using Typography.OpenFont;
using Typography.OpenFont.Tables;


namespace Typography.FontCollection
{
    [Flags]
    public enum TypefaceStyle
    {
        Others = 0,
        Regular = 1,
        Italic = 1 << 1,
    }

    public delegate FontNameDuplicatedDecision FontNameDuplicatedHandler(InstalledTypeface existing, InstalledTypeface newAddedFont);

    public enum FontNameDuplicatedDecision
    {
        /// <summary>
        /// use existing, skip latest font
        /// </summary>
        Skip,
        /// <summary>
        /// replace with existing with the new one
        /// </summary>
        Replace
    }

    /// <summary>
    /// AlternativeTypefaceSelector
    /// </summary>
    public abstract class AltTypefaceSelectorBase
    {

#if DEBUG
        public AltTypefaceSelectorBase() { }
#endif

        public Typeface LatestTypeface { get; set; }
        public abstract SelectedTypeface Select(List<InstalledTypeface> choices, UnicodeRangeInfo unicodeRangeInfo, int codepoint);


        
        public readonly struct SelectedTypeface
        {
            public readonly InstalledTypeface InstalledTypeface;
            public readonly Typeface Typeface;
            public SelectedTypeface(InstalledTypeface installedTypeface)
            {
                
                Typeface = null;
                InstalledTypeface = installedTypeface;
            }
            public SelectedTypeface(Typeface typeface)
            {
                Typeface = typeface;
                InstalledTypeface = null;
            }
            public bool IsEmpty() => Typeface == null && InstalledTypeface == null;
        }
    }

    public class PreferredTypeface
    {
        public PreferredTypeface(string reqTypefaceName) => RequestTypefaceName = reqTypefaceName;
        public string RequestTypefaceName { get; }
        public InstalledTypeface InstalledTypeface { get; set; }
        public bool ResolvedInstalledTypeface { get; set; }
    }

    public class PreferredTypefaceList : List<PreferredTypeface>
    {
#if DEBUG
        //TODO: review this again
        public PreferredTypefaceList() { }
#endif
        public void AddTypefaceName(string typefaceName)
        {
            this.Add(new PreferredTypeface(typefaceName));
        }
    }

    public interface IFontStreamSource
    {
        Stream ReadFontStream();
        string PathName { get; }
    }

    public interface IInstalledTypefaceProvider
    {
        InstalledTypeface GetInstalledTypeface(string fontName, TypefaceStyle style, ushort weight);
    }

    public delegate InstalledTypeface FontNotFoundHandler(InstalledTypefaceCollection typefaceCollection,
        string fontName,
        TypefaceStyle style,
        ushort weightClass,
        InstalledTypeface available,
        List<InstalledTypeface> availableList);

    public class FontFileStreamProvider : IFontStreamSource
    {
        public FontFileStreamProvider(string filename)
        {
            this.PathName = filename;
        }
        public string PathName { get; private set; }
        public Stream ReadFontStream()
        {
            //TODO: don't forget to dispose this stream when not use
            return new FileStream(this.PathName, FileMode.Open, FileAccess.Read);
        }
    }


    public delegate void FirstInitFontCollectionDelegate(InstalledTypefaceCollection typefaceCollection);

    public readonly struct TinyCRC32Calculator
    {

        /// <summary>
        /// Update the value for the running CRC32 using the given block of bytes.
        /// This is useful when using the CRC32() class in a Stream.
        /// </summary>
        /// <param name="block">block of bytes to slurp</param>
        /// <param name="offset">starting point in the block</param>
        /// <param name="count">how many bytes within the block to slurp</param>
        static int SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new NotSupportedException("The data buffer must not be null.");
            }

            // UInt32 tmpRunningCRC32Result = _RunningCrc32Result;

            uint _runningCrc32Result = 0xFFFFFFFF;
            for (int i = 0; i < count; i++)
            {
#if DEBUG
                int x = offset + i;
#endif
                //_runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[x]) ^ ((_runningCrc32Result) & 0x000000FF)];
                _runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[offset + i]) ^ ((_runningCrc32Result) & 0x000000FF)];
                //tmpRunningCRC32Result = ((tmpRunningCRC32Result) >> 8) ^ crc32Table[(block[offset + i]) ^ ((tmpRunningCRC32Result) & 0x000000FF)];
            }
            return unchecked((Int32)(~_runningCrc32Result));
        }


        // pre-initialize the crc table for speed of lookup.
        static TinyCRC32Calculator()
        {
            unchecked
            {
                // PKZip specifies CRC32 with a polynomial of 0xEDB88320;
                // This is also the CRC-32 polynomial used bby Ethernet, FDDI,
                // bzip2, gzip, and others.
                // Often the polynomial is shown reversed as 0x04C11DB7.
                // For more details, see http://en.wikipedia.org/wiki/Cyclic_redundancy_check
                UInt32 dwPolynomial = 0xEDB88320;


                s_crc32Table = new UInt32[256];
                UInt32 dwCrc;
                for (uint i = 0; i < 256; i++)
                {
                    dwCrc = i;
                    for (uint j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                    s_crc32Table[i] = dwCrc;
                }
            }
        }


#if DEBUG
        //Int64 dbugTotalBytesRead;
#endif

        static readonly UInt32[] s_crc32Table;
        const int BUFFER_SIZE = 2048;

        [System.ThreadStatic]
        static byte[] s_buffer;

        public static int CalculateCrc32(string inputData)
        {
            if (s_buffer == null)
            {
                s_buffer = new byte[BUFFER_SIZE];
            }

            if (inputData.Length > 512)
            {
                byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(inputData);
                return SlurpBlock(utf8, 0, utf8.Length);
            }
            else
            {
                int write = System.Text.Encoding.UTF8.GetBytes(inputData, 0, inputData.Length, s_buffer, 0);
                if (write >= BUFFER_SIZE)
                {
                    throw new System.NotSupportedException("crc32:");
                }
                return SlurpBlock(s_buffer, 0, write);
            }
        }

        public static int CalculateCrc32(byte[] buffer) => SlurpBlock(buffer, 0, buffer.Length);
    }

    public static class InstalledTypefaceCollectionExtensions
    {

        public delegate R MyFunc<T1, T2, R>(T1 t1, T2 t2);
        public delegate R MyFunc<T, R>(T t);

        public static Action<InstalledTypefaceCollection> CustomSystemFontListLoader;

        public static MyFunc<string, Stream> CustomFontStreamLoader;
        public static void LoadFontsFromFolder(this InstalledTypefaceCollection fontCollection, string folder, bool recursive = false)
        {
            if (!Directory.Exists(folder))
            {
#if DEBUG

                System.Diagnostics.Debug.WriteLine("LoadFontsFromFolder, not found folder:" + folder);

#endif
                return;
            }
            //-------------------------------------

            // 1. font dir
            foreach (string file in Directory.GetFiles(folder))
            {
                //eg. this is our custom font folder
                string ext = Path.GetExtension(file).ToLower();
                switch (ext)
                {
                    default: break;
                    case ".ttc":
                    case ".otc":
                    case ".ttf":
                    case ".otf":
                    case ".woff":
                    case ".woff2":
                        fontCollection.AddFontStreamSource(new FontFileStreamProvider(file));
                        break;
                }
            }

            //2. browse recursively; on Linux, fonts are organised in subdirectories
            if (recursive)
            {
                foreach (string subfolder in Directory.GetDirectories(folder))
                {
                    LoadFontsFromFolder(fontCollection, subfolder, recursive);
                }
            }
        }
        public static void LoadSystemFonts(this InstalledTypefaceCollection fontCollection, bool recursive = false)
        {

            if (CustomSystemFontListLoader != null)
            {
                CustomSystemFontListLoader(fontCollection);
                return;
            }
            // Windows system fonts
            LoadFontsFromFolder(fontCollection, "c:\\Windows\\Fonts");
            // These are reasonable places to look for fonts on Linux
            LoadFontsFromFolder(fontCollection, "/usr/share/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/wine/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/texlive/texmf-dist/fonts", true);
            LoadFontsFromFolder(fontCollection, "/usr/share/texmf/fonts", true);

            // OS X system fonts (https://support.apple.com/en-us/HT201722)

            LoadFontsFromFolder(fontCollection, "/System/Library/Fonts");
            LoadFontsFromFolder(fontCollection, "/Library/Fonts");

        }

        public static IEnumerable<BitposAndAssciatedUnicodeRanges> GetSupportedUnicodeLangIter(this InstalledTypeface instTypeface)
        {
            //check all 0-125 bits 
            for (int i = 0; i <= OpenFontBitPosInfo.MAX_BITPOS; ++i)
            {
                if (instTypeface.DoesSupportUnicode(i))
                {
                    yield return OpenFontBitPosInfo.GetUnicodeRanges(i);
                }
            }
        }


        public static Typeface ResolveTypeface(this InstalledTypefaceCollection fontCollection, string fontName, TypefaceStyle style, ushort weight)
        {
            InstalledTypeface inst = fontCollection.GetInstalledTypeface(fontName, style/*InstalledTypefaceCollection.GetSubFam(style)*/, weight);
            return (inst != null) ? fontCollection.ResolveTypeface(inst) : null;
        }
        public static Typeface ResolveTypeface(this InstalledTypefaceCollection fontCollection, InstalledTypeface installedFont)
        {

            if (installedFont.ResolvedTypeface != null) return installedFont.ResolvedTypeface;

            //load  
            Typeface typeface;
            if (CustomFontStreamLoader != null)
            {
                using (var fontStream = CustomFontStreamLoader(installedFont.FontPath))
                {
                    var reader = new OpenFontReader();
                    typeface = reader.Read(fontStream, installedFont.ActualStreamOffset);
                    typeface.Filename = installedFont.FontPath;
                    installedFont.ResolvedTypeface = typeface;//cache 
                }
            }
            else
            {
                using (var fs = new FileStream(installedFont.FontPath, FileMode.Open, FileAccess.Read))
                {

                    var reader = new OpenFontReader();
                    typeface = reader.Read(fs, installedFont.ActualStreamOffset);
                    typeface.Filename = installedFont.FontPath;
                    installedFont.ResolvedTypeface = typeface;//cache 
                }
            }

            //calculate typeface key for the new create typeface
            //custom key

            OpenFont.Extensions.TypefaceExtensions.SetCustomTypefaceKey(
                typeface,
                TinyCRC32Calculator.CalculateCrc32(typeface.Name.ToUpper()));

            return typeface;

        }

        public static Typeface ResolveTypefaceFromFile(this InstalledTypefaceCollection fontCollection, string filename)
        {
            //from user input typeface filename

            bool enable_absPath = false; //enable abs path or not
#if DEBUG
            enable_absPath = true;//in debug mode
#endif
            //TODO: review here again!!!
            if (!fontCollection._installedTypefacesByFilenames.TryGetValue(filename, out InstalledTypeface found))
            {
                if (!enable_absPath && Path.IsPathRooted(filename))
                {
                    return null;//***
                }

                //search for a file
                if (File.Exists(filename))
                {
                    //TODO: handle duplicated font!!
                    //try read this             
                    InstalledTypeface instTypeface;
                    using (FileStream fs = new FileStream(filename, FileMode.Open))
                    {
                        OpenFontReader reader = new OpenFontReader();
                        Typeface typeface = reader.Read(fs);

                        // 
                        OpenFont.Extensions.TypefaceExtensions.SetCustomTypefaceKey(
                            typeface,
                            TinyCRC32Calculator.CalculateCrc32(typeface.Name.ToUpper()));

                        instTypeface = new InstalledTypeface(typeface, filename);
                        fontCollection._installedTypefacesByFilenames.Add(filename, instTypeface);

                        return instTypeface.ResolvedTypeface = typeface;//assign  and return                         
                    }
                }

            }
            else
            {
                //found inst type
                return ResolveTypeface(fontCollection, found);
            }
            return null;

        }
        //for Windows , how to find Windows' Font Directory from Windows Registry
        //        string[] localMachineFonts = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts", false).GetValueNames();
        //        // get parent of System folder to have Windows folder
        //        DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
        //        string strFontsFolder = Path.Combine(dirWindowsFolder.FullName, "Fonts");
        //        RegistryKey regKey = Registry.LocalMachine.OpenSubKey("Software\\Microsoft\\Windows NT\\CurrentVersion\\Fonts");
        //        //---------------------------------------- 
        //        foreach (string winFontName in localMachineFonts)
        //        {
        //            string f = (string)regKey.GetValue(winFontName);
        //            if (f.EndsWith(".ttf") || f.EndsWith(".otf"))
        //            {
        //                yield return Path.Combine(strFontsFolder, f);
        //            }
        //        }


    }
}