using System;
using System.Windows.Data;

namespace Sample.ViewModels
{
    public class BezierToPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var b = value as NRasterizer.Bezier;
            var pointNumber = int.Parse(parameter.ToString());
            switch (pointNumber)
            {
                case 0: return new System.Windows.Point(b.x0, b.y0);
                case 1: return new System.Windows.Point(b.x1, b.y1);
                case 2: return new System.Windows.Point(b.x2, b.y2); 
            }
            throw new ArgumentException("The parameter needs to be either 0, 1, or 2");            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException("no");
        }
    }
}
