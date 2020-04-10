//MIT, 2017-present, WinterDev
//and ICU data is modified from ICU project (http://site.icu-project.org/)

//----------------------------
//ICU License - ICU 1.8.1 and later
//COPYRIGHT AND PERMISSION NOTICE

//Copyright(c) 1995-2014 International Business Machines Corporation and others
//All rights reserved.
//Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
//associated documentation files (the "Software"), to deal in the Software without restriction,
//including without limitation the rights to use, copy, modify, merge, publish, distribute, 
//and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//provided that the above copyright notice(s) 
//and this permission notice appear in all copies of the Software and
//that both the above copyright notice(s)
//and this permission notice appear in supporting documentation.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT OF THIRD PARTY RIGHTS.
//IN NO EVENT SHALL THE COPYRIGHT HOLDER OR HOLDERS 
//INCLUDED IN THIS NOTICE BE LIABLE FOR ANY CLAIM, OR ANY SPECIAL INDIRECT OR CONSEQUENTIAL DAMAGES,
//OR ANY DAMAGES WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, 
//WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION,
//ARISING OUT OF OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

//Except as contained in this notice,
//the name of a copyright holder shall not be used in advertising or otherwise to promote the sale, 
//use or other dealings in this Software without prior written authorization of the copyright holder.

//----------------------------

using System;
using System.Collections.Generic;
using System.IO;
namespace Typography.TextBreak
{

    public static class IcuData
    {
        //this data is a modified version from 
        //icu/icu4c-60_1-data/data/lang/en.txt

