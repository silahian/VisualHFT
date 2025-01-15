using CryptoExchange.Net.Attributes;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Margin position update event type
    /// </summary>
    public enum MarginEventType
    {
        /// <summary>
        /// When the debt ratio exceeds the liquidation threshold and the position is frozen, the system will push this event.
        /// </summary>
        [Map("FROZEN_FL")]
        Frozen,
        /// <summary>
        /// When the liquidation is finished and the position returns to “EFFECTIVE” status, the system will push this event.
        /// </summary>
        [Map("UNFROZEN_FL")]
        Unfrozen,
        /// <summary>
        /// When the auto-borrow renewing is complete and the position returns to “EFFECTIVE” status, the system will push this event.
        /// </summary>
        [Map("FROZEN_RENEW")]
        FrozenRenew,
        /// <summary>
        /// When the account reaches a negative balance, the system will push this event.
        /// </summary>
        [Map("UNFROZEN_RENEW")]
        UnfrozenRenew,
        /// <summary>
        /// When the account reaches a negative balance, the system will push this event.
        /// </summary>
        [Map("LIABILITY")]
        Liability,
        /// <summary>
        /// When all the liabilities is repaid and the position returns to “EFFECTIVE” status, the system will push this event.
        /// </summary>
        [Map("UNLIABILITY")]
        Unliability
    }
}
