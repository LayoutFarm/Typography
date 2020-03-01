/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

// This effect was graciously provided by David Issel, aka BoltBait. His original
// copyright and license (MIT License) are reproduced below.

/* 
InkSketchEffect.cs 
Copyright (c) 2007 David Issel 
Contact info: BoltBait@hotmail.com http://www.BoltBait.com 

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 

The above copyright notice and this permission notice shall be included in 
all copies or substantial portions of the Software. 

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
THE SOFTWARE. 
*/


//MIT, 2017-present, WinterDev

using PixelFarm.Drawing;

namespace PaintFx.Effects
{
    public class InkSketchRenderer : EffectRendererBase
    {

        static InkSketchRenderer()
        {
            conv = new int[5][];

            for (int i = 0; i < conv.Length; ++i)
            {
                conv[i] = new int[5];
            }

            conv[0] = new int[] { -1, -1, -1, -1, -1 };
            conv[1] = new int[] { -1, -1, -1, -1, -1 };
            conv[2] = new int[] { -1, -1, 30, -1, -1 };
            conv[3] = new int[] { -1, -1, -1, -1, -1 };
            conv[4] = new int[] { -1, -1, -5, -1, -1 };
        }

        static readonly int[][] conv;
        const int SIZE = 5;
        const int RADIUS = (SIZE - 1) / 2;
        readonly UnaryPixelOps.Desaturate _desaturateOp = new UnaryPixelOps.Desaturate();
        readonly UserBlendOps.DarkenBlendOp _darkenOp = new UserBlendOps.DarkenBlendOp();
        int _inkOutline;


        public InkSketchRenderer()
        {
            //glowRenderer = new GlowRenderer();
            //glowRenderer.BrightnessAndContrastRenderer = new BrightnessAndContrastRenderer();
            //glowRenderer.BlurRenderer = new GaussainBlurRenderer();
        }
        public GlowRenderer GlowRenderer { get; set; }
        public void SetParameters(int inkOutline, int radius, int brightness, int contrast)
        {
            _inkOutline = inkOutline;
            GlowRenderer.SetParameters(radius, brightness, contrast);
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            unsafe
            {
                // Glow backgound 
                GlowRenderer.Render(src, dst, rois, startIndex, length);

                // Create black outlines by finding the edges of objects 

                for (int i = startIndex; i < startIndex + length; ++i)
                {
                    Rectangle roi = rois[i];

                    for (int y = roi.Top; y < roi.Bottom; ++y)
                    {
                        int top = y - RADIUS;
                        int bottom = y + RADIUS + 1;

                        if (top < 0)
                        {
                            top = 0;
                        }

                        if (bottom > dst.Height)
                        {
                            bottom = dst.Height;
                        }

                        ColorBgra* srcPtr = src.GetPointAddress(roi.X, y);
                        ColorBgra* dstPtr = src.GetPointAddress(roi.X, y);

                        for (int x = roi.Left; x < roi.Right; ++x)
                        {
                            int left = x - RADIUS;
                            int right = x + RADIUS + 1;

                            if (left < 0)
                            {
                                left = 0;
                            }

                            if (right > dst.Width)
                            {
                                right = dst.Width;
                            }

                            int r = 0;
                            int g = 0;
                            int b = 0;

                            for (int v = top; v < bottom; v++)
                            {
                                ColorBgra* pRow = src.GetRowAddress(v);
                                int j = v - y + RADIUS;

                                for (int u = left; u < right; u++)
                                {
                                    int i1 = u - x + RADIUS;
                                    int w = conv[j][i1];

                                    ColorBgra* pRef = pRow + u;

                                    r += pRef->R * w;
                                    g += pRef->G * w;
                                    b += pRef->B * w;
                                }
                            }

                            ColorBgra topLayer = ColorBgra.FromBgr(
                                PixelUtils.ClampToByte(b),
                                PixelUtils.ClampToByte(g),
                                PixelUtils.ClampToByte(r));

                            // Desaturate 
                            topLayer = _desaturateOp.Apply(topLayer);

                            // Adjust Brightness and Contrast 
                            if (topLayer.R > (_inkOutline * 255 / 100))
                            {
                                topLayer = ColorBgra.FromBgra(255, 255, 255, topLayer.A);
                            }
                            else
                            {
                                topLayer = ColorBgra.FromBgra(0, 0, 0, topLayer.A);
                            }

                            // Change Blend Mode to Darken 
                            ColorBgra myPixel = _darkenOp.Apply(topLayer, *dstPtr);
                            *dstPtr = myPixel;

                            ++srcPtr;
                            ++dstPtr;
                        }
                    }
                }
            }
        }
    }
}