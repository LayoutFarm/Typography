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
    public class TileRenderer : EffectRendererBase
    {
        private double rotation;
        private double squareSize;
        private double curvature;

        private int quality;
        private float sin;
        private float cos;
        private float scale;
        private float intensity;
        public void SetParameters(double rotation, double squareSize, double curvature, int quality)
        {
            this.rotation = rotation;
            this.squareSize = squareSize;
            this.curvature = curvature;

            this.sin = (float)Math.Sin(this.rotation * Math.PI / 180.0);
            this.cos = (float)Math.Cos(this.rotation * Math.PI / 180.0);
            this.scale = (float)(Math.PI / this.squareSize);
            this.intensity = (float)(this.curvature * this.curvature / 10.0 * Math.Sign(this.curvature));

            this.quality = quality;

            if (this.quality != 1)
            {
                ++this.quality;
            }
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            unsafe
            {
                int width = dst.Width;
                int height = dst.Height;
                float hw = width / 2.0f;
                float hh = height / 2.0f;

                int aaSampleCount = this.quality * this.quality;
                PointF* aaPointsArray = stackalloc PointF[aaSampleCount];
                PixelUtils.GetRgssOffsets(aaPointsArray, aaSampleCount, this.quality);
                ColorBgra* samples = stackalloc ColorBgra[aaSampleCount];

                for (int n = start; n < start + len; ++n)
                {
                    Rectangle rect = rois[n];

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        float j = y - hh;
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            float i = x - hw;

                            for (int p = 0; p < aaSampleCount; ++p)
                            {
                                PointF pt = aaPointsArray[p];

                                float u1 = i + pt.X;
                                float v1 = j - pt.Y;

                                float s1 = cos * u1 + sin * v1;
                                float t1 = -sin * u1 + cos * v1;

                                float s2 = s1 + this.intensity * (float)Math.Tan(s1 * this.scale);
                                float t2 = t1 + this.intensity * (float)Math.Tan(t1 * this.scale);

                                float u2 = cos * s2 - sin * t2;
                                float v2 = sin * s2 + cos * t2;

                                float xSample = hw + u2;
                                float ySample = hh + v2;

                                samples[p] = src.GetBilinearSampleWrapped(xSample, ySample);

                                /*
                                int xiSample = (int)xSample;
                                int yiSample = (int)ySample;

                                xiSample = (xiSample + width) % width;
                                if (xiSample < 0) // This makes it a little faster
                                {
                                    xiSample = (xiSample + width) % width;
                                }

                                yiSample = (yiSample + height) % height;
                                if (yiSample < 0) // This makes it a little faster
                                {
                                    yiSample = (yiSample + height) % height;
                                }

                                samples[p] = *src.GetPointAddressUnchecked(xiSample, yiSample);
                                */
                            }

                            *dstPtr = ColorBgra.Blend(samples, aaSampleCount);
                            ++dstPtr;
                        }
                    }
                }
            }
        }
    }
}