//MIT, 2016-present, WinterDev
using System;
using System.IO;

using PixelFarm.CpuBlit;
using Typography.OpenFont.WebFont;
using BrotliSharpLib;

namespace SampleWinForms
{
    static class OurOpenFontSystemSetup
    {
        public static void Setup()
        {
            //1. text breaker
            Typography.TextBreak.CustomBreakerBuilder.Setup(
                new Typography.TextBreak.IcuSimpleTextFileDictionaryProvider()
                {
                    DataDir = "../../../../../Typography.TextBreak/icu62/brkitr"
                });

            //2. woff and woff2 decompressor
            SetupWoffDecompressFunctions();

            //3. read/write image file with gdi+
            MemBitmapExt.DefaultMemBitmapIO = new PixelFarm.Drawing.WinGdi.GdiBitmapIO();

        }
        static void SetupWoffDecompressFunctions()
        {
            //
            //Woff
            WoffDefaultZlibDecompressFunc.DecompressHandler = (byte[] compressedBytes, byte[] decompressedResult) =>
            {
                //ZLIB
                //****
                //YOU can change to  your prefer decode libs***
                //****

                bool result = false;
                try
                {
                    var inflater = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
                    inflater.SetInput(compressedBytes);
                    inflater.Inflate(decompressedResult);
#if DEBUG
                    long outputLen = inflater.TotalOut;
                    if (outputLen != decompressedResult.Length)
                    {

                    }
#endif

                    result = true;
                }
                catch (Exception ex)
                {

                }
                return result;
            };
            //Woff2

            Woff2DefaultBrotliDecompressFunc.DecompressHandler = (byte[] compressedBytes, Stream output) =>
            {
                //BROTLI
                //****
                //YOU can change to  your prefer decode libs***
                //****

                bool result = false;
                try
                {
                    using (MemoryStream ms = new MemoryStream(compressedBytes))
                    {

                        ms.Position = 0;//set to start pos
                        DecompressAndCalculateCrc1(ms, output);
                        //
                        //  

                        //Decompress(ms, output);
                    }
                    //DecompressBrotli(compressedBytes, output);
                    result = true;
                }
                catch (Exception ex)
                {

                }
                return result;
            };
        }

        /// <summary>
        /// ECMA CRC64 polynomial.
        /// </summary>
        static readonly long CRC_64_POLY = Convert.ToInt64("0xC96C5795D7870F42", 16);
        static long UpdateCrc64(long crc, byte[] data, int offset, int length)
        {
            for (int i = offset; i < offset + length; ++i)
            {
                long c = (crc ^ (long)(data[i] & 0xFF)) & 0xFF;
                for (int k = 0; k < 8; k++)
                {
                    c = ((c & 1) == 1) ? CRC_64_POLY ^ (long)((ulong)c >> 1) : (long)((ulong)c >> 1);
                }
                crc = c ^ (long)((ulong)crc >> 8);
            }
            return crc;
        }
        static long DecompressAndCalculateCrc1(Stream input, Stream output)
        {
            try
            {
                long crc = -1;
                byte[] buffer = new byte[65536];
                CSharpBrotli.Decode.BrotliInputStream decompressedStream = new CSharpBrotli.Decode.BrotliInputStream(input);
                using (MemoryStream ms = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    while (true)
                    {
                        int len = decompressedStream.Read(buffer);
                        if (len <= 0)
                        {
                            break;
                        }
                        else
                        {

                        }

                        writer.Write(buffer, 0, len);

                        crc = UpdateCrc64(crc, buffer, 0, len);
                    }

                    decompressedStream.Close();
                    writer.Flush();

                    byte[] outputBuffer = ms.ToArray();

                    output.Write(outputBuffer, 0, outputBuffer.Length);

                    writer.Close();
                }
                return crc ^ -1;
            }
            catch (IOException ex)
            {
                throw ex;
            }
        }
        static void Decompress(Stream input, Stream output)
        {
            /// <exception cref="System.IO.IOException"/>

            byte[] buffer = new byte[65536];
            bool byByte = false;

            Org.Brotli.Dec.BrotliInputStream brotliInput = new Org.Brotli.Dec.BrotliInputStream(input);
            if (byByte)
            {
                byte[] oneByte = new byte[1];
                while (true)
                {
                    int next = brotliInput.ReadByte();
                    if (next == -1)
                    {
                        break;
                    }
                    oneByte[0] = unchecked((byte)next);
                    output.Write(oneByte, 0, 1);
                }
            }
            else
            {
                while (true)
                {
                    int len = brotliInput.Read(buffer, 0, buffer.Length);
                    if (len <= 0)
                    {
                        break;
                    }
                    output.Write(buffer, 0, len);
                }
            }
            brotliInput.Close();
        }
        static void DecompressBrotli(byte[] compressed, Stream output)
        {
            var decompressed = Brotli.DecompressBuffer(compressed, 0, compressed.Length);
        }

    }
}