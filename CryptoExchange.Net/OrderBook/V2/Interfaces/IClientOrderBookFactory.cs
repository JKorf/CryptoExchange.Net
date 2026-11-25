using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.OrderBook.V2.Interfaces
{
    /// <summary>
    /// Order book factory
    /// </summary>
    public interface IClientOrderBookFactory
    {
        /// <summary>
        /// Create a new order book
        /// </summary>
        IClientOrderBook Create(SharedSymbol symbol);
    }
}