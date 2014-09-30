using NRasterizer;
using Sample.ViewModels.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Sample.ViewModels
{
    public class MainViewModel: ViewModel
    {
        private Typeface _typeface;

        private List<Point> _points = new List<Point>();
        private int _selected;
        private string _text = "s";

        private readonly WriteableBitmap _raster = new WriteableBitmap(320, 240, 72, 72, System.Windows.Media.PixelFormats.Gray8, null);

        public List<Point> Points
        {
            get { return _points; }
            private set
            {
                _points = value;
                RaisePropertyChanged();
            }
        }

        public Typeface Typeface
        {
            get { return _typeface; }
            private set
            {
                _typeface = value;
                RaisePropertyChanged();
            }
        }

        private IEnumerable<Point> ToPoints(Glyph glyph, Bounds source, Rect target)
        {
            var allX = glyph.X;
            var allY = glyph.Y;
            for (int i = 0; i < glyph.X.Length; i++)
            {
                float x = allX[i];
                float y = allY[i];
                var p = new Point(
                    (x - source.XMin) / (source.XMax - source.XMin),
                    1.0 - ((y - source.YMin) / (source.YMax - source.YMin)));
                yield return p;
            }
        }

        public int SelectedGlyph
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged();

                // Update points
                Points = ToPoints(_typeface.Glyphs[_selected], _typeface.Bounds, Rect.Empty).ToList();
            }
        }

        public ICommand Load { get { return new DelegatingCommand(LoadTypeface); } }
        //public ICommand Rasterize { get { return new DelegatingCommand(RasterizeGlyph); } }
        public ICommand SaveRaster { get { return new DelegatingCommand(SaveRasterToFile); } }

        public BitmapSource Raster { get { return _raster; } }

        public void SaveRasterToFile()
        {
            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "screenshot.png"));
            SavePng(_raster, file);
        }

        private void SavePng(BitmapSource bitmap, FileInfo file)
        {
            BitmapFrame frame = BitmapFrame.Create(_raster);
            
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame);
            using (var stream = file.OpenWrite())
            {
                encoder.Save(stream);
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Draw(_text);
                RaisePropertyChanged();
            }
        }

        private Int32Rect Bounds(WriteableBitmap bitmap)
        {
            return new Int32Rect(0, 0, bitmap.PixelHeight, bitmap.PixelHeight);
        }

        private void Draw(string s)
        {
            //SelectedGlyph = _typeface.LookupIndex((char)0x0041); // A

            var raster = new Raster(_raster.PixelWidth, _raster.PixelHeight, _raster.PixelWidth, 72);
            var r = new Rasterizer(_typeface);
            r.Rasterize(s, 32, raster);

            _raster.WritePixels(Bounds(_raster), raster.Pixels, raster.Stride, 0);
        }

        private Typeface LoadFrom(FileInfo fontFile)
        {
            using (var stream = fontFile.OpenRead())
            {
                return new OpenTypeReader().Read(stream);
            }
        }

        private void LoadTypeface()
        {
            //var f = new FileInfo(@"C:\Users\vidstige\Desktop\segoe\segoeui.ttf");
            var f = new FileInfo(@"segoeui.ttf");
            Typeface = LoadFrom(f);
        }
    }
}
