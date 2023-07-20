using System;
using System.Windows.Data;
using System.Globalization;
using System.Windows;

namespace VisualHFT.Converters
{
    public class ConverterValueToWidth : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double value = (double)values[0];  // LOBImbalanceValue 
            double width = (double)values[1];  // ActualWidth of the Canvas
            double needleWidth = 10;

            //We are forced to use this, which are related on how the grid gauge is positioned 
            // with respect of the needle.

            // YES, this is terrible. Need to be improved.

            // In order to the needle to be at the very left, we need to return -95
            // In order to the needle to be at the very right, we need to return 355

            double outputMin = -95; 
            double outputMax = 355;

            // Apply the linear transformation
            double output = outputMin + ((value + 1) / 2) * (outputMax - outputMin);

            return output;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
