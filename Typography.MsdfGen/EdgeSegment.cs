//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017, WinterDev (C# port)
using System;

namespace Msdfgen
{
    public abstract class EdgeSegment
    {

        public const int MSDFGEN_CUBIC_SEARCH_STARTS = 4;
        public const int MSDFGEN_CUBIC_SEARCH_STEPS = 4;

        public EdgeColor color;
        public EdgeSegment(EdgeColor edgeColor)
        {
            this.color = edgeColor;
        }
        public abstract void findBounds(ref double left, ref double bottom, ref double right, ref double top);
        public void distanceToPsedoDistance(ref SignedDistance distance, Vector2 origin, double param)
        {
            if (param < 0)
            {
                Vector2 dir = direction(0).normalize();
                Vector2 aq = origin - point(0);
                double ts = Vector2.dotProduct(aq, dir);
                if (ts < 0)
                {
                    double pseudoDistance = Vector2.crossProduct(aq, dir);
                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance))
                    {
                        distance = new Msdfgen.SignedDistance(pseudoDistance, 0);
                    }
                }
            }
            else if (param > 1)
            {
                Vector2 dir = direction(1).normalize();
                Vector2 bq = origin - point(1);
                double ts = Vector2.dotProduct(bq, dir);
                if (ts > 0)
                {
                    double pseudoDistance = Vector2.crossProduct(bq, dir);
                    if (Math.Abs(pseudoDistance) <= Math.Abs(distance.distance))
                    {
                        distance = new Msdfgen.SignedDistance(pseudoDistance, 0);
                    }
                }
            }
        }

        public abstract Vector2 direction(double param);
        public abstract Vector2 point(double param);
        public abstract SignedDistance signedDistance(Vector2 origin, out double param);
        public abstract void splitInThirds(
          out EdgeSegment part1,
          out EdgeSegment part2,
          out EdgeSegment part3);

        protected static int nonZeroSign(double n)
        {
            return 2 * ((n > 0) ? 1 : 0) - 1;
        }
        protected static Vector2 mix(Vector2 a, Vector2 b, double weight)
        {
            //return T((S(1) - weight) * a + weight * b);
            //return T((S(1) - weight) * a + weight * b);
            return new Vector2(
                (1 - weight) * a.x + (weight * b.x),
                (1 - weight) * a.y + (weight * b.y));

        }

    }
    public class LinearSegment : EdgeSegment
    {
        Vector2[] p;
        public LinearSegment(Vector2 p0, Vector2 p1, EdgeColor edgeColor = EdgeColor.WHITE)
            : base(edgeColor)
        {
            p = new Vector2[2];
            p[0] = p0;
            p[1] = p1;
        }
        public override void findBounds(ref double left, ref double bottom, ref double right, ref double top)
        {
            Vector2.pointBounds(p[0], ref left, ref bottom, ref right, ref top);
            Vector2.pointBounds(p[1], ref left, ref bottom, ref right, ref top);
        }
        public override void splitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3)
        {
            part1 = new LinearSegment(p[0], point(1 / 3.0), this.color);
            part2 = new LinearSegment(point(1 / 3.0), point(2 / 3.0), this.color);
            part3 = new LinearSegment(point(2 / 3.0), p[1], this.color);
        }
        public override SignedDistance signedDistance(Vector2 origin, out double param)
        {
            Vector2 aq = origin - p[0];
            Vector2 ab = p[1] - p[0];
            param = Vector2.dotProduct(aq, ab) / Vector2.dotProduct(ab, ab);
            Vector2 eq = p[(param > .5) ? 1 : 0] - origin;
            double endpointDistance = eq.Length();
            if (param > 0 && param < 1)
            {
                double orthoDistance = Vector2.dotProduct(ab.getOrthoNormal(false), aq);
                if (Math.Abs(orthoDistance) < endpointDistance)
                {
                    return new SignedDistance(orthoDistance, 0);
                }
            }
            return new SignedDistance(
                nonZeroSign(Vector2.crossProduct(aq, ab)) * endpointDistance,
                Math.Abs(Vector2.dotProduct(ab.normalize(), eq.normalize())));
        }
        public override Vector2 direction(double param)
        {
            return p[1] - p[0];
        }
        public override Vector2 point(double param)
        {
            return mix(p[0], p[1], param);
        }
