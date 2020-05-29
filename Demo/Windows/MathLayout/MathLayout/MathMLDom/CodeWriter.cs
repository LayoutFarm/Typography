//MIT, 2020, Brezza92
using System;
using System.IO;
using System.Text;
namespace MathLayout
{
    class CodeWriter
    {
        readonly StringBuilder _sb = new StringBuilder();
        bool _enable_debugLineNo = true;
        int _commentNote = 0;

        public int Indent { get; set; }
        public CodeWriter()
        {

        }
        public void AppendLine() => _sb.AppendLine();
        public void Append(string text)
        {
            if (_enable_debugLineNo)
            {
                _sb.Append("/*x" + _commentNote + "*/");
                _commentNote++;
            }
            _sb.Append(text);
        }
        public void AppendLine(string line)
        {
            AppentIndent();
            if (_enable_debugLineNo)
            {
                _sb.Append("/*x" + _commentNote + "*/");
                _commentNote++;
            }

            _sb.AppendLine(line);

        }
        private void AppentIndent()
        {
            if (Indent > 0)
            {
                for(int i = 0; i < Indent; i++)
                {
                    _sb.Append("    ");
                }
            }
        }
        public void Clear()
        {
            _sb.Length = 0;
        }
        public override string ToString()
        {
            return _sb.ToString();
        }
    }
}