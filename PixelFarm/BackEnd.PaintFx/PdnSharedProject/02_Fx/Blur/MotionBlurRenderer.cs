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
        private double angle;
        private int distance;
        private bool centered;
        private PointF[] points;

        public double Angle
        {
            get { return angle; }
        }
        public int Distance
        {
            get { return distance; }
        }
        public bool Centered
        {
            get { return centered; }
        }
        public void SetParameters(double angle, int distance, bool centered)
        {
            this.angle = angle;
            this.distance = distance;
            this.centered = centered;


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

            this.points = new PointF[((1 + distance) * 3) / 2];

            if (this.points.Length == 1)
            {
                this.points[0] = new PointF(0, 0);
            }
            else
            {
                for (int i = 0; i < this.points.Length; ++i)
                {
                    float frac = (float)i / (float)(this.points.Length - 1);
                    this.points[i] = PixelUtils.Lerp(
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
                ColorBgra* samples = stackalloc ColorBgra[this.points.Length];

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    Rectangle rect = rois[i];

                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            int sampleCount = 0;

                            PointF a = new PointF((float)x + points[0].X, (float)y + points[0].Y);
                            PointF b = new PointF((float)x + points[points.Length - 1].X, (float)y + points[points.Length - 1].Y);

                            for (int j = 0; j < this.points.Length; ++j)
                            {
                                PointF pt = new PointF(this.points[j].X + (float)x, this.points[j].Y + (float)y);

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