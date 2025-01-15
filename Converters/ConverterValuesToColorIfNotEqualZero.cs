using System;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace VisualHFT.Converters
{
    public class ConverterValuesToColorIfNotEqualZero : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = value as string;
            if (string.IsNullOrEmpty(str)) return null;

            double dValue;
            if (!double.TryParse(str, out dValue)) return null;

            if (Math.Abs(dValue) > 0.01)
                return Brushes.Pink;
            else
                return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
