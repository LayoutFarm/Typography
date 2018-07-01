//MIT, 2014-present, WinterDev

namespace PixelFarm.Drawing
{
    public struct Point
    {
        int _x, _y;
        public Point(int x, int y)
        {
            this._x = x;
            this._y = y;
        }
        public void Offset(int dx, int dy)
        {
            this._x += dx;
            this._y += dy;
        }
        public int X
        {
            get { return this._x; }
            set { this._x = value; }
        }
        
        public int Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public static bool operator ==(Point p1, Point p2)
        {
            return p1._x == p2._x &&
                   p1._y == p2._y;
        }
        public static bool operator !=(Point p1, Point p2)
        {
            return p1._x != p2._x ||
                   p1._y != p2._y;
        }
        public override bool Equals(object obj)
        {
            Point p2 = (Point)obj;
            return this._x == p2._x &&
                   this._y == p2._y;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

       
        public int x { get { return this._x; } } //temp
        public int y { get { return this._y; } } //temp
        public static readonly Point Empty = new Point();
#if DEBUG
        public override string ToString()
        {
            return "(" + this.X + "," + this.Y + ")";
        }
#endif
    }

    public struct PointF
    {
        float _x, _y;
        public PointF(float x, float y)
        {
            this._x = x;
            this._y = y;
        }
        public float X
        {
            get { return this._x; }
            set { this._x = value; }
        }
        public float Y
        {
            get { return this._y; }
            set { this._y = value; }
        }
        public static implicit operator PointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }
        public bool IsEq(PointF p)
        {
            return this._x == p._x && this._y == p._y;
        }
    }

    public struct Size
    {
        int _w, _h;
        public Size(int w, int h)
        {
            this._w = w;
            this._h = h;
        }
        public int Width
        {
            get { return this._w; }
            set { this._w = value; }
        }
        public int Height
        {
            get { return this._h; }
            set { this._h = value; }
        }
        public static bool operator ==(Size s1, Size s2)
        {
            return (s1._w == s2._w) &&
                  (s1._h == s2._h);
        }
        public static bool operator !=(Size s1, Size s2)
        {
            return (s1._w != s2._w) ||
                  (s1._h != s2._h);
        }
        public override bool Equals(object obj)
        {
            Size s2 = (Size)obj;
            return (this._w == s2._w) &&
                   (this._h == s2._h);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static readonly Size Empty = new Size();
    }

    public struct SizeF
    {
        float _w, _h;
        public SizeF(float w, float h)
        {
            this._w = w;
            this._h = h;
        }
        public float Width
        {
            get { return this._w; }
        }
        public float Height
        {
            get { return this._h; }
        }
        public static implicit operator SizeF(Size p)
        {
            return new SizeF(p.Width, p.Height);
        }
    }
}