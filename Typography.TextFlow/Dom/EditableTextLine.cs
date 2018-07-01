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
        public bool LineContentChanged
        {
            get { return _lineContentChanged; }
        }
        public int LineNumber
        {
            get { return _lineNumber; }
            set { _lineNumber = value; }
        }
        /// <summary>
        /// explicit end line
        /// </summary>
        internal bool ExplicitEnd
        {
            get;
            set;
        }
        public List<IRun> UnsageGetTextRunList()
        {
            return _runList;
        }
#if DEBUG
        public override string ToString()
        {
            return _lineNumber.ToString();
        }
#endif
    }
}