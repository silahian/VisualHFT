using Newtonsoft.Json;

namespace Gemini.Net.Objects.Models.Spot;

/// <summary>
/// New order id
/// </summary>
public record GeminiModifiedOrder
{
    /// <summary>
    /// The id of the new order
    /// </summary>
    [JsonProperty("newOrderId")]
    public string Id { get; set; } = string.Empty;
}
