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

        public static Color Make(double r_, double g_, double b_, double a_)
        {
            return new Color(
               ((byte)CpuBlit.AggMath.uround(a_ * (double)BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(r_ * (double)BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(g_ * (double)BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(b_ * (double)BASE_MASK))
               );
        }
        public static Color Make(double r_, double g_, double b_)
        {
            return new Color(
               ((byte)CpuBlit.AggMath.uround(BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(r_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(g_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround(b_ * BASE_MASK)));
        }
        //------------------------------------------
        public static Color Make(float r_, float g_, float b_)
        {
            return new Color(
               ((byte)CpuBlit.AggMath.uround_f(BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(r_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(g_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(b_ * BASE_MASK))
              );
        }
        public static Color Make(float r_, float g_, float b_, float a_)
        {
            return new Color(
               ((byte)CpuBlit.AggMath.uround_f(a_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(r_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(g_ * BASE_MASK)),
               ((byte)CpuBlit.AggMath.uround_f(b_ * BASE_MASK))
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