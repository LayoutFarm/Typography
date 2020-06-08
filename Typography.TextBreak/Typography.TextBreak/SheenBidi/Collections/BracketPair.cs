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
    class BracketPair
    {
        public int bracketUnicode;
        public BidiLink openingLink;
        public BidiLink closingLink;
        public BidiLink priorStrongLink;
        public CharType innerStrongType;

        public bool IsComplete => closingLink != null;

    }
}
