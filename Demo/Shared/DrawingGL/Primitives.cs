//MIT, 2017, Zou Wei(github/zwcloud), WinterDev
namespace DrawingGL
{
    struct Size
    {
        private float x, y;
        private float width;
        private float height;

        public Size(float width, float height) : this()
        {
            this.width = width;
            this.height = height;
        }
    }
    
    public struct Vector2
    {
        private float x;
        private float y;
        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X { get { return x; } set { x = value; } }

        public float Y { get { return y; } set { y = value; } }

        public static Vector2 operator -(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v0.x - v1.x, v0.y - v1.y);
        }
    }
}