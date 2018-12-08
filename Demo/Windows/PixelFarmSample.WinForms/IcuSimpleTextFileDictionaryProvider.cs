//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System.IO;
using System.Collections.Generic;

namespace Typography.TextBreak
{
    public class IcuSimpleTextFileDictionaryProvider : DictionaryProvider
    {
        //read from original ICU's dictionary
        //.. 
        public string DataDir
        {
            get;
            set;
        }
        public override IEnumerable<string> GetSortedUniqueWordList(string dicName)
        {
            //user can provide their own data 
            //....

            switch (dicName)
            {
                default:
                    return null;
                case "abbrv-en":
                    return GetAbbrvDicFromTextFile(DataDir + "/en.txt");
                case "thai":
                    return GetTextListIterFromTextFile(DataDir + "/dictionaries/thaidict.txt");
                case "lao":
                    return GetTextListIterFromTextFile(DataDir + "/dictionaries/laodict.txt");
            }
        }
        static IEnumerable<string> GetAbbrvDicFromTextFile(string filename)
        {

            //read from original ICU's rule file
            //..
            //
            List<string> allAbbrvs = new List<string>();
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    //---------------------
                    //CAUTION:!!!, Very file-specific
                    //not a full parse, this is for extract content  only
                    //---------------------
                    if (line.Length > 0 && (line[0] == '"')) //not a comment
                    {
                        int endpos = line.IndexOf('"', 1);
                        string word = line.Substring(1, endpos - 1);
                        allAbbrvs.Add(word);
                    }
                    //
                    line = reader.ReadLine();//next line
                }
            }
            allAbbrvs.Sort();
            return allAbbrvs;
        }
        static IEnumerable<string> GetTextListIterFromTextFile(string filename)
        {
            //read from original ICU's dictionary
            //..

            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    if (line.Length > 0 && (line[0] != '#')) //not a comment
                    {
                        yield return line.Trim();
                    }
                    line = reader.ReadLine();//next line
                }
            }
        }
    }
}
