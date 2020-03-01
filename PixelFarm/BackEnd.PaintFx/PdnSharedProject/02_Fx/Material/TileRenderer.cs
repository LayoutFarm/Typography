/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev
using System;

using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class TileRenderer : EffectRendererBase
    {
        double _rotation;
        double _squareSize;
        double _curvature;

        int _quality;
        float _sin;
        float _cos;
        float _scale;
        float _intensity;
        public void SetParameters(double rotation, double squareSize, double curvature, int quality)
        {
            _rotation = rotation;
            _squareSize = squareSize;
            _curvature = curvature;

            _sin = (float)Math.Sin(_rotation * Math.PI / 180.0);
            _cos = (float)Math.Cos(_rotation * Math.PI / 180.0);
            _scale = (float)(Math.PI / _squareSize);
            _intensity = (float)(_curvature * _curvature / 10.0 * Math.Sign(_curvature));

            _quality = quality;

            if (_quality != 1)
            {
                ++_quality;
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

                int aaSampleCount = _quality * _quality;
                PointF* aaPointsArray = stackalloc PointF[aaSampleCount];
                PixelUtils.GetRgssOffsets(aaPointsArray, aaSampleCount, _quality);
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

                                float s1 = _cos * u1 + _sin * v1;
                                float t1 = -_sin * u1 + _cos * v1;

                                float s2 = s1 + _intensity * (float)Math.Tan(s1 * _scale);
                                float t2 = t1 + _intensity * (float)Math.Tan(t1 * _scale);

                                float u2 = _cos * s2 - _sin * t2;
                                float v2 = _sin * s2 + _cos * t2;

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