//Apache2, 2016-present, WinterDev 
using System.Collections.Generic;
namespace Typography.OpenFont
{
    public sealed class ScriptLang
    {
        public readonly string fullname;
        public readonly string shortname;
        public readonly int internalName;
        internal ScriptLang(string fullname, string shortname, int internalName)
        {
            this.fullname = fullname;
            this.shortname = shortname;
            this.internalName = internalName;
        }
        public override string ToString()
        {
            return this.fullname;
        }
    }
    public static class UnicodeLangBitsExtension
    {
        public static UnicodeRangeInfo ToUnicodeRangeInfo(this UnicodeLangBits unicodeLangBits)
        {
            long bits = (long)unicodeLangBits;
            int bitpos = (int)(bits >> 32);
            uint lower32 = (uint)(bits & 0xFFFFFFFF);
            return new UnicodeRangeInfo(bitpos,
                (int)(lower32 >> 16),
                 (int)(lower32 & 0xFFFF));
        }
    }

    //unicode range 
    public struct UnicodeRangeInfo
    {
        public readonly int BitNo;
        public readonly int StartAt;
        public readonly int EndAt;
        public UnicodeRangeInfo(int bitNo, int startAt, int endAt)
        {
            BitNo = bitNo;
            StartAt = startAt;
            EndAt = endAt;
        }

        public bool IsInRange(int value)
        {
            return (value >= StartAt) && (value <= EndAt);
        }

#if DEBUG
        public override string ToString()
        {
            return BitNo + ",[" + StartAt + "," + EndAt + "]";
        }
#endif
    }



    public enum UnicodeLangBits : long
    {
        Unset = 0,

