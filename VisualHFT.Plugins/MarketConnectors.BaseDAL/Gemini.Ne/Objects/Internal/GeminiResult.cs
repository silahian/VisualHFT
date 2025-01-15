using Newtonsoft.Json;

namespace Gemini.Net.Objects.Internal
{
    internal class GeminiResult
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("msg")]
        public string? Message { get; set; }
    }

    internal class GeminiResult<T> : GeminiResult
    {
        public T Data { get; set; } = default!;
    }
}
