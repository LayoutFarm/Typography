//MIT, 2015, Michael Popoloski's SharpFont
using System;
using System.Numerics;
namespace SharpFont
{


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