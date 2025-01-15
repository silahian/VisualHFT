using Newtonsoft.Json;

namespace Gemini.Net.Objects.Sockets
{
    internal class GeminiSocketUpdate<T>
    {
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
        [JsonProperty("topic")]
        public string Topic { get; set; } = string.Empty;
        [JsonProperty("subject")]
        public string Subject { get; set; } = string.Empty;
        [JsonProperty("data")]
        public T Data { get; set; } = default!;
    }
}
