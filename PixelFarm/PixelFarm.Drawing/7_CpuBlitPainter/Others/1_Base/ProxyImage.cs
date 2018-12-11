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

using PixelFarm.Drawing;
using PixelFarm.CpuBlit.PixelProcessing;
namespace PixelFarm.CpuBlit.Imaging
{
    public abstract class ProxyImage : IBitmapBlender
    {
        protected IBitmapBlender _linkedImage;
        public ProxyImage(IBitmapBlender linkedImage)
        {
            _linkedImage = linkedImage;
        }
        public void WriteBuffer(int[] newbuffer)
        {
            throw new System.NotSupportedException();
        }
        //
        public virtual int Width => _linkedImage.Width;
        public virtual int Height => _linkedImage.Height;
        public virtual int Stride => _linkedImage.Stride;
        // 
        public virtual RectInt GetBounds()
        {
            return _linkedImage.GetBounds();
        }
        public PixelProcessing.PixelBlender32 OutputPixelBlender
        {
            get => _linkedImage.OutputPixelBlender;
            set => _linkedImage.OutputPixelBlender = value;
        }

        public virtual Color GetPixel(int x, int y) => _linkedImage.GetPixel(x, y);

        public virtual void CopyFrom(IBitmapSrc sourceImage, RectInt sourceImageRect, int destXOffset, int destYOffset)
        {
            _linkedImage.CopyFrom(sourceImage, sourceImageRect, destXOffset, destYOffset);
        }

        public virtual void SetPixel(int x, int y, Color color)
        {
            _linkedImage.SetPixel(x, y, color);
        }


        public virtual void CopyHL(int x, int y, int len, Color sourceColor)
        {
            _linkedImage.CopyHL(x, y, len, sourceColor);
        }

        public virtual void CopyVL(int x, int y, int len, Color sourceColor)
        {
            _linkedImage.CopyVL(x, y, len, sourceColor);
        }

        public virtual void BlendHL(int x1, int y, int x2, Color sourceColor, byte cover)
        {
            _linkedImage.BlendHL(x1, y, x2, sourceColor, cover);
        }

        public virtual void BlendVL(int x, int y1, int y2, Color sourceColor, byte cover)
        {
            _linkedImage.BlendVL(x, y1, y2, sourceColor, cover);
        }

        public virtual void BlendSolidHSpan(int x, int y, int len, Color c, byte[] covers, int coversIndex)
        {
            _linkedImage.BlendSolidHSpan(x, y, len, c, covers, coversIndex);
        }

        public virtual void CopyColorHSpan(int x, int y, int len, Color[] colors, int colorIndex)
        {
            _linkedImage.CopyColorHSpan(x, y, len, colors, colorIndex);
        }

        public virtual void CopyColorVSpan(int x, int y, int len, Color[] colors, int colorIndex)
        {
            _linkedImage.CopyColorVSpan(x, y, len, colors, colorIndex);
        }

        public virtual void BlendSolidVSpan(int x, int y, int len, Color c, byte[] covers, int coversIndex)
        {
            _linkedImage.BlendSolidVSpan(x, y, len, c, covers, coversIndex);
        }

        public virtual void BlendColorHSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            _linkedImage.BlendColorHSpan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
        }

        public virtual void BlendColorVSpan(int x, int y, int len, Color[] colors, int colorsIndex, byte[] covers, int coversIndex, bool firstCoverForAll)
        {
            _linkedImage.BlendColorVSpan(x, y, len, colors, colorsIndex, covers, coversIndex, firstCoverForAll);
        }


        public TempMemPtr GetBufferPtr() => _linkedImage.GetBufferPtr();

        public void ReplaceBuffer()
        {
            throw new System.NotSupportedException();
        }

        public int GetBufferOffsetXY32(int x, int y) => _linkedImage.GetBufferOffsetXY32(x, y);

        public virtual int BytesBetweenPixelsInclusive => _linkedImage.BytesBetweenPixelsInclusive;

        public virtual int BitDepth => _linkedImage.BitDepth;

    }
}
