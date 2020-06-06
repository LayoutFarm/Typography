//MIT, 2020, WinterDev
//simple ICU data parser
using System;
using System.Collections.Generic;
using System.IO;

namespace Tools
{
    public class IcuDataTree
    {
        public IcuDataNode RootNode { get; set; }
    }
    public class IcuDataNode
    {
        List<IcuDataNode> _children;

        public string Name { get; set; }
        public string TextValue { get; set; }
        public void AddChild(IcuDataNode node)
        {
            if (_children == null) { _children = new List<IcuDataNode>(); }
            _children.Add(node);
        }

        public int ChildCount => (_children == null) ? 0 : _children.Count;
        public IcuDataNode GetNode(int index) => _children[index];

#if DEBUG
        public override string ToString()
        {
            return Name;
        }
#endif

        public IcuDataNode GetFirstChildNode(string name)
        {
            if (_children == null) return null;
            int j = _children.Count;
            for (int i = 0; i < j; ++i)
            {
                if (_children[i].Name == name)
                {
                    //first found
                    return _children[i];
                }
            }
            return null;
        }

    }
    public class IcuDataParser
    {
        public IcuDataTree ResultTree { get; set; }
        public void ParseData(Stream stream)
        {
            //simple parser
            IcuDataTree result = new IcuDataTree();
            ResultTree = result;

            Stack<IcuDataNode> nodeStack = new Stack<IcuDataNode>();
            IcuDataNode curNode = null;
            using (StreamReader reader = new StreamReader(stream))
            {
                //simple parser 
                string line = reader.ReadLine();
                while (line != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("//"))
                    {
                        line = reader.ReadLine();
                        continue;
                    }

                    if (line.EndsWith("{"))
                    {
                        //open new node
                        int indexOf = line.IndexOf("{");
                        if (indexOf != line.Length - 1)
                        {
                            throw new NotSupportedException();
                        }

                        IcuDataNode newNode = new IcuDataNode();
                        newNode.Name = line.Substring(0, indexOf);

                        if (curNode != null)
                        {
                            curNode.AddChild(newNode);
                            nodeStack.Push(curNode);
                        }
                        curNode = newNode;

                    }
                    else if (line.StartsWith("}"))
                    {
                        //
                        if (nodeStack.Count > 0)
                        {
                            curNode = nodeStack.Pop();
                        }
                    }
                    else if (line.EndsWith("}"))
                    {
                        int firstBrace = line.IndexOf('{');
                        if (firstBrace > -1)
                        {
                            int lastBrace = line.LastIndexOf('}');

                            if (lastBrace > -1)
                            {
                                string nodeName = line.Substring(0, firstBrace);
                                string data = line.Substring(firstBrace + 1, lastBrace - firstBrace - 1);
                                //remove string quote
                                if (data.StartsWith("\"") && data.EndsWith("\""))
                                {
                                    IcuDataNode newnode = new IcuDataNode();
                                    newnode.Name = nodeName;
                                    newnode.TextValue = data.Substring(1, data.Length - 2);
                                    curNode.AddChild(newnode);
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
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }

                    }

                    //--------------------------
                    line = reader.ReadLine();
                    //--------------------------
                }
            }


            result.RootNode = curNode;
        }
    }
}