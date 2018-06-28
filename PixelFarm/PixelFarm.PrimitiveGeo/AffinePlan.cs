//BSD, 2014-present, WinterDev
 
namespace PixelFarm.CpuBlit.VertexProcessing
{
    public enum AffineMatrixCommand : byte
    {
        None,
        Scale,
        Skew,
        Rotate,
        Translate,
        Invert
    }

    public struct AffinePlan
    {
        public readonly AffineMatrixCommand cmd;
        public readonly double x;
        public readonly double y;
        public AffinePlan(AffineMatrixCommand cmd, double x, double y)
        {
            this.x = x;
            this.y = y;
            this.cmd = cmd;
        }
        public AffinePlan(AffineMatrixCommand cmd, double x)
        {
            this.x = x;
            this.y = 0;
            this.cmd = cmd;
        }


        //----------------------------------------------------------------------------
        public static AffinePlan Translate(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Translate, x, y);
        }
        public static AffinePlan Rotate(double radAngle)
        {
            return new AffinePlan(AffineMatrixCommand.Rotate, radAngle);
        }
        public static AffinePlan Skew(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Skew, x, y);
        }
        public static AffinePlan Scale(double x, double y)
        {
            return new AffinePlan(AffineMatrixCommand.Scale, x, y);
        }
        public static AffinePlan Scale(double both)
        {
            return new AffinePlan(AffineMatrixCommand.Scale, both, both);
        }
    }
}
