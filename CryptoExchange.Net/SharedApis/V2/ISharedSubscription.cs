using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Shared WebSocket API subscription capability
    /// </summary>
    public interface ISharedSubscription : ISharedSocket
    {
        /// <summary>
        /// Unsubscribes all subscriptions created by the underlying socket API
        /// client and closes its subscription connections.
        /// </summary>
        Task UnsubscribeAllAsync();
    }
}
