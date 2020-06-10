//MIT, 2020, WinterDev
using System;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tools
{
    public partial class Form1 : Form
    {
        //do some codegen

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //https://docs.microsoft.com/en-us/typography/opentype/spec/languagetags
            string[] allLines = File.ReadAllLines("languagetags.txt");
            //
            //parse each line          
            int line_no = 0;
            List<LangSystemInfo> langInfoList = new List<LangSystemInfo>();
            for (int i = 1; i < allLines.Length; ++i) //first line is a note, so start at 1
            {
                string line = allLines[i];
                int p0 = line.IndexOf('\'');
                if (p0 < 0) { throw new NotSupportedException(); }
                int p1 = line.IndexOf('\'', p0 + 1);
                if (p1 < 0) { throw new NotSupportedException(); }

                string langName = line.Substring(0, p0).Trim();
                string tag = line.Substring(p0 + 1, p1 - p0 - 1).Trim();
                string iso = line.Substring(p1 + 1).Trim();

                langInfoList.Add(new LangSystemInfo() { LangName = langName, SystemTag = tag, ISO639 = iso });
                line_no++;
            }

            //--------
            //generate cs code for this
            StringBuilder sb = new StringBuilder();
            foreach (LangSystemInfo lanSysInfo in langInfoList)
            {
                //sb.AppendLine("///<summary>");
                //sb.AppendLine("///" + lanSysInfo.LangName);
                //sb.AppendLine("///</summary>");
                //sb.Append("public static readonly LangSys ");

                string field_name = GetProperFieldName(lanSysInfo.LangName) + "_" + lanSysInfo.SystemTag;

                sb.AppendLine(field_name + "=_(\"" + lanSysInfo.LangName + "\",\"" + lanSysInfo.SystemTag + "\",\"" +
                    lanSysInfo.ISO639 + "\"),");
            }
        }


        static string GetProperFieldName(string fieldname)
        {

            char[] fieldNameBuffers = fieldname.ToCharArray();
            for (int i = 0; i < fieldNameBuffers.Length; ++i)
            {
                char c = fieldNameBuffers[i];
                if (char.IsLetter(c) || char.IsNumber(c))
                {
                    continue;
                }
                else
                {
                    fieldNameBuffers[i] = '_';
                }
            }
            return new string(fieldNameBuffers);
        }

        class LangSystemInfo
        {

            public string LangName { get; set; }
            public string SystemTag { get; set; }
            public string ISO639 { get; set; }

            public override string ToString()
            {
                return SystemTag + ":" + LangName;
            }
        }


        List<UnicodeRangeInfo> _unicode13Ranges;
        Dictionary<string, UnicodeRangeInfo> _unicode13Dic;
        private void button2_Click(object sender, EventArgs e)
        {
            LoadUnicode13Ranges();
        }
        void LoadUnicode13Ranges()
        {

            //https://www.unicode.org/versions/Unicode13.0.0/UnicodeStandard-13.0.pdf
            //generate unicode ranges
            string[] allLines = File.ReadAllLines("unicode13_ranges.txt");
            //skip 1st line
            _unicode13Ranges = new List<UnicodeRangeInfo>();
            _unicode13Dic = new Dictionary<string, UnicodeRangeInfo>();
            for (int i = 1; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split(',');
                if (fields.Length != 3)
                {
                    throw new NotSupportedException();
                }


                var rangeInfo = new UnicodeRangeInfo { LangName = fields[0].Trim(), StartCodePoint = fields[1].Trim(), EndCodePoint = fields[2].Trim() };

                _unicode13Ranges.Add(rangeInfo);

                _unicode13Dic.Add(rangeInfo.LangName, rangeInfo);
            }


        }
        class UnicodeRangeInfo
        {
            public string BitPlane { get; set; }
            public string LangName { get; set; }
            public string StartCodePoint { get; set; }
            public string EndCodePoint { get; set; }
            public override string ToString()
            {
                if (BitPlane == null)
                {
                    return LangName + "," + StartCodePoint + "," + EndCodePoint;
                }
                else
                {
                    return BitPlane + "," + LangName + "," + StartCodePoint + "," + EndCodePoint;
                }

            }
        }

        List<UnicodeRangeInfo> _unicode5_1Ranges;
        private void button3_Click(object sender, EventArgs e)
        {

            //https://docs.microsoft.com/en-us/typography/opentype/spec/os2#ulunicoderange1-bits-031ulunicoderange2-bits-3263ulunicoderange3-bits-6495ulunicoderange4-bits-96127

            string[] allLines = File.ReadAllLines("opentype_unicode5.1_ranges.txt");
            //skip 1st line
            _unicode5_1Ranges = new List<UnicodeRangeInfo>();

            for (int i = 1; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split('\t');
                if (fields.Length != 3)
                {
                    throw new NotSupportedException();
                }

                string[] codePointRanges = ParseCodePointRanges(fields[2].Trim());
                _unicode5_1Ranges.Add(new UnicodeRangeInfo
                {
                    BitPlane = fields[0].Trim(),
                    LangName = fields[1].Trim(),
                    StartCodePoint = codePointRanges[0],
                    EndCodePoint = codePointRanges[1]
                });
            }

            //generate unicode langs bits
            //generate cs code for this
            {
                StringBuilder sb = new StringBuilder();
                foreach (UnicodeRangeInfo unicodeRangeInfo in _unicode5_1Ranges)
                {
                    string field_name = GetProperFieldName(unicodeRangeInfo.LangName);

                    sb.AppendLine(field_name + "= (" + unicodeRangeInfo.BitPlane + "L<<32) | (0x" + unicodeRangeInfo.StartCodePoint + " << 16)|0x" + unicodeRangeInfo.EndCodePoint + ",");
                }
                File.WriteAllText("unicode5.1.gen.txt", sb.ToString());
            }
            {
                //enum iter
                StringBuilder sb = new StringBuilder();
                foreach (UnicodeRangeInfo unicodeRangeInfo in _unicode5_1Ranges)
                {
                    string field_name = GetProperFieldName(unicodeRangeInfo.LangName);
                    sb.AppendLine("UnicodeLangBits." + field_name + ", ");
                }
                File.WriteAllText("unicode5.1.enum.gen.txt", sb.ToString());
            }
        }

        string[] ParseCodePointRanges(string codepoint_range) => codepoint_range.Split('-');
        class ScriptNameAndTag
        {
            public string ScriptName { get; set; }
            public string ScriptTag { get; set; }

            public override string ToString()
            {
                return ScriptTag + ":" + ScriptName;
            }
        }

        Dictionary<string, ScriptNameAndTag> _scNameAndTags;
        void LoadNameAndTags()
        {
            //https://docs.microsoft.com/en-us/typography/opentype/spec/scripttags

            //Script tags generally correspond to a Unicode script. 
            //However, the associations between them may not always be one - to - one,
            //and the OpenType script tags are not guaranteed to be the same as Unicode Script property-value aliases or 
            //ISO 15924 script IDs. 
            //Since the development of OpenType script tags predates the ISO 15924 or Unicode Script property, 
            //the rules for script tags defined in this document may not always be the same as rules for ISO 15924 script IDs.
            //The OpenType script tags can also correlate with a particular OpenType Layout implementation,
            //with the result that more than one script tag may be registered for a given Unicode script(e.g. 'deva' and 'dev2').

            _scNameAndTags = new Dictionary<string, ScriptNameAndTag>();

            string[] allLines = File.ReadAllLines("script_tags.txt");
            //skip 1st line            
            for (int i = 2; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split('\t');

                if (fields.Length != 2)
                {
                    throw new NotSupportedException();
                }

                string scName = fields[0].Trim();
                string scTag = fields[1].Replace("'", "").Trim();
                if (!_scNameAndTags.ContainsKey(scTag))
                {
                    _scNameAndTags.Add(scTag, new ScriptNameAndTag { ScriptName = scName, ScriptTag = scTag });
                }
                else
                {
                    Console.WriteLine("duplicated:" + scName);
                }

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {

            LoadNameAndTags();
            //enum iter
            StringBuilder sb = new StringBuilder();
            foreach (ScriptNameAndTag scNameAndTag in _scNameAndTags.Values)
            {
                string field_name = GetProperFieldName(scNameAndTag.ScriptName);
                sb.AppendLine(field_name + "=_(\"" + scNameAndTag.ScriptTag + "\",\"" + scNameAndTag.ScriptName + "\"),");
            }

            File.WriteAllText("script_tags.gen.txt", sb.ToString());

        }

        class MappingLangAndScript
        {
            public string FullLangName;
            public string ShortLang;
            public string ShortScript;

            public override string ToString()
            {
                return FullLangName + "(" + ShortLang + ", script=>" + ShortScript + ")";
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            //read some part of icu data 

            IcuDataParser icuDataParser = new IcuDataParser();

            Dictionary<string, string> short_to_lang = new Dictionary<string, string>();
            Dictionary<string, string> lang_to_short = new Dictionary<string, string>();
            //-------
            Dictionary<string, string> short_to_script = new Dictionary<string, string>();
            Dictionary<string, string> script_to_short = new Dictionary<string, string>();

            using (FileStream fs = new FileStream("icu_data/data/lang/en.txt", FileMode.Open))
            {
                icuDataParser.ParseData(fs);
                IcuDataTree tree = icuDataParser.ResultTree;
                IcuDataNode node = tree.RootNode;
                {
                    //lang node
                    IcuDataNode langNode = node?.GetFirstChildNode("Languages");
                    if (langNode != null)
                    {
                        //now we can get data from the node
                        //get all lang
                        //collect all data to 

                        int count = langNode.ChildCount;
                        for (int i = 0; i < count; ++i)
                        {
                            IcuDataNode childNode = langNode.GetNode(i);
                            short_to_lang.Add(childNode.Name, childNode.TextValue);
                            lang_to_short.Add(childNode.TextValue, childNode.Name);
                        }

                    }
                }
                {
                    //script node
                    IcuDataNode scriptNode = node?.GetFirstChildNode("Scripts");
                    if (scriptNode != null)
                    {
                        int count = scriptNode.ChildCount;
                        for (int i = 0; i < count; ++i)
                        {
                            IcuDataNode childNode = scriptNode.GetNode(i);
                            short_to_script.Add(childNode.Name, childNode.TextValue);
                            script_to_short.Add(childNode.TextValue, childNode.Name);
                        }
                    }
                }
            }



            //---------------------------------
            LoadUnicode13Ranges();
            //try mapping exact lang and script
            Dictionary<string, MappingLangAndScript> mappingLangAndScripts = new Dictionary<string, MappingLangAndScript>();
            //since we have script < lang, so 
            //match name
            List<string> notFoundList = new List<string>();
            foreach (var kv in script_to_short)
            {
                string script_name = kv.Key;
                string script_tag = kv.Value;

                if (lang_to_short.TryGetValue(script_name, out string lang_short))
                {
                    //try map script name with lang
                    //found
                    mappingLangAndScripts.Add(script_name, new MappingLangAndScript()
                    {
                        FullLangName = script_name,
                        ShortScript = script_tag,
                        ShortLang = lang_short
                    });
                }
                else
                {

                    if (_unicode13Dic.TryGetValue(script_name, out UnicodeRangeInfo rangeInfo))
                    {
                        mappingLangAndScripts.Add(script_name, new MappingLangAndScript()
                        {
                            FullLangName = rangeInfo.LangName,
                            ShortScript = script_tag,
                            ShortLang = "?"
                        });
                    }
                    else
                    {
                        //not found
                        notFoundList.Add(script_name);
                    }
                }
            }
            //---------------------------------



            //---------------------------------
            //write notFoundList
            {

                using (FileStream fs = new FileStream("not_found_langs.txt", FileMode.Create))
                using (StreamWriter w = new StreamWriter(fs))
                {
                    foreach (string notFoundLang in notFoundList)
                    {
                        w.WriteLine(notFoundLang);
                    }
                    w.Close();
                }

            }
        }

        List<ScriptLangGroup> _scriptLangGroup1;
        List<ScriptLangGroup> _scriptLangGroup2;
        string GetFormatedTagName(string input)
        {
            input = input.Trim();
            if (input.StartsWith("\'"))
            {
                if (!input.EndsWith("\'"))
                {
                    throw new NotSupportedException();
                }
                return input.Substring(1, input.Length - 2).Trim();
            }
            else if (input.StartsWith("\""))
            {
                if (!input.EndsWith("\""))
                {
                    throw new NotSupportedException();
                }
                return input.Substring(1, input.Length - 2).Trim();
            }
            else
            {
                return input;
            }

        }
        void LoadScriptLang01()
        {
            //parse script_lang01.txt
            string[] allLines = File.ReadAllLines("script_lang01.txt");
            _scriptLangGroup1 = new List<ScriptLangGroup>();
            ScriptLangGroup currentGroup = null;
            for (int i = 0; i < allLines.Length; ++i)
            {
                string line = allLines[i];
                if (line.StartsWith("#"))
                {
                    //new section with url link
                    currentGroup = new ScriptLangGroup();
                    currentGroup.Url = line.Substring(1).Trim();
                    _scriptLangGroup1.Add(currentGroup);

                    //read next 2 line
                    string[] cells_next1 = allLines[i + 1].Split('\t');
                    string[] cells_next2 = allLines[i + 2].Split('\t');
                    if (cells_next1.Length == 4 && cells_next2.Length == 4)
                    {
                        if (cells_next2[0].ToLower().Trim() == "script tag")
                        {
                            //ensure
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    i += 3;
                }
                else if (line.StartsWith("\""))
                {
                    string[] cells = line.Split('\t');

                    var pair = new ScriptAndLangPair()
                    {
                        ScriptTag = GetFormatedTagName(cells[0]),
                        ScriptName = cells[1].Trim(),
                        LanguageSystemTag = GetFormatedTagName(cells[2]),
                        LanguageSystemName = cells[3].Trim()
                    };

                    if (!pair.LanguageSystemTag.StartsWith("("))
                    {
                        currentGroup.Pairs.Add(pair);
                    }
                }
                else
                {
                    //
                    //blank line or title
                    string line2 = line.ToLower().Trim();
                    if (line2.Length != 0)
                    {
                        throw new NotSupportedException();
                    }
                    //string[] cells = line2.Split('\t');
                    //ensure all blank line

                }
            }

            //-------
        }

        void LoadScriptLang02()
        {
            string[] allLines = File.ReadAllLines("script_lang02.txt");
            _scriptLangGroup2 = new List<ScriptLangGroup>();
            ScriptLangGroup currentGroup = new ScriptLangGroup();
            _scriptLangGroup2.Add(currentGroup);
            currentGroup.GroupName = allLines[0].Substring(1);
            for (int i = 2; i < allLines.Length; ++i) //start at2
            {
                string[] cells = allLines[i].Split('\t');
                //USE= universal shaping engine
                currentGroup.Pairs.Add(new ScriptAndLangPair()
                {
                    ScriptTag = GetFormatedTagName(cells[1]),//1 
                    ScriptName = cells[0].Trim(),
                    LanguageSystemTag = "USE",
                    LanguageSystemName = "USE",
                });
            }
        }
        private void button6_Click(object sender, EventArgs e)
        {
            LoadScriptLang01();
            LoadScriptLang02();


            LoadNameAndTags();//another set data from https://docs.microsoft.com/en-us/typography/opentype/spec/scripttags

            //generate code for script_lang data
            //collect script tag

            Dictionary<string, string> script_tags = new Dictionary<string, string>();
            Dictionary<string, string> langs = new Dictionary<string, string>();

            void CollectData(List<ScriptLangGroup> groups)
            {
                //NESTED
                foreach (ScriptLangGroup scriptLangGroup in groups)
                {
                    foreach (ScriptAndLangPair pair in scriptLangGroup.Pairs)
                    {
                        if (!script_tags.TryGetValue(pair.ScriptTag, out string scriptName))
                        {
                            script_tags.Add(pair.ScriptTag, pair.ScriptName);
                        }
                        else
                        {
                            if (scriptName != pair.ScriptName)
                            {
                                throw new NotSupportedException();
                            }
                            //ensure
                        }
                        //---------------------------
                        if (!langs.TryGetValue(pair.LanguageSystemTag, out string langName))
                        {
                            langs.Add(pair.LanguageSystemTag, pair.LanguageSystemName);
                        }
                        else
                        {
                            if (langName != pair.LanguageSystemName)
                            {
                                throw new NotSupportedException();
                            }
                        }
                    }
                }

            }

            CollectData(_scriptLangGroup1);
            CollectData(_scriptLangGroup2);

            //---------------------------
            //compare data from 2 sources
            List<ScriptNameAndTag> notFounds1 = new List<ScriptNameAndTag>();
            List<ScriptNameAndTag> founds1 = new List<ScriptNameAndTag>();
            foreach (ScriptNameAndTag scName in _scNameAndTags.Values)
            {
                if (!script_tags.ContainsKey(scName.ScriptTag))
                {
                    notFounds1.Add(scName);
                }
                else
                {
                    founds1.Add(scName);
                }
            }
            //---------------------------
            StringBuilder sb = new StringBuilder();


        }
        class ScriptLangGroup
        {
            public string Url;
            public string GroupName;
            public List<ScriptAndLangPair> Pairs = new List<ScriptAndLangPair>();
            public override string ToString()
            {
                return Url;
            }
        }

        class ScriptAndLangPair
        {
            public string ScriptTag;
            public string ScriptName;
            public string LanguageSystemTag;
            public string LanguageSystemName;

            public override string ToString()
            {
                return ScriptTag + "(" + ScriptName + "):" + LanguageSystemName;
            }
        }


        List<Iso15924Record> _iso15924Records = new List<Iso15924Record>();
        void LoadAndParseIso15924()
        {
            string[] lines = File.ReadAllLines("iso15924-codes.txt");
            _iso15924Records.Clear();
            for (int i = 2; i < lines.Length; ++i) //start at i=2
            {
                //
                string[] cells = lines[i].Split('\t');
                if (cells.Length != 7)
                {
                    //ensure
                    throw new NotSupportedException();
                }
                _iso15924Records.Add(new Iso15924Record()
                {
                    Code = cells[0],
                    N = cells[1],
                    EngName = cells[2],
                    Alias = cells[4],
                });

            }
        }

        class Iso15924Record
        {
            public string Code;
            public string N; //N°
            public string EngName;
            public string Alias;

            public override string ToString()
            {
                return Code + ":" + EngName;
            }
        }


        private void button7_Click(object sender, EventArgs e)
        {
            LoadAndParseIso15924();
        }


        List<LangSubTagReg> _langSubTagRegs = new List<LangSubTagReg>();
        void LoadAndParseLangSubTagsReg()
        {
            _langSubTagRegs.Clear();
            string[] lines = File.ReadAllLines("lang_subtag_registry.txt");
            LangSubTagReg lang_subTag = null;
            bool comment_mode = false;

            for (int i = 2; i < lines.Length; ++i) //start at i=2
            {

                string line = lines[i].Trim();

                if (line == "%%")
                {
                    //begin new record
                    lang_subTag = new LangSubTagReg();
                    _langSubTagRegs.Add(lang_subTag);
                    comment_mode = false;
                }
                else
                {
                    string[] kv = line.Split(':');
                    if (kv.Length == 1)
                    {
                        //append data
                        continue;
                    }
                    //
                    if (kv.Length != 2)
                    {
                        if (kv[0].Trim() != "Comments" && !comment_mode)
                        {
                            throw new NotSupportedException();
                        }
                        if (comment_mode)
                        {
                            continue;
                        }
                    }

                    comment_mode = false;
                    switch (kv[0].Trim())
                    {
                        default: throw new NotSupportedException();
                        case "Type":
                            lang_subTag.Type = kv[1].Trim();
                            break;
                        case "Subtag":
                            lang_subTag.SubLang = kv[1].Trim();
                            break;
                        case "Description":
                            lang_subTag.Description = kv[1].Trim();
                            break;
                        case "Added":
                            break;
                        case "Suppress-Script":
                            break;
                        case "Scope":
                            break;
                        case "Macrolanguage":
                            break;
                        case "Comments":
                            {
                                comment_mode = true;
                                //start multi-line mode
                            }
                            break;
                        case "Deprecated":
                            break;
                        case "Preferred-Value":
                            break;
                        case "Prefix":
                            break;
                        case "Tag":
                            break;
                    }
                }
            }




        }

        class LangSubTagReg
        {
            public string Type;
            public string SubLang;
            public string Description;

            public override string ToString()
            {
                return SubLang ?? "??";
            }
        }
        private void button8_Click(object sender, EventArgs e)
        {
            LoadAndParseLangSubTagsReg();

            //test
            var dic1 = new Dictionary<string, LangSubTagReg>();
            List<LangSubTagReg> excludeList = new List<LangSubTagReg>();

            int j = _langSubTagRegs.Count;
            for (int i = 0; i < j; ++i)
            {
                LangSubTagReg langSub = _langSubTagRegs[i];

                if (langSub.SubLang == null)
                {
                    //possible
                    excludeList.Add(langSub);
                    System.Diagnostics.Debug.WriteLine("no sub_lang:" + langSub.Description);
                    continue;//skip this

                    //throw new NotSupportedException();
                }
                if (!dic1.TryGetValue(langSub.SubLang, out LangSubTagReg found))
                {
                    dic1.Add(langSub.SubLang, langSub);
                }
                else
                {
                    //! duplicated?
                    //replace it?
                    excludeList.Add(found);
                    System.Diagnostics.Debug.WriteLine("duplicated key, replace:" + found.ToString());
                    dic1[langSub.SubLang] = langSub;
                }
            }

        }
    }
}
