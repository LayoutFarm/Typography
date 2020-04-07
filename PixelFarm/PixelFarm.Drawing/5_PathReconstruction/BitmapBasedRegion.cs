//MIT, 2019-present, WinterDev

using System;
using System.Collections.Generic;
using PixelFarm.Drawing;
using PixelFarm.Drawing.Internal;
using PixelFarm.CpuBlit;

namespace PixelFarm.PathReconstruction
{
    public class BitmapBasedRegion : CpuBlitRegion
    {
        MemBitmap _bmp;
        ReconstructedRegionData _reconRgnData;
        Rectangle _bounds;

        /// <summary>
        /// we STORE membitmap inside this rgn, 
        /// </summary>
        /// <param name="bmp"></param>
        public BitmapBasedRegion(CpuBlit.MemBitmap bmp)
        {
            _bmp = bmp;
            _bounds = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
        }

        public BitmapBasedRegion(ReconstructedRegionData reconRgnData)
        {
            _reconRgnData = reconRgnData;
            _bounds = reconRgnData.GetBounds();
        }
        public override bool IsSimpleRect => false;
        public override CpuBlitRegionKind Kind => CpuBlitRegionKind.BitmapBasedRegion;

        public override Region CreateComplement(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    {
                        //TODO: review here
                        BitmapBasedRegion bmpRgn = (BitmapBasedRegion)rgnB;
                    }
                    break;
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }
            return null;
        }

        public override Region CreateExclude(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    return CreateNewRegion((BitmapBasedRegion)rgnB, SetOperationName.Diff);

                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }

