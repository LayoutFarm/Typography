using notf;
using System;
using System.IO;
using System.Windows;

namespace Sample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Typeface _typeface;

        public MainWindow()
        {
            InitializeComponent();
        }

        private Typeface Load(FileInfo fontFile)
        {
            using (var stream = fontFile.OpenRead())
            {
                return new OpenTypeReader().Read(stream);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = new FileInfo(@"C:\Users\vidstige\Desktop\segoe\segoeui.ttf");
            _typeface = Load(f);
        }
    }
}
