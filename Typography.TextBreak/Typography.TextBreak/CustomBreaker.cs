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
        EngBreakingEngine engBreakingEngine = new EngBreakingEngine();
        //current lang breaking engine
        BreakingEngine breakingEngine;
        List<BreakingEngine> otherEngines = new List<BreakingEngine>();

        WordVisitor visitor;
        int _endAt;

        public CustomBreaker()
        {
            visitor = new WordVisitor(this);
            breakingEngine = engBreakingEngine;
        }
        public void AddBreakingEngine(BreakingEngine engine)
        {
            //TODO: make this accept more than 1 engine
            otherEngines.Add(engine);
            breakingEngine = engine;
        }

        protected BreakingEngine SelectEngine(char c)
        {
            if (breakingEngine.CanHandle(c))
            {
                return breakingEngine;
            }
            else
            {
                //find other engine
                for (int i = otherEngines.Count - 1; i >= 0; --i)
                {
                    //not the current engine 
                    //and can handle the character
                    BreakingEngine engine = otherEngines[i];
                    if (engine != breakingEngine && engine.CanHandle(c))
                    {
                        return engine;
                    }
                }

                //default 
#if DEBUG
                if (!engBreakingEngine.CanHandle(c))
                {
                    //even default can't handle the char

                }
#endif
                return engBreakingEngine;
            }
        }
        public void BreakWords(char[] charBuff, int startAt, int len, bool throwIfCharOutOfRange = true)
        {
            //conver to char buffer 
            int j = charBuff.Length;
            if (j < 1)
            {
                _endAt = 0;
                return;
            }
            _endAt = startAt + len;
            visitor.LoadText(charBuff, startAt);
            //---------------------------------------- 
            BreakingEngine currentEngine = breakingEngine = SelectEngine(charBuff[startAt]);
            //----------------------------------------
            //select breaking engine
            int endAt = startAt + len;

            for (; ; )
            {
                //----------------------------------------
                currentEngine.BreakWord(visitor, charBuff, startAt, endAt - startAt); //please note that len is decreasing
                switch (visitor.State)
                {
                    default: throw new NotSupportedException();

                    case VisitorState.End:
                        //ok
                        return;
                    case VisitorState.OutOfRangeChar:
                        {
                            //find proper breaking engine for current char

                            BreakingEngine anotherEngine = SelectEngine(visitor.Char);
                            if (anotherEngine == currentEngine)
                            {
                                if (throwIfCharOutOfRange) throw new NotSupportedException($"A proper breaking engine for character '{visitor.Char}' was not found.");
                                startAt = visitor.CurrentIndex + 1;
                                visitor.SetCurrentIndex(startAt);
                                visitor.AddWordBreakAtCurrentIndex(WordKind.Unknown);
                            }
                            else
                            {
                                currentEngine = anotherEngine;
                                startAt = visitor.CurrentIndex;
                            }
                        }
                        break;
                }
            }
        }

        public void BreakWords(string inputstr, bool throwIfCharOutOfRange = true)
        {
            //TODO: review here
            char[] buffer = inputstr.ToCharArray();
            BreakWords(buffer, 0, inputstr.Length, throwIfCharOutOfRange); //all
        }
        public void LoadBreakAtList(List<BreakAtInfo> outputList)
        {
            outputList.AddRange(visitor.GetBreakList());
        }
        public void LoadBreakAtList(List<int> outputList)
        {
            List<BreakAtInfo> breakAtList = visitor.GetBreakList();
            int j = breakAtList.Count;
            for (int i = 0; i < j; ++i)
            {
                BreakAtInfo brk = breakAtList[i];
                outputList.Add(brk.breakAt);
            }
        }
        public bool CanBeStartChar(char c)
        {
            return breakingEngine.CanBeStartChar(c);
        }

        public int BreakAtCount
        {
            get { return visitor.GetBreakList().Count; }
        }
        public IEnumerable<BreakSpan> GetBreakSpanIter()
        {
            List<BreakAtInfo> breakAtList = visitor.GetBreakList();
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


        //
    }


}
