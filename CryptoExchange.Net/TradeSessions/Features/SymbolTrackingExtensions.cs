using System;

namespace CryptoExchange.Net.TradeSessions.Features
{
    /// <summary>
    /// Symbol tracking extension methods
    /// </summary>
    public static partial class SymbolTrackingExtensions
    {
        private static void ValidateQueueCapacity(int? maxQueuedItems)
        {
            if (maxQueuedItems <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxQueuedItems));
        }
    }
}
