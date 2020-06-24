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

namespace Typography.TextBreak.SheenBidi.Data
{
    enum CharType : byte
    {
        Nil = 0,
        L = 1,
        R = 2,
        AL = 3,
        EN = 4,
        ES = 10,
        ET = 6,
        AN = 5,
        CS = 9,
        NSM = 7,
        BN = 8,
        B = 11,
        S = 12,
        WS = 14,
        ON = 13,
        LRE = 19,
        RLE = 20,
        LRO = 21,
        RLO = 22,
        PDF = 23,
        LRI = 15,
        RLI = 16,
        FSI = 17,
        PDI = 18,
    }
}
