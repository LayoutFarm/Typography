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

using Typography.TextBreak.SheenBidi.Data;

namespace Typography.TextBreak.SheenBidi.Collections
{
    internal class RunQueue
    {
        private class List
        {
            public const int Length = 8;
            public const int MaxIndex = (Length - 1);

            public readonly LevelRun[] Runs = new LevelRun[Length];

            public List previous;
            public List next;
        }

        List _frontList;
        int _frontTop;

        List _rearList;
        int _rearTop;

        List _isolatingList;
        int _isolatingTop;

        public RunQueue()
        {
            _frontList = new List();
            _frontTop = 0;

            _rearList = _frontList;
            _rearTop = -1;

            _isolatingList = null;
            _isolatingTop = -1;

            Count = 0;
        }

        public bool IsEmpty => Count == 0;


        public int Count { get; private set; }

        public bool ShouldDequeue => (_isolatingTop == -1);


        public void Enqueue(LevelRun levelRun)
        {
            if (_rearTop == List.MaxIndex)
            {
                List list = _rearList.next;
                if (list == null)
                {
                    list = new List();
                    list.previous = _rearList;
                    list.next = null;

                    _rearList.next = list;
                }

                _rearList = list;
                _rearTop = 0;
            }
            else
            {
                ++_rearTop;
            }

            ++Count;
            _rearList.Runs[_rearTop] = levelRun;

            // Complete the latest isolating run with this terminating run.
            if (_isolatingTop != -1 && levelRun.IsIsolateTerminator)
            {
                LevelRun incompleteRun = _isolatingList.Runs[_isolatingTop];
                incompleteRun.AttachLevelRun(levelRun);
                FindPreviousIncompleteRun();
            }

            // Save the location of the isolating run.
            if (levelRun.IsIsolateInitiator)
            {
                _isolatingList = _rearList;
                _isolatingTop = _rearTop;
            }
        }

        public void Dequeue()
        {
            if (_frontTop == List.MaxIndex)
            {
                if (_frontList == _rearList)
                    _rearTop = -1;
                else
                    _frontList = _frontList.next;

                _frontTop = 0;
            }
            else
            {
                ++_frontTop;
            }

            --Count;
        }

        public LevelRun Peek() => _frontList.Runs[_frontTop];


        private void FindPreviousIncompleteRun()
        {
            List list = _isolatingList;
            int top = _isolatingTop;

            do
            {
                int limit = (list == _frontList ? _frontTop : 0);

                do
                {
                    LevelRun levelRun = list.Runs[top];
                    if (levelRun.IsPartialIsolate)
                    {
                        _isolatingList = list;
                        _isolatingTop = top;
                        return;
                    }
                } while (top-- > limit);

                list = list.previous;
                top = List.MaxIndex;
            } while (list != null);

            _isolatingList = null;
            _isolatingTop = -1;
        }
    }
}
