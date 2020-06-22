//Apache2, 2014, Muhammad Tayyab Akram, https://sheenbidi.codeplex.com/
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using Typography.TextBreak.SheenBidi.Data;
using Typography.TextBreak.SheenBidi.Collections;
using Typography.TextBreak.SheenBidi.Text;

namespace Typography.TextBreak.SheenBidi
{
    public class Paragraph
    {
        string _text;
        BaseDirection _direction;
        byte _baseLevel;
        CharType[] _types;
        byte[] _levels;

        public byte BaseLevel => _baseLevel;
        public BaseDirection Direction => _direction;
        public string Text => _text;
        internal CharType[] Types => _types;
        internal byte[] Levels => _levels;

        

        public Paragraph(string text, BaseDirection direction)
        {
            if (string.IsNullOrEmpty(text))
                throw (new ArgumentException("Text is empty."));

            _text = text;
            _direction = direction;
            _types = new CharType[text.Length];
            _levels = new byte[text.Length];

            BidiChain chain = new BidiChain();
            DetermineTypes(chain);
            DetermineBaseLevel(chain);
            DetermineLevels(chain);
        }

        public Paragraph(string text)
            : this(text, BaseDirection.AutoLeftToRight)
        {
        }

        

        //>>Determine Character Types

        private void DetermineTypes(BidiChain chain)
        {
            CharType type = CharType.Nil;

            UnicodeLocator locator = new UnicodeLocator();
            UnicodeAgent agent = locator.Agent;
            locator.LoadString(_text);

            while (locator.MoveNext())
            {
                CharType priorType = type;
                _types[agent.index] = type = CharTypeLookup.DetermineCharType(agent.unicode);

                switch (type)
                {
                    case CharType.ON:
                    case CharType.LRI:
                    case CharType.RLI:
                    case CharType.FSI:
                    case CharType.PDI:
                    case CharType.LRE:
                    case CharType.RLE:
                    case CharType.LRO:
                    case CharType.RLO:
                    case CharType.PDF:
                        AddConsecutiveLink(chain, type, agent.index);
                        break;

                    default:
                        if (priorType != type)
                        {
                            AddConsecutiveLink(chain, type, agent.index);
                        }
                        break;
                }

                if (agent.length == 2)
                {
                    _types[agent.index + 1] = CharType.BN;

                    if (priorType != CharType.BN)
                    {
                        AddConsecutiveLink(chain, CharType.BN, agent.index + 1);
                    }
                }
            }

            AddConsecutiveLink(chain, CharType.Nil, _text.Length);
        }

        private void AddConsecutiveLink(BidiChain chain, CharType type, int offset)
        {
            BidiLink lastLink = chain.LastLink;
            if (lastLink != null)
            {
                lastLink.length = offset - lastLink.offset;
            }

            BidiLink newLink = new BidiLink()
            {
                type = type,
                offset = offset
            };
            chain.AddLink(newLink);
        }

        //<<Determine Character Types


        //>>Determine Base Level

        private void DetermineBaseLevel(BidiChain chain)
        {
            switch (_direction)
            {
                case BaseDirection.LeftToRight:
                    _baseLevel = 0;
                    break;

                case BaseDirection.RightToLeft:
                    _baseLevel = 1;
                    break;

                default:
                    _baseLevel = DetermineBaseLevel(chain.RollerLink, chain.RollerLink,
                                                   (byte)(_direction == BaseDirection.AutoLeftToRight ? 0 : 1),
                                                   false);
                    break;
            }
        }

        private static byte DetermineBaseLevel(BidiLink skipLink, BidiLink breakLink, byte defaultLevel, bool isIsolate)
        {
            // Rules P2, P3
            for (BidiLink link = skipLink.Next; link != breakLink; link = link.Next)
            {
                CharType type = link.type;

                switch (type)
                {
                    case CharType.L:
                        return 0;

                    case CharType.AL:
                    case CharType.R:
                        return 1;

                    case CharType.LRI:
                    case CharType.RLI:
                    case CharType.FSI:
                        link = SkipIsolatingRun(link, breakLink);
                        if (link == null)
                        {
                            goto Default;
                        }
                        break;

                    case CharType.PDI:
                        if (isIsolate)
                        {
                            // In case of isolating run, PDI will be the last character.
                            // Note: Inner isolating runs will be skipped by the case
                            //       above this one.
                            goto Default;
                        }
                        break;
                }
            }

        Default:
            return 0;
        }

        private static BidiLink SkipIsolatingRun(BidiLink skipLink, BidiLink breakLink)
        {
            int depth = 1;

            for (BidiLink link = skipLink.Next; link != breakLink; link = link.Next)
            {
                CharType type = link.type;

                switch (type)
                {
                    case CharType.LRI:
                    case CharType.RLI:
                    case CharType.FSI:
                        ++depth;
                        break;

                    case CharType.PDI:
                        if (--depth == 0)
                        {
                            return link;
                        }
                        break;
                }
            }

            return null;
        }

        //<<Determine Base Level

        //>>Determine Levels

