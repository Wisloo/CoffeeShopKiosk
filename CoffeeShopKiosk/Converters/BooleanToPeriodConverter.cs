using System;
using System.Globalization;
using System.Windows.Data;

namespace CoffeeShopKiosk.Converters
{
    public class BooleanToPeriodConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return b ? "Work" : "Break";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}