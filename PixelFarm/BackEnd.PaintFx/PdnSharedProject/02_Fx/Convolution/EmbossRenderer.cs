/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
//                                                                            //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev
using System;

using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public class EmbossRenderer : EffectRendererBase
    {
        private double angle;
        private double[][] weights;
        public void SetParameters(double angle)
        {
            this.angle = angle;
            // adjust and convert angle to radians
            double r = (double)this.angle * 2.0 * Math.PI / 360.0;

            // angle delta for each weight
            double dr = Math.PI / 4.0;

            // for r = 0 this builds an emboss filter pointing straight left
            this.weights = new double[3][];

            for (int i = 0; i < 3; ++i)
            {
                this.weights[i] = new double[3];
            }

            this.weights[0][0] = Math.Cos(r + dr);
            this.weights[0][1] = Math.Cos(r + 2.0 * dr);
            this.weights[0][2] = Math.Cos(r + 3.0 * dr);

            this.weights[1][0] = Math.Cos(r);
            this.weights[1][1] = 0;
            this.weights[1][2] = Math.Cos(r + 4.0 * dr);

            this.weights[2][0] = Math.Cos(r - dr);
            this.weights[2][1] = Math.Cos(r - 2.0 * dr);
            this.weights[2][2] = Math.Cos(r - 3.0 * dr);
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            //TODO: review here
            unsafe
            {
                for (int i = start; i < start + len; ++i)
                {
                    Rectangle rect = rois[i];

                    // loop through each line of target rectangle
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        int fyStart = 0;
                        int fyEnd = 3;

                        if (y == src.Bounds.Top)
                        {
                            fyStart = 1;
                        }

                        if (y == src.Bounds.Bottom - 1)
                        {
                            fyEnd = 2;
                        }

                        // loop through each point in the line 
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            int fxStart = 0;
                            int fxEnd = 3;

                            if (x == src.Bounds.Left)
                            {
                                fxStart = 1;
                            }

                            if (x == src.Bounds.Right - 1)
                            {
                                fxEnd = 2;
                            }

                            // loop through each weight
                            double sum = 0.0;

                            for (int fy = fyStart; fy < fyEnd; ++fy)
                            {
                                for (int fx = fxStart; fx < fxEnd; ++fx)
                                {
                                    double weight = this.weights[fy][fx];
                                    ColorBgra c = src.GetPointUnchecked(x - 1 + fx, y - 1 + fy);
                                    double intensity = (double)c.GetIntensityByte();
                                    sum += weight * intensity;
                                }
                            }

                            int iSum = (int)sum + 128;

                            if (iSum > 255)
                            {
                                iSum = 255;
                            }
                            else if (iSum < 0)
                            {
                                iSum = 0;
                            }

                            *dstPtr = ColorBgra.FromBgra((byte)iSum, (byte)iSum, (byte)iSum, 255);

                            ++dstPtr;
                        }
                    }
                }
            }

        }
    }
}