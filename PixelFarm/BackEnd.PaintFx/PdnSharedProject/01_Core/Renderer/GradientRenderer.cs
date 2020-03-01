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
namespace PaintFx
{
    public abstract class GradientRenderer
    {
        BinaryPixelOp _normalBlendOp;
        ColorBgra _startColor;
        ColorBgra _endColor;
        PointF _startPoint;
        PointF _endPoint;
        bool _alphaBlending;
        bool _alphaOnly;

        bool _lerpCacheIsValid = false;
        byte[] _lerpAlphas;
        ColorBgra[] _lerpColors;

        public ColorBgra StartColor
        {
            get => _startColor;
            set
            {
                if (_startColor != value)
                {
                    _startColor = value;
                    _lerpCacheIsValid = false;
                }
            }
        }

        public ColorBgra EndColor
        {
            get => _endColor;
            set
            {
                if (_endColor != value)
                {
                    _endColor = value;
                    _lerpCacheIsValid = false;
                }
            }
        }

        public PointF StartPoint
        {
            get => _startPoint;
            set
            {
                _startPoint = value;
            }
        }

        public PointF EndPoint
        {
            get => _endPoint;
            set
            {
                _endPoint = value;
            }
        }

        public bool AlphaBlending
        {
            get => _alphaBlending;

            set
            {
                _alphaBlending = value;
            }
        }

        public bool AlphaOnly
        {
            get => _alphaOnly;
            set
            {
                _alphaOnly = value;
            }
        }

        public virtual void BeforeRender()
        {
            if (!_lerpCacheIsValid)
            {
                byte startAlpha;
                byte endAlpha;

                if (_alphaOnly)
                {
                    ComputeAlphaOnlyValuesFromColors(_startColor, _endColor, out startAlpha, out endAlpha);
                }
                else
                {
                    startAlpha = _startColor.A;
                    endAlpha = _endColor.A;
                }

                _lerpAlphas = new byte[256];
                _lerpColors = new ColorBgra[256];

                for (int i = 0; i < 256; ++i)
                {
                    byte a = (byte)i;
                    _lerpColors[a] = ColorBgra.Blend(_startColor, _endColor, a);
                    _lerpAlphas[a] = (byte)(startAlpha + ((endAlpha - startAlpha) * a) / 255);
                }

                _lerpCacheIsValid = true;
            }
        }

        public abstract float ComputeUnboundedLerp(int x, int y);
        public abstract float BoundLerp(float t);

        public virtual void AfterRender()
        {
        }

        private static void ComputeAlphaOnlyValuesFromColors(ColorBgra startColor, ColorBgra endColor, out byte startAlpha, out byte endAlpha)
        {
            startAlpha = startColor.A;
            endAlpha = (byte)(255 - endColor.A);
        }

        public unsafe void Render(Surface surface, Rectangle[] rois, int startIndex, int length)
        {
            byte startAlpha;
            byte endAlpha;

            if (_alphaOnly)
            {
                ComputeAlphaOnlyValuesFromColors(_startColor, _endColor, out startAlpha, out endAlpha);
            }
            else
            {
                startAlpha = _startColor.A;
                endAlpha = _endColor.A;
            }

            for (int ri = startIndex; ri < startIndex + length; ++ri)
            {
                Rectangle rect = rois[ri];

                if (_startPoint.Equals(_endPoint))
                {
                    // Start and End point are the same ... fill with solid color.
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* pixelPtr = surface.GetPointAddress(rect.Left, y);

                        for (int x = rect.Left; x < rect.Right; ++x)
                        {
                            ColorBgra result;

                            if (_alphaOnly && _alphaBlending)
                            {
                                byte resultAlpha = (byte)PixelUtils.FastDivideShortByByte((ushort)(pixelPtr->A * endAlpha), 255);
                                result = *pixelPtr;
                                result.A = resultAlpha;
                            }
                            else if (_alphaOnly && !_alphaBlending)
                            {
                                result = *pixelPtr;
                                result.A = endAlpha;
                            }
                            else if (!_alphaOnly && _alphaBlending)
                            {
                                result = _normalBlendOp.Apply(*pixelPtr, _endColor);
                            }
                            else //if (!this.alphaOnly && !this.alphaBlending)
                            {
                                result = _endColor;
                            }

                            *pixelPtr = result;
                            ++pixelPtr;
                        }
                    }
                }
                else
                {
                    for (int y = rect.Top; y < rect.Bottom; ++y)
                    {
                        ColorBgra* pixelPtr = surface.GetPointAddress(rect.Left, y);

                        if (_alphaOnly && _alphaBlending)
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                byte lerpAlpha = _lerpAlphas[lerpByte];
                                byte resultAlpha = PixelUtils.FastScaleByteByByte(pixelPtr->A, lerpAlpha);
                                pixelPtr->A = resultAlpha;
                                ++pixelPtr;
                            }
                        }
                        else if (_alphaOnly && !_alphaBlending)
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                byte lerpAlpha = _lerpAlphas[lerpByte];
                                pixelPtr->A = lerpAlpha;
                                ++pixelPtr;
                            }
                        }
                        else if (!_alphaOnly && (_alphaBlending && (startAlpha != 255 || endAlpha != 255)))
                        {
                            // If we're doing all color channels, and we're doing alpha blending, and if alpha blending is necessary
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                ColorBgra lerpColor = _lerpColors[lerpByte];
                                ColorBgra result = _normalBlendOp.Apply(*pixelPtr, lerpColor);
                                *pixelPtr = result;
                                ++pixelPtr;
                            }
                        }
                        else //if (!this.alphaOnly && !this.alphaBlending) // or sC.A == 255 && eC.A == 255
                        {
                            for (int x = rect.Left; x < rect.Right; ++x)
                            {
                                float lerpUnbounded = ComputeUnboundedLerp(x, y);
                                float lerpBounded = BoundLerp(lerpUnbounded);
                                byte lerpByte = (byte)(lerpBounded * 255.0f);
                                ColorBgra lerpColor = _lerpColors[lerpByte];
                                *pixelPtr = lerpColor;
                                ++pixelPtr;
                            }
                        }
                    }
                }
            }

            AfterRender();
        }

        protected internal GradientRenderer(bool alphaOnly, BinaryPixelOp normalBlendOp)
        {
            _normalBlendOp = normalBlendOp;
            _alphaOnly = alphaOnly;
        }
    }
}
