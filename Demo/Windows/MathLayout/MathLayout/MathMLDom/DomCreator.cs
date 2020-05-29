//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Text;
namespace MathLayout
{
    public class DomCreator
    {
        Dictionary<string, MathElemDefinition> _mathElemDefs = new Dictionary<string, MathElemDefinition>();
        public void ReadDomSpec(string domSpecFile)
        {
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(domSpecFile);
            //
            foreach (XmlNode node in xmldoc.DocumentElement)
            {
                if (node is XmlElement elem)
                {
                    switch (elem.Name)
                    {
                        case "math-elem-def-list":
                            {
                                //all elem in side this
                                //is a class definition
                                ParseMathElementList(elem);
                            }
                            break;
                    }
                }
            }

            CodeWriter codeWriter = new CodeWriter();
            codeWriter.AppendLine("//Autogen," + DateTime.Now.ToString("s"));
            codeWriter.AppendLine("//-----");
            codeWriter.AppendLine();
            //-------------------------------------
            codeWriter.AppendLine("using System;");
            codeWriter.AppendLine("using System.IO;");
            codeWriter.AppendLine("using System.Xml;");
            codeWriter.AppendLine("namespace MathLayout{");
            //-------------------------------------


            //-------------------------------------
            codeWriter.AppendLine("//PART1: node-definitions");

            foreach (MathElemDefinition mathdef in _mathElemDefs.Values)
            {
                codeWriter.AppendLine("//" + mathdef.Note);
                codeWriter.AppendLine("public partial class " + mathdef.NodeName + ":MathNode{");
                codeWriter.AppendLine("public override string Name=>\"" + mathdef.NodeName + "\";");
                //codeWriter.AppendLine("public override string ToString()=> Name;");
                codeWriter.AppendLine("}"); //close class
            }
            //-------------------------------------



            codeWriter.AppendLine("//PART2: node-parse-registrations");
            codeWriter.AppendLine("partial class DomNodeDefinitionStore{");
            codeWriter.AppendLine("partial void LoadNodeDefinition(){");
            foreach (MathElemDefinition mathdef in _mathElemDefs.Values)
            {
                codeWriter.AppendLine("Register(\"" + mathdef.NodeName + "\",()=> new " + mathdef.NodeName + "());");
            }
            codeWriter.AppendLine("}"); //LoadNodeDefinition()
            codeWriter.AppendLine("}");//class DomNodeDefinitionStore 

            codeWriter.AppendLine("}");//namespace MathLayout
            //write to file
            string all = codeWriter.ToString();
            File.WriteAllText("..\\..\\MathMLDom\\DomAutoGen.cs", all);

        }
        void ParseMathElementList(XmlElement math_elem_def)
        {
            foreach (XmlNode node in math_elem_def)
            {
                if (node is XmlElement elem)
                {
                    MathElemDefinition elemDef = new MathElemDefinition() {
                        NodeName = elem.Name,
                        Note = elem.GetAttribute("note")
                    };
                    _mathElemDefs.Add(elemDef.NodeName, elemDef);
                }
            }
        }
        class MathElemDefinition
        {
            public string NodeName { get; set; }
            public string Note { get; set; }
#if DEBUG
            public override string ToString() => NodeName;
#endif
        }
    }
}