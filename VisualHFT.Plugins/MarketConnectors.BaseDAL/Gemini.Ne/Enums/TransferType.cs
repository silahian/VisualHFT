using CryptoExchange.Net.Attributes;
using Newtonsoft.Json;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Transfer type
    /// </summary>
    [JsonConverter(typeof(EnumConverter))]
    public enum TransferType
    {
        /// <summary>
        /// Internal
        /// </summary>
        [Map("INTERNAL")]
        Internal,
        /// <summary>
        /// Parent to sub
        /// </summary>
        [Map("PARENT_TO_SUB")]
        ParentToSub,
        /// <summary>
        /// Sub to parent
        /// </summary>
        [Map("SUB_TO_PARENT")]
        SubToParent
    }
}
