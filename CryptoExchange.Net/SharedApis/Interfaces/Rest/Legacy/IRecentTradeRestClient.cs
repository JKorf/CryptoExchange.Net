using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for retrieving the most recent public trades
    /// </summary>
    public interface IRecentTradeRestClient : IGetRecentTradesRestClient
    {
    }
}
