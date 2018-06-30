//MIT, 2009-2015, Rene Schulte and WriteableBitmapEx Contributors, https://github.com/teichgraf/WriteableBitmapEx
//MIT, 2017-present, WinterDev

using System;
using PixelFarm.CpuBlit.VertexProcessing;

namespace BitmapBufferEx
{
    public abstract class GeneralTransform
    {
        public abstract RectD TransformBounds(RectD r1);
        public abstract PointD Transform(PointD p);

        public abstract MatrixTransform Inverse
        {
            get;
        }

    }
    public class MatrixTransform : GeneralTransform
    {
        MatrixTransform _inverseVersion;
        Affine affine;
        public MatrixTransform(AffinePlan[] affPlans)
        {
            affine = Affine.NewMatix(affPlans);
        }
        public MatrixTransform(Affine affine)
        {
            this.affine = affine;
        }
        public override PointD Transform(PointD p)
        {
            double p_x = p.X;
            double p_y = p.Y;
            affine.Transform(ref p_x, ref p_y);
            return new PointD(p_x, p_y);
        }
        public override MatrixTransform Inverse
        {
            get
            {
                if (_inverseVersion == null)
                {
                    return _inverseVersion = new MatrixTransform(affine.CreateInvert());
                }
                return _inverseVersion;
            }
        }

       
        static RectD FindMaxBounds(PointD p0, PointD p1, PointD p2, PointD p3)
        {
            double left = Math.Min(Math.Min(Math.Min(p0.X, p1.X), p2.X), p3.X);
            double top = Math.Min(Math.Min(Math.Min(p0.Y, p1.Y), p2.Y), p3.Y);
            //
            double right = Math.Max(Math.Max(Math.Max(p0.X, p1.X), p2.X), p3.X);
            double bottom = Math.Max(Math.Max(Math.Max(p0.Y, p1.Y), p2.Y), p3.Y);
            return new RectD(left, top, right - left, bottom - top);
        }
        public override RectD TransformBounds(RectD r1)
        {
            var tmp0 = new PointD(r1.Left, r1.Top);
            var tmp1 = new PointD(r1.Right, r1.Top);
            var tmp2 = new PointD(r1.Right, r1.Bottom);
            var tmp3 = new PointD(r1.Left, r1.Bottom);

            affine.Transform(ref tmp0.X, ref tmp0.Y);
            affine.Transform(ref tmp1.X, ref tmp1.Y);
            affine.Transform(ref tmp2.X, ref tmp2.Y);
            affine.Transform(ref tmp3.X, ref tmp3.Y);
            return FindMaxBounds(tmp0, tmp1, tmp2, tmp3);
        }

    }

