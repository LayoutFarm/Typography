/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// Copyright (c) 2006-2008 Ed Harvey 
//
// MIT License: http://www.opensource.org/licenses/mit-license.php
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
//
// The above copyright notice and this permission notice shall be included in 
// all copies or substantial portions of the Software. 
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
// THE SOFTWARE. 
//

//Apache2, 2017-present, WinterDev

using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public abstract class WrapBasedRenderer : EffectRendererBase
    {


        double offsetX;
        double offsetY;
        private WarpEdgeBehavior edgeBehavior = WarpEdgeBehavior.Wrap;
        protected int quality = 2;
        protected ColorBgra colPrimary;
        protected ColorBgra colSecondary;
        private double defaultRadius;
        private double defaultRadius2;
        private double defaultRadiusR;

        private double width;
        private double height;
        private double xCenterOffset;
        private double yCenterOffset;
        protected struct TransformData
        {
            public double X;
            public double Y;
        }

        public void BasicSetup(double defaultRadius, double defaultRadius2, double defaultRadiusR, int w, int h)
        {
            this.width = w;
            this.height = h;
            this.defaultRadius = defaultRadius;
            this.defaultRadius2 = defaultRadius2;
            this.defaultRadiusR = defaultRadiusR;

        }
        public double Width { get { return width; } }
        public double Height { get { return height; } }

        static bool IsOnSurface(Surface src, float u, float v)
        {
            return (u >= 0 && u <= (src.Width - 1) && v >= 0 && v <= (src.Height - 1));
        }
        protected abstract void InverseTransform(ref TransformData data);
        static float ReflectCoord(float value, int max)
        {
            bool reflection = false;

            while (value < 0)
            {
                value += max;
                reflection = !reflection;
            }

            while (value > max)
            {
                value -= max;
                reflection = !reflection;
            }

            if (reflection)
            {
                value = max - value;
            }

            return value;
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            ColorBgra colTransparent = ColorBgra.Transparent;
            unsafe
            {
                int aaSampleCount = quality * quality;
                PointF* aaPoints = stackalloc PointF[aaSampleCount];
                PixelUtils.GetRgssOffsets(aaPoints, aaSampleCount, quality);
                ColorBgra* samples = stackalloc ColorBgra[aaSampleCount];

                TransformData td;

                for (int n = startIndex; n < startIndex + length; ++n)
                {
                    Rectangle rect = rois[n];

                    for (int y = rect.Top; y < rect.Bottom; y++)
                    {
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                        double relativeY = y - this.yCenterOffset;

                        for (int x = rect.Left; x < rect.Right; x++)
                        {
                            double relativeX = x - this.xCenterOffset;

                            int sampleCount = 0;

                            for (int p = 0; p < aaSampleCount; ++p)
                            {
                                td.X = relativeX + aaPoints[p].X;
                                td.Y = relativeY - aaPoints[p].Y;

                                InverseTransform(ref td);

                                float sampleX = (float)(td.X + this.xCenterOffset);
                                float sampleY = (float)(td.Y + this.yCenterOffset);

                                ColorBgra sample = colPrimary;

                                if (IsOnSurface(src, sampleX, sampleY))
                                {
                                    sample = src.GetBilinearSample(sampleX, sampleY);
                                }
                                else
                                {
                                    switch (this.edgeBehavior)
                                    {
                                        case WarpEdgeBehavior.Clamp:
                                            sample = src.GetBilinearSampleClamped(sampleX, sampleY);
                                            break;

                                        case WarpEdgeBehavior.Wrap:
                                            sample = src.GetBilinearSampleWrapped(sampleX, sampleY);
                                            break;

                                        case WarpEdgeBehavior.Reflect:
                                            sample = src.GetBilinearSampleClamped(
                                                ReflectCoord(sampleX, src.Width),
                                                ReflectCoord(sampleY, src.Height));

                                            break;

                                        case WarpEdgeBehavior.Primary:
                                            sample = colPrimary;
                                            break;

                                        case WarpEdgeBehavior.Secondary:
                                            sample = colSecondary;
                                            break;

                                        case WarpEdgeBehavior.Transparent:
                                            sample = colTransparent;
                                            break;

                                        case WarpEdgeBehavior.Original:
                                            sample = src[x, y];
                                            break;

                                        default:
                                            break;
                                    }
                                }

                                samples[sampleCount] = sample;
                                ++sampleCount;
                            }

                            *dstPtr = ColorBgra.Blend(samples, sampleCount);
                            ++dstPtr;
                        }
                    }
                }
            }
        }


        public WarpEdgeBehavior EdgeBehavior
        {
            get
            {
                return edgeBehavior;
            }

            set
            {
                edgeBehavior = value;
            }
        }

        public int Quality
        {
            get
            {
                return quality;
            }

            set
            {
                quality = value;
            }
        }



        public double OffsetX
        {
            get
            {
                return offsetX;
            }
            set
            {
                offsetX = value;
            }
        }
        public double OffsetY
        {
            get { return offsetY; }
            set
            {
                offsetY = value;
            }
        }
        public void SetCenterOffset(double centerOffsetX, double centerOffsetY)
        {
            this.xCenterOffset = centerOffsetX;
            this.yCenterOffset = centerOffsetY;
        }


        /// <summary>
        /// The radius (in pixels) of the largest circle that can completely fit within the effect selection bounds
        /// </summary>
        public double DefaultRadius
        {
            get
            {
                return this.defaultRadius;
            }
        }

        /// <summary>
        /// The square of the DefaultRadius
        /// </summary>
        protected double DefaultRadius2
        {
            get
            {
                return this.defaultRadius2;
            }
        }

        /// <summary>
        /// The reciprical of the DefaultRadius
        /// </summary>
        protected double DefaultRadiusR
        {
            get
            {
                return this.defaultRadiusR;
            }
        }

    }

}