        //Bit Unicode               Range 	Block range
        //0   Basic Latin 	        0000-007F
        //1   Latin-1 Supplement 	0080-00FF
        //2   Latin Extended-A 	    0100-017F
        //3   Latin Extended-B 	    0180-024F
        BasicLatin = (0L << 32) | (0x0000 << 16) | 0x007F,
        Latin1Supplement = (1L << 32) | (0x0080 << 16) | 0x00FF,
        LatinExtendedA = (2L << 32) | (0x0100 << 16) | 0x017F,
        LatinExtendedB = (3L << 32) | (0x0180 << 16) | 0x024F,
        //4   IPA Extensions 	    0250-02AF
        //    Phonetic Extensions 	1D00-1D7F
        //    Phonetic Extensions Supplement 	1D80-1DBF
        IPAExtensions = (4L << 32) | (0x0250 << 16) | 0x02AF,
        PhoneticExtensions = (4L << 32) | (0x1D00 << 16) | 0x1D7F,
        PhoneticExtensionsSupplement = (4L << 32) | (0x1D80 << 16) | 0x1DBF,
        //5   Spacing Modifier Letters 	02B0-02FF
        //    Modifier Tone Letters 	A700-A71F
        SpacingModifierLetters = (5L << 32) | (0x02B0 << 16) | 0x02FF,
        ModifierToneLetters = (5L << 32) | (0xA700L << 16) | 0xA71F,
        //6   Combining Diacritical Marks 	0300-036F
        //    Combining Diacritical Marks Supplement 	1DC0-1DFF
        CombiningDiacriticalMarks = (6L << 32) | (0x0300 << 16) | 0x036F,
        CombiningDiacriticalMarksSupplement = (6L << 32) | (0x1DC0 << 16) | 0x1DFF,
        //7   Greek and Coptic 	0370-03FF
        GreekAndCoptic = (7L << 32) | (0x0370 << 16) | 0x03FF,
        //8   Coptic 	2C80-2C80
        Coptic = (8L << 32) | (0x2C80 << 16) | 0x2C80,
        //9   Cyrillic 	0400-04FF
        //    Cyrillic Supplement 	0500-052F
        //    Cyrillic Extended-A 	2DE0-2DFF
        //    Cyrillic Extended-B 	A640-A69F
        Cyrillic = (9L << 32) | (0x0400 << 16) | 0x04FF,
        CyrillicExtendedA = (9L << 32) | (0x2DE0 << 16) | 0x2DFF,
        CyrillicExtendedB = (9L << 32) | (0xA640L << 16) | 0xA69F,
        //10 	Armenian 	0530-058F
        Armenian = (10L << 32) | (0x0530 << 16) | 0x058F,
        //11 	Hebrew 	0590-05FF
        Hebrew = (11L << 32) | (0x0590 << 16) | 0x05FF,
        //12 	Vai 	A500-A63F
        Vai = (11L << 32) | (0xA500L << 16) | 0xA63F,
        //13 	Arabic 	0600-06FF
        //    Arabic Supplement 	0750-077F
        Arabic = (13L << 32) | (0x0600 << 16) | 0x06FF,
        ArabicSupplement = (13L << 32) | (0x0750 << 16) | 0x077F,
        //14 	NKo 	07C0-07FF
        NKo = (14L << 32) | (0x07C0 << 16) | 0x07FF,
        //15 	Devanagari 	0900-097F
        Devanagari = (15L << 32) | (0x0900 << 16) | 0x097F,
        //16 	Bengali 	0980-09FF
        Bengali = (16L << 32) | (0x0980 << 16) | 0x09FF,
        //17 	Gurmukhi 	0A00-0A7F
        Gurmukhi = (17L << 32) | (0x0A00 << 16) | 0x0A7F,
        //18 	Gujarati 	0A80-0AFF
        Gujarati = (18L << 32) | (0x0A80 << 16) | 0x0AFF,
        //19 	Oriya 	0B00-0B7F
        Oriya = (19L << 32) | (0x0B00 << 16) | 0x0B7F,
        //20 	Tamil 	0B80-0BFF
        Tamil = (20L << 32) | (0x0B80 << 16) | 0x0BFF,
        //21 	Telugu 	0C00-0C7F
        Telugu = (21L << 32) | (0x0C00 << 16) | 0x0C7F,
        //22 	Kannada 	0C80-0CFF
        Kannada = (22L << 32) | (0x0C80 << 16) | 0x0CFF,
        //23 	Malayalam 	0D00-0D7F
        Malayalam = (23L << 32) | (0x0D00 << 16) | 0x0D7F,
        //24 	Thai 	0E00-0E7F
        Thai = (24L << 32) | (0x0E00 << 16) | 0x0E7F,
        //25 	Lao 	0E80-0EFF
        Lao = (25L << 32) | (0x0E80 << 16) | 0x0EFF,
        //26 	Georgian 	10A0-10FF
        //    Georgian Supplement 	2D00-2D2F
        Georgian = (26L << 32) | (0x10A0 << 16) | 0x10FF,
        GeorgianSupplement = (26L << 32) | (0x2D00 << 16) | 0x2D2F,
        //27 	Balinese 	1B00-1B7F
        Balinese = (27L << 32) | (0x1B00 << 16) | 0x1B7F,
        //28 	Hangul Jamo 	1100-11FF
        HangulJamo = (28L << 32) | (0x1100 << 16) | 0x11FF,
        //29 	Latin Extended Additional 	1E00-1EFF
        //    Latin Extended-C 	2C60-2C7F
        //    Latin Extended-D 	A720-A7FF
        LatinExtendedAdditional = (29L << 32) | (0x1E00 << 16) | 0x1EFF,
        LatinExtendedAdditionalC = (29L << 32) | (0x2C60 << 16) | 0x2C7F,
        LatinExtendedAdditionalD = (29L << 32) | (0xA720L << 16) | 0xA7FF,
        //---
        //30 	Greek Extended 	1F00-1FFF
        GreekExtended = (30L << 32) | (0x1F00 << 16) | 0x1FFF,
        //31 	General Punctuation 	2000-206F
        //    Supplemental Punctuation 	2E00-2E7F
        GeneralPunctuation = (31L << 32) | (0x2000 << 16) | 0x206F,
        SupplementPunctuation = (31L << 32) | (0x2E00 << 16) | 0x2E7F,
        //32 	Superscripts And Subscripts 	2070-209F
        Superscripts_And_Subscripts = (32L << 32) | (0x2070 << 16) | 0x209F,
        //33 	Currency Symbols 	20A0-20CF
        Currency_Symbols = (33L << 32) | (0x20A0 << 16) | 0x20CF,
        //34 	Combining Diacritical Marks For Symbols 	20D0-20FF
        Combining_Diacritical_Marks_For_Symbols = (34L << 32) | (0x20D0 << 16) | 0x20FF,
        //35 	Letterlike Symbols 	2100-214F
        Letterlike_Symbols = (35L << 32) | (0x2100 << 16) | 0x214F,
        //36 	Number Forms 	2150-218F
        Number_Forms = (36L << 32) | (0x2150 << 16) | 0x218F,
        //37 	Arrows 	2190-21FF
        //      Supplemental Arrows-A 	27F0-27FF
        //      Supplemental Arrows-B 	2900-297F
        //      Miscellaneous Symbols and Arrows 	2B00-2BFF
        Arrows = (37L << 32) | (0x2190 << 16) | 0x21FF,
        Supplemental_Arrows_A = (37L << 32) | (0x27F0 << 16) | 0x27FF,
        Supplemental_Arrows_B = (37L << 32) | (0x2900 << 16) | 0x297F,
        Miscellaneous_Symbols_and_Arrows = (37L << 32) | (0x2B00 << 16) | 0x2BFF,
        //38 	Mathematical Operators 	2200-22FF
        //    Supplemental Mathematical Operators 	2A00-2AFF
        //    Miscellaneous Mathematical Symbols-A 	27C0-27EF
        //    Miscellaneous Mathematical Symbols-B 	2980-29FF
        Mathematical_Operators = (38L << 32) | (0x2200 << 16) | 0x22FF,
        Supplemental_Mathematical_Operators = (38L << 32) | (0x2A00 << 16) | 0x2AFF,
        Miscellaneous_Mathematical_Symbols_A = (38L << 32) | (0x27C0 << 16) | 0x27EF,
        Miscellaneous_Mathematical_Symbols_B = (38L << 32) | (0x2980 << 16) | 0x29FF,
        //39 	Miscellaneous Technical 	2300-23FF
        Miscellaneous_Technical = (39L << 32) | (0x2300 << 16) | 0x23FF,
        //40 	Control Pictures 	2400-243F
        Control_Pictures = (40L << 32) | (0x2400 << 16) | 0x243F,
        //41 	Optical Character Recognition 	2440-245F
        Optical_Character_Recognition = (41L << 32) | (0x2440 << 16) | 0x245F,
        //42 	Enclosed Alphanumerics 	2460-24FF
        Enclose_Alphanumerics = (42L << 32) | (0x2460 << 16) | 0x24FF,
        //43 	Box Drawing 	2500-257F
        Box_Drawing = (43L << 32) | (0x2500 << 16) | 0x257F,
        //44 	Block Elements 	2580-259F
        Block_Elements = (44L << 32) | (0x2580 << 16) | 0x259F,
        //45 	Geometric Shapes 	25A0-25FF
        Geometric_Shapes = (45L << 32) | (0x2580 << 16) | 0x259F,
        //46 	Miscellaneous Symbols 	2600-26FF
        Miscellaneous_Symbols = (46L << 32) | (0x2600 << 16) | 0x26FF,
        //47 	Dingbats 	2700-27BF
        Dingbats = (47L << 32) | (0x2700 << 16) | 0x27BF,
        //48 	CJK Symbols And Punctuation 	3000-303F
        CJK_Symbols_And_unctuation = (48L << 32) | (0x3000 << 16) | 0x303F,
        //49 	Hiragana 	3040-309F
        Hiragana = (49L << 32) | (0x3040 << 16) | 0x309F,
        //50 	Katakana 	30A0-30FF
        //      Katakana Phonetic Extensions 	31F0-31FF
        Katakana = (50L << 32) | (0x30A0 << 16) | 0x30FF,
        Katakana_Phonetic_Extensions = (50L << 32) | (0x31F0 << 16) | 0x31FF,
        //51 	Bopomofo 	3100-312F
        //      Bopomofo Extended 	31A0-31BF
        Bopomofo = (51L << 32) | (0x3100 << 16) | 0x312F,
        Bopomofo_Extended = (51L << 32) | (0x31A0 << 16) | 0x31BF,
        //52 	Hangul Compatibility Jamo 	3130-318F
        Hangul_Compatibility_Jamo = (52L << 32) | (0x3130 << 16) | 0x318F,
        //53 	Phags-pa 	A840-A87F
        Phags_pa = (53L << 32) | (0xA840L << 16) | 0xA87F,
        //54 	Enclosed CJK Letters And Months 	3200-32FF
        Enclosed_CJK_Letters_And_Months = (54L << 32) | (0x3200 << 16) | 0x32FF,
        //55 	CJK Compatibility 	3300-33FF
        CJK_Compatibility = (55L << 32) | (0x3300 << 16) | 0x33FF,
        //56 	Hangul Syllables 	AC00-D7AF
        Hangul_Syllables = (56L << 32) | (0xAC00L << 16) | 0xD7AF,
        //57 	Non-Plane 0 * 	D800-DFFF
        //(* Setting bit 57 implies that there is at least one codepoint beyond the Basic Multilingual Plane that is supported by this font.)
        Non_Plane_0 = (57L << 32) | (0xD800L << 16) | 0xDFFF,
        //58 	Phoenician 	10900-1091F
        Phoenician = (58L << 32) | (0x10900 << 16) | 0x1091F,
        //59 	CJK Unified Ideographs 	4E00-9FFF
        //    CJK Radicals Supplement 	2E80-2EFF
        //    Kangxi Radicals 	2F00-2FDF
        //    Ideographic Description Characters 	2FF0-2FFF
        //    CJK Unified Ideographs Extension A 	3400-4DBF
        //    CJK Unified Ideographs Extension B 	20000-2A6DF
        //    Kanbun 	3190-319F
        CJK_Unified_Ideographs = (59L << 32) | (0x4E00 << 16) | 0x9FFF,
        CJK_Radicals_Supplement = (59L << 32) | (0x2E80 << 16) | 0x2EFF,
        Kangxi_Radicals = (59L << 32) | (0x2F00 << 16) | 0x2FDF,
        Ideographic_Description_Characters = (59L << 32) | (0x2FF0 << 16) | 0x2FFF,
        CJK_Unified_Ideographs_Extension_A = (59L << 32) | (0x3400 << 16) | 0x4DBF,
        CJK_Unified_Ideographs_Extension_B = (59L << 32) | (0x20000 << 16) | 0x2A6DF,
        Kanbun = (59L << 32) | (0x3190 << 16) | 0x319F,
        //60 	Private Use Area (plane 0) 	E000-F8FF
        Private_Use_Area_Plane0 = (60L << 32) | (0xE000L << 16) | 0xF8FF,
        //61 	CJK Strokes 	31C0-31EF
        //    CJK Compatibility Ideographs 	F900-FAFF
        //    CJK Compatibility Ideographs Supplement 	2F800-2FA1F
        CJK_Strokes = (61L << 32) | (0x31C0 << 16) | 0x31EF,
        CJK_Compatibility_Ideographs = (61L << 32) | (0xF900L << 16) | 0xFAFF,
        CJK_Compatibility_Ideographs_Supplement = (61L << 32) | (0x2F800L << 16) | 0x2FA1F,
        //62 	Alphabetic Presentation Forms 	FB00-FB4F
        Alphabetic_Presentation_Forms = (62L << 32) | (0xFB00L << 16) | 0xFB4F,
        //63 	Arabic Presentation Forms-A 	FB50-FDFF
        Arabic_Presentation_Forms_A = (63L << 32) | (0xFB50L << 16) | 0xFDFF,
        //64 	Combining Half Marks 	FE20-FE2F
        Combining_Half_Marks = (64L << 32) | (0xFE20L << 16) | 0xFE2F,
        //65 	Vertical Forms 	FE10-FE1F
        //      CJK Compatibility Forms 	FE30-FE4F
        Vertical_Forms = (65L << 32) | (0xFE10L << 16) | 0xFE1F,
        CJK_Compatibility_Forms = (65L << 32) | (0xFE30L << 16) | 0xFE4F,
        //66 	Small Form Variants 	FE50-FE6F
        Small_Form_Variants = (66L << 32) | (0xFE50L << 16) | 0xFE6F,
        //67 	Arabic Presentation Forms-B 	FE70-FEFF
        Arabic_Presentation_Forms_B = (67L << 32) | (0xFE70L << 16) | 0xFEFF,
        //68 	Halfwidth And Fullwidth Forms 	FF00-FFEF
        Halfwidth_And_Fullwidth_Forms = (68L << 32) | (0xFF00L << 16) | 0xFFEF,
        //69 	Specials 	FFF0-FFFF
        Specials = (69L << 32) | (0xFFF0L << 16) | 0xFFFF,
        //70 	Tibetan 	0F00-0FFF
        Tibetan = (70L << 32) | (0x0F00 << 16) | 0x0FFF,
        //71 	Syriac 	0700-074F
        Syriac = (71L << 32) | (0x0700 << 16) | 0x074F,
        //72 	Thaana 	0780-07BF
        Thaana = (72L << 32) | (0x0780 << 16) | 0x07BF,
        //73 	Sinhala 	0D80-0DFF
        Sinhala = (73L << 32) | (0x0D80 << 16) | 0x0DFF,
        //74 	Myanmar 	1000-109F
        Myanmar = (74L << 32) | (0x1000 << 16) | 0x109F,
        //75 	Ethiopic 	1200-137F
        //    Ethiopic Supplement 	1380-139F
        //    Ethiopic Extended 	2D80-2DDF
        Ethiopic = (75L << 32) | (0x1200 << 16) | 0x137F,
        Ethiopic_Supplement = (75L << 32) | (0x1380 << 16) | 0x139F,
        Ethiopic_Extended = (75L << 32) | (0x2D80 << 16) | 0x2DDF,
        //76 	Cherokee 	13A0-13FF
        Cherokee = (76L << 32) | (0x13A0 << 16) | 0x13FF,
        //77 	Unified Canadian Aboriginal Syllabics 	1400-167F
        Unified_Canadian_Aboriginal_Syllabics = (77L << 32) | (0x1400 << 16) | 0x167F,
        //78 	Ogham 	1680-169F
        Ogham = (78L << 32) | (0x1680 << 16) | 0x169F,
        //79 	Runic 	16A0-16FF
        Runic = (79L << 32) | (0x16A0 << 16) | 0x16FF,
        //80 	Khmer 	1780-17FF
        //      Khmer Symbols 	19E0-19FF
        Khmer = (80L << 32) | (0x1780 << 16) | 0x17FF,
        Khmer_Symbols = (80L << 32) | (0x19E0 << 16) | 0x19FF,
        //81 	Mongolian 	1800-18AF
        Mongolian = (81L << 32) | (0x1800 << 16) | 0x18AF,
        //82 	Braille Patterns 	2800-28FF
        Braille_Patterns = (82L << 32) | (0x2800 << 16) | 0x28FF,
        //83 	Yi Syllables 	A000-A48F
        //      Yi Radicals 	A490-A4CF
        Yi_Syllables = (83L << 32) | (0xA000L << 16) | 0xA48F,
        Yi_Radicals = (83L << 32) | (0xA490L << 16) | 0xA4CF,
        //84 	Tagalog 	1700-171F
        //    Hanunoo 	1720-173F
        //    Buhid 	1740-175F
        //    Tagbanwa 	1760-177F
        Tagalog = (84L << 32) | (0x1700 << 16) | 0x171F,
        Hanunoo = (84L << 32) | (0x1720 << 16) | 0x173F,
        Buhid = (84L << 32) | (0x1740 << 16) | 0x175F,
        Tagbanwa = (84L << 32) | (0x1760 << 16) | 0x177F,
        //85 	Old Italic 	10300-1032F
        Old_Italic = (85L << 32) | (0x10300 << 16) | 0x1032F,
        //86 	Gothic 	10330-1034F
        Gothic = (86L << 32) | (0x10330 << 16) | 0x1034F,
        //87 	Deseret 	10400-1044F
        Deseret = (87L << 32) | (0x10400 << 16) | 0x1044F,
        //88 	Byzantine Musical Symbols 	1D000-1D0FF
        //    Musical Symbols 	1D100-1D1FF
        //    Ancient Greek Musical Notation 	1D200-1D24F
        Byzantine_Musical_Symbols = (88L << 32) | (0x1D000L << 16) | 0x1D0FF,
        Musical_Symbols = (88L << 32) | (0x1D100L << 16) | 0x1D1FF,
        Ancient_Greek_Musical_Notation = (88L << 32) | (0x1D200L << 16) | 0x1D24F,
        //89 	Mathematical Alphanumeric Symbols 	1D400-1D7FF
        Mathematical_Alphanumeric_Symbols = (89L << 32) | (0x1D400L << 16) | 0x1D7FF,
        //90 	Private Use (plane 15) 	FF000-FFFFD
        //    Private Use (plane 16) 	100000-10FFFD
        Private_Use_plane15 = (90L << 32) | (0xFF000L << 16) | 0xFFFFD,
        Private_Use_plane16 = (90L << 32) | (0x100000 << 16) | 0x10FFFD,
        //91 	Variation Selectors 	FE00-FE0F
        //    Variation Selectors Supplement 	E0100-E01EF
        Variation_Selectors = (91L << 32) | (0xFE00L << 16) | 0xFE0F,
        Variation_Selectors_Supplement = (91L << 32) | (0xE0100 << 16) | 0xE01EF,
        //92 	Tags 	E0000-E007F
        Tags = (92L << 32) | (0xE0000 << 16) | 0xE007F,
        //93 	Limbu 	1900-194F
        Limbu = (93L << 32) | (0x1900 << 16) | 0x194F,
        //94 	Tai Le 	1950-197F
        Tai_Le = (94L << 32) | (0x1950 << 16) | 0x197F,
        //95 	New Tai Lue 	1980-19DF
        New_Tai_Lue = (95L << 32) | (0x1980 << 16) | 0x19DF,
        //96 	Buginese 	1A00-1A1F
        Buginese = (96L << 32) | (0x1A00 << 16) | 0x1A1F,
        //97 	Glagolitic 	2C00-2C5F
        Glagolitic = (97L << 32) | (0x2C00 << 16) | 0x2C5F,
        //98 	Tifinagh 	2D30-2D7F
        Tifinagh = (98L << 32) | (0x2D30 << 16) | 0x2D7F,
        //99 	Yijing Hexagram Symbols 	4DC0-4DFF
        Yijing_Hexagram_Symbols = (99L << 32) | (0x4DC0 << 16) | 0x4DFF,
        //100 	Syloti Nagri 	A800-A82F
        Syloti_Nagri = (100L << 32) | (0xA800L << 16) | 0xA82F,
        //101 	Linear B Syllabary 	10000-1007F
        //    Linear B Ideograms 	10080-100FF
        //    Aegean Numbers 	10100-1013F
        Linear_B_Syllabary = (101L << 32) | (0x10000 << 16) | 0x1007F,
        Linear_B_Ideograms = (101L << 32) | (0x10080 << 16) | 0x100FF,
        Aegean_Numbers = (101L << 32) | (0x10100 << 16) | 0x1013F,
        //102 	Ancient Greek Numbers 	10140-1018F
        Ancient_Greek_Numbers = (102L << 32) | (0x10140 << 16) | 0x1018F,
        //103 	Ugaritic 	10380-1039F
        Ugaritic = (103L << 32) | (0x10380 << 16) | 0x1039F,
        //104 	Old Persian 	103A0-103DF
        Old_Persian = (104L << 32) | (0x103A0 << 16) | 0x103DF,
        //105 	Shavian 	10450-1047F
        Shavian = (105L << 32) | (0x10450 << 16) | 0x1047F,
        //106 	Osmanya 	10480-104AF
        Osmanya = (106L << 32) | (0x10480 << 16) | 0x104AF,
        //107 	Cypriot Syllabary 	10800-1083F
        Cypriot_Syllabary = (107L << 32) | (0x10800 << 16) | 0x1083F,
        //108 	Kharoshthi 	10A00-10A5F
        Kharoshthi = (108L << 32) | (0x10A00 << 16) | 0x10A5F,
        //109 	Tai Xuan Jing Symbols 	1D300-1D35F
        Tai_Xuan_Jing_Symbols = (109L << 32) | (0x1D300L << 16) | 0x1D35F,
        //110 	Cuneiform 	12000-123FF
        //    Cuneiform Numbers and Punctuation 	12400-1247F
        Cuneiform = (110L << 32) | (0x12000 << 16) | 0x123FF,
        Cuneiform_Numbers_and_Punctuation = (110L << 32) | (0x12400 << 16) | 0x1247F,
        //111 	Counting Rod Numerals 	1D360-1D37F
        Counting_Rod_Numerals = (111L << 32) | (0x1D360L << 16) | 0x1D37F,
        //112 	Sundanese 	1B80-1BBF
        Sundanese = (112L << 32) | (0x1B80 << 16) | 0x1BBF,
        //113 	Lepcha 	1C00-1C4F
        Lepcha = (113L << 32) | (0x1C00 << 16) | 0x1C4F,
        //114 	Ol Chiki 	1C50-1C7F
        Ol_Chiki = (114L << 32) | (0x1C50 << 16) | 0x1C7F,
        //115 	Saurashtra 	A880-A8DF
        Saurashtra = (115L << 32) | (0xA880L << 16) | 0xA8DF,

