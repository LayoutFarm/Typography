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

    public class MotionBlurRenderer : EffectRendererBase
    {
        PointF[] _points;

        public double Angle { get; private set; }
        public int Distance { get; private set; }
        public bool Centered { get; private set; }
        public void SetParameters(double angle, int distance, bool centered)
        {
            this.Angle = angle;
            this.Distance = distance;
            this.Centered = centered;


            float start_x = 0, start_y = 0;

            double theta = ((double)(angle + 180) * 2 * Math.PI) / 360.0;
            double alpha = (double)distance;

            float end_x = (float)(alpha * Math.Cos(theta));
            float end_y = (float)(alpha * Math.Sin(theta));

            if (centered)
            {
                start_x = -end_x / 2.0f;
                start_y = -end_y / 2.0f;

                end_x /= 2.0f;
                end_y /= 2.0f;
            }

            _points = new PointF[((1 + distance) * 3) / 2];

            if (_points.Length == 1)
            {
                _points[0] = new PointF(0, 0);
            }
            else
            {
                for (int i = 0; i < _points.Length; ++i)
                {
                    float frac = (float)i / (float)(_points.Length - 1);
                    _points[i] = PixelUtils.Lerp(
                        new PointF(start_x, start_y),
                        new PointF(end_x, end_y),
                        frac);
                }
            }

        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            unsafe
            {
                ColorBgra* samples = stackalloc ColorBgra[_points.Length];

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    Rectangle rect = rois[i];

                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            int sampleCount = 0;

                            PointF a = new PointF((float)x + _points[0].X, (float)y + _points[0].Y);
                            PointF b = new PointF((float)x + _points[_points.Length - 1].X, (float)y + _points[_points.Length - 1].Y);

                            for (int j = 0; j < _points.Length; ++j)
                            {
                                PointF pt = new PointF(_points[j].X + (float)x, _points[j].Y + (float)y);

                                if (pt.X >= 0 && pt.Y >= 0 && pt.X <= (src.Width - 1) && pt.Y <= (src.Height - 1))
                                {
                                    samples[sampleCount] = src.GetBilinearSample(pt.X, pt.Y);
                                    ++sampleCount;
                                }
                            }

                            *dstPtr = ColorBgra.Blend(samples, sampleCount);
                            ++dstPtr;
                        }
                    }
                }
            }
        }
    }
}