//https://www.microsoft.com/typography/otspec/scripttags.htm
//Apache2,  2016,  WinterDev
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

//https://www.microsoft.com/typography/otspec/GDEF.htm 
namespace NRasterizer.Tables
{

    //Script tags 
    //Script tags generally correspond to a Unicode script. 
    //However, the associations between them may not always be one-to-one, and the OFF/OT tags are not guaranteed to be the same as Unicode Script property-value aliases or
    //ISO 15924 script IDs. Since the development of OFF/OT script tags predates the ISO 15924 or Unicode Script property, 
    //the rules for script tags defined in this document may not always be the same as rules for ISO 15924 script IDs. 
    //The OFF/OT script tags can also correlate with a particular OFF/OT layout implementation,
    //with the result that more than one script tag may be registered for a given Unicode script (e.g. ‘deva’ and ‘dev2’).

    //All tags are 4-byte character strings composed of a limited set of ASCII characters in the 0x20-0x7E range.
    //A script tag can consist of four or fewer lowercase letters. If a script tag consists less than four lowercase letters,
    //the letters are followed by the requisite number of spaces (0x20), each consisting of a single byte.



    static class TagsLookup
    {

        static Dictionary<string, TagInfo> registerTags = new Dictionary<string, TagInfo>();

        static TagsLookup()
        {
            RegisterScriptTags();

        }


