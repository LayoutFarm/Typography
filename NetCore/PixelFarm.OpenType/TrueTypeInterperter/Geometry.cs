//MIT, 2015, Michael Popoloski's SharpFont
using System;
using System.Numerics;
namespace SharpFont
{
    struct FUnit
    {
        int value;

        public static explicit operator int(FUnit v) { return v.value; }
        public static explicit operator FUnit(int v) { return new FUnit { value = v }; }

        public static FUnit operator -(FUnit lhs, FUnit rhs) { return (FUnit)(lhs.value - rhs.value); }
        public static FUnit operator +(FUnit lhs, FUnit rhs) { return (FUnit)(lhs.value + rhs.value); }
        public static float operator *(FUnit lhs, float rhs) { return lhs.value * rhs; }

        public static FUnit Max(FUnit a, FUnit b) { return (FUnit)Math.Max(a.value, b.value); }
        public static FUnit Min(FUnit a, FUnit b) { return (FUnit)Math.Min(a.value, b.value); }
    }


    struct GlyphPointF
    {
        public Vector2 P;
        public PointType Type;

        public GlyphPointF(Vector2 position, PointType type)
        {
            P = position;
            Type = type;
        }

        public GlyphPointF Offset(Vector2 offset) { return new GlyphPointF(P + offset, Type); }

        public override string ToString() { return P.ToString() + " " + Type.ToString(); }

        public static implicit operator Vector2(GlyphPointF p) { return p.P; }
    }

    enum PointType : byte
    {
        OnCurve,
        Quadratic,
        Cubic
    }

}