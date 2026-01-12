using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeeShopKiosk.Converters
{
    public class RoleToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var role = (value ?? string.Empty).ToString().ToLowerInvariant();
            if (role == "assistant") return new SolidColorBrush(Color.FromArgb(255, 244, 242, 239));
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}