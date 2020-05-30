//MIT, 2020, Brezza92
using System;
using System.Collections.Generic;
namespace MathLayout
{
    public abstract class MathNode
    {
        List<MathNode> _children = new List<MathNode>();
        Dictionary<string, string> _attributes = new Dictionary<string, string>();
        public int ChildCount => _children.Count;
        public MathNode GetNode(int index) => _children[index];
        public abstract string Name { get; }
        public string Text { get; set; }

        public virtual void AddAttribute(string attName, string attValue)
        {
            if (!_attributes.ContainsKey(attName))
            {
                _attributes.Add(attName, attValue);
            }
        }
        public virtual string GetAttributeValue(string attName)
        {
            if (_attributes.TryGetValue(attName, out string v))
            {
                return v;
            }
            return null;
        }
        public virtual void AppendChild(MathNode node)
        {
            _children.Add(node);
        }
        public override string ToString()
        {
            return Name + "," + Text;
        }
    }
    public class math : MathNode
    {
        public override string Name => "math";
        public override string ToString() => Name;
    }
}