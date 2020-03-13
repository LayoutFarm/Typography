//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port) 

namespace Msdfgen
{
    public struct SignedDistance
    {
        public readonly double distance;
        public readonly double dot;

        public static readonly SignedDistance INFINITE = new SignedDistance(-1e240, 1);

        public SignedDistance(double dist, double d)
        {
            distance = dist;
            dot = d;
        }
        public static bool operator <(SignedDistance a, SignedDistance b)
        {
            return fabs(a.distance) < fabs(b.distance) || (fabs(a.distance) == fabs(b.distance) && a.dot < b.dot);
        }
        public static bool operator >(SignedDistance a, SignedDistance b)
        {
            return fabs(a.distance) > fabs(b.distance) || (fabs(a.distance) == fabs(b.distance) && a.dot > b.dot);
        }
        public static bool operator <=(SignedDistance a, SignedDistance b)
        {
            return fabs(a.distance) < fabs(b.distance) || (fabs(a.distance) == fabs(b.distance) && a.dot <= b.dot);
        }
        public static bool operator >=(SignedDistance a, SignedDistance b)
        {
            return fabs(a.distance) > fabs(b.distance) || (fabs(a.distance) == fabs(b.distance) && a.dot >= b.dot);
        }
        static double fabs(double d)
        {
            return EquationSolver.fabs(d);
        }
    }
}
