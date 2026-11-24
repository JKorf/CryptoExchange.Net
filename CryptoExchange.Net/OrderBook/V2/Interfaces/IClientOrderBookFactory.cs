using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.OrderBook.V2.Interfaces
{
    public interface IClientOrderBookFactory
    {
        IClientOrderBook Create(SharedSymbol symbol);
    }
}