        static void RegisterScriptTags()
        {
            RegisterScriptTag("Adlam", "adlm");
            RegisterScriptTag("Anatolian Hieroglyphs", "hluw");

            RegisterScriptTag("Arabic", "arab");
            RegisterScriptTag("Armenian", "armn");
            RegisterScriptTag("Avestan", "avst");
            //
            RegisterScriptTag("Balinese", "bali");
            RegisterScriptTag("Bamum", "bamu");
            RegisterScriptTag("Bassa Vah ", "bass");
            //
            RegisterScriptTag("Batak", "batk");
            RegisterScriptTag("Bengali", "beng");
            RegisterScriptTag("Bengali v.2", "bng2");
            RegisterScriptTag("Bhaiksuki", "bhks");
            RegisterScriptTag("Brahmi", "brah");
            RegisterScriptTag("Braille", "brai");
            RegisterScriptTag("Buginese", "bugi");
            RegisterScriptTag("Buhid", "buhd");
            RegisterScriptTag("Byzantine Music", "byzm");
            //
            RegisterScriptTag("Canadian Syllabics", "cans");
            RegisterScriptTag("Carian", "cari");
            RegisterScriptTag("Caucasian Albanian", "aghb");

            //

            RegisterScriptTag("Chakma", "cakm");
            RegisterScriptTag("Cham", "cham");
            RegisterScriptTag("Cherokee", "cher");
            RegisterScriptTag("CJK Ideographic", "hani");
            RegisterScriptTag("Coptic", "copt");
            RegisterScriptTag("Cypriot Syllabary", "cprt");
            RegisterScriptTag("Cyrillic", "cyrl");
            //
            RegisterScriptTag("Default", "DFLT");
            RegisterScriptTag("Deseret", "dsrt");
            RegisterScriptTag("Devanagari", "deva");
            RegisterScriptTag("Devanagari v.2", "dev2");
            RegisterScriptTag("Duployan", "dupl");
            //            
            RegisterScriptTag("Egyptian Hieroglyphs", "egyp");
            RegisterScriptTag("Elbasan", "elba");
            RegisterScriptTag("Ethiopic", "ethi");
            // 
            RegisterScriptTag("Georgian", "geor");
            RegisterScriptTag("Glagolitic", "glag");
            RegisterScriptTag("Gothic", "goth");
            RegisterScriptTag("Grantha", "gran");
            RegisterScriptTag("Greek", "grek");
            RegisterScriptTag("Gujarati", "gujr");
            RegisterScriptTag("Gujarati v.2", "gjr2");
            RegisterScriptTag("Gurmukhi", "guru");
            RegisterScriptTag("Gurmukhi v.2", "gur2");
            //

            RegisterScriptTag("Hangul", "hang");
            RegisterScriptTag("Hangul Jamo", "jamo");
            RegisterScriptTag("Hanunoo", "hano");
            RegisterScriptTag("Hatran", "hatr");
            RegisterScriptTag("Hebrew", "hebr");
            RegisterScriptTag("Hiragana", "kana");
            // 
            RegisterScriptTag("Imperial Aramaic", "armi");
            RegisterScriptTag("Inscriptional Pahlavi", "phli");
            RegisterScriptTag("Inscriptional Parthian", "prti");
            //             	
            RegisterScriptTag("Javanese", "java");
            //

            RegisterScriptTag("Kaithi", "kthi");
            RegisterScriptTag("Kannada", "knda");
            RegisterScriptTag("Kannada v.2", "knd2");
            RegisterScriptTag("Katakana", "kana");
            RegisterScriptTag("Kayah Li", "kali");
            RegisterScriptTag("Kharosthi", "khar");
            RegisterScriptTag("Khmer", "khmr");
            RegisterScriptTag("Khojki", "khoj");
            RegisterScriptTag("Khudawadi", "sind");
            //



            RegisterScriptTag("Lao", "lao");
            RegisterScriptTag("Latin", "latn");
            RegisterScriptTag("Lepcha", "lepc");
            RegisterScriptTag("Limbu", "limb");
            RegisterScriptTag("Linear A", "lina");
            RegisterScriptTag("Linear B", "linb");
            RegisterScriptTag("Lisu (Fraser)", "lisu");
            RegisterScriptTag("Lycian", "lyci");
            RegisterScriptTag("Lydian", "lydi");
            // 
            RegisterScriptTag("Mahajani", "mahj");
            RegisterScriptTag("Malayalam", "mlym");
            RegisterScriptTag("Malayalam v.2", "mlm2");
            RegisterScriptTag("Mandaic, Mandaean", "mand");
            RegisterScriptTag("Manichaean", "mani");
            RegisterScriptTag("Marchen", "marc");
            RegisterScriptTag("Mathematical Alphanumeric Symbols", "math");
            RegisterScriptTag("Meitei Mayek (Meithei, Meetei)", "mtei");
            RegisterScriptTag("Mende Kikakui", "mend");
            RegisterScriptTag("Meroitic Cursive", "merc");
            RegisterScriptTag("Meroitic Hieroglyphs", "mero");
            RegisterScriptTag("Miao", "plrd");
            RegisterScriptTag("Modi", "modi");
            RegisterScriptTag("Mongolian", "mong");
            RegisterScriptTag("Mro", "mroo");
            RegisterScriptTag("Multani", "mult");
            RegisterScriptTag("Musical Symbols", "musc");
            RegisterScriptTag("Myanmar", "mymr");
            RegisterScriptTag("Myanmar v.2", "mym2");
            //          	



            RegisterScriptTag("Nabataean", "nbat");
            RegisterScriptTag("Newa", "newa");
            RegisterScriptTag("New Tai Lue", "talu");
            RegisterScriptTag("N'Ko", "nko");
            //


            RegisterScriptTag("Odia (formerly Oriya)", "orya");
            RegisterScriptTag("Odia v.2 (formerly Oriya v.2)", "ory2");
            RegisterScriptTag("Ogham", "ogam");
            RegisterScriptTag("Ol Chiki", "olck");
            RegisterScriptTag("Old Italic", "ital");
            RegisterScriptTag("Old Hungarian", "hung");
            RegisterScriptTag("Old North Arabian", "narb");
            RegisterScriptTag("Old Permic", "perm");
            RegisterScriptTag("Old Persian Cuneiform ", "xpeo");
            RegisterScriptTag("Old South Arabian", "sarb");
            RegisterScriptTag("Old Turkic, Orkhon Runic", "orkh");
            RegisterScriptTag("Osage", "osge");
            RegisterScriptTag("Osmanya", "osma");
            //

            RegisterScriptTag("Pahawh Hmong", "hmng");
            RegisterScriptTag("Palmyrene", "palm");
            RegisterScriptTag("Pau Cin Hau", "pauc");
            RegisterScriptTag("Phags-pa", "phag");
            RegisterScriptTag("Phoenician ", "phnx");
            RegisterScriptTag("Psalter Pahlavi", "phlp");
            //


            RegisterScriptTag("Rejang", "rjng");
            RegisterScriptTag("Runic", "runr");
            //

            RegisterScriptTag("Samaritan", "samr");
            RegisterScriptTag("Saurashtra", "saur");
            RegisterScriptTag("Sharada", "shrd");
            RegisterScriptTag("Shavian", "shaw");
            RegisterScriptTag("Siddham", "sidd");
            RegisterScriptTag("Sign Writing", "sgnw");
            RegisterScriptTag("Sinhala", "sinh");
            RegisterScriptTag("Sora Sompeng", "sora");
            RegisterScriptTag("Sumero-Akkadian Cuneiform", "xsux");
            RegisterScriptTag("Sundanese", "sund");
            RegisterScriptTag("Syloti Nagri", "sylo");
            RegisterScriptTag("Syriac", "syrc");
            //

            RegisterScriptTag("Tagalog", "tglg");
            RegisterScriptTag("Tagbanwa", "tagb");
            RegisterScriptTag("Tai Le", "tale");
            RegisterScriptTag("Tai Tham (Lanna)", "lana");
            RegisterScriptTag("Tai Viet", "tavt");
            RegisterScriptTag("Takri", "takr");
            RegisterScriptTag("Tamil", "taml");
            RegisterScriptTag("Tamil v.2", "tml2");
            RegisterScriptTag("Tangut", "tang");
            RegisterScriptTag("Telugu", "telu");
            RegisterScriptTag("Telugu v.2", "tel2");
            RegisterScriptTag("Thaana", "thaa");
            RegisterScriptTag("Thai", "thai");
            RegisterScriptTag("Tibetan", "tibt");
            RegisterScriptTag("Tifinagh", "tfng");
            RegisterScriptTag("Tirhuta", "tirh");
            //
            RegisterScriptTag("Ugaritic Cuneiform", "ugar");
            //
            RegisterScriptTag("Vai", "vai");
            //
            RegisterScriptTag("Warang Citi", "wara");
            //
            RegisterScriptTag("Yi", "yi");

            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx"); 	

            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx"); 
            // RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");
            //RegisterScriptTag("xxxxxxxxxxx","xxxxxxxxxxx");  

        }
        public static TagInfo GetTagInfo(string shortname)
        {
            TagInfo found;
            registerTags.TryGetValue(shortname, out found);
            return found;
        }
        static void RegisterScriptTag(string fullname, string shortname)
        {
#if DEBUG
            if (shortname.Length > 4)
            {
                throw new NotSupportedException();
            }
#endif

            if (registerTags.ContainsKey(shortname))
            {
                //TODO: fix this
                if (shortname == "kana")
                {

                    return;
                }
            }
            registerTags.Add(shortname, new TagInfo(TagKind.Script, shortname, fullname));
        }


    }

    enum TagKind
    {
        Script
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