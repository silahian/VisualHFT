using System;
using System.Globalization;

namespace VisualHFT.Helpers
{
    public class HelperFormat
	{
		public static string FormatNumber(double number)
		{
			if (number == 0)
				return "";
			double value = Math.Abs(number);
			bool isNegative = number < 0;
			if (value >= 1000000000.0)
				return (isNegative ? "-" : "") + (value / 1000000000.0).ToString("#,##0.##,,,B", CultureInfo.InvariantCulture);// Displays 1B
			else if (value >= 1000000.0)
				return (isNegative ? "-" : "") + (value / 1000000.0).ToString("#,##0.##,,M", CultureInfo.InvariantCulture);// Displays 1,235M
			else if (value >= 1000.0)
				return (isNegative ? "-" : "") + (value / 1000.0).ToString("#,##0.##,K", CultureInfo.InvariantCulture);// Displays 1,234,568K
			else if (value < 1000.0)
				return (isNegative ? "-" : "") + value.ToString();

			return (isNegative ? "-" : "") + value.ToString();
		}
	}
}
