using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Withdrawal type
    /// </summary>
    public enum WithdrawType
    {
        /// <summary>
        /// Withdraw to crypto address
        /// </summary>
        [Map("ADDRESS")]
        Address,
        /// <summary>
        /// Withdraw to user by user id
        /// </summary>
        [Map("UID")]
        Uid,
        /// <summary>
        /// Withdraw to user by email
        /// </summary>
        [Map("MAIL")]
        Mail,
        /// <summary>
        /// Withdraw to user by phone
        /// </summary>
        [Map("Phone")]
        Phone
    }
}
