namespace Gemini.Net.Objects.Models.Spot
{
    /// <summary>
    /// New account id
    /// </summary>
    public record GeminiNewAccount
    {
        /// <summary>
        /// The id of the new account
        /// </summary>
        public string Id { get; set; } = string.Empty;
    }
}
