using System;
using System.IO;
using Gemini.Net.Objects.Models.Spot;
using Newtonsoft.Json;

namespace Gemini.Net.Converters
{
    internal class AccountActivityContextConverter : JsonConverter<GeminiAccountActivityContext>
    {
        public override GeminiAccountActivityContext ReadJson(JsonReader reader, Type objectType, GeminiAccountActivityContext? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.Value == null)
                return new GeminiAccountActivityContext();

            var obj = new JsonSerializer().Deserialize(new JsonTextReader(new StringReader(reader.Value.ToString())), typeof(GeminiAccountActivityContext));
            if (obj == null)
                return new GeminiAccountActivityContext();
            else
                return (GeminiAccountActivityContext)obj;
        }

        public override void WriteJson(JsonWriter writer, GeminiAccountActivityContext? value, JsonSerializer serializer)
        {
            if (value == null) return;

            serializer.Serialize(writer, value);
        }
    }
}
