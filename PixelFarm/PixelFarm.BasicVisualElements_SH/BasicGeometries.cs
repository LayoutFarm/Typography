//MIT, 2014-present, WinterDev

namespace PixelFarm.Drawing
{
    public readonly struct Point
    {
        public readonly int X, Y;
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1.X == p2.X &&
                   p1.Y == p2.Y;
        }
        public static bool operator !=(Point p1, Point p2)
        {
            return p1.X != p2.X ||
                   p1.Y != p2.Y;
        }
        public override bool Equals(object obj)
        {
            Point p2 = (Point)obj;
            return X == p2.X &&
                   Y == p2.Y;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static readonly Point Empty = new Point();

#if DEBUG
        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }
#endif
        public static Point Round(PointF p)
        {
            return new Point((int)System.Math.Round(p.X), (int)System.Math.Round(p.Y));
        }

    }

    public readonly struct PointF
    {
        public readonly float X;
        public readonly float Y;

        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }
        public static implicit operator PointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }
        public bool IsEq(PointF p)
        {
            return Y == p.Y && Y == p.Y;
        }

#if DEBUG
        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }
#endif
        public static readonly PointF Empty = new PointF();

        public bool IsEmpty => X == 0 && Y == 0;
    }

    public readonly struct PointD
    {
        public readonly double X;
        public readonly double Y;
        public PointD(double x, double y)
        {
            X = x;
            Y = y;
        }
        public bool IsEq(PointD p)
        {
            return X == p.X && Y == p.Y;
        }
        public static PointD OffsetPoint(PointD p, double dx, double dy)
        {
            return new PointD(p.X + dx, p.Y + dy);
        }

#if DEBUG
        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }
#endif
    }

    public struct Size
    {
        public readonly int Width, Height;

        public Size(int w, int h)
        {
            Width = w;
            Height = h;
        }

        public static bool operator ==(Size s1, Size s2)
        {
            return (s1.Width == s2.Width) &&
                  (s1.Height == s2.Height);
        }
        public static bool operator !=(Size s1, Size s2)
        {
            return (s1.Width != s2.Width) ||
                  (s1.Height != s2.Height);
        }
        public override bool Equals(object obj)
        {
            Size s2 = (Size)obj;
            return (Width == s2.Width) &&
                   (Height == s2.Height);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static readonly Size Empty = new Size();

#if DEBUG
        public override string ToString()
        {
            return "(" + Width + "," + Height + ")";
        }
#endif
        public static Size Round(SizeF s)
        {
            return new Size((int)System.Math.Round(s.Width), (int)System.Math.Round(s.Height));
        }
    }

    public struct SizeF
    {
        public float Width, Height;
        public SizeF(float w, float h)
        {
            Width = w;
            Height = h;
        }

        public static implicit operator SizeF(Size p)
        {
            return new SizeF(p.Width, p.Height);
        }

#if DEBUG
        public override string ToString()
        {
            return "(" + Width + "," + Height + ")";
        }
#endif

        public bool IsEmpty => Width == 0 && Height == 0;
        public static SizeF Empty => new SizeF();
    }
}