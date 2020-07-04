//MIT, 2016-present, WinterDev
using System;
using System.Collections.Generic;
using Typography.TextBreak;

namespace Typography.OpenFont
{
    public class ScriptLangInfo
    {
        public readonly string fullname;
        public readonly string shortname;
        public UnicodeRangeInfo[] unicodeLangs;
        public ScriptLangInfo(string fullname, string shortname)
        {
            this.fullname = fullname;
            this.shortname = shortname;
        }
        public ScriptLangInfo(ScriptTagDef tagDef, params UnicodeRangeInfo[] unicodeLangs)
        {
            fullname = tagDef.Name;
            shortname = TagUtils.TagToString(tagDef.Tag);
            this.unicodeLangs = unicodeLangs;
        }
#if DEBUG

        public override string ToString()
        {
            return this.fullname;
        }
#endif
    }


    public class LangSys
    {
        public string Name { get; }
        public string Tag { get; }

        readonly string[] _iso639ids;
        public LangSys(string name, string tag, string[] iso639ids)
        {
            Name = name;
            Tag = tag;
            _iso639ids = iso639ids;
        }

    }
    public static partial class LanguageSystemNames
    {
        static Dictionary<string, LangSys> s_registeredLangs;

        //https://docs.microsoft.com/en-us/typography/opentype/spec/languagetags

        static LangSys _(string name, string tag, string iso639ids)
        {
            if (tag.Length < 4)
            {
                tag = tag.PadRight(4, ' ');
            }

            string[] splited = iso639ids.Split(',');
            var langSys = new LangSys(name, tag, splited);

            if (s_registeredLangs == null) { s_registeredLangs = new Dictionary<string, LangSys>(); }

            if (!s_registeredLangs.ContainsKey(tag))
            {
                s_registeredLangs.Add(tag, langSys);
            }
            else
            {
            }
            return langSys;
        }
        public static bool TryGetLangSystem(string tag, out LangSys found)
        {
            if (s_registeredLangs == null)
            {
#if DEBUG

#endif

                found = null;
                return false;
            }
            if (tag.Length < 4)
            {
                tag = tag.PadRight(4, ' ');
            }
            return s_registeredLangs.TryGetValue(tag, out found);
        }

        public static IEnumerable<LangSys> GetLangSysIter()
        {
            if (s_registeredLangs != null)
            {
                foreach (LangSys langSys in s_registeredLangs.Values)
                {
                    yield return langSys;
                }
            }
        }
    }


    partial class LanguageSystemNames
    {
        //AUTOGEN: see Typography\Typography.TextBreak\Tools

