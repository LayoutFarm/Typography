//BSD, 2014-present, WinterDev
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
using PixelFarm.CpuBlit.FragmentProcessing;

namespace PixelFarm.CpuBlit.Rasterization.Lines
{
    public static class LineAA
    {
        public const int SUBPIXEL_SHIFT = 8;                          //----line_subpixel_shift
        public const int SUBPIXEL_SCALE = 1 << SUBPIXEL_SHIFT;  //----line_subpixel_scale
        public const int SUBPIXEL_MARK = SUBPIXEL_SCALE - 1;    //----line_subpixel_mask
        public const int SUBPIXEL_COORD = (1 << 28) - 1;              //----line_max_coord
        public const int MAX_LENGTH = 1 << (SUBPIXEL_SHIFT + 10); //----line_max_length
        public const int MR_SUBPIXEL_SHIFT = 4;                           //----line_mr_subpixel_shift
        public const int MR_SUBPIXEL_SCALE = 1 << MR_SUBPIXEL_SHIFT; //----line_mr_subpixel_scale 
        public const int MR_SUBPIXEL_MASK = MR_SUBPIXEL_SCALE - 1;   //----line_mr_subpixel_mask 
        /// <summary>
        /// line_mr
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int Mr(int x) => x >> (SUBPIXEL_SHIFT - MR_SUBPIXEL_SHIFT);

        /// <summary>
        /// line_hr
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int Hr(int x) => x << (SUBPIXEL_SHIFT - MR_SUBPIXEL_SHIFT);
        /// <summary>
        /// line_dbl_hr
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int DblHr(int x) => x << SUBPIXEL_SHIFT;


        public static void Bisectrix(in LineParameters l1,
                   in LineParameters l2,
                   out int x, out int y)
        {
            double k = (double)(l2.len) / (double)(l1.len);
            double tx = l2.x2 - (l2.x1 - l1.x1) * k;
            double ty = l2.y2 - (l2.y1 - l1.y1) * k;
            //All bisectrices must be on the right of the line
            //If the next point is on the left (l1 => l2.2)
            //then the bisectix should be rotated by 180 degrees.
            if ((double)(l2.x2 - l2.x1) * (double)(l2.y1 - l1.y1) <
               (double)(l2.y2 - l2.y1) * (double)(l2.x1 - l1.x1) + 100.0)
            {
                tx -= (tx - l2.x1) * 2.0;
                ty -= (ty - l2.y1) * 2.0;
            }

            // Check if the bisectrix is too short
            double dx = tx - l2.x1;
            double dy = ty - l2.y1;
            if ((int)Math.Sqrt(dx * dx + dy * dy) < SUBPIXEL_SCALE)
            {
                x = (l2.x1 + l2.x1 + (l2.y1 - l1.y1) + (l2.y2 - l2.y1)) >> 1;
                y = (l2.y1 + l2.y1 - (l2.x1 - l1.x1) - (l2.x2 - l2.x1)) >> 1;
                return;
            }

            x = AggMath.iround(tx);
            y = AggMath.iround(ty);
        }
        /// <summary>
        /// fix_degeneration_bisectrix_start
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void FixDegenBisectrixStart(in LineParameters lp,
                                               ref int x, ref int y)
        {
            int d = AggMath.iround(((double)(x - lp.x2) * (double)(lp.y2 - lp.y1) -
                            (double)(y - lp.y2) * (double)(lp.x2 - lp.x1)) / lp.len);
            if (d < SUBPIXEL_SCALE / 2)
            {
                x = lp.x1 + (lp.y2 - lp.y1);
                y = lp.y1 - (lp.x2 - lp.x1);
            }
        }
        /// <summary>
        /// fix_degeneration_bisectrix_end
        /// </summary>
        /// <param name="lp"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public static void FixDegenBisectrixEnd(in LineParameters lp,
                                             ref int x, ref int y)
        {
            int d = AggMath.iround(((double)(x - lp.x2) * (double)(lp.y2 - lp.y1) -
                            (double)(y - lp.y2) * (double)(lp.x2 - lp.x1)) / lp.len);
            if (d < SUBPIXEL_SCALE / 2)
            {
                x = lp.x2 + (lp.y2 - lp.y1);
                y = lp.y2 - (lp.x2 - lp.x1);
            }
        }
    }


    static class LineAADataPool
    {
        [ThreadStatic]
        static Stack<int[]> s_freeDistPool;
        [ThreadStatic]
        static Stack<byte[]> s_freeConvPool;

        /// <summary>
        /// get reusable distance array
        /// </summary>
        /// <returns></returns>
        internal static int[] GetFreeDistArray()
        {
            if (s_freeDistPool == null) s_freeDistPool = new Stack<int[]>();

            if (s_freeDistPool.Count > 0)
            {
                return s_freeDistPool.Pop();
            }
            else
            {
                //m_dist = new int[MAX_HALF_WIDTH + 1];
                //m_covers = new byte[MAX_HALF_WIDTH * 2 + 4];
                return new int[LineInterpolatorAAData.MAX_HALF_WIDTH + 1];
            }
        }
        internal static void ReleaseDistArray(int[] distArray)
        {
            //clear and add to list
            Array.Clear(distArray, 0, distArray.Length);
            s_freeDistPool.Push(distArray);
        }
        /// <summary>
        /// get reuseable converate-area array
        /// </summary>
        /// <returns></returns>
        internal static byte[] GetFreeConvArray()
        {
            if (s_freeConvPool == null) s_freeConvPool = new Stack<byte[]>();
            if (s_freeConvPool.Count > 0)
            {
                return s_freeConvPool.Pop();
            }
            else
            {   
                return new byte[(OutlineRenderer.MAX_HALF_WIDTH + 1) * 2];
            }
        }
        internal static void ReleaseConvArray(byte[] convArray)
        {
            //clear and add to list
            Array.Clear(convArray, 0, convArray.Length);
            s_freeConvPool.Push(convArray);
        }

        [ThreadStatic]
        static Stack<LineInterpolatorDDA2> s_freeInterpolatorDDA2Pool;
        internal static LineInterpolatorDDA2 GetFreeInterpolatorDDA2()
        {
            if (s_freeInterpolatorDDA2Pool == null) s_freeInterpolatorDDA2Pool = new Stack<LineInterpolatorDDA2>();

            if (s_freeInterpolatorDDA2Pool.Count > 0)
            {
                return s_freeInterpolatorDDA2Pool.Pop();
            }
            else
            {
                return new LineInterpolatorDDA2();
            }
        }
        internal static void ReleaseInterpolatorDDA2(LineInterpolatorDDA2 dda2)
        {
            s_freeInterpolatorDDA2Pool.Push(dda2);
        }

       
    }

}