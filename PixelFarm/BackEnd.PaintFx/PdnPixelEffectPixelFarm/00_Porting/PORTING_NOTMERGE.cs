//Apache2, 2017-present, WinterDev
//this file is not merged on merged project.
//
using System;
using System.Runtime.InteropServices;

namespace PixelFarm.Drawing
{
    

    public class StillNotPortedException : Exception { }



    public struct Color
    {
        byte _r, _g, _b, _a;
        public Color(byte a, byte r, byte g, byte b)
        {
            this._r = r;
            this._g = g;
            this._b = b;
            this._a = a;
        }
        public Color(byte r, byte g, byte b)
        {
            this._r = r;
            this._g = g;
            this._b = b;
            this._a = 255;
        }
        public byte R
        {
            get { return this._r; }
        }
        public byte G
        {
            get { return this._g; }
        }
        public byte B
        {
            get { return this._b; }
        }
        public byte A
        {
            get { return this._a; }
        }
        public byte alpha
        {
            get { return this._a; }

        }

        public byte red { get { return this._r; } }
        public byte green { get { return this._g; } }
        public byte blue { get { return this._b; } }


        public static Color FromArgb(int a, Color c)
        {
            return new Color((byte)a, c.R, c.G, c.B);
        }
        public static Color FromArgb(int a, int r, int g, int b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(int r, int g, int b)
        {
            return new Color(255, (byte)r, (byte)g, (byte)b);
        }
        public static Color FromArgb(float a, float r, float g, float b)
        {
            return new Color((byte)a, (byte)r, (byte)g, (byte)b);
        }
        public override bool Equals(object obj)
        {
            if (obj is Color)
            {
                Color c = (Color)obj;
                return c._a == this._a &&
                    c._b == this._b &&
                    c._r == this._r &&
                    c._g == this._g;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static readonly Color Empty = new Color(0, 0, 0, 0);
        public static readonly Color Transparent = new Color(0, 255, 255, 255);
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(255, 0, 0, 0);
        public static readonly Color Blue = new Color(255, 0, 0, 255);
        public static readonly Color Aqua = new Color(255, 0, 255, 255);
        public static readonly Color Red = new Color(255, 255, 0, 0);
        public static readonly Color Yellow = new Color(255, 255, 255, 0);
        public static readonly Color LightGray = new Color(0xFF, 0xD3, 0xD3, 0xD3);
        public static readonly Color Gray = new Color(0xFF, 0x80, 0x80, 0x80);
        public static readonly Color Green = new Color(0xFF, 0x00, 0x80, 0x00);
        public static readonly Color OrangeRed = new Color(0xFF, 0xFF, 0x45, 0x00);//0xFF FF 45 00
        public static readonly Color DeepPink = new Color(0xFF, 0xFF, 0x14, 0x93);
        public static readonly Color Magenta = new Color(0xFF, 0xFF, 0, 0xFF);


        public static bool operator ==(Color c1, Color c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) ==
                   (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public static bool operator !=(Color c1, Color c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) !=
                  (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public int ToARGB()
        {
            return ((this._a << 24) | (this._r << 16) | (this._g << 8) | this._b);
        }
        public uint ToABGR()
        {
            return (uint)((this._a << 24) | (this._b << 16) | (this._g << 8) | this._r);
        }

        public byte Red0To255
        {
            get { return _r; }
        }
        public byte Green0To255
        {
            get { return _g; }
        }
        public byte Blue0To255
        {
            get { return _b; }
        }
        public byte Alpha0To255
        {
            get { return _a; }
        }




        public Color CreateGradient(Color another, float colorDistanceRatio)
        {
            //int ik = AggBasics.uround(colorDistanceRatio * BASE_SCALE); 
            //byte r = (byte)((int)(Red0To255) + ((((int)(another.Red0To255) - Red0To255) * ik) >> BASE_SHIFT));
            //byte g = (byte)((int)(Green0To255) + ((((int)(another.Green0To255) - Green0To255) * ik) >> BASE_SHIFT));
            //byte b = (byte)((int)(Blue0To255) + ((((int)(another.Blue0To255) - Blue0To255) * ik) >> BASE_SHIFT));
            //byte a = (byte)((int)(Alpha0To255) + ((((int)(another.Alpha0To255) - Alpha0To255) * ik) >> BASE_SHIFT));

            //from this color to another c color
            //colorDistance ratio [0-1]
            //new_color = old_color + diff

            byte r = (byte)(Red0To255 + (another.Red0To255 - this.Red0To255) * colorDistanceRatio);
            byte g = (byte)(Green0To255 + (another.Green0To255 - this.Green0To255) * colorDistanceRatio);
            byte b = (byte)(Blue0To255 + (another.Blue0To255 - this.Blue0To255) * colorDistanceRatio);
            byte a = (byte)(Alpha0To255 + (another.Alpha0To255 - this.Alpha0To255) * colorDistanceRatio);
            return new Color(a, r, g, b);
        }

        public static Color operator +(Color A, Color B)
        {
            byte r = (byte)((A._r + B._r) > 255 ? 255 : (A._r + B._r));
            byte g = (byte)((A._g + B._g) > 255 ? 255 : (A._g + B._g));
            byte b = (byte)((A._b + B._b) > 255 ? 255 : (A._b + B._b));
            byte a = (byte)((A._a + B._a) > 255 ? 255 : (A._a + B._a));
            return new Color(a, r, g, b);
        }

        public static Color operator -(Color A, Color B)
        {
            byte red = (byte)((A._r - B._r) < 0 ? 0 : (A._r - B._r));
            byte green = (byte)((A._g - B._g) < 0 ? 0 : (A._g - B._g));
            byte blue = (byte)((A._b - B._b) < 0 ? 0 : (A._b - B._b));
            byte alpha = (byte)((A._a - B._a) < 0 ? 0 : (A._a - B._a));
            return new Color(alpha, red, green, blue);
        }

        /// <summary>
        /// rgb= original rgb
        /// alpha= (byte)((color.alpha * (cover) + 255) >> 8);
        /// </summary>
        /// <param name="cover"></param>
        /// <returns></returns>
        public Color NewFromChangeCoverage(int cover)
        {
            return new Color(
                (byte)((_a * cover + 255) >> 8),
                _r, _g, _b);
        }
        /// <summary>
        /// new color from changing the alpha value
        /// </summary>
        /// <param name="alpha"></param>
        /// <returns></returns>
        public Color NewFromChangeAlpha(byte alpha)
        {
            return new Color(
                 alpha,
                _r, _g, _b);
        }
        //public void AddColor(ColorRGBA c, int cover)
        //{
        //    int cr, cg, cb, ca;
        //    if (cover == COVER_MASK)
        //    {
        //        if (c.Alpha0To255 == BASE_MASK)
        //        {
        //            this = c;
        //        }
        //        else
        //        {
        //            cr = Red0To255 + c.Red0To255; Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //            cg = Green0To255 + c.Green0To255; Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //            cb = Blue0To255 + c.Blue0To255; Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //            ca = Alpha0To255 + c.Alpha0To255; Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //        }
        //    }
        //    else
        //    {
        //        cr = Red0To255 + ((c.Red0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cg = Green0To255 + ((c.Green0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        cb = Blue0To255 + ((c.Blue0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        ca = Alpha0To255 + ((c.Alpha0To255 * cover + COVER_MASK / 2) >> COVER_SHIFT);
        //        Red0To255 = (cr > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cr;
        //        Green0To255 = (cg > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cg;
        //        Blue0To255 = (cb > (int)(BASE_MASK)) ? (int)(BASE_MASK) : cb;
        //        Alpha0To255 = (ca > (int)(BASE_MASK)) ? (int)(BASE_MASK) : ca;
        //    }
        //}

        //public void ApplyGammaDir(GammaLookUpTable gamma)
        //{
        //    Red0To255 = gamma.dir((byte)Red0To255);
        //    Green0To255 = gamma.dir((byte)Green0To255);
        //    Blue0To255 = gamma.dir((byte)Blue0To255);
        //}

        //-------------------------------------------------------------rgb8_packed
        static public Color CreatRGB8Packed(int v)
        {
            //argb
            return new Color(255, (byte)((v >> 16) & 0xFF), (byte)((v >> 8) & 0xFF), ((byte)(v & 0xFF)));
        }


#if DEBUG
        public override string ToString()
        {
            return "r:" + this._r + ",g:" + this._g + ",b:" + this._b + ",a:" + this._a;
        }
#endif
    }

    public struct Rectangle
    {
        private int x, y, width, height;
        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized Rectangle Structure.
        /// </remarks>

        public static readonly Rectangle Empty;
        /// <summary>
        ///	Ceiling Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Rectangle structure from a RectangleF 
        ///	structure by taking the ceiling of the X, Y, Width,
        ///	and Height properties.
        /// </remarks>

        public static Rectangle Ceiling(RectangleF value)
        {
            int x, y, w, h;
            checked
            {
                x = (int)Math.Ceiling(value.X);
                y = (int)Math.Ceiling(value.Y);
                w = (int)Math.Ceiling(value.Width);
                h = (int)Math.Ceiling(value.Height);
            }

            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///	FromLTRB Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Rectangle structure from left, top, right,
        ///	and bottom coordinates.
        /// </remarks>

        public static Rectangle FromLTRB(int left, int top,
                          int right, int bottom)
        {
            return new Rectangle(left, top, right - left,
                          bottom - top);
        }

        /// <summary>
        ///	Inflate Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new Rectangle by inflating an existing 
        ///	Rectangle by the specified coordinate values.
        /// </remarks>

        public static Rectangle Inflate(Rectangle rect, int x, int y)
        {
            Rectangle r = new Rectangle(rect.Location, rect.Size);
            r.Inflate(x, y);
            return r;
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the Rectangle by a specified width and height.
        /// </remarks>

        public void Inflate(int width, int height)
        {
            Inflate(new Size(width, height));
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the Rectangle by a specified Size.
        /// </remarks>

        public void Inflate(Size size)
        {
            x -= size.Width;
            y -= size.Height;
            Width += size.Width * 2;
            Height += size.Height * 2;
        }

        /// <summary>
        ///	Intersect Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new Rectangle by intersecting 2 existing 
        ///	Rectangles. Returns null if there is no	intersection.
        /// </remarks>

        public static Rectangle Intersect(Rectangle a, Rectangle b)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            if (!a.IntersectsWithInclusive(b))
                return Empty;
            return Rectangle.FromLTRB(
                Math.Max(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Min(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Intersect Method
        /// </summary>
        ///
        /// <remarks>
        ///	Replaces the Rectangle with the intersection of itself
        ///	and another Rectangle.
        /// </remarks>

        public void Intersect(Rectangle rect)
        {
            this = Rectangle.Intersect(this, rect);
        }

        /// <summary>
        ///	Round Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Rectangle structure from a RectangleF by
        ///	rounding the X, Y, Width, and Height properties.
        /// </remarks>

        public static Rectangle Round(RectangleF value)
        {
            int x, y, w, h;
            checked
            {
                x = (int)Math.Round(value.X);
                y = (int)Math.Round(value.Y);
                w = (int)Math.Round(value.Width);
                h = (int)Math.Round(value.Height);
            }

            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///	Truncate Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a Rectangle structure from a RectangleF by
        ///	truncating the X, Y, Width, and Height properties.
        /// </remarks>

        // LAMESPEC: Should this be floor, or a pure cast to int?

        public static Rectangle Truncate(RectangleF value)
        {
            int x, y, w, h;
            checked
            {
                x = (int)value.X;
                y = (int)value.Y;
                w = (int)value.Width;
                h = (int)value.Height;
            }

            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///	Union Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new Rectangle from the union of 2 existing 
        ///	Rectangles.
        /// </remarks>

        public static Rectangle Union(Rectangle a, Rectangle b)
        {
            return FromLTRB(Math.Min(a.Left, b.Left),
                     Math.Min(a.Top, b.Top),
                     Math.Max(a.Right, b.Right),
                     Math.Max(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Rectangle objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two Rectangles.
        /// </remarks>

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return ((left.Location == right.Location) &&
                (left.Size == right.Size));
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two Rectangle objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two Rectangles.
        /// </remarks>

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return ((left.Location != right.Location) ||
                (left.Size != right.Size));
        }


        // -----------------------
        // Public Constructors
        // -----------------------

        /// <summary>
        ///	Rectangle Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a Rectangle from Point and Size values.
        /// </remarks>

        public Rectangle(Point location, Size size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }

        /// <summary>
        ///	Rectangle Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a Rectangle from a specified x,y location and
        ///	width and height values.
        /// </remarks>

        public Rectangle(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }



        /// <summary>
        ///	Bottom Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the bottom edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Bottom
        {
            get
            {
                return y + height;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height of the Rectangle.
        /// </remarks>

        public int Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if the width or height are zero. Read only.
        /// </remarks>		

        public bool IsEmpty
        {
            get
            {
                return ((x == 0) && (y == 0) && (width == 0) && (height == 0));
            }
        }

        /// <summary>
        ///	Left Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the left edge of the Rectangle.
        ///	Read only.
        /// </remarks>

        public int Left
        {
            get
            {
                return X;
            }
        }

        /// <summary>
        ///	Location Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Location of the top-left corner of the Rectangle.
        /// </remarks>


        public Point Location
        {
            get
            {
                return new Point(x, y);
            }
            set
            {
                x = value.X;
                y = value.Y;
            }
        }

        /// <summary>
        ///	Right Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the right edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Right
        {
            get
            {
                return X + Width;
            }
        }

        /// <summary>
        ///	Size Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Size of the Rectangle.
        /// </remarks>


        public Size Size
        {
            get
            {
                return new Size(Width, Height);
            }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        /// <summary>
        ///	Top Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the top edge of the Rectangle.
        ///	Read only.
        /// </remarks>


        public int Top
        {
            get
            {
                return y;
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width of the Rectangle.
        /// </remarks>

        public int Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the Rectangle.
        /// </remarks>

        public int X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the Rectangle.
        /// </remarks>

        public int Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if an x,y coordinate lies within this Rectangle.
        /// </remarks>

        public bool Contains(int x, int y)
        {
            return ((x >= Left) && (x < Right) &&
                (y >= Top) && (y < Bottom));
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Point lies within this Rectangle.
        /// </remarks>

        public bool Contains(Point pt)
        {
            return Contains(pt.X, pt.Y);
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Rectangle lies entirely within this 
        ///	Rectangle.
        /// </remarks>

        public bool Contains(Rectangle rect)
        {
            return (rect == Intersect(this, rect));
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this Rectangle and another object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is Rectangle))
                return false;
            return (this == (Rectangle)obj);
        }

        /// <summary>
        ///	GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///	Calculates a hashing value.
        /// </remarks>

        public override int GetHashCode()
        {
            return (height + width) ^ x + y;
        }

        /// <summary>
        ///	IntersectsWith Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Rectangle intersects with this one.
        /// </remarks>

        public bool IntersectsWith(Rectangle rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
                (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }
        public bool IntersectsWith(int left, int top, int right, int bottom)
        {
            if (((this.Left <= left) && (this.Right > left)) || ((this.Left >= left) && (this.Left < right)))
            {
                if (((this.Top <= top) && (this.Bottom > top)) || ((this.Top >= top) && (this.Top < bottom)))
                {
                    return true;
                }
            }
            return false;
        }
        private bool IntersectsWithInclusive(Rectangle r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the Rectangle a specified distance.
        /// </remarks>

        public void Offset(int x, int y)
        {
            this.x += x;
            this.y += y;
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the Rectangle a specified distance.
        /// </remarks>

        public void Offset(Point pos)
        {
            x += pos.X;
            y += pos.Y;
        }
        public void OffsetX(int dx)
        {
            x += dx;
        }
        public void OffsetY(int dy)
        {
            y += dy;
        }
        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the Rectangle as a string in (x,y,w,h) notation.
        /// </remarks>

        public override string ToString()
        {
            return String.Format("{{X={0},Y={1},Width={2},Height={3}}}",
                         x, y, width, height);
        }
    }
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

   
  
    public struct RectangleF
    {
        float x, y, width, height;
        /// <summary>
        ///	Empty Shared Field
        /// </summary>
        ///
        /// <remarks>
        ///	An uninitialized RectangleF Structure.
        /// </remarks>

        public static readonly RectangleF Empty;
        /// <summary>
        ///	FromLTRB Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a RectangleF structure from left, top, right,
        ///	and bottom coordinates.
        /// </remarks>

        public static RectangleF FromLTRB(float left, float top,
                           float right, float bottom)
        {
            return new RectangleF(left, top, right - left, bottom - top);
        }

        /// <summary>
        ///	Inflate Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF by inflating an existing 
        ///	RectangleF by the specified coordinate values.
        /// </remarks>

        public static RectangleF Inflate(RectangleF rect,
                          float x, float y)
        {
            RectangleF ir = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
            ir.Inflate(x, y);
            return ir;
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the RectangleF by a specified width and height.
        /// </remarks>

        public void Inflate(float x, float y)
        {
            Inflate(new SizeF(x, y));
        }

        /// <summary>
        ///	Inflate Method
        /// </summary>
        ///
        /// <remarks>
        ///	Inflates the RectangleF by a specified Size.
        /// </remarks>

        public void Inflate(SizeF size)
        {
            x -= size.Width;
            y -= size.Height;
            width += size.Width * 2;
            height += size.Height * 2;
        }

        /// <summary>
        ///	Intersect Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF by intersecting 2 existing 
        ///	RectangleFs. Returns null if there is no intersection.
        /// </remarks>

        public static RectangleF Intersect(RectangleF a,
                            RectangleF b)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            if (!a.IntersectsWithInclusive(b))
                return Empty;
            return FromLTRB(
                Math.Max(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Min(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Intersect Method
        /// </summary>
        ///
        /// <remarks>
        ///	Replaces the RectangleF with the intersection of itself
        ///	and another RectangleF.
        /// </remarks>

        public void Intersect(RectangleF rect)
        {
            this = RectangleF.Intersect(this, rect);
        }

        /// <summary>
        ///	Union Shared Method
        /// </summary>
        ///
        /// <remarks>
        ///	Produces a new RectangleF from the union of 2 existing 
        ///	RectangleFs.
        /// </remarks>

        public static RectangleF Union(RectangleF a, RectangleF b)
        {
            return FromLTRB(Math.Min(a.Left, b.Left),
                     Math.Min(a.Top, b.Top),
                     Math.Max(a.Right, b.Right),
                     Math.Max(a.Bottom, b.Bottom));
        }

        /// <summary>
        ///	Equality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two RectangleF objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two RectangleFs.
        /// </remarks>

        public static bool operator ==(RectangleF left, RectangleF right)
        {
            return (left.X == right.X) && (left.Y == right.Y) &&
                                (left.Width == right.Width) && (left.Height == right.Height);
        }

        /// <summary>
        ///	Inequality Operator
        /// </summary>
        ///
        /// <remarks>
        ///	Compares two RectangleF objects. The return value is
        ///	based on the equivalence of the Location and Size 
        ///	properties of the two RectangleFs.
        /// </remarks>

        public static bool operator !=(RectangleF left, RectangleF right)
        {
            return (left.X != right.X) || (left.Y != right.Y) ||
                                (left.Width != right.Width) || (left.Height != right.Height);
        }

        /// <summary>
        ///	Rectangle to RectangleF Conversion
        /// </summary>
        ///
        /// <remarks>
        ///	Converts a Rectangle object to a RectangleF.
        /// </remarks>

        public static implicit operator RectangleF(Rectangle r)
        {
            return new RectangleF(r.X, r.Y, r.Width, r.Height);
        }


        // -----------------------
        // Public Constructors
        // -----------------------

        /// <summary>
        ///	RectangleF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a RectangleF from PointF and SizeF values.
        /// </remarks>

        public RectangleF(PointF location, SizeF size)
        {
            x = location.X;
            y = location.Y;
            width = size.Width;
            height = size.Height;
        }

        /// <summary>
        ///	RectangleF Constructor
        /// </summary>
        ///
        /// <remarks>
        ///	Creates a RectangleF from a specified x,y location and
        ///	width and height values.
        /// </remarks>

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
        /// <summary>
        ///	Bottom Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the bottom edge of the RectangleF.
        ///	Read only.
        /// </remarks> 

        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        /// <summary>
        ///	Height Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Height of the RectangleF.
        /// </remarks>

        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
            }
        }

        /// <summary>
        ///	IsEmpty Property
        /// </summary>
        ///
        /// <remarks>
        ///	Indicates if the width or height are zero. Read only.
        /// </remarks>
        //		

        public bool IsEmpty
        {
            get
            {
                return (width <= 0 || height <= 0);
            }
        }

        /// <summary>
        ///	Left Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the left edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Left
        {
            get
            {
                return X;
            }
        }

        /// <summary>
        ///	Location Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Location of the top-left corner of the RectangleF.
        /// </remarks>


        public PointF Location
        {
            get
            {
                return new PointF(x, y);
            }
            set
            {
                x = value.X;
                y = value.Y;
            }
        }

        /// <summary>
        ///	Right Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the right edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Right
        {
            get
            {
                return X + Width;
            }
        }

        /// <summary>
        ///	Size Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Size of the RectangleF.
        /// </remarks>


        public SizeF Size
        {
            get
            {
                return new SizeF(width, height);
            }
            set
            {
                width = value.Width;
                height = value.Height;
            }
        }

        /// <summary>
        ///	Top Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the top edge of the RectangleF.
        ///	Read only.
        /// </remarks>


        public float Top
        {
            get
            {
                return Y;
            }
        }

        /// <summary>
        ///	Width Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Width of the RectangleF.
        /// </remarks>

        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }

        /// <summary>
        ///	X Property
        /// </summary>
        ///
        /// <remarks>
        ///	The X coordinate of the RectangleF.
        /// </remarks>

        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }

        /// <summary>
        ///	Y Property
        /// </summary>
        ///
        /// <remarks>
        ///	The Y coordinate of the RectangleF.
        /// </remarks>

        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if an x,y coordinate lies within this RectangleF.
        /// </remarks>

        public bool Contains(float x, float y)
        {
            return ((x >= Left) && (x < Right) &&
                (y >= Top) && (y < Bottom));
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a Point lies within this RectangleF.
        /// </remarks>

        public bool Contains(PointF pt)
        {
            return Contains(pt.X, pt.Y);
        }

        /// <summary>
        ///	Contains Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a RectangleF lies entirely within this 
        ///	RectangleF.
        /// </remarks>

        public bool Contains(RectangleF rect)
        {
            return X <= rect.X && Right >= rect.Right && Y <= rect.Y && Bottom >= rect.Bottom;
        }

        /// <summary>
        ///	Equals Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks equivalence of this RectangleF and an object.
        /// </remarks>

        public override bool Equals(object obj)
        {
            if (!(obj is RectangleF))
                return false;
            return (this == (RectangleF)obj);
        }

        /// <summary>
        ///	GetHashCode Method
        /// </summary>
        ///
        /// <remarks>
        ///	Calculates a hashing value.
        /// </remarks>

        public override int GetHashCode()
        {
            return (int)(x + y + width + height);
        }

        /// <summary>
        ///	IntersectsWith Method
        /// </summary>
        ///
        /// <remarks>
        ///	Checks if a RectangleF intersects with this one.
        /// </remarks>

        public bool IntersectsWith(RectangleF rect)
        {
            return !((Left >= rect.Right) || (Right <= rect.Left) ||
                (Top >= rect.Bottom) || (Bottom <= rect.Top));
        }

        private bool IntersectsWithInclusive(RectangleF r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the RectangleF a specified distance.
        /// </remarks>

        public void Offset(float x, float y)
        {
            X += x;
            Y += y;
        }

        /// <summary>
        ///	Offset Method
        /// </summary>
        ///
        /// <remarks>
        ///	Moves the RectangleF a specified distance.
        /// </remarks>

        public void Offset(PointF pos)
        {
            Offset(pos.X, pos.Y);
        }

        /// <summary>
        ///	ToString Method
        /// </summary>
        ///
        /// <remarks>
        ///	Formats the RectangleF in (x,y,w,h) notation.
        /// </remarks>

        public override string ToString()
        {
            return String.Format("{{X={0},Y={1},Width={2},Height={3}}}",
                         x, y, width, height);
        }
    }
}

 
//for .NET 2.0 
namespace System
{
    public delegate R Func<R>();
    public delegate R Func<T, R>(T t1);
    public delegate R Func<T1, T2, R>(T1 t1, T2 t2);
    public delegate R Func<T1, T2, T3, R>(T1 t1, T2 t2, T3 t3);


    internal static class NativeMethods
    {
        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static unsafe void CopyUnmanagedMemory(byte* srcPtr, int srcOffset, byte* dstPtr, int dstOffset, int count)
        {
            srcPtr += srcOffset;
            dstPtr += dstOffset;

            memcpy(dstPtr, srcPtr, count);
        }

        [TargetedPatchingOptOut("Internal method only, inlined across NGen boundaries for performance reasons")]
        internal static void SetUnmanagedMemory(IntPtr dst, int filler, int count)
        {
            memset(dst, filler, count);
        }

        // Win32 memory copy function
        //[DllImport("ntdll.dll")]
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern unsafe byte* memcpy(
            byte* dst,
            byte* src,
            int count);

        // Win32 memory set function
        //[DllImport("ntdll.dll")]
        //[DllImport("coredll.dll", EntryPoint = "memset", SetLastError = false)]
        [DllImport("msvcrt.dll", EntryPoint = "memset", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        private static extern void memset(
            IntPtr dst,
            int filler,
            int count);
    }
}
namespace System.Runtime.InteropServices
{
    public partial class TargetedPatchingOptOutAttribute : Attribute
    {
        public TargetedPatchingOptOutAttribute(string msg) { }
    }
}
namespace System.Runtime.CompilerServices
{
    public partial class ExtensionAttribute : Attribute { }
}