        public static readonly LangSys
     Abaza_ABA = _("Abaza", "ABA", "abq"),
    Abkhazian_ABK = _("Abkhazian", "ABK", "abk"),
    Acholi_ACH = _("Acholi", "ACH", "ach"),
    Achi_ACR = _("Achi", "ACR", "acr"),
    Adyghe_ADY = _("Adyghe", "ADY", "ady"),
    Afrikaans_AFK = _("Afrikaans", "AFK", "afr"),
    Afar_AFR = _("Afar", "AFR", "aar"),
    Agaw_AGW = _("Agaw", "AGW", "ahg"),
    Aiton_AIO = _("Aiton", "AIO", "aio"),
    Akan_AKA = _("Akan", "AKA", "aka"),
    Alsatian_ALS = _("Alsatian", "ALS", "gsw"),
    Altai_ALT = _("Altai", "ALT", "atv, alt"),
    Amharic_AMH = _("Amharic", "AMH", "amh"),
    Anglo_Saxon_ANG = _("Anglo-Saxon", "ANG", "ang"),
    Phonetic_transcription_Americanist_conventions_APPH = _("Phonetic transcription—Americanist conventions", "APPH", ""),
    Arabic_ARA = _("Arabic", "ARA", "ara"),
    Aragonese_ARG = _("Aragonese", "ARG", "arg"),
    Aari_ARI = _("Aari", "ARI", "aiw"),
    Rakhine_ARK = _("Rakhine", "ARK", "mhv, rmz, rki"),
    Assamese_ASM = _("Assamese", "ASM", "asm"),
    Asturian_AST = _("Asturian", "AST", "ast"),
    Athapaskan_ATH = _("Athapaskan", "ATH", "apk, apj, apl, apm, apw, nav, bea, sek, bcr, caf, crx, clc, gwi, haa, chp, dgr, scs, xsl, srs, ing, hoi, koy, hup, ktw, mvb, wlk, coq, ctc, gce, tol, tuu, kkz, tgx, tht, aht, tfn, taa, tau, tcb, kuu, tce, ttm, txc"),
    Avar_AVR = _("Avar", "AVR", "ava"),
    Awadhi_AWA = _("Awadhi", "AWA", "awa"),
    Aymara_AYM = _("Aymara", "AYM", "aym"),
    Torki_AZB = _("Torki", "AZB", "azb"),
    Azerbaijani_AZE = _("Azerbaijani", "AZE", "aze"),
    Badaga_BAD = _("Badaga", "BAD", "bfq"),
    Banda_BAD0 = _("Banda", "BAD0", "bad"),
    Baghelkhandi_BAG = _("Baghelkhandi", "BAG", "bfy"),
    Balkar_BAL = _("Balkar", "BAL", "krc"),
    Balinese_BAN = _("Balinese", "BAN", "ban"),
    Bavarian_BAR = _("Bavarian", "BAR", "bar"),
    Baulé_BAU = _("Baulé", "BAU", "bci"),
    Batak_Toba_BBC = _("Batak Toba", "BBC", "bbc"),
    Berber_BBR = _("Berber", "BBR", ""),
    Bench_BCH = _("Bench", "BCH", "bcq"),
    Bible_Cree_BCR = _("Bible Cree", "BCR", ""),
    Bandjalang_BDY = _("Bandjalang", "BDY", "bdy"),
    Belarussian_BEL = _("Belarussian", "BEL", "bel"),
    Bemba_BEM = _("Bemba", "BEM", "bem"),
    Bengali_BEN = _("Bengali", "BEN", "ben"),
    Haryanvi_BGC = _("Haryanvi", "BGC", "bgc"),
    Bagri_BGQ = _("Bagri", "BGQ", "bgq"),
    Bulgarian_BGR = _("Bulgarian", "BGR", "bul"),
    Bhili_BHI = _("Bhili", "BHI", "bhi, bhb"),
    Bhojpuri_BHO = _("Bhojpuri", "BHO", "bho"),
    Bikol_BIK = _("Bikol", "BIK", "bik, bhk, bcl, bto, cts, bln, fbl, lbl, rbl, ubl"),
    Bilen_BIL = _("Bilen", "BIL", "byn"),
    Bislama_BIS = _("Bislama", "BIS", "bis"),
    Kanauji_BJJ = _("Kanauji", "BJJ", "bjj"),
    Blackfoot_BKF = _("Blackfoot", "BKF", "bla"),
    Baluchi_BLI = _("Baluchi", "BLI", "bal"),
    Pa_o_Karen_BLK = _("Pa’o Karen", "BLK", "blk"),
    Balante_BLN = _("Balante", "BLN", "bjt, ble"),
    Balti_BLT = _("Balti", "BLT", "bft"),
    Bambara__Bamanankan__BMB = _("Bambara (Bamanankan)", "BMB", "bam"),
    Bamileke_BML = _("Bamileke", "BML", ""),
    Bosnian_BOS = _("Bosnian", "BOS", "bos"),
    Bishnupriya_Manipuri_BPY = _("Bishnupriya Manipuri", "BPY", "bpy"),
    Breton_BRE = _("Breton", "BRE", "bre"),
    Brahui_BRH = _("Brahui", "BRH", "brh"),
    Braj_Bhasha_BRI = _("Braj Bhasha", "BRI", "bra"),
    Burmese_BRM = _("Burmese", "BRM", "mya"),
    Bodo_BRX = _("Bodo", "BRX", "brx"),
    Bashkir_BSH = _("Bashkir", "BSH", "bak"),
    Burushaski_BSK = _("Burushaski", "BSK", "bsk"),
    Beti_BTI = _("Beti", "BTI", "btb, beb, bum, bxp, eto, ewo, mct"),
    Batak_Simalungun_BTS = _("Batak Simalungun", "BTS", "bts"),
    Bugis_BUG = _("Bugis", "BUG", "bug"),
    Medumba_BYV = _("Medumba", "BYV", "byv"),
    Kaqchikel_CAK = _("Kaqchikel", "CAK", "cak"),
    Catalan_CAT = _("Catalan", "CAT", "cat"),
    Zamboanga_Chavacano_CBK = _("Zamboanga Chavacano", "CBK", "cbk"),
    Chinantec_CCHN = _("Chinantec", "CCHN", "cco, chj, chq, chz, cle, cnl, cnt, cpa, csa, cso, cte, ctl, cuc, cvn"),
    Cebuano_CEB = _("Cebuano", "CEB", "ceb"),
    Chechen_CHE = _("Chechen", "CHE", "che"),
    Chaha_Gurage_CHG = _("Chaha Gurage", "CHG", "sgw"),
    Chattisgarhi_CHH = _("Chattisgarhi", "CHH", "hne"),
    Chichewa__Chewa__Nyanja__CHI = _("Chichewa (Chewa, Nyanja)", "CHI", "nya"),
    Chukchi_CHK = _("Chukchi", "CHK", "ckt"),
    Chuukese_CHK0 = _("Chuukese", "CHK0", "chk"),
    Choctaw_CHO = _("Choctaw", "CHO", "cho"),
    Chipewyan_CHP = _("Chipewyan", "CHP", "chp"),
    Cherokee_CHR = _("Cherokee", "CHR", "chr"),
    Chamorro_CHA = _("Chamorro", "CHA", "cha"),
    Chuvash_CHU = _("Chuvash", "CHU", "chv"),
    Cheyenne_CHY = _("Cheyenne", "CHY", "chy"),
    Chiga_CGG = _("Chiga", "CGG", "cgg"),
    Western_Cham_CJA = _("Western Cham", "CJA", "cja"),
    Eastern_Cham_CJM = _("Eastern Cham", "CJM", "cjm"),
    Comorian_CMR = _("Comorian", "CMR", "swb, wlc, wni, zdj"),
    Coptic_COP = _("Coptic", "COP", "cop"),
    Cornish_COR = _("Cornish", "COR", "cor"),
    Corsican_COS = _("Corsican", "COS", "cos"),
    Creoles_CPP = _("Creoles", "CPP", "crp, cpe, cpf, cpp"),
    Cree_CRE = _("Cree", "CRE", "cre"),
    Carrier_CRR = _("Carrier", "CRR", "crx, caf"),
    Crimean_Tatar_CRT = _("Crimean Tatar", "CRT", "crh"),
    Kashubian_CSB = _("Kashubian", "CSB", "csb"),
    Church_Slavonic_CSL = _("Church Slavonic", "CSL", "chu"),
    Czech_CSY = _("Czech", "CSY", "ces"),
    Chittagonian_CTG = _("Chittagonian", "CTG", "ctg"),
    San_Blas_Kuna_CUK = _("San Blas Kuna", "CUK", "cuk"),
    Danish_DAN = _("Danish", "DAN", "dan"),
    Dargwa_DAR = _("Dargwa", "DAR", "dar"),
    Dayi_DAX = _("Dayi", "DAX", "dax"),
    Woods_Cree_DCR = _("Woods Cree", "DCR", "cwd"),
    German_DEU = _("German", "DEU", "deu"),
    Dogri_DGO = _("Dogri", "DGO", "dgo"),
    Dogri_DGR = _("Dogri", "DGR", "doi"),
    Dhangu_DHG = _("Dhangu", "DHG", "dhg"),
    Divehi__Dhivehi__Maldivian__DHV = _("Divehi (Dhivehi, Maldivian)", "DHV", "(deprecated)	div"),
    Dimli_DIQ = _("Dimli", "DIQ", "diq"),
    Divehi__Dhivehi__Maldivian__DIV = _("Divehi (Dhivehi, Maldivian)", "DIV", "div"),
    Zarma_DJR = _("Zarma", "DJR", "dje"),
    Djambarrpuyngu_DJR0 = _("Djambarrpuyngu", "DJR0", "djr"),
    Dangme_DNG = _("Dangme", "DNG", "ada"),
    Dan_DNJ = _("Dan", "DNJ", "dnj"),
    Dinka_DNK = _("Dinka", "DNK", "din"),
    Dari_DRI = _("Dari", "DRI", "prs"),
    Dhuwal_DUJ = _("Dhuwal", "DUJ", "duj, dwu, dwy"),
    Dungan_DUN = _("Dungan", "DUN", "dng"),
    Dzongkha_DZN = _("Dzongkha", "DZN", "dzo"),
    Ebira_EBI = _("Ebira", "EBI", "igb"),
    Eastern_Cree_ECR = _("Eastern Cree", "ECR", "crj, crl"),
    Edo_EDO = _("Edo", "EDO", "bin"),
    Efik_EFI = _("Efik", "EFI", "efi"),
    Greek_ELL = _("Greek", "ELL", "ell"),
    Eastern_Maninkakan_EMK = _("Eastern Maninkakan", "EMK", "emk"),
    English_ENG = _("English", "ENG", "eng"),
    Erzya_ERZ = _("Erzya", "ERZ", "myv"),
    Spanish_ESP = _("Spanish", "ESP", "spa"),
    Central_Yupik_ESU = _("Central Yupik", "ESU", "esu"),
    Estonian_ETI = _("Estonian", "ETI", "est"),
    Basque_EUQ = _("Basque", "EUQ", "eus"),
    Evenki_EVK = _("Evenki", "EVK", "evn"),
    Even_EVN = _("Even", "EVN", "eve"),
    Ewe_EWE = _("Ewe", "EWE", "ewe"),
    French_Antillean_FAN = _("French Antillean", "FAN", "acf"),
    Fang_FAN0 = _("Fang", "FAN0", "fan"),
    Persian_FAR = _("Persian", "FAR", "fas"),
    Fanti_FAT = _("Fanti", "FAT", "fat"),
    Finnish_FIN = _("Finnish", "FIN", "fin"),
    Fijian_FJI = _("Fijian", "FJI", "fij"),
    Dutch__Flemish__FLE = _("Dutch (Flemish)", "FLE", "vls"),
    Fe_fe__FMP = _("Fe’fe’", "FMP", "fmp"),
    Forest_Nenets_FNE = _("Forest Nenets", "FNE", "enf, yrk"),
    Fon_FON = _("Fon", "FON", "fon"),
    Faroese_FOS = _("Faroese", "FOS", "fao"),
    French_FRA = _("French", "FRA", "fra"),
    Cajun_French_FRC = _("Cajun French", "FRC", "frc"),
    Frisian_FRI = _("Frisian", "FRI", "fry"),
    Friulian_FRL = _("Friulian", "FRL", "fur"),
    Arpitan_FRP = _("Arpitan", "FRP", "frp"),
    Futa_FTA = _("Futa", "FTA", "fuf"),
    Fulah_FUL = _("Fulah", "FUL", "ful"),
    Nigerian_Fulfulde_FUV = _("Nigerian Fulfulde", "FUV", "fuv"),
    Ga_GAD = _("Ga", "GAD", "gaa"),
    Scottish_Gaelic__Gaelic__GAE = _("Scottish Gaelic (Gaelic)", "GAE", "gla"),
    Gagauz_GAG = _("Gagauz", "GAG", "gag"),
    Galician_GAL = _("Galician", "GAL", "glg"),
    Garshuni_GAR = _("Garshuni", "GAR", ""),
    Garhwali_GAW = _("Garhwali", "GAW", "gbm"),
    Geez_GEZ = _("Geez", "GEZ", "gez"),
    Githabul_GIH = _("Githabul", "GIH", "gih"),
    Gilyak_GIL = _("Gilyak", "GIL", "niv"),
    Kiribati__Gilbertese__GIL0 = _("Kiribati (Gilbertese)", "GIL0", "gil"),
    Kpelle__Guinea__GKP = _("Kpelle (Guinea)", "GKP", "gkp"),
    Gilaki_GLK = _("Gilaki", "GLK", "glk"),
    Gumuz_GMZ = _("Gumuz", "GMZ", "guk"),
    Gumatj_GNN = _("Gumatj", "GNN", "gnn"),
    Gogo_GOG = _("Gogo", "GOG", "gog"),
    Gondi_GON = _("Gondi", "GON", "gon"),
    Greenlandic_GRN = _("Greenlandic", "GRN", "kal"),
    Garo_GRO = _("Garo", "GRO", "grt"),
    Guarani_GUA = _("Guarani", "GUA", "grn"),
    Wayuu_GUC = _("Wayuu", "GUC", "guc"),
    Gupapuyngu_GUF = _("Gupapuyngu", "GUF", "guf"),
    Gujarati_GUJ = _("Gujarati", "GUJ", "guj"),
    Gusii_GUZ = _("Gusii", "GUZ", "guz"),
    Haitian__Haitian_Creole__HAI = _("Haitian (Haitian Creole)", "HAI", "hat"),
    Halam__Falam_Chin__HAL = _("Halam (Falam Chin)", "HAL", "flm, cfm, rnl"),
    Harauti_HAR = _("Harauti", "HAR", "hoj"),
    Hausa_HAU = _("Hausa", "HAU", "hau"),
    Hawaiian_HAW = _("Hawaiian", "HAW", "haw"),
    Haya_HAY = _("Haya", "HAY", "hay"),
    Hazaragi_HAZ = _("Hazaragi", "HAZ", "haz"),
    Hammer_Banna_HBN = _("Hammer-Banna", "HBN", "amf"),
    Herero_HER = _("Herero", "HER", "her"),
    Hiligaynon_HIL = _("Hiligaynon", "HIL", "hil"),
    Hindi_HIN = _("Hindi", "HIN", "hin"),
    High_Mari_HMA = _("High Mari", "HMA", "mrj"),
    Hmong_HMN = _("Hmong", "HMN", "hmn"),
    Hiri_Motu_HMO = _("Hiri Motu", "HMO", "hmo"),
    Hindko_HND = _("Hindko", "HND", "hno, hnd"),
    Ho_HO = _("Ho", "HO", "hoc"),
    Harari_HRI = _("Harari", "HRI", "har"),
    Croatian_HRV = _("Croatian", "HRV", "hrv"),
    Hungarian_HUN = _("Hungarian", "HUN", "hun"),
    Armenian_HYE = _("Armenian", "HYE", "hye, hyw"),
    Armenian_East_HYE0 = _("Armenian East", "HYE0", "hye"),
    Iban_IBA = _("Iban", "IBA", "iba"),
    Ibibio_IBB = _("Ibibio", "IBB", "ibb"),
    Igbo_IBO = _("Igbo", "IBO", "ibo"),
    Ijo_languages_IJO = _("Ijo languages", "IJO", "ijc, ijo"),
    Ido_IDO = _("Ido", "IDO", "ido"),
    Interlingue_ILE = _("Interlingue", "ILE", "ile"),
    Ilokano_ILO = _("Ilokano", "ILO", "ilo"),
    Interlingua_INA = _("Interlingua", "INA", "ina"),
    Indonesian_IND = _("Indonesian", "IND", "ind"),
    Ingush_ING = _("Ingush", "ING", "inh"),
    Inuktitut_INU = _("Inuktitut", "INU", "iku"),
    Inupiat_IPK = _("Inupiat", "IPK", "ipk"),
    Phonetic_transcription_IPA_conventions_IPPH = _("Phonetic transcription—IPA conventions", "IPPH", ""),
    Irish_IRI = _("Irish", "IRI", "gle"),
    Irish_Traditional_IRT = _("Irish Traditional", "IRT", "gle"),
    Icelandic_ISL = _("Icelandic", "ISL", "isl"),
    Inari_Sami_ISM = _("Inari Sami", "ISM", "smn"),
    Italian_ITA = _("Italian", "ITA", "ita"),
    Hebrew_IWR = _("Hebrew", "IWR", "heb"),
    Jamaican_Creole_JAM = _("Jamaican Creole", "JAM", "jam"),
    Japanese_JAN = _("Japanese", "JAN", "jpn"),
    Javanese_JAV = _("Javanese", "JAV", "jav"),
    Lojban_JBO = _("Lojban", "JBO", "jbo"),
    Krymchak_JCT = _("Krymchak", "JCT", "jct"),
    Yiddish_JII = _("Yiddish", "JII", "yid"),
    Ladino_JUD = _("Ladino", "JUD", "lad"),
    Jula_JUL = _("Jula", "JUL", "dyu"),
    Kabardian_KAB = _("Kabardian", "KAB", "kbd"),
    Kabyle_KAB0 = _("Kabyle", "KAB0", "kab"),
    Kachchi_KAC = _("Kachchi", "KAC", "kfr"),
    Kalenjin_KAL = _("Kalenjin", "KAL", "kln"),
    Kannada_KAN = _("Kannada", "KAN", "kan"),
    Karachay_KAR = _("Karachay", "KAR", "krc"),
    Georgian_KAT = _("Georgian", "KAT", "kat"),
    Kazakh_KAZ = _("Kazakh", "KAZ", "kaz"),
    Makonde_KDE = _("Makonde", "KDE", "kde"),
    Kabuverdianu__Crioulo__KEA = _("Kabuverdianu (Crioulo)", "KEA", "kea"),
    Kebena_KEB = _("Kebena", "KEB", "ktb"),
    Kekchi_KEK = _("Kekchi", "KEK", "kek"),
    Khutsuri_Georgian_KGE = _("Khutsuri Georgian", "KGE", "kat"),
    Khakass_KHA = _("Khakass", "KHA", "kjh"),
    Khanty_Kazim_KHK = _("Khanty-Kazim", "KHK", "kca"),
    Khmer_KHM = _("Khmer", "KHM", "khm"),
    Khanty_Shurishkar_KHS = _("Khanty-Shurishkar", "KHS", "kca"),
    Khamti_Shan_KHT = _("Khamti Shan", "KHT", "kht"),
    Khanty_Vakhi_KHV = _("Khanty-Vakhi", "KHV", "kca"),
    Khowar_KHW = _("Khowar", "KHW", "khw"),
    Kikuyu__Gikuyu__KIK = _("Kikuyu (Gikuyu)", "KIK", "kik"),
    Kirghiz__Kyrgyz__KIR = _("Kirghiz (Kyrgyz)", "KIR", "kir"),
    Kisii_KIS = _("Kisii", "KIS", "kqs, kss"),
    Kirmanjki_KIU = _("Kirmanjki", "KIU", "kiu"),
    Southern_Kiwai_KJD = _("Southern Kiwai", "KJD", "kjd"),
    Eastern_Pwo_Karen_KJP = _("Eastern Pwo Karen", "KJP", "kjp"),
    Bumthangkha_KJZ = _("Bumthangkha", "KJZ", "kjz"),
    Kokni_KKN = _("Kokni", "KKN", "kex"),
    Kalmyk_KLM = _("Kalmyk", "KLM", "xal"),
    Kamba_KMB = _("Kamba", "KMB", "kam"),
    Kumaoni_KMN = _("Kumaoni", "KMN", "kfy"),
    Komo_KMO = _("Komo", "KMO", "kmw"),
    Komso_KMS = _("Komso", "KMS", "kxc"),
    Khorasani_Turkic_KMZ = _("Khorasani Turkic", "KMZ", "kmz"),
    Kanuri_KNR = _("Kanuri", "KNR", "kau"),
    Kodagu_KOD = _("Kodagu", "KOD", "kfa"),
    Korean_Old_Hangul_KOH = _("Korean Old Hangul", "KOH", "okm"),
    Konkani_KOK = _("Konkani", "KOK", "kok"),
    Kikongo_KON = _("Kikongo", "KON", "ktu"),
    Komi_KOM = _("Komi", "KOM", "kom"),
    Kongo_KON0 = _("Kongo", "KON0", "kon"),
    Komi_Permyak_KOP = _("Komi-Permyak", "KOP", "koi"),
    Korean_KOR = _("Korean", "KOR", "kor"),
    Kosraean_KOS = _("Kosraean", "KOS", "kos"),
    Komi_Zyrian_KOZ = _("Komi-Zyrian", "KOZ", "kpv"),
    Kpelle_KPL = _("Kpelle", "KPL", "kpe"),
    Krio_KRI = _("Krio", "KRI", "kri"),
    Karakalpak_KRK = _("Karakalpak", "KRK", "kaa"),
    Karelian_KRL = _("Karelian", "KRL", "krl"),
    Karaim_KRM = _("Karaim", "KRM", "kdr"),
    Karen_KRN = _("Karen", "KRN", "kar"),
    Koorete_KRT = _("Koorete", "KRT", "kqy"),
    Kashmiri_KSH = _("Kashmiri", "KSH", "kas"),
    Ripuarian_KSH0 = _("Ripuarian", "KSH0", "ksh"),
    Khasi_KSI = _("Khasi", "KSI", "kha"),
    Kildin_Sami_KSM = _("Kildin Sami", "KSM", "sjd"),
    S_gaw_Karen_KSW = _("S’gaw Karen", "KSW", "ksw"),
    Kuanyama_KUA = _("Kuanyama", "KUA", "kua"),
    Kui_KUI = _("Kui", "KUI", "kxu"),
    Kulvi_KUL = _("Kulvi", "KUL", "kfx"),
    Kumyk_KUM = _("Kumyk", "KUM", "kum"),
    Kurdish_KUR = _("Kurdish", "KUR", "kur"),
    Kurukh_KUU = _("Kurukh", "KUU", "kru"),
    Kuy_KUY = _("Kuy", "KUY", "kdt"),
    Koryak_KYK = _("Koryak", "KYK", "kpy"),
    Western_Kayah_KYU = _("Western Kayah", "KYU", "kyu"),
    Ladin_LAD = _("Ladin", "LAD", "lld"),
    Lahuli_LAH = _("Lahuli", "LAH", "bfu"),
    Lak_LAK = _("Lak", "LAK", "lbe"),
    Lambani_LAM = _("Lambani", "LAM", "lmn"),
    Lao_LAO = _("Lao", "LAO", "lao"),
    Latin_LAT = _("Latin", "LAT", "lat"),
    Laz_LAZ = _("Laz", "LAZ", "lzz"),
    L_Cree_LCR = _("L-Cree", "LCR", "crm"),
    Ladakhi_LDK = _("Ladakhi", "LDK", "lbj"),
    Lezgi_LEZ = _("Lezgi", "LEZ", "lez"),
    Ligurian_LIJ = _("Ligurian", "LIJ", "lij"),
    Limburgish_LIM = _("Limburgish", "LIM", "lim"),
    Lingala_LIN = _("Lingala", "LIN", "lin"),
    Lisu_LIS = _("Lisu", "LIS", "lis"),
    Lampung_LJP = _("Lampung", "LJP", "ljp"),
    Laki_LKI = _("Laki", "LKI", "lki"),
    Low_Mari_LMA = _("Low Mari", "LMA", "mhr"),
    Limbu_LMB = _("Limbu", "LMB", "lif"),
    Lombard_LMO = _("Lombard", "LMO", "lmo"),
    Lomwe_LMW = _("Lomwe", "LMW", "ngl"),
    Loma_LOM = _("Loma", "LOM", "lom"),
    Luri_LRC = _("Luri", "LRC", "lrc, luz, bqi, zum"),
    Lower_Sorbian_LSB = _("Lower Sorbian", "LSB", "dsb"),
    Lule_Sami_LSM = _("Lule Sami", "LSM", "smj"),
    Lithuanian_LTH = _("Lithuanian", "LTH", "lit"),
    Luxembourgish_LTZ = _("Luxembourgish", "LTZ", "ltz"),
    Luba_Lulua_LUA = _("Luba-Lulua", "LUA", "lua"),
    Luba_Katanga_LUB = _("Luba-Katanga", "LUB", "lub"),
    Ganda_LUG = _("Ganda", "LUG", "lug"),
    Luyia_LUH = _("Luyia", "LUH", "luy"),
    Luo_LUO = _("Luo", "LUO", "luo"),
    Latvian_LVI = _("Latvian", "LVI", "lav"),
    Madura_MAD = _("Madura", "MAD", "mad"),
    Magahi_MAG = _("Magahi", "MAG", "mag"),
    Marshallese_MAH = _("Marshallese", "MAH", "mah"),
    Majang_MAJ = _("Majang", "MAJ", "mpe"),
    Makhuwa_MAK = _("Makhuwa", "MAK", "vmw"),
    Malayalam_MAL = _("Malayalam", "MAL", "mal"),
    Mam_MAM = _("Mam", "MAM", "mam"),
    Mansi_MAN = _("Mansi", "MAN", "mns"),
    Mapudungun_MAP = _("Mapudungun", "MAP", "arn"),
    Marathi_MAR = _("Marathi", "MAR", "mar"),
    Marwari_MAW = _("Marwari", "MAW", "mwr, dhd, rwr, mve, wry, mtr, swv"),
    Mbundu_MBN = _("Mbundu", "MBN", "kmb"),
    Mbo_MBO = _("Mbo", "MBO", "mbo"),
    Manchu_MCH = _("Manchu", "MCH", "mnc"),
    Moose_Cree_MCR = _("Moose Cree", "MCR", "crm"),
    Mende_MDE = _("Mende", "MDE", "men"),
    Mandar_MDR = _("Mandar", "MDR", "mdr"),
    Me_en_MEN = _("Me’en", "MEN", "mym"),
    Meru_MER = _("Meru", "MER", "mer"),
    Pattani_Malay_MFA = _("Pattani Malay", "MFA", "mfa"),
    Morisyen_MFE = _("Morisyen", "MFE", "mfe"),
    Minangkabau_MIN = _("Minangkabau", "MIN", "min"),
    Mizo_MIZ = _("Mizo", "MIZ", "lus"),
    Macedonian_MKD = _("Macedonian", "MKD", "mkd"),
    Makasar_MKR = _("Makasar", "MKR", "mak"),
    Kituba_MKW = _("Kituba", "MKW", "mkw"),
    Male_MLE = _("Male", "MLE", "mdy"),
    Malagasy_MLG = _("Malagasy", "MLG", "mlg"),
    Malinke_MLN = _("Malinke", "MLN", "mlq"),
    Malayalam_Reformed_MLR = _("Malayalam Reformed", "MLR", "mal"),
    Malay_MLY = _("Malay", "MLY", "msa"),
    Mandinka_MND = _("Mandinka", "MND", "mnk"),
    Mongolian_MNG = _("Mongolian", "MNG", "mon"),
    Manipuri_MNI = _("Manipuri", "MNI", "mni"),
    Maninka_MNK = _("Maninka", "MNK", "man, mnk, myq, mku, msc, emk, mwk, mlq"),
    Manx_MNX = _("Manx", "MNX", "glv"),
    Mohawk_MOH = _("Mohawk", "MOH", "moh"),
    Moksha_MOK = _("Moksha", "MOK", "mdf"),
    Moldavian_MOL = _("Moldavian", "MOL", "mol"),
    Mon_MON = _("Mon", "MON", "mnw"),
    Moroccan_MOR = _("Moroccan", "MOR", ""),
    Mossi_MOS = _("Mossi", "MOS", "mos"),
    Maori_MRI = _("Maori", "MRI", "mri"),
    Maithili_MTH = _("Maithili", "MTH", "mai"),
    Maltese_MTS = _("Maltese", "MTS", "mlt"),
    Mundari_MUN = _("Mundari", "MUN", "unr"),
    Muscogee_MUS = _("Muscogee", "MUS", "mus"),
    Mirandese_MWL = _("Mirandese", "MWL", "mwl"),
    Hmong_Daw_MWW = _("Hmong Daw", "MWW", "mww"),
    Mayan_MYN = _("Mayan", "MYN", "myn"),
    Mazanderani_MZN = _("Mazanderani", "MZN", "mzn"),
    Naga_Assamese_NAG = _("Naga-Assamese", "NAG", "nag"),
    Nahuatl_NAH = _("Nahuatl", "NAH", "nah"),
    Nanai_NAN = _("Nanai", "NAN", "gld"),
    Neapolitan_NAP = _("Neapolitan", "NAP", "nap"),
    Naskapi_NAS = _("Naskapi", "NAS", "nsk"),
    Nauruan_NAU = _("Nauruan", "NAU", "nau"),
    Navajo_NAV = _("Navajo", "NAV", "nav"),
    N_Cree_NCR = _("N-Cree", "NCR", "csw"),
    Ndebele_NDB = _("Ndebele", "NDB", "nbl, nde"),
    Ndau_NDC = _("Ndau", "NDC", "ndc"),
    Ndonga_NDG = _("Ndonga", "NDG", "ndo"),
    Low_Saxon_NDS = _("Low Saxon", "NDS", "nds"),
    Nepali_NEP = _("Nepali", "NEP", "nep"),
    Newari_NEW = _("Newari", "NEW", "new"),
    Ngbaka_NGA = _("Ngbaka", "NGA", "nga"),
    Nagari_NGR = _("Nagari", "NGR", ""),
    Norway_House_Cree_NHC = _("Norway House Cree", "NHC", "csw"),
    Nisi_NIS = _("Nisi", "NIS", "dap, njz, tgj"),
    Niuean_NIU = _("Niuean", "NIU", "niu"),
    Nyankole_NKL = _("Nyankole", "NKL", "nyn"),
    N_Ko_NKO = _("N’Ko", "NKO", "nqo"),
    Dutch_NLD = _("Dutch", "NLD", "nld"),
    Nimadi_NOE = _("Nimadi", "NOE", "noe"),
    Nogai_NOG = _("Nogai", "NOG", "nog"),
    Norwegian_NOR = _("Norwegian", "NOR", "nob"),
    Novial_NOV = _("Novial", "NOV", "nov"),
    Northern_Sami_NSM = _("Northern Sami", "NSM", "sme"),
    Sotho__Northern_NSO = _("Sotho, Northern", "NSO", "nso"),
    Northern_Tai_NTA = _("Northern Tai", "NTA", "nod"),
    Esperanto_NTO = _("Esperanto", "NTO", "epo"),
    Nyamwezi_NYM = _("Nyamwezi", "NYM", "nym"),
    Norwegian_Nynorsk__Nynorsk__Norwegian__NYN = _("Norwegian Nynorsk (Nynorsk, Norwegian)", "NYN", "nno"),
    Mbembe_Tigon_NZA = _("Mbembe Tigon", "NZA", "nza"),
    Occitan_OCI = _("Occitan", "OCI", "oci"),
    Oji_Cree_OCR = _("Oji-Cree", "OCR", "ojs"),
    Ojibway_OJB = _("Ojibway", "OJB", "oji"),
    Odia__formerly_Oriya__ORI = _("Odia (formerly Oriya)", "ORI", "ori"),
    Oromo_ORO = _("Oromo", "ORO", "orm"),
    Ossetian_OSS = _("Ossetian", "OSS", "oss"),
    Palestinian_Aramaic_PAA = _("Palestinian Aramaic", "PAA", "sam"),
    Pangasinan_PAG = _("Pangasinan", "PAG", "pag"),
    Pali_PAL = _("Pali", "PAL", "pli"),
    Pampangan_PAM = _("Pampangan", "PAM", "pam"),
    Punjabi_PAN = _("Punjabi", "PAN", "pan"),
    Palpa_PAP = _("Palpa", "PAP", "plp"),
    Papiamentu_PAP0 = _("Papiamentu", "PAP0", "pap"),
    Pashto_PAS = _("Pashto", "PAS", "pus"),
    Palauan_PAU = _("Palauan", "PAU", "pau"),
    Bouyei_PCC = _("Bouyei", "PCC", "pcc"),
    Picard_PCD = _("Picard", "PCD", "pcd"),
    Pennsylvania_German_PDC = _("Pennsylvania German", "PDC", "pdc"),
    Polytonic_Greek_PGR = _("Polytonic Greek", "PGR", "ell"),
    Phake_PHK = _("Phake", "PHK", "phk"),
    Norfolk_PIH = _("Norfolk", "PIH", "pih"),
    Filipino_PIL = _("Filipino", "PIL", "fil"),
    Palaung_PLG = _("Palaung", "PLG", "pce, rbb, pll"),
    Polish_PLK = _("Polish", "PLK", "pol"),
    Piemontese_PMS = _("Piemontese", "PMS", "pms"),
    Western_Panjabi_PNB = _("Western Panjabi", "PNB", "pnb"),
    Pocomchi_POH = _("Pocomchi", "POH", "poh"),
    Pohnpeian_PON = _("Pohnpeian", "PON", "pon"),
    Provençal___Old_Provençal_PRO = _("Provençal / Old Provençal", "PRO", "pro"),
    Portuguese_PTG = _("Portuguese", "PTG", "por"),
    Western_Pwo_Karen_PWO = _("Western Pwo Karen", "PWO", "pwo"),
    Chin_QIN = _("Chin", "QIN", "bgr, cnh, cnw, czt, sez, tcp, csy, ctd, flm, pck, tcz, zom, cmr, dao, hlt, cka, cnk, mrh, cbl, cnb, csh"),
    K_iche__QUC = _("K’iche’", "QUC", "quc"),
    Quechua__Bolivia__QUH = _("Quechua (Bolivia)", "QUH", "quh"),
    Quechua_QUZ = _("Quechua", "QUZ", "quz"),
    Quechua__Ecuador__QVI = _("Quechua (Ecuador)", "QVI", "qvi"),
    Quechua__Peru__QWH = _("Quechua (Peru)", "QWH", "qwh"),
    Rajasthani_RAJ = _("Rajasthani", "RAJ", "raj"),
    Rarotongan_RAR = _("Rarotongan", "RAR", "rar"),
    Russian_Buriat_RBU = _("Russian Buriat", "RBU", "bxr"),
    R_Cree_RCR = _("R-Cree", "RCR", "atj"),
    Rejang_REJ = _("Rejang", "REJ", "rej"),
    Riang_RIA = _("Riang", "RIA", "ria"),
    Tarifit_RIF = _("Tarifit", "RIF", "rif"),
    Ritarungo_RIT = _("Ritarungo", "RIT", "rit"),
    Arakwal_RKW = _("Arakwal", "RKW", "rkw"),
    Romansh_RMS = _("Romansh", "RMS", "roh"),
    Vlax_Romani_RMY = _("Vlax Romani", "RMY", "rmy"),
    Romanian_ROM = _("Romanian", "ROM", "ron"),
    Romany_ROY = _("Romany", "ROY", "rom"),
    Rusyn_RSY = _("Rusyn", "RSY", "rue"),
    Rotuman_RTM = _("Rotuman", "RTM", "rtm"),
    Kinyarwanda_RUA = _("Kinyarwanda", "RUA", "kin"),
    Rundi_RUN = _("Rundi", "RUN", "run"),
    Aromanian_RUP = _("Aromanian", "RUP", "rup"),
    Russian_RUS = _("Russian", "RUS", "rus"),
    Sadri_SAD = _("Sadri", "SAD", "sck"),
    Sanskrit_SAN = _("Sanskrit", "SAN", "san"),
    Sasak_SAS = _("Sasak", "SAS", "sas"),
    Santali_SAT = _("Santali", "SAT", "sat"),
    Sayisi_SAY = _("Sayisi", "SAY", "chp"),
    Sicilian_SCN = _("Sicilian", "SCN", "scn"),
    Scots_SCO = _("Scots", "SCO", "sco"),
    Sekota_SEK = _("Sekota", "SEK", "xan"),
    Selkup_SEL = _("Selkup", "SEL", "sel"),
    Old_Irish_SGA = _("Old Irish", "SGA", "sga"),
    Sango_SGO = _("Sango", "SGO", "sag"),
    Samogitian_SGS = _("Samogitian", "SGS", "sgs"),
    Tachelhit_SHI = _("Tachelhit", "SHI", "shi"),
    Shan_SHN = _("Shan", "SHN", "shn"),
    Sibe_SIB = _("Sibe", "SIB", "sjo"),
    Sidamo_SID = _("Sidamo", "SID", "sid"),
    Silte_Gurage_SIG = _("Silte Gurage", "SIG", "xst, stv, wle"),
    Skolt_Sami_SKS = _("Skolt Sami", "SKS", "sms"),
    Slovak_SKY = _("Slovak", "SKY", "slk"),
    North_Slavey_SCS = _("North Slavey", "SCS", "scs"),
    Slavey_SLA = _("Slavey", "SLA", "scs, xsl"),
    Slovenian_SLV = _("Slovenian", "SLV", "slv"),
    Somali_SML = _("Somali", "SML", "som"),
    Samoan_SMO = _("Samoan", "SMO", "smo"),
    Sena_SNA = _("Sena", "SNA", "seh"),
    Shona_SNA0 = _("Shona", "SNA0", "sna"),
    Sindhi_SND = _("Sindhi", "SND", "snd"),
    Sinhala__Sinhalese__SNH = _("Sinhala (Sinhalese)", "SNH", "sin"),
    Soninke_SNK = _("Soninke", "SNK", "snk"),
    Sodo_Gurage_SOG = _("Sodo Gurage", "SOG", "gru"),
    Songe_SOP = _("Songe", "SOP", "sop"),
    Sotho__Southern_SOT = _("Sotho, Southern", "SOT", "sot"),
    Albanian_SQI = _("Albanian", "SQI", "sqi"),
    Serbian_SRB = _("Serbian", "SRB", "srp"),
    Sardinian_SRD = _("Sardinian", "SRD", "srd"),
    Saraiki_SRK = _("Saraiki", "SRK", "skr"),
    Serer_SRR = _("Serer", "SRR", "srr"),
    South_Slavey_SSL = _("South Slavey", "SSL", "xsl"),
    Southern_Sami_SSM = _("Southern Sami", "SSM", "sma"),
    Saterland_Frisian_STQ = _("Saterland Frisian", "STQ", "stq"),
    Sukuma_SUK = _("Sukuma", "SUK", "suk"),
    Sundanese_SUN = _("Sundanese", "SUN", "sun"),
    Suri_SUR = _("Suri", "SUR", "suq"),
    Svan_SVA = _("Svan", "SVA", "sva"),
    Swedish_SVE = _("Swedish", "SVE", "swe"),
    Swadaya_Aramaic_SWA = _("Swadaya Aramaic", "SWA", "aii"),
    Swahili_SWK = _("Swahili", "SWK", "swa"),
    Swati_SWZ = _("Swati", "SWZ", "ssw"),
    Sutu_SXT = _("Sutu", "SXT", "ngo"),
    Upper_Saxon_SXU = _("Upper Saxon", "SXU", "sxu"),
    Sylheti_SYL = _("Sylheti", "SYL", "syl"),
    Syriac_SYR = _("Syriac", "SYR", "aii, amw, cld, syc, syr, tru"),
    Syriac__Estrangela_script_variant__equivalent_to_ISO_15924_Syre__SYRE = _("Syriac, Estrangela script-variant (equivalent to ISO 15924 Syre)", "SYRE", "syc, syr"),
    Syriac__Western_script_variant__equivalent_to_ISO_15924_Syrj__SYRJ = _("Syriac, Western script-variant (equivalent to ISO 15924 Syrj)", "SYRJ", "syc, syr"),
    Syriac__Eastern_script_variant__equivalent_to_ISO_15924_Syrn__SYRN = _("Syriac, Eastern script-variant (equivalent to ISO 15924 Syrn)", "SYRN", "syc, syr"),
    Silesian_SZL = _("Silesian", "SZL", "szl"),
    Tabasaran_TAB = _("Tabasaran", "TAB", "tab"),
    Tajiki_TAJ = _("Tajiki", "TAJ", "tgk"),
    Tamil_TAM = _("Tamil", "TAM", "tam"),
    Tatar_TAT = _("Tatar", "TAT", "tat"),
    TH_Cree_TCR = _("TH-Cree", "TCR", "cwd"),
    Dehong_Dai_TDD = _("Dehong Dai", "TDD", "tdd"),
    Telugu_TEL = _("Telugu", "TEL", "tel"),
    Tetum_TET = _("Tetum", "TET", "tet"),
    Tagalog_TGL = _("Tagalog", "TGL", "tgl"),
    Tongan_TGN = _("Tongan", "TGN", "ton"),
    Tigre_TGR = _("Tigre", "TGR", "tig"),
    Tigrinya_TGY = _("Tigrinya", "TGY", "tir"),
    Thai_THA = _("Thai", "THA", "tha"),
    Tahitian_THT = _("Tahitian", "THT", "tah"),
    Tibetan_TIB = _("Tibetan", "TIB", "bod"),
    Tiv_TIV = _("Tiv", "TIV", "tiv"),
    Turkmen_TKM = _("Turkmen", "TKM", "tuk"),
    Tamashek_TMH = _("Tamashek", "TMH", "tmh"),
    Temne_TMN = _("Temne", "TMN", "tem"),
    Tswana_TNA = _("Tswana", "TNA", "tsn"),
    Tundra_Nenets_TNE = _("Tundra Nenets", "TNE", "enh, yrk"),
    Tonga_TNG = _("Tonga", "TNG", "toi"),
    Todo_TOD = _("Todo", "TOD", "xal"),
    Toma_TOD0 = _("Toma", "TOD0", "tod"),
    Tok_Pisin_TPI = _("Tok Pisin", "TPI", "tpi"),
    Turkish_TRK = _("Turkish", "TRK", "tur"),
    Tsonga_TSG = _("Tsonga", "TSG", "tso"),
    Tshangla_TSJ = _("Tshangla", "TSJ", "tsj"),
    Turoyo_Aramaic_TUA = _("Turoyo Aramaic", "TUA", "tru"),
    Tulu_TUM = _("Tulu", "TUM", "tum"),
    Tumbuka_TUL = _("Tumbuka", "TUL", "tcy"),
    Tuvin_TUV = _("Tuvin", "TUV", "tyv"),
    Tuvalu_TVL = _("Tuvalu", "TVL", "tvl"),
    Twi_TWI = _("Twi", "TWI", "aka"),
    Tày_TYZ = _("Tày", "TYZ", "tyz"),
    Tamazight_TZM = _("Tamazight", "TZM", "tzm"),
    Tzotzil_TZO = _("Tzotzil", "TZO", "tzo"),
    Udmurt_UDM = _("Udmurt", "UDM", "udm"),
    Ukrainian_UKR = _("Ukrainian", "UKR", "ukr"),
    Umbundu_UMB = _("Umbundu", "UMB", "umb"),
    Urdu_URD = _("Urdu", "URD", "urd"),
    Upper_Sorbian_USB = _("Upper Sorbian", "USB", "hsb"),
    Uyghur_UYG = _("Uyghur", "UYG", "uig"),
    Uzbek_UZB = _("Uzbek", "UZB", "uzb"),
    Venetian_VEC = _("Venetian", "VEC", "vec"),
    Venda_VEN = _("Venda", "VEN", "ven"),
    Vietnamese_VIT = _("Vietnamese", "VIT", "vie"),
    Volapük_VOL = _("Volapük", "VOL", "vol"),
    Võro_VRO = _("Võro", "VRO", "vro"),
    Wa_WA = _("Wa", "WA", "wbm"),
    Wagdi_WAG = _("Wagdi", "WAG", "wbr"),
    Waray_Waray_WAR = _("Waray-Waray", "WAR", "war"),
    West_Cree_WCR = _("West-Cree", "WCR", "crk"),
    Welsh_WEL = _("Welsh", "WEL", "cym"),
    Walloon_WLN = _("Walloon", "WLN", "wln"),
    Wolof_WLF = _("Wolof", "WLF", "wol"),
    Mewati_WTM = _("Mewati", "WTM", "wtm"),
    Lü_XBD = _("Lü", "XBD", "khb"),
    Khengkha_XKF = _("Khengkha", "XKF", "xkf"),
    Xhosa_XHS = _("Xhosa", "XHS", "xho"),
    Minjangbal_XJB = _("Minjangbal", "XJB", "xjb"),
    Soga_XOG = _("Soga", "XOG", "xog"),
    Kpelle__Liberia__XPE = _("Kpelle (Liberia)", "XPE", "xpe"),
    Sakha_YAK = _("Sakha", "YAK", "sah"),
    Yao_YAO = _("Yao", "YAO", "yao"),
    Yapese_YAP = _("Yapese", "YAP", "yap"),
    Yoruba_YBA = _("Yoruba", "YBA", "yor"),
    Y_Cree_YCR = _("Y-Cree", "YCR", "cre"),
    Yi_Classic_YIC = _("Yi Classic", "YIC", ""),
    Yi_Modern_YIM = _("Yi Modern", "YIM", "iii"),
    Zealandic_ZEA = _("Zealandic", "ZEA", "zea"),
    Standard_Moroccan_Tamazight_ZGH = _("Standard Moroccan Tamazight", "ZGH", "zgh"),
    Zhuang_ZHA = _("Zhuang", "ZHA", "zha"),
    Chinese__Hong_Kong_SAR_ZHH = _("Chinese, Hong Kong SAR", "ZHH", "zho"),
    Chinese_Phonetic_ZHP = _("Chinese Phonetic", "ZHP", "zho"),
    Chinese_Simplified_ZHS = _("Chinese Simplified", "ZHS", "zho"),
    Chinese_Traditional_ZHT = _("Chinese Traditional", "ZHT", "zho"),
    Zande_ZND = _("Zande", "ZND", "zne"),
    Zulu_ZUL = _("Zulu", "ZUL", "zul"),
    Zazaki_ZZA = _("Zazaki", "ZZA", "zza");
    }