        const string LangsCode =
        @"aa{Afar}
        ab{Abkhazian}
        ace{Achinese}
        ach{Acoli}
        ada{Adangme}
        ady{Adyghe}
        ae{Avestan}
        aeb{Tunisian Arabic}
        af{Afrikaans}
        afh{Afrihili}
        agq{Aghem}
        ain{Ainu}
        ak{Akan}
        akk{Akkadian}
        akz{Alabama}
        ale{Aleut}
        aln{Gheg Albanian}
        alt{Southern Altai}
        am{Amharic}
        an{Aragonese}
        ang{Old English}
        anp{Angika}
        ar{Arabic}
        ar_001{Modern Standard Arabic}
        arc{Aramaic}
        arn{Mapuche}
        aro{Araona}
        arp{Arapaho}
        arq{Algerian Arabic}
        ars{Najdi Arabic}
        arw{Arawak}
        ary{Moroccan Arabic}
        arz{Egyptian Arabic}
        as{Assamese}
        asa{Asu}
        ase{American Sign Language}
        ast{Asturian}
        av{Avaric}
        avk{Kotava}
        awa{Awadhi}
        ay{Aymara}
        az{Azerbaijani}
        ba{Bashkir}
        bal{Baluchi}
        ban{Balinese}
        bar{Bavarian}
        bas{Basaa}
        bax{Bamun}
        bbc{Batak Toba}
        bbj{Ghomala}
        be{Belarusian}
        bej{Beja}
        bem{Bemba}
        bew{Betawi}
        bez{Bena}
        bfd{Bafut}
        bfq{Badaga}
        bg{Bulgarian}
        bgn{Western Balochi}
        bho{Bhojpuri}
        bi{Bislama}
        bik{Bikol}
        bin{Bini}
        bjn{Banjar}
        bkm{Kom}
        bla{Siksika}
        bm{Bambara}
        bn{Bangla}
        bo{Tibetan}
        bpy{Bishnupriya}
        bqi{Bakhtiari}
        br{Breton}
        bra{Braj}
        brh{Brahui}
        brx{Bodo}
        bs{Bosnian}
        bss{Akoose}
        bua{Buriat}
        bug{Buginese}
        bum{Bulu}
        byn{Blin}
        byv{Medumba}
        ca{Catalan}
        cad{Caddo}
        car{Carib}
        cay{Cayuga}
        cch{Atsam}
        ccp{Chakma}
        ce{Chechen}
        ceb{Cebuano}
        cgg{Chiga}
        ch{Chamorro}
        chb{Chibcha}
        chg{Chagatai}
        chk{Chuukese}
        chm{Mari}
        chn{Chinook Jargon}
        cho{Choctaw}
        chp{Chipewyan}
        chr{Cherokee}
        chy{Cheyenne}
        ckb{Central Kurdish}
        co{Corsican}
        cop{Coptic}
        cps{Capiznon}
        cr{Cree}
        crh{Crimean Turkish}
        crs{Seselwa Creole French}
        cs{Czech}
        csb{Kashubian}
        cu{Church Slavic}
        cv{Chuvash}
        cy{Welsh}
        da{Danish}
        dak{Dakota}
        dar{Dargwa}
        dav{Taita}
        de{German}
        de_AT{Austrian German}
        de_CH{Swiss High German}
        del{Delaware}
        den{Slave}
        dgr{Dogrib}
        din{Dinka}
        dje{Zarma}
        doi{Dogri}
        dsb{Lower Sorbian}
        dtp{Central Dusun}
        dua{Duala}
        dum{Middle Dutch}
        dv{Divehi}
        dyo{Jola-Fonyi}
        dyu{Dyula}
        dz{Dzongkha}
        dzg{Dazaga}
        ebu{Embu}
        ee{Ewe}
        efi{Efik}
        egl{Emilian}
        egy{Ancient Egyptian}
        eka{Ekajuk}
        el{Greek}
        elx{Elamite}
        en{English}
        en_AU{Australian English}
        en_CA{Canadian English}
        en_GB{British English}
        en_US{American English}
        enm{Middle English}
        eo{Esperanto}
        es{Spanish}
        es_419{Latin American Spanish}
        es_ES{European Spanish}
        es_MX{Mexican Spanish}
        esu{Central Yupik}
        et{Estonian}
        eu{Basque}
        ewo{Ewondo}
        ext{Extremaduran}
        fa{Persian}
        fa_AF{Dari}
        fan{Fang}
        fat{Fanti}
        ff{Fulah}
        fi{Finnish}
        fil{Filipino}
        fit{Tornedalen Finnish}
        fj{Fijian}
        fo{Faroese}
        fon{Fon}
        fr{French}
        fr_CA{Canadian French}
        fr_CH{Swiss French}
        frc{Cajun French}
        frm{Middle French}
        fro{Old French}
        frp{Arpitan}
        frr{Northern Frisian}
        frs{Eastern Frisian}
        fur{Friulian}
        fy{Western Frisian}
        ga{Irish}
        gaa{Ga}
        gag{Gagauz}
        gan{Gan Chinese}
        gay{Gayo}
        gba{Gbaya}
        gbz{Zoroastrian Dari}
        gd{Scottish Gaelic}
        gez{Geez}
        gil{Gilbertese}
        gl{Galician}
        glk{Gilaki}
        gmh{Middle High German}
        gn{Guarani}
        goh{Old High German}
        gom{Goan Konkani}
        gon{Gondi}
        gor{Gorontalo}
        got{Gothic}
        grb{Grebo}
        grc{Ancient Greek}
        gsw{Swiss German}
        gu{Gujarati}
        guc{Wayuu}
        gur{Frafra}
        guz{Gusii}
        gv{Manx}
        gwi{Gwichʼin}
        ha{Hausa}
        hai{Haida}
        hak{Hakka Chinese}
        haw{Hawaiian}
        he{Hebrew}
        hi{Hindi}
        hif{Fiji Hindi}
        hil{Hiligaynon}
        hit{Hittite}
        hmn{Hmong}
        ho{Hiri Motu}
        hr{Croatian}
        hsb{Upper Sorbian}
        hsn{Xiang Chinese}
        ht{Haitian Creole}
        hu{Hungarian}
        hup{Hupa}
        hy{Armenian}
        hz{Herero}
        ia{Interlingua}
        iba{Iban}
        ibb{Ibibio}
        id{Indonesian}
        ie{Interlingue}
        ig{Igbo}
        ii{Sichuan Yi}
        ik{Inupiaq}
        ilo{Iloko}
        inh{Ingush}
        io{Ido}
        is{Icelandic}
        it{Italian}
        iu{Inuktitut}
        izh{Ingrian}
        ja{Japanese}
        jam{Jamaican Creole English}
        jbo{Lojban}
        jgo{Ngomba}
        jmc{Machame}
        jpr{Judeo-Persian}
        jrb{Judeo-Arabic}
        jut{Jutish}
        jv{Javanese}
        ka{Georgian}
        kaa{Kara-Kalpak}
        kab{Kabyle}
        kac{Kachin}
        kaj{Jju}
        kam{Kamba}
        kaw{Kawi}
        kbd{Kabardian}
        kbl{Kanembu}
        kcg{Tyap}
        kde{Makonde}
        kea{Kabuverdianu}
        ken{Kenyang}
        kfo{Koro}
        kg{Kongo}
        kgp{Kaingang}
        kha{Khasi}
        kho{Khotanese}
        khq{Koyra Chiini}
        khw{Khowar}
        ki{Kikuyu}
        kiu{Kirmanjki}
        kj{Kuanyama}
        kk{Kazakh}
        kkj{Kako}
        kl{Kalaallisut}
        kln{Kalenjin}
        km{Khmer}
        kmb{Kimbundu}
        kn{Kannada}
        ko{Korean}
        koi{Komi-Permyak}
        kok{Konkani}
        kos{Kosraean}
        kpe{Kpelle}
        kr{Kanuri}
        krc{Karachay-Balkar}
        kri{Krio}
        krj{Kinaray-a}
        krl{Karelian}
        kru{Kurukh}
        ks{Kashmiri}
        ksb{Shambala}
        ksf{Bafia}
        ksh{Colognian}
        ku{Kurdish}
        kum{Kumyk}
        kut{Kutenai}
        kv{Komi}
        kw{Cornish}
        ky{Kyrgyz}
        la{Latin}
        lad{Ladino}
        lag{Langi}
        lah{Lahnda}
        lam{Lamba}
        lb{Luxembourgish}
        lez{Lezghian}
        lfn{Lingua Franca Nova}
        lg{Ganda}
        li{Limburgish}
        lij{Ligurian}
        liv{Livonian}
        lkt{Lakota}
        lmo{Lombard}
        ln{Lingala}
        lo{Lao}
        lol{Mongo}
        lou{Louisiana Creole}
        loz{Lozi}
        lrc{Northern Luri}
        lt{Lithuanian}
        ltg{Latgalian}
        lu{Luba-Katanga}
        lua{Luba-Lulua}
        lui{Luiseno}
        lun{Lunda}
        luo{Luo}
        lus{Mizo}
        luy{Luyia}
        lv{Latvian}
        lzh{Literary Chinese}
        lzz{Laz}
        mad{Madurese}
        maf{Mafa}
        mag{Magahi}
        mai{Maithili}
        mak{Makasar}
        man{Mandingo}
        mas{Masai}
        mde{Maba}
        mdf{Moksha}
        mdr{Mandar}
        men{Mende}
        mer{Meru}
        mfe{Morisyen}
        mg{Malagasy}
        mga{Middle Irish}
        mgh{Makhuwa-Meetto}
        mgo{Metaʼ}
        mh{Marshallese}
        mi{Maori}
        mic{Mikmaq}
        min{Minangkabau}
        mk{Macedonian}
        ml{Malayalam}
        mn{Mongolian}
        mnc{Manchu}
        mni{Manipuri}
        moh{Mohawk}
        mos{Mossi}
        mr{Marathi}
        mrj{Western Mari}
        ms{Malay}
        mt{Maltese}
        mua{Mundang}
        mul{Multiple languages}
        mus{Creek}
        mwl{Mirandese}
        mwr{Marwari}
        mwv{Mentawai}
        my{Burmese}
        mye{Myene}
        myv{Erzya}
        mzn{Mazanderani}
        na{Nauru}
        nan{Min Nan Chinese}
        nap{Neapolitan}
        naq{Nama}
        nb{Norwegian Bokmål}
        nd{North Ndebele}
        nds{Low German}
        nds_NL{Low Saxon}
        ne{Nepali}
        new{Newari}
        ng{Ndonga}
        nia{Nias}
        niu{Niuean}
        njo{Ao Naga}
        nl{Dutch}
        nl_BE{Flemish}
        nmg{Kwasio}
        nn{Norwegian Nynorsk}
        nnh{Ngiemboon}
        no{Norwegian}
        nog{Nogai}
        non{Old Norse}
        nov{Novial}
        nqo{N’Ko}
        nr{South Ndebele}
        nso{Northern Sotho}
        nus{Nuer}
        nv{Navajo}
        nwc{Classical Newari}
        ny{Nyanja}
        nym{Nyamwezi}
        nyn{Nyankole}
        nyo{Nyoro}
        nzi{Nzima}
        oc{Occitan}
        oj{Ojibwa}
        om{Oromo}
        or{Odia}
        os{Ossetic}
        osa{Osage}
        ota{Ottoman Turkish}
        pa{Punjabi}
        pag{Pangasinan}
        pal{Pahlavi}
        pam{Pampanga}
        pap{Papiamento}
        pau{Palauan}
        pcd{Picard}
        pcm{Nigerian Pidgin}
        pdc{Pennsylvania German}
        pdt{Plautdietsch}
        peo{Old Persian}
        pfl{Palatine German}
        phn{Phoenician}
        pi{Pali}
        pl{Polish}
        pms{Piedmontese}
        pnt{Pontic}
        pon{Pohnpeian}
        prg{Prussian}
        pro{Old Provençal}
        ps{Pashto}
        pt{Portuguese}
        pt_BR{Brazilian Portuguese}
        pt_PT{European Portuguese}
        qu{Quechua}
        quc{Kʼicheʼ}
        qug{Chimborazo Highland Quichua}
        raj{Rajasthani}
        rap{Rapanui}
        rar{Rarotongan}
        rgn{Romagnol}
        rif{Riffian}
        rm{Romansh}
        rn{Rundi}
        ro{Romanian}
        ro_MD{Moldavian}
        rof{Rombo}
        rom{Romany}
        root{Root}
        rtm{Rotuman}
        ru{Russian}
        rue{Rusyn}
        rug{Roviana}
        rup{Aromanian}
        rw{Kinyarwanda}
        rwk{Rwa}
        sa{Sanskrit}
        sad{Sandawe}
        sah{Sakha}
        sam{Samaritan Aramaic}
        saq{Samburu}
        sas{Sasak}
        sat{Santali}
        saz{Saurashtra}
        sba{Ngambay}
        sbp{Sangu}
        sc{Sardinian}
        scn{Sicilian}
        sco{Scots}
        sd{Sindhi}
        sdc{Sassarese Sardinian}
        sdh{Southern Kurdish}
        se{Northern Sami}
        see{Seneca}
        seh{Sena}
        sei{Seri}
        sel{Selkup}
        ses{Koyraboro Senni}
        sg{Sango}
        sga{Old Irish}
        sgs{Samogitian}
        sh{Serbo-Croatian}
        shi{Tachelhit}
        shn{Shan}
        shu{Chadian Arabic}
        si{Sinhala}
        sid{Sidamo}
        sk{Slovak}
        sl{Slovenian}
        sli{Lower Silesian}
        sly{Selayar}
        sm{Samoan}
        sma{Southern Sami}
        smj{Lule Sami}
        smn{Inari Sami}
        sms{Skolt Sami}
        sn{Shona}
        snk{Soninke}
        so{Somali}
        sog{Sogdien}
        sq{Albanian}
        sr{Serbian}
        srn{Sranan Tongo}
        srr{Serer}
        ss{Swati}
        ssy{Saho}
        st{Southern Sotho}
        stq{Saterland Frisian}
        su{Sundanese}
        suk{Sukuma}
        sus{Susu}
        sux{Sumerian}
        sv{Swedish}
        sw{Swahili}
        sw_CD{Congo Swahili}
        swb{Comorian}
        syc{Classical Syriac}
        syr{Syriac}
        szl{Silesian}
        ta{Tamil}
        tcy{Tulu}
        te{Telugu}
        tem{Timne}
        teo{Teso}
        ter{Tereno}
        tet{Tetum}
        tg{Tajik}
        th{Thai}
        ti{Tigrinya}
        tig{Tigre}
        tiv{Tiv}
        tk{Turkmen}
        tkl{Tokelau}
        tkr{Tsakhur}
        tl{Tagalog}
        tlh{Klingon}
        tli{Tlingit}
        tly{Talysh}
        tmh{Tamashek}
        tn{Tswana}
        to{Tongan}
        tog{Nyasa Tonga}
        tpi{Tok Pisin}
        tr{Turkish}
        tru{Turoyo}
        trv{Taroko}
        ts{Tsonga}
        tsd{Tsakonian}
        tsi{Tsimshian}
        tt{Tatar}
        ttt{Muslim Tat}
        tum{Tumbuka}
        tvl{Tuvalu}
        tw{Twi}
        twq{Tasawaq}
        ty{Tahitian}
        tyv{Tuvinian}
        tzm{Central Atlas Tamazight}
        udm{Udmurt}
        ug{Uyghur}
        uga{Ugaritic}
        uk{Ukrainian}
        umb{Umbundu}
        und{Unknown language}
        ur{Urdu}
        uz{Uzbek}
        vai{Vai}
        ve{Venda}
        vec{Venetian}
        vep{Veps}
        vi{Vietnamese}
        vls{West Flemish}
        vmf{Main-Franconian}
        vo{Volapük}
        vot{Votic}
        vro{Võro}
        vun{Vunjo}
        wa{Walloon}
        wae{Walser}
        wal{Wolaytta}
        war{Waray}
        was{Washo}
        wbp{Warlpiri}
        wo{Wolof}
        wuu{Wu Chinese}
        xal{Kalmyk}
        xh{Xhosa}
        xmf{Mingrelian}
        xog{Soga}
        yao{Yao}
        yap{Yapese}
        yav{Yangben}
        ybb{Yemba}
        yi{Yiddish}
        yo{Yoruba}
        yrl{Nheengatu}
        yue{Cantonese}
        za{Zhuang}
        zap{Zapotec}
        zbl{Blissymbols}
        zea{Zeelandic}
        zen{Zenaga}
        zgh{Standard Moroccan Tamazight}
        zh{Chinese}
        zh_Hans{Simplified Chinese}
        zh_Hant{Traditional Chinese}
        zu{Zulu}
        zun{Zuni}
        zxx{No linguistic content}
        zza{Zaza}";

