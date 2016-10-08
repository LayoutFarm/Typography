namespace NRasterizer
{
    public class Bounds
    {
        private readonly short _xmin;
        private readonly short _ymin;
        private readonly short _xmax;
        private readonly short _ymax;

        public static readonly Bounds Zero = new Bounds(0, 0, 0, 0);

        public Bounds(short xmin, short ymin, short xmax, short ymax)
        {
            _xmin = xmin;
            _ymin = ymin;
            _xmax = xmax;
            _ymax = ymax;
        }

        public short XMin { get { return _xmin; } }
        public short YMin { get { return _ymin; } }
        public short XMax { get { return _xmax; } }
        public short YMax { get { return _ymax; } }
    }
}
