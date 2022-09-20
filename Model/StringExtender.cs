using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

public static class StringExtender
    {
        /// <summary>
        /// Split a text into an array
        /// </summary>
        /// <param name="seperator"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string[] Explode(this string text, string seperator)
        {
            string[] stringSeparators = new string[] { seperator };
            return text.Split(stringSeparators, StringSplitOptions.None);
        }

        #region "Conversion methods"
        public static int ToInt(this double obj)
        {
            return ToInt(Math.Truncate(obj).ToString());
        }
        public static int ToInt(this object obj)
        {
            return ToInt(obj.ToString());
        }
        public static int ToInt(this string obj)
        {
            int i = 0;
            int.TryParse(obj, out i);
            return i;
        }
        public static double ToDouble(this double obj)
        {
            if (double.IsNaN(obj) || double.IsInfinity(obj) || obj == null)
                return 0;
            else
                return obj;
        }
        public static double ToDouble(this object obj)
        {
            if (obj == null)
                return 0.00;
            else
                return ToDouble(obj.ToString());
        }
        public static double ToDouble(this string obj)
        {
            try
            {
                bool isPercentage = false;
                double i = 0.00;
                if (obj.IndexOf("%") > -1)
                {
                    obj = obj.Replace("%", "");
                    isPercentage = true;
                }
                double.TryParse(obj, out i);
                if (isPercentage)
                    i = i / 100;
                return i;
            }
            catch { return 0; }
        }


        public static decimal ToDecimal(this double obj)
        {
            try
            {
                return Convert.ToDecimal(obj);
            }
            catch { return 0.00M; }
        }
        public static decimal ToDecimal(this object obj)
        {
            if (obj == null)
                return 0.00M;
            else
                return ToDecimal(obj.ToString());
        }
        public static decimal ToDecimal(this string obj)
        {
            decimal i = 0.00M;
            decimal.TryParse(obj, out i);
            return i;
        }

        public static Double PercentageDiff(this double val1, double val2)
        {
            return (val2 - val1) / Math.Abs(val1);
        }



        public static bool IsNumeric(this string s)
        {
            float output;
            return float.TryParse(s, out output);
        }
        public static bool ToBoolean(this object obj)
        {
            return ToBoolean(obj.ToString());
        }
        public static bool ToBoolean(this string obj)
        {
            return ToBoolean(obj, false);
        }
        public static bool ToBoolean(this string obj, bool defaultValue)
        {
            bool i = defaultValue;
            bool.TryParse(obj, out i);
            return i;
        }

        public static DateTime ToDateTime(this object obj)
        {
            //string DatePositionFormat = "yyyy'.'MM'.'dd'-'HH'.'mm'.'ss'.'ffffff";
            //DateTime.TryParseExact(rawDate, DatePositionFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
            return ToDateTime(obj.ToString());
        }
        public static DateTime ToDateTime(this string obj)
        {
            return ToDateTime(obj, DateTime.MinValue);
        }
        public static DateTime ToDateTime(this string obj, DateTime defaultValue)
        {
            DateTime i = defaultValue;
            DateTime.TryParse(obj, out i);
            return i;
        }

        public static DateTime ToDate(this object obj)
        {
            return ToDate(obj.ToString());
        }
        public static DateTime ToDate(this string obj)
        {
            return ToDate(obj, DateTime.MinValue);
        }
        public static DateTime ToDate(this string obj, DateTime defaultValue)
        {
            DateTime i = obj.ToDateTime();
            return i.ToString("MM/dd/yyyy").ToDateTime();
        }

        
        public static DateTime LastDayOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1).AddMonths(1).AddDays(-1);
        }

        public static DateTime NthOf(this DateTime CurDate, int Occurrence, DayOfWeek Day)
        {
            var fday = new DateTime(CurDate.Year, CurDate.Month, 1);
            var fOc = fday.DayOfWeek == Day ? fday : fday.AddDays(Day - fday.DayOfWeek);
            // CurDate = 2011.10.1 Occurance = 1, Day = Friday >> 2011.09.30 FIX. 
            if (fOc.Month < CurDate.Month) Occurrence = Occurrence + 1;
            return fOc.AddDays(7 * (Occurrence - 1));
        }

        public static DateTime FromUnixFormatToDateTime(this double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static double ToUnixFormat(this DateTime value)
        {
            return (value - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).TotalSeconds;
        }
        #endregion
    }


    