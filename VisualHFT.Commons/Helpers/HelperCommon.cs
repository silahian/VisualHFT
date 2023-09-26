using System.ComponentModel;
using System.Reflection;

namespace VisualHFT.Helpers
{
    public class HelperCommon
    {
        /*public static double GetCurrencyRate(string currencySymbol, bool getBid)
        {
            var stock = HelperYahoo.GetStock(currencySymbol);
            if (stock == null)
                return 0;
            else
                return (getBid ? stock.Bid : stock.Ask);
        }*/
        public static string GetKiloFormatter(int num)
        {
            return GetKiloFormatter((double)num);
        }
        public static string GetKiloFormatter(decimal num)
        {
            return GetKiloFormatter((double)num);
        }
        public static string GetKiloFormatter(double num)
        {
            if (num < 500)
                return num.ToString("N");
            if (num < 10000)
                return num.ToString("N0");


            if (num >= 100000000)
                return (num / 1000000D).ToString("0.#M");
            if (num >= 1000000)
                return (num / 1000000D).ToString("0.##M");
            if (num >= 100000)
                return (num / 1000D).ToString("0.#k");
            if (num >= 10000)
                return (num / 1000D).ToString("0.##k");
            return num.ToString("#,0");
        }
        public static string GetKiloFormatterTime(double milliseconds)
        {
            double num = milliseconds;

            if (num >= 1000 * 60 * 60.0)
                return (num / (60.0 * 60.0 * 1000D)).ToString("0.0 hs");
            if (num >= 1000 * 60.0)
                return (num / (60.0 * 1000D)).ToString("0.0 min");
            if (num >= 1000)
                return (num / 1000D).ToString("0.0# sec");

            return num.ToString("#,0 ms");
        }
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            DescriptionAttribute[] attributes =
                (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}