        static Dictionary<string, string> s_shortNameToFullNames = new Dictionary<string, string>();

        static bool _init_data;
        static void InitData()
        {
            //simple icu data
            //parse
            using (StringReader reader = new StringReader(LangsCode))
            {
                string? line = reader.ReadLine();
                while (line != null)
                {

                    //parse each line
                    line = line.Trim();
                    int firstBrace = line.IndexOf('{');
                    if (firstBrace > -1)
                    {
                        int lastBrace = line.IndexOf('}', firstBrace);
                        if (lastBrace > -1)
                        {
                            string langCode = line.Substring(0, firstBrace);
                            string fullLangName = line.Substring(firstBrace + 1, lastBrace - firstBrace - 1);
                            if (!s_shortNameToFullNames.ContainsKey(langCode))
                            {
                                s_shortNameToFullNames.Add(langCode, fullLangName);
                            }
                            else
                            {

                                //this should not occur?
                            }
                        }

                    }
                    //read next line
                    line = reader.ReadLine();
                }
            }
        }
        
        public static bool TryGetFullLanguageNameFromLangCode
            (string langCode1, string langCode2, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? fullLangName)
        {
            if (!_init_data) InitData();
            //
            _init_data = true;
            if (langCode1 != null)
            {
                if (s_shortNameToFullNames.TryGetValue(langCode1, out fullLangName))
                {   //found
                    return true;
                }
            }
            //another chance
            if (langCode2 != null)
            {
                if (s_shortNameToFullNames.TryGetValue(langCode2, out fullLangName))
                {   //found
                    return true;
                }
            }
            //not found
            fullLangName = null;
            return false;
        }
    }