#if DEBUG
        public override string ToString()
        {
            return "L:" + this.color.ToString() + ":" + p[0].ToString() + "," + p[1].ToString();
        }
#endif
    }
    public class QuadraticSegment : EdgeSegment
    {
        Vector2[] p;
        public QuadraticSegment(Vector2 p0, Vector2 p1, Vector2 p2, EdgeColor edgeColor = EdgeColor.WHITE)
            : base(edgeColor)
        {
            p = new Vector2[3];
            if (Vector2.IsEq(p1, p0) || Vector2.IsEq(p1, p2))
            {
                p1 = 0.5 * (p0 + p2);
            }
            p[0] = p0;
            p[1] = p1;
            p[2] = p2;

        }
        public override void findBounds(ref double left, ref double bottom, ref double right, ref double top)
        {
            Vector2.pointBounds(p[0], ref left, ref bottom, ref right, ref top);
            Vector2.pointBounds(p[2], ref left, ref bottom, ref right, ref top);
            Vector2 bot = (p[1] - p[0]) - (p[2] - p[1]);
            if (bot.x != 0)
            {
                double param = (p[1].x - p[0].x) / bot.x;
                if (param > 0 && param < 1)
                    Vector2.pointBounds(point(param), ref left, ref bottom, ref right, ref top);
            }
            if (bot.y != 0)
            {
                double param = (p[1].y - p[0].y) / bot.y;
                if (param > 0 && param < 1)
                    Vector2.pointBounds(point(param), ref left, ref bottom, ref right, ref top);
            }
        }
        public override void splitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3)
        {
            part1 = new QuadraticSegment(p[0], mix(p[0], p[1], 1 / 3.0), point(1 / 3.0), this.color);
            part2 = new QuadraticSegment(point(1 / 3.0), mix(mix(p[0], p[1], 5 / 9.0), mix(p[1], p[2], 4 / 9.0), .5), point(2 / 3.0), this.color);
            part3 = new QuadraticSegment(point(2 / 3.0), mix(p[1], p[2], 2 / 3.0), p[2], this.color);
        }
        public override Vector2 direction(double param)
        {
            return mix(p[1] - p[0], p[2] - p[1], param);
        }
        public override Vector2 point(double param)
        {
            return mix(mix(p[0], p[1], param), mix(p[1], p[2], param), param);
        }
        public override SignedDistance signedDistance(Vector2 origin, out double param)
        {
            Vector2 qa = p[0] - origin;
            Vector2 ab = p[1] - p[0];
            Vector2 br = p[0] + p[2] - p[1] - p[1];
            double a = Vector2.dotProduct(br, br);
            double b = 3 * Vector2.dotProduct(ab, br);
            double c = 2 * Vector2.dotProduct(ab, ab) + Vector2.dotProduct(qa, br);
            double d = Vector2.dotProduct(qa, ab);
            double[] t = new double[3];
            int solutions = EquationSolver.SolveCubic(t, a, b, c, d);

            double minDistance = nonZeroSign(Vector2.crossProduct(ab, qa)) * qa.Length(); // distance from A
            param = -Vector2.dotProduct(qa, ab) / Vector2.dotProduct(ab, ab);
            {
                double distance = nonZeroSign(
                    Vector2.crossProduct(p[2] - p[1], p[2] - origin)) * (p[2] - origin).Length(); // distance from B
                if (Math.Abs(distance) < Math.Abs(minDistance))
                {
                    minDistance = distance;
                    param = Vector2.dotProduct(origin - p[1], p[2] - p[1]) / Vector2.dotProduct(p[2] - p[1], p[2] - p[1]);
                }
            }
            for (int i = 0; i < solutions; ++i)
            {
                if (t[i] > 0 && t[i] < 1)
                {
                    Vector2 endpoint = p[0] + 2 * t[i] * ab + t[i] * t[i] * br;
                    double distance = nonZeroSign(
                        Vector2.crossProduct(p[2] - p[0], endpoint - origin)) * (endpoint - origin).Length();
                    if (Math.Abs(distance) <= Math.Abs(minDistance))
                    {
                        minDistance = distance;
                        param = t[i];
                    }
                }
            }

            if (param >= 0 && param <= 1)
                return new SignedDistance(minDistance, 0);
            if (param < .5)
                return new SignedDistance(minDistance, Math.Abs(Vector2.dotProduct(ab.normalize(), qa.normalize())));
            else
                return new SignedDistance(minDistance, Math.Abs(Vector2.dotProduct((p[2] - p[1]).normalize(), (p[2] - origin).normalize())));
        }