    static class TagUtils
    {
        static byte GetByte(char c)
        {
            if (c >= 0 && c < 256)
            {
                return (byte)c;
            }
            return 0;
        }
        public static uint StringToTag(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length != 4)
            {
                return 0;
            }

            char[] buff = str.ToCharArray();
            byte b0 = GetByte(buff[0]);
            byte b1 = GetByte(buff[1]);
            byte b2 = GetByte(buff[2]);
            byte b3 = GetByte(buff[3]);

            return (uint)((b0 << 24) | (b1 << 16) | (b2 << 8) | b3);
        }

        public static string TagToString(uint tag)
        {
            byte[] bytes = BitConverter.GetBytes(tag);
            Array.Reverse(bytes);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }

    public class ScriptTagDef
    {
        public uint Tag { get; }
        public string Name { get; }
        public ScriptTagDef(string tag, string name)
        {

            StringTag = tag;
            Tag = TagUtils.StringToTag(tag);
            Name = name;
        }
        public string StringTag { get; set; }
#if DEBUG

        public override string ToString() => StringTag;
#endif


    }

    public static partial class ScriptTagDefs
    {
        static Dictionary<string, ScriptTagDef> s_registerScriptTags;

        static ScriptTagDef _(string tag, string name)
        {
            if (s_registerScriptTags == null)
            {
                s_registerScriptTags = new Dictionary<string, ScriptTagDef>();
            }


            var scriptTagDef = new ScriptTagDef(tag, name);

            if (!s_registerScriptTags.ContainsKey(tag))
            {
                //duplicated script tag
                System.Diagnostics.Debug.WriteLine("script_tags: duplicated");
            }
            s_registerScriptTags[tag] = scriptTagDef;

            return scriptTagDef;
        }

    }