        //116 	Kayah Li 	A900-A92F
        Kayah_Li = (116L << 32) | (0xA900L << 16) | 0xA92F,
        //117 	Rejang 	A930-A95F
        Rejang = (117L << 32) | (0xA930L << 16) | 0xA95F,
        //118 	Cham 	AA00-AA5F
        Cham = (118L << 32) | (0xAA00L << 16) | 0xAA5F,
        //119 	Ancient Symbols 	10190-101CF
        Ancient_Symbols = (119L << 32) | (0x10190 << 16) | 0x101CF,
        //120 	Phaistos Disc 	101D0-101FF
        Phaistos_Disc = (120L << 32) | (0x101D0 << 16) | 0x101FF,
        //121 	Carian 	102A0-102DF
        Carian = (121L << 32) | (0x102A0 << 16) | 0x102DF,
        //    Lycian 	10280-1029F
        Lycian = (121L << 32) | (0x10280 << 16) | 0x1029F,
        //    Lydian 	10920-1093F
        Lydian = (121L << 32) | (0x10920 << 16) | 0x1093F,
        //122 	Domino Tiles 	1F030-1F09F
        //    Mahjong Tiles 	1F000-1F02F
        Domino_Tiles = (122L << 32) | (0x1F030L << 16) | 0x1F09F,
        Mahjong_Tiles = (122L << 32) | (0x1F000L << 16) | 0x1F02F,
        //123-127 	Reserved for process-internal usage
        //
        Reserved123 = (123L << 32),
        Reserved124 = (124L << 32),
        Reserved125 = (125L << 32),
        Reserved126 = (126L << 32),
        Reserved127 = (127L << 32),
    }


