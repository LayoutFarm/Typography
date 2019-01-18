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

namespace PixelFarm.PathReconstruction
{
    public interface IPixelEvaluator
    {
        int BufferOffset { get; }
        int X { get; }
        int Y { get; }
        int OrgBitmapWidth { get; }
        int OrgBitmapHeight { get; }
        /// <summary>
        /// set init pos, collect init check data
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void SetStartPos(int x, int y);

        /// <summary>
        /// move evaluaion point to 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void MoveTo(int x, int y);
        void RestoreMoveToPos();
        /// <summary>
        /// move check position to right side 1 px and check , if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool ReadNext();
        /// <summary>
        /// move check position to left side 1 px, and check, if not pass, return back to prev pos
        /// </summary>
        /// <returns>true if pass condition</returns>
        bool ReadPrev();
        /// <summary>
        /// read current and check
        /// </summary>
        /// <returns></returns>
        bool Read();
        void SetSourceDimension(int width, int height);
    }

    /// <summary>
    /// horizontal (scanline) span
    /// </summary>
    public struct HSpan
    {
        public readonly int startX;

        /// <summary>
        /// BEFORE touch endX, not include!
        /// </summary>
        public readonly int endX;

        public readonly int y;

        public HSpan(int startX, int endX, int y)
        {
            this.startX = startX;
            this.endX = endX;
            this.y = y;

            //spanLen= endX-startX
        }

        internal bool HorizontalTouchWith(int otherStartX, int otherEndX)
        {
            return HorizontalTouchWith(this.startX, this.endX, otherStartX, otherEndX);
        }
        internal static bool HorizontalTouchWith(int x0, int x1, int x2, int x3)
        {
            if (x0 == x2)
            {
                return true;
            }
            else if (x0 > x2)
            {
                //
                return x0 <= x3;
            }
            else
            {
                return x1 >= x2;
            }
        }

#if DEBUG
        public override string ToString()
        {
            return "line:" + y + ", x_start=" + startX + ",end_x=" + endX + ",len=" + (endX - startX);
        }
#endif
    }


    sealed class FloodFillRunner
    {
        bool[] _pixelsChecked;
        SimpleQueue<HSpan> _hspanQueue = new SimpleQueue<HSpan>(9);
        List<HSpan> _upperSpans = new List<HSpan>();
        List<HSpan> _lowerSpans = new List<HSpan>();
        int _yCutAt;
        IPixelEvaluator _pixelEvalutor;
        public HSpan[] InternalFill(IPixelEvaluator pixelEvalutor, int x, int y, bool collectHSpans)
        {

            _pixelEvalutor = pixelEvalutor;
            _yCutAt = y;
            //set cut-point 
            if (collectHSpans)
            {
                _upperSpans.Clear();
                _lowerSpans.Clear();
            }

            int imgW = pixelEvalutor.OrgBitmapWidth;
            int imgH = pixelEvalutor.OrgBitmapHeight;

            //reset new buffer, clear mem?
            _pixelsChecked = new bool[imgW * imgH];
            //*** 
            pixelEvalutor.SetStartPos(x, y);
            TryLinearFill(x, y);

            while (_hspanQueue.Count > 0)
            {
                HSpan hspan = _hspanQueue.Dequeue();
                if (collectHSpans)
                {
                    AddHSpan(hspan);
                }
                int downY = hspan.y - 1;
                int upY = hspan.y + 1;
                int downPixelOffset = (imgW * (hspan.y - 1)) + hspan.startX;
                int upPixelOffset = (imgW * (hspan.y + 1)) + hspan.startX;

                for (int rangeX = hspan.startX; rangeX < hspan.endX; rangeX++)
                {
                    if (hspan.y > 0 && !_pixelsChecked[downPixelOffset])
                    {
                        TryLinearFill(rangeX, downY);
                    }
                    if (hspan.y < (imgH - 1) && !_pixelsChecked[upPixelOffset])
                    {
                        TryLinearFill(rangeX, upY);
                    }
                    upPixelOffset++;
                    downPixelOffset++;
                }
            }

            //reset 
            _hspanQueue.Clear();
            _pixelEvalutor = null;

            return collectHSpans ? SortAndCollectHSpans() : null;
        }

