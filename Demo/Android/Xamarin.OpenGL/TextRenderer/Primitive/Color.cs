//MIT, 2017, Zou Wei(github/zwcloud)
namespace Typography.Rendering
{
    internal struct Color
    {
        internal static readonly Color Black = new Color(0, 0, 0, 1);

        public float r, g, b, a;

        public Color(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public override string ToString()
        {
            return string.Format("(r:{0},g:{1},b:{2},a:{3})", r, g, b, a);
        }
    }
}
