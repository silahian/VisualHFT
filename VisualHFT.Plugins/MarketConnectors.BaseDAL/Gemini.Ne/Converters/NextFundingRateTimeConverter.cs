using System;
using Newtonsoft.Json;

namespace Gemini.Net.Converters
{
    /// <summary>
    /// Does basically the same thing as the shared DateTimeConverter, with a slight difference.
    /// Instead of adding the time to Jan 01 1970, it adds it to the current time. This isn't ideal since
    /// it produces inaccuracies, but it's the format the gemini api returns for the next funding rate time.
    /// </summary>
    internal class NextFundingRateTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.Value == null)
            {
                return null;
            }

            var value = (long)reader.Value;
            var now = DateTime.UtcNow;
            var offset = TimeSpan.FromMilliseconds(value);
            return now + offset;
        }

        public override void WriteJson(
            JsonWriter writer,
            DateTime? value,
            JsonSerializer serializer
        )
        {
            throw new NotSupportedException("This converter only supports reading");
        }
    }
}
