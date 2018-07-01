//MIT, 2017-present, WinterDev
using System;
namespace PixelFarm.VectorMath
{
    public struct PointF
    {
        public float X;
        public float Y;
        public PointF(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public void Offset(float dx, float dy)
        {
            this.X += dx;
            this.Y += dy;
        }
    }
    public struct Point
    {
        public int x;
        public int y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public void Offset(int dx, int dy)
        {
            this.x += dx;
            this.y += dy;
        }
#if DEBUG
        public override string ToString()
        {
            return "(" + x + "," + y + ")";
        }
#endif
    }

    public static class MyVectorHelper
    {
        public static Vector NewFromPoint(PointF p)
        {
            return new Vector(p.X, p.Y);
        }

        /// <summary>
        /// create vector from start to end
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static Vector NewFromTwoPoints(PointF start, PointF end)
        {
            return new Vector(end.X - start.X, end.Y - start.Y);
        }

        public static bool IsClockwise(PointF pt1, PointF pt2, PointF pt3)
        {
            Vector V21 = NewFromTwoPoints(pt2, pt1);
            Vector v23 = NewFromTwoPoints(pt2, pt3);
            return V21.CrossProduct(v23) < 0; // sin(angle pt1 pt2 pt3) > 0, 0<angle pt1 pt2 pt3 <180
        }

        public static bool IsCCW(PointF pt1, PointF pt2, PointF pt3)
        {
            Vector V21 = NewFromTwoPoints(pt2, pt1);
            Vector v23 = NewFromTwoPoints(pt2, pt3);
            return V21.CrossProduct(v23) > 0;  // sin(angle pt2 pt1 pt3) < 0, 180<angle pt2 pt1 pt3 <360
        }

    }


    public struct Vector
    {
        readonly double _x, _y;
        public Vector(double x, double y)
        {
            _x = x; _y = y;
        }
        public Vector(Vector pt)
        {
            _x = pt.X;
            _y = pt.Y;
        }
        public Vector(Vector st, Vector end)
        {
            _x = end.X - st.X;
            _y = end.Y - st.Y;
        }

        public double X
        {
            get { return _x; }
            //set { _x = value; }
        }

        public double Y
        {
            get { return _y; }
            //set { _y = value; }
        }

        public double Magnitude
        {
            get { return Math.Sqrt(X * X + Y * Y); }
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.X + v2.X, v1.Y + v2.Y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.X - v2.X, v1.Y - v2.Y);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y);
        }

        public static Vector operator *(double c, Vector v)
        {
            return new Vector(c * v.X, c * v.Y);
        }

        public static Vector operator *(Vector v, double c)
        {
            return new Vector(c * v.X, c * v.Y);
        }

        public static Vector operator /(Vector v, double c)
        {
            return new Vector(v.X / c, v.Y / c);
        }

        // A * B =|A|.|B|.sin(angle AOB)
        public double CrossProduct(Vector v)
        {
            return _x * v.Y - v.X * _y;
        }

        // A. B=|A|.|B|.cos(angle AOB)
        public double DotProduct(Vector v)
        {
            return _x * v.X + _y * v.Y;
        }

        public static bool IsClockwise(Vector pt1, Vector pt2, Vector pt3)
        {
            Vector V21 = new Vector(pt2, pt1);
            Vector v23 = new Vector(pt2, pt3);
            return V21.CrossProduct(v23) < 0; // sin(angle pt1 pt2 pt3) > 0, 0<angle pt1 pt2 pt3 <180
        }

        public static bool IsCCW(Vector pt1, Vector pt2, Vector pt3)
        {
            Vector V21 = new Vector(pt2, pt1);
            Vector v23 = new Vector(pt2, pt3);
            return V21.CrossProduct(v23) > 0;  // sin(angle pt2 pt1 pt3) < 0, 180<angle pt2 pt1 pt3 <360
        }

        public static double DistancePointLine(Vector pt, Vector lnA, Vector lnB)
        {
            Vector v1 = new Vector(lnA, lnB);
            Vector v2 = new Vector(lnA, pt);
            v1 /= v1.Magnitude;
            return Math.Abs(v2.CrossProduct(v1));
        }

        public Vector Rotate(float degree)
        {
            //
            double radian = degree * (Math.PI / 180.0);
            double sin = Math.Sin(radian);
            double cos = Math.Cos(radian);
            double nx = _x * cos - _y * sin;
            double ny = _x * sin + _y * cos;
            return new Vector(nx, ny);
        }
        public Vector NewLength(double newLength)
        {
            //radian
            double atan = Math.Atan2(_y, _x);
            return new Vector(Math.Cos(atan) * newLength, Math.Sin(atan) * newLength);
        }

#if DEBUG
        public override string ToString()
        {
            return _x + "," + _y;
        }
#endif
    }

}