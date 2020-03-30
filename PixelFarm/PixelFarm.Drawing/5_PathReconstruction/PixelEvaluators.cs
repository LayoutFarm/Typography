//MIT, 2014-present, WinterDev
//MatterHackers 
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// Permission to copy, use, modify, sell and distribute this software 
// is granted provided this copyright notice appears in all copies. 
// This software is provided "as is" without express or implied
// warranty, and with no claim as to its suitability for any purpose.
//
//----------------------------------------------------------------------------
// Contact: mcseem@antigrain.com
//          mcseemagg@yahoo.com
//          http://www.antigrain.com
//----------------------------------------------------------------------------



using PixelFarm.Drawing;
using PixelFarm.CpuBlit;


namespace PixelFarm.PathReconstruction
{


    public abstract class Bmp32PixelEvaluator : IPixelEvaluator
    {
        /// <summary>
        /// address to head of the source of _bmpSrc
        /// </summary>
        unsafe int* _destBuffer;
        unsafe int* _currentAddr;

        int _srcW;
        /// <summary>
        /// width -1
        /// </summary>
        int _rightLim;
        int _srcH;
        int _curX;
        int _curY;
        int _bufferOffset;

        /// <summary>
        /// start move to at x
        /// </summary>
        int _moveToX;
        /// <summary>
        /// start move to at y
        /// </summary>
        int _moveToY;

        protected abstract unsafe bool CheckPixel(int* pixelAddr);
        protected abstract unsafe void SetStartColor(int* pixelAddr);
        protected virtual void OnSetSoureBitmap() { }
        protected virtual void OnReleaseSourceBitmap() { }

        public void SetSourceBitmap(MemBitmap bmpSrc)
        {
            ((IPixelEvaluator)this).SetSourceDimension(bmpSrc.Width, bmpSrc.Height);
            var memPtr = MemBitmap.GetBufferPtr(bmpSrc);
            unsafe
            {
                _currentAddr = _destBuffer = (int*)memPtr.Ptr;
            }
            OnSetSoureBitmap();
        }
        public void ReleaseSourceBitmap()
        {
            OnReleaseSourceBitmap();
        }
        void InternalMoveTo(int x, int y)
        {
            if (x >= 0 && x < _srcW && y >= 0 && y < _srcH)
            {
                _moveToX = _curX = x;
                _moveToY = _curY = y;
                unsafe
                {
                    //assign _bufferOffset too!!! 
                    _currentAddr = _destBuffer + (_bufferOffset = (y * _srcW) + x);
                }
            }

        }
        //------------------------------
        protected int CurrentBufferOffset => _bufferOffset;

        int IPixelEvaluator.BufferOffset => _bufferOffset;
        int IPixelEvaluator.X => _curX;
        int IPixelEvaluator.Y => _curY;
        int IPixelEvaluator.OrgBitmapWidth => _srcW;
        int IPixelEvaluator.OrgBitmapHeight => _srcH;
        /// <summary>
        /// set init pos, collect init check data
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void IPixelEvaluator.SetStartPos(int x, int y)
        {
            InternalMoveTo(x, y);//*** 
            unsafe
            {
                SetStartColor(_currentAddr);
            }
        }

        /// <summary>
        /// move evaluaion point to 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void IPixelEvaluator.MoveTo(int x, int y) => InternalMoveTo(x, y);
        void IPixelEvaluator.RestoreMoveToPos() => InternalMoveTo(_moveToX, _moveToY);

