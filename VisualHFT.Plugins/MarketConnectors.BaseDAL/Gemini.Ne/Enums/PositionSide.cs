using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Position side
    /// </summary>
    public enum PositionSide
    {
        /// <summary>
        /// Both (One way position mode)
        /// </summary>
        [Map("BOTH")]
        Both,
        /// <summary>
        /// Long
        /// </summary>
        [Map("LONG")]
        Long,
        /// <summary>
        /// Short
        /// </summary>
        [Map("SHORT")]
        Short
    }
}
