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
                if (_engBreakingEngine.CanHandle(c)) return _engBreakingEngine;
                //find other engine
                for (int i = _otherEngines.Count - 1; i >= 0; --i)
                {
                    //check if engine can handle the character
                    BreakingEngine engine = _otherEngines[i];
                    if (engine.CanHandle(c)) return engine;
                }
                return null;
            }
        }

        public BreakingEngine GetBreakingEngineFor(char c) => SelectEngine(c);

        public void BreakWords(string inputstr, ICollection<BreakSpan> outputBreakSpanList) =>
            BreakWords(inputstr.AsSpan(), outputBreakSpanList);

        public void BreakWords(string inputstr, ICollection<BreakAtInfo> outputBreakAtList) =>
            BreakWords(inputstr.AsSpan(), outputBreakAtList);

        private static BreakAtInfo BreakSpanToAt(BreakSpan span) => new BreakAtInfo(span.startAt + span.len, span.wordKind);
        public void BreakWords(ReadOnlySpan<char> charBuff, ICollection<BreakAtInfo> outputBreakAtList) =>
            BreakWords(charBuff, span => outputBreakAtList.Add(BreakSpanToAt(span)));

        public void BreakWords(ReadOnlySpan<char> charBuff, ICollection<BreakSpan> outputBreakSpanList) =>
            BreakWords(charBuff, outputBreakSpanList.Add);
        
        public void BreakWords(ReadOnlySpan<char> charBuff, Action<BreakSpan> outputBreakSpanAction)
        {
            //convert to char buffer 
            if (charBuff.IsEmpty) return;
            var visitor = new WordVisitor(charBuff, outputBreakSpanAction);
            //---------------------------------------- 
            Select_Engine: BreakingEngine currentEngine = SelectEngine(visitor.CurrentChar);
            //----------------------------------------
            if (currentEngine == null)
            {
                //find proper breaking engine for current char
                if (ThrowIfCharOutOfRange) throw new NotSupportedException($"A proper breaking engine for character '{visitor.CurrentChar}' was not found.");
                visitor.SetCurrentIndex(visitor.CurrentIndex + 1);
                visitor.AddWordBreakAtCurrentIndex(WordKind.Unknown);
                if (visitor.IsEnd) return;
                goto Select_Engine;
            }
            //select breaking engine
            _breakingEngine = currentEngine;

            while (!visitor.IsEnd)
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
                        goto Select_Engine;
                }
            }
        }
    }
}
