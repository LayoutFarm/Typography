//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------
//
// Adaptation for high precision colors has been sponsored by 
// Liberty Technology Systems, Inc., visit http://lib-sys.com
//
// Liberty Technology Systems, Inc. is the provider of
// PostScript and PDF technology for software developers.
// 
//----------------------------------------------------------------------------
#define USE_BLENDER

using PixelFarm.Drawing;
using CO = PixelFarm.Drawing.Internal.CO;
namespace PixelFarm.CpuBlit.PixelProcessing
{
    public abstract class PixelBlender32
    {

        public const int NUM_PIXEL_BITS = 32;
        internal const byte BASE_MASK = 255;

        /// <summary>
        /// blend single pixel
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        internal abstract void BlendPixel(int[] dstBuffer, int arrayOffset, Color srcColor);
        //
        /// <summary>
        /// blend multiple pixels
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        internal abstract void BlendPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor);

        internal abstract void BlendPixels(
            int[] dstBuffer, int arrayElemOffset,
            Color[] sourceColors, int sourceColorsOffset,
            byte[] covers, int coversIndex, bool firstCoverForAll, int count);
        /// <summary>
        /// blend multiple pixels
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayElemOffset"></param>
        /// <param name="sourceColors"></param>
        /// <param name="sourceColorsOffset"></param>
        /// <param name="covers"></param>
        /// <param name="coversIndex"></param>
        /// <param name="firstCoverForAll"></param>
        /// <param name="count"></param>
        internal abstract void BlendPixels(
           TempMemPtr dstBuffer, int arrayElemOffset,
           Color[] sourceColors, int sourceColorsOffset,
           byte[] covers, int coversIndex, bool firstCoverForAll, int count);

        /// <summary>
        /// copy multiple pixels
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        /// <param name="count"></param>
        internal abstract void CopyPixels(int[] dstBuffer, int arrayOffset, Color srcColor, int count);
        /// <summary>
        /// copy single pixel
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        internal abstract void CopyPixel(int[] dstBuffer, int arrayOffset, Color srcColor);

        /// <summary>
        /// copy multiple pixels
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        /// <param name="count"></param>
        internal abstract void CopyPixels(TempMemPtr dstBuffer, int arrayOffset, Color srcColor, int count);
        /// <summary>
        /// copy single pixel
        /// </summary>
        /// <param name="dstBuffer"></param>
        /// <param name="arrayOffset"></param>
        /// <param name="srcColor"></param>
        internal abstract void CopyPixel(TempMemPtr dstBuffer, int arrayOffset, Color srcColor);


        internal abstract unsafe void BlendPixel32(int* ptr, Color sc);
        //----------------
        internal unsafe Color PixelToColorRGBA(int* buffer, int bufferOffset32)
        {
            int value = buffer[bufferOffset32];
            return new Color(
               (byte)((value >> (CO.A_SHIFT)) & 0xff),
               (byte)((value >> (CO.R_SHIFT)) & 0xff),
               (byte)((value >> (CO.G_SHIFT)) & 0xff),
               (byte)((value >> (CO.B_SHIFT)) & 0xff));

        }
        internal Color PixelToColorRGBA(int[] buffer, int bufferOffset32)
        {
            //TODO: review here ...             
            //check if the buffer is pre-multiplied color?
            //if yes=> this is not correct, 
            //we must convert the pixel from pre-multiplied color 
            //to the 'straight alpha color'

            int value = buffer[bufferOffset32];
            return new Color(
               (byte)((value >> (CO.A_SHIFT)) & 0xff),
               (byte)((value >> (CO.R_SHIFT)) & 0xff),
               (byte)((value >> (CO.G_SHIFT)) & 0xff),
               (byte)((value >> (CO.B_SHIFT)) & 0xff));

            //        buffer[bufferOffset + CO.A],
            //        buffer[bufferOffset + CO.R],
            //        buffer[bufferOffset + CO.G],
            //        buffer[bufferOffset + CO.B]
            //        );
            //}

        }
    }

}

