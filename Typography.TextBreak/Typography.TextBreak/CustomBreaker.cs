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


        public CustomBreaker(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            ThrowIfCharOutOfRange = false;
            _breakingEngine = _engBreakingEngine; //default eng-breaking engine
            _visitor = new WordVisitor(newWordBreakHandler);
        }
        public void SetNewBreakHandler(NewWordBreakHandlerDelegate newWordBreakHandler)
        {
            _visitor = new WordVisitor(newWordBreakHandler);
        }
        //
        public EngBreakingEngine EngBreakingEngine => _engBreakingEngine;
        //
        public bool BreakNumberAfterText
        {
            get => _breakNumberAfterText;
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

        public void BreakWords(char[] charBuff)
        {
            BreakWords(charBuff, 0, charBuff.Length);
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
            //TODO: review here
            char[] buffer = inputstr.ToCharArray();
            BreakWords(buffer, 0, inputstr.Length); //all
        }
        public BreakingEngine GetBreakingEngineFor(char c)
        {
            return SelectEngine(c);
        }
        //
    }


}
