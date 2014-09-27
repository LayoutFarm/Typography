namespace NRasterizer
{
    public class Glyph
    {
        private readonly byte[] _instructions;
        private readonly short[] _x;
        private readonly short[] _y;
        private readonly Bounds _bounds;

        public static readonly Glyph Empty = new Glyph(new byte[0], new short[0], new short[0], Bounds.Zero);

        public Glyph(byte[] instructions, short[] x, short[] y, Bounds bounds)
        {
            _instructions = instructions;
            _x = x;
            _y = y;
            _bounds = bounds;
        }

        public Bounds Bounds { get { return _bounds; } }
        public int PointCount { get { return _x.Length; } } // or y...
        public short[] X { get { return _x; } }
        public short[] Y { get { return _y; } }

        // For debug
        public void Run()
        {
            var interpreter = new Interpreter();
            interpreter.Run(_instructions);
        }
    }
}
