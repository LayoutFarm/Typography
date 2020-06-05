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

    }
}