    static partial class ScriptTagDefs
    {
        //https://docs.microsoft.com/en-us/typography/opentype/spec/scripttags
        public static readonly ScriptTagDef

        //AUTOGEN
        Adlam = _("adlm", "Adlam"),
Ahom = _("ahom", "Ahom"),
Anatolian_Hieroglyphs = _("hluw", "Anatolian Hieroglyphs"),
Arabic = _("arab", "Arabic"),
Armenian = _("armn", "Armenian"),
Avestan = _("avst", "Avestan"),
Balinese = _("bali", "Balinese"),
Bamum = _("bamu", "Bamum"),
Bassa_Vah = _("bass", "Bassa Vah"),
Batak = _("batk", "Batak"),
Bengali = _("beng", "Bengali"),
Bengali_v_2 = _("bng2", "Bengali v.2"),
Bhaiksuki = _("bhks", "Bhaiksuki"),
Bopomofo = _("bopo", "Bopomofo"),
Brahmi = _("brah", "Brahmi"),
Braille = _("brai", "Braille"),
Buginese = _("bugi", "Buginese"),
Buhid = _("buhd", "Buhid"),
Byzantine_Music = _("byzm", "Byzantine Music"),
Canadian_Syllabics = _("cans", "Canadian Syllabics"),
Carian = _("cari", "Carian"),
Caucasian_Albanian = _("aghb", "Caucasian Albanian"),
Chakma = _("cakm", "Chakma"),
Cham = _("cham", "Cham"),
Cherokee = _("cher", "Cherokee"),
CJK_Ideographic = _("hani", "CJK Ideographic"),
Coptic = _("copt", "Coptic"),
Cypriot_Syllabary = _("cprt", "Cypriot Syllabary"),
Cyrillic = _("cyrl", "Cyrillic"),
Default = _("DFLT", "Default"),
Deseret = _("dsrt", "Deseret"),
Devanagari = _("deva", "Devanagari"),
Devanagari_v_2 = _("dev2", "Devanagari v.2"),
Dogra = _("dogr", "Dogra"),
Duployan = _("dupl", "Duployan"),
Egyptian_Hieroglyphs = _("egyp", "Egyptian Hieroglyphs"),
Elbasan = _("elba", "Elbasan"),
Ethiopic = _("ethi", "Ethiopic"),
Georgian = _("geor", "Georgian"),
Glagolitic = _("glag", "Glagolitic"),
Gothic = _("goth", "Gothic"),
Grantha = _("gran", "Grantha"),
Greek = _("grek", "Greek"),
Gujarati = _("gujr", "Gujarati"),
Gujarati_v_2 = _("gjr2", "Gujarati v.2"),
Gunjala_Gondi = _("gong", "Gunjala Gondi"),
Gurmukhi = _("guru", "Gurmukhi"),
Gurmukhi_v_2 = _("gur2", "Gurmukhi v.2"),
Hangul = _("hang", "Hangul"),
Hangul_Jamo = _("jamo", "Hangul Jamo"),
Hanifi_Rohingya = _("rohg", "Hanifi Rohingya"),
Hanunoo = _("hano", "Hanunoo"),
Hatran = _("hatr", "Hatran"),
Hebrew = _("hebr", "Hebrew"),
Hiragana = _("kana", "Hiragana"),
Imperial_Aramaic = _("armi", "Imperial Aramaic"),
Inscriptional_Pahlavi = _("phli", "Inscriptional Pahlavi"),
Inscriptional_Parthian = _("prti", "Inscriptional Parthian"),
Javanese = _("java", "Javanese"),
Kaithi = _("kthi", "Kaithi"),
Kannada = _("knda", "Kannada"),
Kannada_v_2 = _("knd2", "Kannada v.2"),
Katakana = _("kana", "Katakana"),
Kayah_Li = _("kali", "Kayah Li"),
Kharosthi = _("khar", "Kharosthi"),
Khmer = _("khmr", "Khmer"),
Khojki = _("khoj", "Khojki"),
Khudawadi = _("sind", "Khudawadi"),
Lao = _("lao", "Lao"),
Latin = _("latn", "Latin"),
Lepcha = _("lepc", "Lepcha"),
Limbu = _("limb", "Limbu"),
Linear_A = _("lina", "Linear A"),
Linear_B = _("linb", "Linear B"),
Lisu__Fraser_ = _("lisu", "Lisu (Fraser)"),
Lycian = _("lyci", "Lycian"),
Lydian = _("lydi", "Lydian"),
Mahajani = _("mahj", "Mahajani"),
Makasar = _("maka", "Makasar"),
Malayalam = _("mlym", "Malayalam"),
Malayalam_v_2 = _("mlm2", "Malayalam v.2"),
Mandaic__Mandaean = _("mand", "Mandaic, Mandaean"),
Manichaean = _("mani", "Manichaean"),
Marchen = _("marc", "Marchen"),
Masaram_Gondi = _("gonm", "Masaram Gondi"),
Mathematical_Alphanumeric_Symbols = _("math", "Mathematical Alphanumeric Symbols"),
Medefaidrin__Oberi_Okaime__Oberi_Ɔkaimɛ_ = _("medf", "Medefaidrin (Oberi Okaime, Oberi Ɔkaimɛ)"),
Meitei_Mayek__Meithei__Meetei_ = _("mtei", "Meitei Mayek (Meithei, Meetei)"),
Mende_Kikakui = _("mend", "Mende Kikakui"),
Meroitic_Cursive = _("merc", "Meroitic Cursive"),
Meroitic_Hieroglyphs = _("mero", "Meroitic Hieroglyphs"),
Miao = _("plrd", "Miao"),
Modi = _("modi", "Modi"),
Mongolian = _("mong", "Mongolian"),
Mro = _("mroo", "Mro"),
Multani = _("mult", "Multani"),
Musical_Symbols = _("musc", "Musical Symbols"),
Myanmar = _("mymr", "Myanmar"),
Myanmar_v_2 = _("mym2", "Myanmar v.2"),
Nabataean = _("nbat", "Nabataean"),
Newa = _("newa", "Newa"),
New_Tai_Lue = _("talu", "New Tai Lue"),
N_Ko = _("nko", "N'Ko"),
Nüshu = _("nshu", "Nüshu"),
Odia__formerly_Oriya_ = _("orya", "Odia (formerly Oriya)"),
Odia_v_2__formerly_Oriya_v_2_ = _("ory2", "Odia v.2 (formerly Oriya v.2)"),
Ogham = _("ogam", "Ogham"),
Ol_Chiki = _("olck", "Ol Chiki"),
Old_Italic = _("ital", "Old Italic"),
Old_Hungarian = _("hung", "Old Hungarian"),
Old_North_Arabian = _("narb", "Old North Arabian"),
Old_Permic = _("perm", "Old Permic"),
Old_Persian_Cuneiform = _("xpeo", "Old Persian Cuneiform"),
Old_Sogdian = _("sogo", "Old Sogdian"),
Old_South_Arabian = _("sarb", "Old South Arabian"),
Old_Turkic__Orkhon_Runic = _("orkh", "Old Turkic, Orkhon Runic"),
Osage = _("osge", "Osage"),
Osmanya = _("osma", "Osmanya"),
Pahawh_Hmong = _("hmng", "Pahawh Hmong"),
Palmyrene = _("palm", "Palmyrene"),
Pau_Cin_Hau = _("pauc", "Pau Cin Hau"),
Phags_pa = _("phag", "Phags-pa"),
Phoenician = _("phnx", "Phoenician"),
Psalter_Pahlavi = _("phlp", "Psalter Pahlavi"),
Rejang = _("rjng", "Rejang"),
Runic = _("runr", "Runic"),
Samaritan = _("samr", "Samaritan"),
Saurashtra = _("saur", "Saurashtra"),
Sharada = _("shrd", "Sharada"),
Shavian = _("shaw", "Shavian"),
Siddham = _("sidd", "Siddham"),
Sign_Writing = _("sgnw", "Sign Writing"),
Sinhala = _("sinh", "Sinhala"),
Sogdian = _("sogd", "Sogdian"),
Sora_Sompeng = _("sora", "Sora Sompeng"),
Soyombo = _("soyo", "Soyombo"),
Sumero_Akkadian_Cuneiform = _("xsux", "Sumero-Akkadian Cuneiform"),
Sundanese = _("sund", "Sundanese"),
Syloti_Nagri = _("sylo", "Syloti Nagri"),
Syriac = _("syrc", "Syriac"),
Tagalog = _("tglg", "Tagalog"),
Tagbanwa = _("tagb", "Tagbanwa"),
Tai_Le = _("tale", "Tai Le"),
Tai_Tham__Lanna_ = _("lana", "Tai Tham (Lanna)"),
Tai_Viet = _("tavt", "Tai Viet"),
Takri = _("takr", "Takri"),
Tamil = _("taml", "Tamil"),
Tamil_v_2 = _("tml2", "Tamil v.2"),
Tangut = _("tang", "Tangut"),
Telugu = _("telu", "Telugu"),
Telugu_v_2 = _("tel2", "Telugu v.2"),
Thaana = _("thaa", "Thaana"),
Thai = _("thai", "Thai"),
Tibetan = _("tibt", "Tibetan"),
Tifinagh = _("tfng", "Tifinagh"),
Tirhuta = _("tirh", "Tirhuta"),
Ugaritic_Cuneiform = _("ugar", "Ugaritic Cuneiform"),
Vai = _("vai", "Vai"),
Warang_Citi = _("wara", "Warang Citi"),
Yi = _("yi", "Yi"),
Zanabazar_Square__Zanabazarin_Dörböljin_Useg__Xewtee_Dörböljin_Bicig__Horizontal_Square_Script_ = _("zanb", "Zanabazar Square (Zanabazarin Dörböljin Useg, Xewtee Dörböljin Bicig, Horizontal Square Script)")