        /// <summary>
        /// move check position to right side 1 px and check , if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool IPixelEvaluator.ReadNext()
        {
            //append right pos 1 step
            unsafe
            {
                if (_curX < _rightLim)
                {
                    _curX++;
                    _bufferOffset++;
                    _currentAddr++;
                    if (!CheckPixel(_currentAddr))
                    {
                        //if not pass check => move back to prev pos
                        _curX--;
                        _bufferOffset--;
                        _currentAddr--;
                        return false;
                    }
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// move check position to left side 1 px, and check, if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool IPixelEvaluator.ReadPrev()
        {
            unsafe
            {
                if (_curX > 0)
                {
                    _curX--;
                    _bufferOffset--;
                    _currentAddr--;
                    if (!CheckPixel(_currentAddr))
                    {
                        //if not pass check => move back to prev pos
                        _curX++;
                        _bufferOffset++;
                        _currentAddr++;
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        /// <summary>
        /// read current and check
        /// </summary>
        /// <returns></returns>
        bool IPixelEvaluator.Read()
        {
            //check current pos
            unsafe
            {
                return CheckPixel(_currentAddr);
            }
        }

        void IPixelEvaluator.SetSourceDimension(int width, int height)
        {
            _srcW = width;
            _srcH = height;
            _rightLim = _srcW - 1;
        }
    }

    public class Bmp32PixelEvalExactMatch : Bmp32PixelEvaluator
    {
        int _startColorInt32;
        public Bmp32PixelEvalExactMatch()
        {

        }
        protected override unsafe void SetStartColor(int* colorAddr)
        {
            _startColorInt32 = *colorAddr;
        }
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            //ARGB
            return (_startColorInt32 == *pixelAddr);
        }
    }



    public class Bmp32PixelEvalToleranceMatch : Bmp32PixelEvaluator
    {

        byte _tolerance0To255;
        //** only RGB?
        byte _red_min, _red_max;
        byte _green_min, _green_max;
        byte _blue_min, _blue_max;

        int _latestInputValue;

        //start color
        byte _r;
        byte _g;
        byte _b;
        byte _a;
        //-------------- 

        System.Func<int, bool> _pixelPreviewDel;
        bool _hasSkipColor;
        int _skipColorInt32;

        public Bmp32PixelEvalToleranceMatch(byte initTolerance)
        {
            _tolerance0To255 = initTolerance;
        }
        public byte Tolerance
        {
            get => _tolerance0To255;
            set => _tolerance0To255 = value;
        }

        public void SetSkipColor(Color c)
        {
            _hasSkipColor = true;
            _skipColorInt32 =
                (c.A << CO.A_SHIFT) |
                (c.R << CO.R_SHIFT) |
                (c.G << CO.G_SHIFT) |
                (c.B << CO.B_SHIFT);

        }
        public void SetCustomPixelChecker(System.Func<int, bool> pixelPreviewDel)
        {
            _pixelPreviewDel = pixelPreviewDel;
        }
        protected override unsafe void SetStartColor(int* colorAddr)
        {
            int pixelValue32 = *colorAddr;

            _r = (byte)((pixelValue32 >> CO.R_SHIFT) & 0xff);
            _g = (byte)((pixelValue32 >> CO.G_SHIFT) & 0xff);
            _b = (byte)((pixelValue32 >> CO.B_SHIFT) & 0xff);
            _a = (byte)((pixelValue32 >> CO.A_SHIFT) & 0xff);
            //

            _red_min = Clamp(_r - _tolerance0To255);
            _red_max = Clamp(_r + _tolerance0To255);
            //
            _green_min = Clamp(_g - _tolerance0To255);
            _green_max = Clamp(_g + _tolerance0To255);
            //
            _blue_min = Clamp(_b - _tolerance0To255);
            _blue_max = Clamp(_b + _tolerance0To255);
        }
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            int pixelValue32 = _latestInputValue = *pixelAddr;

            if (_hasSkipColor && _skipColorInt32 == pixelValue32)
            {
                return false;
            }
            //if (_pixelPreviewDel != null && !_pixelPreviewDel(pixelValue32))
            //{
            //    //not pass preview 
            //    return false;
            //}

            int r = (pixelValue32 >> CO.R_SHIFT) & 0xff;
            int g = (pixelValue32 >> CO.G_SHIFT) & 0xff;
            int b = (pixelValue32 >> CO.B_SHIFT) & 0xff;
            //range test
            return ((r >= _red_min) && (r <= _red_max) &&
                   (g >= _green_min) && (g <= _green_max) &&
                   (b >= _blue_min) && (b <= _blue_max));
        }

        static byte Clamp(int value)
        {
            if (value < 0) return 0;
            if (value > 255) return 255;
            return (byte)value;
        }

        //
        protected int LatestInputValue => _latestInputValue;
        /// <summary>
        /// calculate diff of latest Input value separate by component
        /// </summary>
        /// <param name="rDiff"></param>
        /// <param name="gDiff"></param>
        /// <param name="bDiff"></param>
        protected void CalculateComponentDiff(out short rDiff, out short gDiff, out short bDiff, out short aDiff)
        {
            rDiff = (short)(_r - ((_latestInputValue >> CO.R_SHIFT) & 0xff));
            gDiff = (short)(_g - ((_latestInputValue >> CO.G_SHIFT) & 0xff));
            bDiff = (short)(_b - ((_latestInputValue >> CO.B_SHIFT) & 0xff));
            aDiff = (short)(_a - ((_latestInputValue >> CO.A_SHIFT) & 0xff));
        }
        /// <summary>
        ///  calculate diff of latest Input value separate by component
        /// </summary>
        /// <param name="rDiff"></param>
        /// <param name="gDiff"></param>
        /// <param name="bDiff"></param>
        protected void CalculateComponentDiff(out short rDiff, out short gDiff, out short bDiff)
        {
            rDiff = (short)(_r - ((_latestInputValue >> CO.R_SHIFT) & 0xff));
            gDiff = (short)(_g - ((_latestInputValue >> CO.G_SHIFT) & 0xff));
            bDiff = (short)(_b - ((_latestInputValue >> CO.B_SHIFT) & 0xff));
        }
        /// <summary>
        /// calculate diff of latest input value (only RGB? ,and sum)
        /// </summary>
        /// <returns></returns>
        protected short CalculateDiff()
        {
            return (short)(((_r - ((_latestInputValue >> CO.R_SHIFT) & 0xff)) +
                            (_g - ((_latestInputValue >> CO.G_SHIFT) & 0xff)) +
                            (_b - ((_latestInputValue >> CO.B_SHIFT) & 0xff))) / 3);
        }

    }


}