namespace Gemini.Net.Enums
{
    /// <summary>
    /// Time the order is valid for
    /// </summary>
    public enum TimeInForce
    {
        /// <summary>
        /// Good until canceled by user
        /// </summary>
        GoodTillCanceled,
        /// <summary>
        /// Good until a certain time
        /// </summary>
        GoodTillTime,
        /// <summary>
        /// Immediately has to be (partially) filled or it will be canceled
        /// </summary>
        ImmediateOrCancel,
        /// <summary>
        /// Immediately has to be full filled or it will be canceled
        /// </summary>
        FillOrKill
    }
}
