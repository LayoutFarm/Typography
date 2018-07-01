/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//Apache2, 2017-present, WinterDev
using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public class ZoomBlurEffRenderer : EffectRendererBase
    {
        int amount;
        double offsetX;
        double offsetY;
        public void SetParameters(int amount, double offsetX, double offsetY)
        {
            this.amount = amount;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }
        const int n = 64;
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            long w = dst.Width;
            long h = dst.Height;
            long fox = (long)(dst.Width * offsetX * 32768.0);
            long foy = (long)(dst.Height * offsetY * 32768.0);
            long fcx = fox + (w << 15);
            long fcy = foy + (h << 15);
            long fz = this.amount;


            unsafe
            {
                for (int r = startIndex; r < startIndex + length; ++r)
                {
                    Rectangle rect = rois[r];

                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            long fx = (x << 16) - fcx;
                            long fy = (y << 16) - fcy;

                            int sr = 0;
                            int sg = 0;
                            int sb = 0;
                            int sa = 0;
                            int sc = 0;

                            sr += srcPtr->R * srcPtr->A;
                            sg += srcPtr->G * srcPtr->A;
                            sb += srcPtr->B * srcPtr->A;
                            sa += srcPtr->A;
                            ++sc;

                            for (int i = 0; i < n; ++i)
                            {
                                fx -= ((fx >> 4) * fz) >> 10;
                                fy -= ((fy >> 4) * fz) >> 10;

                                int u = (int)(fx + fcx + 32768 >> 16);
                                int v = (int)(fy + fcy + 32768 >> 16);

                                if (src.IsVisible(u, v))
                                {
                                    ColorBgra* srcPtr2 = src.GetPointAddressUnchecked(u, v);

                                    sr += srcPtr2->R * srcPtr2->A;
                                    sg += srcPtr2->G * srcPtr2->A;
                                    sb += srcPtr2->B * srcPtr2->A;
                                    sa += srcPtr2->A;
                                    ++sc;
                                }
                            }

                            if (sa != 0)
                            {
                                *dstPtr = ColorBgra.FromBgra(
                                    PixelUtils.ClampToByte(sb / sa),
                                    PixelUtils.ClampToByte(sg / sa),
                                    PixelUtils.ClampToByte(sr / sa),
                                    PixelUtils.ClampToByte(sa / sc));
                            }
                            else
                            {
                                dstPtr->Bgra = 0;
                            }

                            ++srcPtr;
                            ++dstPtr;
                        }
                    }
                }
            }
        }
    }
}
