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
        private int _size;
        private string _text = "A";

        private bool _toFlags;

        private List<Segment> _segments;

        private readonly WriteableBitmap _raster = new WriteableBitmap(160, 120, 72, 72, System.Windows.Media.PixelFormats.Gray8, null);

        private bool _drawOutline = true;
        private bool _drawRaster = true;

        public MainViewModel()
        {
            LoadTypeface();
            _size = 48;
            UpdateRaster();
        }

        //public List<Point> Points
        //{
        //    get { return _points; }
        //    private set
        //    {
        //        _points = value;
        //        RaisePropertyChanged();
        //    }
        //}

        public bool DrawRaster
        {
            get { return _drawRaster; }
            set
            {
                _drawRaster = value;
                RaisePropertyChanged();
            }
        }

        public bool DrawOutline
        {
            get { return _drawOutline; }
            set
            {
                _drawOutline = value;
                RaisePropertyChanged();
            }
        }

        public List<Segment> Segments
        {
            get { return _segments; }
            set
            {
                _segments = value;
                RaisePropertyChanged();
            }
        }

        public bool ToFlags
        {
            get { return _toFlags; }
            set
            {
                _toFlags = value;
                UpdateRaster();
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

        //private IEnumerable<Point> ToPoints(Glyph glyph, Bounds source, Rect target)
        //{
        //    var allX = glyph.X;
        //    var allY = glyph.Y;
        //    for (int i = 0; i < glyph.X.Length; i++)
        //    {
        //        float x = allX[i];
        //        float y = allY[i];
        //        var p = new Point(
        //            (x - source.XMin) / (source.XMax - source.XMin),
        //            1.0 - ((y - source.YMin) / (source.YMax - source.YMin)));
        //        yield return p;
        //    }
        //}

        public int FontSize
        {
            get { return _size; }
            set
            {
                _size = value;
                UpdateRaster();
                RaisePropertyChanged();
            }
        }
        
        public ICommand Load { get { return new DelegatingCommand(LoadTypeface); } }
        //public ICommand Rasterize { get { return new DelegatingCommand(RasterizeGlyph); } }
        public ICommand SaveRaster { get { return new DelegatingCommand(SaveRasterToFile); } }

        public BitmapSource Raster
        {
            get
            {
                return _raster;
            }
        }

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
                UpdateRaster();
                RaisePropertyChanged();
            }
        }

        private Int32Rect Bounds(WriteableBitmap bitmap)
        {
            return new Int32Rect(0, 0, bitmap.PixelHeight, bitmap.PixelHeight);
        }

        private void UpdateRaster()
        {
            Draw(_text, _size);
        }

        private void Draw(string text, int size)
        {
            var raster = new Raster(_raster.PixelWidth, _raster.PixelHeight, _raster.PixelWidth, 72);
            var r = new Rasterizer(_typeface);
            if (DrawRaster)
            {
                r.Rasterize(text, size, raster, _toFlags);
            }
            _raster.WritePixels(Bounds(_raster), raster.Pixels, raster.Stride, 0);

            if (DrawOutline)
            {
                Segments = r.GetAllSegments(text, size, raster.Resolution).ToList();
            }
            else
            {
                Segments = Enumerable.Empty<Segment>().ToList();
            }
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
