//MIT, 2020, WinterDev
using System;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Collections.Generic;

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
            string[] allLines = File.ReadAllLines("lang_system_1.txt");
            //
            //parse each line          
            int line_no = 0;
            List<LangSystemInfo> langInfoList = new List<LangSystemInfo>();
            foreach (string line in allLines)
            {
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

        private void button2_Click(object sender, EventArgs e)
        {
            //https://www.unicode.org/versions/Unicode13.0.0/UnicodeStandard-13.0.pdf
            //generate unicode ranges
            string[] allLines = File.ReadAllLines("unicode13_ranges.txt");
            //skip 1st line
            List<UnicodeRangeInfo> unicodeRanges = new List<UnicodeRangeInfo>();

            for (int i = 1; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split(',');
                if (fields.Length != 3)
                {
                    throw new NotSupportedException();
                }
                unicodeRanges.Add(new UnicodeRangeInfo { LangName = fields[0].Trim(), StartCodePoint = fields[1].Trim(), EndCodePoint = fields[2].Trim() });
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

        private void button3_Click(object sender, EventArgs e)
        {

            //https://docs.microsoft.com/en-us/typography/opentype/spec/os2#ulunicoderange1-bits-031ulunicoderange2-bits-3263ulunicoderange3-bits-6495ulunicoderange4-bits-96127

            string[] allLines = File.ReadAllLines("opentype_unicode5.1_ranges.txt");
            //skip 1st line
            List<UnicodeRangeInfo> unicodeRanges = new List<UnicodeRangeInfo>();

            for (int i = 1; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split('\t');
                if (fields.Length != 3)
                {
                    throw new NotSupportedException();
                }

                string[] codePointRanges = ParseCodePointRanges(fields[2].Trim());
                unicodeRanges.Add(new UnicodeRangeInfo
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
                foreach (UnicodeRangeInfo unicodeRangeInfo in unicodeRanges)
                {
                    string field_name = GetProperFieldName(unicodeRangeInfo.LangName);

                    sb.AppendLine(field_name + "= (" + unicodeRangeInfo.BitPlane + "L<<32) | (0x" + unicodeRangeInfo.StartCodePoint + " << 16)|0x" + unicodeRangeInfo.EndCodePoint + ",");
                }
                File.WriteAllText("unicode5.1.gen.txt", sb.ToString());
            }
            {
                //enum iter
                StringBuilder sb = new StringBuilder();
                foreach (UnicodeRangeInfo unicodeRangeInfo in unicodeRanges)
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
        private void button4_Click(object sender, EventArgs e)
        {
            //https://docs.microsoft.com/en-us/typography/opentype/spec/scripttags
            //script_tags.txt

            string[] allLines = File.ReadAllLines("script_tags.txt");
            //skip 1st line           

            List<ScriptNameAndTag> scNameAndTags = new List<ScriptNameAndTag>();
            for (int i = 1; i < allLines.Length; ++i)
            {
                string[] fields = allLines[i].Split('\t');

                if (fields.Length != 2)
                {
                    throw new NotSupportedException();
                }

                string scName = fields[0].Trim();
                string scTag = fields[1].Replace("'", "").Trim();

                scNameAndTags.Add(new ScriptNameAndTag { ScriptName = scName, ScriptTag = scTag });
                 
            }

            {
                //enum iter
                StringBuilder sb = new StringBuilder();
                foreach (ScriptNameAndTag scNameAndTag in scNameAndTags)
                {
                    string field_name = GetProperFieldName(scNameAndTag.ScriptName);
                    sb.AppendLine(field_name + "=_(\"" + scNameAndTag.ScriptTag + "\",\"" + scNameAndTag.ScriptName + "\"),");
                }

                File.WriteAllText("script_tags.gen.txt", sb.ToString());
            }


        }
    }
}
