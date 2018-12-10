//BSD, 2014-present, WinterDev
//----------------------------------------------------------------------------
// Anti-Grain Geometry - Version 2.4
// Copyright (C) 2002-2005 Maxim Shemanarev (http://www.antigrain.com)
//
// C# Port port by: Lars Brubaker
//                  larsbrubaker@gmail.com
// Copyright (C) 2007
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
//#ifndef AGG_RASTERIZER_SL_CLIP_INCLUDED
//#define AGG_RASTERIZER_SL_CLIP_INCLUDED

//#include "agg_clip_liang_barsky.h"


using PixelFarm.CpuBlit.PrimitiveProcessing;
namespace PixelFarm.CpuBlit.Rasterization
{

    partial class ScanlineRasterizer
    {
        class VectorClipper
        {
            RectInt _clipBox;
            int _x1;
            int _y1;
            int _f1;
            bool _clipping;
            CellAARasterizer _ras;
            public VectorClipper(CellAARasterizer ras)
            {
                _ras = ras;
                _clipBox = new RectInt(0, 0, 0, 0);
                _x1 = _y1 = _f1 = 0;
                _clipping = false;
            }
            public RectInt GetVectorClipBox() => _clipBox;

            public void SetClipBox(int x1, int y1, int x2, int y2)
            {
                _clipBox = new RectInt(x1, y1, x2, y2);
                _clipBox.Normalize();
                _clipping = true;
            }

            /// <summary>
            /// clip box width is extened 3 times for lcd-effect subpixel rendering
            /// </summary>
            bool _clipBoxWidthX3ForSubPixelLcdEffect = false; //default

            /// <summary>
            /// when we render in subpixel rendering, we extend a row length 3 times (expand RGB)
            /// </summary>
            /// <param name="value"></param>
            public void SetClipBoxWidthX3ForSubPixelLcdEffect(bool value)
            {
                //-----------------------------------------------------------------------------
                //if we don't want to expand our img buffer 3 times (larger than normal)
                //we should use this method to extend only a cliper box's width x3                 
                //-----------------------------------------------------------------------------

                //special method for our need
                if (value != _clipBoxWidthX3ForSubPixelLcdEffect)
                {
                    //changed
                    if (value)
                    {
                        _clipBox = new RectInt(_clipBox.Left, _clipBox.Bottom, _clipBox.Left + (_clipBox.Width * 3), _clipBox.Height);
                    }
                    else
                    {
                        //set back
                        _clipBox = new RectInt(_clipBox.Left, _clipBox.Bottom, _clipBox.Left + (_clipBox.Width / 3), _clipBox.Height);
                    }
                    _clipBoxWidthX3ForSubPixelLcdEffect = value;
                }
            }

            public void ResetClipping()
            {
                _clipping = false;
            }
            public void MoveTo(int x1, int y1)
            {
                _x1 = x1;
                _y1 = y1;
                if (_clipping)
                {
                    _f1 = ClipLiangBarsky.Flags(x1, y1, _clipBox);
                }
            }

            //------------------------------------------------------------------------
            void LineClipY(int x1, int y1,
                           int x2, int y2,
                           int f1, int f2)
            {
                f1 &= 10;
                f2 &= 10;
                if ((f1 | f2) == 0)
                {
                    // Fully visible
                    _ras.DrawLine(x1, y1, x2, y2);
                }
                else
                {
                    if (f1 == f2)
                    {
                        // Invisible by Y
                        return;
                    }

                    int tx1 = x1;
                    int ty1 = y1;
                    int tx2 = x2;
                    int ty2 = y2;
                    if ((f1 & 8) != 0) // y1 < clip.y1
                    {
                        tx1 = x1 + MulDiv(_clipBox.Bottom - y1, x2 - x1, y2 - y1);
                        ty1 = _clipBox.Bottom;
                    }

                    if ((f1 & 2) != 0) // y1 > clip.y2
                    {
                        tx1 = x1 + MulDiv(_clipBox.Top - y1, x2 - x1, y2 - y1);
                        ty1 = _clipBox.Top;
                    }

                    if ((f2 & 8) != 0) // y2 < clip.y1
                    {
                        tx2 = x1 + MulDiv(_clipBox.Bottom - y1, x2 - x1, y2 - y1);
                        ty2 = _clipBox.Bottom;
                    }

                    if ((f2 & 2) != 0) // y2 > clip.y2
                    {
                        tx2 = x1 + MulDiv(_clipBox.Top - y1, x2 - x1, y2 - y1);
                        ty2 = _clipBox.Top;
                    }

                    _ras.DrawLine(tx1, ty1, tx2, ty2);
                }
            }

