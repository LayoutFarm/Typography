//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MathLayout
{
    public class OperatorInfoDictionary
    {
        public Dictionary<string, OperatorInfo> Result = new Dictionary<string, OperatorInfo>();
        public void Read(string filename)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filename);

            foreach (XmlElement opElement in xmldoc.DocumentElement.SelectNodes("//tr"))
            {
                ParseAndAddToDictionary(opElement);
            }
        }

        public bool IsStretchable(string op)
        {
            OperatorInfo operatorInfo;
            if (Result.TryGetValue(op, out operatorInfo))
            {
                if ((operatorInfo.Properties & OperatorProperty.Stretchy) > 0)
                {
                    return true;
                }
            }
            return false;
        }
        public void ParseAndAddToDictionary(XmlElement xmlElement)
        {
            XmlNode ch = xmlElement.ChildNodes.Item(0);
            XmlNode prop = xmlElement.ChildNodes.Item(7);
            string op = ParseOperator(ch.InnerText);
            OperatorProperty properties = ParseProperty(prop.InnerText);
            if (!Result.ContainsKey(op))
            {
                Result.Add(op, new OperatorInfo() { Operator = op, Properties = properties });
            }
        }

        private OperatorProperty ParseProperty(string propertiesText)
        {
            OperatorProperty property = OperatorProperty.None;
            if (propertiesText.Length > 0)
            {
                string[] props = propertiesText.Split(',');
                //stretchy, accent,separator, linebreakstyle=after,largeop, symmetric,movablelimits,fence
                foreach (string p in props)
                {
                    switch(p.Trim())
                    {
                        default: throw new NotSupportedException();
                        case "stretchy":
                            property |= OperatorProperty.Stretchy;
                            break;
                        case "accent":
                            property |= OperatorProperty.Accent;
                            break;
                        case "separator":
                            property |= OperatorProperty.Separator;
                            break;
                        case "linebreakstyle=after":
                            property |= OperatorProperty.LineBreakStyle;
                            break;
                        case "largeop":
                            property |= OperatorProperty.LargeOp;
                            break;
                        case "symmetric":
                            property |= OperatorProperty.Symmetric;
                            break;
                        case "movablelimits":
                            property |= OperatorProperty.MovableLimits;
                            break;
                        case "fence":
                            property |= OperatorProperty.Fence;
                            break;
                    }
                }
            }
            return property;
        }

        private string ParseOperator(string text)
        {
            //&#x2ADD;&#x338;
            if (text.StartsWith("&#x"))
            {
                int endIndex = text.IndexOf(';');
                string code = text.Substring(3, endIndex - 3);
                switch (code.Length)
                {
                    default:
                        throw new NotSupportedException();
                    case 1:
                        code = "0x000" + code;
                        break;
                    case 2:
                        code = "0x00" + code;
                        break;
                    case 3:
                        code = "0x0" + code;
                        break;
                    case 4:
                        code = "0x" + code;
                        break;
                }
                char ch = (char)Convert.ToInt32(code, 16);
                if (text.Length < endIndex + 1)
                {
                    text = text.Substring(endIndex + 1);
                    if (text.Length > 0)
                    {
                        return ch + ParseOperator(text);
                    }
                }
                return ch + "";
            }
            else if (text.StartsWith("&"))
            {
                //xml operator name
                int endIndex = text.IndexOf(';');
                string code = text.Substring(1, endIndex - 1);
                char ch = char.MinValue;
                switch (code)
                {
                    default:
                        throw new NotSupportedException();
                    case "amp":
                        ch = '&';
                        break;
                    case "lt":
                        ch = '<';
                        break;
                    case "gt":
                        ch = '>';
                        break;
                    case "p":
                        break;
                }
                if (text.Length < endIndex + 1)
                {
                    text = text.Substring(endIndex + 1);
                    if (text.Length > 0)
                    {
                        return ch + ParseOperator(text);
                    }
                }
                return ch + "";
            }
            else
            {
                return text;
            }
        }
    }

    public class OperatorInfo
    {
        public string Operator { get; set; }
        public OperatorProperty Properties { get; set; }
    }

    public enum OperatorProperty : uint
    {
        //stretchy, accent,separator, linebreakstyle=after,largeop, symmetric,movablelimits,fence
        None           =   0,
        Fence          =   1,//0000 0001
        Stretchy       =   2,//0000 0010
        Symmetric      =   4,//0000 0100
        Separator      =   8,//0000 1000
        Accent         =  16,//0001 0000
        LargeOp        =  32,//0010 0000
        MovableLimits  =  64,//0100 0000
        LineBreakStyle = 128,//1000 0000
    }
}