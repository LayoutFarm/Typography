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

        int Width { get { return _width; } }
        int Height { get { return _height; } }
        int Stride { get { return _stride; } }
        byte[] Pixels { get { return _pixels; } }
    }
}
