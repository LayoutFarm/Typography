// CRC32.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// Last Saved: <2011-August-02 18:25:54>
//
// ------------------------------------------------------------------
//
// This module defines the CRC32 class, which can do the CRC32 algorithm, using
// arbitrary starting polynomials, and bit reversal. The bit reversal is what
// distinguishes this CRC-32 used in BZip2 from the CRC-32 that is used in PKZIP
// files, or GZIP files. This class does both.
//
// ------------------------------------------------------------------

//from Ionic.Crc

using System;
namespace Typography.TextServices
{
    /// <summary>
    ///   Computes a CRC-32. The CRC-32 algorithm is parameterized - you
    ///   can set the polynomial and enable or disable bit
    ///   reversal. This can be used for GZIP, BZip2, or ZIP.
    /// </summary>
    /// <remarks>
    ///   This type is used internally by DotNetZip; it is generally not used
    ///   directly by applications wishing to create, read, or manipulate zip
    ///   archive files.
    /// </remarks>



    static class CRC32
    {

        /// <summary>
        /// calculate crc32, not reverver bits
        /// </summary>
        /// <param name="charBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int CalculateCRC32(char[] charBuffer, int startAt, int len)
        {
            //calculate CRC32 
            uint register = SlurpBlock2_(charBuffer, startAt, len);
            return (Int32)(~register);
        }
        /// <summary>
        /// calculate with reverse bit
        /// </summary>
        /// <param name="charBuffer"></param>
        /// <param name="startAt"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static int CalculateCRC32_ReverseBit(char[] charBuffer, int startAt, int len)
        {
            //calculate CRC32
            uint register = SlurpBlock2_ReverseBits(charBuffer, startAt, len);
            return (Int32)(~register);
        }
        static uint SlurpBlock2_(char[] block, int offset, int count)
        {
            uint _register = RESET_REGISTER;
            // bzip algorithm
            for (int i = 0; i < count; i++)
            {

                char ch = block[offset + i];
                byte b0 = (byte)(ch >> 8);
                byte b1 = (byte)ch;
                //b0
                UInt32 temp = (_register & 0x000000FF) ^ b0;
                _register = (_register >> 8) ^ s_crc32[temp];
                //b1
                temp = (_register & 0x000000FF) ^ b1;
                _register = (_register >> 8) ^ s_crc32[temp];

            }
            return _register;
        }
        static uint SlurpBlock2_ReverseBits(char[] block, int offset, int count)
        {
            uint _register = RESET_REGISTER;
            // bzip algorithm
            for (int i = 0; i < count; i++)
            {

                char ch = block[offset + i];
                byte b0 = (byte)(ch >> 8);
                byte b1 = (byte)ch;
                //b0
                //b1
                UInt32 temp = (_register >> 24) ^ b0;
                _register = (_register << 8) ^ s_crc32_reverse_bits[temp];
                //
                temp = (_register >> 24) ^ b1;
                _register = (_register << 8) ^ s_crc32_reverse_bits[temp];

            }
            return _register;
        }
        static uint ReverseBits(uint data)
        {
            unchecked
            {
                uint ret = data;
                ret = (ret & 0x55555555) << 1 | (ret >> 1) & 0x55555555;
                ret = (ret & 0x33333333) << 2 | (ret >> 2) & 0x33333333;
                ret = (ret & 0x0F0F0F0F) << 4 | (ret >> 4) & 0x0F0F0F0F;
                ret = (ret << 24) | ((ret & 0xFF00) << 8) | ((ret >> 8) & 0xFF00) | (ret >> 24);
                return ret;
            }
        }

        static byte ReverseBits(byte data)
        {
            unchecked
            {
                uint u = (uint)data * 0x00020202;
                uint m = 0x01044010;
                uint s = u & m;
                uint t = (u << 2) & (m << 1);
                return (byte)((0x01001001 * (s + t)) >> 24);
            }
        }



        static readonly UInt32[] s_crc32;
        static readonly UInt32[] s_crc32_reverse_bits;
        static CRC32()
        {
            /// <summary>
            ///   Create an instance of the CRC32 class, specifying the polynomial and
            ///   whether to reverse data bits or not.
            /// </summary>
            /// <param name='polynomial'>
            ///   The polynomial to use for the CRC, expressed in the reversed (LSB)
            ///   format: the highest ordered bit in the polynomial value is the
            ///   coefficient of the 0th power; the second-highest order bit is the
            ///   coefficient of the 1 power, and so on. Expressed this way, the
            ///   polynomial for the CRC-32C used in IEEE 802.3, is 0xEDB88320.
            /// </param>
            /// <param name='reverseBits'>
            ///   specify true if the instance should reverse data bits.
            /// </param>
            ///
            /// <remarks>
            ///   <para>
            ///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
            ///     want a CRC32 with compatibility with BZip2, you should pass true
            ///     here for the <c>reverseBits</c> parameter. In the CRC-32 used by
            ///     GZIP and PKZIP, the bits are not reversed; Therefore if you want a
            ///     CRC32 with compatibility with those, you should pass false for the
            ///     <c>reverseBits</c> parameter.
            ///   </para>
            /// </remarks>
            /// 
            //----------
            /// <summary>
            ///   Create an instance of the CRC32 class, specifying whether to reverse
            ///   data bits or not.
            /// </summary>
            /// <param name='reverseBits'>
            ///   specify true if the instance should reverse data bits.
            /// </param>
            /// <remarks>
            ///   <para>
            ///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
            ///     want a CRC32 with compatibility with BZip2, you should pass true
            ///     here. In the CRC-32 used by GZIP and PKZIP, the bits are not
            ///     reversed; Therefore if you want a CRC32 with compatibility with
            ///     those, you should pass false.
            ///   </para>
            /// </remarks>
            /// 

            s_crc32 = new uint[256];
            GenerateLookupTable(unchecked(0xEDB88320), s_crc32, false);

            //
            s_crc32_reverse_bits = new uint[256];
            GenerateLookupTable(unchecked(0xEDB88320), s_crc32_reverse_bits, true);
        }

        static void GenerateLookupTable(uint dwPolynomial, UInt32[] crc32Table, bool reverseBits)
        {
            unchecked
            {
                UInt32 dwCrc;
                byte i = 0;
                do
                {
                    dwCrc = i;
                    for (byte j = 8; j > 0; j--)
                    {
                        if ((dwCrc & 1) == 1)
                        {
                            dwCrc = (dwCrc >> 1) ^ dwPolynomial;
                        }
                        else
                        {
                            dwCrc >>= 1;
                        }
                    }
                    if (reverseBits)
                    {
                        crc32Table[ReverseBits(i)] = ReverseBits(dwCrc);
                    }
                    else
                    {
                        crc32Table[i] = dwCrc;
                    }
                    i++;
                } while (i != 0);
            }
        }

        const uint RESET_REGISTER = 0xFFFFFFFFU;

    }



}