//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port)

using System;
namespace Msdfgen
{
    //#include "equation-solver.h"


    public struct EqResult
    {
        public double x0, x1, x2;
        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return x0;
                    case 1: return x1;
                    case 2: return x2;
                    default: throw new NotSupportedException();
                }
            }
        }
    }

    public static class EquationSolver
    {
        public const double EPSILON = 1.0e-14;

        public static int SolveQuadratic(ref EqResult x, double a, double b, double c)
        {
            // ax^2 + bx + c = 0

            if (fabs(a) < EPSILON)
            {
                if (fabs(b) < EPSILON)
                {
                    if (c == 0)
                        return -1;
                    return 0;
                }
                x.x0 = -c / b;
                return 1;
            }
            double dscr = b * b - 4 * a * c;
            if (dscr > 0)
            {
                dscr = Math.Sqrt(dscr);
                x.x0 = (-b + dscr) / (2 * a);
                x.x1 = (-b - dscr) / (2 * a);
                return 2;
            }
            else if (dscr == 0)
            {
                x.x0 = -b / (2 * a);
                return 1;
            }
            else
            {
                return 0;
            }
        }

        static int SolveCubicNormed(ref EqResult x, double a, double b, double c)
        {
            double a2 = a * a;
            double q = (a2 - 3 * b) / 9;
            double r = (a * (2 * a2 - 9 * b) + 27 * c) / 54;
            double r2 = r * r;
            double q3 = q * q * q;
            double A, B;
            if (r2 < q3)
            {
                double t = r / Math.Sqrt(q3);
                if (t < -1) t = -1;
                if (t > 1) t = 1;
                t = Math.Acos(t);
                a /= 3; q = -2 * Math.Sqrt(q);
                x.x0 = q * Math.Cos(t / 3) - a;
                x.x1 = q * Math.Cos((t + 2 * Math.PI) / 3) - a;
                x.x2 = q * Math.Cos((t - 2 * Math.PI) / 3) - a;
                return 3;
            }
            else
            {
                A = -Math.Pow(fabs(r) + Math.Sqrt(r2 - q3), 1 / 3.0);
                if (r < 0) A = -A;
                B = A == 0 ? 0 : q / A;
                a /= 3;
                x.x0 = (A + B) - a;
                x.x1 = -0.5 * (A + B) - a;
                x.x2 = 0.5 * Math.Sqrt(3.0) * (A - B);
                if (fabs(x.x2) < EPSILON)
                    return 2;
                return 1;
            }
        }
        public static int SolveCubic(ref EqResult x/*3*/, double a, double b, double c, double d)
        {
            // ax^3 + bx^2 + cx + d = 0
            if (fabs(a) < EPSILON)
            {
                return SolveQuadratic(ref x, b, c, d);
            }
            return SolveCubicNormed(ref x, b / a, c / a, d / a);
        }
        public static double fabs(double m)
        {
            //TODO: review performance
            return Math.Abs(m);
        }
    }
}