    public static class ScriptLangs
    {

        //https://www.microsoft.com/typography/otspec/scripttags.htm
        //https://www.microsoft.com/typography/otspec/languagetags.htm
        //--------------------------------------------------------------------
        static Dictionary<string, int> s_registerNames = new Dictionary<string, int>();
        static Dictionary<string, ScriptLang> s_registeredScriptTags = new Dictionary<string, ScriptLang>();
        static Dictionary<string, ScriptLang> s_registerScriptFromFullNames = new Dictionary<string, ScriptLang>();
        static SortedList<int, UnicodeRangeMapWithScriptLang> s_unicodeLangToScriptLang = new SortedList<int, UnicodeRangeMapWithScriptLang>();


        static Dictionary<string, UnicodeLangBits[]> s_registeredScriptTagsToUnicodeLangBits = new Dictionary<string, UnicodeLangBits[]>();

        struct UnicodeRangeMapWithScriptLang
        {
            public readonly ScriptLang scLang;
            public readonly UnicodeLangBits unicodeRangeBits;
            public UnicodeRangeMapWithScriptLang(UnicodeLangBits unicodeRangeBits, ScriptLang scLang)
            {
                this.scLang = scLang;
                this.unicodeRangeBits = unicodeRangeBits;
            }
            public bool IsInRange(char c)
            {
                return unicodeRangeBits.ToUnicodeRangeInfo().IsInRange(c);
            }
        }