            ;

    }




    public partial class ScriptLangs
    {
        //https://docs.microsoft.com/en-us/typography/opentype/spec/scripttags
        //https://docs.microsoft.com/en-us/typography/opentype/spec/languagetags
        //--------------------------------------------------------------------
        readonly static Dictionary<string, int> s_registerNames = new Dictionary<string, int>();
        readonly static Dictionary<string, ScriptLangInfo> s_registeredScriptTags = new Dictionary<string, ScriptLangInfo>();
        readonly static Dictionary<string, ScriptLangInfo> s_registerScriptFromFullNames = new Dictionary<string, ScriptLangInfo>();
        readonly static List<UnicodeRangeMapWithScriptLang> s_unicodeLangToScriptLang = new List<UnicodeRangeMapWithScriptLang>();

        readonly static Dictionary<string, UnicodeRangeInfo[]> s_registeredScriptTagsToUnicodeLangRanges = new Dictionary<string, UnicodeRangeInfo[]>();

        readonly struct UnicodeRangeMapWithScriptLang
        {
            public readonly ScriptLangInfo scLang;
            public readonly UnicodeRangeInfo unicodeLangRange;
            public UnicodeRangeMapWithScriptLang(UnicodeRangeInfo unicodeRangeBits, ScriptLangInfo scLang)
            {
                this.scLang = scLang;
                this.unicodeLangRange = unicodeRangeBits;
            }
            public bool IsInRange(char c) => unicodeLangRange.IsInRange(c);
        }

