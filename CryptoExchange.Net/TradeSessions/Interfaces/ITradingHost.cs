using CryptoExchange.Net.TradeSessions;

namespace CryptoExchange.Net.TradeSessions.Interfaces;

public interface ITradingHost<out TStrategy> : ITradingSession
    where TStrategy : TradeSession
{
    TStrategy Strategy { get; }
}
