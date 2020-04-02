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

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.CpuBlit;

namespace PixelFarm.PathReconstruction
{
    class MagicWandBmp32PixelEvaluatorWithTolerance : Bmp32PixelEvalToleranceMatch
    {
        short[] _diffMap;
        bool _fillDiffMap;
        short _latestDiff;

        public MagicWandBmp32PixelEvaluatorWithTolerance(byte tolerance) : base(tolerance)
        {
        }
        public void SetDiffMap(short[] diffMap)
        {
            _diffMap = diffMap;
        }
        public bool FillDiffMap
        {
            get => _fillDiffMap;
            set => _fillDiffMap = value;
        }
        protected short LatestDiff => _latestDiff;
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            if (base.CheckPixel(pixelAddr))
            {
                if (_fillDiffMap)
                {
                    _diffMap[base.CurrentBufferOffset] = _latestDiff = base.CalculateDiff();
                }
                return true;
            }
            return false;
        }
        protected override unsafe void SetStartColor(int* colorAddr)
        {
            base.SetStartColor(colorAddr);
            //next step is actual run
            if (_fillDiffMap && _diffMap == null)
            {
                //user want diffmap but dose not provide a diff map
                //so => auto turn off
                _fillDiffMap = false;
            }
        }
        protected override void OnSetSoureBitmap()
        {

            base.OnSetSoureBitmap();
        }
        protected override void OnReleaseSourceBitmap()
        {
            _diffMap = null;//clear
            base.OnReleaseSourceBitmap();
        }
    }

    class FillBmp32PixelEvaluatorWithTolerance : MagicWandBmp32PixelEvaluatorWithTolerance
    {
        int _fillColorInt32;
        public FillBmp32PixelEvaluatorWithTolerance(int fillColorInt32, byte tolerance) : base(tolerance)
        {
            _fillColorInt32 = fillColorInt32;
        }
        protected override unsafe bool CheckPixel(int* pixelAddr)
        {
            if (base.CheckPixel(pixelAddr))
            {
                //if pass then fill the pixel with the fill color
                *pixelAddr = _fillColorInt32;
                return true;
            }
            return false;
        }
    }



    /// <summary>
    /// solid color bucket tool
    /// </summary>
    public class ColorBucket
    {
        byte _tolerance;
        Color _fillColor;
        FillBmp32PixelEvaluatorWithTolerance _pixEval;
        FloodFillRunner _floodRunner = new FloodFillRunner();
        short[] _diffMap;
        int _fillColorInt32;
        public ColorBucket(Color fillColor)
          : this(fillColor, 0)
        {
        }
        public ColorBucket(Color fillColor, byte tolerance)
        {
            Update(fillColor, tolerance);
            _pixEval.FillDiffMap = true;
        }
        public Color FillColor => _fillColor;
        public byte Tolerance => _tolerance;
        public void Update(Color fillColor, byte tolerance)
        {
            _tolerance = tolerance;
            _fillColor = fillColor;

            _fillColorInt32 =
                (_fillColor.R << CO.R_SHIFT) |
                (_fillColor.G << CO.G_SHIFT) |
                (_fillColor.B << CO.B_SHIFT) |
                (_fillColor.A << CO.A_SHIFT);

            _pixEval = new FillBmp32PixelEvaluatorWithTolerance(_fillColorInt32, tolerance);

        }


        /// <summary>
        /// fill target bmp, start at (x,y), 
        /// </summary>
        /// <param name="bmpTarget"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="output"></param>
        public void Fill(MemBitmap bmp, int x, int y, ReconstructedRegionData output = null)
        {
            if (x < bmp.Width && y < bmp.Height)
            {
                _pixEval.SetSourceBitmap(bmp);

                if (_pixEval.FillDiffMap)
                {
                    _diffMap = new short[bmp.Width * bmp.Height];
                    _pixEval.SetDiffMap(_diffMap);
                }

                output.HSpans = _floodRunner.InternalFill(_pixEval, x, y, output != null);
                _pixEval.ReleaseSourceBitmap();
            }
        }

        public Action<List<HSpan[]>> AutoFilled;
        public void AutoFill(MemBitmap bmp,
            int x, int y, int w, int h,
            List<ReconstructedRegionData> output = null)
        {   
            if (x < bmp.Width && y < bmp.Height)
            {
                //autofill
                _pixEval.SetSourceBitmap(bmp);
                if (_pixEval.FillDiffMap)
                {
                    _diffMap = new short[bmp.Width * bmp.Height];
                    _pixEval.SetDiffMap(_diffMap);
                } 
                //--------------------
                //run the filling process
                _pixEval.SetCustomPixelChecker(p =>
                {
                    return p != _fillColorInt32;
                });

                _pixEval.SetSkipColor(Color.Red);

                int x0 = x;
                int x1 = x + w;
                int y0 = y;
                int y1 = y + h;

                //***
                _fillColor = Color.Red;

                List<HSpan[]> hSpanList = new List<HSpan[]>();
                for (int cur_y = y0; cur_y < y1; ++cur_y)
                {
                    for (int cur_x = x0; cur_x < x1; ++cur_x)
                    {
                        HSpan[] hspans = _floodRunner.InternalFill(_pixEval, cur_x, cur_y, output != null);
                        if (hspans != null && hspans.Length > 1)
                        {
                            hSpanList.Add(hspans);
                        }
                    }
                }
                //--------------------
                _pixEval.ReleaseSourceBitmap();
                AutoFilled?.Invoke(hSpanList);
            }
        }
    }


    public class MagicWand
    {
        byte _tolerance;
        MagicWandBmp32PixelEvaluatorWithTolerance _pixEval;
        FloodFillRunner _floodRunner = new FloodFillRunner();
        short[] _diffMap;
        public MagicWand(byte tolerance)
        {
            //no actual fill  
            Tolerance = tolerance;
        }
        public byte Tolerance
        {
            get => _tolerance;
            set
            {
                _tolerance = value;
                _pixEval = new MagicWandBmp32PixelEvaluatorWithTolerance(value);
                _pixEval.FillDiffMap = true;
            }
        }
        /// <summary>
        /// collect hspans into output region data
        /// </summary>
        /// <param name="bmpTarget"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="output"></param>
        public void CollectRegion(MemBitmap bmp, int x, int y, ReconstructedRegionData output)
        {
            if (x < bmp.Width && y < bmp.Height)
            {
                _pixEval.SetSourceBitmap(bmp);
                if (_pixEval.FillDiffMap)
                {
                    _diffMap = new short[bmp.Width * bmp.Height];
                    _pixEval.SetDiffMap(_diffMap);
                }

                output.HSpans = _floodRunner.InternalFill(_pixEval, x, y, output != null);
                _pixEval.ReleaseSourceBitmap();
            }
        }
    }

}