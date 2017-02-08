//BSD, 2014-2017, WinterDev
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
//#define USE_UNSAFE // no real code for this yet

using System;
namespace PixelFarm.Agg
{
    public static class AggBasics
    {
        public static bool is_equal_eps(double v1, double v2, double epsilon)
        {
            return Math.Abs(v1 - v2) <= (epsilon);
        }
        //------------------------------------------------------------------deg2rad
        public static double deg2rad(double deg)
        {
            return deg * Math.PI / 180.0;
        }

        //------------------------------------------------------------------rad2deg
        public static double rad2deg(double rad)
        {
            return rad * 180.0 / Math.PI;
        }

        public static int iround(double v)
        {
            unchecked
            {
                return (int)((v < 0.0) ? v - 0.5 : v + 0.5);
            }
        }
        public static int iround_f(float v)
        {
            unchecked
            {
                return (int)((v < 0.0) ? v - 0.5 : v + 0.5);
            }
        }
        public static int iround(double v, int saturationLimit)
        {
            if (v < (double)(-saturationLimit)) return -saturationLimit;
            if (v > (double)(saturationLimit)) return saturationLimit;
            return iround(v);
        }

        public static int uround(double v)
        {
            return (int)(uint)(v + 0.5);
        }
        public static int uround_f(float v)
        {
            return (int)(uint)(v + 0.5);
        }
        public static int ufloor(double v)
        {
            return (int)(uint)(v);
        }

        public static int uceil(double v)
        {
            return (int)(uint)(Math.Ceiling(v));
        }

        //----------------------------------------------------poly_subpixel_scale_e
        // These constants determine the subpixel accuracy, to be more precise, 
        // the number of bits of the fractional part of the coordinates. 
        // The possible coordinate capacity in bits can be calculated by formula:
        // sizeof(int) * 8 - poly_subpixel_shift, i.e, for 32-bit integers and
        // 8-bits fractional part the capacity is 24 bits.

        public static class PolySubPix
        {
            public const int SHIFT = 8;          //----poly_subpixel_shif
            public const int SCALE = 1 << SHIFT; //----poly_subpixel_scale 
            public const int MASK = SCALE - 1;  //----poly_subpixel_mask 
        }
    }
}