        //
        public static readonly ScriptLang
        //
        Adlam = _("Adlam", "adlm"),
        Anatolian_Hieroglyphs = _("Anatolian Hieroglyphs", "hluw"),
        Arabic = _("Arabic", "arab", UnicodeLangBits.Arabic,
            UnicodeLangBits.ArabicSupplement,
            UnicodeLangBits.Arabic_Presentation_Forms_A,
            UnicodeLangBits.Arabic_Presentation_Forms_B),
        Armenian = _("Armenian", "armn", UnicodeLangBits.Armenian),
        Avestan = _("Avestan", "avst"),
        //
        Balinese = _("Balinese", "bali", UnicodeLangBits.Balinese),
        Bamum = _("Bamum", "bamu"),
        Bassa_Vah = _("Bassa Vah ", "bass"),
        Batak = _("Batak", "batk"),
        Bengali = _("Bengali", "beng", UnicodeLangBits.Bengali),
        Bengali_v_2 = _("Bengali v.2", "bng2", UnicodeLangBits.Bengali),
        Bhaiksuki = _("Bhaiksuki", "bhks"),
        Brahmi = _("Brahmi", "brah"),
        Braille = _("Braille", "brai", UnicodeLangBits.Braille_Patterns),
        Buginese = _("Buginese", "bugi", UnicodeLangBits.Buginese),
        Buhid = _("Buhid", "buhd", UnicodeLangBits.Buhid),
        Byzantine_Music = _("Byzantine Music", "byzm", UnicodeLangBits.Byzantine_Musical_Symbols),
        //
        Canadian_Syllabics = _("Canadian Syllabics", "cans", UnicodeLangBits.Unified_Canadian_Aboriginal_Syllabics),
        Carian = _("Carian", "cari", UnicodeLangBits.Carian),
        Caucasian_Albanian = _("Caucasian Albanian", "aghb"),
        Chakma = _("Chakma", "cakm"),
        Cham = _("Cham", "cham", UnicodeLangBits.Cham),
        Cherokee = _("Cherokee", "cher", UnicodeLangBits.Cherokee),
        CJK_Ideographic = _("CJK Ideographic", "hani",
            UnicodeLangBits.CJK_Compatibility,
            UnicodeLangBits.CJK_Compatibility_Forms,
            UnicodeLangBits.CJK_Compatibility_Ideographs,
            UnicodeLangBits.CJK_Compatibility_Ideographs_Supplement,
            UnicodeLangBits.CJK_Radicals_Supplement
            ),
        Coptic = _("Coptic", "copt", UnicodeLangBits.Coptic),
        Cypriot_Syllabary = _("Cypriot Syllabary", "cprt", UnicodeLangBits.Cypriot_Syllabary),
        Cyrillic = _("Cyrillic", "cyrl", UnicodeLangBits.Cyrillic, UnicodeLangBits.CyrillicExtendedA, UnicodeLangBits.CyrillicExtendedB),
        ////
        Default = _("Default", "DFLT"),
        Deseret = _("Deseret", "dsrt", UnicodeLangBits.Deseret),
        Devanagari = _("Devanagari", "deva", UnicodeLangBits.Devanagari),
        Devanagari_v_2 = _("Devanagari v.2", "dev2", UnicodeLangBits.Devanagari),
        Duployan = _("Duployan", "dupl"),
        ////            
        Egyptian_Hieroglyphs = _("Egyptian Hieroglyphs", "egyp"),
        Elbasan = _("Elbasan", "elba"),
        Ethiopic = _("Ethiopic", "ethi", UnicodeLangBits.Ethiopic, UnicodeLangBits.Ethiopic_Extended, UnicodeLangBits.Ethiopic_Supplement),
        //// 
        Georgian = _("Georgian", "geor", UnicodeLangBits.Georgian, UnicodeLangBits.GeorgianSupplement),
        Glagolitic = _("Glagolitic", "glag", UnicodeLangBits.Glagolitic),
        Gothic = _("Gothic", "goth", UnicodeLangBits.Gothic),
        Grantha = _("Grantha", "gran"),
        Greek = _("Greek", "grek", UnicodeLangBits.GreekAndCoptic, UnicodeLangBits.GreekExtended),
        Gujarati = _("Gujarati", "gujr", UnicodeLangBits.Gujarati),
        Gujarati_v_2 = _("Gujarati v.2", "gjr2", UnicodeLangBits.Gujarati),
        Gurmukhi = _("Gurmukhi", "guru", UnicodeLangBits.Gurmukhi),
        Gurmukhi_v_2 = _("Gurmukhi v.2", "gur2", UnicodeLangBits.Gurmukhi),
        //// 
        Hangul = _("Hangul", "hang", UnicodeLangBits.Hangul_Syllables),
        Hangul_Jamo = _("Hangul Jamo", "jamo", UnicodeLangBits.HangulJamo),
        Hanunoo = _("Hanunoo", "hano", UnicodeLangBits.Hanunoo),
        Hatran = _("Hatran", "hatr"),
        Hebrew = _("Hebrew", "hebr", UnicodeLangBits.Hebrew),
        Hiragana = _("Hiragana", "kana", UnicodeLangBits.Hiragana),
        //// 
        Imperial_Aramaic = _("Imperial Aramaic", "armi"),
        Inscriptional_Pahlavi = _("Inscriptional Pahlavi", "phli"),
        Inscriptional_Parthian = _("Inscriptional Parthian", "prti"),
        ////             	
        Javanese = _("Javanese", "java"),
        //// 
        Kaithi = _("Kaithi", "kthi"),
        Kannada = _("Kannada", "knda", UnicodeLangBits.Kannada),
        Kannada_v_2 = _("Kannada v.2", "knd2", UnicodeLangBits.Kannada),
        Katakana = _("Katakana", "kana", UnicodeLangBits.Katakana, UnicodeLangBits.Katakana_Phonetic_Extensions),
        Kayah_Li = _("Kayah Li", "kali"),
        Kharosthi = _("Kharosthi", "khar", UnicodeLangBits.Kharoshthi),
        Khmer = _("Khmer", "khmr", UnicodeLangBits.Khmer, UnicodeLangBits.Khmer_Symbols),
        Khojki = _("Khojki", "khoj"),
        Khudawadi = _("Khudawadi", "sind"),
        //// 
        Lao = _("Lao", "lao", UnicodeLangBits.Lao),
        Latin = _("Latin", "latn",
            UnicodeLangBits.BasicLatin, UnicodeLangBits.Latin1Supplement,
            UnicodeLangBits.LatinExtendedA, UnicodeLangBits.LatinExtendedAdditional,
            UnicodeLangBits.LatinExtendedAdditionalC, UnicodeLangBits.LatinExtendedAdditionalD,
            UnicodeLangBits.LatinExtendedB),

