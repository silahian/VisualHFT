using Newtonsoft.Json;

namespace Gemini.Net.Objects.Sockets
{
    internal class GeminiPong
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;
        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;
    }
}