    //Locale identifier
    //(LCID)    Default code page Language: sublanguage
    //0x0436 	1252 	Afrikaans: South Africa
    //0x041c 	1250 	Albanian: Albania
    //0x1401 	1256 	Arabic: Algeria
    //0x3c01 	1256 	Arabic: Bahrain
    //0x0c01 	1256 	Arabic: Egypt
    //0x0801 	1256 	Arabic: Iraq
    //0x2c01 	1256 	Arabic: Jordan
    //0x3401 	1256 	Arabic: Kuwait
    //0x3001 	1256 	Arabic: Lebanon
    //0x1001 	1256 	Arabic: Libya
    //0x1801 	1256 	Arabic: Morocco
    //0x2001 	1256 	Arabic: Oman
    //0x4001 	1256 	Arabic: Qatar
    //0x0401 	1256 	Arabic: Saudi Arabia
    //0x2801 	1256 	Arabic: Syria
    //0x1c01 	1256 	Arabic: Tunisia
    //0x3801 	1256 	Arabic: U.A.E.
    //0x2401 	1256 	Arabic: Yemen
    //0x042b 	Unicode only    Armenian: Armenia
    //0x082c 	1251 	Azeri: Azerbaijan(Cyrillic)
    //0x042c 	1250 	Azeri: Azerbaijan(Latin)
    //0x042d 	1252 	Basque: Spain
    //0x0423 	1251 	Belarusian: Belarus
    //0x0402 	1251 	Bulgarian: Bulgaria
    //0x0403 	1252 	Catalan: Spain
    //0x0c04 	950 	Chinese: Hong Kong SAR, PRC(Traditional)
    //0x1404 	950 	Chinese: Macao SAR(Traditional)
    //0x0804 	936 	Chinese: PRC(Simplified)
    //0x1004 	936 	Chinese: Singapore(Simplified)
    //0x0404 	950 	Chinese: Taiwan(Traditional)
    //0x0827 	1257 	Classic Lithuanian: Lithuania
    //0x041a 	1250 	Croatian: Croatia
    //0x0405 	1250 	Czech: Czech Republic
    //0x0406 	1252 	Danish: Denmark
    //0x0813 	1252 	Dutch: Belgium
    //0x0413 	1252 	Dutch: Netherlands
    //0x0c09 	1252 	English: Australia
    //0x2809 	1252 	English: Belize
    //0x1009 	1252 	English: Canada
    //0x2409 	1252 	English: Caribbean
    //0x1809 	1252 	English: Ireland
    //0x2009 	1252 	English: Jamaica
    //0x1409 	1252 	English: New Zealand
    //0x3409 	1252 	English: Philippines
    //0x1c09 	1252 	English: South Africa
    //0x2c09 	1252 	English: Trinidad
    //0x0809 	1252 	English: United Kingdom
    //0x0409 	1252 	English: United States
    //0x3009 	1252 	English: Zimbabwe
    //0x0425 	1257 	Estonian: Estonia
    //0x0438 	1252 	Faeroese: Faeroe Islands
    //0x0429 	1256 	Farsi: Iran
    //0x040b 	1252 	Finnish: Finland
    //0x080c 	1252 	French: Belgium
    //0x0c0c 	1252 	French: Canada
    //0x040c 	1252 	French: France
    //0x140c 	1252 	French: Luxembourg
    //0x180c 	1252 	French: Monaco
    //0x100c 	1252 	French: Switzerland
    //0x042f 	1251 	Macedonian(FYROM)
    //0x0437 	Unicode only    Georgian: Georgia
    //0x0c07 	1252 	German: Austria
    //0x0407 	1252 	German: Germany
    //0x1407 	1252 	German: Liechtenstein
    //0x1007 	1252 	German: Luxembourg
    //0x0807 	1252 	German: Switzerland
    //0x0408 	1253 	Greek: Greece
    //0x0447 	Unicode only    Gujarati: India
    //0x040d 	1255 	Hebrew: Israel
    //0x0439 	Unicode only    Hindi: India
    //0x040e 	1250 	Hungarian: Hungary
    //0x040f 	1252 	Icelandic: Iceland
    //0x0421 	1252 	Indonesian: Indonesia
    //0x0410 	1252 	Italian: Italy
    //0x0810 	1252 	Italian: Switzerland
    //0x0411 	932 	Japanese: Japan
    //0x044b 	Unicode only    Kannada: India
    //0x0457 	Unicode only    Konkani: India
    //0x0412 	949 	Korean(Extended Wansung): Korea
    //0x0440 	1251 	Kyrgyz: Kyrgyzstan
    //0x0426 	1257 	Latvian: Latvia
    //0x0427 	1257 	Lithuanian: Lithuania
    //0x083e 	1252 	Malay: Brunei Darussalam
    //0x043e 	1252 	Malay: Malaysia
    //0x044e 	Unicode only    Marathi: India
    //0x0450 	1251 	Mongolian: Mongolia
    //0x0414 	1252 	Norwegian: Norway(Bokmål)
    //0x0814 	1252 	Norwegian: Norway(Nynorsk)
    //0x0415 	1250 	Polish: Poland
    //0x0416 	1252 	Portuguese: Brazil
    //0x0816 	1252 	Portuguese: Portugal
    //0x0446 	Unicode only    Punjabi: India
    //0x0418 	1250 	Romanian: Romania
    //0x0419 	1251 	Russian: Russia
    //0x044f 	Unicode only    Sanskrit: India
    //0x0c1a 	1251 	Serbian: Serbia(Cyrillic)
    //0x081a 	1250 	Serbian: Serbia(Latin)
    //0x041b 	1250 	Slovak: Slovakia
    //0x0424 	1250 	Slovenian: Slovenia
    //0x2c0a 	1252 	Spanish: Argentina
    //0x400a 	1252 	Spanish: Bolivia
    //0x340a 	1252 	Spanish: Chile
    //0x240a 	1252 	Spanish: Colombia
    //0x140a 	1252 	Spanish: Costa Rica
    //0x1c0a 	1252 	Spanish: Dominican Republic
    //0x300a 	1252 	Spanish: Ecuador
    //0x440a 	1252 	Spanish: El Salvador
    //0x100a 	1252 	Spanish: Guatemala
    //0x480a 	1252 	Spanish: Honduras
    //0x080a 	1252 	Spanish: Mexico
    //0x4c0a 	1252 	Spanish: Nicaragua
    //0x180a 	1252 	Spanish: Panama
    //0x3c0a 	1252 	Spanish: Paraguay
    //0x280a 	1252 	Spanish: Peru
    //0x500a 	1252 	Spanish: Puerto Rico
    //0x0c0a 	1252 	Spanish: Spain(Modern Sort)
    //0x040a 	1252 	Spanish: Spain(International Sort)
    //0x380a 	1252 	Spanish: Uruguay
    //0x200a 	1252 	Spanish: Venezuela
    //0x0441 	1252 	Swahili: Kenya
    //0x081d 	1252 	Swedish: Finland
    //0x041d 	1252 	Swedish: Sweden
    //0x0444 	1251 	Tatar: Tatarstan
    //0x044a 	Unicode only    Telgu: India
    //0x041e 	874 	Thai: Thailand
    //0x041f 	1254 	Turkish: Turkey
    //0x0422 	1251 	Ukrainian: Ukraine
    //0x0820 	1256 	Urdu: India
    //0x0420 	1256 	Urdu: Pakistan
    //0x0843 	1251 	Uzbek: Uzbekistan(Cyrillic)
    //0x0443 	1250 	Uzbek: Uzbekistan(Latin)
    //0x042a 	1258 	Vietnamese: Vietnam


}