        static ScriptLangInfo _(string fullname, string shortname, params UnicodeRangeInfo[] langRanges)
        {

            if (s_registeredScriptTags.ContainsKey(shortname))
            {
                if (shortname == "kana")
                {
                    //***
                    //Hiragana and Katakana 
                    //both have same short name "kana"                     
                    return new ScriptLangInfo(fullname, shortname) { unicodeLangs = langRanges };
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
                var scriptLang = new ScriptLangInfo(fullname, shortname) { unicodeLangs = langRanges };
                s_registeredScriptTags.Add(shortname, scriptLang);
                //                 
                s_registerScriptFromFullNames[fullname] = scriptLang;

                //also register unicode langs with the script lang

                for (int i = langRanges.Length - 1; i >= 0; --i)
                {
                    s_unicodeLangToScriptLang.Add(new UnicodeRangeMapWithScriptLang(langRanges[i], scriptLang));
                }


                if (langRanges.Length > 0)
                {
                    s_registeredScriptTagsToUnicodeLangRanges.Add(shortname, langRanges);
                }
                return scriptLang;
            }
        }
        public static bool TryGetUnicodeLangRangesArray(string langShortName, out UnicodeRangeInfo[] unicodeLangRanges)
        {
            return s_registeredScriptTagsToUnicodeLangRanges.TryGetValue(langShortName, out unicodeLangRanges);
        }
        public static bool TryGetScriptLang(int codepoint, out ScriptLangInfo scLang)
        {
            if (UnicodeRangeFinder.GetUniCodeRangeFor(codepoint, out UnicodeRangeInfo rangeInfo, out SpanBreakInfo breakInfo))
            {

            }
            scLang = null;
            return false;
        }
        public static bool TryGetUnicodeRangeInfo(int codepoint, out UnicodeRangeInfo unicodeRangeInfo)
        {
            if (UnicodeRangeFinder.GetUniCodeRangeFor(codepoint, out unicodeRangeInfo, out SpanBreakInfo breakInfo))
            {
                return true;
            }
            return false;
        }
        public static ScriptLangInfo GetRegisteredScriptLang(string shortname)
        {
            s_registeredScriptTags.TryGetValue(shortname, out ScriptLangInfo found);
            return found;
        }
        public static ScriptLangInfo GetRegisteredScriptLangFromLanguageName(string languageName)
        {
            s_registerScriptFromFullNames.TryGetValue(languageName, out ScriptLangInfo found);
            return found;
        }
    }

