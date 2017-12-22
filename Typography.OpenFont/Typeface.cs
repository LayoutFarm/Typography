//Apache2, 2017, WinterDev
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

            //---------------------------------------------------
            //cmap - Character To Glyph Index Mapping Table
            //---------------------------------------------------
            //This table defines the mapping of character codes to the glyph index values used in the font. It may contain more than one subtable, in order to support more than one character encoding scheme.Character codes that do not correspond to any glyph in the font should be mapped to glyph index 0.The glyph at this location must be a special glyph representing a missing character, commonly known as .notdef.
            //The table header indicates the character encodings for which subtables are present.Each subtable is in one of seven possible formats and begins with a format code indicating the format used.
            //The platform ID and platform - specific encoding ID in the header entry(and, in the case of the Macintosh platform, the language field in the subtable itself) are used to specify a particular 'cmap' encoding.The header entries must be sorted first by platform ID, then by platform - specific encoding ID, and then by the language field in the corresponding subtable.Each platform ID, platform - specific encoding ID, and subtable language combination may appear only once in the 'cmap' table.
            //When building a Unicode font for Windows, the platform ID should be 3 and the encoding ID should be 1.When building a symbol font for Windows, the platform ID should be 3 and the encoding ID should be 0.When building a font that will be used on the Macintosh, the platform ID should be 1 and the encoding ID should be 0.
            //All Microsoft Unicode BMP encodings(Platform ID = 3, Encoding ID = 1) must provide at least a Format 4 'cmap' subtable.If the font is meant to support supplementary(non - BMP) Unicode characters, it will additionally need a Format 12 subtable with a platform encoding ID 10.The contents of the Format 12 subtable need to be a superset of the contents of the Format 4 subtable.Microsoft strongly recommends using a BMP Unicode 'cmap' for all fonts. However, some other encodings that appear in current fonts follow:
            //Windows Encodings
            //Platform ID Encoding ID Description
            //3   0   Symbol
            //3   1   Unicode BMP(UCS - 2)
            //3   2   ShiftJIS
            //3   3   PRC
            //3   4   Big5
            //3   5   Wansung
            //3   6   Johab
            //3   7   Reserved
            //3   8   Reserved
            //3   9   Reserved
            //3   10  Unicode UCS - 4
            //---------------------------------------------------
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

        public ushort LookupIndex(int codepoint, int nextCodepoint = 0)
        {
            return CmapTable.LookupIndex(codepoint, nextCodepoint);
        }

        public Glyph Lookup(int codepoint)
        {
            return _glyphs[LookupIndex(codepoint)];
        }
        public Glyph GetGlyphByIndex(int glyphIndex)
        {
            return _glyphs[glyphIndex];
        }

        public ushort GetAdvanceWidth(int codepoint)
        {
            return _horizontalMetrics.GetAdvanceWidth(LookupIndex(codepoint));
        }
        public ushort GetHAdvanceWidthFromGlyphIndex(int glyphIndex)
        {

            return _horizontalMetrics.GetAdvanceWidth(glyphIndex);
        }
        public short GetHFrontSideBearingFromGlyphIndex(int glyphIndex)
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

        public void Lookup(char[] buffer, List<int> output)
        {
            //do shaping here?
            //1. do look up and substitution 
            for (int i = 0; i < buffer.Length; ++i)
            {
                char ch = buffer[i];
                int codepoint = ch;
                if (ch >= 0xd800 && ch <= 0xdbff && i + 1 < buffer.Length)
                {
                    char nextCh = buffer[i + 1];
                    if (nextCh >= 0xdc00 && nextCh <= 0xdfff)
                    {
                        ++i;
                        codepoint = char.ConvertToUtf32(ch, nextCh);
                    }
                }

                output.Add(LookupIndex(codepoint));
            }
            //tmp disable here
            //check for glyph substitution
            //this.GSUBTable.CheckSubstitution(output[1]);
        }
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
            }
        }
    }


    public interface IGlyphPositions
    {
        int Count { get; }

        GlyphClassKind GetGlyphClassKind(int index);
        void AppendGlyphOffset(int index, short appendOffsetX, short appendOffsetY);
        void AppendGlyphAdvance(int index, short appendAdvX, short appendAdvY);

        ushort GetGlyph(int index, out ushort advW);
        ushort GetGlyph(int index, out short offsetX, out short offsetY, out short advW);
        //
        void GetOffset(int index, out short offsetX, out short offsetY);
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

        public static class CurrentEnv
        {
            public static CurrentOSName CurrentOSName;
        }
    }
}
