using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualHFT.Converters
{
    public class ConverterEnum : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
