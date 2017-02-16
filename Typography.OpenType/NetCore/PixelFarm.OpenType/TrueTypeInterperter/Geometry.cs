//MIT, 2015, Michael Popoloski's SharpFont,
//MIT, 2016-2017, WinterDev

using System;
using System.Numerics;
namespace NOpenType
{


    public struct GlyphPointF
    {
        public Vector2 P;
        public bool onCurve;
        public GlyphPointF(float x, float y, bool onCurve)
        {
            P = new Vector2(x, y);
            this.onCurve = onCurve;
        }
        public GlyphPointF(Vector2 position, bool onCurve)
        {
            P = position;
            this.onCurve = onCurve;
        }

        public GlyphPointF Offset(Vector2 offset) { return new GlyphPointF(P + offset, onCurve); }
        public GlyphPointF Offset(short dx, short dy) { return new GlyphPointF(new Vector2(P.X + dx, P.Y + dy), onCurve); }

        public void ApplyScale(float scale)
        {
            P *= scale;
        }
        public void ApplyScaleOnlyOnXAxis(float scale)
        {
            P = new Vector2(P.X * scale, P.Y);
        }

        public override string ToString() { return P.ToString() + " " + onCurve.ToString(); }

        public static implicit operator Vector2(GlyphPointF p) { return p.P; }

        public static GlyphPointF operator *(GlyphPointF p, float n)
        {
            return new GlyphPointF(p.P * n, p.onCurve);
        }
        public bool IsEqualsWith(GlyphPointF another)
        {
            return this.P == another.P && this.onCurve == another.onCurve;
        }
        public float X { get { return this.P.X; } }
        public float Y { get { return this.P.Y; } }
    }


}