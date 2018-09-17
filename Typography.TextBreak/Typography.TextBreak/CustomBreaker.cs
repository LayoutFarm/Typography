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
        EngAndNonWordBreakingEngine _engBreakingEngine = new EngAndNonWordBreakingEngine();
        //current lang breaking engine
        BreakingEngine _breakingEngine;
        List<BreakingEngine> _otherEngines = new List<BreakingEngine>();
        int _endAt;
        bool _breakNumberAfterText;


        public CustomBreaker()
        {
            ThrowIfCharOutOfRange = false;
            _breakingEngine = _engBreakingEngine; //default eng-breaking engine
        }

        public EngAndNonWordBreakingEngine EngBreakingEngine
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
        
        public BreakingEngine GetBreakingEngineFor(char c)
        {
            return SelectEngine(c);
        }
        
        public void BreakWords(string inputstr, ICollection<BreakAtInfo> outputBreakAtList) =>
            BreakWords(inputstr.AsSpan(), outputBreakAtList);

        public void BreakWords(string inputstr, ICollection<BreakSpan> outputBreakSpanList) =>
            BreakWords(inputstr.AsSpan(), outputBreakSpanList);

        public void BreakWords(ReadOnlySpan<char> charBuff, ICollection<BreakSpan> outputBreakSpanList) =>
            BreakWords(charBuff, new BreakSpanProcessor(outputBreakSpanList.Add));

        public void BreakWords(ReadOnlySpan<char> charBuff, ICollection<BreakAtInfo> outputBreakAtList)
        {
            //convert to char buffer 
            if (charBuff.IsEmpty) return;
            var visitor = new WordVisitor(charBuff, outputBreakAtList, new Stack<int>());
            //---------------------------------------- 
            BreakingEngine currentEngine = _breakingEngine = SelectEngine(visitor.CurrentChar);
            //----------------------------------------
            //select breaking engine

            while(!visitor.IsEnd)
            {
                //----------------------------------------
                visitor = currentEngine.BreakWord(visitor, charBuff); //please note that len is decreasing
                switch (visitor.State)
                {
                    default: throw new NotSupportedException();
                    case VisitorState.End:
                        //ok
                        return;
                    case VisitorState.OutOfRangeChar:
                        {
                            //find proper breaking engine for current char
                            BreakingEngine anotherEngine = SelectEngine(visitor.CurrentChar);
                            if (anotherEngine == currentEngine)
                            {
                                if (ThrowIfCharOutOfRange) throw new NotSupportedException($"A proper breaking engine for character '{visitor.CurrentChar}' was not found.");
                                visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                                visitor.AddWordBreakAtCurrentIndex(WordKind.Unknown);
                            }
                            else
                            {
                                currentEngine = anotherEngine;
                                visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                            }
                        }
                        break;
                }
            }
        }
    }
}
