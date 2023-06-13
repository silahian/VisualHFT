using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualHFT.Converters
{
    public class KiloFormatter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {            
            double num = value.ToDouble();
            bool isNegative = num < 0;
            if (num != 0)
                return (isNegative? "-" : "") + Helpers.HelperCommon.GetKiloFormatter(Math.Abs(num));
            else
                return null;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}