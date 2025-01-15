using System;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace VisualHFT.Converters
{
    public class ConverterIsBuyToColor : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBuy)
            {
                return isBuy ? Brushes.LightGreen: Brushes.LightPink;
            }
            return Brushes.Black; // Default color
        }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
