using Newtonsoft.Json;

namespace Gemini.Net.Objects.Internal
{
    internal class GeminiRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("topic")]
        public string Topic { get; set; }
        [JsonProperty("privateChannel")]
        public bool PrivateChannel { get; set; }
        [JsonProperty("response")]
        public bool Response { get; set; }
        
        public GeminiRequest(string id, string type, string topic, bool userEvents)
        {
            Id = id;
            Topic = topic;
            Type = type;
            Response = true;
            PrivateChannel = userEvents;
        }
    }
}
