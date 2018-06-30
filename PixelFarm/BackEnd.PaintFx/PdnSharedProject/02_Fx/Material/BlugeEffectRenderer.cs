/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev
using System;
using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public class BulgeEffectRenderer : EffectRendererBase
    {
        private int amount;
        private float offsetX;
        private float offsetY;
        public void SetParameters(int amount, float offsetX, float offsetY)
        {
            this.amount = amount;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {

            float bulge = this.amount;

            float hw = dst.Width / 2.0f;
            float hh = dst.Height / 2.0f;
            float maxrad = Math.Min(hw, hh);
            float maxrad2 = maxrad * maxrad;
            float amt = this.amount / 100.0f;

            hh = hh + this.offsetY * hh;
            hw = hw + this.offsetX * hw;
            unsafe
            {
                for (int n = startIndex; n < startIndex + length; ++n)
                {
                    Rectangle rect = rois[n];

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);
                        float v = y - hh;

                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            float u = x - hw;
                            float r = (float)Math.Sqrt(u * u + v * v);
                            float rscale1 = (1.0f - (r / maxrad));

                            if (rscale1 > 0)
                            {
                                float rscale2 = 1 - amt * rscale1 * rscale1;

                                float xp = u * rscale2;
                                float yp = v * rscale2;

                                *dstPtr = src.GetBilinearSampleClamped(xp + hw, yp + hh);
                            }
                            else
                            {
                                *dstPtr = *srcPtr;
                            }

                            ++dstPtr;
                            ++srcPtr;
                        }
                    }
                }
            }
        }
    }
}