        private void DetermineLevels(BidiChain chain)
        {
            StatusStack stack = new StatusStack();
            RunQueue runQueue = new RunQueue();
            IsolatingRun isolatingRun = new IsolatingRun()
            {
                Text = _text,
                ParagraphLevel = _baseLevel
            };

            BidiLink roller = chain.RollerLink;
            BidiLink firstLink = null;
            BidiLink lastLink = null;

            BidiLink priorLink = roller;
            byte priorLevel = _baseLevel;

            CharType sor = CharType.Nil;
            CharType eor = CharType.Nil;

            // Rule X1
            int overIsolate = 0;
            int overEmbedding = 0;
            int validIsolate = 0;

            stack.Push(_baseLevel, CharType.ON, false);

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                bool forceFinish = false;
                bool bnEquivalent = false;

                CharType type = link.type;

                switch (type)
                {
                    /* Rule X2, X3, X4, X5, X5a, X5b, X5c */
                    case CharType.RLE:
                    case CharType.LRE:
                    case CharType.RLO:
                    case CharType.LRO:
                    case CharType.RLI:
                    case CharType.LRI:
                    case CharType.FSI:
                        bool isIsolate = (type == CharType.RLI || type == CharType.LRI || type == CharType.FSI);
                        bool isRTL = (type == CharType.RLE || type == CharType.RLO || type == CharType.RLI);

                        if (type == CharType.FSI)
                            isRTL = (DetermineBaseLevel(link, roller, (byte)0, true) == 1);

                        if (isIsolate)
                            link.level = stack.EmbeddingLevel;
                        else
                            bnEquivalent = true;

                        byte newLevel = (isRTL ? stack.OddLevel : stack.EvenLevel);
                        if (newLevel <= Level.MaxValue && overIsolate == 0 && overEmbedding == 0)
                        {
                            if (isIsolate)
                                ++validIsolate;

                            CharType overrideStatus = (type == CharType.LRO ? CharType.L
                                                       : type == CharType.RLO ? CharType.R
                                                       : CharType.ON);
                            stack.Push(newLevel, overrideStatus, isIsolate);
                        }
                        else
                        {
                            if (isIsolate)
                            {
                                ++overIsolate;
                            }
                            else
                            {
                                if (overIsolate == 0)
                                    ++overEmbedding;
                            }
                        }
                        break;

                    /* Rule X6 */
                    default:
                        link.level = stack.EmbeddingLevel;

                        if (stack.OverrideStatus != CharType.ON)
                        {
                            link.type = stack.OverrideStatus;

                            if (priorLink.type == link.type && priorLink.level == link.level)
                            {
                                // Properties of this link are same as previous link,
                                // therefore merge it and continue the loop.
                                priorLink.MergeNext();
                                continue;
                            }
                        }
                        break;

                    /* Rule X6a */
                    case CharType.PDI:
                        if (overIsolate != 0)
                        {
                            --overIsolate;
                        }
                        else if (validIsolate == 0)
                        {
                            // Do nothing
                        }
                        else
                        {
                            overEmbedding = 0;

                            while (!stack.IsolateStatus)
                            {
                                stack.Pop();
                            }
                            stack.Pop();

                            --validIsolate;
                        }

                        link.level = stack.EmbeddingLevel;
                        break;

                    /* Rule X7 */
                    case CharType.PDF:
                        bnEquivalent = true;

                        if (overIsolate != 0)
                        {
                            // Do nothing
                        }
                        else if (overEmbedding != 0)
                        {
                            --overEmbedding;
                        }
                        else if (!stack.IsolateStatus && stack.Count >= 2)
                        {
                            stack.Pop();
                        }
                        break;

                    // Rule X8
                    case CharType.B:
                        // These values are reset for clarity, in this implementation B
                        // can only occur as the last code in the array.
                        stack.Clear();

                        overIsolate = 0;
                        overEmbedding = 0;
                        validIsolate = 0;

                        link.level = _baseLevel;
                        break;

                    case CharType.BN:
                        bnEquivalent = true;
                        break;

                    case CharType.Nil:
                        forceFinish = true;
                        link.level = _baseLevel;
                        break;
                }

                // Rule X9
                if (bnEquivalent)
                {
                    // The type of this link is BN equivalent, so abandon it and
                    // continue the loop.
                    priorLink.AbandonNext();
                    continue;
                }

                if (sor == CharType.Nil)
                {
                    sor = Level.MakeExtremeType(_baseLevel, link.level);
                    firstLink = link;
                    priorLevel = link.level;
                }
                else if (priorLevel != link.level || forceFinish)
                {
                    // Save the current level i.e. level of the next run.
                    byte currentLevel = link.level;

                    // Since the level has changed at this link, therefore, the run
                    // must end at the prior link.
                    lastLink = priorLink;

                    // Now we have both the prior level and the current level i.e.
                    // unchanged levels of both the current run and the next run.
                    // So, identify eos of the current run.
                    // Note:
                    //     sor of the run has already been determined at this stage.
                    eor = Level.MakeExtremeType(priorLevel, currentLevel);

                    runQueue.Enqueue(new LevelRun(firstLink, lastLink, sor, eor));
                    if (runQueue.ShouldDequeue || forceFinish)
                    {
                        // Rule X10
                        while (!runQueue.IsEmpty)
                        {
                            LevelRun peek = runQueue.Peek();
                            if (!peek.IsAttachedTerminator)
                            {
                                isolatingRun.BaseLevelRun = peek;
                                isolatingRun.Resolve();
                            }

                            // Dequeue the run.
                            runQueue.Dequeue();
                        }
                    }

                    // The sor of next run (if any) will be technically equal to eor of
                    // this run.
                    sor = eor;
                    // The next run (if any) will start from this link.
                    firstLink = link;

                    priorLevel = currentLevel;
                }

                priorLink = link;
            }

            SaveLevels(chain);
        }

        private void SaveLevels(BidiChain chain)
        {
            BidiLink roller = chain.RollerLink;
            byte level = _baseLevel;
            int index = 0;

            for (BidiLink link = roller.Next; link != roller; link = link.Next)
            {
                int offset = link.offset;

                for (; index < offset; index++)
                {
                    _levels[index] = level;
                }

                level = link.level;
            }
        }

        //<<Determine Levels
    }
}