            //--------------------------------------------------------------------
            public void LineTo(int x2, int y2)
            {
                if (_clipping)
                {
                    int f2 = ClipLiangBarsky.Flags(x2, y2, _clipBox);
                    if ((_f1 & 10) == (f2 & 10) && (_f1 & 10) != 0)
                    {
                        // Invisible by Y
                        _x1 = x2;
                        _y1 = y2;
                        _f1 = f2;
                        return;
                    }

                    int x1 = _x1;
                    int y1 = _y1;
                    int f1 = _f1;
                    int y3, y4;
                    int f3, f4;
                    switch (((f1 & 5) << 1) | (f2 & 5))
                    {
                        case 0: // Visible by X
                            LineClipY(x1, y1, x2, y2, f1, f2);
                            break;
                        case 1: // x2 > clip.x2
                            y3 = y1 + MulDiv(_clipBox.Right - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            LineClipY(x1, y1, _clipBox.Right, y3, f1, f3);
                            LineClipY(_clipBox.Right, y3, _clipBox.Right, y2, f3, f2);
                            break;
                        case 2: // x1 > clip.x2
                            y3 = y1 + MulDiv(_clipBox.Right - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            LineClipY(_clipBox.Right, y1, _clipBox.Right, y3, f1, f3);
                            LineClipY(_clipBox.Right, y3, x2, y2, f3, f2);
                            break;
                        case 3: // x1 > clip.x2 && x2 > clip.x2
                            LineClipY(_clipBox.Right, y1, _clipBox.Right, y2, f1, f2);
                            break;
                        case 4: // x2 < clip.x1
                            y3 = y1 + MulDiv(_clipBox.Left - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            LineClipY(x1, y1, _clipBox.Left, y3, f1, f3);
                            LineClipY(_clipBox.Left, y3, _clipBox.Left, y2, f3, f2);
                            break;
                        case 6: // x1 > clip.x2 && x2 < clip.x1
                            y3 = y1 + MulDiv(_clipBox.Right - x1, y2 - y1, x2 - x1);
                            y4 = y1 + MulDiv(_clipBox.Left - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            f4 = ClipLiangBarsky.GetFlagsY(y4, _clipBox);
                            LineClipY(_clipBox.Right, y1, _clipBox.Right, y3, f1, f3);
                            LineClipY(_clipBox.Right, y3, _clipBox.Left, y4, f3, f4);
                            LineClipY(_clipBox.Left, y4, _clipBox.Left, y2, f4, f2);
                            break;
                        case 8: // x1 < clip.x1
                            y3 = y1 + MulDiv(_clipBox.Left - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            LineClipY(_clipBox.Left, y1, _clipBox.Left, y3, f1, f3);
                            LineClipY(_clipBox.Left, y3, x2, y2, f3, f2);
                            break;
                        case 9:  // x1 < clip.x1 && x2 > clip.x2
                            y3 = y1 + MulDiv(_clipBox.Left - x1, y2 - y1, x2 - x1);
                            y4 = y1 + MulDiv(_clipBox.Right - x1, y2 - y1, x2 - x1);
                            f3 = ClipLiangBarsky.GetFlagsY(y3, _clipBox);
                            f4 = ClipLiangBarsky.GetFlagsY(y4, _clipBox);
                            LineClipY(_clipBox.Left, y1, _clipBox.Left, y3, f1, f3);
                            LineClipY(_clipBox.Left, y3, _clipBox.Right, y4, f3, f4);
                            LineClipY(_clipBox.Right, y4, _clipBox.Right, y2, f4, f2);
                            break;
                        case 12: // x1 < clip.x1 && x2 < clip.x1
                            LineClipY(_clipBox.Left, y1, _clipBox.Left, y2, f1, f2);
                            break;
                    }
                    _f1 = f2;
                }
                else
                {
                    _ras.DrawLine(_x1, _y1,
                             x2, y2);
                }
                _x1 = x2;
                _y1 = y2;
            }


            static int MulDiv(int a, int b, int c)
            {
                return AggMath.iround_f((float)a * (float)b / (float)c);
            }
        }
    }
}