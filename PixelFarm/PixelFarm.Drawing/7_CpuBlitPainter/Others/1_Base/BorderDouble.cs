//BSD, 2014-present, WinterDev

namespace PixelFarm.CpuBlit.VertexProcessing
{
    /// <summary>
    /// BorderDouble is used to represent the border around (Margin) on inside (Padding) of a rectangular area.
    /// </summary>
    public struct BorderDouble
    {
        public double Left, Bottom, Right, Top;
        public BorderDouble(double valueForAll)
            : this(valueForAll, valueForAll, valueForAll, valueForAll)
        {
        }

        public BorderDouble(double leftRight, double bottomTop)
            : this(leftRight, bottomTop, leftRight, bottomTop)
        {
        }

        public BorderDouble(double left = 0, double bottom = 0, double right = 0, double top = 0)
        {
            this.Left = left;
            this.Bottom = bottom;
            this.Right = right;
            this.Top = top;
        }

        public static bool operator ==(BorderDouble a, BorderDouble b)
        {
            if (a.Left == b.Left && a.Bottom == b.Bottom && a.Right == b.Right && a.Top == b.Top)
            {
                return true;
            }

            return false;
        }

        public static bool operator !=(BorderDouble a, BorderDouble b)
        {
            if (a.Left != b.Left || a.Bottom != b.Bottom || a.Right != b.Right || a.Top != b.Top)
            {
                return true;
            }

            return false;
        }

        static public BorderDouble operator *(BorderDouble a, double b)
        {
            return new BorderDouble(a.Left * b, a.Bottom * b, a.Right * b, a.Top * b);
        }

        static public BorderDouble operator *(double b, BorderDouble a)
        {
            return new BorderDouble(a.Left * b, a.Bottom * b, a.Right * b, a.Top * b);
        }

        public static BorderDouble operator +(BorderDouble left, BorderDouble right)
        {
            left.Left += right.Left;
            left.Bottom += right.Bottom;
            left.Right += right.Right;
            left.Top += right.Top;
            return left;
        }


        public override int GetHashCode()
        {
            return new { x1 = Left, x2 = Right, y1 = Bottom, y2 = Top }.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BorderDouble))
            {
                return this == (BorderDouble)obj;
            }
            return false;
        }

        public double Width
        {
            get
            {
                return Left + Right;
            }
        }

        // This function assumes the rect is normalized
        public double Height
        {
            get
            {
                return Bottom + Top;
            }
        }

        public override string ToString()
        {
            return string.Format("L:{0}, B:{1}, R:{2}, T:{3}", Left, Bottom, Right, Top);
        }
    }
}
