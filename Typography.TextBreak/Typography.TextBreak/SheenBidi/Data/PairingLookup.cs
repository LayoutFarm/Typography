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
    partial class PairingLookup
    {
        private const byte PrimaryMask = (byte)(BracketType.Open | BracketType.Close);
        private const byte InverseMask = unchecked((byte)(~PrimaryMask));

        public static int DetermineMirror(int unicode)
        {
            if (unicode >= 40 && unicode <= 0xFF63)
            {
                int trim = unicode - 40;
                short value = PairDifferences[
                               PairData[
                                PairIndexes[
                                     trim / 152
                                ] + (trim % 152)
                               ] & InverseMask
                              ];

                if (value != 0)
                    return (unicode + value);
            }

            return 0;
        }

        public static int DetermineBracketPair(int unicode, out BracketType type)
        {
            if (unicode >= 40 && unicode <= 0xFF63)
            {
                int trim = (unicode - 40);
                byte value = PairData[
                              PairIndexes[
                                   trim / 152
                              ] + (trim % 152)
                             ];
                type = (BracketType)(value & PrimaryMask);

                if (type != BracketType.None)
                    return (unicode + PairDifferences[value & InverseMask]);
            }

            type = BracketType.None;
            return 0;
        }
    }
}
