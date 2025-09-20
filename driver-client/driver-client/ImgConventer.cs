using driver_client.driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace driver_client
{
    public class ImgConventer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //var lesson = value as Lessons;
            bool lesson = value.ToString() == "Yes" ? true : false;
            if (lesson == null) return DependencyProperty.UnsetValue;

            // If parameter says "Visibility", return Visibility
            //if (parameter as string == "Visibility")
            //    return lesson ? Visibility.Collapsed : Visibility.Visible;

            // Otherwise return the image path
            return lesson
                ? "picture/check.jpg"
                : "picture/cross.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}
