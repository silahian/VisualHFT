using System;
using System.Windows.Media;
using System.Windows.Data;
using System.Globalization;

namespace VisualHFT.Converters
{
    public class ConverterValuesToColorIfEquals : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool areEqual = false;
            if (values[0] is int && values[1] is int)
            {
                int? value1 = (int?)values[0];
                int? value2 = (int?)values[1];
                areEqual = value1 == value2;
            }
            else if (values[0] is double && values[1] is double)
            {
                double? value1 = (double?)values[0];
                double? value2 = (double?)values[1];
                areEqual = value1 == value2;
            }
            else if (values[0] is string && values[1] is string)
            {
                string value1 = (string)values[0];
                string value2 = (string)values[1];
                areEqual = value1 == value2;
            }

            return areEqual ? Brushes.LightGreen : Brushes.LightPink;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
