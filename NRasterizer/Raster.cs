namespace NRasterizer
{
        public class Raster
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _stride;
        private readonly byte[] _pixels;

        public Raster(int width, int height, int stride)
        {
            _width = width;
            _height = height;
            _stride = stride;
            _pixels = new byte[_stride * _height];
        }

        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public int Stride { get { return _stride; } }
        public byte[] Pixels { get { return _pixels; } }
    }
}
