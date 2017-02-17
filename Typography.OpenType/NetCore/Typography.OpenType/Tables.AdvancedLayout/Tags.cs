//Apache2, 2016-2017, WinterDev
using System;
using System.Collections.Generic;

namespace Typography.OpenType
{

    public sealed class ScriptLang
    {
        public readonly string fullname;
        public readonly string shortname;
        public ScriptLang(string fullname, string shortname)
        {
            this.fullname = fullname;
            this.shortname = shortname;
        }
    }

    public static class ScriptLangs
    {
        static Dictionary<string, Tables.TagInfo> registeredScriptTags = new Dictionary<string, Tables.TagInfo>();

        //https://www.microsoft.com/typography/otspec/scripttags.htm
        //https://www.microsoft.com/typography/otspec/languagetags.htm

        //
        public static readonly ScriptLang
        //
        Adlam = _("Adlam", "adlm"),
        Anatolian_Hieroglyphs = _("Anatolian Hieroglyphs", "hluw"),
        Arabic = _("Arabic", "arab"),
        Armenian = _("Armenian", "armn"),
        Avestan = _("Avestan", "avst"),
        //
        Balinese = _("Balinese", "bali"),
        Bamum = _("Bamum", "bamu"),
        Bassa_Vah = _("Bassa Vah ", "bass"),
        Batak = _("Batak", "batk"),
        Bengali = _("Bengali", "beng"),
        Bengali_v_2 = _("Bengali v.2", "bng2"),
        Bhaiksuki = _("Bhaiksuki", "bhks"),
        Brahmi = _("Brahmi", "brah"),
        Braille = _("Braille", "brai"),
        Buginese = _("Buginese", "bugi"),
        Buhid = _("Buhid", "buhd"),
        Byzantine_Music = _("Byzantine Music", "byzm"),
        //
        Canadian_Syllabics = _("Canadian Syllabics", "cans"),
        Carian = _("Carian", "cari"),
        Caucasian_Albanian = _("Caucasian Albanian", "aghb"),
        Chakma = _("Chakma", "cakm"),
        Cham = _("Cham", "cham"),
        Cherokee = _("Cherokee", "cher"),
        CJK_Ideographic = _("CJK Ideographic", "hani"),
        Coptic = _("Coptic", "copt"),
        Cypriot_Syllabary = _("Cypriot Syllabary", "cprt"),
        Cyrillic = _("Cyrillic", "cyrl"),
        ////
        Default = _("Default", "DFLT"),
        Deseret = _("Deseret", "dsrt"),
        Devanagari = _("Devanagari", "deva"),
        Devanagari_v_2 = _("Devanagari v.2", "dev2"),
        Duployan = _("Duployan", "dupl"),
        ////            
        Egyptian_Hieroglyphs = _("Egyptian Hieroglyphs", "egyp"),
        Elbasan = _("Elbasan", "elba"),
        Ethiopic = _("Ethiopic", "ethi"),
        //// 
        Georgian = _("Georgian", "geor"),
        Glagolitic = _("Glagolitic", "glag"),
        Gothic = _("Gothic", "goth"),
        Grantha = _("Grantha", "gran"),
        Greek = _("Greek", "grek"),
        Gujarati = _("Gujarati", "gujr"),
        Gujarati_v_2 = _("Gujarati v.2", "gjr2"),
        Gurmukhi = _("Gurmukhi", "guru"),
        Gurmukhi_v_2 = _("Gurmukhi v.2", "gur2"),
        //// 
        Hangul = _("Hangul", "hang"),
        Hangul_Jamo = _("Hangul Jamo", "jamo"),
        Hanunoo = _("Hanunoo", "hano"),
        Hatran = _("Hatran", "hatr"),
        Hebrew = _("Hebrew", "hebr"),
        Hiragana = _("Hiragana", "kana"),
        //// 
        Imperial_Aramaic = _("Imperial Aramaic", "armi"),
        Inscriptional_Pahlavi = _("Inscriptional Pahlavi", "phli"),
        Inscriptional_Parthian = _("Inscriptional Parthian", "prti"),
        ////             	
        Javanese = _("Javanese", "java"),
        //// 
        Kaithi = _("Kaithi", "kthi"),
        Kannada = _("Kannada", "knda"),
        Kannada_v_2 = _("Kannada v.2", "knd2"),
        Katakana = _("Katakana", "kana"),
        Kayah_Li = _("Kayah Li", "kali"),
        Kharosthi = _("Kharosthi", "khar"),
        Khmer = _("Khmer", "khmr"),
        Khojki = _("Khojki", "khoj"),
        Khudawadi = _("Khudawadi", "sind"),
        //// 
        Lao = _("Lao", "lao"),
        Latin = _("Latin", "latn"),
        Lepcha = _("Lepcha", "lepc"),
        Limbu = _("Limbu", "limb"),
        Linear_A = _("Linear A", "lina"),
        Linear_B = _("Linear B", "linb"),
        Lisu = _("Lisu (Fraser)", "lisu"),
        Lycian = _("Lycian", "lyci"),
        Lydian = _("Lydian", "lydi"),
        //// 
        Mahajani = _("Mahajani", "mahj"),
        Malayalam = _("Malayalam", "mlym"),
        Malayalam_v_2 = _("Malayalam v.2", "mlm2"),
        Mandaic = _("Mandaic, Mandaean", "mand"),
        Manichaean = _("Manichaean", "mani"),
        Marchen = _("Marchen", "marc"),
        Math = _("Mathematical Alphanumeric Symbols", "math"),
        Meitei_Mayek = _("Meitei Mayek (Meithei, Meetei)", "mtei"),
        Mende_Kikakui = _("Mende Kikakui", "mend"),
        Meroitic_Cursive = _("Meroitic Cursive", "merc"),
        Meroitic_Hieroglyphs = _("Meroitic Hieroglyphs", "mero"),
        Miao = _("Miao", "plrd"),
        Modi = _("Modi", "modi"),
        Mongolian = _("Mongolian", "mong"),
        Mro = _("Mro", "mroo"),
        Multani = _("Multani", "mult"),
        Musical_Symbols = _("Musical Symbols", "musc"),
        Myanmar = _("Myanmar", "mymr"),
        Myanmar_v_2 = _("Myanmar v.2", "mym2"),
        ////      
        Nabataean = _("Nabataean", "nbat"),
        Newa = _("Newa", "newa"),
        New_Tai_Lue = _("New Tai Lue", "talu"),
        N_Ko = _("N'Ko", "nko"),
        //// 
        Odia = _("Odia (formerly Oriya)", "orya"),
        Odia_V_2 = _("Odia v.2 (formerly Oriya v.2)", "ory2"),
        Ogham = _("Ogham", "ogam"),
        Ol_Chiki = _("Ol Chiki", "olck"),
        Old_Italic = _("Old Italic", "ital"),
        Old_Hungarian = _("Old Hungarian", "hung"),
        Old_North_Arabian = _("Old North Arabian", "narb"),
        Old_Permic = _("Old Permic", "perm"),
        Old_Persian_Cuneiform = _("Old Persian Cuneiform ", "xpeo"),
        Old_South_Arabian = _("Old South Arabian", "sarb"),
        Old_Turkic = _("Old Turkic, Orkhon Runic", "orkh"),
        Osage = _("Osage", "osge"),
        Osmanya = _("Osmanya", "osma"),
        //// 
        Pahawh_Hmong = _("Pahawh Hmong", "hmng"),
        Palmyrene = _("Palmyrene", "palm"),
        Pau_Cin_Hau = _("Pau Cin Hau", "pauc"),
        Phags_pa = _("Phags-pa", "phag"),
        Phoenician = _("Phoenician ", "phnx"),
        Psalter_Pahlavi = _("Psalter Pahlavi", "phlp"),

