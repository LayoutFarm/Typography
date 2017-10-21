//Apache2, 2017, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

namespace Typography.OpenFont
{
    /// <summary>
    /// original glyph bounds
    /// </summary>
    public struct Bounds
    {
        readonly short _xmin;
        readonly short _ymin;
        readonly short _xmax;
        readonly short _ymax;
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
#if DEBUG
        public override string ToString()
        {
            return "(" + _xmin + "," + _ymin + "," + _xmax + "," + _ymax + ")";
        }
#endif
    }
}
