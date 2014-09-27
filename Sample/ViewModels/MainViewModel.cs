using notf;
using notf.Tables;
using Sample.ViewModels.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Sample.ViewModels
{
    public class MainViewModel: ViewModel
    {
        private Typeface _typeface;

        private List<Point> _points = new List<Point>();
        private int _selected;

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

        private IEnumerable<Point> ToPoints(Glyph glyph)
        {
            var x = glyph.X;
            var y = glyph.Y;
            for (int i = 0; i < glyph.PointCount; i++)
                yield return new Point(x[i], y[i]);
        }

        public int SelectedGlyph
        {
            get { return _selected; }
            set
            {
                _selected = value;
                RaisePropertyChanged();

                // Update points
                Points = ToPoints(_typeface.Glyphs[_selected]).ToList();
            }
        }

        public ICommand Load { get { return new DelegatingCommand(LoadTypeface); } }

        private Typeface LoadFrom(FileInfo fontFile)
        {
            using (var stream = fontFile.OpenRead())
            {
                return new OpenTypeReader().Read(stream);
            }
        }

        private void LoadTypeface()
        {
            var f = new FileInfo(@"C:\Users\vidstige\Desktop\segoe\segoeui.ttf");
            Typeface = LoadFrom(f);
        }
    }
}