    struct RectInt32
    {
        public RectInt32(int left, int top, int width, int height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
        public int Left { get; private set; }
        public int Top { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int X { get { return this.Left; } }
        public int Y { get { return this.Top; } }
        public int Bottom { get { return Y + Height; } }
        public int Right { get { return Left + Width; } }
        public bool IsEmpty
        {
            get
            {
                //TODO: eval once
                return (this.Left | this.Top | this.Width | this.Height) == 0;
            }
        }
        private bool IntersectsWithInclusive(RectInt32 r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }
        public static RectInt32 Intersect(RectInt32 a, RectInt32 b)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            if (!a.IntersectsWithInclusive(b))
            {
                return new RectInt32(0, 0, 0, 0);
            }
            //
            return RectInt32.FromLTRB(
                Math.Max(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Min(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        public static RectInt32 FromLTRB(int left, int top, int right, int bottom)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            return new RectInt32(left, top, right - left, bottom - top);
        }

        /// <summary>
        ///	Intersect Method
        /// </summary>
        ///
        /// <remarks>
        ///	Replaces the Rectangle with the intersection of itself
        ///	and another Rectangle.
        /// </remarks>

        public void Intersect(RectInt32 rect)
        {
            this = RectInt32.Intersect(this, rect);
        }

    }

    public struct RectD
    {
        public RectD(double left, double top, double width, double height)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
        }
        public RectD(PointD location, SizeD size)
        {
            this.Left = location.X;
            this.Top = location.Y;

            this.Width = size.Width;
            this.Height = size.Height;
        }
        public double Left { get; private set; }
        public double Top { get; private set; }
        public double Width { get; private set; }
        public double Height { get; private set; }
        public double X { get { return this.Left; } }
        public double Y { get { return this.Top; } }
        public double Bottom { get { return Y + Height; } }
        public double Right { get { return Left + Width; } }
        public bool IsEmpty
        {
            get
            {
                //TODO: eval once
                return this.Left == 0 && this.Top == 0
                    && this.Width == 0 && this.Height == 0;
            }
        }
        private bool IntersectsWithInclusive(RectD r)
        {
            return !((Left > r.Right) || (Right < r.Left) ||
                (Top > r.Bottom) || (Bottom < r.Top));
        }
        public static RectD Intersect(RectD a, RectD b)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            if (!a.IntersectsWithInclusive(b))
                return new RectD(0, 0, 0, 0);
            return RectD.FromLTRB(
                Math.Max(a.Left, b.Left),
                Math.Max(a.Top, b.Top),
                Math.Min(a.Right, b.Right),
                Math.Min(a.Bottom, b.Bottom));
        }

        public static RectD FromLTRB(double left, double top, double right, double bottom)
        {
            // MS.NET returns a non-empty rectangle if the two rectangles
            // touch each other
            return new RectD(left, top, right - left, bottom - top);
        }

        /// <summary>
        ///	Intersect Method
        /// </summary>
        ///
        /// <remarks>
        ///	Replaces the Rectangle with the intersection of itself
        ///	and another Rectangle.
        /// </remarks>

        public void Intersect(RectD rect)
        {
            this = RectD.Intersect(this, rect);
        }

    }
    public struct PointD
    {
        public double X;
        public double Y;
        public PointD(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }

    }
    public struct SizeD
    {
        public SizeD(int w, int h)
        {
            this.Width = w;
            this.Height = h;
        }
        public SizeD(double w, double h)
        {
            this.Width = w;
            this.Height = h;
        }
        public double Width { get; private set; }
        public double Height { get; private set; }
    }
    public struct ColorInt
    {
        //see https://github.com/PaintLab/PixelFarm/issues/12
        //we store color as 'straight alpha'
        byte _r, _g, _b, _a;

        public byte R { get { return _r; } }
        public byte G { get { return _g; } }
        public byte B { get { return _b; } }
        public byte A { get { return _a; } }

        public static ColorInt CreateNew(ColorInt oldColor, byte a)
        {
            ColorInt c = new ColorInt();
            c._r = oldColor._r;
            c._g = oldColor._g;
            c._b = oldColor._b;
            c._a = a;
            return c;
        }
        public static ColorInt FromArgb(byte a, byte r, byte g, byte b)
        {
            ColorInt c = new ColorInt();
            c._r = r;
            c._g = g;
            c._b = b;
            c._a = a;
            return c;
        }
        public static ColorInt FromArgb(int argb)
        {
            ColorInt c = new ColorInt();
            c._a = (byte)((argb >> 24));
            c._r = (byte)((argb >> 16) & 0xff);
            c._g = (byte)((argb >> 8) & 0xff);
            c._b = (byte)((argb >> 0) & 0xff);

            return c;
        }
        public static bool operator ==(ColorInt c1, ColorInt c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) ==
                   (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public static bool operator !=(ColorInt c1, ColorInt c2)
        {
            return (uint)((c1._a << 24) | (c1._r << 16) | (c1._g << 8) | (c1._b)) !=
                  (uint)((c2._a << 24) | (c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public override bool Equals(object obj)
        {
            if (obj is ColorInt)
            {
                ColorInt c = (ColorInt)obj;
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


        /// <summary>
        /// convert to 'premultiplied alpha' and arrange to int value
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public int ToPreMultAlphaColor()
        {
            //see more at https://github.com/PaintLab/PixelFarm/issues/12
            if (_a == 0) return 0; //for premultiplied alpha => this return (0,0,0,0) NOT (r,g,b,0)
            //
            int a = _a + 1; // Add one to use mul and cheap bit shift for multiplicaltion

            return (_a << 24)
             | ((byte)((_r * a) >> 8) << 16)
             | ((byte)((_g * a) >> 8) << 8)
             | ((byte)((_b * a) >> 8));
        }
        /// <summary>
        /// check if this color is equals on another compare on RGB only, not alpha
        /// </summary>
        /// <param name="another"></param>
        /// <returns></returns>
        public bool EqualsOnRGB(ref ColorInt c2)
        {
            return (uint)((this._r << 16) | (this._g << 8) | (this._b)) ==
                (uint)((c2._r << 16) | (c2._g << 8) | (c2._b));
        }
        public bool EqualsOnRGB(int c2_r, int c2_g, int c2_b)
        {
            return (uint)((this._r << 16) | (this._g << 8) | (this._b)) ==
                (uint)((c2_r << 16) | (c2_g << 8) | (c2_b));
        }
    }

    public struct BitmapBuffer
    {
        //from WriteableBitmap*** 
        public static readonly BitmapBuffer Empty = new BitmapBuffer();
        //in this version , only 32 bits  
        public BitmapBuffer(int w, int h, int[] orgBuffer)
        {
            this.PixelWidth = w;
            this.PixelHeight = h;
            this.Pixels = orgBuffer;
        }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }
        /// <summary>
        /// pre-multiplied alpha color pixels
        /// </summary>
        public int[] Pixels { get; private set; }

        public bool IsEmpty { get { return Pixels == null; } }
    }
}
