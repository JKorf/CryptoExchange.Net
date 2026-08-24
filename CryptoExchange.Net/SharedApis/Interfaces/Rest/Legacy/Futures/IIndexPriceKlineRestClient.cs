using CryptoExchange.Net.Objects;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoExchange.Net.SharedApis
{
    /// <summary>
    /// Client for getting the index price klines for a symbol
    /// </summary>
    public interface IIndexPriceKlineRestClient : IGetIndexPriceKlinesRestClient
    {
    }
}
