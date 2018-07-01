/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev

using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class PosterizeRenderer : EffectRendererBase
    {
        private class PosterizePixelOp
        : UnaryPixelOp
        {
            private byte[] redLevels;
            private byte[] greenLevels;
            private byte[] blueLevels;

            public PosterizePixelOp(int red, int green, int blue)
            {
                this.redLevels = CalcLevels(red);
                this.greenLevels = CalcLevels(green);
                this.blueLevels = CalcLevels(blue);
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
                return ColorBgra.FromBgra(blueLevels[color.B], greenLevels[color.G], redLevels[color.R], color.A);
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    ptr->B = this.blueLevels[ptr->B];
                    ptr->G = this.greenLevels[ptr->G];
                    ptr->R = this.redLevels[ptr->R];

                    ++ptr;
                    --length;
                }
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    dst->B = this.blueLevels[src->B];
                    dst->G = this.greenLevels[src->G];
                    dst->R = this.redLevels[src->R];
                    dst->A = src->A;

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }

        private UnaryPixelOp op;

        public void SetParameters(int red, int green, int blue)
        {
            this.op = new PosterizePixelOp(red, green, blue);
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            this.op.Apply(dst, src, rois, start, len);
        }
    }
}