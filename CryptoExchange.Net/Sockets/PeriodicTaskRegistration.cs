using CryptoExchange.Net.Objects;
using System;

namespace CryptoExchange.Net.Sockets
{
    /// <summary>
    /// Periodic task registration
    /// </summary>
    public class PeriodicTaskRegistration
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public string Identifier { get; set; } = string.Empty;
        /// <summary>
        /// Interval of query
        /// </summary>
        public TimeSpan Interval { get; set; }
        /// <summary>
        /// Delegate for getting the query
        /// </summary>
        public Func<SocketConnection, Query> QueryDelegate { get; set; } = null!;
        /// <summary>
        /// Callback after query
        /// </summary>
        public Action<CallResult>? Callback { get; set; }
    }
}
