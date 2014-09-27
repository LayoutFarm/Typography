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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Load(FileInfo fontFile)
        {
            var reader = new OpenTypeReader();
            using (var stream = fontFile.OpenRead())
            {
                reader.Reader(stream);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = new FileInfo(@"C:\Users\vidstige\Desktop\segoe\segoeui.ttf");
            Load(f);

            //foreach (var path in Directory.EnumerateFiles(@"C:\Windows\Fonts"))
            //{
            //    Console.WriteLine("path: " + path);
            //    Load(new FileInfo(path));
            //}
        }
    }
}