#if DEBUG
        public override string ToString()
        {
            return "Q:" + this.color.ToString() + ":" + p[0].ToString() + "," + p[1].ToString() + "," + p[2].ToString();
        }
#endif
    }
    public class CubicSegment : EdgeSegment
    {
        Vector2[] p;
        public CubicSegment(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, EdgeColor edgeColor = EdgeColor.WHITE)
            : base(edgeColor)
        {
            p = new Vector2[4];
            p[0] = p0;
            p[1] = p1;
            p[2] = p2;
            p[3] = p3;
        }
        public override void findBounds(ref double left, ref double bottom, ref double right, ref double top)
        {
            Vector2.pointBounds(p[0], ref left, ref bottom, ref right, ref top);
            Vector2.pointBounds(p[3], ref left, ref bottom, ref right, ref top);
            Vector2 a0 = p[1] - p[0];
            Vector2 a1 = 2 * (p[2] - p[1] - a0);
            Vector2 a2 = p[3] - 3 * p[2] + 3 * p[1] - p[0];
            double[] pars = new double[2];
            int solutions;
            solutions = EquationSolver.SolveQuadratic(pars, a2.x, a1.x, a0.x);
            for (int i = 0; i < solutions; ++i)
                if (pars[i] > 0 && pars[i] < 1)
                    Vector2.pointBounds(point(pars[i]), ref left, ref bottom, ref right, ref top);
            solutions = EquationSolver.SolveQuadratic(pars, a2.y, a1.y, a0.y);
            for (int i = 0; i < solutions; ++i)
                if (pars[i] > 0 && pars[i] < 1)
                    Vector2.pointBounds(point(pars[i]), ref left, ref bottom, ref right, ref top);
        }
        public override void splitInThirds(out EdgeSegment part1, out EdgeSegment part2, out EdgeSegment part3)
        {
            part1 = new CubicSegment(p[0], Vector2.IsEq(p[0], p[1]) ? p[0] : mix(p[0], p[1], 1 / 3.0), mix(mix(p[0], p[1], 1 / 3.0), mix(p[1], p[2], 1 / 3.0), 1 / 3.0), point(1 / 3.0), color);
            part2 = new CubicSegment(point(1 / 3.0),
                mix(mix(mix(p[0], p[1], 1 / 3.0), mix(p[1], p[2], 1 / 3.0), 1 / 3.0), mix(mix(p[1], p[2], 1 / 3.0), mix(p[2], p[3], 1 / 3.0), 1 / 3.0), 2 / 3.0),
                mix(mix(mix(p[0], p[1], 2 / 3.0), mix(p[1], p[2], 2 / 3.0), 2 / 3.0), mix(mix(p[1], p[2], 2 / 3.0), mix(p[2], p[3], 2 / 3.0), 2 / 3.0), 1 / 3.0),
                point(2 / 3.0), color);
            part3 = new CubicSegment(point(2 / 3.0), mix(mix(p[1], p[2], 2 / 3.0), mix(p[2], p[3], 2 / 3.0), 2 / 3.0), Vector2.IsEq(p[2], p[3]) ? p[3] : mix(p[2], p[3], 2 / 3.0), p[3], color);
        }
        public override Vector2 direction(double param)
        {

            Vector2 tangent = mix(mix(p[1] - p[0], p[2] - p[1], param), mix(p[2] - p[1], p[3] - p[2], param), param);
            if (!tangent.IsZero())
            {
                if (param == 0) return p[2] - p[0];
                if (param == 1) return p[3] - p[1];
            }
            return tangent;
        }
        public override Vector2 point(double param)
        {
            Vector2 p12 = mix(p[1], p[2], param);
            return mix(mix(mix(p[0], p[1], param), p12, param), mix(p12, mix(p[2], p[3], param), param), param);
        }
        public override SignedDistance signedDistance(Vector2 origin, out double param)
        {
            Vector2 qa = p[0] - origin;
            Vector2 ab = p[1] - p[0];
            Vector2 br = p[2] - p[1] - ab;
            Vector2 as_ = (p[3] - p[2]) - (p[2] - p[1]) - br;
            Vector2 epDir = direction(0);

            double minDistance = nonZeroSign(Vector2.crossProduct(epDir, qa)) * qa.Length(); // distance from A
            param = -Vector2.dotProduct(qa, epDir) / Vector2.dotProduct(epDir, epDir);
            {
                epDir = direction(1);
                double distance = nonZeroSign(Vector2.crossProduct(epDir, p[3] - origin)) * (p[3] - origin).Length(); // distance from B
                if (EquationSolver.fabs(distance) < EquationSolver.fabs(minDistance))
                {
                    minDistance = distance;
                    param = Vector2.dotProduct(origin + epDir - p[3], epDir) / Vector2.dotProduct(epDir, epDir);
                }
            }
            // Iterative minimum distance search
            for (int i = 0; i <= MSDFGEN_CUBIC_SEARCH_STARTS; ++i)
            {
                double t = (double)((double)i / MSDFGEN_CUBIC_SEARCH_STARTS);
                for (int step = 0; ; ++step)
                {
                    Vector2 qpt = point(t) - origin;
                    double distance = nonZeroSign(Vector2.crossProduct(direction(t), qpt)) * qpt.Length();

                    if (EquationSolver.fabs(distance) < EquationSolver.fabs(minDistance))
                    {
                        minDistance = distance;
                        param = t;
                    }
                    if (step == MSDFGEN_CUBIC_SEARCH_STEPS)
                        break;
                    // Improve t
                    Vector2 d1 = 3 * as_ * t * t + 6 * br * t + 3 * ab;
                    Vector2 d2 = 6 * as_ * t + 6 * br;
                    t -= Vector2.dotProduct(qpt, d1) / (Vector2.dotProduct(d1, d1) + Vector2.dotProduct(qpt, d2));
                    if (t < 0 || t > 1)
                        break;
                }
            }

            if (param >= 0 && param <= 1)
                return new SignedDistance(minDistance, 0);
            if (param < .5)
                return new SignedDistance(minDistance,
                    EquationSolver.fabs(Vector2.dotProduct(direction(0), qa.normalize())));
            else
                return new SignedDistance(minDistance,
                    EquationSolver.fabs(Vector2.dotProduct(direction(1).normalize(), (p[3] - origin).normalize())));
        }
#if DEBUG
        public override string ToString()
        {
            return "C:" + this.color.ToString() + ":" + p[0].ToString() + "," + p[1].ToString() + "," + p[2].ToString() + "," + p[3].ToString();
        }
#endif
    }
}