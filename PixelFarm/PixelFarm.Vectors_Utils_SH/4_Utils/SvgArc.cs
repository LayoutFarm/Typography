//MIT, 2014-present, WinterDev
using System;
namespace PixelFarm.CpuBlit.VertexProcessing
{
  
    public static class SvgPathSegArcInfo
    {
        public const double RAD_PER_DEG = Math.PI / 180.0;
        public const double DOUBLE_PI = Math.PI * 2;
        public static double CalculateVectorAngle(double ux, double uy, double vx, double vy)
        {
            double ta = Math.Atan2(uy, ux);
            double tb = Math.Atan2(vy, vx);
            if (tb >= ta)
            {
                return tb - ta;
            }

            return DOUBLE_PI - (ta - tb);
        }
    }
}