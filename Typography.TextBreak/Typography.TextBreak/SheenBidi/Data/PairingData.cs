﻿//Apache2, 2014, Muhammad Tayyab Akram, https://sheenbidi.codeplex.com/
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
    partial class PairingLookup
    {
        private static readonly short[] PairDifferences = new short[] {
            0,     1,     -1,    2,     -2,    16,    -16,   3,     -3,    2016,  138,
            1824,  2104,  2108,  2106,  -138,  8,     7,     -8,    -7,    -1824, -2016,
            -2104, -2106, -2108
        };

        private static readonly byte[] PairData = new byte[] {
        // Index : 0x000
            65,  130, 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   3,   0,   4,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   67,  0,   132, 0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   67,  0,   132, 0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   5,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   6,   0,   0,
            0,   0,
        // Index : 0x098
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x130
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130,
            65,  130, 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x1C8
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   65,  130, 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x260
            0,   1,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130, 0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130, 0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x2F8
            0,   0,   0,   0,   0,   0,   0,   0,   7,   7,   7,   8,   8,   8,   0,
            0,   0,   0,   0,   0,   0,   9,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            1,   2,   0,   0,   0,   0,   0,   10,  0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   1,   2,   1,   2,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   2,   1,   2,   1,
            2,   1,   2,   0,   0,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,
            1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,
            2,   1,   2,   1,   2,   0,   0,   0,   1,   2,   1,   2,   0,   0,   0,
            0,   0,
        // Index : 0x390
            11,  0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   2,   0,   0,   12,
            0,   13,  14,  0,   14,  0,   0,   0,   0,   1,   2,   1,   2,   1,   2,
            1,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   1,   2,   1,   2,   15,  0,   0,   1,   2,   0,   0,
            0,   0,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,
            2,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   0,   0,   1,   2,
            16,  16,  16,  0,   17,  17,  0,   0,   18,  18,  18,  19,  19,  0,   0,
            0,   0,   0,   0,   0,   0,   0,   65,  130, 65,  130, 0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130, 0,   0,   0,
            0,   0,
        // Index : 0x428
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   65,  130, 65,  130, 65,  130, 65,  130, 65,  130, 65,  130, 65,  130,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   1,   2,   65,  130, 0,   1,   2,   0,   3,   0,   4,   0,   0,
            0,   0,   0,   0,   0,   1,   2,   0,   0,   0,   0,   0,   0,   1,   2,
            0,   0,   0,   1,   2,   1,   2,   65,  130, 65,  130, 65,  130, 65,  130,
            65,  130,
        // Index : 0x4C0
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130, 65,  130, 65,  130,
            65,  130, 65,  130, 71,  129, 66,  136, 65,  130, 65,  130, 65,  130, 65,
            130, 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x558
            20,  0,   0,   0,   0,   0,   0,   0,   1,   2,   0,   0,   1,   2,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   1,   2,   1,   2,   0,   1,   2,
            0,   0,   65,  130, 65,  130, 0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   21,  0,   0,   1,   2,   0,   0,   65,  130, 0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   2,   1,   2,   0,
            0,   0,   0,   0,   1,   2,   0,   0,   0,   0,   0,   0,   1,   2,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x5F0
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   1,   2,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   1,   2,   0,   0,
            1,   2,   1,   2,   1,   2,   1,   2,   0,   0,   0,   0,   0,   0,   1,
            2,   0,   0,   0,   0,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,
            1,   2,   0,   0,   0,   0,   1,   2,   0,   0,   0,   1,   2,   1,   2,
            1,   2,   1,   2,   0,   1,   2,   0,   0,   1,   2,   0,   0,   0,   0,
            0,   0,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,   0,
            0,   0,   0,   0,   0,   1,   2,   1,   2,   1,   2,   1,   2,   1,   2,
            0,   0,   0,   0,   0,   0,   0,   22,  0,   0,   0,   0,   23,  24,  23,
            0,   0,
        // Index : 0x688
            0,   0,   0,   0,   1,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            1,   2,   1,   2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x720
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   1,   2,   1,   2,   0,   0,   0,   1,   2,   0,   1,
            2,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            1,   2,   0,   0,   1,   2,   65,  130, 65,  130, 65,  130, 65,  130, 0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x7B8
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   65,  130, 65,  130, 65,  130, 65,  130, 65,
            130, 0,   0,   65,  130, 65,  130, 65,  130, 65,  130, 0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x850
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   65,  130, 65,  130, 65,  130, 0,   0,   0,   0,   0,   1,   2,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,
        // Index : 0x8E8
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   65,  130, 0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            3,   0,   4,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   67,  0,   132, 0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
            0,   0,   0,   67,  0,   132, 0,   65,  130, 0,   65,  130
        };

        private static readonly short[] PairIndexes = new short[] {
            0x000, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x130, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x1C8, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x260,
            0x098, 0x098, 0x2F8, 0x390, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x428, 0x098, 0x098, 0x4C0, 0x558, 0x5F0, 0x688, 0x098, 0x098, 0x098, 0x098,
            0x720, 0x098, 0x098, 0x7B8, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098,
            0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x098, 0x850,
            0x8E8
        };
    }
}
