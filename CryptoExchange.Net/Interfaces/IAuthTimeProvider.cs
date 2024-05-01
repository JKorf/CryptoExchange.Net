using System;

namespace CryptoExchange.Net.Interfaces
{
    /// <summary>
    /// Time provider
    /// </summary>
    internal interface IAuthTimeProvider
    {
        /// <summary>
        /// Get current time
        /// </summary>
        /// <returns></returns>
        DateTime GetTime();
    }
}
