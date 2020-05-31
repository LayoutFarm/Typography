//MIT, 2020, Brezza92, WinterDev

using MathLayout;
using System;
using System.Collections.Generic;

namespace LayoutFarm.MathLayout
{
    public struct Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
        public int X { get; }
        public int Y { get; }
    }
    public struct Rect
    {
        public Rect(double x, double y, double w, double h)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }
        public double X { get; }
        public double Y { get; }
        public double Top => Y;
        public double Left => X;
        public double Bottom => Top + Height;
        public double Width { get; }
        public double Height { get; }
    }
}