using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces.Handlers;

/// <summary>
/// Asynchronous ticker update handler
/// </summary>
public interface IAsyncTickerUpdateHandler
{
    /// <summary>
    /// Handle a queued ticker update
    /// </summary>
    /// <param name="symbol">The symbol for which the update is received</param>
    /// <param name="update">The update data</param>
    Task HandleTickerUpdateAsync(SharedSymbol symbol, DataEvent<SharedTicker> update);
}
