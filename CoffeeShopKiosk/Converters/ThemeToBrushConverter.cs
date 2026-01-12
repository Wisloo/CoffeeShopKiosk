using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace CoffeeShopKiosk.Converters
{
    public class ThemeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = (value ?? string.Empty).ToString().ToLowerInvariant();
            switch (s)
            {
                case "rain":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B3A66"));
                case "sunset":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7A2E3A"));
                case "minimal":
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#222222"));
                default: // cafe
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5B3A2A"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}