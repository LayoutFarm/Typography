//MIT, 2020, Brezza92
using PixelFarm.Drawing.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace MathLayout
{
    public class OperatorTableCreator
    {
        public void AutogenFrom(string filename)
        {
            OperatorInfoDictionary operatorInfo = new OperatorInfoDictionary();
            operatorInfo.Read(filename);

            SeparateToPropertyDict(operatorInfo.Result);
            Autogen();
        }

        private void SeparateToPropertyDict(Dictionary<string, OperatorInfo> opDict)
        {
            foreach (var op in opDict)
            {
                OperatorInfo info = op.Value;
                OperatorProperty property = info.Properties;
                if (property == OperatorProperty.None)
                {
                    continue;
                }
                AddToDictIfContainProperty(info, OperatorProperty.Accent);
                AddToDictIfContainProperty(info, OperatorProperty.Fence);
                AddToDictIfContainProperty(info, OperatorProperty.LargeOp);
                AddToDictIfContainProperty(info, OperatorProperty.LineBreakStyle);
                AddToDictIfContainProperty(info, OperatorProperty.MovableLimits);
                AddToDictIfContainProperty(info, OperatorProperty.Separator);
                AddToDictIfContainProperty(info, OperatorProperty.Stretchy);
                AddToDictIfContainProperty(info, OperatorProperty.Symmetric);
            }
        }
        private void Autogen()
        {
            CodeWriter codeWriter = new CodeWriter();
            
            codeWriter.AppendLine("//Autogen," + DateTime.Now.ToString("s"));
            codeWriter.AppendLine("//-----");
            codeWriter.AppendLine("//Operator dictionary entries reference https://www.w3.org/TR/MathML3/appendixc.html");
            codeWriter.AppendLine();

            //generate namespace
            codeWriter.AppendLine("namespace MathLayout");
            codeWriter.AppendLine("{");//start namespace area
            codeWriter.Indent++;

            codeWriter.AppendLine("public static class MathMLOperatorTable");
            codeWriter.AppendLine("{");//start class area
            codeWriter.Indent++;
            codeWriter.AppendLine();

            //extend case
            codeWriter.AppendLine("public static bool IsInvicibleCharacter (char ch)");
            codeWriter.AppendLine("{");
            codeWriter.Indent++;
            
            codeWriter.AppendLine("switch ((int)ch)");
            codeWriter.AppendLine("{");
            codeWriter.Indent++;

            codeWriter.AppendLine("case 0x2061:");
            codeWriter.AppendLine("case 0x2062:");
            codeWriter.AppendLine("case 0x2063:");
            codeWriter.AppendLine("case 0x2064:");
            codeWriter.Indent++;
            codeWriter.AppendLine("return true;");
            codeWriter.Indent--;
            codeWriter.AppendLine("default:");
            codeWriter.Indent++;
            codeWriter.AppendLine("return false;");
            codeWriter.Indent--;

            codeWriter.Indent--;
            codeWriter.AppendLine("}");

            codeWriter.Indent--;
            codeWriter.AppendLine("}");

            foreach (var prop in _propertyDict)
            {
                codeWriter.AppendLine("public static bool Is" + prop.Key + "PropertyOperator (string op)");
                codeWriter.AppendLine("{");//start method area
                codeWriter.Indent++;

                codeWriter.AppendLine("switch (op)");
                codeWriter.AppendLine("{");//start switch case area
                codeWriter.Indent++;

                codeWriter.AppendLine("default: return false;");
                //each op in List
                foreach(var op in prop.Value)
                {
                    if (op.Operator != "\"")
                    {
                        codeWriter.AppendLine("case \"" + op.Operator + "\":");
                    }
                    else
                    {
                        codeWriter.AppendLine("case \"\\" + op.Operator + "\":");
                    }
                }
                codeWriter.AppendLine("return true;");

                codeWriter.Indent--;
                codeWriter.AppendLine("}");//end switch case area

                codeWriter.Indent--;
                codeWriter.AppendLine("}");//end method area
            }

            codeWriter.AppendLine();
            codeWriter.Indent--;
            codeWriter.AppendLine("}");//end class area

            codeWriter.Indent--;
            codeWriter.AppendLine("}");//end namespace area

            System.IO.File.WriteAllText("..\\..\\Operator Dictionary\\OperatorTableAutogen.cs", codeWriter.ToString());
        }

        private Dictionary<OperatorProperty, List<OperatorInfo>> _propertyDict = new Dictionary<OperatorProperty, List<OperatorInfo>>();

        private void AddToDictIfContainProperty(OperatorInfo info, OperatorProperty targetProoerty)
        {
            OperatorProperty infoProp = info.Properties;
            if ((infoProp &= targetProoerty) > 0)
            {
                List<OperatorInfo> temp;
                if (_propertyDict.TryGetValue(targetProoerty, out temp))
                {
                    temp.Add(info);
                }
                else
                {
                    List<OperatorInfo> infos = new List<OperatorInfo>();
                    infos.Add(info);
                    _propertyDict.Add(targetProoerty, infos);
                }
            }
        }
    }
}