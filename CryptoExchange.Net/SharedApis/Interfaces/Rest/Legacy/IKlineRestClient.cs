using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting kline/candlestick data
    /// </summary>
    public interface IKlineRestClient : IGetKlinesRestClient
    {
    }
}
