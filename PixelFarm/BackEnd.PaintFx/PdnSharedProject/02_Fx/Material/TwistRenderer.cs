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
    public class TwistRenderer : EffectRendererBase
    {
        const double inv100 = 1.0 / 100.0;

        double amount;
        double size;
        int quality;
        double offsetX;
        double offsetY;
        public void SetParameters(double amount, double size, int quality, double offsetX, double offsetY)
        {
            this.amount = amount;
            this.size = size;
            this.quality = quality;
            this.offsetX = offsetX;
            this.offsetY = offsetY;

        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            unsafe
            {
                double twist = this.amount * this.amount * Math.Sign(this.amount);

                float hw = dst.Width / 2.0f;
                hw += (float)(hw * this.offsetX);
                float hh = dst.Height / 2.0f;
                hh += (float)(hh * this.offsetY);

                //*double maxrad = Math.Min(dst.Width / 2.0, dst.Height / 2.0);
                double invmaxrad = 1.0 / Math.Min(dst.Width / 2.0, dst.Height / 2.0);

                int aaLevel = this.quality;
                int aaSamples = aaLevel * aaLevel;
                PointF* aaPoints = stackalloc PointF[aaSamples];
                PixelUtils.GetRgssOffsets(aaPoints, aaSamples, aaLevel);

                ColorBgra* samples = stackalloc ColorBgra[aaSamples];

                //TODO: review here
                for (int n = start; n < start + len; ++n)
                {
                    Rectangle rect = rois[n];

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        float j = y - hh;
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            float i = x - hw;

                            int sampleCount = 0;

                            for (int p = 0; p < aaSamples; ++p)
                            {
                                float u = i + aaPoints[p].X;
                                float v = j + aaPoints[p].Y;

                                double rad = Math.Sqrt(u * u + v * v);
                                double theta = Math.Atan2(v, u);

                                double t = 1 - ((rad * this.size) * invmaxrad);

                                t = (t < 0) ? 0 : (t * t * t);

                                theta += (t * twist) * inv100;

                                float sampleX = (hw + (float)(rad * Math.Cos(theta)));
                                float sampleY = (hh + (float)(rad * Math.Sin(theta)));

                                samples[sampleCount] = src.GetBilinearSampleClamped(sampleX, sampleY);
                                ++sampleCount;
                            }

                            *dstPtr = ColorBgra.Blend(samples, sampleCount);


                            ++dstPtr;
                            ++srcPtr;
                        }
                    }
                }
            }
        }
    }
}