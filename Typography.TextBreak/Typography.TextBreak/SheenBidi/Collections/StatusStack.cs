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

namespace Typography.TextBreak.SheenBidi.Collections
{
    class StatusStack
    {
        class List
        {
            public const int Length = 16;
            public const int MaxIndex = (Length - 1);

            public readonly byte[] EmbeddingLevel = new byte[Length];
            public readonly CharType[] OverrideStatus = new CharType[Length];
            public readonly bool[] IsolateStatus = new bool[Length];

            public List previous;
            public List next;
        }

        const int MaxElements = Level.MaxValue + 2;

        readonly List _firstList = new List();
        List _peekList;
        int _peekTop;

        public StatusStack()
        {
            Reset();
        }

        public void Reset()
        {
            _peekList = _firstList;
            _peekTop = 0;
            Count = 0;
        }

        public int Count { get; private set; }

        public bool IsEmpty => (Count == 0);

        public byte EmbeddingLevel => _peekList.EmbeddingLevel[_peekTop];

        public CharType OverrideStatus => _peekList.OverrideStatus[_peekTop];

        public bool IsolateStatus => _peekList.IsolateStatus[_peekTop];

        public byte EvenLevel => (byte)((EmbeddingLevel + 2) & ~1);

        public byte OddLevel => (byte)((EmbeddingLevel + 1) | 1);


        public void Clear()
        {
            Count = 0;
        }

        public void Push(byte embeddingLevel, CharType overrideStatus, bool isolateStatus)
        {
#if DEBUG
            if (Count == MaxElements)
                throw (new InvalidOperationException("The stack is full."));
#endif

            if (_peekTop != List.MaxIndex)
            {
                ++_peekTop;
            }
            else
            {
                if (_peekList.next == null)
                {
                    List list = new List();
                    list.previous = _peekList;
                    list.next = null;

                    _peekList.next = list;
                    _peekList = list;
                }
                else
                {
                    _peekList = _peekList.next;
                }

                _peekTop = 0;
            }
            ++Count;

            _peekList.EmbeddingLevel[_peekTop] = embeddingLevel;
            _peekList.OverrideStatus[_peekTop] = overrideStatus;
            _peekList.IsolateStatus[_peekTop] = isolateStatus;
        }

        public void Pop()
        {
#if DEBUG
            if (Count == 0)
                throw (new InvalidOperationException("The stack is empty."));
#endif

            if (_peekTop != 0)
            {
                --_peekTop;
            }
            else
            {
                _peekList = _peekList.previous;
                _peekTop = List.MaxIndex;
            }
            --Count;
        }
    }
}
