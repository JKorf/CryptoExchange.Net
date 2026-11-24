using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces.Handlers;

/// <summary>
/// Asynchronous trade update handler
/// </summary>
public interface IAsyncTradeUpdateHandler
{
    /// <summary>
    /// Handle a queued trade update
    /// </summary>
    /// <param name="symbol">The symbol for which the update is received</param>
    /// <param name="update">The update data</param>
    Task HandleTradeUpdateAsync(SharedSymbol symbol, DataEvent<SharedTrade[]> update);
}
