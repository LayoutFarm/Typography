//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev
using System.Collections.Generic;
using Typography.OpenFont.Tables;

namespace Typography.OpenFont
{
    public class Typeface
    {
        readonly Bounds _bounds;
        readonly ushort _unitsPerEm;
        readonly Glyph[] _glyphs;
        //TODO: implement vertical metrics
        readonly HorizontalMetrics _horizontalMetrics;
        readonly NameEntry _nameEntry;

        Kern _kern;

        internal Typeface(
            NameEntry nameEntry,
            Bounds bounds,
            ushort unitsPerEm,
            Glyph[] glyphs,
            HorizontalMetrics horizontalMetrics,
            OS2Table os2Table)
        {
            _nameEntry = nameEntry;
            _bounds = bounds;
            _unitsPerEm = unitsPerEm;
            _glyphs = glyphs;
            _horizontalMetrics = horizontalMetrics;
            OS2Table = os2Table;


        }


        CFFTable _cffTable;
        internal Typeface(
           NameEntry nameEntry,
           Bounds bounds,
           ushort unitsPerEm,
           CFFTable cffTable,
           HorizontalMetrics horizontalMetrics,
           OS2Table os2Table)
        {

            _nameEntry = nameEntry;
            _bounds = bounds;
            _unitsPerEm = unitsPerEm;
            _cffTable = cffTable;
            _horizontalMetrics = horizontalMetrics;
            OS2Table = os2Table;


            //------
            this._glyphs = _cffTable.Cff1FontSet._fonts[0].glyphs;
        }

        /// <summary>
        /// control values in Font unit
        /// </summary>
        internal int[] ControlValues { get; set; }
        internal byte[] PrepProgramBuffer { get; set; }
        internal byte[] FpgmProgramBuffer { get; set; }
        internal MaxProfile MaxProfile { get; set; }

        public bool HasPrepProgramBuffer { get { return PrepProgramBuffer != null; } }
        internal Cmap CmapTable { get; set; }
        internal Kern KernTable
        {
            get { return _kern; }
            set { this._kern = value; }
        }
        internal Gasp GaspTable
        {
            get;
            set;
        }
        internal HorizontalHeader HheaTable
        {
            get;
            set;
        }
        internal OS2Table OS2Table
        {
            get;
            set;
        }
        internal CFFTable CffTable { get { return _cffTable; } }
        /// <summary>
        /// actual font filename
        /// </summary>
        public string Filename { get; set; }
        /// <summary>
        /// OS2 sTypoAscender, in font designed unit
        /// </summary>
        public short Ascender
        {
            get
            {

                return OS2Table.sTypoAscender;
            }
        }
        /// <summary>
        /// OS2 sTypoDescender, in font designed unit
        /// </summary>
        public short Descender
        {
            get
            {
                return OS2Table.sTypoDescender;
            }
        }
        /// <summary>
        /// OS2 Linegap
        /// </summary>
        public short LineGap
        {
            get
            {
                //The typographic line gap for this font.
                //Remember that this is not the same as the LineGap value in the 'hhea' table, 
                //which Apple defines in a far different manner.
                //The suggested usage for sTypoLineGap is 
                //that it be used in conjunction with unitsPerEm 
                //to compute a typographically correct default line spacing.
                //
                //Typical values average 7 - 10 % of units per em.
                //The goal is to free applications from Macintosh or Windows - specific metrics
                //which are constrained by backward compatability requirements
                //(see chapter, “Recommendations for OpenType Fonts”).
                //These new metrics, when combined with the character design widths,
                //will allow applications to lay out documents in a typographically correct and portable fashion. 
                //These metrics will be exposed through Windows APIs.
                //Macintosh applications will need to access the 'sfnt' resource and 
                //parse it to extract this data from the “OS / 2” table
                //(unless Apple exposes the 'OS/2' table through a new API)


                return OS2Table.sTypoLineGap;
            }
        }

        public string Name
        {
            get { return _nameEntry.FontName; }
        }
        public string FontSubFamily
        {
            get { return _nameEntry.FontSubFamily; }
        }

        /// <summary>
        /// find glyph index by codepoint
        /// </summary>
        /// <param name="codepoint"></param>
        /// <param name="nextCodepoint"></param>
        /// <returns></returns>
        public ushort LookupIndex(int codepoint, int nextCodepoint = 0)
        {
            return CmapTable.LookupIndex(codepoint, nextCodepoint);
        }

