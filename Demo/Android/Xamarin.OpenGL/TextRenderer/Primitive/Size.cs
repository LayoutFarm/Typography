//MIT, 2017, Zou Wei(github/zwcloud)
namespace Typography.Rendering
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
}