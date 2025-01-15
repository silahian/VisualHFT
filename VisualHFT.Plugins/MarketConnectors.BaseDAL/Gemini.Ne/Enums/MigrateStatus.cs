using CryptoExchange.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gemini.Net.Enums
{
    /// <summary>
    /// Migration status
    /// </summary>
    public enum MigrateStatus
    {
        /// <summary>
        /// Not migrated
        /// </summary>
        [Map("0")]
        NotMigrated,
        /// <summary>
        /// Migrating
        /// </summary>
        [Map("1")]
        Migrating,
        /// <summary>
        /// Migrated
        /// </summary>
        [Map("2")]
        Migrated
    }
}
