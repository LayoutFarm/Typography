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

namespace Typography.TextBreak.SheenBidi.Text
{
    internal class UnicodeLocator
    {
        UnicodeAgent _agent; 
        string _str;
        int _index;
        public UnicodeLocator()
        {
            _agent = new UnicodeAgent();
            Reset();
        }
        public UnicodeAgent Agent => _agent; 
        public void LoadString(string str)
        {
            _str = str;
            Reset();
        }

        public bool MoveTo(int index)
        {
            if (index >= 0 && index < _str.Length)
            {
                _index = index;
                _agent.length = 0;

                return MoveNext();
            }

            Reset();
            return false;
        }

        public bool MoveNext()
        {
            // Note: _index is expected to be correct in this function.

            _index += _agent.length;
            _agent.index = _index;

            int remaining = _str.Length - _index;
            switch (remaining)
            {
                case 0:
                    Reset();
                    return false;

                case 1:
                    _agent.length = 1;
                    _agent.unicode = _str[_index];
                    break;

                default:
                    char high = _str[_index];
                    if (high >= 0xD800 && high <= 0xDBFF)
                    {
                        char low = _str[_index + 1];
                        if (low >= 0xDC00 && low <= 0xDFFF)
                        {
                            _agent.length = 2;
                            _agent.unicode = (high << 10) + low - 0x35FDC00;
                            return true;
                        }
                    }

                    _agent.length = 1;
                    _agent.unicode = high;
                    break;
            }

            return true;
        }

        public void Reset()
        {
            _index = 0;
            _agent.index = -1;
            _agent.length = 0;
            _agent.unicode = 0;
        }
    }
}
