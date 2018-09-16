//MIT, 2016-present, WinterDev
// some code from icu-project
// © 2016 and later: Unicode, Inc. and others.
// License & terms of use: http://www.unicode.org/copyright.html#License

using System;
using System.Collections.Generic;

namespace Typography.TextBreak
{
    public class CustomBreaker
    {
        //default for latin breaking engine
        EngBreakingEngine _engBreakingEngine = new EngBreakingEngine();
        //current lang breaking engine
        BreakingEngine _breakingEngine;
        List<BreakingEngine> _otherEngines = new List<BreakingEngine>();

        WordVisitor _visitor;
        int _endAt;
        bool _breakNumberAfterText;


        public CustomBreaker()
        {
            ThrowIfCharOutOfRange = false;
            //
            _visitor = new WordVisitor();
            _breakingEngine = _engBreakingEngine; //default eng-breaking engine
        }

        public EngBreakingEngine EngBreakingEngine
        {
            get { return _engBreakingEngine; }
        }


        public bool BreakNumberAfterText
        {
            get { return _breakNumberAfterText; }
            set
            {
                _breakNumberAfterText = value;
                _engBreakingEngine.BreakNumberAfterText = value;
                //TODO: apply to other engine
            }
        }
        public bool ThrowIfCharOutOfRange { get; set; }

        public void AddBreakingEngine(BreakingEngine engine)
        {
            //TODO: make this accept more than 1 engine
            _otherEngines.Add(engine);
            _breakingEngine = engine;
        }

        protected BreakingEngine SelectEngine(char c)
        {
            if (_breakingEngine.CanHandle(c))
            {
                return _breakingEngine;
            }
            else
            {
                //find other engine
                for (int i = _otherEngines.Count - 1; i >= 0; --i)
                {
                    //not the current engine 
                    //and can handle the character
                    BreakingEngine engine = _otherEngines[i];
                    if (engine != _breakingEngine && engine.CanHandle(c))
                    {
                        return engine;
                    }
                }

                //default 
#if DEBUG
                if (!_engBreakingEngine.CanHandle(c))
                {
                    //even default can't handle the char

                }
#endif
                return _engBreakingEngine;
            }
        }


        public void BreakWords(char[] charBuff, int startAt, int len)
        {
            //conver to char buffer 
            int j = charBuff.Length;
            if (j < 1)
            {
                _endAt = 0;
                return;
            }
            _endAt = startAt + len;
            _visitor.LoadText(charBuff, startAt, len);
            //---------------------------------------- 
            BreakingEngine currentEngine = _breakingEngine = SelectEngine(charBuff[startAt]);
            //----------------------------------------
            //select breaking engine
            int endAt = startAt + len;

            for (; ; )
            {
                //----------------------------------------
                currentEngine.BreakWord(_visitor, charBuff, startAt, endAt - startAt); //please note that len is decreasing
                switch (_visitor.State)
                {
                    default: throw new NotSupportedException();

                    case VisitorState.End:
                        //ok
                        return;
                    case VisitorState.OutOfRangeChar:
                        {
                            //find proper breaking engine for current char

                            BreakingEngine anotherEngine = SelectEngine(_visitor.Char);
                            if (anotherEngine == currentEngine)
                            {
                                if (ThrowIfCharOutOfRange) throw new NotSupportedException($"A proper breaking engine for character '{_visitor.Char}' was not found.");
                                startAt = _visitor.CurrentIndex + 1;
                                _visitor.SetCurrentIndex(startAt);
                                _visitor.AddWordBreakAtCurrentIndex(WordKind.Unknown);
                            }
                            else
                            {
                                currentEngine = anotherEngine;
                                startAt = _visitor.CurrentIndex;
                            }
                        }
                        break;
                }
            }
        }

        public void BreakWords(string inputstr)
        {
            BreakWords(MemoryExtensions. buffer, 0, inputstr.Length); //all
        }

        /// <summary>
        /// copy break-at result to outputList
        /// </summary>
        /// <param name="outputList"></param>
        public void CopyBreakResults(List<BreakAtInfo> outputList)
        {
            outputList.AddRange(_visitor.GetBreakList());
        }

        /// <summary>
        /// copy break-at result (only break pos) to outputList
        /// </summary>
        /// <param name="outputList"></param>
        public void CopyBreakResults(List<int> outputList)
        {
            List<BreakAtInfo> breakAtList = _visitor.GetBreakList();
            int j = breakAtList.Count;
            for (int i = 0; i < j; ++i)
            {
                BreakAtInfo brk = breakAtList[i];
                outputList.Add(brk.breakAt);
            }
        }


        public int BreakItemCount
        {
            get { return _visitor.GetBreakList().Count; }
        }
        public IEnumerable<BreakSpan> GetBreakSpanIter()
        {
            List<BreakAtInfo> breakAtList = _visitor.GetBreakList();
            int c_index = 0;
            int count = breakAtList.Count;
            for (int i = 0; i < count; ++i)
            {

                BreakAtInfo brkInfo = breakAtList[i];
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = (ushort)(brkInfo.breakAt - c_index);
                sp.wordKind = brkInfo.wordKind;

                c_index += sp.len;

                yield return sp;
            }
            //-------------------
            if (c_index < _endAt)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = (ushort)(_endAt - c_index);
                yield return sp;
            }
        }


        public BreakingEngine GetBreakingEngineFor(char c)
        {
            return SelectEngine(c);
        }
        //
    }


}
