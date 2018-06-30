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
    public class OilPaintRenderer
    {
        private int brushSize;
        private byte coarseness;
        public void SetParameters(int brushSize, byte coarseness)
        {
            this.brushSize = brushSize;
            this.coarseness = coarseness;
        }
        public unsafe void Render(Surface src, Surface dst, Rectangle[] rois, int start, int len)
        {
            int width = src.Width;
            int height = src.Height;

            int arrayLens = 1 + this.coarseness;

            int localStoreSize = arrayLens * 5 * sizeof(int);

            byte* localStore = stackalloc byte[localStoreSize];
            byte* p = localStore;

            int* intensityCount = (int*)p;
            p += arrayLens * sizeof(int);

            uint* avgRed = (uint*)p;
            p += arrayLens * sizeof(uint);

            uint* avgGreen = (uint*)p;
            p += arrayLens * sizeof(uint);

            uint* avgBlue = (uint*)p;
            p += arrayLens * sizeof(uint);

            uint* avgAlpha = (uint*)p;
            p += arrayLens * sizeof(uint);

            byte maxIntensity = this.coarseness;

            //TODO: review here
            for (int r = start; r < start + len; ++r)
            {
                Rectangle rect = rois[r];

                int rectTop = rect.Top;
                int rectBottom = rect.Bottom;
                int rectLeft = rect.Left;
                int rectRight = rect.Right;

                for (int y = rectTop; y < rectBottom; ++y)
                {
                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);

                    int top = y - brushSize;
                    int bottom = y + brushSize + 1;

                    if (top < 0)
                    {
                        top = 0;
                    }

                    if (bottom > height)
                    {
                        bottom = height;
                    }

                    for (int x = rectLeft; x < rectRight; ++x)
                    {
                        PlatformMemory.SetToZero(localStore, (ulong)localStoreSize);

                        int left = x - brushSize;
                        int right = x + brushSize + 1;

                        if (left < 0)
                        {
                            left = 0;
                        }

                        if (right > width)
                        {
                            right = width;
                        }

                        int numInt = 0;

                        for (int j = top; j < bottom; ++j)
                        {
                            ColorBgra* srcPtr = src.GetPointAddressUnchecked(left, j);

                            for (int i = left; i < right; ++i)
                            {
                                byte intensity = PixelUtils.FastScaleByteByByte(srcPtr->GetIntensityByte(), maxIntensity);

                                ++intensityCount[intensity];
                                ++numInt;

                                avgRed[intensity] += srcPtr->R;
                                avgGreen[intensity] += srcPtr->G;
                                avgBlue[intensity] += srcPtr->B;
                                avgAlpha[intensity] += srcPtr->A;

                                ++srcPtr;
                            }
                        }

                        byte chosenIntensity = 0;
                        int maxInstance = 0;

                        for (int i = 0; i <= maxIntensity; ++i)
                        {
                            if (intensityCount[i] > maxInstance)
                            {
                                chosenIntensity = (byte)i;
                                maxInstance = intensityCount[i];
                            }
                        }

                        // TODO: correct handling of alpha values?

                        byte R = (byte)(avgRed[chosenIntensity] / maxInstance);
                        byte G = (byte)(avgGreen[chosenIntensity] / maxInstance);
                        byte B = (byte)(avgBlue[chosenIntensity] / maxInstance);
                        byte A = (byte)(avgAlpha[chosenIntensity] / maxInstance);

                        *dstPtr = ColorBgra.FromBgra(B, G, R, A);
                        ++dstPtr;
                    }
                }
            }
        }
    }
}