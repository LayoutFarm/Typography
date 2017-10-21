//MIT, 2016-2017, WinterDev

using System.Numerics;

namespace Typography.Contours
{
    public enum GlyphPartKind
    {
        Unknown,
        Line,
        Curve3,
        Curve4
    }


    public abstract class GlyphPart
    {
        float _x0, _y0;
        public Vector2 FirstPoint
        {
            get
            {
                if (PrevPart != null)
                {
                    return PrevPart.GetLastPoint();
                }
                else
                {
                    return new Vector2(_x0, _y0);
                }
            }
            protected set
            {
                this._x0 = value.X;
                this._y0 = value.Y;
            }
        }
        public abstract GlyphPartKind Kind { get; }
        public GlyphPart NextPart { get; set; }
        public GlyphPart PrevPart { get; set; }
        internal abstract void Flatten(GlyphPartFlattener flattener);

        public abstract Vector2 GetLastPoint();
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
        public GlyphPart()
        {
            //if (this.dbugId == 16)
            //{
            //}
        }
#endif
    }



    public class GlyphLine : GlyphPart
    {

        public float x1;
        public float y1;

        public GlyphLine(float x0, float y0, float x1, float y1)
        {
            this.FirstPoint = new Vector2(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
        }
        public GlyphLine(GlyphPart prevPart, float x1, float y1)
        {
            //this.x0 = x0;
            //this.y0 = y0;
            this.PrevPart = prevPart;
            this.x1 = x1;
            this.y1 = y1;
        }
        public override Vector2 GetLastPoint()
        {
            return new Vector2(x1, y1);
        }
        internal override void Flatten(GlyphPartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromLine(
                this.FirstPoint,
                new Vector2(x1, y1));
        }

        public override GlyphPartKind Kind { get { return GlyphPartKind.Line; } }

#if DEBUG
        public override string ToString()
        {
            return "L(" + this.FirstPoint + "), (" + x1 + "," + y1 + ")";
        }
#endif
    }
    public class GlyphCurve3 : GlyphPart
    {
        public float x1, y1, x2, y2;
        public GlyphCurve3(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            this.FirstPoint = new Vector2(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
        public GlyphCurve3(GlyphPart prevPart, float x1, float y1, float x2, float y2)
        {
            this.PrevPart = prevPart;
            //this.x0 = x0;
            //this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
        public override Vector2 GetLastPoint()
        {
            return new Vector2(x2, y2);
        }
        internal override void Flatten(GlyphPartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromCurve3(
                flattener.NSteps,
                this.FirstPoint, //first
                new Vector2(x2, y2), //end
                new Vector2(x1, y1)); //control1
        }

        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve3; } }
#if DEBUG
        public override string ToString()
        {
            return "C3(" + this.FirstPoint + "), (" + x1 + "," + y1 + "),(" + x2 + "," + y2 + ")";
        }
#endif
    }
    public class GlyphCurve4 : GlyphPart
    {
        public float x1, y1, x2, y2, x3, y3;

        public GlyphCurve4(float x0, float y0, float x1, float y1,
            float x2, float y2,
            float x3, float y3)
        {
            this.FirstPoint = new Vector2(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }
        public GlyphCurve4(GlyphPart prevPart, float x1, float y1,
         float x2, float y2,
         float x3, float y3)
        {
            //this.x0 = x0;
            //this.y0 = y0;
            this.PrevPart = prevPart;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }
        public override Vector2 GetLastPoint()
        {
            return new Vector2(x3, y3);
        }
        internal override void Flatten(GlyphPartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromCurve4(
                flattener.NSteps,
                this.FirstPoint,    //first
                new Vector2(x3, y3), //end
                new Vector2(x1, y1), //control1
                new Vector2(x2, y2) //control2
                );
        }

        public override GlyphPartKind Kind { get { return GlyphPartKind.Curve4; } }
#if DEBUG
        public override string ToString()
        {
            return "C4(" + this.FirstPoint + "), (" + x1 + "," + y1 + "),(" + x2 + "," + y2 + "), (" + x3 + "," + y3 + ")";
        }
#endif

    }


}