        //// 
        Rejang = _("Rejang", "rjng"),
        Runic = _("Runic", "runr"),

        //// 
        Samaritan = _("Samaritan", "samr"),
        Saurashtra = _("Saurashtra", "saur"),
        Sharada = _("Sharada", "shrd"),
        Shavian = _("Shavian", "shaw"),
        Siddham = _("Siddham", "sidd"),
        Sign_Writing = _("Sign Writing", "sgnw"),
        Sinhala = _("Sinhala", "sinh"),
        Sora_Sompeng = _("Sora Sompeng", "sora"),
        Sumero_Akkadian_Cuneiform = _("Sumero-Akkadian Cuneiform", "xsux"),
        Sundanese = _("Sundanese", "sund"),
        Syloti_Nagri = _("Syloti Nagri", "sylo"),
        Syriac = _("Syriac", "syrc"),
        ////       
        Tagalog = _("Tagalog", "tglg"),
        Tagbanwa = _("Tagbanwa", "tagb"),
        Tai_Le = _("Tai Le", "tale"),
        Tai_Tham = _("Tai Tham (Lanna)", "lana"),
        Tai_Viet = _("Tai Viet", "tavt"),
        Takri = _("Takri", "takr"),
        Tamil = _("Tamil", "taml"),
        Tamil_v_2 = _("Tamil v.2", "tml2"),
        Tangut = _("Tangut", "tang"),
        Telugu = _("Telugu", "telu"),
        Telugu_v_2 = _("Telugu v.2", "tel2"),
        Thaana = _("Thaana", "thaa"),
        Thai = _("Thai", "thai"),
        Tibetan = _("Tibetan", "tibt"),
        Tifinagh = _("Tifinagh", "tfng"),
        Tirhuta = _("Tirhuta", "tirh"),
        ////
        Ugaritic_Cuneiform = _("Ugaritic Cuneiform", "ugar"),
        ////
        Vai = _("Vai", "vai"),
        ////
        Warang_Citi = _("Warang Citi", "wara"),