        Lepcha = _("Lepcha", "lepc", UnicodeLangBits.Lepcha),
        Limbu = _("Limbu", "limb", UnicodeLangBits.Limbu),
        Linear_A = _("Linear A", "lina"),
        Linear_B = _("Linear B", "linb", UnicodeLangBits.Linear_B_Ideograms, UnicodeLangBits.Linear_B_Syllabary),
        Lisu = _("Lisu (Fraser)", "lisu"),
        Lycian = _("Lycian", "lyci", UnicodeLangBits.Lycian),
        Lydian = _("Lydian", "lydi", UnicodeLangBits.Lydian),
        //// 
        Mahajani = _("Mahajani", "mahj"),
        Malayalam = _("Malayalam", "mlym", UnicodeLangBits.Malayalam),
        Malayalam_v_2 = _("Malayalam v.2", "mlm2", UnicodeLangBits.Malayalam),
        Mandaic = _("Mandaic, Mandaean", "mand"),
        Manichaean = _("Manichaean", "mani"),
        Marchen = _("Marchen", "marc"),
        Math = _("Mathematical Alphanumeric Symbols", "math", UnicodeLangBits.Mathematical_Alphanumeric_Symbols),
        Meitei_Mayek = _("Meitei Mayek (Meithei, Meetei)", "mtei"),
        Mende_Kikakui = _("Mende Kikakui", "mend"),
        Meroitic_Cursive = _("Meroitic Cursive", "merc"),
        Meroitic_Hieroglyphs = _("Meroitic Hieroglyphs", "mero"),
        Miao = _("Miao", "plrd"),
        Modi = _("Modi", "modi"),
        Mongolian = _("Mongolian", "mong", UnicodeLangBits.Mongolian),
        Mro = _("Mro", "mroo"),
        Multani = _("Multani", "mult"),
        Musical_Symbols = _("Musical Symbols", "musc", UnicodeLangBits.Musical_Symbols),
        Myanmar = _("Myanmar", "mymr", UnicodeLangBits.Myanmar),
        Myanmar_v_2 = _("Myanmar v.2", "mym2", UnicodeLangBits.Myanmar),
        ////      
        Nabataean = _("Nabataean", "nbat"),
        Newa = _("Newa", "newa"),
        New_Tai_Lue = _("New Tai Lue", "talu", UnicodeLangBits.New_Tai_Lue),
        N_Ko = _("N'Ko", "nko", UnicodeLangBits.NKo),
        //// 
        Odia = _("Odia (formerly Oriya)", "orya"),
        Odia_V_2 = _("Odia v.2 (formerly Oriya v.2)", "ory2"),
        Ogham = _("Ogham", "ogam", UnicodeLangBits.Ogham),
        Ol_Chiki = _("Ol Chiki", "olck", UnicodeLangBits.Ol_Chiki),
        Old_Italic = _("Old Italic", "ital"),
        Old_Hungarian = _("Old Hungarian", "hung"),
        Old_North_Arabian = _("Old North Arabian", "narb"),
        Old_Permic = _("Old Permic", "perm"),
        Old_Persian_Cuneiform = _("Old Persian Cuneiform ", "xpeo"),
        Old_South_Arabian = _("Old South Arabian", "sarb"),
        Old_Turkic = _("Old Turkic, Orkhon Runic", "orkh"),
        Osage = _("Osage", "osge"),
        Osmanya = _("Osmanya", "osma", UnicodeLangBits.Osmanya),
        //// 
        Pahawh_Hmong = _("Pahawh Hmong", "hmng"),
        Palmyrene = _("Palmyrene", "palm"),
        Pau_Cin_Hau = _("Pau Cin Hau", "pauc"),
        Phags_pa = _("Phags-pa", "phag", UnicodeLangBits.Phags_pa),
        Phoenician = _("Phoenician ", "phnx"),
        Psalter_Pahlavi = _("Psalter Pahlavi", "phlp"),

