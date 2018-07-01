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
//
// Liang-Barsky clipping 
//
//----------------------------------------------------------------------------

namespace PixelFarm.CpuBlit.PrimitiveProcessing
{
    public static class ClipLiangBarsky
    {
        //------------------------------------------------------------------------
        static class ClippingFlags
        {
            public const int cX1 = 4; // x1 clipped
            public const int cX2 = 1; //x2 clipped
            public const int cY1 = 8; //y1 clipped
            public const int cY2 = 2; //y2 clipped
            public const int cX1X2 = cX1 | cX2;
            public const int cY1Y2 = cY1 | cY2;
        }

        //----------------------------------------------------------clipping_flags
        // Determine the clipping code of the vertex according to the 
        // Cyrus-Beck line clipping algorithm
        //
        //        |        |
        //  0110  |  0010  | 0011
        //        |        |
        // -------+--------+-------- clip_box.y2
        //        |        |
        //  0100  |  0000  | 0001
        //        |        |
        // -------+--------+-------- clip_box.y1
        //        |        |
        //  1100  |  1000  | 1001
        //        |        |
        //  clip_box.x1  clip_box.x2
        //
        // 
        //template<class T>
        public static int Flags(int x, int y, RectInt clip_box)
        {
            return ((x > clip_box.Right) ? 1 : 0)
                | ((y > clip_box.Top) ? 1 << 1 : 0)
                | ((x < clip_box.Left) ? 1 << 2 : 0)
                | ((y < clip_box.Bottom) ? 1 << 3 : 0);
        }

        public static int GetFlagsX(int x, RectInt clip_box)
        {
            return ((x > clip_box.Right ? 1 : 0) | ((x < clip_box.Left ? 1 : 0) << 2));
        }

        public static int GetFlagsY(int y, RectInt clip_box)
        {
            return (((y > clip_box.Top ? 1 : 0) << 1) | ((y < clip_box.Bottom ? 1 : 0) << 3));
        }

        public static int DoClipLiangBarsky(int x1, int y1, int x2, int y2,
                                          RectInt clip_box,
                                          int[] x, int[] y)
        {
            int xIndex = 0;
            int yIndex = 0;
            double nearzero = 1e-30;
            double deltax = x2 - x1;
            double deltay = y2 - y1;
            double xin;
            double xout;
            double yin;
            double yout;
            double tinx;
            double tiny;
            double toutx;
            double touty;
            double tin1;
            double tin2;
            double tout1;
            int np = 0;
            if (deltax == 0.0)
            {
                // bump off of the vertical
                deltax = (x1 > clip_box.Left) ? -nearzero : nearzero;
            }

            if (deltay == 0.0)
            {
                // bump off of the horizontal 
                deltay = (y1 > clip_box.Bottom) ? -nearzero : nearzero;
            }

            if (deltax > 0.0)
            {
                // points to right
                xin = clip_box.Left;
                xout = clip_box.Right;
            }
            else
            {
                xin = clip_box.Right;
                xout = clip_box.Left;
            }

            if (deltay > 0.0)
            {
                // points up
                yin = clip_box.Bottom;
                yout = clip_box.Top;
            }
            else
            {
                yin = clip_box.Top;
                yout = clip_box.Bottom;
            }

            tinx = (xin - x1) / deltax;
            tiny = (yin - y1) / deltay;
            if (tinx < tiny)
            {
                // hits x first
                tin1 = tinx;
                tin2 = tiny;
            }
            else
            {
                // hits y first
                tin1 = tiny;
                tin2 = tinx;
            }

            if (tin1 <= 1.0)
            {
                if (0.0 < tin1)
                {
                    x[xIndex++] = (int)xin;
                    y[yIndex++] = (int)yin;
                    ++np;
                }

                if (tin2 <= 1.0)
                {
                    toutx = (xout - x1) / deltax;
                    touty = (yout - y1) / deltay;
                    tout1 = (toutx < touty) ? toutx : touty;
                    if (tin2 > 0.0 || tout1 > 0.0)
                    {
                        if (tin2 <= tout1)
                        {
                            if (tin2 > 0.0)
                            {
                                if (tinx > tiny)
                                {
                                    x[xIndex++] = (int)xin;
                                    y[yIndex++] = (int)(y1 + tinx * deltay);
                                }
                                else
                                {
                                    x[xIndex++] = (int)(x1 + tiny * deltax);
                                    y[yIndex++] = (int)yin;
                                }
                                ++np;
                            }

                            if (tout1 < 1.0)
                            {
                                if (toutx < touty)
                                {
                                    x[xIndex++] = (int)xout;
                                    y[yIndex++] = (int)(y1 + toutx * deltay);
                                }
                                else
                                {
                                    x[xIndex++] = (int)(x1 + touty * deltax);
                                    y[yIndex++] = (int)yout;
                                }
                            }
                            else
                            {
                                x[xIndex++] = x2;
                                y[yIndex++] = y2;
                            }
                            ++np;
                        }
                        else
                        {
                            if (tinx > tiny)
                            {
                                x[xIndex++] = (int)xin;
                                y[yIndex++] = (int)yout;
                            }
                            else
                            {
                                x[xIndex++] = (int)xout;
                                y[yIndex++] = (int)yin;
                            }
                            ++np;
                        }
                    }
                }
            }
            return np;
        }

