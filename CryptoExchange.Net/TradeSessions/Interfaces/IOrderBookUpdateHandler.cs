using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.SharedApis;

namespace CryptoExchange.Net.TradeSessions.Interfaces
{
    public interface IOrderBookUpdateHandler
    {
        void HandleOrderBookUpdate(SharedSymbol symbol, ISymbolOrderBook orderBook);
    }
}
