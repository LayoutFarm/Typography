//MIT, 2016-present, WinterDev

using PixelFarm.VectorMath;
namespace PixelFarm.Contours
{
    public enum PartKind
    {
        Unknown,
        Line,
        Curve3,
        Curve4
    }


    public abstract class ContourPart
    {
        float _x0, _y0;
        public Vector2f FirstPoint
        {
            get
            {
                if (PrevPart != null)
                {
                    return PrevPart.GetLastPoint();
                }
                else
                {
                    return new Vector2f(_x0, _y0);
                }
            }
            protected set
            {
                _x0 = value.X;
                _y0 = value.Y;
            }
        }
        public abstract PartKind Kind { get; }
        public ContourPart NextPart { get; protected set; }
        public ContourPart PrevPart { get; protected set; }
        internal abstract void Flatten(PartFlattener flattener);

        public abstract Vector2f GetLastPoint();
#if DEBUG
        static int dbugTotalId;
        public readonly int dbugId = dbugTotalId++;
        public ContourPart()
        {
            //if (this.dbugId == 16)
            //{
            //}
        }
#endif
    }



    public class Line : ContourPart
    {

        public float x1;
        public float y1;

        public Line(float x0, float y0, float x1, float y1)
        {
            this.FirstPoint = new Vector2f(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
        }
        public Line(ContourPart prevPart, float x1, float y1)
        {
            //this.x0 = x0;
            //this.y0 = y0;
            this.PrevPart = prevPart;
            this.x1 = x1;
            this.y1 = y1;
        }
        public override Vector2f GetLastPoint()
        {
            return new Vector2f(x1, y1);
        }
        internal override void Flatten(PartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromLine(
                this.FirstPoint,
                new Vector2f(x1, y1));
        }

        public override PartKind Kind => PartKind.Line;

#if DEBUG
        public override string ToString()
        {
            return "L(" + this.FirstPoint + "), (" + x1 + "," + y1 + ")";
        }
#endif
    }
    public class Curve3 : ContourPart
    {
        public float x1, y1, x2, y2;
        public Curve3(float x0, float y0, float x1, float y1, float x2, float y2)
        {
            this.FirstPoint = new Vector2f(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
        public Curve3(ContourPart prevPart, float x1, float y1, float x2, float y2)
        {
            this.PrevPart = prevPart;
            //this.x0 = x0;
            //this.y0 = y0;
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
        }
        public override Vector2f GetLastPoint()
        {
            return new Vector2f(x2, y2);
        }
        internal override void Flatten(PartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromCurve3(
                flattener.NSteps,
                this.FirstPoint, //first
                new Vector2f(x2, y2), //end
                new Vector2f(x1, y1)); //control1
        }

        public override PartKind Kind => PartKind.Curve3;
#if DEBUG
        public override string ToString()
        {
            return "C3(" + this.FirstPoint + "), (" + x1 + "," + y1 + "),(" + x2 + "," + y2 + ")";
        }
#endif
    }
    public class Curve4 : ContourPart
    {
        public float x1, y1, x2, y2, x3, y3;

        public Curve4(float x0, float y0, float x1, float y1,
            float x2, float y2,
            float x3, float y3)
        {
            this.FirstPoint = new Vector2f(x0, y0);
            this.x1 = x1;
            this.y1 = y1;
            this.x2 = x2;
            this.y2 = y2;
            this.x3 = x3;
            this.y3 = y3;
        }
        public Curve4(ContourPart prevPart, float x1, float y1,
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
        public override Vector2f GetLastPoint()
        {
            return new Vector2f(x3, y3);
        }
        internal override void Flatten(PartFlattener flattener)
        {
#if DEBUG
            flattener.dbugSetCurrentOwnerPart(this);
#endif
            flattener.GeneratePointsFromCurve4(
                flattener.NSteps,
                this.FirstPoint,    //first
                new Vector2f(x3, y3), //end
                new Vector2f(x1, y1), //control1
                new Vector2f(x2, y2) //control2
                );
        }

        public override PartKind Kind => PartKind.Curve4;
#if DEBUG
        public override string ToString()
        {
            return "C4(" + this.FirstPoint + "), (" + x1 + "," + y1 + "),(" + x2 + "," + y2 + "), (" + x3 + "," + y3 + ")";
        }
#endif

    }


}