        ////
        Yi = _("Yi", "yi")
        //
        ;

        //--------------------------------------------------------------------

        static ScriptLang _(string fullname, string shortname)
        {
            var scriptLang = new ScriptLang(fullname, shortname);
            //
            Tables.TagInfo tagInfo = new Tables.TagInfo(Tables.TagKind.Script, shortname, fullname);
            if (registeredScriptTags.ContainsKey(shortname))
            {
                if (shortname == "kana")
                {
                    //Hiragana and Katakana 
                    //both have same short name "kana"
                }
                else
                {

                }
            }
            else
            {
                registeredScriptTags.Add(shortname, tagInfo);
            }
            return scriptLang;
        }


        internal static Tables.TagInfo GetTagInfo(string shortname)
        {
            Tables.TagInfo found;
            registeredScriptTags.TryGetValue(shortname, out found);
            return found;
        }
    }


}

namespace Typography.OpenType.Tables
{


    static class TagsLookup
    {

        static Dictionary<string, TagInfo> registeredFeatureTags = new Dictionary<string, TagInfo>();

        static TagsLookup()
        {

            RegisterFeatureTags();
            RegisterBaselineTags();
        }
#if DEBUG
        static void debugCheckShortName(string shortname)
        {
            if (shortname.Length > 4)
            {
                throw new NotSupportedException();
            }
        }
#endif

