//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
namespace MathLayout
{

    partial class DomNodeDefinitionStore
    {
        delegate MathNode MathNodeCreatorDelegate();

        //another part of this is on autogen side
        public DomNodeDefinitionStore()
        {
            LoadNodeDefinition();
        }
        partial void LoadNodeDefinition();//this is partial method, the implementation is on autogen side

        Dictionary<string, MathNodeCreatorDelegate> _nodeCreatorDic = new Dictionary<string, MathNodeCreatorDelegate>();
        void Register(string nodeName, MathNodeCreatorDelegate creatorFunc)
        {
            _nodeCreatorDic.Add(nodeName, creatorFunc);
        }
        public MathNode CreateMathNode(XmlElement xmlElem)
        {
            if (!_nodeCreatorDic.TryGetValue(xmlElem.Name, out MathNodeCreatorDelegate creator))
            {
                System.Diagnostics.Debugger.Break();//???
            }
            //if found this
            MathNode newNode = creator();
            return newNode;
        }
    }

    public class MathMLReader
    {
        DomNodeDefinitionStore _defStore;

        public List<math> ResultMathNodes { get; private set; }
        public void Read(string filename)
        {
            _defStore = new DomNodeDefinitionStore();
            ResultMathNodes = new List<math>();
            //read math ml from file 
            //for resolve character like &plusmn;
            string html = System.Net.WebUtility.HtmlDecode(File.ReadAllText(filename));
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(filename);
            //xmldoc.LoadXml(html);
            //
            //walk /find all <math> node


            foreach (XmlElement mathElm in xmldoc.DocumentElement.SelectNodes("//math")) //select all math nodes in the document
            {
                //begin with math node
                math math = new math();

                Parse(mathElm, math); 
                //FINISH each math node
                ResultMathNodes.Add(math);
            }



        }
        void Parse(XmlElement mathNodeElem, MathNode parentNode)
        {
            //this is a single math node
            foreach (XmlAttribute att in mathNodeElem.Attributes)
            {
                parentNode.AddAttribute(att.Name, att.Value);
            }

            foreach (XmlNode node in mathNodeElem)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Comment:
                        //nothing
                        break;
                    case XmlNodeType.Text:
                        {
                            //text content inside parent node
                            XmlText textnode = (XmlText)node;
                            string value = textnode.Value.Trim();
                            if (value.Length > 0)
                            {
                                parentNode.Text = value;
                            }
                        }
                        break;
                    case XmlNodeType.Element:
                        {
                            //create dom from each node type
                            XmlElement elem = (XmlElement)node;
                            MathNode newNode = _defStore.CreateMathNode(elem);
                            parentNode.AppendChild(newNode);
                            //inside this elem may has child
                            //parse node inside this
                            Parse(elem, newNode);
                        }
                        break;
                }
            }
        }

    }
}