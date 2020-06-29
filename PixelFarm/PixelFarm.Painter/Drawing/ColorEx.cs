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

using PixelFarm.CpuBlit;
using System;
namespace PixelFarm.Drawing
{

    /// <summary>
    /// Agg's Color Extension
    /// </summary>
    public static class ColorEx
    {
        public const int COVER_SHIFT = 8;
        public const int COVER_SIZE = 1 << COVER_SHIFT;  //----cover_size 
        public const int COVER_MASK = COVER_SIZE - 1;    //----cover_mask   
        public const int BASE_SHIFT = 8;
        public const int BASE_SCALE = (1 << BASE_SHIFT);
        public const int BASE_MASK = (BASE_SCALE - 1);
        //------------------------------------------
#if DEBUG
        static System.Random s_dbugRandom = new System.Random();
        public static Color dbugGetRandomColor() => Color.FromArgb(255, s_dbugRandom.Next(0, 255), s_dbugRandom.Next(0, 255), s_dbugRandom.Next(0, 255));
#endif
        public static Color Make(double r_, double g_, double b_, double a_)
        {
            
            return new Color(
               ((byte)AggMathRound.uround(a_ * (double)BASE_MASK)),
               ((byte)AggMathRound.uround(r_ * (double)BASE_MASK)),
               ((byte)AggMathRound.uround(g_ * (double)BASE_MASK)),
               ((byte)AggMathRound.uround(b_ * (double)BASE_MASK))
               );
        }
        public static Color Make(double r_, double g_, double b_)
        {
            return new Color(
               ((byte)AggMathRound.uround(BASE_MASK)),
               ((byte)AggMathRound.uround(r_ * BASE_MASK)),
               ((byte)AggMathRound.uround(g_ * BASE_MASK)),
               ((byte)AggMathRound.uround(b_ * BASE_MASK)));
        }
        //------------------------------------------
        public static Color Make(float r_, float g_, float b_)
        {
            return new Color(
               ((byte)AggMathRound.uround_f(BASE_MASK)),
               ((byte)AggMathRound.uround_f(r_ * BASE_MASK)),
               ((byte)AggMathRound.uround_f(g_ * BASE_MASK)),
               ((byte)AggMathRound.uround_f(b_ * BASE_MASK))
              );
        }
        public static Color Make(float r_, float g_, float b_, float a_)
        {
            return new Color(
               ((byte)AggMathRound.uround_f(a_ * BASE_MASK)),
               ((byte)AggMathRound.uround_f(r_ * BASE_MASK)),
               ((byte)AggMathRound.uround_f(g_ * BASE_MASK)),
               ((byte)AggMathRound.uround_f(b_ * BASE_MASK))
               );
        }
        public static Color Blend(this Color a, Color other, float weight)
        {
            return mul(a, (1 - weight)) + mul(other, weight);
        }
        public static Color mul(this Color A, float b)
        {
            float conv = b / 255f;
            return ColorEx.Make(A.R * conv, A.B * conv, A.B * conv, A.A * conv);
        }
        //------------------------------------------
        public static Color Make(int r_, int g_, int b_, int a_)
        {

            return new Color(
               (byte)Math.Min(Math.Max(a_, 0), 255), //clamp to 0-255
               (byte)Math.Min(Math.Max(r_, 0), 255),
               (byte)Math.Min(Math.Max(g_, 0), 255),
               (byte)Math.Min(Math.Max(b_, 0), 255)
               );
        }
    }
}