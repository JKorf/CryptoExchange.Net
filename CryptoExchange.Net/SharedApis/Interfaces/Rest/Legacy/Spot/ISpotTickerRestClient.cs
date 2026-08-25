using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for requesting spot tickers
    /// </summary>
    public interface ISpotTickerRestClient :
        IGetSpotTickerRestClient,
        IGetAllSpotTickersRestClient
    {
    }
}
