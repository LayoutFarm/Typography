//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of interchange extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author: unknown $
//   Changed on:        $Date: 2015-03-17 16:18:14 +0100 (Di, 17 Mrz 2015) $
//   Changed in:        $Revision: 113386 $
//   Project:           $URL: https://writeablebitmapex.svn.codeplex.com/svn/trunk/Source/WriteableBitmapEx/WriteableBitmapConvertExtensions.cs $
//   Id:                $Id: WriteableBitmapConvertExtensions.cs 113386 2015-03-17 15:18:14Z unknown $
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
// 


using System.IO;

namespace BitmapBufferEx
{
    /// <summary>
    /// Collection of interchange extension methods for the WriteableBitmap class.
    /// </summary>
    public static partial class BitmapBufferExtensions
    {


        /// <summary>
        /// Copies the Pixels from the WriteableBitmap into a ARGB byte array starting at a specific Pixels index.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="offset">The starting Pixels index.</param>
        /// <param name="count">The number of Pixels to copy, -1 for all</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this BitmapBuffer bmp, int offset, int count)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                if (count == -1)
                {
                    // Copy all to byte array
                    count = context.Length;
                }

                int len = count * ARGB_SIZE;
                byte[] result = new byte[len]; // ARGB
                BitmapContext.BlockCopy(context, offset, result, 0, len);
                return result;
            }
        }

        /// <summary>
        /// Copies the Pixels from the WriteableBitmap into a ARGB byte array.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="count">The number of pixels to copy.</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this BitmapBuffer bmp, int count)
        {
            return bmp.ToByteArray(0, count);
        }

        /// <summary>
        /// Copies all the Pixels from the WriteableBitmap into a ARGB byte array.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>The color buffer as byte ARGB values.</returns>
        public static byte[] ToByteArray(this BitmapBuffer bmp)
        {
            return bmp.ToByteArray(0, -1);
        }

        /// <summary>
        /// Copies color information from an ARGB byte array into this WriteableBitmap starting at a specific buffer index.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="offset">The starting index in the buffer.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static BitmapBuffer FromByteArray(this BitmapBuffer bmp, byte[] buffer, int offset, int count)
        {
            using (BitmapContext context = bmp.GetBitmapContext())
            {
                BitmapContext.BlockCopy(buffer, offset, context, 0, count);
                return bmp;
            }
        }

        /// <summary>
        /// Copies color information from an ARGB byte array into this WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static BitmapBuffer FromByteArray(this BitmapBuffer bmp, byte[] buffer, int count)
        {
            return bmp.FromByteArray(buffer, 0, count);
        }

        /// <summary>
        /// Copies all the color information from an ARGB byte array into this WriteableBitmap.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="buffer">The color buffer as byte ARGB values.</param>
        /// <returns>The WriteableBitmap that was passed as parameter.</returns>
        public static BitmapBuffer FromByteArray(this BitmapBuffer bmp, byte[] buffer)
        {
            return bmp.FromByteArray(buffer, 0, buffer.Length);
        }



        /// <summary>
        /// Writes the WriteableBitmap as a TGA image to a stream. 
        /// Used with permission from Nokola: http://nokola.com/blog/post/2010/01/21/Quick-and-Dirty-Output-of-WriteableBitmap-as-TGA-Image.aspx
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="destination">The destination stream.</param>
        public static void WriteTga(this BitmapBuffer bmp, Stream destination)
        {
            using (BitmapContext context = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                int width = context.Width;
                int height = context.Height;
                int[] pixels = context.Pixels;
                byte[] data = new byte[context.Length * ARGB_SIZE];

                // Copy bitmap data as BGRA
                int offsetSource = 0;
                int width4 = width << 2;
                int width8 = width << 3;
                int offsetDest = (height - 1) * width4;
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Account for pre-multiplied alpha
                        int c = pixels[offsetSource];
                        byte a = (byte)(c >> 24);

                        // Prevent division by zero
                        int ai = a;
                        if (ai == 0)
                        {
                            ai = 1;
                        }

                        // Scale inverse alpha to use cheap integer mul bit shift
                        ai = ((255 << 8) / ai);
                        data[offsetDest + 3] = (byte)a;                                // A
                        data[offsetDest + 2] = (byte)((((c >> 16) & 0xFF) * ai) >> 8); // R
                        data[offsetDest + 1] = (byte)((((c >> 8) & 0xFF) * ai) >> 8);  // G
                        data[offsetDest] = (byte)((((c & 0xFF) * ai) >> 8));           // B

                        offsetSource++;
                        offsetDest += ARGB_SIZE;
                    }
                    offsetDest -= width8;
                }

                // Create header
                var header = new byte[]
                 {
                    0, // ID length
                    0, // no color map
                    2, // uncompressed, true color
                    0, 0, 0, 0,
                    0,
                    0, 0, 0, 0, // x and y origin
                    (byte)(width & 0x00FF),
                    (byte)((width & 0xFF00) >> 8),
                    (byte)(height & 0x00FF),
                    (byte)((height & 0xFF00) >> 8),
                    32, // 32 bit bitmap
                    0
                 };

                // Write header and data
                using (var writer = new BinaryWriter(destination))
                {
                    writer.Write(header);
                    writer.Write(data);
                }
            }
        }
    }
}