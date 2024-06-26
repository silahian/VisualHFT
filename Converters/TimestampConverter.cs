using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VisualHFT.Converters
{
    public class TimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime timestamp)
            {
                var now = DateTime.Now;
                if (timestamp.Date == now.Date)
                {
                    return $"Today at {timestamp:HH:mm:ss}";
                }
                else if (timestamp.Date == now.AddDays(-1).Date)
                {
                    return $"Yesterday at {timestamp:HH:mm:ss}";
                }
                else
                {
                    var daysAgo = (now.Date - timestamp.Date).Days;
                    return $"{daysAgo} days ago at {timestamp:HH:mm:ss}";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}