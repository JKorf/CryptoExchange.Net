using CryptoExchange.Net.Sessions;

namespace CryptoExchange.Net.Trackers.Sessions.Interfaces;

public interface ITradingHost<out TStrategy> : ITradingSession
    where TStrategy : TradingStrategy
{
    TStrategy Strategy { get; }
}
