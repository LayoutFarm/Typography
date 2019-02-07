//MIT, 2016, Viktor Chlumsky, Multi-channel signed distance field generator, from https://github.com/Chlumsky/msdfgen
//MIT, 2017-present, WinterDev (C# port)
using System;
using System.Collections.Generic;

namespace Msdfgen
{
    public struct Vector2
    {
        public readonly double x;
        public readonly double y;
        public Vector2(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
        public static bool IsEq(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public bool IsZero()
        {
            return x == 0 && y == 0;
        }
        public Vector2 getOrthoNormal(bool polarity = true, bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return polarity ? new Vector2(0, (!allowZero ? 1 : 0)) : new Vector2(0, -(!allowZero ? 1 : 0));
            }
            return polarity ? new Vector2(-y / len, x / len) : new Vector2(y / len, -x / len);
        }
        public Vector2 getOrthogonal(bool polarity = true)
        {
            return polarity ? new Vector2(-y, x) : new Vector2(y, -x);
        }
        public static double dotProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.x + a.y * b.y;
        }
        public static double crossProduct(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x - b.x,
                a.y - b.y);
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x + b.x,
                a.y + b.y);
        }
        public static Vector2 operator *(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x * b.x,
                a.y * b.y);
        }
        public static Vector2 operator /(Vector2 a, Vector2 b)
        {
            return new Vector2(
                a.x / b.x,
                a.y / b.y);
        }
        public static Vector2 operator *(Vector2 a, double n)
        {
            return new Vector2(
                a.x * n,
                a.y * n);
        }
        public static Vector2 operator /(Vector2 a, double n)
        {
            return new Vector2(
                a.x / n,
                a.y / n);
        }
        public static Vector2 operator *(double n, Vector2 a)
        {
            return new Vector2(
                a.x * n,
                a.y * n);
        }
        public static Vector2 operator /(double n, Vector2 a)
        {
            return new Vector2(
                a.x / n,
                a.y / n);
        }
        public Vector2 normalize(bool allowZero = false)
        {
            double len = Length();
            if (len == 0)
            {
                return new Vector2(0, !allowZero ? 1 : 0);
            }
            return new Vector2(x / len, y / len);
        }
        public double Length()
        {
            return Math.Sqrt(x * x + y * y);
        }
        /// <summary>
        /// Clamps the number to the interval from 0 to b.
        /// </summary>
        /// <returns></returns>
        public static int Clamp(int n, int b)
        {
            if (n > 0)
            {
                return (n <= b) ? n : b;
            }
            return 0;
        }
        /// <summary>
        /// Returns 1 for positive values, -1 for negative values, and 0 for zero.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static int sign(double n)
        {
            return (n == 0) ? 0 : (n > 0) ? 1 : -1;
        }

        public static double shoelace(Vector2 a, Vector2 b)
        {
            return (b.x - a.x) * (a.y + b.y);
        }
        public override string ToString()
        {
            return x + "," + y;
        }
        public static void pointBounds(Vector2 p, ref double l, ref double b, ref double r, ref double t)
        {
            if (p.x < l) l = p.x;
            if (p.y < b) b = p.y;
            if (p.x > r) r = p.x;
            if (p.y > t) t = p.y;
        }

    }
    public class Shape
    {
        public List<Contour> contours = new List<Contour>();
        public bool InverseYAxis { get; set; }
        public void normalized()
        {
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                Contour contour = contours[i];
                List<EdgeHolder> edges = contour.edges;
                if (edges.Count == 1)
                {
                    //TODO:
                    EdgeSegment e0, e1, e2;
                    edges[0].edgeSegment.splitInThirds(out e0, out e1, out e2);
                    edges.Clear();
                    edges.Add(new EdgeHolder(e0));
                    edges.Add(new EdgeHolder(e1));
                    edges.Add(new EdgeHolder(e2));

                }
            }
        }

        public void findBounds(out double left, out double bottom, out double right, out double top)
        {
            left = top = right = bottom = 0;
            int j = contours.Count;
            for (int i = 0; i < j; ++i)
            {
                contours[i].findBounds(ref left, ref bottom, ref right, ref top);
            }
        }
    }
    public class Contour
    {
        public List<EdgeHolder> edges = new List<EdgeHolder>();
        public void AddEdge(EdgeSegment edge)
        {
            EdgeHolder holder = new EdgeHolder(edge);
            edges.Add(holder);
        }
        public void AddLine(double x0, double y0, double x1, double y1)
        {
            this.AddEdge(new LinearSegment(new Vector2(x0, y0), new Vector2(x1, y1)));
        }
        public void AddQuadraticSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double x1, double y1)
        {
            this.AddEdge(new QuadraticSegment(
                new Vector2(x0, y0),
                new Vector2(ctrl0X, ctrl0Y),
                new Vector2(x1, y1)
                ));
        }
        public void AddCubicSegment(double x0, double y0,
            double ctrl0X, double ctrl0Y,
            double ctrl1X, double ctrl1Y,
            double x1, double y1)
        {
            this.AddEdge(new CubicSegment(
               new Vector2(x0, y0),
               new Vector2(ctrl0X, ctrl0Y),
               new Vector2(ctrl1X, ctrl1Y),
               new Vector2(x1, y1)
               ));
        }
        public void findBounds(ref double left, ref double bottom, ref double right, ref double top)
        {
            int j = edges.Count;
            for (int i = 0; i < j; ++i)
            {
                edges[i].edgeSegment.findBounds(ref left, ref bottom, ref right, ref top);
            }
        }
        public int winding()
        {
            int j = edges.Count;
            double total = 0;
            switch (j)
            {
                case 0: return 0;
                case 1:
                    {
                        Vector2 a = edges[0].point(0), b = edges[0].point(1 / 3.0), c = edges[0].point(2 / 3.0);
                        total += Vector2.shoelace(a, b);
                        total += Vector2.shoelace(b, c);
                        total += Vector2.shoelace(c, a);

                    }
                    break;
                case 2:
                    {
                        Vector2 a = edges[0].point(0), b = edges[0].point(0.5), c = edges[1].point(0), d = edges[1].point(0.5);
                        total += Vector2.shoelace(a, b);
                        total += Vector2.shoelace(b, c);
                        total += Vector2.shoelace(c, d);
                        total += Vector2.shoelace(d, a);
                    }
                    break;
                default:
                    {
                        Vector2 prev = edges[j - 1].point(0);
                        for (int i = 0; i < j; ++i)
                        {
                            Vector2 cur = edges[i].point(0);
                            total += Vector2.shoelace(prev, cur);
                            prev = cur;
                        }
                    }
                    break;
            }
            return Vector2.sign(total);

        }
    }
    public struct FloatRGB
    {
        public float r, g, b;
        public FloatRGB(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }
#if DEBUG
        public override string ToString()
        {
            return r + "," + g + "," + b;
        }
#endif
    }
    public struct Pair<T, U>
    {
        public T first;
        public U second;
        public Pair(T first, U second)
        {
            this.first = first;
            this.second = second;
        }
    }
    public class FloatBmp
    {
        float[] buffer;
        public FloatBmp(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            buffer = new float[w * h];
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public void SetPixel(int x, int y, float value)
        {
            this.buffer[x + (y * Width)] = value;
        }
        public float GetPixel(int x, int y)
        {
            return this.buffer[x + (y * Width)];
        }
    }
    public class FloatRGBBmp
    {
        FloatRGB[] buffer;
        public FloatRGBBmp(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            buffer = new FloatRGB[w * h];
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public void SetPixel(int x, int y, FloatRGB value)
        {
            this.buffer[x + (y * Width)] = value;
        }
        public FloatRGB GetPixel(int x, int y)
        {
            return this.buffer[x + (y * Width)];
        }
    }

    

}