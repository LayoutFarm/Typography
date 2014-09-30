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

    public static class RasterExtensions
    {
        public static void SetPixel(this Raster raster, int x, int y, byte value)
        {
            if (x < 0 || x >= raster.Width) return;
            if (y < 0 || y >= raster.Height) return;
            raster.Pixels[x + y * raster.Stride] = value;
        }
    }
}
