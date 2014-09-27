using notf;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var f = new FileInfo(@"C:\Users\vidstige\Desktop\segoe\segoeui.ttf");
            var reader = new OpenTypeReader();
            using (var stream = f.OpenRead())
            {
                reader.Reader(stream);
            }
        }
    }
}
