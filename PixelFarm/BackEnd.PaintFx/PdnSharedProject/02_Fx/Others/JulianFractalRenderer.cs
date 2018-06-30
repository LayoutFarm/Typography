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
    public class JulianFractalRenderer : EffectRendererBase
    {
        double factor;
        double zoom;
        double angle;
        double angleTheta;
        int quality;

        static readonly double log2_10000 = Math.Log(10000);
        const double jr = 0.3125;
        const double ji = 0.03;

        public void SetParameters(double zoom, int quality, double angle, double factor)
        {
            this.zoom = zoom;
            this.quality = quality;
            this.angle = angle;
            this.angleTheta = (this.angle * Math.PI * 2) / 360.0;
            this.factor = factor;
        }
        public override void Render(Surface src, Surface dest, Rectangle[] rois, int startIndex, int length)
        {
            unsafe
            {
                int w = dest.Width;
                int h = dest.Height;
                double invH = 1.0 / h;
                double invZoom = 1.0 / this.zoom;
                double invQuality = 1.0 / this.quality;
                double aspect = (double)h / (double)w;
                int count = this.quality * this.quality + 1;
                double invCount = 1.0 / (double)count;

                for (int ri = startIndex; ri < startIndex + length; ++ri)
                {
                    Rectangle rect = rois[ri];

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        ColorBgra* dstPtr = dest.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            int r = 0;
                            int g = 0;
                            int b = 0;
                            int a = 0;

                            for (double i = 0; i < count; i++)
                            {
                                double u = (2.0 * x - w + (i * invCount)) * invH;
                                double v = (2.0 * y - h + ((i * invQuality) % 1)) * invH;

                                double radius = Math.Sqrt((u * u) + (v * v));
                                double radiusP = radius;
                                double theta = Math.Atan2(v, u);
                                double thetaP = theta + this.angleTheta;

                                double uP = radiusP * Math.Cos(thetaP);
                                double vP = radiusP * Math.Sin(thetaP);

                                double jX = (uP - vP * aspect) * invZoom;
                                double jY = (vP + uP * aspect) * invZoom;

                                double j = Julia(jX, jY, jr, ji);

                                double c = this.factor * j;


                                b += PixelUtils.ClampToByte(c - 768);
                                g += PixelUtils.ClampToByte(c - 512);
                                r += PixelUtils.ClampToByte(c - 256);
                                a += PixelUtils.ClampToByte(c - 0);
                            }

                            *dstPtr = ColorBgra.FromBgra(
                                PixelUtils.ClampToByte(b / count),
                                PixelUtils.ClampToByte(g / count),
                                PixelUtils.ClampToByte(r / count),
                                PixelUtils.ClampToByte(a / count));

                            ++dstPtr;
                        }
                    }
                }
            }
        }
        static double Julia(double x, double y, double r, double i)
        {
            double c = 0;

            while (c < 256 && x * x + y * y < 10000)
            {
                double t = x;
                x = x * x - y * y + r;
                y = 2 * t * y + i;
                ++c;
            }

            c -= 2 - 2 * log2_10000 / Math.Log(x * x + y * y);

            return c;
        }
    }
}