    partial class ScriptLangs
    {
        //Manual map
        //TODO: find a proper method
        public static readonly ScriptLangInfo
        //
        Adlam = _("Adlam", "adlm"),
        Anatolian_Hieroglyphs = _("Anatolian Hieroglyphs", "hluw"),
        Arabic = _("Arabic", "arab",
            Unicode13RangeInfoList.Arabic,
            Unicode13RangeInfoList.Arabic_Extended_A,
            Unicode13RangeInfoList.Arabic_Mathematical_Alphabetic_Symbols,
            Unicode13RangeInfoList.Arabic_Presentation_Forms_A,
            Unicode13RangeInfoList.Arabic_Presentation_Forms_B,
            Unicode13RangeInfoList.Arabic_Supplement),
        Armenian = _("Armenian", "armn", Unicode13RangeInfoList.Armenian, Unicode13RangeInfoList.Armenian_Ligatures),
        Avestan = _("Avestan", "avst"),
        //
        Balinese = _("Balinese", "bali", Unicode13RangeInfoList.Balinese),
        Bamum = _("Bamum", "bamu"),
        Bassa_Vah = _("Bassa Vah ", "bass"),
        Batak = _("Batak", "batk"),
        Bengali = _("Bengali", "beng", Unicode13RangeInfoList.Bengali),
        Bengali_v_2 = _("Bengali v.2", "bng2", Unicode13RangeInfoList.Bengali),
        Bhaiksuki = _("Bhaiksuki", "bhks"),
        Brahmi = _("Brahmi", "brah"),
        Braille = _("Braille", "brai", Unicode13RangeInfoList.Braille_Patterns),
        Buginese = _("Buginese", "bugi", Unicode13RangeInfoList.Buginese),
        Buhid = _("Buhid", "buhd", Unicode13RangeInfoList.Buhid),
        Byzantine_Music = _("Byzantine Music", "byzm", Unicode13RangeInfoList.Byzantine_Musical_Symbols),
        //
        Canadian_Syllabics = _("Canadian Syllabics", "cans", Unicode13RangeInfoList.Unified_Canadian_Aboriginal_Syllabics),
        Carian = _("Carian", "cari", Unicode13RangeInfoList.Carian),
        Caucasian_Albanian = _("Caucasian Albanian", "aghb"),
        Chakma = _("Chakma", "cakm"),
        Cham = _("Cham", "cham", Unicode13RangeInfoList.Cham),
        Cherokee = _("Cherokee", "cher", Unicode13RangeInfoList.Cherokee),
        CJK_Ideographic = _("CJK Ideographic", "hani",
            Unicode13RangeInfoList.CJK_Compatibility,
            Unicode13RangeInfoList.CJK_Compatibility_Forms,
            Unicode13RangeInfoList.CJK_Compatibility_Ideographs,
            Unicode13RangeInfoList.CJK_Compatibility_Ideographs_Supplement,
            Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_A,
            Unicode13RangeInfoList.CJK_Unified_Ideographs_Extension_B,
            Unicode13RangeInfoList.CJK_Radicals_Supplement,
            Unicode13RangeInfoList.CJK_Strokes
            ),