        void AddHSpan(HSpan hspan)
        {
            if (hspan.y >= _yCutAt)
            {
                _lowerSpans.Add(hspan);
            }
            else
            {
                _upperSpans.Add(hspan);
            }
        }

        HSpan[] SortAndCollectHSpans()
        {
            int spanSort(HSpan sp1, HSpan sp2)
            {
                //NESTED METHOD
                //sort  asc,
                if (sp1.y > sp2.y)
                {
                    return 1;
                }
                else if (sp1.y < sp2.y)
                {
                    return -1;
                }
                else
                {
                    return sp1.startX.CompareTo(sp2.startX);
                }
            }
            //1.
            _upperSpans.Sort(spanSort);
            _lowerSpans.Sort(spanSort);

            HSpan[] hspans = new HSpan[_upperSpans.Count + _lowerSpans.Count];
            _upperSpans.CopyTo(hspans);
            _lowerSpans.CopyTo(hspans, _upperSpans.Count);

            //clear
            _upperSpans.Clear();
            _lowerSpans.Clear();

            return hspans;
        }


        /// <summary>
        /// fill to left side and right side of the line
        /// </summary>
        /// <param name="destBuffer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void TryLinearFill(int x, int y)
        {
            _pixelEvalutor.MoveTo(x, y);
            if (!_pixelEvalutor.Read())
            {
                return;
            }

            //if pass then...
            int pixelOffset = _pixelEvalutor.BufferOffset;
            //check at current pos 
            //then we will check each pixel on the left side step by step 
            for (; ; )
            {
                _pixelsChecked[pixelOffset] = true; //mark => current pixel is read 
                pixelOffset--;
                if ((_pixelsChecked[pixelOffset]) || !_pixelEvalutor.ReadPrev())
                {
                    break;
                }
            }

            int leftFillX = _pixelEvalutor.X; //save to use later

            _pixelEvalutor.RestoreMoveToPos();
            pixelOffset = _pixelEvalutor.BufferOffset;
            for (; ; )
            {
                _pixelsChecked[pixelOffset] = true;
                pixelOffset++;
                if (_pixelsChecked[pixelOffset] || !_pixelEvalutor.ReadNext())
                {
                    break;
                }
            }
            _hspanQueue.Enqueue(new HSpan(leftFillX, _pixelEvalutor.X, _pixelEvalutor.Y));//**   
        }

        class SimpleQueue<T>
        {
            T[] _itemArray;
            int _size;
            int _head;
            int _shiftFactor;
            int _mask;
            //

            public SimpleQueue(int shiftFactor)
            {
                _shiftFactor = shiftFactor;
                _mask = (1 << shiftFactor) - 1;
                _itemArray = new T[1 << shiftFactor];
                _head = 0;
                _size = 0;
            }
            public int Count => _size;
            public T First => _itemArray[_head & _mask];

            public void Clear() => _head = 0;

            public void Enqueue(T itemToQueue)
            {
                if (_size == _itemArray.Length)
                {
                    int headIndex = _head & _mask;
                    _shiftFactor += 1;
                    _mask = (1 << _shiftFactor) - 1;
                    T[] newArray = new T[1 << _shiftFactor];
                    // copy the from head to the end
                    Array.Copy(_itemArray, headIndex, newArray, 0, _size - headIndex);
                    // copy form 0 to the size
                    Array.Copy(_itemArray, 0, newArray, _size - headIndex, headIndex);
                    _itemArray = newArray;
                    _head = 0;
                }
                _itemArray[(_head + (_size++)) & _mask] = itemToQueue;
            }

            public T Dequeue()
            {
                int headIndex = _head & _mask;
                T firstItem = _itemArray[headIndex];
                if (_size > 0)
                {
                    _head++;
                    _size--;
                }
                return firstItem;
            }
        }
    }



}
