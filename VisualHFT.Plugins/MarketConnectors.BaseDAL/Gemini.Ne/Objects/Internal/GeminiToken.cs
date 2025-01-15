using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Gemini.Net.Objects.Internal
{
    internal class GeminiToken
    {
        public string Token { get; set; } = string.Empty;
        [JsonProperty("instanceServers")]
        public IEnumerable<GeminiInstanceServer> Servers { get; set; } = Array.Empty<GeminiInstanceServer>();
    }

    internal class GeminiInstanceServer
    {
        public int PingInterval { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string Protocol { get; set; } = string.Empty;
        public bool Encrypt { get; set; }
        public int PingTimeout { get; set; }
    }
}
