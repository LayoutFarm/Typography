//MIT, 2017, Zou Wei(github/zwcloud), WinterDev
namespace DrawingGL
{
    readonly struct Size
    {
        readonly float _width;
        readonly float _height;

        public Size(float width, float height)
        {
            _width = width;
            _height = height;
        }
    }

    public struct Vector2
    {
        float _x;
        float _y;
        public Vector2(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public float X
        {
            get => _x;
            set => _x = value;
        }


        public float Y
        {
            get => _y;
            set => _y = value;
        }

        public static Vector2 operator -(Vector2 v0, Vector2 v1)
        {
            return new Vector2(v0._x - v1._x, v0._y - v1._y);
        }
    }
}