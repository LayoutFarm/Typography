//BSD, 2014-present, WinterDev

using System;
using PixelFarm.VectorMath;

namespace PixelFarm.CpuBlit.VertexProcessing
{
    public struct RectD
    {
        public double Left, Bottom, Right, Top;
        public static RectD ZeroIntersection() => new RectD(double.MaxValue, double.MaxValue, double.MinValue, double.MinValue);


        public RectD(double left, double bottom, double right, double top)
        {
            this.Left = left;
            this.Bottom = bottom;
            this.Right = right;
            this.Top = top;
        }
 
        public static RectD CreateFromLTWH(double left, double top, double width, double height)
        {
            return new RectD(left, top + height, left + width, top);
        }

        // This function assumes the rect is normalized
        public double Width => Right - Left;
        // This function assumes the rect is normalized
        public double Height => Top - Bottom;
       
        ////
        public bool Contains(double x, double y) => (x >= Left && x <= Right && y >= Bottom && y <= Top);
        ////
        public bool Contains(RectD innerRect) => Contains(innerRect.Left, innerRect.Bottom) && Contains(innerRect.Right, innerRect.Top);
        ////
        public bool Contains(Vector2 position) => Contains(position.x, position.y);
        ////
     
        public void Offset(double x, double y)
        {
            Left = Left + x;
            Bottom = Bottom + y;
            Right = Right + x;
            Top = Top + y;
        } 
        public override string ToString()
        {
            return string.Format("L:{0}, B:{1}, R:{2}, T:{3}", Left, Bottom, Right, Top);
        }
    }
}
