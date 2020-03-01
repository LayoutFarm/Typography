/////////////////////////////////////////////////////////////////////////////////
// Paint.NET (MIT,from version 3.36.7, see=> https://github.com/rivy/OpenPDN   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
/////////////////////////////////////////////////////////////////////////////////
//MIT, 2017-present, WinterDev
using PixelFarm.Drawing;
namespace PaintFx.Effects
{
    public class BrightnessAndContrastRenderer : EffectRendererBase
    {
        int _brightness;
        int _contrast;
        int _multiply;
        int _divide;
        byte[] _rgbTable;

        public void SetParameters(int brightness, int contrast)
        {
            _brightness = brightness;
            _contrast = contrast;
            if (_contrast < 0)
            {
                _multiply = _contrast + 100;
                _divide = 100;
            }
            else if (_contrast > 0)
            {
                _multiply = 100;
                _divide = 100 - _contrast;
            }
            else
            {
                _multiply = 1;
                _divide = 1;
            }

            if (_rgbTable == null)
            {
                _rgbTable = new byte[65536];
            }

            if (_divide == 0)
            {
                for (int intensity = 0; intensity < 256; ++intensity)
                {
                    if (intensity + _brightness < 128)
                    {
                        _rgbTable[intensity] = 0;
                    }
                    else
                    {
                        _rgbTable[intensity] = 255;
                    }
                }
            }
            else if (_divide == 100)
            {
                for (int intensity = 0; intensity < 256; ++intensity)
                {
                    int shift = (intensity - 127) * _multiply / _divide + 127 - intensity + _brightness;

                    for (int col = 0; col < 256; ++col)
                    {
                        int index = (intensity * 256) + col;
                        _rgbTable[index] = PixelUtils.ClampToByte(col + shift);
                    }
                }
            }
            else
            {
                for (int intensity = 0; intensity < 256; ++intensity)
                {
                    int shift = (intensity - 127 + _brightness) * _multiply / _divide + 127 - intensity;

                    for (int col = 0; col < 256; ++col)
                    {
                        int index = (intensity * 256) + col;
                        _rgbTable[index] = PixelUtils.ClampToByte(col + shift);
                    }
                }
            }
        }
        public override void Render(Surface src, Surface dst, Rectangle[] rois, int startIndex, int length)
        {
            unsafe
            {
                for (int r = startIndex; r < startIndex + length; ++r)
                {
                    Rectangle rect = rois[r];

                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* srcRowPtr = src.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* dstRowPtr = dst.GetPointAddressUnchecked(rect.Left, y);
                        ColorBgra* dstRowEndPtr = dstRowPtr + rect.Width;

                        if (_divide == 0)
                        {
                            while (dstRowPtr < dstRowEndPtr)
                            {
                                ColorBgra col = *srcRowPtr;
                                int i = col.GetIntensityByte();
                                uint c = _rgbTable[i];
                                dstRowPtr->Bgra = (col.Bgra & 0xff000000) | c | (c << 8) | (c << 16);

                                ++dstRowPtr;
                                ++srcRowPtr;
                            }
                        }
                        else
                        {
                            while (dstRowPtr < dstRowEndPtr)
                            {
                                ColorBgra col = *srcRowPtr;
                                int i = col.GetIntensityByte();
                                int shiftIndex = i * 256;

                                col.R = _rgbTable[shiftIndex + col.R];
                                col.G = _rgbTable[shiftIndex + col.G];
                                col.B = _rgbTable[shiftIndex + col.B];

                                *dstRowPtr = col;
                                ++dstRowPtr;
                                ++srcRowPtr;
                            }
                        }
                    }
                }
            }
        }
    }
}