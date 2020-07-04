//MIT, 2020, WinterDev
//simple ICU data parser
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tools
{

    //simple parser for UnicodeData.txt

    class CharData
    {
        public int codepoint { get; set; }
        public string name { get; set; }
        public CharKind kind { get; set; }
#if DEBUG
        public override string ToString()
        {
            return codepoint + ";" + name + ";" + kind;
        }
#endif
    }

    //from http://www.unicode.org/reports/tr44/#UCD_in_XML
    //5.7.1 General Category Values...
    //....
    //Table 12. General_Category Values
    //Abbr 	Long 	            Description
    //Lu 	Uppercase_Letter 	an uppercase letter
    //Ll 	Lowercase_Letter 	a lowercase letter
    //Lt 	Titlecase_Letter 	a digraphic character, with first part uppercase
    //LC 	Cased_Letter 	    Lu | Ll | Lt
    //Lm 	Modifier_Letter 	a modifier letter
    //Lo 	Other_Letter 	    other letters, including syllables and ideographs    
    //L 	Letter 	            Lu | Ll | Lt | Lm | Lo

    //Mn 	Nonspacing_Mark 	a nonspacing combining mark (zero advance width)
    //Mc 	Spacing_Mark 	    a spacing combining mark (positive advance width)
    //Me 	Enclosing_Mark 	    an enclosing combining mark
    //M 	Mark 	            Mn | Mc | Me

    //Nd 	Decimal_Number 	    a decimal digit
    //Nl 	Letter_Number 	    a letterlike numeric character
    //No 	Other_Number 	    a numeric character of other type
    //N 	Number 	            Nd | Nl | No

    //Pc 	Connector_Punctuation 	a connecting punctuation mark, like a tie
    //Pd 	Dash_Punctuation 	a dash or hyphen punctuation mark
    //Ps 	Open_Punctuation 	an opening punctuation mark (of a pair)
    //Pe 	Close_Punctuation 	a closing punctuation mark (of a pair)
    //Pi 	Initial_Punctuation an initial quotation mark
    //Pf 	Final_Punctuation 	a final quotation mark
    //Po 	Other_Punctuation 	a punctuation mark of other type
    //P 	Punctuation 	    Pc | Pd | Ps | Pe | Pi | Pf | Po

    //Sm 	Math_Symbol     	a symbol of mathematical use
    //Sc 	Currency_Symbol 	a currency sign
    //Sk 	Modifier_Symbol 	a non-letterlike modifier symbol
    //So 	Other_Symbol 	    a symbol of other type
    //S 	Symbol 	            Sm | Sc | Sk | So

    //Zs 	Space_Separator 	a space character (of various non-zero widths)
    //Zl 	Line_Separator 	    U+2028 LINE SEPARATOR only
    //Zp 	Paragraph_Separator U+2029 PARAGRAPH SEPARATOR only
    //Z 	Separator 	        Zs | Zl | Zp

    //Cc 	Control 	        a C0 or C1 control code
    //Cf 	Format          	a format control character
    //Cs 	Surrogate 	        a surrogate code point
    //Co 	Private_Use     	a private-use character
    //Cn 	Unassigned 	        a reserved unassigned code point or a noncharacter
    //C 	Other 	            Cc | Cf | Cs | Co | Cn

    enum CharKind
    {
        Lu, //Lu  Uppercase_Letter an uppercase letter
        Ll, //Ll Lowercase_Letter    a lowercase letter
        Lt,//Lt  Titlecase_Letter a digraphic character, with first part uppercase

        Lm, //Lm  Modifier_Letter a modifier letter
        Lo, //Lo Other_Letter    other letters, including syllables and ideographs

        Mn,  //Mn  Nonspacing_Mark a nonspacing combining mark(zero advance width)
        Mc, //Mc Spacing_Mark    a spacing combining mark(positive advance width)
        Me,  //Me Enclosing_Mark  an enclosing combining mark

        Nd, //Nd  Decimal_Number a decimal digit
        Nl, //Nl Letter_Number   a letterlike numeric character
        No, //No Other_Number    a numeric character of other type

        Pc, //Pc  Connector_Punctuation a connecting punctuation mark, like a tie
        Pd, //Pd  Dash_Punctuation a dash or hyphen punctuation mark
        Ps, //Ps  Open_Punctuation an opening punctuation mark(of a pair)
        Pe, //Pe Close_Punctuation   a closing punctuation mark(of a pair)
        Pi, //Pi Initial_Punctuation     an initial quotation mark
        Pf, //Pf Final_Punctuation   a final quotation mark
        Po, //Other_Punctuation   a punctuation mark of other type

        Sm, //Sm  Math_Symbol a symbol of mathematical use
        Sc, //Sc Currency_Symbol     a currency sign
        Sk, //Sk  Modifier_Symbol a non-letterlike modifier symbol
        So, //So  Other_Symbol a symbol of other type

        Zs, //Zs  Space_Separator a space character(of various non-zero widths)
        Zl, //Zl Line_Separator  U+2028 LINE SEPARATOR only
        Zp, //Zp  Paragraph_Separator U+2029 PARAGRAPH SEPARATOR only


        Cc, //Cc  Control a C0 or C1 control code  
        Cf, //Cf  Format a format control character 
        Cs,// Surrogate a surrogate code point
        Co,// Private_Use a private-use character
        Cn,  //Cn 	Unassigned 	a reserved unassigned code point or a noncharacter

    }

    class UnicodeDataTxtParser
    {
        List<CharData> _chars = new List<CharData>();


        Dictionary<string, CharKind> _charKinds = new Dictionary<string, CharKind>();

        public UnicodeDataTxtParser()
        {
            _(CharKind.Lu, "Lu");
            _(CharKind.Ll, "Ll");
            _(CharKind.Lt, "Lt");
            _(CharKind.Lm, "Lm");
            _(CharKind.Lo, "Lo");

            _(CharKind.Mn, "Mn");
            _(CharKind.Mc, "Mc");
            _(CharKind.Me, "Me");

            _(CharKind.Nd, "Nd");
            _(CharKind.Nl, "Nl");
            _(CharKind.No, "No");

            _(CharKind.Pc, "Pc");
            _(CharKind.Pd, "Pd");
            _(CharKind.Ps, "Ps");
            _(CharKind.Pe, "Pe");
            _(CharKind.Pi, "Pi");
            _(CharKind.Pf, "Pf");
            _(CharKind.Po, "Po");

            _(CharKind.Sm, "Sm");
            _(CharKind.Sc, "Sc");
            _(CharKind.Sk, "Sk");
            _(CharKind.So, "So");

            _(CharKind.Zs, "Zs");
            _(CharKind.Zl, "Zl");
            _(CharKind.Zp, "Zp");

            _(CharKind.Cc, "Cc");
            _(CharKind.Cf, "Cf");
            _(CharKind.Cs, "Cs");
            _(CharKind.Co, "Co");
            _(CharKind.Cn, "Cn");

        }
        void _(CharKind kind, string str)
        {
            _charKinds.Add(str, kind);
        }
        public void Parse(string filename)
        { 
            using (FileStream fs = new FileStream(filename, FileMode.Open))
            using (StreamReader reader = new StreamReader(fs))
            {
                string line = reader.ReadLine();

                int m = 0;
                while (line != null)
                {
                    string[] splits = line.Split(';');

                    CharData char_data = new CharData();

                    int codepoint = int.Parse(splits[0], System.Globalization.NumberStyles.HexNumber);

                    char_data.codepoint = codepoint;
                    char_data.name = splits[1].Trim();
                    char_data.kind = _charKinds[splits[2].Trim()];
                    _chars.Add(char_data);
                    //
                    line = reader.ReadLine();
                    m++;
                }
            }


        }


    }
}