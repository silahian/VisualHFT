﻿using Newtonsoft.Json;
using System.Globalization;
using System;

namespace MarketConnectors.WebSocket
{
    internal class CustomDateConverter : JsonConverter
    {
        const string DatePositionFormat = "yyyy'.'MM'.'dd'-'HH'.'mm'.'ss'.'ffffff";

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(DateTime) || objectType == typeof(DateTime?));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            //scenario: "2022.10.19-14.49.58.808384"            -- OK
            //scenario: "2022-10-19T14:40:49.2291586-04:00"     -- NOT WORKING

            string rawDate = (string)reader.Value;
            if (string.IsNullOrEmpty(rawDate))
                return null;
            //Validate that milliseconds are 6 chars long
            if (rawDate.IndexOf(".") > -1)
            {
                int lenghtMilliseconds = rawDate.Split('.')[5].Length;
                if (rawDate.Split('.')[5].Length < 6)
                {
                    for (int i = 0; i < (6 - lenghtMilliseconds); i++)
                        rawDate = rawDate + "0";
                }
            }
            DateTime date;

            // First try to parse the date string as is (in case it is correctly formatted)
            if (DateTime.TryParse(rawDate, out date))
            {
                return date;
            }
            else if (DateTime.TryParseExact(rawDate, DatePositionFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                return date;

            // If not, see if the string matches the known bad format. 
            // If so, replace the ':' with '.' and reparse.
            if (rawDate.Length > 19 && rawDate[19] == ':')
            {
                rawDate = rawDate.Substring(0, 19) + '.' + rawDate.Substring(20);
                if (DateTime.TryParse(rawDate, out date))
                {
                    return date;
                }
            }

            // It's not a date after all, so just return the default value
            if (objectType == typeof(DateTime?))
                return null;

            return DateTime.MinValue;
        }

        public override bool CanWrite
        {
            get { return false; }
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}