        public Glyph Lookup(int codepoint)
        {
            return _glyphs[LookupIndex(codepoint)];
        }
        public Glyph GetGlyphByIndex(ushort glyphIndex)
        {
            return _glyphs[glyphIndex];
        }
        public int GlyphCount
        {
            get { return _glyphs.Length; }
        }
        public Glyph GetGlyphByName(string glyphName)
        {

            if (_cffTable != null)
            {
                //early preview ...
                List<CFF.Cff1Font> cff1Fonts = _cffTable.Cff1FontSet._fonts;
                for (int i = 0; i < cff1Fonts.Count; i++)
                {
                    Glyph glyph = cff1Fonts[i].GetGlyphByName(glyphName);
                    if (glyph != null) return glyph;
                }
                return null;
            }
            else if (PostTable != null)
            {
                return GetGlyphByIndex(GetGlyphIndexByName(glyphName));
            }
            return null;
        }
        public ushort GetGlyphIndexByName(string glyphName)
        {
            if (_cffTable != null)
            {
                return GetGlyphByName(glyphName)?.GlyphIndex ?? 0;
            }
            else if (PostTable != null)
            {
                if (PostTable.Version == 2)
                {
                    return PostTable.GetGlyphIndex(glyphName);
                }
                else
                {
                    //check data from adobe glyph list 
                    //from the unicode value
                    //select glyph index   

                    //we use AdobeGlyphList
                    //from https://github.com/adobe-type-tools/agl-aglfn/blob/master/glyphlist.txt

                    //but user can provide their own map here...

                    return LookupIndex(AdobeGlyphList.GetUnicodeValueByGlyphName(glyphName));
                }
            }
            return 0;
        }


        public ushort GetAdvanceWidth(int codepoint)
        {
            return _horizontalMetrics.GetAdvanceWidth(LookupIndex(codepoint));
        }
        public ushort GetHAdvanceWidthFromGlyphIndex(ushort glyphIndex)
        {

            return _horizontalMetrics.GetAdvanceWidth(glyphIndex);
        }
        public short GetHFrontSideBearingFromGlyphIndex(ushort glyphIndex)
        {
            return _horizontalMetrics.GetLeftSideBearing(glyphIndex);
        }
        public short GetKernDistance(ushort leftGlyphIndex, ushort rightGlyphIndex)
        {
            return _kern.GetKerningDistance(leftGlyphIndex, rightGlyphIndex);
        }
        public Bounds Bounds { get { return _bounds; } }
        public ushort UnitsPerEm { get { return _unitsPerEm; } }
        public Glyph[] Glyphs { get { return _glyphs; } }


        const int pointsPerInch = 72;
        /// <summary>
        /// convert from point-unit value to pixel value
        /// </summary>
        /// <param name="targetPointSize"></param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public static float ConvPointsToPixels(float targetPointSize, int resolution = 96)
        {
            //http://stackoverflow.com/questions/139655/convert-pixels-to-points
            //points = pixels * 72 / 96
            //------------------------------------------------
            //pixels = targetPointSize * 96 /72
            //pixels = targetPointSize * resolution / pointPerInch
            return targetPointSize * resolution / pointsPerInch;
        }
        /// <summary>
        /// calculate scale to target pixel size based on current typeface's UnitsPerEm
        /// </summary>
        /// <param name="targetPixelSize">target font size in point unit</param>
        /// <returns></returns>
        public float CalculateScaleToPixel(float targetPixelSize)
        {
            //1. return targetPixelSize / UnitsPerEm
            return targetPixelSize / this.UnitsPerEm;
        }
        /// <summary>
        ///  calculate scale to target pixel size based on current typeface's UnitsPerEm
        /// </summary>
        /// <param name="targetPointSize">target font size in point unit</param>
        /// <param name="resolution"></param>
        /// <returns></returns>
        public float CalculateScaleToPixelFromPointSize(float targetPointSize, int resolution = 96)
        {
            //1. var sizeInPixels = ConvPointsToPixels(sizeInPointUnit);
            //2. return  sizeInPixels / UnitsPerEm
            return (targetPointSize * resolution / pointsPerInch) / this.UnitsPerEm;
        }

        internal BASE BaseTable { get; set; }
        internal GDEF GDEFTable { get; set; }

