using System;

namespace CryptoExchange.Net.Sockets.HighPerf
{
    /// <summary>
    /// Periodic task registration
    /// </summary>
    public class HighPerfPeriodicTaskRegistration
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
        /// Delegate for getting the request which should be send
        /// </summary>
        public Func<HighPerfSocketConnection, object> GetRequestDelegate { get; set; } = null!;
    }
}
