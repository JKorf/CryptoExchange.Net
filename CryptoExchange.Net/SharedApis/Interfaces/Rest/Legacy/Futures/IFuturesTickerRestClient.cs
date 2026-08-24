using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting ticker info for futures symbols
    /// </summary>
    public interface IFuturesTickerRestClient :
        IGetFuturesTickerRestClient,
        IGetAllFuturesTickersRestClient
    {
    }
}