            return null;
        }

        public override Region CreateIntersect(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    return CreateNewRegion((BitmapBasedRegion)rgnB, SetOperationName.Intersect);

                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }
            return null;
        }

        public override bool IsVisible(PointF p)
        {
            throw new NotImplementedException();
        }
        public override bool IsVisible(RectangleF p)
        {
            throw new NotImplementedException();
        }


        struct MaskBitmapReader
        {
            //MemBitmap _bmp;
            int _width;
            int _widthLim;
            int _height;
            int _heightLim;
            unsafe int* _bufferPtr;
            unsafe int* _curAddr;

            int _readX;

            bool _canRead;
            public void SetBitmap(MemBitmap bmp)
            {

                _width = bmp.Width;
                _height = bmp.Height;
                _widthLim = _width - 1;
                _heightLim = _height - 1;

                unsafe
                {
                    _curAddr = _bufferPtr = (int*)MemBitmap.GetBufferPtr(bmp).Ptr;
                }
            }
            public void MoveTo(int x, int y)
            {
                //(0,0) is left-top 


                if (x < 0 || x >= _width)
                {
                    _canRead = false;
                    return;
                }
                if (y < 0 || y >= _height)
                {
                    _canRead = false;
                    return;
                }

                _canRead = true;
                _readX = x;
                unsafe
                {
                    _curAddr = _bufferPtr + (_width * y) + x;
                }

            }
            public bool CanRead => _canRead;

            public void MoveRight()
            {
                if (_canRead = (_readX < _widthLim))
                {
                    _readX++;
                    unsafe { _curAddr++; }
                }
            }
            public int Read()
            {
                unsafe
                {
                    return _canRead ? (*_curAddr) : 0;
                }
            }
        }

        struct MaskBitmapWriter
        {
            int _curX;

            int _width;
            int _widthLim;
            int _height;
            //int _heightLim;
            unsafe int* _bufferPtr;
            unsafe int* _curAddr;

            const int WHITE = (255 << CO.A_SHIFT) | (255 << CO.B_SHIFT) | (255 << CO.G_SHIFT) | (255 << CO.R_SHIFT);

            public void SetBitmap(MemBitmap bmp)
            {
                _width = bmp.Width;
                _height = bmp.Height;
                _widthLim = _width - 1;
                unsafe
                {
                    _curAddr = _bufferPtr = (int*)MemBitmap.GetBufferPtr(bmp).Ptr;
                }
            }
            public void MoveTo(int x, int y)
            {
                //(0,0) is left-top   

                if (x < 0 || x >= _width)
                {
                    return;
                }
                if (y < 0 || y >= _height)
                {
                    return;
                }


                _curX = x;
                unsafe
                {
                    _curAddr = _bufferPtr + (_width * y) + x;
                }
            }
            public void MoveRight()
            {
                if (_curX > _widthLim)
                {
                    return;
                }
                _curX++;
                unsafe
                {
                    _curAddr++;
                }
            }
            public void Union(int a_color, int b_color)
            {

                if (((a_color | b_color) & 0xff) != 0)
                {
                    unsafe { *_curAddr = WHITE; }
                }
            }
            public void Xor(int a_color, int b_color)
            {

                if (((a_color | b_color) & 0xff) != 0)
                {

                }
                else
                {
                    unsafe { *_curAddr = WHITE; }
                }
            }
            public void Intersect(int a_color, int b_color)
            {
                if (((a_color & b_color) & 0xff) != 0)
                {
                    unsafe { *_curAddr = WHITE; }
                }
            }
            public void Diff(int a_color, int b_color)
            {
                if ((a_color & 0xff) != 0 && (b_color & 0xff) == 0)
                {
                    unsafe { *_curAddr = WHITE; }
                }
            }
        }


        enum SetOperationName
        {
            Union,
            Intersect,
            Diff,
            Xor,
        }

        BitmapBasedRegion CreateNewRegion(BitmapBasedRegion another, SetOperationName opName)
        {

            //
            MemBitmap myBmp = this.GetRegionBitmap(); //or create new as need
            MemBitmap anotherBmp = another.GetRegionBitmap();
            //do bitmap union
            //2 rgn merge may 
            Rectangle r1Rect = this.GetRectBounds();
            Rectangle r2Rect = another.GetRectBounds();
            Rectangle r3Rect = Rectangle.Union(r1Rect, r2Rect);

            //
            MemBitmap r3Bmp = new MemBitmap(r3Rect.Width, r3Rect.Height);
            r3Bmp.Clear(Color.Black);

            MaskBitmapReader r1 = new MaskBitmapReader();
            r1.SetBitmap(myBmp);
            MaskBitmapReader r2 = new MaskBitmapReader();
            r2.SetBitmap(anotherBmp);
            MaskBitmapWriter w3 = new MaskBitmapWriter();
            w3.SetBitmap(r3Bmp);

            int height = r3Rect.Height;
            int width = r3Rect.Width;

            switch (opName)
            {
                case SetOperationName.Union:
                    for (int y = 0; y < height; ++y)
                    {
                        r1.MoveTo(0, y);
                        r2.MoveTo(0, y);
                        w3.MoveTo(0, y);
                        for (int x = 0; x < width; ++x)
                        {
                            w3.Union(r1.Read(), r2.Read());

                            r1.MoveRight();
                            r2.MoveRight();
                            w3.MoveRight();
                        }
                    }
                    break;
                case SetOperationName.Intersect:
                    for (int y = 0; y < height; ++y)
                    {
                        r1.MoveTo(0, y);
                        r2.MoveTo(0, y);
                        w3.MoveTo(0, y);
                        for (int x = 0; x < width; ++x)
                        {
                            w3.Intersect(r1.Read(), r2.Read());

                            r1.MoveRight();
                            r2.MoveRight();
                            w3.MoveRight();
                        }
                    }
                    break;
                case SetOperationName.Diff:
                    for (int y = 0; y < height; ++y)
                    {
                        r1.MoveTo(0, y);
                        r2.MoveTo(0, y);
                        w3.MoveTo(0, y);
                        for (int x = 0; x < width; ++x)
                        {
                            w3.Diff(r1.Read(), r2.Read());

                            r1.MoveRight();
                            r2.MoveRight();
                            w3.MoveRight();
                        }
                    }
                    break;
                case SetOperationName.Xor:
                    for (int y = 0; y < height; ++y)
                    {
                        r1.MoveTo(0, y);
                        r2.MoveTo(0, y);
                        w3.MoveTo(0, y);
                        for (int x = 0; x < width; ++x)
                        {
                            w3.Xor(r1.Read(), r2.Read());

                            r1.MoveRight();
                            r2.MoveRight();
                            w3.MoveRight();
                        }
                    }
                    break;
            }
            return new BitmapBasedRegion(r3Bmp);
        }

        public override Region CreateUnion(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    return CreateNewRegion((BitmapBasedRegion)rgnB, SetOperationName.Union);
                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }
            return null;
        }

        public override Region CreateXor(Region another)
        {
            CpuBlitRegion rgnB = another as CpuBlitRegion;
            if (rgnB == null) return null;
            //
            switch (rgnB.Kind)
            {
                default: throw new System.NotSupportedException();
                case CpuBlitRegionKind.BitmapBasedRegion:
                    return CreateNewRegion((BitmapBasedRegion)rgnB, SetOperationName.Xor);

                case CpuBlitRegionKind.MixedRegion:
                    break;
                case CpuBlitRegionKind.VxsRegion:
                    //TODO: review complement
                    break;
            }
            return null;
        }
        public override void Dispose()
        {
            if (_bmp != null)
            {
                _bmp.Dispose();
            }
            _bmp = null;
        }
        public override Rectangle GetRectBounds()
        {
            if (_bmp != null)
            {
                return new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            }
            else
            {
                return _reconRgnData.GetBounds();
            }
        }

        public MemBitmap GetRegionBitmap()
        {
            if (_bmp != null)
            {
                return _bmp;
            }
            else if (_reconRgnData != null)
            {
                //
                return _bmp = _reconRgnData.CreateMaskBitmap();
            }
            return null;
        }
    }



}