        Coptic = _("Coptic", "copt", Unicode13RangeInfoList.Coptic),
        Cypriot_Syllabary = _("Cypriot Syllabary", "cprt", Unicode13RangeInfoList.Cypriot_Syllabary),
        Cyrillic = _("Cyrillic", "cyrl",
            Unicode13RangeInfoList.Cyrillic,
            Unicode13RangeInfoList.Cyrillic_Extended_A,
            Unicode13RangeInfoList.Cyrillic_Extended_B),
        ////
        Default = _("Default", "DFLT"),
        Deseret = _("Deseret", "dsrt", Unicode13RangeInfoList.Deseret),
        Devanagari = _("Devanagari", "deva", Unicode13RangeInfoList.Devanagari),
        Devanagari_v_2 = _("Devanagari v.2", "dev2", Unicode13RangeInfoList.Devanagari),
        Duployan = _("Duployan", "dupl"),
        ////            
        Egyptian_Hieroglyphs = _("Egyptian Hieroglyphs", "egyp"),
        Elbasan = _("Elbasan", "elba"),
        Ethiopic = _("Ethiopic", "ethi",
            Unicode13RangeInfoList.Ethiopic,
            Unicode13RangeInfoList.Ethiopic_Extended,
            Unicode13RangeInfoList.Ethiopic_Supplement),
        //// 
        Georgian = _("Georgian", "geor", Unicode13RangeInfoList.Georgian, Unicode13RangeInfoList.Georgian_Supplement),
        Glagolitic = _("Glagolitic", "glag", Unicode13RangeInfoList.Glagolitic),
        Gothic = _("Gothic", "goth", Unicode13RangeInfoList.Gothic),
        Grantha = _("Grantha", "gran"),
        Greek = _("Greek", "grek", Unicode13RangeInfoList.Greek_and_Coptic, Unicode13RangeInfoList.Greek_Extended),
        Gujarati = _("Gujarati", "gujr", Unicode13RangeInfoList.Gujarati),
        Gujarati_v_2 = _("Gujarati v.2", "gjr2", Unicode13RangeInfoList.Gujarati),
        Gurmukhi = _("Gurmukhi", "guru", Unicode13RangeInfoList.Gurmukhi),
        Gurmukhi_v_2 = _("Gurmukhi v.2", "gur2", Unicode13RangeInfoList.Gurmukhi),
        //// 
        Hangul = _("Hangul", "hang", Unicode13RangeInfoList.Hangul_Jamo),
        Hangul_Jamo = _("Hangul Jamo", "jamo", Unicode13RangeInfoList.Hangul_Jamo),
        Hanunoo = _("Hanunoo", "hano", Unicode13RangeInfoList.Hanunoo),
        Hatran = _("Hatran", "hatr"),
        Hebrew = _("Hebrew", "hebr", Unicode13RangeInfoList.Hebrew),
        Hiragana = _("Hiragana", "kana", Unicode13RangeInfoList.Hiragana),
        //// 
        Imperial_Aramaic = _("Imperial Aramaic", "armi"),
        Inscriptional_Pahlavi = _("Inscriptional Pahlavi", "phli"),
        Inscriptional_Parthian = _("Inscriptional Parthian", "prti"),
        ////             	
        Javanese = _("Javanese", "java"),
        //// 
        Kaithi = _("Kaithi", "kthi"),
        Kannada = _("Kannada", "knda", Unicode13RangeInfoList.Kannada),
        Kannada_v_2 = _("Kannada v.2", "knd2", Unicode13RangeInfoList.Kannada),
        Katakana = _("Katakana", "kana", Unicode13RangeInfoList.Katakana, Unicode13RangeInfoList.Katakana_Phonetic_Extensions),
        Kayah_Li = _("Kayah Li", "kali"),
        Kharosthi = _("Kharosthi", "khar", Unicode13RangeInfoList.Kharoshthi),
        Khmer = _("Khmer", "khmr", Unicode13RangeInfoList.Khmer, Unicode13RangeInfoList.Khmer_Symbols),
        Khojki = _("Khojki", "khoj"),
        Khudawadi = _("Khudawadi", "sind"),
        //// 
        Lao = _("Lao", "lao", Unicode13RangeInfoList.Lao),
        Latin = _("Latin", "latn",
            //0-127
            Unicode13RangeInfoList.C0_controls,
            Unicode13RangeInfoList.ASCII_punctuation_and_symbols_1,
            Unicode13RangeInfoList.ASCII_digits,
            Unicode13RangeInfoList.ASCII_punctuation_and_symbols_2,
            Unicode13RangeInfoList.Uppercase_Latin_alphabet,
            Unicode13RangeInfoList.ASCII_punctuation_and_symbols_3,
            Unicode13RangeInfoList.Lowercase_Latin_alphabet,
            Unicode13RangeInfoList.ASCII_punctuation_and_symbols_4,
            Unicode13RangeInfoList.Control_character_2, 
            //128-255
            Unicode13RangeInfoList.C1_controls, 
            Unicode13RangeInfoList.Latin_1_punctuation_and_symbols,
            Unicode13RangeInfoList.Vulgar_fractions,
            Unicode13RangeInfoList.Punctuation_5,
            Unicode13RangeInfoList.Letters_of_the_Latin_1_Supplement_part1,
            Unicode13RangeInfoList.Mathematical_operator_multiplication,
            Unicode13RangeInfoList.Letters_of_the_Latin_1_Supplement_part2,
            Unicode13RangeInfoList.Mathematical_operator_division,
            Unicode13RangeInfoList.Letters_of_Latin_1_Supplement_part3,

            Unicode13RangeInfoList.Latin_Extended_A,
            Unicode13RangeInfoList.Latin_Extended_Additional,
            Unicode13RangeInfoList.Latin_Extended_B,
            Unicode13RangeInfoList.Latin_Extended_C,
            Unicode13RangeInfoList.Latin_Extended_D),

        Lepcha = _("Lepcha", "lepc", Unicode13RangeInfoList.Lepcha),
        Limbu = _("Limbu", "limb", Unicode13RangeInfoList.Limbu),
        Linear_A = _("Linear A", "lina"),
        Linear_B = _("Linear B", "linb", Unicode13RangeInfoList.Linear_B_Ideograms, Unicode13RangeInfoList.Linear_B_Syllabary),
        Lisu = _("Lisu (Fraser)", "lisu"),
        Lycian = _("Lycian", "lyci", Unicode13RangeInfoList.Lycian),
        Lydian = _("Lydian", "lydi", Unicode13RangeInfoList.Lydian),
        //// 
        Mahajani = _("Mahajani", "mahj"),
        Malayalam = _("Malayalam", "mlym", Unicode13RangeInfoList.Malayalam),
        Malayalam_v_2 = _("Malayalam v.2", "mlm2", Unicode13RangeInfoList.Malayalam),
        Mandaic = _("Mandaic, Mandaean", "mand"),
        Manichaean = _("Manichaean", "mani"),
        Marchen = _("Marchen", "marc"),
        Math = _("Mathematical Alphanumeric Symbols", "math", Unicode13RangeInfoList.Mathematical_Alphanumeric_Symbols),
        Meitei_Mayek = _("Meitei Mayek (Meithei, Meetei)", "mtei"),
        Mende_Kikakui = _("Mende Kikakui", "mend"),
        Meroitic_Cursive = _("Meroitic Cursive", "merc"),
        Meroitic_Hieroglyphs = _("Meroitic Hieroglyphs", "mero"),
        Miao = _("Miao", "plrd"),
        Modi = _("Modi", "modi"),
        Mongolian = _("Mongolian", "mong", Unicode13RangeInfoList.Mongolian),
        Mro = _("Mro", "mroo"),
        Multani = _("Multani", "mult"),
        Musical_Symbols = _("Musical Symbols", "musc", Unicode13RangeInfoList.Musical_Symbols),
        Myanmar = _("Myanmar", "mymr", Unicode13RangeInfoList.Myanmar),
        Myanmar_v_2 = _("Myanmar v.2", "mym2", Unicode13RangeInfoList.Myanmar),
        ////      
        Nabataean = _("Nabataean", "nbat"),
        Newa = _("Newa", "newa"),
        New_Tai_Lue = _("New Tai Lue", "talu", Unicode13RangeInfoList.New_Tai_Lue),
        N_Ko = _("N'Ko", "nko", Unicode13RangeInfoList.N_Ko),
        //// 
        Odia = _("Odia (formerly Oriya)", "orya"),
        Odia_V_2 = _("Odia v.2 (formerly Oriya v.2)", "ory2"),
        Ogham = _("Ogham", "ogam", Unicode13RangeInfoList.Ogham),
        Ol_Chiki = _("Ol Chiki", "olck", Unicode13RangeInfoList.Ol_Chiki),
        Old_Italic = _("Old Italic", "ital"),
        Old_Hungarian = _("Old Hungarian", "hung"),
        Old_North_Arabian = _("Old North Arabian", "narb"),
        Old_Permic = _("Old Permic", "perm"),
        Old_Persian_Cuneiform = _("Old Persian Cuneiform ", "xpeo"),
        Old_South_Arabian = _("Old South Arabian", "sarb"),
        Old_Turkic = _("Old Turkic, Orkhon Runic", "orkh"),
        Osage = _("Osage", "osge"),
        Osmanya = _("Osmanya", "osma", Unicode13RangeInfoList.Osmanya),
        //// 
        Pahawh_Hmong = _("Pahawh Hmong", "hmng"),
        Palmyrene = _("Palmyrene", "palm"),
        Pau_Cin_Hau = _("Pau Cin Hau", "pauc"),
        Phags_pa = _("Phags-pa", "phag", Unicode13RangeInfoList.Phags_pa),
        Phoenician = _("Phoenician ", "phnx"),
        Psalter_Pahlavi = _("Psalter Pahlavi", "phlp"),

        //// 
        Rejang = _("Rejang", "rjng", Unicode13RangeInfoList.Rejang),
        Runic = _("Runic", "runr", Unicode13RangeInfoList.Runic),

        //// 
        Samaritan = _("Samaritan", "samr"),
        Saurashtra = _("Saurashtra", "saur", Unicode13RangeInfoList.Saurashtra),
        Sharada = _("Sharada", "shrd"),
        Shavian = _("Shavian", "shaw", Unicode13RangeInfoList.Shavian),
        Siddham = _("Siddham", "sidd"),
        Sign_Writing = _("Sign Writing", "sgnw"),
        Sinhala = _("Sinhala", "sinh", Unicode13RangeInfoList.Sinhala),
        Sora_Sompeng = _("Sora Sompeng", "sora"),
        Sumero_Akkadian_Cuneiform = _("Sumero-Akkadian Cuneiform", "xsux"),
        Sundanese = _("Sundanese", "sund", Unicode13RangeInfoList.Sundanese),
        Syloti_Nagri = _("Syloti Nagri", "sylo", Unicode13RangeInfoList.Syloti_Nagri),
        Syriac = _("Syriac", "syrc", Unicode13RangeInfoList.Syriac),
        ////       
        Tagalog = _("Tagalog", "tglg"),
        Tagbanwa = _("Tagbanwa", "tagb", Unicode13RangeInfoList.Tagbanwa),
        Tai_Le = _("Tai Le", "tale", Unicode13RangeInfoList.Tai_Le),
        Tai_Tham = _("Tai Tham (Lanna)", "lana"),
        Tai_Viet = _("Tai Viet", "tavt"),
        Takri = _("Takri", "takr"),
        Tamil = _("Tamil", "taml", Unicode13RangeInfoList.Tamil),
        Tamil_v_2 = _("Tamil v.2", "tml2", Unicode13RangeInfoList.Tamil),
        Tangut = _("Tangut", "tang"),
        Telugu = _("Telugu", "telu", Unicode13RangeInfoList.Telugu),
        Telugu_v_2 = _("Telugu v.2", "tel2", Unicode13RangeInfoList.Telugu),
        Thaana = _("Thaana", "thaa", Unicode13RangeInfoList.Thaana),
        Thai = _("Thai", "thai", Unicode13RangeInfoList.Thai),
        Tibetan = _("Tibetan", "tibt", Unicode13RangeInfoList.Tibetan),
        Tifinagh = _("Tifinagh", "tfng", Unicode13RangeInfoList.Tifinagh),
        Tirhuta = _("Tirhuta", "tirh"),
        ////
        Ugaritic_Cuneiform = _("Ugaritic Cuneiform", "ugar"),
        ////
        Vai = _("Vai", "vai"),
        ////
        Warang_Citi = _("Warang Citi", "wara"),

        ////
        Yi = _("Yi", "yi", Unicode13RangeInfoList.Yi_Syllables)
        //
        ;


    }


}