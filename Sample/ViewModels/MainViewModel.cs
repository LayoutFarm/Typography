using notf;
using Sample.ViewModels.Commands;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Sample.ViewModels
{
    public class MainViewModel
    {
        private Typeface _typeface;

        private List<Point> _points = new List<Point>();

        public List<Point> Points
        {
            get { return _points; }
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
            _typeface = LoadFrom(f);
        }
    }
}