        //// 
        Rejang = _("Rejang", "rjng", UnicodeLangBits.Rejang),
        Runic = _("Runic", "runr", UnicodeLangBits.Runic),

        //// 
        Samaritan = _("Samaritan", "samr"),
        Saurashtra = _("Saurashtra", "saur", UnicodeLangBits.Saurashtra),
        Sharada = _("Sharada", "shrd"),
        Shavian = _("Shavian", "shaw", UnicodeLangBits.Shavian),
        Siddham = _("Siddham", "sidd"),
        Sign_Writing = _("Sign Writing", "sgnw"),
        Sinhala = _("Sinhala", "sinh", UnicodeLangBits.Sinhala),
        Sora_Sompeng = _("Sora Sompeng", "sora"),
        Sumero_Akkadian_Cuneiform = _("Sumero-Akkadian Cuneiform", "xsux"),
        Sundanese = _("Sundanese", "sund", UnicodeLangBits.Sundanese),
        Syloti_Nagri = _("Syloti Nagri", "sylo", UnicodeLangBits.Syloti_Nagri),
        Syriac = _("Syriac", "syrc", UnicodeLangBits.Syriac),
        ////       
        Tagalog = _("Tagalog", "tglg"),
        Tagbanwa = _("Tagbanwa", "tagb", UnicodeLangBits.Tagbanwa),
        Tai_Le = _("Tai Le", "tale", UnicodeLangBits.Tai_Le),
        Tai_Tham = _("Tai Tham (Lanna)", "lana"),
        Tai_Viet = _("Tai Viet", "tavt"),
        Takri = _("Takri", "takr"),
        Tamil = _("Tamil", "taml", UnicodeLangBits.Tamil),
        Tamil_v_2 = _("Tamil v.2", "tml2", UnicodeLangBits.Tamil),
        Tangut = _("Tangut", "tang"),
        Telugu = _("Telugu", "telu", UnicodeLangBits.Telugu),
        Telugu_v_2 = _("Telugu v.2", "tel2", UnicodeLangBits.Telugu),
        Thaana = _("Thaana", "thaa", UnicodeLangBits.Thaana),
        Thai = _("Thai", "thai", UnicodeLangBits.Thai),
        Tibetan = _("Tibetan", "tibt", UnicodeLangBits.Tibetan),
        Tifinagh = _("Tifinagh", "tfng", UnicodeLangBits.Tifinagh),
        Tirhuta = _("Tirhuta", "tirh"),
        ////
        Ugaritic_Cuneiform = _("Ugaritic Cuneiform", "ugar"),
        ////
        Vai = _("Vai", "vai"),
        ////
        Warang_Citi = _("Warang Citi", "wara"),

