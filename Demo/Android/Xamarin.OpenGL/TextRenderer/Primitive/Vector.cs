namespace Typography.Rendering
{
    public struct Vector
    {
        private float x;
        private float y;

        public Vector(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public float X { get { return x; } }

        public float Y { get { return y; } }
    }
}