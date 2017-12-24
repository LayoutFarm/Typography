//MIT, 2016-2017, WinterDev
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
        }
        public void AddBreakingEngine(BreakingEngine engine)
        {
            //TODO: make this accept more than 1 engine
            otherEngines.Add(engine);
            breakingEngine = engine;
        }
        BreakingEngine SelectEngine(char c)
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

                return engBreakingEngine;
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
                                throw new NotSupportedException();
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

        public void BreakWords(string inputstr)
        {
            //TODO: review here
            char[] buffer = inputstr.ToCharArray();
            BreakWords(buffer, 0, inputstr.Length); //all
        }
        public void LoadBreakAtList(List<int> outputList)
        {
            outputList.AddRange(visitor.GetBreakList());
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
            List<int> breakAtList = visitor.GetBreakList();
            int c_index = 0;
            int i = 0;
            foreach (int breakAt in breakAtList)
            {
                BreakSpan sp = new BreakSpan();
                sp.startAt = c_index;
                sp.len = (ushort)(breakAtList[i] - c_index);
                c_index += sp.len;
                i++;
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
    }


}