        static void RegisterFeatureTags()
        {
            //https://www.microsoft.com/typography/otspec/featurelist.htm

            RegisterFeatureTag("aalt", "Access All Alternates");
            RegisterFeatureTag("abvf", "Above-base Forms");
            RegisterFeatureTag("abvm", "Above-base Mark Positioning");
            RegisterFeatureTag("abvs", "Above-base Substitutions");
            RegisterFeatureTag("afrc", "Alternative Fractions");
            RegisterFeatureTag("akhn", "Akhands");
            //
            RegisterFeatureTag("blwf", "Below-base Forms");
            RegisterFeatureTag("blwm", "Below-base Mark Positioning");
            RegisterFeatureTag("blws", "Below-base Substitutions");
            //
            RegisterFeatureTag("calt", "Contextual Alternates");
            RegisterFeatureTag("case", "Case-Sensitive Forms");
            RegisterFeatureTag("ccmp", "Glyph Composition / Decomposition");
            RegisterFeatureTag("cfar", "Conjunct Form After Ro");
            RegisterFeatureTag("cjct", "Conjunct Forms");
            RegisterFeatureTag("clig", "Contextual Ligatures");
            RegisterFeatureTag("cpct", "Centered CJK Punctuation");
            RegisterFeatureTag("cpsp", "Capital Spacing");
            RegisterFeatureTag("cswh", "Contextual Swash");
            RegisterFeatureTag("curs", "Cursive Positioning");
            for (int i = 1; i < 9; ++i)
            {
                RegisterFeatureTag("cv0" + i, "Character Variants" + i);
            }
            for (int i = 10; i < 100; ++i)
            {
                RegisterFeatureTag("cv" + i, "Character Variants" + i);
            }
            RegisterFeatureTag("c2pc", "Petite Capitals From Capitals");
            RegisterFeatureTag("c2sc", "Small Capitals From Capitals");
            //
            RegisterFeatureTag("dist", "Distances");
            RegisterFeatureTag("dlig", "Discretionary Ligatures");
            RegisterFeatureTag("dnom", "Denominators");
            RegisterFeatureTag("dtls", "Dotless Forms");
            //
            RegisterFeatureTag("expt", "Expert Forms");
            //
            RegisterFeatureTag("falt", "Final Glyph on Line Alternates");
            RegisterFeatureTag("fin2", "Terminal Forms #2");
            RegisterFeatureTag("fin3", "Terminal Forms #3");
            RegisterFeatureTag("fina", "Terminal Forms");
            RegisterFeatureTag("flac", "Flattened accent forms");
            RegisterFeatureTag("frac", "Fractions");
            RegisterFeatureTag("fwid", "Full Widths");
            //
            RegisterFeatureTag("half", "Half Forms");
            RegisterFeatureTag("haln", "Halant Forms");
            RegisterFeatureTag("halt", "Alternate Half Widths");
            RegisterFeatureTag("hist", "Historical Forms");
            RegisterFeatureTag("hkna", "Horizontal Kana Alternates");
            RegisterFeatureTag("hlig", "Historical Ligatures");
            RegisterFeatureTag("hngl", "Hangul");
            RegisterFeatureTag("hojo", "Hojo Kanji Forms (JIS X 0212-1990 Kanji Forms)");
            RegisterFeatureTag("hwid", "Half Widths");
            //
            RegisterFeatureTag("init", "Initial Forms");
            RegisterFeatureTag("isol", "Isolated Forms");
            RegisterFeatureTag("ital", "Italics");
            //Italics
            RegisterFeatureTag("jalt", "Justification Alternates");
            RegisterFeatureTag("jp78", "JIS78 Forms");
            RegisterFeatureTag("jp83", "JIS83 Forms");
            RegisterFeatureTag("jp90", "JIS90 Forms");
            RegisterFeatureTag("jp04", "JIS2004 Forms");
            //
            RegisterFeatureTag("kern", "Kerning");
            //
            RegisterFeatureTag("lfbd", "Left Bounds");
            RegisterFeatureTag("liga", "Standard Ligatures");
            RegisterFeatureTag("ljmo", "Leading Jamo Forms");
            RegisterFeatureTag("lnum", "Lining Figures");
            RegisterFeatureTag("locl", "Localized Forms");
            RegisterFeatureTag("ltra", "Left-to-right alternates");
            RegisterFeatureTag("ltrm", "Left-to-right mirrored forms");
            //
            RegisterFeatureTag("mark", "Mark Positioning");
            RegisterFeatureTag("med2", "Medial Forms #2");
            RegisterFeatureTag("medi", "Medial Forms");
            RegisterFeatureTag("mgrk", "Mathematical Greek");
            RegisterFeatureTag("mkmk", "Mark to Mark Positioning");
            RegisterFeatureTag("mset", "Mark Positioning via Substitution");
            //
            RegisterFeatureTag("nalt", "Alternate Annotation Forms");
            RegisterFeatureTag("nlck", "NLC Kanji Forms");
            RegisterFeatureTag("nukt", "Nukta Forms");
            RegisterFeatureTag("numr", "Numerators");
            //
            RegisterFeatureTag("onum", "Oldstyle Figures");
            RegisterFeatureTag("opbd", "Optical Bounds");
            RegisterFeatureTag("ordn", "Ordinals");
            RegisterFeatureTag("ornm", "Ornaments");
            //
            RegisterFeatureTag("palt", "Proportional Alternate Widths");
            RegisterFeatureTag("pcap", "Petite Capitals");
            RegisterFeatureTag("pkna", "Proportional Kana");
            RegisterFeatureTag("pnum", "Proportional Figures");
            RegisterFeatureTag("pref", "Pre-Base Forms");
            RegisterFeatureTag("pres", "Pre-base Substitutions");
            RegisterFeatureTag("pstf", "Post-base Forms");
            RegisterFeatureTag("psts", "Post-base Substitutions");
            RegisterFeatureTag("pwid", "Proportional Widths");
            //
            RegisterFeatureTag("qwid", "Quarter Widths");
            //
            RegisterFeatureTag("rand", "Randomize");
            RegisterFeatureTag("rclt", "Required Contextual Alternates");
            RegisterFeatureTag("rkrf", "Rakar Forms");
            RegisterFeatureTag("rlig", "Required Ligatures");
            RegisterFeatureTag("rphf", "Reph Forms");
            //

            RegisterFeatureTag("rtbd", "Right Bounds");
            RegisterFeatureTag("rtla", "Right-to-left alternates");
            RegisterFeatureTag("rtlm", "Right-to-left mirrored forms");
            RegisterFeatureTag("ruby", "Ruby Notation Forms");
            RegisterFeatureTag("rvrn", "Required Variation Alternates");
            //
            RegisterFeatureTag("salt", "Stylistic Alternates");
            RegisterFeatureTag("sinf", "Scientific Inferiors");
            RegisterFeatureTag("size", "Optical size");
            RegisterFeatureTag("smcp", "Small Capitals");
            RegisterFeatureTag("smpl", "Simplified Forms");
            RegisterFeatureTag("ss01", "Stylistic Set 1");
            RegisterFeatureTag("ss02", "Stylistic Set 2");
            RegisterFeatureTag("ss03", "Stylistic Set 3");
            RegisterFeatureTag("ss04", "Stylistic Set 4");
            RegisterFeatureTag("ss05", "Stylistic Set 5");
            RegisterFeatureTag("ss06", "Stylistic Set 6");
            RegisterFeatureTag("ss07", "Stylistic Set 7");
            RegisterFeatureTag("ss08", "Stylistic Set 8");
            RegisterFeatureTag("ss09", "Stylistic Set 9");
            RegisterFeatureTag("ss10", "Stylistic Set 10");
            RegisterFeatureTag("ss11", "Stylistic Set 11");
            RegisterFeatureTag("ss12", "Stylistic Set 12");
            RegisterFeatureTag("ss13", "Stylistic Set 13");
            RegisterFeatureTag("ss14", "Stylistic Set 14");
            RegisterFeatureTag("ss15", "Stylistic Set 15");
            RegisterFeatureTag("ss16", "Stylistic Set 16");
            RegisterFeatureTag("ss17", "Stylistic Set 17");
            RegisterFeatureTag("ss18", "Stylistic Set 18");
            RegisterFeatureTag("ss19", "Stylistic Set 19");
            RegisterFeatureTag("ss20", "Stylistic Set 20");
            // 
            RegisterFeatureTag("ssty", "Math script style alternates");
            RegisterFeatureTag("stch", "Stretching Glyph Decomposition");
            RegisterFeatureTag("subs", "Subscript");
            RegisterFeatureTag("sups", "Superscript");
            RegisterFeatureTag("swsh", "Swash");
            //
            RegisterFeatureTag("titl", "Titling");
            RegisterFeatureTag("tjmo", "Trailing Jamo Forms");
            RegisterFeatureTag("tnam", "Traditional Name Forms");
            RegisterFeatureTag("tnum", "Tabular Figures");
            RegisterFeatureTag("trad", "Traditional Forms");
            RegisterFeatureTag("twid", "Third Widths");
            //            
            RegisterFeatureTag("unic", "Unicase");

            RegisterFeatureTag("valt", "Alternate Vertical Metrics");
            RegisterFeatureTag("vatu", "Vattu Variants");
            RegisterFeatureTag("vert", "Vertical Writing");
            RegisterFeatureTag("vhal", "Alternate Vertical Half Metrics");
            RegisterFeatureTag("vjmo", "Vowel Jamo Forms");
            RegisterFeatureTag("vkna", "Vertical Kana Alternates");
            RegisterFeatureTag("vkrn", "Vertical Kerning");
            RegisterFeatureTag("vpal", "Proportional Alternate Vertical Metrics");
            RegisterFeatureTag("vrt2", "Vertical Alternates and Rotation");
            RegisterFeatureTag("vrtr", "Vertical Alternates for Rotation");
            //


        }

        static void RegisterBaselineTags()
        {
            //TODO: implement here
        }

        public static TagInfo GetFeatureTagInfo(string shortname)
        {
            TagInfo found;
            registeredFeatureTags.TryGetValue(shortname, out found);
            return found;
        }
        static void RegisterFeatureTag(string shortname, string fullname)
        {
#if DEBUG
            debugCheckShortName(shortname);
#endif
            registeredFeatureTags.Add(shortname, new TagInfo(TagKind.Feature, shortname, fullname));
        }
    }

    enum TagKind
    {
        Script,
        Feature,
    }
    class TagInfo
    {

        public string ShortName { get; private set; }
        public string FullName { get; private set; }
        public TagKind Kind { get; private set; }
        public TagInfo(TagKind kind, string shortName, string fullname)
        {
            this.Kind = kind;
            this.FullName = fullname;
            this.ShortName = shortName;
        }
    }


}