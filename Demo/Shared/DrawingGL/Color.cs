//MIT, 2014-2017, WinterDev  


namespace DrawingGL
{
    public struct Color
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            this.R = r;
            this.G = g;
            this.B = b;
            this.A = a;
        }

        public static readonly Color Black = new Color(0, 0, 0, 255);
        public static readonly Color White = new Color(255, 255, 255, 255);
    }
}