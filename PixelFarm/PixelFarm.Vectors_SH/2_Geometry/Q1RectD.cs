//BSD, 2014-present, WinterDev

using System;
using PixelFarm.VectorMath;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    /// <summary>
    /// Cartesian's Quadrant1 Rect, (x0,y0)=> (x1,y1) = (left,bottom)=> (right,top)
    /// </summary>
    public struct Q1RectD
    {
        public double Left, Bottom, Right, Top;
        public static Q1RectD ZeroIntersection() => new Q1RectD(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);


        public Q1RectD(double left, double bottom, double right, double top)
        {
            this.Left = left;
            this.Bottom = bottom;
            this.Right = right;
            this.Top = top;
        }

        public static Q1RectD CreateFromLTWH(double left, double top, double width, double height)
        {
            return new Q1RectD(left, top + height, left + width, top);
        }

        // This function assumes the rect is normalized
        public double Width => Right - Left;
        // This function assumes the rect is normalized
        public double Height => Top - Bottom;

        ////
        public bool Contains(double x, double y) => (x >= Left && x <= Right && y >= Bottom && y <= Top);
        ////
        public bool Contains(Q1RectD innerRect) => Contains(innerRect.Left, innerRect.Bottom) && Contains(innerRect.Right, innerRect.Top);
        ////
        public bool Contains(Vector2 position) => Contains(position.x, position.y);
        ////

        public void Offset(double x, double y)
        {
            Left += x;
            Bottom += y;
            Right += x;
            Top += y;
        }
        public override string ToString()
        {
            return string.Format("L:{0}, B:{1}, R:{2}, T:{3}", Left, Bottom, Right, Top);
        }
    }
}
