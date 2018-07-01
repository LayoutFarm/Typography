/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////

//Apache2, 2018, WinterDev

using System;
using PixelFarm.Drawing;
namespace PaintFx.Effects
{

    public sealed class RotateZoomEffect : EffectRendererBase
    {
        public Rectangle SelectionBounds { get; set; }
        public EffectConfigToken Parameters { get; set; }

        public unsafe override void Render(
            Surface src,
            Surface dst,
            PixelFarm.Drawing.Rectangle[] rois,
            int startIndex, int length)
        {
            RotateZoomEffectConfigToken token = (RotateZoomEffectConfigToken)Parameters;
            RotateZoomEffectConfigToken.RzInfo rzInfo = token.ComputedOnce;
            //Rectangle bounds = this.EnvironmentParameters.GetSelection(dstArgs.Bounds).GetBoundsInt();

            Rectangle bounds = SelectionBounds;
            bounds.Intersect(dst.Bounds);


            //PdnRegion selection = this.EnvironmentParameters.GetSelection(src.Bounds);
            Rectangle srcBounds = src.Bounds;
            int srcMaxX = srcBounds.Width - 1;
            int srcMaxY = srcBounds.Height - 1;

            float dsxdx = rzInfo.dsxdx;
            float dsydx = rzInfo.dsydx;
            float dszdx = rzInfo.dszdx;
            float dsxdy = rzInfo.dsxdy;
            float dsydy = rzInfo.dsydy;
            float dszdy = rzInfo.dszdy;
            float zoom = token.Zoom;
            uint srcMask = token.SourceAsBackground ? 0xffffffff : 0;

            bool tile = token.Tile;
            float divZ = 0.5f * (float)Math.Sqrt(dst.Width * dst.Width + dst.Height * dst.Height);
            float centerX = (float)dst.Width / 2.0f;
            float centerY = (float)dst.Height / 2.0f;
            float tx = (token.Offset.X) * dst.Width / 2.0f;
            float ty = (token.Offset.Y) * dst.Height / 2.0f;

            uint tilingMask = tile ? 0xffffffff : 0;

            for (int i = startIndex; i < startIndex + length; ++i)
            {
                Rectangle rect = rois[i];

                float cx = rzInfo.startX;
                float cy = rzInfo.startY;
                float cz = rzInfo.startZ;

                float mcl = ((rect.Left - tx) - dst.Width / 2.0f);
                cx += dsxdx * mcl;
                cy += dsydx * mcl;
                cz += dszdx * mcl;

                float mct = ((rect.Top - ty) - dst.Height / 2.0f);
                cx += dsxdy * mct;
                cy += dsydy * mct;
                cz += dszdy * mct;

                for (int y = rect.Top; y < rect.Bottom; ++y)
                {

                    ColorBgra* dstPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                    ColorBgra* srcPtr = src.GetPointAddressUnchecked(rect.Left, y);

                    float rx = cx;
                    float ry = cy;
                    float rz = cz;

                    for (int x = rect.Left; x < rect.Right; ++x)
                    {
                        if (rz > -divZ)
                        {
                            float div = divZ / (zoom * (divZ + rz));
                            float u = (rx * div) + centerX;
                            float v = (ry * div) + centerY;

                            if (tile || (u >= -1 && v >= -1 && u <= srcBounds.Width && v <= srcBounds.Height))
                            {
                                unchecked
                                {
                                    int iu = (int)Math.Floor(u);
                                    uint sxfrac = (uint)(256 * (u - (float)iu));
                                    uint sxfracinv = 256 - sxfrac;

                                    int iv = (int)Math.Floor(v);
                                    uint syfrac = (uint)(256 * (v - (float)iv));
                                    uint syfracinv = 256 - syfrac;

                                    uint wul = (uint)(sxfracinv * syfracinv);
                                    uint wur = (uint)(sxfrac * syfracinv);
                                    uint wll = (uint)(sxfracinv * syfrac);
                                    uint wlr = (uint)(sxfrac * syfrac);

                                    uint inBoundsMaskLeft = tilingMask;
                                    uint inBoundsMaskTop = tilingMask;
                                    uint inBoundsMaskRight = tilingMask;
                                    uint inBoundsMaskBottom = tilingMask;

                                    int sx = iu;
                                    if (sx < 0)
                                    {
                                        sx = srcMaxX + ((sx + 1) % srcBounds.Width);
                                    }
                                    else if (sx > srcMaxX)
                                    {
                                        sx = sx % srcBounds.Width;
                                    }
                                    else
                                    {
                                        inBoundsMaskLeft = 0xffffffff;
                                    }

                                    int sy = iv;
                                    if (sy < 0)
                                    {
                                        sy = srcMaxY + ((sy + 1) % srcBounds.Height);
                                    }
                                    else if (sy > srcMaxY)
                                    {
                                        sy = sy % srcBounds.Height;
                                    }
                                    else
                                    {
                                        inBoundsMaskTop = 0xffffffff;
                                    }

                                    int sleft = sx;
                                    int sright;

                                    if (sleft == srcMaxX)
                                    {
                                        sright = 0;
                                        inBoundsMaskRight = (iu == -1) ? 0xffffffff : tilingMask;
                                    }
                                    else
                                    {
                                        sright = sleft + 1;
                                        inBoundsMaskRight = inBoundsMaskLeft & 0xffffffff;
                                    }

                                    int stop = sy;
                                    int sbottom;

                                    if (stop == srcMaxY)
                                    {
                                        sbottom = 0;
                                        inBoundsMaskBottom = (iv == -1) ? 0xffffffff : tilingMask;
                                    }
                                    else
                                    {
                                        sbottom = stop + 1;
                                        inBoundsMaskBottom = inBoundsMaskTop & 0xffffffff;
                                    }

                                    uint maskUL = inBoundsMaskLeft & inBoundsMaskTop;
                                    ColorBgra cul = ColorBgra.FromUInt32(src.GetPointUnchecked(sleft, stop).Bgra & maskUL);

                                    uint maskUR = inBoundsMaskRight & inBoundsMaskTop;
                                    ColorBgra cur = ColorBgra.FromUInt32(src.GetPointUnchecked(sright, stop).Bgra & maskUR);

                                    uint maskLL = inBoundsMaskLeft & inBoundsMaskBottom;
                                    ColorBgra cll = ColorBgra.FromUInt32(src.GetPointUnchecked(sleft, sbottom).Bgra & maskLL);

                                    uint maskLR = inBoundsMaskRight & inBoundsMaskBottom;
                                    ColorBgra clr = ColorBgra.FromUInt32(src.GetPointUnchecked(sright, sbottom).Bgra & maskLR);

                                    ColorBgra c = ColorBgra.BlendColors4W16IP(cul, wul, cur, wur, cll, wll, clr, wlr);

                                    if (c.A == 255 || !token.SourceAsBackground)
                                    {
                                        dstPtr->Bgra = c.Bgra;
                                    }
                                    else
                                    {

                                        *dstPtr = PaintFx.UserBlendOps.NormalBlendOp.ApplyStatic(*srcPtr, c);
                                    }
                                }
                            }
                            else
                            {
                                if (srcMask != 0)
                                {
                                    dstPtr->Bgra = srcPtr->Bgra;
                                }
                                else
                                {
                                    dstPtr->Bgra = 0;
                                }
                            }
                        }
                        else
                        {
                            if (srcMask != 0)
                            {
                                dstPtr->Bgra = srcPtr->Bgra;
                            }
                            else
                            {
                                dstPtr->Bgra = 0;
                            }
                        }

                        rx += dsxdx;
                        ry += dsydx;
                        rz += dszdx;

                        ++dstPtr;
                        ++srcPtr;
                    }

                    cx += dsxdy;
                    cy += dsydy;
                    cz += dszdy;
                }
            }
        }


    }
}
