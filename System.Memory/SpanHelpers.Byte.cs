// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Runtime.CompilerServices;

using nuint = System.NUInt;

namespace System
{
    internal static partial class SpanHelpers // .Byte
    {

        public static int IndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            byte valueHead = value;
            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int index = 0;
            for (; ; )
            {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = IndexOf(ref Unsafe.Add(ref searchSpace, index), valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;
                index += (relativeIndex);

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, index + 1), ref valueTail, valueTailLength))
                    return index;  // The tail matched. Return a successful find.

                index++;
            }
            return -1;
        }

        public static int IndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            int index = -1;
            for (int i = 0; i < valueLength; i++)
            {
                var tempIndex = IndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if ((uint)tempIndex < (uint)index)
                {
                    index = tempIndex;
                    // Reduce space for search, cause we don't care if we find the search value after the index of a previously found value
                    searchSpaceLength = tempIndex;

                    if (index == 0)
                        break;
                }
            }
            return index;
        }

        public static int LastIndexOfAny(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            int index = -1;
            for (int i = 0; i < valueLength; i++)
            {
                var tempIndex = LastIndexOf(ref searchSpace, Unsafe.Add(ref value, i), searchSpaceLength);
                if (tempIndex > index)
                    index = tempIndex;
            }
            return index;
        }

        // Adapted from IndexOf(...)
        public static unsafe bool Contains(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(0)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(4)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(5)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(6)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(7)))
                {
                    goto Found;
                }

                index = index.Offset(8);
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(0)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)) ||
                    uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)))
                {
                    goto Found;
                }

                index = index.Offset(4);
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;

                index = index.Offset(1);
            }
            
            return false;

            Found:
            return true;
        }

        public static unsafe int IndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(4)))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(5)))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(6)))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(7)))
                    goto Found7;

                index = index.Offset(8);
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)))
                    goto Found3;

                index = index.Offset(4);
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;

                index = index.Offset(1);
            }
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        public static int LastIndexOf(ref byte searchSpace, int searchSpaceLength, ref byte value, int valueLength)
        {
            Debug.Assert(searchSpaceLength >= 0);
            Debug.Assert(valueLength >= 0);

            if (valueLength == 0)
                return 0;  // A zero-length sequence is always treated as "found" at the start of the search space.

            byte valueHead = value;
            ref byte valueTail = ref Unsafe.Add(ref value, 1);
            int valueTailLength = valueLength - 1;

            int index = 0;
            for (; ; )
            {
                Debug.Assert(0 <= index && index <= searchSpaceLength); // Ensures no deceptive underflows in the computation of "remainingSearchSpaceLength".
                int remainingSearchSpaceLength = searchSpaceLength - index - valueTailLength;
                if (remainingSearchSpaceLength <= 0)
                    break;  // The unsearched portion is now shorter than the sequence we're looking for. So it can't be there.

                // Do a quick search for the first element of "value".
                int relativeIndex = LastIndexOf(ref searchSpace, valueHead, remainingSearchSpaceLength);
                if (relativeIndex == -1)
                    break;

                // Found the first element of "value". See if the tail matches.
                if (SequenceEqual(ref Unsafe.Add(ref searchSpace, relativeIndex + 1), ref valueTail, valueTailLength))
                    return relativeIndex;  // The tail matched. Return a successful find.

                index += remainingSearchSpaceLength - relativeIndex;
            }
            return -1;
        }

        public static unsafe int LastIndexOf(ref byte searchSpace, byte value, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue = value; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);
                index = index.Offset(-8);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(7)))
                    goto Found7;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(6)))
                    goto Found6;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(5)))
                    goto Found5;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(4)))
                    goto Found4;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);
                index = index.Offset(-4);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(3)))
                    goto Found3;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(2)))
                    goto Found2;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index.Offset(1)))
                    goto Found1;
                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);
                index = index.Offset(-1);

                if (uValue == Unsafe.AddByteOffset(ref searchSpace, index))
                    goto Found;
            }
            
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        public static unsafe int IndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            uint lookUp;
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(4));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(5));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(6));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(7));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;

                index = index.Offset(8);
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;

                index = index.Offset(4);
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;

                index = index.Offset(1);
            }

            
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        public static unsafe int IndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue2 = value2; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            uint lookUp;
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(4));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(5));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(6));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(7));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found7;

                index = index.Offset(8);
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;

                index = index.Offset(4);
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;

                index = index.Offset(1);
            }
            
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        public static unsafe int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            uint lookUp;
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);
                index = index.Offset(-8);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(7));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(6));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(5));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(4));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);
                index = index.Offset(-4);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);
                index = index.Offset(-1);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp)
                    goto Found;
            }
            
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        public static unsafe int LastIndexOfAny(ref byte searchSpace, byte value0, byte value1, byte value2, int length)
        {
            Debug.Assert(length >= 0);

            uint uValue0 = value0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue1 = value1; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            uint uValue2 = value2; // Use uint for comparisons to avoid unnecessary 8->32 extensions
            IntPtr index = (IntPtr)length; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr nLength = (IntPtr)length;
            
            uint lookUp;
            while ((byte*)nLength >= (byte*)8)
            {
                nLength = nLength.Offset(-8);
                index = index.Offset(-8);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(7));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found7;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(6));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found6;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(5));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found5;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(4));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found4;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            if ((byte*)nLength >= (byte*)4)
            {
                nLength = nLength.Offset(-4);
                index = index.Offset(-4);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(3));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found3;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(2));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found2;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index.Offset(1));
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found1;
                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }

            while ((byte*)nLength > (byte*)0)
            {
                nLength = nLength.Offset(-1);
                index = index.Offset(-1);

                lookUp = Unsafe.AddByteOffset(ref searchSpace, index);
                if (uValue0 == lookUp || uValue1 == lookUp || uValue2 == lookUp)
                    goto Found;
            }
            
            return -1;
            Found: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return (int)(byte*)index;
            Found1:
            return (int)(byte*)(index.Offset(1));
            Found2:
            return (int)(byte*)(index.Offset(2));
            Found3:
            return (int)(byte*)(index.Offset(3));
            Found4:
            return (int)(byte*)(index.Offset(4));
            Found5:
            return (int)(byte*)(index.Offset(5));
            Found6:
            return (int)(byte*)(index.Offset(6));
            Found7:
            return (int)(byte*)(index.Offset(7));
        }

        // Optimized byte-based SequenceEquals. The "length" parameter for this one is declared a nuint rather than int as we also use it for types other than byte
        // where the length can exceed 2Gb once scaled by sizeof(T).
        public static unsafe bool SequenceEqual(ref byte first, ref byte second, nuint length)
        {
            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            IntPtr i = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr n = (IntPtr)(void*)length;

            if ((byte*)n >= (byte*)sizeof(UIntPtr))
            {
                n = n.Offset(-sizeof(UIntPtr));
                while ((byte*)n > (byte*)i)
                {
                    if (Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, i)) !=
                        Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, i)))
                    {
                        goto NotEqual;
                    }
                    i = i.Offset(sizeof(UIntPtr));
                }
                return Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, n)) ==
                       Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, n));
            }

            while ((byte*)n > (byte*)i)
            {
                if (Unsafe.AddByteOffset(ref first, i) != Unsafe.AddByteOffset(ref second, i))
                    goto NotEqual;
                i = i.Offset(1);
            }

            Equal:
            return true;

            NotEqual: // Workaround for https://github.com/dotnet/coreclr/issues/13549
            return false;
        }

        public static unsafe int SequenceCompareTo(ref byte first, int firstLength, ref byte second, int secondLength)
        {
            Debug.Assert(firstLength >= 0);
            Debug.Assert(secondLength >= 0);

            if (Unsafe.AreSame(ref first, ref second))
                goto Equal;

            IntPtr minLength = (IntPtr)((firstLength < secondLength) ? firstLength : secondLength);

            IntPtr i = (IntPtr)0; // Use IntPtr for arithmetic to avoid unnecessary 64->32->64 truncations
            IntPtr n = (IntPtr)(void*)minLength;
            
            if ((byte*)n > (byte*)sizeof(UIntPtr))
            {
                n = n.Offset(-sizeof(UIntPtr));
                while ((byte*)n > (byte*)i)
                {
                    if (Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref first, i)) !=
                        Unsafe.ReadUnaligned<UIntPtr>(ref Unsafe.AddByteOffset(ref second, i)))
                    {
                        goto NotEqual;
                    }
                    i = i.Offset(sizeof(UIntPtr));
                }
            }

            NotEqual:  // Workaround for https://github.com/dotnet/coreclr/issues/13549
            while ((byte*)minLength > (byte*)i)
            {
                int result = Unsafe.AddByteOffset(ref first, i).CompareTo(Unsafe.AddByteOffset(ref second, i));
                if (result != 0)
                    return result;
                i = i.Offset(1);
            }

            Equal:
            return firstLength - secondLength;
        }
    }
}