        public static bool ClipMovePoint(int x1, int y1, int x2, int y2,
                             RectInt clip_box,
                             ref int x, ref int y, int flags)
        {
            int bound;
            if ((flags & ClippingFlags.cX1X2) != 0)
            {
                if (x1 == x2)
                {
                    return false;
                }
                bound = ((flags & ClippingFlags.cX1) != 0) ? clip_box.Left : clip_box.Right;
                y = (int)((double)(bound - x1) * (y2 - y1) / (x2 - x1) + y1);
                x = bound;
            }

            flags = GetFlagsY(y, clip_box);
            if ((flags & ClippingFlags.cY1Y2) != 0)
            {
                if (y1 == y2)
                {
                    return false;
                }
                bound = ((flags & ClippingFlags.cX1) != 0) ? clip_box.Bottom : clip_box.Top;
                x = (int)((double)(bound - y1) * (x2 - x1) / (y2 - y1) + x1);
                y = bound;
            }
            return true;
        }

        //-------------------------------------------------------clip_line_segment
        // Returns: ret >= 4        - Fully clipped
        //          (ret & 1) != 0  - First point has been moved
        //          (ret & 2) != 0  - Second point has been moved
        //
        //template<class T>
        public static int ClipLineSegment(ref int x1, ref int y1, ref int x2, ref int y2,
                                   RectInt clip_box)
        {
            int f1 = Flags(x1, y1, clip_box);
            int f2 = Flags(x2, y2, clip_box);
            int ret = 0;
            if ((f2 | f1) == 0)
            {
                // Fully visible
                return 0;
            }

            if ((f1 & ClippingFlags.cX1X2) != 0 &&
               (f1 & ClippingFlags.cX1X2) == (f2 & ClippingFlags.cX1X2))
            {
                // Fully clipped
                return 4;
            }

            if ((f1 & ClippingFlags.cY1Y2) != 0 &&
               (f1 & ClippingFlags.cY1Y2) == (f2 & ClippingFlags.cY1Y2))
            {
                // Fully clipped
                return 4;
            }

            int tx1 = x1;
            int ty1 = y1;
            int tx2 = x2;
            int ty2 = y2;
            if (f1 != 0)
            {
                if (!ClipMovePoint(tx1, ty1, tx2, ty2, clip_box, ref x1, ref y1, f1))
                {
                    return 4;
                }
                if (x1 == x2 && y1 == y2)
                {
                    return 4;
                }
                ret |= 1;
            }
            if (f2 != 0)
            {
                if (!ClipMovePoint(tx1, ty1, tx2, ty2, clip_box, ref x2, ref y2, f2))
                {
                    return 4;
                }
                if (x1 == x2 && y1 == y2)
                {
                    return 4;
                }
                ret |= 2;
            }
            return ret;
        }
    }
}


//#endif
