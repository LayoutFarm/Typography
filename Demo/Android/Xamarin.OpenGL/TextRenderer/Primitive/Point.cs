namespace Typography.Rendering
{
    struct Point
    {
        public float x, y;

        public float X
        {
            get { return x; }
        }

        public float Y
        {
            get { return y; }
        }

        public Point(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public Point(double x, double y)
        {
            this.x = (float)x;
            this.y = (float)y;
        }

        public static Point Zero = new Point(0f, 0f);

        public static Vector operator -(Point point1, Point point2)
        {
            return new Vector(point1.x - point2.x, point1.y - point2.y);
        }


        public override string ToString()
        {
            return string.Format("({0:0.00},{1:0.00})", this.x, this.y);
        }
    }
}