        ////
        Yi = _("Yi", "yi", UnicodeLangBits.Yi_Syllables)
        //
        ;



        static ScriptLang _(string fullname, string shortname, params UnicodeLangBits[] langBits)
        {

            if (s_registeredScriptTags.ContainsKey(shortname))
            {
                if (shortname == "kana")
                {
                    //***
                    //Hiragana and Katakana 
                    //both have same short name "kana"                     
                    return new ScriptLang(fullname, shortname, s_registerNames[shortname]);
                }
                else
                {
                    //errors
                    throw new System.NotSupportedException();
                }
            }
            else
            {
                int internalName = s_registerNames.Count;
                s_registerNames[shortname] = internalName;
                var scriptLang = new ScriptLang(fullname, shortname, internalName);
                s_registeredScriptTags.Add(shortname, scriptLang);
                //                 
                s_registerScriptFromFullNames[fullname] = scriptLang;

                //also register unicode langs with the script lang

                for (int i = langBits.Length - 1; i >= 0; --i)
                {
                    UnicodeRangeInfo unicodeRange = langBits[i].ToUnicodeRangeInfo();
                    if (!s_unicodeLangToScriptLang.ContainsKey(unicodeRange.StartAt))
                    {
                        s_unicodeLangToScriptLang.Add(unicodeRange.StartAt, new UnicodeRangeMapWithScriptLang(langBits[i], scriptLang));
                    }
                    else
                    {

                    }
                }


                if (langBits.Length > 0)
                {
                    s_registeredScriptTagsToUnicodeLangBits.Add(shortname, langBits);
                }


                return scriptLang;
            }
        }
        public static bool TryGetUnicodeLangBitsArray(string langShortName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out UnicodeLangBits[]? unicodeLangBits)
        {
            return s_registeredScriptTagsToUnicodeLangBits.TryGetValue(langShortName, out unicodeLangBits);
        }
        public static bool TryGetScriptLang(char c, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ScriptLang? scLang)
        {
            foreach (var kp in s_unicodeLangToScriptLang)
            {
                if (kp.Key > c)
                {
                    scLang = null;
                    return false;
                }
                else
                {
                    if (kp.Value.IsInRange(c))
                    {
                        //found
                        scLang = kp.Value.scLang;
                        return true;
                    }
                }
            }

            scLang = null;
            return false;
        }

        public static ScriptLang? GetRegisteredScriptLang(string shortname)
        {
            s_registeredScriptTags.TryGetValue(shortname, out ScriptLang? found);
            return found;
        }
        public static ScriptLang? GetRegisteredScriptLangFromLanguageName(string languageName)
        {
            s_registerScriptFromFullNames.TryGetValue(languageName, out ScriptLang? found);
            return found;
        }
        public static IEnumerable<ScriptLang> GetRegiteredScriptLangIter()
        {
            foreach (ScriptLang scriptLang in s_registeredScriptTags.Values)
            {
                yield return scriptLang;
            }
        }


    }


}