        public COLR COLRTable { get; set; }
        public CPAL CPALTable { get; set; }
        public GPOS GPOSTable { get; set; }
        public GSUB GSUBTable { get; set; }

        //-------------------------------------------------------



        //experiment
        internal void LoadOpenFontLayoutInfo(GDEF gdefTable, GSUB gsubTable, GPOS gposTable, BASE baseTable, COLR colrTable, CPAL cpalTable)
        {

            //***
            this.GDEFTable = gdefTable;
            this.GSUBTable = gsubTable;
            this.GPOSTable = gposTable;
            this.BaseTable = baseTable;
            this.COLRTable = colrTable;
            this.CPALTable = cpalTable;
            //---------------------------
            //1. fill glyph definition            
            if (gdefTable != null)
            {
                gdefTable.FillGlyphData(this.Glyphs);
                //if (this.Glyphs != null)
                //{

                //}
                //else if (this._cffTable != null)
                //{
                //    //post script outline
                //    //TODO: fill gdef for cff font

                //}

            }
        }


        //---------        
        internal PostTable PostTable { get; set; }
        internal bool _evalCffGlyphBounds;
        internal bool IsCffFont
        {
            get
            {
                return _cffTable != null;
            }
        }
        //---------
        internal MathTable _mathTable;
        internal MathGlyphs.MathGlyphInfo[] _mathGlyphInfos;
        internal Glyph[] GetRawGlyphList()
        {
            return _glyphs;
        }
        public MathGlyphs.MathConstants MathConsts
        {
            get
            {
                return (_mathTable != null) ? _mathTable._mathConstTable : null;
            }
        }
        //---------
        internal SvgTable _svgTable;

    }


    public interface IGlyphPositions
    {
        int Count { get; }

