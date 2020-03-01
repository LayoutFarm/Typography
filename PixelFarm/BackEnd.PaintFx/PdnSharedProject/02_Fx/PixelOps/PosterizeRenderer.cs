/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev

using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class PosterizeRenderer : EffectRendererBase
    {
        private class PosterizePixelOp : UnaryPixelOp
        {
            byte[] _redLevels;
            byte[] _greenLevels;
            byte[] _blueLevels;

            public PosterizePixelOp(int red, int green, int blue)
            {
                _redLevels = CalcLevels(red);
                _greenLevels = CalcLevels(green);
                _blueLevels = CalcLevels(blue);
            }

            private static byte[] CalcLevels(int levelCount)
            {
                byte[] t1 = new byte[levelCount];

                for (int i = 1; i < levelCount; i++)
                {
                    t1[i] = (byte)((255 * i) / (levelCount - 1));
                }

                byte[] levels = new byte[256];

                int j = 0;
                int k = 0;

                for (int i = 0; i < 256; i++)
                {
                    levels[i] = t1[j];

                    k += levelCount;

                    if (k > 255)
                    {
                        k -= 255;
                        j++;
                    }
                }

                return levels;
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra(_blueLevels[color.B], _greenLevels[color.G], _redLevels[color.R], color.A);
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    ptr->B = _blueLevels[ptr->B];
                    ptr->G = _greenLevels[ptr->G];
                    ptr->R = _redLevels[ptr->R];

                    ++ptr;
                    --length;
                }
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    dst->B = _blueLevels[src->B];
                    dst->G = _greenLevels[src->G];
                    dst->R = _redLevels[src->R];
                    dst->A = src->A;

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        UnaryPixelOp _op;

        public void SetParameters(int red, int green, int blue)
        {
            _op = new PosterizePixelOp(red, green, blue);
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            _op.Apply(dst, src, rois, start, len);
        }
    }
}