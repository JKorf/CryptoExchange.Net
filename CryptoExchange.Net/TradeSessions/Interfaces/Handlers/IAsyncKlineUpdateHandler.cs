using CryptoExchange.Net.Objects.Sockets;
using CryptoExchange.Net.SharedApis;
using System.Threading.Tasks;

namespace CryptoExchange.Net.TradeSessions.Interfaces.Handlers;

/// <summary>
/// Asynchronous kline update handler
/// </summary>
public interface IAsyncKlineUpdateHandler
{
    /// <summary>
    /// Handle a queued kline update
    /// </summary>
    /// <param name="symbol">The symbol for which the update is received</param>
    /// <param name="update">The update data</param>
    Task HandleKlineUpdateAsync(SharedSymbol symbol, DataEvent<SharedKline> update);
}
