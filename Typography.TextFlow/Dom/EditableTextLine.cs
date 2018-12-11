//MIT, 2014-present, WinterDev
using System.Collections.Generic;

namespace Typography.TextLayout
{
    public class EditableTextLine
    {
        List<IRun> _runList = new List<IRun>();
        bool _lineContentChanged = true;
        int _lineNumber;

        public void AppendLast(IRun run)
        {
            _runList.Add(run);
            _lineContentChanged = true;
        }
        //
        public bool LineContentChanged => _lineContentChanged;
        public int LineNumber { get; set; }
        /// <summary>
        /// explicit end line
        /// </summary>
        internal bool ExplicitEnd { get; set; }
        public List<IRun> UnsageGetTextRunList() => _runList;
#if DEBUG
        public override string ToString()
        {
            return LineNumber.ToString();
        }
#endif
    }
}