        GlyphClassKind GetGlyphClassKind(int index);
        void AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY);
        void AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY);

        ushort GetGlyph(int index, out ushort advW);
        ushort GetGlyph(int index, out ushort inputOffset, out short offsetX, out short offsetY, out short advW);
        //
        void GetOffset(int index, out short offsetX, out short offsetY);
    }


    public static class StringUtils
    {
        public static void FillWithCodepoints(List<int> codepoints, char[] str, int startAt = 0, int len = -1)
        {

            if (len == -1) len = str.Length;
            // this is important!
            // -----------------------
            //  from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
            // In many places, "char" is not a valid type to handle characters, because it
            // only supports 16 bits.In order to handle the full range of Unicode characters,
            // we need to use "int".
            // This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
            // though they are encoded as two "char"s in a C# string.
            for (int i = 0; i < len; ++i)
            {
                char ch = str[startAt + i];
                int codepoint = ch;
                if (char.IsHighSurrogate(ch) && i + 1 < len)
                {
                    char nextCh = str[startAt + i + 1];
                    if (char.IsLowSurrogate(nextCh))
                    {
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }
                codepoints.Add(codepoint);
            }
        }
        public static IEnumerable<int> GetCodepoints(char[] str, int startAt = 0, int len = -1)
        {
            if (len == -1) len = str.Length;
            // this is important!
            // -----------------------
            //  from @samhocevar's PR: (https://github.com/LayoutFarm/Typography/pull/56/commits/b71c7cf863531ebf5caa478354d3249bde40b96e)
            // In many places, "char" is not a valid type to handle characters, because it
            // only supports 16 bits.In order to handle the full range of Unicode characters,
            // we need to use "int".
            // This allows characters such as 🙌 or 𐐷 or to be treated as single codepoints even
            // though they are encoded as two "char"s in a C# string.
            for (int i = 0; i < len; ++i)
            {
                char ch = str[startAt + i];
                int codepoint = ch;
                if (char.IsHighSurrogate(ch) && i + 1 < len)
                {
                    char nextCh = str[startAt + i + 1];
                    if (char.IsLowSurrogate(nextCh))
                    {
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }
                yield return codepoint;
            }
        }
    }

    namespace Extensions
    {

        public static class TypefaceExtensions
        {


            public static bool DoesSupportUnicode(
                this Typeface typeface,
                UnicodeLangBits unicodeLangBits)
            {
                if (typeface.OS2Table == null)
                {
                    return false;
                }
                //-----------------------------
                long bits = (long)unicodeLangBits;
                int bitpos = (int)(bits >> 32);

                if (bitpos == 0)
                {
                    return true; //default
                }
                else if (bitpos < 32)
                {
                    //use range 1
                    return (typeface.OS2Table.ulUnicodeRange1 & (1 << bitpos)) != 0;
                }
                else if (bitpos < 64)
                {
                    return (typeface.OS2Table.ulUnicodeRange2 & (1 << (bitpos - 32))) != 0;
                }
                else if (bitpos < 96)
                {
                    return (typeface.OS2Table.ulUnicodeRange3 & (1 << (bitpos - 64))) != 0;
                }
                else if (bitpos < 128)
                {
                    return (typeface.OS2Table.ulUnicodeRange4 & (1 << (bitpos - 96))) != 0;
                }
                else
                {
                    throw new System.NotSupportedException();
                }
            }

            public static bool RecommendToUseTypoMetricsForLineSpacing(this Typeface typeface)
            {
                //https://www.microsoft.com/typography/otspec/os2.htm
                //
                //fsSelection ...
                //
                //bit     name                
                //7       USE_TYPO_METRICS   
                //  
                //        Description
                //        If set, it is strongly recommended to use
                //        OS/2.sTypoAscender - OS/2.sTypoDescender + OS/2.sTypoLineGap 
                //        as a value for default line spacing for this font.

                return ((typeface.OS2Table.fsSelection >> 7) & 1) != 0;
            }
            public static TranslatedOS2FontStyle TranslatedOS2FontStyle(this Typeface typeface)
            {
                return TranslatedOS2FontStyle(typeface.OS2Table);
            }

            internal static TranslatedOS2FontStyle TranslatedOS2FontStyle(OS2Table os2Table)
            {
                //@prepare's note, please note:=> this is not real value, this is 'translated' value from OS2.fsSelection 


                //https://www.microsoft.com/typography/otspec/os2.htm
                //Bit # 	macStyle bit 	C definition 	Description
                //0         bit 1           ITALIC          Font contains italic or oblique characters, otherwise they are upright.
                //1                         UNDERSCORE      Characters are underscored.
                //2                         NEGATIVE        Characters have their foreground and background reversed.
                //3                         OUTLINED        Outline(hollow) characters, otherwise they are solid.
                //4                         STRIKEOUT       Characters are overstruck.
                //5         bit 0           BOLD            Characters are emboldened.
                //6                         REGULAR Characters are in the standard weight / style for the font.
                //7                         USE_TYPO_METRICS    If set, it is strongly recommended to use OS / 2.sTypoAscender - OS / 2.sTypoDescender + OS / 2.sTypoLineGap as a value for default line spacing for this font.
                //8                         WWS     The font has ‘name’ table strings consistent with a weight / width / slope family without requiring use of ‘name’ IDs 21 and 22. (Please see more detailed description below.)
                //9                         OBLIQUE     Font contains oblique characters.
                //10–15 < reserved > Reserved; set to 0.
                ushort fsSelection = os2Table.fsSelection;
                TranslatedOS2FontStyle result = Extensions.TranslatedOS2FontStyle.UNSET;

                if ((fsSelection & 0x1) != 0)
                {

                    result |= Extensions.TranslatedOS2FontStyle.ITALIC;
                }

                if (((fsSelection >> 5) & 0x1) != 0)
                {
                    result |= Extensions.TranslatedOS2FontStyle.BOLD;
                }

                if (((fsSelection >> 6) & 0x1) != 0)
                {
                    result |= Extensions.TranslatedOS2FontStyle.REGULAR;
                }
                if (((fsSelection >> 9) & 0x1) != 0)
                {
                    result |= Extensions.TranslatedOS2FontStyle.OBLIQUE;
                }

                return result;
            }


            /// <summary>
            /// overall calculated line spacing 
            /// </summary>
            static int Calculate_TypoMetricLineSpacing(Typeface typeface)
            {

                //from https://www.microsoft.com/typography/OTSpec/recom.htm#tad
                //sTypoAscender, sTypoDescender and sTypoLineGap
                //sTypoAscender is used to determine the optimum offset from the top of a text frame to the first baseline.
                //sTypoDescender is used to determine the optimum offset from the last baseline to the bottom of the text frame. 
                //The value of (sTypoAscender - sTypoDescender) is recommended to equal one em.
                //
                //While the OpenType specification allows for CJK (Chinese, Japanese, and Korean) fonts' sTypoDescender and sTypoAscender 
                //fields to specify metrics different from the HorizAxis.ideo and HorizAxis.idtp baselines in the 'BASE' table,
                //CJK font developers should be aware that existing applications may not read the 'BASE' table at all but simply use 
                //the sTypoDescender and sTypoAscender fields to describe the bottom and top edges of the ideographic em-box. 
                //If developers want their fonts to work correctly with such applications, 
                //they should ensure that any ideographic em-box values in the 'BASE' table describe the same bottom and top edges as the sTypoDescender and
                //sTypoAscender fields. 
                //See the sections “OpenType CJK Font Guidelines“ and ”Ideographic Em-Box“ for more details.

                //For Western fonts, the Ascender and Descender fields in Type 1 fonts' AFM files are a good source of sTypoAscender
                //and sTypoDescender, respectively. 
                //The Minion Pro font family (designed on a 1000-unit em), 
                //for example, sets sTypoAscender = 727 and sTypoDescender = -273.

                //sTypoAscender, sTypoDescender and sTypoLineGap specify the recommended line spacing for single-spaced horizontal text.
                //The baseline-to-baseline value is expressed by:
                //OS/2.sTypoAscender - OS/2.sTypoDescender + OS/2.sTypoLineGap




                //sTypoLineGap will usually be set by the font developer such that the value of the above expression is approximately 120% of the em.
                //The application can use this value as the default horizontal line spacing. 
                //The Minion Pro font family (designed on a 1000-unit em), for example, sets sTypoLineGap = 200.


                return typeface.Ascender - typeface.Descender + typeface.LineGap;

            }

            /// <summary>
            /// calculate Baseline-to-Baseline Distance (BTBD) for Windows
            /// </summary>
            /// <param name="typeface"></param>
            /// <returns>return 'unscaled-to-pixel' BTBD value</returns>
            static int Calculate_BTBD_Windows(Typeface typeface)
            {

                //from https://www.microsoft.com/typography/otspec/recom.htm#tad

                //Baseline to Baseline Distances
                //The 'OS/2' table fields sTypoAscender, sTypoDescender, and sTypoLineGap 
                //free applications from Macintosh-or Windows - specific metrics
                //which are constrained by backward compatibility requirements.
                //
                //The following discussion only pertains to the platform-specific metrics.
                //The suggested Baseline to Baseline Distance(BTBD) is computed differently for Windows and the Macintosh,
                //and it is based on different OpenType metrics.
                //However, if the recommendations below are followed, the BTBD will be the same for both Windows and the Mac.

                //Windows Metric         OpenType Metric
                //ascent                    usWinAscent
                //descent                   usWinDescent
                //internal leading          usWinAscent + usWinDescent - unitsPerEm
                //external leading          MAX(0, LineGap - ((usWinAscent + usWinDescent) - (Ascender - Descender)))

                //The suggested BTBD = ascent + descent + external leading

                //It should be clear that the “external leading” can never be less than zero. 
                //Pixels above the ascent or below the descent will be clipped from the character; 
                //this is true for all output devices.

                //The usWinAscent and usWinDescent are values 
                //from the 'OS/2' table.
                //The unitsPerEm value is from the 'head' table.
                //The LineGap, Ascender and Descender values are from the 'hhea' table.

                int usWinAscent = typeface.OS2Table.usWinAscent;
                int usWinDescent = typeface.OS2Table.usWinDescent;
                int internal_leading = usWinAscent + usWinDescent - typeface.UnitsPerEm;
                HorizontalHeader hhea = typeface.HheaTable;
                int external_leading = System.Math.Max(0, hhea.LineGap - ((usWinAscent + usWinDescent) - (hhea.Ascent - hhea.Descent)));
                return usWinAscent + usWinDescent + external_leading;
            }
            /// <summary>
            /// calculate Baseline-to-Baseline Distance (BTBD) for macOS
            /// </summary>
            /// <param name="typeface"></param>
            /// <returns>return 'unscaled-to-pixel' BTBD value</returns>
            static int CalculateBTBD_Mac(Typeface typeface)
            {
                //from https://www.microsoft.com/typography/otspec/recom.htm#tad

                //Ascender and Descender are metrics defined by Apple 
                //and are not to be confused with the Windows ascent or descent, 
                //nor should they be confused with the true typographic ascender and descender that are found in AFM files.
                //The Macintosh metrics below are returned by the Apple Advanced Typography(AAT) GetFontInfo() API.
                //
                //
                //Macintosh Metric      OpenType Metric
                //ascender                  Ascender
                //descender                 Descender
                //leading                   LineGap

                //The suggested BTBD = ascent + descent + leading
                //If pixels extend above the ascent or below the descent, 
                //the character will be squashed in the vertical direction 
                //so that all pixels fit within these limitations; this is true for screen display only.

                //TODO: please test this
                HorizontalHeader hhea = typeface.HheaTable;
                return hhea.Ascent + hhea.Descent + hhea.LineGap;
            }

            public static int CalculateRecommendLineSpacing(this Typeface typeface, out LineSpacingChoice choice)
            {
                //check if we are on Windows env or macOS eve
                if (RecommendToUseTypoMetricsForLineSpacing(typeface))
                {
                    choice = LineSpacingChoice.TypoMetric;
                    return Calculate_TypoMetricLineSpacing(typeface);
                }
                else
                {
                    //check if we are on Windows or mac 
                    if (CurrentEnv.CurrentOSName == CurrentOSName.Mac)
                    {
                        choice = LineSpacingChoice.Mac;
                        return CalculateBTBD_Mac(typeface);
                    }
                    else
                    {
                        choice = LineSpacingChoice.Windows;
                        return Calculate_BTBD_Windows(typeface);
                    }
                }
            }
            public static int CalculateRecommendLineSpacing(this Typeface typeface)
            {
                LineSpacingChoice selectedChoice;
                return CalculateRecommendLineSpacing(typeface, out selectedChoice);
            }


            public static int CalculateLineSpacing(this Typeface typeface, LineSpacingChoice choice)
            {
                switch (choice)
                {
                    default:
                    case LineSpacingChoice.Windows:
                        return Calculate_BTBD_Windows(typeface);
                    case LineSpacingChoice.Mac:
                        return CalculateBTBD_Mac(typeface);
                    case LineSpacingChoice.TypoMetric:
                        return Calculate_TypoMetricLineSpacing(typeface);
                }
            }

        }
        public enum LineSpacingChoice
        {
            TypoMetric,
            Windows,
            Mac
        }
        public enum CurrentOSName
        {
            None,//not evaluate yet
            Windows,
            Mac,
            Others
        }


        [System.Flags]
        public enum TranslatedOS2FontStyle : ushort
        {

            //@prepare's note, please note:=> this is not real value, this is 'translated' value from OS2.fsSelection 

            UNSET = 0,

            ITALIC = 1,
            BOLD = 1 << 1,
            REGULAR = 1 << 2,
            OBLIQUE = 1 << 3,
        }

        public static class CurrentEnv
        {
            public static CurrentOSName CurrentOSName;
        }
    }


    public struct GlyphNameMap
    {
        public readonly ushort glyphIndex;
        public readonly string glyphName;
        public GlyphNameMap(ushort glyphIndex, string glyphName)
        {
            this.glyphIndex = glyphIndex;
            this.glyphName = glyphName;
        }
    }

    public static class TypefaceExtension2
    {


        public static IEnumerable<GlyphNameMap> GetGlyphNameIter(this Typeface typeface)
        {
            if (typeface.IsCffFont)
            {
                CFF.Cff1Font cff1Font = typeface.CffTable.Cff1FontSet._fonts[0];
                foreach (var kp in cff1Font.GetGlyphNameIter())
                {
                    yield return kp;
                }
            }
            else if (typeface.PostTable.Version == 2)
            {
                //version 1 and 3 => no glyph names

                foreach (var kp in typeface.PostTable.GlyphNames)
                {
                    yield return new GlyphNameMap(kp.Key, kp.Value);
                }
            }
        }

        public static bool HasMathTable(this Typeface typeface)
        {
            return typeface.MathConsts != null;
        }
        public static bool HasSvgTable(this Typeface typeface)
        {
            return typeface._svgTable != null;
        }

        class CffBoundFinder : IGlyphTranslator
        {

            float _minX, _maxX, _minY, _maxY;
            float _curX, _curY;
            float _latestMove_X, _latestMove_Y;
            /// <summary>
            /// curve flatten steps  => this a copy from Typography.Contours's GlyphPartFlattener
            /// </summary>
            int nsteps = 3;
            bool _contourOpen = false;
            bool _first_eval = true;
            public CffBoundFinder()
            {

            }
            public void Reset()
            {
                _curX = _curY = _latestMove_X = _latestMove_Y = 0;
                _minX = _minY = float.MaxValue;//**
                _maxX = _maxY = float.MinValue;//**
                _first_eval = true;
                _contourOpen = false;
            }
            public void BeginRead(int contourCount)
            {

            }
            public void EndRead()
            {

            }
            public void CloseContour()
            {
                _contourOpen = false;
                _curX = _latestMove_X;
                _curY = _latestMove_Y;
            }
            public void Curve3(float x1, float y1, float x2, float y2)
            {

                //this a copy from Typography.Contours -> GlyphPartFlattener

                float eachstep = (float)1 / nsteps;
                float t = eachstep;//start

                for (int n = 1; n < nsteps; ++n)
                {
                    float c = 1.0f - t;

                    UpdateMinMax(
                         (c * c * _curX) + (2 * t * c * x1) + (t * t * x2),  //x
                         (c * c * _curY) + (2 * t * c * y1) + (t * t * y2)); //y

                    t += eachstep;
                }

                //
                UpdateMinMax(
                    _curX = x2,
                    _curY = y2);

                _contourOpen = true;
            }

            public void Curve4(float x1, float y1, float x2, float y2, float x3, float y3)
            {

                //this a copy from Typography.Contours -> GlyphPartFlattener


                float eachstep = (float)1 / nsteps;
                float t = eachstep;//start

                for (int n = 1; n < nsteps; ++n)
                {
                    float c = 1.0f - t;

                    UpdateMinMax(
                        (_curX * c * c * c) + (x1 * 3 * t * c * c) + (x2 * 3 * t * t * c) + x3 * t * t * t,  //x
                        (_curY * c * c * c) + (y1 * 3 * t * c * c) + (y2 * 3 * t * t * c) + y3 * t * t * t); //y

                    t += eachstep;
                }
                //
                UpdateMinMax(
                    _curX = x3,
                    _curY = y3);

                _contourOpen = true;
            }
            public void LineTo(float x1, float y1)
            {
                UpdateMinMax(
                    _curX = x1,
                    _curY = y1);

                _contourOpen = true;
            }
            public void MoveTo(float x0, float y0)
            {

                if (_contourOpen)
                {
                    CloseContour();
                }

                UpdateMinMax(
                    _curX = x0,
                    _curY = y0);
            }
            void UpdateMinMax(float x0, float y0)
            {

                if (_first_eval)
                {
                    //4 times

                    if (x0 < _minX)
                    {
                        _minX = x0;
                    }
                    //
                    if (x0 > _maxX)
                    {
                        _maxX = x0;
                    }
                    //
                    if (y0 < _minY)
                    {
                        _minY = y0;
                    }
                    //
                    if (y0 > _maxY)
                    {
                        _maxY = y0;
                    }

                    _first_eval = false;
                }
                else
                {
                    //2 times

                    if (x0 < _minX)
                    {
                        _minX = x0;
                    }
                    else if (x0 > _maxX)
                    {
                        _maxX = x0;
                    }

                    if (y0 < _minY)
                    {
                        _minY = y0;
                    }
                    else if (y0 > _maxY)
                    {
                        _maxY = y0;
                    }
                }

            }

            public Bounds GetResultBounds()
            {
                return new Bounds(
                    (short)System.Math.Floor(_minX),
                    (short)System.Math.Floor(_minY),
                    (short)System.Math.Ceiling(_maxX),
                    (short)System.Math.Ceiling(_maxY));
            }


        }
        public static void UpdateAllCffGlyphBounds(this Typeface typeface)
        {

            if (typeface.IsCffFont && !typeface._evalCffGlyphBounds)
            {
                int j = typeface.GlyphCount;
                CFF.CffEvaluationEngine evalEngine = new CFF.CffEvaluationEngine();
                CffBoundFinder boundFinder = new CffBoundFinder();
                for (ushort i = 0; i < j; ++i)
                {
#if DEBUG

                    //if (i == 3084)
                    //{

                    //}
#endif
                    Glyph g = typeface.GetGlyphByIndex(i);
                    boundFinder.Reset();

                    evalEngine.Run(boundFinder,
                        g._ownerCffFont,
                        g._cff1GlyphData.GlyphInstructions);

                    g.Bounds = boundFinder.GetResultBounds();
                }
                typeface._evalCffGlyphBounds = true;
            }
        }





    }
}
