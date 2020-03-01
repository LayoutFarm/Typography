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

namespace PaintFx
{
    /// <summary>
    /// Provides a set of standard UnaryPixelOps.
    /// </summary>
    public sealed class UnaryPixelOps
    {
        private UnaryPixelOps()
        {
        }

        /// <summary>
        /// Passes through the given color value.
        /// result(color) = color
        /// </summary>

        public class Identity : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return color;
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                PlatformMemory.Copy(dst, src, (ulong)length * (ulong)sizeof(ColorBgra));
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                return;
            }
        }

        /// <summary>
        /// Always returns a constant color.
        /// </summary>

        public class Constant : UnaryPixelOp
        {
            ColorBgra _setColor;

            public override ColorBgra Apply(ColorBgra color)
            {
                return _setColor;
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    *dst = _setColor;
                    ++dst;
                    --length;
                }
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    *ptr = _setColor;
                    ++ptr;
                    --length;
                }
            }

            public Constant(ColorBgra setColor)
            {
                _setColor = setColor;
            }
        }

        /// <summary>
        /// Blends pixels with the specified constant color.
        /// </summary>

        public class BlendConstant : UnaryPixelOp
        {
            ColorBgra _blendColor;

            public override ColorBgra Apply(ColorBgra color)
            {
                int a = _blendColor.A;
                int invA = 255 - a;

                int r = ((color.R * invA) + (_blendColor.R * a)) / 256;
                int g = ((color.G * invA) + (_blendColor.G * a)) / 256;
                int b = ((color.B * invA) + (_blendColor.B * a)) / 256;
                byte a2 = ComputeAlpha(color.A, _blendColor.A);

                return ColorBgra.FromBgra((byte)b, (byte)g, (byte)r, a2);
            }

            public BlendConstant(ColorBgra blendColor)
            {
                _blendColor = blendColor;
            }
        }

        /// <summary>
        /// Used to set a given channel of a pixel to a given, predefined color.
        /// Useful if you want to set only the alpha value of a given region.
        /// </summary>

        public class SetChannel : UnaryPixelOp
        {
            int _channel;
            byte _setValue;

            public override ColorBgra Apply(ColorBgra color)
            {
                color[_channel] = _setValue;
                return color;
            }

            public override unsafe void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    *dst = *src;
                    (*dst)[_channel] = _setValue;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            public override unsafe void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    (*ptr)[_channel] = _setValue;
                    ++ptr;
                    --length;
                }
            }


            public SetChannel(int channel, byte setValue)
            {
                _channel = channel;
                _setValue = setValue;
            }
        }

        /// <summary>
        /// Specialization of SetChannel that sets the alpha channel.
        /// </summary>
        /// <remarks>This class depends on the system being litte-endian with the alpha channel 
        /// occupying the 8 most-significant-bits of a ColorBgra instance.
        /// By the way, we use addition instead of bitwise-OR because an addition can be
        /// perform very fast (0.5 cycles) on a Pentium 4.</remarks>

        public class SetAlphaChannel
            : UnaryPixelOp
        {
            uint _addValue;

            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromUInt32((color.Bgra & 0x00ffffff) + _addValue);
            }

            public override unsafe void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    dst->Bgra = (src->Bgra & 0x00ffffff) + _addValue;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            public override unsafe void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    ptr->Bgra = (ptr->Bgra & 0x00ffffff) + _addValue;
                    ++ptr;
                    --length;
                }
            }

            public SetAlphaChannel(byte alphaValue)
            {
                _addValue = (uint)alphaValue << 24;
            }
        }

        /// <summary>
        /// Specialization of SetAlphaChannel that always sets alpha to 255.
        /// </summary>

        public class SetAlphaChannelTo255 : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromUInt32(color.Bgra | 0xff000000);
            }

            public override unsafe void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    dst->Bgra = src->Bgra | 0xff000000;
                    ++dst;
                    ++src;
                    --length;
                }
            }

            public override unsafe void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    ptr->Bgra |= 0xff000000;
                    ++ptr;
                    --length;
                }
            }
        }

        /// <summary>
        /// Inverts a pixel's color, and passes through the alpha component.
        /// </summary>

        public class Invert : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra((byte)(255 - color.B), (byte)(255 - color.G), (byte)(255 - color.R), color.A);
            }
        }

        /// <summary>
        /// If the color is within the red tolerance, remove it
        /// </summary>

        public class RedEyeRemove : UnaryPixelOp
        {
            int _tolerence;
            double _setSaturation;

            public RedEyeRemove(int tol, int sat)
            {
                _tolerence = tol;
                _setSaturation = (double)sat / 100;
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                // The higher the saturation, the more red it is
                int saturation = GetSaturation(color);

                // The higher the difference between the other colors, the more red it is
                int difference = color.R - Math.Max(color.B, color.G);

                // If it is within tolerence, and the saturation is high
                if ((difference > _tolerence) && (saturation > 100))
                {
                    double i = 255.0 * color.GetIntensity();
                    byte ib = (byte)(i * _setSaturation); // adjust the red color for user inputted saturation
                    return ColorBgra.FromBgra((byte)color.B, (byte)color.G, ib, color.A);
                }
                else
                {
                    return color;
                }
            }

            //Saturation formula from RgbColor.cs, public HsvColor ToHsv()
            private int GetSaturation(ColorBgra color)
            {
                double min;
                double max;
                double delta;

                double r = (double)color.R / 255;
                double g = (double)color.G / 255;
                double b = (double)color.B / 255;

                double s;

                min = Math.Min(Math.Min(r, g), b);
                max = Math.Max(Math.Max(r, g), b);
                delta = max - min;

                if (max == 0 || delta == 0)
                {
                    // R, G, and B must be 0, or all the same.
                    // In this case, S is 0, and H is undefined.
                    // Using H = 0 is as good as any...
                    s = 0;
                }
                else
                {
                    s = delta / max;
                }

                return (int)(s * 255);
            }
        }

        /// <summary>
        /// Inverts a pixel's color and its alpha component.
        /// </summary>

        public class InvertWithAlpha : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra((byte)(255 - color.B), (byte)(255 - color.G), (byte)(255 - color.R), (byte)(255 - color.A));
            }
        }

        /// <summary>
        /// Averages the input color's red, green, and blue channels. The alpha component
        /// is unaffected.
        /// </summary>

        public class AverageChannels : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                byte average = (byte)(((int)color.R + (int)color.G + (int)color.B) / 3);
                return ColorBgra.FromBgra(average, average, average, color.A);
            }
        }


        public class Desaturate : UnaryPixelOp
        {
            public override ColorBgra Apply(ColorBgra color)
            {
                byte i = color.GetIntensityByte();
                return ColorBgra.FromBgra(i, i, i, color.A);
            }

            public unsafe override void Apply(ColorBgra* ptr, int length)
            {
                while (length > 0)
                {
                    byte i = ptr->GetIntensityByte();

                    ptr->R = i;
                    ptr->G = i;
                    ptr->B = i;

                    ++ptr;
                    --length;
                }
            }

            public unsafe override void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (length > 0)
                {
                    byte i = src->GetIntensityByte();

                    dst->B = i;
                    dst->G = i;
                    dst->R = i;
                    dst->A = src->A;

                    ++dst;
                    ++src;
                    --length;
                }
            }
        }


        public class LuminosityCurve : UnaryPixelOp
        {
            public byte[] Curve = new byte[256];

            public LuminosityCurve()
            {
                for (int i = 0; i < 256; ++i)
                {
                    Curve[i] = (byte)i;
                }
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                byte lumi = color.GetIntensityByte();
                int diff = Curve[lumi] - lumi;

                return ColorBgra.FromBgraClamped(
                    color.B + diff,
                    color.G + diff,
                    color.R + diff,
                    color.A);
            }
        }


        public class ChannelCurve : UnaryPixelOp
        {
            public byte[] CurveB = new byte[256];
            public byte[] CurveG = new byte[256];
            public byte[] CurveR = new byte[256];

            public ChannelCurve()
            {
                for (int i = 0; i < 256; ++i)
                {
                    CurveB[i] = (byte)i;
                    CurveG[i] = (byte)i;
                    CurveR[i] = (byte)i;
                }
            }

            public override unsafe void Apply(ColorBgra* dst, ColorBgra* src, int length)
            {
                while (--length >= 0)
                {
                    dst->B = CurveB[src->B];
                    dst->G = CurveG[src->G];
                    dst->R = CurveR[src->R];
                    dst->A = src->A;

                    ++dst;
                    ++src;
                }
            }

            public override unsafe void Apply(ColorBgra* ptr, int length)
            {
                while (--length >= 0)
                {
                    ptr->B = CurveB[ptr->B];
                    ptr->G = CurveG[ptr->G];
                    ptr->R = CurveR[ptr->R];

                    ++ptr;
                }
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                return ColorBgra.FromBgra(CurveB[color.B], CurveG[color.G], CurveR[color.R], color.A);
            }

            public override void Apply(Surface dst, Point dstOffset, Surface src, Point srcOffset, int scanLength)
            {
                base.Apply(dst, dstOffset, src, srcOffset, scanLength);
            }
        }


        public class Level : ChannelCurve
        {
            ColorBgra _colorInLow;
            public ColorBgra ColorInLow
            {
                get => _colorInLow;


                set
                {
                    if (value.R == 255)
                    {
                        value.R = 254;
                    }

                    if (value.G == 255)
                    {
                        value.G = 254;
                    }

                    if (value.B == 255)
                    {
                        value.B = 254;
                    }

                    if (_colorInHigh.R < value.R + 1)
                    {
                        _colorInHigh.R = (byte)(value.R + 1);
                    }

                    if (_colorInHigh.G < value.G + 1)
                    {
                        _colorInHigh.G = (byte)(value.R + 1);
                    }

                    if (_colorInHigh.B < value.B + 1)
                    {
                        _colorInHigh.B = (byte)(value.R + 1);
                    }

                    _colorInLow = value;
                    UpdateLookupTable();
                }
            }

            ColorBgra _colorInHigh;
            public ColorBgra ColorInHigh
            {
                get => _colorInHigh;

                set
                {
                    if (value.R == 0)
                    {
                        value.R = 1;
                    }

                    if (value.G == 0)
                    {
                        value.G = 1;
                    }

                    if (value.B == 0)
                    {
                        value.B = 1;
                    }

                    if (_colorInLow.R > value.R - 1)
                    {
                        _colorInLow.R = (byte)(value.R - 1);
                    }

                    if (_colorInLow.G > value.G - 1)
                    {
                        _colorInLow.G = (byte)(value.R - 1);
                    }

                    if (_colorInLow.B > value.B - 1)
                    {
                        _colorInLow.B = (byte)(value.R - 1);
                    }

                    _colorInHigh = value;
                    UpdateLookupTable();
                }
            }

            ColorBgra _colorOutLow;
            public ColorBgra ColorOutLow
            {
                get => _colorOutLow;
                set
                {
                    if (value.R == 255)
                    {
                        value.R = 254;
                    }

                    if (value.G == 255)
                    {
                        value.G = 254;
                    }

                    if (value.B == 255)
                    {
                        value.B = 254;
                    }

                    if (_colorOutHigh.R < value.R + 1)
                    {
                        _colorOutHigh.R = (byte)(value.R + 1);
                    }

                    if (_colorOutHigh.G < value.G + 1)
                    {
                        _colorOutHigh.G = (byte)(value.G + 1);
                    }

                    if (_colorOutHigh.B < value.B + 1)
                    {
                        _colorOutHigh.B = (byte)(value.B + 1);
                    }

                    _colorOutLow = value;
                    UpdateLookupTable();
                }
            }

            ColorBgra _colorOutHigh;
            public ColorBgra ColorOutHigh
            {
                get => _colorOutHigh;

                set
                {
                    if (value.R == 0)
                    {
                        value.R = 1;
                    }

                    if (value.G == 0)
                    {
                        value.G = 1;
                    }

                    if (value.B == 0)
                    {
                        value.B = 1;
                    }

                    if (_colorOutLow.R > value.R - 1)
                    {
                        _colorOutLow.R = (byte)(value.R - 1);
                    }

                    if (_colorOutLow.G > value.G - 1)
                    {
                        _colorOutLow.G = (byte)(value.G - 1);
                    }

                    if (_colorOutLow.B > value.B - 1)
                    {
                        _colorOutLow.B = (byte)(value.B - 1);
                    }

                    _colorOutHigh = value;
                    UpdateLookupTable();
                }
            }

            float[] _gamma = new float[3];
            public float GetGamma(int index)
            {
                if (index < 0 || index >= 3)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Index must be between 0 and 2");
                }

                return _gamma[index];
            }

            public void SetGamma(int index, float val)
            {
                if (index < 0 || index >= 3)
                {
                    throw new ArgumentOutOfRangeException("index", index, "Index must be between 0 and 2");
                }

                _gamma[index] = PixelUtils.Clamp(val, 0.1f, 10.0f);
                UpdateLookupTable();
            }

            public bool isValid = true;

            public static Level AutoFromLoMdHi(ColorBgra lo, ColorBgra md, ColorBgra hi)
            {
                float[] gamma = new float[3];

                for (int i = 0; i < 3; i++)
                {
                    if (lo[i] < md[i] && md[i] < hi[i])
                    {
                        gamma[i] = (float)PixelUtils.Clamp(Math.Log(0.5, (float)(md[i] - lo[i]) / (float)(hi[i] - lo[i])), 0.1, 10.0);
                    }
                    else
                    {
                        gamma[i] = 1.0f;
                    }
                }

                return new Level(lo, hi, gamma, ColorBgra.FromColor(Color.Black), ColorBgra.FromColor(Color.White));
            }

            private void UpdateLookupTable()
            {
                for (int i = 0; i < 3; i++)
                {
                    if (_colorOutHigh[i] < _colorOutLow[i] ||
                        _colorInHigh[i] <= _colorInLow[i] ||
                        _gamma[i] < 0)
                    {
                        isValid = false;
                        return;
                    }

                    for (int j = 0; j < 256; j++)
                    {
                        ColorBgra col = Apply(j, j, j);
                        CurveB[j] = col.B;
                        CurveG[j] = col.G;
                        CurveR[j] = col.R;
                    }
                }
            }

            public Level()
                : this(ColorBgra.FromColor(Color.Black),
                       ColorBgra.FromColor(Color.White),
                       new float[] { 1, 1, 1 },
                       ColorBgra.FromColor(Color.Black),
                       ColorBgra.FromColor(Color.White))
            {
            }

            public Level(ColorBgra in_lo, ColorBgra in_hi, float[] gamma, ColorBgra out_lo, ColorBgra out_hi)
            {
                _colorInLow = in_lo;
                _colorInHigh = in_hi;
                _colorOutLow = out_lo;
                _colorOutHigh = out_hi;

                if (gamma.Length != 3)
                {
                    throw new ArgumentException("gamma", "gamma must be a float[3]");
                }

                _gamma = gamma;
                UpdateLookupTable();
            }

            public ColorBgra Apply(float r, float g, float b)
            {
                ColorBgra ret = new ColorBgra();
                float[] input = new float[] { b, g, r };

                for (int i = 0; i < 3; i++)
                {
                    float v = (input[i] - _colorInLow[i]);

                    if (v < 0)
                    {
                        ret[i] = _colorOutLow[i];
                    }
                    else if (v + _colorInLow[i] >= _colorInHigh[i])
                    {
                        ret[i] = _colorOutHigh[i];
                    }
                    else
                    {
                        ret[i] = (byte)PixelUtils.Clamp(
                            _colorOutLow[i] + (_colorOutHigh[i] - _colorOutLow[i]) * Math.Pow(v / (_colorInHigh[i] - _colorInLow[i]), _gamma[i]),
                            0.0f,
                            255.0f);
                    }
                }

                return ret;
            }

            public void UnApply(ColorBgra after, float[] beforeOut, float[] slopesOut)
            {
                if (beforeOut.Length != 3)
                {
                    throw new ArgumentException("before must be a float[3]", "before");
                }

                if (slopesOut.Length != 3)
                {
                    throw new ArgumentException("slopes must be a float[3]", "slopes");
                }

                for (int i = 0; i < 3; i++)
                {
                    beforeOut[i] = _colorInLow[i] + (_colorInHigh[i] - _colorInLow[i]) *
                        (float)Math.Pow((float)(after[i] - _colorOutLow[i]) / (_colorOutHigh[i] - _colorOutLow[i]), 1 / _gamma[i]);

                    slopesOut[i] = (float)(_colorInHigh[i] - _colorInLow[i]) / ((_colorOutHigh[i] - _colorOutLow[i]) * _gamma[i]) *
                        (float)Math.Pow((float)(after[i] - _colorOutLow[i]) / (_colorOutHigh[i] - _colorOutLow[i]), 1 / _gamma[i] - 1);

                    if (float.IsInfinity(slopesOut[i]) || float.IsNaN(slopesOut[i]))
                    {
                        slopesOut[i] = 0;
                    }
                }
            }

            public object Clone()
            {
                Level copy = new Level(_colorInLow, _colorInHigh, (float[])_gamma.Clone(), _colorOutLow, _colorOutHigh);

                copy.CurveB = (byte[])this.CurveB.Clone();
                copy.CurveG = (byte[])this.CurveG.Clone();
                copy.CurveR = (byte[])this.CurveR.Clone();

                return copy;
            }
        }


        public class HueSaturationLightness : UnaryPixelOp
        {
            readonly int _hueDelta;
            readonly int _satFactor;
            UnaryPixelOp _blendOp;

            public HueSaturationLightness(int hueDelta, int satDelta, int lightness)
            {
                _hueDelta = hueDelta;
                _satFactor = (satDelta * 1024) / 100;

                if (lightness == 0)
                {
                    _blendOp = new UnaryPixelOps.Identity();
                }
                else if (lightness > 0)
                {
                    _blendOp = new UnaryPixelOps.BlendConstant(ColorBgra.FromBgra(255, 255, 255, (byte)((lightness * 255) / 100)));
                }
                else // if (lightness < 0)
                {
                    _blendOp = new UnaryPixelOps.BlendConstant(ColorBgra.FromBgra(0, 0, 0, (byte)((-lightness * 255) / 100)));
                }
            }

            public override ColorBgra Apply(ColorBgra color)
            {
                //adjust saturation
                byte intensity = color.GetIntensityByte();
                color.R = PixelUtils.ClampToByte((intensity * 1024 + (color.R - intensity) * _satFactor) >> 10);
                color.G = PixelUtils.ClampToByte((intensity * 1024 + (color.G - intensity) * _satFactor) >> 10);
                color.B = PixelUtils.ClampToByte((intensity * 1024 + (color.B - intensity) * _satFactor) >> 10);

                HsvColor hsvColor = HsvColor.FromColor(color.ToColor());
                int hue = hsvColor.Hue;

                hue += _hueDelta;

                while (hue < 0)
                {
                    hue += 360;
                }

                while (hue > 360)
                {
                    hue -= 360;
                }

                hsvColor.Hue = hue;

                ColorBgra newColor = ColorBgra.FromColor(hsvColor.ToColor());
                newColor = _blendOp.Apply(newColor);
                newColor.A = color.A;

                return newColor;
            }
        }
    }
}
