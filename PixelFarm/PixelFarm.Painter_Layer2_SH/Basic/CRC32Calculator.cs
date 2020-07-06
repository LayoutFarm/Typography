using System;
// Copyright (c) 2006-2009 Dino Chiesa and Microsoft Corporation.
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
// last saved (in emacs):
// Time-stamp: <2010-January-16 13:16:27>
//
// ------------------------------------------------------------------
//
// Implements the CRC algorithm, which is used in zip files.  The zip format calls for
// the zipfile to contain a CRC for the unencrypted byte stream of each file.
//
// It is based on example source code published at
//    http://www.vbaccelerator.com/home/net/code/libraries/CRC32/Crc32_zip_CRC32_CRC32_cs.asp
//
// This implementation adds a tweak of that code for use within zip creation.  While
// computing the CRC we also compress the byte stream, in the same read loop. This
// avoids the need to read through the uncompressed stream twice - once to compute CRC
// and another time to compress.
//
// ------------------------------------------------------------------
namespace PixelFarm.Drawing
{

    public readonly struct TinyCRC32Calculator
    {

        /// <summary>
        /// Update the value for the running CRC32 using the given block of bytes.
        /// This is useful when using the CRC32() class in a Stream.
        /// </summary>
        /// <param name="block">block of bytes to slurp</param>
        /// <param name="offset">starting point in the block</param>
        /// <param name="count">how many bytes within the block to slurp</param>
        static int SlurpBlock(byte[] block, int offset, int count)
        {
            if (block == null)
            {
                throw new NotSupportedException("The data buffer must not be null.");
            }

            // UInt32 tmpRunningCRC32Result = _RunningCrc32Result;

            uint _runningCrc32Result = 0xFFFFFFFF;
            for (int i = 0; i < count; i++)
            {
#if DEBUG
                int x = offset + i;
#endif
                //_runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[x]) ^ ((_runningCrc32Result) & 0x000000FF)];
                _runningCrc32Result = ((_runningCrc32Result) >> 8) ^ s_crc32Table[(block[offset + i]) ^ ((_runningCrc32Result) & 0x000000FF)];
                //tmpRunningCRC32Result = ((tmpRunningCRC32Result) >> 8) ^ crc32Table[(block[offset + i]) ^ ((tmpRunningCRC32Result) & 0x000000FF)];
            }
            return unchecked((Int32)(~_runningCrc32Result));
        }


        // pre-initialize the crc table for speed of lookup.
        static TinyCRC32Calculator()
        {
            unchecked
            {
                // PKZip specifies CRC32 with a polynomial of 0xEDB88320;
                // This is also the CRC-32 polynomial used bby Ethernet, FDDI,
                // bzip2, gzip, and others.
                // Often the polynomial is shown reversed as 0x04C11DB7.
                // For more details, see http://en.wikipedia.org/wiki/Cyclic_redundancy_check
                UInt32 dwPolynomial = 0xEDB88320;


                s_crc32Table = new UInt32[256];
                UInt32 dwCrc;
                for (uint i = 0; i < 256; i++)
                {
                    dwCrc = i;
                    for (uint j = 8; j > 0; j--)
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
                    s_crc32Table[i] = dwCrc;
                }
            }
        }


#if DEBUG
        //Int64 dbugTotalBytesRead;
#endif

        static readonly UInt32[] s_crc32Table;
        const int BUFFER_SIZE = 2048;

        [System.ThreadStatic]
        static byte[] s_buffer;

        public static int CalculateCrc32(string inputData)
        {
            if (s_buffer == null)
            {
                s_buffer = new byte[BUFFER_SIZE];
            }

            if (inputData.Length > 512)
            {
                byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(inputData);
                return SlurpBlock(utf8, 0, utf8.Length);
            }
            else
            {
                int write = System.Text.Encoding.UTF8.GetBytes(inputData, 0, inputData.Length, s_buffer, 0);
                if (write >= BUFFER_SIZE)
                {
                    throw new System.NotSupportedException("crc32:");
                }
                return SlurpBlock(s_buffer, 0, write);
            }
        }

        public static int CalculateCrc32(byte[] buffer) => SlurpBlock(buffer, 0, buffer.Length);
    }

}