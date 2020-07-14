//MIT, 2016-present, WinterDev

using static Typography.TextBreak.Unicode13RangeInfoList;

namespace Typography.OpenFont.Tables
{
    /// <summary>
    /// OpenFont's bit position and unicode range
    /// </summary>
    public readonly struct BitposAndAssciatedUnicodeRanges
    {
        public readonly int Bitpos;
        public readonly UnicodeRangeInfo[] Ranges;
        public BitposAndAssciatedUnicodeRanges(int bitpos, UnicodeRangeInfo[] ranges)
        {
            Bitpos = bitpos;
            Ranges = ranges;
        }
        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append(Bitpos + ",");
            for (int i = 0; i < Ranges.Length; ++i)
            {
                sb.Append(Ranges[i].ToString());
            }
            return sb.ToString();
        }
        public bool IsInRange(int codepoint)
        {
            for (int i = 0; i < Ranges.Length; ++i)
            {
                if (Ranges[i].IsInRange(codepoint))
                {
                    return true;
                }
            }
            return false;
        }


        public static readonly UnicodeRangeInfo None_Plane_0 = new UnicodeRangeInfo(0x10000, 0x10FFFF, "None Plane 0");
    }

    static class OpenFontBitPosInfo
    {
        //from https://docs.microsoft.com/en-us/typography/opentype/spec/os2#ulunicoderange1-bits-031ulunicoderange2-bits-3263ulunicoderange3-bits-6495ulunicoderange4-bits-96127     
        //All available bits were exhausted as of Unicode 5.1. 
        //The bit assignments were last updated for OS/2 version 4 in OpenType 1.5. 
        //There are many additional ranges supported in the current version of Unicode that are not supported by these fields in the OS/2 table.
        //See the 'dlng' and 'slng' tags in the 'meta' table for an alternate mechanism to declare what scripts or languages that a font can support or 
        //is designed for. 



        static readonly BitposAndAssciatedUnicodeRanges[] s_bitposAndAssocRanges = new BitposAndAssciatedUnicodeRanges[]
        {
_(0, C0_Controls_and_Basic_Latin),
_(1, C1_Controls_and_Latin_1_Supplement),
_(2,Latin_Extended_A),
_(3,Latin_Extended_B),
_(4,IPA_Extensions,Phonetic_Extensions,Phonetic_Extensions_Supplement),
_(5,Spacing_Midifier_Letters,Modifier_Tone_Letters),
_(6,Combining_Diacritical_Marks,Combining_Diacritical_Marks_Supplement),
_(7,Greek_and_Coptic),
_(8,Coptic),
_(9,Cyrillic,Cyrillic_Supplement,Cyrillic_Extended_A,Cyrillic_Extended_B),
_(10,Armenian),
_(11,Hebrew),
_(12,Vai),
_(13,Arabic,Arabic_Supplement),
_(14,N_Ko),
_(15,Devanagari),
_(16,Bengali),
_(17,Gurmukhi),
_(18,Gujarati),
_(19,Oriya),
_(20,Tamil),
_(21,Telugu),
_(22,Kannada),
_(23,Malayalam),
_(24,Thai),
_(25,Lao),
_(26,Georgian,Georgian_Supplement),
_(27,Balinese),
_(28,Hangul_Jamo),
_(29,Latin_Extended_Additional,Latin_Extended_C,Latin_Extended_D),
_(30,Greek_Extended),
_(31,General_Punctuation,Supplemental_Punctuation),
_(32,Superscripts_and_Subscripts),
_(33,Currency_Symbols),
_(34,Combining_Diacritical_Marks_for_Symbols),
_(35,Letterlike_Symbols),
_(36,Number_Forms),
_(37,Arrows,Supplemental_Arrows_A,Supplemental_Arrows_B,Miscellaneous_Symbols_and_Arrows),
_(38,Mathematical_Operators,Supplemental_Mathematical_Operators,Miscellaneous_Mathematical_Symbols_A,Miscellaneous_Mathematical_Symbols_B),
_(39,Miscellaneous_Technical),
_(40,Control_Pictures),
_(41,Optical_Character_Recognition),
_(42,Enclosed_Alphanumerics),
_(43,Box_Drawing),
_(44,Block_Elements),
_(45,Geometric_Shapes),
_(46,Miscellaneous_Symbols),
_(47,Dingbat ),
_(48, CJK_Symbols_and_Punctuation ),
_(49,Hiragana),
_(50,Katakana,Katakana_Phonetic_Extensions),
_(51,Bopomofo,Bopomofo_Extended),
_(52,Hangul_Compatibility_Jamo),
_(53,Phags_pa),
_(54,Enclosed_CJK_Letters_and_Months),
_(55,CJK_Compatibility),
_(56,Hangul_Syllables),

_(57,BitposAndAssciatedUnicodeRanges.None_Plane_0), //**TODO: review here again

_(58,Phoenician),
_(59,CJK_Unified_Ideographs,CJK_Radicals_Supplement,Kangxi_radical ,Ideographic_Description_Characters,CJK_Unified_Ideographs_Extension_A,CJK_Unified_Ideographs_Extension_B,Kanbun),
_(60,Private_Use_Area),/*Private_Use_Area__plane_0_*/
_(61,CJK_Strokes,CJK_Compatibility_Ideographs,CJK_Compatibility_Ideographs_Supplement),
_(62,Alphabetic_Presentation_Forms),
_(63,Arabic_Presentation_Forms_A),
_(64,Combining_Half_Marks),
_(65,Vertical_Forms,CJK_Compatibility_Forms),
_(66,Small_Form_Variants),
_(67,Arabic_Presentation_Forms_B),
_(68,Halfwidth_and_Fullwidth_Forms),
_(69,Specials),
_(70,Tibetan),
_(71,Syriac),
_(72,Thaana),
_(73,Sinhala),
_(74,Myanmar),
_(75,Ethiopic,Ethiopic_Supplement,Ethiopic_Extended),
_(76,Cherokee),
_(77,Unified_Canadian_Aboriginal_Syllabics),
_(78,Ogham),
_(79,Runic),
_(80,Khmer,Khmer_Symbols),
_(81,Mongolian),
_(82,Braille_Patterns),
_(83,Yi_Syllables,Yi_Radicals),
_(84,Tagalog,Hanunoo,Buhid,Tagbanwa),
_(85,Old_Italic),
_(86,Gothic),
_(87,Deseret),
_(88,Byzantine_Musical_Symbols,Musical_Symbols,Ancient_Greek_Musical_Notation),
_(89,Mathematical_Alphanumeric_Symbols),
_(90,Supplementary_Private_Use_Area_A,Supplementary_Private_Use_Area_B),
_(91,Variation_Selectors,Variation_Selectors_Supplement),
_(92,Tags),
_(93,Limbu),
_(94,Tai_Le),
_(95,New_Tai_Lue),
_(96,Buginese),
_(97,Glagolitic),
_(98,Tifinagh),
_(99,Yijing_Hexagram_Symbols),
_(100,Syloti_Nagri),
_(101,Linear_B_Syllabary,Linear_B_Ideograms,Aegean_Numbers),
_(102,Ancient_Greek_Numbers),
_(103,Ugaritic),
_(104,Old_Persian),
_(105,Shavian),
_(106,Osmanya),
_(107,Cypriot_Syllabary),
_(108,Kharoshthi),
_(109,Tai_Xuan_Jing_Symbols),
_(110,Cuneiform,Cuneiform_Numbers_and_Punctuation),
_(111,Counting_Rod_Numerals),
_(112,Sundanese),
_(113,Lepcha),
_(114,Ol_Chiki),
_(115,Saurashtra),
_(116,Kayah_Li),
_(117,Rejang),
_(118,Cham),
_(119,Ancient_Symbols),
_(120,Phaistos_Disc),
_(121,Carian,Lycian,Lydian),
_(122,Domino_Tiles,Mahjong_Tiles),

        };

        static BitposAndAssciatedUnicodeRanges _(int bitpos, params UnicodeRangeInfo[] ranges) => new BitposAndAssciatedUnicodeRanges(bitpos, ranges);

        public const int MAX_BITPOS = 122;

        public static BitposAndAssciatedUnicodeRanges GetUnicodeRanges(int bitpos) => s_bitposAndAssocRanges[bitpos];
    }
}