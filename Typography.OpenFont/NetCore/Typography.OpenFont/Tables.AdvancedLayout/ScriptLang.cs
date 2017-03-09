//Apache2, 2016-2017, WinterDev
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
    }

    public static class ScriptLangs
    {
        static Dictionary<string, int> s_registerNames = new Dictionary<string, int>();
        static Dictionary<string, ScriptLang> s_registeredScriptTags = new Dictionary<string, ScriptLang>();

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


            // 
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
                return scriptLang;
            }

        }


        public static ScriptLang GetRegisteredScriptLang(string shortname)
        {
            ScriptLang found;
            s_registeredScriptTags.TryGetValue(shortname, out found);
            return found;
        }
    }


}
