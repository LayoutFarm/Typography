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
                *pixelAddr = _fillColorInt32;//action
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

            int fillColorInt32 =
                (_fillColor.red << CO.R_SHIFT) |
                (_fillColor.green << CO.G_SHIFT) |
                (_fillColor.blue << CO.B_SHIFT) |
                (_fillColor.alpha << CO.A_SHIFT);

            _pixEval = new FillBmp32PixelEvaluatorWithTolerance(fillColorInt32, tolerance);

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
                //****
                output.HSpans = _floodRunner.InternalFill(_pixEval, x, y, output != null);
                _pixEval.ReleaseSourceBitmap();
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